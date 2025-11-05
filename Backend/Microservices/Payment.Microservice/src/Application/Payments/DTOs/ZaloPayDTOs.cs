using System;
using System.Text.Json.Serialization;

namespace Application.Payments.DTOs
{
    public class CreateZaloPayOrderRequest
    {
        /// <summary>
        /// Required: UserSubscriptionId (Guid) - used to fetch the subscription price
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string? RedirectUrl { get; set; }
    }

    public class ZaloPayCreateOrderRequest
    {
        [JsonPropertyName("app_id")]
        public int AppId { get; set; }

        [JsonPropertyName("app_user")]
        public string AppUser { get; set; } = string.Empty;

        [JsonPropertyName("app_trans_id")]
        public string AppTransId { get; set; } = string.Empty;

        [JsonPropertyName("app_time")]
        public long AppTime { get; set; }

        [JsonPropertyName("expire_duration_seconds")]
        public long ExpireDurationSeconds { get; set; } = 900; // 15 minutes default

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("callback_url")]
        public string CallbackUrl { get; set; } = string.Empty;

        [JsonPropertyName("item")]
        public string Item { get; set; } = "[]";

        [JsonPropertyName("embed_data")]
        public string EmbedData { get; set; } = "{}";

        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;
    }

    public class ZaloPayCreateOrderResponse
    {
        [JsonPropertyName("return_code")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("return_message")]
        public string ReturnMessage { get; set; } = string.Empty;

        [JsonPropertyName("sub_return_code")]
        public int SubReturnCode { get; set; }

        [JsonPropertyName("sub_return_message")]
        public string SubReturnMessage { get; set; } = string.Empty;

        [JsonPropertyName("zp_trans_token")]
        public string? ZpTransToken { get; set; }

        [JsonPropertyName("order_url")]
        public string? OrderUrl { get; set; }

        [JsonPropertyName("order_token")]
        public string? OrderToken { get; set; }

        [JsonPropertyName("qr_code")]
        public string? QrCode { get; set; }
    }

    public class CreateZaloPayOrderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? OrderUrl { get; set; }
        public string? AppTransId { get; set; }
        public string? ZpTransToken { get; set; }
        public string? QrCode { get; set; }
        public string? OrderToken { get; set; }
        public int? ReturnCode { get; set; }
        public int? SubReturnCode { get; set; }
    }
}
