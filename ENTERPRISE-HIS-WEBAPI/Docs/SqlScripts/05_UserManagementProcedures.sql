-- ============================================================================
-- USER MANAGEMENT STORED PROCEDURES
-- Enterprise HIS - User CRUD Operations
-- ============================================================================

-- Drop existing procedures if they exist
IF OBJECT_ID('config.SP_GetUserById', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_GetUserById;
GO

IF OBJECT_ID('config.SP_GetUserByUsername', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_GetUserByUsername;
GO

IF OBJECT_ID('config.SP_GetUserByEmail', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_GetUserByEmail;
GO

IF OBJECT_ID('config.SP_CreateUser', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_CreateUser;
GO

IF OBJECT_ID('config.SP_UpdateUser', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_UpdateUser;
GO

IF OBJECT_ID('config.SP_DeleteUser', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_DeleteUser;
GO

IF OBJECT_ID('config.SP_DeactivateUser', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_DeactivateUser;
GO

IF OBJECT_ID('config.SP_ActivateUser', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_ActivateUser;
GO

IF OBJECT_ID('config.SP_UpdateUserPassword', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_UpdateUserPassword;
GO

IF OBJECT_ID('config.SP_CheckUsernameExists', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_CheckUsernameExists;
GO

IF OBJECT_ID('config.SP_CheckEmailExists', 'P') IS NOT NULL
    DROP PROCEDURE config.SP_CheckEmailExists;
GO

-- ============================================================================
-- GET USER BY ID
-- ============================================================================
CREATE PROCEDURE config.SP_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UserId],
        [Username],
        [Email],
        [FirstName],
        [LastName],
        [IsActive],
        [CreatedDate],
        [ModifiedDate],
        [LastLoginDate]
    FROM [core].[UserAccount]
    WHERE [UserId] = @UserId
        AND [IsActive] = 1;
END;
GO

-- ============================================================================
-- GET USER BY USERNAME
-- ============================================================================
CREATE PROCEDURE config.SP_GetUserByUsername
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UserId],
        [Username],
        [Email],
        [FirstName],
        [LastName],
        [IsActive],
        [CreatedDate],
        [ModifiedDate],
        [LastLoginDate]
    FROM [core].[UserAccount]
    WHERE [Username] = @Username
        AND [IsActive] = 1;
END;
GO

-- ============================================================================
-- GET USER BY EMAIL
-- ============================================================================
CREATE PROCEDURE config.SP_GetUserByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [UserId],
        [Username],
        [Email],
        [FirstName],
        [LastName],
        [IsActive],
        [CreatedDate],
        [ModifiedDate],
        [LastLoginDate]
    FROM [core].[UserAccount]
    WHERE [Email] = @Email
        AND [IsActive] = 1;
END;
GO

-- ============================================================================
-- CREATE USER
-- ============================================================================
CREATE PROCEDURE config.SP_CreateUser
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(MAX),
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if username exists
        IF EXISTS (SELECT 1 FROM [core].[UserAccount] WHERE [Username] = @Username)
        BEGIN
            RAISERROR('Username already exists', 16, 1);
            RETURN;
        END;
        
        -- Check if email exists
        IF EXISTS (SELECT 1 FROM [core].[UserAccount] WHERE [Email] = @Email)
        BEGIN
            RAISERROR('Email already exists', 16, 1);
            RETURN;
        END;
        
        -- Insert new user
        INSERT INTO [core].[UserAccount]
        (
            [Username],
            [Email],
            [PasswordHash],
            [FirstName],
            [LastName],
            [IsActive],
            [CreatedDate]
        )
        VALUES
        (
            @Username,
            @Email,
            @PasswordHash,
            @FirstName,
            @LastName,
            @IsActive,
            GETUTCDATE()
        );
        
        -- Return the newly created UserId
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    END TRY
    BEGIN CATCH
        RAISERROR('Error creating user', 16, 1);
    END CATCH;
END;
GO

-- ============================================================================
-- UPDATE USER
-- ============================================================================
CREATE PROCEDURE config.SP_UpdateUser
    @UserId INT,
    @FirstName NVARCHAR(50) = NULL,
    @LastName NVARCHAR(50) = NULL,
    @Email NVARCHAR(100) = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [core].[UserAccount] WHERE [UserId] = @UserId)
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END;
        
        -- Check if new email is being set and if it's unique
        IF @Email IS NOT NULL AND @Email <> (SELECT [Email] FROM [core].[UserAccount] WHERE [UserId] = @UserId)
        BEGIN
            IF EXISTS (SELECT 1 FROM [core].[UserAccount] WHERE [Email] = @Email AND [UserId] <> @UserId)
            BEGIN
                RAISERROR('Email already in use', 16, 1);
                RETURN;
            END;
        END;
        
        -- Update user
        UPDATE [core].[UserAccount]
        SET
            [FirstName] = ISNULL(@FirstName, [FirstName]),
            [LastName] = ISNULL(@LastName, [LastName]),
            [Email] = ISNULL(@Email, [Email]),
            [IsActive] = ISNULL(@IsActive, [IsActive]),
            [ModifiedDate] = GETUTCDATE()
        WHERE [UserId] = @UserId;
        
        SELECT @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        RAISERROR('Error updating user', 16, 1);
    END CATCH;
END;
GO

-- ============================================================================
-- DELETE USER (Hard Delete)
-- ============================================================================
CREATE PROCEDURE config.SP_DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [core].[UserAccount] WHERE [UserId] = @UserId)
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END;
        
        -- Remove user roles first
        DELETE FROM [config].[UserRole] WHERE [UserId] = @UserId;
        
        -- Delete user
        DELETE FROM [core].[UserAccount] WHERE [UserId] = @UserId;
        
        SELECT @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        RAISERROR('Error deleting user', 16, 1);
    END CATCH;
END;
GO

-- ============================================================================
-- DEACTIVATE USER (Soft Delete)
-- ============================================================================
CREATE PROCEDURE config.SP_DeactivateUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [core].[UserAccount] WHERE [UserId] = @UserId)
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END;
        
        -- Deactivate user
        UPDATE [core].[UserAccount]
        SET [IsActive] = 0, [ModifiedDate] = GETUTCDATE()
        WHERE [UserId] = @UserId;
        
        SELECT @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        RAISERROR('Error deactivating user', 16, 1);
    END CATCH;
END;
GO

-- ============================================================================
-- ACTIVATE USER
-- ============================================================================
CREATE PROCEDURE config.SP_ActivateUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [core].[UserAccount] WHERE [UserId] = @UserId)
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END;
        
        -- Activate user
        UPDATE [core].[UserAccount]
        SET [IsActive] = 1, [ModifiedDate] = GETUTCDATE()
        WHERE [UserId] = @UserId;
        
        SELECT @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        RAISERROR('Error activating user', 16, 1);
    END CATCH;
END;
GO

-- ============================================================================
-- UPDATE USER PASSWORD
-- ============================================================================
CREATE PROCEDURE config.SP_UpdateUserPassword
    @UserId INT,
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [core].[UserAccount] WHERE [UserId] = @UserId)
        BEGIN
            RAISERROR('User not found', 16, 1);
            RETURN;
        END;
        
        -- Update password
        UPDATE [core].[UserAccount]
        SET [PasswordHash] = @PasswordHash, [ModifiedDate] = GETUTCDATE()
        WHERE [UserId] = @UserId;
        
        SELECT @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        RAISERROR('Error updating password', 16, 1);
    END CATCH;
END;
GO

-- ============================================================================
-- CHECK USERNAME EXISTS
-- ============================================================================
CREATE PROCEDURE config.SP_CheckUsernameExists
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) FROM [core].[UserAccount] WHERE [Username] = @Username;
END;
GO

-- ============================================================================
-- CHECK EMAIL EXISTS
-- ============================================================================
CREATE PROCEDURE config.SP_CheckEmailExists
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) FROM [core].[UserAccount] WHERE [Email] = @Email;
END;
GO

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- Verify all procedures were created
PRINT '? User Management Procedures Created Successfully';
PRINT '';
PRINT 'Available Procedures:';
SELECT 
    ROUTINE_NAME, 
    ROUTINE_TYPE
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'config'
    AND ROUTINE_NAME LIKE 'SP_%User%'
    OR ROUTINE_NAME LIKE 'SP_Check%'
ORDER BY ROUTINE_NAME;

-- Verify table exists
IF OBJECT_ID('core.UserAccount', 'U') IS NOT NULL
BEGIN
    PRINT '';
    PRINT '? UserAccount table exists';
    PRINT '';
    PRINT 'Table Structure:';
    SELECT 
        COLUMN_NAME, 
        DATA_TYPE, 
        IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'core' 
        AND TABLE_NAME = 'UserAccount'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '';
    PRINT '? UserAccount table NOT FOUND - Please create it first';
END;
