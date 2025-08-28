using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealTimeMediaBot.Bots;
using RealTimeMediaBot.Models;

namespace RealTimeMediaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeetingsController : ControllerBase
{
    private readonly ILogger<MeetingsController> _logger;
    private readonly TeamsBot _teamsBot;

    public MeetingsController(
        ILogger<MeetingsController> logger,
        TeamsBot teamsBot)
    {
        _logger = logger;
        _teamsBot = teamsBot;
    }

    // Dołączanie do spotkania Teams
    [HttpPost("join")]
    public async Task<IActionResult> JoinMeeting([FromBody] JoinMeetingRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.MeetingUrl))
            {
                return BadRequest(new { error = "URL spotkania jest wymagany" });
            }

            _logger.LogInformation("Próba dołączenia do spotkania: {MeetingUrl}", request.MeetingUrl);

            var callInfo = await _teamsBot.JoinMeetingAsync(request.MeetingUrl, request.DisplayName);

            return Ok(new { 
                message = "Pomyślnie dołączono do spotkania",
                callInfo = new
                {
                    callId = callInfo.CallId,
                    meetingUrl = callInfo.MeetingUrl,
                    state = callInfo.State,
                    timestamp = callInfo.Timestamp
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas dołączania do spotkania");
            return StatusCode(500, new { error = "Błąd podczas dołączania do spotkania", details = ex.Message });
        }
    }

    // Sprawdzenie statusu spotkania
    [HttpGet("status/{callId}")]
    public IActionResult GetMeetingStatus(string callId)
    {
        try
        {
            var callStatus = _teamsBot.GetCallStatus();
            return Ok(callStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania statusu spotkania");
            return StatusCode(500, new { error = "Błąd podczas pobierania statusu spotkania" });
        }
    }

    // Opuszczenie spotkania
    [HttpPost("leave/{callId}")]
    public async Task<IActionResult> LeaveMeeting(string callId)
    {
        try
        {
            _logger.LogInformation("Opuszczanie spotkania: {CallId}", callId);

            await _teamsBot.EndCallAsync(callId);

            return Ok(new { message = "Pomyślnie opuszczono spotkanie" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas opuszczania spotkania");
            return StatusCode(500, new { error = "Błąd podczas opuszczania spotkania" });
        }
    }

    // Lista aktywnych spotkań
    [HttpGet("active")]
    public IActionResult GetActiveMeetings()
    {
        try
        {
            var callStatus = _teamsBot.GetCallStatus();
            return Ok(callStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania listy aktywnych spotkań");
            return StatusCode(500, new { error = "Błąd podczas pobierania listy aktywnych spotkań" });
        }
    }
}

public class JoinMeetingRequest
{
    public string MeetingUrl { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
