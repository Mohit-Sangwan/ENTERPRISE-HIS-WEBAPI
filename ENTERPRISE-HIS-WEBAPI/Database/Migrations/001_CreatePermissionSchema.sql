-- ============================================================
-- ENTERPRISE AUTHORIZATION FRAMEWORK - DATABASE SCHEMA
-- ============================================================
-- Phase 2: Database Integration for Zero-Hardcoding Authorization
-- .NET 8 | SQL Server 2019+
--
-- This script creates the schema for enterprise-grade authorization
-- Based on Module.Resource.Operation permission model
-- ============================================================

-- ============================================================
-- 0. CREATE SCHEMAS (Must run first!)
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'master')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA master';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'config')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA config';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'audit')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA audit';
END
GO

-- ============================================================
-- 1. PERMISSION MASTER TABLE
-- ============================================================
-- Stores all permissions in the system
-- Format: Module.Resource.Operation[.Scope]
-- Example: Lookups.LookupType.View
--          Billing.Invoice.Approve
--          EMR.Encounter.Sign.Department:ED

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'master.PermissionMaster') AND type in (N'U'))
BEGIN
    CREATE TABLE master.PermissionMaster (
        PermissionId INT PRIMARY KEY IDENTITY(1,1),
        
        -- Permission components (database-driven, not code constants)
        Module NVARCHAR(50) NOT NULL,              -- "Lookups", "Billing", "EMR", etc.
        Resource NVARCHAR(50) NOT NULL,            -- "LookupType", "Invoice", "Encounter"
        Operation NVARCHAR(50) NOT NULL,           -- "View", "Create", "Delete", "Approve"
        Scope NVARCHAR(50),                        -- NULL (global), "Facility", "Department", custom
        
        -- Metadata
        PermissionCode NVARCHAR(100) NOT NULL,     -- Unique code: "LOOKUPS_LOOKUPTYPE_VIEW"
        Description NVARCHAR(500),                 -- "Can view all lookups"
        OperationCategory NVARCHAR(50),            -- "CRUD", "Approval", "StateManagement", "DataOps", "Advanced"
        
        -- Status
        IsActive BIT DEFAULT 1,                    -- Soft delete capability
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        ModifiedAt DATETIME2 DEFAULT GETUTCDATE()
    );
    
    PRINT 'Created master.PermissionMaster table';
END
ELSE
BEGIN
    PRINT 'Table master.PermissionMaster already exists - skipping creation';
END
GO

-- Add constraints if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints 
               WHERE name = 'UQ_PermissionMaster_ModuleResourceOperationScope'
               AND parent_object_id = OBJECT_ID('master.PermissionMaster'))
BEGIN
    ALTER TABLE master.PermissionMaster
    ADD CONSTRAINT UQ_PermissionMaster_ModuleResourceOperationScope 
    UNIQUE (Module, Resource, Operation, Scope);
    PRINT 'Added UNIQUE constraint to PermissionMaster';
END
GO

-- Create indexes if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Module_Resource' 
               AND object_id = OBJECT_ID('master.PermissionMaster'))
BEGIN
    CREATE INDEX IX_Module_Resource ON master.PermissionMaster(Module, Resource);
    PRINT 'Created IX_Module_Resource index';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PermissionCode' 
               AND object_id = OBJECT_ID('master.PermissionMaster'))
BEGIN
    CREATE INDEX IX_PermissionCode ON master.PermissionMaster(PermissionCode);
    PRINT 'Created IX_PermissionCode index';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IsActive' 
               AND object_id = OBJECT_ID('master.PermissionMaster'))
BEGIN
    CREATE INDEX IX_IsActive ON master.PermissionMaster(IsActive);
    PRINT 'Created IX_IsActive index';
END
GO

-- ============================================================
-- 2. ROLE PERMISSION MAPPING TABLE
-- ============================================================
-- Maps roles to their permissions
-- Many-to-Many relationship

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'config.RolePermissionMapping') AND type in (N'U'))
BEGIN
    CREATE TABLE config.RolePermissionMapping (
        RolePermissionId INT PRIMARY KEY IDENTITY(1,1),
        
        -- Foreign Keys
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL FOREIGN KEY REFERENCES master.PermissionMaster(PermissionId),
        
        -- Audit
        AssignedAt DATETIME2 DEFAULT GETUTCDATE(),
        AssignedByUserId INT
    );
    
    PRINT 'Created config.RolePermissionMapping table';
