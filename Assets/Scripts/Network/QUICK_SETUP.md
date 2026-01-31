# 🚀 SZYBKI SETUP MULTIPLAYER

## ✅ Co naprawiłem:

1. **Ruch działa** - skrypty MovementSpine i GridMovement działają w multiplayer
2. **Kamery automatyczne** - kamery znajdą graczy i będą ich śledzić
3. **Input System** - wszystko kompatybilne z waszym Input System

---

## 📦 Nowe pliki:

- `NetworkPlayerController.cs` - automatyczna konfiguracja gracza (ruch + kamera)
- `CameraFollow.cs` - kamera śledząca gracza
- `InputHelper.cs` - helper dla input (działa w single i multi)

---

## 🎮 SETUP W UNITY (15 minut):

### 1. Prefaby Graczy

**WAŻNE:** Oba prefaby muszą mieć tę samą strukturę!

```
OperatorPlayer / RunnerPlayer (prefab)
├── NetworkObject ← DODAJ!
├── NetworkPlayerController ← DODAJ!
├── MovementSpine (Operator) / GridMovement (Runner)
└── Model/Animator
```

**Krok po kroku:**

1. Otwórz prefab **OperatorPlayer**
2. Dodaj komponent: `NetworkObject`
3. Dodaj komponent: `NetworkPlayerController`
4. W NetworkPlayerController:
   - Przypisz `Movement Spine` → przeciągnij komponent MovementSpine
   - `Grid Movement` → zostaw puste
5. Zarejestruj prefab w **NetworkManager → Network Prefabs**

6. Powtórz dla **RunnerPlayer**:
   - Dodaj `NetworkObject`
   - Dodaj `NetworkPlayerController`
   - W NetworkPlayerController:
     - `Movement Spine` → zostaw puste
     - Przypisz `Grid Movement` → przeciągnij komponent GridMovement

---

### 2. Kamery

**Operator Camera:**
1. W scenie znajdź kamerę dla Operatora
2. Dodaj **Tag**: `OperatorCamera`
3. **Wyłącz** kamerę (unchecked w inspektorze)
4. Ustaw pozycję: np. powyżej strefy Operatora

**Grid Camera (Runner):**
1. W scenie znajdź kamerę dla Runnera
2. Dodaj **Tag**: `GridCamera`
3. **Wyłącz** kamerę
4. Ustaw pozycję: np. z góry patrząc na mapę Runnera (widok siatki)

**Tworzenie Tagów:**
- Edit → Project Settings → Tags and Layers
- Dodaj: `OperatorCamera`
- Dodaj: `GridCamera`

---

### 3. NetworkGameManager

W inspektorze **GameManager** → **NetworkGameManager**:
- `Operator Spawn Point` → punkt w OperatorZone
- `Runner Spawn Point` → punkt w RunnerZone
- `Operator Prefab` → przeciągnij OperatorPlayer prefab
- `Runner Prefab` → przeciągnij RunnerPlayer prefab
- `Game Duration` → 300

---

### 4. Serwery z Kablami

Na **każdym** ServerConnection:
1. Dodaj komponent `NetworkCableInteraction`
2. Ustaw:
   - `Light Circuit` → kolor (Red/Green/Blue/Yellow)
   - `Danger Type` → typ (Fire/Water/Electric/Toxic)

---

## 🧪 TEST:

### W edytorze:
1. Play
2. Naciśnij **H** (Host)
3. **Powinieneś widzieć:**
   - Gracz się pojawił (Runner)
   - Kamera śledzi gracza
   - Możesz się poruszać (WASD/Strzałki)
   - W konsoli: "Gracz X skonfigurowany"

### Build + drugi gracz:
1. Build gry
2. Uruchom build, naciśnij **J** (Join)
3. **Powinieneś widzieć:**
   - Drugi gracz się pojawił (Operator)
   - Kamera śledzi Operatora
   - Operator może się poruszać
   - Operator może łączyć kable (E)
   - Runner widzi zmiany świateł/hazardów

---

## 🔧 Debugowanie:

### "Nie mogę się poruszać"
✅ Sprawdź konsolę - powinno być: `[NetworkPlayerController] Gracz X skonfigurowany`
✅ Sprawdź czy prefab ma `NetworkPlayerController`
✅ Sprawdź czy `MovementSpine`/`GridMovement` są przypisane w inspektorze

### "Kamera nie śledzi"
✅ Sprawdź czy kamery mają tagi `OperatorCamera` i `GridCamera`
✅ Sprawdź czy kamery są **wyłączone** przed startem gry

### "Operator nie może łączyć kabli"
✅ Sprawdź czy ServerConnection ma `NetworkCableInteraction`
✅ Sprawdź czy `Light Circuit` i `Danger Type` są ustawione

---

## 📊 Jak to działa:

```
1. Gracz się spawnuje
   ↓
2. NetworkPlayerController sprawdza rolę (Operator/Runner)
   ↓
3. Włącza odpowiedni skrypt ruchu
   ↓
4. Znajduje kamerę po tagu
   ↓
5. Dodaje CameraFollow → kamera śledzi gracza
   ↓
6. Gotowe! Gracz może się poruszać!
```

---

## ✅ Checklist przed testem:

- [ ] NetworkManager + UnityTransport w scenie
- [ ] GameManager + NetworkGameManager w scenie (osobny GameObject!)
- [ ] NetworkGameManager ma wypełnione wszystkie pola
- [ ] Oba prefaby mają NetworkObject + NetworkPlayerController
- [ ] Oba prefaby zarejestrowane w Network Prefabs
- [ ] Kamery mają tagi i są wyłączone
- [ ] Każdy serwer ma NetworkCableInteraction

**Jeśli wszystko zaznaczone - testuj!** 🎮

---

## 🎯 Szybkie klawiszologie:

- **H** - Host (w menu)
- **J** - Join localhost (w menu)
- **ESC** - Rozłącz
- **WASD/Strzałki** - Ruch
- **E** - Podnieś/Podłącz kabel (Operator)

Powodzenia! 🚀

