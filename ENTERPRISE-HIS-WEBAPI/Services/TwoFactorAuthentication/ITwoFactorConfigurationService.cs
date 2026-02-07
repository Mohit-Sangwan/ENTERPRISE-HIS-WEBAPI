using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication
{
    /// <summary>
    /// 2FA Configuration Service Interface
    /// Manages all 2FA settings from database (enterprise-level)
    /// No hardcoded configuration - all settings are database-driven
    /// </summary>
    public interface ITwoFactorConfigurationService
    {
        // ===== Global Configuration =====
        Task<T> GetConfigAsync<T>(string configKey, T defaultValue = default);
        Task<bool> SetConfigAsync(string configKey, string configValue, string configType);
        Task<Dictionary<string, string>> GetAllConfigAsync();
        Task<bool> ResetConfigAsync();

        // ===== 2FA Policies =====
        Task<TwoFactorPolicyDto> GetPolicyAsync(int policyId);
        Task<TwoFactorPolicyDto> GetPolicyByCodeAsync(string policyCode);
        Task<IEnumerable<TwoFactorPolicyDto>> GetAllPoliciesAsync();
        Task<int> CreatePolicyAsync(CreatePolicyRequest request);
        Task<bool> UpdatePolicyAsync(int policyId, UpdatePolicyRequest request);
        Task<bool> DeletePolicyAsync(int policyId);

        // ===== Role-to-Policy Mapping =====
        Task<TwoFactorPolicyDto> GetRolePolicyAsync(int roleId);
        Task<bool> MapRoleToPolicyAsync(int roleId, int policyId);
        Task<bool> RemoveRolePolicyAsync(int roleId);

        // ===== User-Specific Overrides =====
        Task<UserTwoFactorOverrideDto> GetUserOverrideAsync(int userId);
        Task<int> SetUserOverrideAsync(int userId, SetUserOverrideRequest request);
        Task<bool> UpdateUserOverrideAsync(int userId, UpdateUserOverrideRequest request);
        Task<bool> RemoveUserOverrideAsync(int userId);

        // ===== Effective Settings (Resolved from Policy/Override) =====
        Task<EffectiveTwoFactorSettings> GetEffectiveSettingsAsync(int userId);
        Task<bool> IsOTPRequiredAsync(int userId);
        Task<int> GetOTPExpiryMinutesAsync(int userId);
        Task<int> GetMaxOTPAttemptsAsync(int userId);
    }

    // ===== DTOs =====

    public class TwoFactorPolicyDto
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; }
        public string PolicyCode { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }

        // Policy settings
        public int OTPExpiryMinutes { get; set; }
        public int OTPLength { get; set; }
        public int MaxOTPAttempts { get; set; }
        public int SessionExpiryMinutes { get; set; }
        public int BackupCodeCount { get; set; }
        public int TrustedDeviceExpiryDays { get; set; }

        // Rate limiting
        public bool RateLimitEnabled { get; set; }
        public int MaxOTPPerHour { get; set; }
        public int MaxOTPPerDay { get; set; }
    }

    public class CreatePolicyRequest
    {
        public string PolicyName { get; set; }
        public string PolicyCode { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public int OTPExpiryMinutes { get; set; }
        public int MaxOTPAttempts { get; set; }
        public int SessionExpiryMinutes { get; set; }
        public int BackupCodeCount { get; set; }
        public int TrustedDeviceExpiryDays { get; set; }
        public int MaxOTPPerHour { get; set; }
        public int MaxOTPPerDay { get; set; }
    }

    public class UpdatePolicyRequest
    {
        public string PolicyName { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public int OTPExpiryMinutes { get; set; }
        public int MaxOTPAttempts { get; set; }
        public int SessionExpiryMinutes { get; set; }
        public int BackupCodeCount { get; set; }
        public int TrustedDeviceExpiryDays { get; set; }
        public int MaxOTPPerHour { get; set; }
        public int MaxOTPPerDay { get; set; }
    }

    public class UserTwoFactorOverrideDto
    {
        public int OverrideId { get; set; }
        public int UserId { get; set; }
        public bool? IsTwoFactorRequired { get; set; }
        public string PreferredMethod { get; set; }
        public string AllowedMethods { get; set; }
        public int? MaxOTPAttempts { get; set; }
        public int? OTPExpiryMinutes { get; set; }
        public bool AllowBackupCodes { get; set; }
        public bool AllowDeviceTrust { get; set; }
    }

    public class SetUserOverrideRequest
    {
        public bool? IsTwoFactorRequired { get; set; }
        public string PreferredMethod { get; set; }
        public string AllowedMethods { get; set; }
        public int? MaxOTPAttempts { get; set; }
        public int? OTPExpiryMinutes { get; set; }
        public bool AllowBackupCodes { get; set; }
        public bool AllowDeviceTrust { get; set; }
    }

    public class UpdateUserOverrideRequest
    {
        public bool? IsTwoFactorRequired { get; set; }
        public string PreferredMethod { get; set; }
        public string AllowedMethods { get; set; }
        public int? MaxOTPAttempts { get; set; }
        public int? OTPExpiryMinutes { get; set; }
        public bool AllowBackupCodes { get; set; }
        public bool AllowDeviceTrust { get; set; }
    }

    public class EffectiveTwoFactorSettings
    {
        public bool IsEnabled { get; set; }
        public bool IsRequired { get; set; }
        public int OTPExpiryMinutes { get; set; }
        public int OTPLength { get; set; }
        public int MaxOTPAttempts { get; set; }
        public int SessionExpiryMinutes { get; set; }
        public int BackupCodeCount { get; set; }
        public int TrustedDeviceExpiryDays { get; set; }
        public bool RateLimitEnabled { get; set; }
        public int MaxOTPPerHour { get; set; }
        public int MaxOTPPerDay { get; set; }
        public bool EmailEnabled { get; set; }
        public bool TOTPEnabled { get; set; }
        public bool BackupCodesEnabled { get; set; }
        public bool DeviceTrustEnabled { get; set; }
        public string AppliedFrom { get; set; }  // "Global", "Policy", "UserOverride"
    }
}
