# ? Quick Hash Generation - 60 Seconds

## TL;DR - Just Want the Hash?

Run this C# code to get your hash instantly:

```csharp
using System;
using System.Security.Cryptography;

// 1. Copy this function
string HashPassword(string password)
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

// 2. Call it
var hash = HashPassword("admin123");
Console.WriteLine(hash);

// 3. Copy the output hash
// 4. Use in SQL: UPDATE [core].[UserAccount] SET [PasswordHash] = '{hash}' WHERE [Username] = 'admin';
```

---

## SQL Update (Fill in Your Hash)

```sql
UPDATE [core].[UserAccount]
SET [PasswordHash] = 'PASTE_YOUR_GENERATED_HASH_HERE'
WHERE [Username] = 'admin';
```

---

## Test Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

**You should get a token!** ?

---

## Files You Have

| What | Where |
|------|-------|
| Hash Utility | `Utilities/PasswordHasher.cs` |
| Generator Tool | `Docs/PasswordHashGenerator.cs` |
| SQL Template | `Docs/SqlScripts/06_SetupTestUserPasswords.sql` |
| Full Guide | `Docs/ADMIN123_HASH_SETUP.md` |
| This File | `Docs/QUICK_HASH_GUIDE.md` |

---

## That's It! ??

Your hash is ready. Just update the database and login!
