using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealTimeMediaBot.Services;
using RealTimeMediaBot.Bots;
using RealTimeMediaBot.Models;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja usług
builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("Bot"));
builder.Services.Configure<AzureAdConfiguration>(builder.Configuration.GetSection("AzureAd"));
builder.Services.Configure<GraphConfiguration>(builder.Configuration.GetSection("Graph"));

// Rejestracja usług
builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<IGraphService, GraphService>();
builder.Services.AddSingleton<IAudioCaptureService, AudioCaptureService>();
builder.Services.AddSingleton<TeamsBot>();

// Hosted Service dla bota
builder.Services.AddHostedService<BotHostedService>();

// Dodaj kontrolery
builder.Services.AddControllers();

// Konfiguracja logowania
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Konfiguracja middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

// Mapowanie kontrolerów
app.MapControllers();

// Endpoint statusu
app.MapGet("/", () => "Real-Time Media Bot dla Teams jest uruchomiony na https://rtmbot.sniezka.com!");

app.MapGet("/health", () => new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    endpoint = "https://rtmbot.sniezka.com/api/calling",
    environment = app.Environment.EnvironmentName
});

// Endpoint zgodny z konfiguracją Azure Portal
app.MapGet("/api/calling", () => new { 
    message = "Calling endpoint jest aktywny",
    timestamp = DateTime.UtcNow,
    status = "ready"
});

// Uruchom aplikację
await app.RunAsync();
