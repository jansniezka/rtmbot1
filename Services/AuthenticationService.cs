using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using RealTimeMediaBot.Models;

namespace RealTimeMediaBot.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IOptions<AzureAdConfiguration> _azureConfig;
    private IConfidentialClientApplication? _app;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IOptions<AzureAdConfiguration> azureConfig)
    {
        _logger = logger;
        _azureConfig = azureConfig;
    }

    public async Task<string> GetAccessTokenAsync(bool forceRefresh = false)
    {
        try
        {
            // Sprawdź czy token jest wciąż ważny (z 5-minutowym marginesem)
            if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            {
                _logger.LogDebug("Używanie zapamiętanego tokenu");
                return _cachedToken;
            }

            _logger.LogInformation("Uzyskiwanie nowego tokenu dostępu z Azure AD");

            // Inicjalizacja aplikacji MSAL
            if (_app == null)
            {
                _app = ConfidentialClientApplicationBuilder
                    .Create(_azureConfig.Value.ClientId)
                    .WithClientSecret(_azureConfig.Value.ClientSecret)
                    .WithAuthority(new Uri($"{_azureConfig.Value.Instance}{_azureConfig.Value.TenantId}"))
                    .Build();
            }

            // Zakresy dla Microsoft Graph
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // Uzyskanie tokenu
            var result = await _app.AcquireTokenForClient(scopes).ExecuteAsync();

            if (result != null && !string.IsNullOrEmpty(result.AccessToken))
            {
                _cachedToken = result.AccessToken;
                _tokenExpiry = result.ExpiresOn.DateTime;
                
                _logger.LogInformation("Token dostępu został pomyślnie uzyskany. Ważny do: {Expiry}", _tokenExpiry);
                
                return result.AccessToken;
            }
            else
            {
                throw new InvalidOperationException("Nie udało się uzyskać tokenu dostępu");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas uzyskiwania tokenu dostępu");
            throw;
        }
    }

    public async Task<bool> HasValidTokenAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_cachedToken))
            {
                return false;
            }

            // Sprawdź czy token nie wygasł (z 5-minutowym marginesem)
            if (DateTime.UtcNow >= _tokenExpiry.AddMinutes(-5))
            {
                return false;
            }

            // Sprawdź czy można uzyskać nowy token
            var token = await GetAccessTokenAsync();
            return !string.IsNullOrEmpty(token);
        }
        catch
        {
            return false;
        }
    }
}
