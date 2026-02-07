-- ============================================================================
-- ROLE-BASED AUTHORIZATION SYSTEM - SQL SCRIPT
-- Database: EnterpriseHIS
-- Purpose: Create tables and stored procedures for role and permission management
-- ============================================================================

-- ============================================================================
-- PART 1: CREATE SCHEMAS (if not exists)
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'master')
    EXEC('CREATE SCHEMA master');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'config')
    EXEC('CREATE SCHEMA config');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'core')
    EXEC('CREATE SCHEMA core');
GO

-- ============================================================================
-- PART 2: DROP EXISTING TABLES (for fresh install)
-- ============================================================================

-- Drop in reverse order of dependencies
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRole' AND schema_id = SCHEMA_ID('config'))
    DROP TABLE [config].[UserRole];
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissionConfig' AND schema_id = SCHEMA_ID('config'))
    DROP TABLE [config].[RolePermissionConfig];
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PermissionMaster' AND schema_id = SCHEMA_ID('master'))
    DROP TABLE [master].[PermissionMaster];
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoleMaster' AND schema_id = SCHEMA_ID('master'))
    DROP TABLE [master].[RoleMaster];
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAccount' AND schema_id = SCHEMA_ID('core'))
    DROP TABLE [core].[UserAccount];
GO

-- ============================================================================
-- PART 3: CREATE TABLES
-- ============================================================================

-- Table: RoleMaster
-- Purpose: Store application roles
CREATE TABLE [master].[RoleMaster] (
    [RoleId] INT PRIMARY KEY IDENTITY(1,1),
    [RoleName] NVARCHAR(100) NOT NULL UNIQUE,
    [Description] NVARCHAR(500),
    [IsActive] BIT DEFAULT 1,
    [CreatedDate] DATETIME DEFAULT GETUTCDATE(),
    [ModifiedDate] DATETIME
);

CREATE INDEX [IX_RoleMaster_RoleName] ON [master].[RoleMaster]([RoleName]);
CREATE INDEX [IX_RoleMaster_IsActive] ON [master].[RoleMaster]([IsActive]);
GO

-- Table: PermissionMaster
-- Purpose: Store application permissions
CREATE TABLE [master].[PermissionMaster] (
    [PermissionId] INT PRIMARY KEY IDENTITY(1,1),
    [PermissionName] NVARCHAR(100) NOT NULL UNIQUE,      -- e.g., "CanViewLookups"
    [PermissionCode] NVARCHAR(50) NOT NULL UNIQUE,       -- e.g., "VIEW_LOOKUPS"
    [Description] NVARCHAR(500),
    [IsActive] BIT DEFAULT 1,
    [CreatedDate] DATETIME DEFAULT GETUTCDATE(),
    [ModifiedDate] DATETIME
);

CREATE INDEX [IX_PermissionMaster_PermissionCode] ON [master].[PermissionMaster]([PermissionCode]);
CREATE INDEX [IX_PermissionMaster_IsActive] ON [master].[PermissionMaster]([IsActive]);
GO

-- Table: RolePermissionConfig
-- Purpose: Map roles to permissions (many-to-many)
CREATE TABLE [config].[RolePermissionConfig] (
    [RolePermissionId] INT PRIMARY KEY IDENTITY(1,1),
    [RoleId] INT NOT NULL FOREIGN KEY REFERENCES [master].[RoleMaster]([RoleId]) ON DELETE CASCADE,
    [PermissionId] INT NOT NULL FOREIGN KEY REFERENCES [master].[PermissionMaster]([PermissionId]) ON DELETE CASCADE,
    [IsActive] BIT DEFAULT 1,
    [AssignedDate] DATETIME DEFAULT GETUTCDATE(),
    [ModifiedDate] DATETIME,
    UNIQUE ([RoleId], [PermissionId])
);

CREATE INDEX [IX_RolePermissionConfig_RoleId] ON [config].[RolePermissionConfig]([RoleId]);
CREATE INDEX [IX_RolePermissionConfig_PermissionId] ON [config].[RolePermissionConfig]([PermissionId]);
CREATE INDEX [IX_RolePermissionConfig_IsActive] ON [config].[RolePermissionConfig]([IsActive]);
GO

-- Table: UserAccount
-- Purpose: Store user information
-- Note: Using 'UserAccount' instead of 'User' because 'User' is a reserved keyword in SQL Server
CREATE TABLE [core].[UserAccount] (
    [UserId] INT PRIMARY KEY IDENTITY(1,1),
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [Email] NVARCHAR(100) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX),
    [FirstName] NVARCHAR(50),
    [LastName] NVARCHAR(50),
    [IsActive] BIT DEFAULT 1,
    [CreatedDate] DATETIME DEFAULT GETUTCDATE(),
    [ModifiedDate] DATETIME,
    [LastLoginDate] DATETIME
);

