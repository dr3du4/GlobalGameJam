# 🎥 PROBLEMY Z KAMERAMI - Debug

## ❌ "No cameras rendering"

Dodałem szczegółowe logi - teraz zobaczysz dokładnie co się dzieje!

---

## ✅ SPRAWDŹ W UNITY (przed uruchomieniem):

### 1. Kamery w Hierarchy:
- [ ] **GridCamera** - istnieje
- [ ] **OperatorCamera** - istnieje
- [ ] Obie mają komponent **Camera**
- [ ] Obie **NIE MUSZĄ** być wyłączone jako GameObjecty (kod je włączy)
- [ ] Ale komponent Camera może być disabled

### 2. Tagi:
1. Zaznacz **GridCamera**
2. Sprawdź w Inspector → Tag → musi być: **GridCamera**
3. Zaznacz **OperatorCamera**  
4. Sprawdź w Inspector → Tag → musi być: **OperatorCamera**

### 3. Prefaby Graczy:

**OperatorPlayer prefab:**
- [ ] Ma komponent **NetworkObject**
- [ ] Ma komponent **NetworkPlayerController**
- [ ] W NetworkPlayerController:
  - [ ] `Movement Spine` przypisany
  - [ ] `Grid Movement` pusty
  - [ ] `Operator Camera Tag` = "OperatorCamera"
  - [ ] `Runner Camera Tag` = "GridCamera"

**RunnerPlayer prefab:**
- [ ] Ma komponent **NetworkObject**
- [ ] Ma komponent **NetworkPlayerController**
- [ ] W NetworkPlayerController:
  - [ ] `Movement Spine` pusty
  - [ ] `Grid Movement` przypisany
  - [ ] `Operator Camera Tag` = "OperatorCamera"
  - [ ] `Runner Camera Tag` = "GridCamera"

---

## 📊 CO ZOBACZYSZ W KONSOLI (po poprawkach):

### Gdy wszystko działa ✅:
```
[NetworkPlayerController] Gracz 0 skonfigurowany
[NetworkPlayerController] Setup jako RUNNER
[NetworkPlayerController] ✅ GridMovement włączony
[NetworkPlayerController] Szukam kamery z tagiem: GridCamera
[NetworkPlayerController] ✅ Znaleziono kamerę: GridCamera
[NetworkPlayerController] ✅ Kamera włączona na pozycji: (100, 10, 90)
[NetworkPlayerController] ✅ Kamera śledzi gracza na: (100, 0, 0)
```

### Jeśli brak kamery ❌:
```
[NetworkPlayerController] ❌ Nie znaleziono kamery z tagiem: GridCamera
```
**Rozwiązanie:** Sprawdź tag na kamerze!

### Jeśli brak skryptu ruchu ❌:
```
[NetworkPlayerController] ❌ GridMovement nie przypisany!
```
**Rozwiązanie:** W prefabie przypisz GridMovement w NetworkPlayerController!

---

## 🔧 NAJCZĘSTSZE PROBLEMY:

### Problem 1: Kamery mają złe tagi
**Sprawdź:**
```
Hierarchy → GridCamera → Inspector (góra) → Tag = "GridCamera"
Hierarchy → OperatorCamera → Inspector (góra) → Tag = "OperatorCamera"
```

### Problem 2: Prefaby nie mają NetworkPlayerController
**Napraw:**
1. Otwórz prefab (double-click)
2. Add Component → NetworkPlayerController
3. Przypisz MovementSpine LUB GridMovement

### Problem 3: NetworkPlayerController nie ma przypisanych skryptów
**Napraw:**
1. Otwórz prefab
2. W NetworkPlayerController przeciągnij:
   - Dla Operatora: `Movement Spine`
   - Dla Runnera: `Grid Movement`

---

## 🎯 SZYBKI TEST:

1. **Uruchom grę**
2. **Naciśnij H** (host)
3. **Sprawdź konsolę** - szukaj:
   - ✅ Zielone checkmarki = działa
   - ❌ Czerwone X = problem

4. **Wklej logi tutaj** - pomogę zdiagnozować!

---

## 💡 TIP:

Możesz też ręcznie włączyć kamerę w Scene view i sprawdzić czy działa:
1. Zaznacz GridCamera
2. GameObject → Set as Active Camera
3. Zobacz czy coś renderuje

Jeśli tak = kamery działają, problem w NetworkPlayerController.
Jeśli nie = problem z samą kamerą (sprawdź Culling Mask, Clear Flags).

---

**Uruchom grę i wklej co pokazuje konsola!** 🔍


