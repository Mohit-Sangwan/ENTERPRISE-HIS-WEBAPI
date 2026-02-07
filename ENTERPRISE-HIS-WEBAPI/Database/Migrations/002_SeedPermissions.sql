-- ============================================================
-- SEED 56 PERMISSIONS INTO EXISTING TABLE
-- ============================================================
-- This script safely adds 56 enterprise permissions
-- Handles existing table structure and skips duplicates
-- .NET 8 | SQL Server 2019+
-- ============================================================

-- STEP 1: Clean up any test/duplicate data
PRINT 'Cleaning up duplicate test data...';
GO

DELETE FROM master.PermissionMaster 
WHERE (Module = 'General' AND Resource = 'General' AND Operation = 'View')
   OR Module IS NULL 
   OR Resource IS NULL 
   OR Operation IS NULL;

PRINT 'Cleanup complete';
GO

-- STEP 2: Insert all 56 permissions
PRINT 'Inserting 56 permissions...';
GO

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
        ('EMR', 'Encounter', 'View', NULL, 'EMR_ENCOUNTER_VIEW', 'EMR - Encounter - View', 'Can view encounters', 'CRUD'),
        ('EMR', 'Encounter', 'Create', NULL, 'EMR_ENCOUNTER_CREATE', 'EMR - Encounter - Create', 'Can create encounters', 'CRUD'),
        ('EMR', 'Encounter', 'Edit', NULL, 'EMR_ENCOUNTER_EDIT', 'EMR - Encounter - Edit', 'Can edit encounters', 'CRUD'),
        ('EMR', 'Encounter', 'Sign', NULL, 'EMR_ENCOUNTER_SIGN', 'EMR - Encounter - Sign', 'Can sign encounters', 'Approval'),
        ('EMR', 'Encounter', 'Verify', NULL, 'EMR_ENCOUNTER_VERIFY', 'EMR - Encounter - Verify', 'Can verify encounters', 'Approval'),
        ('EMR', 'Encounter', 'Export', NULL, 'EMR_ENCOUNTER_EXPORT', 'EMR - Encounter - Export', 'Can export encounter data', 'DataOps'),
        ('EMR', 'Encounter', 'Admin', NULL, 'EMR_ENCOUNTER_ADMIN', 'EMR - Encounter - Admin', 'Admin access to encounters', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.PermissionName, source.Description, source.OperationCategory, 1);

-- Billing Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Billing', 'Invoice', 'View', NULL, 'BILLING_INVOICE_VIEW', 'Billing - Invoice - View', 'Can view invoices', 'CRUD'),
        ('Billing', 'Invoice', 'Create', NULL, 'BILLING_INVOICE_CREATE', 'Billing - Invoice - Create', 'Can create invoices', 'CRUD'),
        ('Billing', 'Invoice', 'Edit', NULL, 'BILLING_INVOICE_EDIT', 'Billing - Invoice - Edit', 'Can edit invoices', 'CRUD'),
        ('Billing', 'Invoice', 'Approve', NULL, 'BILLING_INVOICE_APPROVE', 'Billing - Invoice - Approve', 'Can approve invoices', 'Approval'),
        ('Billing', 'Invoice', 'Export', NULL, 'BILLING_INVOICE_EXPORT', 'Billing - Invoice - Export', 'Can export invoices', 'DataOps'),
        ('Billing', 'Invoice', 'Delete', NULL, 'BILLING_INVOICE_DELETE', 'Billing - Invoice - Delete', 'Can delete invoices', 'CRUD'),
        ('Billing', 'Invoice', 'Admin', NULL, 'BILLING_INVOICE_ADMIN', 'Billing - Invoice - Admin', 'Admin access to invoices', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.PermissionName, source.Description, source.OperationCategory, 1);

-- LIS Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('LIS', 'LabResult', 'View', NULL, 'LIS_LABRESULT_VIEW', 'LIS - LabResult - View', 'Can view lab results', 'CRUD'),
        ('LIS', 'LabResult', 'Create', NULL, 'LIS_LABRESULT_CREATE', 'LIS - LabResult - Create', 'Can create lab results', 'CRUD'),
        ('LIS', 'LabResult', 'Edit', NULL, 'LIS_LABRESULT_EDIT', 'LIS - LabResult - Edit', 'Can edit lab results', 'CRUD'),
        ('LIS', 'LabResult', 'Verify', NULL, 'LIS_LABRESULT_VERIFY', 'LIS - LabResult - Verify', 'Can verify lab results', 'Approval'),
        ('LIS', 'LabResult', 'Approve', NULL, 'LIS_LABRESULT_APPROVE', 'LIS - LabResult - Approve', 'Can approve lab results', 'Approval'),
        ('LIS', 'LabResult', 'Export', NULL, 'LIS_LABRESULT_EXPORT', 'LIS - LabResult - Export', 'Can export lab results', 'DataOps'),
        ('LIS', 'LabResult', 'Admin', NULL, 'LIS_LABRESULT_ADMIN', 'LIS - LabResult - Admin', 'Admin access to lab results', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.PermissionName, source.Description, source.OperationCategory, 1);

