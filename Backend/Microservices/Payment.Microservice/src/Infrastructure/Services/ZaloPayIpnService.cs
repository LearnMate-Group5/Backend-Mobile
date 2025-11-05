using System;
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
using Microsoft.Extensions.Options;
using SharedLibrary.Contracts;

namespace Infrastructure.Services
{
    public class ZaloPayIpnService : Application.Payments.Services.IZaloPayIpnService
    {
        private readonly ZaloPayConfig _config;
        private readonly ILogger<ZaloPayIpnService> _logger;
        private readonly MyDbContext _dbContext;
        private readonly IBus _bus;

        public ZaloPayIpnService(
            IOptions<ZaloPayConfig> config,
            ILogger<ZaloPayIpnService> logger,
            MyDbContext dbContext,
            IBus bus)
        {
            _config = config.Value;
            _logger = logger;
            _dbContext = dbContext;
            _bus = bus;
        }

        public async Task<ZaloPayIpnResponse> ProcessIpnAsync(ZaloPayIpnRequest ipnRequest)
        {
            try
            {
                _logger.LogInformation("Processing ZaloPay IPN. Type: {Type}", ipnRequest.Type);

                // Verify MAC signature first
                if (!VerifyMac(ipnRequest.Data, ipnRequest.Mac))
                {
                    _logger.LogWarning("ZaloPay IPN MAC verification failed");
                    return new ZaloPayIpnResponse
                    {
                        ReturnCode = -1,
                        ReturnMessage = "Invalid MAC signature"
                    };
                }

                // Parse data
                var ipnData = JsonSerializer.Deserialize<ZaloPayIpnData>(ipnRequest.Data);
                if (ipnData == null)
                {
                    _logger.LogError("Failed to parse ZaloPay IPN data");
                    return new ZaloPayIpnResponse
                    {
                        ReturnCode = 0,
                        ReturnMessage = "Failed to parse data"
                    };
                }

                _logger.LogInformation("ZaloPay IPN for AppTransId: {AppTransId}, ZpTransId: {ZpTransId}, Amount: {Amount}",
                    ipnData.AppTransId, ipnData.ZpTransId, ipnData.Amount);

                // Extract userId from embed_data if available
                string? userId = null;
                if (!string.IsNullOrWhiteSpace(ipnData.EmbedData))
                {
                    try
                    {
                        var embedDataObj = JsonSerializer.Deserialize<JsonDocument>(ipnData.EmbedData);
                        if (embedDataObj?.RootElement.TryGetProperty("userId", out var userIdElement) == true)
                        {
                            userId = userIdElement.GetString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse embed_data for AppTransId: {AppTransId}", ipnData.AppTransId);
                    }
                }

                // Extract orderId from embed_data if available
                string? orderId = null;
                if (!string.IsNullOrWhiteSpace(ipnData.EmbedData))
                {
                    try
                    {
                        var embedDataObj = JsonSerializer.Deserialize<JsonDocument>(ipnData.EmbedData);
                        if (embedDataObj?.RootElement.TryGetProperty("orderId", out var orderIdElement) == true)
                        {
                            orderId = orderIdElement.GetString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to extract orderId from embed_data");
                    }
                }

                // Use orderId if available, otherwise use AppTransId
                var transactionOrderId = orderId ?? ipnData.AppTransId;

                // Check if transaction already exists (by AppTransId or OrderId)
                var existingTransaction = await _dbContext.PaymentTransactions
                    .FirstOrDefaultAsync(t => t.AppTransId == ipnData.AppTransId || t.OrderId == transactionOrderId);

                if (existingTransaction != null)
                {
                    // Update existing transaction
                    existingTransaction.Status = "Success";
                    existingTransaction.ResultCode = 1; // ZaloPay success code
                    existingTransaction.Message = "Payment successful";
                    existingTransaction.ZpTransToken = ipnData.ZpTransId.ToString();
                    existingTransaction.UpdatedAt = DateTime.UtcNow;
                    existingTransaction.CallbackData = ipnRequest.Data;

                    _logger.LogInformation("Updated existing transaction for AppTransId: {AppTransId}, OrderId: {OrderId}, Status: Success",
                        ipnData.AppTransId, transactionOrderId);
                }
                else
                {
                    // Create new transaction record
                    var transaction = new PaymentTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId ?? ipnData.AppUser,
                        OrderId = transactionOrderId,
                        RequestId = ipnData.AppTransId,
                        AppTransId = ipnData.AppTransId,
                        Amount = ipnData.Amount,
                        PaymentGateway = "ZaloPay",
                        Status = "Success",
                        ResultCode = 1,
                        Message = "Payment successful",
                        OrderInfo = $"ZaloPay payment - Channel {ipnData.Channel}",
                        ZpTransToken = ipnData.ZpTransId.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CallbackData = ipnRequest.Data
                    };

                    _dbContext.PaymentTransactions.Add(transaction);

                    _logger.LogInformation("Created new transaction for AppTransId: {AppTransId}, OrderId: {OrderId}, Status: Success",
                        ipnData.AppTransId, transactionOrderId);
                }

                await _dbContext.SaveChangesAsync();

                // Publish payment success event to RabbitMQ
                var paymentEvent = new PaymentSuccessEvent
                {
                    OrderId = transactionOrderId,
                    UserId = userId ?? ipnData.AppUser,
                    PaymentMethod = "ZaloPay",
                    Amount = ipnData.Amount,
                    TransactionId = ipnData.ZpTransId.ToString(),
                    CompletedAt = DateTime.UtcNow
                };

                await _bus.Publish(paymentEvent);

                _logger.LogInformation("Published payment success event for OrderId: {OrderId}, UserId: {UserId}",
                    transactionOrderId, userId ?? ipnData.AppUser);

                // Return success response to ZaloPay
                return new ZaloPayIpnResponse
                {
                    ReturnCode = 1,
                    ReturnMessage = "Success"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ZaloPay IPN");
                return new ZaloPayIpnResponse
                {
                    ReturnCode = 0,
                    ReturnMessage = "System error"
                };
            }
        }

        public bool VerifyMac(string data, string mac)
        {
            try
            {
                // ZaloPay MAC verification: mac = HMAC_SHA256(key2, data)
                var computedMac = ComputeHmacSha256(data, _config.Key2);

                _logger.LogInformation("ZaloPay IPN MAC verification - Computed: {Computed}, Received: {Received}",
                    computedMac, mac);

                return computedMac.Equals(mac, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying ZaloPay IPN MAC");
                return false;
            }
        }

        private string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
