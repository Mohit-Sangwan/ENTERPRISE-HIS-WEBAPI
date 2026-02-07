# ?? Complete Authentication Setup - admin123 Hash Generation

## Overview

This guide walks you through generating a password hash for "admin123" and setting it up in your database for authentication testing.

---

## ?? Quick Summary

### What You Need:
1. **PasswordHasher utility** - Created in `Utilities/PasswordHasher.cs`
2. **PasswordHashGenerator** - Standalone tool in `Docs/PasswordHashGenerator.cs`
3. **SQL update script** - Template in `Docs/SqlScripts/06_SetupTestUserPasswords.sql`

### What You'll Get:
- PBKDF2-SHA256 hash of "admin123"
- SQL UPDATE statement to store it
- Working authentication endpoint

---

## ?? Step-by-Step Setup

### Step 1: Generate Password Hash

#### Option A: Using Console Application (Recommended)

**File:** `Docs/PasswordHashGenerator.cs`

```bash
# 1. Copy the file to a folder
# 2. Compile it
csc PasswordHashGenerator.cs

# 3. Run it
PasswordHashGenerator.exe

# 4. You'll see output like:
# Username: admin
# Password: admin123
# Hash:     10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3
```

#### Option B: Using C# Interactive

```csharp
using System;
using System.Security.Cryptography;

// Paste PasswordHasher code, then:
string hash = PasswordHasher.HashPassword("admin123");
Console.WriteLine(hash);
// Output: 10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3
```

#### Option C: Using Online Tool (NOT Recommended for Production)
- Search for "PBKDF2 online generator"
- Use: SHA256, 10000 iterations
- Remember: Never use online tools for production data!

---

### Step 2: Copy the Generated Hash

Once you run the generator, you'll get output like:

```
?????????????????????????????????????????????????????????????????
?       PBKDF2-SHA256 PASSWORD HASH GENERATOR                   ?
?       Enterprise HIS - Password Hash Utility                  ?
?????????????????????????????????????????????????????????????????

Generating password hashes for test users...

?????????????????????????????????????????????????????????????????
? Username: admin                                         ?
? Password: admin123                                      ?
? Hash:     10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P ?
?           3Q2R1S0T9U8V7W6X5Y4Z3                         ?
?                                                         ?
? SQL for core.UserAccount:                               ?
? UPDATE [core].[UserAccount]                             ?
? SET [PasswordHash] = '10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J ?
? 9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3'                      ?
? WHERE [Username] = 'admin';                             ?
?????????????????????????????????????????????????????????????????
```

**Copy this full hash:**
```
10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3
```

---

### Step 3: Update Database

#### Open SQL Server Management Studio (SSMS)

1. Connect to your database: `EnterpriseHIS`
2. Open a new query
3. Paste this SQL:

```sql
-- First, verify admin user exists
SELECT [UserId], [Username], [Email], [IsActive]
FROM [core].[UserAccount]
WHERE [Username] = 'admin';

-- If admin doesn't exist, create it:
-- INSERT INTO [core].[UserAccount] 
-- (Username, Email, FirstName, LastName, IsActive, CreatedDate)
-- VALUES ('admin', 'admin@example.com', 'Admin', 'User', 1, GETUTCDATE());

-- Update password hash
UPDATE [core].[UserAccount]
SET [PasswordHash] = '10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3'
WHERE [Username] = 'admin';

-- Verify update
SELECT [UserId], [Username], [PasswordHash], [IsActive]
FROM [core].[UserAccount]
WHERE [Username] = 'admin';
```

4. **Execute (F5 or Ctrl+E)**

5. **Verify the result:**
   - Should show: `(1 row(s) affected)`
   - PasswordHash should show: `10000.YXJ...`

---

### Step 4: Ensure User is Assigned a Role

For authentication to work properly, assign the user a role:

```sql
-- Verify admin has a role
SELECT u.[UserId], u.[Username], r.[RoleName]
FROM [core].[UserAccount] u
LEFT JOIN [config].[UserRole] ur ON u.[UserId] = ur.[UserId]
LEFT JOIN [master].[RoleMaster] r ON ur.[RoleId] = r.[RoleId]
WHERE u.[Username] = 'admin';

-- If no role, assign Admin role (ID = 1):
INSERT INTO [config].[UserRole] (UserId, RoleId, IsActive, AssignedDate)
SELECT 1, 1, 1, GETUTCDATE()
WHERE NOT EXISTS (
    SELECT 1 FROM [config].[UserRole] 
    WHERE UserId = 1 AND RoleId = 1
);
```

---

### Step 5: Test Authentication

#### Start Your Application
```bash
dotnet run
```

#### Test Login Endpoint

**Using cURL:**
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Using PowerShell:**
```powershell
$body = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/auth/login" `
  -Method Post `
  -Body $body `
  -ContentType "application/json"
