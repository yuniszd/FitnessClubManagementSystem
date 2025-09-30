
using FCMS.Application.Abstracts;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Application.Responses;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FCMS.Persistence.Services;

public class AuthService : IAuthService
{
    private readonly FitnessDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(FitnessDbContext context, UserManager<AppUser> userManager, IConfiguration config, ILogger<AuthService> logger)
    {
        _context = context;
        _userManager = userManager;
        _config = config;
        _logger = logger;
    }

    public async Task<TokenResponse> LoginAsync(string username, string password)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("username", "Username is required");

        if (string.IsNullOrWhiteSpace(password))
            throw new ValidationException("password", "Password is required");

        var user = await _userManager.FindByNameAsync(username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            _logger.LogWarning("Unauthorized login attempt for username: {Username}", username);
            throw new UnauthorizedException("Invalid credentials!", "Username or password is incorrect");
        }

        var accessToken = await GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetConfigInt("Jwt:RefreshTokenExpirationDays", 7));

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError("Failed to update refresh token for user {Username}", username);
            throw new InternalErrorException("Failed to update refresh token for user");
        }

        _logger.LogInformation("User {Username} logged in successfully", username);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ValidationException("refreshToken", "Refresh token is required");

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired refresh token used");
            throw new UnauthorizedException("Invalid or expired refresh token!", "Your refresh token is invalid or expired");
        }

        var newAccessToken = await GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetConfigInt("Jwt:RefreshTokenExpirationDays", 7));

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError("Failed to update refresh token for user {UserId}", user.Id);
            throw new InternalErrorException("Failed to update refresh token for user");
        }

        _logger.LogInformation("Refresh token successfully renewed for user {UserId}", user.Id);

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    #region Private Helpers

    private async Task<string> GenerateAccessToken(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyString))
            throw new InternalErrorException("JWT key is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetConfigInt("Jwt:AccessTokenExpirationMinutes", 60)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private int GetConfigInt(string key, int defaultValue)
    {
        if (int.TryParse(_config[key], out var value))
            return value;
        return defaultValue;
    }

    #endregion
}
