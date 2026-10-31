using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using backend_II.Models;

namespace backend_II.Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string PrimaryApiKeyHeader = "X-Api-Key";
        private const string AlternativeApiKeyHeader = "ApiKey";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            string? extractedApiKey = null;

            if (headers.TryGetValue(PrimaryApiKeyHeader, out var primaryHeaderValue) && !string.IsNullOrWhiteSpace(primaryHeaderValue))
            {
                extractedApiKey = primaryHeaderValue.ToString();
            }
            else if (headers.TryGetValue(AlternativeApiKeyHeader, out var alternativeHeaderValue) && !string.IsNullOrWhiteSpace(alternativeHeaderValue))
            {
                extractedApiKey = alternativeHeaderValue.ToString();
            }

            if (string.IsNullOrWhiteSpace(extractedApiKey))
            {
                context.Result = new ObjectResult(new GeneralResponse<object>
                {
                    Success = false,
                    Message = $"API Key no proporcionada. Se requiere el encabezado '{PrimaryApiKeyHeader}'."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var apiKeyValidator = context.HttpContext.RequestServices.GetRequiredService<IApiKeyValidator>();
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
        }
    }
}
