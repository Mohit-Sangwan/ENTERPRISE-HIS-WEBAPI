# ?? COMPLETE SETUP & TROUBLESHOOTING GUIDE

## ? Your Setup Status

| Component | Status | Details |
|-----------|--------|---------|
| **Application** | ? Ready | All code compiled and optimized |
| **Database** | ? Ready | SQL procedures fixed |
| **Configuration** | ? Ready | HTTPS on 5001 configured |
| **Health Checks** | ? Ready | 3 endpoints available |
| **Swagger UI** | ? Ready | Available at root |

---

## ?? Getting Started in 3 Steps

### Step 1: Start Application
```powershell
cd D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### Step 2: Open Browser
```
https://localhost:5001
```

### Step 3: Accept Certificate Warning
- Click "Advanced" or "Proceed"
- Done! ??

---

## ?? Available URLs

### Swagger UI & Documentation
```
https://localhost:5001              ? Main entry point
https://localhost:5001/swagger      ? Alternative
```

### Health Check Endpoints
```
https://localhost:5001/health       ? Full status
https://localhost:5001/health/ready ? Readiness (K8s)
https://localhost:5001/health/live  ? Liveness (K8s)
```

### API Endpoints
```
https://localhost:5001/api/v1/lookuptypes
https://localhost:5001/api/v1/lookuptypevalues
```

---

## ?? Troubleshooting

### Problem 1: 404 Error
**Symptom:** "No web page found"  
**Cause:** Wrong URL (HTTP instead of HTTPS, or port 5000 instead of 5001)  
**Solution:** Use `https://localhost:5001`

### Problem 2: SSL Certificate Error
**Symptom:** "Your connection is not private"  
**Cause:** Browser doesn't trust self-signed certificate  
**Solution:** Click "Advanced" ? "Proceed", OR trust certificate:
```powershell
dotnet dev-certs https --trust
```

### Problem 3: Connection Refused
**Symptom:** "Can't reach localhost"  
**Cause:** Application not running  
**Solution:** 
```powershell
dotnet run
```

### Problem 4: Port Already in Use
**Symptom:** "Address already in use"  
**Cause:** Something else is using port 5001  
**Solution:**
```powershell
# Find what's using port
netstat -ano | findstr :5001

# Kill the process
taskkill /PID [ProcessID] /F
```

### Problem 5: Database Connection Error
**Symptom:** Health check shows "unhealthy"  
**Cause:** Connection string invalid or database not accessible  
**Solution:**
```json
// Check appsettings.json
"DefaultConnection": "Server=.;Database=EnterpriseHIS;..."
// Verify SQL Server is running
// Verify database exists
```

---

## ?? Current Configuration

### Endpoints
```
HTTP:  http://localhost:5000  (auto-redirects to HTTPS)
HTTPS: https://localhost:5001 (main)
```

### Middleware Pipeline
```
1. Error Handler
2. HTTPS Redirect
3. CORS ?
4. Swagger
5. Response Compression
6. Authentication
7. Authorization
8. Controllers
```

### Features Enabled
```
? Caching (5 min TTL)
? High Performance Mode
? Data Masking
? Retry Policy (3 attempts)
? Connection Pooling (50-200)
? Response Compression
? CORS (AllowAll)
```

---

## ?? Quick Test Commands

### Test Health Endpoint
```bash
curl -k https://localhost:5001/health
```

### Test with HTTPS Redirect
```bash
curl -k -L http://localhost:5000/health
```

### Test LookupTypes API
```bash
curl -k https://localhost:5001/api/v1/lookuptypes
```

### Test with Pretty JSON (if jq installed)
```bash
curl -k https://localhost:5001/health | jq .
```

---

## ?? Files Modified/Created

### Core Files
- ? `Program.cs` - Fully optimized
- ? `appsettings.json` - Configured correctly
- ? Database procedures - SQL fixed

### Documentation Created
- ?? `SWAGGER_404_FIX.md` - This issue explained
- ?? `QUICK_REFERENCE_URLs.md` - URL reference
- ?? `SSL_CERTIFICATE_SETUP.md` - Certificate guide
- ?? `PROGRAM_CS_FINAL_GUIDE.md` - Program.cs details
- ?? And many more...

---

## ?? Development Workflow

### Daily Development
```powershell
# 1. Open terminal
cd D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI

# 2. Run application
dotnet run

# 3. Open browser
https://localhost:5001

# 4. Test endpoints in Swagger
# Click "Try it out" on any endpoint
```

### After Database Changes
```powershell
# 1. Update SQL files
# 2. Run in SQL Server Management Studio
# 3. Restart application
# 4. Test in Swagger
```

### Before Deployment
```powershell
# 1. Build release
dotnet publish -c Release

# 2. Update appsettings for production
# 3. Update CORS policy
# 4. Update connection string
# 5. Deploy to server/Azure
```

---

## ?? Next Steps

1. **Start Application**
   ```powershell
   dotnet run
   ```

2. **Trust Certificate** (optional but recommended)
   ```powershell
   dotnet dev-certs https --trust
   ```

3. **Access Swagger**
   ```
   https://localhost:5001
   ```

4. **Test Endpoints**
   - Click "Try it out" on any endpoint
   - Or use cURL commands above

5. **Create Sample Data**
   - POST /api/v1/lookuptypes
   - Try creating a lookup type

---

## ?? Application Health

```
? Source Code:        Compiled successfully
? Configuration:      All settings applied
? Database:           Connection pooling ready
? Caching:            5-minute TTL configured
? Health Checks:      3 endpoints available
? CORS:               Enabled and working
? Swagger:            Available at root
? Logging:            Console configured
? SSL:                Development certificate ready
```

---

## ?? You're All Set!

Everything is working correctly:
- ? Application compiles
- ? Configuration is correct
- ? Database procedures are fixed
- ? Swagger UI is ready
- ? Health checks are available

**Just access: `https://localhost:5001`** ??

---

## ?? Support Resources

- **Microsoft Docs:** https://docs.microsoft.com/dotnet
- **ASP.NET Core:** https://docs.microsoft.com/aspnet/core
- **Swagger UI:** https://swagger.io
- **HTTPS Setup:** https://docs.microsoft.com/aspnet/core/security/https

---

**Happy coding!** ?
