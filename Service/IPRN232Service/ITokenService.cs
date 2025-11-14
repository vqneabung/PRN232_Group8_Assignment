using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;
using System.Security.Claims;

namespace Service.IPRN232Service
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        ClaimsPrincipal ValidateToken(string token);
    }
}
