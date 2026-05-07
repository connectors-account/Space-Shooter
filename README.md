# Unity Space Shooter (C#)

This project contains a complete, ready-to-use Unity 2D space shooter implementation.

## Project Structure

```text
space_shooter_unity/
├── README.md
└── Scripts/
    ├── PlayerController.cs
    ├── EnemyController.cs
    ├── BulletController.cs
    ├── EnemySpawner.cs
    ├── GameManager.cs
    ├── UIManager.cs
    ├── PowerUp.cs
    ├── HealthSystem.cs
    └── MenuManager.cs
```

---

## 1) Create and Set Up the Unity Project

1. Open **Unity Hub**.
2. Click **New project**.
3. Select template: **2D (Built-In Render Pipeline)**.
4. Project name: `SpaceShooter` (or any name).
5. Create the project.
6. In your Unity project, create folders under `Assets/`:
   - `Assets/Scripts`
   - `Assets/Prefabs`
   - `Assets/Scenes`
   - `Assets/Sprites`
   - `Assets/UI`
7. Copy all `.cs` files from this repository’s `Scripts/` folder into `Assets/Scripts/`.
8. Save scene as `Assets/Scenes/Main.unity`.

---

## 2) Scene Configuration (Step-by-Step)

### A. Camera
1. Select **Main Camera**.
2. Set **Projection** = Orthographic.
3. Position = `(0, 0, -10)`.
4. Size around `5` (adjust based on your visible play area).

### B. Player Setup
1. Create GameObject: **Player** (2D Object → Sprite).
2. Assign a simple player sprite (instructions below).
3. Position player at `(0, -4, 0)`.
4. Add components:
   - `Rigidbody2D` (Body Type: Kinematic, Gravity Scale: 0)
   - `BoxCollider2D` (Is Trigger: true)
   - `HealthSystem`
   - `PlayerController`
5. Tag this object as **Player**.
6. Create child empty GameObject `FirePoint` at `(0, 0.6, 0)` and assign it in `PlayerController.firePoint`.

### C. Bullet Prefab
1. Create GameObject **Bullet** (Sprite).
2. Add components:
   - `Rigidbody2D` (Kinematic, Gravity Scale: 0)
   - `CircleCollider2D` (Is Trigger: true)
   - `BulletController`
3. Scale small (e.g. `0.2, 0.4, 1`).
4. Drag to `Assets/Prefabs/Bullet.prefab`.
5. Assign this prefab to `PlayerController.bulletPrefab`.

### D. Enemy Prefab
1. Create GameObject **Enemy** (Sprite).
2. Add components:
   - `Rigidbody2D` (Kinematic, Gravity Scale: 0)
   - `BoxCollider2D` (Is Trigger: true)
   - `EnemyController`
3. Create prefab at `Assets/Prefabs/Enemy.prefab`.

### E. Rapid Fire Power-Up Prefab
1. Create GameObject **RapidFirePowerUp** (Sprite, e.g. circle).
2. Add components:
   - `Rigidbody2D` (Kinematic, Gravity Scale: 0)
   - `CircleCollider2D` (Is Trigger: true)
   - `PowerUp`
3. Create prefab at `Assets/Prefabs/RapidFirePowerUp.prefab`.
4. Assign this prefab to `EnemyController.rapidFirePowerUpPrefab`.

### F. Enemy Spawner
1. Create empty GameObject `EnemySpawner`.
2. Add component `EnemySpawner`.
3. Assign `enemyPrefab = Enemy.prefab`.
4. Tune spawn bounds (`minSpawnX`, `maxSpawnX`) to match camera width.

### G. Game Manager
1. Create empty GameObject `GameManager`.
2. Add component `GameManager`.
3. Assign references:
   - `enemySpawner` → EnemySpawner object
   - `uiManager` → UIManager object (created below)
   - `player` → Player object

### H. UI Setup
1. Create Canvas (Screen Space - Overlay).
2. Create `UIManager` empty object and add `UIManager` + `MenuManager` scripts.
3. Under Canvas create:
   - `HUDPanel` with Texts: `ScoreText`, `HealthText`, `WaveText`
   - `StartMenuPanel` with title + **Start** button
   - `GameOverPanel` with **Game Over** label + `FinalScoreText` + **Restart** button + **Quit** button
