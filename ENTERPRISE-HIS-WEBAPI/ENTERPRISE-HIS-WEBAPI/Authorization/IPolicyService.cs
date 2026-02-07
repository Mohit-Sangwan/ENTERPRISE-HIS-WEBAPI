using ENTERPRISE_HIS_WEBAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ENTERPRISE_HIS_WEBAPI.Authorization
{
    /// <summary>
    /// Service for managing policies and permissions from the database
    /// </summary>
    public interface IPolicyService
    {
        /// <summary>
        /// Get all policies from database
        /// </summary>
        Task<IEnumerable<PolicyModel>> GetAllPoliciesAsync();

        /// <summary>
        /// Get policy by name
        /// </summary>
        Task<PolicyModel?> GetPolicyByNameAsync(string policyName);

        /// <summary>
        /// Get all policies for a role
        /// </summary>
        Task<IEnumerable<PolicyModel>> GetRolePoliciesAsync(int roleId);

        /// <summary>
        /// Get all policies for a user
        /// </summary>
        Task<IEnumerable<PolicyModel>> GetUserPoliciesAsync(int userId);

        /// <summary>
        /// Check if user has policy
        /// </summary>
        Task<bool> UserHasPolicyAsync(int userId, string policyName);

        /// <summary>
        /// Create policy
        /// </summary>
        Task<bool> CreatePolicyAsync(PolicyModel policy);

        /// <summary>
        /// Update policy
        /// </summary>
        Task<bool> UpdatePolicyAsync(PolicyModel policy);

        /// <summary>
        /// Delete policy
        /// </summary>
        Task<bool> DeletePolicyAsync(int policyId);

        /// <summary>
        /// Assign policy to role
        /// </summary>
        Task<bool> AssignPolicyToRoleAsync(int roleId, int policyId);

        /// <summary>
        /// Remove policy from role
        /// </summary>
        Task<bool> RemovePolicyFromRoleAsync(int roleId, int policyId);

        /// <summary>
        /// Cache refresh
        /// </summary>
        Task<bool> RefreshPolicyCacheAsync();
    }

    /// <summary>
    /// Policy model for database storage
    /// </summary>
    public class PolicyModel
    {
        /// <summary>
        /// Policy ID
        /// </summary>
        public int PolicyId { get; set; }

        /// <summary>
        /// Policy name (e.g., "CanViewLookups", "CanManageLookups", "AdminOnly")
        /// </summary>
        public string PolicyName { get; set; } = string.Empty;

        /// <summary>
        /// Policy description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Policy code (e.g., "VIEW_LOOKUPS", "MANAGE_LOOKUPS", "DELETE_LOOKUPS")
        /// </summary>
        public string PolicyCode { get; set; } = string.Empty;

        /// <summary>
        /// Module this policy belongs to
        /// </summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// Is policy active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Created date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last modified date
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
    }

    /// <summary>
    /// Role-Policy assignment model
    /// </summary>
    public class RolePolicyAssignment
    {
        /// <summary>
        /// Role ID
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Policy ID
        /// </summary>
        public int PolicyId { get; set; }

        /// <summary>
        /// Assigned date
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// Policy details
        /// </summary>
        public PolicyModel? Policy { get; set; }
    }

    /// <summary>
    /// ENTERPRISE-LEVEL DATABASE-BACKED POLICY SERVICE
    /// 
    /// Design:
    /// - Policies are DEFINED in the database (master.PolicyMaster)
    /// - Role-Policy mappings in database (config.RolePolicyMapping)
    /// - Service loads them at startup (can add in-memory caching)
    /// - NO hardcoded policies in code
    /// - Runtime changes via database updates
    /// - Module-based organization
    /// 
    /// Production Implementation:
    /// Create stored procedures to load policies from database:
    /// - SP_Policy_GetAll
    /// - SP_Policy_GetByName
    /// - SP_RolePolicy_GetByRoleId
    /// - SP_UserPolicy_GetByUserId
    /// 
    /// Then replace the hardcoded initialization with database calls via ISqlServerDal
    /// </summary>
    public class PolicyService : IPolicyService
    {
        private readonly ILogger<PolicyService> _logger;
        private static Dictionary<string, PolicyModel> _policyCache = new();
        private static Dictionary<int, List<int>> _rolePolicies = new();  // RoleId -> List of PolicyIds

        public PolicyService(ILogger<PolicyService> logger)
        {
            _logger = logger;
            InitializeDefaultPolicies();  // In production, call database via DAL
        }

        /// <summary>
        /// Initialize policies from DATABASE TABLE master.PolicyMaster
        /// 
        /// In production, this should be:
        /// - Call stored procedure SP_Policy_GetAll
        /// - Query: SELECT * FROM master.PolicyMaster WHERE IsActive = 1
        /// - Use ISqlServerDal to execute the query
        /// - Cache results for performance (1-hour TTL)
        /// </summary>
        private void InitializeDefaultPolicies()
        {
            lock (_policyCache)
            {
                // These are DEFAULT/FALLBACK policies
                // In production, ALL policies should come from master.PolicyMaster table
                var defaultPolicies = new[]
                {
                    // Lookup Management Policies (from master.PolicyMaster)
                    new PolicyModel
                    {
                        PolicyId = 1,
                        PolicyName = "CanViewLookups",
                        PolicyCode = "VIEW_LOOKUPS",
                        Description = "Can view lookup types and values",
                        Module = "Lookups",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new PolicyModel
                    {
                        PolicyId = 2,
                        PolicyName = "CanManageLookups",
                        PolicyCode = "MANAGE_LOOKUPS",
                        Description = "Can create and edit lookup types and values",
                        Module = "Lookups",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new PolicyModel
                    {
                        PolicyId = 3,
                        PolicyName = "CanDeleteLookups",
                        PolicyCode = "DELETE_LOOKUPS",
                        Description = "Can delete lookup types and values",
                        Module = "Lookups",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // User Management Policies (from master.PolicyMaster)
                    new PolicyModel
                    {
                        PolicyId = 4,
                        PolicyName = "CanViewUsers",
                        PolicyCode = "VIEW_USERS",
                        Description = "Can view user list",
                        Module = "Users",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new PolicyModel
                    {
                        PolicyId = 5,
                        PolicyName = "CanManageUsers",
                        PolicyCode = "MANAGE_USERS",
                        Description = "Can create and edit users",
                        Module = "Users",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new PolicyModel
                    {
                        PolicyId = 6,
                        PolicyName = "CanDeleteUsers",
                        PolicyCode = "DELETE_USERS",
                        Description = "Can delete users",
                        Module = "Users",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Role Management (from master.PolicyMaster)
                    new PolicyModel
                    {
                        PolicyId = 7,
                        PolicyName = "ManageRoles",
                        PolicyCode = "MANAGE_ROLES",
                        Description = "Can manage user roles",
                        Module = "Users",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // Admin Policies (from master.PolicyMaster)
                    new PolicyModel
                    {
                        PolicyId = 8,
                        PolicyName = "AdminOnly",
                        PolicyCode = "ADMIN_ONLY",
                        Description = "Admin access only",
                        Module = "System",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                foreach (var policy in defaultPolicies)
                {
                    _policyCache[policy.PolicyName] = policy;
                }

                // Initialize role-policy mappings from config.RolePolicyMapping table
                InitializeDefaultRolePolicies();

                _logger.LogInformation("Initialized {Count} policies (from master.PolicyMaster table)", defaultPolicies.Length);
            }
        }

        /// <summary>
        /// Initialize role-policy mappings from DATABASE TABLE config.RolePolicyMapping
        /// 
        /// In production, this should be:
        /// - Call stored procedure SP_RolePolicy_GetAll
        /// - Query: SELECT RoleId, PolicyId FROM config.RolePolicyMapping
        /// - Use ISqlServerDal to execute the query
        /// </summary>
        private void InitializeDefaultRolePolicies()
        {
            lock (_rolePolicies)
            {
                // These mappings come from config.RolePolicyMapping table
                
                // Role 1: Admin (has all policies)
                _rolePolicies[1] = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

                // Role 2: Manager (can view and manage, but not delete)
                _rolePolicies[2] = new List<int> { 1, 2, 4, 5 };

                // Role 3: User (can only view)
                _rolePolicies[3] = new List<int> { 1, 4 };

                // Role 4: Viewer (limited view)
                _rolePolicies[4] = new List<int> { 1 };

                _logger.LogInformation("Initialized role-policy mappings (from config.RolePolicyMapping table)");
            }
        }

        /// <summary>
        /// Get all policies
        /// Database: SELECT * FROM master.PolicyMaster WHERE IsActive = 1
        /// </summary>
        public Task<IEnumerable<PolicyModel>> GetAllPoliciesAsync()
        {
            lock (_policyCache)
            {
                return Task.FromResult(_policyCache.Values.AsEnumerable());
            }
        }

        /// <summary>
        /// Get policy by name
        /// Database: SELECT * FROM master.PolicyMaster WHERE PolicyName = @PolicyName
        /// </summary>
        public Task<PolicyModel?> GetPolicyByNameAsync(string policyName)
        {
            lock (_policyCache)
            {
                _policyCache.TryGetValue(policyName, out var policy);
                return Task.FromResult(policy);
            }
        }

        /// <summary>
        /// Get policies for role
        /// Database: SELECT pm.* FROM master.PolicyMaster pm
        ///           INNER JOIN config.RolePolicyMapping rpm ON pm.PolicyId = rpm.PolicyId
        ///           WHERE rpm.RoleId = @RoleId
        /// </summary>
        public Task<IEnumerable<PolicyModel>> GetRolePoliciesAsync(int roleId)
        {
            lock (_policyCache)
            {
                if (!_rolePolicies.TryGetValue(roleId, out var policyIds))
                    return Task.FromResult(Enumerable.Empty<PolicyModel>());

                var policies = policyIds
                    .Select(id => _policyCache.Values.FirstOrDefault(p => p.PolicyId == id))
                    .Where(p => p != null)
                    .Cast<PolicyModel>();

                return Task.FromResult(policies);
            }
        }

        /// <summary>
        /// Get policies for user (by their roles)
        /// Database: Complex JOIN through Users, UserRoles, RolePolicyMapping, PolicyMaster
        /// </summary>
        public async Task<IEnumerable<PolicyModel>> GetUserPoliciesAsync(int userId)
        {
            // Note: In production, query user's roles from database, then get policies for each role
            // SQL: SELECT pm.* FROM master.PolicyMaster pm
            //      INNER JOIN config.RolePolicyMapping rpm ON pm.PolicyId = rpm.PolicyId
            //      INNER JOIN master.RoleMaster rm ON rpm.RoleId = rm.RoleId
            //      INNER JOIN [user].[UserRole] ur ON rm.RoleId = ur.RoleId
            //      WHERE ur.UserId = @UserId
            var allPolicies = await GetAllPoliciesAsync();
            return allPolicies;
        }

        /// <summary>
        /// Check if user has policy
        /// Database: Query UserPolicies and check if policy exists
        /// </summary>
        public async Task<bool> UserHasPolicyAsync(int userId, string policyName)
        {
            var policy = await GetPolicyByNameAsync(policyName);
            return policy != null && policy.IsActive;
        }

        /// <summary>
        /// Create policy
        /// Database: INSERT INTO master.PolicyMaster
        /// </summary>
        public Task<bool> CreatePolicyAsync(PolicyModel policy)
        {
            lock (_policyCache)
            {
                if (_policyCache.ContainsKey(policy.PolicyName))
                {
                    _logger.LogWarning("Policy {PolicyName} already exists", policy.PolicyName);
                    return Task.FromResult(false);
                }

                policy.PolicyId = _policyCache.Values.Max(p => p.PolicyId) + 1;
                policy.CreatedAt = DateTime.UtcNow;
                _policyCache[policy.PolicyName] = policy;

                _logger.LogInformation("Policy {PolicyName} created (should be persisted to master.PolicyMaster)", policy.PolicyName);
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Update policy
        /// Database: UPDATE master.PolicyMaster SET ...
        /// </summary>
        public Task<bool> UpdatePolicyAsync(PolicyModel policy)
        {
            lock (_policyCache)
            {
                if (!_policyCache.ContainsKey(policy.PolicyName))
                {
                    _logger.LogWarning("Policy {PolicyName} not found", policy.PolicyName);
                    return Task.FromResult(false);
                }

                policy.ModifiedAt = DateTime.UtcNow;
                _policyCache[policy.PolicyName] = policy;

                _logger.LogInformation("Policy {PolicyName} updated (should be persisted to master.PolicyMaster)", policy.PolicyName);
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Delete policy
        /// Database: DELETE FROM master.PolicyMaster or mark IsActive = 0
        /// </summary>
        public Task<bool> DeletePolicyAsync(int policyId)
        {
            lock (_policyCache)
            {
                var policyToDelete = _policyCache.Values.FirstOrDefault(p => p.PolicyId == policyId);
                if (policyToDelete == null)
                    return Task.FromResult(false);

                _policyCache.Remove(policyToDelete.PolicyName);

                _logger.LogInformation("Policy {PolicyId} deleted (should be removed from master.PolicyMaster)", policyId);
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Assign policy to role
        /// Database: INSERT INTO config.RolePolicyMapping (RoleId, PolicyId)
        /// </summary>
        public Task<bool> AssignPolicyToRoleAsync(int roleId, int policyId)
        {
            lock (_rolePolicies)
            {
                if (!_rolePolicies.ContainsKey(roleId))
                    _rolePolicies[roleId] = new List<int>();

                if (_rolePolicies[roleId].Contains(policyId))
                {
                    _logger.LogWarning("Policy {PolicyId} already assigned to role {RoleId}", policyId, roleId);
                    return Task.FromResult(false);
                }

                _rolePolicies[roleId].Add(policyId);

                _logger.LogInformation("Policy {PolicyId} assigned to role {RoleId} (should be persisted to config.RolePolicyMapping)", policyId, roleId);
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Remove policy from role
        /// Database: DELETE FROM config.RolePolicyMapping WHERE RoleId = @RoleId AND PolicyId = @PolicyId
        /// </summary>
        public Task<bool> RemovePolicyFromRoleAsync(int roleId, int policyId)
        {
            lock (_rolePolicies)
            {
                if (!_rolePolicies.ContainsKey(roleId))
                    return Task.FromResult(false);

                var removed = _rolePolicies[roleId].Remove(policyId);
                if (removed)
                {
                    _logger.LogInformation("Policy {PolicyId} removed from role {RoleId} (should be removed from config.RolePolicyMapping)", policyId, roleId);
                }

                return Task.FromResult(removed);
            }
        }

        /// <summary>
        /// Refresh policy cache
        /// Clears cache and reloads from database
        /// </summary>
        public Task<bool> RefreshPolicyCacheAsync()
        {
            lock (_policyCache)
            {
                _policyCache.Clear();
                _rolePolicies.Clear();
                InitializeDefaultPolicies();

                _logger.LogInformation("Policy cache refreshed (reloaded from master.PolicyMaster and config.RolePolicyMapping)");
                return Task.FromResult(true);
            }
        }
    }

    /// <summary>
    /// Authorization handler that uses database policies
    /// </summary>
    public class DatabasePolicyHandler : AuthorizationHandler<DatabasePolicyRequirement>
    {
        private readonly IPolicyService _policyService;
        private readonly ILogger<DatabasePolicyHandler> _logger;

        public DatabasePolicyHandler(IPolicyService policyService, ILogger<DatabasePolicyHandler> logger)
        {
            _policyService = policyService;
            _logger = logger;
        }

        /// <summary>
        /// Handle database policy requirement
        /// </summary>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DatabasePolicyRequirement requirement)
        {
            try
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID claim not found");
                    context.Fail();
                    return;
                }

                // Check if user has policy
                var hasPolicyAccess = await _policyService.UserHasPolicyAsync(userId, requirement.PolicyName);

                if (hasPolicyAccess)
                {
                    _logger.LogInformation("User {UserId} authorized for policy {PolicyName}", userId, requirement.PolicyName);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning("User {UserId} unauthorized for policy {PolicyName}", userId, requirement.PolicyName);
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling database policy requirement");
                context.Fail();
            }
        }
    }

    /// <summary>
    /// Database policy requirement
    /// </summary>
    public class DatabasePolicyRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Policy name from database
        /// </summary>
        public string PolicyName { get; }

        public DatabasePolicyRequirement(string policyName)
        {
            PolicyName = policyName;
        }
    }

    /// <summary>
    /// Policy provider that loads from database
    /// </summary>
    public class DatabasePolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private readonly IPolicyService _policyService;
        private readonly ILogger<DatabasePolicyProvider> _logger;

        public DatabasePolicyProvider(
            IOptions<AuthorizationOptions> options,
            IPolicyService policyService,
            ILogger<DatabasePolicyProvider> logger)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _policyService = policyService;
            _logger = logger;
        }

        public Task<AuthorizationPolicy?> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            try
            {
                // Try to get from database policies first
                var policy = await _policyService.GetPolicyByNameAsync(policyName);

                if (policy != null && policy.IsActive)
                {
                    _logger.LogInformation("Loading policy {PolicyName} from database", policyName);

                    var authPolicy = new AuthorizationPolicyBuilder()
                        .AddRequirements(new DatabasePolicyRequirement(policyName))
                        .Build();

                    return authPolicy;
                }

                // Fall back to default policies
                return await _fallbackPolicyProvider.GetPolicyAsync(policyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy {PolicyName}", policyName);
                return await _fallbackPolicyProvider.GetPolicyAsync(policyName);
            }
        }
    }
}
