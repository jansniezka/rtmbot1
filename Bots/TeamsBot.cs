using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using RealTimeMediaBot.Models;
using RealTimeMediaBot.Services;
using System.Collections.Concurrent;

namespace RealTimeMediaBot.Bots;

public class TeamsBot : IDisposable
{
    private readonly ILogger<TeamsBot> _logger;
    private readonly IOptions<BotConfiguration> _botConfig;
    private readonly IOptions<AzureAdConfiguration> _azureConfig;
    private readonly IOptions<GraphConfiguration> _graphConfig;
    private readonly IAuthenticationService _authService;
    private readonly IAudioCaptureService _audioCaptureService;
    
    private GraphServiceClient? _graphClient;
    private readonly ConcurrentDictionary<string, CallInfo> _activeCalls;
    private bool _disposed = false;

    public TeamsBot(
        ILogger<TeamsBot> logger,
        IOptions<BotConfiguration> botConfig,
        IOptions<AzureAdConfiguration> azureConfig,
        IOptions<GraphConfiguration> graphConfig,
        IAuthenticationService authService,
        IAudioCaptureService audioCaptureService)
    {
        _logger = logger;
        _botConfig = botConfig;
        _azureConfig = azureConfig;
        _graphConfig = graphConfig;
        _authService = authService;
        _audioCaptureService = audioCaptureService;
        _activeCalls = new ConcurrentDictionary<string, CallInfo>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Inicjalizacja bota Teams...");
            
            // Uzyskanie tokenu dostępu
            var accessToken = await _authService.GetAccessTokenAsync();
            
            // Inicjalizacja Graph Client z prostym auth provider
            _graphClient = new GraphServiceClient(new HttpClient());
            
            _logger.LogInformation("Bot Teams został zainicjalizowany pomyślnie. Token uzyskany.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas inicjalizacji bota Teams");
            throw;
        }
    }

    // Obsługa webhook'ów przychodzących połączeń
    public async Task HandleIncomingCallWebhookAsync(TeamsWebhookData webhookData)
    {
        try
        {
            _logger.LogInformation("Obsługa webhook przychodzącego połączenia: {Resource}", webhookData.Resource);

            // Parsuj dane połączenia z webhook
            var callInfo = ParseCallInfoFromWebhook(webhookData);
            if (callInfo != null)
            {
                // Dodaj do aktywnych połączeń
                _activeCalls.TryAdd(callInfo.CallId, callInfo);
                
                _logger.LogInformation("Dodano nowe połączenie: {CallId} od {CallerId}", 
                    callInfo.CallId, callInfo.CallerId);

                // Automatycznie akceptuj połączenie
                await AcceptIncomingCallAsync(callInfo.CallId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas obsługi webhook przychodzącego połączenia");
        }
    }

    // Obsługa webhook'ów aktualizacji połączeń
    public async Task HandleCallUpdatedWebhookAsync(TeamsWebhookData webhookData)
    {
        try
        {
            _logger.LogInformation("Obsługa webhook aktualizacji połączenia: {Resource}", webhookData.Resource);

            var callInfo = ParseCallInfoFromWebhook(webhookData);
            if (callInfo != null && _activeCalls.TryGetValue(callInfo.CallId, out var existingCall))
            {
                // Aktualizuj status połączenia
                existingCall.State = callInfo.State;
                existingCall.LastUpdated = DateTime.UtcNow;

                _logger.LogInformation("Zaktualizowano status połączenia {CallId}: {State}", 
                    callInfo.CallId, callInfo.State);

                // Jeśli połączenie zostało zakończone, usuń z aktywnych
                if (callInfo.State == CallState.Terminated || callInfo.State == CallState.Failed)
                {
                    _activeCalls.TryRemove(callInfo.CallId, out _);
                    _logger.LogInformation("Połączenie {CallId} zostało usunięte z aktywnych", callInfo.CallId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas obsługi webhook aktualizacji połączenia");
        }
    }

    // Obsługa webhook'ów audio media
    public async Task HandleAudioMediaWebhookAsync(TeamsWebhookData webhookData)
    {
        try
        {
            _logger.LogInformation("Obsługa webhook audio media: {Resource}", webhookData.Resource);

            // Parsuj dane audio z webhook
            var audioData = ParseAudioDataFromWebhook(webhookData);
            if (audioData != null)
            {
                // Przekaż do AudioCaptureService - używamy AudioFrame
                var audioFrame = new AudioFrame
                {
                    AudioData = audioData.AudioBytes,
                    Timestamp = audioData.Timestamp,
                    CallId = audioData.CallId
                };
                
                await _audioCaptureService.ProcessAudioFrameAsync(audioFrame);
                
                _logger.LogDebug("Przetworzono audio data dla połączenia: {CallId}", audioData.CallId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas obsługi webhook audio media");
        }
    }

    // Akceptowanie przychodzącego połączenia
    public async Task AcceptIncomingCallAsync(string callId)
    {
        try
        {
            if (_graphClient == null)
            {
                throw new InvalidOperationException("Graph Client nie został zainicjalizowany");
            }

            _logger.LogInformation("Akceptowanie przychodzącego połączenia: {CallId}", callId);

            // Tutaj implementacja akceptowania połączenia przez Microsoft Graph
            // await _graphClient.Communications.Calls[callId].AnswerAsync(...);

            if (_activeCalls.TryGetValue(callId, out var callInfo))
            {
                callInfo.State = CallState.Established;
                callInfo.LastUpdated = DateTime.UtcNow;
            }

            _logger.LogInformation("Połączenie {CallId} zostało zaakceptowane", callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas akceptowania połączenia {CallId}", callId);
            throw;
        }
    }

    // Odrzucanie przychodzącego połączenia
    public async Task RejectIncomingCallAsync(string callId, string? redirectUri = null)
    {
        try
        {
            _logger.LogInformation("Odrzucanie przychodzącego połączenia: {CallId}", callId);

            // Implementacja odrzucania połączenia
            // await _graphClient.Communications.Calls[callId].RejectAsync(...);

            if (_activeCalls.TryRemove(callId, out var callInfo))
            {
                callInfo.State = CallState.Terminated;
                _logger.LogInformation("Połączenie {CallId} zostało odrzucone i usunięte", callId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas odrzucania połączenia {CallId}", callId);
            throw;
        }
    }

    // Przekierowanie połączenia
    public async Task TransferCallAsync(string callId, string targetUri, string? targetDisplayName = null)
    {
        try
        {
            _logger.LogInformation("Przekierowanie połączenia {CallId} do: {TargetUri}", callId, targetUri);

            // Implementacja przekierowania połączenia
            // await _graphClient.Communications.Calls[callId].TransferAsync(...);

            if (_activeCalls.TryRemove(callId, out var callInfo))
            {
                callInfo.State = CallState.Terminated;
                _logger.LogInformation("Połączenie {CallId} zostało przekierowane", callId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przekierowania połączenia {CallId}", callId);
            throw;
        }
    }

    // Kończenie połączenia
    public async Task EndCallAsync(string callId)
    {
        try
        {
            _logger.LogInformation("Kończenie połączenia: {CallId}", callId);

            // Implementacja kończenia połączenia
            // await _graphClient.Communications.Calls[callId].DeleteAsync();

            if (_activeCalls.TryRemove(callId, out var callInfo))
            {
                callInfo.State = CallState.Terminated;
                _logger.LogInformation("Połączenie {CallId} zostało zakończone", callId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas kończenia połączenia {CallId}", callId);
            throw;
        }
    }

    // Pobieranie statusu połączeń
    public object GetCallStatus()
    {
        var calls = _activeCalls.Values.Select(c => new
        {
            c.CallId,
            c.CallerId,
            c.CallerDisplayName,
            c.State,
            c.MeetingUrl,
            c.Timestamp,
            c.LastUpdated
        }).ToList();

        return new
        {
            ActiveCallsCount = _activeCalls.Count,
            Calls = calls
        };
    }

    // Parsowanie informacji o połączeniu z webhook
    private CallInfo? ParseCallInfoFromWebhook(TeamsWebhookData webhookData)
    {
        try
        {
            // Implementacja parsowania webhook Teams
            // To jest uproszczona wersja - w rzeczywistości Teams ma specyficzny format
            return new CallInfo
            {
                CallId = Guid.NewGuid().ToString(), // Tymczasowe ID
                CallerId = "unknown@teams.com",
                CallerDisplayName = "Unknown Caller",
                State = CallState.Incoming,
                MeetingUrl = "",
                Timestamp = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas parsowania informacji o połączeniu z webhook");
            return null;
        }
    }

    // Parsowanie danych audio z webhook
    private AudioData? ParseAudioDataFromWebhook(TeamsWebhookData webhookData)
    {
        try
        {
            // Implementacja parsowania audio data z webhook
            // To jest uproszczona wersja
            return new AudioData
            {
                CallId = "unknown",
                AudioBytes = new byte[1024], // Przykładowe dane
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas parsowania danych audio z webhook");
            return null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _graphClient?.Dispose();
            _disposed = true;
        }
    }
}
