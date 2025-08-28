using System.Text.Json.Serialization;

namespace RealTimeMediaBot.Models;

public class TeamsWebhookData
{
    [JsonPropertyName("resource")]
    public string Resource { get; set; } = string.Empty;
    
    [JsonPropertyName("changeType")]
    public string ChangeType { get; set; } = string.Empty;
    
    [JsonPropertyName("resourceData")]
    public string ResourceData { get; set; } = string.Empty;
    
    [JsonPropertyName("subscriptionId")]
    public string SubscriptionId { get; set; } = string.Empty;
    
    [JsonPropertyName("subscriptionExpirationDateTime")]
    public string SubscriptionExpirationDateTime { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public TeamsWebhookValue[]? Value { get; set; }
}

public class TeamsWebhookValue
{
    [JsonPropertyName("resource")]
    public string Resource { get; set; } = string.Empty;
    
    [JsonPropertyName("changeType")]
    public string ChangeType { get; set; } = string.Empty;
    
    [JsonPropertyName("resourceData")]
    public string ResourceData { get; set; } = string.Empty;
    
    [JsonPropertyName("subscriptionId")]
    public string SubscriptionId { get; set; } = string.Empty;
    
    [JsonPropertyName("subscriptionExpirationDateTime")]
    public string SubscriptionExpirationDateTime { get; set; } = string.Empty;
}

// Model dla danych połączenia z Teams
public class TeamsCallResourceData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
    
    [JsonPropertyName("from")]
    public TeamsCallParticipant? From { get; set; }
    
    [JsonPropertyName("to")]
    public TeamsCallParticipant[]? To { get; set; }
    
    [JsonPropertyName("meetingInfo")]
    public TeamsMeetingInfo? MeetingInfo { get; set; }
}

public class TeamsCallParticipant
{
    [JsonPropertyName("user")]
    public TeamsUser? User { get; set; }
    
    [JsonPropertyName("phone")]
    public TeamsPhone? Phone { get; set; }
}

public class TeamsUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("userPrincipalName")]
    public string UserPrincipalName { get; set; } = string.Empty;
}

public class TeamsPhone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class TeamsMeetingInfo
{
    [JsonPropertyName("joinWebUrl")]
    public string JoinWebUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("joinMeetingIdSettings")]
    public TeamsJoinMeetingIdSettings? JoinMeetingIdSettings { get; set; }
}

public class TeamsJoinMeetingIdSettings
{
    [JsonPropertyName("isPasscodeRequired")]
    public bool IsPasscodeRequired { get; set; }
    
    [JsonPropertyName("isContentSharingDisabled")]
    public bool IsContentSharingDisabled { get; set; }
}

// Model dla danych audio media z Teams
public class TeamsAudioMediaResourceData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("callId")]
    public string CallId { get; set; } = string.Empty;
    
    [JsonPropertyName("audioData")]
    public string AudioData { get; set; } = string.Empty; // Base64 encoded audio
    
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}
