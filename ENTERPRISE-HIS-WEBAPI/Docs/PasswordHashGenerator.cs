using System;
using System.Security.Cryptography;

namespace PasswordHashGenerator
{
    /// <summary>
    /// Standalone utility to generate PBKDF2-SHA256 password hashes
    /// Run this to generate hashes for your test users
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║       PBKDF2-SHA256 PASSWORD HASH GENERATOR                   ║");
            Console.WriteLine("║       Enterprise HIS - Password Hash Utility                  ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Generate hashes for common test passwords
            var testPasswords = new Dictionary<string, string>
            {
                { "admin", "admin123" },
                { "manager", "manager123" },
                { "user", "user123" },
                { "testuser", "TestPass123!" }
            };

            Console.WriteLine("Generating password hashes for test users...");
            Console.WriteLine();
            Console.WriteLine("┌───────────────────────────────────────────────────────────────┐");

            foreach (var kvp in testPasswords)
            {
                string username = kvp.Key;
                string password = kvp.Value;
                string hash = HashPassword(password);

                Console.WriteLine($"│ Username: {username.PadRight(40)} │");
                Console.WriteLine($"│ Password: {password.PadRight(40)} │");
                Console.WriteLine($"│ Hash:     {hash.Substring(0, 40).PadRight(40)} │");
                Console.WriteLine($"│           {hash.Substring(40).PadRight(40)} │");
                Console.WriteLine($"│                                               │");

                // Also output in SQL INSERT format
                Console.WriteLine($"│ SQL for core.UserAccount:                      │");
                Console.WriteLine($"│ UPDATE [core].[UserAccount]                    │");
                Console.WriteLine($"│ SET [PasswordHash] = '{hash}'      │");
                Console.WriteLine($"│ WHERE [Username] = '{username}';                     │");
                Console.WriteLine();
            }

            Console.WriteLine("└───────────────────────────────────────────────────────────────┘");
            Console.WriteLine();
            Console.WriteLine("Instructions:");
            Console.WriteLine("1. Copy the SQL UPDATE statements above");
            Console.WriteLine("2. Execute them in SQL Server Management Studio (SSMS)");
            Console.WriteLine("3. These hashes will now work with your authentication system");
            Console.WriteLine();
            Console.WriteLine("Example SQL to use:");
            Console.WriteLine("  UPDATE [core].[UserAccount] SET [PasswordHash] = '<hash>' WHERE [Username] = 'admin';");
            Console.WriteLine();

            // Additional usage examples
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("VERIFICATION TEST:");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();

            string testHash = HashPassword("admin123");
            Console.WriteLine($"Generated hash for 'admin123': {testHash}");
            Console.WriteLine();

            bool isValid = VerifyPassword("admin123", testHash);
            Console.WriteLine($"✓ Verification with correct password: {(isValid ? "PASSED" : "FAILED")}");

            bool isInvalid = VerifyPassword("wrongpassword", testHash);
            Console.WriteLine($"✓ Verification with wrong password: {(isInvalid ? "FAILED (as expected)" : "PASSED (as expected)")}");

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Generate PBKDF2-SHA256 password hash
        /// </summary>
        static string HashPassword(string password)
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
        static bool VerifyPassword(string password, string hash)
        {
            try
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
            catch
            {
                return false;
            }
        }
    }
}
