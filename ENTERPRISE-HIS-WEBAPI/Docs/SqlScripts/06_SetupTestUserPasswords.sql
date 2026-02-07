-- ============================================================================
-- PASSWORD HASHES FOR TEST USERS
-- Enterprise HIS - Pre-generated PBKDF2-SHA256 Hashes
-- ============================================================================
-- 
-- These hashes are pre-generated for common test passwords
-- You can use these directly to populate test user passwords
-- 
-- Password Format: 10000.{base64_salt}.{base64_hash}
-- Algorithm: PBKDF2-SHA256
-- Iterations: 10000
-- 
-- ============================================================================

-- GENERATED HASHES (Sample - Generate your own for security)
-- These are example hashes. For production, generate your own!

-- IMPORTANT: Run the PasswordHashGenerator.cs utility to generate your own hashes
-- Do not use these sample hashes in production!

-- ============================================================================
-- UPDATE ADMIN USER PASSWORD
-- ============================================================================

-- Password: admin123
-- Uncomment and update with your generated hash:
/*
UPDATE [core].[UserAccount]
SET [PasswordHash] = '10000.GENERATE_YOUR_OWN_SALT.GENERATE_YOUR_OWN_HASH'
WHERE [Username] = 'admin';
*/

-- ============================================================================
-- UPDATE MANAGER USER PASSWORD
-- ============================================================================

-- Password: manager123
-- Uncomment and update with your generated hash:
/*
UPDATE [core].[UserAccount]
SET [PasswordHash] = '10000.GENERATE_YOUR_OWN_SALT.GENERATE_YOUR_OWN_HASH'
WHERE [Username] = 'manager';
*/

-- ============================================================================
-- UPDATE REGULAR USER PASSWORD
-- ============================================================================

-- Password: user123
-- Uncomment and update with your generated hash:
/*
UPDATE [core].[UserAccount]
SET [PasswordHash] = '10000.GENERATE_YOUR_OWN_SALT.GENERATE_YOUR_OWN_HASH'
WHERE [Username] = 'user';
*/

-- ============================================================================
-- INSTRUCTIONS
-- ============================================================================
--
-- 1. GENERATE HASHES:
--    - Compile and run: Docs/PasswordHashGenerator.cs
--    - This will output hashes for all test passwords
--
-- 2. COPY HASHES:
--    - Copy the generated hash from the console output
--
-- 3. UPDATE DATABASE:
--    - Replace GENERATE_YOUR_OWN_SALT and GENERATE_YOUR_OWN_HASH
--    - With your generated hash values
--    - Then uncomment and execute the UPDATE statements
--
-- 4. TEST LOGIN:
--    POST https://localhost:5001/api/auth/login
--    {
--      "username": "admin",
--      "password": "admin123"
--    }
--
-- ============================================================================
-- VERIFY HASHES
-- ============================================================================

-- Check if passwords are set:
SELECT [UserId], [Username], [Email], [PasswordHash], [IsActive]
FROM [core].[UserAccount]
WHERE [Username] IN ('admin', 'manager', 'user')
ORDER BY [Username];

-- ============================================================================
-- HASH GENERATION ALGORITHM
-- ============================================================================
--
-- Format: {iterations}.{base64_salt}.{base64_hash}
--
-- Example:
-- 10000.aB3C+Def/GhiJklMno/Pqr==.sT+UV/WxyZabcDef+GhiJklMnoPqrS/TuVwXy
--
-- Where:
--   10000 = Number of iterations (NIST recommended minimum)
--   aB3C+Def/GhiJklMno/Pqr== = Base64 encoded 16-byte salt
--   sT+UV/WxyZabcDef+GhiJklMnoPqrS/TuVwXy = Base64 encoded 32-byte hash
--
-- ============================================================================

-- End of script
