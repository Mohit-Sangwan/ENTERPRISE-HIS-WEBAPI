using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ENTERPRISE_HIS_WEBAPI.Services
{
    /// <summary>
    /// Extended token service with refresh token support
    /// </summary>
    public interface ITokenServiceExtended : ITokenService
    {
        /// <summary>
        /// Generate both access and refresh tokens
        /// </summary>
        TokenPair GenerateTokenPair(int userId, string userName, string email, List<string> roles);

        /// <summary>
        /// Validate and refresh an access token using refresh token
        /// </summary>
        Task<TokenRefreshResponse> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revoke a refresh token (logout)
        /// </summary>
        Task<bool> RevokeTokenAsync(string refreshToken);

        /// <summary>
        /// Get refresh token expiration time in seconds
        /// </summary>
        int GetRefreshTokenExpirationSeconds();
    }

    /// <summary>
    /// Token pair containing both access and refresh tokens
    /// </summary>
    public class TokenPair
    {
        /// <summary>
        /// Access token for API requests (short-lived)
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token for getting new access tokens (long-lived)
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Type of access token (always "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Access token expiration in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Refresh token expiration in seconds
        /// </summary>
        public int RefreshExpiresIn { get; set; }
    }

    /// <summary>
    /// Response from token refresh operation
    /// </summary>
    public class TokenRefreshResponse
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
        /// New access token if successful
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// New refresh token if successful
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Token type
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Access token expiration in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Refresh token expiration in seconds
        /// </summary>
        public int RefreshExpiresIn { get; set; }
    }

    /// <summary>
    /// Stored refresh token information
    /// </summary>
    public class StoredRefreshToken
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The refresh token value
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When the token was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is token revoked
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// When token was revoked
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Associated access token (for tracking)
        /// </summary>
        public string? AssociatedAccessToken { get; set; }

        /// <summary>
        /// Replacement token (if rotated)
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Is token still valid
        /// </summary>
        public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }

    /// <summary>
    /// Repository for managing refresh tokens
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Store a refresh token
        /// </summary>
        Task<bool> StoreAsync(StoredRefreshToken token);

        /// <summary>
        /// Retrieve a refresh token by value
        /// </summary>
        Task<StoredRefreshToken?> GetAsync(string token);

        /// <summary>
        /// Revoke a token
        /// </summary>
        Task<bool> RevokeAsync(string token);

        /// <summary>
        /// Revoke all tokens for a user
        /// </summary>
        Task<bool> RevokeAllUserTokensAsync(int userId);

        /// <summary>
        /// Check if token is valid
        /// </summary>
        Task<bool> IsValidAsync(string token);

        /// <summary>
        /// Clean up expired tokens
        /// </summary>
        Task<int> CleanupExpiredTokensAsync();
    }

    /// <summary>
    /// In-memory implementation of refresh token repository
    /// Note: In production, use database storage
    /// </summary>
    public class InMemoryRefreshTokenRepository : IRefreshTokenRepository
    {
        private static readonly List<StoredRefreshToken> _tokens = new();
        private readonly ILogger<InMemoryRefreshTokenRepository> _logger;

        public InMemoryRefreshTokenRepository(ILogger<InMemoryRefreshTokenRepository> logger)
        {
            _logger = logger;
        }

        public Task<bool> StoreAsync(StoredRefreshToken token)
        {
            lock (_tokens)
            {
                _tokens.Add(token);
                _logger.LogInformation("Refresh token stored for user {UserId}", token.UserId);
                return Task.FromResult(true);
            }
        }

        public Task<StoredRefreshToken?> GetAsync(string token)
        {
            lock (_tokens)
            {
                return Task.FromResult(_tokens.FirstOrDefault(t => t.Token == token));
            }
        }

        public Task<bool> RevokeAsync(string token)
        {
            lock (_tokens)
            {
                var storedToken = _tokens.FirstOrDefault(t => t.Token == token);
                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    storedToken.RevokedAt = DateTime.UtcNow;
                    _logger.LogInformation("Token revoked for user {UserId}", storedToken.UserId);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
        }

        public Task<bool> RevokeAllUserTokensAsync(int userId)
        {
            lock (_tokens)
            {
                var userTokens = _tokens.Where(t => t.UserId == userId && !t.IsRevoked).ToList();
                foreach (var token in userTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }
                _logger.LogInformation("All tokens revoked for user {UserId}", userId);
                return Task.FromResult(userTokens.Count > 0);
            }
        }

        public Task<bool> IsValidAsync(string token)
        {
            lock (_tokens)
            {
                var storedToken = _tokens.FirstOrDefault(t => t.Token == token);
                return Task.FromResult(storedToken?.IsValid ?? false);
            }
        }

        public Task<int> CleanupExpiredTokensAsync()
        {
            lock (_tokens)
            {
                var beforeCount = _tokens.Count;
                _tokens.RemoveAll(t => t.ExpiresAt <= DateTime.UtcNow);
                var removedCount = beforeCount - _tokens.Count;
                _logger.LogInformation("Cleaned up {RemovedCount} expired tokens", removedCount);
                return Task.FromResult(removedCount);
            }
        }
    }

    /// <summary>
    /// Extended token service implementation with refresh token support
    /// </summary>
    public class TokenServiceExtended : TokenService, ITokenServiceExtended
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<TokenServiceExtended> _logger;
        private readonly int _refreshTokenExpirationDays;

        public TokenServiceExtended(
            IConfiguration configuration,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<TokenServiceExtended> logger)
            : base(configuration, logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
            _refreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
        }

        /// <summary>
        /// Generate both access and refresh tokens
        /// </summary>
        public TokenPair GenerateTokenPair(int userId, string userName, string email, List<string> roles)
        {
            // Generate access token
            var accessToken = GenerateToken(userId, userName, email, roles);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();

            // Store refresh token
            var storedToken = new StoredRefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
                AssociatedAccessToken = accessToken,
                CreatedAt = DateTime.UtcNow
            };

            _refreshTokenRepository.StoreAsync(storedToken).Wait();

            _logger.LogInformation("Token pair generated for user {UserId} ({UserName})", userId, userName);

            return new TokenPair
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = GetTokenExpirationSeconds(),
                RefreshExpiresIn = GetRefreshTokenExpirationSeconds()
            };
        }

        /// <summary>
        /// Refresh an access token using refresh token
        /// </summary>
        public async Task<TokenRefreshResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Validate refresh token is not empty
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return new TokenRefreshResponse
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    };
                }

                // Get stored token
                var storedToken = await _refreshTokenRepository.GetAsync(refreshToken);

                if (storedToken == null)
                {
                    _logger.LogWarning("Attempt to use non-existent refresh token");
                    return new TokenRefreshResponse
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    };
                }

                if (!storedToken.IsValid)
                {
                    _logger.LogWarning("Attempt to use invalid/expired refresh token for user {UserId}", storedToken.UserId);
                    return new TokenRefreshResponse
                    {
                        Success = false,
                        Message = "Refresh token has expired or been revoked"
                    };
                }

                // Extract claims from original access token to get user info
                // Note: In production, you'd retrieve user info from database
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ReadToken(storedToken.AssociatedAccessToken) as JwtSecurityToken;

                if (principal == null)
                {
                    return new TokenRefreshResponse
                    {
                        Success = false,
                        Message = "Invalid access token stored"
                    };
                }

                var userId = int.Parse(principal.Claims.FirstOrDefault(c => c.Type == "userId")?.Value ?? "0");
                var userName = principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? "";
                var email = principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ?? "";
                var roles = principal.Claims
                    .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role")
                    .Select(c => c.Value)
                    .ToList();

                // Generate new token pair
                var newTokenPair = GenerateTokenPair(userId, userName, email, roles);

                // Optionally rotate the refresh token
                await RotateRefreshTokenAsync(storedToken, newTokenPair.RefreshToken);

                _logger.LogInformation("Token refreshed for user {UserId}", userId);

                return new TokenRefreshResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = newTokenPair.AccessToken,
                    RefreshToken = newTokenPair.RefreshToken,
                    TokenType = newTokenPair.TokenType,
                    ExpiresIn = newTokenPair.ExpiresIn,
                    RefreshExpiresIn = newTokenPair.RefreshExpiresIn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new TokenRefreshResponse
                {
                    Success = false,
                    Message = "An error occurred while refreshing token"
                };
            }
        }

        /// <summary>
        /// Revoke a refresh token (logout)
        /// </summary>
        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var result = await _refreshTokenRepository.RevokeAsync(refreshToken);
            if (result)
            {
                _logger.LogInformation("Refresh token revoked");
            }
            return result;
        }

        /// <summary>
        /// Get refresh token expiration time in seconds
        /// </summary>
        public int GetRefreshTokenExpirationSeconds()
        {
            return _refreshTokenExpirationDays * 24 * 60 * 60;  // Convert days to seconds
        }

        /// <summary>
        /// Generate a cryptographically secure refresh token
        /// </summary>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Rotate refresh token for security (optional)
        /// </summary>
        private async Task RotateRefreshTokenAsync(StoredRefreshToken oldToken, string newToken)
        {
            oldToken.ReplacedByToken = newToken;
            oldToken.IsRevoked = true;
            oldToken.RevokedAt = DateTime.UtcNow;
            _logger.LogInformation("Refresh token rotated for user {UserId}", oldToken.UserId);
            await Task.CompletedTask;
        }
    }
}
