# ?? REFRESH TOKEN - QUICK REFERENCE

## ? Implementation Complete

All refresh token functionality has been implemented and integrated into your authentication system.

---

## ?? Three Main Endpoints

### 1. Login (Get Tokens)
```bash
POST /api/auth/login
{
  "username": "admin",
  "password": "admin123"
}

Response:
{
  "token": "eyJhbGc...",              # Access token (15 min)
  "refreshToken": "L2Z1bmN...",       # Refresh token (7 days)
  "expiresIn": 900,
  "refreshExpiresIn": 604800
}
```

### 2. Refresh (Get New Access Token)
```bash
POST /api/auth/refresh
{
  "refreshToken": "L2Z1bmN..."
}

Response:
{
  "accessToken": "newEyJhbGc...",     # New access token
  "refreshToken": "newL2Z1bmN...",    # New refresh token (rotated)
  "expiresIn": 900
}
```

### 3. Logout (Revoke Token)
```bash
POST /api/auth/logout
Authorization: Bearer eyJhbGc...
{
  "refreshToken": "L2Z1bmN..."
}

Response:
{
  "message": "Logout successful"
}
```

---

## ?? Token Timeline

```
Login at T=0
?? Access Token expires at T=15min
?? Refresh Token expires at T=7days

T=14min: Token still valid
?? Use for API calls
?? Works normally

T=15min: Token expires
?? Call POST /api/auth/refresh
?? Provide old refresh token
?? Get new access + refresh token

T=15min to T=22days: Continue using
?? New access token (15 more minutes)
?? New refresh token (7 more days)

T=7days: Refresh token expires
?? All tokens invalid
?? Must login again
?? POST /api/auth/login
```

---

## ?? JavaScript Implementation

```javascript
// Setup axios interceptor for auto-refresh
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000'
});

// Add response interceptor for auto-refresh
api.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config;
    
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const refreshToken = localStorage.getItem('refreshToken');
        const response = await axios.post('/api/auth/refresh', {
          refreshToken
        });
        
        // Update tokens
        localStorage.setItem('accessToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        
        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${response.data.accessToken}`;
        return api(originalRequest);
      } catch (error) {
        // Refresh failed, redirect to login
        window.location.href = '/login';
      }
    }
    
    return Promise.reject(error);
  }
);
```

---

## ?? Configuration

In `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters-long",
    "Issuer": "enterprise-his",
    "Audience": "enterprise-his-api",
    "ExpirationMinutes": 15,        // Access token
    "RefreshTokenExpirationDays": 7  // Refresh token
  }
}
```

---

## ?? Token Rotation

When you refresh a token:

```
Old Refresh Token          New Refresh Token
?? IsValid = false    ?    ?? IsValid = true
?? ReplacedByToken = new   ?? Ready to use
?? Revoked
```

This prevents token reuse attacks while maintaining security.

---

## ?? Features

? **Access Tokens** - Short-lived (15 min)  
? **Refresh Tokens** - Long-lived (7 days)  
? **Token Rotation** - Security best practice  
? **Logout Support** - Revoke tokens  
? **Auto Cleanup** - Expired tokens removed  
? **Audit Trail** - Token tracking  
? **Error Handling** - Comprehensive  

---

## ?? Quick Test

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.token')

REFRESH=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.refreshToken')

# 2. Use token
curl -X GET http://localhost:5000/api/v1/lookuptypes \
  -H "Authorization: Bearer $TOKEN"

# 3. Refresh
curl -X POST http://localhost:5000/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$REFRESH\"}"

# 4. Logout
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$REFRESH\"}"
```

---

## ?? Your API Now Supports

? JWT Authentication  
? Token Refresh  
? Logout with Revocation  
? Token Rotation  
? Automatic Expiration  
? Role-Based Access  
? Policy-Based Authorization  

**Production-Ready!** ??
