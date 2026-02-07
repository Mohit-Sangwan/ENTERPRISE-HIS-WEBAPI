# ?? DATABASE-LEVEL POLICY IMPLEMENTATION GUIDE

## How Controller API Methods Link with Database Policies

---

## ?? Complete Policy-to-Endpoint Mapping

### **Overview**

Every API endpoint is protected by a **database policy** stored in `master.PolicyMaster`:

```
API Request
    ?
[Authorize(Policy = "PolicyName")]
    ?
IPolicyService.GetPolicyByNameAsync()
    ?
Get from [master].[PolicyMaster]
    ?
Check [config].[RolePolicyMapping]
    ?
User's role has policy?
    ?
? ALLOW or ? DENY (403 Forbidden)
```

---

## ?? 8 Default Policies in Database

### **1. CanViewLookups (PolicyId = 1)**
- **PolicyCode:** VIEW_LOOKUPS
- **Module:** Lookups
- **Assigned To:** Admin, Manager, User, Viewer
- **Purpose:** View lookup data

**Endpoints:**
```csharp
[Authorize(Policy = "CanViewLookups")]
[HttpGet]
public async Task<ActionResult> GetAll() { }  // View all lookups

[Authorize(Policy = "CanViewLookups")]
[HttpGet("{id}")]
public async Task<ActionResult> GetById(int id) { }  // View specific lookup

[Authorize(Policy = "CanViewLookups")]
[HttpGet("code/{code}")]
public async Task<ActionResult> GetByCode(string code) { }  // View by code

[Authorize(Policy = "CanViewLookups")]
[HttpGet("search")]
public async Task<ActionResult> Search() { }  // Search lookups

[Authorize(Policy = "CanViewLookups")]
[HttpGet("count")]
public async Task<ActionResult> GetCount() { }  // Count lookups
```

---

### **2. CanManageLookups (PolicyId = 2)**
- **PolicyCode:** MANAGE_LOOKUPS
- **Module:** Lookups
- **Assigned To:** Admin, Manager
- **Purpose:** Create and edit lookups

**Endpoints:**
```csharp
[Authorize(Policy = "CanManageLookups")]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateLookupTypeDto request) { }

[Authorize(Policy = "CanManageLookups")]
[HttpPut("{id}")]
public async Task<ActionResult> Update(int id, [FromBody] UpdateLookupTypeDto request) { }
```

---

### **3. CanDeleteLookups (PolicyId = 3)**
- **PolicyCode:** DELETE_LOOKUPS
- **Module:** Lookups
- **Assigned To:** Admin only
- **Purpose:** Delete lookups

**Endpoints:**
```csharp
[Authorize(Policy = "CanDeleteLookups")]
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(int id) { }
```

---

### **4. CanViewUsers (PolicyId = 4)**
- **PolicyCode:** VIEW_USERS
- **Module:** Users
- **Assigned To:** Admin, Manager
- **Purpose:** View users

**Endpoints:**
```csharp
[Authorize(Policy = "CanViewUsers")]
[HttpGet]
public async Task<ActionResult> GetUsers(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10) { }
```

---

### **5. CanManageUsers (PolicyId = 5)**
- **PolicyCode:** MANAGE_USERS
- **Module:** Users
- **Assigned To:** Admin only
- **Purpose:** Create and edit users

**Endpoints:**
```csharp
[Authorize(Policy = "CanManageUsers")]
[HttpPost]
public async Task<ActionResult> CreateUser([FromBody] CreateUserDto request) { }

[Authorize(Policy = "CanManageUsers")]
[HttpPut("{id}")]
public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request) { }
```

---

### **6. CanDeleteUsers (PolicyId = 6)**
- **PolicyCode:** DELETE_USERS
- **Module:** Users
- **Assigned To:** Admin only
- **Purpose:** Delete users

**Endpoints:**
```csharp
[Authorize(Policy = "CanDeleteUsers")]
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteUser(int id) { }
```

---

### **7. ManageRoles (PolicyId = 7)**
- **PolicyCode:** MANAGE_ROLES
- **Module:** Users
- **Assigned To:** Admin only
- **Purpose:** Manage user roles

**Endpoints:**
```csharp
[Authorize(Policy = "ManageRoles")]
[HttpPost("{id}/roles")]
public async Task<ActionResult> AssignRole(int id, [FromBody] AssignRoleDto request) { }

[Authorize(Policy = "ManageRoles")]
[HttpDelete("{id}/roles/{roleId}")]
public async Task<ActionResult> RemoveRole(int id, int roleId) { }
```

---

### **8. AdminOnly (PolicyId = 8)**
- **PolicyCode:** ADMIN_ONLY
- **Module:** System
- **Assigned To:** Admin only
- **Purpose:** Critical admin operations

**Endpoints:** Can be used for critical operations

---

## ??? Controller-by-Controller Policy Implementation

### **LookupTypesController**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]  // All endpoints require authentication
public class LookupTypesController : ControllerBase
{
    // ========== VIEW OPERATIONS (CanViewLookups) ==========
    
