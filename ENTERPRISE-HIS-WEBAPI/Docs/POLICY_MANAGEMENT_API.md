# ?? POLICY MANAGEMENT API - RUNTIME POLICY ADMINISTRATION

## ? Manage Policies Without Hardcoding or Redeploy!

---

## ?? Overview

The **Policy Management API** allows administrators to dynamically manage all policies at runtime:
- Create new policies
- Update existing policies
- Delete unused policies
- Assign/Remove policies from roles
- Refresh cache
- View statistics

**No hardcoding. No redeploy. No downtime!**

---

## ?? Access Control

**Who Can Access:**
- ? Admin role only
- ? Manager, User, Viewer: Denied (403 Forbidden)

**Authentication:**
- Required: Bearer token in Authorization header
- Must be logged in as Admin

---

## ?? API Endpoints

### **1. Get All Policies**

```http
GET /api/policymanagement/policies
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
[
  {
    "policyId": 1,
    "policyName": "CanViewLookups",
    "policyCode": "VIEW_LOOKUPS",
    "description": "Can view lookup types and values",
    "module": "Lookups",
    "isActive": true,
    "createdAt": "2024-02-07T10:00:00Z"
  },
  {
    "policyId": 2,
    "policyName": "CanManageLookups",
    "policyCode": "MANAGE_LOOKUPS",
    "description": "Can create and edit lookup types and values",
    "module": "Lookups",
    "isActive": true,
    "createdAt": "2024-02-07T10:00:00Z"
  }
  // ... more policies
]
```

---

### **2. Get Specific Policy**

```http
GET /api/policymanagement/policies/{policyName}
Authorization: Bearer <token>

Example:
GET /api/policymanagement/policies/CanViewLookups
```

**Response (200 OK):**
```json
{
  "policyId": 1,
  "policyName": "CanViewLookups",
  "policyCode": "VIEW_LOOKUPS",
  "description": "Can view lookup types and values",
  "module": "Lookups",
  "isActive": true,
  "createdAt": "2024-02-07T10:00:00Z"
}
```

**Response (404 Not Found):**
```json
{
  "error": "Policy 'NonExistent' not found"
}
```

---

### **3. Create New Policy**

```http
POST /api/policymanagement/policies
Authorization: Bearer <token>
Content-Type: application/json

{
  "policyName": "CanExportLookups",
  "policyCode": "EXPORT_LOOKUPS",
  "description": "Can export lookup data to CSV/Excel",
  "module": "Lookups",
  "isActive": true
}
```

**Response (201 Created):**
```json
{
  "policyId": 9,
  "policyName": "CanExportLookups",
  "policyCode": "EXPORT_LOOKUPS",
  "description": "Can export lookup data to CSV/Excel",
  "module": "Lookups",
  "isActive": true,
  "createdAt": "2024-02-07T10:15:00Z"
}
```

**Response (409 Conflict):**
```json
{
  "error": "Policy 'CanExportLookups' already exists"
}
```

---

### **4. Update Policy**

```http
PUT /api/policymanagement/policies/{policyId}
Authorization: Bearer <token>
Content-Type: application/json

{
  "description": "Updated description",
  "isActive": false
}
```

**Response (200 OK):**
```json
{
  "policyId": 9,
  "policyName": "CanExportLookups",
  "policyCode": "EXPORT_LOOKUPS",
  "description": "Updated description",
  "module": "Lookups",
  "isActive": false,
  "createdAt": "2024-02-07T10:15:00Z"
}
```

---

### **5. Delete Policy**

```http
DELETE /api/policymanagement/policies/{policyId}
Authorization: Bearer <token>

Example:
DELETE /api/policymanagement/policies/9
```

**Response (200 OK):**
```json
{
  "message": "Policy deleted successfully"
}
```

**Response (404 Not Found):**
```json
{
  "error": "Policy with ID 999 not found"
}
```

---

### **6. Get Policies for a Role**

```http
GET /api/policymanagement/roles/{roleId}/policies
Authorization: Bearer <token>

Example:
GET /api/policymanagement/roles/1/policies
```

