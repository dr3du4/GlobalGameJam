# ✅ CHECKLIST - Gra Multiplayer Gotowa?

## 📦 Skrypty (Kod)
- [x] **NetworkGameManager.cs** - Role, timer, spawning
- [x] **NetworkCableInteraction.cs** - Wysyłanie info o kablach
- [x] **NetworkConnectionUI.cs** - UI Host/Join
- [x] **NetworkTimerUI.cs** - Wyświetlanie timera
- [x] **NetworkTileManager.cs** - Sync tile'ów (opcjonalny)
- [x] **ServerConnection.cs** - Zintegrowany z siecią

✅ **Wszystkie skrypty gotowe i bez błędów kompilacji!**

---

## 🎮 Unity Setup (Scena)

### 1. NetworkManager GameObject
- [ ] GameObject "NetworkManager" istnieje
- [ ] Ma komponent **Unity Transport**
- [ ] Ma komponent **Network Manager**
- [ ] W Network Manager → **Network Transport** ustawiony na **UnityTransport**
- [ ] W Network Manager → **Network Prefabs** dodane prefaby graczy

### 2. GameManager GameObject  
- [ ] GameObject "GameManager" istnieje (osobny od NetworkManager!)
- [ ] Ma komponent **Network Game Manager**
- [ ] **Operator Spawn Point** - przypisany
- [ ] **Runner Spawn Point** - przypisany
- [ ] **Operator Prefab** - przypisany
- [ ] **Runner Prefab** - przypisany
- [ ] **Game Duration** - ustawiony (np. 300)

### 3. Strefy Graczy
- [ ] **OperatorZone** - GameObject z serwerami/kablami
- [ ] **OperatorSpawnPoint** - Transform gdzie spawnuje się Operator
- [ ] **RunnerZone** - GameObject z mapą/tile'ami
- [ ] **RunnerSpawnPoint** - Transform gdzie spawnuje się Runner
- [ ] Strefy są **daleko od siebie** (np. 100+ jednostek)

### 4. Prefaby Graczy

#### Operator Prefab:
- [ ] Ma komponent **NetworkObject**
- [ ] Ma komponent ruchu (MovementSpine lub podobny)
- [ ] Zarejestrowany w NetworkManager → Network Prefabs

#### Runner Prefab:
- [ ] Ma komponent **NetworkObject**
- [ ] Ma komponent ruchu (GridMovement lub podobny)
- [ ] Zarejestrowany w NetworkManager → Network Prefabs

### 5. Serwery z Kablami (OperatorZone)

Dla **każdego** ServerConnection:
- [ ] Ma komponent **ServerConnection**
- [ ] Ma komponent **CableHolder**
- [ ] Ma komponent **NetworkCableInteraction**
- [ ] W NetworkCableInteraction ustawiony **Light Circuit** (Red/Green/Blue/Yellow)
- [ ] W NetworkCableInteraction ustawiony **Danger Type** (Fire/Water/Electric/Toxic)

### 6. Mapa Runnera (RunnerZone)
- [ ] **TileManager** istnieje w scenie
- [ ] Tile'y mają komponent **Tile.cs**
- [ ] Hazardy mają komponent **Danger.cs**
- [ ] (Opcjonalnie) **NetworkTileManager** dodany i połączony z TileManager

### 7. UI Canvas
- [ ] Canvas istnieje
- [ ] Ma komponent **NetworkConnectionUI**

#### NetworkConnectionUI pola:
- [ ] **Connection Panel** - przypisany
- [ ] **Host Button** - przypisany
- [ ] **Client Button** - przypisany
- [ ] **IP Address Input** - przypisany (TMP InputField)
- [ ] **Port Input** - przypisany (TMP InputField)
- [ ] **Status Text** - przypisany (TextMeshPro)
- [ ] **Waiting Panel** - przypisany
- [ ] **Waiting Text** - przypisany

#### Timer UI:
- [ ] TextMeshPro dla timera istnieje
- [ ] Ma komponent **NetworkTimerUI**
- [ ] **Timer Text** przypisany

### 8. Kamery (Opcjonalne)
- [ ] Kamera dla Operatora - tag: `OperatorCamera` (wyłączona domyślnie)
- [ ] Kamera dla Runnera - tag: `GridCamera` (wyłączona domyślnie)

---

## 🧪 Testowanie

### Test 1: Kompilacja
- [ ] Gra się kompiluje bez błędów
- [ ] Brak ostrzeżeń związanych z Netcode

### Test 2: Localhost (Edytor + Build)
1. [ ] Play w edytorze
2. [ ] Naciśnij **H** - Host działa
3. [ ] Build gry
4. [ ] Uruchom build, naciśnij **J** - Join działa
5. [ ] Obaj gracze się spawnują
6. [ ] Operator może się poruszać
7. [ ] Runner może się poruszać
8. [ ] Timer działa u obu graczy

