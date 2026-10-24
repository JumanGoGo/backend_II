using System.Data;
using Microsoft.Data.SqlClient;
using backend_II.Models;
using backend_II.Utils;

namespace backend_II.Database
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public GeneralResponse<User> GetUserByUsername(string username)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("sp_GetAllActiveProducts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@method", "GetUserForLogin");
                        command.Parameters.AddWithValue("@Username", username);

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader.GetName(0) == "code")
                                {
                                    return new GeneralResponse<User>
                                    {
                                        Success = false,
                                        Message = reader["message"].null_string("Error en la consulta."),
                                    };
                                }

                                return new GeneralResponse<User>
                                {
                                    Success = true,
                                    Message = reader["message"].null_string("Success"),
                                    Data = new User
                                    {
                                        UserId = reader["UserId"].null_int(),
                                        Username = username,
                                        PasswordHash = reader["PasswordHash"].null_string(),
                                        Role = reader["Role"].null_string("User")
                                    }
                                };
                            }
                        }
                    }
                }

                return new GeneralResponse<User>
                {
                    Success = false,
                    Message = "No se obtuvo respuesta de la base de datos.",
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<User>
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }
    }
}