**Response (200 OK):**
```json
[
  {
    "policyId": 1,
    "policyName": "CanViewLookups",
    "policyCode": "VIEW_LOOKUPS",
    "description": "Can view lookup types and values",
    "module": "Lookups",
    "isActive": true,
    "createdAt": "2024-02-07T10:00:00Z"
  },
  {
    "policyId": 2,
    "policyName": "CanManageLookups",
    "policyCode": "MANAGE_LOOKUPS",
    "description": "Can create and edit lookup types and values",
    "module": "Lookups",
    "isActive": true,
    "createdAt": "2024-02-07T10:00:00Z"
  }
  // ... more policies for this role
]
```

---

### **7. Assign Policy to Role**

```http
POST /api/policymanagement/roles/{roleId}/policies
Authorization: Bearer <token>
Content-Type: application/json

{
  "policyId": 9
}

Example:
POST /api/policymanagement/roles/2/policies
{
  "policyId": 9
}
```

**Response (200 OK):**
```json
{
  "message": "Policy 9 assigned to role 2"
}
```

**Use Case:** Add "CanExportLookups" policy to Manager role

---

### **8. Remove Policy from Role**

```http
DELETE /api/policymanagement/roles/{roleId}/policies/{policyId}
Authorization: Bearer <token>

Example:
DELETE /api/policymanagement/roles/2/policies/9
```

**Response (200 OK):**
```json
{
  "message": "Policy 9 removed from role 2"
}
```

---

### **9. Refresh Cache**

```http
POST /api/policymanagement/cache/refresh
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "message": "Policy cache refreshed successfully"
}
```

**Use Case:** After updating policies in database, refresh cache to apply changes immediately

---

### **10. Get Statistics**

```http
GET /api/policymanagement/statistics
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "totalPolicies": 9,
  "activePolicies": 8,
  "inactivePolicies": 1,
  "modules": 3,
  "policiesByModule": {
    "Lookups": 3,
    "Users": 3,
    "System": 2,
    "Other": 1
  }
}
```

---

## ?? Complete Workflow Example

### **Scenario: Add New Export Feature**

#### **Step 1: Create New Policy**
```bash
curl -X POST http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "policyName": "CanExportLookups",
    "policyCode": "EXPORT_LOOKUPS",
    "description": "Can export lookup data",
    "module": "Lookups",
    "isActive": true
  }'

# Response: Policy created with PolicyId 9
```

#### **Step 2: Assign to Manager Role**
```bash
curl -X POST http://localhost:5000/api/policymanagement/roles/2/policies \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{"policyId": 9}'

# Response: Policy assigned to role 2 (Manager)
```

#### **Step 3: Refresh Cache**
```bash
curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer <admin_token>"

# Response: Cache refreshed
```

#### **Step 4: Verify in Code**
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class LookupsController : ControllerBase
{
    // New export endpoint protected by new policy
    [HttpGet("export")]
    [Authorize(Policy = "CanExportLookups")]  // Uses new policy!
    public async Task<ActionResult> ExportLookups()
    {
        // No code redeploy needed - just add the [Authorize] attribute
        // Policy already exists in database!
    }
}
```

---

## ?? Common Operations

### **Add New Role Permission**
```bash
# 1. Get all policies for a role
curl -X GET http://localhost:5000/api/policymanagement/roles/3/policies \
  -H "Authorization: Bearer <admin_token>"

# 2. Find the policy ID you want to add (e.g., CanDeleteLookups = 3)

# 3. Assign it
curl -X POST http://localhost:5000/api/policymanagement/roles/3/policies \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{"policyId": 3}'
```

### **Remove Role Permission**
```bash
curl -X DELETE http://localhost:5000/api/policymanagement/roles/3/policies/3 \
  -H "Authorization: Bearer <admin_token>"
```

### **Disable Policy (Temporarily)**
```bash
curl -X PUT http://localhost:5000/api/policymanagement/policies/2 \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{"isActive": false}'

