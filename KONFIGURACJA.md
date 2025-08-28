# Instrukcje konfiguracji Real-Time Media Bot

## 🚨 WAŻNE: Przed uruchomieniem bota musisz skonfigurować Azure AD!

### Krok 1: Utworzenie aplikacji w Azure AD

1. **Przejdź do [Azure Portal](https://portal.azure.com)**
2. **Azure Active Directory → Rejestracje aplikacji → Nowa rejestracja**
3. **Wypełnij formularz**:
   - **Nazwa**: `RealTimeMediaBot`
   - **Typ konta**: `Konta w tym katalogu organizacji i konta osobiste Microsoft`
   - **URI przekierowania**: `https://rtmbot.sniezka.com/api/auth/callback`
4. **Kliknij "Zarejestruj"**

### Krok 2: Konfiguracja platformy Web

1. **W aplikacji → Authentication → Add a platform → Web**
2. **Redirect URIs** (dodaj oba):
   ```
   https://rtmbot.sniezka.com/api/auth/callback
   https://rtmbot.sniezka.com/signin-oidc
   ```
3. **Front-channel logout URL**: `https://rtmbot.sniezka.com/signout-oidc`
4. **Implicit grant and hybrid flows**: Zaznacz **Access tokens** i **ID tokens**
5. **Kliknij "Configure"**

### Krok 3: Skopiowanie danych aplikacji

Po utworzeniu aplikacji skopiuj:
- **Application (client) ID** → to będzie Twój `ClientId`
- **Directory (tenant) ID** → to będzie Twój `TenantId`

### Krok 4: Konfiguracja uprawnień Microsoft Graph

1. **W aplikacji → API i uprawnienia → Uprawnienia**
2. **Dodaj uprawnienie → Microsoft Graph → Uprawnienia aplikacji**
3. **Dodaj następujące uprawnienia**:
   - `Calls.JoinGroupCall.All` - Dołączanie do połączeń grupowych
   - `Calls.InitiateGroupCall.All` - Inicjowanie połączeń grupowych
   - `Calls.AccessMedia.All` - Dostęp do mediów w połączeniach
   - `User.Read.All` - Odczyt informacji o użytkownikach
4. **Kliknij "Dodaj uprawnienia"**
5. **Udziel zgody administratora** (niebieski przycisk)

### Krok 5: Utworzenie Client Secret

1. **Certyfikaty i wpisy tajne → Wpisy tajne klienta → Nowy wpis tajny klienta**
2. **Opis**: `Bot Secret`
3. **Ważność**: `24 miesiące` (lub dłużej)
4. **Kliknij "Dodaj"**
5. **SKOPIUJ WARTOŚĆ** (będzie widoczna tylko raz!)

### Krok 6: Aktualizacja appsettings.json

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
  },
  "Hosting": {
    "Urls": "https://rtmbot.sniezka.com:443",
    "PublicUrl": "https://rtmbot.sniezka.com"
  }
}
```

### Krok 7: Testowanie konfiguracji

Po skonfigurowaniu:

1. **Uruchom projekt**: `dotnet run`
2. **Sprawdź endpoint auth**: `curl https://rtmbot.sniezka.com/api/auth/status`
3. **Sprawdź logi** - powinny pokazać "Bot Teams został zainicjalizowany pomyślnie"
4. **Jeśli błąd uwierzytelniania** - sprawdź poprawność danych w `appsettings.json`

## 🔒 Bezpieczeństwo

- **NIGDY nie commituj `appsettings.json` z rzeczywistymi danymi**
- **Użyj `appsettings.Development.json` dla lokalnego rozwoju**
- **W produkcji użyj Azure Key Vault lub zmiennych środowiskowych**

## 📝 Przykład appsettings.Development.json

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "TWÓJ_TENANT_ID",
    "ClientId": "TWÓJ_CLIENT_ID",
    "ClientSecret": "TWÓJ_CLIENT_SECRET"
  },
  "Bot": {
    "MicrosoftAppId": "TWÓJ_CLIENT_ID",
    "MicrosoftAppPassword": "TWÓJ_CLIENT_SECRET",
    "MicrosoftAppType": "SingleTenant",
    "MicrosoftAppTenantId": "TWÓJ_TENANT_ID"
  },
  "Hosting": {
    "Urls": "https://rtmbot.sniezka.com:443",
    "PublicUrl": "https://rtmbot.sniezka.com"
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

### Błąd "AADSTS50011: The reply URL specified in the request does not match the reply URLs configured for the application"
- Sprawdź czy URI przekierowania w Azure AD to: `https://rtmbot.sniezka.com/api/auth/callback`

### Błąd "AADSTS50020: User account from a different tenant than the current application"
- Sprawdź czy `MicrosoftAppType` jest ustawiony na `SingleTenant` (nie `MultiTenant`)

## 🌐 **URI Przekierowania - Wyjaśnienie**

### **Co to jest?**
URI przekierowania to adres URL, na który Azure AD przekieruje użytkownika po pomyślnym uwierzytelnieniu.

### **Dlaczego `https://login.microsoftonline.com/common/oauth2/nativeclient` nie działa?**
- To URI jest **tylko dla aplikacji desktopowych**
- **Bot Teams to aplikacja webowa** - potrzebuje własnego URI na Twojej domenie

### **Poprawny URI dla Twojego bota:**
```
https://rtmbot.sniezka.com/api/auth/callback
```

### **Jak to działa:**
1. **Użytkownik loguje się** do Azure AD
2. **Azure AD przekierowuje** na `https://rtmbot.sniezka.com/api/auth/callback`
3. **Twój bot odbiera callback** i wymienia kod na token
4. **Bot może teraz** używać Microsoft Graph API

### **Ważne uwagi:**
- **URI musi być HTTPS** (nie HTTP)
- **URI musi być publicznie dostępny**
- **URI musi być dokładnie taki sam** w Azure AD i w Twojej aplikacji
