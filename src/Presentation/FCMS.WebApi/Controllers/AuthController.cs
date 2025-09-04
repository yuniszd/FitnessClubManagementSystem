// 📂 FCMS.API/Controllers/AuthController.cs

using FCMS.Application.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// Üzvü kart nömrəsi ilə sistemə daxil edir.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        await _authService.LoginWithCardAsync(request.CardNumber);
        return Ok(new { Message = "Login successful" });
    }

    /// <summary>
    /// Aktiv istifadəçini çıxış etdirir.
    /// </summary>
    [Authorize] // yalnız login olan istifadəçi çıxa bilər
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { Message = "Logout successful" });
    }
}

// DTO (Request modeli)
public class LoginRequest
{
    public string CardNumber { get; set; }
}
