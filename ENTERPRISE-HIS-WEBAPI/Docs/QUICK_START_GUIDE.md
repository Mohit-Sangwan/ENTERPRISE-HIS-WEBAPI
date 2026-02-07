# User CRUD - Quick Start Guide

## ? Quick Setup (5 minutes)

### Step 1: Run SQL Script
```sql
-- Open SQL Server Management Studio (SSMS)
-- Execute the file:
Docs/SqlScripts/05_UserManagementProcedures.sql

-- Verify procedures were created
SELECT * FROM sys.procedures 
WHERE schema_id = SCHEMA_ID('config')
AND [name] LIKE 'SP_%User%'
ORDER BY [name];
```

### Step 2: Build & Run
```bash
# Build
dotnet build

# Run
dotnet run
```

### Step 3: Access Swagger
Open: `https://localhost:5001`

---

## ?? Quick API Test

### 1. Create User
```bash
curl -X POST https://localhost:5001/api/v1/users \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "TestPass123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

**Response (201):**
```json
{
  "userId": 5,
  "username": "testuser",
  "email": "test@example.com",
  "firstName": "Test",
  "lastName": "User",
  "isActive": true,
  "createdDate": "2024-01-15T10:30:00Z",
  "roles": [],
  "permissions": []
}
```

### 2. Get User by ID
```bash
curl https://localhost:5001/api/v1/users/5 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 3. List All Users
```bash
curl "https://localhost:5001/api/v1/users?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 4. Update User
```bash
curl -X PUT https://localhost:5001/api/v1/users/5 \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "UpdatedTest",
    "email": "newemail@example.com"
  }'
```

### 5. Assign Role
```bash
curl -X POST https://localhost:5001/api/v1/users/5/roles \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "roleId": 2 }'
```

### 6. Delete User
```bash
curl -X DELETE https://localhost:5001/api/v1/users/5 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ?? Authorization Requirements

| Endpoint | Method | Required Role |
|----------|--------|---------------|
| Create User | POST `/users` | Admin |
| Get User | GET `/users/{id}` | Any Authenticated User |
| List Users | GET `/users` | Admin, Manager |
| Update User | PUT `/users/{id}` | Admin, Manager |
| Delete User | DELETE `/users/{id}` | Admin |
| Assign Role | POST `/users/{id}/roles` | Admin |
| Remove Role | DELETE `/users/{id}/roles/{roleId}` | Admin |

---

## ?? Password Requirements

- **Minimum Length:** 8 characters
- **Must Include:**
  - ? Uppercase letter (A-Z)
  - ? Lowercase letter (a-z)
  - ? Number (0-9)
  - ? Special character (@$!%*?&)

**Valid Example:** `SecurePass123!`
**Invalid Examples:**
- `password123` (no uppercase, no special)
- `Pass123` (too short)
- `PASS123!` (no lowercase)

---

## ??? Troubleshooting

### Issue: "Stored procedure not found"
```sql
-- Verify procedures exist:
EXEC config.SP_GetUserById @UserId = 1;

-- If missing, re-run the SQL script:
-- Docs/SqlScripts/05_UserManagementProcedures.sql
```

### Issue: "Username already exists" (400)
```
This means the username is taken. Try a different username.
```

### Issue: "Unauthorized" (401)
```
You need a valid JWT token. Check your Bearer token in the Authorization header.
```

### Issue: "Forbidden" (403)
```
Your user role doesn't have permission for this action.
Check your role assignment with an Admin user.
```

---

## ?? Key Files

| File | Purpose |
|------|---------|
| `Data/Dtos/UserDtos.cs` | DTOs with validation |
| `Data/Repositories/IUserRepository.cs` | Database layer |
| `Services/IUserService.cs` | Business logic |
| `Controllers/UsersController.cs` | API endpoints |
| `Program.cs` | DI & policies |
| `Docs/SqlScripts/05_UserManagementProcedures.sql` | SQL procedures |

---

## ?? Request/Response Examples

