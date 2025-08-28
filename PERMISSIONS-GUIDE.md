# 🔐 Microsoft Graph Permissions - Kompletny przewodnik

## 📋 **WSZYSTKIE WYMAGANE UPRAWNIENIA**

### **📞 CALLING & MEDIA PERMISSIONS**
```
Calls.AccessMedia.All                    - Dostęp do audio/video media w połączeniach
Calls.InitiateOutgoingCall.All          - Inicjowanie połączeń wychodzących  
Calls.JoinGroupCall.All                 - Dołączanie do połączeń grupowych/spotkań
Calls.JoinGroupCallAsGuest.All          - Dołączanie do spotkań jako gość
```

### **💬 MESSAGING PERMISSIONS**
```
Chat.ReadWrite.All                     - Odczyt i zapis wszystkich czatów (włącza wysyłanie wiadomości)
ChatMessage.Read.All                   - Odczytywanie wszystkich wiadomości w czatach
Team.ReadBasic.All                     - Odczytywanie podstawowych info o zespołach
User.Read.All                          - Odczytywanie profili użytkowników
Directory.Read.All                     - Odczytywanie katalogu organizacji (opcjonalne)
```

### **⚠️ WAŻNE WYJAŚNIENIE UPRAWNIEŃ:**
**Microsoft Graph NIE MA bezpośrednich uprawnień `ChannelMessage.Send` czy `ChatMessage.Send`!**

**Zamiast tego używamy:**
- `Chat.ReadWrite.All` - daje możliwość wysyłania wiadomości w czatach
- Bot Framework automatycznie obsługuje wysyłanie przez endpoint `/api/messages`
- Teams Bot nie potrzebuje osobnych uprawnień do wysyłania - robi to przez Bot Service

### **🔍 OPCJONALNE PERMISSIONS (dla rozszerzonych funkcji)**
```
Directory.Read.All                     - Odczytywanie katalogu organizacji
Group.Read.All                         - Odczytywanie grup i zespołów
Presence.Read.All                      - Odczytywanie statusu obecności użytkowników
Calendar.Read.All                      - Odczytywanie kalendarzy (dla spotkań)
Files.Read.All                         - Dostęp do plików SharePoint/OneDrive
```

## 🎯 **DLACZEGO POTRZEBUJEMY KAŻDEGO UPRAWNIENIA?**

### **📞 CALLING PERMISSIONS - SZCZEGÓŁY:**

#### `Calls.AccessMedia.All`
- **Do czego:** Przechwytywanie audio/video stream z połączeń
- **Nasz bot:** ✅ Kluczowe - do real-time audio capture
- **Przykład:** Dostęp do `AudioMediaBuffer`, `VideoMediaBuffer`

#### `Calls.InitiateOutgoingCall.All`
- **Do czego:** Bot może sam inicjować połączenia
- **Nasz bot:** ⚠️ Opcjonalne - jeśli chcemy aby bot dzwonił do użytkowników
- **Przykład:** `POST /communications/calls`

#### `Calls.JoinGroupCall.All`
- **Do czego:** Dołączanie do spotkań Teams jako uczestnik
- **Nasz bot:** ✅ Ważne - dla spotkań zespołowych
- **Przykład:** Dołączenie do meeting przez meeting URL

#### `Calls.JoinGroupCallAsGuest.All`
- **Do czego:** Dołączanie do spotkań zewnętrznych jako gość
- **Nasz bot:** ✅ Ważne - dla spotkań z zewnętrznymi uczestnikami
- **Przykład:** Spotkania z partnerami biznesowymi

### **💬 MESSAGING PERMISSIONS - SZCZEGÓŁY:**

#### `ChannelMessage.Send`
- **Do czego:** Wysyłanie wiadomości do kanałów Teams
- **Nasz bot:** ✅ Potrzebne - dla odpowiedzi na komendy w kanałach
- **Przykład:** Odpowiedź na `@bot help` w kanale

#### `ChatMessage.Send`
- **Do czego:** Wysyłanie wiadomości w czatach 1:1 i grupowych
- **Nasz bot:** ✅ Kluczowe - dla odpowiedzi na wiadomości prywatne
- **Przykład:** Odpowiedź na `help` w czacie prywatnym

#### `Chat.Read.All`
- **Do czego:** Odczytywanie treści czatów
- **Nasz bot:** ✅ Potrzebne - aby zrozumieć kontekst wiadomości
- **Przykład:** Odczytanie historii czatu dla lepszych odpowiedzi

