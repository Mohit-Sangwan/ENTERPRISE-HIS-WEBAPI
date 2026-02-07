# ?? ENTERPRISE-LEVEL POLICY NAMING CONVENTIONS

## Replace Specific Names with Generic Enterprise Patterns

---

## ?? Current vs Enterprise-Level Naming

### **Current (Specific)**
```csharp
[Authorize(Policy = "CanViewLookups")]
[Authorize(Policy = "CanManageLookups")]
[Authorize(Policy = "CanDeleteLookups")]
[Authorize(Policy = "CanViewUsers")]
// ... specific to each module
```

### **Enterprise-Level (Generic & Reusable)**
```csharp
[Authorize(Policy = "View")]
[Authorize(Policy = "Create")]
[Authorize(Policy = "Update")]
[Authorize(Policy = "Delete")]
[Authorize(Policy = "Manage")]
[Authorize(Policy = "Admin")]
// ... used across ALL modules
```

---

## ?? Enterprise Policy Framework

### **5 Core Enterprise Policies**

```sql
-- master.PolicyMaster
PolicyId | PolicyName | PolicyCode | Module      | IsActive | Description
---------|------------|------------|-------------|----------|----------------------------
1        | View       | POLICY_VIEW| Core        | 1        | View any resource
2        | Create     | POLICY_CREATE| Core      | 1        | Create resources
3        | Update     | POLICY_UPDATE| Core      | 1        | Update resources
4        | Delete     | POLICY_DELETE| Core      | 1        | Delete resources
5        | Manage     | POLICY_MANAGE| Core      | 1        | Full resource management
6        | Admin      | POLICY_ADMIN| Core       | 1        | System administration
7        | ViewOwn    | POLICY_VIEWOWN| Core    | 1        | View own resources only
8        | ManageOwn  | POLICY_MANAGEOWN| Core  | 1        | Manage own resources only
```

---

## ?? Role-Based Policy Mapping (Enterprise)

### **Admin Role**
```sql
-- Gets all policies
INSERT INTO config.RolePolicyMapping (RoleId, PolicyId) VALUES
(1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6);
-- View, Create, Update, Delete, Manage, Admin
```

### **Manager Role**
```sql
-- Gets View, Create, Update, Manage (no Delete/Admin)
INSERT INTO config.RolePolicyMapping (RoleId, PolicyId) VALUES
(2, 1), (2, 2), (2, 3), (2, 5);
-- View, Create, Update, Manage
```

### **User Role**
```sql
-- Gets View and ViewOwn only
INSERT INTO config.RolePolicyMapping (RoleId, PolicyId) VALUES
(3, 1), (3, 7);
-- View, ViewOwn
```

### **Viewer Role**
```sql
-- Gets View only
INSERT INTO config.RolePolicyMapping (RoleId, PolicyId) VALUES
(4, 1);
-- View
```

---

## ?? Enterprise Usage Pattern

### **For ANY Module (Lookups, Users, Products, Orders, etc.)**

```csharp
// ============ VIEW OPERATIONS ============
[Authorize(Policy = "View")]
[HttpGet]
public async Task<ActionResult> GetAll() { }
// Admin ?, Manager ?, User ?, Viewer ?

[Authorize(Policy = "View")]
[HttpGet("{id}")]
public async Task<ActionResult> GetById(int id) { }

[Authorize(Policy = "ViewOwn")]
[HttpGet("mine")]
public async Task<ActionResult> GetOwnResources() { }
// User can only see their own resources

// ============ CREATE OPERATIONS ============
[Authorize(Policy = "Create")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }
// Admin ?, Manager ?, User ?, Viewer ?

// ============ UPDATE OPERATIONS ============
[Authorize(Policy = "Update")]
[HttpPut("{id}")]
public async Task<ActionResult> Update(int id, [FromBody] UpdateDto request) { }
// Admin ?, Manager ?, User ?, Viewer ?

// ============ DELETE OPERATIONS ============
[Authorize(Policy = "Delete")]
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(int id) { }
// Admin ?, Manager ?, User ?, Viewer ?

// ============ FULL MANAGEMENT ============
[Authorize(Policy = "Manage")]
[HttpPost("{id}/activate")]
public async Task<ActionResult> ActivateResource(int id) { }
// Admin ?, Manager ?, User ?, Viewer ?

// ============ ADMIN OPERATIONS ============
[Authorize(Policy = "Admin")]
[HttpPost("bulk-delete")]
public async Task<ActionResult> BulkDelete([FromBody] int[] ids) { }
// Admin ? only
```

---

## ?? Complete Enterprise Policy Matrix

