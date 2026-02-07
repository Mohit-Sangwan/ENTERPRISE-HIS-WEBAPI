# ? MODULE-BASED POLICIES - QUICK IMPLEMENTATION (5 Minutes)

## What You Have

? **ModulePolicyConstants.cs** - 6 modules × 7 operations  
? **03_ModuleBasedPoliciesMigration.sql** - Complete database setup  
? **Documentation** - Complete implementation guide  

---

## ?? Implementation Steps

### **Step 1: Run SQL Script (1 minute)**

```bash
# Open SQL Server Management Studio
# Open: Database/03_ModuleBasedPoliciesMigration.sql
# Press: F5 (Execute)
```

**What happens:**
- Creates 42 policies (6 modules × 7 operations)
- Assigns policies to 4 roles
- Backup created automatically

---

### **Step 2: Update LookupTypesController (1 minute)**

**Before:**
```csharp
[Authorize(Policy = "CanViewLookups")]
[HttpGet]
public async Task<ActionResult> GetAll() { }
```

**After:**
```csharp
using ENTERPRISE_HIS_WEBAPI.Constants;

[Authorize(Policy = ModulePolicyConstants.Lookups.VIEW)]
[HttpGet]
public async Task<ActionResult> GetAll() { }
```

**All replacements:**
```
OLD                                    NEW
[Authorize(Policy = "CanViewLookups")]  ? [Authorize(Policy = ModulePolicyConstants.Lookups.VIEW)]
[Authorize(Policy = "CanManageLookups")]? [Authorize(Policy = ModulePolicyConstants.Lookups.EDIT)] (for PUT)
[Authorize(Policy = "CanManageLookups")]? [Authorize(Policy = ModulePolicyConstants.Lookups.CREATE)] (for POST)
[Authorize(Policy = "CanDeleteLookups")]? [Authorize(Policy = ModulePolicyConstants.Lookups.DELETE)]
[Authorize(Policy = "AdminOnly")]       ? [Authorize(Policy = ModulePolicyConstants.Lookups.ADMIN)]
```

---

### **Step 3: Update All Controllers (2 minutes)**

**Pattern for every controller:**

```csharp
using ENTERPRISE_HIS_WEBAPI.Constants;

// VIEW
[Authorize(Policy = ModulePolicyConstants.{Module}.VIEW)]
public async Task<ActionResult> GetAll() { }

// PRINT
[Authorize(Policy = ModulePolicyConstants.{Module}.PRINT)]
public async Task<ActionResult> Export() { }

// CREATE
[Authorize(Policy = ModulePolicyConstants.{Module}.CREATE)]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }

// EDIT
[Authorize(Policy = ModulePolicyConstants.{Module}.EDIT)]
public async Task<ActionResult> Update(int id, [FromBody] UpdateDto request) { }

// DELETE
[Authorize(Policy = ModulePolicyConstants.{Module}.DELETE)]
public async Task<ActionResult> Delete(int id) { }

// MANAGE
[Authorize(Policy = ModulePolicyConstants.{Module}.MANAGE)]
public async Task<ActionResult> Activate(int id) { }

// ADMIN
[Authorize(Policy = ModulePolicyConstants.{Module}.ADMIN)]
public async Task<ActionResult> BulkImport(IFormFile file) { }
```

---

### **Step 4: Test (1 minute)**

```bash
# Test Admin
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"admin","password":"admin123"}' | jq -r '.token')
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes  # ? 200

# Test Manager
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"manager","password":"manager123"}' | jq -r '.token')
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes  # ? 200

# Test User
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"user","password":"user123"}' | jq -r '.token')
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes  # ? 200

# Try Delete (should fail for Manager/User)
curl -X DELETE -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes/1  # ? 403
```

---

## ?? What's Included

### **6 Modules**
```
1. Lookups       ? ModulePolicyConstants.Lookups
2. Users         ? ModulePolicyConstants.Users
3. Roles         ? ModulePolicyConstants.Roles
4. Patients      ? ModulePolicyConstants.Patients (Future)
5. Appointments  ? ModulePolicyConstants.Appointments (Future)
6. Prescriptions ? ModulePolicyConstants.Prescriptions (Future)
```