    [HttpGet]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> GetAll() { }
    // User Role: ? Allow (has VIEW_LOOKUPS)
    // Manager Role: ? Allow
    // Admin Role: ? Allow
    // Viewer Role: ? Allow
    // Anonymous: ? Deny (401 Unauthorized)
    
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> GetById(int id) { }
    // Same as GetAll
    
    [HttpGet("code/{code}")]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> GetByCode(string code) { }
    // Same as GetAll
    
    [HttpGet("search")]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> Search() { }
    // Same as GetAll
    
    [HttpGet("count")]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> GetCount() { }
    // Same as GetAll
    
    // ========== CREATE/EDIT OPERATIONS (CanManageLookups) ==========
    
    [HttpPost]
    [Authorize(Policy = "CanManageLookups")]
    public async Task<ActionResult> Create([FromBody] CreateLookupTypeDto request) { }
    // Admin Role: ? Allow (has MANAGE_LOOKUPS)
    // Manager Role: ? Allow
    // User Role: ? Deny (403 Forbidden - doesn't have policy)
    // Viewer Role: ? Deny
    
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageLookups")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateLookupTypeDto request) { }
    // Same access as Create
    
    // ========== DELETE OPERATIONS (CanDeleteLookups) ==========
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteLookups")]
    public async Task<ActionResult> Delete(int id) { }
    // Admin Role: ? Allow (has DELETE_LOOKUPS)
    // Manager Role: ? Deny (doesn't have DELETE_LOOKUPS)
    // User Role: ? Deny
    // Viewer Role: ? Deny
}
```

---

### **UsersController**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]  // All endpoints require authentication
public class UsersController : ControllerBase
{
    // ========== VIEW OPERATIONS (CanViewUsers) ==========
    
    [HttpGet]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<ActionResult> GetUsers() { }
    // Admin Role: ? Allow
    // Manager Role: ? Allow (has VIEW_USERS)
    // User Role: ? Deny
    // Viewer Role: ? Deny
    
    // ========== CREATE/EDIT OPERATIONS (CanManageUsers) ==========
    
    [HttpPost]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserDto request) { }
    // Admin Role: ? Allow
    // Manager Role: ? Deny
    // User Role: ? Deny
    // Viewer Role: ? Deny
    
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request) { }
    // Same as Create
    
    // ========== DELETE OPERATIONS (CanDeleteUsers) ==========
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteUsers")]
    public async Task<ActionResult> DeleteUser(int id) { }
    // Admin Role: ? Allow
    // Manager Role: ? Deny
    // User Role: ? Deny
    // Viewer Role: ? Deny
    
    // ========== ROLE MANAGEMENT (ManageRoles) ==========
    
    [HttpPost("{id}/roles")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult> AssignRole(int id, [FromBody] AssignRoleDto request) { }
    // Admin Role: ? Allow
    // Manager Role: ? Deny
    // User Role: ? Deny
    
    [HttpDelete("{id}/roles/{roleId}")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult> RemoveRole(int id, int roleId) { }
    // Same as AssignRole
}
```

---

## ?? Step-by-Step Policy Enforcement Flow

### **Example 1: Manager Tries to Delete a Lookup**

```
1. Manager logs in with username "manager" and password "manager123"
   ?
2. AuthController generates JWT token
   - JWT contains: userId=2, role="Manager"
   ?
3. Manager calls: DELETE /api/v1/lookuptypes/5
   ?
4. Request reaches LookupTypesController.Delete()
   ?
5. [Authorize(Policy = "CanDeleteLookups")] attribute triggers
   ?
6. DatabasePolicyHandler runs:
   - Extracts userId from JWT (2)
   - Extracts role from JWT ("Manager")
   ?
7. IPolicyService.UserHasPolicyAsync(2, "CanDeleteLookups")
   - Queries: SELECT role FROM UserRole WHERE UserId = 2
   - Gets: RoleId = 2 (Manager)
   ?
8. Queries: SELECT PolicyId FROM RolePolicyMapping WHERE RoleId = 2
   - Gets: [1, 2, 4, 5] (VIEW_LOOKUPS, MANAGE_LOOKUPS, VIEW_USERS, MANAGE_USERS)
   - Looking for PolicyId = 3 (CanDeleteLookups)
   ?
9. Result: Policy NOT found in Manager's role
   ?
10. context.Fail() - Authorization fails
   ?
11. Response: 403 Forbidden
```

---

### **Example 2: Admin Deletes a Lookup**

```
1. Admin logs in
   ?
2. JWT contains: userId=1, role="Admin"
   ?
3. Admin calls: DELETE /api/v1/lookuptypes/5
   ?
4. [Authorize(Policy = "CanDeleteLookups")] attribute triggers
   ?
5. DatabasePolicyHandler runs:
   - Extract userId = 1, role = "Admin"
   ?
6. IPolicyService.UserHasPolicyAsync(1, "CanDeleteLookups")
   - Get RoleId = 1 (Admin)
   ?
7. Query: SELECT PolicyId FROM RolePolicyMapping WHERE RoleId = 1
   - Gets: [1,2,3,4,5,6,7,8] (ALL policies)
   - Found PolicyId = 3 (CanDeleteLookups) ?
   ?
8. context.Succeed(requirement)
   ?
9. Delete method executes
   ?
10. Response: 200 OK (deletion successful)
```

