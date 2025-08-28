namespace RealTimeMediaBot.Models;

public class IncomingCall
{
    public string Id { get; set; } = string.Empty;
    public string CallerId { get; set; } = string.Empty;
    public string CallerDisplayName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public CallState State { get; set; }
    public string MeetingUrl { get; set; } = string.Empty;
}

public enum CallState
{
    Unknown,
    Incoming,
    Establishing,
    Established,
    Terminated,
    Failed
}

public class CallAnswerRequest
{
    public string CallId { get; set; } = string.Empty;
    public bool Accept { get; set; } = true;
    public string? RedirectUri { get; set; }
}

public class CallTransferRequest
{
    public string CallId { get; set; } = string.Empty;
    public string TargetUri { get; set; } = string.Empty;
    public string? TargetDisplayName { get; set; }
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

public class AudioFrame
{
    public string CallId { get; set; } = string.Empty;
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public DateTime Timestamp { get; set; }
}
