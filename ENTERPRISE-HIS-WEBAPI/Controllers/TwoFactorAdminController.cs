using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication;
using ENTERPRISE_HIS_WEBAPI.Extensions;
using Microsoft.Extensions.Logging;

namespace ENTERPRISE_HIS_WEBAPI.Controllers
{
    /// <summary>
    /// Enterprise 2FA Administration Controller
    /// Manages all 2FA configuration at enterprise level
    /// Authorization: Admin/Security team only
    /// 
    /// Operations:
    /// - Global 2FA configuration
    /// - 2FA policy CRUD
    /// - Role-to-policy mapping
    /// - User-specific overrides
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize(Policy = "Admin2FA")]  // Custom authorization policy for 2FA admins
    public class TwoFactorAdminController : ControllerBase
    {
        private readonly ITwoFactorConfigurationService _configService;
        private readonly ILogger<TwoFactorAdminController> _logger;

        public TwoFactorAdminController(
            ITwoFactorConfigurationService configService,
            ILogger<TwoFactorAdminController> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        // ===== Global Configuration Operations =====

        /// <summary>
        /// Get single configuration value
        /// </summary>
        [HttpGet("config/{configKey}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<string>>> GetConfig(string configKey)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("Admin {UserId} retrieving config {ConfigKey}", userId, configKey);

                var value = await _configService.GetConfigAsync<string>(configKey);
                
                if (value == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Configuration key '{configKey}' not found"
                    });

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = value,
                    Message = $"Configuration retrieved: {configKey}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting config {ConfigKey}", configKey);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get all configuration values
        /// </summary>
        [HttpGet("config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<Dictionary<string, string>>>> GetAllConfig()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("Admin {UserId} retrieving all 2FA configuration", userId);

                var config = await _configService.GetAllConfigAsync();

                return Ok(new ApiResponse<Dictionary<string, string>>
                {
                    Success = true,
                    Data = config,
                    Message = "All 2FA configuration retrieved"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all config");
                return StatusCode(500, new ApiResponse<Dictionary<string, string>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Update configuration value
        /// </summary>
        [HttpPut("config/{configKey}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateConfig(
            string configKey,
            [FromBody] UpdateConfigRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogWarning("Admin {UserId} ({UserName}) updating config {ConfigKey}", userId, userName, configKey);

                var result = await _configService.SetConfigAsync(configKey, request.ConfigValue, request.ConfigType);

                if (!result)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to update configuration"
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = $"Configuration '{configKey}' updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating config {ConfigKey}", configKey);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Reset all configuration to defaults
        /// </summary>
        [HttpPost("config/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> ResetConfig()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogWarning("Admin {UserId} ({UserName}) resetting ALL 2FA configuration", userId, userName);

                var result = await _configService.ResetConfigAsync();

                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Configuration reset to defaults" : "Failed to reset configuration"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting config");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // ===== 2FA Policy CRUD Operations =====

        /// <summary>
        /// Get 2FA policy by ID
        /// </summary>
        [HttpGet("policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<TwoFactorPolicyDto>>> GetPolicy(int policyId)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("Admin {UserId} retrieving policy {PolicyId}", userId, policyId);

                var policy = await _configService.GetPolicyAsync(policyId);

                if (policy == null)
                    return NotFound(new ApiResponse<TwoFactorPolicyDto>
                    {
                        Success = false,
                        Message = $"Policy {policyId} not found"
                    });

                return Ok(new ApiResponse<TwoFactorPolicyDto>
                {
                    Success = true,
                    Data = policy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy {PolicyId}", policyId);
                return StatusCode(500, new ApiResponse<TwoFactorPolicyDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get 2FA policy by code
        /// </summary>
        [HttpGet("policies/code/{policyCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<TwoFactorPolicyDto>>> GetPolicyByCode(string policyCode)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("Admin {UserId} retrieving policy by code {PolicyCode}", userId, policyCode);

                var policy = await _configService.GetPolicyByCodeAsync(policyCode);

                if (policy == null)
                    return NotFound(new ApiResponse<TwoFactorPolicyDto>
                    {
                        Success = false,
                        Message = $"Policy {policyCode} not found"
                    });

                return Ok(new ApiResponse<TwoFactorPolicyDto>
                {
                    Success = true,
                    Data = policy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy by code {PolicyCode}", policyCode);
                return StatusCode(500, new ApiResponse<TwoFactorPolicyDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get all 2FA policies
        /// </summary>
        [HttpGet("policies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TwoFactorPolicyDto>>>> GetAllPolicies()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("Admin {UserId} retrieving all policies", userId);

                var policies = await _configService.GetAllPoliciesAsync();

                return Ok(new ApiResponse<IEnumerable<TwoFactorPolicyDto>>
                {
                    Success = true,
                    Data = policies,
                    Message = $"Retrieved {policies.Count()} policies"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all policies");
                return StatusCode(500, new ApiResponse<IEnumerable<TwoFactorPolicyDto>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Create new 2FA policy
        /// </summary>
        [HttpPost("policies")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<int>>> CreatePolicy([FromBody] TwoFactorAuthentication.CreatePolicyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogInformation("Admin {UserId} ({UserName}) creating policy {PolicyCode}", userId, userName, request.PolicyCode);

                var policyId = await _configService.CreatePolicyAsync(request);

                if (policyId == 0)
                    return BadRequest(new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Failed to create policy"
                    });

                return CreatedAtAction(nameof(GetPolicy), new { policyId }, new ApiResponse<int>
                {
                    Success = true,
                    Data = policyId,
                    Message = $"Policy created with ID {policyId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy");
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Update existing 2FA policy
        /// </summary>
        [HttpPut("policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdatePolicy(
            int policyId,
            [FromBody] UpdatePolicyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogInformation("Admin {UserId} ({UserName}) updating policy {PolicyId}", userId, userName, policyId);

                var result = await _configService.UpdatePolicyAsync(policyId, request);

                if (!result)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to update policy"
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Policy updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy {PolicyId}", policyId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Delete 2FA policy
        /// </summary>
        [HttpDelete("policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePolicy(int policyId)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogWarning("Admin {UserId} ({UserName}) deleting policy {PolicyId}", userId, userName, policyId);

                var result = await _configService.DeletePolicyAsync(policyId);

                if (!result)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to delete policy"
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Policy deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy {PolicyId}", policyId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // ===== Role-to-Policy Mapping =====

        /// <summary>
        /// Get 2FA policy for a role
        /// </summary>
        [HttpGet("roles/{roleId}/policy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<TwoFactorPolicyDto>>> GetRolePolicy(int roleId)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("Admin {UserId} retrieving 2FA policy for role {RoleId}", userId, roleId);

                var policy = await _configService.GetRolePolicyAsync(roleId);

                if (policy == null)
                    return NotFound(new ApiResponse<TwoFactorPolicyDto>
                    {
                        Success = false,
                        Message = $"No policy mapped for role {roleId}"
                    });

                return Ok(new ApiResponse<TwoFactorPolicyDto>
                {
                    Success = true,
                    Data = policy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy for role {RoleId}", roleId);
                return StatusCode(500, new ApiResponse<TwoFactorPolicyDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Map 2FA policy to role
        /// </summary>
        [HttpPost("roles/{roleId}/policy/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> MapRoleToPolicy(int roleId, int policyId)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogInformation("Admin {UserId} ({UserName}) mapping role {RoleId} to policy {PolicyId}", 
                    userId, userName, roleId, policyId);

                var result = await _configService.MapRoleToPolicyAsync(roleId, policyId);

                if (!result)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to map role to policy"
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = $"Role {roleId} mapped to policy {policyId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping role {RoleId} to policy {PolicyId}", roleId, policyId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Remove 2FA policy mapping from role
        /// </summary>
        [HttpDelete("roles/{roleId}/policy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveRolePolicy(int roleId)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var userName = HttpContext.GetUserName();
                _logger.LogInformation("Admin {UserId} ({UserName}) removing policy mapping for role {RoleId}", 
                    userId, userName, roleId);

                var result = await _configService.RemoveRolePolicyAsync(roleId);

                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Policy mapping removed" : "Failed to remove mapping"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing policy for role {RoleId}", roleId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // ===== User-Specific Overrides =====

        /// <summary>
        /// Get 2FA override for user
        /// </summary>
        [HttpGet("users/{userId}/override")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<UserTwoFactorOverrideDto>>> GetUserOverride(int userId)
        {
            try
            {
                var adminId = HttpContext.GetUserId();
                _logger.LogInformation("Admin {AdminId} retrieving 2FA override for user {UserId}", adminId, userId);

                var userOverride = await _configService.GetUserOverrideAsync(userId);

                if (userOverride == null)
                    return NotFound(new ApiResponse<UserTwoFactorOverrideDto>
                    {
                        Success = false,
                        Message = $"No override found for user {userId}"
                    });

                return Ok(new ApiResponse<UserTwoFactorOverrideDto>
                {
                    Success = true,
                    Data = userOverride
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting override for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<UserTwoFactorOverrideDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Set 2FA override for user
        /// </summary>
        [HttpPost("users/{userId}/override")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<int>>> SetUserOverride(
            int userId,
            [FromBody] SetUserOverrideRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var adminId = HttpContext.GetUserId();
                var adminName = HttpContext.GetUserName();
                _logger.LogInformation("Admin {AdminId} ({AdminName}) setting 2FA override for user {UserId}", 
                    adminId, adminName, userId);

                var overrideId = await _configService.SetUserOverrideAsync(userId, request);

                if (overrideId == 0)
                    return BadRequest(new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Failed to set override"
                    });

                return CreatedAtAction(nameof(GetUserOverride), new { userId }, new ApiResponse<int>
                {
                    Success = true,
                    Data = overrideId,
                    Message = $"Override created with ID {overrideId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting override for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Update 2FA override for user
        /// </summary>
        [HttpPut("users/{userId}/override")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUserOverride(
            int userId,
            [FromBody] UpdateUserOverrideRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var adminId = HttpContext.GetUserId();
                var adminName = HttpContext.GetUserName();
                _logger.LogInformation("Admin {AdminId} ({AdminName}) updating 2FA override for user {UserId}", 
                    adminId, adminName, userId);

                var result = await _configService.UpdateUserOverrideAsync(userId, request);

                if (!result)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to update override"
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Override updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating override for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Remove 2FA override for user
        /// </summary>
        [HttpDelete("users/{userId}/override")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveUserOverride(int userId)
        {
            try
            {
                var adminId = HttpContext.GetUserId();
                var adminName = HttpContext.GetUserName();
                _logger.LogWarning("Admin {AdminId} ({AdminName}) removing 2FA override for user {UserId}", 
                    adminId, adminName, userId);

                var result = await _configService.RemoveUserOverrideAsync(userId);

                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Override removed" : "Failed to remove override"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing override for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get effective 2FA settings for user (resolved from policies and overrides)
        /// </summary>
        [HttpGet("users/{userId}/effective-settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<EffectiveTwoFactorSettings>>> GetEffectiveSettings(int userId)
        {
            try
            {
                var adminId = HttpContext.GetUserId();
                _logger.LogInformation("Admin {AdminId} retrieving effective settings for user {UserId}", adminId, userId);

                var settings = await _configService.GetEffectiveSettingsAsync(userId);

                return Ok(new ApiResponse<EffectiveTwoFactorSettings>
                {
                    Success = true,
                    Data = settings,
                    Message = $"Settings resolved from: {settings.AppliedFrom}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting effective settings for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<EffectiveTwoFactorSettings>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }

    // ===== Request DTOs =====

    public class UpdateConfigRequest
    {
        public string ConfigValue { get; set; }
        public string ConfigType { get; set; }
    }
}
