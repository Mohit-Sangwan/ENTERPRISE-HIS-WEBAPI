-- ============================================================================
-- DATABASE SCHEMA FOR ENTERPRISE-LEVEL POLICY MANAGEMENT
-- INTEGRATES WITH EXISTING SCHEMA:
-- - master.RoleMaster (existing roles)
-- - master.PermissionMaster (existing permissions)
-- - config.RolePermissionConfig (existing role-permission mappings)
-- - core.UserAccount (existing users)
-- - config.UserRole (existing user-role assignments)
-- ============================================================================

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
        CONSTRAINT CK_PolicyMaster_Names CHECK (LEN([PolicyName]) > 0 AND LEN([PolicyCode]) > 0),
        CONSTRAINT FK_PolicyMaster_CreatedBy FOREIGN KEY ([CreatedBy]) REFERENCES [core].[UserAccount]([UserId]),
        CONSTRAINT FK_PolicyMaster_ModifiedBy FOREIGN KEY ([ModifiedBy]) REFERENCES [core].[UserAccount]([UserId])
    );

    CREATE INDEX IX_PolicyMaster_Active ON [master].[PolicyMaster]([IsActive]);
    CREATE INDEX IX_PolicyMaster_Module ON [master].[PolicyMaster]([Module]);
    CREATE INDEX IX_PolicyMaster_Code ON [master].[PolicyMaster]([PolicyCode]);
    
    PRINT 'Created [master].[PolicyMaster] table';
END
ELSE
    PRINT '[master].[PolicyMaster] already exists';

-- Create RolePolicy junction table for role-policy assignments
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
        CONSTRAINT FK_RolePolicyMapping_Policy FOREIGN KEY ([PolicyId]) REFERENCES [master].[PolicyMaster]([PolicyId]) ON DELETE CASCADE,
        CONSTRAINT FK_RolePolicyMapping_AssignedBy FOREIGN KEY ([AssignedBy]) REFERENCES [core].[UserAccount]([UserId])
    );

    CREATE INDEX IX_RolePolicyMapping_Role ON [config].[RolePolicyMapping]([RoleId]);
    CREATE INDEX IX_RolePolicyMapping_Policy ON [config].[RolePolicyMapping]([PolicyId]);
    CREATE INDEX IX_RolePolicyMapping_AssignedAt ON [config].[RolePolicyMapping]([AssignedAt]);
    
    PRINT 'Created [config].[RolePolicyMapping] table';
END
ELSE
    PRINT '[config].[RolePolicyMapping] already exists';

-- ============================================================================
-- INSERT DEFAULT POLICIES
-- ============================================================================

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

-- ============================================================================
-- ASSIGN DEFAULT ROLE-POLICY MAPPINGS
-- Finds roles automatically from your existing master.RoleMaster
-- ============================================================================

DECLARE @AdminRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] LIKE '%Admin%');
DECLARE @ManagerRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] LIKE '%Manager%');
DECLARE @UserRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] LIKE '%User%');
DECLARE @ViewerRoleId INT = (SELECT TOP 1 [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] LIKE '%Viewer%');

-- Admin role gets all policies
IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
    SELECT @AdminRoleId, p.[PolicyId] FROM [master].[PolicyMaster] p
    WHERE NOT EXISTS (SELECT 1 FROM [config].[RolePolicyMapping] WHERE [RoleId] = @AdminRoleId AND [PolicyId] = p.[PolicyId]);
END

-- Manager role: can view and manage, but not delete
IF @ManagerRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
    SELECT @ManagerRoleId, p.[PolicyId] FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] IN ('VIEW_LOOKUPS', 'MANAGE_LOOKUPS', 'VIEW_USERS', 'MANAGE_USERS')
      AND NOT EXISTS (SELECT 1 FROM [config].[RolePolicyMapping] WHERE [RoleId] = @ManagerRoleId AND [PolicyId] = p.[PolicyId]);
END

-- User role: can only view
IF @UserRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
    SELECT @UserRoleId, p.[PolicyId] FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] IN ('VIEW_LOOKUPS', 'VIEW_USERS')
      AND NOT EXISTS (SELECT 1 FROM [config].[RolePolicyMapping] WHERE [RoleId] = @UserRoleId AND [PolicyId] = p.[PolicyId]);
END

-- Viewer role: limited view only
IF @ViewerRoleId IS NOT NULL
BEGIN
    INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
    SELECT @ViewerRoleId, p.[PolicyId] FROM [master].[PolicyMaster] p
    WHERE p.[PolicyCode] IN ('VIEW_LOOKUPS')
      AND NOT EXISTS (SELECT 1 FROM [config].[RolePolicyMapping] WHERE [RoleId] = @ViewerRoleId AND [PolicyId] = p.[PolicyId]);
END

-- ============================================================================
-- VERIFICATION QUERIES
-- Run these to verify the schema and data
-- ============================================================================

/*
-- View all policies
SELECT * FROM [master].[PolicyMaster] ORDER BY [PolicyId];

-- View all role-policy assignments
SELECT 
    r.[RoleName],
    p.[PolicyName],
    p.[Description],
    rpm.[AssignedAt]
FROM [config].[RolePolicyMapping] rpm
INNER JOIN [master].[RoleMaster] r ON rpm.[RoleId] = r.[RoleId]
INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]
ORDER BY r.[RoleId], p.[PolicyName];

-- View policies for a specific role (Admin)
SELECT p.[PolicyName], p.[Description]
FROM [config].[RolePolicyMapping] rpm
INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]
INNER JOIN [master].[RoleMaster] r ON rpm.[RoleId] = r.[RoleId]
WHERE r.[RoleName] LIKE '%Admin%'
ORDER BY p.[PolicyName];

-- View policies for a specific user
SELECT DISTINCT p.[PolicyName], p.[Description]
FROM [config].[UserRole] ur
INNER JOIN [config].[RolePolicyMapping] rpm ON ur.[RoleId] = rpm.[RoleId]
INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]
WHERE ur.[UserId] = 1  -- Replace 1 with actual user ID
ORDER BY p.[PolicyName];

-- View role statistics
SELECT 
    r.[RoleName],
    COUNT(rpm.[PolicyId]) AS [PolicyCount]
FROM [master].[RoleMaster] r
LEFT JOIN [config].[RolePolicyMapping] rpm ON r.[RoleId] = rpm.[RoleId]
GROUP BY r.[RoleId], r.[RoleName]
ORDER BY r.[RoleName];
*/

PRINT '===== DATABASE SCHEMA FOR POLICIES CREATED SUCCESSFULLY =====';
PRINT 'Tables created: [master].[PolicyMaster], [config].[RolePolicyMapping]';
PRINT 'Schema integration: Linked with existing master.RoleMaster and core.UserAccount';
PRINT 'Default policies inserted: 8 policies';
PRINT 'Default role-policy assignments created (automatic role detection)';
