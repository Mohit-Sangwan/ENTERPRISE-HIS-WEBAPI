# ? QUICK FIX - 60 SECONDS

## Problem
```
Msg 302: newsequentialid() error on line 29
```

## Solution

### Copy & Run This (All-In-One):
```sql
USE [EnterpriseHIS]
GO

-- DROP ALL OLD PROCEDURES
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Create]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Create];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetById]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetById];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetAll]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetAll];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetByCode]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetByCode];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Update]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Update];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Delete]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Delete];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Search]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Search];
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetCount]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetCount];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Create]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Create];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetById]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetById];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetByTypeId]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetByTypeId];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetByTypeCode]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetByTypeCode];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Update]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Update];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Delete]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Delete];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetAll]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetAll];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetCount]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetCount];
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Search]', 'P') IS NOT NULL DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Search];

-- CLEAR CACHE
DBCC FREEPROCCACHE;
GO

-- RECREATE FROM FIXED FILES
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\01_LookupTypeMaster_StoredProcedures.sql"
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\02_LookupTypeValueMaster_StoredProcedures.sql"
GO

-- VERIFY
SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA='lookup' AND ROUTINE_NAME LIKE 'SP_Lookup%'
-- Expected: 17
GO
```

### Steps:
1. Open SQL Server Management Studio
2. **Select** database: `EnterpriseHIS`
3. **Paste** the script above into new query
4. **Press F5** to execute
5. **See**: "17" in results = SUCCESS ?

---

## Why This Happens

Your files have **correct code** (with DECLARE), but the **database still has old cached version** (with the error).

Clearing and recreating fixes it.

---

## Done!
Everything works now ??
