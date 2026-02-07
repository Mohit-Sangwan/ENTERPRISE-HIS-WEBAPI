using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication
{
    /// <summary>
    /// 2FA Configuration Service Implementation
    /// Manages all 2FA settings from database
    /// Caches configuration for performance
    /// </summary>
    public class TwoFactorConfigurationService : ITwoFactorConfigurationService
    {
        private readonly ILogger<TwoFactorConfigurationService> _logger;
        private readonly IMemoryCache _cache;
        private const string CONFIG_CACHE_KEY = "2fa_config_{0}";
        private const string POLICY_CACHE_KEY = "2fa_policy_{0}";
        private const int CACHE_MINUTES = 60;

        public TwoFactorConfigurationService(
            ILogger<TwoFactorConfigurationService> logger,
            IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // ===== Global Configuration =====

        public async Task<T> GetConfigAsync<T>(string configKey, T defaultValue = default)
        {
            try
            {
                var cacheKey = string.Format(CONFIG_CACHE_KEY, configKey);
                
                if (_cache.TryGetValue(cacheKey, out T cachedValue))
                {
                    _logger.LogDebug("Config {ConfigKey} retrieved from cache", configKey);
                    return cachedValue;
                }

                // TODO: Query database
                // var config = await _dal.ExecuteQueryAsync("SELECT ConfigValue FROM config.TwoFactorConfiguration WHERE ConfigKey = @Key");
                // var value = Convert<T>(config.ConfigValue);

                var value = defaultValue;
                _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CACHE_MINUTES));
                
                _logger.LogInformation("Config {ConfigKey} = {Value}", configKey, value);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting config {ConfigKey}", configKey);
                return defaultValue;
            }
        }

        public async Task<bool> SetConfigAsync(string configKey, string configValue, string configType)
        {
            try
            {
                _logger.LogInformation("Setting config {ConfigKey} = {ConfigValue}", configKey, configValue);

                // TODO: Update database
                // var result = await _dal.ExecuteNonQueryAsync(
                //     "UPDATE config.TwoFactorConfiguration SET ConfigValue = @Value, UpdatedAt = GETUTCDATE() WHERE ConfigKey = @Key");

                // Clear cache
                var cacheKey = string.Format(CONFIG_CACHE_KEY, configKey);
                _cache.Remove(cacheKey);

                _logger.LogInformation("Config {ConfigKey} updated successfully", configKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting config {ConfigKey}", configKey);
                return false;
            }
        }

        public async Task<Dictionary<string, string>> GetAllConfigAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all configurations");

                // TODO: Query database for all configs
                // var configs = await _dal.ExecuteQueryAsync("SELECT ConfigKey, ConfigValue FROM config.TwoFactorConfiguration WHERE IsActive = 1");

                var configs = new Dictionary<string, string>
                {
                    { "Enabled", "true" },
                    { "OTPExpiryMinutes", "10" },
                    { "MaxOTPAttempts", "5" }
                };

                return configs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all configurations");
                return new Dictionary<string, string>();
            }
        }

        public async Task<bool> ResetConfigAsync()
        {
            try
            {
                _logger.LogWarning("Resetting all 2FA configuration to defaults");

                // TODO: Reset all configs in database to defaults

                // Clear all cache
                _cache.Remove("2fa_config_*");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting configuration");
                return false;
            }
        }

        // ===== 2FA Policies =====

        public async Task<TwoFactorPolicyDto> GetPolicyAsync(int policyId)
        {
            try
            {
                var cacheKey = string.Format(POLICY_CACHE_KEY, policyId);
                
                if (_cache.TryGetValue(cacheKey, out TwoFactorPolicyDto cachedPolicy))
                {
                    return cachedPolicy;
                }

                // TODO: Query database
                // var policy = await _dal.ExecuteQueryAsync("SELECT * FROM config.TwoFactorPolicy WHERE PolicyId = @Id");

                var policy = new TwoFactorPolicyDto { PolicyId = policyId };
                _cache.Set(cacheKey, policy, TimeSpan.FromMinutes(CACHE_MINUTES));
                
                _logger.LogInformation("Policy {PolicyId} retrieved", policyId);
                return policy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy {PolicyId}", policyId);
                return null;
            }
        }

        public async Task<TwoFactorPolicyDto> GetPolicyByCodeAsync(string policyCode)
        {
            try
            {
                _logger.LogInformation("Retrieving policy by code: {PolicyCode}", policyCode);

                // TODO: Query database
                // var policy = await _dal.ExecuteQueryAsync("SELECT * FROM config.TwoFactorPolicy WHERE PolicyCode = @Code");

                var policy = new TwoFactorPolicyDto { PolicyCode = policyCode };
                return policy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy by code {PolicyCode}", policyCode);
                return null;
            }
        }

        public async Task<IEnumerable<TwoFactorPolicyDto>> GetAllPoliciesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all 2FA policies");

                // TODO: Query database for all active policies
                // var policies = await _dal.ExecuteQueryAsync("SELECT * FROM config.TwoFactorPolicy WHERE IsActive = 1");

                var policies = new List<TwoFactorPolicyDto>();
                return policies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all policies");
                return new List<TwoFactorPolicyDto>();
            }
        }

        public async Task<int> CreatePolicyAsync(CreatePolicyRequest request)
        {
            try
            {
                _logger.LogInformation("Creating 2FA policy: {PolicyCode}", request.PolicyCode);

                // TODO: Insert into database
                // var result = await _dal.ExecuteScalarAsync("INSERT INTO config.TwoFactorPolicy (...) VALUES (...);");

                int policyId = 0;  // Result from database
                _logger.LogInformation("Policy {PolicyCode} created with ID {PolicyId}", request.PolicyCode, policyId);
                return policyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy {PolicyCode}", request.PolicyCode);
                return 0;
            }
        }

        public async Task<bool> UpdatePolicyAsync(int policyId, UpdatePolicyRequest request)
        {
            try
            {
                _logger.LogInformation("Updating policy {PolicyId}", policyId);

                // TODO: Update database
                // var result = await _dal.ExecuteNonQueryAsync("UPDATE config.TwoFactorPolicy SET ... WHERE PolicyId = @Id");

                // Clear cache
                var cacheKey = string.Format(POLICY_CACHE_KEY, policyId);
                _cache.Remove(cacheKey);

                _logger.LogInformation("Policy {PolicyId} updated successfully", policyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy {PolicyId}", policyId);
                return false;
            }
        }

        public async Task<bool> DeletePolicyAsync(int policyId)
        {
            try
            {
                _logger.LogWarning("Deleting policy {PolicyId}", policyId);

                // TODO: Mark as inactive in database
                // var result = await _dal.ExecuteNonQueryAsync("UPDATE config.TwoFactorPolicy SET IsActive = 0 WHERE PolicyId = @Id");

                // Clear cache
                var cacheKey = string.Format(POLICY_CACHE_KEY, policyId);
                _cache.Remove(cacheKey);

                _logger.LogWarning("Policy {PolicyId} deleted", policyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy {PolicyId}", policyId);
                return false;
            }
        }

        // ===== Role-to-Policy Mapping =====

        public async Task<TwoFactorPolicyDto> GetRolePolicyAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Getting 2FA policy for role {RoleId}", roleId);

                // TODO: Query database for role policy mapping
                // var policy = await _dal.ExecuteQueryAsync(
                //     "SELECT p.* FROM config.TwoFactorPolicy p " +
                //     "JOIN config.RoleTwoFactorPolicyMapping m ON p.PolicyId = m.PolicyId " +
                //     "WHERE m.RoleId = @RoleId AND m.IsActive = 1");

                var policy = new TwoFactorPolicyDto();
                return policy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role policy for role {RoleId}", roleId);
                return null;
            }
        }

        public async Task<bool> MapRoleToPolicyAsync(int roleId, int policyId)
        {
            try
            {
                _logger.LogInformation("Mapping role {RoleId} to policy {PolicyId}", roleId, policyId);

                // TODO: Insert mapping into database
                // var result = await _dal.ExecuteNonQueryAsync(
                //     "INSERT INTO config.RoleTwoFactorPolicyMapping (RoleId, PolicyId) VALUES (@RoleId, @PolicyId)");

                _logger.LogInformation("Role {RoleId} mapped to policy {PolicyId}", roleId, policyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping role {RoleId} to policy {PolicyId}", roleId, policyId);
                return false;
            }
        }

        public async Task<bool> RemoveRolePolicyAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Removing policy mapping for role {RoleId}", roleId);

                // TODO: Mark mapping as inactive in database
                // var result = await _dal.ExecuteNonQueryAsync(
                //     "UPDATE config.RoleTwoFactorPolicyMapping SET IsActive = 0 WHERE RoleId = @RoleId");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing policy for role {RoleId}", roleId);
                return false;
            }
        }

        // ===== User-Specific Overrides =====

        public async Task<UserTwoFactorOverrideDto> GetUserOverrideAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting 2FA override for user {UserId}", userId);

                // TODO: Query database
                // var override = await _dal.ExecuteQueryAsync("SELECT * FROM config.User2FAOverride WHERE UserId = @UserId AND IsActive = 1");

                var userOverride = new UserTwoFactorOverrideDto { UserId = userId };
                return userOverride;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user override for user {UserId}", userId);
                return null;
            }
        }

        public async Task<int> SetUserOverrideAsync(int userId, SetUserOverrideRequest request)
        {
            try
            {
                _logger.LogInformation("Setting 2FA override for user {UserId}", userId);

                // TODO: Insert into database
                // var result = await _dal.ExecuteScalarAsync("INSERT INTO config.User2FAOverride (...) VALUES (...);");

                int overrideId = 0;  // Result from database
                _logger.LogInformation("Override set for user {UserId} with ID {OverrideId}", userId, overrideId);
                return overrideId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting override for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<bool> UpdateUserOverrideAsync(int userId, UpdateUserOverrideRequest request)
        {
            try
            {
                _logger.LogInformation("Updating 2FA override for user {UserId}", userId);

                // TODO: Update database
                // var result = await _dal.ExecuteNonQueryAsync("UPDATE config.User2FAOverride SET ... WHERE UserId = @UserId");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating override for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveUserOverrideAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Removing 2FA override for user {UserId}", userId);

                // TODO: Mark as inactive in database
                // var result = await _dal.ExecuteNonQueryAsync("UPDATE config.User2FAOverride SET IsActive = 0 WHERE UserId = @UserId");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing override for user {UserId}", userId);
                return false;
            }
        }

        // ===== Effective Settings (Resolution Logic) =====

        public async Task<EffectiveTwoFactorSettings> GetEffectiveSettingsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Resolving effective 2FA settings for user {UserId}", userId);

                // 1. Start with global config
                var settings = new EffectiveTwoFactorSettings
                {
                    IsEnabled = await GetConfigAsync("Enabled", true),
                    IsRequired = await GetConfigAsync("Required", false),
                    OTPExpiryMinutes = await GetConfigAsync("OTPExpiryMinutes", 10),
                    MaxOTPAttempts = await GetConfigAsync("MaxOTPAttempts", 5),
                    AppliedFrom = "Global"
                };

                // 2. Override with role policy if exists
                var rolePolicy = await GetRolePolicyAsync(userId);  // TODO: Get actual roleId from user
                if (rolePolicy != null)
                {
                    settings.IsRequired = rolePolicy.IsRequired;
                    settings.OTPExpiryMinutes = rolePolicy.OTPExpiryMinutes;
                    settings.MaxOTPAttempts = rolePolicy.MaxOTPAttempts;
                    settings.AppliedFrom = "Policy";
                }

                // 3. Override with user-specific settings if exists
                var userOverride = await GetUserOverrideAsync(userId);
                if (userOverride != null && userOverride.IsTwoFactorRequired.HasValue)
                {
                    settings.IsRequired = userOverride.IsTwoFactorRequired.Value;
                    if (userOverride.MaxOTPAttempts.HasValue)
                        settings.MaxOTPAttempts = userOverride.MaxOTPAttempts.Value;
                    if (userOverride.OTPExpiryMinutes.HasValue)
                        settings.OTPExpiryMinutes = userOverride.OTPExpiryMinutes.Value;
                    settings.AppliedFrom = "UserOverride";
                }

                _logger.LogInformation("Effective settings resolved for user {UserId}, applied from {AppliedFrom}", 
                    userId, settings.AppliedFrom);
                
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving effective settings for user {UserId}", userId);
                return new EffectiveTwoFactorSettings();
            }
        }

        public async Task<bool> IsOTPRequiredAsync(int userId)
        {
            var settings = await GetEffectiveSettingsAsync(userId);
            return settings.IsRequired;
        }

        public async Task<int> GetOTPExpiryMinutesAsync(int userId)
        {
            var settings = await GetEffectiveSettingsAsync(userId);
            return settings.OTPExpiryMinutes;
        }

        public async Task<int> GetMaxOTPAttemptsAsync(int userId)
        {
            var settings = await GetEffectiveSettingsAsync(userId);
            return settings.MaxOTPAttempts;
        }
    }
}
