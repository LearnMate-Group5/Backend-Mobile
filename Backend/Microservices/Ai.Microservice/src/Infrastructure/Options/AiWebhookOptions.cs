namespace Infrastructure.Options;

public class AiWebhookOptions
{
    public const string SectionName = "AiWebhook";

    public string Endpoint { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 100;
}

