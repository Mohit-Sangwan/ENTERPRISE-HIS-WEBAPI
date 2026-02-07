# ?? QUICK START: Enterprise Policy Naming

## Replace Module-Specific Policies with Generic Enterprise Names

---

## ?? The Change

### **From (Specific)**
```csharp
[Authorize(Policy = "CanViewLookups")]
[Authorize(Policy = "CanManageLookups")]
[Authorize(Policy = "CanDeleteLookups")]
[Authorize(Policy = "CanViewUsers")]
[Authorize(Policy = "AdminOnly")]
```

### **To (Enterprise)**
```csharp
[Authorize(Policy = PolicyConstants.VIEW)]
[Authorize(Policy = PolicyConstants.CREATE)]
[Authorize(Policy = PolicyConstants.DELETE)]
[Authorize(Policy = PolicyConstants.VIEW)]
[Authorize(Policy = PolicyConstants.ADMIN)]
```

---

## ? 5-Minute Setup

### **1. Database Update (1 min)**
```sql
-- Copy & paste this SQL script into SQL Server Management Studio
-- It updates old policies to enterprise names

UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'View', [PolicyCode] = 'POLICY_VIEW'
WHERE [PolicyCode] IN ('VIEW_LOOKUPS', 'VIEW_USERS');

UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'Create', [PolicyCode] = 'POLICY_CREATE'
WHERE [PolicyCode] IN ('MANAGE_LOOKUPS', 'MANAGE_USERS');

UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'Delete', [PolicyCode] = 'POLICY_DELETE'
WHERE [PolicyCode] IN ('DELETE_LOOKUPS', 'DELETE_USERS');

UPDATE [master].[PolicyMaster]
SET [PolicyName] = 'Admin', [PolicyCode] = 'POLICY_ADMIN'
WHERE [PolicyCode] IN ('ADMIN_ONLY', 'MANAGE_ROLES');

INSERT INTO [master].[PolicyMaster] 
([PolicyName], [PolicyCode], [Description], [Module], [IsActive], [CreatedAt])
VALUES ('Update', 'POLICY_UPDATE', 'Can modify resources', 'Core', 1, GETUTCDATE());
```

### **2. PolicyConstants.cs (Already Created!)**
File: `Constants/PolicyConstants.cs`

Contains all enterprise policy names you'll use.

### **3. Update Controllers (2 min)**
Find & Replace in Visual Studio (`Ctrl+H`):

```
Find:              [Authorize(Policy = "CanViewLookups")]
Replace:           [Authorize(Policy = PolicyConstants.VIEW)]

Find:              [Authorize(Policy = "CanManageLookups")]
Replace:           [Authorize(Policy = PolicyConstants.CREATE)]

Find:              [Authorize(Policy = "CanDeleteLookups")]
Replace:           [Authorize(Policy = PolicyConstants.DELETE)]

... (repeat for other policies)
```

### **4. Add Using Statement (1 min)**
Add to top of each controller:
```csharp
using ENTERPRISE_HIS_WEBAPI.Constants;
```

### **5. Build & Test (1 min)**
```bash
# Build
dotnet build

# Test with each role
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"admin","password":"admin123"}' | jq -r '.token')
curl http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN"
```

---

## ?? 6 Core Enterprise Policies

That's it! These 6 policies work for ALL modules:

```csharp
PolicyConstants.VIEW      // View any resource
PolicyConstants.CREATE    // Create resources
PolicyConstants.UPDATE    // Update resources
PolicyConstants.DELETE    // Delete resources
PolicyConstants.MANAGE    // Full management
PolicyConstants.ADMIN     // Admin operations
```

---

## ?? Usage in ANY Controller

```csharp
using ENTERPRISE_HIS_WEBAPI.Constants;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    [Authorize(Policy = PolicyConstants.VIEW)]
    [HttpGet]
    public async Task<ActionResult> GetAll() { }

    [Authorize(Policy = PolicyConstants.CREATE)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateDto request) { }

    [Authorize(Policy = PolicyConstants.UPDATE)]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDto request) { }

    [Authorize(Policy = PolicyConstants.DELETE)]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id) { }
}
```

**Same code pattern works for:**
- LookupTypes
- LookupTypeValues
- Users
- Products
- Orders
- Reports
- ... ANY module!

---

## ?? Verify It Works

### **Admin (Full Access)**
```bash
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"admin","password":"admin123"}' | jq -r '.token')
curl http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN"  # ? 200
curl -X POST http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN" -d '{"name":"x"}'  # ? 201
curl -X DELETE http://localhost:5000/api/v1/lookuptypes/1 -H "Authorization: Bearer $TOKEN"  # ? 200
```

### **Manager (No Delete)**
```bash
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"manager","password":"manager123"}' | jq -r '.token')
curl http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN"  # ? 200
curl -X POST http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN" -d '{"name":"x"}'  # ? 201
curl -X DELETE http://localhost:5000/api/v1/lookuptypes/1 -H "Authorization: Bearer $TOKEN"  # ? 403
```

### **User (View Only)**
```bash
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"user","password":"user123"}' | jq -r '.token')
curl http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN"  # ? 200
curl -X POST http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN" -d '{"name":"x"}'  # ? 403
```

---

## ? Checklist

- [ ] SQL script executed
- [ ] PolicyConstants.cs exists
- [ ] Controllers updated (Find & Replace)
- [ ] Using statement added
- [ ] Build successful
- [ ] Tests pass

---

## ?? Benefits

? One set of 6 policies works for ALL modules  
? No need to create new policies per module  
? Scales infinitely  
? Easy to understand  
? Enterprise-grade  
? All tests still pass  

---

**See full details in: ENTERPRISE_POLICY_NAMING_CONVENTIONS.md**
**See migration steps in: POLICY_MIGRATION_GUIDE.md**

---

**Done!** Your API now uses enterprise-level policy naming! ??
