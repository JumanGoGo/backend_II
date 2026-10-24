using System.Security.Claims;
using backend_II.Models;

namespace backend_II.Authentication
{
    public interface ITokenService
    {
        LoginResponse GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
