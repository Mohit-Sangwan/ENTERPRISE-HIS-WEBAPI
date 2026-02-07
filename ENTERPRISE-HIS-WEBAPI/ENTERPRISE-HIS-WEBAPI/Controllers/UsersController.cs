using Microsoft.AspNetCore.Mvc;
using ENTERPRISE_HIS_WEBAPI.Data.Dtos;
using ENTERPRISE_HIS_WEBAPI.Services;

namespace ENTERPRISE_HIS_WEBAPI.Controllers
{
    /// <summary>
    /// User Management API Controller
    /// Authorization: Enterprise Authorization Middleware handles all permissions
    /// Permissions auto-resolved: POST?Create, GET?View, PUT?Edit, DELETE?Delete
    /// No [Authorize] attributes needed - middleware derives everything
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new user
        /// Permission: Administration.User.Create (auto-resolved from POST)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                _logger.LogInformation("Creating new user: {Username}", createUserDto.Username);

                var (success, message, user) = await _userService.CreateUserAsync(createUserDto);

                if (!success)
                {
                    _logger.LogWarning("Failed to create user: {Message}", message);
                    return BadRequest(new ErrorResponse { Message = message });
                }

                _logger.LogInformation("User created successfully: {UserId}", user?.UserId);
                return CreatedAtAction(nameof(GetUserById), new { id = user?.UserId }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while creating the user" });
            }
        }

        /// <summary>
        /// Get user by ID
        /// Permission: Administration.User.View (auto-resolved from GET)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDto>> GetUserById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching user: {UserId}", id);

                var (success, message, user) = await _userService.GetUserByIdAsync(id);

                if (!success)
                {
                    _logger.LogWarning("User not found: {UserId}", id);
                    return NotFound(new ErrorResponse { Message = message });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while fetching the user" });
            }
        }

        /// <summary>
        /// Get user by username
        /// Permission: Administration.User.View (auto-resolved from GET)
        /// </summary>
        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDto>> GetUserByUsername(string username)
        {
            try
            {
                _logger.LogInformation("Fetching user: {Username}", username);

                var (success, message, user) = await _userService.GetUserByUsernameAsync(username);

                if (!success)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return NotFound(new ErrorResponse { Message = message });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by username: {Username}", username);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while fetching the user" });
            }
        }

        /// <summary>
        /// Get all users with pagination
        /// Permission: Administration.User.View (auto-resolved from GET)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedUserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedUserResponseDto>> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Message = "Page number and page size must be greater than 0" 
                    });
                }

                _logger.LogInformation("Fetching users - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

                var (success, message, result) = await _userService.GetUsersAsync(pageNumber, pageSize);

                if (!success)
                {
                    return BadRequest(new ErrorResponse { Message = message });
                }

                Response.Headers.Add("X-Pagination-Page", pageNumber.ToString());
                Response.Headers.Add("X-Pagination-PageSize", pageSize.ToString());
                Response.Headers.Add("X-Pagination-TotalCount", result.TotalCount.ToString());
                Response.Headers.Add("X-Pagination-TotalPages", result.TotalPages.ToString());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while fetching users" });
            }
        }

        /// <summary>
        /// Update user information
        /// Permission: Administration.User.Edit (auto-resolved from PUT)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SuccessResponse>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                _logger.LogInformation("Updating user: {UserId}", id);

                var (success, message) = await _userService.UpdateUserAsync(id, updateUserDto);

                if (!success)
                {
                    _logger.LogWarning("Failed to update user {UserId}: {Message}", id, message);
                    if (message.Contains("not found"))
                        return NotFound(new ErrorResponse { Message = message });
                    return BadRequest(new ErrorResponse { Message = message });
                }

                _logger.LogInformation("User updated successfully: {UserId}", id);
                return Ok(new SuccessResponse { Message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while updating the user" });
            }
        }

        /// <summary>
        /// Delete user
        /// Permission: Administration.User.Delete (auto-resolved from DELETE)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SuccessResponse>> DeleteUser(int id)
        {
            try
            {
                _logger.LogInformation("Deleting user: {UserId}", id);

                var (success, message) = await _userService.DeleteUserAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Failed to delete user {UserId}: {Message}", id, message);
                    if (message.Contains("not found"))
                        return NotFound(new ErrorResponse { Message = message });
                    return BadRequest(new ErrorResponse { Message = message });
                }

                _logger.LogInformation("User deleted successfully: {UserId}", id);
                return Ok(new SuccessResponse { Message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while deleting the user" });
            }
        }

        /// <summary>
        /// Change user password
        /// Permission: Administration.User.Edit (auto-resolved from POST)
        /// </summary>
        [HttpPost("{id}/change-password")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SuccessResponse>> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                _logger.LogInformation("Changing password for user: {UserId}", id);

                var (success, message) = await _userService.ChangePasswordAsync(id, changePasswordDto);

                if (!success)
                {
                    _logger.LogWarning("Failed to change password for user {UserId}: {Message}", id, message);
                    if (message.Contains("not found"))
                        return NotFound(new ErrorResponse { Message = message });
                    return BadRequest(new ErrorResponse { Message = message });
                }

                _logger.LogInformation("Password changed successfully for user: {UserId}", id);
                return Ok(new SuccessResponse { Message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while changing the password" });
            }
        }

        /// <summary>
        /// Assign role to user
        /// Permission: Administration.Role.Edit (auto-resolved from POST)
        /// </summary>
        [HttpPost("{id}/roles")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SuccessResponse>> AssignRole(int id, [FromBody] AssignRoleDto assignRoleDto)
        {
            try
            {
                _logger.LogInformation("Assigning role {RoleId} to user {UserId}", assignRoleDto.RoleId, id);

                var (success, message) = await _userService.AssignRoleAsync(id, assignRoleDto.RoleId);

                if (!success)
                {
                    _logger.LogWarning("Failed to assign role to user {UserId}: {Message}", id, message);
                    if (message.Contains("not found"))
                        return NotFound(new ErrorResponse { Message = message });
                    return BadRequest(new ErrorResponse { Message = message });
                }

                _logger.LogInformation("Role assigned successfully to user: {UserId}", id);
                return Ok(new SuccessResponse { Message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while assigning the role" });
            }
        }

        /// <summary>
        /// Remove role from user
        /// Permission: Administration.Role.Edit (auto-resolved from DELETE)
        /// </summary>
        [HttpDelete("{id}/roles/{roleId}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SuccessResponse>> RemoveRole(int id, int roleId)
        {
            try
            {
                _logger.LogInformation("Removing role {RoleId} from user {UserId}", roleId, id);

                var (success, message) = await _userService.RemoveRoleAsync(id, roleId);

                if (!success)
                {
                    _logger.LogWarning("Failed to remove role from user {UserId}: {Message}", id, message);
                    if (message.Contains("not found"))
                        return NotFound(new ErrorResponse { Message = message });
                    return BadRequest(new ErrorResponse { Message = message });
                }

                _logger.LogInformation("Role removed successfully from user: {UserId}", id);
                return Ok(new SuccessResponse { Message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse { Message = "An error occurred while removing the role" });
            }
        }
    }

    /// <summary>
    /// Standard error response
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Standard success response
    /// </summary>
    public class SuccessResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