4. Assign all references in `UIManager` inspector.
5. Wire button events:
   - Start button → `MenuManager.OnStartButtonPressed()`
   - Restart button → `MenuManager.OnRestartButtonPressed()`
   - Quit button → `MenuManager.OnQuitButtonPressed()`

### I. Physics and Layers
1. Ensure all colliders are **Is Trigger** for these scripts.
2. Use Unity’s default 2D physics collision matrix or custom layers if preferred.

---

## 3) Create Simple Sprite Placeholders in Unity

You can make placeholders without external art:

### Option A (Fast): Built-in Square/Circle Sprites
1. Right click `Assets` → Create → Sprites → Square (or Circle).
2. Duplicate and recolor via **Sprite Renderer → Color**:
   - Player: Cyan
   - Enemy: Red
   - Bullet: Yellow
   - PowerUp: Green

### Option B: Basic Shapes via Sprite Editor
1. Create multiple squares/circles from built-in sprites.
2. Use scale and color to differentiate gameplay objects.

### Optional polish
- Add a dark background color in camera.
- Add a starfield image as a sprite in the background.

---

## 4) Controls

- **Move left/right:** `A/D` or `Left/Right Arrow`
- **Shoot:** `Space` (or Left Mouse Button)
- **Start game / restart:** UI buttons

---

## 5) Gameplay Logic Included

- Player horizontal movement with clamped boundaries
- Bullet shooting upward
- Enemies spawn from top and move downward
- Progressive wave difficulty scaling:
  - more enemies per wave
  - faster spawn rate
  - increased enemy speed/health/score values
- Collisions:
  - bullet → enemy (damage + destroy)
  - enemy → player (contact damage)
- Player health system with game over on death
- Score tracking
- Rapid-fire power-up drop and timed buff
- Start menu, game HUD, game over screen, restart support

---

## 6) Build Windows Executable (Step-by-Step)

1. Open **File → Build Settings**.
2. Platform: select **Windows, Mac, Linux Standalone**.
3. Target Platform: **Windows**.
4. Architecture: **x86_64**.
5. Click **Switch Platform** (if needed).
6. In **Scenes In Build**, click **Add Open Scenes** (ensure `Main.unity` is added).
7. Open **Player Settings** and set:
   - Product Name (e.g., SpaceShooter)
   - Company Name
   - Resolution defaults as desired
8. Click **Build**.
9. Choose output folder, e.g. `Builds/Windows`.
10. Unity produces:
    - `SpaceShooter.exe`
    - `SpaceShooter_Data/` folder

To run the game, keep `.exe` and `_Data` folder together.

---

## 7) Recommended Inspector Defaults

These values are good starting points:

- **PlayerController**
  - moveSpeed: `8`
  - baseFireCooldown: `0.25`
  - rapidFireCooldownMultiplier: `0.4`
  - minX / maxX: `-8 / 8`
- **HealthSystem**
  - maxHealth: `5`
- **EnemySpawner**
  - baseEnemiesPerWave: `6`
  - enemiesAddedPerWave: `2`
  - initialSpawnInterval: `1.1`
  - minimumSpawnInterval: `0.35`
- **EnemyController**
  - moveSpeed: `3`
  - maxHealth: `1`
  - scoreValue: `10`
  - powerUpDropChance: `0.1`
- **PowerUp**
  - rapidFireDuration: `5`

---

## 8) Troubleshooting

- **Bullets pass through enemies:** Verify both objects have `Collider2D` set to trigger and at least one object has `Rigidbody2D`.
- **No enemies spawn:** Check `GameManager.enemySpawner` and `EnemySpawner.enemyPrefab` references.
- **UI not updating:** Confirm `GameManager.uiManager` and all `UIManager` text/panel fields are assigned.
- **Buttons do nothing:** Check button OnClick events target object with `MenuManager`.

---

You now have a complete, expandable Unity 2D space shooter baseline ready for desktop Windows builds.
