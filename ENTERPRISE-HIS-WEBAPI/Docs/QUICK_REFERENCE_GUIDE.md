# ?? ENTERPRISE API - QUICK REFERENCE GUIDE

## ? Everything You Need to Know (2-Minute Read)

---

## ?? Three Systems in One

```
???????????????????
?  AUTH TOKENS    ? WHO (Identity)
?  60 min valid   ?
???????????????????

???????????????????
? ACCESS TOKENS   ? WHAT (Permissions)
?  15 min valid   ?
???????????????????

???????????????????
? POLICIES (DB)   ? ALLOW/DENY
? No hardcoding   ?
???????????????????
```

---

## ?? Four Key Concepts

### **1. DUAL TOKENS**
- **Auth Token**: Proves who you are (60 min)
- **Access Token**: Proves what you can do (15 min)
- **Why?** Shorter-lived access tokens = more secure

### **2. DATABASE POLICIES**
- **Before**: Hardcoded in Program.cs
- **Now**: Stored in database
- **Benefit**: Change without redeploy!

### **3. ROLE-BASED ACCESS**
- **4 Default Roles**: Admin, Manager, User, Viewer
- **8 Policies**: View, Manage, Delete (lookups + users)
- **Flexible**: Add new roles/policies anytime

### **4. IN-MEMORY CACHE**
- **Fast**: O(1) policy lookups
- **Efficient**: No database queries per request
- **Smart**: Auto-refresh every hour

---

## ?? Login Flow (Simple Version)

```
1. curl -X POST /api/auth/login
   ?? Send: username + password

2. API returns
   ?? authToken (60 min)
   ?? accessToken (15 min)
   ?? refreshToken (7 days)

3. Use accessToken
   ?? Authorization: Bearer <token>

4. When accessToken expires
   ?? POST /api/auth/refresh-access with authToken

5. Get new accessToken
   ?? Use new token for next 15 minutes

6. Repeat until authToken expires (60 min)
   ?? Then login again
```

---

## ?? Quick Test

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | grep -o '"token":"[^"]*' | cut -d'"' -f4)

# 2. Use token
curl -X GET http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer $TOKEN"

# Result: ? 200 OK (you see data)
```

---

## ?? 8 Policies (What Can Be Done)

| # | Policy | Roles | What |
|---|--------|-------|------|
| 1 | CanViewLookups | All | View lookup data |
| 2 | CanManageLookups | Admin, Manager | Create/edit |
| 3 | CanDeleteLookups | Admin | Delete |
| 4 | CanViewUsers | Admin, Manager | View users |
| 5 | CanManageUsers | Admin | Create/edit users |
| 6 | CanDeleteUsers | Admin | Delete users |
| 7 | ManageRoles | Admin | Manage roles |
| 8 | AdminOnly | Admin | Admin access |

---

## ?? 4 Roles (Who Can Do It)

| Role | Permissions | Example |
|------|-------------|---------|
| **Admin** | All policies | Can view, create, edit, delete anything |
| **Manager** | View + Manage (no delete) | Can create/edit, but can't delete |
| **User** | View only | Can see data, can't modify |
| **Viewer** | Limited view | Read-only access |

---

## ?? Token Timeline

```
T=0min   ? Login ? Get both tokens
T=0-15   ? Use accessToken
T=15min  ? accessToken expires ? Refresh
T=15-30  ? Use new accessToken
T=30min  ? accessToken expires ? Refresh
T=30-45  ? Use new accessToken
T=45min  ? accessToken expires ? Refresh
T=45-60  ? Use new accessToken
T=60min  ? authToken expires ? Must login again
T=7days  ? refreshToken expires ? Session ends
```

---

## ?? Key Endpoints

```
LOGIN (Public)
POST /api/auth/login
?? Get: authToken, accessToken, refreshToken

USE (Protected)
GET /api/v1/lookuptypes
Authorization: Bearer <accessToken>
?? Policy checked, result returned

REFRESH (Semi-Public)
POST /api/auth/refresh-access
Body: {"authToken": "..."}
?? Get: new accessToken

LOGOUT (Protected)
POST /api/auth/logout
Authorization: Bearer <accessToken>
?? All tokens revoked
```

---

## ?? How Authorization Works

```
Request arrives
    ?
[Authorize(Policy = "CanViewLookups")]
    ?
Get user's role from token
    ?
Check: Does role have policy?
    ?
YES ? ? 200 OK
NO  ? ? 403 Forbidden
```

---

## ?? Configuration (One-Time Setup)

### appsettings.json
```json
{
  "Jwt": {
    "Secret": "use-strong-32-character-secret",
    "AuthTokenExpirationMinutes": 60,
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## ?? Deploy Steps

1. **Set strong JWT secret** (32+ chars)
2. **Enable HTTPS** (production only)
3. **Run SQL script** (01_PolicySchema.sql)
4. **Test login** (verify tokens)
5. **Test authorization** (verify policies)
6. **Deploy** (confident it works!)

---

## ?? What Changed

| Item | Before | After |
|------|--------|-------|
| Tokens | Single | Dual |
| Policies | Hardcoded | Database |
| Refresh | Manual | Automatic |
| Change Policies | Redeploy | No redeploy |
| Performance | Slower | Faster |
| Security | Basic | Enterprise |

---

## ? Key Benefits

? **Secure**: Dual tokens, HMAC-SHA256, PBKDF2  
? **Fast**: In-memory cache, O(1) lookups  
? **Flexible**: Database policies, runtime changes  
? **Scalable**: Add policies/roles anytime  
? **Professional**: Enterprise-grade features  
? **Ready**: Production-ready code  

---

## ?? Quick Help

**Q: Token expired?**  
A: Use authToken to refresh ? POST /api/auth/refresh-access

**Q: Access denied (403)?**  
A: Your role doesn't have the policy. Contact admin.

**Q: Want to change a policy?**  
A: Update database, no redeploy needed!

**Q: How long until logout?**  
A: 7 days (refreshToken expiration). Then must login again.

**Q: Can I add a new policy?**  
A: Yes! Add to database, refresh cache, assign to roles.

---

## ?? You're Ready!

? **Authentication**: Working  
? **Authorization**: Working  
? **Policies**: Database-backed  
? **Performance**: Optimized  
? **Security**: Enterprise-grade  
? **Build**: SUCCESS  

---

## ?? Full Documentation

- `ENTERPRISE_AUTH_AND_POLICIES_COMPLETE.md` - Full guide
- `DATABASE_POLICIES_QUICK_START.md` - Policies reference
- `DUAL_TOKEN_AUTHENTICATION.md` - Token system
- `FINAL_ENTERPRISE_COMPLETE_SUMMARY.md` - Complete overview

---

**Your Enterprise API is production-ready!** ??
