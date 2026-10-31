using Microsoft.AspNetCore.Mvc;
using backend_II.Authentication;
using backend_II.Models;
using backend_II.Services;

namespace backend_II.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsServices _services;

        public ProductsController(ProductsServices services)
        {
            _services = services;
        }

        [HttpGet]
        [AuthorizeToken("Admin", "User")]
        public async Task<GeneralResponse<List<Product>>> GetAll()
        {
            return await _services.GetAll();
        }

        [HttpGet("{sku}")]
        [AuthorizeToken]
        public async Task<GeneralResponse<Product>> GetByRoute([FromRoute] string sku)
        {
            return await _services.GetByRoute(sku);
        }

        [HttpGet("search")]
        [ApiKey]
        public async Task<GeneralResponse<Product>> GetByQuery([FromQuery] string sku)
        {
            return await _services.GetByQuery(sku);
        }

        [HttpPost("search")]
        [AuthorizeApiKeyOrToken]
        public async Task<GeneralResponse<Product>> GetByBody([FromBody] ProductSearchRequest request)
        {
            return await _services.GetByBody(request);
        }

        // ===== INICIO ACTIVIDAD: BÚSQUEDA POR NOMBRE =====

        // GET: api/products/by-name?name=laptop
        [HttpGet("by-name")]
        public async Task<GeneralResponse<List<Product>>> GetByNameFromQuery([FromQuery] string? name)
        {
            return await _services.GetByName(name);
        }

        // POST: api/products/by-name     Body: { "name": "laptop" }
        [HttpPost("by-name")]
        public async Task<GeneralResponse<List<Product>>> GetByNameFromBody([FromBody] ProductSearchRequest? request)
        {
            // El operador ?. evita una excepción si el cuerpo llega vacío.
            return await _services.GetByName(request?.Name);
        }

        // ===== FIN ACTIVIDAD: BÚSQUEDA POR NOMBRE =====

        [HttpGet("whoami")]
        [AuthorizeToken]
        public GeneralResponse<UserInfoResponse> WhoAmI()
        {
            return _services.GetCurrentUserInfo();
        }
    }
}
