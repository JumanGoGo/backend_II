using backend_II.Authentication;
using backend_II.Database;
using backend_II.Models;
using backend_II.Utils;

namespace backend_II.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthService(UserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public GeneralResponse<LoginResponse> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new GeneralResponse<LoginResponse>
                {
                    Success = false,
                    Message = "El nombre de usuario y la contraseña son obligatorios.",
                };
            }

            try
            {
                var userResponse = _userRepository.GetUserByUsername(request.Username);

                if (!userResponse.Success)
                {
                    return new GeneralResponse<LoginResponse>
                    {
                        Success = false,
                        Message = userResponse.Message,
                    };
                }

                User user = userResponse.Data!;

                if (!PasswordUtils.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return new GeneralResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Usuario o contraseña incorrectos.",
                    };
                }

                LoginResponse loginResponse = _tokenService.GenerateToken(user);

                return new GeneralResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Inicio de sesión exitoso.",
                    Data = loginResponse
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"Error al procesar el inicio de sesión: {ex.Message}",
                };
            }
        }
    }
}
