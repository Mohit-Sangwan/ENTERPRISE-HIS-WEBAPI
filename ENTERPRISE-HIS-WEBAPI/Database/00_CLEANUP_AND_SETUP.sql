-- ===== COMPLETE CLEANUP AND RECREATION SCRIPT =====
-- Run this ENTIRE script all at once to fix the issue

USE [EnterpriseHIS]
GO

-- ===== STEP 1: DROP ALL OLD PROCEDURES =====
PRINT '===== DROPPING ALL EXISTING PROCEDURES ====='

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

PRINT 'All procedures dropped successfully'
GO

-- ===== STEP 2: CLEAR PROCEDURE CACHE =====
DBCC FREEPROCCACHE
GO

PRINT 'Procedure cache cleared'
GO

-- ===== STEP 3: NOW RUN THE FIXED PROCEDURES =====
PRINT '===== CREATING NEW PROCEDURES ====='
GO

-- Execute the corrected procedures
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\01_LookupTypeMaster_StoredProcedures.sql"
GO

:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\02_LookupTypeValueMaster_StoredProcedures.sql"
GO

-- ===== VERIFICATION =====
PRINT '===== VERIFICATION ====='
SELECT 
    ROUTINE_SCHEMA,
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'lookup'
AND ROUTINE_NAME LIKE 'SP_Lookup%'
ORDER BY ROUTINE_NAME;

PRINT 'Expected: 17 rows (8 LookupTypeMaster + 9 LookupTypeValueMaster)'
GO

PRINT '===== SETUP COMPLETE ====='
GO
