# Asymetryczna Gra Multiplayer - Przewodnik

## Koncepcja Gry

```
OPERATOR (Host)                    RUNNER (Client)
┌─────────────────┐               ┌─────────────────┐
│  Pokój serwerów │               │   Mapa z siatką │
│                 │               │                 │
│ 🔴 Server Red   │──────────────>│ 🔴 Red hazards  │
│ 🟡 Server Yellow│──────────────>│ 🟡 Yellow lights│
│ 🟢 Server Green │──────────────>│ 🟢 Green tiles  │
│ 🔵 Server Blue  │──────────────>│ 🔵 Blue paths   │
│                 │               │                 │
│ [Łączy kable]   │               │ [Widzi zmiany]  │
└─────────────────┘               └─────────────────┘
```

## Przepływ Sieciowy

```
Operator łączy kabel do żółtego serwera
              ↓
ServerConnection.OnCablePluggedIn()
              ↓
NetworkCableInteraction.OnCableConnected()
              ↓
ServerRpc → NetworkGameManager (serwer)
              ↓
ClientRpc → Runner
              ↓
NetworkTileManager.SetLightCircuit(Yellow)
              ↓
TileManager włącza żółte światła i hazardy
```

## Pliki w Scripts/Network/

| Plik | Opis |
|------|------|
| `NetworkGameManager.cs` | Role graczy (Runner/Operator), timer, routing wiadomości |
| `NetworkCableInteraction.cs` | Wysyła info o podłączeniu kabla do serwera |
| `NetworkTileManager.cs` | Synchronizuje kolory świateł/hazardów na mapie Runnera |
| `NetworkConnectionUI.cs` | UI do Host/Join |
| `NetworkTimerUI.cs` | Wyświetla timer |
| `RunnerWorldController.cs` | Opcjonalny - do własnych akcji |

## Konfiguracja Krok po Kroku

### 1. Zainstaluj Netcode for GameObjects

Window → Package Manager → + → Add package by name:
```
com.unity.netcode.gameobjects
```

### 2. Utwórz NetworkManager

1. Utwórz pusty GameObject `NetworkManager`
2. Dodaj komponenty:
   - `NetworkManager`
   - `UnityTransport`
   - `NetworkGameManager`

### 3. Skonfiguruj NetworkGameManager

W inspektorze ustaw:
- `Operator Spawn Point` → pozycja w pokoju serwerów
- `Runner Spawn Point` → pozycja na mapie Runnera
- `Operator Prefab` → prefab gracza (z NetworkObject!)
- `Runner Prefab` → prefab gracza (z NetworkObject!)
- `Game Duration` → czas gry (sekundy)

### 4. Prefaby Graczy

**Ważne:** Oba prefaby muszą mieć `NetworkObject` component!

Zarejestruj je w NetworkManager → Network Prefabs.

### 5. Serwery z Kablami (Operator Zone)

Dla każdego ServerConnection:
1. Dodaj komponent `NetworkCableInteraction`
2. Ustaw `Interaction Id` odpowiadający kolorowi:
   - 0 = Red
   - 1 = Green  
   - 2 = Blue
   - 3 = Yellow

### 6. Mapa Runnera

1. Dodaj `NetworkTileManager` do sceny (w RunnerZone)
2. Przypisz istniejący `TileManager` do referencji

### 7. UI

Utwórz Canvas z:
- `NetworkConnectionUI` panel (Host/Join)
- `NetworkTimerUI` (timer)

## Testowanie

**Skróty klawiszowe:**
- `H` - Host (zostań serwerem)
- `J` - Join (dołącz do localhost)
- `ESC` - Rozłącz

**LAN:**
1. Host: Uruchom, kliknij Host
2. Client: Wpisz IP hosta, kliknij Join

## Mapowanie Kolorów

```csharp
// W Tile.cs i Danger.cs już masz:
public enum LightCircuit
{
    Red,    // = 0
    Green,  // = 1
    Blue,   // = 2
    Yellow  // = 3
}
```

Ustaw `Interaction Id` w `NetworkCableInteraction` tak samo:
- Server z czerwonym kablem → ID: 0
- Server z zielonym kablem → ID: 1
- Server z niebieskim kablem → ID: 2
- Server z żółtym kablem → ID: 3
