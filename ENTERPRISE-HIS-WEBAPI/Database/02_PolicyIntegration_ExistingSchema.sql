-- ============================================================================
-- ENTERPRISE-LEVEL POLICY MANAGEMENT - INTEGRATION WITH EXISTING SCHEMA
-- This script integrates with existing tables:
-- - master.RoleMaster (existing roles)
-- - master.PermissionMaster (existing permissions)
-- - config.RolePermissionConfig (role-permission mappings)
-- - core.UserAccount (existing users)
-- - config.UserRole (user-role assignments)
-- ============================================================================

-- ============================================================================
-- STEP 1: VERIFY EXISTING TABLES
-- ============================================================================

PRINT '===== VERIFYING EXISTING SCHEMA =====';

-- Check if master.RoleMaster exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[master].[RoleMaster]') AND type in (N'U'))
    PRINT 'ü Found [master].[RoleMaster]'
ELSE
    PRINT 'û [master].[RoleMaster] NOT FOUND - Please create this table first';

-- Check if master.PermissionMaster exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[master].[PermissionMaster]') AND type in (N'U'))
    PRINT 'ü Found [master].[PermissionMaster]'
ELSE
    PRINT 'û [master].[PermissionMaster] NOT FOUND - Please create this table first';

-- Check if config.RolePermissionConfig exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[config].[RolePermissionConfig]') AND type in (N'U'))
    PRINT 'ü Found [config].[RolePermissionConfig]'
ELSE
    PRINT 'û [config].[RolePermissionConfig] NOT FOUND - Please create this table first';

-- ============================================================================
-- STEP 2: CREATE POLICY TABLE (if not exists) - Uses existing master schema
-- ============================================================================

PRINT '';
PRINT '===== CREATING POLICY MANAGEMENT TABLES =====';

-- Create Policy table in master schema to align with existing structure
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[master].[PolicyMaster]') AND type in (N'U'))
BEGIN
    CREATE TABLE [master].[PolicyMaster]
    (
        [PolicyId] INT IDENTITY(1,1) PRIMARY KEY,
        [PolicyName] NVARCHAR(100) NOT NULL UNIQUE,
        [PolicyCode] NVARCHAR(100) NOT NULL UNIQUE,
        [Description] NVARCHAR(500),
        [Module] NVARCHAR(100),
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedAt] DATETIME2,
        [CreatedBy] INT,
        [ModifiedBy] INT,
        CONSTRAINT CK_Policy_Names CHECK (LEN([PolicyName]) > 0 AND LEN([PolicyCode]) > 0)
    );

    CREATE INDEX IX_PolicyMaster_Active ON [master].[PolicyMaster]([IsActive]);
    CREATE INDEX IX_PolicyMaster_Module ON [master].[PolicyMaster]([Module]);
    CREATE INDEX IX_PolicyMaster_Code ON [master].[PolicyMaster]([PolicyCode]);
    
    PRINT 'ü Created [master].[PolicyMaster] table';
END
ELSE
    PRINT 'ü [master].[PolicyMaster] already exists';

-- ============================================================================
-- STEP 3: CREATE JUNCTION TABLE - Role to Policy Mapping
-- ============================================================================

