-- ============================================================
-- QUICK DIAGNOSTIC: Check existing table structure
-- ============================================================

-- Run this to see your current PermissionMaster table structure:
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PermissionMaster' 
  AND TABLE_SCHEMA = 'master'
ORDER BY ORDINAL_POSITION;

-- ============================================================
-- If your table exists, we need to alter it to add missing columns
-- ============================================================

-- Check if table exists
IF OBJECT_ID('master.PermissionMaster', 'U') IS NOT NULL
BEGIN
    PRINT 'Table exists - checking columns...';
    
    -- Add missing columns if they don't exist
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'Module')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD Module NVARCHAR(50) NOT NULL DEFAULT 'General';
        PRINT 'Added Module column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'Resource')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD Resource NVARCHAR(50) NOT NULL DEFAULT 'General';
        PRINT 'Added Resource column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'Operation')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD Operation NVARCHAR(50) NOT NULL DEFAULT 'View';
        PRINT 'Added Operation column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'Scope')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD Scope NVARCHAR(50);
        PRINT 'Added Scope column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'OperationCategory')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD OperationCategory NVARCHAR(50);
        PRINT 'Added OperationCategory column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'PermissionCode')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD PermissionCode NVARCHAR(100) NOT NULL DEFAULT 'UNKNOWN';
        PRINT 'Added PermissionCode column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'Description')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD Description NVARCHAR(500);
        PRINT 'Added Description column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'IsActive')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD IsActive BIT NOT NULL DEFAULT 1;
        PRINT 'Added IsActive column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'CreatedAt')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added CreatedAt column';
    END
    
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'PermissionMaster' 
                   AND TABLE_SCHEMA = 'master'
                   AND COLUMN_NAME = 'ModifiedAt')
    BEGIN
        ALTER TABLE master.PermissionMaster ADD ModifiedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added ModifiedAt column';
    END
    
    PRINT 'All columns verified/added successfully!';
END
ELSE
BEGIN
    PRINT 'Table does not exist - will be created by migration script';
END

-- ============================================================
-- Verify final structure
-- ============================================================
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PermissionMaster' 
  AND TABLE_SCHEMA = 'master'
ORDER BY ORDINAL_POSITION;
