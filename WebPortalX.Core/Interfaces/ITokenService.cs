using WebPortalX.Core.Models;
using System.Security.Claims;

namespace WebPortalX.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(UserManager user);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        ClaimsPrincipal GetPrincipalFromToken(string token);
    }
} 