### **7 Operations per Module**
```
1. View   - GET /api/v1/{controller}
2. Print  - GET /api/v1/{controller}/export
3. Create - POST /api/v1/{controller}
4. Edit   - PUT /api/v1/{controller}/{id}
5. Delete - DELETE /api/v1/{controller}/{id}
6. Manage - PATCH /api/v1/{controller}/{id}/activate
7. Admin  - POST /api/v1/{controller}/admin/bulk-import
```

### **4 Roles**
```
Admin   - All policies (42/42)
Manager - View, Print, Create, Edit, Manage (30/42)
User    - View, Print (12/42)
Viewer  - View (6/42)
```

---

## ?? Database Structure

### **master.PolicyMaster (42 rows)**
```
PolicyId | PolicyName           | PolicyCode        | Module
---------|----------------------|-------------------|----------
1        | Lookups - View       | LOOKUPS_VIEW      | Lookups
2        | Lookups - Print      | LOOKUPS_PRINT     | Lookups
3        | Lookups - Create     | LOOKUPS_CREATE    | Lookups
... (7 per module)
```

### **config.RolePolicyMapping**
```
RoleId | PolicyId | Module
--------|----------|----------
1      | 1-42     | Admin (All)
2      | 1-30     | Manager (View, Print, Create, Edit, Manage)
3      | 1-12     | User (View, Print)
4      | 1-6      | Viewer (View)
```

---

## ?? Key Constants to Use

```csharp
// Lookups
ModulePolicyConstants.Lookups.VIEW
ModulePolicyConstants.Lookups.PRINT
ModulePolicyConstants.Lookups.CREATE
ModulePolicyConstants.Lookups.EDIT
ModulePolicyConstants.Lookups.DELETE
ModulePolicyConstants.Lookups.MANAGE
ModulePolicyConstants.Lookups.ADMIN

// Users (same pattern)
ModulePolicyConstants.Users.VIEW
ModulePolicyConstants.Users.PRINT
... etc

// Roles (same pattern)
ModulePolicyConstants.Roles.VIEW
... etc
```

---

## ? Verification

### **Check Database**
```sql
-- Count policies
SELECT COUNT(*) FROM [master].[PolicyMaster];
-- Result: 42

-- Check by module
SELECT [Module], COUNT(*) FROM [master].[PolicyMaster] GROUP BY [Module];
-- Result: 6 modules, 7 each

-- Check role mappings
SELECT r.[RoleName], COUNT(rpm.[PolicyId]) 
FROM [master].[RoleMaster] r
LEFT JOIN [config].[RolePolicyMapping] rpm ON r.[RoleId] = rpm.[RoleId]
GROUP BY r.[RoleName];
-- Result: Admin 42, Manager 30, User 12, Viewer 6
```

---

## ?? Add to Program.cs (Optional)

If you want automatic policy registration:

```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    // Register all module-based policies
    foreach (var policy in ModulePolicyConstants.AllModules)
    {
        foreach (var operation in ModulePolicyConstants.AllOperations)
        {
            var policyName = $"{policy}.{operation}";
            options.AddPolicy(policyName, policy =>
                policy.Requirements.Add(
                    new DatabasePolicyRequirement(policyName)));
        }
    }
});
```

---

## ?? Access Control Summary

```
Admin      ? View? Print? Create? Edit? Delete? Manage? Admin?
Manager    ? View? Print? Create? Edit? Delete? Manage? Admin?
User       ? View? Print? Create? Edit? Delete? Manage? Admin?
Viewer     ? View? Print? Create? Edit? Delete? Manage? Admin?
```

---

## ?? Tips

1. **Use IntelliSense**: Type `ModulePolicyConstants.` to see all options
2. **Consistent Naming**: Always use the module name from the class
3. **For Future Modules**: Just add new nested class to ModulePolicyConstants
4. **For Operations**: Already includes all common operations (View, Print, Create, Edit, Delete, Manage, Admin)
5. **Audit Trail**: All changes tracked with CreatedBy/ModifiedBy

---

## ?? Done!

Your API now has:
- ? 42 module-based policies
- ? 6 modules (current + future)
- ? 7 operations per module
- ? 4 role configurations
- ? Enterprise-grade authorization

**Ready for production!** ??

---

**See full details:** `ENTERPRISE_MODULE_BASED_POLICIES.md`
