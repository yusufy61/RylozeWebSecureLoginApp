using System.Security.Claims;
using RylozeWebSecureLoginApp.API.Entities;

namespace RylozeWebSecureLoginApp.API.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(AppUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
