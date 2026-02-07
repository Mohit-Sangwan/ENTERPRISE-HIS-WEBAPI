-- =====================================================
-- ENTERPRISE HIS - ROLES & PERMISSIONS DATABASE SCHEMA
-- Database: EnterpriseHIS
-- Schema: Creates tables with proper naming conventions
-- =====================================================

-- Drop existing tables if they exist (for clean slate)
IF OBJECT_ID('config.UserRoleConfig', 'U') IS NOT NULL DROP TABLE config.UserRoleConfig;
IF OBJECT_ID('config.RolePermissionConfig', 'U') IS NOT NULL DROP TABLE config.RolePermissionConfig;
IF OBJECT_ID('core.UserAccount', 'U') IS NOT NULL DROP TABLE core.UserAccount;
IF OBJECT_ID('master.PermissionMaster', 'U') IS NOT NULL DROP TABLE master.PermissionMaster;
IF OBJECT_ID('master.RoleMaster', 'U') IS NOT NULL DROP TABLE master.RoleMaster;

-- ===== MASTER SCHEMA TABLES =====

-- Roles table
CREATE TABLE master.RoleMaster (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME
);

-- Permissions table
CREATE TABLE master.PermissionMaster (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,      -- e.g., "CanViewLookups"
    PermissionCode NVARCHAR(50) NOT NULL UNIQUE,       -- e.g., "VIEW_LOOKUPS"
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME
);

-- ===== CORE SCHEMA TABLES =====

-- Users table (renamed from User to avoid reserved keyword)
CREATE TABLE core.UserAccount (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    LastLoginDate DATETIME,
    ModifiedDate DATETIME
);

-- ===== CONFIG SCHEMA TABLES (Many-to-Many Mappings) =====

-- User-Role mapping (many-to-many)
CREATE TABLE config.UserRoleConfig (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES core.UserAccount(UserId),
    RoleId INT NOT NULL FOREIGN KEY REFERENCES master.RoleMaster(RoleId),
    IsActive BIT DEFAULT 1,
    AssignedDate DATETIME DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME,
    UNIQUE(UserId, RoleId) -- Prevent duplicate assignments
);

-- Role-Permission mapping (many-to-many)
CREATE TABLE config.RolePermissionConfig (
    RolePermissionId INT PRIMARY KEY IDENTITY(1,1),
    RoleId INT NOT NULL FOREIGN KEY REFERENCES master.RoleMaster(RoleId),
    PermissionId INT NOT NULL FOREIGN KEY REFERENCES master.PermissionMaster(PermissionId),
    IsActive BIT DEFAULT 1,
    AssignedDate DATETIME DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME,
    UNIQUE(RoleId, PermissionId) -- Prevent duplicate assignments
);

-- ===== CREATE INDEXES FOR PERFORMANCE =====

CREATE INDEX IDX_UserAccount_Username ON core.UserAccount(Username);
CREATE INDEX IDX_UserAccount_Email ON core.UserAccount(Email);
CREATE INDEX IDX_UserAccount_IsActive ON core.UserAccount(IsActive);

CREATE INDEX IDX_RoleMaster_RoleName ON master.RoleMaster(RoleName);
CREATE INDEX IDX_RoleMaster_IsActive ON master.RoleMaster(IsActive);

CREATE INDEX IDX_PermissionMaster_PermissionCode ON master.PermissionMaster(PermissionCode);
CREATE INDEX IDX_PermissionMaster_IsActive ON master.PermissionMaster(IsActive);

CREATE INDEX IDX_UserRoleConfig_UserId ON config.UserRoleConfig(UserId);
CREATE INDEX IDX_UserRoleConfig_RoleId ON config.UserRoleConfig(RoleId);
CREATE INDEX IDX_UserRoleConfig_IsActive ON config.UserRoleConfig(IsActive);

CREATE INDEX IDX_RolePermissionConfig_RoleId ON config.RolePermissionConfig(RoleId);
CREATE INDEX IDX_RolePermissionConfig_PermissionId ON config.RolePermissionConfig(PermissionId);
CREATE INDEX IDX_RolePermissionConfig_IsActive ON config.RolePermissionConfig(IsActive);
