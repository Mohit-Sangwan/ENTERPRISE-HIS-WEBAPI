using Enterprise.DAL.V1.Bootstrap;
using Enterprise.DAL.V1.Contracts;
using ENTERPRISE_HIS_WEBAPI.Data.Repositories;
using ENTERPRISE_HIS_WEBAPI.Services;
using ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication;
using ENTERPRISE_HIS_WEBAPI.Authentication;
using ENTERPRISE_HIS_WEBAPI.Authorization;
using System.IO.Compression;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ===== Configuration =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

var enterpriseConfig = builder.Configuration.GetSection("Enterprise:DAL");

// ===== JWT Configuration =====
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSettings.GetValue<string>("Secret") 
    ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = jwtSettings.GetValue<string>("Issuer") ?? "enterprise-his";
var jwtAudience = jwtSettings.GetValue<string>("Audience") ?? "enterprise-his-api";

// ===== Enterprise DAL Setup =====
var dalBootstrap = EnterpriseDalBootstrap.Initialize(connectionString);

// Apply configuration options
if (enterpriseConfig.GetValue<bool>("EnableHighPerformance"))
    dalBootstrap = dalBootstrap.WithHighPerformance();

if (enterpriseConfig.GetValue<bool>("EnableCaching"))
{
    var cacheTtl = enterpriseConfig.GetValue<int>("CacheTTLMinutes", 5);
    dalBootstrap = dalBootstrap.WithCaching(ttlMinutes: cacheTtl);
}

if (enterpriseConfig.GetValue<bool>("EnableDataMasking"))
    dalBootstrap = dalBootstrap.WithDataMasking();      // Enterprise security feature

var retryConfig = enterpriseConfig.GetSection("RetryPolicy");
if (retryConfig.GetValue<bool>("Enabled"))
{
    var maxRetries = retryConfig.GetValue<int>("MaxRetries", 3);
    dalBootstrap = dalBootstrap.WithRetryPolicy(maxRetries: maxRetries);
}

// Connection pool is configured via appsettings.json

var builtDal = dalBootstrap.Build();

// ===== Register DAL Services =====
builder.Services.AddSingleton(builtDal.Dal);  // ✅ Perfect!
builder.Services.AddSingleton(builtDal.Logger);    // ✅ Shared logger
builder.Services.AddSingleton(builtDal.Cache);  // ✅ Singleton cache
builder.Services.AddSingleton(builtDal.Monitor);   // ✅ Shared monitor

// ===== Register Business Services =====
builder.Services.AddScoped<ILookupTypeRepository, LookupTypeRepository>();
builder.Services.AddScoped<ILookupTypeValueRepository, LookupTypeValueRepository>();

// ===== Register Business Services =====
builder.Services.AddScoped<ILookupTypeService, LookupTypeService>();
builder.Services.AddScoped<ILookupTypeValueService, LookupTypeValueService>();

// ===== Register Security Services =====
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDualTokenService, DualTokenService>();
builder.Services.AddScoped<ITokenServiceExtended, TokenServiceExtended>();
builder.Services.AddScoped<IRefreshTokenRepository, InMemoryRefreshTokenRepository>();

// ===== Register Database Policy Services (ENTERPRISE-LEVEL, DATABASE-DRIVEN) =====
// All policies are stored in database - NO hardcoding in code
// PolicyService loads all policies from master.PolicyMaster table
// Role-Policy mappings loaded from config.RolePolicyMapping table
builder.Services.AddMemoryCache();  // For policy caching (1-hour TTL)
builder.Services.AddScoped<IPolicyService, PolicyService>();

// ===== Register Enterprise Authorization Services =====
builder.Services.AddScoped<ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise.IAuthorizationAuditService, 
    ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise.AuthorizationAuditService>();
builder.Services.AddScoped<ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise.Services.IEnterprisePermissionService,
    ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise.Services.EnterprisePermissionService>();

// ===== Register Database Services for Roles & Permissions =====
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IDynamicAuthorizationService, DynamicAuthorizationService>();
builder.Services.AddHttpContextAccessor();  // Required for DynamicPolicyProvider

// ===== Register User Management Services =====
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// ===== Register Authentication Services =====
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// ===== Register Two-Factor Authentication (2FA) Services =====
builder.Services.AddScoped<ISMSService, SMSService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<ITwoFactorConfigurationService, TwoFactorConfigurationService>();

// ===== Authentication Setup =====
builder.Services.AddAuthentication("Bearer")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, BearerTokenAuthenticationHandler>("Bearer", null);

