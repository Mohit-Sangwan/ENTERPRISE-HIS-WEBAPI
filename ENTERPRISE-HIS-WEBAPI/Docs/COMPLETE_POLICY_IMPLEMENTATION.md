# ?? DATABASE-LEVEL POLICY IMPLEMENTATION - COMPLETE GUIDE

## How Every Controller API Method Links with Database Policies

---

## ?? Complete Overview

### **System Architecture**

```
??????????????????????????????????????????????????????????
?             API ENDPOINT (Controller Method)           ?
?              [Authorize(Policy = "X")]                 ?
??????????????????????????????????????????????????????????
                       ?
??????????????????????????????????????????????????????????
?          Master.PolicyMaster (Database)                ?
?  ???????????????????????????????????????????????????   ?
?  ? PolicyId ? PolicyName     ? Code           ?   ?   ?
?  ?    1     ? CanViewLookups ? VIEW_LOOKUPS   ?   ?   ?
?  ?    2     ? CanManageLookups?MANAGE_LOOKUPS ?   ?   ?
?  ?    3     ? CanDeleteLookups?DELETE_LOOKUPS ?   ?   ?
?  ?    ...   ? ...            ? ...            ?   ?   ?
?  ???????????????????????????????????????????????????   ?
??????????????????????????????????????????????????????????
                       ?
??????????????????????????????????????????????????????????
?       Config.RolePolicyMapping (Database)              ?
?  ????????????????????????????????????????????????????  ?
?  ? RoleId ? PolicyId ? AssignedAt                  ?  ?
?  ?   1    ?    1     ? 2024-02-07 (Admin all)     ?  ?
?  ?   2    ?    1     ? 2024-02-07 (Manager view)  ?  ?
?  ?   3    ?    1     ? 2024-02-07 (User view)     ?  ?
?  ?   ...  ?   ...    ? ...                        ?  ?
?  ????????????????????????????????????????????????????  ?
??????????????????????????????????????????????????????????
                       ?
??????????????????????????????????????????????????????????
?    IPolicyService (In-Memory Cache)                   ?
?    Fast O(1) policy lookups                          ?
??????????????????????????????????????????????????????????
                       ?
??????????????????????????????????????????????????????????
?    User's Role Checked Against Policy                 ?
?    ? ALLOW or ? DENY                               ?
??????????????????????????????????????????????????????????
```

---

## ?? All 8 Database Policies

### **1. CanViewLookups**
- **PolicyId:** 1
- **PolicyCode:** VIEW_LOOKUPS
- **Module:** Lookups
- **Assigned To:** Admin, Manager, User, Viewer (ALL)

**Used In:**
```csharp
[Authorize(Policy = "CanViewLookups")]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "CanViewLookups")]
public async Task<ActionResult> GetById(int id) { }

[Authorize(Policy = "CanViewLookups")]
public async Task<ActionResult> GetByCode(string code) { }

[Authorize(Policy = "CanViewLookups")]
public async Task<ActionResult> Search() { }

[Authorize(Policy = "CanViewLookups")]
public async Task<ActionResult> GetCount() { }

// Same for LookupTypeValues endpoints
```

---

### **2. CanManageLookups**
- **PolicyId:** 2
- **PolicyCode:** MANAGE_LOOKUPS
- **Module:** Lookups
- **Assigned To:** Admin, Manager

**Used In:**
```csharp
[Authorize(Policy = "CanManageLookups")]
public async Task<ActionResult> Create([FromBody] CreateLookupTypeDto request) { }

[Authorize(Policy = "CanManageLookups")]
public async Task<ActionResult> Update(int id, [FromBody] UpdateLookupTypeDto request) { }

// Same for LookupTypeValues endpoints
```

---

### **3. CanDeleteLookups**
- **PolicyId:** 3
- **PolicyCode:** DELETE_LOOKUPS
- **Module:** Lookups
- **Assigned To:** Admin only

**Used In:**
```csharp
[Authorize(Policy = "CanDeleteLookups")]
public async Task<ActionResult> Delete(int id) { }

// Same for LookupTypeValues endpoints
```

---

### **4. CanViewUsers**
- **PolicyId:** 4
- **PolicyCode:** VIEW_USERS
- **Module:** Users
- **Assigned To:** Admin, Manager

**Used In:**
```csharp
[Authorize(Policy = "CanViewUsers")]
public async Task<ActionResult> GetUsers(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10) { }
```

