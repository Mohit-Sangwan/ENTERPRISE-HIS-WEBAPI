# ?? QUICK TEST GUIDE - HTTP & HTTPS

## ? Your Setup

```
HTTP:  http://localhost:5000    (unencrypted, development)
HTTPS: https://localhost:5001   (encrypted, production-ready)
```

---

## ?? Test HTTP (Port 5000)

### Browser
```
http://localhost:5000/health
```

**Result:**
```json
{
  "status": "healthy",
  "database": "connected"
}
```

### cURL
```bash
curl http://localhost:5000/health
```

### PowerShell
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/health"
```

---

## ?? Test HTTPS (Port 5001)

### Browser
```
https://localhost:5001
```

**Result:**
- Shows certificate warning
- Click "Advanced" ? "Proceed"
- Swagger UI loads

### cURL (Ignore Cert Warning)
```bash
curl -k https://localhost:5001/health
```

### cURL (Trust Certificate First)
```bash
# Option 1: Trust certificate
dotnet dev-certs https --trust

# Option 2: Then normal curl works
curl https://localhost:5001/health
```

### PowerShell
```powershell
$uri = "https://localhost:5001/health"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
Invoke-RestMethod -Uri $uri
```

---

## ?? Test All Endpoints

### Health Checks (HTTP)
```bash
curl http://localhost:5000/health
curl http://localhost:5000/health/ready
curl http://localhost:5000/health/live
```

### Health Checks (HTTPS)
```bash
curl -k https://localhost:5001/health
curl -k https://localhost:5001/health/ready
curl -k https://localhost:5001/health/live
```

### API Endpoints (HTTP)
```bash
# Get all LookupTypes
curl http://localhost:5000/api/v1/lookuptypes

# Get specific LookupType
curl http://localhost:5000/api/v1/lookuptypes/1

# Search
curl "http://localhost:5000/api/v1/lookuptypes/search?searchTerm=gender"
```

### API Endpoints (HTTPS)
```bash
# Get all LookupTypes
curl -k https://localhost:5001/api/v1/lookuptypes

# Get specific LookupType
curl -k https://localhost:5001/api/v1/lookuptypes/1

# Search
curl -k "https://localhost:5001/api/v1/lookuptypes/search?searchTerm=gender"
```

---

## ?? Common Test Scenarios

### Scenario 1: Quick Health Check
```bash
# Fastest - no SSL overhead
curl http://localhost:5000/health
```

### Scenario 2: Test Redirect (HTTP ? HTTPS)
```bash
# Follow redirect
curl -L http://localhost:5000/api/v1/lookuptypes
# Should end up at: https://localhost:5001/api/v1/lookuptypes
```

### Scenario 3: Swagger Testing
```
1. Open: https://localhost:5001
2. Accept certificate warning
3. Click "Try it out" on any endpoint
4. Click "Execute"
```

### Scenario 4: Database Connection
```bash
# HTTP
curl http://localhost:5000/health

# Check database status in response:
{
  "status": "healthy",
  "database": "connected",  ? Here
  "checks": {
    "database": "? OK"      ? And here
  }
}
```

---

## ?? Response Comparison

### HTTP Request
```bash
curl http://localhost:5000/health
```

**Response (Development):**
```json
HTTP/1.1 200 OK

{
  "status": "healthy",
  "database": "connected",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0",
  "environment": "Development",
  "checks": {
    "database": "? OK",
    "api": "? OK"
  }
}
```

### HTTPS Request
```bash
curl -k https://localhost:5001/health
```

**Response (Same):**
```json
HTTP/2 200 OK

{
  "status": "healthy",
  "database": "connected",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0",
  "environment": "Development",
  "checks": {
    "database": "? OK",
    "api": "? OK"
  }
}
```

---

## ?? Verify Both Ports Are Working

### Check HTTP
```bash
curl -v http://localhost:5000/health
# Should see: Connected to localhost (127.0.0.1) port 5000 (#0)
```

### Check HTTPS
```bash
curl -v -k https://localhost:5001/health
# Should see: Connected to localhost (127.0.0.1) port 5001 (#0)
```

### Check Using netstat
```powershell
netstat -ano | findstr "5000\|5001"

# Should show:
# TCP    127.0.0.1:5000    LISTENING    [PID]
# TCP    127.0.0.1:5001    LISTENING    [PID]
```

---

## ?? Browser Testing

### HTTP in Browser
```
Address bar: http://localhost:5000/health

Result:
{
  "status": "healthy",
  "database": "connected"
}
```

### HTTPS in Browser
```
Address bar: https://localhost:5001

Result:
- Certificate warning appears
- Click "Advanced"
- Click "Proceed to localhost (unsafe)"
- Swagger UI loads
```

### Swagger Testing
```
1. Go to: https://localhost:5001
2. Click any GET endpoint (e.g., "GET /health")
3. Click "Try it out"
4. Click "Execute"
5. See response below
```

---

## ??? Troubleshooting Tests

### Issue: "Cannot connect to localhost:5000"
```bash
# Solution 1: App not running
dotnet run

# Solution 2: Port in use
netstat -ano | findstr 5000
taskkill /PID [PID] /F

# Solution 3: Firewall blocked
# Add exception in Windows Firewall
```

### Issue: "Certificate verification failed"
```bash
# Solution 1: Ignore for testing
curl -k https://localhost:5001/health

# Solution 2: Trust certificate
dotnet dev-certs https --trust

# Solution 3: Use HTTP instead
curl http://localhost:5000/health
```

### Issue: "301 Redirect Loop"
```bash
# In Development: Should not redirect
curl http://localhost:5000/health
# Should return 200, not 301

# If redirecting, check Program.cs:
# app.UseHttpsRedirection() should only run in !Development
```

---

## ?? Quick Reference Table

| Test | URL | Command |
|------|-----|---------|
| Health (HTTP) | http://localhost:5000/health | `curl http://localhost:5000/health` |
| Health (HTTPS) | https://localhost:5001/health | `curl -k https://localhost:5001/health` |
| Swagger | https://localhost:5001 | Open in browser |
| API (HTTP) | http://localhost:5000/api/v1/lookuptypes | `curl http://localhost:5000/api/v1/lookuptypes` |
| API (HTTPS) | https://localhost:5001/api/v1/lookuptypes | `curl -k https://localhost:5001/api/v1/lookuptypes` |
| Redirect Test | http://localhost:5000/api/v1/... | `curl -L http://localhost:5000/api/v1/...` |

---

## ? Expected Results

Both endpoints should work:
- ? HTTP (5000) - Fast, unencrypted
- ? HTTPS (5001) - Secure, encrypted
- ? Swagger at https://localhost:5001
- ? Health checks at both ports

---

**Test both protocols to ensure everything works!** ?