CREATE INDEX [IX_UserAccount_Username] ON [core].[UserAccount]([Username]);
CREATE INDEX [IX_UserAccount_Email] ON [core].[UserAccount]([Email]);
CREATE INDEX [IX_UserAccount_IsActive] ON [core].[UserAccount]([IsActive]);
GO

-- Table: UserRole
-- Purpose: Map users to roles (many-to-many)
CREATE TABLE [config].[UserRole] (
    [UserRoleId] INT PRIMARY KEY IDENTITY(1,1),
    [UserId] INT NOT NULL FOREIGN KEY REFERENCES [core].[UserAccount]([UserId]) ON DELETE CASCADE,
    [RoleId] INT NOT NULL FOREIGN KEY REFERENCES [master].[RoleMaster]([RoleId]) ON DELETE CASCADE,
    [IsActive] BIT DEFAULT 1,
    [AssignedDate] DATETIME DEFAULT GETUTCDATE(),
    [ModifiedDate] DATETIME,
    UNIQUE ([UserId], [RoleId])
);

CREATE INDEX [IX_UserRole_UserId] ON [config].[UserRole]([UserId]);
CREATE INDEX [IX_UserRole_RoleId] ON [config].[UserRole]([RoleId]);
CREATE INDEX [IX_UserRole_IsActive] ON [config].[UserRole]([IsActive]);
GO

-- ============================================================================
-- PART 4: INSERT SEED DATA
-- ============================================================================

-- Insert Roles
INSERT INTO [master].[RoleMaster] ([RoleName], [Description], [IsActive], [CreatedDate])
VALUES
    ('Admin', 'System Administrator - Full access', 1, GETUTCDATE()),
    ('Manager', 'Manager Role - Can manage lookups', 1, GETUTCDATE()),
    ('User', 'Regular User - View only access', 1, GETUTCDATE()),
    ('Viewer', 'Viewer Role - Limited view access', 1, GETUTCDATE());
GO

-- Insert Permissions
INSERT INTO [master].[PermissionMaster] ([PermissionName], [PermissionCode], [Description], [IsActive], [CreatedDate])
VALUES
    ('View Lookups', 'VIEW_LOOKUPS', 'View lookup data', 1, GETUTCDATE()),
    ('Create Lookups', 'CREATE_LOOKUPS', 'Create new lookups', 1, GETUTCDATE()),
    ('Edit Lookups', 'EDIT_LOOKUPS', 'Edit existing lookups', 1, GETUTCDATE()),
    ('Delete Lookups', 'DELETE_LOOKUPS', 'Delete lookups', 1, GETUTCDATE()),
    ('View Users', 'VIEW_USERS', 'View user information', 1, GETUTCDATE()),
    ('Create Users', 'CREATE_USERS', 'Create new users', 1, GETUTCDATE()),
    ('Edit Users', 'EDIT_USERS', 'Edit user information', 1, GETUTCDATE()),
    ('Delete Users', 'DELETE_USERS', 'Delete users', 1, GETUTCDATE()),
    ('Manage Roles', 'MANAGE_ROLES', 'Manage roles and permissions', 1, GETUTCDATE());
GO

