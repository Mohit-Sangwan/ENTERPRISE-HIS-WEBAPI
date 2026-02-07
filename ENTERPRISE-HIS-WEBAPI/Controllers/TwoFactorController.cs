using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication;
using ENTERPRISE_HIS_WEBAPI.Extensions;
using Microsoft.Extensions.Logging;

namespace ENTERPRISE_HIS_WEBAPI.Controllers
{
    /// <summary>
    /// Two-Factor Authentication (2FA) API Controller
    /// Supports: SMS OTP, Email OTP, TOTP (Google Authenticator), Backup Codes
    /// Enterprise-level with audit trails and compliance
    /// 
    /// Workflow:
    /// 1. User initiates 2FA setup
    /// 2. System sends OTP or generates TOTP secret
    /// 3. User verifies with OTP/TOTP code
    /// 4. 2FA is enabled on account
    /// 5. On login, 2FA verification required
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [Authorize]  // All 2FA endpoints require authentication
    public class TwoFactorController : ControllerBase
    {
        private readonly ITwoFactorService _twoFactorService;
        private readonly ISMSService _smsService;
        private readonly ILogger<TwoFactorController> _logger;

        public TwoFactorController(
            ITwoFactorService twoFactorService,
            ISMSService smsService,
            ILogger<TwoFactorController> logger)
        {
            _twoFactorService = twoFactorService;
            _smsService = smsService;
            _logger = logger;
        }

        // ===== Setup Endpoints =====

        /// <summary>
        /// Initiate 2FA setup for a specific method
        /// Supported methods: SMS_OTP, EMAIL_OTP, TOTP, BACKUP_CODES
        /// </summary>
        /// <remarks>
        /// POST /api/v1/twofactor/setup
        /// {
        ///   "method": "SMS_OTP"
        /// }
        /// </remarks>
        [HttpPost("setup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TwoFactorSetupResponse>>> InitializeSetup(
            [FromBody] SetupRequest request)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} initiating 2FA setup with method {Method}", userId, request.Method);

                if (string.IsNullOrWhiteSpace(request.Method))
                    return BadRequest(new ApiResponse<TwoFactorSetupResponse>
                    {
                        Success = false,
                        Message = "Method is required"
                    });

                var result = await _twoFactorService.SetupTwoFactorAsync(userId, request.Method);

                if (!result.Success)
                    return BadRequest(new ApiResponse<TwoFactorSetupResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Errors = result.Errors
                    });

