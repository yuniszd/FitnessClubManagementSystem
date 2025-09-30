using FCMS.Application.Responses;
using FCMS.Domain.Entities;

namespace FCMS.Application.Abstracts;

public interface IJwtTokenService
{
    Task<TokenResponse> GenerateTokensAsync(AppUser user);
}

