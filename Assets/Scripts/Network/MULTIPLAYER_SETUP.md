# 🎮 Multiplayer Setup Guide

## 🎬 Przepływ gry
```
Menu (scena) → Start → NetowrkScene (scena z grą)
                            │
                            ├─ Host (H) → Runner
                            └─ Join (J) → Operator
```

---

## Opis gry
Asymetryczna gra kooperacyjna dla 2 graczy:
- **Operator (Host)** - porusza się po serwerowni, łączy kable między serwerami
- **Runner (Client)** - porusza się po osobnej mapie, gdzie zmieniają się światła i hazardy w zależności od kabli podłączonych przez Operatora

---

## 🎬 Menu Scene Setup

### Build Settings
1. File → Build Settings
2. Dodaj sceny w kolejności:
   - `Menu` (index 0)
   - `NetowrkScene` (index 1)

### Scena Menu
1. Utwórz Canvas z przyciskami:
   - Button "Start" → OnClick: `MainMenu.OnStartClicked()`
   - Button "Credits" → OnClick: `MainMenu.OnCreditsClicked()`
   - Button "Exit" → OnClick: `MainMenu.OnExitClicked()`

2. Dodaj pusty GameObject `MainMenu`:
   - Dodaj komponent `MainMenu`
   - Ustaw `Game Scene Name` = "NetowrkScene"
   - Opcjonalnie przypisz `Credits Panel`

3. **NIE dodawaj NetworkManager na scenie Menu** - będzie na scenie gry

---

## 📋 Szybki Setup (Checklist)

### 1. NetworkManager
- [ ] Dodaj pusty GameObject `NetworkManager`
- [ ] Dodaj komponent `NetworkManager` (Unity Netcode)
- [ ] Dodaj komponent `UnityTransport`
- [ ] **WAŻNE**: Zostaw pole "Player Prefab" PUSTE (spawnujemy ręcznie)

### 2. NetworkGameManager
- [ ] Dodaj pusty GameObject `NetworkGameManager`
- [ ] Dodaj komponent `NetworkObject`
- [ ] Dodaj komponent `NetworkGameManager`
- [ ] Przypisz:
  - `Operator Spawn Point` - Transform w serwerowni
  - `Runner Spawn Point` - Transform na mapie Runnera
  - `Operator Prefab` - prefab CableEnjoyer
  - `Runner Prefab` - prefab GridWalker
  - `Game Duration` - czas gry w sekundach (domyślnie 300)

### 3. Prefaby graczy

#### CableEnjoyer (Operator)
- [ ] Dodaj komponent `NetworkObject`
- [ ] Dodaj komponent `NetworkPlayerController`
- [ ] W `NetworkPlayerController` przypisz:
  - `Movement Spine` - komponent MovementSpine
  - `Grid Movement` - zostaw puste
  - `Camera Offset` - np. (0, 10, -10)
- [ ] Upewnij się że ma `Rigidbody` (dla MovementSpine)
- [ ] Tag: `Player`

#### GridWalker (Runner)
- [ ] Dodaj komponent `NetworkObject`
- [ ] Dodaj komponent `NetworkPlayerController`
- [ ] W `NetworkPlayerController` przypisz:
  - `Movement Spine` - zostaw puste
  - `Grid Movement` - komponent GridMovement
  - `Camera Offset` - np. (0, 10, -10)
- [ ] Tag: `Player`

### 4. Kamery
- [ ] Utwórz `OperatorCamera` z tagiem `OperatorCamera`
- [ ] Utwórz `GridCamera` z tagiem `GridCamera`
- [ ] Obie kamery na starcie powinny być **wyłączone** (`SetActive(false)`)
- [ ] NetworkPlayerController automatycznie włączy właściwą kamerę

### 5. Tagi (Project Settings → Tags and Layers)
- [ ] `Player`
- [ ] `Server`
- [ ] `OperatorCamera`
- [ ] `GridCamera`

### 6. Serwery (dla kabli)
Dla każdego obiektu Server:
- [ ] Dodaj komponent `ServerConnection`
- [ ] Dodaj komponent `NetworkObject`
- [ ] Dodaj komponent `NetworkCableInteraction`
- [ ] Tag: `Server`
- [ ] Ustaw:
  - `Server Color` - kolor kabla (Yellow/Red/Green/Blue)
  - `Tile Light Circuit` - który obwód świateł włączyć
  - `Danger Type` - jaki typ hazardu aktywować

