using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Payments.DTOs;
using Application.Payments.Services;
using Domain.Configs;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class MoMoService : IMoMoService
    {
        private readonly HttpClient _httpClient;
        private readonly MoMoConfig _config;
        private readonly ILogger<MoMoService> _logger;
        private readonly MyDbContext _dbContext;
        private readonly ISubscriptionService _subscriptionService;

        public MoMoService(
            HttpClient httpClient,
            ILogger<MoMoService> logger,
            MyDbContext dbContext,
            ISubscriptionService subscriptionService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dbContext = dbContext;
            _subscriptionService = subscriptionService;

            // Read configuration from environment variables
            _config = new MoMoConfig
            {
                PartnerCode = Environment.GetEnvironmentVariable("MoMo__PartnerCode") ?? string.Empty,
                AccessKey = Environment.GetEnvironmentVariable("MoMo__AccessKey") ?? string.Empty,
                SecretKey = Environment.GetEnvironmentVariable("MoMo__SecretKey") ?? string.Empty,
                BaseUrl = Environment.GetEnvironmentVariable("MoMo__BaseUrl") ?? "https://test-payment.momo.vn",
                IpnUrl = Environment.GetEnvironmentVariable("MoMo__IpnUrl") ?? string.Empty,
                RedirectUrl = Environment.GetEnvironmentVariable("MoMo__RedirectUrl") ?? string.Empty
            };

            _logger.LogInformation("MoMo Configuration - PartnerCode: {PartnerCode}, BaseUrl: {BaseUrl}, IpnUrl: {IpnUrl}",
                _config.PartnerCode, _config.BaseUrl, _config.IpnUrl);
        }

        public async Task<CreateMoMoOrderResponse> CreateOrderAsync(string userId, CreateMoMoOrderRequest request)
        {
            try
            {
                // Parse orderId as UserSubscriptionId (Guid)
                if (string.IsNullOrWhiteSpace(request.OrderId) || !Guid.TryParse(request.OrderId, out var userSubscriptionId))
                {
                    _logger.LogError("Invalid or missing OrderId (UserSubscriptionId): {OrderId}", request.OrderId);
                    return new CreateMoMoOrderResponse
                    {
                        Success = false,
                        Message = "OrderId must be a valid UserSubscriptionId (Guid)"
                    };
                }

                // Get subscription price via RabbitMQ
                _logger.LogInformation("Fetching subscription price for UserSubscriptionId: {UserSubscriptionId}", userSubscriptionId);
                var priceResponse = await _subscriptionService.GetSubscriptionPriceAsync(userSubscriptionId);

                if (!priceResponse.Success)
                {
                    _logger.LogError("Failed to get subscription price: {Message}", priceResponse.Message);
                    return new CreateMoMoOrderResponse
                    {
                        Success = false,
                        Message = $"Failed to get subscription price: {priceResponse.Message}"
                    };
                }

                // Convert decimal to long (VND, no decimal places)
                var amount = (long)priceResponse.FinalPrice;

                _logger.LogInformation(
                    "Retrieved subscription price: UserSubscriptionId={UserSubscriptionId}, " +
                    "SubscriptionName={SubscriptionName}, FinalPrice={FinalPrice}",
                    userSubscriptionId,
                    priceResponse.SubscriptionName,
                    amount);

                // Generate unique IDs
                var requestId = Guid.NewGuid().ToString();
                var orderId = request.OrderId; // Use the UserSubscriptionId as orderId

                // Check for existing PENDING transaction that hasn't expired yet
                var existingTransaction = await _dbContext.PaymentTransactions
                    .Where(t => t.UserId == userId &&
                               t.Amount == amount &&
                               t.PaymentGateway == "MoMo" &&
                               t.Status == "Pending" &&
                               t.ExpiresAt.HasValue &&
                               t.ExpiresAt.Value > DateTime.UtcNow)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if (existingTransaction != null)
                {
                    _logger.LogInformation("Found existing non-expired MoMo transaction for UserId: {UserId}, OrderId: {OrderId}, ExpiresAt: {ExpiresAt}. Returning existing order.",
                        userId, existingTransaction.OrderId, existingTransaction.ExpiresAt);

                    // Return existing transaction details
                    return new CreateMoMoOrderResponse
                    {
                        Success = true,
                        Message = existingTransaction.Message ?? "Using existing order",
                        PayUrl = existingTransaction.PayUrl,
                        Deeplink = existingTransaction.Deeplink,
                        QrCodeUrl = existingTransaction.QrCodeUrl,
                        OrderId = existingTransaction.OrderId,
                        RequestId = existingTransaction.RequestId,
                        ResultCode = existingTransaction.ResultCode
                    };
                }

                _logger.LogInformation("Creating new MoMo order: RequestId={RequestId}, OrderId={OrderId}, Amount={Amount}, IpnUrl={IpnUrl}",
                    requestId, orderId, amount, _config.IpnUrl);

                // Prepare extraData (must be base64 encoded JSON)
                var extraDataObj = new { userId = userId };
                var extraDataJson = JsonSerializer.Serialize(extraDataObj);
                var extraDataBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(extraDataJson));

                var normalizedLang = request.Lang == "vn" ? "vi" : request.Lang;

                // Use RedirectUrl from request if provided, otherwise from config
                var redirectUrl = !string.IsNullOrWhiteSpace(request.RedirectUrl)
                    ? request.RedirectUrl
                    : _config.RedirectUrl;

                // Prepare orderInfo with subscription name
                var orderInfo = !string.IsNullOrWhiteSpace(request.OrderInfo)
                    ? request.OrderInfo
                    : $"Payment for {priceResponse.SubscriptionName}";

                // Build signature string (alphabetically sorted keys)
                var rawSignature = $"accessKey={_config.AccessKey}" +
                                 $"&amount={amount}" +
                                 $"&extraData={extraDataBase64}" +
                                 $"&ipnUrl={_config.IpnUrl}" +
                                 $"&orderId={orderId}" +
                                 $"&orderInfo={orderInfo}" +
                                 $"&partnerCode={_config.PartnerCode}" +
                                 $"&redirectUrl={redirectUrl}" +
                                 $"&requestId={requestId}" +
                                 $"&requestType=captureWallet";

                var signature = ComputeHmacSha256(rawSignature, _config.SecretKey);

                _logger.LogInformation("MoMo signature data: {RawSignature}", rawSignature);

                // Create MoMo request
                var momoRequest = new MoMoCreateOrderRequest
                {
                    PartnerCode = _config.PartnerCode,
                    RequestId = requestId,
                    OrderId = orderId,
                    Amount = amount,
                    OrderInfo = orderInfo,
                    RedirectUrl = redirectUrl,
                    IpnUrl = _config.IpnUrl,
                    RequestType = "captureWallet",
                    ExtraData = extraDataBase64,
                    Lang = normalizedLang,
                    Signature = signature
                };

                // Send request to MoMo
                var jsonContent = JsonSerializer.Serialize(momoRequest, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                _logger.LogInformation("MoMo request payload: {Payload}", jsonContent);

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_config.BaseUrl}{_config.CreateOrderEndpoint}",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("MoMo response: {Response}", responseString);

                var momoResponse = JsonSerializer.Deserialize<MoMoCreateOrderResponse>(responseString);

                if (momoResponse == null)
                {
                    return new CreateMoMoOrderResponse
                    {
                        Success = false,
                        Message = "Failed to parse MoMo response"
                    };
                }

                // Build detailed error message if there are subErrors
                var errorMessage = momoResponse.Message;
                if (momoResponse.SubErrors != null && momoResponse.SubErrors.Count > 0)
                {
                    var subErrorDetails = string.Join(", ", momoResponse.SubErrors.Select(e => $"{e.Field}: {e.Message}"));
                    errorMessage = $"{momoResponse.Message} Details: {subErrorDetails}";
                }

                // Save transaction to database (only if successful)
                if (momoResponse.ResultCode == 0)
                {
                    var transaction = new PaymentTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        OrderId = orderId,
                        RequestId = requestId,
                        Amount = amount,
                        PaymentGateway = "MoMo",
                        Status = "Pending",
                        ResultCode = momoResponse.ResultCode,
                        Message = momoResponse.Message,
                        OrderInfo = orderInfo,
                        PayUrl = momoResponse.PayUrl,
                        Deeplink = momoResponse.Deeplink,
                        QrCodeUrl = momoResponse.QrCodeUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                    };

                    _dbContext.PaymentTransactions.Add(transaction);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Saved MoMo transaction: OrderId={OrderId}, RequestId={RequestId}, Amount={Amount}, ExpiresAt={ExpiresAt}",
                        orderId, requestId, amount, transaction.ExpiresAt);
                }

                // ResultCode 0 means success for MoMo
                return new CreateMoMoOrderResponse
                {
                    Success = momoResponse.ResultCode == 0,
                    Message = errorMessage,
                    PayUrl = momoResponse.PayUrl,
                    Deeplink = momoResponse.Deeplink,
                    QrCodeUrl = momoResponse.QrCodeUrl,
                    OrderId = orderId,
                    RequestId = requestId,
                    ResultCode = momoResponse.ResultCode,
                    SubErrors = momoResponse.SubErrors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo order");
                return new CreateMoMoOrderResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
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