### Create User Request
```http
POST /api/v1/users HTTP/1.1
Host: localhost:5001
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "username": "john.doe",
  "email": "john@example.com",
  "password": "MySecure123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890"
}
```

### Create User Response (201)
```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: /api/v1/users/5

{
  "userId": 5,
  "username": "john.doe",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "isActive": true,
  "createdDate": "2024-01-15T10:30:00Z",
  "roles": [],
  "permissions": []
}
```

### List Users Response (200)
```http
HTTP/1.1 200 OK
Content-Type: application/json
X-Pagination-Page: 1
X-Pagination-PageSize: 10
X-Pagination-TotalCount: 42
X-Pagination-TotalPages: 5

{
  "data": [
    {
      "userId": 1,
      "username": "admin",
      "email": "admin@example.com",
      "firstName": "Admin",
      "lastName": "User",
      "isActive": true,
      "createdDate": "2024-01-01T00:00:00Z",
      "roles": ["Admin"],
      "permissions": [...]
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Error Response (400)
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "message": "Username 'john.doe' already exists",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## ?? Common Tasks

### Task: Create New Admin User
```bash
# 1. Create the user (requires existing Admin token)
curl -X POST https://localhost:5001/api/v1/users \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newadmin",
    "email": "newadmin@example.com",
    "password": "AdminPass123!",
    "firstName": "New",
    "lastName": "Admin"
  }'

# 2. Assign Admin role (roleId = 1)
curl -X POST https://localhost:5001/api/v1/users/6/roles \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{ "roleId": 1 }'
```

### Task: Change User Password
```bash
curl -X POST https://localhost:5001/api/v1/users/5/change-password \
  -H "Authorization: Bearer {user_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "OldPass123!",
    "newPassword": "NewPass456!",
    "confirmPassword": "NewPass456!"
  }'
```

### Task: Deactivate User (Soft Delete)
```bash
# Update user with IsActive = false
curl -X PUT https://localhost:5001/api/v1/users/5 \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{ "isActive": false }'
```

### Task: Permanently Delete User
```bash
curl -X DELETE https://localhost:5001/api/v1/users/5 \
  -H "Authorization: Bearer {admin_token}"
```

---

## ?? API Documentation

Access the interactive API documentation at:
```
https://localhost:5001
```

Or download OpenAPI/Swagger JSON:
```
https://localhost:5001/swagger/v1/swagger.json
```

---

## ?? Configuration

Edit `appsettings.Development.json` to customize:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters-long-for-HS256",
    "Issuer": "enterprise-his",
    "Audience": "enterprise-his-api"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
}
```

---

## ?? Postman Collection

Save this as `Postman_UserCRUD.json` and import into Postman:

```json
{
  "info": {
    "name": "Enterprise HIS - User CRUD API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Create User",
      "request": {
        "method": "POST",
        "url": "{{base_url}}/api/v1/users",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"username\": \"testuser\",\n  \"email\": \"test@example.com\",\n  \"password\": \"TestPass123!\",\n  \"firstName\": \"Test\",\n  \"lastName\": \"User\"\n}"
        }
      }
    }
  ]
}
```

---

## ? Verification Checklist

After setup, verify everything works:

- [ ] SQL procedures created successfully
- [ ] Application builds without errors
- [ ] Application starts without errors
- [ ] Swagger UI loads at `https://localhost:5001`
- [ ] Can create a user via API
- [ ] Can retrieve user by ID
- [ ] Can list users with pagination
- [ ] Can update user information
- [ ] Can assign roles
- [ ] Can delete users
- [ ] Authorization policies work
- [ ] Error handling returns correct status codes

---

## ?? Support

**Issues?** Check:
1. `Docs/USER_CRUD_IMPLEMENTATION_GUIDE.md` - Detailed guide
2. `Docs/USER_CRUD_IMPLEMENTATION_SUMMARY.md` - Complete summary
3. Swagger documentation at `https://localhost:5001`
4. Application logs in the Output window

---

**Quick Start Complete!** ??

Your User CRUD system is now ready to use. Start with the Quick API Test above to verify everything is working.
