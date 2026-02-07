# ? **QUICK START - DATABASE ROLES & PERMISSIONS**

## ?? Overview

Your application now supports **both static and dynamic authorization**:

- **Static**: Fast, JWT-based role checks (already in token)
- **Dynamic**: Flexible, database-driven permission checks

---

## ?? 3-MINUTE SETUP

### Step 1: Create Tables

```sql
-- Paste the SQL from DATABASE_ROLES_PERMISSIONS_COMPLETE.md
-- Creates Role, Permission, RolePermission, User, UserRole tables
```

### Step 2: Insert Sample Data

```sql
-- Insert test roles, permissions, and users
-- (SQL provided in documentation)
```

### Step 3: Use in Controllers

```csharp
// OPTION A: Static Policy (current - JWT based)
[Authorize(Policy = "AdminOnly")]
[HttpDelete("{id}")]
public ActionResult Delete(int id) { }

// OPTION B: Dynamic Policy (new - database based)
[Authorize(Policy = "Permission_DELETE_LOOKUPS")]
[HttpDelete("{id}")]
public ActionResult Delete(int id) { }
```

---

## ?? How It Works

```
Request with Token
    ?
[Check Policy Type]
    ?? "Permission_*"     ? Query database
    ?? Other policies     ? Check JWT roles
    ?
? Authorized? ? Allow
? Unauthorized? ? 403 Forbidden
```

---

## ?? Code Examples

### Inject Services

```csharp
public class MyController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IDynamicAuthorizationService _authService;

    public MyController(IRoleRepository roleRepository, IDynamicAuthorizationService authService)
    {
        _roleRepository = roleRepository;
        _authService = authService;
    }
}
```

### Query Permissions

```csharp
// Get user roles
var roles = await _roleRepository.GetUserRoleNamesAsync(userId);
// Returns: ["Admin", "Manager"]

// Get user permissions
var permissions = await _roleRepository.GetUserPermissionsAsync(userId);
// Returns: ["VIEW_LOOKUPS", "CREATE_LOOKUPS", "DELETE_LOOKUPS"]

// Check specific permission
var hasPermission = await _roleRepository.UserHasPermissionAsync(userId, "DELETE_LOOKUPS");
// Returns: true/false
```

### Use High-Level Service

```csharp
// Is user authorized?
bool isAuthorized = await _authService.IsUserAuthorizedAsync(userId, "DELETE_LOOKUPS");

// Get all roles
var roles = await _authService.GetUserRolesAsync(userId);

// Get all permissions
var permissions = await _authService.GetUserPermissionsAsync(userId);
```

---

## ?? Permission Codes

```
Common permission codes to use:
- VIEW_LOOKUPS
- CREATE_LOOKUPS
- EDIT_LOOKUPS
- DELETE_LOOKUPS
- VIEW_USERS
- CREATE_USERS
- EDIT_USERS
- DELETE_USERS
```

---

## ?? Comparison

| Feature | Static | Dynamic |
|---------|--------|---------|
| Speed | ??? Fast | ? Slower |
| Database Query | ? No | ? Yes |
| Changes Need Deploy | ? Yes | ? No |
| Complexity | Simple | Complex |
| Use Case | Common rules | Custom rules |

---

## ?? Recommendation

```csharp
// Use STATIC for common, stable permissions
[Authorize(Policy = "AdminOnly")]

// Use DYNAMIC for business-specific, changing permissions
[Authorize(Policy = "Permission_APPROVAL_NEEDED")]
```

---

## ?? Tables Overview

```
User (1) ????? (M) UserRole (M) ????? (1) Role (1) ????? (M) RolePermission (M) ????? (1) Permission
                                              ?
                                        (Many Roles per User)
                                              ?
                                        (Many Permissions per Role)
```

---

## ? BUILD STATUS

```
? Complete
? No errors
? Ready to use
```

---

**Static + Dynamic Authorization System Ready!** ??