# Then refresh cache:
curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer <admin_token>"
```

### **Update Policy Description**
```bash
curl -X PUT http://localhost:5000/api/policymanagement/policies/2 \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{"description": "New description for managing lookups"}'
```

---

## ?? Default Policies (Reference)

| PolicyId | PolicyName | PolicyCode | Module | Active |
|----------|-----------|-----------|--------|--------|
| 1 | CanViewLookups | VIEW_LOOKUPS | Lookups | ? |
| 2 | CanManageLookups | MANAGE_LOOKUPS | Lookups | ? |
| 3 | CanDeleteLookups | DELETE_LOOKUPS | Lookups | ? |
| 4 | CanViewUsers | VIEW_USERS | Users | ? |
| 5 | CanManageUsers | MANAGE_USERS | Users | ? |
| 6 | CanDeleteUsers | DELETE_USERS | Users | ? |
| 7 | ManageRoles | MANAGE_ROLES | Users | ? |
| 8 | AdminOnly | ADMIN_ONLY | System | ? |

---

## ?? Security Notes

? **Admin Only**: All endpoints require Admin role  
? **Authorization**: All operations checked against user's role  
? **Logging**: All policy changes logged with admin ID  
? **Validation**: Input validation on all requests  
? **Error Handling**: Safe error messages, no stack traces  

---

## ? Real-World Examples

### **Example 1: Quick Permission Grant**
```bash
# User asks: "Can Manager role export lookups?"
# Admin action:
curl -X POST http://localhost:5000/api/policymanagement/roles/2/policies \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{"policyId": 9}'

# Done! Manager can export immediately (after cache refresh)
```

### **Example 2: Audit Compliance**
```bash
# Requirement: Temporarily disable sensitive operations
# Admin action:
curl -X PUT http://localhost:5000/api/policymanagement/policies/6 \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{"isActive": false, "description": "Disabled for audit period"}'

curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer <admin_token>"

# Done! CanDeleteUsers disabled until re-enabled
```

### **Example 3: New Feature Launch**
```bash
# New feature: Bulk import lookups
# 1. Create policy
curl -X POST http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer <admin_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "policyName": "CanImportLookups",
    "policyCode": "IMPORT_LOOKUPS",
    "description": "Can bulk import lookup data",
    "module": "Lookups"
  }'

# 2. Assign to Admin and Manager
curl -X POST http://localhost:5000/api/policymanagement/roles/1/policies \
  -H "Authorization: Bearer <admin_token>" \
  -d '{"policyId": 10}'

curl -X POST http://localhost:5000/api/policymanagement/roles/2/policies \
  -H "Authorization: Bearer <admin_token>" \
  -d '{"policyId": 10}'

# 3. Refresh and deploy
curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer <admin_token>"

# Done! Feature ready immediately, no code redeploy needed
```

---

## ?? Benefits

? **No Redeploy**: Change policies instantly  
? **No Hardcoding**: Everything in database  
? **Runtime Changes**: No restart needed  
? **Audit Trail**: Track all changes  
? **Flexible**: Add new roles/policies anytime  
? **Admin Friendly**: Simple REST API  
? **Scalable**: Works with unlimited policies  

---

## ?? Testing

### **Test with cURL**
```bash
# 1. Get admin token
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | grep -o '"token":"[^"]*' | cut -d'"' -f4)

# 2. Get all policies
curl -X GET http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer $TOKEN"

# 3. Create new policy
curl -X POST http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "policyName": "TestPolicy",
    "policyCode": "TEST_POLICY",
    "description": "Test policy",
    "module": "Test"
  }'
```

### **Test with Postman**
1. Get admin token from login
2. Set header: `Authorization: Bearer <token>`
3. Import collection: GET, POST, PUT, DELETE examples
4. Test each endpoint

---

## ?? Integration with Controllers

```csharp
// In any controller that needs the new policy:
[ApiController]
[Route("api/v1/[controller]")]
public class LookupsController : ControllerBase
{
    [HttpGet("export")]
    [Authorize(Policy = "CanExportLookups")]  // Uses policy from database!
    public async Task<ActionResult> ExportLookups()
    {
        // Automatically checks if user has the policy
        // No code change needed, policy managed via API
    }
}
```

---

**Your API now has complete runtime policy management!** ??
