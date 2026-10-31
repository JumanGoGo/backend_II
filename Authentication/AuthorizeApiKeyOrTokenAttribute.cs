using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using backend_II.Models;

namespace backend_II.Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeApiKeyOrTokenAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string PrimaryApiKeyHeader = "X-Api-Key";
        private const string AlternativeApiKeyHeader = "ApiKey";
        private readonly string[] _requiredRoles;

        public AuthorizeApiKeyOrTokenAttribute(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles?.Where(r => !string.IsNullOrWhiteSpace(r)).ToArray() ?? Array.Empty<string>();
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var authHeader = httpContext.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var tokenService = httpContext.RequestServices.GetRequiredService<ITokenService>();
                var token = authHeader["Bearer ".Length..].Trim();
                var principal = tokenService.ValidateToken(token);

                if (principal == null)
                {
                    context.Result = new ObjectResult(new GeneralResponse<object>
                    {
                        Success = false,
                        Message = "Token de autenticación inválido o expirado."
                    })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }

                httpContext.User = principal;

                if (_requiredRoles.Length > 0)
                {
                    var role = principal.FindFirst(ClaimTypes.Role)?.Value 
                               ?? principal.FindFirst("role")?.Value;

                    var isAuthorized = _requiredRoles.Any(r =>
                        r.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                         .Any(subRole => string.Equals(subRole, role, StringComparison.OrdinalIgnoreCase))
                    );

                    if (!isAuthorized)
                    {
                        context.Result = new ObjectResult(new GeneralResponse<object>
                        {
                            Success = false,
                            Message = $"No cuenta con los permisos requeridos (Rol requerido: {string.Join(", ", _requiredRoles)})."
                        })
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return;
                    }
                }
                return;
            }

            var headers = httpContext.Request.Headers;
            string? extractedApiKey = null;

            if (headers.TryGetValue(PrimaryApiKeyHeader, out var primaryHeaderValue) && !string.IsNullOrWhiteSpace(primaryHeaderValue))
            {
                extractedApiKey = primaryHeaderValue.ToString();
            }
            else if (headers.TryGetValue(AlternativeApiKeyHeader, out var alternativeHeaderValue) && !string.IsNullOrWhiteSpace(alternativeHeaderValue))
            {
                extractedApiKey = alternativeHeaderValue.ToString();
            }

            if (!string.IsNullOrWhiteSpace(extractedApiKey))
            {
                var apiKeyValidator = httpContext.RequestServices.GetRequiredService<IApiKeyValidator>();
                var isValid = await apiKeyValidator.ValidateApiKeyAsync(extractedApiKey.Trim());

                if (!isValid)
                {
                    context.Result = new ObjectResult(new GeneralResponse<object>
                    {
                        Success = false,
                        Message = "API Key inválida o inactiva."
                    })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }
                return;
            }

            context.Result = new ObjectResult(new GeneralResponse<object>
            {
                Success = false,
                Message = "Autenticación requerida. Debe proporcionar un Token Bearer en 'Authorization' o una clave en 'X-Api-Key'."
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}
