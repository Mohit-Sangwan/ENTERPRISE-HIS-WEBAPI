# ?? SWAGGER 404 ERROR - COMPLETE TROUBLESHOOTING

## ? What You're Doing Wrong

```
? http://localhost:5000/swagger/index.html
   ?? HTTP (should be HTTPS)
   ?? Port 5000 (Swagger runs on 5001)
   ?? Path /swagger (should be root /)
```

## ? What You Should Do

```
? https://localhost:5001
   ?? HTTPS ?
   ?? Port 5001 ?
   ?? Root path ?
```

---

## ?? Your Program.cs Configuration

Looking at your code:

```csharp
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Enterprise HIS API v1");
    options.RoutePrefix = string.Empty;  // ? This means ROOT PATH
    options.DocumentTitle = "Enterprise HIS API";
});
```

**What this means:**
- `RoutePrefix = string.Empty` ? Swagger UI is at `/` (root), NOT `/swagger`
- Swagger middleware only runs on HTTPS
- Swagger only runs in Development environment

---

## ?? Correct URLs

### ? For Swagger UI
```
https://localhost:5001
```

### ? For Health Check (both work)
```
http://localhost:5000/health      (no SSL overhead)
https://localhost:5001/health     (encrypted)
```

### ? For API Endpoints (both work)
```
http://localhost:5000/api/v1/lookuptypes
https://localhost:5001/api/v1/lookuptypes
```

---

## ?? Step-by-Step Verification

### Step 1: Verify App is Running
```powershell
# You should see output like:
# Now listening on: https://localhost:5001
# Now listening on: http://localhost:5000
```

### Step 2: Test Health Endpoint First
```bash
# Test HTTP (should work)
curl http://localhost:5000/health

# Should return:
# {"status":"healthy","database":"connected",...}
```

### Step 3: Access Swagger UI
```
Open browser: https://localhost:5001
(NOT http://localhost:5000/swagger/index.html)
```

### Step 4: Accept Certificate Warning
- Chrome/Edge: Click "Advanced" ? "Proceed to localhost"
- Firefox: Click "Advanced" ? "Accept the Risk"
- Safari: Click "Show Details" ? "Visit this website"

---

## ?? Debugging: Why Swagger Isn't Loading

### Issue 1: Using HTTP Instead of HTTPS
**Symptom:** 404 error on http://localhost:5000/swagger  
**Cause:** Swagger only runs on HTTPS in this config  
**Fix:** Use `https://localhost:5001`

### Issue 2: Wrong Swagger Path
**Symptom:** 404 on https://localhost:5001/swagger  
**Cause:** RoutePrefix is empty, so it's at `/` not `/swagger`  
**Fix:** Use `https://localhost:5001` (root)

### Issue 3: App Not in Development Mode
**Symptom:** No Swagger at all  
**Cause:** Swagger only loads if IsDevelopment() = true  
**Fix:** Ensure `ASPNETCORE_ENVIRONMENT=Development`

### Issue 4: App Not Running
**Symptom:** Connection refused  
**Cause:** Application not started  
**Fix:** Run `dotnet run`

---

## ?? URL Routing Map

```
REQUEST URL                              RESPONSE
================================================

http://localhost:5000/health            ? JSON response
https://localhost:5001/health           ? JSON response
https://localhost:5001/health/ready     ? JSON response
https://localhost:5001/health/live      ? JSON response

https://localhost:5001                  ? Swagger UI
https://localhost:5001/                 ? Swagger UI (same)
https://localhost:5001/swagger          ? 404 (wrong path)
https://localhost:5001/swagger/ui       ? 404 (wrong path)
https://localhost:5001/swagger/index    ? 404 (wrong path)

http://localhost:5000/swagger           ? 404 (wrong port/protocol)
http://localhost:5000/swagger/index     ? 404 (wrong port/protocol)
```

---

## ?? How Swagger is Configured in Your App

```csharp
// This code sets up Swagger:
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();           // ? Enables /swagger/* endpoints
    app.UseSwaggerUI(options => 
    {
        options.RoutePrefix = string.Empty;  // ? Swagger at ROOT (/)
        options.DocumentTitle = "Enterprise HIS API";
    });
}

app.UseCors("AllowAll");        // ? CORS must be before this
app.UseHttpsRedirection();      // ? Redirects HTTP to HTTPS (in prod)
```

**Result:**
- Swagger endpoint: `/swagger/v1/swagger.json` (auto-generated)
- Swagger UI: `/` (root, because RoutePrefix is empty)
- Only in Development mode
- Only on HTTPS in production

