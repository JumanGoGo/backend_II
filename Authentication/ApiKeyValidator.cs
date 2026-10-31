using System.Data;
using Microsoft.Data.SqlClient;

namespace backend_II.Authentication
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly string _connectionString;

        public ApiKeyValidator(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !Guid.TryParse(apiKey, out var apiKeyGuid))
            {
                return false;
            }

            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("sp_GetAllActiveProducts", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@method", "ValidateApiKey");
                command.Parameters.AddWithValue("@KeyValue", apiKeyGuid);

                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var code = reader["code"]?.ToString();
                    return code == "1";
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
