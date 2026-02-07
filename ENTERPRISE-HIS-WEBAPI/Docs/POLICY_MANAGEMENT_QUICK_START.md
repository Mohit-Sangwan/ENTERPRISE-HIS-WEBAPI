# ?? POLICY MANAGEMENT API - QUICK START (5 MINUTES)

## ? Manage Policies Without Coding!

---

## ?? What You Can Do

? Create new policies (via API)  
? Update existing policies (via API)  
? Delete policies (via API)  
? Assign policies to roles (via API)  
? Revoke policy from roles (via API)  
? Get statistics (via API)  
? **Zero code changes needed!**  
? **Zero redeploy needed!**  

---

## 5-Minute Setup

### **Step 1: Get Admin Token (1 min)**

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

**Response:**
```json
{
  "token": "eyJ...",
  "tokenType": "Bearer",
  "user": { "userId": 1, "username": "admin", "roles": ["Admin"] }
}
```

**Save this token!** Use it for all policy management calls.

---

### **Step 2: Test List Policies (30 sec)**

```bash
TOKEN="eyJ..."  # From step 1

curl -X GET http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
[
  { "policyId": 1, "policyName": "CanViewLookups", "isActive": true },
  { "policyId": 2, "policyName": "CanManageLookups", "isActive": true },
  ...
]
```

? You can see all policies!

---

### **Step 3: Create New Policy (1 min)**

```bash
curl -X POST http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "policyName": "CanExportLookups",
    "policyCode": "EXPORT_LOOKUPS",
    "description": "Can export lookup data to CSV",
    "module": "Lookups"
  }'
```

**Response:**
```json
{
  "policyId": 9,
  "policyName": "CanExportLookups",
  "isActive": true
}
```

? Policy created! (PolicyId = 9)

---

### **Step 4: Assign to Role (1 min)**

```bash
# Add to Manager role (RoleId = 2)
curl -X POST http://localhost:5000/api/policymanagement/roles/2/policies \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"policyId": 9}'
```

**Response:**
```json
{ "message": "Policy 9 assigned to role 2" }
```

? Managers now have "CanExportLookups" policy!

---

### **Step 5: Refresh Cache (30 sec)**

```bash
curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
{ "message": "Policy cache refreshed successfully", "policiesLoaded": 9 }
```

? Changes applied immediately!

---

## ?? Common Tasks

### **View All Policies**
```bash
curl -X GET http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer $TOKEN"
```

### **Get One Policy**
```bash
curl -X GET http://localhost:5000/api/policymanagement/policies/CanExportLookups \
  -H "Authorization: Bearer $TOKEN"
```

### **Update Policy Description**
```bash
curl -X PUT http://localhost:5000/api/policymanagement/policies/9 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"description": "Updated: Can export and download lookup data"}'
```

### **Disable Policy (Emergency)**
```bash
curl -X PUT http://localhost:5000/api/policymanagement/policies/9 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"isActive": false}'
```

### **Remove from Role**
```bash
curl -X DELETE http://localhost:5000/api/policymanagement/roles/2/policies/9 \
  -H "Authorization: Bearer $TOKEN"
```

### **Get Role Policies**
```bash
# Get all policies for Manager role
curl -X GET http://localhost:5000/api/policymanagement/roles/2/policies \
  -H "Authorization: Bearer $TOKEN"
```

### **Get Statistics**
```bash
curl -X GET http://localhost:5000/api/policymanagement/statistics \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
{
  "totalPolicies": 9,
  "activePolicies": 8,
  "inactivePolicies": 1,
  "modules": 3,
  "policiesByModule": {
    "Lookups": 3,
    "Users": 3,
    "System": 2
  }
}
```

---

## ?? Real Scenarios

### **Scenario 1: Quick Permission Grant**

Manager asks: "Can I export lookups?"

```bash
# Step 1: Create policy if needed (or use existing)
# Step 2: Assign to manager role
curl -X POST http://localhost:5000/api/policymanagement/roles/2/policies \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"policyId": 9}'

# Step 3: Refresh
curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer $TOKEN"

# Done! Manager has permission immediately
```

### **Scenario 2: Emergency Lockdown**

Require: Disable all delete operations during audit

```bash
# Disable each delete policy
curl -X PUT http://localhost:5000/api/policymanagement/policies/3 \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"isActive": false}'

curl -X PUT http://localhost:5000/api/policymanagement/policies/6 \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"isActive": false}'

# Refresh
curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer $TOKEN"

# Done! No one can delete anything
```

### **Scenario 3: New Feature Launch**

Create policy for new import feature

```bash
# Create policy
curl -X POST http://localhost:5000/api/policymanagement/policies \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"policyName":"CanImportLookups","policyCode":"IMPORT_LOOKUPS"}'

# Assign to Admin and Manager
curl -X POST http://localhost:5000/api/policymanagement/roles/1/policies \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"policyId": 10}'

curl -X POST http://localhost:5000/api/policymanagement/roles/2/policies \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"policyId": 10}'

# Refresh
curl -X POST http://localhost:5000/api/policymanagement/cache/refresh \
  -H "Authorization: Bearer $TOKEN"

# Done! Feature ready without code redeploy
```

---

## ?? Key Points

? **Admin Only**: Only admin can manage policies  
? **No Redeploy**: Changes take effect immediately  
? **No Restart**: Zero downtime  
? **API-Driven**: Manage from any tool  
? **Scriptable**: Automate policy management  
? **Audit Trail**: All changes logged  
? **Reversible**: Easy to undo changes  

---

## ?? Test with Postman

1. **Get admin token**
   - POST /api/auth/login
   - Save token

2. **Set header**
   - Authorization: Bearer <token>

3. **Test endpoints**
   - GET /api/policymanagement/policies
   - POST /api/policymanagement/policies
   - PUT /api/policymanagement/policies/1
   - DELETE /api/policymanagement/policies/1
   - etc.

---

## ?? Response Codes

| Code | Meaning | Example |
|------|---------|---------|
| 200 | Success | Policy retrieved/updated |
| 201 | Created | Policy created |
| 400 | Bad Request | Invalid input |
| 403 | Forbidden | Not admin |
| 404 | Not Found | Policy doesn't exist |
| 409 | Conflict | Duplicate policy |
| 500 | Error | Server error |

---

## ?? Troubleshooting

**Q: Getting 403 Forbidden?**  
A: You're not logged in as Admin. Get admin token.

**Q: Getting 404 Policy not found?**  
A: Check policy ID or name spelling.

**Q: Changes not taking effect?**  
A: Refresh cache: POST /api/policymanagement/cache/refresh

**Q: How long until changes apply?**  
A: Immediately after cache refresh!

---

## ?? Learn More

- Full API docs: `POLICY_MANAGEMENT_API.md`
- System overview: `COMPLETE_SOLUTION_SUMMARY.md`
- Authentication: `ENTERPRISE_AUTH_AND_POLICIES_COMPLETE.md`

---

## ?? You're Done!

You now have complete runtime policy management!

```
? Create policies ? 30 seconds
? Assign to roles ? 30 seconds
? Apply changes ? Immediate
? No code changes ? Ever!
? No redeploy ? Ever!
```

**Enterprise-grade policy management at your fingertips!** ??