-- Create RolePolicy junction table (alternative to modifying RolePermissionConfig)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[config].[RolePolicyMapping]') AND type in (N'U'))
BEGIN
    CREATE TABLE [config].[RolePolicyMapping]
    (
        [RoleId] INT NOT NULL,
        [PolicyId] INT NOT NULL,
        [AssignedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [AssignedBy] INT,
        
        CONSTRAINT PK_RolePolicyMapping PRIMARY KEY ([RoleId], [PolicyId]),
        CONSTRAINT FK_RolePolicyMapping_Role FOREIGN KEY ([RoleId]) REFERENCES [master].[RoleMaster]([RoleId]) ON DELETE CASCADE,
        CONSTRAINT FK_RolePolicyMapping_Policy FOREIGN KEY ([PolicyId]) REFERENCES [master].[PolicyMaster]([PolicyId]) ON DELETE CASCADE
    );

    CREATE INDEX IX_RolePolicyMapping_Role ON [config].[RolePolicyMapping]([RoleId]);
    CREATE INDEX IX_RolePolicyMapping_Policy ON [config].[RolePolicyMapping]([PolicyId]);
    
    PRINT 'ü Created [config].[RolePolicyMapping] table';
END
ELSE
    PRINT 'ü [config].[RolePolicyMapping] already exists';

-- ============================================================================
-- STEP 4: INSERT DEFAULT POLICIES
-- ============================================================================

PRINT '';
PRINT '===== INSERTING DEFAULT POLICIES =====';

-- Lookup Management Policies
INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
SELECT 'CanViewLookups', 'VIEW_LOOKUPS', 'Can view lookup types and values', 'Lookups', 1
WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] WHERE [PolicyCode] = 'VIEW_LOOKUPS');

INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
SELECT 'CanManageLookups', 'MANAGE_LOOKUPS', 'Can create and edit lookup types and values', 'Lookups', 1
WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] WHERE [PolicyCode] = 'MANAGE_LOOKUPS');

INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
SELECT 'CanDeleteLookups', 'DELETE_LOOKUPS', 'Can delete lookup types and values', 'Lookups', 1
WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] WHERE [PolicyCode] = 'DELETE_LOOKUPS');

-- User Management Policies
INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
SELECT 'CanViewUsers', 'VIEW_USERS', 'Can view user list', 'Users', 1
WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] WHERE [PolicyCode] = 'VIEW_USERS');

INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
SELECT 'CanManageUsers', 'MANAGE_USERS', 'Can create and edit users', 'Users', 1
WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] WHERE [PolicyCode] = 'MANAGE_USERS');

INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
SELECT 'CanDeleteUsers', 'DELETE_USERS', 'Can delete users', 'Users', 1
WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] WHERE [PolicyCode] = 'DELETE_USERS');

-- Role Management
INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
SELECT 'ManageRoles', 'MANAGE_ROLES', 'Can manage user roles', 'Users', 1
WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] WHERE [PolicyCode] = 'MANAGE_ROLES');

-- Admin Policies
INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
SELECT 'AdminOnly', 'ADMIN_ONLY', 'Admin access only', 'System', 1
WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] WHERE [PolicyCode] = 'ADMIN_ONLY');

PRINT 'ü Default policies inserted (8 total)';

-- ============================================================================
-- STEP 5: ASSIGN DEFAULT ROLE-POLICY MAPPINGS
-- ============================================================================

PRINT '';
PRINT '===== ASSIGNING DEFAULT ROLE-POLICY MAPPINGS =====';

-- Get role IDs from your existing RoleMaster table
DECLARE @AdminRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Admin' OR [RoleName] = 'ADMIN');
DECLARE @ManagerRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Manager' OR [RoleName] = 'MANAGER');
DECLARE @UserRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'User' OR [RoleName] = 'USER');
DECLARE @ViewerRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Viewer' OR [RoleName] = 'VIEWER');

PRINT 'Role IDs found:';
PRINT 'Admin: ' + ISNULL(CAST(@AdminRoleId AS VARCHAR(10)), 'NOT FOUND');
PRINT 'Manager: ' + ISNULL(CAST(@ManagerRoleId AS VARCHAR(10)), 'NOT FOUND');
PRINT 'User: ' + ISNULL(CAST(@UserRoleId AS VARCHAR(10)), 'NOT FOUND');
PRINT 'Viewer: ' + ISNULL(CAST(@ViewerRoleId AS VARCHAR(10)), 'NOT FOUND');

-- Admin role gets all policies
IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
    SELECT @AdminRoleId, p.[PolicyId] FROM [master].[PolicyMaster] p
    WHERE NOT EXISTS (SELECT 1 FROM [config].[RolePolicyMapping] WHERE [RoleId] = @AdminRoleId AND [PolicyId] = p.[PolicyId]);
    
    PRINT 'ü Admin role: all policies assigned';
