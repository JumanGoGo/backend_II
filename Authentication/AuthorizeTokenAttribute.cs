using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using backend_II.Models;

namespace backend_II.Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeTokenAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string? _requiredRole;

        public AuthorizeTokenAttribute(string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new GeneralResponse<object>
                {
                    Success = false,
                    Message = "Token de autenticación no proporcionado o formato inválido."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();
            var principal = tokenService.ValidateToken(token);

            if (principal == null)
            {
                context.Result = new ObjectResult(new GeneralResponse<object>
                {
                    Success = false,
                    Message = "Token inválido o expirado."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            context.HttpContext.User = principal;

            if (!string.IsNullOrEmpty(_requiredRole))
            {
                var role = principal.FindFirst(ClaimTypes.Role)?.Value 
                           ?? principal.FindFirst("role")?.Value;

                if (!string.Equals(role, _requiredRole, StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new ObjectResult(new GeneralResponse<object>
                    {
                        Success = false,
                        Message = $"No cuenta con los permisos requeridos (Rol requerido: {_requiredRole})."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
            }
        }
    }
}
