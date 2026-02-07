namespace ENTERPRISE_HIS_WEBAPI.Models
{
    /// <summary>
    /// Represents a user role in the system
    /// </summary>
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>
    /// Represents a permission in the system
    /// </summary>
    public class Permission
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;  // e.g., "CanViewLookups"
        public string PermissionCode { get; set; } = string.Empty;  // e.g., "VIEW_LOOKUPS"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>
    /// Junction table: Maps roles to permissions (many-to-many)
    /// </summary>
    public class RolePermission
    {
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime AssignedDate { get; set; }

        public virtual Role? Role { get; set; }
        public virtual Permission? Permission { get; set; }
    }

    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    /// <summary>
    /// Junction table: Maps users to roles (many-to-many)
    /// </summary>
    public class UserRole
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime AssignedDate { get; set; }

        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }
    }
}
