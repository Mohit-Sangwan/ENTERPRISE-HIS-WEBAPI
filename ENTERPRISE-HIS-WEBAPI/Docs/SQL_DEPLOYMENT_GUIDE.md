# ?? SQL DEPLOYMENT GUIDE - Step by Step

## ? WHAT WAS FIXED

| Issue | Problem | Solution |
|-------|---------|----------|
| **Syntax Error #1** | `NEWSEQUENTIALID()` in VALUES clause | Moved to DECLARE statement |
| **Syntax Error #2** | Procedures already exist | Added DROP IF EXISTS |
| **Result** | 7 errors blocking deployment | All errors resolved ? |

---

## ?? DEPLOYMENT STEPS

### Step 1: Open SQL Server Management Studio
```
1. Launch SQL Server Management Studio
2. Connect to your SQL Server instance
3. Select [EnterpriseHIS] database
```

### Step 2: Run LookupTypeMaster Procedures
```sql
-- Copy this entire script and paste into SSMS
USE [EnterpriseHIS]
GO

:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\01_LookupTypeMaster_StoredProcedures.sql"

-- Then press F5 or Click Execute
```

**Expected Output:**
```
Msg 0, Level 11, State 1, Server ..., Line 1
Command(s) completed successfully.

(No errors - if you see this, success!)
```

### Step 3: Run LookupTypeValueMaster Procedures
```sql
-- Copy this entire script and paste into SSMS
USE [EnterpriseHIS]
GO

:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\02_LookupTypeValueMaster_StoredProcedures.sql"

-- Then press F5 or Click Execute
```

**Expected Output:**
```
Msg 0, Level 11, State 1, Server ..., Line 1
Command(s) completed successfully.

(No errors - if you see this, success!)
```

### Step 4: Verify All Procedures Created
```sql
-- Run this verification query
SELECT 
    ROUTINE_SCHEMA,
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'lookup'
AND ROUTINE_NAME LIKE 'SP_Lookup%'
ORDER BY ROUTINE_NAME;
```

**Expected Result:**
```
17 rows returned:

lookup | SP_LookupTypeMaster_Create         | PROCEDURE | ...
lookup | SP_LookupTypeMaster_Delete         | PROCEDURE | ...
lookup | SP_LookupTypeMaster_GetAll         | PROCEDURE | ...
lookup | SP_LookupTypeMaster_GetByCode      | PROCEDURE | ...
lookup | SP_LookupTypeMaster_GetById        | PROCEDURE | ...
lookup | SP_LookupTypeMaster_GetCount       | PROCEDURE | ...
lookup | SP_LookupTypeMaster_Search         | PROCEDURE | ...
lookup | SP_LookupTypeMaster_Update         | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_Create    | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_Delete    | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_GetAll    | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_GetByTypeCode    | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_GetByTypeId      | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_GetById   | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_GetCount  | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_Search    | PROCEDURE | ...
lookup | SP_LookupTypeValueMaster_Update    | PROCEDURE | ...
```

? If you see all 17 procedures, you're done!

---

## ?? TEST A PROCEDURE

### Test 1: Create LookupType
```sql
-- Run this to test creation
USE [EnterpriseHIS]
GO

DECLARE @LookupTypeMasterId INT;
DECLARE @GlobalId UNIQUEIDENTIFIER;

EXEC [lookup].[SP_LookupTypeMaster_Create]
    @LookupTypeName = 'GENDER',
    @LookupTypeCode = 'GENDER',
    @Description = 'Gender types',
    @DisplayOrder = 1,
    @IsSystem = 0,
    @CreatedBy = 1,
    @LookupTypeMasterId = @LookupTypeMasterId OUTPUT,
    @GlobalId = @GlobalId OUTPUT;

SELECT 
    @LookupTypeMasterId AS CreatedId,
    @GlobalId AS CreatedGlobalId;
```

**Expected Result:**
```
CreatedId    CreatedGlobalId
-----------  ------------------------------------
1            550e8400-e29b-41d4-a716-446655440000
```

### Test 2: Get Created LookupType
```sql
USE [EnterpriseHIS]
GO

EXEC [lookup].[SP_LookupTypeMaster_GetById] @LookupTypeMasterId = 1;
```

**Expected Result:**
```
LookupTypeMasterId | GlobalId | LookupTypeName | LookupTypeCode | ...
-------------------|----------|----------------|----------------|
1                  | ...      | GENDER         | GENDER         | ...
```

