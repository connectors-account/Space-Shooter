# Space Shooter - Unity Setup Guide (Complete)

This guide shows exactly how to set up the project in Unity and export a Windows `.exe`.

## 1) Open the Project
1. Install Unity Hub + Unity 2021.3 LTS or newer.
2. In Unity Hub, click **Open**.
3. Select this repository root (`Space-Shooter`).
4. Wait for import/compile to finish.

## 2) Create Required Tags
Go to **Edit -> Project Settings -> Tags and Layers** and add tags:
- `Player`
- `Enemy`
- `PlayerBullet`
- `EnemyBullet`
- `PowerUp`

## 3) Create Scene
1. Create a new 2D scene.
2. Save as `Assets/Scenes/GameScene.unity`.
3. Camera setup:
   - Projection: Orthographic
   - Position: `(0, 0, -10)`
   - Size: around `5.5`
   - Background: dark color

## 4) Create Manager Objects
### GameManager
- Create Empty GameObject named `GameManager`
- Add `SpaceShooter.Managers.GameManager`

### SpawnManager
- Create Empty GameObject named `SpawnManager`
- Add `SpaceShooter.Managers.SpawnManager`

### InputHandler
- Create Empty GameObject named `InputHandler`
- Add `SpaceShooter.InputSystem.InputHandler`

### AudioManager (optional but recommended)
- Create Empty GameObject named `AudioManager`
- Add `SpaceShooter.Managers.AudioManager`
- Assign clips from `Assets/Audio`

## 5) Create Prefabs (Assets/Prefabs)
Use `Assets/Prefabs/PREFAB_SETUP.md` checklist.

Minimum required prefabs for playable game:
1. `Player`
2. `PlayerBullet`
3. `EnemyBullet`
4. `EnemyBasic`
5. `EnemyZigzag`
6. `EnemyTank`
7. `PowerUpHealth`
8. `PowerUpRapidFire`
9. `PowerUpShield`

### Critical Wiring
- In `PlayerController`, assign `Bullet Prefab` to `PlayerBullet`.
- In each enemy prefab, assign:
  - `Bullet Prefab` -> `EnemyBullet`
  - `Fire Point` child transform
  - `Power Up Prefabs` array
- In `SpawnManager`, assign basic/zigzag/tank prefabs.

## 6) Create Scene Objects
### Player
- Drag `Player` prefab into scene
- Set position `(0, -3, 0)`
- Confirm tag is `Player`

### Background
- Add `StarField` object with `SpaceShooter.Environment.StarField`
- Add `ParallaxBackground` object with `SpaceShooter.Environment.ParallaxBackground`
- Assign 2 background layers (sprites from `Assets/Sprites`)

## 7) Setup UI
1. Create `Canvas` (Screen Space - Overlay) + `EventSystem`.
2. Create panels:
   - `MainMenuPanel`
   - `HUDPanel`
   - `GameOverPanel`
   - `PausePanel`
3. Create text/buttons/sliders needed by `UIManager`.
4. Create empty object `UIManager` and attach `SpaceShooter.UI.UIManager`.
5. Assign every serialized UI field in inspector.
6. (Optional) Add `MainMenuUI`, `HUDUI`, `GameOverUI` scripts to dedicated panel objects.

## 8) Test Playability (Editor)
Press **Play** and verify:
- Main menu appears
- Start enters gameplay
- Move with `WASD` or Arrow keys
- Shoot with `Space`
- Enemies spawn in waves
- Score increases on enemy kill
- Health decreases on hit
- Power-up pickups work
- On death, game over panel appears
- `Esc` pauses/unpauses

## 9) Build Windows Executable (.exe)
1. Open **File -> Build Settings**.
2. Click **Add Open Scenes** (ensure `GameScene` is listed).
3. Choose **PC, Mac & Linux Standalone**.
4. Set Target Platform to **Windows**.
5. Set Architecture to **x86_64**.
6. Open **Player Settings** and set:
   - Product Name: `Space Shooter`
   - Default resolution (e.g. 1024 x 768)
   - Fullscreen mode preference
7. Click **Build**.
8. Select an output folder, e.g. `Builds/Windows`.
9. Run `Space Shooter.exe` from the output folder.

## 10) Notes
- If references are missing, check inspector assignments first.
- If collisions fail, ensure colliders are trigger colliders and proper tags are set.
- Keep all built files together when sharing (`.exe`, `_Data`, dlls).
