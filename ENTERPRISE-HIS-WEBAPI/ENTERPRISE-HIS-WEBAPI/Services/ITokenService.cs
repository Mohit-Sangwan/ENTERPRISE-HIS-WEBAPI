using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ENTERPRISE_HIS_WEBAPI.Services
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate a JWT token for the specified user
        /// </summary>
        string GenerateToken(int userId, string userName, string email, List<string> roles);

        /// <summary>
        /// Get token expiration time in seconds
        /// </summary>
        int GetTokenExpirationSeconds();
    }

    /// <summary>
    /// Implementation of token generation using JWT
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generate JWT token with user claims
        /// </summary>
        public string GenerateToken(int userId, string userName, string email, List<string> roles)
        {
            try
            {
                // Get JWT settings from configuration
                var jwtSettings = _configuration.GetSection("Jwt");
                var secret = jwtSettings.GetValue<string>("Secret");
                var issuer = jwtSettings.GetValue<string>("Issuer") ?? "enterprise-his";
                var audience = jwtSettings.GetValue<string>("Audience") ?? "enterprise-his-api";
                var expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes", 60);

                if (string.IsNullOrEmpty(secret))
                    throw new InvalidOperationException("JWT Secret is not configured");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secret);

                // Build claims list
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Email, email),
                    new Claim("userId", userId.ToString()),  // Additional claim for convenience
                    new Claim("issuedAt", DateTime.UtcNow.ToString("O"))
                };

                // Add roles as claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Create token descriptor
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                // Create and write token
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);

                _logger.LogInformation("Token generated for user {UserId} ({UserName})", userId, userName);
                return jwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get token expiration time in seconds
        /// </summary>
        public int GetTokenExpirationSeconds()
        {
            var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
            return expirationMinutes * 60;  // Convert minutes to seconds
        }
    }
}
