using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealTimeMediaBot.Models;
using RealTimeMediaBot.Services;
using RealTimeMediaBot.Bots;

namespace RealTimeMediaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CallsController : ControllerBase
{
    private readonly ILogger<CallsController> _logger;
    private readonly TeamsBot _teamsBot;
    private readonly IAudioCaptureService _audioCaptureService;

    public CallsController(
        ILogger<CallsController> logger,
        TeamsBot teamsBot,
        IAudioCaptureService audioCaptureService)
    {
        _logger = logger;
        _teamsBot = teamsBot;
        _audioCaptureService = audioCaptureService;
    }

    [HttpPost("incoming")]
    public async Task<IActionResult> HandleIncomingCall([FromBody] IncomingCall incomingCall)
    {
        try
        {
            _logger.LogInformation("Otrzymano przychodzące połączenie od: {CallerDisplayName} ({CallerId})", 
                incomingCall.CallerDisplayName, incomingCall.CallerId);

            // Automatycznie akceptuj połączenie
            await _teamsBot.AcceptIncomingCallAsync(incomingCall.Id);

            return Ok(new { 
                message = "Połączenie zostało zaakceptowane", 
                callId = incomingCall.Id,
                endpoint = "https://rtmbot.sniezka.com/api/calling"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas obsługi przychodzącego połączenia");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpPost("{callId}/answer")]
    public async Task<IActionResult> AnswerCall(string callId, [FromBody] CallAnswerRequest request)
    {
        try
        {
            _logger.LogInformation("Odpowiadanie na połączenie: {CallId}", callId);

            if (request.Accept)
            {
                await _teamsBot.AcceptIncomingCallAsync(callId);
                return Ok(new { message = "Połączenie zostało zaakceptowane" });
            }
            else
            {
                await _teamsBot.RejectIncomingCallAsync(callId, request.RedirectUri);
                return Ok(new { message = "Połączenie zostało odrzucone" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas odpowiadania na połączenie");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpPost("{callId}/transfer")]
    public async Task<IActionResult> TransferCall(string callId, [FromBody] CallTransferRequest request)
    {
        try
        {
            _logger.LogInformation("Przekierowanie połączenia {CallId} do: {TargetUri}", callId, request.TargetUri);

            await _teamsBot.TransferCallAsync(callId, request.TargetUri, request.TargetDisplayName);
            return Ok(new { message = "Połączenie zostało przekierowane" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przekierowania połączenia");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpDelete("{callId}")]
    public async Task<IActionResult> EndCall(string callId)
    {
        try
        {
            _logger.LogInformation("Kończenie połączenia: {CallId}", callId);

            await _teamsBot.EndCallAsync(callId);
            return Ok(new { message = "Połączenie zostało zakończone" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas kończenia połączenia");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpGet("status")]
    public IActionResult GetCallStatus()
    {
        try
        {
            var status = _teamsBot.GetCallStatus();
            return Ok(new { 
                status,
                endpoint = "https://rtmbot.sniezka.com/api/calling",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania statusu połączeń");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpGet("audio/buffer-size")]
    public IActionResult GetAudioBufferSize()
    {
        try
        {
            var bufferSize = _audioCaptureService.GetBufferSize();
            return Ok(new { bufferSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania rozmiaru bufora audio");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpPost("audio/save")]
    public async Task<IActionResult> SaveAudioBuffer([FromBody] SaveAudioRequest request)
    {
        try
        {
            var filePath = request.FilePath ?? $"audio_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
            await _audioCaptureService.SaveAudioBufferAsync(filePath);
            
            return Ok(new { message = "Audio zostało zapisane", filePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zapisywania audio");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    [HttpDelete("audio/clear")]
    public IActionResult ClearAudioBuffer()
    {
        try
        {
            _audioCaptureService.ClearBuffer();
            return Ok(new { message = "Bufor audio został wyczyszczony" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas czyszczenia bufora audio");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }
}

public class SaveAudioRequest
{
    public string? FilePath { get; set; }
}
