namespace RealTimeMediaBot.Services;

public interface IGraphService
{
    Task<bool> ValidateMeetingUrlAsync(string meetingUrl);
    Task<string> GetMeetingInfoAsync(string meetingUrl);
}