### 7. UI Połączenia (opcjonalne)
- [ ] Dodaj Canvas z przyciskami Host/Join
- [ ] Dodaj komponent `NetworkConnectionUI`
- [ ] Przypisz przyciski i pola tekstowe

---

## 🎯 Jak to działa

```
┌─────────────────────────────────────────────────────────────┐
│                         HOST (Operator)                      │
│  1. Gracz porusza się (MovementSpine)                       │
│  2. Podnosi kabel (E) z CableHolder                         │
│  3. Podłącza kabel do serwera (E)                           │
│  4. ServerConnection → NetworkCableInteraction              │
│  5. ServerRpc → Serwer przetwarza                           │
└──────────────────────────┬──────────────────────────────────┘
                           │ ClientRpc
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                        CLIENT (Runner)                       │
│  1. Otrzymuje ClientRpc                                      │
│  2. TileManager.SetupLights() - włącza światła              │
│  3. TileManager.SetupDangers() - aktywuje hazardy           │
│  4. Gracz widzi zmiany i musi je omijać (GridMovement)      │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔧 Sterowanie

| Klawisz | Operator | Runner |
|---------|----------|--------|
| WASD | Ruch | Ruch (grid) |
| E | Podnieś kabel / Podłącz do serwera | - |
| F | Odłóż kabel / Odłącz od serwera | - |
| H | Host (przed połączeniem) | - |
| J | Join (przed połączeniem) | - |
| ESC | Rozłącz | Rozłącz |

---

## 🚀 Testowanie

### Lokalnie (jeden komputer)
1. Zbuduj grę (File → Build)
2. Uruchom build - naciśnij **J** (Join)
3. W edytorze naciśnij Play, potem **H** (Host)
4. Build automatycznie połączy się z edytorem

### Przez sieć
1. Host: Uruchom grę, naciśnij **H**
2. Client: Uruchom grę, wpisz IP hosta, naciśnij **J**
3. Domyślny port: 7777

---

## ⚠️ Częste problemy

### Gracze nie spawnują się
- Sprawdź czy `NetworkManager` ma PUSTE pole "Player Prefab"
- Sprawdź czy prefaby mają `NetworkObject`
- Sprawdź czy spawn pointy są przypisane

### Kamera nie działa
- Sprawdź tagi: `OperatorCamera`, `GridCamera`
- Kamery muszą być wyłączone na starcie
- Sprawdź czy `NetworkPlayerController` jest na prefabach

### Ruch nie działa
- Operator: Sprawdź `Rigidbody` na prefabie
- Runner: Sprawdź `GridMovement` bounds
- Sprawdź czy `InputHelper.cs` istnieje

### Kable nie działają w multiplayer
- Serwery muszą mieć `NetworkCableInteraction`
- Serwery muszą mieć `NetworkObject`

---

## 📁 Struktura plików

```
Scripts/
├── Network/
│   ├── NetworkGameManager.cs    - role, timer, spawn
│   ├── NetworkPlayerController.cs - ruch, kamera
│   ├── NetworkCableInteraction.cs - kable → sieć
│   ├── NetworkTileManager.cs    - kafelki (opcjonalnie)
│   ├── NetworkConnectionUI.cs   - UI host/join
│   ├── NetworkTimerUI.cs        - wyświetlanie timera
│   └── CameraFollow.cs          - śledzenie gracza
│
├── InputHelper.cs      - abstrakcja inputu (single/multi)
├── MovementSpine.cs    - ruch Operatora (Rigidbody)
├── GridMovement.cs     - ruch Runnera (grid)
├── AbstractPlayer.cs   - bazowa klasa animacji
├── GameManager.cs      - single-player manager
├── TileManager.cs      - zarządzanie kafelkami
├── Tile.cs             - pojedynczy kafelek
├── Danger.cs           - hazard
└── DangerList.cs       - lista prefabów hazardów

Assets/
├── CableHolder.cs      - trzymanie kabla
├── ServerConnection.cs - podłączanie kabli
└── CableVisualizer.cs  - wizualizacja kabla (spline)
```

---

## ✅ Gotowe!

Jeśli wszystko jest skonfigurowane:
1. Host widzi serwerownię i może łączyć kable
2. Client widzi mapę Runnera
3. Podłączenie kabla u Hosta → światła i hazardy u Clienta
4. Timer odlicza wspólny czas

Powodzenia! 🎮

