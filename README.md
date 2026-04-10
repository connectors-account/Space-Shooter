# Unity Space Shooter (Windows Desktop)

This repository contains a complete **2D Unity space shooter** implementation targeted for **Windows desktop (x86_64)**.

## Unity Version
- Recommended: **Unity 2022.3 LTS** (tested code style for this branch)
- Scripting backend: IL2CPP or Mono (both supported)
- Input: **Legacy Input Manager** (`Input.GetAxis`, `Input.GetButton`, `Input.GetKey`)

## Project Structure

```text
Assets/
  Audio/
  Prefabs/
  Scenes/
  Sprites/
  UI/
  Scripts/
    AudioManager.cs
    EnemyBullet.cs
    EnemyController.cs
    EnemySpawner.cs
    GameManager.cs
    GameOverScreen.cs
    HealthBar.cs
    MainMenu.cs
    ObjectPool.cs
    ParallaxBackground.cs
    PauseMenu.cs
    PlayerBullet.cs
    PlayerController.cs
    PowerUp.cs
    ScoreDisplay.cs
    ScoreManager.cs
    SimpleCameraFollow.cs
    WaveDisplay.cs
    WaveManager.cs
ProjectSettings/
```

## Gameplay Systems Included
- Player movement, shooting, health, i-frames
- 3 enemy types: basic, fast (zig-zag), tank (spread shots)
- Player/enemy bullets with pooling
- Powerups: rapid fire, shield, health restore
- Collision + damage handling
- Score + combo multiplier
- Progressive wave system with increasing difficulty
- Parallax scrolling background
- UI scripts: health, score/combo, wave display
- Menus: main menu, pause, game over
- Audio manager for music + SFX
- High score save/load via `PlayerPrefs`
- Explosion particle support

## Scene Setup (Step-by-Step)

### 1) Open Project
1. Clone this repo.
2. Open **Unity Hub** → **Add project from disk** → select repository root.
3. Open with **Unity 2022.3 LTS**.

### 2) Create Scenes
Create and save two scenes in `Assets/Scenes/`:
- `MainMenu.unity`
- `Game.unity`

Add both scenes to Build Settings in this order:
1. MainMenu
2. Game

### 3) Physics + Camera
In `Game` scene:
1. Main Camera: Orthographic.
2. Optional smooth follow: add `SimpleCameraFollow` and assign Player target.
3. Ensure all gameplay colliders use **Is Trigger** and Rigidbody2D where required:
   - Player: Rigidbody2D (dynamic or kinematic), Collider2D
   - Enemy prefabs: Collider2D
   - Bullet prefabs: Collider2D trigger
   - PowerUp prefab: Collider2D trigger

### 4) Create Simple Procedural Sprites (No external assets required)
Use one of these methods:
- **Method A (Quickest):** Create `Sprites > Square` and tint colors in SpriteRenderer:
  - Player: cyan/blue
  - Basic enemy: red
  - Fast enemy: yellow
  - Tank enemy: magenta
  - Player bullet: green
  - Enemy bullet: orange
  - Powerups: purple / blue / green variants
- **Method B:** Import tiny PNGs (16x16 / 32x32) and set filter mode to Point.

### 5) Prefab Setup
Create prefabs and add scripts:

1. **Player Prefab**
   - SpriteRenderer + Collider2D + Rigidbody2D
   - `PlayerController`
   - Child `FirePoint` transform above ship nose

2. **Enemy Prefabs** (Basic/Fast/Tank)
   - SpriteRenderer + Collider2D
   - `EnemyController`
   - Configure per type:
     - Basic: balanced speed/health
     - Fast: higher speed, lower health
     - Tank: lower speed, high health, spread shooting

3. **Bullets**
   - Player bullet prefab with `PlayerBullet`
   - Enemy bullet prefab with `EnemyBullet`

4. **PowerUp Prefab**
   - `PowerUp` script + Collider2D trigger + sprite

5. **Object Pools**
   Create empty GameObjects each with `ObjectPool`:
   - PlayerBulletPool
   - EnemyBulletPool
   - BasicEnemyPool
   - FastEnemyPool
   - TankEnemyPool
   - PowerUpPool
   Assign corresponding prefab and initial size.

### 6) Managers and Wiring
In `Game` scene create:

1. `GameManager` GameObject with `GameManager`
   - Assign player, score manager, wave manager, game over screen, pause menu
   - Assign all object pools
   - Assign explosion particle prefab (optional but recommended)

2. `ScoreManager` GameObject with `ScoreManager`
3. `WaveManager` GameObject with `WaveManager`
4. `EnemySpawner` GameObject with `EnemySpawner`
5. `AudioManager` GameObject with:
   - 2 AudioSources (music + SFX)
   - `AudioManager` script

### 7) UI Setup
Create a Canvas and add:
- Health slider + `HealthBar`
- Score text + combo text + `ScoreDisplay`
- Wave text + `WaveDisplay`
- Pause panel + buttons wired to `PauseMenu`
- GameOver panel + buttons wired to `GameOverScreen`

### 8) Main Menu Scene
In `MainMenu` scene:
- Add menu UI with Start and Quit buttons
- Add `MainMenu` script and set game scene name = `Game`
- Hook buttons to `StartGame()` and `QuitGame()`

## Input Mapping (Legacy)
Default Unity legacy mappings are used:
- Move: Arrow keys / WASD (`Horizontal`, `Vertical`)
- Shoot: Space / Ctrl / Mouse0 (`Fire1`)
- Pause: Escape

## Build for Windows x86_64
1. Unity: **File → Build Settings**
2. Platform: **PC, Mac & Linux Standalone**
3. Target Platform: **Windows**
4. Architecture: **x86_64**
5. Add scenes (`MainMenu`, `Game`) if not present.
6. Click **Build** and choose output folder.

## Notes
- High score is stored in `PlayerPrefs` key: `HIGH_SCORE`.
- This branch is intentionally simple and fully script-driven for rapid iteration.
