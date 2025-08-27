# Real-Time Media Bot dla Microsoft Teams

Bot Teams napisany w C#, który komunikuje się z MS Teams przez Microsoft Graph Communications API i przechwytuje audio w czasie rzeczywistym ze spotkań.

## 🚨 WAŻNE: Konfiguracja wymagana przed uruchomieniem!

**Przed uruchomieniem bota musisz skonfigurować Azure AD!** Zobacz plik [KONFIGURACJA.md](KONFIGURACJA.md) ze szczegółowymi instrukcjami.

## 🌐 **Publiczny Endpoint**

Bot jest skonfigurowany do działania na publicznym endpoincie:
- **URL**: `https://rtmbot.sniezka.com`
- **Calling Endpoint**: `https://rtmbot.sniezka.com/api/calling`
- **Port**: 443 (HTTPS)

## Funkcjonalności

- 🔐 Uwierzytelnianie przez Azure AD
- 📞 Dołączanie do spotkań Teams
- 🎵 Przechwytywanie audio w czasie rzeczywistym
- 📦 Buforowanie audio klatka po klatce
- 💾 Zapis audio do plików WAV
- 📊 Monitorowanie statusu połączeń
- 🌐 Publiczny endpoint HTTPS z certyfikatem SSL

## Wymagania

- .NET 6.0 lub nowszy
- Konto Azure z zarejestrowanym Bot Service
- Aplikacja Azure AD z odpowiednimi uprawnieniami
- Microsoft Teams
- **Publiczny adres IP z certyfikatem SSL** (skonfigurowany: rtmbot.sniezka.com)

## 🚀 Szybki start

### 1. Konfiguracja Azure AD (WYMAGANE!)
```bash
# Zobacz szczegółowe instrukcje
cat KONFIGURACJA.md
```

### 2. Aktualizacja konfiguracji
```bash
# Skopiuj appsettings.Development.json i uzupełnij dane
cp appsettings.Development.json appsettings.Local.json
# Edytuj appsettings.Local.json i dodaj swoje dane Azure AD
```

### 3. Uruchomienie
```bash
dotnet restore
dotnet build
dotnet run
```

Bot uruchomi się na `https://rtmbot.sniezka.com:443`

## Konfiguracja

### 1. Konfiguracja Azure Bot Service

