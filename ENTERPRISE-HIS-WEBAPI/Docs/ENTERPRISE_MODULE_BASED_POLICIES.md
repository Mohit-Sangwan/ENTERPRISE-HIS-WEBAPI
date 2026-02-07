# ?? ENTERPRISE-LEVEL MODULE/MENU-BASED POLICIES

## Complete Implementation Guide with All Operations

---

## ?? System Overview

### **6 Modules (Menus)**
```
1. Lookups Management
2. Users Management  
3. Roles Management
4. Patients (Future)
5. Appointments (Future)
6. Prescriptions (Future)
```

### **7 Operations per Module**
```
1. View   - View/Read data
2. Print  - Export/Print reports
3. Create - Add new records
4. Edit   - Modify existing records
5. Delete - Remove records
6. Manage - Full module management
7. Admin  - Administrative operations
```

### **4 Roles**
```
Admin   - All 42 policies
Manager - 30 policies (View, Print, Create, Edit, Manage)
User    - 12 policies (View, Print only)
Viewer  - 6 policies (View only)
```

---

## ?? Policy Naming Pattern

### **Format**
```
{ModuleName}.{Operation}
```

### **Examples**
```
Lookups.View      ? Can view lookup data
Lookups.Print     ? Can print lookup data
Lookups.Create    ? Can create lookups
Lookups.Edit      ? Can edit lookups
Lookups.Delete    ? Can delete lookups
Lookups.Manage    ? Full lookup management
Lookups.Admin     ? Admin lookups operations

Users.View        ? Can view users
Users.Print       ? Can print user list
Users.Create      ? Can create users
Users.Edit        ? Can edit users
Users.Delete      ? Can delete users
Users.Manage      ? Full user management
Users.Admin       ? Admin user operations
```

---

## ?? Controller Implementation

### **LookupTypesController Example**

```csharp
using ENTERPRISE_HIS_WEBAPI.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ENTERPRISE_HIS_WEBAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class LookupTypesController : ControllerBase
    {
        private readonly ILookupTypeService _service;
        private readonly ILogger<LookupTypesController> _logger;

        // ========== VIEW OPERATIONS ==========

        /// <summary>
        /// Get all lookups - VIEW permission required
        /// </summary>
        [HttpGet]
        [Authorize(Policy = ModulePolicyConstants.Lookups.VIEW)]
        public async Task<ActionResult> GetAll() { }

        /// <summary>
        /// Get lookup by ID - VIEW permission required
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.VIEW)]
        public async Task<ActionResult> GetById(int id) { }

        /// <summary>
        /// Search lookups - VIEW permission required
        /// </summary>
        [HttpGet("search")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.VIEW)]
        public async Task<ActionResult> Search(string? searchTerm) { }

        // ========== PRINT/EXPORT OPERATIONS ==========

        /// <summary>
        /// Export lookups to PDF - PRINT permission required
        /// </summary>
        [HttpGet("export/pdf")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.PRINT)]
        public async Task<ActionResult> ExportPdf() { }

        /// <summary>
        /// Export lookups to Excel - PRINT permission required
        /// </summary>
        [HttpGet("export/excel")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.PRINT)]
        public async Task<ActionResult> ExportExcel() { }

        /// <summary>
        /// Print lookups - PRINT permission required
        /// </summary>
        [HttpPost("print")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.PRINT)]
        public async Task<ActionResult> Print([FromBody] PrintRequest request) { }

        // ========== CREATE OPERATIONS ==========

        /// <summary>
        /// Create new lookup - CREATE permission required
        /// </summary>
        [HttpPost]
        [Authorize(Policy = ModulePolicyConstants.Lookups.CREATE)]
        public async Task<ActionResult> Create([FromBody] CreateLookupTypeDto request) { }

        // ========== EDIT/UPDATE OPERATIONS ==========

        /// <summary>
        /// Update lookup - EDIT permission required
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.EDIT)]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateLookupTypeDto request) { }

        /// <summary>
        /// Bulk update lookups - EDIT permission required
        /// </summary>
        [HttpPost("bulk-update")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.EDIT)]
        public async Task<ActionResult> BulkUpdate([FromBody] BulkUpdateRequest request) { }

        // ========== DELETE OPERATIONS ==========

        /// <summary>
        /// Delete lookup - DELETE permission required
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.DELETE)]
        public async Task<ActionResult> Delete(int id) { }

        /// <summary>
        /// Bulk delete lookups - DELETE permission required
        /// </summary>
        [HttpPost("bulk-delete")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.DELETE)]
        public async Task<ActionResult> BulkDelete([FromBody] BulkDeleteRequest request) { }

        // ========== MANAGE OPERATIONS ==========

        /// <summary>
        /// Activate lookup - MANAGE permission required
        /// </summary>
        [HttpPatch("{id}/activate")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.MANAGE)]
        public async Task<ActionResult> Activate(int id) { }

        /// <summary>
        /// Deactivate lookup - MANAGE permission required
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.MANAGE)]
        public async Task<ActionResult> Deactivate(int id) { }

        /// <summary>
        /// Archive lookup - MANAGE permission required
        /// </summary>
        [HttpPatch("{id}/archive")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.MANAGE)]
        public async Task<ActionResult> Archive(int id) { }

        // ========== ADMIN OPERATIONS ==========

        /// <summary>
        /// Bulk import lookups - ADMIN permission required
        /// </summary>
        [HttpPost("admin/bulk-import")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.ADMIN)]
        public async Task<ActionResult> BulkImport(IFormFile file) { }

        /// <summary>
        /// Reset lookup defaults - ADMIN permission required
        /// </summary>
        [HttpPost("admin/reset-defaults")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.ADMIN)]
        public async Task<ActionResult> ResetDefaults() { }

        /// <summary>
        /// View lookup audit trail - ADMIN permission required
        /// </summary>
        [HttpGet("admin/audit-trail")]
        [Authorize(Policy = ModulePolicyConstants.Lookups.ADMIN)]
        public async Task<ActionResult> GetAuditTrail() { }
    }
}
```

