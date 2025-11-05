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
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class ZaloPayService : IZaloPayService
    {
        private readonly HttpClient _httpClient;
        private readonly ZaloPayConfig _config;
        private readonly ILogger<ZaloPayService> _logger;
        private readonly MyDbContext _dbContext;
        private readonly ISubscriptionService _subscriptionService;

        public ZaloPayService(
            HttpClient httpClient,
            IOptions<ZaloPayConfig> config,
            ILogger<ZaloPayService> logger,
            MyDbContext dbContext,
            ISubscriptionService subscriptionService)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
            _dbContext = dbContext;
            _subscriptionService = subscriptionService;

            // Log configuration on initialization
            _logger.LogInformation("ZaloPay Configuration - AppId: {AppId}, BaseUrl: {BaseUrl}, CallbackUrl: {CallbackUrl}",
                _config.AppId, _config.BaseUrl, _config.CallbackUrl);
        }

        public async Task<CreateZaloPayOrderResponse> CreateOrderAsync(string userId, CreateZaloPayOrderRequest request)
        {
            try
            {
                // Parse orderId as UserSubscriptionId (Guid)
                if (string.IsNullOrWhiteSpace(request.OrderId) || !Guid.TryParse(request.OrderId, out var userSubscriptionId))
                {
                    _logger.LogError("Invalid or missing OrderId (UserSubscriptionId): {OrderId}", request.OrderId);
                    return new CreateZaloPayOrderResponse
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
                    return new CreateZaloPayOrderResponse
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

                // Check for existing PENDING transaction that hasn't expired yet
                var existingTransaction = await _dbContext.PaymentTransactions
                    .Where(t => t.UserId == userId &&
                               t.Amount == amount &&
                               t.PaymentGateway == "ZaloPay" &&
                               t.Status == "Pending" &&
                               t.ExpiresAt.HasValue &&
                               t.ExpiresAt.Value > DateTime.UtcNow)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if (existingTransaction != null)
                {
                    _logger.LogInformation("Found existing non-expired ZaloPay transaction for UserId: {UserId}, OrderId: {OrderId}, ExpiresAt: {ExpiresAt}. Returning existing order.",
                        userId, existingTransaction.OrderId, existingTransaction.ExpiresAt);

                    // Return existing transaction details
                    return new CreateZaloPayOrderResponse
                    {
                        Success = true,
                        Message = existingTransaction.Message ?? "Using existing order",
                        OrderUrl = existingTransaction.OrderInfo,
                        AppTransId = existingTransaction.AppTransId,
                        ZpTransToken = existingTransaction.ZpTransToken,
                        QrCode = existingTransaction.QrCode,
                        ReturnCode = existingTransaction.ResultCode,
                        SubReturnCode = 0
                    };
                }

                // Generate unique transaction ID with format: yyMMdd_xxxxx
                var appTransId = GenerateAppTransId();
                var appTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // Use the UserSubscriptionId as orderId
                var orderId = request.OrderId;

                // Prepare description with subscription name
                var description = !string.IsNullOrWhiteSpace(request.Description)
                    ? request.Description
                    : $"Payment for {priceResponse.SubscriptionName}";

                // Prepare embed_data with userId and orderId
                var redirectUrl = !string.IsNullOrWhiteSpace(request.RedirectUrl)
                    ? request.RedirectUrl
                    : _config.RedirectUrl;

                var embedData = new
                {
                    redirecturl = redirectUrl,
                    userId = userId,
                    orderId = orderId
                };
                var embedDataString = JsonSerializer.Serialize(embedData);

                // Create ZaloPay request
                var zaloPayRequest = new ZaloPayCreateOrderRequest
                {
                    AppId = _config.AppId,
                    AppUser = userId,
                    AppTransId = appTransId,
                    AppTime = appTime,
                    Amount = amount,
                    Description = description,
                    CallbackUrl = _config.CallbackUrl, // Use config callback URL
                    Item = "[]",
                    EmbedData = embedDataString
                };

                // Calculate MAC using Key1 for order creation
                var macData = $"{zaloPayRequest.AppId}|{zaloPayRequest.AppTransId}|{zaloPayRequest.AppUser}|{zaloPayRequest.Amount}|{zaloPayRequest.AppTime}|{zaloPayRequest.EmbedData}|{zaloPayRequest.Item}";
                zaloPayRequest.Mac = ComputeHmacSha256(macData, _config.Key1);

                _logger.LogInformation("Creating new ZaloPay order: AppTransId={AppTransId}, OrderId={OrderId}, Amount={Amount}, CallbackUrl={CallbackUrl}",
                    appTransId, orderId, amount, _config.CallbackUrl);

                // Send request to ZaloPay
                var jsonContent = JsonSerializer.Serialize(zaloPayRequest);
                _logger.LogInformation("ZaloPay request payload: {Payload}", jsonContent);

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_config.BaseUrl}{_config.CreateOrderEndpoint}",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("ZaloPay response: {Response}", responseString);

                var zaloPayResponse = JsonSerializer.Deserialize<ZaloPayCreateOrderResponse>(responseString);

                if (zaloPayResponse == null)
                {
                    return new CreateZaloPayOrderResponse
                    {
                        Success = false,
                        Message = "Failed to parse ZaloPay response"
                    };
                }

                // Save transaction to database
                if (zaloPayResponse.ReturnCode == 1)
                {
                    var transaction = new PaymentTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        OrderId = orderId,
                        RequestId = appTransId,
                        AppTransId = appTransId,
                        Amount = amount,
                        PaymentGateway = "ZaloPay",
                        Status = "Pending",
                        ResultCode = zaloPayResponse.ReturnCode,
                        Message = zaloPayResponse.ReturnMessage,
                        OrderInfo = zaloPayResponse.OrderUrl ?? "",
                        ZpTransToken = zaloPayResponse.ZpTransToken,
                        QrCode = zaloPayResponse.QrCode,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                    };

                    _dbContext.PaymentTransactions.Add(transaction);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Saved ZaloPay transaction: OrderId={OrderId}, AppTransId={AppTransId}, Amount={Amount}, ExpiresAt={ExpiresAt}",
                        orderId, appTransId, amount, transaction.ExpiresAt);
                }

                // Return success = true only if return_code is 1
                return new CreateZaloPayOrderResponse
                {
                    Success = zaloPayResponse.ReturnCode == 1,
                    Message = zaloPayResponse.ReturnMessage,
                    OrderUrl = zaloPayResponse.OrderUrl,
                    AppTransId = appTransId,
                    ZpTransToken = zaloPayResponse.ZpTransToken,
                    QrCode = zaloPayResponse.QrCode,
                    OrderToken = zaloPayResponse.OrderToken,
                    ReturnCode = zaloPayResponse.ReturnCode,
                    SubReturnCode = zaloPayResponse.SubReturnCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZaloPay order");
                return new CreateZaloPayOrderResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ZaloPayQueryResponse> QueryOrderAsync(string appTransId)
        {
            try
            {
                // Calculate MAC: HMAC_SHA256(app_id|app_trans_id, key2)
                // According to ZaloPay docs for query API:
                // mac = HMAC(hmac_algorithm, key1, app_id+"|"+app_trans_id+"|"+key1)
                // Key1 is INCLUDED in the data string and used as HMAC key
                var macData = $"{_config.AppId}|{appTransId}|{_config.Key1}";
                var mac = ComputeHmacSha256(macData, _config.Key1);

                _logger.LogInformation("Querying ZaloPay order status: AppTransId={AppTransId}, MacData={MacData}, MAC={Mac}",
                    appTransId, macData, mac);

                // Send request to ZaloPay as form data
                var formData = new Dictionary<string, string>
                {
                    { "app_id", _config.AppId.ToString() },
                    { "app_trans_id", appTransId },
                    { "mac", mac }
                };

                var content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.PostAsync(
                    $"{_config.BaseUrl}{_config.QueryOrderEndpoint}",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("ZaloPay query response: {Response}", responseString);

                var queryResponse = JsonSerializer.Deserialize<ZaloPayQueryResponse>(responseString);

                if (queryResponse == null)
                {
                    return new ZaloPayQueryResponse
                    {
                        ReturnCode = 0,
                        ReturnMessage = "Failed to parse query response"
                    };
                }

                // If payment is successful (return_code = 1), update the transaction in database
                if (queryResponse.ReturnCode == 1)
                {
                    var transaction = await _dbContext.PaymentTransactions
                        .FirstOrDefaultAsync(t => t.AppTransId == appTransId);

                    if (transaction != null && transaction.Status == "Pending")
                    {
                        transaction.Status = "Success";
                        transaction.ResultCode = 1;
                        transaction.Message = "Payment successful (verified by query)";
                        transaction.ZpTransToken = queryResponse.ZpTransId.ToString();
                        transaction.UpdatedAt = DateTime.UtcNow;

                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation("Updated transaction status to Success for AppTransId: {AppTransId}", appTransId);
                    }
                }

                return queryResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying ZaloPay order");
                return new ZaloPayQueryResponse
                {
                    ReturnCode = 0,
                    ReturnMessage = $"Error: {ex.Message}"
                };
            }
        }

        private string GenerateAppTransId()
        {
            var date = DateTime.Now.ToString("yyMMdd");
            var randomId = Guid.NewGuid().ToString("N").Substring(0, 10);
            return $"{date}_{randomId}";
        }

        private string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
