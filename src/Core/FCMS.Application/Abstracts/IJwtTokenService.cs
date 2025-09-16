using FCMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCMS.Application.Abstracts;

public interface IJwtTokenService
{
    Task<string> GenerateAccessToken(AppUser user);
    Task<(string token, DateTime expires)> GenerateRefreshToken(AppUser user);
}

