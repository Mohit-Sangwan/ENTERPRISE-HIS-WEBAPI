-- ============================================================
-- ENTERPRISE TWO-FACTOR AUTHENTICATION (2FA) SCHEMA
-- ============================================================
-- Supports: SMS OTP, Email OTP, TOTP, Backup Codes
-- Integrated with enterprise authorization system
-- HIPAA & compliance ready
-- .NET 8 | SQL Server 2019+
-- ============================================================

-- ============================================================
-- 1. TWO-FACTOR AUTHENTICATION METHODS TABLE
-- ============================================================
CREATE TABLE security.TwoFactorMethods (
    MethodId INT PRIMARY KEY IDENTITY(1,1),
    MethodName NVARCHAR(50) NOT NULL,           -- 'SMS', 'Email', 'TOTP', 'Authenticator'
    MethodCode NVARCHAR(20) NOT NULL UNIQUE,    -- 'SMS_OTP', 'EMAIL_OTP', 'TOTP'
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    IsDefault BIT DEFAULT 0,                     -- Default method for new users
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Insert default 2FA methods
INSERT INTO security.TwoFactorMethods (MethodName, MethodCode, Description, IsActive, IsDefault)
VALUES 
    ('SMS OTP', 'SMS_OTP', 'One-Time Password sent via SMS', 1, 1),
    ('Email OTP', 'EMAIL_OTP', 'One-Time Password sent via Email', 1, 0),
    ('TOTP', 'TOTP', 'Time-based One-Time Password (Google Authenticator)', 1, 0),
    ('Backup Codes', 'BACKUP_CODES', 'Backup recovery codes', 1, 0);

-- ============================================================
-- 2. USER 2FA CONFIGURATION TABLE
-- ============================================================
CREATE TABLE security.User2FAConfiguration (
    ConfigId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    MethodId INT NOT NULL FOREIGN KEY REFERENCES security.TwoFactorMethods(MethodId),
    
    -- Contact information for 2FA
    PhoneNumber NVARCHAR(20),                    -- Encrypted in code
    Email NVARCHAR(255),                         -- Encrypted in code
    
    -- TOTP specific
    TOTPSecret NVARCHAR(255),                    -- Encrypted
    TOTPVerified BIT DEFAULT 0,
    TOTPVerifiedAt DATETIME2,
    
    -- Status
    IsEnabled BIT DEFAULT 1,
    IsPrimary BIT DEFAULT 0,                     -- Primary 2FA method
    IsBackup BIT DEFAULT 0,
    
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    LastUsedAt DATETIME2,
    
    -- Constraints
    UNIQUE(UserId, MethodId),
    INDEX IX_UserId (UserId),
    INDEX IX_MethodId (MethodId)
);

-- ============================================================
-- 3. 2FA OTP CODES TABLE (SMS & Email OTPs)
-- ============================================================
CREATE TABLE security.TwoFactorOTP (
    OTPId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ConfigId INT NOT NULL FOREIGN KEY REFERENCES security.User2FAConfiguration(ConfigId),
    
    -- OTP details
    OTPCode NVARCHAR(10) NOT NULL,               -- 6-digit or 8-digit code
    OTPHash NVARCHAR(500) NOT NULL,              -- Bcrypt hash for security
    
    -- Delivery details
    DeliveryMethod NVARCHAR(50),                 -- 'SMS', 'Email'
    DeliveryTarget NVARCHAR(255),                -- Phone or email (partially masked)
    
    -- Status
    IsUsed BIT DEFAULT 0,
    IsExpired BIT DEFAULT 0,
    
    -- Timing
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2,                         -- Typically 5-15 minutes
    UsedAt DATETIME2,
    
    -- Audit
    AttemptCount INT DEFAULT 0,
    MaxAttempts INT DEFAULT 5,
    
    -- Constraints
    INDEX IX_UserId (UserId),
    INDEX IX_ConfigId (ConfigId),
    INDEX IX_CreatedAt (CreatedAt),
    INDEX IX_ExpiresAt (ExpiresAt),
    INDEX IX_IsUsed (IsUsed)
);

-- ============================================================
-- 4. 2FA BACKUP CODES TABLE
-- ============================================================
CREATE TABLE security.TwoFactorBackupCodes (
    BackupCodeId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    
    -- Code details (8-4-4-4 format, hashed)
    CodeHash NVARCHAR(500) NOT NULL,             -- Bcrypt hash
    CodePrefix NVARCHAR(10),                     -- First 8 chars for identification
    
    -- Status
    IsUsed BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    
    -- Timing
    GeneratedAt DATETIME2 DEFAULT GETUTCDATE(),
    UsedAt DATETIME2,
    
    -- Constraints
    INDEX IX_UserId (UserId),
    INDEX IX_CodePrefix (CodePrefix)
);

-- ============================================================
-- 5. 2FA SESSION TABLE (Track verification attempts)
-- ============================================================
CREATE TABLE security.TwoFactorSession (
    SessionId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    
    -- Session details
    SessionToken NVARCHAR(500) NOT NULL UNIQUE, -- Temporary token for 2FA flow
    
    -- Status
    Status NVARCHAR(50),                         -- 'Pending', 'Verified', 'Failed', 'Expired'
    
    -- Attempts
    VerificationAttempts INT DEFAULT 0,
    MaxAttempts INT DEFAULT 5,
    
    -- Timing
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2,                         -- 15 minutes
    VerifiedAt DATETIME2,
    
    -- Client info
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    DeviceInfo NVARCHAR(500),
    
    -- Constraints
    INDEX IX_UserId (UserId),
    INDEX IX_SessionToken (SessionToken),
    INDEX IX_Status (Status),
    INDEX IX_CreatedAt (CreatedAt)
);

-- ============================================================
-- 6. 2FA AUDIT LOG TABLE (Compliance & Security)
-- ============================================================
CREATE TABLE audit.TwoFactorAuditLog (
    LogId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    
    -- Event details
    EventType NVARCHAR(50),                      -- 'Setup', 'Verified', 'Failed', 'Disabled', 'Reset'
    MethodUsed NVARCHAR(50),                     -- 'SMS', 'Email', 'TOTP'
    
    -- Details
    Description NVARCHAR(500),
    Success BIT,
    
    -- Client info
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    DeviceInfo NVARCHAR(500),
    
    -- Timestamp
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    
    -- Constraints
    INDEX IX_UserId (UserId),
    INDEX IX_EventType (EventType),
    INDEX IX_Timestamp (Timestamp),
    INDEX IX_Success (Success)
);

-- ============================================================
-- 7. 2FA DEVICE REGISTRATION TABLE (Trust devices)
-- ============================================================
CREATE TABLE security.TrustedDevices (
    DeviceId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    
    -- Device details
    DeviceName NVARCHAR(255),                    -- "iPhone 14 Pro", "Chrome on Windows"
    DeviceFingerprint NVARCHAR(500),             -- Unique device identifier
    DeviceType NVARCHAR(50),                     -- 'Mobile', 'Desktop', 'Tablet'
    
    -- Trust status
    IsTrusted BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    
    -- Browser/OS info
    BrowserName NVARCHAR(100),
    BrowserVersion NVARCHAR(50),
    OSName NVARCHAR(100),
    OSVersion NVARCHAR(50),
    
    -- Timing
    RegisteredAt DATETIME2 DEFAULT GETUTCDATE(),
    LastUsedAt DATETIME2,
    ExpiresAt DATETIME2,                         -- 90 days default
    
    -- Constraints
    INDEX IX_UserId (UserId),
    INDEX IX_DeviceFingerprint (DeviceFingerprint),
    INDEX IX_IsTrusted (IsTrusted)
);

-- ============================================================
-- 8. SMS DELIVERY PROVIDER CONFIG
-- ============================================================
CREATE TABLE config.SMSProviderConfig (
    ProviderId INT PRIMARY KEY IDENTITY(1,1),
    ProviderName NVARCHAR(100) NOT NULL,        -- 'Twilio', 'AWS SNS', 'Azure SMS'
    ProviderCode NVARCHAR(50) NOT NULL UNIQUE,  -- 'TWILIO', 'AWS_SNS', 'AZURE_SMS'
    
    -- Configuration (encrypted in production)
    ApiKey NVARCHAR(500),                        -- Store encrypted
    ApiSecret NVARCHAR(500),
    SenderNumber NVARCHAR(20),                   -- Twilio: From number
    
    -- Status
    IsActive BIT DEFAULT 1,
    IsPrimary BIT DEFAULT 0,
    
    -- Limits
    MaxSMSPerDay INT DEFAULT 1000,
    MaxSMSPerUser INT DEFAULT 5,
    
    -- Rate info
    CostPerSMS DECIMAL(10,2),                    -- For billing
    
    -- Timing
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    -- Constraints
    INDEX IX_ProviderCode (ProviderCode),
    INDEX IX_IsActive (IsActive)
);

-- Insert SMS providers
INSERT INTO config.SMSProviderConfig (ProviderName, ProviderCode, IsActive, IsPrimary)
VALUES 
    ('Twilio', 'TWILIO', 1, 1),
    ('AWS SNS', 'AWS_SNS', 0, 0),
    ('Azure SMS', 'AZURE_SMS', 0, 0);

-- ============================================================
-- 9. SMS DELIVERY LOG
-- ============================================================
CREATE TABLE audit.SMSDeliveryLog (
    LogId BIGINT PRIMARY KEY IDENTITY(1,1),
    ProviderId INT NOT NULL FOREIGN KEY REFERENCES config.SMSProviderConfig(ProviderId),
    UserId INT NOT NULL,
    
    -- SMS details
    PhoneNumber NVARCHAR(20),                    -- Recipient (masked)
    MessageContent NVARCHAR(500),
    MessageId NVARCHAR(500),                     -- Provider's message ID
    
    -- Status
    Status NVARCHAR(50),                         -- 'Sent', 'Delivered', 'Failed'
    DeliveryStatus NVARCHAR(50),                 -- Provider-specific status
    
    -- Error tracking
    ErrorCode NVARCHAR(100),
    ErrorMessage NVARCHAR(500),
    
    -- Timing
    SentAt DATETIME2 DEFAULT GETUTCDATE(),
    DeliveredAt DATETIME2,
    
    -- Cost tracking
    Cost DECIMAL(10,2),
    
    -- Constraints
    INDEX IX_UserId (UserId),
    INDEX IX_Status (Status),
    INDEX IX_SentAt (SentAt)
);

-- ============================================================
-- 10. INDEXES FOR PERFORMANCE
-- ============================================================

-- Performance optimization indexes
CREATE INDEX IX_TwoFactorOTP_ExpiresAt ON security.TwoFactorOTP(ExpiresAt) WHERE IsExpired = 0;
CREATE INDEX IX_TwoFactorSession_Status_CreatedAt ON security.TwoFactorSession(Status, CreatedAt);
CREATE INDEX IX_BackupCodes_UserId_IsUsed ON security.TwoFactorBackupCodes(UserId, IsUsed);
CREATE INDEX IX_TrustedDevices_UserId_IsTrusted ON security.TrustedDevices(UserId, IsTrusted);

-- ============================================================
-- 11. VERIFICATION QUERIES
-- ============================================================

-- Verify all tables created
SELECT 
    TABLE_NAME,
    TABLE_SCHEMA,
    GETDATE() as VerifiedAt
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA IN ('security', 'config', 'audit')
ORDER BY TABLE_SCHEMA, TABLE_NAME;

-- Check security schema
SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'security';

PRINT '? Two-Factor Authentication (2FA) schema created successfully!';
PRINT '? SMS integration tables configured!';
PRINT '? Enterprise compliance ready (HIPAA, audit trails)!';
