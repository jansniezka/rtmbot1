# 🤖 Real-Time Media Bot dla Microsoft Teams

Bot Teams z możliwością przechwytywania audio w czasie rzeczywistym, wykorzystujący Microsoft Graph Communications API.

## ✨ **Funkcjonalności**

- 📞 **Odbieranie połączeń przychodzących** - Bot automatycznie odbiera połączenia z Teams
- 🎯 **Dołączanie do spotkań** - Możliwość dołączania do istniejących spotkań Teams
- 🎵 **Przechwytywanie audio w czasie rzeczywistym** - Buforowanie audio klatka po klatce
- 🔄 **Zarządzanie połączeniami** - Akceptowanie, odrzucanie, przekierowywanie, kończenie
- 🌐 **Webhook'i Teams** - Obsługa powiadomień o połączeniach i audio media
- 💾 **Zapis audio** - Zapis przechwyconego audio do plików WAV
- 🔐 **Azure AD Authentication** - Bezpieczne uwierzytelnianie przez Microsoft Identity
- 📊 **Monitorowanie statusu połączeń** - API do sprawdzania stanu aktywnych połączeń

## 🏗️ **Architektura**

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Microsoft     │    │   Real-Time      │    │   Azure AD &    │
│   Teams        │◄──►│   Media Bot      │◄──►│   Bot Service   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌──────────────────┐
                       │   Audio Buffer   │
                       │   & WAV Files   │
                       └──────────────────┘
```

## 🚀 **Nowe funkcjonalności Microsoft Graph Communications API**

### **1. Rzeczywiste połączenia z Teams**
- ✅ **Odbieranie połączeń** - Bot może faktycznie odbierać połączenia przychodzące
- ✅ **Dołączanie do spotkań** - Możliwość dołączania do istniejących spotkań Teams
- ✅ **Zarządzanie połączeniami** - Pełna kontrola nad połączeniami

### **2. Przechwytywanie audio**
- ✅ **Audio Media Events** - Odbieranie audio w czasie rzeczywistym
- ✅ **Buforowanie klatek** - Każda klatka audio jest buforowana osobno
- ✅ **Zapis do WAV** - Automatyczny zapis audio do plików

### **3. Webhook'i Teams**
- ✅ **Incoming Call** - Powiadomienia o przychodzących połączeniach
- ✅ **Call Updated** - Aktualizacje statusu połączeń
- ✅ **Audio Media** - Powiadomienia o dostępności audio

## 📋 **Wymagania**

- **.NET 6.0** lub nowszy
- **Azure Bot Service** z włączonymi funkcjami "calling bot"
- **Azure AD App Registration** z odpowiednimi uprawnieniami
- **Publiczny HTTPS endpoint** z certyfikatem SSL
- **Microsoft Graph Communications API** uprawnienia

## 🔧 **Konfiguracja Azure**

### **1. Azure Bot Service**
- Włącz "Calling bot" w ustawieniach bota
- Skonfiguruj endpoint: `https://rtmbot.sniezka.com/api/calling`

### **2. Azure AD App Registration**
- **Redirect URI**: `https://rtmbot.sniezka.com/api/auth/callback`
- **Platform**: Web
- **Implicit grant**: Access tokens & ID tokens

### **3. Microsoft Graph Permissions**
- `Calls.AccessMedia.All`
- `Calls.InitiateOutgoingCall.All`
- `Calls.JoinGroupCall.All`
- `Calls.JoinGroupCallAsGuest.All`

## 📁 **Struktura projektu**

```
RealTimeMediaBot/
├── Controllers/                 # HTTP API Controllers
│   ├── AuthController.cs       # Azure AD authentication
│   ├── CallsController.cs      # Zarządzanie połączeniami
│   ├── MeetingsController.cs   # Zarządzanie spotkaniami
│   └── TeamsWebhookController.cs # Webhook'i Teams
├── Bots/                       # Logika bota
│   ├── TeamsBot.cs            # Główna logika bota Teams
│   └── BotHostedService.cs    # Background service
├── Services/                   # Usługi aplikacji
│   ├── AuthenticationService.cs # Azure AD auth
│   ├── AudioCaptureService.cs  # Przechwytywanie audio
│   └── GraphService.cs        # Microsoft Graph API
├── Models/                     # Modele danych
│   ├── Configuration.cs       # Konfiguracja
│   ├── CallModels.cs          # Modele połączeń
│   └── TeamsWebhookModels.cs  # Modele webhook'ów Teams
└── appsettings.json           # Konfiguracja aplikacji
```

## 🎯 **API Endpoints**

### **Główne endpoint'y**
- `GET /` - Status aplikacji
- `GET /health` - Health check
- `GET /api/calling` - Informacje o calling API

### **Zarządzanie połączeniami**
- `POST /api/calls/incoming` - Symulacja przychodzącego połączenia
- `POST /api/calls/answer/{callId}` - Akceptowanie połączenia
- `POST /api/calls/reject/{callId}` - Odrzucanie połączenia
- `POST /api/calls/transfer/{callId}` - Przekierowanie połączenia
- `POST /api/calls/end/{callId}` - Kończenie połączenia
- `GET /api/calls/status` - Status aktywnych połączeń

### **Zarządzanie spotkaniami**
- `POST /api/meetings/join` - Dołączanie do spotkania
- `GET /api/meetings/status/{callId}` - Status spotkania
- `POST /api/meetings/leave/{callId}` - Opuszczanie spotkania
- `GET /api/meetings/active` - Lista aktywnych spotkań

