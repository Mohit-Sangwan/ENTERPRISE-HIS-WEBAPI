-- ============================================================================
-- ENTERPRISE-LEVEL MODULE/MENU-BASED POLICY MIGRATION
-- Migrate from specific policies to module-based enterprise policies
-- Includes: View, Print, Create, Edit, Delete, Manage, Admin per module
-- ============================================================================

-- ============================================================================
-- BACKUP EXISTING POLICIES
-- ============================================================================

PRINT '===== BACKING UP EXISTING POLICIES =====';

IF OBJECT_ID('[master].[PolicyMaster_Backup_' + CONVERT(VARCHAR, GETDATE(), 112) + ']') IS NULL
BEGIN
    SELECT * INTO [master].[PolicyMaster_Backup_20240207]
    FROM [master].[PolicyMaster];
    PRINT 'Backup created: [master].[PolicyMaster_Backup_20240207]';
END;

-- ============================================================================
-- DELETE EXISTING OLD POLICIES (from old system)
-- ============================================================================

PRINT '';
PRINT '===== DELETING OLD MODULE-SPECIFIC POLICIES =====';

DELETE FROM [config].[RolePolicyMapping]
WHERE [PolicyId] IN (
    SELECT [PolicyId] FROM [master].[PolicyMaster]
    WHERE [PolicyCode] LIKE 'CAN_%' OR [PolicyCode] = 'ADMIN_ONLY' OR [PolicyCode] = 'MANAGE_ROLES'
);
PRINT 'Old policy mappings removed from [config].[RolePolicyMapping]';

DELETE FROM [master].[PolicyMaster]
WHERE [PolicyCode] LIKE 'CAN_%' OR [PolicyCode] = 'ADMIN_ONLY' OR [PolicyCode] = 'MANAGE_ROLES';
PRINT 'Old policies removed from [master].[PolicyMaster]';

-- ============================================================================
-- SECTION 1: LOOKUPS MODULE POLICIES
-- ============================================================================

PRINT '';
PRINT '===== INSERTING LOOKUPS MODULE POLICIES =====';

INSERT INTO [master].[PolicyMaster] 
([PolicyName], [PolicyCode], [Description], [Module], [IsActive], [CreatedAt])
VALUES 
('Lookups - View', 'LOOKUPS_VIEW', 'Can view lookup data', 'Lookups', 1, GETUTCDATE()),
('Lookups - Print', 'LOOKUPS_PRINT', 'Can print lookup data', 'Lookups', 1, GETUTCDATE()),
('Lookups - Create', 'LOOKUPS_CREATE', 'Can create lookups', 'Lookups', 1, GETUTCDATE()),
('Lookups - Edit', 'LOOKUPS_EDIT', 'Can edit lookups', 'Lookups', 1, GETUTCDATE()),
('Lookups - Delete', 'LOOKUPS_DELETE', 'Can delete lookups', 'Lookups', 1, GETUTCDATE()),
('Lookups - Manage', 'LOOKUPS_MANAGE', 'Can manage lookups (full control)', 'Lookups', 1, GETUTCDATE()),
('Lookups - Admin', 'LOOKUPS_ADMIN', 'Admin access to lookups', 'Lookups', 1, GETUTCDATE());

PRINT 'Lookups module policies inserted (7 policies)';

-- ============================================================================
-- SECTION 2: USERS MODULE POLICIES
-- ============================================================================

PRINT '';
PRINT '===== INSERTING USERS MODULE POLICIES =====';

INSERT INTO [master].[PolicyMaster] 
([PolicyName], [PolicyCode], [Description], [Module], [IsActive], [CreatedAt])
VALUES 
('Users - View', 'USERS_VIEW', 'Can view users', 'Users', 1, GETUTCDATE()),
('Users - Print', 'USERS_PRINT', 'Can print user list', 'Users', 1, GETUTCDATE()),
('Users - Create', 'USERS_CREATE', 'Can create users', 'Users', 1, GETUTCDATE()),
('Users - Edit', 'USERS_EDIT', 'Can edit users', 'Users', 1, GETUTCDATE()),
('Users - Delete', 'USERS_DELETE', 'Can delete users', 'Users', 1, GETUTCDATE()),
('Users - Manage', 'USERS_MANAGE', 'Can manage users (full control)', 'Users', 1, GETUTCDATE()),
('Users - Admin', 'USERS_ADMIN', 'Admin access to users', 'Users', 1, GETUTCDATE());

