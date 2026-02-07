# ?? DATABASE-LEVEL POLICY DOCUMENTATION - START HERE

## How Controller API Methods Link with Database Policies

---

## ?? The Link Explained

```
Controller Method with [Authorize(Policy = "CanViewLookups")]
                            ?
        Check: Does this policy exist in database?
                            ?
        master.PolicyMaster.PolicyCode = "VIEW_LOOKUPS"
                            ?
        Get user's role from JWT token
                            ?
        Check: Does user's role have this policy?
                            ?
        config.RolePolicyMapping.RoleId = X, PolicyId = Y
                            ?
        ? YES ? Execute endpoint ? 200 OK
        ? NO ? Deny access ? 403 Forbidden
```

---

## ?? Which Document to Read?

### **Need Quick Answer? (1 page)**
? **QUICK_REFERENCE_CARD.md** ?
- All 8 policies listed
- All endpoints listed  
- Quick test commands
- Common answers

### **Want to Visualize? (Charts & Tables)**
? **POLICY_VISUAL_REFERENCE.md** ??
- Role permission matrices
- Endpoint access charts
- Policy summary tables
- Diagrams

### **Want Complete Details?**
? **COMPLETE_POLICY_IMPLEMENTATION.md** ??
- Full architecture
- All scenarios explained
- Step-by-step guide
- All controller code

### **Want Database Schema?**
? **DATABASE_INTEGRATION_GUIDE.md** ??
- Table structure
- SQL queries
- Verification steps

### **Want to Manage Policies?**
? **POLICY_MANAGEMENT_API.md** ??
- 10 API endpoints
- Create/update/delete policies
- Assign to roles
- Curl examples

---

## ?? 30-Second Summary

**8 Database Policies:**
```
1. CanViewLookups     ? Everyone can view lookups
2. CanManageLookups   ? Admin, Manager create/edit lookups
3. CanDeleteLookups   ? Admin only deletes lookups
4. CanViewUsers       ? Admin, Manager view users
5. CanManageUsers     ? Admin only manage users
6. CanDeleteUsers     ? Admin only delete users
7. ManageRoles        ? Admin only assign/remove roles
8. AdminOnly          ? Admin only critical ops
```

**4 Roles:**
```
Admin   ? Has all 8 policies
Manager ? Has policies 1,2,4,5
User    ? Has policies 1,4
Viewer  ? Has policy 1
```

**How It Works:**
```
[Authorize(Policy = "CanViewLookups")]
                ?
Database lookup + role check
                ?
? Allow (200) or ? Deny (403)
```

---

## ?? Test All Roles

### **Admin (Everything)**
```bash
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"admin","password":"admin123"}' | jq -r '.token')

# View ?
curl http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN"

# Create ?
curl -X POST http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer $TOKEN" -d '{"name":"Test"}'

# Delete ?
curl -X DELETE http://localhost:5000/api/v1/lookuptypes/1 \
  -H "Authorization: Bearer $TOKEN"
```

### **Manager (View + Manage, No Delete)**
```bash
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"manager","password":"manager123"}' | jq -r '.token')

# View ?
curl http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN"

# Create ?
curl -X POST http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer $TOKEN" -d '{"name":"Test"}'

# Delete ? (403 Forbidden)
curl -X DELETE http://localhost:5000/api/v1/lookuptypes/1 \
  -H "Authorization: Bearer $TOKEN"
```

### **User (View Only)**
```bash
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"user","password":"user123"}' | jq -r '.token')

# View ?
curl http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN"

# Create ? (403 Forbidden)
curl -X POST http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer $TOKEN" -d '{"name":"Test"}'
```

### **Viewer (Limited)**
```bash
TOKEN=$(curl -s http://localhost:5000/api/auth/login \
  -d '{"username":"viewer","password":"viewer123"}' | jq -r '.token')

# View Lookups ?
curl http://localhost:5000/api/v1/lookuptypes -H "Authorization: Bearer $TOKEN"

# View Users ? (403 Forbidden)
curl http://localhost:5000/api/v1/users -H "Authorization: Bearer $TOKEN"
```

---

## ?? All Endpoints & Policies

### **LookupTypes Controller**

| Method | Endpoint | Policy | Admin | Manager | User | Viewer |
|--------|----------|--------|-------|---------|------|--------|
| GET | /api/v1/lookuptypes | CanViewLookups | ? | ? | ? | ? |
| POST | /api/v1/lookuptypes | CanManageLookups | ? | ? | ? | ? |
| PUT | /api/v1/lookuptypes/{id} | CanManageLookups | ? | ? | ? | ? |
| DELETE | /api/v1/lookuptypes/{id} | CanDeleteLookups | ? | ? | ? | ? |

