# Enterprise HIS Web API - Lookup CRUD Implementation Guide

## ?? Overview

This guide provides complete, enterprise-level implementation for **CRUD operations** on `LookupTypeMaster` and `LookupTypeValueMaster` tables using:

- **SQL Server Stored Procedures** for data access
- **Repository Pattern** for data abstraction
- **Service Layer** for business logic
- **REST API Controllers** for HTTP endpoints
- **Enterprise.DAL.V1** for connection pooling and performance
- **Dependency Injection** for loose coupling
- **Pagination & Filtering** for scalability

---

## ??? Architecture Layers

```
???????????????????????????????????????
?   REST API Controllers              ?  ? HTTP Endpoints (api/v1/...)
???????????????????????????????????????
?   Service Layer (ILookupType*Service)? ? Business Logic
???????????????????????????????????????
?  Repository (ILookupType*Repository) ? ? Data Access Abstraction
???????????????????????????????????????
?   Enterprise.DAL.V1 (ISqlServerDal) ? ? Connection Pooling & Caching
???????????????????????????????????????
?   SQL Server Stored Procedures      ? ? Database Operations
???????????????????????????????????????
?   SQL Server Database               ? ? Persistent Storage
???????????????????????????????????????
```

---

## ?? Project Structure

```
ENTERPRISE-HIS-WEBAPI/
?
??? Controllers/
?   ??? LookupController.cs              # REST API endpoints
?
??? Data/
?   ??? Repositories/
?   ?   ??? ILookupRepository.cs         # Repository interfaces
?   ?   ??? LookupRepository.cs          # Repository implementations
?   ??? Dtos/
?       ??? LookupDtos.cs               # Data Transfer Objects
?
??? Services/
?   ??? LookupService.cs                # Business logic
?
??? Program.cs                           # Dependency injection setup
?
??? Database/
    ??? 01_LookupTypeMaster_StoredProcedures.sql
    ??? 02_LookupTypeValueMaster_StoredProcedures.sql
```

---

## ??? SQL Stored Procedures

### LookupTypeMaster Procedures

**1. SP_LookupTypeMaster_Create**
- Creates a new lookup type
- Validates inputs and prevents duplicates
- Returns new ID and GlobalId

**2. SP_LookupTypeMaster_GetById**
- Retrieves single lookup type by ID
- Returns complete record

**3. SP_LookupTypeMaster_GetByCode**
- Retrieves lookup type by code (case-insensitive)

**4. SP_LookupTypeMaster_GetAll**
- Paginated retrieval of all lookup types
- Supports filtering by active status
- Returns: data + pagination metadata

**5. SP_LookupTypeMaster_Search**
- Full-text search on name, code, description
- Paginated results

**6. SP_LookupTypeMaster_Update**
- Updates existing lookup type
- Prevents updates to system records

**7. SP_LookupTypeMaster_Delete**
- Soft delete (sets IsDeleted = 1, IsActive = 0)
- Prevents deletion of system records

**8. SP_LookupTypeMaster_GetCount**
- Returns total count (useful for pagination)

### LookupTypeValueMaster Procedures

Same 9 procedures for managing lookup values with additional:
- **SP_LookupTypeValueMaster_GetByTypeId** - Get all values for a type
- **SP_LookupTypeValueMaster_GetByTypeCode** - Get values by parent type code

---

## ?? Database Setup

### Run Stored Procedures

```sql
-- 1. Execute LookupTypeMaster procedures
USE [EnterpriseHIS]
GO
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\01_LookupTypeMaster_StoredProcedures.sql"

-- 2. Execute LookupTypeValueMaster procedures
:r "D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\Database\02_LookupTypeValueMaster_StoredProcedures.sql"
```

---

## ?? DTOs (Data Transfer Objects)

### LookupType DTOs

