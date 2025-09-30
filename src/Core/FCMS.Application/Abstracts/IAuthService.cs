
using FCMS.Application.Responses;

namespace FCMS.Application.Abstracts;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(string username, string password);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
}
