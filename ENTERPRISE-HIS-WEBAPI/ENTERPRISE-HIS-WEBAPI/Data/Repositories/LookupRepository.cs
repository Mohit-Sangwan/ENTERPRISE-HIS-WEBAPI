using ENTERPRISE_HIS_WEBAPI.Data.Dtos;
using Enterprise.DAL.V1.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ENTERPRISE_HIS_WEBAPI.Data.Repositories
{
    /// <summary>
    /// Repository for LookupTypeMaster CRUD operations using Enterprise.DAL.V1
    /// </summary>
    public class LookupTypeRepository : ILookupTypeRepository
    {
        private readonly ISqlServerDal _dal;
        private readonly IDbLogger? _logger;

        public LookupTypeRepository(ISqlServerDal dal, IDbLogger? logger = null)
        {
            _dal = dal;
            _logger = logger;
        }

        public async Task<LookupTypeDto?> GetByIdAsync(int lookupTypeMasterId)
        {
            _logger?.LogInfo("Fetching LookupTypeMaster by ID", new Dictionary<string, object?> { ["LookupTypeMasterId"] = lookupTypeMasterId });
            
            var result = await _dal.QueryAsync<LookupTypeDto>(
                "lookup.SP_LookupTypeMaster_GetById",
                MapLookupTypeRow,
                new[] { DbParam.Input("@LookupTypeMasterId", lookupTypeMasterId) },
                isStoredProc: true);

            return result.Success ? result.Data.FirstOrDefault() : null;
        }

        public async Task<LookupTypeDto?> GetByCodeAsync(string lookupTypeCode)
        {
            _logger?.LogInfo("Fetching LookupTypeMaster by Code", new Dictionary<string, object?> { ["LookupTypeCode"] = lookupTypeCode });
            
            var result = await _dal.QueryAsync<LookupTypeDto>(
                "lookup.SP_LookupTypeMaster_GetByCode",
                MapLookupTypeRow,
                new[] { DbParam.Input("@LookupTypeCode", lookupTypeCode) },
                isStoredProc: true);

            return result.Success ? result.Data.FirstOrDefault() : null;
        }

        public async Task<PaginatedResponse<LookupTypeDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null)
        {
            _logger?.LogInfo("Fetching all LookupTypesMaster", new Dictionary<string, object?> { ["PageNumber"] = pageNumber, ["PageSize"] = pageSize });
            
            var result = await _dal.QueryAsync<LookupTypeDto>(
                "lookup.SP_LookupTypeMaster_GetAll",
                MapLookupTypeRow,
                new[]
                {
                    DbParam.Input("@PageNumber", pageNumber),
                    DbParam.Input("@PageSize", pageSize),
                    DbParam.Input("@IsActive", isActive)
                },
                isStoredProc: true);

            var totalCount = await GetCountAsync(isActive);

            return new PaginatedResponse<LookupTypeDto>
            {
                Data = result.Success ? result.Data.ToList() : new List<LookupTypeDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResponse<LookupTypeDto>> SearchAsync(string? searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            _logger?.LogInfo("Searching LookupTypesMaster", new Dictionary<string, object?> { ["SearchTerm"] = searchTerm, ["PageNumber"] = pageNumber });
            
            var result = await _dal.QueryAsync<LookupTypeDto>(
                "lookup.SP_LookupTypeMaster_Search",
                MapLookupTypeRow,
                new[]
                {
                    DbParam.Input("@SearchTerm", searchTerm),
                    DbParam.Input("@PageNumber", pageNumber),
                    DbParam.Input("@PageSize", pageSize)
                },
                isStoredProc: true);

            var totalCount = await GetCountAsync();

            return new PaginatedResponse<LookupTypeDto>
            {
                Data = result.Success ? result.Data.ToList() : new List<LookupTypeDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<int> GetCountAsync(bool? isActive = null)
        {
            var result = await _dal.ExecuteScalarAsync<int>(
                "lookup.SP_LookupTypeMaster_GetCount",
                new[] { DbParam.Input("@IsActive", isActive) },
                isStoredProc: true);

            return result.Success ? result.Data : 0;
        }

        public async Task<(int Id, Guid GlobalId)> CreateAsync(CreateLookupTypeDto request, int createdBy)
        {
            _logger?.LogInfo("Creating new LookupTypeMaster", new Dictionary<string, object?> { ["LookupTypeCode"] = request.LookupTypeCode });
            
            var idOutput = DbParam.Output("@LookupTypeMasterId", System.Data.SqlDbType.Int);
            var globalIdOutput = DbParam.Output("@GlobalId", System.Data.SqlDbType.UniqueIdentifier);

            var result = await _dal.ExecuteNonQueryAsync(
                "lookup.SP_LookupTypeMaster_Create",
                new[]
                {
                    DbParam.Input("@LookupTypeName", request.LookupTypeName),
                    DbParam.Input("@LookupTypeCode", request.LookupTypeCode),
                    DbParam.Input("@Description", request.Description),
                    DbParam.Input("@DisplayOrder", request.DisplayOrder),
                    DbParam.Input("@IsSystem", request.IsSystem),
                    DbParam.Input("@CreatedBy", createdBy),
                    idOutput,
                    globalIdOutput
                },
                isStoredProc: true);

            if (!result.Success)
                throw new Exception($"Failed to create LookupTypeMaster: {result.Message}");

            int id = (int)(idOutput.Value ?? 0);
            Guid globalId = (Guid)(globalIdOutput.Value ?? Guid.Empty);

            _logger?.LogInfo("LookupTypeMaster created successfully", new Dictionary<string, object?> { ["Id"] = id });

            return (id, globalId);
        }

        public async Task<bool> UpdateAsync(int id, UpdateLookupTypeDto request, int updatedBy)
        {
            _logger?.LogInfo("Updating LookupTypeMaster", new Dictionary<string, object?> { ["Id"] = id });
            
            var result = await _dal.ExecuteNonQueryAsync(
                "lookup.SP_LookupTypeMaster_Update",
                new[]
                {
                    DbParam.Input("@LookupTypeMasterId", id),
                    DbParam.Input("@LookupTypeName", request.LookupTypeName),
                    DbParam.Input("@Description", request.Description),
                    DbParam.Input("@DisplayOrder", request.DisplayOrder),
                    DbParam.Input("@IsActive", request.IsActive),
                    DbParam.Input("@UpdatedBy", updatedBy)
                },
                isStoredProc: true);

            _logger?.LogInfo("LookupTypeMaster updated successfully", new Dictionary<string, object?> { ["Id"] = id });

            return result.Success && result.Data > 0;
        }

        public async Task<bool> DeleteAsync(int id, int deletedBy)
        {
            _logger?.LogInfo("Deleting LookupTypeMaster", new Dictionary<string, object?> { ["Id"] = id });
            
            var result = await _dal.ExecuteNonQueryAsync(
                "lookup.SP_LookupTypeMaster_Delete",
                new[]
                {
                    DbParam.Input("@LookupTypeMasterId", id),
                    DbParam.Input("@DeletedBy", deletedBy)
                },
                isStoredProc: true);

            _logger?.LogInfo("LookupTypeMaster deleted successfully", new Dictionary<string, object?> { ["Id"] = id });

            return result.Success && result.Data > 0;
        }

        private LookupTypeDto MapLookupTypeRow(System.Data.DataRow row)
        {
            return new LookupTypeDto
            {
                LookupTypeMasterId = (int)row["LookupTypeMasterId"],
                GlobalId = (Guid)row["GlobalId"],
                LookupTypeName = row["LookupTypeName"].ToString() ?? string.Empty,
                LookupTypeCode = row["LookupTypeCode"].ToString() ?? string.Empty,
                Description = row["Description"] != System.DBNull.Value ? row["Description"].ToString() : null,
                DisplayOrder = (int)row["DisplayOrder"],
                IsSystem = (bool)row["IsSystem"],
                IsActive = (bool)row["IsActive"],
                CreatedOnUTC = (DateTime)row["CreatedOnUTC"],
                UpdatedOnUTC = row["UpdatedOnUTC"] != System.DBNull.Value ? (DateTime)row["UpdatedOnUTC"] : null,
                CreatedBy = (int)row["CreatedBy"],
                UpdatedBy = row["UpdatedBy"] != System.DBNull.Value ? (int)row["UpdatedBy"] : null
            };
        }
    }

    /// <summary>
    /// Repository for LookupTypeValueMaster CRUD operations using Enterprise.DAL.V1
    /// </summary>
    public class LookupTypeValueRepository : ILookupTypeValueRepository
    {
        private readonly ISqlServerDal _dal;
        private readonly IDbLogger? _logger;

        public LookupTypeValueRepository(ISqlServerDal dal, IDbLogger? logger = null)
        {
            _dal = dal;
            _logger = logger;
        }

        public async Task<LookupTypeValueDto?> GetByIdAsync(int lookupTypeValueId)
        {
            _logger?.LogInfo("Fetching LookupTypeValueMaster by ID", new Dictionary<string, object?> { ["LookupTypeValueId"] = lookupTypeValueId });
            
            var result = await _dal.QueryAsync<LookupTypeValueDto>(
                "lookup.SP_LookupTypeValueMaster_GetById",
                MapLookupTypeValueRow,
                new[] { DbParam.Input("@LookupTypeValueId", lookupTypeValueId) },
                isStoredProc: true);

            return result.Success ? result.Data.FirstOrDefault() : null;
        }

        public async Task<IEnumerable<LookupTypeValueDto>> GetByTypeIdAsync(int lookupTypeMasterId, bool? isActive = null)
        {
            _logger?.LogInfo("Fetching LookupTypeValues by Type ID", new Dictionary<string, object?> { ["LookupTypeMasterId"] = lookupTypeMasterId });
            
            var result = await _dal.QueryAsync<LookupTypeValueDto>(
                "lookup.SP_LookupTypeValueMaster_GetByTypeId",
                MapLookupTypeValueRow,
                new[]
                {
                    DbParam.Input("@LookupTypeMasterId", lookupTypeMasterId),
                    DbParam.Input("@IsActive", isActive)
                },
                isStoredProc: true);

            return result.Success ? result.Data : new List<LookupTypeValueDto>();
        }

        public async Task<IEnumerable<LookupTypeValueDetailDto>> GetByTypeCodeAsync(string lookupTypeCode, bool? isActive = null)
        {
            _logger?.LogInfo("Fetching LookupTypeValues by Type Code", new Dictionary<string, object?> { ["LookupTypeCode"] = lookupTypeCode });
            
            var result = await _dal.QueryAsync<LookupTypeValueDetailDto>(
                "lookup.SP_LookupTypeValueMaster_GetByTypeCode",
                MapLookupTypeValueDetailRow,
                new[]
                {
                    DbParam.Input("@LookupTypeCode", lookupTypeCode),
                    DbParam.Input("@IsActive", isActive)
                },
                isStoredProc: true);

            return result.Success ? result.Data : new List<LookupTypeValueDetailDto>();
        }

        public async Task<PaginatedResponse<LookupTypeValueDetailDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null)
        {
            _logger?.LogInfo("Fetching all LookupTypeValuesMaster", new Dictionary<string, object?> { ["PageNumber"] = pageNumber });
            
            var result = await _dal.QueryAsync<LookupTypeValueDetailDto>(
                "lookup.SP_LookupTypeValueMaster_GetAll",
                MapLookupTypeValueDetailRow,
                new[]
                {
                    DbParam.Input("@PageNumber", pageNumber),
                    DbParam.Input("@PageSize", pageSize),
                    DbParam.Input("@IsActive", isActive)
                },
                isStoredProc: true);

            var totalCount = await GetCountAsync(null, isActive);

            return new PaginatedResponse<LookupTypeValueDetailDto>
            {
                Data = result.Success ? result.Data.ToList() : new List<LookupTypeValueDetailDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResponse<LookupTypeValueDetailDto>> SearchAsync(int? lookupTypeMasterId = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            _logger?.LogInfo("Searching LookupTypeValuesMaster", new Dictionary<string, object?> { ["SearchTerm"] = searchTerm });
            
            var result = await _dal.QueryAsync<LookupTypeValueDetailDto>(
                "lookup.SP_LookupTypeValueMaster_Search",
                MapLookupTypeValueDetailRow,
                new[]
                {
                    DbParam.Input("@LookupTypeMasterId", lookupTypeMasterId),
                    DbParam.Input("@SearchTerm", searchTerm),
                    DbParam.Input("@PageNumber", pageNumber),
                    DbParam.Input("@PageSize", pageSize)
                },
                isStoredProc: true);

            var totalCount = await GetCountAsync(lookupTypeMasterId);

            return new PaginatedResponse<LookupTypeValueDetailDto>
            {
                Data = result.Success ? result.Data.ToList() : new List<LookupTypeValueDetailDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<int> GetCountAsync(int? lookupTypeMasterId = null, bool? isActive = null)
        {
            var result = await _dal.ExecuteScalarAsync<int>(
                "lookup.SP_LookupTypeValueMaster_GetCount",
                new[]
                {
                    DbParam.Input("@LookupTypeMasterId", lookupTypeMasterId),
                    DbParam.Input("@IsActive", isActive)
                },
                isStoredProc: true);

            return result.Success ? result.Data : 0;
        }

        public async Task<(int Id, Guid GlobalId)> CreateAsync(CreateLookupTypeValueDto request, int createdBy)
        {
            _logger?.LogInfo("Creating new LookupTypeValueMaster", new Dictionary<string, object?> { ["LookupValueCode"] = request.LookupValueCode });
            
            var idOutput = DbParam.Output("@LookupTypeValueId", System.Data.SqlDbType.Int);
            var globalIdOutput = DbParam.Output("@GlobalId", System.Data.SqlDbType.UniqueIdentifier);

            var result = await _dal.ExecuteNonQueryAsync(
                "lookup.SP_LookupTypeValueMaster_Create",
                new[]
                {
                    DbParam.Input("@LookupTypeMasterId", request.LookupTypeMasterId),
                    DbParam.Input("@LookupValueName", request.LookupValueName),
                    DbParam.Input("@LookupValueCode", request.LookupValueCode),
                    DbParam.Input("@Description", request.Description),
                    DbParam.Input("@DisplayOrder", request.DisplayOrder),
                    DbParam.Input("@IsSystem", request.IsSystem),
                    DbParam.Input("@CreatedBy", createdBy),
                    idOutput,
                    globalIdOutput
                },
                isStoredProc: true);

            if (!result.Success)
                throw new Exception($"Failed to create LookupTypeValueMaster: {result.Message}");

            int id = (int)(idOutput.Value ?? 0);
            Guid globalId = (Guid)(globalIdOutput.Value ?? Guid.Empty);

            _logger?.LogInfo("LookupTypeValueMaster created successfully", new Dictionary<string, object?> { ["Id"] = id });

            return (id, globalId);
        }

        public async Task<bool> UpdateAsync(int id, UpdateLookupTypeValueDto request, int updatedBy)
        {
            _logger?.LogInfo("Updating LookupTypeValueMaster", new Dictionary<string, object?> { ["Id"] = id });
            
            var result = await _dal.ExecuteNonQueryAsync(
                "lookup.SP_LookupTypeValueMaster_Update",
                new[]
                {
                    DbParam.Input("@LookupTypeValueId", id),
                    DbParam.Input("@LookupValueName", request.LookupValueName),
                    DbParam.Input("@Description", request.Description),
                    DbParam.Input("@DisplayOrder", request.DisplayOrder),
                    DbParam.Input("@IsActive", request.IsActive),
                    DbParam.Input("@UpdatedBy", updatedBy)
                },
                isStoredProc: true);

            _logger?.LogInfo("LookupTypeValueMaster updated successfully", new Dictionary<string, object?> { ["Id"] = id });

            return result.Success && result.Data > 0;
        }

        public async Task<bool> DeleteAsync(int id, int deletedBy)
        {
            _logger?.LogInfo("Deleting LookupTypeValueMaster", new Dictionary<string, object?> { ["Id"] = id });
            
            var result = await _dal.ExecuteNonQueryAsync(
                "lookup.SP_LookupTypeValueMaster_Delete",
                new[]
                {
                    DbParam.Input("@LookupTypeValueId", id),
                    DbParam.Input("@DeletedBy", deletedBy)
                },
                isStoredProc: true);

            _logger?.LogInfo("LookupTypeValueMaster deleted successfully", new Dictionary<string, object?> { ["Id"] = id });

            return result.Success && result.Data > 0;
        }

        private LookupTypeValueDto MapLookupTypeValueRow(System.Data.DataRow row)
        {
            return new LookupTypeValueDto
            {
                LookupTypeValueId = (int)row["LookupTypeValueId"],
                LookupTypeMasterId = (int)row["LookupTypeMasterId"],
                GlobalId = (Guid)row["GlobalId"],
                LookupValueName = row["LookupValueName"].ToString() ?? string.Empty,
                LookupValueCode = row["LookupValueCode"].ToString() ?? string.Empty,
                Description = row["Description"] != System.DBNull.Value ? row["Description"].ToString() : null,
                DisplayOrder = (int)row["DisplayOrder"],
                IsSystem = (bool)row["IsSystem"],
                IsActive = (bool)row["IsActive"],
                CreatedOnUTC = (DateTime)row["CreatedOnUTC"],
                UpdatedOnUTC = row["UpdatedOnUTC"] != System.DBNull.Value ? (DateTime)row["UpdatedOnUTC"] : null,
                CreatedBy = (int)row["CreatedBy"],
                UpdatedBy = row["UpdatedBy"] != System.DBNull.Value ? (int)row["UpdatedBy"] : null
            };
        }

        private LookupTypeValueDetailDto MapLookupTypeValueDetailRow(System.Data.DataRow row)
        {
            return new LookupTypeValueDetailDto
            {
                LookupTypeValueId = (int)row["LookupTypeValueId"],
                LookupTypeMasterId = (int)row["LookupTypeMasterId"],
                GlobalId = (Guid)row["GlobalId"],
                LookupValueName = row["LookupValueName"].ToString() ?? string.Empty,
                LookupValueCode = row["LookupValueCode"].ToString() ?? string.Empty,
                Description = row["Description"] != System.DBNull.Value ? row["Description"].ToString() : null,
                DisplayOrder = (int)row["DisplayOrder"],
                IsSystem = (bool)row["IsSystem"],
                IsActive = (bool)row["IsActive"],
                CreatedOnUTC = (DateTime)row["CreatedOnUTC"],
                UpdatedOnUTC = row["UpdatedOnUTC"] != System.DBNull.Value ? (DateTime)row["UpdatedOnUTC"] : null,
                CreatedBy = (int)row["CreatedBy"],
                UpdatedBy = row["UpdatedBy"] != System.DBNull.Value ? (int)row["UpdatedBy"] : null,
                LookupTypeName = row["LookupTypeName"].ToString() ?? string.Empty,
                LookupTypeCode = row["LookupTypeCode"].ToString() ?? string.Empty
            };
        }
    }
}
