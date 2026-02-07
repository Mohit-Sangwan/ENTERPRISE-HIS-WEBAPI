-- ============================================================
-- ENTERPRISE 2FA CONFIGURATION - DATABASE-DRIVEN SETTINGS
-- ============================================================
-- Store all 2FA configuration in database (not appsettings.json)
-- Enterprise-level configuration management
-- .NET 8 | SQL Server 2019+
-- ============================================================

-- ================================================
-- 1. 2FA CONFIGURATION TABLE (Global Settings)
-- ============================================================
CREATE TABLE config.TwoFactorConfiguration (
    ConfigId INT PRIMARY KEY IDENTITY(1,1),
    ConfigKey NVARCHAR(100) NOT NULL UNIQUE,     -- e.g., 'OTPExpiryMinutes', 'MaxOTPAttempts'
    ConfigValue NVARCHAR(500) NOT NULL,           -- Value (stored as string, converted by service)
    ConfigType NVARCHAR(50),                       -- 'Integer', 'Boolean', 'String', 'Decimal'
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedBy INT,                                 -- Admin user who updated
    
    INDEX IX_ConfigKey (ConfigKey),
    INDEX IX_IsActive (IsActive)
);

-- ============================================================
-- 2. SMS PROVIDER CONFIGURATION (Global Settings)
-- ============================================================
CREATE TABLE config.SMSProviderConfigurationExtended (
    ProviderConfigId INT PRIMARY KEY IDENTITY(1,1),
    ProviderId INT NOT NULL FOREIGN KEY REFERENCES config.SMSProviderConfig(ProviderId),
    
    -- Provider-specific settings
    ConfigKey NVARCHAR(100) NOT NULL,              -- e.g., 'MaxRetries', 'Timeout', 'RateLimitPerMinute'
    ConfigValue NVARCHAR(500) NOT NULL,
    ConfigType NVARCHAR(50),                       -- 'Integer', 'String', 'Boolean'
    
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    UNIQUE(ProviderId, ConfigKey),
    INDEX IX_ProviderId (ProviderId),
    INDEX IX_ConfigKey (ConfigKey)
);

-- ============================================================
-- 3. 2FA POLICY TABLE (Organizational Policies)
-- ============================================================
CREATE TABLE config.TwoFactorPolicy (
    PolicyId INT PRIMARY KEY IDENTITY(1,1),
    PolicyName NVARCHAR(100) NOT NULL UNIQUE,     -- e.g., 'StrictPolicy', 'BasicPolicy'
    PolicyCode NVARCHAR(50) NOT NULL UNIQUE,      -- e.g., 'STRICT_2FA', 'BASIC_2FA'
    Description NVARCHAR(500),
    
    -- Policy settings (override global config)
    OTPExpiryMinutes INT DEFAULT 10,
    OTPLength INT DEFAULT 6,
    MaxOTPAttempts INT DEFAULT 5,
    SessionExpiryMinutes INT DEFAULT 15,
    BackupCodeCount INT DEFAULT 10,
    TrustedDeviceExpiryDays INT DEFAULT 90,
    
    -- Rate limiting
    RateLimitEnabled BIT DEFAULT 1,
    MaxOTPPerHour INT DEFAULT 10,
    MaxOTPPerDay INT DEFAULT 50,
    
    -- Enforcement
    IsRequired BIT DEFAULT 0,                      -- Force 2FA for this policy
    IsActive BIT DEFAULT 1,
    
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    INDEX IX_PolicyCode (PolicyCode),
    INDEX IX_IsActive (IsActive)
);

-- ============================================================
-- 4. ROLE-TO-2FA-POLICY MAPPING
-- ============================================================
CREATE TABLE config.RoleTwoFactorPolicyMapping (
    MappingId INT PRIMARY KEY IDENTITY(1,1),
    RoleId INT NOT NULL,                           -- From your existing Role table
    PolicyId INT NOT NULL FOREIGN KEY REFERENCES config.TwoFactorPolicy(PolicyId),
    
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    UNIQUE(RoleId, PolicyId),
    INDEX IX_RoleId (RoleId),
    INDEX IX_PolicyId (PolicyId)
);

