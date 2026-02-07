# ?? QUICK TEST GUIDE - AUTH SYSTEM

## ? 30-SECOND SETUP

### 1. Update appsettings.json
```json
"Jwt": {
  "Secret": "your-32-character-minimum-secret-key-here123456",
  "Issuer": "enterprise-his",
  "Audience": "enterprise-his-api",
  "ExpirationMinutes": 60
}
```

### 2. Run Application
```powershell
dotnet run
```

### 3. Test Authentication
Go to: `https://localhost:5001`

---

## ?? DEMO CREDENTIALS

```
Admin:
  username: admin
  password: admin123

Manager:
  username: manager
  password: manager123

User:
  username: user
  password: user123
```

---

## ?? TEST PROCEDURES

### Test 1: Login via Swagger

```
1. Open: https://localhost:5001
2. Find: POST /api/auth/login
3. Click: "Try it out"
4. Enter:
   {
     "username": "admin",
     "password": "admin123"
   }
5. Click: "Execute"
6. See: Token in response
```

### Test 2: Login via cURL

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  -k
```

**Copy the token from response**

### Test 3: Authorize in Swagger

```
1. Click: Green "Authorize" button (top right)
2. Paste:
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
3. Click: "Authorize"
4. Click: "Close"
5. Try any endpoint!
```

### Test 4: Current User Info

```bash
curl -X GET https://localhost:5001/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -k
```

### Test 5: Auth Health Check

```bash
curl -X GET https://localhost:5001/api/auth/health \
  -k
```

---

## ? EXPECTED RESULTS

### Login Success
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "userId": 1,
    "username": "admin",
    "email": "admin@enterprise-his.com",
    "roles": ["Admin"]
  }
}
```

### Login Failure
```json
{
  "error": "Invalid username or password",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Current User Info
```json
{
  "userId": 1,
  "username": "admin",
  "email": "admin@enterprise-his.com",
  "roles": ["Admin"],
  "authenticatedAt": "2024-01-15T10:30:00Z"
}
```

---

## ?? TROUBLESHOOTING

### Issue: "JWT Secret not configured"
**Solution:** Add Jwt section to appsettings.json with Secret value

### Issue: "Invalid token"
**Solution:** 
- Ensure token starts with "Bearer "
- Token may have expired (default 60 min)
- Secret in config must match

### Issue: "User not found"
**Solution:**
- Use demo credentials (admin/admin123)
- Check username case sensitivity

### Issue: "401 Unauthorized"
**Solution:**
- Not authenticated - get token first
- Click "Authorize" button in Swagger
- Add token to request header

---

## ?? AUTHENTICATION FLOW

```
POST /api/auth/login
?? Validate credentials
?? Check user exists
?? Verify password
?? Generate token
   ?? Returns: JWT + User Info

GET /api/auth/me
?? Extract token from header
?? Validate signature
?? Check expiration
?? Returns: Current user info

Subsequent API calls
?? Include: Authorization: Bearer <token>
?? Validated automatically
?? Request processed with user context
```

---

## ?? NEXT STEPS

1. ? Test login and token generation
2. ? Test /me endpoint
3. ? Next: Add [Authorize] to endpoints
4. ? Next: Add audit logging
5. ? Next: Replace demo users with real DB

---

**Ready to test?** Start with Step 1! ??