```csharp
// Create Request
public class CreateLookupTypeDto
{
    public string LookupTypeName { get; set; }      // Required
    public string LookupTypeCode { get; set; }      // Required
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsSystem { get; set; } = false;
}

// Update Request
public class UpdateLookupTypeDto
{
    public string LookupTypeName { get; set; }      // Required
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

// Response
public class LookupTypeDto
{
    public int LookupTypeMasterId { get; set; }
    public Guid GlobalId { get; set; }
    public string LookupTypeName { get; set; }
    public string LookupTypeCode { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOnUTC { get; set; }
    public DateTime? UpdatedOnUTC { get; set; }
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
```

### Response Wrappers

```csharp
// Paginated Response
public class PaginatedResponse<T>
{
    public List<T> Data { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// API Response
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
```

---

## ?? Repository Pattern

### Repository Interfaces

```csharp
public interface ILookupTypeRepository
{
    // Read Operations
    Task<LookupTypeDto?> GetByIdAsync(int lookupTypeMasterId);
    Task<LookupTypeDto?> GetByCodeAsync(string lookupTypeCode);
    Task<PaginatedResponse<LookupTypeDto>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 10, 
        bool? isActive = null);
    Task<PaginatedResponse<LookupTypeDto>> SearchAsync(
        string? searchTerm, 
        int pageNumber = 1, 
        int pageSize = 10);
    Task<int> GetCountAsync(bool? isActive = null);

    // Write Operations
    Task<(int Id, Guid GlobalId)> CreateAsync(CreateLookupTypeDto request, int createdBy);
    Task<bool> UpdateAsync(int id, UpdateLookupTypeDto request, int updatedBy);
    Task<bool> DeleteAsync(int id, int deletedBy);
}
```

### Key Implementation Details

```csharp
public class LookupTypeRepository : ILookupTypeRepository
{
    private readonly ISqlServerDal _dal;
    private readonly IDbLogger? _logger;

    public LookupTypeRepository(ISqlServerDal dal, IDbLogger? logger = null)
    {
        _dal = dal;              // Enterprise DAL (singleton)
        _logger = logger;        // Logging (optional)
    }

    // All methods use:
    // 1. Stored procedures (isStoredProc: true)
    // 2. Parameterized queries (DbParam for safety)
    // 3. Row mapping (DataRow ? DTO)
    // 4. Logging for auditing
}
```

---

## ?? Service Layer

### Service Interfaces & Implementations

```csharp
public interface ILookupTypeService
{
    Task<LookupTypeDto?> GetByIdAsync(int id);
    Task<LookupTypeDto?> GetByCodeAsync(string code);
    Task<PaginatedResponse<LookupTypeDto>> GetAllAsync(...);
    Task<PaginatedResponse<LookupTypeDto>> SearchAsync(...);
    Task<int> GetCountAsync(bool? isActive = null);
    Task<ApiResponse<(int Id, Guid GlobalId)>> CreateAsync(CreateLookupTypeDto request, int createdBy);
    Task<ApiResponse<bool>> UpdateAsync(int id, UpdateLookupTypeDto request, int updatedBy);
    Task<ApiResponse<bool>> DeleteAsync(int id, int deletedBy);
}
```

### Business Logic

```csharp
public class LookupTypeService : ILookupTypeService
{
    private readonly ILookupTypeRepository _repository;
    private readonly IDbLogger? _logger;

    public async Task<ApiResponse<(int Id, Guid GlobalId)>> CreateAsync(
        CreateLookupTypeDto request, 
        int createdBy)
    {
        try
        {
            // Validation
            ValidateCreateLookupTypeDto(request);
            if (createdBy <= 0)
                throw new ArgumentException("Invalid CreatedBy");

            // Business Logic
            var result = await _repository.CreateAsync(request, createdBy);

            return new ApiResponse<(int Id, Guid GlobalId)>
            {
                Success = true,
                Message = "Created successfully",
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error creating LookupType");
            return new ApiResponse<(int Id, Guid GlobalId)>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
```

---

## ?? REST API Endpoints

### LookupTypes Controller