---

## ?? Database Policy Codes

```sql
-- LOOKUPS MODULE
LOOKUPS_VIEW      ? Lookups.View
LOOKUPS_PRINT     ? Lookups.Print
LOOKUPS_CREATE    ? Lookups.Create
LOOKUPS_EDIT      ? Lookups.Edit
LOOKUPS_DELETE    ? Lookups.Delete
LOOKUPS_MANAGE    ? Lookups.Manage
LOOKUPS_ADMIN     ? Lookups.Admin

-- USERS MODULE
USERS_VIEW        ? Users.View
USERS_PRINT       ? Users.Print
USERS_CREATE      ? Users.Create
USERS_EDIT        ? Users.Edit
USERS_DELETE      ? Users.Delete
USERS_MANAGE      ? Users.Manage
USERS_ADMIN       ? Users.Admin

-- ROLES MODULE
ROLES_VIEW        ? Roles.View
ROLES_PRINT       ? Roles.Print
ROLES_CREATE      ? Roles.Create
ROLES_EDIT        ? Roles.Edit
ROLES_DELETE      ? Roles.Delete
ROLES_MANAGE      ? Roles.Manage
ROLES_ADMIN       ? Roles.Admin

-- PATIENTS MODULE (Future)
PATIENTS_VIEW     ? Patients.View
PATIENTS_PRINT    ? Patients.Print
PATIENTS_CREATE   ? Patients.Create
PATIENTS_EDIT     ? Patients.Edit
PATIENTS_DELETE   ? Patients.Delete
PATIENTS_MANAGE   ? Patients.Manage
PATIENTS_ADMIN    ? Patients.Admin

-- APPOINTMENTS MODULE (Future)
APPOINTMENTS_VIEW ? Appointments.View
... (7 policies per module)

-- PRESCRIPTIONS MODULE (Future)
PRESCRIPTIONS_VIEW ? Prescriptions.View
... (7 policies per module)
```

---

## ?? Access Control Matrix

```
???????????????????????????????????????????????????????????????
? Operation           ? Lookups  ? Users   ? Roles   ? Access ?
???????????????????????????????????????????????????????????????
? View                ? Admin ? ? Mgr ?  ? Mgr ?  ? VIEW   ?
? View                ? Mgr ?   ? User ? ? User ? ?        ?
? View                ? User ?  ? View ? ? View ? ?        ?
? Print               ? Admin ? ? Mgr ?  ? Mgr ?  ? PRINT  ?
? Print               ? Mgr ?   ? User ? ? User ? ?        ?
? Print               ? User ?  ? View ? ? View ? ?        ?
? Create              ? Admin ? ? Mgr ?  ? Mgr ?  ? CREATE ?
? Create              ? Mgr ?   ? User ? ? User ? ?        ?
? Edit                ? Admin ? ? Mgr ?  ? Mgr ?  ? EDIT   ?
? Edit                ? Mgr ?   ? User ? ? User ? ?        ?
? Delete              ? Admin ? ? Mgr ?  ? Mgr ?  ? DELETE ?
? Delete              ? Mgr ?   ? User ? ? User ? ?        ?
? Manage              ? Admin ? ? Mgr ?  ? Mgr ?  ? MANAGE ?
? Manage              ? Mgr ?   ? User ? ? User ? ?        ?
? Admin Operations    ? Admin ? ? Mgr ?  ? Mgr ?  ? ADMIN  ?
? Admin Operations    ? Mgr ?   ? User ? ? User ? ?        ?
???????????????????????????????????????????????????????????????
```

