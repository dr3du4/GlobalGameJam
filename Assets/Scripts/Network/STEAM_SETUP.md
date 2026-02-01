# 🎮 Steam Lobby Setup (Game Jam Edition)

## 📦 Krok 1: Instalacja pakietów

### A) Facepunch.Steamworks (przez NuGet for Unity lub ręcznie)

**Opcja 1 - Unity Package Manager (zalecane):**
1. Window → Package Manager
2. Kliknij `+` → "Add package from git URL"
3. Wklej: `https://github.com/Facepunch/Facepunch.Steamworks.git`

**Opcja 2 - Ręcznie:**
1. Pobierz z: https://github.com/Facepunch/Facepunch.Steamworks/releases
2. Rozpakuj do `Assets/Plugins/Facepunch.Steamworks`

### B) Netcode Transport for Steam

1. Window → Package Manager
2. Kliknij `+` → "Add package from git URL"
3. Wklej: `https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.facepunch`

**Jeśli nie działa**, dodaj ręcznie do `Packages/manifest.json`:
```json
"com.community.netcode.transport.facepunch": "https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.facepunch"
```

---

## 📦 Krok 2: Scripting Define Symbol

**WAŻNE**: Po zainstalowaniu pakietów dodaj symbol kompilacji:

1. Edit → Project Settings → Player
2. Znajdź "Scripting Define Symbols"
3. Dodaj: `FACEPUNCH_STEAMWORKS`
4. Kliknij "Apply"

Bez tego kodu Steam nie będzie działać!

---

## 📦 Krok 3: Plik steam_appid.txt

✅ **Już utworzony!** Sprawdź czy istnieje:
```
GlobalGameJam/
├── Assets/
├── Packages/
├── ProjectSettings/
└── steam_appid.txt  ← zawiera "480"
```

---

## 🔧 Krok 4: Konfiguracja NetworkManager

1. Na obiekcie `NetworkManager`:
   - Usuń komponent `UnityTransport`
   - Dodaj komponent `FacepunchTransport`

2. W `FacepunchTransport`:
   - Zostaw domyślne ustawienia

---

## 🎮 Krok 5: Dodaj Steam Managery na scenę

**WAŻNE!** Bez tego Steam nie zadziała!

### A) SteamManager
1. Utwórz pusty GameObject: `SteamManager`
2. Add Component → `SteamManager`
3. App ID zostaw `480` (testowe)

### B) SteamLobbyManager
1. Utwórz pusty GameObject: `SteamLobbyManager`
2. Add Component → `SteamLobbyManager`
3. Max Players zostaw `2`

### Hierarchia sceny:
```
Scene
├── NetworkManager (NetworkManager + FacepunchTransport)
├── NetworkGameManager (NetworkObject + NetworkGameManager)
├── SteamManager (SteamManager)        ← NOWE!
├── SteamLobbyManager (SteamLobbyManager)  ← NOWE!
├── Canvas
│   └── NetworkUI (NetworkConnectionUI)
└── ... reszta sceny
```

---

## 🎯 Krok 6: Użycie w grze

### Host:
1. Kliknij "HOSTER" (lub H na klawiaturze)
2. Gra tworzy Steam Lobby
3. Wyświetla się kod (np. "ABC123")
4. Podaj kod drugiemu graczowi

### Client:
1. Wpisz kod w pole
2. Kliknij "PROXY" / "Join" (lub J na klawiaturze)
3. Automatycznie łączy się przez Steam

---

## ⚠️ Wymagania

- **Steam musi być uruchomiony** podczas testowania
- Obaj gracze muszą mieć Steam
- Dla testowego App ID (480) - obaj muszą mieć grę "Spacewar" w bibliotece (jest darmowa)

---

## 🐛 Troubleshooting

### "Steam not initialized"
- Upewnij się że Steam jest uruchomiony
- Sprawdź czy `steam_appid.txt` jest w dobrym miejscu
- Zrestartuj Unity Editor

### "Lobby not found"
- Sprawdź czy kod jest poprawny
- Lobby wygasa po ~5 minutach bez aktywności
- Host musi być nadal w lobby

### "Connection failed"
- Sprawdź czy host nadal hostuje
- Spróbuj ponownie (Steam relay może potrzebować chwili)

