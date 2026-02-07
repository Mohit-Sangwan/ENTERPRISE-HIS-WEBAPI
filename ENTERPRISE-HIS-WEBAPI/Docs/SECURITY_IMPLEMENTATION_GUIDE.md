# ?? CRITICAL IMPLEMENTATION GUIDE

## Security Stack Implementation (Priority 1)

### 1. JWT AUTHENTICATION - Step by Step

#### Step 1: Add NuGet Packages
```xml
<!-- Add to .csproj -->
<ItemGroup>
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
  <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
</ItemGroup>
```

#### Step 2: Create JWT Settings
```csharp
// Create: Configuration/JwtSettings.cs
public class JwtSettings
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationMinutes { get; set; }
}
```

#### Step 3: Configure in appsettings.json
```json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-key-minimum-32-characters",
    "Issuer": "enterprise-his",
    "Audience": "enterprise-his-api",
    "ExpirationMinutes": 60
  }
}
```

#### Step 4: Update Program.cs
```csharp
// Add before builder.Build():
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
```

---

### 2. AUTHORIZATION & ROLES

#### Create Roles
```csharp
// Constants/AppRoles.cs
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Viewer = "Viewer";
}
```

#### Configure Policies
```csharp
// In Program.cs after AddAuthorization()
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(AppRoles.Admin));
    
    options.AddPolicy("CanManageLookups", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager));
    
    options.AddPolicy("CanViewLookups", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager, 
                          AppRoles.User, AppRoles.Viewer));
});
```

#### Apply to Controllers
```csharp
// In LookupTypesController:

[Authorize(Policy = "CanViewLookups")]
[HttpGet]
public async Task<ActionResult> GetAll() { }

[Authorize(Policy = "CanManageLookups")]
[HttpPost]
public async Task<ActionResult> Create() { }

[Authorize(Policy = "AdminOnly")]
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(int id) { }
```

---

### 3. AUDIT LOGGING

#### Create Audit Table (SQL)
```sql
CREATE TABLE [audit].[AuditLog]
(
    [AuditLogId] INT PRIMARY KEY IDENTITY(1,1),
    [EntityName] NVARCHAR(128) NOT NULL,
    [EntityId] INT NOT NULL,
    [Action] NVARCHAR(50) NOT NULL,  -- CREATE, UPDATE, DELETE
    [UserId] INT NOT NULL,
    [UserName] NVARCHAR(256),
    [OldValues] NVARCHAR(MAX),  -- JSON
    [NewValues] NVARCHAR(MAX),  -- JSON
    [Timestamp] DATETIME2 NOT NULL,
    [IpAddress] NVARCHAR(45),
    [Changes] NVARCHAR(MAX)
);
```

#### Create Audit Service
```csharp
// Services/IAuditService.cs
public interface IAuditService
{
    Task LogActionAsync(
        string entityName,
        int entityId,
        string action,
        int userId,
        string userName,
        object oldValues,
        object newValues,
        string ipAddress);
}

// Services/AuditService.cs
public class AuditService : IAuditService
{
    private readonly ISqlServerDal _dal;

    public AuditService(ISqlServerDal dal)
    {
        _dal = dal;
    }

    public async Task LogActionAsync(
        string entityName, int entityId, string action,
        int userId, string userName,
        object oldValues, object newValues, string ipAddress)
    {
        var oldJson = JsonConvert.SerializeObject(oldValues);
        var newJson = JsonConvert.SerializeObject(newValues);

        // Call stored procedure to insert audit log
        await _dal.ExecuteNonQueryAsync(
            "[audit].[SP_InsertAuditLog]",
            isStoredProc: true,
            parameters: new Dictionary<string, object>
            {
                { "@EntityName", entityName },
                { "@EntityId", entityId },
                { "@Action", action },
                { "@UserId", userId },
                { "@UserName", userName },
                { "@OldValues", oldJson },
                { "@NewValues", newJson },
                { "@IpAddress", ipAddress }
            });
    }
}
```

#### Register Service
```csharp
// In Program.cs
builder.Services.AddScoped<IAuditService, AuditService>();
```

#### Use in Services
```csharp
// In LookupTypeService.cs
public class LookupTypeService
{
    private readonly IAuditService _auditService;

    public async Task CreateAsync(CreateLookupTypeDto request, int userId)
    {
        // Create logic...
        var created = new { /* created object */ };

        // Log audit
        await _auditService.LogActionAsync(
            "LookupType",
            createdId,
            "CREATE",
            userId,
            "unknown", // Get from claims
            null,
            created,
            "0.0.0.0" // Get from HttpContext
        );
    }
}
```

---

### 4. INPUT VALIDATION

#### Install Fluent Validation
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

#### Create Validators
```csharp
// Validators/CreateLookupTypeDtoValidator.cs
public class CreateLookupTypeDtoValidator : AbstractValidator<CreateLookupTypeDto>
{
    public CreateLookupTypeDtoValidator()
    {
        RuleFor(x => x.LookupTypeName)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 100).WithMessage("Name must be 2-100 characters")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("Name must only contain letters and spaces");

        RuleFor(x => x.LookupTypeCode)
            .NotEmpty().WithMessage("Code is required")
            .Length(1, 50).WithMessage("Code must be 1-50 characters")
            .Matches(@"^[A-Z_]+$").WithMessage("Code must be uppercase letters and underscores only");

        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0).WithMessage("DisplayOrder must be greater than 0")
            .LessThanOrEqualTo(9999).WithMessage("DisplayOrder must be 9999 or less");
    }
}
```

#### Register Validators
```csharp
// In Program.cs
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<Program>();
```

---

### 5. GLOBAL EXCEPTION HANDLER

#### Create Middleware
```csharp
// Middleware/GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, 
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = "error",
            message = ex.Message,
            timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = ex switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

#### Register Middleware
```csharp
// In Program.cs (before other middleware)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

---

### 6. FIX HARDCODED USER ID

#### Extract from Claims
```csharp
// Extension method
public static class HttpContextExtensions
{
    public static int GetUserId(this HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userId, out var id) ? id : 0;
    }

    public static string GetUserName(this HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    public static string GetClientIpAddress(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
```

#### Use in Controllers
```csharp
// Replace: private const int DEFAULT_USER_ID = 1;

[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateLookupTypeDto request)
{
    var userId = HttpContext.GetUserId();
    if (userId == 0)
        return Unauthorized();

    var response = await _service.CreateAsync(request, userId);
    return Created AtAction(nameof(GetById), new { id = response.Data.Id }, response);
}
```

---

## ?? IMPLEMENTATION CHECKLIST

```
Priority 1 - Security (Week 1):
? Add NuGet packages
? Configure JWT in appsettings
? Implement JWT authentication
? Create and configure roles
? Add [Authorize] attributes
? Create audit table
? Implement audit service
? Add fluent validation
? Create global exception handler
? Fix hardcoded user ID

Testing:
? Test authentication flow
? Test authorization policies
? Test audit logging
? Test input validation
? Test exception handling
? Test with Swagger

Documentation:
? Update Swagger with Authorize
? Document API security
? Document roles/permissions
? Document error codes
```

---

**Implement these 6 features to reach enterprise-grade security!** ??
