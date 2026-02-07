# ?? Password Hash Generation Guide

## Quick Start

### Step 1: Generate Hashes

Run the password hash generator to create PBKDF2-SHA256 hashes:

**File:** `Docs/PasswordHashGenerator.cs`

```bash
# Compile and run
csc Docs/PasswordHashGenerator.cs
PasswordHashGenerator.exe
```

**Output will show:**
```
Username: admin
Password: admin123
Hash:     10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3
          

Username: manager
Password: manager123
Hash:     10000.bXNlcmphZ2tyA...
          ...
```

### Step 2: Copy the Hash

For example, if admin123 generates:
```
10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3
```

### Step 3: Update Database

**In SQL Server Management Studio (SSMS):**

```sql
UPDATE [core].[UserAccount]
SET [PasswordHash] = '10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3'
WHERE [Username] = 'admin';
```

### Step 4: Test Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "userId": 1,
    "username": "admin",
    "email": "admin@example.com",
    "roles": ["Admin"]
  }
}
```

---

## Using PasswordHasher in C# Code

### Direct Usage

```csharp
using ENTERPRISE_HIS_WEBAPI.Utilities;

// Generate hash
string password = "admin123";
string hash = PasswordHasher.HashPassword(password);
// Result: "10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3"

// Verify password
bool isValid = PasswordHasher.VerifyPassword("admin123", hash);  // true
bool isInvalid = PasswordHasher.VerifyPassword("wrong", hash);   // false
```

### In Services

The `UserService` already uses this for:

```csharp
// When creating users
var passwordHash = HashPassword(createUserDto.Password);

// When changing passwords
var newHash = HashPassword(changePasswordDto.NewPassword);
```

---

## Password Hash Format

### Structure
```
{iterations}.{base64_salt}.{base64_hash}
```

### Example
```
10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3
```

**Components:**
- `10000` - Number of iterations (NIST recommended minimum)
- `YXJnaGsrCjEyMzQ1Njc4OTAx` - Base64 encoded 16-byte salt
- `J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3` - Base64 encoded 32-byte hash

---

## Security Considerations

### ? What We're Using

- **Algorithm:** PBKDF2 with SHA256 (NIST approved)
- **Iterations:** 10,000 (industry standard)
- **Salt Size:** 16 bytes (256 bits)
- **Hash Size:** 32 bytes (256 bits)

### ? Security Properties

- **Irreversible:** Hash cannot be reversed to get password
- **Unique Salt:** Each password gets random salt
- **Constant Time:** Comparison resistant to timing attacks
- **Salted:** Same password produces different hashes
- **Slow:** 10,000 iterations makes brute force expensive

### ?? Important Notes

1. **Never store plaintext passwords**
2. **Each password gets unique random salt**
3. **Don't use sample hashes in production**
4. **Generate your own hashes for test data**
5. **Keep JWT secret secure**

---

## Common Passwords & Hashes

For testing purposes, here are common test passwords:

| Password | Use Case |
|----------|----------|
| admin123 | Admin user |
| manager123 | Manager user |
| user123 | Regular user |
| TestPass123! | Complex password |

**Note:** These are examples. Always generate your own hashes!

---

## Step-by-Step Setup

### 1. Ensure UserAccount Table Exists
```sql
SELECT * FROM [core].[UserAccount];
```

### 2. Create Test Users (if needed)
```sql
INSERT INTO [core].[UserAccount] 
(Username, Email, FirstName, LastName, IsActive, CreatedDate)
VALUES
('admin', 'admin@example.com', 'Admin', 'User', 1, GETUTCDATE()),
('manager', 'manager@example.com', 'Manager', 'User', 1, GETUTCDATE()),
('user', 'user@example.com', 'Regular', 'User', 1, GETUTCDATE());
```

### 3. Generate Password Hashes
Run: `Docs/PasswordHashGenerator.cs`

### 4. Update User Passwords
```sql
UPDATE [core].[UserAccount]
SET [PasswordHash] = '{your_generated_hash}'
WHERE [Username] = 'admin';
```

### 5. Verify Update
```sql
SELECT UserId, Username, PasswordHash 
FROM [core].[UserAccount] 
WHERE Username = 'admin';
```

### 6. Test Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

---

## Troubleshooting

### Issue: "Invalid username or password"
**Possible Causes:**
1. PasswordHash is NULL in database
2. Hash format is incorrect
3. User is deactivated (IsActive = 0)

**Solution:**
```sql
-- Check hash
SELECT Username, PasswordHash, IsActive 
FROM [core].[UserAccount] 
WHERE Username = 'admin';

-- Should show hash in format: 10000.{salt}.{hash}
```

### Issue: PasswordHashGenerator won't run
**Solution:**
1. Copy code to standalone .csx file
2. Run with dotnet script tool
3. Or compile as console app

### Issue: Hash verification fails
**Possible Causes:**
1. Hash is corrupted
2. Wrong password
3. Hash format changed

**Solution:**
1. Generate new hash
2. Verify password matches
3. Update database

---

## API Usage After Setup

### Login
```bash
POST /api/auth/login
{
  "username": "admin",
  "password": "admin123"
}
```

### Get Token
Response contains:
```json
{
  "token": "...",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

### Use Token
```bash
GET /api/v1/users/1
Authorization: Bearer {token}
```

---

## Files Reference

| File | Purpose |
|------|---------|
| `Utilities/PasswordHasher.cs` | Hash utility class |
| `Docs/PasswordHashGenerator.cs` | Standalone hash generator |
| `Docs/SqlScripts/06_SetupTestUserPasswords.sql` | SQL template |

---

## Next Steps

1. ? Run `PasswordHashGenerator.cs`
2. ? Copy generated hash
3. ? Update database with hash
4. ? Test login endpoint
5. ? Verify token generation

**Your authentication system is ready!** ??
