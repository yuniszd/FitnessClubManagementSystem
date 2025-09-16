// 📂 FCMS.Application/Abstracts/IAuthService.cs

namespace FCMS.Application.Abstracts;

public interface IAuthService
{
    /// <summary>
    /// Admin / Reception istifadəçisi login olur, JWT + Refresh token qaytarılır
    /// </summary>
    /// <param name="username">İstifadəçi adı</param>
    /// <param name="password">Parol</param>
    /// <returns>Tuple: AccessToken və RefreshToken</returns>
    Task<(string AccessToken, string RefreshToken)> LoginAsync(string username, string password);

    /// <summary>
    /// Refresh token ilə yeni AccessToken və RefreshToken yaratmaq
    /// </summary>
    /// <param name="refreshToken">Əvvəlki refresh token</param>
    /// <returns>Yeni AccessToken və RefreshToken</returns>
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
}
