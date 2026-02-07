# User CRUD Implementation Guide

## Overview
This document provides a comprehensive guide for the newly implemented **User CRUD (Create, Read, Update, Delete) system** with enterprise-level features including:
- ? Full CRUD operations
- ? Role-based access control (RBAC)
- ? Password management and hashing
- ? Input validation and error handling
- ? Audit logging
- ? Comprehensive API documentation (Swagger)

---

## Architecture

### Layer Structure
```
Presentation Layer (Controllers)
    ? [API Requests]
Business Logic Layer (Services)
    ? [Business Rules, Validation]
Data Access Layer (Repositories)
    ? [Database Operations]
Database Layer (SQL Server)
```

### Key Components

#### 1. **Controllers** (`UsersController.cs`)
- HTTP endpoints for CRUD operations
- Input validation via DTOs
- Error handling and standardized responses
- Authorization policy enforcement
- Comprehensive XML documentation for Swagger

#### 2. **Services** (`IUserService.cs`)
- Business logic implementation
- Password hashing using PBKDF2
- Validation logic
- Business rule enforcement
- Logging

#### 3. **Repositories** (`IUserRepository.cs`)
- Database access layer
- SQL stored procedures
- Role and permission retrieval
- Error handling and retry logic

#### 4. **DTOs** (`UserDtos.cs`)
- `CreateUserDto` - User creation payload
- `UpdateUserDto` - User update payload
- `UserResponseDto` - API response format
- `ChangePasswordDto` - Password change payload
- `PaginatedUserResponseDto` - Paginated results

---

## API Endpoints

### Base URL
```
https://localhost:5001/api/v1/users
```

### 1. Create User
```http
POST /api/v1/users
Authorization: Bearer {token}
Content-Type: application/json

{
  "username": "john.doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "isActive": true
}
```

**Response (201 Created):**
```json
{
  "userId": 5,
  "username": "john.doe",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "isActive": true,
  "createdDate": "2024-01-15T10:30:00Z",
  "roles": [],
  "permissions": []
}
```

**Required Permissions:** `MANAGE_USERS` role

---

### 2. Get User by ID
```http
GET /api/v1/users/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "username": "admin",
  "email": "admin@example.com",
  "firstName": "Admin",
  "lastName": "User",
  "phoneNumber": null,
  "isActive": true,
  "createdDate": "2024-01-01T00:00:00Z",
  "modifiedDate": "2024-01-15T10:30:00Z",
  "roles": ["Admin"],
  "permissions": ["VIEW_LOOKUPS", "CREATE_LOOKUPS", "EDIT_LOOKUPS", "DELETE_LOOKUPS", ...]
}
```

---

### 3. Get User by Username
```http
GET /api/v1/users/username/{username}
Authorization: Bearer {token}
```

---

