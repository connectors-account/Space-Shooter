# Unity Space Shooter (Windows Desktop)

This project contains a complete starter structure and full C# gameplay scripts for a 2D wave-based space shooter.

## Unity Version
Use **Unity 2022.3.25f1 LTS** (or any close 2022.3 LTS version).

---

## Project Structure

```
space_shooter_unity/
├─ Assets/
│  ├─ Scripts/
│  │  ├─ PlayerController.cs
│  │  ├─ EnemyController.cs
│  │  ├─ BulletController.cs
│  │  ├─ SpawnManager.cs
│  │  ├─ PowerUpController.cs
│  │  ├─ GameManager.cs
│  │  ├─ UIManager.cs
│  │  ├─ ParallaxBackground.cs
│  │  └─ CollisionHandler.cs
│  ├─ Prefabs/
│  ├─ Scenes/
│  ├─ Sprites/
│  ├─ UI/
│  ├─ Materials/
│  └─ Audio/
├─ Packages/
├─ ProjectSettings/
└─ README.md
```

---

## Keyboard Controls (Input Setup)

This project uses Unity's built-in input axes and keys:

- **Move**: `WASD` or Arrow Keys (`Horizontal` / `Vertical` axes)
- **Shoot**: `Space`
- **Pause/Resume**: `Esc`

### Verify Input Manager
1. Open **Edit → Project Settings → Input Manager**.
2. Ensure axes `Horizontal` and `Vertical` exist (default in new projects).
3. No extra setup is required unless you removed default axes.

---

## Scene Setup (Step-by-Step)

### 1) Create main scene
1. Create `Assets/Scenes/Main.unity`.
2. Set camera to **Orthographic**, size around `5`.

### 2) Create placeholder sprites (simple shapes)
1. In Unity: **Assets → Create → Sprites → Square** (or import basic PNGs).
2. Create/prepare these placeholder visuals:
   - Player ship sprite (e.g., cyan square/triangle)
   - Enemy sprite (e.g., red square)
   - Bullet sprite (small rectangle)
   - 3 power-up sprites (different colors/icons)
   - 2 background layers (dark + star dots)

### 3) Create Player prefab
1. Create GameObject `Player` with:
   - `SpriteRenderer`
   - `BoxCollider2D` (set **Is Trigger = true**)
   - `PlayerController`
   - `CollisionHandler`
2. Create child `FirePoint` at player nose (e.g., y = 0.6).
3. Assign in `PlayerController`:
   - `Fire Point` = child transform
   - `Player Bullet Prefab` = your player bullet prefab (created below)
4. Tag object as `Player`.
5. Drag to `Assets/Prefabs/Player.prefab`.

### 4) Create Bullet prefabs
Create two prefabs:

#### PlayerBullet prefab
- Components:
  - `SpriteRenderer`
  - `BoxCollider2D` (Is Trigger = true)
  - `BulletController`
  - `CollisionHandler`
- In `BulletController`:
  - `Is Player Bullet = true`
  - Speed ~ `12`
  - Damage ~ `10`

#### EnemyBullet prefab
- Same components
- In `BulletController`:
  - `Is Player Bullet = false`
  - Speed ~ `7`
  - Damage ~ `12`

### 5) Create Enemy prefab
1. GameObject `Enemy` with:
   - `SpriteRenderer`
   - `BoxCollider2D` (Is Trigger = true)
   - `EnemyController`
   - `CollisionHandler`
2. Add child `FirePoint` below center (e.g., y = -0.5).
3. Assign in `EnemyController`:
   - `Enemy Bullet Prefab` = EnemyBullet
   - `Fire Point` = child fire point
4. Tag as `Enemy`.
5. Save as `Assets/Prefabs/Enemy.prefab`.

### 6) Create Power-up prefabs (3)
Create `PowerUp_Shield`, `PowerUp_RapidFire`, `PowerUp_Health` with:
- `SpriteRenderer`
- `CircleCollider2D` (Is Trigger = true)
- `PowerUpController`
- `CollisionHandler`

Set each prefab's `PowerUp Type` accordingly.

### 7) Create Managers

#### GameManager object
Add `GameManager` script and assign:
- `Spawn Manager` reference
- `UI Manager` reference

#### SpawnManager object
Add `SpawnManager` script and assign:
- `Enemy Prefab`
- `Power Up Prefabs` array with the 3 power-ups
- `UI Manager`
- Keep default 5 waves or tune values in inspector.

#### UIManager object
Add `UIManager` script and assign text/panel references (described below).

### 8) Background + parallax
1. Create empty object `BackgroundSystem` with `ParallaxBackground`.
2. Add two child sprite objects (`BG_Layer1`, `BG_Layer2`) that tile vertically.
3. In `ParallaxBackground`, set each layer entry:
   - `Layer Transform`
   - `Scroll Speed` (e.g., 0.2 and 0.6)
   - `Reset Y` and `Start Y` (e.g., -12 and 12)

---

## UI Setup (Main Menu, HUD, Pause, Game Over)

1. Create a `Canvas` (Screen Space Overlay) and `EventSystem`.
2. Build these panels under canvas:

### HUD Panel
- Text: `ScoreText`
- Text: `LivesText`
- Text: `WaveText`

### Main Menu Panel
- Title text
- Start button (OnClick → `UIManager.OnStartButton`)
- Quit button (OnClick → `UIManager.OnQuitButton`)

### Pause Panel
- Resume button (OnClick → `UIManager.OnResumeButton`)
- Restart button (OnClick → `UIManager.OnRestartButton`)

### Game Over Panel
- Title text (`Victory!` / `Game Over` filled by script)
- Final score text
- Restart button (OnClick → `UIManager.OnRestartButton`)
- Quit button (OnClick → `UIManager.OnQuitButton`)

3. Assign all panel/text references in `UIManager` inspector.

---

## Script Wiring Checklist

- `PlayerController.playerBulletPrefab` → PlayerBullet prefab
- `EnemyController.enemyBulletPrefab` → EnemyBullet prefab
- `SpawnManager.enemyPrefab` → Enemy prefab
- `SpawnManager.powerUpPrefabs` → 3 power-up prefabs
- `GameManager.spawnManager` → SpawnManager object
- `GameManager.uiManager` → UIManager object
- `SpawnManager.uiManager` → UIManager object
- `CollisionHandler` attached to Player, Enemy, Bullet, and PowerUp prefabs

---

## Gameplay Included

- Player movement, shooting, health/life handling
- Enemy AI with 3 movement patterns (Straight / ZigZag / SineWave)
- Enemy shooting and contact damage
- Bullet system for player + enemy bullets
- 5 wave progression with increasing difficulty
- Random power-up spawning (shield, rapid fire, health)
- Score and life tracking
- Main menu, pause menu, game over/victory screens
- Parallax scrolling background

---

## Build for Windows (.exe)

1. Open **File → Build Settings**.
2. Select **PC, Mac & Linux Standalone**.
3. Target Platform: **Windows**.
4. Architecture: **x86_64**.
5. Click **Add Open Scenes** (ensure `Main.unity` is included).
6. (Optional) Player Settings:
   - Product Name: `SpaceShooter`
   - Company Name: your name/company
   - Default screen mode and resolution as desired
7. Click **Build**.
8. Choose output folder, e.g. `Builds/Windows/`.
9. Run generated `SpaceShooter.exe`.

---

## Notes

- If UI Text components are missing, add **Legacy Text** (`UnityEngine.UI.Text`) or adapt scripts to TextMeshPro.
- Make sure all gameplay colliders are **Is Trigger** for trigger-based collision logic.
- Keep `Time.timeScale` at `1` outside pause state.
