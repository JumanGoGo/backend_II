using Microsoft.AspNetCore.Mvc;
using backend_II.Models;
using backend_II.Database;

namespace backend_II.Services
{
    public class ProductsServices
    {
        private readonly ProductRepository _repository;
        private readonly IUserContextService _userContextService;

        public ProductsServices(ProductRepository repository, IUserContextService userContextService)
        {
            _repository = repository;
            _userContextService = userContextService;
        }

        public async Task<GeneralResponse<List<Product>>> GetAll()
        {
            try
            {
                var products = await _repository.GetAllActiveProductsAsync();
                
                return new GeneralResponse<List<Product>>
                {
                    Success = true,
                    Message = "Productos obtenidos exitosamente.",
                    Data = products
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<Product>> 
                { 
                    Success = false, 
                    Message = ex.Message, 
                    Data = null 
                };
            }
        }
        public async Task<GeneralResponse<Product>> GetByRoute(string sku)
        {
            try
            {
                var product = await _repository.GetProductBySkuAsync(sku);
                
                if (product == null)
                {
                    return new GeneralResponse<Product>
                    {
                        Success = false,
                        Message = "Producto no encontrado o inactivo.",
                        Data = null
                    };
                }

                return new GeneralResponse<Product>
                {
                    Success = true,
                    Message = "Producto encontrado.",
                    Data = product
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<Product> 
                { 
                    Success = false, 
                    Message = ex.Message, 
                    Data = null 
                };
            }
        }
        public async Task<GeneralResponse<Product>> GetByQuery(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return new GeneralResponse<Product>
                {
                    Success = false,
                    Message = "El parámetro 'sku' en la URL es obligatorio.",
                    Data = null
                };
            }

            try
            {
                var product = await _repository.GetProductBySkuAsync(sku);

                if (product == null)
                {
                    return new GeneralResponse<Product>
                    {
                        Success = false,
                        Message = "Producto no encontrado o inactivo.",
                        Data = null
                    };
                }

                return new GeneralResponse<Product>
                {
                    Success = true,
                    Message = "Producto encontrado.",
                    Data = product
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<Product> 
                { 
                    Success = false, 
                    Message = ex.Message, 
                    Data = null 
                };
            }
        }
        public async Task<GeneralResponse<Product>> GetByBody(ProductSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
            {
                return new GeneralResponse<Product>
                {
                    Success = false,
                    Message = "El atributo 'sku' en el cuerpo (Body) de la petición es obligatorio.",
                    Data = null
                };
            }

            try
            {
                var product = await _repository.GetProductBySkuAsync(request.Sku);

                if (product == null)
                {
                    return new GeneralResponse<Product>
                    {
                        Success = false,
                        Message = "Producto no encontrado o inactivo.",
                        Data = null
                    };
                }

                return new GeneralResponse<Product>
                {
                    Success = true,
                    Message = "Producto encontrado.",
                    Data = product
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<Product> 
                { 
                    Success = false, 
                    Message = ex.Message, 
                    Data = null 
                };
            }
        }

        public GeneralResponse<UserInfoResponse> GetCurrentUserInfo()
        {
            if (!_userContextService.IsAuthenticated())
            {
                return new GeneralResponse<UserInfoResponse>
                {
                    Success = false,
                    Message = "No se detectó un usuario autenticado en el contexto actual.",
                    Data = null
                };
            }

            return new GeneralResponse<UserInfoResponse>
            {
                Success = true,
                Message = "Información del usuario autenticado extraída desde la capa de servicios.",
                Data = new UserInfoResponse
                {
                    UserId = _userContextService.GetUserId(),
                    Username = _userContextService.GetUsername() ?? string.Empty,
                    Role = _userContextService.GetRole() ?? string.Empty
                }
            };
        }
    }
}