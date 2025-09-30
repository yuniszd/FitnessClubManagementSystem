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

    #region DTOs

    public record LoginRequest(
        [Required(ErrorMessage = "Username boş ola bilməz")] string Username,
        [Required(ErrorMessage = "Password boş ola bilməz")] string Password
    );

    public record RefreshTokenRequest(
        [Required(ErrorMessage = "RefreshToken boş ola bilməz")] string RefreshToken
    );

    #endregion

    [HttpPost("login")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = "Username və Password tələb olunur"
            });

        try
        {
            var tokens = await _authService.LoginAsync(request.Username, request.Password);

            return Ok(SuccessResponse(tokens, "Login uğurlu oldu"));
        }
        catch (Exception ex)
        {
            return Unauthorized(FailResponse(ex.Message));
        }
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(FailResponse("RefreshToken tələb olunur"));

        try
        {
            var tokens = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(SuccessResponse(tokens, "Token yeniləndi"));
        }
        catch (Exception ex)
        {
            return Unauthorized(FailResponse(ex.Message));
        }
    }

    #region Helper Methods

    private static BaseResponse<TokenResponse> SuccessResponse(TokenResponse data, string message) =>
        new() { Success = true, Message = message, Data = data };

    private static BaseResponse<object> FailResponse(string message) =>
        new() { Success = false, Message = message };

    #endregion
}
