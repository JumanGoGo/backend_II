using System.Security.Claims;

namespace backend_II.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? GetUser()
        {
            return _httpContextAccessor.HttpContext?.User;
        }

        public bool IsAuthenticated()
        {
            var user = GetUser();
            return user?.Identity?.IsAuthenticated == true;
        }

        public int? GetUserId()
        {
            var user = GetUser();
            var val = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                      ?? user?.FindFirst("userId")?.Value;

            return int.TryParse(val, out var id) ? id : null;
        }

        public string? GetUsername()
        {
            var user = GetUser();
            return user?.FindFirst(ClaimTypes.Name)?.Value 
                   ?? user?.FindFirst("username")?.Value;
        }

        public string? GetRole()
        {
            var user = GetUser();
            return user?.FindFirst(ClaimTypes.Role)?.Value 
                   ?? user?.FindFirst("role")?.Value;
        }

        public string? GetClaim(string claimType)
        {
            return GetUser()?.FindFirst(claimType)?.Value;
        }
    }
}
