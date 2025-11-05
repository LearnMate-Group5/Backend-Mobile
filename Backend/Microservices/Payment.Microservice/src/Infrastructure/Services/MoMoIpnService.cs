using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Payments.DTOs;
using Domain.Configs;
using Domain.Entities;
using Infrastructure.Context;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Contracts;

namespace Infrastructure.Services
{
    public class MoMoIpnService : Application.Payments.Services.IMoMoIpnService
    {
        private readonly MoMoConfig _config;
        private readonly ILogger<MoMoIpnService> _logger;
        private readonly MyDbContext _dbContext;
        private readonly IBus _bus;

        public MoMoIpnService(
            ILogger<MoMoIpnService> logger,
            MyDbContext dbContext,
            IBus bus)
        {
            _logger = logger;
            _dbContext = dbContext;
            _bus = bus;

            // Read configuration from environment variables
            _config = new MoMoConfig
            {
                PartnerCode = Environment.GetEnvironmentVariable("MoMo__PartnerCode") ?? string.Empty,
                AccessKey = Environment.GetEnvironmentVariable("MoMo__AccessKey") ?? string.Empty,
                SecretKey = Environment.GetEnvironmentVariable("MoMo__SecretKey") ?? string.Empty,
                BaseUrl = Environment.GetEnvironmentVariable("MoMo__BaseUrl") ?? "https://test-payment.momo.vn"
            };
        }

