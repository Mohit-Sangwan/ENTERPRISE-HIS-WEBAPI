using ENTERPRISE_HIS_WEBAPI.Data.Repositories;

namespace ENTERPRISE_HIS_WEBAPI.Services
{
    /// <summary>
    /// Service for dynamic authorization based on database roles and permissions
    /// </summary>
    public interface IDynamicAuthorizationService
    {
        Task<bool> IsUserAuthorizedAsync(int userId, string requiredPermission);
        Task<bool> IsUserInRoleAsync(int userId, string roleName);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<List<string>> GetUserPermissionsAsync(int userId);
    }

    /// <summary>
    /// Implementation of authorization service using database
    /// </summary>
    public class DynamicAuthorizationService : IDynamicAuthorizationService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<DynamicAuthorizationService> _logger;

        public DynamicAuthorizationService(IRoleRepository roleRepository, ILogger<DynamicAuthorizationService> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Check if user has required permission
        /// </summary>
        public async Task<bool> IsUserAuthorizedAsync(int userId, string requiredPermission)
        {
            try
            {
                var isAuthorized = await _roleRepository.UserHasPermissionAsync(userId, requiredPermission);
                
                if (!isAuthorized)
                    _logger.LogWarning("User {UserId} denied access to permission {Permission}", userId, requiredPermission);
                
                return isAuthorized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authorization for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        public async Task<bool> IsUserInRoleAsync(int userId, string roleName)
        {
            try
            {
                var roles = await _roleRepository.GetUserRoleNamesAsync(userId);
                return roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            try
            {
                return await _roleRepository.GetUserRoleNamesAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles for user {UserId}", userId);
                return new List<string>();
            }
        }

        /// <summary>
        /// Get all permissions for a user
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                return await _roleRepository.GetUserPermissionsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching permissions for user {UserId}", userId);
                return new List<string>();
            }
        }
    }
}
