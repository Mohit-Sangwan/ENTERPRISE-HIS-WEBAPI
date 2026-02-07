using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ENTERPRISE_HIS_WEBAPI.Services
{
    /// <summary>
    /// Dual-token authentication service with separate Auth and Access tokens
    /// </summary>
    public interface IDualTokenService
    {
        /// <summary>
        /// Generate both authentication and access tokens
        /// </summary>
        DualTokenPair GenerateDualTokens(int userId, string userName, string email, List<string> roles);

        /// <summary>
        /// Validate authentication token
        /// </summary>
        bool ValidateAuthToken(string authToken);

        /// <summary>
        /// Validate access token
        /// </summary>
        bool ValidateAccessToken(string accessToken);

        /// <summary>
        /// Refresh access token using authentication token
        /// </summary>
        Task<AccessTokenResponse> RefreshAccessTokenAsync(string authToken);

        /// <summary>
        /// Get token expiration time in seconds
        /// </summary>
        int GetAuthTokenExpirationSeconds();
        int GetAccessTokenExpirationSeconds();
    }

    /// <summary>
    /// Dual token pair containing both auth and access tokens
    /// </summary>
    public class DualTokenPair
    {
        /// <summary>
        /// Authentication token (medium-lived, proves identity)
        /// </summary>
        public string AuthToken { get; set; } = string.Empty;

        /// <summary>
        /// Access token (short-lived, contains permissions)
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type (Bearer)
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Auth token expiration in seconds
        /// </summary>
        public int AuthExpiresIn { get; set; }

        /// <summary>
        /// Access token expiration in seconds
        /// </summary>
        public int AccessExpiresIn { get; set; }
    }

    /// <summary>
    /// Response from access token refresh operation
    /// </summary>
    public class AccessTokenResponse
    {
        /// <summary>
        /// Success flag
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// New access token
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Token type
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Access token expiration in seconds
        /// </summary>
        public int ExpiresIn { get; set; }
    }

    /// <summary>
    /// Dual token service implementation
    /// </summary>
    public class DualTokenService : IDualTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DualTokenService> _logger;
        private readonly int _authTokenExpirationMinutes;
        private readonly int _accessTokenExpirationMinutes;

        public DualTokenService(IConfiguration configuration, ILogger<DualTokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _authTokenExpirationMinutes = configuration.GetValue<int>("Jwt:AuthTokenExpirationMinutes", 60);
            _accessTokenExpirationMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 15);
        }

        /// <summary>
        /// Generate both authentication and access tokens
        /// </summary>
        public DualTokenPair GenerateDualTokens(int userId, string userName, string email, List<string> roles)
        {
            try
            {
                // Generate authentication token (proves identity)
                var authToken = GenerateAuthenticationToken(userId, userName, email);

                // Generate access token (contains permissions/roles)
                var accessToken = GenerateAccessToken(userId, userName, roles);

                _logger.LogInformation("Dual token pair generated for user {UserId} ({UserName})", userId, userName);

                return new DualTokenPair
                {
                    AuthToken = authToken,
                    AccessToken = accessToken,
                    TokenType = "Bearer",
                    AuthExpiresIn = GetAuthTokenExpirationSeconds(),
                    AccessExpiresIn = GetAccessTokenExpirationSeconds()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dual token pair for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Generate authentication token (identity only)
        /// </summary>
        private string GenerateAuthenticationToken(int userId, string userName, string email)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secret = jwtSettings.GetValue<string>("Secret");
            var issuer = jwtSettings.GetValue<string>("Issuer") ?? "enterprise-his";
            var audience = jwtSettings.GetValue<string>("Audience") ?? "enterprise-his-api";

            if (string.IsNullOrEmpty(secret))
                throw new InvalidOperationException("JWT Secret is not configured");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);

            // Auth token contains only identity information
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, email),
                new Claim("tokenType", "auth"),  // Mark as auth token
                new Claim("userId", userId.ToString()),
                new Claim("issuedAt", DateTime.UtcNow.ToString("O"))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_authTokenExpirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate access token (permissions/roles)
        /// </summary>
        private string GenerateAccessToken(int userId, string userName, List<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secret = jwtSettings.GetValue<string>("Secret");
            var issuer = jwtSettings.GetValue<string>("Issuer") ?? "enterprise-his";
            var audience = jwtSettings.GetValue<string>("Audience") ?? "enterprise-his-api";

            if (string.IsNullOrEmpty(secret))
                throw new InvalidOperationException("JWT Secret is not configured");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);

            // Access token contains permissions and roles
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim("tokenType", "access"),  // Mark as access token
                new Claim("userId", userId.ToString()),
                new Claim("issuedAt", DateTime.UtcNow.ToString("O"))
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate authentication token
        /// </summary>
        public bool ValidateAuthToken(string authToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(authToken) as JwtSecurityToken;

                if (token == null)
                    return false;

                var tokenType = token.Claims.FirstOrDefault(c => c.Type == "tokenType")?.Value;
                return tokenType == "auth" && token.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate access token
        /// </summary>
        public bool ValidateAccessToken(string accessToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(accessToken) as JwtSecurityToken;

                if (token == null)
                    return false;

                var tokenType = token.Claims.FirstOrDefault(c => c.Type == "tokenType")?.Value;
                return tokenType == "access" && token.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Refresh access token using authentication token
        /// </summary>
        public async Task<AccessTokenResponse> RefreshAccessTokenAsync(string authToken)
        {
            try
            {
                if (!ValidateAuthToken(authToken))
                {
                    return new AccessTokenResponse
                    {
                        Success = false,
                        Message = "Invalid or expired authentication token"
                    };
                }

                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(authToken) as JwtSecurityToken;

                if (token == null)
                {
                    return new AccessTokenResponse
                    {
                        Success = false,
                        Message = "Invalid authentication token"
                    };
                }

                var userId = int.Parse(token.Claims.FirstOrDefault(c => c.Type == "userId")?.Value ?? "0");
                var userName = token.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? "";
                var roles = token.Claims
                    .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role")
                    .Select(c => c.Value)
                    .ToList();

                // Generate new access token
                var newAccessToken = GenerateAccessToken(userId, userName, roles);

                _logger.LogInformation("Access token refreshed for user {UserId}", userId);

                return await Task.FromResult(new AccessTokenResponse
                {
                    Success = true,
                    Message = "Access token refreshed successfully",
                    AccessToken = newAccessToken,
                    TokenType = "Bearer",
                    ExpiresIn = GetAccessTokenExpirationSeconds()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing access token");
                return new AccessTokenResponse
                {
                    Success = false,
                    Message = "An error occurred while refreshing access token"
                };
            }
        }

        /// <summary>
        /// Get authentication token expiration time in seconds
        /// </summary>
        public int GetAuthTokenExpirationSeconds()
        {
            return _authTokenExpirationMinutes * 60;
        }

        /// <summary>
        /// Get access token expiration time in seconds
        /// </summary>
        public int GetAccessTokenExpirationSeconds()
        {
            return _accessTokenExpirationMinutes * 60;
        }
    }
}