// ===== Authorization Setup (True Enterprise - Dynamic Database-Driven) =====
// Uses DynamicPolicyProvider to load policies from Module.Operation format
// NO hardcoding - pure database-driven at runtime
// Example: "Lookups.View" → Module=Lookups, Operation=View (loaded from database)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, DynamicModuleOperationHandler>();  // NEW: Module.Operation handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();  // LEGACY: Permission_CODE handler

builder.Services.AddAuthorization();  // Empty - all policies from database

// ===== Add API Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Enterprise HIS Web API",
        Version = "v1.0",
        Description = "Healthcare Information System API with Enterprise.DAL.V1 Integration",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Enterprise Support"
        }
    });
});

// ===== Add Response Compression =====
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = new[]
    {
        "application/json",
        "application/javascript",
        "text/plain",
        "text/css",
        "text/html"
    };
});

// ===== Add CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Type", "X-Total-Count");
    });

    // Development-specific CORS policy
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5000", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Type", "X-Total-Count");
    });
});

// ===== Build Application =====
var app = builder.Build();

// ===== Middleware Configuration - ORDER MATTERS =====
// 1. Error handling (must be first)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

// 2. HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 3. CORS (must be before authorization)
app.UseCors("AllowAll");

// 4. Response compression
app.UseResponseCompression();

// 5. Authentication and Authorization (CRITICAL ORDER)
app.UseAuthentication();  // Must be before UseAuthorization
app.UseAuthorization();

// 5.5. Enterprise Authorization Middleware (NEW: Automatic authorization based on request)
app.UseMiddleware<ENTERPRISE_HIS_WEBAPI.Authorization.Enterprise.EnterpriseAuthorizationMiddleware>();

// 6. Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Enterprise HIS API v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "Enterprise HIS API";
    });
}

// 7. Map controllers
app.MapControllers();

// ===== Health Check Endpoints =====
app.MapGet("/health", async (ISqlServerDal dal) =>
{
    try
    {
        var dbCheck = await dal.ExecuteScalarAsync<int>(
            "SELECT 1",
            isStoredProc: false);

        var response = new
        {
            status = dbCheck.Success ? "healthy" : "unhealthy",
            database = dbCheck.Success ? "connected" : "disconnected",
            timestamp = DateTime.UtcNow,
            version = "1.0",
            environment = app.Environment.EnvironmentName,
            checks = new
            {
                database = dbCheck.Success ? "✓ OK" : "✗ FAILED",
                api = "✓ OK"
            }
        };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "unhealthy",
            database = "disconnected",
            error = ex.Message,
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        });
    }
})
.WithName("HealthCheck")
.Produces(StatusCodes.Status200OK);

app.MapGet("/health/ready", async (ISqlServerDal dal) =>
{
    try
    {
        var result = await dal.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES",
            isStoredProc: false);

        var readinessChecks = new
        {
            database = result.Success ? "ready" : "not_ready",
            api = "ready",
            cache = "ready"
        };

        return result.Success 
            ? Results.Ok(new
            {
                ready = true,
                timestamp = DateTime.UtcNow,
                checks = readinessChecks
            })
            : Results.StatusCode(503);
    }
    catch (Exception ex)
    {
        return Results.StatusCode(503);
    }
})
.WithName("ReadinessCheck")
.Produces(StatusCodes.Status200OK);

app.MapGet("/health/live", () =>
{
    return Results.Ok(new
    {
        status = "alive",
        timestamp = DateTime.UtcNow
    });
})
.WithName("LivenessCheck")
.Produces(StatusCodes.Status200OK);

// ===== Graceful Shutdown =====
app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application is shutting down...");
});

app.Run();


// https://localhost:5001              (Swagger)
// https://localhost:5001/health       (Status)
// https://localhost:5001/health/ready (Readiness)
// https://localhost:5001/health/live  (Liveness)
// https://localhost:5001/api/v1/...   (API)

// TODO:
/*
✓ Well-designed API (RESTful, versioned)
✓ Complete database layer (17 SPs)
✓ Proper middleware pipeline
✓ Good error handling structure
✓ Comprehensive endpoints
✓ Clear documentation
✓ Health checks implemented
✓ Caching configured
*/

// Week 1: Security (Auth, Audit, Validation)
// Week 2: Logging & Rate Limiting
// Week 3: Testing
// Week 4: Monitoring & Fine-tuning


//Authentication .......................... 0%
//Authorization .......................... 0%
//Audit & Compliance ..................... 0%
//Request Logging ........................ 0%
//Rate Limiting .......................... 0%
//Input Validation ....................... 50%
//Global Error Handling .................. 0%
//Testing ............................... 0%
//User Tracking ......................... 10%
//Monitoring ............................ 30%
//────────────────────────────────────
//Average ............................ 9%