END
ELSE
BEGIN
    PRINT 'Table config.RolePermissionMapping already exists - skipping creation';
END
GO

-- Add constraints if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints 
               WHERE name = 'UQ_RolePermissionMapping_RoleIdPermissionId'
               AND parent_object_id = OBJECT_ID('config.RolePermissionMapping'))
BEGIN
    ALTER TABLE config.RolePermissionMapping
    ADD CONSTRAINT UQ_RolePermissionMapping_RoleIdPermissionId 
    UNIQUE (RoleId, PermissionId);
    PRINT 'Added UNIQUE constraint to RolePermissionMapping';
END
GO

-- Create indexes if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RoleId' 
               AND object_id = OBJECT_ID('config.RolePermissionMapping'))
BEGIN
    CREATE INDEX IX_RoleId ON config.RolePermissionMapping(RoleId);
    PRINT 'Created IX_RoleId index';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PermissionId' 
               AND object_id = OBJECT_ID('config.RolePermissionMapping'))
BEGIN
    CREATE INDEX IX_PermissionId ON config.RolePermissionMapping(PermissionId);
    PRINT 'Created IX_PermissionId index';
END
GO

-- ============================================================
-- 3. AUTHORIZATION ACCESS LOG TABLE
-- ============================================================
-- Audit trail for all authorization decisions (success & denial)
-- Critical for compliance: NABH, ISO, SOC2

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'audit.AuthorizationAccessLog') AND type in (N'U'))
BEGIN
    CREATE TABLE audit.AuthorizationAccessLog (
        LogId BIGINT PRIMARY KEY IDENTITY(1,1),
        
        -- User & Request Info
        UserId INT NOT NULL,
        Permission NVARCHAR(200) NOT NULL,         -- "Module.Resource.Operation[.Scope]"
        
        -- Request Details
        Path NVARCHAR(500) NOT NULL,               -- "/api/v1/invoices/123"
        Method NVARCHAR(10) NOT NULL,              -- "GET", "POST", "DELETE"
        StatusCode INT NOT NULL,                   -- 200, 403, etc.
        
        -- For denied access
        DeniedReason NVARCHAR(500),                -- "User does not have permission"
        
        -- Client Info
        IpAddress NVARCHAR(50),                    -- Client IP
        UserAgent NVARCHAR(500),                   -- Browser/Client info
        
        -- Timestamp
        Timestamp DATETIME2 DEFAULT GETUTCDATE()
    );
    
    PRINT 'Created audit.AuthorizationAccessLog table';
END
ELSE
BEGIN
    PRINT 'Table audit.AuthorizationAccessLog already exists - skipping creation';
END
GO

-- Create indexes if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserId' 
               AND object_id = OBJECT_ID('audit.AuthorizationAccessLog'))
BEGIN
    CREATE INDEX IX_UserId ON audit.AuthorizationAccessLog(UserId);
    PRINT 'Created IX_UserId index';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Permission' 
               AND object_id = OBJECT_ID('audit.AuthorizationAccessLog'))
BEGIN
    CREATE INDEX IX_Permission ON audit.AuthorizationAccessLog(Permission);
    PRINT 'Created IX_Permission index';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StatusCode' 
               AND object_id = OBJECT_ID('audit.AuthorizationAccessLog'))
BEGIN
    CREATE INDEX IX_StatusCode ON audit.AuthorizationAccessLog(StatusCode);
    PRINT 'Created IX_StatusCode index';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Timestamp' 
               AND object_id = OBJECT_ID('audit.AuthorizationAccessLog'))
BEGIN
    CREATE INDEX IX_Timestamp ON audit.AuthorizationAccessLog(Timestamp);
    PRINT 'Created IX_Timestamp index';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DeniedAccess' 
               AND object_id = OBJECT_ID('audit.AuthorizationAccessLog'))
