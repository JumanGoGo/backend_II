using System.Security.Claims;

namespace backend_II.Services
{
    public interface IUserContextService
    {
        int? GetUserId();
        string? GetUsername();
        string? GetRole();
        string? GetClaim(string claimType);
        bool IsAuthenticated();
        ClaimsPrincipal? GetUser();
    }
}
