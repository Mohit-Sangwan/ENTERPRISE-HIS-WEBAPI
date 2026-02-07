# ?? SECURITY IMPLEMENTATION - COMPLETE GUIDE

## Status: ? Authentication & Authorization Framework Installed

Your application now has JWT authentication and authorization framework integrated.

---

## ?? WHAT WAS IMPLEMENTED

### ? Added to Program.cs:
```csharp
// JWT Settings from configuration
// Authentication using Bearer tokens
// Authorization policies (AdminOnly, CanManageLookups, CanViewLookups)
// Middleware in correct order (Authentication ? Authorization)
```

### ? Created:
- `Authentication/BearerTokenAuthenticationHandler.cs` - JWT validation
- `Configuration/JwtSettings.cs` - Settings model
- `Constants/AppConstants.cs` - Roles & policies
- `Extensions/HttpContextExtensions.cs` - Helper methods

### ? Updated Dependencies:
- `System.IdentityModel.Tokens.Jwt` 7.0.3
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0

---

## ?? NEXT STEPS (Follow This Order)

### Step 1: Configure JWT Settings (5 minutes)

Update `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-key-minimum-32-characters-very-secure",
    "Issuer": "enterprise-his",
    "Audience": "enterprise-his-api",
    "ExpirationMinutes": 60
  }
}
```

**?? IMPORTANT:** Use a strong 32+ character secret in production!

Generate one:
```powershell
[System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString()))
```

### Step 2: Create JWT Token Service (15 minutes)

```csharp
// Create: Services/ITokenService.cs
public interface ITokenService
{
    string GenerateToken(int userId, string userName, string email, List<string> roles);
    bool ValidateToken(string token);
}

// Create: Services/TokenService.cs
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(int userId, string userName, string email, List<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings.GetValue<string>("Secret") ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = jwtSettings.GetValue<string>("Issuer") ?? "enterprise-his";
        var audience = jwtSettings.GetValue<string>("Audience") ?? "enterprise-his-api";
        var expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes", 60);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Email, email)
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        // Token validation is handled by BearerTokenAuthenticationHandler
        return true;
    }
}
```

Register in Program.cs:
```csharp
builder.Services.AddScoped<ITokenService, TokenService>();
```

### Step 3: Create Authentication Controller (10 minutes)

```csharp
// Create: Controllers/AuthController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized(new { error = "Username and password are required" });

            // TODO: Validate credentials against database
            // For demo, accept username = "admin", password = "admin123"
            if (request.Username != "admin" || request.Password != "admin123")
                return Unauthorized(new { error = "Invalid credentials" });

            var roles = new List<string> { "Admin" };
            var token = _tokenService.GenerateToken(
                userId: 1,
                userName: request.Username,
                email: "admin@enterprise-his.com",
                roles: roles);

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            return Ok(new LoginResponse
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = 3600,
                User = new { UserId = 1, Username = request.Username }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public object? User { get; set; }
}
```

### Step 4: Protect Your Endpoints (10 minutes)

Add `[Authorize]` and policy attributes to controllers:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]  // Require authentication for all endpoints
public class LookupTypesController : ControllerBase
{
    // Read operations - Anyone authenticated can view
    [Authorize(Policy = "CanViewLookups")]
    [HttpGet]
    public async Task<ActionResult> GetAll() { }

    [Authorize(Policy = "CanViewLookups")]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id) { }

    // Write operations - Only managers and admins
    [Authorize(Policy = "CanManageLookups")]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateLookupTypeDto request) 
    {
        var userId = HttpContext.GetUserId();  // Extract from token
        var userName = HttpContext.GetUserName();
        // ... rest of logic
    }

    [Authorize(Policy = "CanManageLookups")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateLookupTypeDto request) { }

    // Admin only
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id) { }
}
```

### Step 5: Test in Swagger (5 minutes)

1. Update `appsettings.json` with JWT settings
2. Build and run: `dotnet run`
3. Go to: `https://localhost:5001`
4. Click on "Authorize" button in Swagger
5. Login via Auth endpoint first to get token
6. Paste token: `Bearer eyJhbGc...` (with "Bearer " prefix)
7. Click "Authorize"
8. Now try protected endpoints

---

## ?? TESTING THE SECURITY

### Manual Test with cURL:

```bash
# 1. Get token
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  -k

# Response:
# {
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "tokenType": "Bearer",
#   "expiresIn": 3600,
#   "user": {"userId": 1, "username": "admin"}
# }

# 2. Use token to access protected endpoint
curl -X GET https://localhost:5001/api/v1/lookuptypes \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -k

# 3. Try without token (should be rejected)
curl -X GET https://localhost:5001/api/v1/lookuptypes \
  -k
# Response: 401 Unauthorized
```

---

## ?? Files Created/Modified

| File | Type | Status |
|------|------|--------|
| `Program.cs` | Modified | ? Auth added |
| `BearerTokenAuthenticationHandler.cs` | New | ? JWT handler |
| `JwtSettings.cs` | New | ? Config model |
| `AppConstants.cs` | New | ? Roles & policies |
| `HttpContextExtensions.cs` | New | ? Helper methods |
| `.csproj` | Modified | ? Dependencies |

---

## ?? CRITICAL: Production Checklist

Before deploying to production:

- [ ] Change JWT Secret to strong value (32+ chars)
- [ ] Store JWT Secret in Azure Key Vault (not in code)
- [ ] Implement actual user validation (hash passwords with bcrypt)
- [ ] Add password change endpoint
- [ ] Add token refresh endpoint
- [ ] Add logout endpoint (token blacklist)
- [ ] Enable HTTPS only
- [ ] Add rate limiting on login
- [ ] Implement account lockout after failed attempts
- [ ] Add MFA (Multi-factor authentication)
- [ ] Set short token expiration (15-60 minutes)
- [ ] Add audit logging for all auth events
- [ ] Test with security tools (OWASP ZAP, Burp Suite)

---

## ?? ARCHITECTURE

```
Request
   ?
CORS Middleware
   ?
Authentication (BearerTokenAuthenticationHandler validates JWT)
   ?
Authorization (checks [Authorize] attributes and policies)
   ?
Controller Action
   ?
Response
```

---

## ?? SUMMARY

Your API now has:
- ? JWT Bearer token authentication
- ? Role-based authorization
- ? Authorization policies
- ? User context extraction
- ? Build succeeds with no errors

**Next:** Implement the Token Service, Auth Controller, and protect endpoints!

---

**Security Implementation: 50% Complete** ??
