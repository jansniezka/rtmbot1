using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealTimeMediaBot.Models;
using RealTimeMediaBot.Services;

namespace RealTimeMediaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IOptions<AzureAdConfiguration> _azureConfig;
    private readonly IAuthenticationService _authService;

    public AuthController(
        ILogger<AuthController> logger,
        IOptions<AzureAdConfiguration> azureConfig,
        IAuthenticationService authService)
    {
        _logger = logger;
        _azureConfig = azureConfig;
        _authService = authService;
    }

    // Endpoint callback dla Azure AD
    [HttpGet("callback")]
    public async Task<IActionResult> AuthCallback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            _logger.LogInformation("Otrzymano callback uwierzytelniania z Azure AD");

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { error = "Brak kodu autoryzacji" });
            }

            // Tutaj możesz zaimplementować logikę wymiany kodu na token
            // Na razie zwracamy informację o pomyślnym callback
            return Ok(new { 
                message = "Callback uwierzytelniania został przetworzony",
                code = code.Substring(0, Math.Min(code.Length, 10)) + "...",
                state = state,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przetwarzania callback uwierzytelniania");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    // Endpoint do sprawdzenia statusu uwierzytelniania
    [HttpGet("status")]
    public async Task<IActionResult> GetAuthStatus()
    {
        try
        {
            _logger.LogInformation("Sprawdzanie statusu uwierzytelniania");

            // Sprawdź czy można uzyskać token
            var hasToken = await _authService.HasValidTokenAsync();
            
            return Ok(new { 
                authenticated = hasToken,
                timestamp = DateTime.UtcNow,
                azureAdConfig = new {
                    tenantId = _azureConfig.Value.TenantId,
                    clientId = _azureConfig.Value.ClientId,
                    hasClientSecret = !string.IsNullOrEmpty(_azureConfig.Value.ClientSecret)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas sprawdzania statusu uwierzytelniania");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }

    // Endpoint do wymuszenia odświeżenia tokenu
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            _logger.LogInformation("Wymuszenie odświeżenia tokenu");

            // Wymuś odświeżenie tokenu
            var newToken = await _authService.GetAccessTokenAsync(forceRefresh: true);
            
            return Ok(new { 
                message = "Token został odświeżony",
                hasNewToken = !string.IsNullOrEmpty(newToken),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas odświeżania tokenu");
            return StatusCode(500, new { error = "Błąd wewnętrzny serwera" });
        }
    }
}