BEGIN
    CREATE INDEX IX_DeniedAccess ON audit.AuthorizationAccessLog(StatusCode, Timestamp) WHERE StatusCode = 403;
    PRINT 'Created IX_DeniedAccess index';
END
GO

-- ============================================================
-- 4. CLEAN UP DUPLICATE DATA
-- ============================================================

-- Delete duplicate/test data from existing table (if any)
DELETE FROM master.PermissionMaster 
WHERE Module = 'General' AND Resource = 'General' AND Operation = 'View';

PRINT 'Cleaned up duplicate test data';
GO

-- ============================================================
-- 4. SEED DATA - ALL PERMISSIONS (56 pre-configured)
-- ============================================================

-- Helper: Insert permissions without duplicates
SET NOCOUNT ON;

-- Lookups Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Lookups', 'LookupType', 'View', NULL, 'LOOKUPS_LOOKUPTYPE_VIEW', 'Lookups - LookupType - View', 'Can view all lookups', 'CRUD'),
        ('Lookups', 'LookupType', 'Create', NULL, 'LOOKUPS_LOOKUPTYPE_CREATE', 'Lookups - LookupType - Create', 'Can create lookups', 'CRUD'),
        ('Lookups', 'LookupType', 'Edit', NULL, 'LOOKUPS_LOOKUPTYPE_EDIT', 'Lookups - LookupType - Edit', 'Can edit lookups', 'CRUD'),
        ('Lookups', 'LookupType', 'Delete', NULL, 'LOOKUPS_LOOKUPTYPE_DELETE', 'Lookups - LookupType - Delete', 'Can delete lookups', 'CRUD'),
        ('Lookups', 'LookupType', 'Print', NULL, 'LOOKUPS_LOOKUPTYPE_PRINT', 'Lookups - LookupType - Print', 'Can print lookups', 'DataOps'),
        ('Lookups', 'LookupType', 'Export', NULL, 'LOOKUPS_LOOKUPTYPE_EXPORT', 'Lookups - LookupType - Export', 'Can export lookups', 'DataOps'),
        ('Lookups', 'LookupType', 'Admin', NULL, 'LOOKUPS_LOOKUPTYPE_ADMIN', 'Lookups - LookupType - Admin', 'Admin access to lookups', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.PermissionName, source.Description, source.OperationCategory, 1);

-- Administration Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Administration', 'User', 'View', NULL, 'ADMINISTRATION_USER_VIEW', 'Administration - User - View', 'Can view users', 'CRUD'),
        ('Administration', 'User', 'Create', NULL, 'ADMINISTRATION_USER_CREATE', 'Administration - User - Create', 'Can create users', 'CRUD'),
        ('Administration', 'User', 'Edit', NULL, 'ADMINISTRATION_USER_EDIT', 'Administration - User - Edit', 'Can edit users', 'CRUD'),
        ('Administration', 'User', 'Delete', NULL, 'ADMINISTRATION_USER_DELETE', 'Administration - User - Delete', 'Can delete users', 'CRUD'),
        ('Administration', 'Role', 'View', NULL, 'ADMINISTRATION_ROLE_VIEW', 'Administration - Role - View', 'Can view roles', 'CRUD'),
        ('Administration', 'Role', 'Manage', NULL, 'ADMINISTRATION_ROLE_MANAGE', 'Administration - Role - Manage', 'Can manage roles', 'Advanced'),
        ('Administration', 'Policy', 'Admin', NULL, 'ADMINISTRATION_POLICY_ADMIN', 'Administration - Policy - Admin', 'Admin access to policies', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.PermissionName, source.Description, source.OperationCategory, 1);