                return Ok(new ApiResponse<TwoFactorSetupResponse>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating 2FA setup");
                return StatusCode(500, new ApiResponse<TwoFactorSetupResponse>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new System.Collections.Generic.List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Verify 2FA setup with OTP/TOTP code
        /// </summary>
        /// <remarks>
        /// POST /api/v1/twofactor/verify-setup
        /// {
        ///   "code": "123456",
        ///   "sessionToken": "guid-token"
        /// }
        /// </remarks>
        [HttpPost("verify-setup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> VerifySetup(
            [FromBody] VerifySetupRequest request)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} verifying 2FA setup", userId);

                var result = await _twoFactorService.VerifyTwoFactorSetupAsync(userId, request.Code);

                if (!result.Success)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = result.Data,
                    Message = "2FA setup verified successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA setup");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // ===== OTP Endpoints =====

        /// <summary>
        /// Send OTP via SMS or Email
        /// </summary>
        /// <remarks>
        /// POST /api/v1/twofactor/send-otp
        /// {
        ///   "method": "SMS_OTP"
        /// }
        /// </remarks>
        [HttpPost("send-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<OTPSentResponse>>> SendOTP(
            [FromBody] SendOTPRequest request)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} requesting OTP via {Method}", userId, request.Method);

                var result = await _twoFactorService.SendOTPAsync(userId, request.Method);

                if (!result.Success)
                {
                    if (result.Message.Contains("rate limit", System.StringComparison.OrdinalIgnoreCase))
                        return StatusCode(429, new ApiResponse<OTPSentResponse>
                        {
                            Success = false,
                            Message = result.Message
                        });

                    return BadRequest(new ApiResponse<OTPSentResponse>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(new ApiResponse<OTPSentResponse>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP");
                return StatusCode(500, new ApiResponse<OTPSentResponse>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Verify OTP code
        /// </summary>
        /// <remarks>
        /// POST /api/v1/twofactor/verify-otp
        /// {
        ///   "code": "123456",
        ///   "sessionToken": "guid-token"
        /// }
        /// </remarks>
        [HttpPost("verify-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TwoFactorVerificationResponse>>> VerifyOTP(
            [FromBody] VerifyOTPRequest request)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} verifying OTP", userId);

                var result = await _twoFactorService.VerifyOTPAsync(userId, request.Code, request.SessionToken);

                if (!result.Success)
                    return BadRequest(new ApiResponse<TwoFactorVerificationResponse>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<TwoFactorVerificationResponse>
                {
                    Success = true,
                    Data = result.Data,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return StatusCode(500, new ApiResponse<TwoFactorVerificationResponse>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // ===== TOTP Endpoints =====

        /// <summary>
        /// Generate TOTP secret for Google Authenticator
        /// </summary>
        /// <remarks>
        /// GET /api/v1/twofactor/totp/setup
        /// 
        /// Returns QR code and manual entry key for scanning
        /// </remarks>
        [HttpGet("totp/setup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TOTPSetupResponse>>> GenerateTOTPSecret()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} requesting TOTP setup", userId);

                var result = await _twoFactorService.GenerateTOTPSecretAsync(userId);

                if (!result.Success)
                    return BadRequest(new ApiResponse<TOTPSetupResponse>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<TOTPSetupResponse>
                {
                    Success = true,
                    Data = result.Data,
                    Message = "TOTP secret generated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating TOTP secret");
                return StatusCode(500, new ApiResponse<TOTPSetupResponse>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Verify TOTP code from authenticator app
        /// </summary>
        /// <remarks>
        /// POST /api/v1/twofactor/totp/verify
        /// {
        ///   "code": "123456"
        /// }
        /// </remarks>
        [HttpPost("totp/verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyTOTP(
            [FromBody] VerifyTOTPRequest request)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogInformation("User {UserId} verifying TOTP", userId);

                var result = await _twoFactorService.VerifyTOTPAsync(userId, request.Code);

                if (!result.Success)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = result.Data,
                    Message = "TOTP verified successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying TOTP");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // ===== Backup Codes Endpoints =====

        /// <summary>
        /// Generate backup codes for account recovery
        /// IMPORTANT: Show only once! User must save them.
        /// </summary>
        /// <remarks>
        /// POST /api/v1/twofactor/backup-codes/generate
        /// </remarks>
        [HttpPost("backup-codes/generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BackupCodesResponse>>> GenerateBackupCodes()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogWarning("User {UserId} generating backup codes", userId);

                var result = await _twoFactorService.GenerateBackupCodesAsync(userId);

                if (!result.Success)
                    return BadRequest(new ApiResponse<BackupCodesResponse>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<BackupCodesResponse>
                {
                    Success = true,
                    Data = result.Data,
                    Message = "?? SAVE THESE CODES IN A SAFE PLACE! They will only be shown once."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating backup codes");
                return StatusCode(500, new ApiResponse<BackupCodesResponse>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Verify backup code for emergency access
        /// </summary>
        /// <remarks>
        /// POST /api/v1/twofactor/backup-codes/verify
        /// {
        ///   "code": "XXXXXXXX-XXXXX-XXXXX-XXXXX"
        /// }
        /// </remarks>
        [HttpPost("backup-codes/verify")]
        [AllowAnonymous]  // Allow during login if 2FA device unavailable
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyBackupCode(
            [FromBody] VerifyBackupCodeRequest request)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogWarning("User {UserId} using backup code for access", userId);

                var result = await _twoFactorService.VerifyBackupCodeAsync(userId, request.Code);

                if (!result.Success)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = result.Data,
                    Message = "Backup code verified"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying backup code");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get status of backup codes (how many used/remaining)
        /// </summary>
        [HttpGet("backup-codes/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<System.Collections.Generic.IEnumerable<BackupCodeStatusDto>>>> GetBackupCodesStatus()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var result = await _twoFactorService.GetBackupCodesStatusAsync(userId);

                if (!result.Success)
                    return BadRequest(new ApiResponse<System.Collections.Generic.IEnumerable<BackupCodeStatusDto>>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<System.Collections.Generic.IEnumerable<BackupCodeStatusDto>>
                {
                    Success = true,
                    Data = result.Data,
                    Message = "Backup codes status retrieved"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backup codes status");
                return StatusCode(500, new ApiResponse<System.Collections.Generic.IEnumerable<BackupCodeStatusDto>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // ===== Configuration Endpoints =====

        /// <summary>
        /// Get user's current 2FA configuration
        /// </summary>
        [HttpGet("config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TwoFactorConfigDto>>> GetConfig()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var result = await _twoFactorService.GetUserTwoFactorConfigAsync(userId);

                if (!result.Success)
                    return NotFound(new ApiResponse<TwoFactorConfigDto>
                    {
                        Success = false,
                        Message = "2FA not configured"
                    });

                return Ok(new ApiResponse<TwoFactorConfigDto>
                {
                    Success = true,
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting 2FA config");
                return StatusCode(500, new ApiResponse<TwoFactorConfigDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get all 2FA methods configured for user
        /// </summary>
        [HttpGet("config/all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<System.Collections.Generic.IEnumerable<TwoFactorConfigDto>>>> GetAllConfigs()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var result = await _twoFactorService.GetAllUserTwoFactorConfigsAsync(userId);

                if (!result.Success)
                    return NotFound(new ApiResponse<System.Collections.Generic.IEnumerable<TwoFactorConfigDto>>
                    {
                        Success = false,
                        Message = "No 2FA methods configured"
                    });

                return Ok(new ApiResponse<System.Collections.Generic.IEnumerable<TwoFactorConfigDto>>
                {
                    Success = true,
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all 2FA configs");
                return StatusCode(500, new ApiResponse<System.Collections.Generic.IEnumerable<TwoFactorConfigDto>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Disable 2FA method
        /// </summary>
        [HttpDelete("config/{method}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> DisableTwoFactor(string method)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogWarning("User {UserId} disabling 2FA method {Method}", userId, method);

                var result = await _twoFactorService.DisableTwoFactorAsync(userId, method);

                if (!result.Success)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = result.Data,
                    Message = $"2FA method {method} disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Reset all 2FA settings (admin recovery)
        /// </summary>
        [HttpDelete("reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> ResetTwoFactor()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                _logger.LogWarning("User {UserId} resetting all 2FA settings", userId);

                var result = await _twoFactorService.ResetTwoFactorAsync(userId);

                if (!result.Success)
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = result.Data,
                    Message = "All 2FA settings reset"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting 2FA");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }

    // ===== Request DTOs =====

    public class SetupRequest
    {
        public string Method { get; set; }  // SMS_OTP, EMAIL_OTP, TOTP, BACKUP_CODES
        public string PhoneNumber { get; set; }  // For SMS_OTP
        public string Email { get; set; }  // For EMAIL_OTP
    }

    public class VerifySetupRequest
    {
        public string Code { get; set; }
        public string SessionToken { get; set; }
    }

    public class SendOTPRequest
    {
        public string Method { get; set; }  // SMS_OTP or EMAIL_OTP
    }

    public class VerifyOTPRequest
    {
        public string Code { get; set; }
        public string SessionToken { get; set; }
    }

    public class VerifyTOTPRequest
    {
        public string Code { get; set; }
    }

    public class VerifyBackupCodeRequest
    {
        public string Code { get; set; }
    }

    // ===== Response DTO =====

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public System.Collections.Generic.List<string> Errors { get; set; } = new();
    }
}
