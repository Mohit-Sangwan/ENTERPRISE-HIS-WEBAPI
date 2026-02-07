namespace ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise
{
    using Microsoft.AspNetCore.Mvc.Controllers;

    /// <summary>
    /// Builds enterprise permission strings from HTTP context
    /// Format: Module.Resource.Operation[.Scope]
    /// 
    /// Examples:
    /// Lookups.LookupType.View
    /// Lookups.LookupType.Delete
    /// Billing.Invoice.Approve
    /// EMR.Encounter.Sign
    /// EMR.Encounter.View.Department:ED
    /// Billing.Invoice.View.Facility:Main
    /// </summary>
    public static class PermissionBuilder
    {
        /// <summary>
        /// Build full permission string from HTTP context
        /// Returns: Module.Resource.Operation[.Scope]
        /// </summary>
        public static string? Build(HttpContext context)
        {
            try
            {
                // Get operation from HTTP method and route
                var operation = OperationResolver.Resolve(context);
                if (operation == "Unknown")
                    return null;

                // Get module and resource from controller
                var (module, resource) = GetModuleAndResource(context);
                if (module == "Unknown" || resource == "Unknown")
                    return null;

                // Get scope if applicable (department, facility, etc.)
                var scope = OperationResolver.ResolveScopeFromQuery(context);

                // Build permission string
                var permission = $"{module}.{resource}.{operation}";

                // Add scope if not global
                if (scope != "Global")
                    permission += $".{scope}";

                return permission;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get module and resource from current request context
        /// </summary>
        private static (string Module, string Resource) GetModuleAndResource(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (descriptor != null)
                return ResourceResolver.Resolve(descriptor);

            // Fallback: try to extract from route
            if (context.GetRouteValue("controller") is string controller)
                return ResourceResolver.Resolve(controller);

            return ("Unknown", "Unknown");
        }

        /// <summary>
        /// Build permission from explicit parameters
        /// Useful for internal authorization checks
        /// </summary>
        public static string BuildExplicit(string module, string resource, string operation, string? scope = null)
        {
            if (string.IsNullOrWhiteSpace(module) || 
                string.IsNullOrWhiteSpace(resource) || 
                string.IsNullOrWhiteSpace(operation))
                throw new ArgumentException("Module, Resource, and Operation cannot be empty");

            var permission = $"{module}.{resource}.{operation}";

            if (!string.IsNullOrWhiteSpace(scope) && scope != "Global")
                permission += $".{scope}";

            return permission;
        }

        /// <summary>
        /// Check if a permission matches a pattern
        /// Supports wildcards: "Lookups.*.*", "Lookups.LookupType.*", etc.
        /// </summary>
        public static bool Matches(string permission, string pattern)
        {
            if (permission == pattern)
                return true;

            // Handle wildcards
            var permissionParts = permission.Split('.');
            var patternParts = pattern.Split('.');

            for (int i = 0; i < patternParts.Length; i++)
            {
                if (i >= permissionParts.Length)
                    return false;

                if (patternParts[i] == "*")
                    continue; // Wildcard matches anything

                if (patternParts[i] != permissionParts[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parse permission string into components
        /// </summary>
        public static (string Module, string Resource, string Operation, string? Scope) Parse(string permission)
        {
            var parts = permission.Split('.');
            
            if (parts.Length < 3)
                throw new ArgumentException($"Invalid permission format: {permission}. Expected: Module.Resource.Operation[.Scope]");

            var module = parts[0];
            var resource = parts[1];
            var operation = parts[2];
            var scope = parts.Length > 3 ? string.Join(".", parts.Skip(3)) : null;

            return (module, resource, operation, scope);
        }

        /// <summary>
        /// Verify permission format
        /// </summary>
        public static bool IsValidFormat(string permission)
        {
            try
            {
                var parts = permission.Split('.');
                if (parts.Length < 3)
                    return false;

                // Check for empty parts
                if (parts.Any(string.IsNullOrWhiteSpace))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all permission variants for a resource
        /// Generates all permutations of Module.Resource.[Operation]
        /// </summary>
        public static IEnumerable<string> GetPermutations(string module, string resource)
        {
            foreach (var operation in StandardOperations.All)
            {
                yield return BuildExplicit(module, resource, operation);
            }

            // Also yield the wildcard version
            yield return $"{module}.{resource}.*";
        }
    }
}