        public async Task<MoMoIpnResponse> ProcessIpnAsync(MoMoIpnRequest ipnRequest)
        {
            var responseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            try
            {
                _logger.LogInformation("Processing MoMo IPN - OrderId: {OrderId}, TransId: {TransId}, ResultCode: {ResultCode}, Amount: {Amount}, RequestId: {RequestId}",
                    ipnRequest.OrderId, ipnRequest.TransId, ipnRequest.ResultCode, ipnRequest.Amount, ipnRequest.RequestId);

                // Verify signature first
                if (!VerifySignature(ipnRequest))
                {
                    _logger.LogWarning("MoMo IPN signature verification failed for OrderId: {OrderId}. Signature mismatch!", ipnRequest.OrderId);
                    return CreateErrorResponse(ipnRequest, 97, "Invalid signature", responseTime);
                }

                _logger.LogInformation("MoMo IPN signature verified successfully for OrderId: {OrderId}", ipnRequest.OrderId);

                // Extract userId from extraData
                string? userId = null;
                if (!string.IsNullOrWhiteSpace(ipnRequest.ExtraData))
                {
                    try
                    {
                        var extraDataJson = Encoding.UTF8.GetString(Convert.FromBase64String(ipnRequest.ExtraData));
                        _logger.LogInformation("MoMo IPN ExtraData decoded: {ExtraData}", extraDataJson);
                        var extraDataObj = JsonSerializer.Deserialize<JsonDocument>(extraDataJson);
                        userId = extraDataObj?.RootElement.GetProperty("userId").GetString();
                        _logger.LogInformation("MoMo IPN Extracted UserId: {UserId}", userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse extraData for OrderId: {OrderId}. ExtraData: {ExtraData}",
                            ipnRequest.OrderId, ipnRequest.ExtraData);
                    }
                }

                // Check if transaction already exists - try both OrderId and RequestId
                var existingTransaction = await _dbContext.PaymentTransactions
                    .FirstOrDefaultAsync(t => t.OrderId == ipnRequest.OrderId || t.RequestId == ipnRequest.RequestId);

                if (existingTransaction != null)
                {
                    _logger.LogInformation("Found existing transaction - Id: {Id}, OrderId: {OrderId}, RequestId: {RequestId}, CurrentStatus: {CurrentStatus}",
                        existingTransaction.Id, existingTransaction.OrderId, existingTransaction.RequestId, existingTransaction.Status);

                    // Update existing transaction
                    var previousStatus = existingTransaction.Status;
                    existingTransaction.MomoTransId = ipnRequest.TransId.ToString();
                    existingTransaction.Status = GetPaymentStatus(ipnRequest.ResultCode);
                    existingTransaction.ResultCode = ipnRequest.ResultCode;
                    existingTransaction.Message = ipnRequest.Message;
                    existingTransaction.PayType = ipnRequest.PayType;
                    existingTransaction.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation("Updating transaction for OrderId: {OrderId} - Status: {PreviousStatus} -> {NewStatus}, ResultCode: {ResultCode}",
                        ipnRequest.OrderId, previousStatus, existingTransaction.Status, ipnRequest.ResultCode);

                    var saveResult = await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("MoMo IPN - SaveChanges result: {SaveResult} records updated for OrderId: {OrderId}",
                        saveResult, ipnRequest.OrderId);
                }
                else
                {
                    _logger.LogWarning("No existing transaction found for OrderId: {OrderId} or RequestId: {RequestId}. Creating new record.",
                        ipnRequest.OrderId, ipnRequest.RequestId);

                    // Create new transaction record
                    var transaction = new PaymentTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        OrderId = ipnRequest.OrderId,
                        RequestId = ipnRequest.RequestId,
                        Amount = ipnRequest.Amount,
                        PaymentGateway = "MoMo",
                        MomoTransId = ipnRequest.TransId.ToString(),
                        Status = GetPaymentStatus(ipnRequest.ResultCode),
                        ResultCode = ipnRequest.ResultCode,
                        Message = ipnRequest.Message,
                        OrderInfo = ipnRequest.OrderInfo,
                        PayType = ipnRequest.PayType,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _dbContext.PaymentTransactions.Add(transaction);

                    var saveResult = await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created new transaction for OrderId: {OrderId}, Status: {Status}, SaveResult: {SaveResult}",
                        ipnRequest.OrderId, transaction.Status, saveResult);
                }

                // Publish payment success event to RabbitMQ if payment was successful
                if (ipnRequest.ResultCode == 0) // 0 = success in MoMo
                {
                    var paymentEvent = new PaymentSuccessEvent
                    {
                        OrderId = ipnRequest.OrderId,
                        UserId = userId ?? "Unknown",
                        PaymentMethod = "MoMo",
                        Amount = ipnRequest.Amount,
                        TransactionId = ipnRequest.TransId.ToString(),
                        CompletedAt = DateTime.UtcNow
                    };

                    await _bus.Publish(paymentEvent);

                    _logger.LogInformation("Published payment success event for OrderId: {OrderId}, UserId: {UserId}",
                        ipnRequest.OrderId, userId);
                }

                // Return success response to MoMo
                _logger.LogInformation("MoMo IPN processing completed successfully for OrderId: {OrderId}", ipnRequest.OrderId);
                return CreateSuccessResponse(ipnRequest, responseTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo IPN for OrderId: {OrderId}. Exception: {Message}, StackTrace: {StackTrace}",
                    ipnRequest.OrderId, ex.Message, ex.StackTrace);
                return CreateErrorResponse(ipnRequest, 99, "System error", responseTime);
            }
        }

