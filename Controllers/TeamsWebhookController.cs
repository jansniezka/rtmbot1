using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealTimeMediaBot.Models;
using RealTimeMediaBot.Services;
using RealTimeMediaBot.Bots;
using System.Text.Json;

namespace RealTimeMediaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsWebhookController : ControllerBase
{
    private readonly ILogger<TeamsWebhookController> _logger;
    private readonly TeamsBot _teamsBot;

    public TeamsWebhookController(
        ILogger<TeamsWebhookController> logger,
        TeamsBot teamsBot)
    {
        _logger = logger;
        _teamsBot = teamsBot;
    }

    // Główny endpoint calling zgodny z konfiguracją Azure Portal
    [HttpPost("calling")]
    public async Task<IActionResult> HandleCallingWebhook()
    {
        try
        {
            _logger.LogInformation("Otrzymano webhook calling z Teams na publicznym endpoincie");

            // Odczytaj body request
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            
            _logger.LogDebug("Calling webhook body: {Body}", body);

            // Parsuj webhook data (Teams używa specyficznego formatu)
            var webhookData = ParseTeamsWebhook(body);
            
            if (webhookData != null)
            {
                // Przekaż do TeamsBot do obsługi
                await _teamsBot.HandleIncomingCallWebhookAsync(webhookData);
                
                return Ok(new { message = "Calling webhook został przetworzony", endpoint = "https://rtmbot.sniezka.com/api/calling" });
            }

            return BadRequest(new { error = "Nieprawidłowy format webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przetwarzania calling webhook");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpPost("incoming-call")]
    public async Task<IActionResult> HandleIncomingCallWebhook()
    {
        try
        {
            _logger.LogInformation("Otrzymano webhook przychodzącego połączenia z Teams");

            // Odczytaj body request
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            
            _logger.LogDebug("Webhook body: {Body}", body);

            // Parsuj webhook data (Teams używa specyficznego formatu)
            var webhookData = ParseTeamsWebhook(body);
            
            if (webhookData != null)
            {
                // Przekaż do TeamsBot do obsługi
                await _teamsBot.HandleIncomingCallWebhookAsync(webhookData);
                
                return Ok(new { message = "Webhook został przetworzony" });
            }

            return BadRequest(new { error = "Nieprawidłowy format webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przetwarzania webhook przychodzącego połączenia");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpPost("call-updated")]
    public async Task<IActionResult> HandleCallUpdatedWebhook()
    {
        try
        {
            _logger.LogInformation("Otrzymano webhook aktualizacji połączenia z Teams");

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            
            _logger.LogDebug("Call updated webhook body: {Body}", body);

            var webhookData = ParseTeamsWebhook(body);
            
            if (webhookData != null)
            {
                await _teamsBot.HandleCallUpdatedWebhookAsync(webhookData);
                return Ok(new { message = "Webhook aktualizacji połączenia został przetworzony" });
            }

            return BadRequest(new { error = "Nieprawidłowy format webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przetwarzania webhook aktualizacji połączenia");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpPost("audio-media")]
    public async Task<IActionResult> HandleAudioMediaWebhook()
    {
        try
        {
            _logger.LogInformation("Otrzymano webhook audio media z Teams");

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            
            _logger.LogDebug("Audio media webhook body: {Body}", body);

            var webhookData = ParseTeamsWebhook(body);
            
            if (webhookData != null)
            {
                await _teamsBot.HandleAudioMediaWebhookAsync(webhookData);
                return Ok(new { message = "Webhook audio media został przetworzony" });
            }

            return BadRequest(new { error = "Nieprawidłowy format webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przetwarzania webhook audio media");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    private TeamsWebhookData? ParseTeamsWebhook(string body)
    {
        try
        {
            // Teams webhook może mieć różne formaty, więc próbujemy parsować elastycznie
            var jsonDoc = JsonDocument.Parse(body);
            var root = jsonDoc.RootElement;

            // Sprawdź czy to standardowy Teams webhook
            if (root.TryGetProperty("value", out var valueArray) && 
                valueArray.ValueKind == JsonValueKind.Array && 
                valueArray.GetArrayLength() > 0)
            {
                var firstValue = valueArray[0];
                
                return new TeamsWebhookData
                {
                    Resource = firstValue.GetProperty("resource").GetString() ?? "",
                    ChangeType = firstValue.GetProperty("changeType").GetString() ?? "",
                    ResourceData = firstValue.GetProperty("resourceData").GetRawText(),
                    SubscriptionId = firstValue.GetProperty("subscriptionId").GetString() ?? "",
                    SubscriptionExpirationDateTime = firstValue.GetProperty("subscriptionExpirationDateTime").GetString() ?? ""
                };
            }

            // Sprawdź czy to prostszy format
            if (root.TryGetProperty("resource", out var resource))
            {
                return new TeamsWebhookData
                {
                    Resource = resource.GetString() ?? "",
                    ChangeType = root.TryGetProperty("changeType", out var changeType) ? changeType.GetString() ?? "" : "",
                    ResourceData = body,
                    SubscriptionId = "",
                    SubscriptionExpirationDateTime = ""
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas parsowania webhook Teams");
            return null;
        }
    }
}