---

## ? Working Test Sequence

### 1. Start Application
```powershell
cd D:\Mohit\ENTERPRISE-HIS\ENTERPRISE-HIS-WEBAPI\ENTERPRISE-HIS-WEBAPI
dotnet run
```

**Wait for:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### 2. Test HTTP Endpoint
```bash
curl http://localhost:5000/health
# Should return: {"status":"healthy",...}
```

### 3. Test HTTPS Endpoint
```bash
curl -k https://localhost:5001/health
# Should return: {"status":"healthy",...}
```

### 4. Open Swagger UI
```
Browser address bar:
https://localhost:5001

(Then accept certificate warning)
```

### 5. See Swagger UI
```
? You should see:
   - "Enterprise HIS API" title
   - List of endpoints (/health, /api/v1/lookuptypes, etc.)
   - Try it out buttons on each endpoint
```

---

## ??? If Swagger Still Doesn't Show

### Check 1: Verify Development Environment
```csharp
// Add this to Program.cs temporarily to debug
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"IsDevelopment: {app.Environment.IsDevelopment()}");
```

**Expected:** IsDevelopment = true

### Check 2: Verify CORS is Working
```bash
curl -k -H "Origin: http://localhost:3000" https://localhost:5001
# Should have CORS headers in response
```

### Check 3: View Network Errors
```
1. Open browser DevTools (F12)
2. Go to Console tab
3. Try to load https://localhost:5001
4. Look for error messages
5. Copy error and search online
```

### Check 4: Check Console Output
```
Look for any error messages when starting app:
? "Could not load assembly"
? "Could not find endpoint"
? Configuration errors
```

---

## ?? Browser-Specific Issues

### Chrome/Edge
```
1. Address: https://localhost:5001
2. Error appears: Certificate not trusted
3. Click "Advanced"
4. Click "Proceed to localhost (unsafe)"
5. Swagger loads ?
```

### Firefox
```
1. Address: https://localhost:5001
2. Error appears: Connection is not secure
3. Click "Advanced"
4. Click "Accept the Risk and Continue"
5. Swagger loads ?
```

### Safari
```
1. Address: https://localhost:5001
2. Error appears: Safari can't verify server
3. Click "Show Details"
4. Click "Visit this website"
5. Swagger loads ?
```

---

## ?? Quick Checklist

Before accessing Swagger:

- [ ] Application is running (`dotnet run`)
- [ ] Console shows "Now listening on: https://localhost:5001"
- [ ] No error messages in console
- [ ] ASPNETCORE_ENVIRONMENT is "Development"
- [ ] Using HTTPS (not HTTP)
- [ ] Using port 5001 (not 5000)
- [ ] Using root path (not /swagger)
- [ ] Browser allows certificate exception
- [ ] JavaScript is enabled in browser
- [ ] No proxy/VPN interfering

---

## ? Expected Result

### When You Access: https://localhost:5001

You should see:
```
???????????????????????????????????????????
?  Enterprise HIS API                     ?
???????????????????????????????????????????
?                                         ?
?  Servers                                ?
?  ? https://localhost:5001              ?
?                                         ?
?  Health                                 ?
?  GET /health                            ?
?  GET /health/ready                      ?
?  GET /health/live                       ?
?                                         ?
?  LookupTypes                            ?
?  GET /api/v1/lookuptypes               ?
?  POST /api/v1/lookuptypes              ?
?  PUT /api/v1/lookuptypes/{id}          ?
?  DELETE /api/v1/lookuptypes/{id}       ?
?                                         ?
?  ... (more endpoints)                   ?
?                                         ?
?  Try it out >                           ?
?                                         ?
???????????????????????????????????????????
```

---

## ?? Quick Command Reference

```bash
# Start app
dotnet run

# Test health (HTTP)
curl http://localhost:5000/health

# Test health (HTTPS)
curl -k https://localhost:5001/health

# Test API
curl -k https://localhost:5001/api/v1/lookuptypes

# Trust certificate (so you don't see warnings)
dotnet dev-certs https --trust
```

---

## ?? If Still Having Issues

1. **What you tried:** Tell me the exact URL
2. **What you saw:** 404? Timeout? Certificate error?
3. **Console output:** Any errors?
4. **Browser:** Which browser?
5. **Network:** Any proxy/VPN?

---

**The correct URL is: `https://localhost:5001` (not HTTP, not port 5000, not /swagger)** ?

Try that now and it should work! ??
