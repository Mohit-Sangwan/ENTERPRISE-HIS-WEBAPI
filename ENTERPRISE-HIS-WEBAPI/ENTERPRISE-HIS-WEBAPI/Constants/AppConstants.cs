namespace ENTERPRISE_HIS_WEBAPI.Constants
{
    /// <summary>
    /// Application role constants for authorization
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
        public const string Viewer = "Viewer";
    }

    /// <summary>
    /// Authorization policy names
    /// </summary>
    public static class PolicyNames
    {
        public const string AdminOnly = "AdminOnly";
        public const string CanManageLookups = "CanManageLookups";
        public const string CanViewLookups = "CanViewLookups";
    }
}
