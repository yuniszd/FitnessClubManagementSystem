using FCMS.Application.Responses;
using FCMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCMS.Application.Abstracts;

public interface IJwtTokenService
{
    Task<TokenResponse> GenerateTokensAsync(AppUser user);
}

