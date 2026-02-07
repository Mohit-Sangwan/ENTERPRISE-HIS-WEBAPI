# ? SWAGGER 404 - QUICK VISUAL GUIDE

## ? WRONG URLs (All Fail)

```
http://localhost:5000/swagger           ? 404
http://localhost:5000/swagger/index     ? 404
https://localhost:5001/swagger          ? 404
https://localhost:5001/swagger/ui       ? 404
https://localhost:5001/swagger/index    ? 404
```

## ? CORRECT URL

```
https://localhost:5001                  ? WORKS!
```

---

## ?? Why?

Your `Program.cs` has:
```csharp
options.RoutePrefix = string.Empty;  // ? Swagger at ROOT, not /swagger
```

So:
- Swagger UI is at `/` (root)
- NOT at `/swagger`
- Only on HTTPS
- Only on port 5001
- Only in Development mode

---

## ?? 3 Steps to Success

### Step 1: Start App
```powershell
dotnet run
```

### Step 2: Open Browser
```
https://localhost:5001
```

### Step 3: Accept Certificate
Click "Advanced" ? "Proceed"

---

## ? What You'll See

```
? Swagger UI with all endpoints
? "Try it out" buttons to test APIs
? Request/Response examples
? Full API documentation
```

---

## ?? URL Reference

| Need | URL |
|------|-----|
| Swagger UI | `https://localhost:5001` |
| Health Check | `http://localhost:5000/health` |
| API | `https://localhost:5001/api/v1/lookuptypes` |
| Readiness | `https://localhost:5001/health/ready` |

---

**Use: `https://localhost:5001` (not HTTP, not /swagger)** ?