### **LookupTypeValues Controller**

| Method | Endpoint | Policy | Admin | Manager | User | Viewer |
|--------|----------|--------|-------|---------|------|--------|
| GET | /api/v1/lookuptypevalues | CanViewLookups | ? | ? | ? | ? |
| POST | /api/v1/lookuptypevalues | CanManageLookups | ? | ? | ? | ? |
| PUT | /api/v1/lookuptypevalues/{id} | CanManageLookups | ? | ? | ? | ? |
| DELETE | /api/v1/lookuptypevalues/{id} | CanDeleteLookups | ? | ? | ? | ? |

### **Users Controller**

| Method | Endpoint | Policy | Admin | Manager | User | Viewer |
|--------|----------|--------|-------|---------|------|--------|
| GET | /api/v1/users | CanViewUsers | ? | ? | ? | ? |
| POST | /api/v1/users | CanManageUsers | ? | ? | ? | ? |
| PUT | /api/v1/users/{id} | CanManageUsers | ? | ? | ? | ? |
| DELETE | /api/v1/users/{id} | CanDeleteUsers | ? | ? | ? | ? |
| POST | /api/v1/users/{id}/roles | ManageRoles | ? | ? | ? | ? |
| DELETE | /api/v1/users/{id}/roles/{roleId} | ManageRoles | ? | ? | ? | ? |

---

## ?? Understanding the Link

### **What Happens Behind the Scenes**

```
1. Request arrives: GET /api/v1/lookuptypes
   With: Authorization: Bearer eyJhbGc...

2. Controller checks: [Authorize(Policy = "CanViewLookups")]
   
3. DatabasePolicyHandler intercepts:
   - Decode JWT ? Extract userId = 2, role = "Manager"
   
4. Query database:
   SELECT DISTINCT PolicyId 
   FROM config.RolePolicyMapping 
   WHERE RoleId = 2

5. Result: [1, 2, 4, 5]
   (Manager has policies: VIEW_LOOKUPS, MANAGE_LOOKUPS, VIEW_USERS, MANAGE_USERS)

6. Check: Does this list include policy ID for "CanViewLookups"?
   (Policy 1 = CanViewLookups)
   
7. YES! Policy 1 is in list [1, 2, 4, 5]
   
8. context.Succeed() ? Allow access
   
9. Controller method executes
   
10. Returns: 200 OK with data
```

---

## ?? Key Files

| File | Purpose | Read Time |
|------|---------|-----------|
| QUICK_REFERENCE_CARD.md | 1-page cheat sheet | 2 min |
| POLICY_VISUAL_REFERENCE.md | Charts & matrices | 5 min |
| CONTROLLER_POLICY_IMPLEMENTATION.md | Implementation details | 10 min |
| COMPLETE_POLICY_IMPLEMENTATION.md | Everything | 15 min |
| DATABASE_INTEGRATION_GUIDE.md | Database schema | 5 min |
| DATABASE_POLICY_FINAL_SUMMARY.md | Executive summary | 10 min |
| POLICY_MANAGEMENT_API.md | Manage policies | 8 min |
| POLICY_MANAGEMENT_QUICK_START.md | Quick setup | 3 min |

---

## ? Checklist

- [x] Database policies created (01_PolicySchema.sql)
- [x] 8 policies seeded into master.PolicyMaster
- [x] Role mappings created in config.RolePolicyMapping
- [x] All controllers protected with [Authorize(Policy = "...")]
- [x] All endpoints tested
- [x] Documentation complete
- [x] Ready for production

---

## ?? Ready to Go!

**Next Steps:**

1. **Read QUICK_REFERENCE_CARD.md** (understand in 2 min)
2. **Run 01_PolicySchema.sql** (setup database)
3. **Test each role** (verify access)
4. **Deploy to production** (with confidence!)

---

## ?? Quick Help

**Q: Why does my endpoint return 403?**
A: Your role doesn't have the required policy. Check config.RolePolicyMapping.

**Q: Can I change policies without redeploy?**
A: Yes! Use PolicyManagementController API (10 endpoints).

**Q: Which roles can create lookups?**
A: Admin and Manager (CanManageLookups policy).

**Q: Which roles can delete lookups?**
A: Admin only (CanDeleteLookups policy).

**Q: How do I add a new policy?**
A: Use POST /api/policymanagement/policies (admin only).

---

**Start with QUICK_REFERENCE_CARD.md** ? Your API is fully protected! ??
