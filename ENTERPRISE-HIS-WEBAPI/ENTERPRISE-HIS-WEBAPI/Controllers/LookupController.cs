using ENTERPRISE_HIS_WEBAPI.Authorization;
using ENTERPRISE_HIS_WEBAPI.Data.Dtos;
using ENTERPRISE_HIS_WEBAPI.Services;
using ENTERPRISE_HIS_WEBAPI.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ENTERPRISE_HIS_WEBAPI.Controllers
{
    /// <summary>
    /// API Controller for LookupType CRUD operations
    /// Authorization: Enterprise Authorization Middleware handles all permissions
    /// No attributes needed - permissions auto-resolved from HTTP method + route
    /// Format: GET ? Lookups.LookupType.View | POST ? Lookups.LookupType.Create | etc.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class LookupTypesController : ControllerBase
    {
        private readonly ILookupTypeService _service;
        private readonly ILogger<LookupTypesController> _logger;

        public LookupTypesController(ILookupTypeService service, ILogger<LookupTypesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get LookupType by ID
        /// Permission: Lookups.LookupType.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LookupTypeDto>> GetById(int id)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching LookupType {LookupTypeId}", userId, id);

                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { error = "LookupType not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching LookupType {id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get LookupType by Code
        /// Permission: Lookups.LookupType.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("code/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LookupTypeDto>> GetByCode(string code)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching LookupType by code {Code}", userId, code);

                var result = await _service.GetByCodeAsync(code);
                if (result == null)
                    return NotFound(new { error = "LookupType not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching LookupType by code {code}", code);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all LookupTypes with pagination
        /// Permission: Lookups.LookupType.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginatedResponse<LookupTypeDto>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching all LookupTypes (Page {Page}, Size {Size})", userId, pageNumber, pageSize);

                var result = await _service.GetAllAsync(pageNumber, pageSize, isActive);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid pagination parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching LookupTypes");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Search LookupTypes
        /// Permission: Lookups.LookupType.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginatedResponse<LookupTypeDto>>> Search(
            [FromQuery] string? searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} searching LookupTypes with term: {SearchTerm}", userId, searchTerm);

                var result = await _service.SearchAsync(searchTerm, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid search parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching LookupTypes");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get LookupType count
        /// Permission: Lookups.LookupType.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<int>> GetCount([FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching LookupType count", userId);

                var result = await _service.GetCountAsync(isActive);
                return Ok(new { count = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LookupType count");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new LookupType
        /// Permission: Lookups.LookupType.Create (auto-resolved from POST method)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<(int Id, Guid GlobalId)>>> Create([FromBody] CreateLookupTypeDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogInformation("User {UserId} ({UserName}) creating LookupType", userId, userName);

                var response = await _service.CreateAsync(request, userId);
                
                if (!response.Success)
                    return BadRequest(response);

                _logger.LogInformation("User {UserId} successfully created LookupType {LookupTypeId}", userId, response.Data.Id);
                return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating LookupType");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing LookupType
        /// Permission: Lookups.LookupType.Edit (auto-resolved from PUT method)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] UpdateLookupTypeDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogInformation("User {UserId} ({UserName}) updating LookupType {LookupTypeId}", userId, userName, id);

                var lookupType = await _service.GetByIdAsync(id);
                if (lookupType == null)
                    return NotFound(new { error = "LookupType not found" });

                var response = await _service.UpdateAsync(id, request, userId);
                
                _logger.LogInformation("User {UserId} successfully updated LookupType {LookupTypeId}", userId, id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LookupType {id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a LookupType
        /// Permission: Lookups.LookupType.Delete (auto-resolved from DELETE method)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogWarning("User {UserId} ({UserName}) deleting LookupType {LookupTypeId}", userId, userName, id);

                var lookupType = await _service.GetByIdAsync(id);
                if (lookupType == null)
                    return NotFound(new { error = "LookupType not found" });

                var response = await _service.DeleteAsync(id, userId);
                
                _logger.LogWarning("User {UserId} successfully deleted LookupType {LookupTypeId}", userId, id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting LookupType {id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// API Controller for LookupTypeValue CRUD operations
    /// Authorization: Enterprise Authorization Middleware handles all permissions
    /// No attributes needed - permissions auto-resolved from HTTP method + route
    /// Format: GET ? Lookups.LookupTypeValue.View | POST ? Lookups.LookupTypeValue.Create | etc.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class LookupTypeValuesController : ControllerBase
    {
        private readonly ILookupTypeValueService _service;
        private readonly ILogger<LookupTypeValuesController> _logger;

        public LookupTypeValuesController(ILookupTypeValueService service, ILogger<LookupTypeValuesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get LookupTypeValue by ID
        /// Permission: Lookups.LookupTypeValue.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LookupTypeValueDto>> GetById(int id)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching LookupTypeValue {LookupTypeValueId}", userId, id);

                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { error = "LookupTypeValue not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching LookupTypeValue {id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get LookupTypeValues by Type ID
        /// Permission: Lookups.LookupTypeValue.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("by-type/{typeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<LookupTypeValueDto>>> GetByTypeId(
            int typeId,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching LookupTypeValues by type {TypeId}", userId, typeId);

                var result = await _service.GetByTypeIdAsync(typeId, isActive);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid type ID");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching LookupTypeValues by type ID");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get LookupTypeValues by Type Code
        /// Permission: Lookups.LookupTypeValue.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("by-type-code/{typeCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<LookupTypeValueDetailDto>>> GetByTypeCode(
            string typeCode,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching LookupTypeValues by code {TypeCode}", userId, typeCode);

                var result = await _service.GetByTypeCodeAsync(typeCode, isActive);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid type code");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching LookupTypeValues by type code");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all LookupTypeValues with pagination
        /// Permission: Lookups.LookupTypeValue.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginatedResponse<LookupTypeValueDetailDto>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching all LookupTypeValues (Page {Page}, Size {Size})", userId, pageNumber, pageSize);

                var result = await _service.GetAllAsync(pageNumber, pageSize, isActive);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid pagination parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching LookupTypeValues");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Search LookupTypeValues
        /// Permission: Lookups.LookupTypeValue.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginatedResponse<LookupTypeValueDetailDto>>> Search(
            [FromQuery] int? typeId,
            [FromQuery] string? searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} searching LookupTypeValues with term: {SearchTerm}", userId, searchTerm);

                var result = await _service.SearchAsync(typeId, searchTerm, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid search parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching LookupTypeValues");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get LookupTypeValue count
        /// Permission: Lookups.LookupTypeValue.View (auto-resolved from GET method)
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<int>> GetCount(
            [FromQuery] int? typeId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} fetching LookupTypeValue count", userId);

                var result = await _service.GetCountAsync(typeId, isActive);
                return Ok(new { count = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LookupTypeValue count");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new LookupTypeValue
        /// Permission: Lookups.LookupTypeValue.Create (auto-resolved from POST method)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<(int Id, Guid GlobalId)>>> Create([FromBody] CreateLookupTypeValueDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogInformation("User {UserId} ({UserName}) creating LookupTypeValue", userId, userName);

                var response = await _service.CreateAsync(request, userId);
                
                if (!response.Success)
                    return BadRequest(response);

                _logger.LogInformation("User {UserId} successfully created LookupTypeValue {LookupTypeValueId}", userId, response.Data.Id);
                return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating LookupTypeValue");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing LookupTypeValue
        /// Permission: Lookups.LookupTypeValue.Edit (auto-resolved from PUT method)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] UpdateLookupTypeValueDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogInformation("User {UserId} ({UserName}) updating LookupTypeValue {LookupTypeValueId}", userId, userName, id);

                var lookupValue = await _service.GetByIdAsync(id);
                if (lookupValue == null)
                    return NotFound(new { error = "LookupTypeValue not found" });

                var response = await _service.UpdateAsync(id, request, userId);
                
                _logger.LogInformation("User {UserId} successfully updated LookupTypeValue {LookupTypeValueId}", userId, id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LookupTypeValue {id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a LookupTypeValue
        /// Permission: Lookups.LookupTypeValue.Delete (auto-resolved from DELETE method)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogWarning("User {UserId} ({UserName}) deleting LookupTypeValue {LookupTypeValueId}", userId, userName, id);

                var lookupValue = await _service.GetByIdAsync(id);
                if (lookupValue == null)
                    return NotFound(new { error = "LookupTypeValue not found" });

                var response = await _service.DeleteAsync(id, userId);
                
                _logger.LogWarning("User {UserId} successfully deleted LookupTypeValue {LookupTypeValueId}", userId, id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting LookupTypeValue {id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}
