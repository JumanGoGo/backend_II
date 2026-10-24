using System.Data;
using Microsoft.Data.SqlClient;
using backend_II.Models;

namespace backend_II.Database
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<List<Product>> GetAllActiveProductsAsync()
        {
            var products = new List<Product>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_GetAllActiveProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    
                    command.Parameters.AddWithValue("@method", "Products");
                    command.Parameters.AddWithValue("@isActive", true);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader.GetName(0) == "code") 
                            {
                                throw new Exception(reader["message"].ToString());
                            }

                            products.Add(new Product
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                SKU = reader["SKU"].ToString() ?? string.Empty,
                                Name = reader["Name"].ToString() ?? string.Empty,
                                Description = reader["Description"] != DBNull.Value
                                                ? reader["Description"].ToString() ?? string.Empty 
                                                : string.Empty,
                                Price = Convert.ToDecimal(reader["Price"]),
                                StockQuantity = Convert.ToInt32(reader["StockQuantity"])
                            });
                        }
                    }
                }
            }

            return products;
        }

        public async Task<Product?> GetProductBySkuAsync(string sku)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_GetAllActiveProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@method", "GetProductBySKU");
                    command.Parameters.AddWithValue("@SKU", sku);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetName(0) == "code") 
                            {
                                return null;
                            }

                            return new Product
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                SKU = reader["SKU"].ToString() ?? string.Empty,
                                Name = reader["Name"].ToString() ?? string.Empty,
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() ?? string.Empty : string.Empty,
                                Price = Convert.ToDecimal(reader["Price"]),
                                StockQuantity = Convert.ToInt32(reader["StockQuantity"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        // ===== INICIO ACTIVIDAD: BÚSQUEDA POR NOMBRE =====

        /// <summary>
        /// Invoca el método 'GetProductsByName' del procedimiento almacenado
        /// sp_GetAllActiveProducts y devuelve todos los productos activos cuyo
        /// nombre contenga el texto recibido.
        /// </summary>
        public async Task<List<Product>> GetProductsByNameAsync(string name)
        {
            var products = new List<Product>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("sp_GetAllActiveProducts", connection))
                {
                    // Se indica que el texto anterior es el nombre de un
                    // procedimiento almacenado, no una sentencia SQL.
                    command.CommandType = CommandType.StoredProcedure;

                    // Se declara el tipo y el tamaño exactos de cada parámetro.
                    // AddWithValue obliga al driver a adivinarlos, lo que puede
                    // provocar conversiones implícitas dentro del servidor.
                    command.Parameters.Add("@method", SqlDbType.VarChar, 100).Value = "GetProductsByName";
                    command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Si la primera columna se llama 'code', el procedimiento
                            // devolvió un error. Se propaga su mensaje tal cual, para
                            // que el usuario vea la validación hecha en la base de datos.
                            if (reader.GetName(0) == "code")
                            {
                                throw new InvalidOperationException(reader["message"].ToString());
                            }

                            products.Add(new Product
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                SKU = reader["SKU"].ToString() ?? string.Empty,
                                Name = reader["Name"].ToString() ?? string.Empty,
                                Description = reader["Description"] != DBNull.Value
                                                ? reader["Description"].ToString() ?? string.Empty
                                                : string.Empty,
                                Price = Convert.ToDecimal(reader["Price"]),
                                StockQuantity = Convert.ToInt32(reader["StockQuantity"])
                            });
                        }
                    }
                }
            }

            return products;
        }

        // ===== FIN ACTIVIDAD: BÚSQUEDA POR NOMBRE =====
    }
}
