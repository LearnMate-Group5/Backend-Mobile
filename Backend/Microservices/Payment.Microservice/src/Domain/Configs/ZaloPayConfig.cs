using System;

namespace Domain.Configs
{
    public class ZaloPayConfig
    {
        public int AppId { get; set; }
        public string Key1 { get; set; } = string.Empty; // Key for creating order MAC
        public string Key2 { get; set; } = string.Empty; // Key for IPN/Query MAC
        public string BaseUrl { get; set; } = string.Empty;
        public string CreateOrderEndpoint { get; set; } = "/v2/create";
        public string QueryOrderEndpoint { get; set; } = "/v2/query";
        public string CallbackUrl { get; set; } = string.Empty; // IPN/Callback URL for ZaloPay
        public string RedirectUrl { get; set; } = string.Empty; // Redirect URL after payment
    }
}
