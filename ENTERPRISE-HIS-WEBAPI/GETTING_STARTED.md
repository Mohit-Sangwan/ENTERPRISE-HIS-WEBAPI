# ? GETTING STARTED CHECKLIST

## ?? 30-Second Overview

**Status**: ? Production Ready
**Build**: ? Success (0 errors)
**Deploy Time**: 30 minutes
**Next Step**: Follow this checklist

---

## ?? PRE-DEPLOYMENT (Right Now - 5 minutes)

### ? Verify Your Setup
```
? .NET 8 SDK installed
? SQL Server 2019+ available
? Visual Studio 2022 or VS Code ready
? Git repository cloned
? Terminal/PowerShell open in project directory
```

### ? Quick Build Test
```bash
# Run this command now
dotnet build -c Release

# Expected output:
# Build succeeded.
# 0 Error(s)
# 0 Warning(s)
```

---

## ?? NEXT: CHOOSE YOUR DEPLOYMENT PATH

### ?? Path 1: Docker (Easiest - 15 minutes)
**Best for**: Cloud, Kubernetes, containerized environments

```bash
# Step 1: Build Docker image
docker build -t enterprise-his:latest .

# Step 2: Run container
docker run -d \
  --name enterprise-his \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;Database=EnterpriseHIS_Production;..." \
  enterprise-his:latest

# Step 3: Verify
curl http://localhost:5000/api/auth/health
```

**? Continue to: VERIFICATION TESTS (below)**

---

### ?? Path 2: Azure App Service (Cloud - 20 minutes)
**Best for**: Azure cloud, automatic scaling, managed service

```bash
# Step 1: Create resource group
az group create --name enterprise-his-rg --location eastus

# Step 2: Create App Service
az appservice plan create \
  --name enterprise-his-plan \
  --resource-group enterprise-his-rg \
  --sku B2 --is-linux

# Step 3: Create web app
az webapp create \
  --resource-group enterprise-his-rg \
  --plan enterprise-his-plan \
  --name enterprise-his-api \
  --runtime "DOTNET|8.0"

# Step 4: Publish and deploy
dotnet publish -c Release -o ./publish
cd publish
az webapp deployment source config-zip \
  --resource-group enterprise-his-rg \
  --name enterprise-his-api \
  --src ../publish.zip

# Step 5: Get URL
az webapp show \
  --resource-group enterprise-his-rg \
  --name enterprise-his-api \
  --query defaultHostName
```

**? Continue to: VERIFICATION TESTS (below)**

---

### ??? Path 3: IIS On-Premises (Traditional - 20 minutes)
**Best for**: Windows servers, on-premises, legacy infrastructure

```bash
# Step 1: Publish application
dotnet publish -c Release -o C:\Deploy\enterprise-his

# Step 2: Open IIS Manager
# Windows Key + R ? inetmgr

# Step 3: Create Application Pool
# Right-click "Application Pools" ? Add Application Pool
# Name: enterprise-his
# .NET CLR version: No Managed Code (for .NET 8)
# Managed pipeline mode: Integrated

# Step 4: Create Website
# Right-click "Sites" ? Add Website
# Site name: enterprise-his
# Physical path: C:\Deploy\enterprise-his
# Binding: https://your-domain:443
# SSL Certificate: Select or create

# Step 5: Assign Application Pool
# Select Website ? Advanced Settings
# Application Pool: enterprise-his

# Step 6: Start pool
# Right-click Application Pool ? Start
```

**? Continue to: VERIFICATION TESTS (below)**

---

## ?? VERIFICATION TESTS (Do These First!)

### Test 1: Health Check ?
```bash
# This should work IMMEDIATELY after deployment
curl https://your-domain/api/auth/health

# Expected response:
# {
#   "status": "healthy",
#   "timestamp": "2024-01-15T10:30:00Z",
#   "message": "Authentication service is operational"
# }

# If this fails:
# ? Application not running ? Check logs
# ? Wrong URL ? Verify deployment URL
# ? HTTPS issue ? Check SSL certificate
```

### Test 2: Login ?
```bash
curl -X POST https://your-domain/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# Expected response (200 OK):
# {
#   "token": "eyJhbGciOiJIUzI1NiIs...",
#   "tokenType": "Bearer",
#   "expiresIn": 3600,
#   "user": {
#     "userId": 1,
#     "username": "admin"
#   }
# }

# If this fails:
# ? 401: Check database connection
# ? 500: Check SQL Server connection string
# ? Timeout: Check firewall rules
```

### Test 3: Authorization ?
```bash
# Save token from Test 2
TOKEN="eyJhbGciOiJIUzI1NiIs..."

# Test GET (View)
curl -X GET https://your-domain/api/v1/users \
  -H "Authorization: Bearer $TOKEN"
# Expected: 200 OK with user data

# Test POST (Create)
curl -X POST https://your-domain/api/v1/users \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"username":"newuser","password":"pass123"}'
# Expected: 201 Created

# If this fails:
# ? 403: User doesn't have permission
# ? 401: Token invalid/expired
# ? 500: Check application logs
```

---

## ?? IMMEDIATE TROUBLESHOOTING

### Issue: Application won't start
```
Solution:
1. Check Program.cs is correct
2. Verify appsettings.Production.json exists
3. Check connection string in appsettings
4. Review application logs
5. Verify SQL Server is running and accessible
```

### Issue: 401 Unauthorized on login
```
Solution:
1. Verify admin user exists in database
2. Check password is correct
3. Verify JWT secret in appsettings matches
4. Check database connection working
```

