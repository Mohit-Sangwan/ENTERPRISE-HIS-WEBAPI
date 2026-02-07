# ?? **INSTALLATION & EXECUTION GUIDE**

## ? QUICK SETUP (5 minutes)

### 1?? OPEN SQL SCRIPT

**File**: `ENTERPRISE-HIS-WEBAPI/SqlScripts/04_RolePermissionSystem.sql`

### 2?? EXECUTE IN SQL SERVER

```
Option A: SQL Server Management Studio (SSMS)
  1. Open SSMS
  2. File ? Open ? Select SQL file
  3. Press F5 to execute

Option B: Azure Data Studio
  1. Open Azure Data Studio
  2. File ? Open ? Select SQL file
  3. Click Execute

Option C: Command Line (sqlcmd)
  sqlcmd -S "ServerName" -d "DatabaseName" -i "04_RolePermissionSystem.sql"
```

### 3?? VERIFY SUCCESS

```sql
-- Run these queries to verify
SELECT COUNT(*) as RoleCount FROM [master].[RoleMaster];
-- Should return: 4

SELECT COUNT(*) as PermissionCount FROM [master].[PermissionMaster];
-- Should return: 9

SELECT COUNT(*) as UserCount FROM [core].[UserAccount];
-- Should return: 4
```

---

## ?? WHAT GETS CREATED

```
SCHEMAS:
  ? [master]     - Master data
  ? [config]     - Configuration & mappings
  ? [core]       - Core entities

TABLES:
  ? [master].[RoleMaster]                  (4 rows)
  ? [master].[PermissionMaster]            (9 rows)
  ? [core].[UserAccount]                   (4 rows)
  ? [config].[UserRole]                    (4 rows)
  ? [config].[RolePermissionConfig]        (8 rows)

STORED PROCEDURES:
  ? [config].[SP_GetUserRoles]
  ? [config].[SP_GetRolePermissions]
  ? [config].[SP_GetUserPermissions]
  ? [config].[SP_CheckUserPermission]
  ? [config].[SP_CheckRolePermission]
  ? [config].[SP_GetAllUsers]
  ? [config].[SP_AssignRoleToUser]
  ? [config].[SP_RemoveRoleFromUser]

INDEXES:
  ? 7 indexes on frequently queried columns
```

---

## ?? QUICK VERIFICATION

### Check 1: Tables Exist
```sql
SELECT TABLE_SCHEMA, TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA IN ('master', 'config', 'core')
ORDER BY TABLE_SCHEMA, TABLE_NAME;

-- Expected Output:
-- config, RolePermissionConfig
-- config, UserRole
-- core, UserAccount
-- master, PermissionMaster
-- master, RoleMaster
```

### Check 2: Procedures Exist
```sql
SELECT SCHEMA_NAME(schema_id) AS SchemaName, name AS ProcedureName
FROM sys.procedures
WHERE schema_id = SCHEMA_ID('config')
AND name LIKE 'SP_%'
ORDER BY name;

-- Expected: 8 procedures listed
```

### Check 3: Data Inserted
```sql
-- Users
SELECT * FROM [core].[UserAccount];
-- Expected: 4 rows (admin, manager, user, viewer)

-- Roles
SELECT * FROM [master].[RoleMaster];
-- Expected: 4 rows (Admin, Manager, User, Viewer)

-- Permissions
SELECT * FROM [master].[PermissionMaster];
-- Expected: 9 rows
```

---

## ? SUCCESS CHECKLIST

After running the script, verify:

- [ ] No error messages in output
- [ ] All 5 tables created
- [ ] All 8 stored procedures created
- [ ] 4 users inserted
- [ ] 4 roles inserted
- [ ] 9 permissions inserted
- [ ] Test queries return expected results
- [ ] No syntax errors

---

## ?? FUNCTIONAL TESTS

### Test Admin User
```sql
EXEC [config].[SP_GetUserPermissions] @UserId = 1
-- Expected: 9 permissions returned
```

### Test Manager User
```sql
EXEC [config].[SP_GetUserPermissions] @UserId = 2
-- Expected: 6 permissions (no delete permissions)
```

### Test Regular User
```sql
EXEC [config].[SP_GetUserPermissions] @UserId = 3
-- Expected: 2 permissions (VIEW_LOOKUPS, VIEW_USERS)
```

### Test Permission Check
```sql
DECLARE @HasPermission BIT
EXEC [config].[SP_CheckUserPermission] 
    @UserId = 3,
    @PermissionCode = 'DELETE_LOOKUPS',
    @HasPermission = @HasPermission OUTPUT
SELECT @HasPermission
-- Expected: 0 (user doesn't have delete permission)
```

---

## ?? TROUBLESHOOTING

### Problem: "Incorrect syntax near the keyword 'User'"
**Solution**: Script uses `[UserAccount]` instead of `[User]` ? (Fixed in new script)

### Problem: "Cannot find schema 'master'"
**Solution**: Script creates schemas automatically - already handled ?

### Problem: "Foreign key constraint failed"
**Solution**: Ensure you're running script on fresh database or drop existing tables first ?

### Problem: "Stored procedure does not exist"
**Solution**: Run verification check to ensure SPs were created ?

---

## ?? PERFORMANCE CONSIDERATIONS

```
Indexes Created:
? IX_RoleMaster_RoleName
? IX_RoleMaster_IsActive
? IX_PermissionMaster_PermissionCode
? IX_PermissionMaster_IsActive
? IX_RolePermissionConfig_RoleId
? IX_RolePermissionConfig_PermissionId
? IX_RolePermissionConfig_IsActive

These indexes ensure:
  • Fast permission lookups
  • Quick user role retrieval
  • Efficient filtering by IsActive
```

---

## ?? NEXT: USE IN C#

After SQL setup, use the repository:

```csharp
// Inject into controller/service
private readonly IRoleRepository _roleRepository;

// Get user permissions
var permissions = await _roleRepository.GetUserPermissionsAsync(userId);

// Check permission
var canDelete = await _roleRepository.UserHasPermissionAsync(userId, "DELETE_LOOKUPS");

// Get user roles
var roles = await _roleRepository.GetUserRoleNamesAsync(userId);
```

---

## ?? DOCUMENTATION REFERENCE

- **Full Details**: `SQL_ROLE_PERMISSION_COMPLETE.md`
- **Quick Guide**: `SQL_QUICK_START.md`
- **Summary**: `SQL_IMPLEMENTATION_SUMMARY.md`
- **Script File**: `SqlScripts/04_RolePermissionSystem.sql`

---

## ? WHAT'S DIFFERENT FROM ORIGINAL

```
ORIGINAL SQL (Had Issues):
? [User] - Reserved keyword error
? No schemas
? No stored procedures
? Raw SQL queries

NEW SQL (Fixed):
? [UserAccount] - No keyword conflicts
? Proper schemas (master, config, core)
? 8 comprehensive stored procedures
? Optimized with indexes
? Production-ready
```

---

## ?? YOU'RE READY!

```
Step 1: Execute SqlScripts/04_RolePermissionSystem.sql
Step 2: Run verification queries
Step 3: See "SUCCESS" in output
Step 4: Start using in C# application
Step 5: Deploy to production
```

---

**Estimated Time: 5 minutes ??**

**Complexity: Simple ?**

**Ready: YES ?**

**Go! ? Execute the SQL script now** ??