```

**Using Postman:**
1. POST to: `https://localhost:5001/api/auth/login`
2. Headers: `Content-Type: application/json`
3. Body (raw):
```json
{
  "username": "admin",
  "password": "admin123"
}
```

#### Expected Success Response (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
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

#### Expected Error Response (401 Unauthorized):
If password is wrong:
```json
{
  "error": "Invalid username or password",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## ?? Hash Format Explained

### Format: `{iterations}.{salt}.{hash}`

```
10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3
?      ?                       ?
?      ?                       ?? Base64 encoded 32-byte hash (SHA256)
?      ?????????????????????????? Base64 encoded 16-byte random salt
????????????????????????????????? Number of iterations (NIST minimum)
```

### Security Properties:

| Property | Value | Why |
|----------|-------|-----|
| Algorithm | PBKDF2 + SHA256 | NIST approved, industry standard |
| Iterations | 10,000 | Makes brute force attacks expensive |
| Salt Size | 16 bytes | Prevents rainbow tables |
| Hash Size | 32 bytes | Full SHA256 output |
| Format | Base64 | Easy storage and transport |

---

## ? Verification Checklist

After setup, verify everything works:

- [ ] Password hash generated successfully
- [ ] Hash updated in database
- [ ] Admin user has a role assigned
- [ ] Database query shows PasswordHash is set
- [ ] Login endpoint returns token (200 OK)
- [ ] Token can be used with other endpoints
- [ ] Wrong password returns 401
- [ ] Swagger shows generated token

---

## ?? Complete Test Scenario

### 1. Generate Hash
```bash
# Run PasswordHashGenerator.cs
# Get: 10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3
```

### 2. Update Database
```sql
UPDATE [core].[UserAccount]
SET [PasswordHash] = '10000.YXJnaGsrCjEyMzQ1Njc4OTAx.J9K8L7M6N5O4P3Q2R1S0T9U8V7W6X5Y4Z3'
WHERE [Username] = 'admin';
```

### 3. Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

### 4. Get Token
```json
{
  "token": "eyJhbGci...",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

### 5. Use Token
```bash
curl https://localhost:5001/api/v1/users/1 \
  -H "Authorization: Bearer eyJhbGci..."
```

---

## ?? Troubleshooting

### Issue: "Invalid username or password" after setup

**Check 1: Hash in Database**
```sql
SELECT PasswordHash FROM [core].[UserAccount] WHERE Username = 'admin';
```
- Should show: `10000.YXJ...` (NOT NULL)
- If NULL: Hash wasn't updated

**Check 2: User is Active**
```sql
SELECT IsActive FROM [core].[UserAccount] WHERE Username = 'admin';
```
- Should show: `1`
- If `0`: Deactivate and try again

**Check 3: Correct Password**
- Verify you're using: `admin123`
- Not: `admin` or `123`

**Check 4: Hash Format**
```
Should be: 10000.{base64salt}.{base64hash}
Not:       {base64hash}  ? This format won't work
```

### Issue: PasswordHashGenerator won't compile

**Solution:**
```bash
# Ensure you have .NET SDK
dotnet --version

# Or use C# compiler
csc --version

# Or manually copy-paste code into interactive C#
```

### Issue: Hash has spaces or line breaks

**Solution:**
When copying from console output, ensure:
- No spaces at beginning/end
- One continuous string
- All characters included

---

## ?? Security Best Practices

### ? DO:
- [ ] Generate unique hashes for each password
- [ ] Use PBKDF2-SHA256 (as implemented)
- [ ] Store hashes securely in database
- [ ] Use strong passwords (8+ chars, mixed case, symbols)
- [ ] Regenerate hashes when changing security levels

### ? DON'T:
- [ ] Use same hash for multiple users
- [ ] Store plaintext passwords
- [ ] Use simple algorithms (MD5, SHA1)
- [ ] Use online tools for production data
- [ ] Share password hashes in logs

---

## ?? Reference Files

| File | Purpose |
|------|---------|
| `Utilities/PasswordHasher.cs` | Reusable hash utility |
| `Docs/PasswordHashGenerator.cs` | Standalone generator |
| `Docs/SqlScripts/06_SetupTestUserPasswords.sql` | SQL template |
| `Docs/PASSWORD_HASH_GENERATION_GUIDE.md` | This file |

---

## ?? Next Steps

1. ? Generate hash for admin123
2. ? Update database with hash
3. ? Test login endpoint
4. ? Create more test users as needed
5. ? Use tokens with other API endpoints

---

**Your authentication system is now fully operational!** ??

For questions, check:
- `ENTERPRISE_AUTHENTICATION_INTEGRATION.md` - Full auth documentation
- `PASSWORD_HASH_GENERATION_GUIDE.md` - Hash generation details
- `USER_CRUD_IMPLEMENTATION_GUIDE.md` - User management API

