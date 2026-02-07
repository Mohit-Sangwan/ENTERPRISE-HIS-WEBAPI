using ENTERPRISE_HIS_WEBAPI.Data.Dtos;
using Enterprise.DAL.V1.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ENTERPRISE_HIS_WEBAPI.Data.Repositories
{
    /// <summary>
    /// Repository interface for LookupTypeMaster CRUD operations
    /// </summary>
    public interface ILookupTypeRepository
    {
        Task<LookupTypeDto?> GetByIdAsync(int lookupTypeMasterId);
        Task<LookupTypeDto?> GetByCodeAsync(string lookupTypeCode);
        Task<PaginatedResponse<LookupTypeDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null);
        Task<PaginatedResponse<LookupTypeDto>> SearchAsync(string? searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<int> GetCountAsync(bool? isActive = null);
        Task<(int Id, Guid GlobalId)> CreateAsync(CreateLookupTypeDto request, int createdBy);
        Task<bool> UpdateAsync(int id, UpdateLookupTypeDto request, int updatedBy);
        Task<bool> DeleteAsync(int id, int deletedBy);
    }

    /// <summary>
    /// Repository interface for LookupTypeValueMaster CRUD operations
    /// </summary>
    public interface ILookupTypeValueRepository
    {
        Task<LookupTypeValueDto?> GetByIdAsync(int lookupTypeValueId);
        Task<IEnumerable<LookupTypeValueDto>> GetByTypeIdAsync(int lookupTypeMasterId, bool? isActive = null);
        Task<IEnumerable<LookupTypeValueDetailDto>> GetByTypeCodeAsync(string lookupTypeCode, bool? isActive = null);
        Task<PaginatedResponse<LookupTypeValueDetailDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null);
        Task<PaginatedResponse<LookupTypeValueDetailDto>> SearchAsync(int? lookupTypeMasterId = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 10);
        Task<int> GetCountAsync(int? lookupTypeMasterId = null, bool? isActive = null);
        Task<(int Id, Guid GlobalId)> CreateAsync(CreateLookupTypeValueDto request, int createdBy);
        Task<bool> UpdateAsync(int id, UpdateLookupTypeValueDto request, int updatedBy);
        Task<bool> DeleteAsync(int id, int deletedBy);
    }
}
