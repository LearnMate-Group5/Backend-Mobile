using System.Text.Json.Serialization;

namespace Application.Payments.DTOs
{
    public class ZaloPayQueryRequest
    {
        [JsonPropertyName("app_id")]
        public int AppId { get; set; }

        [JsonPropertyName("app_trans_id")]
        public string AppTransId { get; set; } = string.Empty;

        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;
    }

    public class ZaloPayQueryResponse
    {
        [JsonPropertyName("return_code")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("return_message")]
        public string ReturnMessage { get; set; } = string.Empty;

        [JsonPropertyName("sub_return_code")]
        public int SubReturnCode { get; set; }

        [JsonPropertyName("sub_return_message")]
        public string SubReturnMessage { get; set; } = string.Empty;

        [JsonPropertyName("is_processing")]
        public bool IsProcessing { get; set; }

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("zp_trans_id")]
        public long ZpTransId { get; set; }

        [JsonPropertyName("server_time")]
        public long ServerTime { get; set; }

        [JsonPropertyName("discount_amount")]
        public long DiscountAmount { get; set; }
    }
}
