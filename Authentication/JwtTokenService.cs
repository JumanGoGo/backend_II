using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using backend_II.Models;

namespace backend_II.Authentication
{
    public class JwtTokenService : ITokenService
    {
        private readonly string _jwtKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expireMinutes;

        public JwtTokenService(IConfiguration configuration)
        {
            _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
                      ?? configuration["JWT_KEY"] 
                      ?? configuration["Jwt:Key"]
                      ?? throw new InvalidOperationException("La clave JWT (JWT_KEY) no está configurada en el archivo .env.");

            _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                      ?? configuration["JWT_ISSUER"] 
                      ?? configuration["Jwt:Issuer"] 
                      ?? "backend_II";

            _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                        ?? configuration["JWT_AUDIENCE"] 
                        ?? configuration["Jwt:Audience"] 
                        ?? "backend_II_Clients";

            var expireMinutesStr = Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES") 
                                   ?? configuration["JWT_EXPIRE_MINUTES"] 
                                   ?? configuration["Jwt:ExpireMinutes"] 
                                   ?? "60";

            if (!int.TryParse(expireMinutesStr, out _expireMinutes))
            {
                _expireMinutes = 60;
            }
        }

        public LoginResponse GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userId", user.UserId.ToString()),
                new Claim("username", user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiresAt = DateTime.UtcNow.AddMinutes(_expireMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes), 
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            return new LoginResponse
            {
                Token = tokenString,
                TokenType = "Bearer",
                ExpiresAt = expiresAt,
                ExpiresInMinutes = _expireMinutes,
                Username = user.Username,
                Role = user.Role
            };
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = !string.IsNullOrWhiteSpace(_issuer),
                ValidIssuer = _issuer,
                ValidateAudience = !string.IsNullOrWhiteSpace(_audience),
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
