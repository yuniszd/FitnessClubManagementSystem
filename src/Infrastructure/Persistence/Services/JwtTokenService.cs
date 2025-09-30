using FCMS.Application.Abstracts;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Application.Responses;
using FCMS.Domain.Entities;
using FCMS.Persistence.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FCMS.Persistence.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings, UserManager<AppUser> userManager, ILogger<JwtTokenService> logger)
    {
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TokenResponse> GenerateTokensAsync(AppUser user)
    {
        if (user == null)
            throw new ValidationException("user", "User cannot be null");

        try
        {
            // 🔹 Get roles
            var roles = await _userManager.GetRolesAsync(user);
            var normalizedRoles = roles.Select(r => r.ToUpperInvariant()).ToList();

            // 🔹 Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(normalizedRoles.Select(r => new Claim(ClaimTypes.Role, r)));
            claims.Add(new Claim("roles", string.Join(",", normalizedRoles)));

            // 🔹 Create access token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: accessTokenExpiration,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // 🔹 Create refresh token
            var refreshToken = GenerateRefreshToken(out DateTime refreshTokenExpiration);

            // 🔹 Save refresh token to user safely
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiration;

            _logger.LogInformation("Tokens generated for user {UserId}", user.Id);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = accessTokenExpiration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate tokens for user {UserId}", user.Id);
            throw new InternalErrorException("Failed to generate JWT tokens", ex);
        }
    }

    private string GenerateRefreshToken(out DateTime expiry)
    {
        try
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            return Convert.ToBase64String(randomBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate refresh token");
            throw new InternalErrorException("Failed to generate refresh token", ex);
        }
    }
}