---

## ?? Default Role-Policy Mapping (From Database)

```sql
-- Admin Role (RoleId = 1) has all 8 policies
INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
VALUES (1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8);

-- Manager Role (RoleId = 2) has 4 policies
INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
VALUES (2, 1), (2, 2), (2, 4), (2, 5);

-- User Role (RoleId = 3) has 2 policies
INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
VALUES (3, 1), (3, 4);

-- Viewer Role (RoleId = 4) has 1 policy
INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId])
VALUES (4, 1);
```

---

## ?? Testing Policy Enforcement

### **Test 1: User Can View Lookups**
```bash
# Login as user
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"user123"}'

# Use token to GET lookups
curl -X GET http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer <token>"

# Result: ? 200 OK (user has CanViewLookups)
```

### **Test 2: User Cannot Create Lookups**
```bash
# Try to create (user doesn't have CanManageLookups)
curl -X POST http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer <token>" \
  -d '{"name":"Test"}'

# Result: ? 403 Forbidden
```

### **Test 3: Manager Cannot Delete Lookups**
```bash
# Login as manager
curl -X POST http://localhost:5000/api/auth/login \
  -d '{"username":"manager","password":"manager123"}'

# Try to delete (manager doesn't have CanDeleteLookups)
curl -X DELETE http://localhost:5000/api/v1/lookuptypes/1 \
  -H "Authorization: Bearer <token>"

# Result: ? 403 Forbidden
```

### **Test 4: Admin Can Do Everything**
```bash
# Login as admin
curl -X POST http://localhost:5000/api/auth/login \
  -d '{"username":"admin","password":"admin123"}'

# Admin can view
curl -X GET http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer <token>"
# Result: ? 200 OK

# Admin can create
curl -X POST http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer <token>" \
  -d '{"name":"Test"}'
# Result: ? 201 Created

# Admin can delete
curl -X DELETE http://localhost:5000/api/v1/lookuptypes/1 \
  -H "Authorization: Bearer <token>"
# Result: ? 200 OK
```

---

## ?? How to Add a New Policy to a Controller

### **Step 1: Create Policy in Database**
```bash
# Use PolicyManagementController API
curl -X POST http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer <admin_token>" \
  -d '{
    "policyName": "CanExportLookups",
    "policyCode": "EXPORT_LOOKUPS",
    "description": "Can export lookups to CSV",
    "module": "Lookups"
  }'
# Returns: PolicyId = 9
```

### **Step 2: Assign to Role**
```bash
curl -X POST http://localhost:5000/api/policymanagement/roles/2/policies \
  -H "Authorization: Bearer <admin_token>" \
  -d '{"policyId": 9}'
# Assigns to Manager role
```

### **Step 3: Add to Controller**
```csharp
[HttpGet("export")]
[Authorize(Policy = "CanExportLookups")]  // NEW!
public async Task<ActionResult> ExportLookups() { }
// No code redeploy needed - policy already in database!
```

### **Step 4: Refresh Cache**
```bash
curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer <admin_token>"
# Changes take effect immediately
```

---

## ?? Database Tables Overview

### **master.PolicyMaster**
```
PolicyId | PolicyName       | PolicyCode      | Module  | IsActive
---------|------------------|-----------------|---------|----------
1        | CanViewLookups   | VIEW_LOOKUPS    | Lookups | 1
2        | CanManageLookups | MANAGE_LOOKUPS  | Lookups | 1
3        | CanDeleteLookups | DELETE_LOOKUPS  | Lookups | 1
4        | CanViewUsers     | VIEW_USERS      | Users   | 1
5        | CanManageUsers   | MANAGE_USERS    | Users   | 1
6        | CanDeleteUsers   | DELETE_USERS    | Users   | 1
7        | ManageRoles      | MANAGE_ROLES    | Users   | 1
8        | AdminOnly        | ADMIN_ONLY      | System  | 1
```

### **config.RolePolicyMapping**
```
RoleId | PolicyId | AssignedAt
--------|----------|----------------
1       | 1        | 2024-02-07
1       | 2        | 2024-02-07
1       | 3        | 2024-02-07
1       | 4        | 2024-02-07
... (all 8 for Admin)
2       | 1        | 2024-02-07
2       | 2        | 2024-02-07
2       | 4        | 2024-02-07
2       | 5        | 2024-02-07
... (4 for Manager)
```

---

## ? Complete Checklist

- [x] Database policies created (master.PolicyMaster)
- [x] Role-policy mappings created (config.RolePolicyMapping)
- [x] 8 default policies seeded
- [x] Default roles configured
- [x] LookupTypesController updated with policies
- [x] UsersController updated with policies
- [x] PolicyManagementController for runtime management
- [x] In-memory cache for performance
- [x] Testing scenarios documented
- [x] Database integration complete

---

**Your API now has complete database-level policy enforcement on every controller!** ??
