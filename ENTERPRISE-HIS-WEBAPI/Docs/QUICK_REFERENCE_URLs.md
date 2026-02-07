# ? QUICK FIX - SWAGGER 404 ERROR

## ? Wrong URL
```
http://localhost:5000/swagger/index.html   ? WRONG
```

## ? Correct URL
```
https://localhost:5001                     ? RIGHT
```

---

## ?? Key Points

| Point | Value |
|-------|-------|
| **Protocol** | HTTPS (not HTTP) |
| **Port** | 5001 (not 5000) |
| **Path** | Root `/` (not `/swagger`) |
| **Certificate** | Self-signed (accept warning) |

---

## ?? Access Swagger

### Step 1: Start App
```powershell
dotnet run
```

### Step 2: Open Browser
```
https://localhost:5001
```

### Step 3: Accept Warning
Click "Advanced" ? "Proceed" ? Done

---

## ?? URL Reference

```
Swagger UI:       https://localhost:5001
Health Check:     https://localhost:5001/health
Readiness:        https://localhost:5001/health/ready
Liveness:         https://localhost:5001/health/live
API Endpoint:     https://localhost:5001/api/v1/lookuptypes
```

---

**That's it! Bookmark: `https://localhost:5001`** ?
