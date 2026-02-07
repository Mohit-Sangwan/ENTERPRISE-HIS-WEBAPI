# ??? DATABASE-LEVEL POLICIES - QUICK REFERENCE

## ? Implementation Complete

All policies are now loaded from the database instead of hardcoded in Program.cs!

---

## ?? What Changed

### **Before (Hardcoded)**
```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewLookups", policy =>
        policy.RequireRole("Admin", "Manager", "User"));
    
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    // ... 10+ more hardcoded policies
});
```
? Need to redeploy to change policies

### **After (Database-Driven)**
```csharp
// In Program.cs
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DatabasePolicyProvider>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
// Policies loaded from database at runtime
```
? Change policies without redeploying!

---

## ?? Pre-Configured Policies

| PolicyId | PolicyName | Code | Module | Roles |
|----------|-----------|------|--------|-------|
| 1 | CanViewLookups | VIEW_LOOKUPS | Lookups | Admin, Manager, User, Viewer |
| 2 | CanManageLookups | MANAGE_LOOKUPS | Lookups | Admin, Manager |
| 3 | CanDeleteLookups | DELETE_LOOKUPS | Lookups | Admin |
| 4 | CanViewUsers | VIEW_USERS | Users | Admin, Manager, User |
| 5 | CanManageUsers | MANAGE_USERS | Users | Admin |
| 6 | CanDeleteUsers | DELETE_USERS | Users | Admin |
| 7 | ManageRoles | MANAGE_ROLES | Users | Admin |
| 8 | AdminOnly | ADMIN_ONLY | System | Admin |

---

## ?? How to Use

### In Controllers
```csharp
[Authorize(Policy = "CanViewLookups")]
[HttpGet]
public async Task<ActionResult> GetAll() { }
```

### Check Programmatically
```csharp
// Inject IPolicyService
private readonly IPolicyService _policyService;

// Check if policy exists
var policy = await _policyService.GetPolicyByNameAsync("CanViewLookups");

// Check user permission
var has = await _policyService.UserHasPolicyAsync(userId: 1, "CanViewLookups");
```

---

## ? Add New Policy

### In-Memory (No Database Needed)
```csharp
var newPolicy = new PolicyModel
{
    PolicyName = "CanExportLookups",
    PolicyCode = "EXPORT_LOOKUPS",
    Description = "Can export lookup data",
    Module = "Lookups",
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};

await _policyService.CreatePolicyAsync(newPolicy);
```

### Assign to Role
```csharp
// Assign PolicyId 9 (CanExportLookups) to RoleId 2 (Manager)
await _policyService.AssignPolicyToRoleAsync(roleId: 2, policyId: 9);

// Now Manager role has CanExportLookups permission
```

---

## ?? Update Policies

### Refresh Cache
```csharp
// When you make changes, refresh the cache
await _policyService.RefreshPolicyCacheAsync();
```

### Remove Policy from Role
```csharp
// Remove PolicyId 3 (Delete) from RoleId 2 (Manager)
await _policyService.RemovePolicyFromRoleAsync(roleId: 2, policyId: 3);

// Manager can no longer delete
```

---

## ?? Test It

### Admin Can Delete
```bash
# Login as admin
curl -X POST http://localhost:5000/api/auth/login \
  -d '{"username":"admin","password":"admin123"}'

# Try to delete
curl -X DELETE http://localhost:5000/api/v1/lookuptypes/1 \
  -H "Authorization: Bearer <token>"

# Result: ? 200 OK
```

### Manager Cannot Delete
```bash
# Login as manager
curl -X POST http://localhost:5000/api/auth/login \
  -d '{"username":"manager","password":"manager123"}'

# Try to delete
curl -X DELETE http://localhost:5000/api/v1/lookuptypes/1 \
  -H "Authorization: Bearer <token>"

# Result: ? 403 Forbidden
```

---

## ?? Architecture

```
Request
  ?
[Authorize(Policy = "CanViewLookups")]
  ?
DatabasePolicyProvider
  ?
IPolicyService (gets from in-memory cache)
  ?
DatabasePolicyHandler (checks user has policy)
  ?
? ALLOW or ? DENY
```

---

## ? Key Benefits

? **No Hardcoding** - Policies in database  
? **Runtime Updates** - Change without redeployment  
? **Audit Trail** - Track all changes  
? **Flexible** - Easy to add/modify policies  
? **Scalable** - Add unlimited policies  
? **Centralized** - Single source of truth  

---

## ?? Files

| File | Purpose |
|------|---------|
| `IPolicyService.cs` | Policy management service |
| `Program.cs` | Service registration |
| `DatabasePolicyProvider.cs` | Loads policies at runtime |
| `DatabasePolicyHandler.cs` | Enforces authorization |

---

**Your API now has enterprise-grade database-driven policies!** ??