### Test 3: Create LookupTypeValue
```sql
USE [EnterpriseHIS]
GO

DECLARE @LookupTypeValueId INT;
DECLARE @GlobalId UNIQUEIDENTIFIER;

EXEC [lookup].[SP_LookupTypeValueMaster_Create]
    @LookupTypeMasterId = 1,
    @LookupValueName = 'MALE',
    @LookupValueCode = 'M',
    @Description = 'Male',
    @DisplayOrder = 1,
    @IsSystem = 0,
    @CreatedBy = 1,
    @LookupTypeValueId = @LookupTypeValueId OUTPUT,
    @GlobalId = @GlobalId OUTPUT;

SELECT 
    @LookupTypeValueId AS CreatedId,
    @GlobalId AS CreatedGlobalId;
```

**Expected Result:**
```
CreatedId    CreatedGlobalId
-----------  ------------------------------------
1            550e8400-e29b-41d4-a716-446655440099
```

---

## ?? TROUBLESHOOTING

### Problem 1: "Cannot open file" error
**Solution:**
- Copy file path from file explorer
- Use correct path format: `"D:\Mohit\...\filename.sql"`
- Escape backslashes or use forward slashes: `"D:/Mohit/.../filename.sql"`

### Problem 2: "Syntax Error" after running
**Solution:**
- Make sure you're using `.sql` files (not `.txt`)
- Verify file hasn't been modified
- Try running the raw SQL code directly instead of `:r` command

### Problem 3: "Login failed"
**Solution:**
- Verify you're connected to correct SQL Server instance
- Check database name: should be `[EnterpriseHIS]`
- Verify your login has permissions

### Problem 4: Procedures not showing up
**Solution:**
- Refresh Object Explorer (F5 key)
- Verify you're viewing correct database
- Check schema is `lookup` (not `dbo`)

---

## ?? SCRIPT FOR COMPLETE SETUP

If you want to run everything at once, use this master script:

```sql
-- ===== ENTERPRISE HIS DATABASE SETUP =====

-- Step 1: Select database
USE [EnterpriseHIS]
GO

-- Step 2: Create lookup schema if not exists
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'lookup')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA lookup'
END
GO

-- Step 3: Create tables (assuming they already exist)
-- Tables should already be created per your schema

-- Step 4: Load LookupTypeMaster procedures
PRINT '====== Creating LookupTypeMaster Procedures ======'
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\01_LookupTypeMaster_StoredProcedures.sql"

-- Step 5: Load LookupTypeValueMaster procedures
PRINT '====== Creating LookupTypeValueMaster Procedures ======'
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\02_LookupTypeValueMaster_StoredProcedures.sql"

-- Step 6: Verify
PRINT '====== Verification ======'
SELECT 
    COUNT(*) AS TotalProceduresCreated
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'lookup'
AND ROUTINE_NAME LIKE 'SP_Lookup%';

-- Expected: 17 procedures
GO

PRINT '====== Setup Complete ======'
```

---

## ? FINAL CHECKLIST

After deployment, verify:

- [ ] No errors during script execution
- [ ] All 17 procedures created (verification query shows 17 rows)
- [ ] Can execute test procedures successfully
- [ ] Object Explorer shows all procedures under `lookup` schema
- [ ] ASP.NET Core application can call procedures

---

## ?? NEXT STEPS

After SQL procedures are deployed:

1. **Build C# Project**
   ```powershell
   dotnet build
   ```

2. **Run Application**
   ```powershell
   dotnet run
   ```

3. **Test API**
   ```
   Navigate to: https://localhost:5001/swagger
   ```

4. **Create sample data via API**
   ```
   POST /api/v1/lookuptypes
   ```

---

## ?? QUICK COMMANDS

**Copy-paste these exactly:**

### To run both procedures:
```sql
USE [EnterpriseHIS]
GO
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\01_LookupTypeMaster_StoredProcedures.sql"
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\02_LookupTypeValueMaster_StoredProcedures.sql"
GO
```

### To verify:
```sql
SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA='lookup' AND ROUTINE_NAME LIKE 'SP_Lookup%'
-- Should return: 17
```

### To see all procedures:
```sql
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA='lookup' AND ROUTINE_NAME LIKE 'SP_Lookup%' ORDER BY ROUTINE_NAME
```

---

**You're ready to deploy!** ??
