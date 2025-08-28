# ⚡ QUICK START - Real-Time Media Bot

## 🎯 **SZYBKA KONFIGURACJA (15 minut)**

### **KROK 1: Azure AD App Registration** ⏱️ 3 min
1. [Azure Portal](https://portal.azure.com) → **Azure AD** → **App registrations** → **New registration**
2. Nazwa: `Real-Time Media Bot`
3. Redirect URI: `https://rtmbot.sniezka.com/api/auth/callback`
4. **Zapisz:** Application ID, Tenant ID
5. **Certificates & secrets** → **New client secret** → **Zapisz Value**

### **KROK 2: API Permissions** ⏱️ 3 min
1. **API permissions** → **Add permission** → **Microsoft Graph** → **Application permissions**
2. **📞 Calling:** `Calls.AccessMedia.All`, `Calls.InitiateOutgoingCall.All`, `Calls.JoinGroupCall.All`
3. **💬 Messaging:** `Chat.ReadWrite.All`, `ChatMessage.Read.All`, `User.Read.All`
4. **Grant admin consent** ✅

### **KROK 3: Azure Bot Service** ⏱️ 3 min
1. **Create resource** → **Azure Bot**
2. Bot handle: `real-time-media-bot`
3. **Use existing app ID** → wklej Application ID
4. **Configuration** → Messaging endpoint: `https://rtmbot.sniezka.com/api/messages`
5. **Channels** → **Teams** → **Enable calling** → Webhook: `https://rtmbot.sniezka.com/api/calling`

### **KROK 4: Konfiguracja aplikacji** ⏱️ 2 min
Uzupełnij `appsettings.json`:
```json
{
  "AzureAd": {
    "TenantId": "TWOJ_TENANT_ID",
    "ClientId": "TWOJ_APPLICATION_ID", 
    "ClientSecret": "TWOJ_CLIENT_SECRET"
  },
  "Bot": {
    "MicrosoftAppId": "TWOJ_APPLICATION_ID",
    "MicrosoftAppPassword": "TWOJ_CLIENT_SECRET",
    "MicrosoftAppTenantId": "TWOJ_TENANT_ID"
  }
}
```

### **KROK 5: Teams Manifest** ⏱️ 3 min
1. W `manifest.json` zamień `TWOJ_BOT_APP_ID_TUTAJ` → Twój Application ID
2. Stwórz ikony: `icon-outline.png` (32x32), `icon-color.png` (192x192)
3. Spakuj: `Compress-Archive -Path manifest.json,icon-*.png -DestinationPath RealTimeMediaBot.zip`

### **KROK 6: Instalacja w Teams** ⏱️ 2 min
1. **Teams** → **Apps** → **Upload custom app** → wybierz `RealTimeMediaBot.zip`
2. **Add** → Bot dodany! 🎉

## 🚀 **URUCHOMIENIE I TEST**

```bash
# Uruchom aplikację
dotnet run

# Test wiadomości
# W Teams napisz do bota: help

# Test połączenia
# W Teams zadzwoń do bota - powinien automatycznie odebrać
```

## 📋 **QUICK CHECKLIST**
- [ ] Azure AD App + Secret
- [ ] API Permissions + Admin Consent  
- [ ] Bot Service + Endpoints
- [ ] appsettings.json uzupełniony
- [ ] manifest.json + ikony
- [ ] Bot w Teams zainstalowany
- [ ] `dotnet run` + testy OK

**Gotowe! Bot działa w Teams!** ✅

---

💡 **Szczegółowy przewodnik:** Zobacz `AZURE-SETUP.md`  
🔧 **Troubleshooting:** Zobacz sekcję w `AZURE-SETUP.md`
