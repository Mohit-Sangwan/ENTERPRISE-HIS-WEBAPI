namespace ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise.Services
{
    using ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise;
    using Microsoft.Extensions.Caching.Memory;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Permission data model
    /// </summary>
    public class Permission
    {
        public int PermissionId { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string? Scope { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OperationCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        /// <summary>
        /// Get full permission string
        /// </summary>
        public string GetPermissionString()
        {
            var permission = $"{Module}.{Resource}.{Operation}";
            if (!string.IsNullOrWhiteSpace(Scope))
                permission += $".{Scope}";
            return permission;
        }
    }

    /// <summary>
    /// Service for managing permissions
    /// Queries database + caches results
    /// </summary>
    public interface IEnterprisePermissionService
    {
        /// <summary>
        /// Get all permissions for a user (from their roles)
        /// </summary>
        Task<List<string>> GetUserPermissionsAsync(int userId);

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        Task<bool> UserHasPermissionAsync(int userId, string permission);

        /// <summary>
        /// Get all active permissions
        /// </summary>
        Task<List<Permission>> GetAllPermissionsAsync();

        /// <summary>
        /// Get permissions by module
        /// </summary>
        Task<List<Permission>> GetPermissionsByModuleAsync(string module);

        /// <summary>
        /// Get permissions for a role
        /// </summary>
        Task<List<Permission>> GetRolePermissionsAsync(int roleId);

        /// <summary>
        /// Invalidate cache (call when permissions change)
        /// </summary>
        void InvalidateCache(int userId = 0);
    }

    /// <summary>
    /// Production implementation
    /// Queries database + caches for 1 hour
    /// Note: For database queries, inject IPermissionRepository or similar
    /// </summary>
    public class EnterprisePermissionService : IEnterprisePermissionService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<EnterprisePermissionService> _logger;

        // Cache keys
        private const string UserPermissionsCacheKeyPrefix = "user_permissions_";
        private const string AllPermissionsCacheKey = "all_permissions";
        private const string RolePermissionsCacheKeyPrefix = "role_permissions_";
        private const int CacheDurationMinutes = 60;

        public EnterprisePermissionService(
            IMemoryCache cache,
            ILogger<EnterprisePermissionService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get all permissions for a user (from their roles)
        /// Returns list of permission strings like "Lookups.LookupType.View"
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var cacheKey = $"{UserPermissionsCacheKeyPrefix}{userId}";

            // Try cache first
            if (_cache.TryGetValue(cacheKey, out List<string>? cached))
            {
                _logger.LogDebug("User permissions cache hit for UserId={UserId}", userId);
                return cached ?? new List<string>();
            }

            // TODO: Phase 3 - Query database
            // Query should: Get all permissions for user's roles
            // Query database:
            /*
            SELECT DISTINCT CONCAT(p.Module, '.', p.Resource, '.', p.Operation)
            FROM master.PermissionMaster p
            INNER JOIN config.RolePermissionMapping rpm ON p.PermissionId = rpm.PermissionId
            INNER JOIN master.UserRoleMaster ur ON rpm.RoleId = ur.RoleId
            WHERE ur.UserId = @UserId AND p.IsActive = 1
            */

            _logger.LogDebug("Querying user permissions for UserId={UserId}", userId);

            // Placeholder: Return empty for now
            var result = new List<string>();

            // Cache results
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

            _cache.Set(cacheKey, result, cacheOptions);

            _logger.LogInformation(
                "Loaded {Count} permissions for UserId={UserId} from database",
                result.Count, userId);

            return result;
        }

        /// <summary>
        /// Check if user has specific permission
        /// Supports wildcard matching
        /// </summary>
        public async Task<bool> UserHasPermissionAsync(int userId, string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return false;

            var userPermissions = await GetUserPermissionsAsync(userId);

            // Exact match
            if (userPermissions.Contains(permission))
                return true;

            // Wildcard match
            foreach (var userPerm in userPermissions)
            {
                if (PermissionBuilder.Matches(permission, userPerm))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get all active permissions
        /// </summary>
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            // Try cache
            if (_cache.TryGetValue(AllPermissionsCacheKey, out List<Permission>? cached))
            {
                _logger.LogDebug("All permissions cache hit");
                return cached ?? new List<Permission>();
            }

            // TODO: Phase 3 - Query database
            _logger.LogDebug("Querying all permissions from database");

            // Placeholder result
            var permissions = new List<Permission>();

            // Cache
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

            _cache.Set(AllPermissionsCacheKey, permissions, cacheOptions);

            _logger.LogInformation("Loaded {Count} permissions from database", permissions.Count);

            return permissions;
        }

        /// <summary>
        /// Get permissions by module
        /// </summary>
        public async Task<List<Permission>> GetPermissionsByModuleAsync(string module)
        {
            if (string.IsNullOrWhiteSpace(module))
                return new List<Permission>();

            var allPermissions = await GetAllPermissionsAsync();
            return allPermissions
                .Where(p => p.Module.Equals(module, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Get permissions for a role
        /// </summary>
        public async Task<List<Permission>> GetRolePermissionsAsync(int roleId)
        {
            var cacheKey = $"{RolePermissionsCacheKeyPrefix}{roleId}";

            // Try cache
            if (_cache.TryGetValue(cacheKey, out List<Permission>? cached))
            {
                _logger.LogDebug("Role permissions cache hit for RoleId={RoleId}", roleId);
                return cached ?? new List<Permission>();
            }

            // TODO: Phase 3 - Query database
            _logger.LogDebug("Querying permissions for RoleId={RoleId}", roleId);

            // Placeholder result
            var permissions = new List<Permission>();

            // Cache
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

            _cache.Set(cacheKey, permissions, cacheOptions);

            _logger.LogInformation(
                "Loaded {Count} permissions for RoleId={RoleId}",
                permissions.Count, roleId);

            return permissions;
        }

        /// <summary>
        /// Invalidate cache
        /// Call this when permissions are changed, roles are updated, etc.
        /// </summary>
        public void InvalidateCache(int userId = 0)
        {
            if (userId > 0)
            {
                var cacheKey = $"{UserPermissionsCacheKeyPrefix}{userId}";
                _cache.Remove(cacheKey);
                _logger.LogInformation("Invalidated permission cache for UserId={UserId}", userId);
            }
            else
            {
                // Invalidate all caches
                _cache.Remove(AllPermissionsCacheKey);
                _logger.LogInformation("Invalidated all permission caches");
            }
        }
    }

    /// <summary>
    /// In-memory mock implementation for testing
    /// </summary>
    public class MockEnterprisePermissionService : IEnterprisePermissionService
    {
        private readonly List<Permission> _mockPermissions;
        private readonly Dictionary<int, List<int>> _rolePermissions; // roleId -> permissionIds

        public MockEnterprisePermissionService()
        {
            _mockPermissions = new List<Permission>
            {
                new Permission
                {
                    PermissionId = 1,
                    Module = "Lookups",
                    Resource = "LookupType",
                    Operation = "View",
                    PermissionCode = "LOOKUPS_LOOKUPTYPE_VIEW",
                    Description = "Can view lookups",
                    IsActive = true
                },
                new Permission
                {
                    PermissionId = 2,
                    Module = "Lookups",
                    Resource = "LookupType",
                    Operation = "Create",
                    PermissionCode = "LOOKUPS_LOOKUPTYPE_CREATE",
                    Description = "Can create lookups",
                    IsActive = true
                }
            };

            _rolePermissions = new Dictionary<int, List<int>>
            {
                { 1, new List<int> { 1, 2 } } // Admin role has all
            };
        }

        public Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var permissions = _mockPermissions
                .Select(p => p.GetPermissionString())
                .ToList();

            return Task.FromResult(permissions);
        }

        public Task<bool> UserHasPermissionAsync(int userId, string permission)
        {
            return Task.FromResult(_mockPermissions.Any(p => p.GetPermissionString() == permission));
        }

        public Task<List<Permission>> GetAllPermissionsAsync()
        {
            return Task.FromResult(_mockPermissions);
        }

        public Task<List<Permission>> GetPermissionsByModuleAsync(string module)
        {
            var result = _mockPermissions
                .Where(p => p.Module == module)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<List<Permission>> GetRolePermissionsAsync(int roleId)
        {
            if (_rolePermissions.TryGetValue(roleId, out var permIds))
            {
                var result = _mockPermissions
                    .Where(p => permIds.Contains(p.PermissionId))
                    .ToList();

                return Task.FromResult(result);
            }

            return Task.FromResult(new List<Permission>());
        }

        public void InvalidateCache(int userId = 0)
        {
            // No-op for mock
        }
    }
}