#### `User.Read.All`
- **Do czego:** Odczytywanie profili użytkowników
- **Nasz bot:** ✅ Przydatne - aby personalizować odpowiedzi
- **Przykład:** "Cześć Jan!" zamiast "Cześć Użytkowniku!"

## ⚠️ **UPRAWNIENIA WYSOKIEGO RYZYKA**

Niektóre uprawnienia wymagają dodatkowej uwagi administratora:

### **🔴 WYSOKIE RYZYKO:**
```
Chat.Read.All           - Dostęp do wszystkich czatów w organizacji
User.Read.All          - Dostęp do wszystkich profili użytkowników  
Team.ReadBasic.All     - Dostęp do wszystkich zespołów
Directory.Read.All     - Dostęp do całego katalogu AD
```

### **🟡 ŚREDNIE RYZYKO:**
```
ChannelMessage.Send    - Może wysyłać wiadomości do kanałów
ChatMessage.Send       - Może wysyłać wiadomości prywatne
Calls.AccessMedia.All  - Dostęp do audio/video z połączeń
```

### **🟢 NISKIE RYZYKO:**
```
Calls.JoinGroupCall.All     - Tylko dołączanie do spotkań
TeamsAppInstallation.*      - Zarządzanie własnymi instalacjami
```

## 📋 **MINIMALNE UPRAWNIENIA DLA RÓŻNYCH SCENARIUSZY**

### **🎧 TYLKO AUDIO CAPTURE (bez messaging):**
```
Calls.AccessMedia.All
Calls.JoinGroupCall.All
Calls.JoinGroupCallAsGuest.All
```

### **💬 TYLKO MESSAGING (bez calling):**
```
ChannelMessage.Send
ChatMessage.Send
Chat.Read.All
User.Read.All
```

### **🤖 PEŁNY BOT (calling + messaging):**
```
# Calling
Calls.AccessMedia.All
Calls.InitiateOutgoingCall.All
Calls.JoinGroupCall.All
Calls.JoinGroupCallAsGuest.All

# Messaging  
ChannelMessage.Send
ChatMessage.Send
Chat.Read.All
User.Read.All
Team.ReadBasic.All
```

## 🛠️ **JAK DODAĆ UPRAWNIENIA W AZURE PORTAL**

### **KROK PO KROKU:**
1. **Azure Portal** → **Azure AD** → **App registrations**
2. Wybierz swoją aplikację
3. **API permissions** → **Add a permission**
4. **Microsoft Graph** → **Application permissions**
5. Wyszukaj i zaznacz każde uprawnienie z listy
6. **Add permissions**
7. ⚠️ **KLUCZOWE:** **Grant admin consent for [Organization]**
8. Potwierdź **Yes**

### **WERYFIKACJA UPRAWNIEŃ:**
Po dodaniu powinieneś zobaczyć:
```
Status: ✅ Granted for [Organization]
Type: Application
```

## 🔍 **TROUBLESHOOTING UPRAWNIEŃ**

### **❌ Problem: "Insufficient privileges to complete the operation"**
**Rozwiązanie:** Brakuje admin consent - kliknij "Grant admin consent"

### **❌ Problem: "AADSTS65001: The user or administrator has not consented"**
**Rozwiązanie:** Uprawnienia nie zostały zatwierdzone przez administratora

### **❌ Problem: Bot nie może wysyłać wiadomości**
**Rozwiązanie:** Sprawdź czy masz `ChatMessage.Send` i `ChannelMessage.Send`

### **❌ Problem: Bot nie może dołączyć do spotkania**
**Rozwiązanie:** Sprawdź czy masz `Calls.JoinGroupCall.All`

### **❌ Problem: Bot nie przechwytuje audio**
**Rozwiązanie:** Sprawdź czy masz `Calls.AccessMedia.All`

## ✅ **CHECKLIST UPRAWNIEŃ**

**CALLING:**
- [ ] `Calls.AccessMedia.All` ✅ 
- [ ] `Calls.InitiateOutgoingCall.All` ✅
- [ ] `Calls.JoinGroupCall.All` ✅
- [ ] `Calls.JoinGroupCallAsGuest.All` ✅

**MESSAGING:**
- [ ] `ChannelMessage.Send` ✅
- [ ] `ChatMessage.Send` ✅
- [ ] `Chat.Read.All` ✅
- [ ] `User.Read.All` ✅
- [ ] `Team.ReadBasic.All` ✅

**ADMIN CONSENT:**
- [ ] Wszystkie uprawnienia mają status "Granted" ✅
- [ ] Brak błędów w Event Log ✅

**Po dodaniu wszystkich uprawnień bot będzie w pełni funkcjonalny!** 🎉
