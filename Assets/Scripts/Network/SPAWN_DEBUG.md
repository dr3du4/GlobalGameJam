# 🐛 DEBUG: Gracze nie spawnują się w spawn pointach

## ✅ CHECKLIST - Sprawdź po kolei:

### 1. W NetworkManager (GameObject):
- [ ] **Default Player Prefab** = **NONE** (musi być puste!)
  - Jeśli coś tam jest → usuń to
  - My spawnujemy ręcznie, nie automatycznie

### 2. W GameManager → NetworkGameManager (Inspector):
- [ ] **Operator Spawn Point** - przypisany (przeciągnij Transform)
- [ ] **Runner Spawn Point** - przypisany (przeciągnij Transform)
- [ ] **Operator Prefab** - przypisany (przeciągnij prefab)
- [ ] **Runner Prefab** - przypisany (przeciągnij prefab)

### 3. Prefaby:
- [ ] Oba prefaby mają komponent **NetworkObject**
- [ ] Oba prefaby zarejestrowane w **NetworkManager → Network Prefabs** (lista)

### 4. Spawn Pointy:
- [ ] Istnieją w scenie (Empty GameObjects)
- [ ] Mają sensowne pozycje (nie (0,0,0))
  - OperatorSpawnPoint: np. (0, 0, 0) w strefie Operatora
  - RunnerSpawnPoint: np. (100, 0, 0) w strefie Runnera

---

## 📊 Co powinieneś widzieć w konsoli:

Po naciśnięciu **H** (Host):
```
[NetworkGameManager] Gracz 0 dołączył
[NetworkGameManager] Przydzielono Runner graczowi 0
[NetworkGameManager] You are the RUNNER (chodzisz po mapie)!
[NetworkGameManager] SpawnPlayerForRole: clientId=0, role=Runner
[NetworkGameManager] SpawnPoint: (100, 0, 0)  ← POWINNA BYĆ POZYCJA
[NetworkGameManager] Prefab: RunnerPlayer
[NetworkGameManager] Spawning at: (100, 0, 0)
[NetworkGameManager] ✅ Zespawnowano Runner dla gracza 0 na pozycji (100, 0, 0)
[NetworkPlayerController] Gracz 0 skonfigurowany
[NetworkPlayerController] Setup jako RUNNER
```

Po naciśnięciu **J** (Join) w drugiej instancji:
```
[NetworkGameManager] Gracz 1 dołączył
[NetworkGameManager] Przydzielono Operator graczowi 1
[NetworkGameManager] You are the OPERATOR (łączysz kable)!
[NetworkGameManager] SpawnPlayerForRole: clientId=1, role=Operator
[NetworkGameManager] SpawnPoint: (0, 0, 0)  ← POWINNA BYĆ POZYCJA
[NetworkGameManager] Prefab: OperatorPlayer
[NetworkGameManager] Spawning at: (0, 0, 0)
[NetworkGameManager] ✅ Zespawnowano Operator dla gracza 1 na pozycji (0, 0, 0)
[NetworkPlayerController] Gracz 1 skonfigurowany
[NetworkPlayerController] Setup jako OPERATOR
```

---

## ❌ Błędy które mogą się pojawić:

### "Brak prefaba dla Runner/Operator"
```
[NetworkGameManager] ❌ Brak prefaba dla Runner! Przypisz prefab w inspektorze.
```
**Rozwiązanie:** Przeciągnij prefab do pola w inspektorze GameManager

### "Brak spawn pointu"
```
[NetworkGameManager] ❌ Brak spawn pointu dla Runner! Przypisz spawn point w inspektorze.
```
**Rozwiązanie:** Przeciągnij Empty GameObject do pola w inspektorze

### "Prefab nie ma NetworkObject"
```
[NetworkGameManager] ❌ Prefab RunnerPlayer nie ma komponentu NetworkObject!
```
**Rozwiązanie:** Otwórz prefab, dodaj komponent NetworkObject

### "SpawnPoint: NULL"
```
[NetworkGameManager] SpawnPoint: NULL
```
**Rozwiązanie:** Spawn point nie jest przypisany w inspektorze

---

## 🔧 Szybka Naprawa:

### Krok 1: NetworkManager
1. Zaznacz GameObject **NetworkManager**
2. W komponencie **Network Manager**
3. **Default Player Prefab** → ustaw na **None**
4. Zapisz scenę

### Krok 2: GameManager
1. Zaznacz GameObject **GameManager**
2. W komponencie **Network Game Manager**
3. Sprawdź wszystkie 4 pola:
   - Operator Spawn Point ← przeciągnij z Hierarchy
   - Runner Spawn Point ← przeciągnij z Hierarchy
   - Operator Prefab ← przeciągnij z Project
   - Runner Prefab ← przeciągnij z Project
4. Zapisz scenę

### Krok 3: Test
1. Play
2. H (host)
3. Sprawdź konsolę - powinny być zielone ✅ logi
4. Sprawdź Scene view - gracz powinien być w spawn poincie

---

## 📍 Przykład struktury:

```
Hierarchy:
├── NetworkManager
├── GameManager (NetworkGameManager)
│
├── OperatorZone
│   └── OperatorSpawnPoint (0, 0, 0)
│
└── RunnerZone
    └── RunnerSpawnPoint (100, 0, 0)

Project (Prefabs):
├── OperatorPlayer.prefab (ma NetworkObject)
└── RunnerPlayer.prefab (ma NetworkObject)
```

---

## 🎯 Po naprawie:

Uruchom grę i sprawdź:
- [ ] W konsoli: logi z pozycjami spawn pointów
- [ ] W konsoli: zielone ✅ "Zespawnowano..."
- [ ] W Scene view: gracz jest w spawn poincie
- [ ] W Game view: widzisz gracza
- [ ] Gracz może się poruszać

**Jeśli wszystko działa - gotowe!** 🎉

---

## 💡 Protip:

W inspektorze GameManager możesz kliknąć na przypisane obiekty:
- Kliknij na OperatorSpawnPoint → pokaże go w Hierarchy
- Kliknij na OperatorPrefab → otworzy prefab

To pomoże sprawdzić czy wszystko jest dobrze przypisane!


