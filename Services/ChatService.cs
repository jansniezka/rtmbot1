using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using RealTimeMediaBot.Models;

namespace RealTimeMediaBot.Services;

public class ChatService : IChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly IAuthenticationService _authService;
    private GraphServiceClient? _graphClient;

    public ChatService(
        ILogger<ChatService> logger,
        IAuthenticationService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    private async Task<GraphServiceClient> GetGraphClientAsync()
    {
        if (_graphClient == null)
        {
            var accessToken = await _authService.GetAccessTokenAsync();
            
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            _graphClient = new GraphServiceClient(httpClient);
        }
        
        return _graphClient;
    }

    public async Task<string> GetUserDisplayNameAsync(string userId)
    {
        try
        {
            _logger.LogDebug("🔍 Pobieranie displayName dla użytkownika: {UserId}", userId);
            
            var graphClient = await GetGraphClientAsync();
            var user = await graphClient.Users[userId].GetAsync();
            
            var displayName = user?.DisplayName ?? "Unknown User";
            
            _logger.LogDebug("✅ DisplayName dla {UserId}: {DisplayName}", userId, displayName);
            return displayName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas pobierania displayName dla użytkownika {UserId}", userId);
            return "Unknown User";
        }
    }

    public async Task<string> GetChatContextAsync(string chatId, int messageCount = 5)
    {
        try
        {
            _logger.LogDebug("🔍 Pobieranie kontekstu czatu: {ChatId} (ostatnie {Count} wiadomości)", chatId, messageCount);
            
            var graphClient = await GetGraphClientAsync();
            var messages = await graphClient.Chats[chatId].Messages
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Top = messageCount;
                    requestConfiguration.QueryParameters.Orderby = new string[] { "createdDateTime desc" };
                });

            if (messages?.Value != null && messages.Value.Count > 0)
            {
                var context = string.Join("\n", messages.Value
                    .Where(m => !string.IsNullOrEmpty(m.Body?.Content))
                    .Select(m => $"{m.From?.User?.DisplayName ?? "Unknown"}: {m.Body?.Content}")
                    .Reverse());
                
                _logger.LogDebug("✅ Kontekst czatu {ChatId}: {MessageCount} wiadomości", chatId, messages.Value.Count);
                return context;
            }
            
            return "Brak historii wiadomości";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas pobierania kontekstu czatu {ChatId}", chatId);
            return "Nie udało się pobrać kontekstu czatu";
        }
    }

    public async Task<bool> SendMessageToChatAsync(string chatId, string message)
    {
        try
        {
            _logger.LogDebug("💬 Wysyłanie wiadomości do czatu: {ChatId}", chatId);
            
            var graphClient = await GetGraphClientAsync();
            
            var chatMessage = new Microsoft.Graph.Models.ChatMessage
            {
                Body = new Microsoft.Graph.Models.ItemBody
                {
                    ContentType = Microsoft.Graph.Models.BodyType.Text,
                    Content = message
                }
            };

            var result = await graphClient.Chats[chatId].Messages.PostAsync(chatMessage);
            
            if (result?.Id != null)
            {
                _logger.LogInformation("✅ Wiadomość wysłana do czatu {ChatId}: {MessageId}", chatId, result.Id);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas wysyłania wiadomości do czatu {ChatId}", chatId);
            return false;
        }
    }

    public async Task<string> GetTeamInfoAsync(string teamId)
    {
        try
        {
            _logger.LogDebug("🔍 Pobieranie informacji o zespole: {TeamId}", teamId);
            
            var graphClient = await GetGraphClientAsync();
            var team = await graphClient.Teams[teamId].GetAsync();
            
            var teamInfo = $"Team: {team?.DisplayName ?? "Unknown Team"}";
            
            _logger.LogDebug("✅ Informacje o zespole {TeamId}: {TeamInfo}", teamId, teamInfo);
            return teamInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas pobierania informacji o zespole {TeamId}", teamId);
            return "Unknown Team";
        }
    }
}