END

-- Manager role: can view and manage, but not delete
IF @ManagerRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
    SELECT @ManagerRoleId, p.[PolicyId] FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] IN ('VIEW_LOOKUPS', 'MANAGE_LOOKUPS', 'VIEW_USERS', 'MANAGE_USERS')
      AND NOT EXISTS (SELECT 1 FROM [config].[RolePolicyMapping] WHERE [RoleId] = @ManagerRoleId AND [PolicyId] = p.[PolicyId]);
    
    PRINT 'ü Manager role: view/manage policies assigned (no delete)';
END

-- User role: can only view
IF @UserRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
    SELECT @UserRoleId, p.[PolicyId] FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] IN ('VIEW_LOOKUPS', 'VIEW_USERS')
      AND NOT EXISTS (SELECT 1 FROM [config].[RolePolicyMapping] WHERE [RoleId] = @UserRoleId AND [PolicyId] = p.[PolicyId]);
    
    PRINT 'ü User role: view policies assigned';
END

-- Viewer role: limited view only
IF @ViewerRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
    SELECT @ViewerRoleId, p.[PolicyId] FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] IN ('VIEW_LOOKUPS')
      AND NOT EXISTS (SELECT 1 FROM [config].[RolePolicyMapping] WHERE [RoleId] = @ViewerRoleId AND [PolicyId] = p.[PolicyId]);
    
    PRINT 'ü Viewer role: limited view policy assigned';
END

-- ============================================================================
-- STEP 6: VERIFICATION QUERIES
-- ============================================================================

PRINT '';
PRINT '===== VERIFICATION QUERIES =====';
PRINT '';
PRINT '-- View all policies';
PRINT 'SELECT * FROM [master].[PolicyMaster] ORDER BY [PolicyId];';
PRINT '';
PRINT '-- View all role-policy mappings';
PRINT 'SELECT ';
PRINT '    r.[RoleName],';
PRINT '    p.[PolicyName],';
PRINT '    p.[Description],';
PRINT '    rpm.[AssignedAt]';
PRINT 'FROM [config].[RolePolicyMapping] rpm';
PRINT 'INNER JOIN [master].[RoleMaster] r ON rpm.[RoleId] = r.[RoleId]';
PRINT 'INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]';
PRINT 'ORDER BY r.[RoleId], p.[PolicyName];';
PRINT '';
PRINT '-- View policies for a specific role';
PRINT 'SELECT p.[PolicyName], p.[Description]';
PRINT 'FROM [config].[RolePolicyMapping] rpm';
PRINT 'INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]';
PRINT 'WHERE rpm.[RoleId] = 1 -- Replace with actual RoleId';
PRINT 'ORDER BY p.[PolicyName];';
PRINT '';
PRINT '-- View policies for a specific user';
PRINT 'SELECT DISTINCT p.[PolicyName], p.[Description]';
PRINT 'FROM [config].[UserRole] ur';
PRINT 'INNER JOIN [config].[RolePolicyMapping] rpm ON ur.[RoleId] = rpm.[RoleId]';
PRINT 'INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]';
PRINT 'WHERE ur.[UserId] = 1 -- Replace with actual UserId';
PRINT 'ORDER BY p.[PolicyName];';

-- ============================================================================
-- STEP 7: SUMMARY
-- ============================================================================

PRINT '';
PRINT '===== SUMMARY =====';
PRINT 'ü Tables created: [master].[PolicyMaster], [config].[RolePolicyMapping]';
PRINT 'ü Integrated with existing schema (master, config schemas)';
PRINT 'ü Default policies inserted: 8 policies';
PRINT 'ü Default role-policy assignments created';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Run verification queries above to confirm setup';
PRINT '2. Update IPolicyService in .NET code to use [master].[PolicyMaster]';
PRINT '3. Update PolicyManagementController to work with existing schema';
PRINT '4. Test policy assignments via API';
PRINT '';
PRINT '===== DATABASE SCHEMA INTEGRATION COMPLETE =====';
