using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealTimeMediaBot.Models;
using RealTimeMediaBot.Services;
using System.Text.Json;
using RealTimeMediaBot.Bots;

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

    [HttpPost("calling")] // Main calling endpoint as per Azure Portal config
    public async Task<IActionResult> HandleCallingWebhook()
    {
        try
        {
            _logger.LogInformation("🔔 OTRZYMANO WEBHOOK CALLING z Teams!");
            _logger.LogInformation("📍 Endpoint: POST /api/teamswebhook/calling");
            _logger.LogInformation("🌐 Remote IP: {RemoteIp}", HttpContext.Connection.RemoteIpAddress);
            _logger.LogInformation("📋 User-Agent: {UserAgent}", Request.Headers.UserAgent.ToString());
            _logger.LogInformation("📋 Content-Type: {ContentType}", Request.ContentType);
            _logger.LogInformation("📏 Content-Length: {ContentLength}", Request.ContentLength);

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogInformation("📄 Webhook body length: {Length} characters", body.Length);
            _logger.LogInformation("📄 Webhook body: {Body}", body);

            var webhookData = ParseTeamsWebhook(body);

            if (webhookData != null)
            {
                // Określ typ webhook'a na podstawie resource i changeType
                if (webhookData.Resource.Contains("/communications/calls/"))
                {
                    if (webhookData.ChangeType == "created")
                    {
                        await _teamsBot.HandleIncomingCallWebhookAsync(webhookData);
                    }
                    else if (webhookData.ChangeType == "updated")
                    {
                        await _teamsBot.HandleCallUpdatedWebhookAsync(webhookData);
                    }
                }
                else if (webhookData.Resource.Contains("/audioMedia"))
                {
                    await _teamsBot.HandleAudioMediaWebhookAsync(webhookData);
                }

                return Ok(new { 
                    message = "Calling webhook został przetworzony", 
                    endpoint = "https://rtmbot.sniezka.com/api/calling",
                    webhookType = DetermineWebhookType(webhookData),
                    resource = webhookData.Resource,
                    changeType = webhookData.ChangeType
                });
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
            _logger.LogInformation("Otrzymano webhook przychodzącego połączenia");

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogDebug("Incoming call webhook body: {Body}", body);

            var webhookData = ParseTeamsWebhook(body);

            if (webhookData != null)
            {
                await _teamsBot.HandleIncomingCallWebhookAsync(webhookData);

                return Ok(new { 
                    message = "Webhook przychodzącego połączenia został przetworzony",
                    endpoint = "https://rtmbot.sniezka.com/api/teamswebhook/incoming-call"
                });
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
            _logger.LogInformation("Otrzymano webhook aktualizacji połączenia");

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogDebug("Call updated webhook body: {Body}", body);

            var webhookData = ParseTeamsWebhook(body);

            if (webhookData != null)
            {
                await _teamsBot.HandleCallUpdatedWebhookAsync(webhookData);

                return Ok(new { 
                    message = "Webhook aktualizacji połączenia został przetworzony",
                    endpoint = "https://rtmbot.sniezka.com/api/teamswebhook/call-updated"
                });
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
            _logger.LogInformation("Otrzymano webhook audio media");

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogDebug("Audio media webhook body: {Body}", body);

            var webhookData = ParseTeamsWebhook(body);

            if (webhookData != null)
            {
                await _teamsBot.HandleAudioMediaWebhookAsync(webhookData);

                return Ok(new { 
                    message = "Webhook audio media został przetworzony",
                    endpoint = "https://rtmbot.sniezka.com/api/teamswebhook/audio-media"
                });
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
            if (string.IsNullOrEmpty(body))
            {
                return null;
            }

            var webhookData = JsonSerializer.Deserialize<TeamsWebhookData>(body);
            return webhookData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas parsowania webhook Teams");
            return null;
        }
    }

    private string DetermineWebhookType(TeamsWebhookData webhookData)
    {
        if (webhookData.Resource.Contains("/communications/calls/"))
        {
            return webhookData.ChangeType switch
            {
                "created" => "Incoming Call",
                "updated" => "Call Updated",
                "deleted" => "Call Deleted",
                _ => "Call Event"
            };
        }
        else if (webhookData.Resource.Contains("/audioMedia"))
        {
            return "Audio Media";
        }
        
        return "Unknown";
    }
}