-- ============================================================
-- 5. USER-SPECIFIC 2FA OVERRIDES
-- ============================================================
CREATE TABLE config.User2FAOverride (
    OverrideId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    
    -- Override settings
    IsTwoFactorRequired BIT,                       -- NULL = use role policy, 1 = required, 0 = optional
    PreferredMethod NVARCHAR(50),                  -- SMS_OTP, EMAIL_OTP, TOTP
    AllowedMethods NVARCHAR(500),                  -- Comma-separated methods
    
    MaxOTPAttempts INT,                            -- Override default
    OTPExpiryMinutes INT,                          -- Override default
    AllowBackupCodes BIT DEFAULT 1,
    AllowDeviceTrust BIT DEFAULT 1,
    
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    INDEX IX_UserId (UserId),
    INDEX IX_IsActive (IsActive)
);

-- ============================================================
-- 6. DEFAULT 2FA CONFIGURATION (Insert defaults)
-- ============================================================

INSERT INTO config.TwoFactorConfiguration (ConfigKey, ConfigValue, ConfigType, Description)
VALUES 
    ('Enabled', 'true', 'Boolean', 'Enable/disable 2FA globally'),
    ('Required', 'false', 'Boolean', 'Require 2FA for all users'),
    ('OTPExpiryMinutes', '10', 'Integer', 'OTP expiry time in minutes'),
    ('OTPLength', '6', 'Integer', '6-digit or 8-digit OTP'),
    ('MaxOTPAttempts', '5', 'Integer', 'Maximum OTP verification attempts'),
    ('SessionExpiryMinutes', '15', 'Integer', '2FA session expiry in minutes'),
    ('BackupCodeCount', '10', 'Integer', 'Number of backup codes per user'),
    ('TrustedDeviceExpiryDays', '90', 'Integer', 'Trusted device expiry in days'),
    ('RateLimitingEnabled', 'true', 'Boolean', 'Enable rate limiting'),
    ('MaxOTPPerHour', '10', 'Integer', 'Max OTP sends per hour per user'),
    ('MaxOTPPerDay', '50', 'Integer', 'Max OTP sends per day per user'),
    ('SMSPrimaryProvider', 'TWILIO', 'String', 'Primary SMS provider'),
    ('EmailEnabled', 'true', 'Boolean', 'Enable email OTP'),
    ('TOTPEnabled', 'true', 'Boolean', 'Enable Google Authenticator'),
    ('BackupCodesEnabled', 'true', 'Boolean', 'Enable backup codes'),
    ('DeviceTrustEnabled', 'true', 'Boolean', 'Enable device trust'),
    ('AuditLoggingEnabled', 'true', 'Boolean', 'Enable audit logging');

-- ============================================================
-- 7. DEFAULT 2FA POLICIES
-- ============================================================

INSERT INTO config.TwoFactorPolicy (PolicyName, PolicyCode, Description, IsRequired, IsActive)
VALUES 
    ('Strict 2FA', 'STRICT_2FA', 'Enforce 2FA with minimum settings', 1, 1),
    ('Standard 2FA', 'STANDARD_2FA', 'Standard 2FA with moderate restrictions', 0, 1),
    ('Lenient 2FA', 'LENIENT_2FA', 'Optional 2FA with flexible settings', 0, 1),
    ('Admin 2FA', 'ADMIN_2FA', 'Admin-only 2FA with strict requirements', 1, 1);

-- ============================================================
-- 8. VERIFICATION QUERIES
-- ============================================================

SELECT 'Global 2FA Configuration' AS [Table], COUNT(*) as [Record Count]
FROM config.TwoFactorConfiguration
UNION ALL
SELECT '2FA Policies', COUNT(*)
FROM config.TwoFactorPolicy
UNION ALL
SELECT 'SMS Provider Config Extended', COUNT(*)
FROM config.SMSProviderConfigurationExtended
UNION ALL
SELECT 'User 2FA Overrides', COUNT(*)
FROM config.User2FAOverride;

PRINT '✅ Enterprise 2FA Configuration tables created!';
PRINT '✅ All configuration now database-driven!';
PRINT '✅ Ready for enterprise-level management!';
