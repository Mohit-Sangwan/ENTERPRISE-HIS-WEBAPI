# ?? MODULE-BASED POLICIES - VISUAL REFERENCE GUIDE

## Complete At-A-Glance Reference

---

## ?? 42 Policies at a Glance

### **LOOKUPS Module (7 Policies)**
```
1. Lookups.View    (LOOKUPS_VIEW)     ? Use: ModulePolicyConstants.Lookups.VIEW
2. Lookups.Print   (LOOKUPS_PRINT)    ? Use: ModulePolicyConstants.Lookups.PRINT
3. Lookups.Create  (LOOKUPS_CREATE)   ? Use: ModulePolicyConstants.Lookups.CREATE
4. Lookups.Edit    (LOOKUPS_EDIT)     ? Use: ModulePolicyConstants.Lookups.EDIT
5. Lookups.Delete  (LOOKUPS_DELETE)   ? Use: ModulePolicyConstants.Lookups.DELETE
6. Lookups.Manage  (LOOKUPS_MANAGE)   ? Use: ModulePolicyConstants.Lookups.MANAGE
7. Lookups.Admin   (LOOKUPS_ADMIN)    ? Use: ModulePolicyConstants.Lookups.ADMIN
```

### **USERS Module (7 Policies)**
```
1. Users.View      (USERS_VIEW)       ? Use: ModulePolicyConstants.Users.VIEW
2. Users.Print     (USERS_PRINT)      ? Use: ModulePolicyConstants.Users.PRINT
3. Users.Create    (USERS_CREATE)     ? Use: ModulePolicyConstants.Users.CREATE
4. Users.Edit      (USERS_EDIT)       ? Use: ModulePolicyConstants.Users.EDIT
5. Users.Delete    (USERS_DELETE)     ? Use: ModulePolicyConstants.Users.DELETE
6. Users.Manage    (USERS_MANAGE)     ? Use: ModulePolicyConstants.Users.MANAGE
7. Users.Admin     (USERS_ADMIN)      ? Use: ModulePolicyConstants.Users.ADMIN
```

### **ROLES Module (7 Policies)**
```
1. Roles.View      (ROLES_VIEW)       ? Use: ModulePolicyConstants.Roles.VIEW
2. Roles.Print     (ROLES_PRINT)      ? Use: ModulePolicyConstants.Roles.PRINT
3. Roles.Create    (ROLES_CREATE)     ? Use: ModulePolicyConstants.Roles.CREATE
4. Roles.Edit      (ROLES_EDIT)       ? Use: ModulePolicyConstants.Roles.EDIT
5. Roles.Delete    (ROLES_DELETE)     ? Use: ModulePolicyConstants.Roles.DELETE
6. Roles.Manage    (ROLES_MANAGE)     ? Use: ModulePolicyConstants.Roles.MANAGE
7. Roles.Admin     (ROLES_ADMIN)      ? Use: ModulePolicyConstants.Roles.ADMIN
```

### **PATIENTS Module (7 Policies - Future)**
```
1. Patients.View   (PATIENTS_VIEW)    ? Use: ModulePolicyConstants.Patients.VIEW
... (5 more) ... 
7. Patients.Admin  (PATIENTS_ADMIN)   ? Use: ModulePolicyConstants.Patients.ADMIN
```

### **APPOINTMENTS Module (7 Policies - Future)**
```
1. Appointments.View      (APPOINTMENTS_VIEW)
... (5 more) ...
7. Appointments.Admin     (APPOINTMENTS_ADMIN)
```

### **PRESCRIPTIONS Module (7 Policies - Future)**
```
1. Prescriptions.View     (PRESCRIPTIONS_VIEW)
... (5 more) ...
7. Prescriptions.Admin    (PRESCRIPTIONS_ADMIN)
```

---

## ?? Role Access Control Matrix

```
OPERATIONS × ROLES

                Admin    Manager   User    Viewer
                ????     ????      ????    ????
View            ?       ?        ?      ?
Print           ?       ?        ?      ?
Create          ?       ?        ?      ?
Edit            ?       ?        ?      ?
Delete          ?       ?        ?      ?
Manage          ?       ?        ?      ?
Admin           ?       ?        ?      ?
                ????     ????      ????    ????
Policies        42/42    30/42     12/42   6/42
```