PRINT 'Users module policies inserted (7 policies)';

-- ============================================================================
-- SECTION 3: ROLES MODULE POLICIES
-- ============================================================================

PRINT '';
PRINT '===== INSERTING ROLES MODULE POLICIES =====';

INSERT INTO [master].[PolicyMaster] 
([PolicyName], [PolicyCode], [Description], [Module], [IsActive], [CreatedAt])
VALUES 
('Roles - View', 'ROLES_VIEW', 'Can view roles', 'Roles', 1, GETUTCDATE()),
('Roles - Print', 'ROLES_PRINT', 'Can print role list', 'Roles', 1, GETUTCDATE()),
('Roles - Create', 'ROLES_CREATE', 'Can create roles', 'Roles', 1, GETUTCDATE()),
('Roles - Edit', 'ROLES_EDIT', 'Can edit roles', 'Roles', 1, GETUTCDATE()),
('Roles - Delete', 'ROLES_DELETE', 'Can delete roles', 'Roles', 1, GETUTCDATE()),
('Roles - Manage', 'ROLES_MANAGE', 'Can manage roles (full control)', 'Roles', 1, GETUTCDATE()),
('Roles - Admin', 'ROLES_ADMIN', 'Admin access to roles', 'Roles', 1, GETUTCDATE());

PRINT 'Roles module policies inserted (7 policies)';

-- ============================================================================
-- SECTION 4: PATIENTS MODULE POLICIES (Future module)
-- ============================================================================

PRINT '';
PRINT '===== INSERTING PATIENTS MODULE POLICIES =====';

INSERT INTO [master].[PolicyMaster] 
([PolicyName], [PolicyCode], [Description], [Module], [IsActive], [CreatedAt])
VALUES 
('Patients - View', 'PATIENTS_VIEW', 'Can view patients', 'Patients', 1, GETUTCDATE()),
('Patients - Print', 'PATIENTS_PRINT', 'Can print patient data', 'Patients', 1, GETUTCDATE()),
('Patients - Create', 'PATIENTS_CREATE', 'Can create patients', 'Patients', 1, GETUTCDATE()),
('Patients - Edit', 'PATIENTS_EDIT', 'Can edit patients', 'Patients', 1, GETUTCDATE()),
('Patients - Delete', 'PATIENTS_DELETE', 'Can delete patients', 'Patients', 1, GETUTCDATE()),
('Patients - Manage', 'PATIENTS_MANAGE', 'Can manage patients (full control)', 'Patients', 1, GETUTCDATE()),
('Patients - Admin', 'PATIENTS_ADMIN', 'Admin access to patients', 'Patients', 1, GETUTCDATE());

PRINT 'Patients module policies inserted (7 policies)';

-- ============================================================================
-- SECTION 5: APPOINTMENTS MODULE POLICIES (Future module)
-- ============================================================================

PRINT '';
PRINT '===== INSERTING APPOINTMENTS MODULE POLICIES =====';

INSERT INTO [master].[PolicyMaster] 
([PolicyName], [PolicyCode], [Description], [Module], [IsActive], [CreatedAt])
VALUES 
('Appointments - View', 'APPOINTMENTS_VIEW', 'Can view appointments', 'Appointments', 1, GETUTCDATE()),
('Appointments - Print', 'APPOINTMENTS_PRINT', 'Can print appointments', 'Appointments', 1, GETUTCDATE()),
('Appointments - Create', 'APPOINTMENTS_CREATE', 'Can create appointments', 'Appointments', 1, GETUTCDATE()),
('Appointments - Edit', 'APPOINTMENTS_EDIT', 'Can edit appointments', 'Appointments', 1, GETUTCDATE()),
('Appointments - Delete', 'APPOINTMENTS_DELETE', 'Can delete appointments', 'Appointments', 1, GETUTCDATE()),
('Appointments - Manage', 'APPOINTMENTS_MANAGE', 'Can manage appointments (full control)', 'Appointments', 1, GETUTCDATE()),
('Appointments - Admin', 'APPOINTMENTS_ADMIN', 'Admin access to appointments', 'Appointments', 1, GETUTCDATE());

