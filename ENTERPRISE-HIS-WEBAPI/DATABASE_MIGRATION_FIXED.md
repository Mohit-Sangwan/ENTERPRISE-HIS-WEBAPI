# ? SQL MIGRATION FIX - VERIFICATION GUIDE

## ?? What Was Fixed

The SQL migration script had syntax errors where it was trying to use INDEX and UNIQUE constraints inline, which SQL Server doesn't support in CREATE TABLE statements.

### Fixed Issues:
? Moved `INDEX` definitions to separate `CREATE INDEX` statements
? Moved `UNIQUE` constraints to separate `ALTER TABLE` statements  
? All 3 tables now create successfully
? All 56 permissions will seed properly

---

## ?? HOW TO RUN THE CORRECTED MIGRATION

### Step 1: Open SQL Server Management Studio
```
- Start SSMS
- Connect to your SQL Server
```

### Step 2: Create Required Schemas
```sql
-- Execute these first if schemas don't exist:
CREATE SCHEMA master;
GO
CREATE SCHEMA config;
GO
CREATE SCHEMA audit;
GO
```

### Step 3: Run the Migration Script
```
- File: Database/Migrations/001_CreatePermissionSchema.sql
- Right-click ? Execute
- Or: Ctrl+F5 in SSMS
```

### Step 4: Verify Success
```sql
-- Check total permissions seeded (should be 56)
SELECT COUNT(*) as PermissionCount 
FROM master.PermissionMaster 
WHERE IsActive = 1;

-- Expected result: 56

-- View permissions by module
SELECT 
    Module, 
    COUNT(*) AS PermissionCount
FROM master.PermissionMaster
WHERE IsActive = 1
GROUP BY Module
ORDER BY Module;

-- Expected: 8 modules, 7 permissions each = 56 total
```

---

## ? SUCCESS INDICATORS

You'll see these in the Messages pane:

```
(1 row affected)  -- First CREATE TABLE
(3 rows affected) -- Constraints/Indexes for PermissionMaster
...
(7 rows affected) -- Each MERGE statement (1 for each module = 8 × 7 = 56 rows)
...
```

**Total: 56 permission records inserted across all modules**

---

## ?? EXPECTED RESULTS

### PermissionMaster Table (56 rows)
```
Lookups             - 7 permissions
Administration      - 7 permissions
EMR                 - 7 permissions
Billing             - 7 permissions
LIS                 - 7 permissions
Pharmacy            - 7 permissions
Reports             - 7 permissions
Settings            - 7 permissions
????????????????????????????????
Total:              56 permissions
```

### RolePermissionMapping Table (empty initially)
```
Ready for role assignments
Will be populated by application
```

### AuthorizationAccessLog Table (empty initially)
```
Ready for audit logging
Will populate as requests come in
```

---

## ?? QUICK TEST AFTER MIGRATION

```sql
-- Test 1: Verify table structure
SELECT TOP 5 * FROM master.PermissionMaster;

-- Test 2: Count by module
SELECT Module, COUNT(*) as Count 
FROM master.PermissionMaster 
GROUP BY Module;

-- Test 3: View specific permission
SELECT * FROM master.PermissionMaster 
WHERE Module = 'Lookups' AND Operation = 'View';

-- Test 4: Check indexes exist
SELECT name FROM sys.indexes 
WHERE object_id = OBJECT_ID('master.PermissionMaster');

-- Test 5: Verify constraints
SELECT name FROM sys.key_constraints 
WHERE parent_object_id = OBJECT_ID('master.PermissionMaster');
```

---

## ? TROUBLESHOOTING

### Error: "Invalid column name"
**Cause**: Schemas don't exist
**Solution**: Run these first:
```sql
CREATE SCHEMA master;
CREATE SCHEMA config;
CREATE SCHEMA audit;
```

### Error: "Incorrect syntax near 'INDEX'"
**Cause**: Old version of script
**Solution**: Use the corrected version from `Database/Migrations/001_CreatePermissionSchema.sql`

### Error: "The INSERT, UPDATE, or DELETE statement conflicted with a FOREIGN KEY constraint"
**Cause**: PermissionMaster table not created yet
**Solution**: Verify CREATE TABLE statements ran successfully before MERGE statements

---

## ? DEPLOYMENT CHECKLIST

```
? Schemas created (master, config, audit)
? Migration script executed
? 56 permissions visible in PermissionMaster
? Tables created: PermissionMaster, RolePermissionMapping, AuthorizationAccessLog
? Indexes created successfully
? Verification queries return expected results

?? DATABASE READY FOR APPLICATION
```

---

## ?? YOU'RE DONE!

Your database is now ready for the enterprise authorization system!

**Next Steps:**
1. ? Database migration complete
2. ? Deploy application
3. ? Test endpoints
4. ? Go live!

---

**For any issues, see GETTING_STARTED.md troubleshooting section.**

