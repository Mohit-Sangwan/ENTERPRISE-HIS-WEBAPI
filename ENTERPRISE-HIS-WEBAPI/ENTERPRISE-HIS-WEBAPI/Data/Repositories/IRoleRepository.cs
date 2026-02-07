using Enterprise.DAL.V1.Contracts;

namespace ENTERPRISE_HIS_WEBAPI.Data.Repositories
{
    /// <summary>
    /// Repository interface for role operations
    /// </summary>
    public interface IRoleRepository
    {
        Task<List<string>> GetUserRoleNamesAsync(int userId);
        Task<List<string>> GetRolePermissionsAsync(int roleId);
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<bool> UserHasPermissionAsync(int userId, string permissionCode);
        Task<bool> RoleHasPermissionAsync(int roleId, string permissionCode);
    }

    /// <summary>
    /// Repository for role and permission operations from database
    /// Uses stored procedures in config schema
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly ISqlServerDal _dal;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(ISqlServerDal dal, ILogger<RoleRepository> logger)
        {
            _dal = dal;
            _logger = logger;
        }

        /// <summary>
        /// Get all role names for a user
        /// SP: config.SP_GetUserRoles
        /// </summary>
        public async Task<List<string>> GetUserRoleNamesAsync(int userId)
        {
            try
            {
                var result = await _dal.QueryAsync<string>(
                    "config.SP_GetUserRoles",
                    row => row["RoleName"].ToString() ?? string.Empty,
                    new[] { DbParam.Input("@UserId", userId) },
                    isStoredProc: true);

                return result.Success ? result.Data.ToList() : new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles for user {UserId}", userId);
                return new List<string>();
            }
        }

        /// <summary>
        /// Get all permission names for a role
        /// SP: config.SP_GetRolePermissions
        /// </summary>
        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            try
            {
                var result = await _dal.QueryAsync<string>(
                    "config.SP_GetRolePermissions",
                    row => row["PermissionName"].ToString() ?? string.Empty,
                    new[] { DbParam.Input("@RoleId", roleId) },
                    isStoredProc: true);

                return result.Success ? result.Data.ToList() : new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching permissions for role {RoleId}", roleId);
                return new List<string>();
            }
        }

        /// <summary>
        /// Get all permissions for a user (through all their roles)
        /// SP: config.SP_GetUserPermissions
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                var result = await _dal.QueryAsync<string>(
                    "config.SP_GetUserPermissions",
                    row => row["PermissionName"].ToString() ?? string.Empty,
                    new[] { DbParam.Input("@UserId", userId) },
                    isStoredProc: true);

                return result.Success ? result.Data.ToList() : new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching permissions for user {UserId}", userId);
                return new List<string>();
            }
        }

        /// <summary>
        /// Check if user has specific permission
        /// SP: config.SP_CheckUserPermission
        /// </summary>
        public async Task<bool> UserHasPermissionAsync(int userId, string permissionCode)
        {
            try
            {
                // Use output parameter to get result
                var hasPermissionParam = DbParam.Output("@HasPermission", System.Data.SqlDbType.Bit);

                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_CheckUserPermission",
                    new[] 
                    { 
                        DbParam.Input("@UserId", userId),
                        DbParam.Input("@PermissionCode", permissionCode),
                        hasPermissionParam
                    },
                    isStoredProc: true);

                if (result.Success && hasPermissionParam.Value != null)
                {
                    return (bool)hasPermissionParam.Value;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Check if role has specific permission
        /// SP: config.SP_CheckRolePermission
        /// </summary>
        public async Task<bool> RoleHasPermissionAsync(int roleId, string permissionCode)
        {
            try
            {
                // Use output parameter to get result
                var hasPermissionParam = DbParam.Output("@HasPermission", System.Data.SqlDbType.Bit);

                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_CheckRolePermission",
                    new[] 
                    { 
                        DbParam.Input("@RoleId", roleId),
                        DbParam.Input("@PermissionCode", permissionCode),
                        hasPermissionParam
                    },
                    isStoredProc: true);

                if (result.Success && hasPermissionParam.Value != null)
                {
                    return (bool)hasPermissionParam.Value;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for role {RoleId}", roleId);
                return false;
            }
        }
    }
}
