# Quick Reference Guide - Lookup CRUD Operations

## ?? Quick Start (5 Minutes)

### 1. Run SQL Scripts

```sql
-- Execute in SQL Server Management Studio
USE [EnterpriseHIS]

-- Create all stored procedures
:r "Database\01_LookupTypeMaster_StoredProcedures.sql"
:r "Database\02_LookupTypeValueMaster_StoredProcedures.sql"
```

### 2. Verify Build

```powershell
dotnet build
# Should see: Build successful
```

### 3. Run Application

```powershell
dotnet run
```

### 4. Test in Swagger

Navigate to: `https://localhost:5001/swagger`

---

## ?? API Endpoints Reference

### LookupTypes

| Method | Endpoint | Description | Example |
|--------|----------|-------------|---------|
| POST | `/api/v1/lookuptypes` | Create | `{ "lookupTypeName": "GENDER", "lookupTypeCode": "GENDER" }` |
| GET | `/api/v1/lookuptypes` | List all (paginated) | `?pageNumber=1&pageSize=10` |
| GET | `/api/v1/lookuptypes/{id}` | Get by ID | `/api/v1/lookuptypes/1` |
| GET | `/api/v1/lookuptypes/code/{code}` | Get by code | `/api/v1/lookuptypes/code/GENDER` |
| GET | `/api/v1/lookuptypes/search` | Search | `?searchTerm=gender` |
| GET | `/api/v1/lookuptypes/count` | Count | `?isActive=true` |
| PUT | `/api/v1/lookuptypes/{id}` | Update | `{ "lookupTypeName": "GENDER_TYPES", ... }` |
| DELETE | `/api/v1/lookuptypes/{id}` | Delete | `/api/v1/lookuptypes/1` |

### LookupTypeValues

| Method | Endpoint | Description | Example |
|--------|----------|-------------|---------|
| POST | `/api/v1/lookuptypevalues` | Create | `{ "lookupTypeMasterId": 1, "lookupValueName": "MALE", ... }` |
| GET | `/api/v1/lookuptypevalues` | List all (paginated) | `?pageNumber=1&pageSize=10` |
| GET | `/api/v1/lookuptypevalues/{id}` | Get by ID | `/api/v1/lookuptypevalues/1` |
| GET | `/api/v1/lookuptypevalues/by-type/{typeId}` | By Type ID | `/api/v1/lookuptypevalues/by-type/1` |
| GET | `/api/v1/lookuptypevalues/by-type-code/{code}` | By Type Code | `/api/v1/lookuptypevalues/by-type-code/GENDER` |
| GET | `/api/v1/lookuptypevalues/search` | Search | `?searchTerm=male&typeId=1` |
| GET | `/api/v1/lookuptypevalues/count` | Count | `?typeId=1` |
| PUT | `/api/v1/lookuptypevalues/{id}` | Update | `{ "lookupValueName": "MALE_NEW", ... }` |
| DELETE | `/api/v1/lookuptypevalues/{id}` | Delete | `/api/v1/lookuptypevalues/1` |

---

## ?? Database Layer

### Stored Procedures Available

**LookupTypeMaster:**
- `SP_LookupTypeMaster_Create` - Insert
- `SP_LookupTypeMaster_GetById` - Read by ID
- `SP_LookupTypeMaster_GetByCode` - Read by code
- `SP_LookupTypeMaster_GetAll` - Read paginated
- `SP_LookupTypeMaster_Search` - Search
- `SP_LookupTypeMaster_Update` - Update
- `SP_LookupTypeMaster_Delete` - Soft delete
- `SP_LookupTypeMaster_GetCount` - Count

**LookupTypeValueMaster:**
- `SP_LookupTypeValueMaster_Create` - Insert
- `SP_LookupTypeValueMaster_GetById` - Read by ID
- `SP_LookupTypeValueMaster_GetByTypeId` - Read by parent type
- `SP_LookupTypeValueMaster_GetByTypeCode` - Read by parent code
- `SP_LookupTypeValueMaster_GetAll` - Read paginated
- `SP_LookupTypeValueMaster_Search` - Search
- `SP_LookupTypeValueMaster_Update` - Update
- `SP_LookupTypeValueMaster_Delete` - Soft delete
- `SP_LookupTypeValueMaster_GetCount` - Count

---

## ??? Code Layer

### Repositories
```csharp
// Inject in services
private readonly ILookupTypeRepository _repository;
private readonly ILookupTypeValueRepository _valueRepository;

// Usage
await _repository.GetByIdAsync(1);
await _repository.CreateAsync(dto, userId);
await _repository.UpdateAsync(1, dto, userId);
await _repository.DeleteAsync(1, userId);
```

### Services
```csharp
// Inject in controllers
private readonly ILookupTypeService _service;
private readonly ILookupTypeValueService _valueService;

// Usage
var result = await _service.GetByIdAsync(1);
var created = await _service.CreateAsync(dto, userId);
var updated = await _service.UpdateAsync(1, dto, userId);
```

