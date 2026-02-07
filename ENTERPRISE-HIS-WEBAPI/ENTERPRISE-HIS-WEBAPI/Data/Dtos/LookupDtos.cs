namespace ENTERPRISE_HIS_WEBAPI.Data.Dtos
{
    /// <summary>
    /// DTO for creating a new LookupTypeMaster
    /// </summary>
    public class CreateLookupTypeDto
    {
        public string LookupTypeName { get; set; } = string.Empty;
        public string LookupTypeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 1;
        public bool IsSystem { get; set; } = false;
    }

    /// <summary>
    /// DTO for updating an existing LookupTypeMaster
    /// </summary>
    public class UpdateLookupTypeDto
    {
        public string LookupTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for responding with LookupTypeMaster data
    /// </summary>
    public class LookupTypeDto
    {
        public int LookupTypeMasterId { get; set; }
        public Guid GlobalId { get; set; }
        public string LookupTypeName { get; set; } = string.Empty;
        public string LookupTypeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    /// <summary>
    /// DTO for creating a new LookupTypeValueMaster
    /// </summary>
    public class CreateLookupTypeValueDto
    {
        public int LookupTypeMasterId { get; set; }
        public string LookupValueName { get; set; } = string.Empty;
        public string LookupValueCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 1;
        public bool IsSystem { get; set; } = false;
    }

    /// <summary>
    /// DTO for updating an existing LookupTypeValueMaster
    /// </summary>
    public class UpdateLookupTypeValueDto
    {
        public string LookupValueName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for responding with LookupTypeValueMaster data
    /// </summary>
    public class LookupTypeValueDto
    {
        public int LookupTypeValueId { get; set; }
        public int LookupTypeMasterId { get; set; }
        public Guid GlobalId { get; set; }
        public string LookupValueName { get; set; } = string.Empty;
        public string LookupValueCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    /// <summary>
    /// DTO for responding with LookupTypeValueMaster data including parent type info
    /// </summary>
    public class LookupTypeValueDetailDto
    {
        public int LookupTypeValueId { get; set; }
        public int LookupTypeMasterId { get; set; }
        public Guid GlobalId { get; set; }
        public string LookupValueName { get; set; } = string.Empty;
        public string LookupValueCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        
        // Parent Type Information
        public string LookupTypeName { get; set; } = string.Empty;
        public string LookupTypeCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
