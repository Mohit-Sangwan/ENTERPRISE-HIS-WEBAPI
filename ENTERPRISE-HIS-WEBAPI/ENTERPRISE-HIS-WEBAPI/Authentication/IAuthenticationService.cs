using ENTERPRISE_HIS_WEBAPI.Data.Dtos;
using ENTERPRISE_HIS_WEBAPI.Data.Repositories;
using ENTERPRISE_HIS_WEBAPI.Services;
using System.Security.Cryptography;

namespace ENTERPRISE_HIS_WEBAPI.Authentication
{
    /// <summary>
    /// Authentication service for user login with database integration
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate user with username and password
        /// </summary>
        Task<(bool Success, string Message, UserResponseDto? User)> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Verify password against stored hash
        /// </summary>
        bool VerifyPassword(string password, string hash);
    }

    /// <summary>
    /// Authentication service implementation
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user by username and password
        /// </summary>
        public async Task<(bool Success, string Message, UserResponseDto? User)> AuthenticateAsync(string username, string password)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Authentication attempt with missing credentials");
                    return (false, "Username and password are required", null);
                }

                // Get user from database
                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Authentication attempt with unknown username: {Username}", username);
                    return (false, "Invalid username or password", null);
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Authentication attempt with inactive user: {Username}", username);
                    return (false, "User account is inactive", null);
                }

                // Get password hash from database
                var passwordHash = await _userRepository.GetPasswordHashAsync(username);
                if (string.IsNullOrWhiteSpace(passwordHash))
                {
                    _logger.LogWarning("Password hash not found for user: {Username}", username);
                    return (false, "Invalid username or password", null);
                }

                // Verify password
                if (!VerifyPassword(password, passwordHash))
                {
                    _logger.LogWarning("Authentication attempt with incorrect password for user: {Username}", username);
                    return (false, "Invalid username or password", null);
                }

                _logger.LogInformation("User {Username} authenticated successfully", username);
                return (true, "Authentication successful", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user: {Username}", username);
                return (false, "An error occurred during authentication", null);
            }
        }

        /// <summary>
        /// Verify password against PBKDF2 hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                {
                    _logger.LogWarning("Password or hash is null or empty");
                    return false;
                }

                // Parse hash format: iterations.base64salt.base64hash
                var parts = hash.Split('.');
                if (parts.Length != 3)
                {
                    _logger.LogWarning("Invalid password hash format - expected 3 parts separated by '.', got {PartCount}", parts.Length);
                    return false;
                }

                // Parse iterations
                if (!int.TryParse(parts[0], out int iterations))
                {
                    _logger.LogWarning("Invalid iterations value in hash: {Iterations}", parts[0]);
                    return false;
                }

                if (iterations < 1000 || iterations > 1000000)
                {
                    _logger.LogWarning("Iterations out of valid range: {Iterations}", iterations);
                    return false;
                }

                // Parse salt (base64)
                byte[] salt;
                try
                {
                    // Add padding if needed
                    var saltPadded = parts[1];
                    while (saltPadded.Length % 4 != 0)
                        saltPadded += "=";
                    salt = Convert.FromBase64String(saltPadded);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Invalid base64 salt in hash: {Salt}", parts[1]);
                    return false;
                }

                // Parse key (base64)
                byte[] key;
                try
                {
                    // Add padding if needed
                    var keyPadded = parts[2];
                    while (keyPadded.Length % 4 != 0)
                        keyPadded += "=";
                    key = Convert.FromBase64String(keyPadded);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Invalid base64 key in hash: {Key}", parts[2]);
                    return false;
                }

                // Validate salt and key sizes
                if (salt.Length < 8 || salt.Length > 128)
                {
                    _logger.LogWarning("Salt size out of valid range: {SaltLength}", salt.Length);
                    return false;
                }

                if (key.Length < 16 || key.Length > 128)
                {
                    _logger.LogWarning("Key size out of valid range: {KeyLength}", key.Length);
                    return false;
                }

                // Derive key from password using same parameters
                using (var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var keyToCheck = algorithm.GetBytes(key.Length);
                    
                    // Use constant-time comparison to prevent timing attacks
                    return ConstantTimeEquals(keyToCheck, key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error verifying password");
                return false;
            }
        }

        /// <summary>
        /// Constant-time comparison to prevent timing attacks
        /// </summary>
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}
