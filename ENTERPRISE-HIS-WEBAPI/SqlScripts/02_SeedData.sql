-- =====================================================
-- ENTERPRISE HIS - SAMPLE DATA
-- Database: EnterpriseHIS
-- Purpose: Insert test data for development and testing
-- =====================================================

-- ===== INSERT ROLES =====
INSERT INTO master.RoleMaster (RoleName, Description, IsActive, CreatedDate)
VALUES
    ('Admin', 'System Administrator with full access', 1, GETUTCDATE()),
    ('Manager', 'Manager with oversight permissions', 1, GETUTCDATE()),
    ('Doctor', 'Doctor with clinical permissions', 1, GETUTCDATE()),
    ('Nurse', 'Nurse with clinical support permissions', 1, GETUTCDATE()),
    ('Viewer', 'Read-only access', 1, GETUTCDATE());

-- ===== INSERT PERMISSIONS =====
INSERT INTO master.PermissionMaster (PermissionName, PermissionCode, Description, IsActive, CreatedDate)
VALUES
    -- Lookup Permissions
    ('View Lookups', 'VIEW_LOOKUPS', 'Can view lookup master data', 1, GETUTCDATE()),
    ('Create Lookups', 'CREATE_LOOKUPS', 'Can create new lookup entries', 1, GETUTCDATE()),
    ('Edit Lookups', 'EDIT_LOOKUPS', 'Can edit existing lookup entries', 1, GETUTCDATE()),
    ('Delete Lookups', 'DELETE_LOOKUPS', 'Can delete lookup entries', 1, GETUTCDATE()),
    
    -- User Management Permissions
    ('View Users', 'VIEW_USERS', 'Can view user accounts', 1, GETUTCDATE()),
    ('Create Users', 'CREATE_USERS', 'Can create new users', 1, GETUTCDATE()),
    ('Edit Users', 'EDIT_USERS', 'Can edit user accounts', 1, GETUTCDATE()),
    ('Delete Users', 'DELETE_USERS', 'Can delete user accounts', 1, GETUTCDATE()),
    
    -- Role & Permission Management
    ('Manage Roles', 'MANAGE_ROLES', 'Can manage roles and permissions', 1, GETUTCDATE()),
    
    -- Patient Management
    ('View Patients', 'VIEW_PATIENTS', 'Can view patient records', 1, GETUTCDATE()),
    ('Create Patients', 'CREATE_PATIENTS', 'Can create patient records', 1, GETUTCDATE()),
    ('Edit Patients', 'EDIT_PATIENTS', 'Can edit patient records', 1, GETUTCDATE()),
    ('Delete Patients', 'DELETE_PATIENTS', 'Can delete patient records', 1, GETUTCDATE()),
    
    -- Clinical Permissions
    ('View Appointments', 'VIEW_APPOINTMENTS', 'Can view appointments', 1, GETUTCDATE()),
    ('Create Appointments', 'CREATE_APPOINTMENTS', 'Can create appointments', 1, GETUTCDATE()),
    ('Edit Appointments', 'EDIT_APPOINTMENTS', 'Can edit appointments', 1, GETUTCDATE()),
    ('View Medical Records', 'VIEW_MEDICAL_RECORDS', 'Can view medical records', 1, GETUTCDATE()),
    ('Create Prescriptions', 'CREATE_PRESCRIPTIONS', 'Can create prescriptions', 1, GETUTCDATE()),
    
    -- Audit & Compliance
    ('View Audit Logs', 'VIEW_AUDIT_LOGS', 'Can view system audit logs', 1, GETUTCDATE()),
    ('Export Data', 'EXPORT_DATA', 'Can export data', 1, GETUTCDATE());

-- ===== INSERT USERS =====
INSERT INTO core.UserAccount (Username, Email, PasswordHash, IsActive, CreatedDate)
VALUES
    ('admin', 'admin@enterprise-his.com', '$2a$11$admin_hash_placeholder', 1, GETUTCDATE()),
    ('manager', 'manager@enterprise-his.com', '$2a$11$manager_hash_placeholder', 1, GETUTCDATE()),
    ('doctor_john', 'john.smith@enterprise-his.com', '$2a$11$doctor_hash_placeholder', 1, GETUTCDATE()),
    ('nurse_jane', 'jane.doe@enterprise-his.com', '$2a$11$nurse_hash_placeholder', 1, GETUTCDATE()),
    ('viewer_user', 'viewer@enterprise-his.com', '$2a$11$viewer_hash_placeholder', 1, GETUTCDATE());

-- ===== ASSIGN ROLES TO USERS =====

-- Admin user gets Admin role
INSERT INTO config.UserRoleConfig (UserId, RoleId, IsActive, AssignedDate)
SELECT u.UserId, r.RoleId, 1, GETUTCDATE()
FROM core.UserAccount u, master.RoleMaster r
WHERE u.Username = 'admin' AND r.RoleName = 'Admin';