### **Audio management**
- `GET /api/calls/audio/buffer-size` - Rozmiar bufora audio
- `POST /api/calls/audio/save` - Zapis audio do pliku
- `POST /api/calls/audio/clear` - Czyszczenie bufora audio

### **Webhook'i Teams**
- `POST /api/teamswebhook/calling` - Główny endpoint webhook'ów
- `POST /api/teamswebhook/incoming-call` - Przychodzące połączenia
- `POST /api/teamswebhook/call-updated` - Aktualizacje połączeń
- `POST /api/teamswebhook/audio-media` - Audio media

### **Authentication**
- `GET /api/auth/callback` - Azure AD callback
- `GET /api/auth/status` - Status uwierzytelniania
- `POST /api/auth/refresh` - Odświeżanie tokenu

## 🚀 **Instalacja i uruchomienie**

### **1. Klonowanie repozytorium**
```bash
git clone <repository-url>
cd real-time-media-bot
```

### **2. Konfiguracja**
```bash
# Skopiuj i skonfiguruj appsettings.json
cp appsettings.json appsettings.Development.json
# Edytuj plik z Twoimi danymi Azure
```

### **3. Uruchomienie**
```bash
dotnet restore
dotnet build
dotnet run
```

## 🧪 **Testowanie**

### **1. Test infrastruktury**
```bash
# Sprawdź status aplikacji
curl https://rtmbot.sniezka.com/health

# Sprawdź calling endpoint
curl https://rtmbot.sniezka.com/api/calling
```

### **2. Test webhook'ów**
```bash
# Symuluj webhook z Teams
curl -X POST https://rtmbot.sniezka.com/api/teamswebhook/calling \
  -H "Content-Type: application/json" \
  -d '{"resource": "/communications/calls/123", "changeType": "created"}'
```

### **3. Test zarządzania połączeniami**
```bash
# Sprawdź status połączeń
curl https://rtmbot.sniezka.com/api/calls/status

# Symuluj przychodzące połączenie
curl -X POST https://rtmbot.sniezka.com/api/calls/incoming \
  -H "Content-Type: application/json" \
  -d '{"id": "test-call", "callerId": "test@example.com", "callerDisplayName": "Test User"}'
```

### **4. Test dołączania do spotkań**
```bash
# Dołącz do spotkania
curl -X POST https://rtmbot.sniezka.com/api/meetings/join \
  -H "Content-Type: application/json" \
  -d '{"meetingUrl": "https://teams.microsoft.com/l/meetup-join/...", "displayName": "Test Bot"}'
```

## 🔍 **Monitoring i logi**

### **1. Logi aplikacji**
- Wszystkie operacje są logowane z poziomem `Information`
- Błędy są logowane z poziomem `Error`
- Debug informacje o audio są logowane z poziomem `Debug`

### **2. Status połączeń**
```bash
# Sprawdź aktywnych połączeń
curl https://rtmbot.sniezka.com/api/calls/status

# Sprawdź rozmiar bufora audio
curl https://rtmbot.sniezka.com/api/calls/audio/buffer-size
```

## 🛠️ **Rozwiązywanie problemów**

### **1. Błędy uwierzytelniania**
- Sprawdź konfigurację Azure AD w `appsettings.json`
- Upewnij się, że Redirect URI jest poprawnie skonfigurowany
- Sprawdź uprawnienia Microsoft Graph

### **2. Problemy z webhook'ami**
- Sprawdź endpoint w Azure Bot Service
- Upewnij się, że bot ma uprawnienia "calling bot"
- Sprawdź logi aplikacji pod kątem błędów

### **3. Problemy z audio**
- Sprawdź czy `AudioCaptureService` jest poprawnie skonfigurowany
- Upewnij się, że masz uprawnienia `Calls.AccessMedia.All`
- Sprawdź logi audio media webhook'ów

## 🔒 **Bezpieczeństwo**

- **HTTPS** - Wszystkie komunikacje przez HTTPS
- **Azure AD** - Uwierzytelnianie przez Microsoft Identity
- **Token caching** - Bezpieczne przechowywanie tokenów
- **Webhook validation** - Walidacja webhook'ów z Teams

## 📚 **Dokumentacja dodatkowa**

- [KONFIGURACJA.md](KONFIGURACJA.md) - Szczegółowa konfiguracja Azure
- [TESTOWANIE.md](TESTOWANIE.md) - Instrukcje testowania
- [Microsoft Graph Communications API](https://docs.microsoft.com/en-us/graph/api/resources/calls-api-overview)

## 🤝 **Wsparcie**

W przypadku problemów:
1. Sprawdź logi aplikacji
2. Sprawdź konfigurację Azure
3. Sprawdź uprawnienia Microsoft Graph
4. Sprawdź endpoint'y webhook'ów

## 📝 **Status implementacji**

- ✅ **Infrastruktura HTTP** - Gotowa
- ✅ **Azure AD Authentication** - Gotowe
- ✅ **Webhook endpoints** - Gotowe
- ✅ **Audio buffering** - Gotowe
- ✅ **Microsoft Graph Communications API** - Gotowe
- ✅ **Call management** - Gotowe
- ✅ **Meeting management** - Gotowe

**Bot jest gotowy do rzeczywistego testowania z Microsoft Teams!** 🎉
