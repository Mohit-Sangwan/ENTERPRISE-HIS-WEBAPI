-- ===== CLEANUP: FORCE DROP ALL EXISTING PROCEDURES =====
USE [EnterpriseHIS]
GO

-- This ensures we start fresh
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Create]', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Create];
    PRINT 'Dropped SP_LookupTypeMaster_Create'
END
GO

-- ===== LOOKUP TYPE MASTER - STORED PROCEDURES =====

-- 1. SP_LookupTypeMaster_Create
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_Create]
    @LookupTypeName NVARCHAR(100),
    @LookupTypeCode NVARCHAR(50),
    @Description NVARCHAR(255) = NULL,
    @DisplayOrder INT = 1,
    @IsSystem BIT = 0,
    @IsActive BIT = 1,
    @CreatedBy INT,
    @LookupTypeMasterId INT OUTPUT,
    @GlobalId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF ISNULL(@LookupTypeName, '') = ''
            THROW 50001, 'LookupTypeName is required', 1;
        
        IF ISNULL(@LookupTypeCode, '') = ''
            THROW 50001, 'LookupTypeCode is required', 1;
        
        IF EXISTS (SELECT 1 FROM [lookup].[LookupTypeMaster] WHERE [LookupTypeCode] = UPPER(@LookupTypeCode) AND [IsDeleted] = 0)
            THROW 50002, 'LookupTypeCode already exists', 1;
        
        -- Generate GlobalId first
        DECLARE @NewGlobalId UNIQUEIDENTIFIER = NEWID();
        
        -- Insert new record
        INSERT INTO [lookup].[LookupTypeMaster] 
        (
            [GlobalId],
            [LookupTypeName],
            [LookupTypeCode],
            [Description],
            [DisplayOrder],
            [IsSystem],
            [IsActive],
            [IsDeleted],
            [CreatedOnUTC],
            [CreatedBy]
        )
        VALUES 
        (
            @NewGlobalId,
            UPPER(@LookupTypeName),
            UPPER(@LookupTypeCode),
            @Description,
            @DisplayOrder,
            @IsSystem,
            @IsActive,
            0,
            SYSUTCDATETIME(),
            @CreatedBy
        );
        
        SET @LookupTypeMasterId = SCOPE_IDENTITY();
        SET @GlobalId = @NewGlobalId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetById]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetById];
GO

-- 2. SP_LookupTypeMaster_GetById
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_GetById]
    @LookupTypeMasterId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [LookupTypeMasterId],
        [GlobalId],
        [LookupTypeName],
        [LookupTypeCode],
        [Description],
        [DisplayOrder],
        [IsSystem],
        [IsActive],
        [CreatedOnUTC],
        [UpdatedOnUTC],
        [CreatedBy],
        [UpdatedBy]
    FROM [lookup].[LookupTypeMaster]
    WHERE [LookupTypeMasterId] = @LookupTypeMasterId
    AND [IsDeleted] = 0;
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetAll]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetAll];
GO

-- 3. SP_LookupTypeMaster_GetAll
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowsToSkip INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        [LookupTypeMasterId],
        [GlobalId],
        [LookupTypeName],
        [LookupTypeCode],
        [Description],
        [DisplayOrder],
        [IsSystem],
        [IsActive],
        [CreatedOnUTC],
        [UpdatedOnUTC],
        [CreatedBy],
        [UpdatedBy]
    FROM [lookup].[LookupTypeMaster]
    WHERE [IsDeleted] = 0
    AND (@IsActive IS NULL OR [IsActive] = @IsActive)
    ORDER BY [DisplayOrder], [LookupTypeName]
    OFFSET @RowsToSkip ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetByCode]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetByCode];
GO

