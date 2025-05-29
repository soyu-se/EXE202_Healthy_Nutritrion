using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HealthyNutritionApp.Application.Interfaces.Authentication
{
    public interface IJsonWebToken
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);

        ClaimsPrincipal ValidateToken(string token);

        JwtSecurityToken DecodeToken(string token);
    }
}
