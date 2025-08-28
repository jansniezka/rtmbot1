# 🎨 Tworzenie ikon dla Teams Bot

## 📋 **WYMAGANIA IKON**

Teams wymaga dwóch ikon:
- **`icon-outline.png`** - 32x32 pikseli, przezroczyste tło, outline
- **`icon-color.png`** - 192x192 pikseli, kolorowa ikona

## 🎯 **OPCJE TWORZENIA IKON**

### **OPCJA 1: Proste ikony z emoji** ⚡ Najszybsze
```
🤖 - Bot emoji
📞 - Telefon  
🎧 - Słuchawki
🔊 - Głośnik
🎤 - Mikrofon
```

**Narzędzia online:**
- [Emoji to PNG](https://emoji-to-png.com/)
- [Emoji Kitchen](https://emojikitchen.dev/)

### **OPCJA 2: Ikony z bibliotek** 🎨 Profesjonalne
**Darmowe źródła:**
- [Heroicons](https://heroicons.com/) - `microphone`, `phone`, `speaker-wave`
- [Lucide Icons](https://lucide.dev/) - `phone-call`, `headphones`, `mic`
- [Tabler Icons](https://tabler-icons.io/) - `phone`, `microphone`, `headset`
- [Feather Icons](https://feathericons.com/) - `phone-call`, `headphones`

### **OPCJA 3: AI Generator** 🤖 Kreatywne
**Prompty dla AI:**
```
"32x32 pixel icon, simple line art, phone with sound waves, transparent background"
"192x192 pixel icon, modern bot head with headphones, blue and white colors"
"Simple microphone icon with audio waveform, minimalist design"
```

**Narzędzia AI:**
- [DALL-E](https://openai.com/dall-e-2/)
- [Midjourney](https://midjourney.com/)
- [Stable Diffusion](https://stablediffusionweb.com/)

### **OPCJA 4: Canva Template** 🎨 Łatwe
1. Otwórz [Canva](https://canva.com)
2. Utwórz custom size: 32x32 i 192x192
3. Wyszukaj: "bot icon", "phone icon", "microphone"
4. Dostosuj kolory: niebieski (#0078D4), biały
5. Pobierz jako PNG z przezroczystym tłem

## 🖼️ **PRZYKŁADOWE IKONY**

### **Koncepcja 1: Mikrofon z falami**
```
icon-outline.png (32x32): Prosty outline mikrofonu z falami dźwiękowymi
icon-color.png (192x192): Niebieski mikrofon z animowanymi falami
```

### **Koncepcja 2: Bot z słuchawkami**
```
icon-outline.png (32x32): Outline głowy bota ze słuchawkami
icon-color.png (192x192): Niebieski bot z białymi słuchawkami
```

### **Koncepcja 3: Telefon z audio**
```
icon-outline.png (32x32): Outline telefonu z symbolem audio
icon-color.png (192x192): Niebieski telefon z falami dźwiękowymi
```

## 🛠️ **SZYBKIE TWORZENIE W PHOTOSHOP/GIMP**

### **icon-outline.png (32x32):**
```
1. Nowy dokument: 32x32px, przezroczyste tło
2. Narzędzie Line/Pen: grubość 2px, kolor czarny
3. Narysuj prosty outline (mikrofon, telefon, słuchawki)
4. Eksport: PNG z przezroczystością
```

### **icon-color.png (192x192):**
```
1. Nowy dokument: 192x192px, przezroczyste tło
2. Kolory: niebieski (#0078D4), biały (#FFFFFF)
3. Narysuj szczegółową ikonę z kolorami
4. Dodaj cienie/gradienty dla głębi
5. Eksport: PNG z przezroczystością
```

## ⚡ **NAJSZYBSZA OPCJA - GOTOWE PLIKI**

Jeśli chcesz szybko przetestować, możesz użyć prostych kolorowych kwadratów:

### **Tymczasowe ikony do testów:**
```powershell
# PowerShell - stwórz podstawowe ikony
# (wymaga ImageMagick lub podobnego narzędzia)

# Lub po prostu pobierz dowolne 32x32 i 192x192 PNG
# i zmień nazwy na icon-outline.png i icon-color.png
```

## 📁 **STRUKTURA PLIKÓW**

```
real-time-media-bot/
├── manifest.json
├── icon-outline.png    (32x32px)
├── icon-color.png      (192x192px)
└── RealTimeMediaBot.zip (spakowane)
```

## ✅ **WERYFIKACJA IKON**

Przed pakowaniem sprawdź:
- [ ] `icon-outline.png` - dokładnie 32x32 pikseli
- [ ] `icon-color.png` - dokładnie 192x192 pikseli
- [ ] Oba pliki mają przezroczyste tło
- [ ] Ikony są czytelne i profesjonalne
- [ ] Kolory pasują do Teams (niebieski #0078D4)

## 🎨 **REKOMENDOWANE KOLORY TEAMS**

```css
Niebieski Teams: #0078D4
Ciemny niebieski: #005A9E
Biały: #FFFFFF
Szary: #8A8886
Czarny: #201F1E
```

**Po utworzeniu ikon możesz spakować manifest i zainstalować bota w Teams!** 📦✨