```
????????????????????????????????????????????????????
? Policy          ? View ? Create  ? Edit ? Delete ?
????????????????????????????????????????????????????
? POLICY_VIEW     ? ?   ? ?      ? ?   ? ?     ?
? POLICY_CREATE   ? ?   ? ?      ? ?   ? ?     ?
? POLICY_UPDATE   ? ?   ? ?      ? ?   ? ?     ?
? POLICY_DELETE   ? ?   ? ?      ? ?   ? ?     ?
? POLICY_MANAGE   ? ?   ? ?      ? ?   ? ?     ?
? POLICY_ADMIN    ? ?   ? ?      ? ?   ? ?     ?
????????????????????????????????????????????????????

Role Access:
Admin   ? All policies
Manager ? View, Create, Update, Manage
User    ? View, ViewOwn
Viewer  ? View only
```

---

## ?? Migration Guide: Specific ? Enterprise

### **Before (Specific)**
```csharp
// LookupTypesController
[Authorize(Policy = "CanViewLookups")]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "CanManageLookups")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }

[Authorize(Policy = "CanDeleteLookups")]
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(int id) { }

// UsersController (same pattern repeated)
[Authorize(Policy = "CanViewUsers")]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "CanManageUsers")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }

// ProductsController (future)
[Authorize(Policy = "CanViewProducts")]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "CanManageProducts")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }
```

### **After (Enterprise)**
```csharp
// LookupTypesController
[Authorize(Policy = "View")]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "Create")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }

[Authorize(Policy = "Delete")]
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(int id) { }

// UsersController (same patterns work!)
[Authorize(Policy = "View")]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "Create")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }

// ProductsController (same patterns!)
[Authorize(Policy = "View")]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "Create")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }

// OrdersController (same patterns!)
[Authorize(Policy = "View")]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "Create")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }
```

---

## ?? Benefits of Enterprise Naming

### **Specific Naming (Old)**
? Duplicate policies per module  
? Hard to manage (8+ policies for just 2 modules)  
? Not scalable (Add new module = add 3+ new policies)  
? Inconsistent patterns  
? Complex role-policy mapping  

### **Enterprise Naming (New)**
? Single set of 6-8 core policies  
? Works across ALL modules  
? Highly scalable (add module without new policies)  
? Consistent patterns  
? Simple role-policy mapping  
? Easy to understand and maintain  

---

## ?? SQL Migration Script

```sql
-- Step 1: Update existing policies to enterprise names
UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'View', 
    [PolicyCode] = 'POLICY_VIEW'
WHERE [PolicyCode] IN ('VIEW_LOOKUPS', 'VIEW_USERS');

UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'Create', 
    [PolicyCode] = 'POLICY_CREATE'
WHERE [PolicyCode] IN ('MANAGE_LOOKUPS', 'MANAGE_USERS');

UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'Update', 
    [PolicyCode] = 'POLICY_UPDATE'
WHERE [PolicyCode] LIKE '%MANAGE%';

UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'Delete', 
    [PolicyCode] = 'POLICY_DELETE'
WHERE [PolicyCode] LIKE '%DELETE%';

UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'Admin', 
    [PolicyCode] = 'POLICY_ADMIN'
WHERE [PolicyCode] = 'ADMIN_ONLY';

-- Step 2: Add missing enterprise policies
INSERT INTO [master].[PolicyMaster] ([PolicyName], [PolicyCode], [Description], [Module], [IsActive])
VALUES 
('Manage', 'POLICY_MANAGE', 'Full resource management', 'Core', 1),
('ViewOwn', 'POLICY_VIEWOWN', 'View own resources only', 'Core', 1),
('ManageOwn', 'POLICY_MANAGEOWN', 'Manage own resources only', 'Core', 1);

-- Step 3: Keep role mappings simple (they reference PolicyId, which stays the same)
-- No changes needed to config.RolePolicyMapping!
```

---

## ??? Enterprise Policy Constants

```csharp
// Create a constants file: PolicyConstants.cs
public static class PolicyConstants
{
    // Core Enterprise Policies
    public const string VIEW = "View";
    public const string CREATE = "Create";
    public const string UPDATE = "Update";
    public const string DELETE = "Delete";
    public const string MANAGE = "Manage";
    public const string ADMIN = "Admin";
    public const string VIEW_OWN = "ViewOwn";
    public const string MANAGE_OWN = "ManageOwn";

    // Policy Codes (for database)
    public const string VIEW_CODE = "POLICY_VIEW";
    public const string CREATE_CODE = "POLICY_CREATE";
    public const string UPDATE_CODE = "POLICY_UPDATE";
    public const string DELETE_CODE = "POLICY_DELETE";
    public const string MANAGE_CODE = "POLICY_MANAGE";
    public const string ADMIN_CODE = "POLICY_ADMIN";
    public const string VIEW_OWN_CODE = "POLICY_VIEWOWN";
    public const string MANAGE_OWN_CODE = "POLICY_MANAGEOWN";
}

// Usage in controller:
[Authorize(Policy = PolicyConstants.VIEW)]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = PolicyConstants.CREATE)]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }
```

