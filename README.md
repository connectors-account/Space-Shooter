# SpaceShooterGame (Unity 2D, Windows Desktop)

This project contains all core gameplay scripts in `Assets/Scripts/` for a fully playable space-shooter.

## Included Scripts

- `PlayerController.cs` — movement, shooting, health, shield, rapid-fire
- `EnemyController.cs` — enemy movement patterns, enemy shooting, damage/death
- `BulletController.cs` — projectile movement, collision, damage
- `GameManager.cs` — game state, wave progression, score, pause/game-over flow
- `SpawnManager.cs` — enemy wave spawning and power-up spawning
- `PowerUp.cs` — health/shield/rapid-fire behavior
- `UIManager.cs` — main menu, HUD, pause, game-over UI handling
- `BackgroundScroller.cs` — parallax scrolling using repeating layer pairs
- `AudioManager.cs` — music/SFX playback with clip hooks

---

## 1) Create / Open the Unity Project

1. Open **Unity Hub**.
2. Click **Open** and select this folder:
   - `/home/ubuntu/SpaceShooterGame`
3. Use Unity **2022.3.x LTS** (matches `ProjectVersion.txt`).

---

## 2) Scene Setup (Complete)

Create one scene named `MainScene` in `Assets/Scenes/`.

### A. Camera & General
1. Select `Main Camera`:
   - Projection: **Orthographic**
   - Size: **5.5**
   - Position: `(0, 0, -10)`
   - Background: dark blue/black.
2. Save scene as `Assets/Scenes/MainScene.unity`.

### B. Procedural/Simple Graphics (No external art needed)
Use Unity built-in **Square** sprite (`Sprites > Square`) and color/scale it.

#### Player prefab (`Player`)
1. Create GameObject → **2D Object > Sprites > Square**.
2. Rename to `Player`.
3. Add components:
   - `Rigidbody2D` (Body Type: Kinematic, Gravity Scale: 0)
   - `BoxCollider2D` (Is Trigger: true)
   - `PlayerController`
4. Sprite setup:
   - Scale `(0.9, 0.9, 1)`
   - Color: cyan or white
5. Create child empty object `FirePoint` at `(0, 0.65, 0)`.
6. Drag `Player` into `Assets/Prefabs/` to create prefab.

#### Enemy prefab (`Enemy`)
1. Create another Square sprite named `Enemy`.
2. Add:
   - `Rigidbody2D` (Kinematic, Gravity 0)
   - `BoxCollider2D` (Is Trigger: true)
   - `EnemyController`
3. Sprite setup:
   - Scale `(0.8, 0.8, 1)`
   - Color: red
4. Create child `FirePoint` at `(0, -0.55, 0)`.
5. Save as `Assets/Prefabs/Enemy.prefab`.

#### Bullet prefabs
Create two bullet prefabs from square sprites:

1. `PlayerBullet`:
   - Scale `(0.18, 0.4, 1)`
   - Color: yellow
   - Add `BoxCollider2D` (Is Trigger: true)
   - Add `BulletController`
   - Save as prefab
2. `EnemyBullet`:
   - Duplicate `PlayerBullet`, color magenta/orange
   - Save as `EnemyBullet.prefab`

> Assign `PlayerBullet` to `PlayerController.playerBulletPrefab`.
> Assign `EnemyBullet` to `EnemyController.enemyBulletPrefab`.

#### Power-up prefab (`PowerUp`)
1. Create Square sprite named `PowerUp`.
2. Scale `(0.6, 0.6, 1)`.
3. Add:
   - `Rigidbody2D` (Kinematic, Gravity 0)
   - `CircleCollider2D` (Is Trigger: true)
   - `PowerUp`
4. Save as `Assets/Prefabs/PowerUp.prefab`.

### C. Managers

Create empty GameObject `Managers` with children:

1. `GameManager` object:
   - Add `GameManager` script.
2. `SpawnManager` object:
   - Add `SpawnManager` script.
3. `UIManager` object:
   - Add `UIManager` script.
