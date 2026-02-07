USE [EnterpriseHIS]
GO

-- ===== LOOKUP TYPE VALUE MASTER - STORED PROCEDURES =====

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Create]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Create];
GO

-- 1. SP_LookupTypeValueMaster_Create
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_Create]
    @LookupTypeMasterId INT,
    @LookupValueName NVARCHAR(100),
    @LookupValueCode NVARCHAR(50),
    @Description NVARCHAR(255) = NULL,
    @DisplayOrder INT = 1,
    @IsSystem BIT = 0,
    @IsActive BIT = 1,
    @CreatedBy INT,
    @LookupTypeValueId INT OUTPUT,
    @GlobalId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF ISNULL(@LookupTypeMasterId, 0) = 0
            THROW 50001, 'LookupTypeMasterId is required', 1;
        
        IF ISNULL(@LookupValueName, '') = ''
            THROW 50001, 'LookupValueName is required', 1;
        
        IF ISNULL(@LookupValueCode, '') = ''
            THROW 50001, 'LookupValueCode is required', 1;
        
        -- Check if LookupTypeMasterId exists
        IF NOT EXISTS (SELECT 1 FROM [lookup].[LookupTypeMaster] WHERE [LookupTypeMasterId] = @LookupTypeMasterId AND [IsDeleted] = 0)
            THROW 50005, 'LookupTypeMaster not found', 1;
        
        -- Check for duplicate code within same type
        IF EXISTS (
            SELECT 1 FROM [lookup].[LookupTypeValueMaster] 
            WHERE [LookupTypeMasterId] = @LookupTypeMasterId 
            AND [LookupValueCode] = UPPER(@LookupValueCode) 
            AND [IsDeleted] = 0
        )
            THROW 50002, 'LookupValueCode already exists for this type', 1;
        
        -- Generate GlobalId first
        DECLARE @NewGlobalId UNIQUEIDENTIFIER = NEWID();
        
        -- Insert new record
        INSERT INTO [lookup].[LookupTypeValueMaster] 
        (
            [LookupTypeMasterId],
            [GlobalId],
            [LookupValueName],
            [LookupValueCode],
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
            @LookupTypeMasterId,
            @NewGlobalId,
            UPPER(@LookupValueName),
            UPPER(@LookupValueCode),
            @Description,
            @DisplayOrder,
            @IsSystem,
            @IsActive,
            0,
            SYSUTCDATETIME(),
            @CreatedBy
        );
        
        SET @LookupTypeValueId = SCOPE_IDENTITY();
        SET @GlobalId = @NewGlobalId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetById]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetById];
GO

-- 2. SP_LookupTypeValueMaster_GetById
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetById]
    @LookupTypeValueId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [LookupTypeValueId],
        [LookupTypeMasterId],
        [GlobalId],
        [LookupValueName],
        [LookupValueCode],
        [Description],
        [DisplayOrder],
        [IsSystem],
        [IsActive],
        [CreatedOnUTC],
        [UpdatedOnUTC],
        [CreatedBy],
        [UpdatedBy]
    FROM [lookup].[LookupTypeValueMaster]
    WHERE [LookupTypeValueId] = @LookupTypeValueId
    AND [IsDeleted] = 0;
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetByTypeId]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetByTypeId];
GO

-- 3. SP_LookupTypeValueMaster_GetByTypeId
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetByTypeId]
    @LookupTypeMasterId INT,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [LookupTypeValueId],
        [LookupTypeMasterId],
        [GlobalId],
        [LookupValueName],
        [LookupValueCode],
        [Description],
        [DisplayOrder],
        [IsSystem],
        [IsActive],
        [CreatedOnUTC],
        [UpdatedOnUTC],
        [CreatedBy],
        [UpdatedBy]
    FROM [lookup].[LookupTypeValueMaster]
    WHERE [LookupTypeMasterId] = @LookupTypeMasterId
    AND [IsDeleted] = 0
    AND (@IsActive IS NULL OR [IsActive] = @IsActive)
    ORDER BY [DisplayOrder], [LookupValueName];
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetByTypeCode]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetByTypeCode];
GO

-- 4. SP_LookupTypeValueMaster_GetByTypeCode
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetByTypeCode]
    @LookupTypeCode NVARCHAR(50),
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ltv.[LookupTypeValueId],
        ltv.[LookupTypeMasterId],
        ltv.[GlobalId],
        ltv.[LookupValueName],
        ltv.[LookupValueCode],
        ltv.[Description],
        ltv.[DisplayOrder],
        ltv.[IsSystem],
        ltv.[IsActive],
        ltv.[CreatedOnUTC],
        ltv.[UpdatedOnUTC],
        ltv.[CreatedBy],
        ltv.[UpdatedBy],
        ltm.[LookupTypeName],
        ltm.[LookupTypeCode]
    FROM [lookup].[LookupTypeValueMaster] ltv
    INNER JOIN [lookup].[LookupTypeMaster] ltm ON ltv.[LookupTypeMasterId] = ltm.[LookupTypeMasterId]
    WHERE ltm.[LookupTypeCode] = UPPER(@LookupTypeCode)
    AND ltv.[IsDeleted] = 0
    AND ltm.[IsDeleted] = 0
    AND (@IsActive IS NULL OR ltv.[IsActive] = @IsActive)
    ORDER BY ltv.[DisplayOrder], ltv.[LookupValueName];
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Update]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Update];
GO

