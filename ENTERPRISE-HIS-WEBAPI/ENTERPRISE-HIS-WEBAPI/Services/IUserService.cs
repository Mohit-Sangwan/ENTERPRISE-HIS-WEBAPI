using System.Security.Cryptography;
using System.Text;
using ENTERPRISE_HIS_WEBAPI.Data.Dtos;
using ENTERPRISE_HIS_WEBAPI.Data.Repositories;

namespace ENTERPRISE_HIS_WEBAPI.Services
{
    /// <summary>
    /// Service interface for user management
    /// </summary>
    public interface IUserService
    {
        Task<(bool Success, string Message, UserResponseDto? Data)> CreateUserAsync(CreateUserDto createUserDto);
        Task<(bool Success, string Message, UserResponseDto? Data)> GetUserByIdAsync(int userId);
        Task<(bool Success, string Message, UserResponseDto? Data)> GetUserByUsernameAsync(string username);
        Task<(bool Success, string Message, PaginatedUserResponseDto Data)> GetUsersAsync(int pageNumber, int pageSize);
        Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<(bool Success, string Message)> AssignRoleAsync(int userId, int roleId);
        Task<(bool Success, string Message)> RemoveRoleAsync(int userId, int roleId);
    }

    /// <summary>
    /// Service for user management with business logic
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<(bool Success, string Message, UserResponseDto? Data)> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(createUserDto.Username))
                    return (false, "Username is required", null);

                if (string.IsNullOrWhiteSpace(createUserDto.Email))
                    return (false, "Email is required", null);

                if (string.IsNullOrWhiteSpace(createUserDto.Password))
                    return (false, "Password is required", null);

                // Check if username exists
                var userExists = await _userRepository.UserExistsAsync(createUserDto.Username);
                if (userExists)
                    return (false, $"Username '{createUserDto.Username}' already exists", null);

                // Check if email exists
                var emailExists = await _userRepository.EmailExistsAsync(createUserDto.Email);
                if (emailExists)
                    return (false, $"Email '{createUserDto.Email}' is already in use", null);

                // Hash password
                var passwordHash = HashPassword(createUserDto.Password);

                // Create user
                var userId = await _userRepository.CreateUserAsync(createUserDto, passwordHash);
                if (userId <= 0)
                    return (false, "Failed to create user in database", null);

                // Get created user
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, "Failed to retrieve created user", null);

                _logger.LogInformation("User created successfully: {Username}", createUserDto.Username);
                return (true, "User created successfully", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", createUserDto.Username);
                return (false, "An error occurred while creating the user", null);
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<(bool Success, string Message, UserResponseDto? Data)> GetUserByIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return (false, "Invalid user ID", null);

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, $"User with ID {userId} not found", null);

                return (true, "User retrieved successfully", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return (false, "An error occurred while retrieving the user", null);
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public async Task<(bool Success, string Message, UserResponseDto? Data)> GetUserByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return (false, "Username is required", null);

                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                    return (false, $"User '{username}' not found", null);

                return (true, "User retrieved successfully", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return (false, "An error occurred while retrieving the user", null);
            }
        }

        /// <summary>
        /// Get paginated list of users
        /// </summary>
        public async Task<(bool Success, string Message, PaginatedUserResponseDto Data)> GetUsersAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber <= 0)
                    return (false, "Page number must be greater than 0", new PaginatedUserResponseDto());

                if (pageSize <= 0 || pageSize > 100)
                    return (false, "Page size must be between 1 and 100", new PaginatedUserResponseDto());

                var result = await _userRepository.GetUsersPagedAsync(pageNumber, pageSize);
                return (true, "Users retrieved successfully", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return (false, "An error occurred while retrieving users", new PaginatedUserResponseDto());
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            try
            {
                if (userId <= 0)
                    return (false, "Invalid user ID");

                // Verify user exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, $"User with ID {userId} not found");

                // Check if email is being updated and if it already exists
                if (!string.IsNullOrWhiteSpace(updateUserDto.Email) && 
                    !updateUserDto.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _userRepository.EmailExistsAsync(updateUserDto.Email);
                    if (emailExists)
                        return (false, $"Email '{updateUserDto.Email}' is already in use");
                }

                var success = await _userRepository.UpdateUserAsync(userId, updateUserDto);
                if (!success)
                    return (false, "Failed to update user");

                _logger.LogInformation("User updated successfully: {UserId}", userId);
                return (true, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return (false, "An error occurred while updating the user");
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return (false, "Invalid user ID");

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, $"User with ID {userId} not found");

                var success = await _userRepository.DeleteUserAsync(userId);
                if (!success)
                    return (false, "Failed to delete user");

                _logger.LogInformation("User deleted successfully: {UserId}", userId);
                return (true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return (false, "An error occurred while deleting the user");
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (userId <= 0)
                    return (false, "Invalid user ID");

                // Validate passwords match
                if (!changePasswordDto.NewPassword.Equals(changePasswordDto.ConfirmPassword))
                    return (false, "New passwords do not match");

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, $"User with ID {userId} not found");

                // Verify current password (in a real app, you'd hash and compare)
                // This is a placeholder - implement actual password verification

                // Hash new password
                var newPasswordHash = HashPassword(changePasswordDto.NewPassword);

                var success = await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
                if (!success)
                    return (false, "Failed to update password");

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return (true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return (false, "An error occurred while changing the password");
            }
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        public async Task<(bool Success, string Message)> AssignRoleAsync(int userId, int roleId)
        {
            try
            {
                if (userId <= 0)
                    return (false, "Invalid user ID");

                if (roleId <= 0)
                    return (false, "Invalid role ID");

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, $"User with ID {userId} not found");

                var success = await _userRepository.AssignRoleToUserAsync(userId, roleId);
                if (!success)
                    return (false, "Failed to assign role to user");

                _logger.LogInformation("Role {RoleId} assigned to user {UserId}", roleId, userId);
                return (true, "Role assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}", userId);
                return (false, "An error occurred while assigning the role");
            }
        }

        /// <summary>
        /// Remove role from user
        /// </summary>
        public async Task<(bool Success, string Message)> RemoveRoleAsync(int userId, int roleId)
        {
            try
            {
                if (userId <= 0)
                    return (false, "Invalid user ID");

                if (roleId <= 0)
                    return (false, "Invalid role ID");

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return (false, $"User with ID {userId} not found");

                var success = await _userRepository.RemoveRoleFromUserAsync(userId, roleId);
                if (!success)
                    return (false, "Failed to remove role from user");

                _logger.LogInformation("Role {RoleId} removed from user {UserId}", roleId, userId);
                return (true, "Role removed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user {UserId}", userId);
                return (false, "An error occurred while removing the role");
            }
        }

        // Helper methods

        /// <summary>
        /// Hash password using PBKDF2
        /// </summary>
        private string HashPassword(string password)
        {
            const int saltSize = 16;
            const int hashSize = 32;
            const int iterations = 10000;

            using (var algorithm = new Rfc2898DeriveBytes(password, saltSize, iterations, HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(hashSize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{iterations}.{salt}.{key}";
            }
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            var parts = hash.Split('.', 3);
            if (parts.Length != 3)
                return false;

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using (var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(32);
                return keyToCheck.SequenceEqual(key);
            }
        }
    }
}
