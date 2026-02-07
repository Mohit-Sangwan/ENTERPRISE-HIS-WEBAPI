using System.Security.Claims;

namespace ENTERPRISE_HIS_WEBAPI.Extensions
{
    /// <summary>
    /// Extension methods for HttpContext to extract user information
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get User ID from JWT claims
        /// </summary>
        public static int GetUserId(this HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var id) ? id : 0;
        }

        /// <summary>
        /// Get User Name from JWT claims
        /// </summary>
        public static string GetUserName(this HttpContext context)
        {
            return context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        }

        /// <summary>
        /// Get User Email from JWT claims
        /// </summary>
        public static string GetUserEmail(this HttpContext context)
        {
            return context.User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
        }

        /// <summary>
        /// Get Client IP Address
        /// </summary>
        public static string GetClientIpAddress(this HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
                ipAddress = context.Connection.RemoteIpAddress?.ToString();

            return ipAddress ?? "Unknown";
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        public static bool HasRole(this HttpContext context, string role)
        {
            return context.User.IsInRole(role);
        }

        /// <summary>
        /// Check if user has any of the specified roles
        /// </summary>
        public static bool HasAnyRole(this HttpContext context, params string[] roles)
        {
            return roles.Any(role => context.User.IsInRole(role));
        }
    }
}
