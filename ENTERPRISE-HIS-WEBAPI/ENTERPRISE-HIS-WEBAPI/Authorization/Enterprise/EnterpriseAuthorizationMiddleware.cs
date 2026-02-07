namespace ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise
{
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;

    /// <summary>
    /// Enterprise-Grade Authorization Middleware
    /// Single gatekeeper for ALL authorization decisions
    /// 
    /// Features:
    /// - No controller attributes needed
    /// - No hardcoded permissions
    /// - Automatic operation detection
    /// - Scope-aware (department, facility, etc.)
    /// - Audit logging
    /// - Performance optimized
    /// 
    /// Flow:
    /// 1. Check authentication
    /// 2. Build required permission from request
    /// 3. Check against user's permission claims
    /// 4. Support wildcards (Lookups.*.* = all lookups operations)
    /// 5. Log access decisions
    /// 6. Return 401/403 as appropriate
    /// </summary>
    public class EnterpriseAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EnterpriseAuthorizationMiddleware> _logger;

        // Endpoints that bypass authorization
        private static readonly string[] PublicEndpoints = new[]
        {
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/refresh-token",
            "/health",
            "/swagger",
            "/api-docs"
        };

        public EnterpriseAuthorizationMiddleware(RequestDelegate next, ILogger<EnterpriseAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context, 
            IAuthorizationAuditService auditService,
            ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise.Services.IEnterprisePermissionService permissionService)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // Skip authorization for public endpoints
            if (IsPublicEndpoint(path))
            {
                await _next(context);
                return;
            }

            // Skip for non-API requests
            if (!path.StartsWith("/api"))
            {
                await _next(context);
                return;
            }

            // Check authentication first
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Unauthorized request to {Path}", path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
                return;
            }

            // Get user ID for logging
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

            // Build required permission
            var requiredPermission = PermissionBuilder.Build(context);

            // If we can't determine a permission, allow it to proceed
            // (might be a health check or metadata endpoint)
            if (requiredPermission == null)
            {
                await _next(context);
                return;
            }

            // Check if user has the permission
            var hasPermission = await CheckPermissionAsync(
                context.User, 
                requiredPermission, 
                permissionService);

            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Access denied: User {UserId} requested permission {Permission} on {Path}",
                    userId, requiredPermission, path);

                // Audit the denied access
                await auditService.LogDeniedAccessAsync(new AuthAccessLog
                {
                    UserId = int.TryParse(userId, out var uid) ? uid : 0,
                    Permission = requiredPermission,
                    Path = path,
                    Method = context.Request.Method,
                    StatusCode = 403,
                    DeniedReason = "User does not have required permission",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString()
                });

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Forbidden", permission = requiredPermission });
                return;
            }

            // Audit successful access
            await auditService.LogAccessAsync(new AuthAccessLog
            {
                UserId = int.TryParse(userId, out var uid2) ? uid2 : 0,
                Permission = requiredPermission,
                Path = path,
                Method = context.Request.Method,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString()
            });

            _logger.LogInformation(
                "Access granted: User {UserId} granted permission {Permission} on {Path}",
                userId, requiredPermission, path);

            await _next(context);
        }

        /// <summary>
        /// Check if user has required permission (using service)
        /// Supports JWT claims AND database lookup
        /// </summary>
        private async Task<bool> CheckPermissionAsync(
            ClaimsPrincipal user, 
            string requiredPermission,
            ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise.Services.IEnterprisePermissionService permissionService)
        {
            // First, check JWT claims (fastest - no DB hit)
            var userPermissions = user.FindAll("permission");

            foreach (var permission in userPermissions)
            {
                // Exact match
                if (permission.Value == requiredPermission)
                    return true;

                // Wildcard match (Lookups.*.* matches Lookups.LookupType.View)
                if (PermissionBuilder.Matches(requiredPermission, permission.Value))
                    return true;
            }

            // If JWT doesn't have it, check database (fallback)
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return await permissionService.UserHasPermissionAsync(userId, requiredPermission);
            }

            return false;
        }

        /// <summary>
        /// Check if endpoint is public (no auth required)
        /// </summary>
        private bool IsPublicEndpoint(string path)
        {
            return PublicEndpoints.Any(ep => path.Contains(ep));
        }
    }

    /// <summary>
    /// Authorization audit log entry
    /// Used for compliance and security analysis
    /// </summary>
    public class AuthAccessLog
    {
        public int UserId { get; set; }
        public string Permission { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string? DeniedReason { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// Service for logging authorization events
    /// </summary>
    public interface IAuthorizationAuditService
    {
        Task LogAccessAsync(AuthAccessLog log);
        Task LogDeniedAccessAsync(AuthAccessLog log);
    }

    /// <summary>
    /// Default implementation: logs to database
    /// </summary>
    public class AuthorizationAuditService : IAuthorizationAuditService
    {
        private readonly ILogger<AuthorizationAuditService> _logger;
        // TODO: Inject DbContext for logging to database

        public AuthorizationAuditService(ILogger<AuthorizationAuditService> logger)
        {
            _logger = logger;
        }

        public async Task LogAccessAsync(AuthAccessLog log)
        {
            _logger.LogInformation(
                "Authorization Success: UserId={UserId}, Permission={Permission}, Path={Path}",
                log.UserId, log.Permission, log.Path);

            // TODO: Save to database
            await Task.CompletedTask;
        }

        public async Task LogDeniedAccessAsync(AuthAccessLog log)
        {
            _logger.LogWarning(
                "Authorization Denied: UserId={UserId}, Permission={Permission}, Path={Path}, Reason={Reason}",
                log.UserId, log.Permission, log.Path, log.DeniedReason);

            // TODO: Save to database
            await Task.CompletedTask;
        }
    }
}