-- Pharmacy Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Pharmacy', 'Prescription', 'View', NULL, 'PHARMACY_PRESCRIPTION_VIEW', 'Pharmacy - Prescription - View', 'Can view prescriptions', 'CRUD'),
        ('Pharmacy', 'Prescription', 'Create', NULL, 'PHARMACY_PRESCRIPTION_CREATE', 'Pharmacy - Prescription - Create', 'Can create prescriptions', 'CRUD'),
        ('Pharmacy', 'Prescription', 'Edit', NULL, 'PHARMACY_PRESCRIPTION_EDIT', 'Pharmacy - Prescription - Edit', 'Can edit prescriptions', 'CRUD'),
        ('Pharmacy', 'Prescription', 'Approve', NULL, 'PHARMACY_PRESCRIPTION_APPROVE', 'Pharmacy - Prescription - Approve', 'Can approve prescriptions', 'Approval'),
        ('Pharmacy', 'Prescription', 'Cancel', NULL, 'PHARMACY_PRESCRIPTION_CANCEL', 'Pharmacy - Prescription - Cancel', 'Can cancel prescriptions', 'StateManagement'),
        ('Pharmacy', 'Prescription', 'Export', NULL, 'PHARMACY_PRESCRIPTION_EXPORT', 'Pharmacy - Prescription - Export', 'Can export prescriptions', 'DataOps'),
        ('Pharmacy', 'Prescription', 'Admin', NULL, 'PHARMACY_PRESCRIPTION_ADMIN', 'Pharmacy - Prescription - Admin', 'Admin access to prescriptions', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.PermissionName, source.Description, source.OperationCategory, 1);

-- Reports Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Reports', 'Report', 'View', NULL, 'REPORTS_REPORT_VIEW', 'Reports - Report - View', 'Can view reports', 'CRUD'),
        ('Reports', 'Report', 'Create', NULL, 'REPORTS_REPORT_CREATE', 'Reports - Report - Create', 'Can create reports', 'CRUD'),
        ('Reports', 'Report', 'Edit', NULL, 'REPORTS_REPORT_EDIT', 'Reports - Report - Edit', 'Can edit reports', 'CRUD'),
        ('Reports', 'Report', 'Export', NULL, 'REPORTS_REPORT_EXPORT', 'Reports - Report - Export', 'Can export reports', 'DataOps'),
        ('Reports', 'Report', 'Print', NULL, 'REPORTS_REPORT_PRINT', 'Reports - Report - Print', 'Can print reports', 'DataOps'),
        ('Reports', 'Report', 'Schedule', NULL, 'REPORTS_REPORT_SCHEDULE', 'Reports - Report - Schedule', 'Can schedule reports', 'Advanced'),
        ('Reports', 'Report', 'Admin', NULL, 'REPORTS_REPORT_ADMIN', 'Reports - Report - Admin', 'Admin access to reports', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.PermissionName, source.Description, source.OperationCategory, 1);

-- Settings Module (7 permissions)
MERGE INTO master.PermissionMaster AS target
USING (
    VALUES 
        ('Settings', 'Configuration', 'View', NULL, 'SETTINGS_CONFIGURATION_VIEW', 'Settings - Configuration - View', 'Can view settings', 'CRUD'),
        ('Settings', 'Configuration', 'Edit', NULL, 'SETTINGS_CONFIGURATION_EDIT', 'Settings - Configuration - Edit', 'Can edit settings', 'CRUD'),
        ('Settings', 'Configuration', 'Restore', NULL, 'SETTINGS_CONFIGURATION_RESTORE', 'Settings - Configuration - Restore', 'Can restore default settings', 'Advanced'),
        ('Settings', 'Audit', 'View', NULL, 'SETTINGS_AUDIT_VIEW', 'Settings - Audit - View', 'Can view audit logs', 'CRUD'),
        ('Settings', 'Audit', 'Export', NULL, 'SETTINGS_AUDIT_EXPORT', 'Settings - Audit - Export', 'Can export audit logs', 'DataOps'),
        ('Settings', 'System', 'Monitor', NULL, 'SETTINGS_SYSTEM_MONITOR', 'Settings - System - Monitor', 'Can monitor system', 'Advanced'),
        ('Settings', 'System', 'Admin', NULL, 'SETTINGS_SYSTEM_ADMIN', 'Settings - System - Admin', 'Admin access to system settings', 'Advanced')
) AS source (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory)
ON target.PermissionCode = source.PermissionCode
WHEN NOT MATCHED THEN
    INSERT (Module, Resource, Operation, Scope, PermissionCode, PermissionName, Description, OperationCategory, IsActive)
    VALUES (source.Module, source.Resource, source.Operation, source.Scope, source.PermissionCode, source.PermissionName, source.Description, source.OperationCategory, 1);

GO

-- STEP 3: Verify success
PRINT 'Verifying results...';
GO

SELECT 'Total Permissions' AS [Status], COUNT(*) AS [Count]
FROM master.PermissionMaster
WHERE IsActive = 1;

SELECT 
    Module, 
    COUNT(*) AS PermissionCount,
    STRING_AGG(CONCAT(Operation, '(', Resource, ')'), ', ') AS Operations
FROM master.PermissionMaster
WHERE IsActive = 1
GROUP BY Module
ORDER BY Module;

PRINT '? All 56 permissions seeded successfully!';
