# Instrukcje testowania Real-Time Media Bot z MS Teams

## 🚀 Jak przetestować bota

### 1. Uruchomienie aplikacji

```bash
# Zbuduj projekt
dotnet build

# Uruchom aplikację
dotnet run
```

Aplikacja uruchomi się na `https://rtmbot.sniezka.com:443`.

## 🌐 **Publiczny Endpoint**

Bot jest skonfigurowany do działania na:
- **URL**: `https://rtmbot.sniezka.com`
- **Calling Endpoint**: `https://rtmbot.sniezka.com/api/calling`
- **Port**: 443 (HTTPS)

## 2. Sprawdzenie statusu

```bash
# Sprawdź czy bot działa
curl https://rtmbot.sniezka.com/
# Powinno zwrócić: "Real-Time Media Bot dla Teams jest uruchomiony na https://rtmbot.sniezka.com!"

# Sprawdź zdrowie aplikacji
curl https://rtmbot.sniezka.com/health
# Powinno zwrócić status, timestamp i endpoint

# Sprawdź calling endpoint
curl https://rtmbot.sniezka.com/api/calling
# Powinno zwrócić: "Calling endpoint jest aktywny"
```

## 3. Testowanie endpoint'ów API

#### Sprawdzenie statusu połączeń
```bash
curl https://rtmbot.sniezka.com/api/calls/status
```

#### Sprawdzenie rozmiaru bufora audio
```bash
curl https://rtmbot.sniezka.com/api/calls/audio/buffer-size
```

#### Zapisanie bufora audio
```bash
curl -X POST https://rtmbot.sniezka.com/api/calls/audio/save \
  -H "Content-Type: application/json" \
  -d '{"filePath": "test_audio.wav"}'
```

#### Wyczyszczenie bufora audio
```bash
curl -X DELETE https://rtmbot.sniezka.com/api/calls/audio/clear
```

## 4. Testowanie z MS Teams

#### A. Konfiguracja Teams

1. **W Teams, dodaj bota jako kontakt**:
   - Wyszukaj bota po nazwie lub ID
   - Dodaj do kontaktów

2. **Zadzwoń do bota**:
   - Kliknij ikonę połączenia w Teams
   - Wybierz bota z listy kontaktów
   - Rozpocznij połączenie

#### B. Obserwowanie logów

Podczas połączenia z Teams, w konsoli aplikacji powinieneś zobaczyć:

```
info: RealTimeMediaBot.Controllers.TeamsWebhookController[0]
      Otrzymano webhook calling z Teams na publicznym endpoincie
info: RealTimeMediaBot.Bots.TeamsBot[0]
      Obsługa webhook przychodzącego połączenia: /communications/calls/{callId}
info: RealTimeMediaBot.Bots.TeamsBot[0]
      Dodano nowe połączenie: {CallId} od {callerId}
info: RealTimeMediaBot.Bots.TeamsBot[0]
      Akceptowanie przychodzącego połączenia: {CallId}
info: RealTimeMediaBot.Bots.TeamsBot[0]
      Połączenie {CallId} zostało zaakceptowane
```

#### C. Sprawdzanie statusu połączenia

```bash
# Sprawdź status połączeń
curl https://rtmbot.sniezka.com/api/calls/status
```

Powinno zwrócić informacje o aktywnych połączeniach.

## 5. Testowanie audio

#### A. Rozmowa przez Teams

1. **Rozpocznij połączenie z botem**
2. **Mów przez mikrofon** - bot powinien przechwytywać audio
3. **Sprawdź rozmiar bufora**:
   ```bash
   curl https://rtmbot.sniezka.com/api/calls/audio/buffer-size
   ```

#### B. Zapisanie audio

```bash
# Zapisuj audio co jakiś czas
curl -X POST https://rtmbot.sniezka.com/api/calls/audio/save \
  -H "Content-Type: application/json" \
  -d '{"filePath": "rozmowa_teams.wav"}'
```

#### C. Sprawdzenie plików audio

Pliki WAV powinny być zapisane w katalogu aplikacji.