-- 4. SP_LookupTypeMaster_GetByCode
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_GetByCode]
    @LookupTypeCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [LookupTypeMasterId],
        [GlobalId],
        [LookupTypeName],
        [LookupTypeCode],
        [Description],
        [DisplayOrder],
        [IsSystem],
        [IsActive],
        [CreatedOnUTC],
        [UpdatedOnUTC],
        [CreatedBy],
        [UpdatedBy]
    FROM [lookup].[LookupTypeMaster]
    WHERE [LookupTypeCode] = UPPER(@LookupTypeCode)
    AND [IsDeleted] = 0;
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Update]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Update];
GO

-- 5. SP_LookupTypeMaster_Update
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_Update]
    @LookupTypeMasterId INT,
    @LookupTypeName NVARCHAR(100),
    @Description NVARCHAR(255) = NULL,
    @DisplayOrder INT,
    @IsActive BIT,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF ISNULL(@LookupTypeName, '') = ''
            THROW 50001, 'LookupTypeName is required', 1;
        
        -- Check if record exists
        IF NOT EXISTS (SELECT 1 FROM [lookup].[LookupTypeMaster] WHERE [LookupTypeMasterId] = @LookupTypeMasterId AND [IsDeleted] = 0)
            THROW 50003, 'LookupTypeMaster not found', 1;
        
        -- Update record
        UPDATE [lookup].[LookupTypeMaster]
        SET 
            [LookupTypeName] = UPPER(@LookupTypeName),
            [Description] = @Description,
            [DisplayOrder] = @DisplayOrder,
            [IsActive] = @IsActive,
            [UpdatedOnUTC] = SYSUTCDATETIME(),
            [UpdatedBy] = @UpdatedBy
        WHERE [LookupTypeMasterId] = @LookupTypeMasterId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Delete]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Delete];
GO

-- 6. SP_LookupTypeMaster_Delete (Soft Delete)
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_Delete]
    @LookupTypeMasterId INT,
    @DeletedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if record exists
        IF NOT EXISTS (SELECT 1 FROM [lookup].[LookupTypeMaster] WHERE [LookupTypeMasterId] = @LookupTypeMasterId AND [IsDeleted] = 0)
            THROW 50003, 'LookupTypeMaster not found', 1;
        
        -- Check if it's a system record
        IF (SELECT [IsSystem] FROM [lookup].[LookupTypeMaster] WHERE [LookupTypeMasterId] = @LookupTypeMasterId) = 1
            THROW 50004, 'Cannot delete system records', 1;
        
        -- Soft delete
        UPDATE [lookup].[LookupTypeMaster]
        SET 
            [IsDeleted] = 1,
            [IsActive] = 0,
            [DeletedOnUTC] = SYSUTCDATETIME(),
            [DeletedBy] = @DeletedBy
        WHERE [LookupTypeMasterId] = @LookupTypeMasterId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_Search]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_Search];
GO

-- 7. SP_LookupTypeMaster_Search
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_Search]
    @SearchTerm NVARCHAR(100) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowsToSkip INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        [LookupTypeMasterId],
        [GlobalId],
        [LookupTypeName],
        [LookupTypeCode],
        [Description],
        [DisplayOrder],
        [IsSystem],
        [IsActive],
        [CreatedOnUTC],
        [UpdatedOnUTC],
        [CreatedBy],
        [UpdatedBy]
    FROM [lookup].[LookupTypeMaster]
    WHERE [IsDeleted] = 0
    AND (
        @SearchTerm IS NULL 
        OR [LookupTypeName] LIKE '%' + @SearchTerm + '%'
        OR [LookupTypeCode] LIKE '%' + @SearchTerm + '%'
        OR [Description] LIKE '%' + @SearchTerm + '%'
    )
    ORDER BY [DisplayOrder], [LookupTypeName]
    OFFSET @RowsToSkip ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeMaster_GetCount]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeMaster_GetCount];
GO

-- 8. SP_LookupTypeMaster_GetCount
CREATE PROCEDURE [lookup].[SP_LookupTypeMaster_GetCount]
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS [TotalCount]
    FROM [lookup].[LookupTypeMaster]
    WHERE [IsDeleted] = 0
    AND (@IsActive IS NULL OR [IsActive] = @IsActive);
END;
GO