---

## ?? 5-Minute Setup

### **Step 1: Run SQL Migration**
```bash
# Execute in SQL Server Management Studio
03_ModuleBasedPoliciesMigration.sql
```

### **Step 2: Use in Controllers**
```csharp
using ENTERPRISE_HIS_WEBAPI.Constants;

[Authorize(Policy = ModulePolicyConstants.Lookups.VIEW)]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = ModulePolicyConstants.Lookups.PRINT)]
[HttpGet("export")]
public async Task<ActionResult> Export() { }

[Authorize(Policy = ModulePolicyConstants.Lookups.CREATE)]
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateDto request) { }
```

### **Step 3: Test Each Role**
```bash
# Admin - Full access ?
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"admin","password":"admin123"}' | jq -r '.token')
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes

# Manager - View, Create, Edit ? (NO Delete)
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"manager","password":"manager123"}' | jq -r '.token')
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes
curl -X DELETE -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes/1  # ? 403

# User - View Only ?
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"user","password":"user123"}' | jq -r '.token')
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes
curl -X POST -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/lookuptypes  # ? 403
```

---

## ?? All Endpoints by Operation

### **VIEW Endpoints**
```csharp
[Authorize(Policy = ModulePolicyConstants.{Module}.VIEW)]

GET     /api/v1/{controller}                    // List all
GET     /api/v1/{controller}/{id}               // Get by ID
GET     /api/v1/{controller}/by-code/{code}    // Get by code
GET     /api/v1/{controller}/search             // Search
GET     /api/v1/{controller}/count              // Get count
GET     /api/v1/{controller}/by-type/{typeId}   // Get by type
```

### **PRINT Endpoints**
```csharp
[Authorize(Policy = ModulePolicyConstants.{Module}.PRINT)]

GET     /api/v1/{controller}/export/pdf         // Export PDF
GET     /api/v1/{controller}/export/excel       // Export Excel
POST    /api/v1/{controller}/print              // Print
```

### **CREATE Endpoints**
```csharp
[Authorize(Policy = ModulePolicyConstants.{Module}.CREATE)]

POST    /api/v1/{controller}                    // Create single
POST    /api/v1/{controller}/bulk-create        // Bulk create
```

### **EDIT Endpoints**
```csharp
[Authorize(Policy = ModulePolicyConstants.{Module}.EDIT)]

PUT     /api/v1/{controller}/{id}               // Update single
POST    /api/v1/{controller}/bulk-update        // Bulk update
PATCH   /api/v1/{controller}/{id}/update-field  // Partial update
```

### **DELETE Endpoints**
```csharp
[Authorize(Policy = ModulePolicyConstants.{Module}.DELETE)]

DELETE  /api/v1/{controller}/{id}               // Delete single
POST    /api/v1/{controller}/bulk-delete        // Bulk delete
```

### **MANAGE Endpoints**
```csharp
[Authorize(Policy = ModulePolicyConstants.{Module}.MANAGE)]

PATCH   /api/v1/{controller}/{id}/activate      // Activate
PATCH   /api/v1/{controller}/{id}/deactivate    // Deactivate
PATCH   /api/v1/{controller}/{id}/archive       // Archive
POST    /api/v1/{controller}/bulk-status-change // Bulk status
```

### **ADMIN Endpoints**
```csharp
[Authorize(Policy = ModulePolicyConstants.{Module}.ADMIN)]

POST    /api/v1/{controller}/admin/bulk-import         // Bulk import
POST    /api/v1/{controller}/admin/reset-defaults      // Reset
GET     /api/v1/{controller}/admin/audit-trail         // Audit log
POST    /api/v1/{controller}/admin/recalculate         // Recalculate
POST    /api/v1/{controller}/admin/sync-external       // Sync
```

---

## ? Checklist

- [ ] Run SQL migration script
- [ ] Verify 42 policies created in database
- [ ] Verify 4 roles configured
- [ ] Add ModulePolicyConstants.cs to project
- [ ] Update all controllers with module-based policies
- [ ] Test each role (Admin, Manager, User, Viewer)
- [ ] Test each operation (View, Print, Create, Edit, Delete, Manage, Admin)
- [ ] Verify role access control
- [ ] Deploy to production

---

## ?? Benefits

? **Scalable**: 6 modules × 7 operations = 42 policies (or more)  
? **Maintainable**: Single pattern for all modules  
? **Extensible**: Add new modules with same operations  
? **Enterprise-Grade**: Module/menu-based organization  
? **Complete**: All operations included (VIEW, PRINT, CREATE, EDIT, DELETE, MANAGE, ADMIN)  
? **Audit Trail**: Track all changes with CreatedBy/ModifiedBy  

---

**Ready to deploy module-based enterprise policies!** ??
