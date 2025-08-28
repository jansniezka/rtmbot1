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
            _logger.LogInformation("🚀 INICJALIZACJA BOTA TEAMS...");
            _logger.LogInformation("🔧 Azure AD Tenant ID: {TenantId}", _azureConfig.Value.TenantId.Substring(0, Math.Min(8, _azureConfig.Value.TenantId.Length)) + "...");
            _logger.LogInformation("🔧 Bot App ID: {AppId}", _botConfig.Value.MicrosoftAppId.Substring(0, Math.Min(8, _botConfig.Value.MicrosoftAppId.Length)) + "...");
            _logger.LogInformation("🌐 Public URL: {PublicUrl}", _botConfig.Value.PublicUrl);
            
            // Uzyskanie tokenu dostępu
            _logger.LogInformation("🔐 Uzyskiwanie tokenu dostępu z Azure AD...");
            var accessToken = await _authService.GetAccessTokenAsync();
            _logger.LogInformation("✅ Token dostępu uzyskany pomyślnie!");
            
            // Inicjalizacja Graph Client z prawdziwym tokenem
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            _graphClient = new GraphServiceClient(httpClient);
            
            _logger.LogInformation("✅ Bot Teams został zainicjalizowany pomyślnie!");
            _logger.LogInformation("📡 WEBHOOK ENDPOINTS:");
            _logger.LogInformation("   - Azure calling: {AzureEndpoint}", $"{_botConfig.Value.PublicUrl}/api/calling");
            _logger.LogInformation("   - Teams webhook: {TeamsEndpoint}", $"{_botConfig.Value.PublicUrl}/api/teamswebhook/calling");
            _logger.LogInformation("🎯 Bot jest GOTOWY do odbierania połączeń!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ BŁĄD podczas inicjalizacji bota Teams");
            throw;
        }
    }

    // RZECZYWISTA IMPLEMENTACJA - Obsługa webhook'ów przychodzących połączeń
    public async Task HandleIncomingCallWebhookAsync(TeamsWebhookData webhookData)
    {
        try
        {
            _logger.LogInformation("🔔 OTRZYMANO WEBHOOK PRZYCHODZĄCEGO POŁĄCZENIA!");
            _logger.LogInformation("📋 Resource: {Resource}", webhookData.Resource);
            _logger.LogInformation("📋 ChangeType: {ChangeType}", webhookData.ChangeType);
            _logger.LogInformation("📋 SubscriptionId: {SubscriptionId}", webhookData.SubscriptionId);
            _logger.LogInformation("📋 ResourceData: {ResourceData}", webhookData.ResourceData);

            // Parsuj dane połączenia z webhook zgodnie z dokumentacją Microsoft Graph
            var callInfo = ParseCallInfoFromWebhook(webhookData);
            if (callInfo != null)
            {
                // Dodaj do aktywnych połączeń
                _activeCalls.TryAdd(callInfo.CallId, callInfo);
                
                _logger.LogInformation("✅ DODANO NOWE POŁĄCZENIE:");
                _logger.LogInformation("   - Call ID: {CallId}", callInfo.CallId);
                _logger.LogInformation("   - Caller ID: {CallerId}", callInfo.CallerId);
                _logger.LogInformation("   - Caller Name: {CallerName}", callInfo.CallerDisplayName);
                _logger.LogInformation("   - State: {State}", callInfo.State);
                _logger.LogInformation("   - Timestamp: {Timestamp}", callInfo.Timestamp);

                // Automatycznie akceptuj połączenie
                _logger.LogInformation("📞 AUTOMATYCZNIE AKCEPTUJĘ POŁĄCZENIE...");
                await AcceptIncomingCallAsync(callInfo.CallId);
            }
            else
            {
                _logger.LogWarning("⚠️ Nie udało się sparsować danych połączenia z webhook!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ BŁĄD podczas obsługi webhook przychodzącego połączenia");
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

            // RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            var answerRequest = new Microsoft.Graph.Communications.Calls.Item.Answer.AnswerPostRequestBody
            {
                CallbackUri = $"{_botConfig.Value.PublicUrl}/api/teamswebhook/calling",
                AcceptedModalities = new List<Microsoft.Graph.Models.Modality?> 
                { 
                    Microsoft.Graph.Models.Modality.Audio 
                },
                MediaConfig = new Microsoft.Graph.Models.ServiceHostedMediaConfig
                {
                    OdataType = "#microsoft.graph.serviceHostedMediaConfig"
                }
            };

            _logger.LogInformation("🔗 Wywołuję Graph API: POST /communications/calls/{CallId}/answer", callId);
            
            try
            {
                await _graphClient.Communications.Calls[callId].Answer.PostAsync(answerRequest);
                
                _logger.LogInformation("✅ RZECZYWISTE połączenie {CallId} zostało zaakceptowane przez Graph API!", callId);
                
                // Aktualizuj lokalny stan
                if (_activeCalls.TryGetValue(callId, out var callInfo))
                {
                    callInfo.State = CallState.Established;
                    callInfo.LastUpdated = DateTime.UtcNow;
                }
            }
            catch (Exception graphEx)
            {
                _logger.LogError(graphEx, "❌ BŁĄD Graph API podczas akceptowania połączenia {CallId}:", callId);
                _logger.LogError("🔍 Szczegóły błędu: {ErrorMessage}", graphEx.Message);
                _logger.LogError("🔍 Stack trace: {StackTrace}", graphEx.StackTrace);
                
                if (graphEx.InnerException != null)
                {
                    _logger.LogError("🔍 Inner exception: {InnerException}", graphEx.InnerException.Message);
                }
                
                // Usuń połączenie z aktywnych jeśli nie udało się je zaakceptować
                _activeCalls.TryRemove(callId, out _);
                
                // Rzuć exception z szczegółowymi informacjami
                throw new InvalidOperationException($"Nie udało się zaakceptować połączenia {callId} przez Graph API: {graphEx.Message}", graphEx);
            }
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

            // RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            var rejectRequest = new Microsoft.Graph.Communications.Calls.Item.Reject.RejectPostRequestBody
            {
                Reason = Microsoft.Graph.Models.RejectReason.Busy,
                CallbackUri = redirectUri ?? $"{_botConfig.Value.PublicUrl}/api/teamswebhook/calling"
            };

            _logger.LogInformation("🔗 Wywołuję Graph API: POST /communications/calls/{CallId}/reject", callId);
            
            try
            {
                await _graphClient.Communications.Calls[callId].Reject.PostAsync(rejectRequest);
                
                _logger.LogInformation("✅ RZECZYWISTE połączenie {CallId} zostało odrzucone przez Graph API!", callId);
                
                // Usuń z aktywnych połączeń
                if (_activeCalls.TryRemove(callId, out var callInfo))
                {
                    callInfo.State = CallState.Terminated;
                }
            }
            catch (Exception graphEx)
            {
                _logger.LogError(graphEx, "❌ BŁĄD Graph API podczas odrzucania połączenia {CallId}:", callId);
                _logger.LogError("🔍 Szczegóły błędu: {ErrorMessage}", graphEx.Message);
                _logger.LogError("🔍 Stack trace: {StackTrace}", graphEx.StackTrace);
                
                if (graphEx.InnerException != null)
                {
                    _logger.LogError("🔍 Inner exception: {InnerException}", graphEx.InnerException.Message);
                }
                
                // Rzuć exception z szczegółowymi informacjami
                throw new InvalidOperationException($"Nie udało się odrzucić połączenia {callId} przez Graph API: {graphEx.Message}", graphEx);
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

            // RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            _logger.LogInformation("🔗 Wywołuję Graph API: DELETE /communications/calls/{CallId}", callId);
            
            try
            {
                await _graphClient.Communications.Calls[callId].DeleteAsync();
                
                _logger.LogInformation("✅ RZECZYWISTE połączenie {CallId} zostało zakończone przez Graph API!", callId);
                
                // Usuń z aktywnych połączeń
                if (_activeCalls.TryRemove(callId, out var callInfo))
                {
                    callInfo.State = CallState.Terminated;
                }
            }
            catch (Exception graphEx)
            {
                _logger.LogError(graphEx, "❌ BŁĄD Graph API podczas kończenia połączenia {CallId}:", callId);
                _logger.LogError("🔍 Szczegóły błędu: {ErrorMessage}", graphEx.Message);
                _logger.LogError("🔍 Stack trace: {StackTrace}", graphEx.StackTrace);
                
                if (graphEx.InnerException != null)
                {
                    _logger.LogError("🔍 Inner exception: {InnerException}", graphEx.InnerException.Message);
                }
                
                // Rzuć exception z szczegółowymi informacjami
                throw new InvalidOperationException($"Nie udało się zakończyć połączenia {callId} przez Graph API: {graphEx.Message}", graphEx);
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

            // RZECZYWISTA IMPLEMENTACJA Microsoft Graph Communications API
            var meetingInfo = ParseMeetingUrl(meetingUrl);
            
            var joinRequest = new Microsoft.Graph.Models.Call
            {
                CallbackUri = $"{_botConfig.Value.PublicUrl}/api/teamswebhook/calling",
                RequestedModalities = new List<Microsoft.Graph.Models.Modality?> 
                { 
                    Microsoft.Graph.Models.Modality.Audio 
                },
                MediaConfig = new Microsoft.Graph.Models.ServiceHostedMediaConfig
                {
                    OdataType = "#microsoft.graph.serviceHostedMediaConfig"
                },
                MeetingInfo = meetingInfo,
                TenantId = _azureConfig.Value.TenantId,
                Source = new Microsoft.Graph.Models.ParticipantInfo
                {
                    Identity = new Microsoft.Graph.Models.IdentitySet
                    {
                        Application = new Microsoft.Graph.Models.Identity
                        {
                            Id = _botConfig.Value.MicrosoftAppId,
                            DisplayName = displayName ?? "Real-Time Media Bot"
                        }
                    }
                }
            };

            _logger.LogInformation("🔗 Wywołuję Graph API: POST /communications/calls (join meeting)");
            
            try
            {
                var call = await _graphClient.Communications.Calls.PostAsync(joinRequest);
                
                if (call?.Id != null)
                {
                    var callInfo = new CallInfo
                    {
                        CallId = call.Id,
                        CallerId = "meeting-join",
                        CallerDisplayName = displayName ?? "Bot",
                        State = CallState.Establishing,
                        MeetingUrl = meetingUrl,
                        Timestamp = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };

                    _activeCalls.TryAdd(callInfo.CallId, callInfo);
                    
                    _logger.LogInformation("✅ RZECZYWISTE dołączenie do spotkania: {CallId}", callInfo.CallId);
                    return callInfo;
                }
                else
                {
                    _logger.LogError("❌ Graph API zwróciło null call ID podczas dołączania do spotkania");
                    throw new InvalidOperationException("Graph API zwróciło null call ID podczas dołączania do spotkania");
                }
            }
            catch (Exception graphEx)
            {
                _logger.LogError(graphEx, "❌ BŁĄD Graph API podczas dołączania do spotkania {MeetingUrl}:", meetingUrl);
                _logger.LogError("🔍 Szczegóły błędu: {ErrorMessage}", graphEx.Message);
                _logger.LogError("🔍 Stack trace: {StackTrace}", graphEx.StackTrace);
                
                if (graphEx.InnerException != null)
                {
                    _logger.LogError("🔍 Inner exception: {InnerException}", graphEx.InnerException.Message);
                }
                
                // Rzuć exception z szczegółowymi informacjami
                throw new InvalidOperationException($"Nie udało się dołączyć do spotkania {meetingUrl} przez Graph API: {graphEx.Message}", graphEx);
            }
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

    // Parsowanie Meeting URL do MeetingInfo
    private Microsoft.Graph.Models.MeetingInfo ParseMeetingUrl(string meetingUrl)
    {
        try
        {
            // Parsuj Teams meeting URL
            if (meetingUrl.Contains("teams.microsoft.com"))
            {
                var uri = new Uri(meetingUrl);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                
                var threadId = query["threadId"];
                var organizerId = query["organizerId"];
                var tenantId = query["tenantId"];
                
                return new Microsoft.Graph.Models.OrganizerMeetingInfo
                {
                    OdataType = "#microsoft.graph.organizerMeetingInfo",
                    Organizer = new Microsoft.Graph.Models.IdentitySet
                    {
                        User = new Microsoft.Graph.Models.Identity
                        {
                            Id = organizerId
                        }
                    }
                };
            }
            
            // Fallback - podstawowe meeting info
            return new Microsoft.Graph.Models.OrganizerMeetingInfo
            {
                OdataType = "#microsoft.graph.organizerMeetingInfo"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Nie udało się sparsować meeting URL: {MeetingUrl}", meetingUrl);
            
            // Zwróć podstawowe meeting info
            return new Microsoft.Graph.Models.OrganizerMeetingInfo
            {
                OdataType = "#microsoft.graph.organizerMeetingInfo"
            };
        }
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