1. Zarejestruj bot w [Azure Bot Service](https://portal.azure.com/#create/Microsoft.BotService)
2. Skonfiguruj Microsoft App ID i App Password
3. Dodaj endpoint messaging dla Teams
4. **Włącz calling bot** i ustaw endpoint: `https://rtmbot.sniezka.com/api/calling`

### 2. Konfiguracja Azure AD

1. Utwórz aplikację w Azure AD
2. Dodaj uprawnienia Microsoft Graph:
   - `Calls.JoinGroupCall.All`
   - `Calls.InitiateGroupCall.All`
   - `Calls.AccessMedia.All`
3. Wygeneruj Client Secret

### 3. Aktualizacja appsettings.json

```json
{
  "AzureAd": {
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET"
  },
  "Bot": {
    "MicrosoftAppId": "YOUR_BOT_APP_ID",
    "MicrosoftAppPassword": "YOUR_BOT_APP_PASSWORD",
    "MicrosoftAppTenantId": "YOUR_TENANT_ID"
  },
  "Hosting": {
    "Urls": "https://rtmbot.sniezka.com:443",
    "PublicUrl": "https://rtmbot.sniezka.com"
  }
}
```

## Instalacja i uruchomienie

### 1. Przywrócenie pakietów NuGet

```bash
dotnet restore
```

### 2. Uruchomienie aplikacji

```bash
dotnet run
```

### 3. Uruchomienie w trybie release

```bash
dotnet run --configuration Release
```

## Struktura projektu

```
RealTimeMediaBot/
├── Bots/
│   ├── TeamsBot.cs              # Główna logika bota Teams
│   └── BotHostedService.cs      # Service hostujący bota
├── Controllers/
│   ├── CallsController.cs       # API dla połączeń
│   └── TeamsWebhookController.cs # Webhook Teams
├── Models/
│   ├── Configuration.cs         # Modele konfiguracyjne
│   ├── CallModels.cs            # Modele połączeń
│   └── TeamsWebhookModels.cs    # Modele webhook
├── Services/
│   ├── AuthenticationService.cs # Uwierzytelnianie Azure AD
│   ├── GraphService.cs          # Microsoft Graph API
│   └── AudioCaptureService.cs   # Przechwytywanie audio
├── Properties/
│   └── launchSettings.json      # Konfiguracja uruchamiania
├── Program.cs                   # Główny program
├── appsettings.json            # Konfiguracja (szablon)
├── appsettings.Development.json # Konfiguracja deweloperska
├── KONFIGURACJA.md             # Instrukcje konfiguracji
├── TESTOWANIE.md               # Instrukcje testowania
└── RealTimeMediaBot.csproj     # Plik projektu
```

## 🌐 **Endpoint API**

### **Główne endpointy:**
- **`/`** - Status aplikacji
- **`/health`** - Zdrowie aplikacji
- **`/api/calling`** - Główny endpoint calling (GET)
- **`/api/teamswebhook/calling`** - Webhook calling (POST)
- **`/api/calls/status`** - Status połączeń
- **`/api/calls/audio/*`** - Zarządzanie audio

### **Testowanie publicznego endpoint:**
```bash
# Sprawdź status
curl https://rtmbot.sniezka.com/health

# Sprawdź calling endpoint
curl https://rtmbot.sniezka.com/api/calling

# Sprawdź status połączeń
curl https://rtmbot.sniezka.com/api/calls/status
```

## Użycie

### Dołączanie do spotkania

```csharp
// Inicjalizacja bota
await teamsBot.InitializeAsync();

// Dołączanie do spotkania
await teamsBot.JoinCallAsync("https://teams.microsoft.com/l/meetup-join/...");

// Opuszczanie spotkania
await teamsBot.LeaveCallAsync();
```

### Przechwytywanie audio

```csharp
// Zapisanie bufora audio
await audioCaptureService.SaveAudioBufferAsync("output.wav");

// Wyczyszczenie bufora
audioCaptureService.ClearBuffer();

// Sprawdzenie rozmiaru bufora
var bufferSize = audioCaptureService.GetBufferSize();
```

## Buforowanie audio

Bot przechwytuje audio w czasie rzeczywistym i buforuje je klatka po klatce:

- **Maksymalny rozmiar bufora**: 1000 klatek
- **Format audio**: 16 kHz, 16-bit, mono
- **Buforowanie**: FIFO (First In, First Out)
- **Automatyczne czyszczenie**: Najstarsze klatki są usuwane gdy bufor jest pełny

## Logowanie

Aplikacja używa wbudowanego systemu logowania .NET:

- **Poziom domyślny**: Information
- **Microsoft**: Warning
- **Format**: Strukturalne logowanie z parametrami

## 🔒 Bezpieczeństwo

- **Client Secret**: Przechowuj w Azure Key Vault w produkcji
- **Uprawnienia**: Używaj minimalnych wymaganych uprawnień
- **Logi**: Nie loguj wrażliwych danych
- **HTTPS**: Używaj tylko bezpiecznych połączeń (skonfigurowane: rtmbot.sniezka.com)
- **Pliki konfiguracyjne**: NIGDY nie commituj plików z rzeczywistymi danymi

## Rozwiązywanie problemów

### Błąd uwierzytelniania

1. Sprawdź poprawność Client ID i Client Secret
2. Upewnij się, że aplikacja ma odpowiednie uprawnienia
3. Sprawdź Tenant ID
4. Zobacz [KONFIGURACJA.md](KONFIGURACJA.md)

### Błąd dołączania do spotkania

1. Sprawdź poprawność URL spotkania
2. Upewnij się, że bot ma uprawnienia do dołączania
3. Sprawdź logi aplikacji

### Problemy z audio

1. Sprawdź uprawnienia `Calls.AccessMedia.All`
2. Upewnij się, że spotkanie ma włączone audio
3. Sprawdź rozmiar bufora audio

### Problemy z publicznym endpoint

1. Sprawdź czy certyfikat SSL jest poprawny
2. Sprawdź czy port 443 jest otwarty
3. Sprawdź czy DNS wskazuje na właściwy adres IP
4. Sprawdź logi aplikacji

## Wsparcie

W przypadku problemów:
1. Sprawdź logi aplikacji
2. Sprawdź [KONFIGURACJA.md](KONFIGURACJA.md)
3. Sprawdź [TESTOWANIE.md](TESTOWANIE.md)
4. Sprawdź dokumentację Microsoft Graph
5. Sprawdź status usług Azure