```
GET    /api/v1/LookupTypes                    # Get all (paginated)
GET    /api/v1/LookupTypes/{id}              # Get by ID
GET    /api/v1/LookupTypes/code/{code}       # Get by code
GET    /api/v1/LookupTypes/search             # Search
GET    /api/v1/LookupTypes/count              # Get count
POST   /api/v1/LookupTypes                    # Create
PUT    /api/v1/LookupTypes/{id}              # Update
DELETE /api/v1/LookupTypes/{id}              # Delete
```

### LookupTypeValues Controller

```
GET    /api/v1/LookupTypeValues               # Get all (paginated)
GET    /api/v1/LookupTypeValues/{id}         # Get by ID
GET    /api/v1/LookupTypeValues/by-type/{typeId}         # Get by Type ID
GET    /api/v1/LookupTypeValues/by-type-code/{typeCode}  # Get by Type Code
GET    /api/v1/LookupTypeValues/search        # Search
GET    /api/v1/LookupTypeValues/count         # Get count
POST   /api/v1/LookupTypeValues               # Create
PUT    /api/v1/LookupTypeValues/{id}         # Update
DELETE /api/v1/LookupTypeValues/{id}         # Delete
```

---

## ?? API Usage Examples

### Create LookupType

```http
POST /api/v1/LookupTypes HTTP/1.1
Content-Type: application/json

{
  "lookupTypeName": "GENDER",
  "lookupTypeCode": "GENDER",
  "description": "Gender lookup type",
  "displayOrder": 1,
  "isSystem": true
}

Response: 201 Created
{
  "success": true,
  "message": "LookupType created successfully",
  "data": {
    "id": 1,
    "globalId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### Get LookupType by ID

```http
GET /api/v1/LookupTypes/1 HTTP/1.1

Response: 200 OK
{
  "lookupTypeMasterId": 1,
  "globalId": "550e8400-e29b-41d4-a716-446655440000",
  "lookupTypeName": "GENDER",
  "lookupTypeCode": "GENDER",
  "description": "Gender lookup type",
  "displayOrder": 1,
  "isSystem": true,
  "isActive": true,
  "createdOnUTC": "2024-01-15T10:30:00Z",
  "createdBy": 1
}
```

### Get All with Pagination

```http
GET /api/v1/LookupTypes?pageNumber=1&pageSize=10&isActive=true HTTP/1.1