### Controllers
```csharp
// Inject services
private readonly ILookupTypeService _service;

// Endpoints automatically available
// GET /api/v1/lookuptypes
// POST /api/v1/lookuptypes
// PUT /api/v1/lookuptypes/{id}
// DELETE /api/v1/lookuptypes/{id}
```

---

## ?? Dependency Injection (Program.cs)

```csharp
// Already configured:
builder.Services.AddScoped<ILookupTypeRepository, LookupTypeRepository>();
builder.Services.AddScoped<ILookupTypeValueRepository, LookupTypeValueRepository>();
builder.Services.AddScoped<ILookupTypeService, LookupTypeService>();
builder.Services.AddScoped<ILookupTypeValueService, LookupTypeValueService>();
```

---

## ?? Request/Response Examples

### Create LookupType Request
```json
POST /api/v1/lookuptypes
{
  "lookupTypeName": "BLOOD_GROUP",
  "lookupTypeCode": "BLOOD_GROUP",
  "description": "Blood group types",
  "displayOrder": 1,
  "isSystem": false
}
```

### Create LookupType Response
```json
201 Created
{
  "success": true,
  "message": "LookupType created successfully",
  "data": {
    "id": 1,
    "globalId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### Get LookupType Response
```json
200 OK
{
  "lookupTypeMasterId": 1,
  "globalId": "550e8400-e29b-41d4-a716-446655440000",
  "lookupTypeName": "BLOOD_GROUP",
  "lookupTypeCode": "BLOOD_GROUP",
  "description": "Blood group types",
  "displayOrder": 1,
  "isSystem": false,
  "isActive": true,
  "createdOnUTC": "2024-01-15T10:30:00Z",
  "createdBy": 1
}
```

### Paginated Response
```json
200 OK
{
  "data": [
    { /* LookupType 1 */ },
    { /* LookupType 2 */ },
    ...
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Error Response
```json
400 Bad Request
{
  "success": false,
  "message": "LookupTypeName is required"
}
```

---

## ?? Testing via Swagger

1. Navigate to `https://localhost:5001/swagger`
2. Expand "LookupTypes" section
3. Click "Try it out" on any endpoint
4. Fill in parameters
5. Click "Execute"

---

## ?? Security Notes

? **SQL Injection Protected** - All queries use parameters  
? **Soft Deletes** - Records never permanently deleted  
? **Audit Trail** - CreatedBy, UpdatedBy, DeletedBy tracked  
? **System Records** - Can't delete system records  
? **Data Masking** - Sensitive data masked in logs  

---

## ?? Performance Tips

| Tip | Implementation |
|-----|-----------------|
| **Pagination** | Always use `pageNumber` and `pageSize` |
| **Filtering** | Use `isActive` parameter to filter |
| **Search** | Use `/search` endpoint for text search |
| **Caching** | 5-minute TTL automatically applied |
| **Batch Ops** | Create multiple values at once |

---

## ??? Common Tasks

### Add New LookupType
```csharp
POST /api/v1/lookuptypes
Content-Type: application/json

{
  "lookupTypeName": "MARITAL_STATUS",
  "lookupTypeCode": "MARITAL_STATUS",
  "description": "Marital status options",
  "displayOrder": 1
}
```

### Get All Active LookupTypes
```http
GET /api/v1/lookuptypes?isActive=true&pageSize=100
```

### Find Values for a Type
```http
GET /api/v1/lookuptypevalues/by-type-code/BLOOD_GROUP?isActive=true
```

### Search by Term
```http
GET /api/v1/lookuptypes/search?searchTerm=gender&pageNumber=1&pageSize=20
```

### Update Lookup Value
```csharp
PUT /api/v1/lookuptypevalues/5
Content-Type: application/json

{
  "lookupValueName": "O_POSITIVE",
  "description": "O Positive blood type",
  "displayOrder": 4,
  "isActive": true
}
```

---

## ?? Troubleshooting

### Issue: "Stored procedure not found"
**Solution:** Run SQL scripts first (`01_*.sql`, `02_*.sql`)

### Issue: "Connection timeout"
**Solution:** Check connection string in `appsettings.json`

### Issue: "Access denied on delete"
**Solution:** Check if record is marked `IsSystem = 1`

### Issue: "404 Not Found"
**Solution:** Verify ID exists and is not deleted (`IsDeleted = 0`)

---

## ?? Files Reference

| File | Purpose |
|------|---------|
| `Database/01_LookupTypeMaster_StoredProcedures.sql` | SQL procedures for LookupType |
| `Database/02_LookupTypeValueMaster_StoredProcedures.sql` | SQL procedures for LookupValue |
| `Data/Dtos/LookupDtos.cs` | DTOs for requests/responses |
| `Data/Repositories/ILookupRepository.cs` | Repository interfaces |
| `Data/Repositories/LookupRepository.cs` | Repository implementations |
| `Services/LookupService.cs` | Business logic services |
| `Controllers/LookupController.cs` | REST API controllers |

---

## ?? Next Steps

1. ? Run SQL scripts to create stored procedures
2. ? Build & verify no errors
3. ? Run application (`dotnet run`)
4. ? Test endpoints in Swagger UI
5. ? Integrate into your frontend
6. ? Deploy to production

**Everything is production-ready!** ??
