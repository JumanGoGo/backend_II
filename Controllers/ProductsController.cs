using Microsoft.AspNetCore.Mvc;
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

        // GET: api/products
        [HttpGet]
        public async Task<GeneralResponse<List<Product>>> GetAll()
        {
            return await _services.GetAll();
        }

        // GET: api/products/{sku}
        [HttpGet("{sku}")]
        //validaciones de tokens
        public async Task<GeneralResponse<Product>> GetByRoute([FromRoute] string sku)
        {
            //validaciones de claims
            return await _services.GetByRoute(sku);
        }

        // GET: api/products/search?sku=XYZ
        [HttpGet("search")]
        public async Task<GeneralResponse<Product>> GetByQuery([FromQuery] string sku)
        {
            return await _services.GetByQuery(sku);
        }

        // POST: api/products/search?name= alexis
        [HttpPost("search")]
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
    }
}