Response: 200 OK
{
  "data": [ ... ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Search

```http
GET /api/v1/LookupTypes/search?searchTerm=gender&pageNumber=1&pageSize=10 HTTP/1.1

Response: 200 OK
{
  "data": [
    { /* matching LookupTypes */ }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### Create LookupTypeValue

```http
POST /api/v1/LookupTypeValues HTTP/1.1
Content-Type: application/json

{
  "lookupTypeMasterId": 1,
  "lookupValueName": "MALE",
  "lookupValueCode": "M",
  "description": "Male",
  "displayOrder": 1,
  "isSystem": true
}

Response: 201 Created
```

### Get Values by Type Code

```http
GET /api/v1/LookupTypeValues/by-type-code/GENDER?isActive=true HTTP/1.1

Response: 200 OK
{
  "data": [
    {
      "lookupTypeValueId": 1,
      "lookupValueName": "MALE",
      "lookupValueCode": "M",
      "lookupTypeName": "GENDER",
      "lookupTypeCode": "GENDER",
      ...
    },
    {
      "lookupTypeValueId": 2,
      "lookupValueName": "FEMALE",
      "lookupValueCode": "F",
      "lookupTypeName": "GENDER",
      "lookupTypeCode": "GENDER",
      ...
    }
  ]
}
```

---

## ?? Dependency Injection Setup

### Program.cs Configuration

```csharp
// ===== Register Repository Services =====
builder.Services.AddScoped<ILookupTypeRepository, LookupTypeRepository>();
builder.Services.AddScoped<ILookupTypeValueRepository, LookupTypeValueRepository>();

// ===== Register Business Services =====
builder.Services.AddScoped<ILookupTypeService, LookupTypeService>();
builder.Services.AddScoped<ILookupTypeValueService, LookupTypeValueService>();
```

### Lifetime Explanation

| Component | Lifetime | Why |
|-----------|----------|-----|
| **ISqlServerDal** | **Singleton** | Connection pooling must be shared |
| **Repositories** | **Scoped** | Per-request isolation, uses singleton DAL |
| **Services** | **Scoped** | Per-request isolation, uses scoped repositories |
| **Controllers** | **Transient** | Created per-request (ASP.NET Core default) |

---

## ?? Testing Examples

### Unit Test for Service

```csharp
[Fact]
public async Task CreateAsync_WithValidInput_ReturnsSuccess()
{
    // Arrange
    var mockRepository = new Mock<ILookupTypeRepository>();
    var service = new LookupTypeService(mockRepository.Object, null);
    var request = new CreateLookupTypeDto 
    { 
        LookupTypeName = "TEST",
        LookupTypeCode = "TEST"
    };

    mockRepository.Setup(x => x.CreateAsync(request, 1))
        .ReturnsAsync((1, Guid.NewGuid()));

    // Act
    var result = await service.CreateAsync(request, 1);

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Data);
}
```

---

## ?? Performance Optimization

### 1. **Connection Pooling**
- Handled automatically by Enterprise.DAL.V1
- Configured in `appsettings.json`
- **Max Pool Size**: 200 connections

### 2. **Caching**
- 5-minute TTL configured
- Improves read performance
- Enabled via `WithCaching(ttlMinutes: 5)`

### 3. **Pagination**
- Prevents loading too much data
- Default: 10 items per page
- Max: 100 items per page

### 4. **Indexing** (SQL Server)
- Index on `LookupTypeCode` (UNIQUE)
- Index on `IsDeleted` (for soft deletes)
- Index on `DisplayOrder`

### 5. **Stored Procedures**
- Pre-compiled for performance
- Reduce network roundtrips
- Better security

---

## ?? Security Features

### 1. **SQL Injection Prevention**
- All queries use parameterized statements
- No string concatenation

### 2. **Data Masking**
- Sensitive data automatically masked in logs
- Configured via `WithDataMasking()`

### 3. **Soft Deletes**
- Records never permanently deleted
- Maintains audit trail
- IsDeleted flag prevents access

### 4. **Audit Trail**
- CreatedBy, UpdatedBy, DeletedBy tracked
- CreatedOnUTC, UpdatedOnUTC, DeletedOnUTC tracked
- RowVersion for concurrency control

### 5. **System Record Protection**
- Can't delete system records
- IsSystem flag prevents modification

---

## ?? Best Practices

### 1. **Repository Pattern**
? Repositories handle data access only
? Services handle business logic
? Controllers handle HTTP concerns

### 2. **Dependency Injection**
? Constructor injection for all dependencies
? Use interfaces for loose coupling
? Scoped lifetime for per-request isolation

### 3. **Async/Await**
? All DB operations use async
? Better scalability and responsiveness
? Prevents blocking threads

### 4. **Error Handling**
? Try-catch in service layer
? Return ApiResponse wrappers
? Log all exceptions

### 5. **Validation**
? Validate input in service layer
? Use FluentValidation for complex rules
? Return meaningful error messages

### 6. **Logging**
? Log create/read/update/delete operations
? Include relevant context (ID, user, timestamp)
? Use appropriate log levels (Info, Warning, Error)

---

## ?? Summary

This implementation provides:

? **Enterprise-grade CRUD operations**
? **SQL Server stored procedures**
? **Repository & Service patterns**
? **REST API with proper HTTP semantics**
? **Pagination & filtering**
? **Comprehensive error handling**
? **Audit trail & soft deletes**
? **Security & SQL injection prevention**
? **Performance optimization**
? **Full async/await support**

All components are production-ready and follow industry best practices!

---

**For questions or issues, refer to the code comments and class documentation.**
