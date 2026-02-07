using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ENTERPRISE_HIS_WEBAPI.Services.TwoFactorAuthentication
{
    /// <summary>
    /// SMS Provider Service Interface
    /// Supports: Twilio, AWS SNS, Azure SMS
    /// Enterprise-level with delivery tracking and retry logic
    /// </summary>
    public interface ISMSService
    {
        // === SMS Sending ===
        Task<Result<SMSSendResponse>> SendSMSAsync(string phoneNumber, string message);
        Task<Result<SMSSendResponse>> SendOTPAsync(string phoneNumber, string otp);
        
        // === Batch Operations ===
        Task<Result<List<SMSSendResponse>>> SendBatchSMSAsync(List<SMSBatchRequest> requests);

        // === Provider Management ===
        Task<Result<SMSProviderDto>> GetActiveProviderAsync();
        Task<Result<List<SMSProviderDto>>> GetAllProvidersAsync();
        Task<Result<bool>> SetPrimaryProviderAsync(int providerId);

        // === Delivery Tracking ===
        Task<Result<SMSDeliveryStatus>> GetDeliveryStatusAsync(string messageId);
        Task<Result<List<SMSDeliveryStatus>>> GetDeliveryStatusBatchAsync(List<string> messageIds);

        // === Rate Limiting & Quotas ===
        Task<Result<SMSQuotaStatus>> GetQuotaStatusAsync();
        Task<Result<bool>> CheckRateLimitAsync(string phoneNumber);
        Task<Result<bool>> CheckDailyQuotaAsync();
    }

    /// <summary>
    /// SMS Provider Configuration
    /// Abstracts different providers (Twilio, AWS SNS, Azure SMS)
    /// </summary>
    public interface ISMSProvider
    {
        string ProviderName { get; }
        Task<Result<SMSSendResponse>> SendAsync(string phoneNumber, string message);
        Task<Result<SMSDeliveryStatus>> GetStatusAsync(string messageId);
    }

    // ===== DTOs =====

    public class SMSSendResponse
    {
        public bool Success { get; set; }
        public string MessageId { get; set; }
        public string PhoneNumber { get; set; }
        public string ProviderName { get; set; }
        public DateTime SentAt { get; set; }
        public decimal CostUSD { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SMSBatchRequest
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }  // For tracking
    }

    public class SMSProviderDto
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        public int MaxSMSPerDay { get; set; }
        public int MaxSMSPerUser { get; set; }
        public decimal CostPerSMS { get; set; }
    }

    public class SMSDeliveryStatus
    {
        public string MessageId { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }  // Sent, Delivered, Failed, Rejected
        public string DeliveryStatus { get; set; }  // Provider-specific
        public DateTime SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SMSQuotaStatus
    {
        public int TotalSMSToday { get; set; }
        public int MaxSMSPerDay { get; set; }
        public int RemainingQuota { get; set; }
        public decimal TotalCostToday { get; set; }
        public decimal CostPerSMS { get; set; }
        public bool IsQuotaExceeded { get; set; }
        public DateTime ResetTime { get; set; }
    }

    public class SMSConfiguration
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string SenderNumber { get; set; }
        public int MaxRetries { get; set; }
        public int TimeoutSeconds { get; set; }
        public int RateLimitPerMinute { get; set; }
    }
}