### 4. List All Users (Paginated)
```http
GET /api/v1/users?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

**Response Headers:**
```
X-Pagination-Page: 1
X-Pagination-PageSize: 10
X-Pagination-TotalCount: 42
X-Pagination-TotalPages: 5
```

**Response (200 OK):**
```json
{
  "data": [
    { "userId": 1, "username": "admin", ... },
    { "userId": 2, "username": "manager", ... }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Required Permissions:** `VIEW_USERS` role

---

### 5. Update User
```http
PUT /api/v1/users/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@newdomain.com",
  "phoneNumber": "+1234567890",
  "isActive": true
}
```

**Response (200 OK):**
```json
{
  "message": "User updated successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Required Permissions:** `EDIT_USERS` role

---

### 6. Delete User
```http
DELETE /api/v1/users/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "message": "User deleted successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Required Permissions:** `DELETE_USERS` role

---

### 7. Change Password
```http
POST /api/v1/users/{id}/change-password
Authorization: Bearer {token}
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!",
  "confirmPassword": "NewPassword456!"
}
```

**Response (200 OK):**
```json
{
  "message": "Password changed successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

### 8. Assign Role to User
```http
POST /api/v1/users/{id}/roles
Authorization: Bearer {token}
Content-Type: application/json

{
  "roleId": 2
}
```

**Response (200 OK):**
```json
{
  "message": "Role assigned successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Required Permissions:** `MANAGE_ROLES` role

---

### 9. Remove Role from User
```http
DELETE /api/v1/users/{id}/roles/{roleId}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "message": "Role removed successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Required Permissions:** `MANAGE_ROLES` role

---

## Error Responses

### 400 Bad Request
```json
{
  "message": "Username 'john.doe' already exists",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 401 Unauthorized
```json
{
  "message": "Authorization token is missing or invalid",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 403 Forbidden
```json
{
  "message": "You do not have permission to perform this action",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 404 Not Found
```json
{
  "message": "User with ID 999 not found",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 500 Internal Server Error
```json
{
  "message": "An error occurred while creating the user",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## Authorization Policies

The following authorization policies control user access:

| Policy | Roles | Description |
|--------|-------|-------------|
| `CanManageUsers` | Admin | Create new users |
| `CanViewUsers` | Admin, Manager | View user list |
| `CanEditUsers` | Admin, Manager | Update user information |
| `CanDeleteUsers` | Admin | Delete users |
| `ManageRoles` | Admin | Assign/remove roles |

**Usage in Controllers:**
```csharp
[Authorize(Policy = "CanManageUsers")]
public async Task<ActionResult<UserResponseDto>> CreateUser(...)
```

---

## Installation Steps

### Step 1: Create User Table (if not exists)
```sql
-- Ensure the UserAccount table exists in the core schema
-- This should already exist from the Role & Permission System setup

SELECT * FROM [core].[UserAccount];
```

### Step 2: Run User Management Procedures Script
```sql
-- Execute the SQL script to create all stored procedures
-- File: Docs/SqlScripts/05_UserManagementProcedures.sql

EXEC config.SP_GetUserById @UserId = 1;
```

### Step 3: Verify Database Setup
```sql
-- Check if all procedures were created
SELECT * FROM sys.procedures 
WHERE schema_id = SCHEMA_ID('config')
AND [name] LIKE 'SP_%User%'
ORDER BY [name];
```

### Step 4: Build Solution
```bash
dotnet build
```

### Step 5: Run Application
```bash
dotnet run
```

### Step 6: Test Endpoints
Access Swagger UI: `https://localhost:5001`

---

## Security Features

### 1. Password Hashing
Uses **PBKDF2** with **SHA256**:
```csharp
// Password format: iterations.salt.hash
// Example: "10000.base64salt.base64hash"

private string HashPassword(string password)
{
    using (var algorithm = new Rfc2898DeriveBytes(
        password, 
        saltSize: 16, 
        iterations: 10000, 
        HashAlgorithmName.SHA256))
    {
        // Returns encrypted password
    }
}
```

### 2. Input Validation
All DTOs include validation attributes:
```csharp
[Required]
[StringLength(50, MinimumLength = 3)]
public string Username { get; set; }

[EmailAddress]
public string Email { get; set; }

[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])")]
public string Password { get; set; }
```

### 3. Role-Based Access Control (RBAC)
```csharp
[Authorize(Policy = "CanManageUsers")]
public async Task<ActionResult> CreateUser(...)
```

### 4. Error Handling
- Prevents SQL injection via stored procedures
- No sensitive data exposed in errors
- Detailed logging for audit trail

---

## Database Schema

### UserAccount Table Structure
```sql
CREATE TABLE [core].[UserAccount]
(
    [UserId] INT PRIMARY KEY IDENTITY(1,1),
    [Username] NVARCHAR(50) UNIQUE NOT NULL,
    [Email] NVARCHAR(100) UNIQUE NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [FirstName] NVARCHAR(50) NOT NULL,
    [LastName] NVARCHAR(50) NOT NULL,
    [PhoneNumber] NVARCHAR(20),
    [IsActive] BIT DEFAULT 1,
    [CreatedDate] DATETIME DEFAULT GETUTCDATE(),
    [ModifiedDate] DATETIME
);
```

### Stored Procedures

| Procedure | Purpose |
|-----------|---------|
| `SP_GetUserById` | Retrieve user by ID |
| `SP_GetUserByUsername` | Retrieve user by username |
| `SP_GetUserByEmail` | Retrieve user by email |
| `SP_CreateUser` | Create new user |
| `SP_UpdateUser` | Update user information |
| `SP_DeleteUser` | Delete user (hard delete) |
| `SP_DeactivateUser` | Deactivate user (soft delete) |
| `SP_ActivateUser` | Activate user |
| `SP_UpdateUserPassword` | Update user password |
| `SP_CheckUsernameExists` | Check if username is unique |
| `SP_CheckEmailExists` | Check if email is unique |

---

## Testing

### Unit Test Example
```csharp
[Test]
public async Task CreateUser_WithValidData_ReturnsCreatedUser()
{
    // Arrange
    var createUserDto = new CreateUserDto
    {
        Username = "testuser",
        Email = "test@example.com",
        Password = "TestPass123!",
        FirstName = "Test",
        LastName = "User"
    };

    // Act
    var result = await _userService.CreateUserAsync(createUserDto);

    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.Data);
    Assert.AreEqual("testuser", result.Data.Username);
}
```

### Integration Test Example
```csharp
[Test]
public async Task CreateUser_WithDuplicateUsername_ReturnsBadRequest()
{
    // Arrange
    var createUserDto = new CreateUserDto
    {
        Username = "admin", // existing username
        Email = "test@example.com",
        Password = "TestPass123!",
        FirstName = "Test",
        LastName = "User"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/users", createUserDto);

    // Assert
    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
}
```

---

## Performance Considerations

### 1. Database Indexing
```sql
CREATE INDEX IX_UserAccount_Username ON [core].[UserAccount]([Username]);
CREATE INDEX IX_UserAccount_Email ON [core].[UserAccount]([Email]);
CREATE INDEX IX_UserAccount_IsActive ON [core].[UserAccount]([IsActive]);
```

### 2. Caching
Users with roles/permissions are cached for 5 minutes (configurable in appsettings.json).

### 3. Pagination
Large user lists are paginated to prevent memory issues.

---

## Troubleshooting

### Issue: "User not created - database error"
**Solution:** Verify stored procedures are created:
```sql
EXEC config.SP_CreateUser 
    @Username = 'test',
    @Email = 'test@example.com',
    @PasswordHash = 'hash',
    @FirstName = 'Test',
    @LastName = 'User';
```

### Issue: "Permission denied"
**Solution:** Check user roles and authorization policies:
```sql
EXEC config.SP_GetUserRoles @UserId = 1;
```

### Issue: "Duplicate username error"
**Solution:** Verify username uniqueness:
```sql
SELECT * FROM [core].[UserAccount] WHERE [Username] = 'john.doe';
```

---

## Future Enhancements

1. **Two-Factor Authentication (2FA)**
2. **Email Verification**
3. **Account Lockout After Failed Attempts**
4. **Audit Trail with Change History**
5. **User Profile Pictures**
6. **OAuth/OIDC Integration**
7. **SSO (Single Sign-On)**

---

## Support

For issues or questions:
1. Check Swagger documentation: `https://localhost:5001`
2. Review logs in the Output window
3. Verify database connectivity and permissions
4. Check authorization policies in `Program.cs`

---

**Status:** ? Production Ready
**Last Updated:** January 2024
**Version:** 1.0
