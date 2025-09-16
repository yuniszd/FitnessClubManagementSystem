using FCMS.Application.Abstracts;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // DTO-lar
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username boş ola bilməz")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password boş ola bilməz")]
        public string Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "RefreshToken boş ola bilməz")]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Admin / Reception istifadəçisini JWT ilə login edir
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = "Username və Password tələb olunur"
            });
        }

        try
        {
            var tokens = await _authService.LoginAsync(request.Username, request.Password);

            return Ok(new BaseResponse<object>
            {
                Success = true,
                Message = "Login uğurlu oldu",
                Data = new
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken
                }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Refresh token ilə yeni JWT access token və refresh token yaradır
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = "RefreshToken tələb olunur"
            });
        }

        try
        {
            var tokens = await _authService.RefreshTokenAsync(request.RefreshToken);

            return Ok(new BaseResponse<object>
            {
                Success = true,
                Message = "Token yeniləndi",
                Data = new
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken
                }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}
