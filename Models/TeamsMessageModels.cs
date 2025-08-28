using System.Text.Json.Serialization;

namespace RealTimeMediaBot.Models;

public class TeamsMessageData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("textFormat")]
    public string? TextFormat { get; set; }
    
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("channelId")]
    public string? ChannelId { get; set; }
    
    [JsonPropertyName("serviceUrl")]
    public string? ServiceUrl { get; set; }
    
    [JsonPropertyName("from")]
    public TeamsMessageFrom? From { get; set; }
    
    [JsonPropertyName("conversation")]
    public TeamsMessageConversation? Conversation { get; set; }
    
    [JsonPropertyName("recipient")]
    public TeamsMessageRecipient? Recipient { get; set; }
    
    [JsonPropertyName("channelData")]
    public TeamsMessageChannelData? ChannelData { get; set; }
    
    [JsonPropertyName("membersAdded")]
    public TeamsMessageMember[]? MembersAdded { get; set; }
    
    [JsonPropertyName("membersRemoved")]
    public TeamsMessageMember[]? MembersRemoved { get; set; }
    
    [JsonPropertyName("replyToId")]
    public string? ReplyToId { get; set; }
}

public class TeamsMessageFrom
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("aadObjectId")]
    public string? AadObjectId { get; set; }
}

public class TeamsMessageConversation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("conversationType")]
    public string? ConversationType { get; set; }
    
    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }
    
    [JsonPropertyName("isGroup")]
    public bool? IsGroup { get; set; }
}

public class TeamsMessageRecipient
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

public class TeamsMessageChannelData
{
    [JsonPropertyName("tenant")]
    public TeamsMessageTenant? Tenant { get; set; }
    
    [JsonPropertyName("channel")]
    public TeamsMessageChannel? Channel { get; set; }
    
    [JsonPropertyName("team")]
    public TeamsMessageTeam? Team { get; set; }
    
    [JsonPropertyName("meeting")]
    public TeamsMessageMeeting? Meeting { get; set; }
}

public class TeamsMessageTenant
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class TeamsMessageChannel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class TeamsMessageTeam
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class TeamsMessageMeeting
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class TeamsMessageMember
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("aadObjectId")]
    public string? AadObjectId { get; set; }
}

// Modele odpowiedzi
public class TeamsMessageResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "message";
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("textFormat")]
    public string? TextFormat { get; set; }
    
    [JsonPropertyName("attachments")]
    public object[]? Attachments { get; set; }
}

public class TeamsInvokeResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; } = 200;
    
    [JsonPropertyName("body")]
    public object? Body { get; set; }
}
