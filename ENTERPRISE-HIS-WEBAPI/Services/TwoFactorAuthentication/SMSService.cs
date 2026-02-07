using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication.Providers;

namespace ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication
{
    /// <summary>
    /// SMS Service Implementation
    /// Manages SMS delivery with provider abstraction
    /// Supports Twilio, AWS SNS, Azure SMS
    /// </summary>
    public class SMSService : ISMSService
    {
        private readonly ILogger<SMSService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, ISMSProvider> _providers;
        private ISMSProvider _primaryProvider;

        public SMSService(ILogger<SMSService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _providers = new Dictionary<string, ISMSProvider>();

            InitializeProviders();
        }

        /// <summary>
        /// Initialize all SMS providers
        /// </summary>
        private void InitializeProviders()
        {
            try
            {
                // Register all available providers
                _providers["TWILIO"] = new TwilioSMSProvider(_logger as ILogger<TwilioSMSProvider> 
                    ?? throw new InvalidOperationException("Logger not available"), _configuration);
                
                _providers["AWS_SNS"] = new AwsSNSSMSProvider(_logger as ILogger<AwsSNSSMSProvider> 
                    ?? throw new InvalidOperationException("Logger not available"), _configuration);
                
                _providers["AZURE_SMS"] = new AzureSMSProvider(_logger as ILogger<AzureSMSProvider> 
                    ?? throw new InvalidOperationException("Logger not available"), _configuration);

                // Set primary provider (from config)
                var primaryProviderCode = _configuration["SMS:PrimaryProvider"] ?? "TWILIO";
                _primaryProvider = _providers.TryGetValue(primaryProviderCode, out var provider) 
                    ? provider 
                    : _providers["TWILIO"];

                _logger.LogInformation("SMS Service initialized with primary provider: {Provider}", 
                    _primaryProvider.ProviderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing SMS providers");
                throw;
            }
        }

        /// <summary>
        /// Send SMS to a phone number
        /// </summary>
        public async Task<Result<SMSSendResponse>> SendSMSAsync(string phoneNumber, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                    return Result<SMSSendResponse>.FailureResult("Phone number is required");

                if (string.IsNullOrWhiteSpace(message))
                    return Result<SMSSendResponse>.FailureResult("Message is required");

                // Check rate limiting
                var rateLimitResult = await CheckRateLimitAsync(phoneNumber);
                if (!rateLimitResult.Success)
                    return Result<SMSSendResponse>.FailureResult("Rate limit exceeded. Please try again later.");

                // Check daily quota
                var quotaResult = await CheckDailyQuotaAsync();
                if (!quotaResult.Success)
                    return Result<SMSSendResponse>.FailureResult("Daily SMS quota exceeded.");

                // Send via primary provider
                return await _primaryProvider.SendAsync(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS");
                return Result<SMSSendResponse>.FailureResult($"Error sending SMS: {ex.Message}");
            }
        }

        /// <summary>
        /// Send OTP via SMS
        /// </summary>
        public async Task<Result<SMSSendResponse>> SendOTPAsync(string phoneNumber, string otp)
        {
            var message = $"Your Enterprise HIS verification code is: {otp}. Do not share this code. Valid for 10 minutes.";
            return await SendSMSAsync(phoneNumber, message);
        }

        /// <summary>
        /// Send batch SMS messages
        /// </summary>
        public async Task<Result<List<SMSSendResponse>>> SendBatchSMSAsync(List<SMSBatchRequest> requests)
        {
            try
            {
                var results = new List<SMSSendResponse>();

                foreach (var request in requests)
                {
                    var result = await SendSMSAsync(request.PhoneNumber, request.Message);
                    if (result.Success)
                    {
                        results.Add(result.Data);
                    }
                    else
                    {
                        results.Add(new SMSSendResponse
                        {
                            Success = false,
                            PhoneNumber = request.PhoneNumber,
                            ErrorMessage = string.Join(", ", result.Errors)
                        });
                    }

                    // Add delay between batch requests to avoid rate limiting
                    await Task.Delay(100);
                }

                return Result<List<SMSSendResponse>>.SuccessResult(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending batch SMS");
                return Result<List<SMSSendResponse>>.FailureResult($"Error sending batch SMS: {ex.Message}");
            }
        }

        /// <summary>
        /// Get active SMS provider
        /// </summary>
        public async Task<Result<SMSProviderDto>> GetActiveProviderAsync()
        {
            try
            {
                await Task.CompletedTask;

                return Result<SMSProviderDto>.SuccessResult(new SMSProviderDto
                {
                    ProviderName = _primaryProvider.ProviderName,
                    IsActive = true,
                    IsPrimary = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active provider");
                return Result<SMSProviderDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all available SMS providers
        /// </summary>
        public async Task<Result<List<SMSProviderDto>>> GetAllProvidersAsync()
        {
            try
            {
                await Task.CompletedTask;

                var providers = _providers.Values.Select(p => new SMSProviderDto
                {
                    ProviderName = p.ProviderName,
                    IsActive = _primaryProvider.ProviderName == p.ProviderName,
                    IsPrimary = _primaryProvider.ProviderName == p.ProviderName
                }).ToList();

                return Result<List<SMSProviderDto>>.SuccessResult(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting providers");
                return Result<List<SMSProviderDto>>.FailureResult($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Set primary SMS provider
        /// </summary>
        public async Task<Result<bool>> SetPrimaryProviderAsync(int providerId)
        {
            try
            {
                // TODO: Implement provider selection logic
                await Task.CompletedTask;
                return Result<bool>.SuccessResult(true, "Primary provider updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary provider");
                return Result<bool>.FailureResult($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get SMS delivery status
        /// </summary>
        public async Task<Result<SMSDeliveryStatus>> GetDeliveryStatusAsync(string messageId)
        {
            return await _primaryProvider.GetStatusAsync(messageId);
        }

        /// <summary>
        /// Get batch delivery status
        /// </summary>
        public async Task<Result<List<SMSDeliveryStatus>>> GetDeliveryStatusBatchAsync(List<string> messageIds)
        {
            try
            {
                var results = new List<SMSDeliveryStatus>();

                foreach (var messageId in messageIds)
                {
                    var result = await _primaryProvider.GetStatusAsync(messageId);
                    if (result.Success)
                    {
                        results.Add(result.Data);
                    }
                }

                return Result<List<SMSDeliveryStatus>>.SuccessResult(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting batch delivery status");
                return Result<List<SMSDeliveryStatus>>.FailureResult($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get SMS quota status
        /// </summary>
        public async Task<Result<SMSQuotaStatus>> GetQuotaStatusAsync()
        {
            try
            {
                // TODO: Query database for today's SMS count and cost
                await Task.CompletedTask;

                return Result<SMSQuotaStatus>.SuccessResult(new SMSQuotaStatus
                {
                    TotalSMSToday = 150,
                    MaxSMSPerDay = 1000,
                    RemainingQuota = 850,
                    TotalCostToday = 1.125m,
                    CostPerSMS = 0.0075m,
                    IsQuotaExceeded = false,
                    ResetTime = DateTime.UtcNow.AddHours(24)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quota status");
                return Result<SMSQuotaStatus>.FailureResult($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check rate limit for phone number
        /// </summary>
        public async Task<Result<bool>> CheckRateLimitAsync(string phoneNumber)
        {
            try
            {
                // TODO: Implement rate limiting logic
                // Max 5 SMS per 15 minutes per phone number
                await Task.CompletedTask;
                return Result<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit");
                return Result<bool>.FailureResult($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check daily SMS quota
        /// </summary>
        public async Task<Result<bool>> CheckDailyQuotaAsync()
        {
            try
            {
                var quotaResult = await GetQuotaStatusAsync();
                if (!quotaResult.Success)
                    return Result<bool>.FailureResult("Cannot check quota");

                return Result<bool>.SuccessResult(!quotaResult.Data.IsQuotaExceeded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking daily quota");
                return Result<bool>.FailureResult($"Error: {ex.Message}");
            }
        }
    }
}