PRINT 'Appointments module policies inserted (7 policies)';

-- ============================================================================
-- SECTION 6: PRESCRIPTIONS MODULE POLICIES (Future module)
-- ============================================================================

PRINT '';
PRINT '===== INSERTING PRESCRIPTIONS MODULE POLICIES =====';

INSERT INTO [master].[PolicyMaster] 
([PolicyName], [PolicyCode], [Description], [Module], [IsActive], [CreatedAt])
VALUES 
('Prescriptions - View', 'PRESCRIPTIONS_VIEW', 'Can view prescriptions', 'Prescriptions', 1, GETUTCDATE()),
('Prescriptions - Print', 'PRESCRIPTIONS_PRINT', 'Can print prescriptions', 'Prescriptions', 1, GETUTCDATE()),
('Prescriptions - Create', 'PRESCRIPTIONS_CREATE', 'Can create prescriptions', 'Prescriptions', 1, GETUTCDATE()),
('Prescriptions - Edit', 'PRESCRIPTIONS_EDIT', 'Can edit prescriptions', 'Prescriptions', 1, GETUTCDATE()),
('Prescriptions - Delete', 'PRESCRIPTIONS_DELETE', 'Can delete prescriptions', 'Prescriptions', 1, GETUTCDATE()),
('Prescriptions - Manage', 'PRESCRIPTIONS_MANAGE', 'Can manage prescriptions (full control)', 'Prescriptions', 1, GETUTCDATE()),
('Prescriptions - Admin', 'PRESCRIPTIONS_ADMIN', 'Admin access to prescriptions', 'Prescriptions', 1, GETUTCDATE());

PRINT 'Prescriptions module policies inserted (7 policies)';

-- ============================================================================
-- SECTION 7: ASSIGN POLICIES TO ROLES
-- ============================================================================

PRINT '';
PRINT '===== ASSIGNING POLICIES TO ROLES =====';

-- Get role IDs
DECLARE @AdminRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] LIKE '%Admin%');
DECLARE @ManagerRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] LIKE '%Manager%');
DECLARE @UserRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] LIKE '%User%');
DECLARE @ViewerRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] LIKE '%Viewer%');

PRINT '';
PRINT 'Role IDs:';
PRINT 'Admin: ' + ISNULL(CAST(@AdminRoleId AS VARCHAR(10)), 'NOT FOUND');
PRINT 'Manager: ' + ISNULL(CAST(@ManagerRoleId AS VARCHAR(10)), 'NOT FOUND');
PRINT 'User: ' + ISNULL(CAST(@UserRoleId AS VARCHAR(10)), 'NOT FOUND');
PRINT 'Viewer: ' + ISNULL(CAST(@ViewerRoleId AS VARCHAR(10)), 'NOT FOUND');

-- ========== ADMIN ROLE: ALL POLICIES ==========
PRINT '';
PRINT 'Assigning ALL policies to Admin role...';

IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId], [AssignedAt])
    SELECT @AdminRoleId, p.[PolicyId], GETUTCDATE()
    FROM [master].[PolicyMaster] p
    WHERE NOT EXISTS (
        SELECT 1 FROM [config].[RolePolicyMapping] 
        WHERE [RoleId] = @AdminRoleId AND [PolicyId] = p.[PolicyId]
    );
    PRINT 'Admin role: All policies assigned';
END

-- ========== MANAGER ROLE: View, Print, Create, Edit, Manage (NO Delete/Admin) ==========
PRINT '';
PRINT 'Assigning View, Print, Create, Edit, Manage policies to Manager role...';