---

## ??? Database Schema

```
master.PolicyMaster
????????????????????????????????????????????
? PolicyId  ? PolicyName        ? Code     ?
? 1         ? Lookups - View    ? LKUP_VW  ?
? 2         ? Lookups - Print   ? LKUP_PR  ?
? ...       ? ...               ? ...      ?
? 42        ? Prescriptions...  ? PRSC_ADM ?
????????????????????????????????????????????

config.RolePolicyMapping
????????????????????????????????????????????
? RoleId    ? PolicyId  ? AssignedAt       ?
? 1         ? 1-42      ? 2024-02-07       ?
? 2         ? 1-30      ? 2024-02-07       ?
? 3         ? 1-12      ? 2024-02-07       ?
? 4         ? 1-6       ? 2024-02-07       ?
????????????????????????????????????????????
```

---

## ?? Code Usage Pattern

### **For ANY Controller**

```csharp
// STEP 1: Add using
using ENTERPRISE_HIS_WEBAPI.Constants;

// STEP 2: Use in controller
[ApiController]
public class {Module}Controller : ControllerBase
{
    // VIEW
    [Authorize(Policy = ModulePolicyConstants.{Module}.VIEW)]
    [HttpGet]
    public async Task<ActionResult> GetAll() { }

    // PRINT
    [Authorize(Policy = ModulePolicyConstants.{Module}.PRINT)]
    [HttpGet("export")]
    public async Task<ActionResult> Export() { }

    // CREATE
    [Authorize(Policy = ModulePolicyConstants.{Module}.CREATE)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateDto req) { }

    // EDIT
    [Authorize(Policy = ModulePolicyConstants.{Module}.EDIT)]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDto req) { }

    // DELETE
    [Authorize(Policy = ModulePolicyConstants.{Module}.DELETE)]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id) { }

    // MANAGE
    [Authorize(Policy = ModulePolicyConstants.{Module}.MANAGE)]
    [HttpPatch("{id}/activate")]
    public async Task<ActionResult> Activate(int id) { }

    // ADMIN
    [Authorize(Policy = ModulePolicyConstants.{Module}.ADMIN)]
    [HttpPost("admin/bulk-import")]
    public async Task<ActionResult> BulkImport(IFormFile file) { }
}
```

---

## ?? Testing Reference

### **Test Template**

```bash
# Login
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"ROLE","password":"PASS"}' | jq -r '.token')

# Test operations
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/{controller}
curl -X POST -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/{controller}
curl -X DELETE -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/v1/{controller}/1
```

### **All Roles to Test**

| Role | Username | Password | Permissions |
|------|----------|----------|-------------|
| Admin | admin | admin123 | All 7 ops ? |
| Manager | manager | manager123 | View, Print, Create, Edit, Manage ? |
| User | user | user123 | View, Print ? |
| Viewer | viewer | viewer123 | View ? |

---

## ?? Endpoint Summary by Operation

### **VIEW Operations**
```
GET     /api/v1/{module}              (List)
GET     /api/v1/{module}/{id}         (Get)
GET     /api/v1/{module}/search       (Search)
GET     /api/v1/{module}/count        (Count)
Policy: ModulePolicyConstants.{Module}.VIEW
```

### **PRINT Operations**
```
GET     /api/v1/{module}/export/pdf   (PDF)
GET     /api/v1/{module}/export/excel (Excel)
POST    /api/v1/{module}/print        (Print)
Policy: ModulePolicyConstants.{Module}.PRINT
```

### **CREATE Operations**
```
POST    /api/v1/{module}              (Create single)
POST    /api/v1/{module}/bulk-create  (Bulk)
Policy: ModulePolicyConstants.{Module}.CREATE
```

### **EDIT Operations**
```
PUT     /api/v1/{module}/{id}         (Update)
POST    /api/v1/{module}/bulk-update  (Bulk)
PATCH   /api/v1/{module}/{id}/field   (Partial)
Policy: ModulePolicyConstants.{Module}.EDIT
```

### **DELETE Operations**
```
DELETE  /api/v1/{module}/{id}         (Delete)
POST    /api/v1/{module}/bulk-delete  (Bulk)
Policy: ModulePolicyConstants.{Module}.DELETE
```