-- EMR Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('EMR', 'Encounter', 'View', NULL, 'EMR_ENCOUNTER_VIEW', 'Can view encounters', 'CRUD'),
        ('EMR', 'Encounter', 'Create', NULL, 'EMR_ENCOUNTER_CREATE', 'Can create encounters', 'CRUD'),
        ('EMR', 'Encounter', 'Edit', NULL, 'EMR_ENCOUNTER_EDIT', 'Can edit encounters', 'CRUD'),
        ('EMR', 'Encounter', 'Sign', NULL, 'EMR_ENCOUNTER_SIGN', 'Can sign encounters', 'Approval'),
        ('EMR', 'Encounter', 'Verify', NULL, 'EMR_ENCOUNTER_VERIFY', 'Can verify encounters', 'Approval'),
        ('EMR', 'Encounter', 'Export', NULL, 'EMR_ENCOUNTER_EXPORT', 'Can export encounter data', 'DataOps'),
        ('EMR', 'Encounter', 'Admin', NULL, 'EMR_ENCOUNTER_ADMIN', 'Admin access to encounters', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.Description, source.OperationCategory, 1);

-- Billing Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Billing', 'Invoice', 'View', NULL, 'BILLING_INVOICE_VIEW', 'Can view invoices', 'CRUD'),
        ('Billing', 'Invoice', 'Create', NULL, 'BILLING_INVOICE_CREATE', 'Can create invoices', 'CRUD'),
        ('Billing', 'Invoice', 'Edit', NULL, 'BILLING_INVOICE_EDIT', 'Can edit invoices', 'CRUD'),
        ('Billing', 'Invoice', 'Approve', NULL, 'BILLING_INVOICE_APPROVE', 'Can approve invoices', 'Approval'),
        ('Billing', 'Invoice', 'Export', NULL, 'BILLING_INVOICE_EXPORT', 'Can export invoices', 'DataOps'),
        ('Billing', 'Invoice', 'Delete', NULL, 'BILLING_INVOICE_DELETE', 'Can delete invoices', 'CRUD'),
        ('Billing', 'Invoice', 'Admin', NULL, 'BILLING_INVOICE_ADMIN', 'Admin access to invoices', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.Description, source.OperationCategory, 1);

-- LIS Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('LIS', 'LabResult', 'View', NULL, 'LIS_LABRESULT_VIEW', 'Can view lab results', 'CRUD'),
        ('LIS', 'LabResult', 'Create', NULL, 'LIS_LABRESULT_CREATE', 'Can create lab results', 'CRUD'),
        ('LIS', 'LabResult', 'Edit', NULL, 'LIS_LABRESULT_EDIT', 'Can edit lab results', 'CRUD'),
        ('LIS', 'LabResult', 'Verify', NULL, 'LIS_LABRESULT_VERIFY', 'Can verify lab results', 'Approval'),
        ('LIS', 'LabResult', 'Approve', NULL, 'LIS_LABRESULT_APPROVE', 'Can approve lab results', 'Approval'),
        ('LIS', 'LabResult', 'Export', NULL, 'LIS_LABRESULT_EXPORT', 'Can export lab results', 'DataOps'),
        ('LIS', 'LabResult', 'Admin', NULL, 'LIS_LABRESULT_ADMIN', 'Admin access to lab results', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.Description, source.OperationCategory, 1);

-- Pharmacy Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Pharmacy', 'Prescription', 'View', NULL, 'PHARMACY_PRESCRIPTION_VIEW', 'Can view prescriptions', 'CRUD'),
        ('Pharmacy', 'Prescription', 'Create', NULL, 'PHARMACY_PRESCRIPTION_CREATE', 'Can create prescriptions', 'CRUD'),
        ('Pharmacy', 'Prescription', 'Edit', NULL, 'PHARMACY_PRESCRIPTION_EDIT', 'Can edit prescriptions', 'CRUD'),
        ('Pharmacy', 'Prescription', 'Approve', NULL, 'PHARMACY_PRESCRIPTION_APPROVE', 'Can approve prescriptions', 'Approval'),
        ('Pharmacy', 'Prescription', 'Cancel', NULL, 'PHARMACY_PRESCRIPTION_CANCEL', 'Can cancel prescriptions', 'StateManagement'),
        ('Pharmacy', 'Prescription', 'Export', NULL, 'PHARMACY_PRESCRIPTION_EXPORT', 'Can export prescriptions', 'DataOps'),
        ('Pharmacy', 'Prescription', 'Admin', NULL, 'PHARMACY_PRESCRIPTION_ADMIN', 'Admin access to prescriptions', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.Description, source.OperationCategory, 1);