IF @ManagerRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId], [AssignedAt])
    SELECT @ManagerRoleId, p.[PolicyId], GETUTCDATE()
    FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] LIKE '%_VIEW' 
       OR p.[PolicyCode] LIKE '%_PRINT'
       OR p.[PolicyCode] LIKE '%_CREATE'
       OR p.[PolicyCode] LIKE '%_EDIT'
       OR p.[PolicyCode] LIKE '%_MANAGE'
    AND NOT EXISTS (
        SELECT 1 FROM [config].[RolePolicyMapping] 
        WHERE [RoleId] = @ManagerRoleId AND [PolicyId] = p.[PolicyId]
    );
    PRINT 'Manager role: View, Print, Create, Edit, Manage policies assigned';
END

-- ========== USER ROLE: View, Print only (NO Create/Edit/Delete/Admin) ==========
PRINT '';
PRINT 'Assigning View, Print policies to User role...';

IF @UserRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId], [AssignedAt])
    SELECT @UserRoleId, p.[PolicyId], GETUTCDATE()
    FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] LIKE '%_VIEW' 
       OR p.[PolicyCode] LIKE '%_PRINT'
    AND NOT EXISTS (
        SELECT 1 FROM [config].[RolePolicyMapping] 
        WHERE [RoleId] = @UserRoleId AND [PolicyId] = p.[PolicyId]
    );
    PRINT 'User role: View, Print policies assigned';
END

-- ========== VIEWER ROLE: View only (LIMITED ACCESS) ==========
PRINT '';
PRINT 'Assigning View policies to Viewer role...';

IF @ViewerRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId], [AssignedAt])
    SELECT @ViewerRoleId, p.[PolicyId], GETUTCDATE()
    FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] LIKE '%_VIEW'
    AND NOT EXISTS (
        SELECT 1 FROM [config].[RolePolicyMapping] 
        WHERE [RoleId] = @ViewerRoleId AND [PolicyId] = p.[PolicyId]
    );
    PRINT 'Viewer role: View policies assigned';
END

-- ============================================================================
-- SECTION 8: VERIFICATION QUERIES
-- ============================================================================

PRINT '';
PRINT '===== VERIFICATION QUERIES =====';
PRINT '';

-- Count new policies
SELECT 
    COUNT(*) AS [Total Policies],
    COUNT(DISTINCT [Module]) AS [Modules],
    COUNT(DISTINCT [PolicyCode]) AS [Unique Codes]
FROM [master].[PolicyMaster];

PRINT '';
PRINT '===== POLICIES BY MODULE =====';
SELECT [Module], COUNT(*) AS [Policy Count]
FROM [master].[PolicyMaster]
GROUP BY [Module]
ORDER BY [Module];

PRINT '';
PRINT '===== POLICIES BY ROLE =====';
SELECT 
    r.[RoleName],
    COUNT(rpm.[PolicyId]) AS [Total Policies],
    COUNT(DISTINCT SUBSTRING(p.[PolicyCode], 1, CHARINDEX('_', p.[PolicyCode]) - 1)) AS [Modules]
FROM [master].[RoleMaster] r
LEFT JOIN [config].[RolePolicyMapping] rpm ON r.[RoleId] = rpm.[RoleId]
LEFT JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]
GROUP BY r.[RoleId], r.[RoleName]
ORDER BY r.[RoleName];

PRINT '';
PRINT '===== SAMPLE: ADMIN ROLE POLICIES =====';
SELECT 
    p.[PolicyName],
    p.[PolicyCode],
    p.[Module]
FROM [config].[RolePolicyMapping] rpm
INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]
INNER JOIN [master].[RoleMaster] r ON rpm.[RoleId] = r.[RoleId]
WHERE r.[RoleName] LIKE '%Admin%'
ORDER BY p.[Module], p.[PolicyCode];

-- ============================================================================
-- FINAL SUMMARY
-- ============================================================================

PRINT '';
PRINT '===== MIGRATION COMPLETE =====';
PRINT 'Total Modules: 6 (Lookups, Users, Roles, Patients, Appointments, Prescriptions)';
PRINT 'Total Policies: 42 (7 per module)';
PRINT 'Total Roles: 4 (Admin, Manager, User, Viewer)';
PRINT '';
PRINT 'Backup Location: [master].[PolicyMaster_Backup_20240207]';
PRINT 'Please verify all policies and role assignments before deployment.';
PRINT '';
