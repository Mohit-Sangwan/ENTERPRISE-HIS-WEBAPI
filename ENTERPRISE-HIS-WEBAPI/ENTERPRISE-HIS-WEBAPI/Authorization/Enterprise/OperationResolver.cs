namespace ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise
{
    /// <summary>
    /// Resolves HTTP operations to enterprise permission operations
    /// Maps HTTP verbs and URL patterns to standard enterprise operations
    /// 
    /// Standard Mapping:
    /// GET     ? View
    /// POST    ? Create
    /// PUT     ? Edit
    /// PATCH   ? Edit
    /// DELETE  ? Delete
    /// 
    /// Special Route-Based Operations:
    /// /approve  ? Approve
    /// /verify   ? Verify
    /// /reject   ? Reject
    /// /cancel   ? Cancel
    /// /print    ? Print
    /// /export   ? Export
    /// /sign     ? Sign
    /// /close    ? Close
    /// /reopen   ? Reopen
    /// </summary>
    public static class OperationResolver
    {
        /// <summary>
        /// Resolve HTTP request to enterprise operation
        /// </summary>
        public static string Resolve(HttpContext context)
        {
            var method = context.Request.Method.ToUpperInvariant();
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // Route-based special operations (highest priority)
            if (path.Contains("/approve")) return "Approve";
            if (path.Contains("/reject")) return "Reject";
            if (path.Contains("/verify")) return "Verify";
            if (path.Contains("/sign")) return "Sign";
            if (path.Contains("/cancel")) return "Cancel";
            if (path.Contains("/close")) return "Close";
            if (path.Contains("/reopen")) return "Reopen";
            if (path.Contains("/print")) return "Print";
            if (path.Contains("/export")) return "Export";
            if (path.Contains("/import")) return "Import";
            if (path.Contains("/restore")) return "Restore";
            if (path.Contains("/archive")) return "Archive";
            if (path.Contains("/bulk")) return "BulkOperation";
            if (path.Contains("/sync")) return "Sync";
            if (path.Contains("/migrate")) return "Migrate";

            // Standard HTTP verb mapping
            return method switch
            {
                "GET" => "View",
                "POST" => "Create",
                "PUT" => "Edit",
                "PATCH" => "Edit",
                "DELETE" => "Delete",
                "HEAD" => "View",
                "OPTIONS" => "View",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Resolve scope from request (department, facility, etc.)
        /// </summary>
        public static string ResolveScopeFromQuery(HttpContext context)
        {
            // Check URL parameters
            if (context.Request.Query.TryGetValue("departmentId", out var dept))
                return $"Department:{dept}";

            if (context.Request.Query.TryGetValue("facilityId", out var facility))
                return $"Facility:{facility}";

            if (context.Request.Query.TryGetValue("scope", out var scope))
                return scope.ToString();

            // Check route values
            if (context.GetRouteValue("departmentId") != null)
                return $"Department:{context.GetRouteValue("departmentId")}";

            // Default: no scope restriction
            return "Global";
        }
    }

    /// <summary>
    /// All standard enterprise operations
    /// Used for caching and validation
    /// </summary>
    public static class StandardOperations
    {
        // CRUD Operations
        public const string View = "View";
        public const string Create = "Create";
        public const string Edit = "Edit";
        public const string Delete = "Delete";

        // Approval Workflows
        public const string Approve = "Approve";
        public const string Reject = "Reject";
        public const string Verify = "Verify";
        public const string Sign = "Sign";

        // State Management
        public const string Cancel = "Cancel";
        public const string Close = "Close";
        public const string Reopen = "Reopen";

        // Data Operations
        public const string Print = "Print";
        public const string Export = "Export";
        public const string Import = "Import";

        // Advanced Operations
        public const string Restore = "Restore";
        public const string Archive = "Archive";
        public const string BulkOperation = "BulkOperation";
        public const string Sync = "Sync";
        public const string Migrate = "Migrate";

        /// <summary>
        /// All standard operations
        /// </summary>
        public static readonly string[] All = new[]
        {
            View, Create, Edit, Delete,
            Approve, Reject, Verify, Sign,
            Cancel, Close, Reopen,
            Print, Export, Import,
            Restore, Archive, BulkOperation, Sync, Migrate
        };

        /// <summary>
        /// Get operation category
        /// </summary>
        public static string GetCategory(string operation) => operation switch
        {
            View or Create or Edit or Delete => "CRUD",
            Approve or Reject or Verify or Sign => "Approval",
            Cancel or Close or Reopen => "StateManagement",
            Print or Export or Import => "DataOps",
            Restore or Archive or BulkOperation or Sync or Migrate => "Advanced",
            _ => "Unknown"
        };
    }
}
