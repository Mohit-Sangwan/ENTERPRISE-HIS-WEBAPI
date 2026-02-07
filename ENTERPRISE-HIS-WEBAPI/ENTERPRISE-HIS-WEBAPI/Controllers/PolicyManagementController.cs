using ENTERPRISE_HIS_WEBAPI.Authorization;
using ENTERPRISE_HIS_WEBAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ENTERPRISE_HIS_WEBAPI.Controllers
{
    /// <summary>
    /// Policy Management API - Manage policies at runtime without hardcoding
    /// Authorization: Enterprise Authorization Middleware handles all permissions
    /// Permissions auto-resolved: GET?View, POST?Create, PUT?Edit, DELETE?Delete
    /// Admin-only operations auto-enforced by middleware based on operation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PolicyManagementController : ControllerBase
    {
        private readonly IPolicyService _policyService;
        private readonly ILogger<PolicyManagementController> _logger;

        public PolicyManagementController(
            IPolicyService policyService,
            ILogger<PolicyManagementController> logger)
        {
            _policyService = policyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all policies
        /// Permission: Administration.Policy.View (auto-resolved from GET)
        /// </summary>
        [HttpGet("policies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<PolicyResponse>>> GetAllPolicies()
        {
            try
            {
                var policies = await _policyService.GetAllPoliciesAsync();
                var response = policies.Select(p => new PolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    PolicyCode = p.PolicyCode,
                    Description = p.Description,
                    Module = p.Module,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).ToList();

                _logger.LogInformation("Retrieved {Count} policies", response.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policies");
                return StatusCode(500, new { error = "Error retrieving policies" });
            }
        }

        /// <summary>
        /// Get policy by name
        /// Permission: Administration.Policy.View (auto-resolved from GET)
        /// </summary>
        [HttpGet("policies/{policyName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PolicyResponse>> GetPolicyByName(string policyName)
        {
            try
            {
                var policy = await _policyService.GetPolicyByNameAsync(policyName);
                if (policy == null)
                    return NotFound(new { error = $"Policy '{policyName}' not found" });

                var response = new PolicyResponse
                {
                    PolicyId = policy.PolicyId,
                    PolicyName = policy.PolicyName,
                    PolicyCode = policy.PolicyCode,
                    Description = policy.Description,
                    Module = policy.Module,
                    IsActive = policy.IsActive,
                    CreatedAt = policy.CreatedAt
                };

                _logger.LogInformation("Retrieved policy {PolicyName}", policyName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policy {PolicyName}", policyName);
                return StatusCode(500, new { error = "Error retrieving policy" });
            }
        }

        /// <summary>
        /// Create new policy
        /// Permission: Administration.Policy.Create (auto-resolved from POST)
        /// </summary>
        [HttpPost("policies")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PolicyResponse>> CreatePolicy([FromBody] CreatePolicyRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PolicyName) || string.IsNullOrWhiteSpace(request.PolicyCode))
                    return BadRequest(new { error = "PolicyName and PolicyCode are required" });

                var existing = await _policyService.GetPolicyByNameAsync(request.PolicyName);
                if (existing != null)
                    return Conflict(new { error = $"Policy '{request.PolicyName}' already exists" });

                var policy = new PolicyModel
                {
                    PolicyName = request.PolicyName,
                    PolicyCode = request.PolicyCode,
                    Description = request.Description,
                    Module = request.Module,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var success = await _policyService.CreatePolicyAsync(policy);
                if (!success)
                    return BadRequest(new { error = "Failed to create policy" });

                var response = new PolicyResponse
                {
                    PolicyId = policy.PolicyId,
                    PolicyName = policy.PolicyName,
                    PolicyCode = policy.PolicyCode,
                    Description = policy.Description,
                    Module = policy.Module,
                    IsActive = policy.IsActive,
                    CreatedAt = policy.CreatedAt
                };

                _logger.LogInformation("Policy {PolicyName} created", request.PolicyName);
                return CreatedAtAction(nameof(GetPolicyByName), new { policyName = policy.PolicyName }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy {PolicyName}", request.PolicyName);
                return StatusCode(500, new { error = "Error creating policy" });
            }
        }

        /// <summary>
        /// Update policy
        /// Permission: Administration.Policy.Edit (auto-resolved from PUT)
        /// </summary>
        [HttpPut("policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PolicyResponse>> UpdatePolicy(int policyId, [FromBody] UpdatePolicyRequest request)
        {
            try
            {
                var policies = await _policyService.GetAllPoliciesAsync();
                var policy = policies.FirstOrDefault(p => p.PolicyId == policyId);
                if (policy == null)
                    return NotFound(new { error = $"Policy with ID {policyId} not found" });

                if (!string.IsNullOrWhiteSpace(request.PolicyName))
                    policy.PolicyName = request.PolicyName;
                if (!string.IsNullOrWhiteSpace(request.PolicyCode))
                    policy.PolicyCode = request.PolicyCode;
                if (!string.IsNullOrWhiteSpace(request.Description))
                    policy.Description = request.Description;
                if (!string.IsNullOrWhiteSpace(request.Module))
                    policy.Module = request.Module;
                if (request.IsActive.HasValue)
                    policy.IsActive = request.IsActive.Value;

                policy.ModifiedAt = DateTime.UtcNow;

                var success = await _policyService.UpdatePolicyAsync(policy);
                if (!success)
                    return BadRequest(new { error = "Failed to update policy" });

                var response = new PolicyResponse
                {
                    PolicyId = policy.PolicyId,
                    PolicyName = policy.PolicyName,
                    PolicyCode = policy.PolicyCode,
                    Description = policy.Description,
                    Module = policy.Module,
                    IsActive = policy.IsActive,
                    CreatedAt = policy.CreatedAt
                };

                _logger.LogInformation("Policy {PolicyName} updated", policy.PolicyName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy {PolicyId}", policyId);
                return StatusCode(500, new { error = "Error updating policy" });
            }
        }

        /// <summary>
        /// Delete policy
        /// Permission: Administration.Policy.Delete (auto-resolved from DELETE)
        /// </summary>
        [HttpDelete("policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePolicy(int policyId)
        {
            try
            {
                var policies = await _policyService.GetAllPoliciesAsync();
                if (!policies.Any(p => p.PolicyId == policyId))
                    return NotFound(new { error = $"Policy with ID {policyId} not found" });

                var success = await _policyService.DeletePolicyAsync(policyId);
                if (!success)
                    return BadRequest(new { error = "Failed to delete policy" });

                _logger.LogInformation("Policy {PolicyId} deleted", policyId);
                return Ok(new { message = "Policy deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy {PolicyId}", policyId);
                return StatusCode(500, new { error = "Error deleting policy" });
            }
        }

        /// <summary>
        /// Get all policies for a specific role
        /// Permission: Administration.Policy.View (auto-resolved from GET)
        /// </summary>
        [HttpGet("roles/{roleId}/policies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<PolicyResponse>>> GetRolePolicies(int roleId)
        {
            try
            {
                var policies = await _policyService.GetRolePoliciesAsync(roleId);
                var response = policies.Select(p => new PolicyResponse
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    PolicyCode = p.PolicyCode,
                    Description = p.Description,
                    Module = p.Module,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).ToList();

                _logger.LogInformation("Retrieved {Count} policies for role {RoleId}", response.Count, roleId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policies for role {RoleId}", roleId);
                return StatusCode(500, new { error = "Error retrieving policies" });
            }
        }

        /// <summary>
        /// Assign policy to role
        /// Permission: Administration.Policy.Edit (auto-resolved from POST)
        /// </summary>
        [HttpPost("roles/{roleId}/policies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> AssignPolicyToRole(int roleId, [FromBody] AssignPolicyRequest request)
        {
            try
            {
                if (request.PolicyId <= 0)
                    return BadRequest(new { error = "Invalid PolicyId" });

                var success = await _policyService.AssignPolicyToRoleAsync(roleId, request.PolicyId);
                if (!success)
                    return BadRequest(new { error = "Failed to assign policy to role (already assigned or invalid)" });

                _logger.LogInformation("Policy {PolicyId} assigned to role {RoleId}", request.PolicyId, roleId);
                return Ok(new { message = $"Policy {request.PolicyId} assigned to role {roleId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning policy to role");
                return StatusCode(500, new { error = "Error assigning policy" });
            }
        }

        /// <summary>
        /// Remove policy from role
        /// Permission: Administration.Policy.Edit (auto-resolved from DELETE)
        /// </summary>
        [HttpDelete("roles/{roleId}/policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemovePolicyFromRole(int roleId, int policyId)
        {
            try
            {
                var success = await _policyService.RemovePolicyFromRoleAsync(roleId, policyId);
                if (!success)
                    return NotFound(new { error = "Policy not found in role assignments" });

                _logger.LogInformation("Policy {PolicyId} removed from role {RoleId}", policyId, roleId);
                return Ok(new { message = $"Policy {policyId} removed from role {roleId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing policy from role");
                return StatusCode(500, new { error = "Error removing policy" });
            }
        }

        /// <summary>
        /// Refresh policy cache from database
        /// Permission: Administration.Policy.View (auto-resolved from POST)
        /// </summary>
        [HttpPost("cache/refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> RefreshCache()
        {
            try
            {
                var policies = await _policyService.GetAllPoliciesAsync();
                _logger.LogInformation("Policy cache refreshed. Total policies: {Count}", policies.Count());
                return Ok(new { message = "Policy cache refreshed successfully", policiesLoaded = policies.Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache");
                return StatusCode(500, new { error = "Error refreshing cache" });
            }
        }

        /// <summary>
        /// Get policy statistics
        /// Permission: Administration.Policy.View (auto-resolved from GET)
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PolicyStatistics>> GetStatistics()
        {
            try
            {
                var policies = await _policyService.GetAllPoliciesAsync();
                var activePolicies = policies.Count(p => p.IsActive);
                var inactivePolicies = policies.Count(p => !p.IsActive);
                var modules = policies.Select(p => p.Module).Distinct().Count();

                var stats = new PolicyStatistics
                {
                    TotalPolicies = policies.Count(),
                    ActivePolicies = activePolicies,
                    InactivePolicies = inactivePolicies,
                    Modules = modules,
                    PoliciesByModule = policies
                        .GroupBy(p => p.Module)
                        .ToDictionary(g => g.Key ?? "Other", g => g.Count())
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics");
                return StatusCode(500, new { error = "Error retrieving statistics" });
            }
        }
    }

    /// <summary>
    /// Policy response model
    /// </summary>
    public class PolicyResponse
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string PolicyCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Module { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Create policy request
    /// </summary>
    public class CreatePolicyRequest
    {
        public string PolicyName { get; set; } = string.Empty;
        public string PolicyCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Module { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update policy request
    /// </summary>
    public class UpdatePolicyRequest
    {
        public string? PolicyName { get; set; }
        public string? PolicyCode { get; set; }
        public string? Description { get; set; }
        public string? Module { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Assign policy request
    /// </summary>
    public class AssignPolicyRequest
    {
        public int PolicyId { get; set; }
    }

    /// <summary>
    /// Policy statistics
    /// </summary>
    public class PolicyStatistics
    {
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int InactivePolicies { get; set; }
        public int Modules { get; set; }
        public Dictionary<string, int> PoliciesByModule { get; set; } = new();
    }
}
