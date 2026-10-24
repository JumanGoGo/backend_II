using Microsoft.AspNetCore.Mvc;
using backend_II.Models;
using backend_II.Services;

namespace backend_II.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public GeneralResponse<LoginResponse> Login([FromBody] LoginRequest request)
        {
            return _authService.Login(request);
        }
    }
}
