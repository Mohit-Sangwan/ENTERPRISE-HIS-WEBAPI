# ?? DATABASE SCHEMA INTEGRATION GUIDE

## ? Policy System Integrated with Existing Schema

---

## ?? Your Current Database Structure

```
master.RoleMaster          ? Existing roles table
  ?? RoleId (PK)
  ?? RoleName

master.PermissionMaster    ? Existing permissions table
  ?? PermissionId (PK)
  ?? PermissionName

config.RolePermissionConfig ? Existing role-permission mapping
  ?? RoleId (FK)
  ?? PermissionId (FK)

config.UserRole            ? Existing user-role assignment
  ?? UserId (FK)
  ?? RoleId (FK)

core.UserAccount           ? Existing user account
  ?? UserId (PK)
  ?? UserName
```

---

## ?? New Policy Tables (Integrating with Existing Schema)

### **1. master.PolicyMaster** (New)
```sql
CREATE TABLE [master].[PolicyMaster]
(
    [PolicyId] INT IDENTITY(1,1) PRIMARY KEY,
    [PolicyName] NVARCHAR(100) NOT NULL UNIQUE,
    [PolicyCode] NVARCHAR(100) NOT NULL UNIQUE,
    [Description] NVARCHAR(500),
    [Module] NVARCHAR(100),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt] DATETIME2,
    [CreatedBy] INT,  -- FK to core.UserAccount
    [ModifiedBy] INT  -- FK to core.UserAccount
);
```

**Purpose:** Store all system policies  
**Schema:** master (aligns with RoleMaster)  
**Audit Trail:** CreatedBy, ModifiedBy, CreatedAt, ModifiedAt  

### **2. config.RolePolicyMapping** (New)
```sql
CREATE TABLE [config].[RolePolicyMapping]
(
    [RoleId] INT NOT NULL,         -- FK to master.RoleMaster
    [PolicyId] INT NOT NULL,       -- FK to master.PolicyMaster
    [AssignedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [AssignedBy] INT,              -- FK to core.UserAccount
    
    CONSTRAINT PK_RolePolicyMapping PRIMARY KEY ([RoleId], [PolicyId]),
    CONSTRAINT FK_RolePolicyMapping_Role FOREIGN KEY ([RoleId]) 
        REFERENCES [master].[RoleMaster]([RoleId]) ON DELETE CASCADE,
    CONSTRAINT FK_RolePolicyMapping_Policy FOREIGN KEY ([PolicyId]) 
        REFERENCES [master].[PolicyMaster]([PolicyId]) ON DELETE CASCADE
);
```

**Purpose:** Map policies to roles  
**Schema:** config (aligns with RolePermissionConfig)  
**Links:** RoleMaster + PolicyMaster  

---

## ?? 8 Default Policies (Created by Script)

```sql
-- Lookup Management (Module: Lookups)
1. CanViewLookups      ? VIEW_LOOKUPS
2. CanManageLookups    ? MANAGE_LOOKUPS
3. CanDeleteLookups    ? DELETE_LOOKUPS

-- User Management (Module: Users)
4. CanViewUsers        ? VIEW_USERS
5. CanManageUsers      ? MANAGE_USERS
6. CanDeleteUsers      ? DELETE_USERS

-- Role Management (Module: Users)
7. ManageRoles         ? MANAGE_ROLES

-- System Admin (Module: System)
8. AdminOnly           ? ADMIN_ONLY
```

---

## ?? Integration Architecture

```
????????????????????????????????????????????????????????
?              POLICY SYSTEM INTEGRATION               ?
????????????????????????????????????????????????????????
?                                                      ?
?  User Login                                          ?
?    ?                                                 ?
?  config.UserRole ? master.RoleMaster                ?
?    ?                                                 ?
?  config.RolePolicyMapping ? master.PolicyMaster     ?
?    ?                                                 ?
?  PolicyService (Cache) ? In-Memory O(1) Lookup      ?
?    ?                                                 ?
?  [Authorize(Policy = "CanViewLookups")]             ?
?    ?                                                 ?
?  ? ALLOW or ? DENY                                ?
?                                                      ?
????????????????????????????????????????????????????????
```

---

## ?? Setup Steps

### **Step 1: Run SQL Script**

Execute `01_PolicySchema.sql`:
```bash
# This script will:
# 1. Create [master].[PolicyMaster] table
# 2. Create [config].[RolePolicyMapping] table
# 3. Insert 8 default policies
# 4. Auto-detect your roles from [master].[RoleMaster]
# 5. Assign default role-policy mappings
```

### **Step 2: Verify Setup**

```sql
-- Check if tables created
SELECT * FROM [master].[PolicyMaster];
SELECT * FROM [config].[RolePolicyMapping];

-- Check policies
SELECT [PolicyId], [PolicyName], [PolicyCode], [Module], [IsActive]
FROM [master].[PolicyMaster]
ORDER BY [PolicyId];

-- Check role-policy assignments
SELECT 
    r.[RoleName],
    p.[PolicyName],
    COUNT(*) AS [Policies]
FROM [config].[RolePolicyMapping] rpm
INNER JOIN [master].[RoleMaster] r ON rpm.[RoleId] = r.[RoleId]
INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]
GROUP BY r.[RoleName], p.[PolicyName];
```

