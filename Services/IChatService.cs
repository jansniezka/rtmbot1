using RealTimeMediaBot.Models;

namespace RealTimeMediaBot.Services;

public interface IChatService
{
    Task<string> GetUserDisplayNameAsync(string userId);
    Task<string> GetChatContextAsync(string chatId, int messageCount = 5);
    Task<bool> SendMessageToChatAsync(string chatId, string message);
    Task<string> GetTeamInfoAsync(string teamId);
}
