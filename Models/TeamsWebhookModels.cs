namespace RealTimeMediaBot.Models;

public class TeamsWebhookData
{
    public string Resource { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string ResourceData { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string SubscriptionExpirationDateTime { get; set; } = string.Empty;
}

public class CallInfo
{
    public string CallId { get; set; } = string.Empty;
    public string CallerId { get; set; } = string.Empty;
    public string CallerDisplayName { get; set; } = string.Empty;
    public CallState State { get; set; }
    public string MeetingUrl { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class AudioData
{
    public string CallId { get; set; } = string.Empty;
    public byte[] AudioBytes { get; set; } = Array.Empty<byte>();
    public DateTime Timestamp { get; set; }
}
