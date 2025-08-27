using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealTimeMediaBot.Services;

namespace RealTimeMediaBot.Bots;

public class BotHostedService : BackgroundService
{
    private readonly ILogger<BotHostedService> _logger;
    private readonly TeamsBot _teamsBot;
    private readonly IAudioCaptureService _audioCaptureService;
    private readonly IGraphService _graphService;

    public BotHostedService(
        ILogger<BotHostedService> logger,
        TeamsBot teamsBot,
        IAudioCaptureService audioCaptureService,
        IGraphService graphService)
    {
        _logger = logger;
        _teamsBot = teamsBot;
        _audioCaptureService = audioCaptureService;
        _graphService = graphService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Uruchamianie Bot Hosted Service...");

            // Inicjalizacja bota
            await _teamsBot.InitializeAsync();

            _logger.LogInformation("Bot został zainicjalizowany. Nasłuchuje na połączenia...");

            // Czekanie na anulowanie
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                
                // Sprawdzanie statusu bufora audio
                var bufferSize = _audioCaptureService.GetBufferSize();
                if (bufferSize > 0)
                {
                    _logger.LogInformation("Bufor audio zawiera {BufferSize} klatek", bufferSize);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Bot Hosted Service został anulowany");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd w Bot Hosted Service");
        }
        finally
        {
            _logger.LogInformation("Zatrzymywanie Bot Hosted Service...");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Zatrzymywanie bota...");
            
            // Zapisanie pozostałego audio w buforze
            var outputPath = Path.Combine(Environment.CurrentDirectory, "final_audio.wav");
            await _audioCaptureService.SaveAudioBufferAsync(outputPath);
            
            _logger.LogInformation("Bot został zatrzymany pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zatrzymywania bota");
        }
        
        await base.StopAsync(cancellationToken);
    }
}
