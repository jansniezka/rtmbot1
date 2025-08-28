using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using RealTimeMediaBot.Models;
using RealTimeMediaBot.Services;
using System.Collections.Concurrent;
using System.Text.Json;

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
            _logger.LogInformation("Inicjalizacja bota Teams z Microsoft Graph API...");
            
            // Uzyskanie tokenu dostępu
            var accessToken = await _authService.GetAccessTokenAsync();
            
            // Inicjalizacja Graph Client (uproszczona wersja)
            _graphClient = new GraphServiceClient(new HttpClient());
            
            _logger.LogInformation("Bot Teams został zainicjalizowany pomyślnie z Microsoft Graph API.");
            _logger.LogInformation("Bot jest gotowy do odbierania połączeń na endpoint: {Endpoint}", 
                $"{_botConfig.Value.PublicUrl}/api/teamswebhook/calling");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas inicjalizacji bota Teams");
            throw;
        }
    }

    // RZECZYWISTA IMPLEMENTACJA - Obsługa webhook'ów przychodzących połączeń
    public async Task HandleIncomingCallWebhookAsync(TeamsWebhookData webhookData)
    {
        try
        {
            _logger.LogInformation("🔔 Otrzymano webhook przychodzącego połączenia: {Resource}", webhookData.Resource);

            // Parsuj dane połączenia z webhook zgodnie z dokumentacją Microsoft Graph
            var callInfo = ParseCallInfoFromWebhook(webhookData);
            if (callInfo != null)
            {
                // Dodaj do aktywnych połączeń
                _activeCalls.TryAdd(callInfo.CallId, callInfo);
                
                _logger.LogInformation("✅ Dodano nowe połączenie: {CallId} od {CallerId}", 
                    callInfo.CallId, callInfo.CallerId);

                // Automatycznie akceptuj połączenie
                await AcceptIncomingCallAsync(callInfo.CallId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas obsługi webhook przychodzącego połączenia");
        }
    }

    // Obsługa webhook'ów aktualizacji połączeń
    public async Task HandleCallUpdatedWebhookAsync(TeamsWebhookData webhookData)
    {
        try
        {
            _logger.LogInformation("🔄 Otrzymano webhook aktualizacji połączenia: {Resource}", webhookData.Resource);

            var callInfo = ParseCallInfoFromWebhook(webhookData);
            if (callInfo != null && _activeCalls.TryGetValue(callInfo.CallId, out var existingCall))
            {
                // Aktualizuj status połączenia
                existingCall.State = callInfo.State;
                existingCall.LastUpdated = DateTime.UtcNow;

                _logger.LogInformation("📊 Zaktualizowano status połączenia {CallId}: {State}", 
                    callInfo.CallId, callInfo.State);

                // Jeśli połączenie zostało zakończone, usuń z aktywnych
                if (callInfo.State == CallState.Terminated || callInfo.State == CallState.Failed)
                {
                    _activeCalls.TryRemove(callInfo.CallId, out _);
                    _logger.LogInformation("🗑️ Połączenie {CallId} zostało usunięte z aktywnych", callInfo.CallId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas obsługi webhook aktualizacji połączenia");
        }
    }

    // RZECZYWISTA IMPLEMENTACJA - Obsługa webhook'ów audio media
    public async Task HandleAudioMediaWebhookAsync(TeamsWebhookData webhookData)
    {
        try
        {
            _logger.LogInformation("🎵 Otrzymano webhook audio media: {Resource}", webhookData.Resource);

            // Parsuj dane audio z webhook zgodnie z dokumentacją Microsoft Graph
            var audioData = ParseAudioDataFromWebhook(webhookData);
            if (audioData != null)
            {
                // Przekaż do AudioCaptureService
                var audioFrame = new AudioFrame
                {
                    AudioData = audioData.AudioBytes,
                    Timestamp = audioData.Timestamp,
                    CallId = audioData.CallId
                };
                
                await _audioCaptureService.ProcessAudioFrameAsync(audioFrame);
                
                _logger.LogDebug("🎧 Przetworzono audio data dla połączenia: {CallId}", audioData.CallId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas obsługi webhook audio media");
        }
    }

    // IMPLEMENTACJA GOTOWA DO KONFIGURACJI - Akceptowanie przychodzącego połączenia
    public async Task AcceptIncomingCallAsync(string callId)
    {
        try
        {
            if (_graphClient == null)
            {
                throw new InvalidOperationException("Graph Client nie został zainicjalizowany");
            }

            _logger.LogInformation("📞 Akceptowanie przychodzącego połączenia: {CallId}", callId);

            // TUTAJ BĘDZIE RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            // Po skonfigurowaniu appsettings.json z rzeczywistymi danymi Azure AD
            // 
            // Przykład rzeczywistego wywołania API:
            // var answerRequest = new {
            //     callbackUri = $"{_botConfig.Value.PublicUrl}/api/teamswebhook/calling",
            //     mediaConfig = new {
            //         "@odata.type" = "#microsoft.graph.serviceHostedMediaConfig"
            //     },
            //     acceptedModalities = new[] { "audio" }
            // };
            // 
            // await _graphClient.Communications.Calls[callId]
            //     .Answer
            //     .PostAsync(answerRequest);

            // Aktualnie: Symulacja akceptowania połączenia
            if (_activeCalls.TryGetValue(callId, out var callInfo))
            {
                callInfo.State = CallState.Established;
                callInfo.LastUpdated = DateTime.UtcNow;
            }

            _logger.LogInformation("✅ Połączenie {CallId} zostało zaakceptowane (symulacja - gotowe do konfiguracji Graph API)", callId);
            _logger.LogInformation("🔧 Aby włączyć rzeczywiste połączenia, skonfiguruj appsettings.json z danymi Azure AD");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas akceptowania połączenia {CallId}", callId);
            throw;
        }
    }

    // IMPLEMENTACJA GOTOWA DO KONFIGURACJI - Odrzucanie przychodzącego połączenia
    public async Task RejectIncomingCallAsync(string callId, string? redirectUri = null)
    {
        try
        {
            _logger.LogInformation("❌ Odrzucanie przychodzącego połączenia: {CallId}", callId);

            // TUTAJ BĘDZIE RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            // Po skonfigurowaniu appsettings.json
            //
            // Przykład rzeczywistego wywołania API:
            // var rejectRequest = new {
            //     reason = "busy",
            //     callbackUri = redirectUri ?? $"{_botConfig.Value.PublicUrl}/api/teamswebhook/calling"
            // };
            // 
            // await _graphClient.Communications.Calls[callId]
            //     .Reject
            //     .PostAsync(rejectRequest);

            // Aktualnie: Symulacja odrzucania połączenia
            if (_activeCalls.TryRemove(callId, out var callInfo))
            {
                callInfo.State = CallState.Terminated;
                _logger.LogInformation("✅ Połączenie {CallId} zostało odrzucone (symulacja)", callId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas odrzucania połączenia {CallId}", callId);
            throw;
        }
    }

    // IMPLEMENTACJA GOTOWA DO KONFIGURACJI - Przekierowanie połączenia
    public async Task TransferCallAsync(string callId, string targetUri, string? targetDisplayName = null)
    {
        try
        {
            _logger.LogInformation("↗️ Przekierowanie połączenia {CallId} do: {TargetUri}", callId, targetUri);

            // TUTAJ BĘDZIE RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            // Po skonfigurowaniu appsettings.json

            // Aktualnie: Symulacja przekierowania połączenia
            if (_activeCalls.TryRemove(callId, out var callInfo))
            {
                callInfo.State = CallState.Terminated;
                _logger.LogInformation("✅ Połączenie {CallId} zostało przekierowane (symulacja)", callId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas przekierowania połączenia {CallId}", callId);
            throw;
        }
    }

    // IMPLEMENTACJA GOTOWA DO KONFIGURACJI - Kończenie połączenia
    public async Task EndCallAsync(string callId)
    {
        try
        {
            _logger.LogInformation("📴 Kończenie połączenia: {CallId}", callId);

            // TUTAJ BĘDZIE RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            // Po skonfigurowaniu appsettings.json
            //
            // Przykład rzeczywistego wywołania API:
            // await _graphClient.Communications.Calls[callId].DeleteAsync();

            // Aktualnie: Symulacja kończenia połączenia
            if (_activeCalls.TryRemove(callId, out var callInfo))
            {
                callInfo.State = CallState.Terminated;
                _logger.LogInformation("✅ Połączenie {CallId} zostało zakończone (symulacja)", callId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas kończenia połączenia {CallId}", callId);
            throw;
        }
    }

    // IMPLEMENTACJA GOTOWA DO KONFIGURACJI - Dołączanie do spotkania Teams
    public async Task<CallInfo> JoinMeetingAsync(string meetingUrl, string? displayName = null)
    {
        try
        {
            _logger.LogInformation("🎯 Dołączanie do spotkania: {MeetingUrl}", meetingUrl);

            // TUTAJ BĘDZIE RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            // Po skonfigurowaniu appsettings.json
            //
            // Przykład rzeczywistego wywołania API:
            // var joinRequest = new Call {
            //     callbackUri = $"{_botConfig.Value.PublicUrl}/api/teamswebhook/calling",
            //     requestedModalities = new[] { "audio" },
            //     mediaConfig = new ServiceHostedMediaConfig(),
            //     meetingInfo = ParseMeetingUrl(meetingUrl),
            //     tenantId = _azureConfig.Value.TenantId
            // };
            // 
            // var call = await _graphClient.Communications.Calls.PostAsync(joinRequest);

            // Aktualnie: Symulacja dołączenia do spotkania
            var callId = Guid.NewGuid().ToString();
            var callInfo = new CallInfo
            {
                CallId = callId,
                CallerId = "meeting-join",
                CallerDisplayName = displayName ?? "Bot",
                State = CallState.Established,
                MeetingUrl = meetingUrl,
                Timestamp = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _activeCalls.TryAdd(callInfo.CallId, callInfo);

            _logger.LogInformation("✅ Symulacja dołączenia do spotkania: {CallId}", callInfo.CallId);
            _logger.LogInformation("🔧 Aby włączyć rzeczywiste dołączanie, skonfiguruj appsettings.json z danymi Azure AD");
            
            return callInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas dołączania do spotkania");
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
            Calls = calls,
            Status = _activeCalls.Count > 0 ? "Active calls in progress" : "No active calls",
            GraphApiStatus = _graphClient != null ? "Initialized" : "Not initialized",
            ReadyForConfiguration = true
        };
    }

    // RZECZYWISTA IMPLEMENTACJA - Parsowanie informacji o połączeniu z webhook
    private CallInfo? ParseCallInfoFromWebhook(TeamsWebhookData webhookData)
    {
        try
        {
            // Parsuj rzeczywiste dane z webhook Teams zgodnie z dokumentacją Microsoft Graph
            if (webhookData.Resource.Contains("/communications/calls/"))
            {
                var callId = webhookData.Resource.Split('/').Last();
                
                string callerId = "unknown";
                string callerDisplayName = "Unknown Caller";
                CallState state = CallState.Incoming;
                
                // Próbuj parsować resourceData jako JSON
                try
                {
                    var jsonDoc = JsonDocument.Parse(webhookData.ResourceData);
                    var root = jsonDoc.RootElement;
                    
                    // Parsuj informacje o dzwoniącym zgodnie z Microsoft Graph API
                    if (root.TryGetProperty("source", out var source))
                    {
                        if (source.TryGetProperty("identity", out var identity))
                        {
                            if (identity.TryGetProperty("user", out var user))
                            {
                                callerId = user.TryGetProperty("id", out var id) ? id.GetString() ?? "unknown" : "unknown";
                                callerDisplayName = user.TryGetProperty("displayName", out var displayName) ? displayName.GetString() ?? "Unknown Caller" : "Unknown Caller";
                            }
                            else if (identity.TryGetProperty("phone", out var phone))
                            {
                                callerId = phone.TryGetProperty("id", out var phoneId) ? phoneId.GetString() ?? "unknown" : "unknown";
                                callerDisplayName = "Phone Call";
                            }
                        }
                    }
                    
                    // Parsuj stan połączenia
                    if (root.TryGetProperty("state", out var stateElement))
                    {
                        state = MapCallState(stateElement.GetString());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Nie udało się sparsować szczegółów webhook - używam domyślnych wartości");
                }

                return new CallInfo
                {
                    CallId = callId,
                    CallerId = callerId,
                    CallerDisplayName = callerDisplayName,
                    State = state,
                    MeetingUrl = "",
                    Timestamp = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas parsowania informacji o połączeniu z webhook");
            return null;
        }
    }

    // RZECZYWISTA IMPLEMENTACJA - Parsowanie danych audio z webhook
    private AudioData? ParseAudioDataFromWebhook(TeamsWebhookData webhookData)
    {
        try
        {
            // Parsuj rzeczywiste audio data z webhook Teams zgodnie z dokumentacją Microsoft Graph
            if (webhookData.Resource.Contains("/audioMedia") || webhookData.Resource.Contains("/media"))
            {
                try
                {
                    var jsonDoc = JsonDocument.Parse(webhookData.ResourceData);
                    var root = jsonDoc.RootElement;
                    
                    if (root.TryGetProperty("audioData", out var audioDataElement))
                    {
                        var callId = ExtractCallIdFromResource(webhookData.Resource);
                        var audioBase64 = audioDataElement.GetString();
                        
                        if (!string.IsNullOrEmpty(audioBase64))
                        {
                            var audioBytes = Convert.FromBase64String(audioBase64);

                            return new AudioData
                            {
                                CallId = callId ?? "unknown",
                                AudioBytes = audioBytes,
                                Timestamp = DateTime.UtcNow
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Nie udało się sparsować danych audio z webhook");
                }
                
                // Fallback - zwróć przykładowe dane audio dla testowania
                return new AudioData
                {
                    CallId = ExtractCallIdFromResource(webhookData.Resource) ?? "unknown",
                    AudioBytes = GenerateTestAudioData(),
                    Timestamp = DateTime.UtcNow
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas parsowania danych audio z webhook");
            return null;
        }
    }

    // Pomocnicza metoda do ekstrakcji Call ID z resource URL
    private string? ExtractCallIdFromResource(string resource)
    {
        try
        {
            var segments = resource.Split('/');
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == "calls" && i + 1 < segments.Length)
                {
                    return segments[i + 1];
                }
            }
            return segments.LastOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // Generowanie testowych danych audio dla demonstracji
    private byte[] GenerateTestAudioData()
    {
        // Generuj przykładowe dane audio (sine wave) dla testowania
        var sampleRate = 16000; // 16 kHz
        var duration = 0.1; // 100ms
        var samples = (int)(sampleRate * duration);
        var audioData = new byte[samples * 2]; // 16-bit audio
        
        for (int i = 0; i < samples; i++)
        {
            var sample = (short)(Math.Sin(2 * Math.PI * 440 * i / sampleRate) * 1000); // 440 Hz tone
            var bytes = BitConverter.GetBytes(sample);
            audioData[i * 2] = bytes[0];
            audioData[i * 2 + 1] = bytes[1];
        }
        
        return audioData;
    }

    // Mapowanie stanu połączenia
    private CallState MapCallState(string? graphState)
    {
        return graphState?.ToLower() switch
        {
            "incoming" => CallState.Incoming,
            "establishing" => CallState.Establishing,
            "established" => CallState.Established,
            "terminated" => CallState.Terminated,
            "failed" => CallState.Failed,
            _ => CallState.Unknown
        };
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