---

### **5. CanManageUsers**
- **PolicyId:** 5
- **PolicyCode:** MANAGE_USERS
- **Module:** Users
- **Assigned To:** Admin only

**Used In:**
```csharp
[Authorize(Policy = "CanManageUsers")]
public async Task<ActionResult> CreateUser([FromBody] CreateUserDto request) { }

[Authorize(Policy = "CanManageUsers")]
public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request) { }
```

---

### **6. CanDeleteUsers**
- **PolicyId:** 6
- **PolicyCode:** DELETE_USERS
- **Module:** Users
- **Assigned To:** Admin only

**Used In:**
```csharp
[Authorize(Policy = "CanDeleteUsers")]
public async Task<ActionResult> DeleteUser(int id) { }
```

---

### **7. ManageRoles**
- **PolicyId:** 7
- **PolicyCode:** MANAGE_ROLES
- **Module:** Users
- **Assigned To:** Admin only

**Used In:**
```csharp
[Authorize(Policy = "ManageRoles")]
public async Task<ActionResult> AssignRole(int id, [FromBody] AssignRoleDto request) { }

[Authorize(Policy = "ManageRoles")]
public async Task<ActionResult> RemoveRole(int id, int roleId) { }
```

---

### **8. AdminOnly**
- **PolicyId:** 8
- **PolicyCode:** ADMIN_ONLY
- **Module:** System
- **Assigned To:** Admin only

**Used In:** Can be used for critical system operations

---

## ?? Complete Controller Implementation

### **LookupTypesController**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]  // All endpoints require authentication
public class LookupTypesController : ControllerBase
{
    // ========== VIEW OPERATIONS ==========
    
    /// GET /api/v1/lookuptypes
    [HttpGet]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> GetAll()
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// GET /api/v1/lookuptypes/{id}
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> GetById(int id)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// GET /api/v1/lookuptypes/code/{code}
    [HttpGet("code/{code}")]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> GetByCode(string code)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// GET /api/v1/lookuptypes/search?searchTerm=value
    [HttpGet("search")]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> Search()
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// GET /api/v1/lookuptypes/count
    [HttpGet("count")]
    [Authorize(Policy = "CanViewLookups")]
    public async Task<ActionResult> GetCount()
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    // ========== CREATE/EDIT OPERATIONS ==========
    
    /// POST /api/v1/lookuptypes
    [HttpPost]
    [Authorize(Policy = "CanManageLookups")]
    public async Task<ActionResult> Create([FromBody] CreateLookupTypeDto request)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// PUT /api/v1/lookuptypes/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageLookups")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateLookupTypeDto request)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    // ========== DELETE OPERATIONS ==========
    
    /// DELETE /api/v1/lookuptypes/{id}
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteLookups")]
    public async Task<ActionResult> Delete(int id)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
}

// LookupTypeValuesController has IDENTICAL permissions!
```

---

### **UsersController**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]  // All endpoints require authentication
public class UsersController : ControllerBase
{
    // ========== VIEW OPERATIONS ==========
    
    /// GET /api/v1/users?pageNumber=1&pageSize=10
    [HttpGet]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<ActionResult> GetUsers()
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// GET /api/v1/users/{id}
    [HttpGet("{id}")]
    [Authorize]  // Any authenticated user can view their own info
    public async Task<ActionResult> GetUserById(int id)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// GET /api/v1/users/username/{username}
    [HttpGet("username/{username}")]
    [Authorize]
    public async Task<ActionResult> GetUserByUsername(string username)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    // ========== CREATE/EDIT OPERATIONS ==========
    
    /// POST /api/v1/users
    [HttpPost]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// PUT /api/v1/users/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    // ========== DELETE OPERATIONS ==========
    
    /// DELETE /api/v1/users/{id}
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteUsers")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    // ========== ROLE MANAGEMENT ==========
    
    /// POST /api/v1/users/{id}/roles
    [HttpPost("{id}/roles")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult> AssignRole(int id, [FromBody] AssignRoleDto request)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    /// DELETE /api/v1/users/{id}/roles/{roleId}
    [HttpDelete("{id}/roles/{roleId}")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult> RemoveRole(int id, int roleId)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
    
    // ========== PASSWORD OPERATIONS ==========
    
    /// POST /api/v1/users/{id}/change-password
    [HttpPost("{id}/change-password")]
    [Authorize]  // Any authenticated user can change own password
    public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto request)
    {
        // Admin ?, Manager ?, User ?, Viewer ?
    }
}
```

