using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ENTERPRISE_HIS_WEBAPI.Data.Repositories;

namespace ENTERPRISE_HIS_WEBAPI.Authorization
{
    /// <summary>
    /// Custom requirement for dynamic permission checks
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionCode { get; set; }

        public PermissionRequirement(string permissionCode)
        {
            PermissionCode = permissionCode;
        }
    }

    /// <summary>
    /// Handler for permission requirement - checks database permissions
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<PermissionHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionHandler(
            IRoleRepository roleRepository,
            ILogger<PermissionHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _roleRepository = roleRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid or missing user ID in claims");
                    context.Fail();
                    return;
                }

                // Check if user has permission from database
                var hasPermission = await _roleRepository.UserHasPermissionAsync(userId, requirement.PermissionCode);

                if (hasPermission)
                {
                    _logger.LogInformation("User {UserId} authorized for permission {Permission}", userId, requirement.PermissionCode);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning("User {UserId} denied permission {Permission}", userId, requirement.PermissionCode);
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling permission requirement");
                context.Fail();
            }
        }
    }

    /// <summary>
    /// Dynamic policy provider that loads policies from Module.Operation format
    /// TRUE enterprise-level - NO hardcoding!
    /// Example: "Lookups.View" ? Module=Lookups, Operation=View
    /// Policy name is parsed at runtime from database table
    /// </summary>
    public class DynamicPolicyProvider : IAuthorizationPolicyProvider
    {
        private DefaultAuthorizationPolicyProvider? _fallbackPolicyProvider;
        private readonly ILogger<DynamicPolicyProvider> _logger;

        public DynamicPolicyProvider(
            IOptions<AuthorizationOptions> options,
            ILogger<DynamicPolicyProvider> logger)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _logger = logger;
        }

        public Task<AuthorizationPolicy?> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider!.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider!.GetFallbackPolicyAsync();
        }

        /// <summary>
        /// Get policy by name - supports both formats:
        /// 1. "Module.Operation" format (e.g., "Lookups.View")  - Dynamic, database-driven
        /// 2. "Permission_CODE" format (e.g., "Permission_VIEW_LOOKUPS") - Legacy
        /// </summary>
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            try
            {
                _logger.LogInformation("Getting authorization policy for: {PolicyName}", policyName);

                // NEW: Module.Operation format (NO hardcoding!)
                // Example: "Lookups.View" ? checks master.PolicyMaster at request time
                if (policyName.Contains(".") && !policyName.StartsWith("Permission_"))
                {
                    var parts = policyName.Split('.');
                    if (parts.Length == 2)
                    {
                        var module = parts[0];  // "Lookups"
                        var operation = parts[1];  // "View"
                        
                        _logger.LogInformation(
                            "Creating dynamic policy from Module.Operation - Module={Module}, Operation={Operation}",
                            module, operation);

                        // Create policy that will check database at request time
                        var policy = new AuthorizationPolicyBuilder()
                            .AddRequirements(new DynamicModuleOperationRequirement(module, operation))
                            .Build();

                        return Task.FromResult<AuthorizationPolicy?>(policy);
                    }
                }

                // LEGACY: Permission_CODE format
                if (policyName.StartsWith("Permission_", StringComparison.OrdinalIgnoreCase))
                {
                    var permissionCode = policyName.Substring("Permission_".Length);
                    _logger.LogInformation("Creating permission-based policy for: {PermissionCode}", permissionCode);

                    var policy = new AuthorizationPolicyBuilder()
                        .AddRequirements(new PermissionRequirement(permissionCode))
                        .Build();

                    return Task.FromResult<AuthorizationPolicy?>(policy);
                }

                // Fallback to default
                _logger.LogWarning("Policy {PolicyName} not recognized, using default provider", policyName);
                return _fallbackPolicyProvider!.GetPolicyAsync(policyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authorization policy for {PolicyName}", policyName);
                return _fallbackPolicyProvider!.GetPolicyAsync(policyName);
            }
        }
    }

    /// <summary>
    /// ENTERPRISE-LEVEL: Dynamic Module+Operation Authorization Requirement
    /// Policy name is constructed at runtime from Module.Operation
    /// Completely database-driven - NO hardcoding!
    /// </summary>
    public class DynamicModuleOperationRequirement : IAuthorizationRequirement
    {
        public string Module { get; }
        public string Operation { get; }
        public string PolicyName { get; }  // Constructed at runtime: Module.Operation

        public DynamicModuleOperationRequirement(string module, string operation)
        {
            Module = module;
            Operation = operation;
            PolicyName = $"{module}.{operation}";  // Never hardcoded!
        }
    }

    /// <summary>
    /// ENTERPRISE-LEVEL: Handler for Dynamic Module+Operation Requirements
    /// Checks database at request time - completely dynamic
    /// </summary>
    public class DynamicModuleOperationHandler : AuthorizationHandler<DynamicModuleOperationRequirement>
    {
        private readonly IPolicyService _policyService;
        private readonly ILogger<DynamicModuleOperationHandler> _logger;

        public DynamicModuleOperationHandler(
            IPolicyService policyService,
            ILogger<DynamicModuleOperationHandler> logger)
        {
            _policyService = policyService;
            _logger = logger;
        }

        /// <summary>
        /// Handle authorization - checks database for policy
        /// Zero hardcoding - all from master.PolicyMaster table
        /// </summary>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DynamicModuleOperationRequirement requirement)
        {
            try
            {
                var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID claim not found or invalid");
                    context.Fail();
                    return;
                }

                // Check policy from database (NOT hardcoded!)
                var hasPolicyAccess = await _policyService.UserHasPolicyAsync(userId, requirement.PolicyName);

                if (hasPolicyAccess)
                {
                    _logger.LogInformation(
                        "User {UserId} authorized - Module={Module}, Operation={Operation}, PolicyName={PolicyName}",
                        userId, requirement.Module, requirement.Operation, requirement.PolicyName);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning(
                        "User {UserId} unauthorized - Module={Module}, Operation={Operation}, PolicyName={PolicyName}",
                        userId, requirement.Module, requirement.Operation, requirement.PolicyName);
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling dynamic module+operation authorization");
                context.Fail();
            }
        }
    }
}