4. `AudioManager` object:
   - Add `AudioManager` script.

Wire references in inspector:
- On `GameManager`:
  - `spawnManager` → SpawnManager object
  - `uiManager` → UIManager object
  - `playerPrefab` → Player prefab
  - `playerSpawnPoint` → create empty object `PlayerSpawnPoint` at `(0, -3.7, 0)`
- On `SpawnManager`:
  - `enemyPrefab` → Enemy prefab
  - `powerUpPrefab` → PowerUp prefab

### D. Parallax Background

1. Create empty object `Background` with `BackgroundScroller`.
2. Make 3 parallax layers (far/mid/near), each with **two** stacked square sprites (`LayerX_A`, `LayerX_B`):
   - Scale each to fill camera width and enough height (example `(20, 12, 1)`).
   - Position A at y=0, B at y=12.
   - Set z-depth: far `z=10`, mid `z=9`, near `z=8` (or adjust sorting order).
   - Color different dark shades for depth.
3. In `BackgroundScroller`, set array size to 3 and assign each layer A/B, speed, tileHeight:
   - Far speed 0.4, tileHeight 12
   - Mid speed 0.8, tileHeight 12
   - Near speed 1.2, tileHeight 12

### E. UI Setup

1. Create `Canvas` (Screen Space - Overlay).
2. Add panels:
   - `MainMenuPanel`
   - `HUDPanel`
   - `PausePanel`
   - `GameOverPanel`
3. HUD elements in `HUDPanel`:
   - `Slider` for health (`HealthSlider`)
   - `Text` for health (`HealthText`)
   - `Text` for score (`ScoreText`)
   - `Text` for wave (`WaveText`)
   - `Text` or `Image` indicator for shield (`ShieldIndicator`)
   - `Text` or `Image` indicator for rapid fire (`RapidFireIndicator`)
4. Main menu:
   - Title text + Start button + Quit button
5. Pause menu:
   - Resume button + Main Menu button + Quit button
6. Game-over panel:
   - Game Over text + Final score text + Main Menu button + Quit button

Assign all references in `UIManager` inspector.

Button OnClick hooks:
- Start → `UIManager.OnClickStartGame`
- Resume → `UIManager.OnClickResumeGame`
- Main Menu/Restart → `UIManager.OnClickRestartToMainMenu`
- Quit → `UIManager.OnClickQuitGame`

### F. Audio Setup

1. On `AudioManager`, add two `AudioSource` components:
   - One for music (`loop` on)
   - One for SFX
2. Assign these into `musicSource` and `sfxSource`.
3. Optional: drag clips into AudioManager fields.
   - Code already contains comments showing where to add audio clips.

### G. Final Script Reference Wiring

- `PlayerController.firePoint` → Player/FirePoint
- `EnemyController.firePoint` → Enemy/FirePoint
- `PlayerController.playerBulletPrefab` → PlayerBullet prefab
- `EnemyController.enemyBulletPrefab` → EnemyBullet prefab

### H. Build Settings Scene

1. Open **File > Build Settings**.
2. Platform: **Windows, Mac, Linux Standalone**.
3. Target Platform: **Windows**.
4. Add `MainScene` to scenes in build.

---

## 3) Controls

- Move: **WASD** or **Arrow Keys**
- Shoot: **Space**
- Pause/Resume: **Esc**

---

## 4) Build Windows .exe

1. Open **File > Build Settings**.
2. Select **Windows** platform.
3. Architecture: **x86_64**.
4. Click **Build**.
5. Choose output folder, e.g. `Builds/Windows/`.
6. Unity outputs:
   - `SpaceShooterGame.exe`
   - Data folder beside it.

Run the `.exe` to play.

---

## Notes

- The game loop is complete: menu → play → waves → score/powerups → game over → restart.
- Power-up colors:
  - Green = Health
  - Cyan = Shield
  - Yellow = Rapid Fire
- If any object is not moving/shooting, verify prefab references are assigned in Inspector.
