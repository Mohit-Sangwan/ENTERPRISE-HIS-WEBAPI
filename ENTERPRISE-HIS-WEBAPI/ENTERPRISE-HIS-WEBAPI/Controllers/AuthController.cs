using Microsoft.AspNetCore.Mvc;
using ENTERPRISE_HIS_WEBAPI.Services;
using ENTERPRISE_HIS_WEBAPI.Extensions;
using ENTERPRISE_HIS_WEBAPI.Authentication;

namespace ENTERPRISE_HIS_WEBAPI.Controllers
{
    /// <summary>
    /// Authentication controller for user login and token generation
    /// Authorization: Middleware automatically bypasses public endpoints (/auth/login, /auth/health, etc.)
    /// No [Authorize] attributes needed - middleware manages public endpoint exceptions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ITokenService _tokenService;
        private readonly ITokenServiceExtended? _tokenServiceExtended;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticationService authenticationService,
            ITokenService tokenService,
            ILogger<AuthController> logger,
            ITokenServiceExtended? tokenServiceExtended = null)
        {
            _authenticationService = authenticationService;
            _tokenService = tokenService;
            _tokenServiceExtended = tokenServiceExtended;
            _logger = logger;
        }

        /// <summary>
        /// Login and get JWT authentication token with refresh token
        /// Public endpoint - no authorization required
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Login attempt with missing credentials");
                    return BadRequest(new
                    {
                        error = "Username and password are required",
                        timestamp = DateTime.UtcNow
                    });
                }

                var (success, message, user) = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

                if (!success || user == null)
                {
                    _logger.LogWarning("Login attempt failed: {Message}", message);
                    return Unauthorized(new
                    {
                        error = message,
                        timestamp = DateTime.UtcNow
                    });
                }

                string accessToken;
                string? refreshToken = null;
                int refreshExpiresIn = 0;

                if (_tokenServiceExtended != null)
                {
                    var tokenPair = _tokenServiceExtended.GenerateTokenPair(
                        userId: user.UserId,
                        userName: user.Username,
                        email: user.Email,
                        roles: user.Roles);

                    accessToken = tokenPair.AccessToken;
                    refreshToken = tokenPair.RefreshToken;
                    refreshExpiresIn = tokenPair.RefreshExpiresIn;
                }
                else
                {
                    accessToken = _tokenService.GenerateToken(
                        userId: user.UserId,
                        userName: user.Username,
                        email: user.Email,
                        roles: user.Roles);
                }

                _logger.LogInformation("User {Username} logged in successfully", request.Username);

                return Ok(new LoginResponse
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = _tokenService.GetTokenExpirationSeconds(),
                    RefreshExpiresIn = refreshExpiresIn,
                    User = new UserInfo
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        Roles = user.Roles
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get current user information from JWT token
        /// Permission: Automatically enforced by middleware
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CurrentUserResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<CurrentUserResponse> GetCurrentUser()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                var email = HttpContext.GetUserEmail();
                var roles = User.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    .Select(c => c.Value)
                    .ToList();

                return Ok(new CurrentUserResponse
                {
                    UserId = userId,
                    Username = userName,
                    Email = email,
                    Roles = roles,
                    AuthenticatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Health check endpoint for token validation
        /// Public endpoint - no authorization required
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                message = "Authentication service is operational"
            });
        }

        /// <summary>
        /// Refresh an access token using refresh token
        /// Public endpoint - no authorization required
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenRefreshResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TokenRefreshResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (_tokenServiceExtended == null)
                {
                    _logger.LogWarning("Refresh token endpoint called but extended token service not configured");
                    return BadRequest(new TokenRefreshResponse
                    {
                        Success = false,
                        Message = "Refresh token service not available"
                    });
                }

                if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    _logger.LogWarning("Refresh token attempt with missing token");
                    return BadRequest(new TokenRefreshResponse
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    });
                }

                var response = await _tokenServiceExtended.RefreshTokenAsync(request.RefreshToken);

                if (!response.Success)
                {
                    _logger.LogWarning("Token refresh failed: {Message}", response.Message);
                    return Unauthorized(response);
                }

                _logger.LogInformation("Token refreshed successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new TokenRefreshResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Logout and revoke refresh token
        /// Permission: Automatically enforced by middleware (requires authentication)
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();

                if (_tokenServiceExtended != null && !string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    await _tokenServiceExtended.RevokeTokenAsync(request.RefreshToken);
                }

                _logger.LogInformation("User {UserId} ({UserName}) logged out", userId, userName);

                return Ok(new
                {
                    message = "Logout successful",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Login request model
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Username for login
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password for login
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login response model with token and refresh token
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT access token (short-lived)
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token for getting new access tokens (long-lived)
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Token type (always "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Access token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Refresh token expiration time in seconds
        /// </summary>
        public int RefreshExpiresIn { get; set; }

        /// <summary>
        /// Authenticated user information
        /// </summary>
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// User information model
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User roles
        /// </summary>
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// Current user information response
    /// </summary>
    public class CurrentUserResponse
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User roles
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// When user was authenticated
        /// </summary>
        public DateTime AuthenticatedAt { get; set; }
    }

    /// <summary>
    /// Refresh token request model
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Refresh token to use for getting new access token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Logout request model
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// Refresh token to revoke
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
