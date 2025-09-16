using Microsoft.AspNetCore.Identity;

namespace FCMS.Domain.Entities;

public class AppUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}