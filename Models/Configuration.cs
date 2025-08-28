namespace RealTimeMediaBot.Models;

public class BotConfiguration
{
    public string MicrosoftAppId { get; set; } = string.Empty;
    public string MicrosoftAppPassword { get; set; } = string.Empty;
    public string MicrosoftAppType { get; set; } = string.Empty;
    public string MicrosoftAppTenantId { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
}

public class AzureAdConfiguration
{
    public string Instance { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class GraphConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
}
