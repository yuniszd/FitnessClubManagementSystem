// 📂 FCMS.Persistence/Services/AuthService.cs

using FCMS.Application.Abstracts;
using FCMS.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FCMS.Persistence.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DbContext _context;

    public AuthService(IHttpContextAccessor httpContextAccessor, DbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    // Login yalnız kart nömrəsi ilə (sadə auth)
    public async Task LoginWithCardAsync(string cardNumber)
    {
        var member = await _context.Set<Member>()
            .FirstOrDefaultAsync(x => x.CardNumber == cardNumber);

        if (member == null)
            throw new Exception("Invalid Card Number!");

        await SignInAsync(member);
    }

    // Logout
    public async Task LogoutAsync()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    // Cookie yazmaq
    private async Task SignInAsync(Member member)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
            new Claim(ClaimTypes.Name, member.FullName),
            new Claim("CardNumber", member.CardNumber)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await _httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false }
        );
    }
}
