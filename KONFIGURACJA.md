# Instrukcje konfiguracji Real-Time Media Bot

## 🚨 WAŻNE: Przed uruchomieniem bota musisz skonfigurować Azure AD!

### Krok 1: Utworzenie aplikacji w Azure AD

1. **Przejdź do [Azure Portal](https://portal.azure.com)**
2. **Azure Active Directory → Rejestracje aplikacji → Nowa rejestracja**
3. **Wypełnij formularz**:
   - **Nazwa**: `RealTimeMediaBot`
   - **Typ konta**: `Konta w tym katalogu organizacji tylko`
   - **URI przekierowania**: `https://login.microsoftonline.com/common/oauth2/nativeclient`
4. **Kliknij "Zarejestruj"**

### Krok 2: Skopiowanie danych aplikacji

Po utworzeniu aplikacji skopiuj:
- **Application (client) ID** → to będzie Twój `ClientId`
- **Directory (tenant) ID** → to będzie Twój `TenantId`

### Krok 3: Konfiguracja uprawnień Microsoft Graph

1. **W aplikacji → API i uprawnienia → Uprawnienia**
2. **Dodaj uprawnienie → Microsoft Graph → Uprawnienia aplikacji**
3. **Dodaj następujące uprawnienia**:
   - `Calls.JoinGroupCall.All` - Dołączanie do połączeń grupowych
   - `Calls.InitiateGroupCall.All` - Inicjowanie połączeń grupowych
   - `Calls.AccessMedia.All` - Dostęp do mediów w połączeniach
   - `User.Read.All` - Odczyt informacji o użytkownikach
4. **Kliknij "Dodaj uprawnienia"**
5. **Udziel zgody administratora** (niebieski przycisk)

### Krok 4: Utworzenie Client Secret

1. **Certyfikaty i wpisy tajne → Wpisy tajne klienta → Nowy wpis tajny klienta**
2. **Opis**: `Bot Secret`
3. **Ważność**: `24 miesiące` (lub dłużej)
4. **Kliknij "Dodaj"**
5. **SKOPIUJ WARTOŚĆ** (będzie widoczna tylko raz!)

### Krok 5: Aktualizacja appsettings.json

Zastąp placeholdery w `appsettings.json` rzeczywistymi danymi:

```json
{
  "AzureAd": {
    "TenantId": "Twój_Tenant_ID_z_Azure_AD",
    "ClientId": "Twój_Client_ID_z_Azure_AD", 
    "ClientSecret": "Twój_Client_Secret_z_Azure_AD"
  },
  "Bot": {
    "MicrosoftAppId": "Twój_Client_ID_z_Azure_AD",
    "MicrosoftAppPassword": "Twój_Client_Secret_z_Azure_AD",
    "MicrosoftAppTenantId": "Twój_Tenant_ID_z_Azure_AD"
  }
}
```

### Krok 6: Testowanie konfiguracji

Po skonfigurowaniu:

1. **Uruchom projekt**: `dotnet run`
2. **Sprawdź logi** - powinny pokazać "Bot Teams został zainicjalizowany pomyślnie"
3. **Jeśli błąd uwierzytelniania** - sprawdź poprawność danych w `appsettings.json`

## 🔒 Bezpieczeństwo

- **NIGDY nie commituj `appsettings.json` z rzeczywistymi danymi**
- **Użyj `appsettings.Development.json` dla lokalnego rozwoju**
- **W produkcji użyj Azure Key Vault lub zmiennych środowiskowych**

## 📝 Przykład appsettings.Development.json

```json
{
  "AzureAd": {
    "TenantId": "12345678-1234-1234-1234-123456789012",
    "ClientId": "87654321-4321-4321-4321-210987654321",
    "ClientSecret": "Twój_rzeczywisty_secret"
  }
}
```

## ❓ Rozwiązywanie problemów

### Błąd "AADSTS700016: Application with identifier was not found"
- Sprawdź poprawność `ClientId` w Azure AD

### Błąd "AADSTS70002: The request body must contain the following parameter: 'client_secret'"
- Sprawdź poprawność `ClientSecret`

### Błąd "Insufficient privileges to complete the operation"
- Sprawdź czy udzielono zgody administratora dla uprawnień Microsoft Graph