### **MANAGE Operations**
```
PATCH   /api/v1/{module}/{id}/activate
PATCH   /api/v1/{module}/{id}/deactivate
PATCH   /api/v1/{module}/{id}/archive
POST    /api/v1/{module}/bulk-status-change
Policy: ModulePolicyConstants.{Module}.MANAGE
```

### **ADMIN Operations**
```
POST    /api/v1/{module}/admin/bulk-import
POST    /api/v1/{module}/admin/reset-defaults
GET     /api/v1/{module}/admin/audit-trail
POST    /api/v1/{module}/admin/recalculate
Policy: ModulePolicyConstants.{Module}.ADMIN
```

---

## ?? Constants Reference

### **IntelliSense Guide**

```csharp
ModulePolicyConstants.
?? Lookups
?  ?? VIEW
?  ?? PRINT
?  ?? CREATE
?  ?? EDIT
?  ?? DELETE
?  ?? MANAGE
?  ?? ADMIN
?? Users
?  ?? VIEW, PRINT, CREATE, EDIT, DELETE, MANAGE, ADMIN
?? Roles
?  ?? VIEW, PRINT, CREATE, EDIT, DELETE, MANAGE, ADMIN
?? Patients
?  ?? VIEW, PRINT, CREATE, EDIT, DELETE, MANAGE, ADMIN
?? Appointments
?  ?? VIEW, PRINT, CREATE, EDIT, DELETE, MANAGE, ADMIN
?? Prescriptions
?  ?? VIEW, PRINT, CREATE, EDIT, DELETE, MANAGE, ADMIN
?? Operations
?  ?? VIEW, PRINT, CREATE, EDIT, DELETE, MANAGE, ADMIN
?? AllModules
?? AllOperations
?? GetPolicyCodeMap()
?? GetPolicyDescriptions()
```

---

## ? Verification Queries

### **Count Everything**
```sql
SELECT COUNT(*) FROM [master].[PolicyMaster];
-- Result: 42

SELECT [Module], COUNT(*) FROM [master].[PolicyMaster] GROUP BY [Module];
-- Result: 6 modules, 7 each

SELECT r.[RoleName], COUNT(rpm.[PolicyId]) FROM [master].[RoleMaster] r
LEFT JOIN [config].[RolePolicyMapping] rpm ON r.[RoleId] = rpm.[RoleId]
GROUP BY r.[RoleName];
-- Result: Admin 42, Manager 30, User 12, Viewer 6
```

---

## ?? Quick Cheat Sheet

| Need | Use |
|------|-----|
| View data | `ModulePolicyConstants.{Module}.VIEW` |
| Print/Export | `ModulePolicyConstants.{Module}.PRINT` |
| Add new | `ModulePolicyConstants.{Module}.CREATE` |
| Modify | `ModulePolicyConstants.{Module}.EDIT` |
| Remove | `ModulePolicyConstants.{Module}.DELETE` |
| Full control | `ModulePolicyConstants.{Module}.MANAGE` |
| Admin ops | `ModulePolicyConstants.{Module}.ADMIN` |

---

## ?? Policy Distribution

```
Per Role:
Admin    42 policies (100% access)
Manager  30 policies (71% access)
User     12 policies (29% access)
Viewer   6 policies (14% access)

Per Operation:
View     6 policies (1 per module)
Print    6 policies (1 per module)
Create   6 policies (1 per module)
Edit     6 policies (1 per module)
Delete   6 policies (1 per module)
Manage   6 policies (1 per module)
Admin    6 policies (1 per module)
         ???????????????????
Total:   42 policies
```

---

## ?? Ready to Use!

Everything is set up and ready to go. Use this as your reference guide while implementing module-based policies in your controllers.

**Key Points:**
1. ? 42 policies configured (6 modules × 7 operations)
2. ? 4 roles with different access levels
3. ? Database migration included
4. ? All operations covered (View, Print, Create, Edit, Delete, Manage, Admin)
5. ? Future modules pre-configured

**Get started:** Run the SQL migration, then update your controllers!

---

**Reference:** `ENTERPRISE_MODULE_BASED_POLICIES.md` for complete details
