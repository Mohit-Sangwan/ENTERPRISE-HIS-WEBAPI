using System.Security.Cryptography;

namespace ENTERPRISE_HIS_WEBAPI.Utilities
{
    /// <summary>
    /// Utility for password hashing and verification
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Generate PBKDF2-SHA256 password hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hash in format: iterations.base64salt.base64key</returns>
        public static string HashPassword(string password)
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
        /// Verify password against PBKDF2 hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">Stored hash in format: iterations.base64salt.base64key</param>
        /// <returns>True if password matches hash</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                    return false;

                // Parse hash format: iterations.base64salt.base64key
                var parts = hash.Split('.');
                if (parts.Length != 3)
                    return false;

                // Parse iterations
                if (!int.TryParse(parts[0], out int iterations) || iterations < 1000 || iterations > 1000000)
                    return false;

                // Parse salt (base64) - add padding if needed
                byte[] salt;
                try
                {
                    var saltPadded = parts[1];
                    while (saltPadded.Length % 4 != 0)
                        saltPadded += "=";
                    salt = Convert.FromBase64String(saltPadded);
                }
                catch
                {
                    return false;
                }

                // Parse key (base64) - add padding if needed
                byte[] key;
                try
                {
                    var keyPadded = parts[2];
                    while (keyPadded.Length % 4 != 0)
                        keyPadded += "=";
                    key = Convert.FromBase64String(keyPadded);
                }
                catch
                {
                    return false;
                }

                // Validate sizes
                if (salt.Length < 8 || salt.Length > 128 || key.Length < 16 || key.Length > 128)
                    return false;

                // Derive key from password using same parameters
                using (var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var keyToCheck = algorithm.GetBytes(key.Length);
                    
                    // Constant-time comparison to prevent timing attacks
                    return ConstantTimeEquals(keyToCheck, key);
                }
            }
            catch
            {
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
