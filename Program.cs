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

// Dodaj middleware logowania wszystkich requestów
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("🌐 HTTP Request: {Method} {Path} z {RemoteIp}", 
        context.Request.Method, 
        context.Request.Path, 
        context.Connection.RemoteIpAddress);
    
    if (context.Request.Headers.Count > 0)
    {
        logger.LogDebug("📋 Headers: {Headers}", 
            string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}")));
    }
    
    await next();
    
    logger.LogInformation("📤 HTTP Response: {StatusCode} dla {Method} {Path}", 
        context.Response.StatusCode, 
        context.Request.Method, 
        context.Request.Path);
});

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
    status = "ready",
    features = new[] {
        "Odbieranie połączeń przychodzących",
        "Dołączanie do spotkań Teams",
        "Przechwytywanie audio w czasie rzeczywistym",
        "Zarządzanie połączeniami"
    }
});

// POST endpoint dla Azure Bot Service - przekierowanie HTTP 307
app.MapPost("/api/calling", async (HttpContext context) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("🎯 OTRZYMANO POST REQUEST na /api/calling!");
    logger.LogInformation("🌐 Remote IP: {RemoteIp}", context.Connection.RemoteIpAddress);
    logger.LogInformation("📋 User-Agent: {UserAgent}", context.Request.Headers.UserAgent.ToString());
    logger.LogInformation("📋 Content-Type: {ContentType}", context.Request.ContentType);
    logger.LogInformation("📏 Content-Length: {ContentLength}", context.Request.ContentLength);
    
    // Przeczytaj body przed przekierowaniem
    context.Request.EnableBuffering();
    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;
    
    logger.LogInformation("📄 Request body length: {Length} characters", body.Length);
    logger.LogInformation("📄 Request body: {Body}", body);
    logger.LogInformation("🔄 Przekierowuję na /api/teamswebhook/calling...");
    
    // Przekieruj POST /api/calling -> /api/teamswebhook/calling
    return Results.Redirect("/api/teamswebhook/calling", permanent: false, preserveMethod: true);
});

// Uruchom aplikację
await app.RunAsync();
