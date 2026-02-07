namespace ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise
{
    using Microsoft.AspNetCore.Mvc.Controllers;

    /// <summary>
    /// Resolves controller/action to enterprise module and resource
    /// Maps controller names to module + resource tuples
    /// 
    /// Example Mappings:
    /// LookupTypesController ? ("Lookups", "LookupType")
    /// LookupTypeValuesController ? ("Lookups", "LookupTypeValue")
    /// UsersController ? ("Users", "User")
    /// InvoicesController ? ("Billing", "Invoice")
    /// EncountersController ? ("EMR", "Encounter")
    /// LabResultsController ? ("LIS", "LabResult")
    /// 
    /// This mapping can be moved to database for runtime configuration.
    /// </summary>
    public static class ResourceResolver
    {
        /// <summary>
        /// Central mapping of controllers to (Module, Resource)
        /// This is the only place where hardcoding should exist
        /// Can be moved to DB table later without code changes
        /// </summary>
        private static readonly Dictionary<string, (string Module, string Resource)> ControllerMapping = new()
        {
            // ========== Lookups Module ==========
            { "lookuptypes", ("Lookups", "LookupType") },
            { "lookuptypevalues", ("Lookups", "LookupTypeValue") },

            // ========== Administration Module ==========
            { "users", ("Administration", "User") },
            { "roles", ("Administration", "Role") },
            { "policies", ("Administration", "Policy") },
            { "permissions", ("Administration", "Permission") },

            // ========== EMR Module (Electronic Medical Record) ==========
            { "patients", ("EMR", "Patient") },
            { "encounters", ("EMR", "Encounter") },
            { "consultations", ("EMR", "Consultation") },
            { "diagnoses", ("EMR", "Diagnosis") },
            { "medications", ("EMR", "Medication") },

            // ========== Billing Module ==========
            { "invoices", ("Billing", "Invoice") },
            { "bills", ("Billing", "Bill") },
            { "payments", ("Billing", "Payment") },
            { "creditnotes", ("Billing", "CreditNote") },

            // ========== LIS Module (Laboratory Information System) ==========
            { "laborders", ("LIS", "LabOrder") },
            { "labresults", ("LIS", "LabResult") },
            { "testsets", ("LIS", "TestSet") },

            // ========== Pharmacy Module ==========
            { "prescriptions", ("Pharmacy", "Prescription") },
            { "medications", ("Pharmacy", "Medication") },
            { "stocks", ("Pharmacy", "Stock") },

            // ========== Reports Module ==========
            { "reports", ("Reports", "Report") },
            { "dashboards", ("Reports", "Dashboard") },

            // ========== Settings Module ==========
            { "settings", ("Settings", "Setting") },
            { "configurations", ("Settings", "Configuration") },
        };

        /// <summary>
        /// Resolve controller to (Module, Resource)
        /// Returns ("General", controllerName) if not found
        /// </summary>
        public static (string Module, string Resource) Resolve(string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                return ("General", "Unknown");

            var normalized = controllerName.ToLowerInvariant();

            if (ControllerMapping.TryGetValue(normalized, out var mapping))
                return mapping;

            // Fallback: derive from controller name
            return ("General", controllerName);
        }

        /// <summary>
        /// Resolve from ControllerActionDescriptor (from routing metadata)
        /// </summary>
        public static (string Module, string Resource) Resolve(ControllerActionDescriptor? descriptor)
        {
            if (descriptor == null)
                return ("Unknown", "Unknown");

            return Resolve(descriptor.ControllerName);
        }

        /// <summary>
        /// Get all modules
        /// </summary>
        public static IEnumerable<string> GetAllModules()
        {
            return ControllerMapping.Values.Select(x => x.Module).Distinct();
        }

        /// <summary>
        /// Get all resources for a module
        /// </summary>
        public static IEnumerable<string> GetResourcesByModule(string module)
        {
            return ControllerMapping.Values
                .Where(x => x.Module == module)
                .Select(x => x.Resource)
                .Distinct();
        }

        /// <summary>
        /// Add or update controller mapping (for runtime configuration)
        /// </summary>
        public static void AddOrUpdate(string controllerName, string module, string resource)
        {
            ControllerMapping[controllerName.ToLowerInvariant()] = (module, resource);
        }

        /// <summary>
        /// Get all mappings (for debugging/configuration UI)
        /// </summary>
        public static Dictionary<string, (string Module, string Resource)> GetAllMappings()
        {
            return new Dictionary<string, (string, string)>(ControllerMapping);
        }
    }

    /// <summary>
    /// All enterprise modules in HIS system
    /// </summary>
    public static class Modules
    {
        public const string Lookups = "Lookups";
        public const string Administration = "Administration";
        public const string EMR = "EMR";
        public const string Billing = "Billing";
        public const string LIS = "LIS";
        public const string Pharmacy = "Pharmacy";
        public const string Reports = "Reports";
        public const string Settings = "Settings";
        public const string General = "General";

        public static readonly string[] All = new[]
        {
            Lookups, Administration, EMR, Billing, LIS, Pharmacy, Reports, Settings
        };
    }
}