        public bool VerifySignature(MoMoIpnRequest ipnRequest)
        {
            try
            {
                _logger.LogInformation("MoMo IPN Signature Verification - Input: OrderId={OrderId}, Amount={Amount}, ResultCode={ResultCode}, TransId={TransId}",
                    ipnRequest.OrderId, ipnRequest.Amount, ipnRequest.ResultCode, ipnRequest.TransId);

                // Build signature string according to MoMo documentation
                // Format: accessKey=$accessKey&amount=$amount&extraData=$extraData&message=$message
                //         &orderId=$orderId&orderInfo=$orderInfo&orderType=$orderType
                //         &partnerCode=$partnerCode&payType=$payType&requestId=$requestId
                //         &responseTime=$responseTime&resultCode=$resultCode&transId=$transId

                var rawSignature = $"accessKey={_config.AccessKey}" +
                                 $"&amount={ipnRequest.Amount}" +
                                 $"&extraData={ipnRequest.ExtraData}" +
                                 $"&message={ipnRequest.Message}" +
                                 $"&orderId={ipnRequest.OrderId}" +
                                 $"&orderInfo={ipnRequest.OrderInfo}" +
                                 $"&orderType={ipnRequest.OrderType}" +
                                 $"&partnerCode={ipnRequest.PartnerCode}" +
                                 $"&payType={ipnRequest.PayType}" +
                                 $"&requestId={ipnRequest.RequestId}" +
                                 $"&responseTime={ipnRequest.ResponseTime}" +
                                 $"&resultCode={ipnRequest.ResultCode}" +
                                 $"&transId={ipnRequest.TransId}";

                _logger.LogInformation("MoMo IPN Raw signature string: {RawSignature}", rawSignature);

                var computedSignature = ComputeHmacSha256(rawSignature, _config.SecretKey);

                _logger.LogInformation("MoMo IPN Signature - Computed: {Computed}, Received: {Received}, Match: {Match}",
                    computedSignature, ipnRequest.Signature, computedSignature.Equals(ipnRequest.Signature, StringComparison.OrdinalIgnoreCase));

                return computedSignature.Equals(ipnRequest.Signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MoMo IPN signature");
                return false;
            }
        }

        private string GetPaymentStatus(int resultCode)
        {
            return resultCode switch
            {
                0 => "Success", // Payment successful
                9000 => "Pending", // Transaction is being processed
                1005 => "Expired", // Transaction expired or did not exist
                1006 => "Expired", // Transaction expired
                8000 => "Failed", // User denied payment
                1001 => "Failed", // Transaction failed (generic)
                1002 => "Failed", // Transaction failed (invalid parameters)
                1003 => "Failed", // Transaction failed (invalid amount)
                1004 => "Failed", // Transaction not found
                1007 => "Failed", // Transaction rejected by MoMo
                1026 => "Failed", // Transaction limited
                1080 => "Failed", // Transaction not found or processed
                1081 => "Failed", // Transaction has been cancelled
                3001 => "Failed", // Payment failed by bank
                3002 => "Failed", // Cancelled by user
                3003 => "Failed", // Blocked transaction
                3004 => "Failed", // Transaction amount exceeds the maximum limit
                3005 => "Failed", // Transaction URL invalid/expired
                3006 => "Failed", // Transaction failed
                3007 => "Failed", // Transaction rejected
                _ => "Failed" // Default to failed for unknown codes
            };
        }

        private MoMoIpnResponse CreateSuccessResponse(MoMoIpnRequest ipnRequest, long responseTime)
        {
            var response = new MoMoIpnResponse
            {
                PartnerCode = ipnRequest.PartnerCode,
                OrderId = ipnRequest.OrderId,
                RequestId = ipnRequest.RequestId,
                ResultCode = 0,
                Message = "Success",
                ResponseTime = responseTime,
                ExtraData = ""
            };

            // Sign the response
            var rawSignature = $"accessKey={_config.AccessKey}" +
                             $"&extraData={response.ExtraData}" +
                             $"&message={response.Message}" +
                             $"&orderId={response.OrderId}" +
                             $"&partnerCode={response.PartnerCode}" +
                             $"&requestId={response.RequestId}" +
                             $"&responseTime={response.ResponseTime}" +
                             $"&resultCode={response.ResultCode}";

            response.Signature = ComputeHmacSha256(rawSignature, _config.SecretKey);

            return response;
        }

        private MoMoIpnResponse CreateErrorResponse(MoMoIpnRequest ipnRequest, int resultCode, string message, long responseTime)
        {
            var response = new MoMoIpnResponse
            {
                PartnerCode = ipnRequest.PartnerCode,
                OrderId = ipnRequest.OrderId,
                RequestId = ipnRequest.RequestId,
                ResultCode = resultCode,
                Message = message,
                ResponseTime = responseTime,
                ExtraData = ""
            };

            // Sign the response
            var rawSignature = $"accessKey={_config.AccessKey}" +
                             $"&extraData={response.ExtraData}" +
                             $"&message={response.Message}" +
                             $"&orderId={response.OrderId}" +
                             $"&partnerCode={response.PartnerCode}" +
                             $"&requestId={response.RequestId}" +
                             $"&responseTime={response.ResponseTime}" +
                             $"&resultCode={response.ResultCode}";

            response.Signature = ComputeHmacSha256(rawSignature, _config.SecretKey);

            return response;
        }

        private string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
