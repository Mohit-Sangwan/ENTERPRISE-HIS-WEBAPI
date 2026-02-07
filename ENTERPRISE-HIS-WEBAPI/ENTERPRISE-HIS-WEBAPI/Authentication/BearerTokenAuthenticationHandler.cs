using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace ENTERPRISE_HIS_WEBAPI.Authentication
{
    /// <summary>
    /// Custom Bearer Token Authentication Handler for JWT validation
    /// </summary>
    public class BearerTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string AuthorizationHeaderName = "Authorization";
        private const string BearerScheme = "Bearer";

        public BearerTokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Get JWT settings from configuration
                var jwtSecret = Context.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetValue<string>("Jwt:Secret");

                var jwtIssuer = Context.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetValue<string>("Jwt:Issuer") ?? "enterprise-his";

                var jwtAudience = Context.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetValue<string>("Jwt:Audience") ?? "enterprise-his-api";

                if (string.IsNullOrEmpty(jwtSecret))
                    return Task.FromResult(AuthenticateResult.Fail("JWT Secret not configured"));

                // Extract bearer token from Authorization header
                if (!Request.Headers.ContainsKey(AuthorizationHeaderName))
                    return Task.FromResult(AuthenticateResult.NoResult());

                var authHeader = Request.Headers[AuthorizationHeaderName].ToString();
                if (!authHeader.StartsWith(BearerScheme + " ", StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(AuthenticateResult.NoResult());

                var token = authHeader.Substring(BearerScheme.Length).Trim();

                // Validate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtSecret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Create authentication ticket
                var ticket = new AuthenticationTicket(principal, BearerScheme);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (SecurityTokenExpiredException)
            {
                return Task.FromResult(AuthenticateResult.Fail("Token has expired"));
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid token signature"));
            }
            catch (SecurityTokenException ex)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Token validation failed: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Authentication failed: {ex.Message}"));
            }
        }
    }
}