-- Assign all permissions to Admin role
INSERT INTO [config].[RolePermissionConfig] ([RoleId], [PermissionId], [IsActive], [AssignedDate])
SELECT 
    (SELECT [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Admin'),
    [PermissionId],
    1,
    GETUTCDATE()
FROM [master].[PermissionMaster]
WHERE [IsActive] = 1;
GO

-- Assign permissions to Manager role (View, Create, Edit Lookups and Users)
INSERT INTO [config].[RolePermissionConfig] ([RoleId], [PermissionId], [IsActive], [AssignedDate])
SELECT 
    (SELECT [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Manager'),
    [PermissionId],
    1,
    GETUTCDATE()
FROM [master].[PermissionMaster]
WHERE [PermissionCode] IN ('VIEW_LOOKUPS', 'CREATE_LOOKUPS', 'EDIT_LOOKUPS', 'VIEW_USERS', 'CREATE_USERS', 'EDIT_USERS')
AND [IsActive] = 1;
GO

-- Assign permissions to User role (View Lookups and Users only)
INSERT INTO [config].[RolePermissionConfig] ([RoleId], [PermissionId], [IsActive], [AssignedDate])
SELECT 
    (SELECT [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'User'),
    [PermissionId],
    1,
    GETUTCDATE()
FROM [master].[PermissionMaster]
WHERE [PermissionCode] IN ('VIEW_LOOKUPS', 'VIEW_USERS')
AND [IsActive] = 1;
GO

-- Assign permissions to Viewer role (View Lookups only)
INSERT INTO [config].[RolePermissionConfig] ([RoleId], [PermissionId], [IsActive], [AssignedDate])
SELECT 
    (SELECT [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Viewer'),
    [PermissionId],
    1,
    GETUTCDATE()
FROM [master].[PermissionMaster]
WHERE [PermissionCode] IN ('VIEW_LOOKUPS')
AND [IsActive] = 1;
GO

-- Create Users
INSERT INTO [core].[UserAccount] ([Username], [Email], [FirstName], [LastName], [IsActive], [CreatedDate])
VALUES
    ('admin', 'admin@enterprise-his.com', 'Admin', 'User', 1, GETUTCDATE()),
    ('manager', 'manager@enterprise-his.com', 'Manager', 'User', 1, GETUTCDATE()),
    ('user', 'user@enterprise-his.com', 'Regular', 'User', 1, GETUTCDATE()),
    ('viewer', 'viewer@enterprise-his.com', 'Viewer', 'User', 1, GETUTCDATE());
GO

-- Assign roles to users
INSERT INTO [config].[UserRole] ([UserId], [RoleId], [IsActive], [AssignedDate])
SELECT 
    (SELECT [UserId] FROM [core].[UserAccount] WHERE [Username] = 'admin'),
    (SELECT [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Admin'),
    1,
    GETUTCDATE()
UNION ALL
SELECT 
    (SELECT [UserId] FROM [core].[UserAccount] WHERE [Username] = 'manager'),
    (SELECT [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Manager'),
    1,
    GETUTCDATE()
UNION ALL
SELECT 
    (SELECT [UserId] FROM [core].[UserAccount] WHERE [Username] = 'user'),
    (SELECT [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'User'),
    1,
    GETUTCDATE()
UNION ALL
SELECT 
    (SELECT [UserId] FROM [core].[UserAccount] WHERE [Username] = 'viewer'),
    (SELECT [RoleId] FROM [master].[RoleMaster] WHERE [RoleName] = 'Viewer'),
    1,
    GETUTCDATE();
GO

-- ============================================================================
-- PART 5: CREATE STORED PROCEDURES
-- ============================================================================

-- SP: Get User Roles
-- Purpose: Get all roles for a specific user
CREATE OR ALTER PROCEDURE [config].[SP_GetUserRoles]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT 
        r.[RoleId],
        r.[RoleName],
        r.[Description]
    FROM [config].[UserRole] ur
    INNER JOIN [master].[RoleMaster] r ON ur.[RoleId] = r.[RoleId]
    WHERE ur.[UserId] = @UserId
    AND ur.[IsActive] = 1
    AND r.[IsActive] = 1
    ORDER BY r.[RoleName];
END;
GO

-- SP: Get Role Permissions
-- Purpose: Get all permissions for a specific role
CREATE OR ALTER PROCEDURE [config].[SP_GetRolePermissions]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT 
        p.[PermissionId],
        p.[PermissionName],
        p.[PermissionCode],
        p.[Description]
    FROM [config].[RolePermissionConfig] rp
    INNER JOIN [master].[PermissionMaster] p ON rp.[PermissionId] = p.[PermissionId]
    WHERE rp.[RoleId] = @RoleId
    AND rp.[IsActive] = 1
    AND p.[IsActive] = 1
    ORDER BY p.[PermissionName];
END;
GO

-- SP: Get User Permissions
-- Purpose: Get all permissions for a specific user (through all their roles)
CREATE OR ALTER PROCEDURE [config].[SP_GetUserPermissions]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT 
        p.[PermissionId],
        p.[PermissionName],
        p.[PermissionCode],
        p.[Description]
    FROM [config].[UserRole] ur
    INNER JOIN [config].[RolePermissionConfig] rp ON ur.[RoleId] = rp.[RoleId]
    INNER JOIN [master].[PermissionMaster] p ON rp.[PermissionId] = p.[PermissionId]
    WHERE ur.[UserId] = @UserId
    AND ur.[IsActive] = 1
    AND rp.[IsActive] = 1
    AND p.[IsActive] = 1
    ORDER BY p.[PermissionName];
END;
GO

-- SP: Check User Permission
-- Purpose: Check if user has specific permission (returns 1 if yes, 0 if no)
CREATE OR ALTER PROCEDURE [config].[SP_CheckUserPermission]
    @UserId INT,
    @PermissionCode NVARCHAR(50),
    @HasPermission BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @HasPermission = CASE 
        WHEN EXISTS (
            SELECT 1
            FROM [config].[UserRole] ur
            INNER JOIN [config].[RolePermissionConfig] rp ON ur.[RoleId] = rp.[RoleId]
            INNER JOIN [master].[PermissionMaster] p ON rp.[PermissionId] = p.[PermissionId]
            WHERE ur.[UserId] = @UserId
            AND p.[PermissionCode] = @PermissionCode
            AND ur.[IsActive] = 1
            AND rp.[IsActive] = 1
            AND p.[IsActive] = 1
        ) THEN 1
        ELSE 0
    END;
END;
GO

-- SP: Check Role Permission
-- Purpose: Check if role has specific permission (returns 1 if yes, 0 if no)
CREATE OR ALTER PROCEDURE [config].[SP_CheckRolePermission]
    @RoleId INT,
    @PermissionCode NVARCHAR(50),
    @HasPermission BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @HasPermission = CASE 
        WHEN EXISTS (
            SELECT 1
            FROM [config].[RolePermissionConfig] rp
            INNER JOIN [master].[PermissionMaster] p ON rp.[PermissionId] = p.[PermissionId]
            WHERE rp.[RoleId] = @RoleId
            AND p.[PermissionCode] = @PermissionCode
            AND rp.[IsActive] = 1
            AND p.[IsActive] = 1
        ) THEN 1
        ELSE 0
    END;
END;
GO

-- SP: Get All Users
-- Purpose: Get all active users with their roles
CREATE OR ALTER PROCEDURE [config].[SP_GetAllUsers]
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SkipRows INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        u.[UserId],
        u.[Username],
        u.[Email],
        u.[FirstName],
        u.[LastName],
        u.[IsActive],
        u.[CreatedDate],
        STRING_AGG(r.[RoleName], ', ') AS [Roles]
    FROM [core].[UserAccount] u
    LEFT JOIN [config].[UserRole] ur ON u.[UserId] = ur.[UserId] AND ur.[IsActive] = 1
    LEFT JOIN [master].[RoleMaster] r ON ur.[RoleId] = r.[RoleId] AND r.[IsActive] = 1
    WHERE u.[IsActive] = 1
    GROUP BY 
        u.[UserId],
        u.[Username],
        u.[Email],
        u.[FirstName],
        u.[LastName],
        u.[IsActive],
        u.[CreatedDate]
    ORDER BY u.[Username]
    OFFSET @SkipRows ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- SP: Assign Role to User
-- Purpose: Add a role to a user
CREATE OR ALTER PROCEDURE [config].[SP_AssignRoleToUser]
    @UserId INT,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (
        SELECT 1 FROM [config].[UserRole] 
        WHERE [UserId] = @UserId AND [RoleId] = @RoleId
    )
    BEGIN
        INSERT INTO [config].[UserRole] ([UserId], [RoleId], [IsActive], [AssignedDate])
        VALUES (@UserId, @RoleId, 1, GETUTCDATE());
        
        SELECT 1 AS [Result], 'Role assigned successfully' AS [Message];
    END
    ELSE
    BEGIN
        SELECT 0 AS [Result], 'User already has this role' AS [Message];
    END
END;
GO

-- SP: Remove Role from User
-- Purpose: Remove a role from a user
CREATE OR ALTER PROCEDURE [config].[SP_RemoveRoleFromUser]
    @UserId INT,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [config].[UserRole]
    WHERE [UserId] = @UserId AND [RoleId] = @RoleId;
    
    SELECT @@ROWCOUNT AS [RowsAffected];
END;
GO

-- ============================================================================
-- PART 6: VERIFICATION QUERIES
-- ============================================================================

-- Verify tables created
PRINT '=== TABLES CREATED ===';
SELECT [name] FROM sys.tables WHERE schema_id IN (
    SCHEMA_ID('master'), 
    SCHEMA_ID('config'), 
    SCHEMA_ID('core')
)
ORDER BY [name];

-- Verify data inserted
PRINT '=== ROLES ===';
SELECT * FROM [master].[RoleMaster];

PRINT '=== PERMISSIONS ===';
SELECT * FROM [master].[PermissionMaster];

PRINT '=== USERS ===';
SELECT * FROM [core].[UserAccount];

PRINT '=== USER ROLES ===';
SELECT * FROM [config].[UserRole];

PRINT '=== ROLE PERMISSIONS ===';
SELECT * FROM [config].[RolePermissionConfig];

-- Test SP: Get User Permissions for Admin user
PRINT '=== ADMIN USER PERMISSIONS ===';
EXEC [config].[SP_GetUserPermissions] @UserId = 1;

-- Test SP: Get User Permissions for Regular User
PRINT '=== REGULAR USER PERMISSIONS ===';
EXEC [config].[SP_GetUserPermissions] @UserId = 3;
