# ?? SWAGGER UI 404 ERROR - FIXED

## ? The Problem

```
You tried:  http://localhost:5000/swagger/index.html
Error:      404 Not Found
```

## ? The Solution

Your application is configured to run on **HTTPS on port 5001**, but you're trying HTTP on port 5000.

---

## ?? Correct Access Points

### Swagger UI
```
URL: https://localhost:5001
```

### Health Check Endpoints
```
https://localhost:5001/health       ? Full health status
https://localhost:5001/health/ready ? Readiness probe
https://localhost:5001/health/live  ? Liveness probe
```

### API Endpoints
```
https://localhost:5001/api/v1/lookuptypes
https://localhost:5001/api/v1/lookuptypevalues
```

---

## ?? Why This Happens

Your `appsettings.json` defines two endpoints:

```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://localhost:5000"     ? HTTP
    },
    "Https": {
      "Url": "https://localhost:5001"    ? HTTPS (Swagger runs here)
    }
  }
}
```

And your `Program.cs` has:
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();  // Redirects HTTP ? HTTPS
}
```

So:
- HTTP (5000) is available but **redirects to HTTPS**
- HTTPS (5001) is where **Swagger UI actually runs**

---

## ? Access Swagger UI

### For Development
1. Open browser
2. Navigate to: **`https://localhost:5001`**
3. Accept self-signed certificate warning

### For Production/Docker
```bash
# If running in Docker
docker run -p 5001:5001 enterprise-his-api:latest
# Access: https://localhost:5001
```

---

## ?? Correct URLs by Environment

### Development
```
Swagger:     https://localhost:5001
Health:      https://localhost:5001/health
API Base:    https://localhost:5001/api/v1
```

### Production
```
Swagger:     https://yourdomain.com
Health:      https://yourdomain.com/health
API Base:    https://yourdomain.com/api/v1
```

---

## ?? How to Run

### 1. Start the Application
```powershell
dotnet run
```

### 2. Wait for Console Output
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### 3. Open Browser
```
https://localhost:5001
```

### 4. Accept Certificate Warning
- Chrome: Click "Advanced" ? "Proceed to localhost (unsafe)"
- Firefox: Click "Advanced" ? "Accept the Risk and Continue"
- Edge: Click "Continue to this website"

---

## ?? Port Configuration

| Port | Protocol | Purpose | Swagger | Health |
|------|----------|---------|---------|--------|
| 5000 | HTTP | Development testing | ? Redirects | ? Redirects |
| 5001 | HTTPS | Development/Production | ? Available | ? Available |

---

## ?? Why HTTPS for Development

Your configuration enforces HTTPS in production for security:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();  // Only in non-dev
}
```

In **Development**, both HTTP and HTTPS work, but Swagger runs on HTTPS.

---

## ?? Quick Troubleshooting

### Problem: "Can't reach localhost:5001"
**Solution:** 
- Make sure application is running
- Check console for "Now listening on:"
- Wait 2-3 seconds after starting

### Problem: "SSL Certificate Error"
**Solution:**
- This is normal for self-signed certificates
- Accept the warning in browser
- Or run: `dotnet dev-certs https --trust`

### Problem: "Still getting 404"
**Solution:**
- Try clearing browser cache (Ctrl+Shift+Delete)
- Try in private/incognito mode
- Check that Swagger endpoint is `/` (empty prefix)

---

## ? Configuration Details

Your Swagger is configured to:
1. Run at root path (`RoutePrefix = string.Empty`)
2. Use HTTPS on port 5001
3. Show documentation title "Enterprise HIS API"
4. Auto-refresh on file changes (dev)

---

## ?? Final Check

Before accessing Swagger:

- [ ] Application is running (`dotnet run`)
- [ ] No compilation errors
- [ ] Console shows "Now listening on: https://localhost:5001"
- [ ] Browser address: **`https://localhost:5001`** (not HTTP)
- [ ] Certificate warning accepted (if development)

---

## ?? Next Steps

1. **Access Swagger:**
   ```
   https://localhost:5001
   ```

2. **Test Health Endpoint:**
   ```
   https://localhost:5001/health
   ```

3. **Try API Endpoint:**
   ```
   https://localhost:5001/api/v1/lookuptypes
   ```

---

**Everything is working correctly!** The URL was just wrong. Access HTTPS on 5001! ?
