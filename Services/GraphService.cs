using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealTimeMediaBot.Models;

namespace RealTimeMediaBot.Services;

public class GraphService : IGraphService
{
    private readonly ILogger<GraphService> _logger;
    private readonly IOptions<GraphConfiguration> _graphConfig;
    private readonly IAuthenticationService _authService;

    public GraphService(
        ILogger<GraphService> logger,
        IOptions<GraphConfiguration> graphConfig,
        IAuthenticationService authService)
    {
        _logger = logger;
        _graphConfig = graphConfig;
        _authService = authService;
    }

    public async Task<bool> ValidateMeetingUrlAsync(string meetingUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(meetingUrl))
                return false;

            // Podstawowa walidacja URL Teams
            if (!meetingUrl.Contains("teams.microsoft.com") && !meetingUrl.Contains("teams.live.com"))
                return false;

            // Można dodać dodatkową walidację przez Graph API
            _logger.LogDebug("URL spotkania został zwalidowany: {MeetingUrl}", meetingUrl);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas walidacji URL spotkania");
            return false;
        }
    }

    public async Task<string> GetMeetingInfoAsync(string meetingUrl)
    {
        try
        {
            // Tutaj można dodać logikę pobierania informacji o spotkaniu
            // Na razie zwracamy podstawowe informacje
            _logger.LogDebug("Pobieranie informacji o spotkaniu: {MeetingUrl}", meetingUrl);
            
            return $"Meeting URL: {meetingUrl}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania informacji o spotkaniu");
            throw;
        }
    }
}