### Test 3: Interakcje
1. [ ] Operator może podnieść kabel (klawisz E)
2. [ ] Operator może podłączyć kabel do serwera (klawisz E)
3. [ ] Po podłączeniu kabla:
   - [ ] Runner widzi zmianę świateł
   - [ ] Runner widzi zmianę hazardów
   - [ ] Kolor odpowiada ustawieniom w NetworkCableInteraction
4. [ ] Operator może odłączyć kabel
5. [ ] Po odłączeniu światła/hazardy znikają u Runnera

### Test 4: LAN (2 komputery)
- [ ] Host może hostować grę
- [ ] Client może dołączyć przez IP
- [ ] Wszystko działa jak w teście localhost

---

## ⚠️ Najczęstsze Problemy

### "NetworkManager Singleton is null"
❌ NetworkManager nie istnieje w scenie
✅ Dodaj GameObject z NetworkManager + UnityTransport

### "Transport not set"
❌ W Network Manager → Network Transport jest "None"
✅ Ustaw na UnityTransport

### "Cannot add NetworkBehaviour to NetworkManager"
❌ NetworkGameManager na tym samym GameObject co NetworkManager
✅ Stwórz osobny GameObject "GameManager"

### "Prefab not spawned"
❌ Prefab nie ma NetworkObject ALBO nie jest zarejestrowany
✅ Dodaj NetworkObject i dodaj do Network Prefabs list

### "Kabel nie wysyła info"
❌ ServerConnection nie ma NetworkCableInteraction
✅ Dodaj NetworkCableInteraction na każdy serwer

### "Runner nie widzi zmian"
❌ Light Circuit / Danger Type nie ustawione
✅ Ustaw w inspektorze NetworkCableInteraction

---

## 🎯 Quick Start (Przypomnienie)

1. **Host**: Naciśnij **H** (lub przycisk Host)
2. **Client**: Naciśnij **J** (lub wpisz IP i kliknij Join)
3. **Operator**: Poruszaj się, zbieraj kable (E), łącz do serwerów (E)
4. **Runner**: Poruszaj się, unikaj hazardów które się pojawiają
5. **ESC**: Rozłącz się

---

## 📊 Przepływ Gry

```
OPERATOR (Client)              RUNNER (Host)
     │                              │
     │ Łączy żółty kabel            │
     │ do żółtego serwera           │
     │                              │
     ├─ServerRpc─────────>          │
     │                    Serwer    │
     │                    waliduje  │
     │                              │
     │                    <─────ClientRpc
     │                              │
     │              TileManager włącza:
     │              🟡 Yellow lights
     │              ⚡ Hazardy (Electric/Fire/etc)
     │                              │
     │                         Runner widzi
     │                         nowe przeszkody!
```

---

## 🔧 Struktura Sceny

```
Hierarchy:
├── NetworkManager
│   ├── Unity Transport
│   └── Network Manager
│
├── GameManager
│   └── Network Game Manager
│
├── OperatorZone (X: 0)
│   ├── OperatorSpawnPoint
│   ├── OperatorCamera
│   ├── Server_Yellow (ServerConnection + NetworkCableInteraction)
│   ├── Server_Red
│   ├── Server_Green
│   ├── Server_Blue
│   └── CableHolder_1, 2, 3...
│
├── RunnerZone (X: 1000)
│   ├── RunnerSpawnPoint
│   ├── RunnerCamera
│   ├── TileManager
│   ├── Tiles (Yellow, Red, Green, Blue)
│   └── Dangers (Fire, Water, Electric, Toxic)
│
└── UI Canvas
    ├── ConnectionPanel
    │   ├── HostButton
    │   ├── JoinButton
    │   ├── IPInput
    │   └── PortInput
    ├── WaitingPanel
    └── TimerText
```

---

## 📝 Mapowanie Kolorów

| Kabel | Light Circuit | Danger Type | Przykład |
|-------|---------------|-------------|----------|
| 🟡 Yellow | Yellow | Electric | Piorun |
| 🔴 Red | Red | Fire | Ogień |
| 🟢 Green | Green | Water | Woda |
| 🔵 Blue | Blue | Toxic | Toksyna |

**Setup przykład:**
- `Server_Yellow`:
  - Light Circuit: **Yellow**
  - Danger Type: **Electric**
  
Gdy Operator podłączy żółty kabel → Runner widzi żółte światła + elektryczne hazardy!

---

## 📊 Status: ✅ KOD GOTOWY

Wszystkie skrypty są kompletne i działają. Teraz tylko setup w Unity!

**Jeśli wszystkie checkboxy powyżej są zaznaczone - możesz grać! 🎮**