---

## ?? Request Processing Example

### **Scenario: Manager Creates a Lookup**

```
1. Manager logs in
   POST /api/auth/login
   {username: "manager", password: "manager123"}
   ? JWT Token: {userId: 2, role: "Manager"}

2. Manager creates lookup
   POST /api/v1/lookuptypes
   Authorization: Bearer <jwt_token>
   Body: {name: "Test Lookup"}
   
3. Request reaches LookupTypesController.Create()
   
4. [Authorize(Policy = "CanManageLookups")] attribute triggers
   
5. DatabasePolicyHandler.HandleRequirementAsync() runs:
   a) Extract userId from JWT ? 2
   b) Extract role from JWT ? "Manager"
   
6. IPolicyService.UserHasPolicyAsync(2, "CanManageLookups")
   a) Query: SELECT RoleId FROM config.UserRole WHERE UserId = 2
      ? RoleId = 2 (Manager)
   
   b) Query: SELECT PolicyId FROM config.RolePolicyMapping 
      WHERE RoleId = 2
      ? PolicyIds = [1, 2, 4, 5]
      ? Includes PolicyId = 2 (CanManageLookups) ?
   
7. context.Succeed(requirement)
   
8. Create() method executes
   ? Lookup created successfully
   
9. Response: 201 Created
```

---

### **Scenario: User Tries to Delete a Lookup**

```
1. User logs in
   JWT Token: {userId: 3, role: "User"}

2. User tries to delete lookup
   DELETE /api/v1/lookuptypes/1
   Authorization: Bearer <jwt_token>
   
3. Request reaches LookupTypesController.Delete()
   
4. [Authorize(Policy = "CanDeleteLookups")] attribute triggers
   
5. DatabasePolicyHandler.HandleRequirementAsync() runs:
   a) Extract userId from JWT ? 3
   b) Extract role from JWT ? "User"
   
6. IPolicyService.UserHasPolicyAsync(3, "CanDeleteLookups")
   a) Query: SELECT RoleId FROM config.UserRole WHERE UserId = 3
      ? RoleId = 3 (User)
   
   b) Query: SELECT PolicyId FROM config.RolePolicyMapping 
      WHERE RoleId = 3
      ? PolicyIds = [1, 4]
      ? Does NOT include PolicyId = 3 (CanDeleteLookups) ?
   
7. context.Fail()
   
8. Delete() method is NOT executed
   
9. Response: 403 Forbidden
   {error: "Access denied"}
```

---

## ?? Role-Policy Assignment (From Database)

```sql
-- Admin (RoleId = 1) gets ALL policies
INSERT INTO config.RolePolicyMapping VALUES
(1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8);

-- Manager (RoleId = 2) gets policies 1,2,4,5
INSERT INTO config.RolePolicyMapping VALUES
(2, 1), (2, 2), (2, 4), (2, 5);

-- User (RoleId = 3) gets policies 1,4
INSERT INTO config.RolePolicyMapping VALUES
(3, 1), (3, 4);

-- Viewer (RoleId = 4) gets policy 1
INSERT INTO config.RolePolicyMapping VALUES
(4, 1);
```

---

## ? Implementation Checklist

- [x] Database tables created (master.PolicyMaster, config.RolePolicyMapping)
- [x] 8 default policies seeded
- [x] Default role-policy mappings created
- [x] LookupTypesController fully protected with policies
- [x] LookupTypeValuesController fully protected with policies
- [x] UsersController fully protected with policies
- [x] PolicyManagementController for runtime administration
- [x] IPolicyService with in-memory caching
- [x] DatabasePolicyHandler for authorization
- [x] Complete documentation created
- [x] All endpoints tested

---

## ?? Key Takeaways

1. **Every endpoint** is protected by `[Authorize(Policy = "...")]`
2. **Every policy** is stored in `master.PolicyMaster` database
3. **Every role-policy mapping** is in `config.RolePolicyMapping`
4. **No hardcoding** - all policies in database
5. **Caching** - in-memory for performance (O(1) lookups)
6. **Runtime changes** - modify policies without code redeploy
7. **Admin** - has all 8 policies, can do anything
8. **Manager** - has 4 policies (view + manage, no delete)
9. **User** - has 2 policies (view only)
10. **Viewer** - has 1 policy (limited view)

---

**Every controller method now links to a database policy!** ??
