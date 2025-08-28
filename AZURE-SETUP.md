# 🚀 Azure Setup - Kompletny przewodnik konfiguracji Real-Time Media Bot

## 📋 **KROK PO KROKU - REJESTRACJA W AZURE**

### **1. 🏢 AZURE AD APP REGISTRATION**

#### **A. Utwórz App Registration:**
1. Otwórz [Azure Portal](https://portal.azure.com)
2. Przejdź do **Azure Active Directory**
3. Kliknij **App registrations** → **New registration**
4. Wypełnij formularz:
   ```
   Name: Real-Time Media Bot
   Supported account types: Accounts in this organizational directory only (Single tenant)
   Redirect URI: Web → https://rtmbot.sniezka.com/api/auth/callback
   ```
5. Kliknij **Register**

#### **B. Zapisz ważne wartości:**
Po utworzeniu zapisz:
- ✅ **Application (client) ID** → to będzie Twój `MicrosoftAppId`
- ✅ **Directory (tenant) ID** → to będzie Twój `TenantId`

#### **C. Utwórz Client Secret:**
1. Przejdź do **Certificates & secrets**
2. Kliknij **New client secret**
3. Wypełnij:
   ```
   Description: Real-Time Media Bot Secret
   Expires: 24 months (recommended)
   ```
4. Kliknij **Add**
5. ⚠️ **SKOPIUJ I ZAPISZ VALUE** → to będzie Twój `ClientSecret`

#### **D. Skonfiguruj Authentication:**
1. Przejdź do **Authentication**
2. W sekcji **Platform configurations** kliknij **Web**
3. Dodaj **Redirect URIs:**
   ```
   https://rtmbot.sniezka.com/api/auth/callback
   ```
4. W sekcji **Implicit grant and hybrid flows** zaznacz:
   - ✅ **Access tokens**
   - ✅ **ID tokens**
5. Kliknij **Save**

#### **E. Dodaj API Permissions:**
1. Przejdź do **API permissions**
2. Kliknij **Add a permission**
3. Wybierz **Microsoft Graph**
4. Wybierz **Application permissions**
5. Dodaj następujące uprawnienia:
   
   **📞 CALLING PERMISSIONS:**
   ```
   Calls.AccessMedia.All
   Calls.InitiateOutgoingCall.All
   Calls.JoinGroupCall.All
   Calls.JoinGroupCallAsGuest.All
   ```
   
   **💬 MESSAGING PERMISSIONS:**
   ```
   Chat.ReadWrite.All
   ChatMessage.Read.All
   Team.ReadBasic.All
   User.Read.All
   Directory.Read.All
   ```
6. Kliknij **Add permissions**
7. ⚠️ **WAŻNE:** Kliknij **Grant admin consent for [Twoja organizacja]**
8. Potwierdź **Yes**

### **2. 🤖 AZURE BOT SERVICE**

#### **A. Utwórz Bot Service:**
1. W Azure Portal kliknij **Create a resource**
2. Wyszukaj **Azure Bot**
3. Kliknij **Create**
4. Wypełnij formularz:
   ```
   Bot handle: real-time-media-bot (unikalna nazwa)
   Subscription: Twoja subskrypcja
   Resource group: Utwórz nową lub użyj istniejącej
   Location: West Europe (lub najbliższa)
   Pricing tier: F0 (Free) lub S1
   Microsoft App ID: Use existing app ID
   App ID: [WKLEJ Application ID z kroku 1B]
   ```
5. Kliknij **Review + create** → **Create**

#### **B. Skonfiguruj Messaging endpoint:**
1. Przejdź do utworzonego Bot Service
2. W menu bocznym kliknij **Configuration**
3. W polu **Messaging endpoint** wpisz:
   ```
   https://rtmbot.sniezka.com/api/messages
   ```
4. Kliknij **Apply**

#### **C. Włącz Calling Bot:**
1. W menu bocznym kliknij **Channels**
2. Znajdź **Microsoft Teams** i kliknij
3. Zaznacz opcję **Enable calling**
4. W polu **Webhook (for calling)** wpisz:
   ```
   https://rtmbot.sniezka.com/api/calling
   ```
5. Kliknij **Save**

#### **D. Skonfiguruj Teams Channel:**
1. Nadal w **Channels** kliknij **Microsoft Teams**
2. Wybierz **Microsoft Teams Commercial (most common)**
3. Zaznacz **Enable calling** jeśli nie było zaznaczone
4. Kliknij **Save**
5. Kliknij **Agree** dla Terms of Service

### **3. 📝 KONFIGURACJA APPSETTINGS.JSON**

Teraz uzupełnij `appsettings.json` z wartościami z Azure:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "TWOJ_TENANT_ID_Z_KROKU_1B",
    "ClientId": "TWOJ_APPLICATION_ID_Z_KROKU_1B", 
    "ClientSecret": "TWOJ_CLIENT_SECRET_Z_KROKU_1C"
  },
  "Bot": {
    "MicrosoftAppId": "TWOJ_APPLICATION_ID_Z_KROKU_1B",
    "MicrosoftAppPassword": "TWOJ_CLIENT_SECRET_Z_KROKU_1C",
    "MicrosoftAppType": "SingleTenant",
    "MicrosoftAppTenantId": "TWOJ_TENANT_ID_Z_KROKU_1B",
    "PublicUrl": "https://rtmbot.sniezka.com"
  }
}
```

### **4. 🎯 TEAMS APP MANIFEST**

#### **A. Przygotuj manifest.json:**
1. Otwórz plik `manifest.json`
2. Zamień `TWOJ_BOT_APP_ID_TUTAJ` na Twój **Application ID** z kroku 1B:
   ```json
   "id": "12345678-1234-1234-1234-123456789abc",
   "botId": "12345678-1234-1234-1234-123456789abc",
   "id": "12345678-1234-1234-1234-123456789abc"
   ```

#### **B. Stwórz ikony bota:**
Potrzebujesz dwa pliki:
- `icon-outline.png` - 32x32px, przezroczyste tło, outline
- `icon-color.png` - 192x192px, kolorowa ikona

**Przykład prostych ikon:**
```
🤖 - Użyj emoji jako podstawy
📞 - Lub ikonę telefonu
🎧 - Lub ikonę słuchawek
```

#### **C. Spakuj aplikację Teams:**
```powershell
# W PowerShell
Compress-Archive -Path manifest.json,icon-outline.png,icon-color.png -DestinationPath RealTimeMediaBot.zip
```

### **5. 📱 INSTALACJA W MICROSOFT TEAMS**

#### **A. Upload custom app:**
1. Otwórz **Microsoft Teams** (desktop lub web)
2. W lewym menu kliknij **Apps**
3. W dolnej części kliknij **Upload a custom app**
4. Wybierz **Upload for [Twoja organizacja]**
5. Wybierz plik `RealTimeMediaBot.zip`
6. Kliknij **Add**

#### **B. Dodaj bota do Teams:**
1. Po instalacji bot pojawi się w Apps
2. Kliknij **Add** aby dodać do Teams
3. Bot będzie dostępny jako:
   - **Kontakt osobisty** - można do niego dzwonić
   - **Członek zespołu** - można dodać do kanałów
   - **Uczestnik czatu** - można dodać do czatów grupowych

## 🧪 **TESTOWANIE KONFIGURACJI**

### **1. 🚀 Uruchom aplikację:**
```bash
dotnet run
```

### **2. 🔍 Sprawdź logi inicjalizacji:**
Powinieneś zobaczyć:
```
🚀 INICJALIZACJA BOTA TEAMS...
🔧 Azure AD Tenant ID: 12345678...
🔧 Bot App ID: 87654321...
🌐 Public URL: https://rtmbot.sniezka.com
🔐 Uzyskiwanie tokenu dostępu z Azure AD...
✅ Token dostępu uzyskany pomyślnie!
✅ Bot Teams został zainicjalizowany pomyślnie!
📡 WEBHOOK ENDPOINTS:
   - Azure calling: https://rtmbot.sniezka.com/api/calling
   - Teams webhook: https://rtmbot.sniezka.com/api/teamswebhook/calling