-- Manager user gets Manager role
INSERT INTO config.UserRoleConfig (UserId, RoleId, IsActive, AssignedDate)
SELECT u.UserId, r.RoleId, 1, GETUTCDATE()
FROM core.UserAccount u, master.RoleMaster r
WHERE u.Username = 'manager' AND r.RoleName = 'Manager';

-- Doctor user gets Doctor role
INSERT INTO config.UserRoleConfig (UserId, RoleId, IsActive, AssignedDate)
SELECT u.UserId, r.RoleId, 1, GETUTCDATE()
FROM core.UserAccount u, master.RoleMaster r
WHERE u.Username = 'doctor_john' AND r.RoleName = 'Doctor';

-- Nurse user gets Nurse role
INSERT INTO config.UserRoleConfig (UserId, RoleId, IsActive, AssignedDate)
SELECT u.UserId, r.RoleId, 1, GETUTCDATE()
FROM core.UserAccount u, master.RoleMaster r
WHERE u.Username = 'nurse_jane' AND r.RoleName = 'Nurse';

-- Viewer user gets Viewer role
INSERT INTO config.UserRoleConfig (UserId, RoleId, IsActive, AssignedDate)
SELECT u.UserId, r.RoleId, 1, GETUTCDATE()
FROM core.UserAccount u, master.RoleMaster r
WHERE u.Username = 'viewer_user' AND r.RoleName = 'Viewer';

-- ===== ASSIGN PERMISSIONS TO ROLES =====

-- Admin role gets ALL permissions
INSERT INTO config.RolePermissionConfig (RoleId, PermissionId, IsActive, AssignedDate)
SELECT r.RoleId, p.PermissionId, 1, GETUTCDATE()
FROM master.RoleMaster r
CROSS JOIN master.PermissionMaster p
WHERE r.RoleName = 'Admin' AND p.IsActive = 1;

-- Manager role gets management permissions
INSERT INTO config.RolePermissionConfig (RoleId, PermissionId, IsActive, AssignedDate)
SELECT r.RoleId, p.PermissionId, 1, GETUTCDATE()
FROM master.RoleMaster r, master.PermissionMaster p
WHERE r.RoleName = 'Manager'
  AND p.PermissionCode IN (
    'VIEW_LOOKUPS', 'VIEW_USERS', 'VIEW_PATIENTS', 
    'VIEW_APPOINTMENTS', 'VIEW_MEDICAL_RECORDS', 'VIEW_AUDIT_LOGS'
  );

-- Doctor role gets clinical permissions
INSERT INTO config.RolePermissionConfig (RoleId, PermissionId, IsActive, AssignedDate)
SELECT r.RoleId, p.PermissionId, 1, GETUTCDATE()
FROM master.RoleMaster r, master.PermissionMaster p
WHERE r.RoleName = 'Doctor'
  AND p.PermissionCode IN (
    'VIEW_PATIENTS', 'CREATE_PATIENTS', 'EDIT_PATIENTS',
    'VIEW_APPOINTMENTS', 'CREATE_APPOINTMENTS', 'EDIT_APPOINTMENTS',
    'VIEW_MEDICAL_RECORDS', 'CREATE_PRESCRIPTIONS'
  );

-- Nurse role gets support permissions
INSERT INTO config.RolePermissionConfig (RoleId, PermissionId, IsActive, AssignedDate)
SELECT r.RoleId, p.PermissionId, 1, GETUTCDATE()
FROM master.RoleMaster r, master.PermissionMaster p
WHERE r.RoleName = 'Nurse'
  AND p.PermissionCode IN (
    'VIEW_PATIENTS', 'VIEW_APPOINTMENTS', 'VIEW_MEDICAL_RECORDS'
  );

-- Viewer role gets read-only permissions
INSERT INTO config.RolePermissionConfig (RoleId, PermissionId, IsActive, AssignedDate)
SELECT r.RoleId, p.PermissionId, 1, GETUTCDATE()
FROM master.RoleMaster r, master.PermissionMaster p
WHERE r.RoleName = 'Viewer'
  AND p.PermissionCode IN (
    'VIEW_LOOKUPS', 'VIEW_USERS', 'VIEW_PATIENTS', 
    'VIEW_APPOINTMENTS', 'VIEW_MEDICAL_RECORDS'
  );

-- ===== VERIFY DATA =====
PRINT '==== SAMPLE DATA INSERTED SUCCESSFULLY ====';
PRINT '';
PRINT 'Roles:';
SELECT RoleId, RoleName, Description FROM master.RoleMaster WHERE IsActive = 1;
PRINT '';
PRINT 'Permissions:';
SELECT PermissionId, PermissionCode, PermissionName FROM master.PermissionMaster WHERE IsActive = 1;
PRINT '';
PRINT 'Users:';
SELECT UserId, Username, Email FROM core.UserAccount WHERE IsActive = 1;
PRINT '';
PRINT 'User Roles:';
SELECT u.Username, r.RoleName FROM config.UserRoleConfig urc
JOIN core.UserAccount u ON urc.UserId = u.UserId
JOIN master.RoleMaster r ON urc.RoleId = r.RoleId
WHERE urc.IsActive = 1;
