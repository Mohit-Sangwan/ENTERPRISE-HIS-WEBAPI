using ENTERPRISE_HIS_WEBAPI.Data.Dtos;
using ENTERPRISE_HIS_WEBAPI.Data.Repositories;
using Enterprise.DAL.V1.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ENTERPRISE_HIS_WEBAPI.Services
{
    /// <summary>
    /// Service interface for LookupType business logic
    /// </summary>
    public interface ILookupTypeService
    {
        Task<LookupTypeDto?> GetByIdAsync(int id);
        Task<LookupTypeDto?> GetByCodeAsync(string code);
        Task<PaginatedResponse<LookupTypeDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null);
        Task<PaginatedResponse<LookupTypeDto>> SearchAsync(string? searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<int> GetCountAsync(bool? isActive = null);
        Task<ApiResponse<(int Id, Guid GlobalId)>> CreateAsync(CreateLookupTypeDto request, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int id, UpdateLookupTypeDto request, int updatedBy);
        Task<ApiResponse<bool>> DeleteAsync(int id, int deletedBy);
    }

    /// <summary>
    /// Service interface for LookupTypeValue business logic
    /// </summary>
    public interface ILookupTypeValueService
    {
        Task<LookupTypeValueDto?> GetByIdAsync(int id);
        Task<IEnumerable<LookupTypeValueDto>> GetByTypeIdAsync(int typeId, bool? isActive = null);
        Task<IEnumerable<LookupTypeValueDetailDto>> GetByTypeCodeAsync(string typeCode, bool? isActive = null);
        Task<PaginatedResponse<LookupTypeValueDetailDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null);
        Task<PaginatedResponse<LookupTypeValueDetailDto>> SearchAsync(int? typeId = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 10);
        Task<int> GetCountAsync(int? typeId = null, bool? isActive = null);
        Task<ApiResponse<(int Id, Guid GlobalId)>> CreateAsync(CreateLookupTypeValueDto request, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int id, UpdateLookupTypeValueDto request, int updatedBy);
        Task<ApiResponse<bool>> DeleteAsync(int id, int deletedBy);
    }

    /// <summary>
    /// Service implementation for LookupType business logic
    /// </summary>
    public class LookupTypeService : ILookupTypeService
    {
        private readonly ILookupTypeRepository _repository;
        private readonly IDbLogger? _logger;

        public LookupTypeService(ILookupTypeRepository repository, IDbLogger? logger = null)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<LookupTypeDto?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<LookupTypeDto?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code cannot be empty", nameof(code));

            return await _repository.GetByCodeAsync(code);
        }

        public async Task<PaginatedResponse<LookupTypeDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null)
        {
            ValidatePagination(pageNumber, pageSize);
            return await _repository.GetAllAsync(pageNumber, pageSize, isActive);
        }

        public async Task<PaginatedResponse<LookupTypeDto>> SearchAsync(string? searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            ValidatePagination(pageNumber, pageSize);
            return await _repository.SearchAsync(searchTerm, pageNumber, pageSize);
        }

        public async Task<int> GetCountAsync(bool? isActive = null)
        {
            return await _repository.GetCountAsync(isActive);
        }

        public async Task<ApiResponse<(int Id, Guid GlobalId)>> CreateAsync(CreateLookupTypeDto request, int createdBy)
        {
            try
            {
                ValidateCreateLookupTypeDto(request);

                if (createdBy <= 0)
                    throw new ArgumentException("Invalid CreatedBy value", nameof(createdBy));

                var result = await _repository.CreateAsync(request, createdBy);
                return new ApiResponse<(int Id, Guid GlobalId)>
                {
                    Success = true,
                    Message = "LookupType created successfully",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating LookupType", new Dictionary<string, object?> { ["Exception"] = ex.Message });
                return new ApiResponse<(int Id, Guid GlobalId)>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, UpdateLookupTypeDto request, int updatedBy)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID", nameof(id));

                ValidateUpdateLookupTypeDto(request);

                if (updatedBy <= 0)
                    throw new ArgumentException("Invalid UpdatedBy value", nameof(updatedBy));

                var result = await _repository.UpdateAsync(id, request, updatedBy);
                return new ApiResponse<bool>
                {
                    Success = result,
                    Message = result ? "LookupType updated successfully" : "Failed to update LookupType"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating LookupType", new Dictionary<string, object?> { ["Exception"] = ex.Message });
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, int deletedBy)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID", nameof(id));

                if (deletedBy <= 0)
                    throw new ArgumentException("Invalid DeletedBy value", nameof(deletedBy));

                var result = await _repository.DeleteAsync(id, deletedBy);
                return new ApiResponse<bool>
                {
                    Success = result,
                    Message = result ? "LookupType deleted successfully" : "Failed to delete LookupType"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting LookupType", new Dictionary<string, object?> { ["Exception"] = ex.Message });
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private void ValidateCreateLookupTypeDto(CreateLookupTypeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LookupTypeName))
                throw new ArgumentException("LookupTypeName is required");

            if (string.IsNullOrWhiteSpace(dto.LookupTypeCode))
                throw new ArgumentException("LookupTypeCode is required");

            if (dto.DisplayOrder < 0)
                throw new ArgumentException("DisplayOrder must be non-negative");
        }

        private void ValidateUpdateLookupTypeDto(UpdateLookupTypeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LookupTypeName))
                throw new ArgumentException("LookupTypeName is required");

            if (dto.DisplayOrder < 0)
                throw new ArgumentException("DisplayOrder must be non-negative");
        }

        private void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("PageNumber must be at least 1", nameof(pageNumber));

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException("PageSize must be between 1 and 100", nameof(pageSize));
        }
    }

    /// <summary>
    /// Service implementation for LookupTypeValue business logic
    /// </summary>
    public class LookupTypeValueService : ILookupTypeValueService
    {
        private readonly ILookupTypeValueRepository _repository;
        private readonly ILookupTypeRepository _lookupTypeRepository;
        private readonly IDbLogger? _logger;

        public LookupTypeValueService(
            ILookupTypeValueRepository repository,
            ILookupTypeRepository lookupTypeRepository,
            IDbLogger? logger = null)
        {
            _repository = repository;
            _lookupTypeRepository = lookupTypeRepository;
            _logger = logger;
        }

        public async Task<LookupTypeValueDto?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<LookupTypeValueDto>> GetByTypeIdAsync(int typeId, bool? isActive = null)
        {
            if (typeId <= 0)
                throw new ArgumentException("Invalid Type ID", nameof(typeId));

            return await _repository.GetByTypeIdAsync(typeId, isActive);
        }

        public async Task<IEnumerable<LookupTypeValueDetailDto>> GetByTypeCodeAsync(string typeCode, bool? isActive = null)
        {
            if (string.IsNullOrWhiteSpace(typeCode))
                throw new ArgumentException("Type Code cannot be empty", nameof(typeCode));

            return await _repository.GetByTypeCodeAsync(typeCode, isActive);
        }

        public async Task<PaginatedResponse<LookupTypeValueDetailDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null)
        {
            ValidatePagination(pageNumber, pageSize);
            return await _repository.GetAllAsync(pageNumber, pageSize, isActive);
        }

        public async Task<PaginatedResponse<LookupTypeValueDetailDto>> SearchAsync(int? typeId = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            ValidatePagination(pageNumber, pageSize);
            return await _repository.SearchAsync(typeId, searchTerm, pageNumber, pageSize);
        }

        public async Task<int> GetCountAsync(int? typeId = null, bool? isActive = null)
        {
            return await _repository.GetCountAsync(typeId, isActive);
        }

        public async Task<ApiResponse<(int Id, Guid GlobalId)>> CreateAsync(CreateLookupTypeValueDto request, int createdBy)
        {
            try
            {
                ValidateCreateLookupTypeValueDto(request);

                if (createdBy <= 0)
                    throw new ArgumentException("Invalid CreatedBy value", nameof(createdBy));

                // Verify parent type exists
                var parentType = await _lookupTypeRepository.GetByIdAsync(request.LookupTypeMasterId);
                if (parentType == null)
                    throw new ArgumentException("Parent LookupType not found", nameof(request.LookupTypeMasterId));

                var result = await _repository.CreateAsync(request, createdBy);
                return new ApiResponse<(int Id, Guid GlobalId)>
                {
                    Success = true,
                    Message = "LookupTypeValue created successfully",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating LookupTypeValue", new Dictionary<string, object?> { ["Exception"] = ex.Message });
                return new ApiResponse<(int Id, Guid GlobalId)>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, UpdateLookupTypeValueDto request, int updatedBy)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID", nameof(id));

                ValidateUpdateLookupTypeValueDto(request);

                if (updatedBy <= 0)
                    throw new ArgumentException("Invalid UpdatedBy value", nameof(updatedBy));

                var result = await _repository.UpdateAsync(id, request, updatedBy);
                return new ApiResponse<bool>
                {
                    Success = result,
                    Message = result ? "LookupTypeValue updated successfully" : "Failed to update LookupTypeValue"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating LookupTypeValue", new Dictionary<string, object?> { ["Exception"] = ex.Message });
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, int deletedBy)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID", nameof(id));

                if (deletedBy <= 0)
                    throw new ArgumentException("Invalid DeletedBy value", nameof(deletedBy));

                var result = await _repository.DeleteAsync(id, deletedBy);
                return new ApiResponse<bool>
                {
                    Success = result,
                    Message = result ? "LookupTypeValue deleted successfully" : "Failed to delete LookupTypeValue"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting LookupTypeValue", new Dictionary<string, object?> { ["Exception"] = ex.Message });
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private void ValidateCreateLookupTypeValueDto(CreateLookupTypeValueDto dto)
        {
            if (dto.LookupTypeMasterId <= 0)
                throw new ArgumentException("LookupTypeMasterId is required");

            if (string.IsNullOrWhiteSpace(dto.LookupValueName))
                throw new ArgumentException("LookupValueName is required");

            if (string.IsNullOrWhiteSpace(dto.LookupValueCode))
                throw new ArgumentException("LookupValueCode is required");

            if (dto.DisplayOrder < 0)
                throw new ArgumentException("DisplayOrder must be non-negative");
        }

        private void ValidateUpdateLookupTypeValueDto(UpdateLookupTypeValueDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LookupValueName))
                throw new ArgumentException("LookupValueName is required");

            if (dto.DisplayOrder < 0)
                throw new ArgumentException("DisplayOrder must be non-negative");
        }

        private void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("PageNumber must be at least 1", nameof(pageNumber));

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException("PageSize must be between 1 and 100", nameof(pageSize));
        }
    }
}