## 6. Debugowanie

#### A. Sprawdzenie logów

Aplikacja loguje wszystkie ważne zdarzenia. Szukaj:

- `Otrzymano webhook calling z Teams na publicznym endpoincie` - Teams wysyła powiadomienia
- `Dodano nowe połączenie` - Bot wykrył połączenie
- `Przetworzono audio data` - Audio jest przechwytywane
- `Dodano klatkę audio do bufora` - Audio jest buforowane

#### B. Sprawdzenie błędów

Szukaj w logach:
- `Błąd podczas obsługi webhook`
- `Błąd podczas parsowania`
- `Błąd podczas przetwarzania audio`

#### C. Testowanie endpoint'ów

```bash
# Test wszystkich endpoint'ów
curl -X POST https://rtmbot.sniezka.com/api/calls/incoming \
  -H "Content-Type: application/json" \
  -d '{"id": "test-call", "callerId": "test@teams.com", "callerDisplayName": "Test User"}'
```

## 7. Rozwiązywanie problemów

#### Problem: Bot nie odbiera połączeń

1. **Sprawdź logi** - czy są błędy inicjalizacji
2. **Sprawdź konfigurację Azure AD** - czy token jest poprawny
3. **Sprawdź uprawnienia** - czy bot ma uprawnienia do odbierania połączeń
4. **Sprawdź publiczny endpoint** - czy `https://rtmbot.sniezka.com/api/calling` jest dostępny

#### Problem: Audio nie jest przechwytywane

1. **Sprawdź czy połączenie jest aktywne**
2. **Sprawdź rozmiar bufora audio**
3. **Sprawdź logi audio media webhook**

#### Problem: Aplikacja nie uruchamia się

1. **Sprawdź czy .NET 6.0 jest zainstalowany**
2. **Sprawdź czy wszystkie pakiety NuGet są przywrócone**
3. **Sprawdź poprawność pliku appsettings.json**

#### Problem: Publiczny endpoint nie działa

1. **Sprawdź czy certyfikat SSL jest poprawny**
2. **Sprawdź czy port 443 jest otwarty**
3. **Sprawdź czy DNS wskazuje na właściwy adres IP**
4. **Sprawdź logi aplikacji**

## 8. Przykłady użycia

#### Automatyczne testowanie

```bash
# Skrypt testowy
#!/bin/bash
echo "Testowanie bota Teams na publicznym endpoincie..."

# Sprawdź status
echo "Status:"
curl -s https://rtmbot.sniezka.com/health | jq .

# Sprawdź połączenia
echo "Połączenia:"
curl -s https://rtmbot.sniezka.com/api/calls/status | jq .

# Sprawdź audio
echo "Audio buffer:"
curl -s https://rtmbot.sniezka.com/api/calls/audio/buffer-size | jq .
```

#### Monitorowanie w czasie rzeczywistym

```bash
# Monitoruj logi w czasie rzeczywistym
while true; do
  curl -s https://rtmbot.sniezka.com/api/calls/status | jq .
  sleep 5
done
```

#### Testowanie z różnych lokalizacji

```bash
# Test z zewnętrznej lokalizacji
curl -s https://rtmbot.sniezka.com/health

# Test z Azure Cloud Shell
curl -s https://rtmbot.sniezka.com/api/calling
```

## 📝 Uwagi

- **Bot automatycznie akceptuje wszystkie połączenia**
- **Audio jest buforowane w czasie rzeczywistym**
- **Pliki WAV są zapisywane w katalogu aplikacji**
- **Wszystkie operacje są logowane**
- **API jest dostępne pod `https://rtmbot.sniezka.com/api/*`**
- **Bot działa na publicznym endpoincie HTTPS z certyfikatem SSL**

## 🔗 Przydatne linki

- [Microsoft Graph Communications API](https://docs.microsoft.com/en-us/graph/api/resources/call)
- [Teams Bot Framework](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots)
- [Webhook Teams](https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/what-are-webhooks-and-connectors)
- [Azure Bot Service Calling](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-channel-connect-teams?view=azure-bot-service-4.0)