-- 5. SP_LookupTypeValueMaster_Update
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_Update]
    @LookupTypeValueId INT,
    @LookupValueName NVARCHAR(100),
    @Description NVARCHAR(255) = NULL,
    @DisplayOrder INT,
    @IsActive BIT,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF ISNULL(@LookupValueName, '') = ''
            THROW 50001, 'LookupValueName is required', 1;
        
        -- Check if record exists
        IF NOT EXISTS (SELECT 1 FROM [lookup].[LookupTypeValueMaster] WHERE [LookupTypeValueId] = @LookupTypeValueId AND [IsDeleted] = 0)
            THROW 50003, 'LookupTypeValueMaster not found', 1;
        
        -- Update record
        UPDATE [lookup].[LookupTypeValueMaster]
        SET 
            [LookupValueName] = UPPER(@LookupValueName),
            [Description] = @Description,
            [DisplayOrder] = @DisplayOrder,
            [IsActive] = @IsActive,
            [UpdatedOnUTC] = SYSUTCDATETIME(),
            [UpdatedBy] = @UpdatedBy
        WHERE [LookupTypeValueId] = @LookupTypeValueId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Delete]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Delete];
GO

-- 6. SP_LookupTypeValueMaster_Delete (Soft Delete)
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_Delete]
    @LookupTypeValueId INT,
    @DeletedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if record exists
        IF NOT EXISTS (SELECT 1 FROM [lookup].[LookupTypeValueMaster] WHERE [LookupTypeValueId] = @LookupTypeValueId AND [IsDeleted] = 0)
            THROW 50003, 'LookupTypeValueMaster not found', 1;
        
        -- Check if it's a system record
        IF (SELECT [IsSystem] FROM [lookup].[LookupTypeValueMaster] WHERE [LookupTypeValueId] = @LookupTypeValueId) = 1
            THROW 50004, 'Cannot delete system records', 1;
        
        -- Soft delete
        UPDATE [lookup].[LookupTypeValueMaster]
        SET 
            [IsDeleted] = 1,
            [IsActive] = 0,
            [DeletedOnUTC] = SYSUTCDATETIME(),
            [DeletedBy] = @DeletedBy
        WHERE [LookupTypeValueId] = @LookupTypeValueId;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetAll]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetAll];
GO

-- 7. SP_LookupTypeValueMaster_GetAll
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowsToSkip INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        ltv.[LookupTypeValueId],
        ltv.[LookupTypeMasterId],
        ltv.[GlobalId],
        ltv.[LookupValueName],
        ltv.[LookupValueCode],
        ltv.[Description],
        ltv.[DisplayOrder],
        ltv.[IsSystem],
        ltv.[IsActive],
        ltv.[CreatedOnUTC],
        ltv.[UpdatedOnUTC],
        ltv.[CreatedBy],
        ltv.[UpdatedBy],
        ltm.[LookupTypeName],
        ltm.[LookupTypeCode]
    FROM [lookup].[LookupTypeValueMaster] ltv
    INNER JOIN [lookup].[LookupTypeMaster] ltm ON ltv.[LookupTypeMasterId] = ltm.[LookupTypeMasterId]
    WHERE ltv.[IsDeleted] = 0
    AND ltm.[IsDeleted] = 0
    AND (@IsActive IS NULL OR ltv.[IsActive] = @IsActive)
    ORDER BY ltv.[DisplayOrder], ltv.[LookupValueName]
    OFFSET @RowsToSkip ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_GetCount]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetCount];
GO

-- 8. SP_LookupTypeValueMaster_GetCount
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_GetCount]
    @LookupTypeMasterId INT = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS [TotalCount]
    FROM [lookup].[LookupTypeValueMaster]
    WHERE [IsDeleted] = 0
    AND (@LookupTypeMasterId IS NULL OR [LookupTypeMasterId] = @LookupTypeMasterId)
    AND (@IsActive IS NULL OR [IsActive] = @IsActive);
END;
GO

-- Drop existing procedures if they exist
IF OBJECT_ID('[lookup].[SP_LookupTypeValueMaster_Search]', 'P') IS NOT NULL
    DROP PROCEDURE [lookup].[SP_LookupTypeValueMaster_Search];
GO

-- 9. SP_LookupTypeValueMaster_Search
CREATE PROCEDURE [lookup].[SP_LookupTypeValueMaster_Search]
    @LookupTypeMasterId INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowsToSkip INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        ltv.[LookupTypeValueId],
        ltv.[LookupTypeMasterId],
        ltv.[GlobalId],
        ltv.[LookupValueName],
        ltv.[LookupValueCode],
        ltv.[Description],
        ltv.[DisplayOrder],
        ltv.[IsSystem],
        ltv.[IsActive],
        ltv.[CreatedOnUTC],
        ltv.[UpdatedOnUTC],
        ltv.[CreatedBy],
        ltv.[UpdatedBy],
        ltm.[LookupTypeName],
        ltm.[LookupTypeCode]
    FROM [lookup].[LookupTypeValueMaster] ltv
    INNER JOIN [lookup].[LookupTypeMaster] ltm ON ltv.[LookupTypeMasterId] = ltm.[LookupTypeMasterId]
    WHERE ltv.[IsDeleted] = 0
    AND ltm.[IsDeleted] = 0
    AND (@LookupTypeMasterId IS NULL OR ltv.[LookupTypeMasterId] = @LookupTypeMasterId)
    AND (
        @SearchTerm IS NULL 
        OR ltv.[LookupValueName] LIKE '%' + @SearchTerm + '%'
        OR ltv.[LookupValueCode] LIKE '%' + @SearchTerm + '%'
        OR ltv.[Description] LIKE '%' + @SearchTerm + '%'
    )
    ORDER BY ltv.[DisplayOrder], ltv.[LookupValueName]
    OFFSET @RowsToSkip ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_SCHEMA='lookup' AND ROUTINE_NAME LIKE 'SP_Lookup%'
-- Expected: 17
GO

