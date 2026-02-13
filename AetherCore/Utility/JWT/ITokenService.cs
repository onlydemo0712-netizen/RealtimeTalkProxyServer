using System.Security.Claims;

namespace AetherCore.Utility.JWT
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string userName, IEnumerable<Claim>? extraClaims = null, int expireMinutes = 60);
    }
}
