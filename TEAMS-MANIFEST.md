# 📋 Teams App Manifest - Real-Time Media Bot

## 🎯 **Instrukcja konfiguracji manifest.json**

### **1. 🔧 ZAMIEŃ PLACEHOLDERY:**

W pliku `manifest.json` zamień:

```json
"id": "TWOJ_BOT_APP_ID_TUTAJ",
"botId": "TWOJ_BOT_APP_ID_TUTAJ", 
"id": "TWOJ_BOT_APP_ID_TUTAJ"
```

**NA TWOJE RZECZYWISTE WARTOŚCI:**
- `TWOJ_BOT_APP_ID_TUTAJ` → Twój **Microsoft App ID** z Azure Bot Service

### **2. 🎨 DODAJ IKONY BOTA:**

Stwórz dwa pliki ikon:
- `icon-outline.png` - 32x32px, przezroczyste tło, outline
- `icon-color.png` - 192x192px, kolorowa ikona

**Przykład ikon:**
```
📁 manifest/
├── manifest.json
├── icon-outline.png  (32x32)
└── icon-color.png    (192x192)
```

### **3. 📦 STWÓRZ PAKIET TEAMS:**

```bash
# Spakuj wszystkie pliki do ZIP
zip -r RealTimeMediaBot.zip manifest.json icon-outline.png icon-color.png

# LUB w PowerShell:
Compress-Archive -Path manifest.json,icon-outline.png,icon-color.png -DestinationPath RealTimeMediaBot.zip
```

### **4. 🚀 ZAINSTALUJ W TEAMS:**

1. **Otwórz Microsoft Teams**
2. **Kliknij "Apps" w lewym menu**
3. **Kliknij "Upload a custom app"**
4. **Wybierz "Upload for [Twoja organizacja]"**
5. **Wybierz plik `RealTimeMediaBot.zip`**
6. **Kliknij "Add"**

## 🔍 **CO ZAWIERA MANIFEST:**

### **📞 CALLING BOT:**
```json
"supportsCalling": true,
"supportsVideo": false
```
- ✅ Bot może odbierać połączenia
- ✅ Bot może dołączać do spotkań
- ❌ Brak obsługi video (tylko audio)

### **💬 KOMENDY TEKSTOWE:**
```json
"commands": [
  { "title": "help", "description": "Wyświetla listę dostępnych komend" },
  { "title": "status", "description": "Pokazuje status bota i aktywnych połączeń" },
  { "title": "calls", "description": "Lista wszystkich aktywnych połączeń" },
  { "title": "audio", "description": "Informacje o buforze audio" }
]
```

### **🔐 UPRAWNIENIA:**
```json
"permissions": [
  "Calls.AccessMedia.All",           // Dostęp do audio media
  "Calls.InitiateOutgoingCall.All",  // Inicjowanie połączeń
  "Calls.JoinGroupCall.All",         // Dołączanie do spotkań
  "Calls.JoinGroupCallAsGuest.All"   // Dołączanie jako gość
]
```

### **🌐 DOMENY:**
```json
"validDomains": ["rtmbot.sniezka.com"]
```

## 🎯 **PRZYKŁAD KOMPLETNEGO MANIFESTU:**

```json
{
  "$schema": "https://developer.microsoft.com/en-us/json-schemas/teams/v1.16/MicrosoftTeams.schema.json",
  "manifestVersion": "1.16",
  "version": "1.0.0",
  "id": "12345678-1234-1234-1234-123456789abc",
  "developer": {
    "name": "Real-Time Media Bot Developer",
    "websiteUrl": "https://rtmbot.sniezka.com",
    "privacyUrl": "https://rtmbot.sniezka.com/privacy",
    "termsOfUseUrl": "https://rtmbot.sniezka.com/terms"
  },
  "name": {
    "short": "Real-Time Media Bot",
    "full": "Real-Time Media Bot - Audio Capture & Call Management"
  },
  "bots": [
    {
      "botId": "12345678-1234-1234-1234-123456789abc",
      "scopes": ["personal", "team", "groupchat"],
      "supportsCalling": true,
      "supportsVideo": false,
      "commandLists": [
        {
          "scopes": ["personal", "team", "groupchat"],
          "commands": [
            { "title": "help", "description": "Wyświetla listę dostępnych komend" },
            { "title": "status", "description": "Pokazuje status bota" },
            { "title": "calls", "description": "Lista aktywnych połączeń" },
            { "title": "audio", "description": "Informacje o buforze audio" }
          ]
        }
      ]
    }
  ]
}
```

## ⚠️ **WAŻNE UWAGI:**

### **🔧 KONFIGURACJA AZURE:**
1. **Bot Service Calling Endpoint:** `https://rtmbot.sniezka.com/api/calling`
2. **Bot Service Messaging Endpoint:** `https://rtmbot.sniezka.com/api/messages`
3. **Azure AD Redirect URI:** `https://rtmbot.sniezka.com/api/auth/callback`

### **📋 WYMAGANE UPRAWNIENIA W AZURE AD:**
- `Calls.AccessMedia.All`
- `Calls.InitiateOutgoingCall.All`
- `Calls.JoinGroupCall.All`
- `Calls.JoinGroupCallAsGuest.All`

### **🎯 TESTOWANIE:**
1. **Zainstaluj bota w Teams**
2. **Napisz wiadomość:** `help`
3. **Zadzwoń do bota** - powinien automatycznie odebrać
4. **Sprawdź logi aplikacji** - zobaczysz szczegółowe informacje

## 🚀 **PO INSTALACJI W TEAMS:**

Bot będzie dostępny jako:
- **Kontakt osobisty** - można do niego dzwonić
- **Członek zespołu** - można go dodać do kanałów
- **Uczestnik czatu grupowego** - można go dodać do czatów

**Gotowy do przechwytywania audio z połączeń Teams!** 🎧📞