🎯 Bot jest GOTOWY do odbierania połączeń!
```

### **3. 💬 Test wiadomości tekstowych:**
1. W Teams napisz do bota: `help`
2. Bot powinien odpowiedzieć listą komend

### **4. 📞 Test połączeń:**
1. W Teams zadzwoń do bota
2. Bot powinien automatycznie odebrać
3. W logach zobaczysz szczegółowe informacje o połączeniu

## ⚠️ **TROUBLESHOOTING**

### **🔴 Problem: "Value cannot be null. (Parameter 'clientSecret')"**
**Rozwiązanie:** Sprawdź czy `ClientSecret` w `appsettings.json` jest prawidłowy i nie wygasł.

### **🔴 Problem: "AADSTS700016: Application not found"**
**Rozwiązanie:** Sprawdź czy `ClientId` i `TenantId` są prawidłowe.

### **🔴 Problem: "AADSTS50011: The reply URL specified in the request does not match"**
**Rozwiązanie:** Sprawdź Redirect URI w Azure AD - musi być `https://rtmbot.sniezka.com/api/auth/callback`

### **🔴 Problem: Bot nie odbiera połączeń**
**Rozwiązanie:** 
1. Sprawdź czy Calling endpoint w Azure Bot Service to `https://rtmbot.sniezka.com/api/calling`
2. Sprawdź czy bot ma uprawnienia `Calls.AccessMedia.All`
3. Sprawdź czy aplikacja działa na HTTPS

### **🔴 Problem: Bot nie odpowiada na wiadomości**
**Rozwiązanie:**
1. Sprawdź Messaging endpoint: `https://rtmbot.sniezka.com/api/messages`
2. Sprawdź czy Teams Channel jest włączony

## ✅ **CHECKLIST KONFIGURACJI**

- [ ] Azure AD App Registration utworzona
- [ ] Client Secret wygenerowany i zapisany
- [ ] API Permissions dodane i zatwierdzone przez admina
- [ ] Authentication skonfigurowane z Redirect URI
- [ ] Azure Bot Service utworzony
- [ ] Messaging endpoint ustawiony
- [ ] Calling bot włączony z webhook endpoint
- [ ] Teams Channel skonfigurowany
- [ ] `appsettings.json` uzupełniony
- [ ] `manifest.json` zaktualizowany z App ID
- [ ] Ikony utworzone
- [ ] Aplikacja Teams spakowana
- [ ] Bot zainstalowany w Teams
- [ ] Aplikacja uruchomiona i logi OK
- [ ] Test wiadomości tekstowych działa
- [ ] Test połączeń działa

**Po wykonaniu wszystkich kroków bot będzie w pełni funkcjonalny!** 🎉
