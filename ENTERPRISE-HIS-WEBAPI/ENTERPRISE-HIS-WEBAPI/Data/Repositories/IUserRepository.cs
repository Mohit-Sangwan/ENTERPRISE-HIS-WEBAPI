using Enterprise.DAL.V1.Contracts;
using ENTERPRISE_HIS_WEBAPI.Data.Dtos;

namespace ENTERPRISE_HIS_WEBAPI.Data.Repositories
{
    /// <summary>
    /// Repository interface for user operations
    /// </summary>
    public interface IUserRepository
    {
        Task<UserResponseDto?> GetUserByIdAsync(int userId);
        Task<UserResponseDto?> GetUserByUsernameAsync(string username);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
        Task<PaginatedUserResponseDto> GetUsersPagedAsync(int pageNumber, int pageSize);
        Task<int> CreateUserAsync(CreateUserDto createUserDto, string passwordHash);
        Task<bool> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> UpdatePasswordAsync(int userId, string passwordHash);
        Task<bool> AssignRoleToUserAsync(int userId, int roleId);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
        Task<bool> UserExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<string?> GetPasswordHashAsync(string username);
    }

    /// <summary>
    /// Repository for user operations from database
    /// Uses stored procedures in config schema
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ISqlServerDal _dal;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ISqlServerDal dal, ILogger<UserRepository> logger)
        {
            _dal = dal;
            _logger = logger;
        }

        /// <summary>
        /// Get user by ID with roles and permissions
        /// </summary>
        public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                var result = await _dal.QueryAsync<UserResponseDto>(
                    "config.SP_GetUserById",
                    row => MapUserRow(row),
                    new[] { DbParam.Input("@UserId", userId) },
                    isStoredProc: true);

                if (!result.Success || !result.Data.Any())
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return null;
                }

                var user = result.Data.First();

                // Get roles and permissions
                var roles = await GetUserRolesAsync(userId);
                var permissions = await GetUserPermissionsAsync(userId);

                user.Roles = roles;
                user.Permissions = permissions;

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Get user by username with roles and permissions
        /// </summary>
        public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var result = await _dal.QueryAsync<UserResponseDto>(
                    "config.SP_GetUserByUsername",
                    row => MapUserRow(row),
                    new[] { DbParam.Input("@Username", username) },
                    isStoredProc: true);

                if (!result.Success || !result.Data.Any())
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return null;
                }

                var user = result.Data.First();

                var roles = await GetUserRolesAsync(user.UserId);
                var permissions = await GetUserPermissionsAsync(user.UserId);

                user.Roles = roles;
                user.Permissions = permissions;

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by username {Username}", username);
                return null;
            }
        }

        /// <summary>
        /// Get user by email with roles and permissions
        /// </summary>
        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var result = await _dal.QueryAsync<UserResponseDto>(
                    "config.SP_GetUserByEmail",
                    row => MapUserRow(row),
                    new[] { DbParam.Input("@Email", email) },
                    isStoredProc: true);

                if (!result.Success || !result.Data.Any())
                {
                    _logger.LogWarning("User not found: {Email}", email);
                    return null;
                }

                var user = result.Data.First();

                var roles = await GetUserRolesAsync(user.UserId);
                var permissions = await GetUserPermissionsAsync(user.UserId);

                user.Roles = roles;
                user.Permissions = permissions;

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by email {Email}", email);
                return null;
            }
        }

        /// <summary>
        /// Get paginated list of users
        /// </summary>
        public async Task<PaginatedUserResponseDto> GetUsersPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var result = await _dal.QueryAsync<UserResponseDto>(
                    "config.SP_GetAllUsers",
                    row => MapUserRow(row),
                    new[]
                    {
                        DbParam.Input("@PageNumber", pageNumber),
                        DbParam.Input("@PageSize", pageSize)
                    },
                    isStoredProc: true);

                if (!result.Success)
                {
                    _logger.LogError("Error fetching paginated users");
                    return new PaginatedUserResponseDto();
                }

                var users = result.Data.ToList();
                var totalCount = users.FirstOrDefault()?.UserId ?? 0; // Adjust based on actual SP response

                return new PaginatedUserResponseDto
                {
                    Data = users,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = pageNumber > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated users at page {PageNumber}", pageNumber);
                return new PaginatedUserResponseDto();
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<int> CreateUserAsync(CreateUserDto createUserDto, string passwordHash)
        {
            try
            {
                var result = await _dal.ExecuteScalarAsync<int>(
                    "config.SP_CreateUser",
                    new[]
                    {
                        DbParam.Input("@Username", createUserDto.Username),
                        DbParam.Input("@Email", createUserDto.Email),
                        DbParam.Input("@PasswordHash", passwordHash),
                        DbParam.Input("@FirstName", createUserDto.FirstName),
                        DbParam.Input("@LastName", createUserDto.LastName),
                        DbParam.Input("@IsActive", createUserDto.IsActive)
                    },
                    isStoredProc: true);

                if (!result.Success || result.Data <= 0)
                {
                    _logger.LogError("Failed to create user: {Username}", createUserDto.Username);
                    return 0;
                }

                _logger.LogInformation("User created successfully: {Username} with UserId: {UserId}", 
                    createUserDto.Username, result.Data);
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", createUserDto.Username);
                return 0;
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_UpdateUser",
                    new[]
                    {
                        DbParam.Input("@UserId", userId),
                        DbParam.Input("@FirstName", updateUserDto.FirstName ?? (object)DBNull.Value),
                        DbParam.Input("@LastName", updateUserDto.LastName ?? (object)DBNull.Value),
                        DbParam.Input("@Email", updateUserDto.Email ?? (object)DBNull.Value),
                        DbParam.Input("@IsActive", updateUserDto.IsActive ?? (object)DBNull.Value)
                    },
                    isStoredProc: true);

                if (!result.Success)
                {
                    _logger.LogError("Failed to update user: {UserId}", userId);
                    return false;
                }

                _logger.LogInformation("User updated successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Delete user (hard delete)
        /// </summary>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_DeleteUser",
                    new[] { DbParam.Input("@UserId", userId) },
                    isStoredProc: true);

                if (!result.Success)
                {
                    _logger.LogError("Failed to delete user: {UserId}", userId);
                    return false;
                }

                _logger.LogInformation("User deleted successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Deactivate user (soft delete)
        /// </summary>
        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_DeactivateUser",
                    new[] { DbParam.Input("@UserId", userId) },
                    isStoredProc: true);

                if (!result.Success)
                {
                    _logger.LogError("Failed to deactivate user: {UserId}", userId);
                    return false;
                }

                _logger.LogInformation("User deactivated successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Activate user
        /// </summary>
        public async Task<bool> ActivateUserAsync(int userId)
        {
            try
            {
                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_ActivateUser",
                    new[] { DbParam.Input("@UserId", userId) },
                    isStoredProc: true);

                if (!result.Success)
                {
                    _logger.LogError("Failed to activate user: {UserId}", userId);
                    return false;
                }

                _logger.LogInformation("User activated successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Update user password
        /// </summary>
        public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash)
        {
            try
            {
                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_UpdateUserPassword",
                    new[]
                    {
                        DbParam.Input("@UserId", userId),
                        DbParam.Input("@PasswordHash", passwordHash)
                    },
                    isStoredProc: true);

                if (!result.Success)
                {
                    _logger.LogError("Failed to update password for user: {UserId}", userId);
                    return false;
                }

                _logger.LogInformation("Password updated successfully for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            try
            {
                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_AssignRoleToUser",
                    new[]
                    {
                        DbParam.Input("@UserId", userId),
                        DbParam.Input("@RoleId", roleId)
                    },
                    isStoredProc: true);

                if (!result.Success)
                {
                    _logger.LogError("Failed to assign role {RoleId} to user {UserId}", roleId, userId);
                    return false;
                }

                _logger.LogInformation("Role {RoleId} assigned to user {UserId}", roleId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Remove role from user
        /// </summary>
        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            try
            {
                var result = await _dal.ExecuteNonQueryAsync(
                    "config.SP_RemoveRoleFromUser",
                    new[]
                    {
                        DbParam.Input("@UserId", userId),
                        DbParam.Input("@RoleId", roleId)
                    },
                    isStoredProc: true);

                if (!result.Success)
                {
                    _logger.LogError("Failed to remove role {RoleId} from user {UserId}", roleId, userId);
                    return false;
                }

                _logger.LogInformation("Role {RoleId} removed from user {UserId}", roleId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        public async Task<bool> UserExistsAsync(string username)
        {
            try
            {
                var result = await _dal.ExecuteScalarAsync<int>(
                    "config.SP_CheckUsernameExists",
                    new[] { DbParam.Input("@Username", username) },
                    isStoredProc: true);

                return result.Success && result.Data > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists: {Username}", username);
                return false;
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                var result = await _dal.ExecuteScalarAsync<int>(
                    "config.SP_CheckEmailExists",
                    new[] { DbParam.Input("@Email", email) },
                    isStoredProc: true);

                return result.Success && result.Data > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email exists: {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Get password hash for user (for authentication)
        /// </summary>
        public async Task<string?> GetPasswordHashAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return null;

                var result = await _dal.QueryAsync<string>(
                    "SELECT [PasswordHash] FROM [core].[UserAccount] WHERE [Username] = @Username AND [IsActive] = 1",
                    row => row["PasswordHash"].ToString() ?? string.Empty,
                    new[] { DbParam.Input("@Username", username) },
                    isStoredProc: false);

                if (!result.Success || !result.Data.Any())
                {
                    _logger.LogWarning("Password hash not found for user: {Username}", username);
                    return null;
                }

                return result.Data.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving password hash for user: {Username}", username);
                return null;
            }
        }

        // Helper methods

        private async Task<List<string>> GetUserRolesAsync(int userId)
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

        private async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                var result = await _dal.QueryAsync<string>(
                    "config.SP_GetUserPermissions",
                    row => row["PermissionCode"].ToString() ?? string.Empty,
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

        private UserResponseDto MapUserRow(System.Data.DataRow row)
        {
            return new UserResponseDto
            {
                UserId = (int)row["UserId"],
                Username = row["Username"].ToString() ?? string.Empty,
                Email = row["Email"].ToString() ?? string.Empty,
                FirstName = row["FirstName"].ToString() ?? string.Empty,
                LastName = row["LastName"].ToString() ?? string.Empty,
                IsActive = (bool)row["IsActive"],
                CreatedDate = (DateTime)row["CreatedDate"],
                ModifiedDate = row["ModifiedDate"] != DBNull.Value ? (DateTime?)row["ModifiedDate"] : null
            };
        }
    }
}
