using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealTimeMediaBot.Models;
using RealTimeMediaBot.Services;
using RealTimeMediaBot.Bots;
using System.Text.Json;

namespace RealTimeMediaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly TeamsBot _teamsBot;
    private readonly IOptions<BotConfiguration> _botConfig;

    public MessagesController(
        ILogger<MessagesController> logger,
        TeamsBot teamsBot,
        IOptions<BotConfiguration> botConfig)
    {
        _logger = logger;
        _teamsBot = teamsBot;
        _botConfig = botConfig;
    }

    [HttpPost] // POST /api/messages - główny endpoint dla wiadomości Teams
    public async Task<IActionResult> HandleMessage()
    {
        try
        {
            _logger.LogInformation("💬 OTRZYMANO WIADOMOŚĆ z Teams!");
            _logger.LogInformation("📍 Endpoint: POST /api/messages");
            _logger.LogInformation("🌐 Remote IP: {RemoteIp}", HttpContext.Connection.RemoteIpAddress);
            _logger.LogInformation("📋 User-Agent: {UserAgent}", Request.Headers.UserAgent.ToString());
            _logger.LogInformation("📋 Content-Type: {ContentType}", Request.ContentType);
            _logger.LogInformation("📏 Content-Length: {ContentLength}", Request.ContentLength);

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogInformation("📄 Message body length: {Length} characters", body.Length);
            _logger.LogInformation("📄 Message body: {Body}", body);

            // Parsuj wiadomość z Teams
            var messageData = ParseTeamsMessage(body);
            
            if (messageData != null)
            {
                // Obsłuż różne typy wiadomości
                var response = await HandleTeamsMessageAsync(messageData);
                
                return Ok(response);
            }

            return BadRequest(new { error = "Nieprawidłowy format wiadomości" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas przetwarzania wiadomości Teams");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera", details = ex.Message });
        }
    }

    [HttpGet] // GET /api/messages - informacje o endpoint
    public IActionResult GetMessagesInfo()
    {
        return Ok(new
        {
            message = "Messages endpoint jest aktywny",
            timestamp = DateTime.UtcNow,
            status = "ready",
            supportedCommands = new[]
            {
                "help - Wyświetla listę dostępnych komend",
                "status - Pokazuje status bota i aktywnych połączeń", 
                "calls - Lista aktywnych połączeń",
                "audio - Informacje o buforze audio"
            },
            endpoint = $"{_botConfig.Value.PublicUrl}/api/messages"
        });
    }

    private TeamsMessageData? ParseTeamsMessage(string body)
    {
        try
        {
            if (string.IsNullOrEmpty(body))
            {
                return null;
            }

            var messageData = JsonSerializer.Deserialize<TeamsMessageData>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return messageData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas parsowania wiadomości Teams");
            return null;
        }
    }

    private async Task<object> HandleTeamsMessageAsync(TeamsMessageData messageData)
    {
        try
        {
            _logger.LogInformation("📨 Przetwarzanie wiadomości Teams:");
            _logger.LogInformation("   - Type: {Type}", messageData.Type);
            _logger.LogInformation("   - Text: {Text}", messageData.Text);
            _logger.LogInformation("   - From: {FromId} ({FromName})", messageData.From?.Id, messageData.From?.Name);
            _logger.LogInformation("   - Channel: {ChannelId}", messageData.ChannelData?.Channel?.Id);

            // Obsłuż różne typy aktywności
            return messageData.Type?.ToLower() switch
            {
                "message" => await HandleTextMessageAsync(messageData),
                "conversationupdate" => HandleConversationUpdateAsync(messageData),
                "membersadded" => HandleMembersAddedAsync(messageData),
                "invoke" => HandleInvokeAsync(messageData),
                _ => new
                {
                    type = "message",
                    text = $"Otrzymano aktywność typu: {messageData.Type}. Użyj 'help' aby zobaczyć dostępne komendy."
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd podczas obsługi wiadomości Teams");
            return new
            {
                type = "message",
                text = "Przepraszam, wystąpił błąd podczas przetwarzania Twojej wiadomości."
            };
        }
    }

    private async Task<object> HandleTextMessageAsync(TeamsMessageData messageData)
    {
        var text = messageData.Text?.Trim().ToLower() ?? "";
        
        _logger.LogInformation("🔍 Analiza komendy: '{Command}'", text);

        return text switch
        {
            "help" or "/help" => new
            {
                type = "message",
                text = "🤖 **Real-Time Media Bot - Dostępne komendy:**\n\n" +
                       "📞 **Połączenia:**\n" +
                       "• `help` - Ta lista komend\n" +
                       "• `status` - Status bota i aktywnych połączeń\n" +
                       "• `calls` - Lista wszystkich aktywnych połączeń\n" +
                       "• `audio` - Informacje o buforze audio\n\n" +
                       "🎯 **Funkcje bota:**\n" +
                       "• Automatyczne odbieranie połączeń Teams\n" +
                       "• Przechwytywanie audio w czasie rzeczywistym\n" +
                       "• Buforowanie i zapis audio do plików WAV\n" +
                       "• Dołączanie do spotkań Teams\n\n" +
                       "🌐 **Endpointy:**\n" +
                       "• Calling: `https://rtmbot.sniezka.com/api/calling`\n" +
                       "• Messages: `https://rtmbot.sniezka.com/api/messages`\n" +
                       "• Health: `https://rtmbot.sniezka.com/health`\n\n" +
                       "💡 **Aby przetestować bota, po prostu do niego zadzwoń!**"
            },
            
            "status" => new
            {
                type = "message",
                text = GetBotStatusMessage()
            },
            
            "calls" => new
            {
                type = "message", 
                text = GetActiveCallsMessage()
            },
            
            "audio" => new
            {
                type = "message",
                text = GetAudioStatusMessage()
            },
            
            _ when text.StartsWith("hello") || text.StartsWith("hi") || text.StartsWith("cześć") => new
            {
                type = "message",
                text = $"Cześć {messageData.From?.Name ?? "Użytkowniku"}! 👋\n\nJestem Real-Time Media Bot. Mogę odbierać połączenia Teams i przechwytywać audio.\n\nNapisz `help` aby zobaczyć wszystkie dostępne komendy."
            },
            
            _ => new
            {
                type = "message",
                text = $"Nie rozumiem komendy: '{messageData.Text}'\n\nNapisz `help` aby zobaczyć dostępne komendy."
            }
        };
    }

    private object HandleConversationUpdateAsync(TeamsMessageData messageData)
    {
        _logger.LogInformation("🔄 Aktualizacja konwersacji");
        return new { type = "message", text = "" }; // Pusta odpowiedź dla aktualizacji konwersacji
    }

    private object HandleMembersAddedAsync(TeamsMessageData messageData)
    {
        _logger.LogInformation("👥 Dodano członków do konwersacji");
        return new
        {
            type = "message",
            text = "Witaj! 🤖 Jestem Real-Time Media Bot.\n\nMogę odbierać połączenia Teams i przechwytywać audio w czasie rzeczywistym.\n\nNapisz `help` aby zobaczyć dostępne komendy."
        };
    }

    private object HandleInvokeAsync(TeamsMessageData messageData)
    {
        _logger.LogInformation("⚡ Otrzymano invoke activity");
        return new { type = "invokeResponse", value = new { status = 200 } };
    }

    private string GetBotStatusMessage()
    {
        try
        {
            var callStatus = _teamsBot.GetCallStatus();
            var statusData = JsonSerializer.Serialize(callStatus, new JsonSerializerOptions { WriteIndented = true });
            
            return "📊 **Status Bota:**\n\n" +
                   "```json\n" + statusData + "\n```\n\n" +
                   "🌐 **Endpointy:**\n" +
                   "• Health: https://rtmbot.sniezka.com/health\n" +
                   "• Calling: https://rtmbot.sniezka.com/api/calling\n" +
                   "• Messages: https://rtmbot.sniezka.com/api/messages";
        }
        catch (Exception ex)
        {
            return $"❌ Błąd podczas pobierania statusu: {ex.Message}";
        }
    }

    private string GetActiveCallsMessage()
    {
        try
        {
            var callStatus = _teamsBot.GetCallStatus();
            var callsData = callStatus.GetType().GetProperty("Calls")?.GetValue(callStatus);
            
            if (callsData is System.Collections.IList calls && calls.Count > 0)
            {
                var callsJson = JsonSerializer.Serialize(calls, new JsonSerializerOptions { WriteIndented = true });
                return $"📞 **Aktywne połączenia ({calls.Count}):**\n\n" +
                       "```json\n" + callsJson + "\n```";
            }
            else
            {
                return "📞 **Brak aktywnych połączeń**\n\nAby przetestować bota, zadzwoń do niego w Teams!";
            }
        }
        catch (Exception ex)
        {
            return $"❌ Błąd podczas pobierania listy połączeń: {ex.Message}";
        }
    }

    private string GetAudioStatusMessage()
    {
        try
        {
            // Informacje o audio będą dodane gdy AudioCaptureService będzie dostępny
            return "🎧 **Status Audio:**\n\n" +
                   "• Buforowanie: ✅ Aktywne\n" +
                   "• Format: 16-bit, 16kHz, Mono\n" +
                   "• Maksymalny bufor: 1000 klatek\n" +
                   "• Zapis do: WAV files\n\n" +
                   "💡 **Aby zobaczyć szczegóły bufora, użyj API:**\n" +
                   "`GET https://rtmbot.sniezka.com/api/calls/audio/buffer-size`";
        }
        catch (Exception ex)
        {
            return $"❌ Błąd podczas pobierania statusu audio: {ex.Message}";
        }
    }
}
