using System.Text.Json.Serialization;

namespace Application.Payments.DTOs;

public class CreateMoMoOrderRequest
{
    /// <summary>
    /// Required: UserSubscriptionId (Guid) - used to fetch the subscription price
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    public string OrderInfo { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
    public string? ExtraData { get; set; }
    public string Lang { get; set; } = "vi";
}

// Internal DTO for MoMo API
public class MoMoCreateOrderRequest
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("orderInfo")]
    public string OrderInfo { get; set; } = string.Empty;

    [JsonPropertyName("redirectUrl")]
    public string RedirectUrl { get; set; } = string.Empty;

    [JsonPropertyName("ipnUrl")]
    public string IpnUrl { get; set; } = string.Empty;

    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = "captureWallet";

    [JsonPropertyName("extraData")]
    public string ExtraData { get; set; } = string.Empty;

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = "vi";

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;
}

public class MoMoCreateOrderResponse
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    [JsonPropertyName("payUrl")]
    public string PayUrl { get; set; } = string.Empty;

    [JsonPropertyName("deeplink")]
    public string Deeplink { get; set; } = string.Empty;

    [JsonPropertyName("qrCodeUrl")]
    public string QrCodeUrl { get; set; } = string.Empty;

    [JsonPropertyName("deeplinkMiniApp")]
    public string DeeplinkMiniApp { get; set; } = string.Empty;

    [JsonPropertyName("subErrors")]
    public List<MoMoSubError>? SubErrors { get; set; }
}

public class CreateMoMoOrderResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PayUrl { get; set; }
    public string? Deeplink { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? OrderId { get; set; }
    public string? RequestId { get; set; }
    public int? ResultCode { get; set; }
    public List<MoMoSubError>? SubErrors { get; set; }
}

public class MoMoSubError
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
