using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication
{
    /// <summary>
    /// Two-Factor Authentication Service Interface
    /// Supports: SMS OTP, Email OTP, TOTP, Backup Codes
    /// Enterprise-level with audit trails and compliance ready
    /// </summary>
    public interface ITwoFactorService
    {
        // === Setup & Configuration ===
        Task<Result<TwoFactorSetupResponse>> SetupTwoFactorAsync(int userId, string method);
        Task<Result<bool>> VerifyTwoFactorSetupAsync(int userId, string code);
        Task<Result<TwoFactorConfigDto>> GetUserTwoFactorConfigAsync(int userId);
        Task<Result<IEnumerable<TwoFactorConfigDto>>> GetAllUserTwoFactorConfigsAsync(int userId);

        // === OTP Generation & Verification ===
        Task<Result<OTPSentResponse>> SendOTPAsync(int userId, string method);
        Task<Result<TwoFactorVerificationResponse>> VerifyOTPAsync(int userId, string code, string sessionToken);

        // === TOTP (Google Authenticator) ===
        Task<Result<TOTPSetupResponse>> GenerateTOTPSecretAsync(int userId);
        Task<Result<bool>> VerifyTOTPAsync(int userId, string code);

        // === Backup Codes ===
        Task<Result<BackupCodesResponse>> GenerateBackupCodesAsync(int userId);
        Task<Result<bool>> VerifyBackupCodeAsync(int userId, string code);
        Task<Result<IEnumerable<BackupCodeStatusDto>>> GetBackupCodesStatusAsync(int userId);

        // === Device Management ===
        Task<Result<bool>> TrustDeviceAsync(int userId, TrustedDeviceRequest request);
        Task<Result<IEnumerable<TrustedDeviceDto>>> GetTrustedDevicesAsync(int userId);
        Task<Result<bool>> RemoveTrustedDeviceAsync(int userId, long deviceId);
        Task<Result<bool>> IsTrustedDeviceAsync(int userId, string deviceFingerprint);

        // === Session Management ===
        Task<Result<string>> CreateTwoFactorSessionAsync(int userId, string ipAddress, string userAgent);
        Task<Result<TwoFactorSessionDto>> GetSessionAsync(string sessionToken);
        Task<Result<bool>> MarkSessionVerifiedAsync(string sessionToken);
        Task<Result<bool>> InvalidateSessionAsync(string sessionToken);

        // === Management ===
        Task<Result<bool>> DisableTwoFactorAsync(int userId, string method);
        Task<Result<bool>> ResetTwoFactorAsync(int userId);
    }

    // ===== DTOs =====

    public class TwoFactorSetupResponse
    {
        public string SessionToken { get; set; }
        public string Method { get; set; }
        public string QRCode { get; set; }  // For TOTP
        public string Secret { get; set; }  // For TOTP (if applicable)
        public DateTime ExpiresAt { get; set; }
    }

    public class OTPSentResponse
    {
        public string SessionToken { get; set; }
        public string MaskedTarget { get; set; }  // e.g., "+1****5678" or "user****@example.com"
        public int ExpiresInSeconds { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class TwoFactorVerificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }  // JWT token after successful 2FA
        public string RefreshToken { get; set; }
        public int AttemptRemaining { get; set; }
    }

    public class TOTPSetupResponse
    {
        public string Secret { get; set; }
        public string QRCodeUrl { get; set; }
        public string ManualEntryKey { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class BackupCodesResponse
    {
        public List<string> Codes { get; set; }  // Only shown once!
        public string DownloadLink { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class TrustedDeviceRequest
    {
        public string DeviceName { get; set; }
        public string DeviceFingerprint { get; set; }
        public string DeviceType { get; set; }  // Mobile, Desktop, Tablet
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string OSName { get; set; }
        public string OSVersion { get; set; }
    }

    public class TrustedDeviceDto
    {
        public long DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public bool IsTrusted { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class TwoFactorConfigDto
    {
        public int ConfigId { get; set; }
        public string Method { get; set; }
        public string MaskedContact { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TwoFactorSessionDto
    {
        public long SessionId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public int VerificationAttempts { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class BackupCodeStatusDto
    {
        public long BackupCodeId { get; set; }
        public string CodePrefix { get; set; }
        public bool IsUsed { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }

    // Helper result class
    public class Result<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();

        public static Result<T> SuccessResult(T data, string message = "Success")
        {
            return new Result<T> { Success = true, Data = data, Message = message };
        }

        public static Result<T> FailureResult(string message, List<string> errors = null)
        {
            return new Result<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string> { message }
            };
        }
    }
}
