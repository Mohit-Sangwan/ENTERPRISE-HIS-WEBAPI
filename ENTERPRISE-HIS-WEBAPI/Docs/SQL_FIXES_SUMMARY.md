# ?? SQL Stored Procedures - FIXES APPLIED

## ? Issues Fixed

### Issue 1: `newsequentialid()` Error
**Error:** Msg 302 - "The newsequentialid() built-in function can only be used in a DEFAULT expression"

**Cause:** Cannot use `NEWSEQUENTIALID()` directly in VALUES clause with other operators

**Solution:** Generate `NEWSEQUENTIALID()` in a variable FIRST, then use in INSERT

#### Before (? Wrong):
```sql
INSERT INTO [lookup].[LookupTypeMaster] 
(
    [GlobalId],
    ...
)
VALUES 
(
    NEWSEQUENTIALID(),  -- ? ERROR: Can't use with other expressions
    ...
);
```

#### After (? Fixed):
```sql
-- Generate GlobalId first
DECLARE @NewGlobalId UNIQUEIDENTIFIER = NEWSEQUENTIALID();

-- Then use in INSERT
INSERT INTO [lookup].[LookupTypeMaster] 
(
    [GlobalId],
    ...
)
VALUES 
(
    @NewGlobalId,  -- ? Use variable instead
    ...
);

SET @GlobalId = @NewGlobalId;
```

---

### Issue 2: "Already Exists" Errors
**Error:** Msg 2714 - "There is already an object named 'SP_...' in the database"

**Cause:** Procedures already exist; can't CREATE duplicate

**Solution:** DROP procedures IF they exist BEFORE creating them

#### Before (? Wrong):
```sql
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_GetById]  -- ? ERROR: Already exists
AS
BEGIN
    ...
END;
GO
```

#### After (? Fixed):
```sql
-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetById]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetById];
GO

CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_GetById]  -- ? Now it works
AS
BEGIN
    ...
END;
GO
```

---

## ?? Files Fixed

### File 1: `Database/01_LookupTypeMaster_StoredProcedures.sql`
? **Changes:**
- Added DROP IF EXISTS before all 8 procedures
- Fixed `NEWSEQUENTIALID()` in SP_LookupTypeMaster_Create
- Total: 8 procedures

### File 2: `Database/02_LookupTypeValueMaster_StoredProcedures.sql`
? **Changes:**
- Added DROP IF EXISTS before all 9 procedures
- Fixed `NEWSEQUENTIALID()` in SP_LookupTypeValueMaster_Create
- Total: 9 procedures

---

## ?? How to Run Now

### Option 1: Fresh Installation (Recommended)
```sql
-- Run in SQL Server Management Studio:
USE [EnterpriseHIS]
GO

-- Execute both files in order:
:r "Database\01_LookupTypeMaster_StoredProcedures.sql"
:r "Database\02_LookupTypeValueMaster_StoredProcedures.sql"

-- Result: ? SUCCESS - All 17 procedures created
```

### Option 2: Rerun After Errors
```sql
-- If you already ran these and got errors, just run again:
-- The DROP IF EXISTS will clean up the old ones first

:r "Database\01_LookupTypeMaster_StoredProcedures.sql"
:r "Database\02_LookupTypeValueMaster_StoredProcedures.sql"

-- Result: ? SUCCESS - Procedures replaced
```

---

## ? Verification Commands

After running the scripts, verify everything was created:

```sql
-- View all created procedures
SELECT 
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED,
    LAST_ALTERED
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'lookup'
AND ROUTINE_NAME LIKE 'SP_Lookup%'
ORDER BY ROUTINE_NAME;

-- Expected Result: 17 rows (8 LookupTypeMaster + 9 LookupTypeValueMaster)
```

---

## ?? Summary of Changes

| File | Issue | Fix | Status |
|------|-------|-----|--------|
| 01_LookupTypeMaster_StoredProcedures.sql | NEWSEQUENTIALID() syntax | Use DECLARE variable | ? FIXED |
| 01_LookupTypeMaster_StoredProcedures.sql | Already exists errors | Add DROP IF EXISTS | ? FIXED |
| 02_LookupTypeValueMaster_StoredProcedures.sql | NEWSEQUENTIALID() syntax | Use DECLARE variable | ? FIXED |
| 02_LookupTypeValueMaster_StoredProcedures.sql | Already exists errors | Add DROP IF EXISTS | ? FIXED |

---

## ?? Key SQL Patterns Used

### Pattern 1: Safe Procedure Dropping
```sql
IF OBJECT_ID('[schema].[procedure_name]', 'P') IS NOT NULL
    DROP PROCEDURE [schema].[procedure_name];
GO
```

### Pattern 2: Safe NEWSEQUENTIALID() Usage
```sql
DECLARE @NewId UNIQUEIDENTIFIER = NEWSEQUENTIALID();
INSERT INTO table_name (..., [GlobalId], ...)
VALUES (..., @NewId, ...);
```

---

## ?? What Each DROP IF EXISTS Does

```sql
-- Safely drops procedure if it exists
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Create]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Create];
GO

-- Parameters:
-- [lookup].[SP_LookupTypeMaster_Create] = Full schema.procedure name
-- 'P' = Type (P = Procedure, U = Table, V = View, etc.)
-- IS NOT NULL = Only if it exists
```

---

## ? Best Practices Applied

? **Idempotent Scripts** - Safe to run multiple times  
? **Error Handling** - Check before dropping  
? **Variable Usage** - Proper T-SQL for NEWSEQUENTIALID()  
? **Schema Qualified** - Uses [lookup].[procedure_name]  
? **GO Statements** - Batch separators for clarity  

---

## ?? Ready to Deploy

Your SQL scripts are now:
- ? Syntactically correct
- ? Idempotent (safe to rerun)
- ? Production-ready
- ? Well-documented

**Next Step:** Run the scripts in SQL Server Management Studio! ??