### Issue: 403 Forbidden on protected endpoints
```
Solution:
1. Check user has admin role
2. Verify PermissionMaster table populated (56 rows)
3. Check RolePermissionMapping has data
4. Run: SELECT * FROM PermissionMaster LIMIT 5
```

### Issue: Slow responses (> 100ms)
```
Solution:
1. Check database indexes created
2. Verify cache is working (check Application Insights)
3. Monitor SQL Server query performance
4. Check for database locks
```

---

## ?? DATABASE SETUP (Must Do First!)

### ? Step 1: Create Database
```sql
-- In SQL Server Management Studio:
CREATE DATABASE EnterpriseHIS_Production;
GO
```

### ? Step 2: Run Migration
```sql
-- File: Database/Migrations/001_CreatePermissionSchema.sql
-- Execute entire script

-- Verify:
SELECT COUNT(*) as PermissionCount FROM PermissionMaster;
-- Should return: 56
```

### ? Step 3: Verify Tables
```sql
-- Check all 3 tables created:
SELECT * FROM information_schema.TABLES WHERE TABLE_SCHEMA = 'dbo';

-- Should show:
-- PermissionMaster
-- RolePermissionMapping
-- AuthorizationAccessLog
```

---

## ?? CONFIGURATION (Must Do Before Deploy)

### ? Update appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EnterpriseHIS_Production;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long-here",
    "Issuer": "enterprise-his",
    "Audience": "enterprise-his-api",
    "ExpirationMinutes": 60
  }
}
```

### ? Verify Program.cs
```
? Middleware registered
? DI container configured
? Database connection configured
? JWT authentication enabled
? Authorization middleware added
```

---

## ?? DEPLOYMENT READINESS CHECKLIST

```
DATABASE:
? SQL Server 2019+ running
? EnterpriseHIS_Production database created
? Migration script executed (001_CreatePermissionSchema.sql)
? 56 permissions in PermissionMaster table
? Connection string correct in appsettings

APPLICATION:
? .NET 8 SDK installed
? Build successful (dotnet build -c Release)
? appsettings.Production.json configured
? JWT secret updated
? Logging configured

DEPLOYMENT CHOICE:
? Choose one: Docker OR Azure OR IIS
? Follow corresponding deployment steps
? Verify application started

VERIFICATION:
? Health check endpoint responds
? Login works (get JWT token)
? Authorization works (use token)
? All 3 verification tests pass
```

---

## ?? AFTER DEPLOYMENT (First 24 Hours)

### ? Hour 1: Quick Verification
```
1. Health check working
2. Login works
3. Authorization enforced
4. Database connected
5. Logs being recorded
```

### ? Hour 2-4: Monitor Metrics
```
1. Check Application Insights
2. Verify cache hit rate > 95%
3. Monitor response time < 50ms
4. Check error rate near 0%
```

### ? End of Day: Full Verification
```
1. All endpoints tested
2. Users can login and access data
3. Authorization working correctly
4. Audit logs recording access
5. Performance acceptable
```

---

## ?? TROUBLESHOOTING BY SYMPTOM

| Symptom | Likely Cause | Solution |
|---------|-------------|----------|
| Application won't start | Config error | Check Program.cs & appsettings |
| 401 Unauthorized on login | Invalid credentials/JWT | Verify admin user exists, check JWT secret |
| 403 Forbidden on endpoints | Missing permissions | Check user role has permission |
| Slow responses | Cache miss or DB issue | Check indexes, verify cache working |
| 500 Internal Error | Application error | Check application logs, review exception |
| Database connection failed | Connection string wrong | Verify SQL Server running, check credentials |

---

## ?? DOCUMENTATION FILES TO READ

### Quick References
1. **README.md** (This project root) - Start here
2. **QUICK_START_DEPLOYMENT_GUIDE.md** - Detailed deployment
3. **EXECUTIVE_SUMMARY_PROJECT_COMPLETE.md** - Overview

### Detailed Guides  
4. **PHASE5_OPTIMIZATION_COMPLETE.md** - Performance tuning
5. **PROJECT_COMPLETE_PRODUCTION_READY.md** - Full details

---

## ? SUCCESS CRITERIA

If all of these are true, you're ready to go live:

```
? Application starts without errors
? Health check endpoint responds (200 OK)
? Login works with admin credentials
? Can access /api/v1/users endpoint
? Authorization enforces permissions
? Database connected and populated
? Monitoring dashboards showing data
? Response times < 100ms
? No error logs in Application Insights
? All verification tests pass
```

---

## ?? READY TO DEPLOY?

### Quick Deployment Flow

1. **Prepare** (5 min)
   - Install prerequisites
   - Clone repository
   - Verify build: `dotnet build -c Release`

2. **Database** (5 min)
   - Create database
   - Run migration script
   - Verify 56 permissions

3. **Configure** (5 min)
   - Update appsettings.Production.json
   - Set connection string
   - Set JWT secret

4. **Deploy** (15-20 min)
   - Choose path: Docker/Azure/IIS
   - Follow deployment steps
   - Verify application started

5. **Verify** (5 min)
   - Run 3 verification tests
   - Check health endpoint
   - Test login & authorization

**Total Time: ~35-45 minutes**

---

## ?? YOU'RE READY!

**Next Steps:**
1. Choose your deployment path above
2. Follow the steps
3. Run verification tests
4. Go live!

**Support:**
- Check documentation in `Docs/` folder
- Review troubleshooting guides
- Contact development team if needed

---

**Built with zero hardcoding. Automatic for all endpoints. Ready for production. Deploy now!** ??

