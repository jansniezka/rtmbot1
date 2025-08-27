using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using RealTimeMediaBot.Models;

namespace RealTimeMediaBot.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IOptions<AzureAdConfiguration> _azureConfig;
    private readonly IConfidentialClientApplication _app;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IOptions<AzureAdConfiguration> azureConfig)
    {
        _logger = logger;
        _azureConfig = azureConfig;

        _app = ConfidentialClientApplicationBuilder
            .Create(_azureConfig.Value.ClientId)
            .WithTenantId(_azureConfig.Value.TenantId)
            .WithClientSecret(_azureConfig.Value.ClientSecret)
            .Build();
    }

    public async Task<string> GetAccessTokenAsync()
    {
        try
        {
            _logger.LogDebug("Uzyskiwanie tokenu dostępu...");

            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var result = await _app.AcquireTokenForClient(scopes).ExecuteAsync();

            _logger.LogDebug("Token dostępu został uzyskany pomyślnie");
            return result.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas uzyskiwania tokenu dostępu");
            throw;
        }
    }
}
