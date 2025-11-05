namespace Domain.Configs;

public class MoMoConfig
{
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string CreateOrderEndpoint { get; set; } = "/v2/gateway/api/create";
    public string IpnUrl { get; set; } = string.Empty; // IPN URL for MoMo callbacks
    public string RedirectUrl { get; set; } = string.Empty; // Redirect URL after payment
}