### **Step 3: Update .NET Code**

Update `IPolicyService` to query your tables:

```csharp
// Instead of:
// WHERE NOT EXISTS (SELECT 1 FROM [core].[Policy] ...)

// Use:
// WHERE NOT EXISTS (SELECT 1 FROM [master].[PolicyMaster] ...)

// Instead of:
// FROM [core].[RolePolicy] rp

// Use:
// FROM [config].[RolePolicyMapping] rpm
```

---

## ?? Query Reference

### **Get All Policies**
```sql
SELECT * FROM [master].[PolicyMaster]
WHERE [IsActive] = 1
ORDER BY [Module], [PolicyName];
```

### **Get Policies for a Role**
```sql
SELECT p.[PolicyName], p.[PolicyCode], p.[Description]
FROM [config].[RolePolicyMapping] rpm
INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]
INNER JOIN [master].[RoleMaster] r ON rpm.[RoleId] = r.[RoleId]
WHERE r.[RoleName] = 'Admin'
ORDER BY p.[PolicyName];
```

### **Get Policies for a User**
```sql
SELECT DISTINCT p.[PolicyName], p.[PolicyCode]
FROM [core].[UserAccount] u
INNER JOIN [config].[UserRole] ur ON u.[UserId] = ur.[UserId]
INNER JOIN [config].[RolePolicyMapping] rpm ON ur.[RoleId] = rpm.[RoleId]
INNER JOIN [master].[PolicyMaster] p ON rpm.[PolicyId] = p.[PolicyId]
WHERE u.[UserId] = 1  -- Replace with actual UserId
ORDER BY p.[PolicyName];
```

### **Assign Policy to Role**
```sql
INSERT INTO [config].[RolePolicyMapping] ([RoleId], [PolicyId], [AssignedBy])
SELECT 2, 5, 1  -- Assign PolicyId 5 to RoleId 2, by UserId 1
WHERE NOT EXISTS (
    SELECT 1 FROM [config].[RolePolicyMapping] 
    WHERE [RoleId] = 2 AND [PolicyId] = 5
);
```

### **Remove Policy from Role**
```sql
DELETE FROM [config].[RolePolicyMapping]
WHERE [RoleId] = 2 AND [PolicyId] = 5;
```

---

## ?? Role-Policy Mapping (Default)

```
Admin Role:
?? CanViewLookups (1)
?? CanManageLookups (2)
?? CanDeleteLookups (3)
?? CanViewUsers (4)
?? CanManageUsers (5)
?? CanDeleteUsers (6)
?? ManageRoles (7)
?? AdminOnly (8)

Manager Role:
?? CanViewLookups (1)
?? CanManageLookups (2)
?? CanViewUsers (4)
?? CanManageUsers (5)

User Role:
?? CanViewLookups (1)
?? CanViewUsers (4)

Viewer Role:
?? CanViewLookups (1)
```

---

## ?? Security Considerations

? **Audit Trail**: CreatedBy, ModifiedBy, AssignedBy tracked  
? **Soft Deletes**: IsActive flag (don't hard delete)  
? **Cascading**: Delete policy removes from all roles  
? **Unique Constraints**: PolicyName and PolicyCode are unique  
? **Indexes**: Fast lookups on Active, Module, Code  

---

## ?? Before vs After

| Item | Before | After |
|------|--------|-------|
| **Policy Storage** | Hardcoded | Database (master.PolicyMaster) |
| **Role-Policy Map** | Hardcoded | Database (config.RolePolicyMapping) |
| **Schema Alignment** | N/A | Aligned with master/config pattern |
| **User Tracking** | None | CreatedBy, ModifiedBy tracked |
| **Audit Trail** | None | Complete timestamp tracking |
| **Changes** | Code redeploy | Direct SQL or API |
| **Real-Time** | No | Yes (with cache refresh) |

---

## ?? Verification Checklist

- [ ] Run `01_PolicySchema.sql` successfully
- [ ] master.PolicyMaster created with 8 policies
- [ ] config.RolePolicyMapping created
- [ ] Policies linked to existing roles (Admin, Manager, User, Viewer)
- [ ] Foreign keys reference correct tables
- [ ] Indexes created
- [ ] Audit columns (CreatedBy, ModifiedBy) ready
- [ ] Ready for .NET code integration

---

## ?? Next: Update .NET Code

Your .NET `IPolicyService` needs to reference:

```csharp
// From:
await _dal.ExecuteQueryAsync<PolicyModel>(
    "SELECT * FROM [core].[Policy]", ...);

// To:
await _dal.ExecuteQueryAsync<PolicyModel>(
    "SELECT * FROM [master].[PolicyMaster]", ...);

// And role policies from:
"SELECT * FROM [config].[RolePolicyMapping]"
```

---

**Your database is now ready for enterprise-grade policy management!** ??
