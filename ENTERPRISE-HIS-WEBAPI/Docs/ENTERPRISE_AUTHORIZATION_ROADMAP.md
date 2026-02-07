# ? Enterprise Authorization Framework - Implementation Roadmap

## Phase 1: Foundation (COMPLETE ?)

- ? OperationResolver.cs (HTTP?Operation mapping)
- ? ResourceResolver.cs (Controller?Module.Resource mapping)
- ? PermissionBuilder.cs (Build permission strings)
- ? EnterpriseAuthorizationMiddleware.cs (Main gatekeeper)
- ? AuthorizationAuditService.cs (Logging)
- ? Program.cs (Register middleware)

**Status: Ready for Phase 2**

---

## Phase 2: Database Integration (NEXT)

### 2.1 Create Schema
```sql
-- Run migration script
-- Creates: master.PermissionMaster, config.RolePermissionMapping, audit.AuthorizationAccessLog
```

### 2.2 Seed Permissions
```
Total: 42 Permissions

Lookups Module (7):
- Lookups.LookupType.View
- Lookups.LookupType.Create
- Lookups.LookupType.Edit
- Lookups.LookupType.Delete
- Lookups.LookupType.Print
- Lookups.LookupType.Export
- Lookups.LookupType.Admin

... (repeat for other modules)

EMR Module (7):
- EMR.Encounter.View
- EMR.Encounter.Create
- EMR.Encounter.Edit
- EMR.Encounter.Delete
- EMR.Encounter.Sign
- EMR.Encounter.Approve
- EMR.Encounter.Admin

Billing Module (7):
- Billing.Invoice.View
- Billing.Invoice.Create
- Billing.Invoice.Edit
- Billing.Invoice.Delete
- Billing.Invoice.Approve
- Billing.Invoice.Export
- Billing.Invoice.Admin

...and so on
```

### 2.3 Create PermissionService
```csharp
public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task<bool> UserHasPermissionAsync(int userId, string permission);
}

public class PermissionService : IPermissionService
{
    // Query database, cache results
}
```

### 2.4 Update TokenService
```csharp
// Add permissions to JWT claims before returning token
```

---

## Phase 3: Controller Cleanup (AFTER Phase 2)

### 3.1 Remove [Authorize] Attributes

**Current:**
```csharp
[Authorize(Policy = "Lookups.View")]
[HttpGet]
public async Task GetAll() { }
```

**Target:**
```csharp
[HttpGet]
public async Task GetAll() { }
```

**Action**: Remove all [Authorize] attributes from all controllers
**Impact**: Middleware takes over - zero change to business logic

### 3.2 Files to Update
- [ ] LookupController.cs
- [ ] UsersController.cs
- [ ] PolicyManagementController.cs
- [ ] (All other controllers)

---

## Phase 4: Testing & Validation

### 4.1 Permission Resolution Tests
- [ ] GET /api/v1/lookuptypes ? Resolves to "Lookups.LookupType.View"
- [ ] POST /api/v1/lookuptypes ? Resolves to "Lookups.LookupType.Create"
- [ ] PUT /api/v1/lookuptypes/{id} ? Resolves to "Lookups.LookupType.Edit"
- [ ] DELETE /api/v1/lookuptypes/{id} ? Resolves to "Lookups.LookupType.Delete"
- [ ] POST /api/v1/invoices/approve ? Resolves to "Billing.Invoice.Approve"

### 4.2 Access Control Tests
- [ ] Admin can do everything
- [ ] Manager can View, Create, Edit (no Delete)
- [ ] User can only View
- [ ] Denied access returns 403 with permission requirement

### 4.3 Audit Logging Tests
- [ ] All access logged
- [ ] All denials logged with reason
- [ ] IP address captured
- [ ] User agent captured

### 4.4 Wildcard Tests
- [ ] "Lookups.*.*" grants all Lookups operations
- [ ] "*.LookupType.*" grants all LookupType operations
- [ ] Specific permissions override wildcards

---

## Phase 5: Performance Optimization

### 5.1 Permission Caching
- [ ] Cache user permissions for 1 hour
- [ ] Invalidate on role change
- [ ] Consider distributed cache (Redis)

### 5.2 Audit Log Optimization
- [ ] Async logging
- [ ] Batch writes to database
- [ ] Archive old logs

### 5.3 Monitoring
- [ ] Track denied access patterns
- [ ] Alert on suspicious activity
- [ ] Generate usage reports

---

## ?? Deliverables Checklist

### Foundation (DONE ?)
- ? OperationResolver.cs
- ? ResourceResolver.cs
- ? PermissionBuilder.cs
- ? EnterpriseAuthorizationMiddleware.cs
- ? AuthorizationAuditService.cs
- ? Documentation

### Phase 2 (TODO)
- [ ] Database schema SQL script
- [ ] Permissions seeding script
- [ ] PermissionService implementation
- [ ] TokenService enhancement

### Phase 3 (TODO)
- [ ] Remove all [Authorize] attributes
- [ ] Test all endpoints

### Phase 4 (TODO)
- [ ] Unit tests (20+)
- [ ] Integration tests (15+)
- [ ] Load tests

### Phase 5 (TODO)
- [ ] Performance tuning
- [ ] Monitoring setup
- [ ] Audit log archival

---

## ?? Success Criteria

? **Zero Hardcoding**
- No policy constants in code
- No operation strings in code
- All from database

? **Enterprise-Grade**
- 42+ permissions pre-configured
- Complete audit trail
- Multi-tenant ready

? **Production-Ready**
- Performance optimized
- Error handling complete
- Monitoring in place

? **Developer-Friendly**
- Controllers have no auth logic
- Middleware is transparent
- Documentation comprehensive

---

## ?? Timeline Estimate

| Phase | Duration | Priority |
|-------|----------|----------|
| Phase 1 (Foundation) | DONE | ? |
| Phase 2 (Database) | 1 day | HIGH |
| Phase 3 (Cleanup) | 2 hours | HIGH |
| Phase 4 (Testing) | 2 days | HIGH |
| Phase 5 (Optimization) | 1 day | MEDIUM |

**Total: ~4 days to production-ready**

---

## ?? Ready to Proceed?

Ask me to:
1. **Create database migration scripts**
2. **Implement PermissionService**
3. **Update TokenService**
4. **Remove [Authorize] attributes**
5. **Create test cases**

Let's build this! ???