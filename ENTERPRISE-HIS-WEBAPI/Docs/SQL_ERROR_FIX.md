# ?? TROUBLESHOOTING: SQL Error on Line 29

## ? The Error

```
Msg 302, Level 16, State 0, Procedure SP_LookupTypeMaster_Create, Line 29
The newsequentialid() built-in function can only be used in a DEFAULT expression
```

## ? The Fix

The issue is that **old cached procedures** are still in the database. Your code is actually correct now, but SQL Server hasn't picked up the changes.

---

## ?? SOLUTION: Run This Script

Copy and paste this **entire cleanup script** into SQL Server Management Studio and execute it:

```sql
USE [EnterpriseHIS]
GO

-- STEP 1: Drop all procedures to clear cache
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Create]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Create];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetById]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetById];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetAll]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetAll];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetByCode]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetByCode];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Update]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Update];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Delete]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Delete];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Search]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Search];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetCount]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetCount];

IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Create]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Create];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetById]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetById];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetByTypeId]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetByTypeId];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetByTypeCode]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetByTypeCode];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Update]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Update];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Delete]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Delete];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetAll]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetAll];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetCount]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetCount];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Search]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Search];

DBCC FREEPROCCACHE;
GO

-- STEP 2: Recreate all procedures
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\01_LookupTypeMaster_StoredProcedures.sql"
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\02_LookupTypeValueMaster_StoredProcedures.sql"
GO
```

---

## ?? What's Happening

### ? OLD CODE (In Database):
```sql
INSERT INTO [lookup].[LookupTypeMaster] (
    [GlobalId],  -- Takes NEWSEQUENTIALID() here
    ...
)
VALUES 
(
    NEWSEQUENTIALID(),  -- ERROR: Can't use here!
    ...
);
```

### ? NEW CODE (In Your Files):
```sql
DECLARE @NewGlobalId UNIQUEIDENTIFIER = NEWSEQUENTIALID();
INSERT INTO [lookup].[LookupTypeMaster] (
    [GlobalId],
    ...
)
VALUES 
(
    @NewGlobalId,  -- Use variable instead
    ...
);
```

---

## ?? Three Options to Fix

### Option 1: Use the Pre-Made Cleanup Script (EASIEST)
```
File: Database/00_CLEANUP_AND_SETUP.sql
Location: D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\00_CLEANUP_AND_SETUP.sql

Steps:
1. Open in SSMS
2. Click Execute (F5)
3. Done! ?
```

### Option 2: Copy-Paste Cleanup SQL Above
- Copy the cleanup SQL in this guide
- Paste into new SSMS query
- Click Execute (F5)
- Done! ?

### Option 3: Manual Steps
1. **Delete old procedures**:
   - Expand `[EnterpriseHIS]` ? `Programmability` ? `Stored Procedures`
   - Right-click each `SP_Lookup*` procedure
   - Click "Delete"

2. **Refresh SSMS**: Press F5

3. **Run the fixed procedures**:
   - Open `01_LookupTypeMaster_StoredProcedures.sql`
   - Click Execute (F5)
   - Open `02_LookupTypeValueMaster_StoredProcedures.sql`
   - Click Execute (F5)

---

## ? Verify It Worked

After running cleanup, execute this:

```sql
SELECT COUNT(*) AS TotalProcedures
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'lookup'
AND ROUTINE_NAME LIKE 'SP_Lookup%';

-- Should return: 17
```

---

## ?? Test It

Try creating a LookupType:

```sql
USE [EnterpriseHIS]
GO

DECLARE @LookupTypeMasterId INT;
DECLARE @GlobalId UNIQUEIDENTIFIER;

EXEC [lookup].[SP_LookupTypeMaster_Create]
    @LookupTypeName = 'TEST',
    @LookupTypeCode = 'TEST',
    @Description = 'Test lookup',
    @DisplayOrder = 1,
    @IsSystem = 0,
    @CreatedBy = 1,
    @LookupTypeMasterId = @LookupTypeMasterId OUTPUT,
    @GlobalId = @GlobalId OUTPUT;

SELECT @LookupTypeMasterId AS CreatedId, @GlobalId AS GlobalId;
```

**Expected Result:**
```
CreatedId    GlobalId
-----------  ------------------------------------
1            (some guid)
```

---

## ?? Key Point

**Your code is already correct!** The file `01_LookupTypeMaster_StoredProcedures.sql` has the right syntax (lines 34-40 show the DECLARE statement). You just need to:

1. Clear out the **old cached procedures** in the database
2. Recreate them from the fixed files

---

## ?? Still Having Issues?

If you still get errors after cleanup:

1. **Verify you're on the right database:**
   ```sql
   SELECT DB_NAME()  -- Should return: EnterpriseHIS
   ```

2. **Verify the schema exists:**
   ```sql
   SELECT * FROM sys.schemas WHERE name = 'lookup'
   ```

3. **Check connection string in Program.cs:**
   ```
   Should be: Server=.;Database=EnterpriseHIS;...
   ```

4. **Restart SQL Server Management Studio** and try again

---

**?? After cleanup, everything will work perfectly!** ?