-- Reports Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Reports', 'Report', 'View', NULL, 'REPORTS_REPORT_VIEW', 'Can view reports', 'CRUD'),
        ('Reports', 'Report', 'Create', NULL, 'REPORTS_REPORT_CREATE', 'Can create reports', 'CRUD'),
        ('Reports', 'Report', 'Edit', NULL, 'REPORTS_REPORT_EDIT', 'Can edit reports', 'CRUD'),
        ('Reports', 'Report', 'Export', NULL, 'REPORTS_REPORT_EXPORT', 'Can export reports', 'DataOps'),
        ('Reports', 'Report', 'Print', NULL, 'REPORTS_REPORT_PRINT', 'Can print reports', 'DataOps'),
        ('Reports', 'Report', 'Schedule', NULL, 'REPORTS_REPORT_SCHEDULE', 'Can schedule reports', 'Advanced'),
        ('Reports', 'Report', 'Admin', NULL, 'REPORTS_REPORT_ADMIN', 'Admin access to reports', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.Description, source.OperationCategory, 1);

-- Settings Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Settings', 'Configuration', 'View', NULL, 'SETTINGS_CONFIGURATION_VIEW', 'Can view settings', 'CRUD'),
        ('Settings', 'Configuration', 'Edit', NULL, 'SETTINGS_CONFIGURATION_EDIT', 'Can edit settings', 'CRUD'),
        ('Settings', 'Configuration', 'Restore', NULL, 'SETTINGS_CONFIGURATION_RESTORE', 'Can restore default settings', 'Advanced'),
        ('Settings', 'Audit', 'View', NULL, 'SETTINGS_AUDIT_VIEW', 'Can view audit logs', 'CRUD'),
        ('Settings', 'Audit', 'Export', NULL, 'SETTINGS_AUDIT_EXPORT', 'Can export audit logs', 'DataOps'),
        ('Settings', 'System', 'Monitor', NULL, 'SETTINGS_SYSTEM_MONITOR', 'Can monitor system', 'Advanced'),
        ('Settings', 'System', 'Admin', NULL, 'SETTINGS_SYSTEM_ADMIN', 'Admin access to system settings', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.Description, source.OperationCategory, 1);

SET NOCOUNT OFF;

-- ============================================================
-- 5. VERIFICATION QUERIES
-- ============================================================

-- Verify total permissions loaded
SELECT 'Total Permissions Created' AS [Status], COUNT(*) AS [Count]
FROM master.PermissionMaster
WHERE IsActive = 1;

-- List permissions by module
SELECT 
    Module, 
    COUNT(*) AS PermissionCount,
    STRING_AGG(CONCAT(Operation, '(', Resource, ')'), ', ') AS Operations
FROM master.PermissionMaster
WHERE IsActive = 1
GROUP BY Module
ORDER BY Module;

-- ============================================================
-- 6. INDEXES FOR PERFORMANCE
-- ============================================================

-- Already created above, but here's a summary:
-- IX_Module_Resource: For quick lookup by module/resource
-- IX_PermissionCode: For direct permission lookups
-- IX_IsActive: For filtering active permissions
-- IX_RoleId: For user permission queries
-- IX_PermissionId: For permission assignment queries
-- IX_Timestamp: For audit log queries
-- IX_DeniedAccess: Optimized for security analysis

-- ============================================================
-- COMPLETION CHECKLIST
-- ============================================================
/*
? Created master.PermissionMaster (56 permissions pre-seeded)
? Created config.RolePermissionMapping (role-permission mapping)
? Created audit.AuthorizationAccessLog (complete audit trail)
? Added all necessary indexes
? Ready for PermissionService implementation
? Ready for Phase 2.2: Permission service queries

NEXT STEPS:
1. Run this migration script
2. Verify permissions seeded: SELECT COUNT(*) FROM master.PermissionMaster
3. Implement PermissionService
4. Update TokenService
5. Remove [Authorize] attributes
6. Deploy to production
*/