---

## ?? Complete Policy Configuration (Enterprise)

```csharp
// Program.cs or Authorization configuration
public static void AddEnterpriseAuthorizationPolicies(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    services.AddAuthorization(options =>
    {
        // Core Enterprise Policies
        options.AddPolicy(PolicyConstants.VIEW, policy =>
            policy.Requirements.Add(new DatabasePolicyRequirement(PolicyConstants.VIEW)));
        
        options.AddPolicy(PolicyConstants.CREATE, policy =>
            policy.Requirements.Add(new DatabasePolicyRequirement(PolicyConstants.CREATE)));
        
        options.AddPolicy(PolicyConstants.UPDATE, policy =>
            policy.Requirements.Add(new DatabasePolicyRequirement(PolicyConstants.UPDATE)));
        
        options.AddPolicy(PolicyConstants.DELETE, policy =>
            policy.Requirements.Add(new DatabasePolicyRequirement(PolicyConstants.DELETE)));
        
        options.AddPolicy(PolicyConstants.MANAGE, policy =>
            policy.Requirements.Add(new DatabasePolicyRequirement(PolicyConstants.MANAGE)));
        
        options.AddPolicy(PolicyConstants.ADMIN, policy =>
            policy.Requirements.Add(new DatabasePolicyRequirement(PolicyConstants.ADMIN)));
        
        options.AddPolicy(PolicyConstants.VIEW_OWN, policy =>
            policy.Requirements.Add(new DatabasePolicyRequirement(PolicyConstants.VIEW_OWN)));
        
        options.AddPolicy(PolicyConstants.MANAGE_OWN, policy =>
            policy.Requirements.Add(new DatabasePolicyRequirement(PolicyConstants.MANAGE_OWN)));
    });
}
```

---

## ?? Implementation Steps

### **Step 1: Update Database (SQL)**
```sql
-- Run the migration script above to rename policies to enterprise names
```

### **Step 2: Create Constants**
```csharp
// Create PolicyConstants.cs
public static class PolicyConstants
{
    public const string VIEW = "View";
    public const string CREATE = "Create";
    public const string UPDATE = "Update";
    public const string DELETE = "Delete";
    public const string MANAGE = "Manage";
    public const string ADMIN = "Admin";
}
```

### **Step 3: Update All Controllers**
```csharp
// Replace all instances:
// [Authorize(Policy = "CanViewLookups")] ? [Authorize(Policy = PolicyConstants.VIEW)]
// [Authorize(Policy = "CanManageLookups")] ? [Authorize(Policy = PolicyConstants.CREATE)]
// etc.
```

### **Step 4: Update Program.cs**
```csharp
// Register enterprise policies in DI
services.AddEnterpriseAuthorizationPolicies(configuration);
```

### **Step 5: Test**
```bash
# All existing tests should pass without changes
# Admin still has full access
# Manager still can't delete
# User still has limited access
```

---

## ?? Before & After Comparison

### **Before (Specific)**
- **Policies per module:** 3+ (View, Manage, Delete)
- **Total policies:** 8+ (for 2 modules)
- **Scalability:** Hard (need new policy per module)
- **Code duplication:** High
- **Database complexity:** High

### **After (Enterprise)**
- **Core policies:** 6-8 (used everywhere)
- **Total policies:** 6-8 (same for 100 modules!)
- **Scalability:** Easy (use existing policies)
- **Code duplication:** None
- **Database complexity:** Low

---

## ? Checklist

- [ ] Update database policies to enterprise names
- [ ] Create PolicyConstants.cs
- [ ] Update all controllers to use PolicyConstants
- [ ] Update Program.cs authorization setup
- [ ] Update documentation
- [ ] Run all tests
- [ ] Verify role permissions still work
- [ ] Deploy to production

---

## ?? Result

**Every controller, every module, every resource:**
```csharp
[Authorize(Policy = PolicyConstants.VIEW)]    // View data
[Authorize(Policy = PolicyConstants.CREATE)]  // Create
[Authorize(Policy = PolicyConstants.UPDATE)]  // Edit
[Authorize(Policy = PolicyConstants.DELETE)]  // Delete
[Authorize(Policy = PolicyConstants.MANAGE)]  // Full management
[Authorize(Policy = PolicyConstants.ADMIN)]   // Admin operations
```

**One set of policies. Infinite modules. Enterprise-grade security.** ??
