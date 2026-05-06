# Unity Space Shooter (Windows Desktop)

A complete 2D space-shooter project structure for Unity, designed for **Windows desktop build (.exe)**.

This project includes 10 full gameplay scripts and setup instructions so you can assemble, run, and build the game quickly.

---

## 1) Implemented Features

- Player movement with **WASD** and **Arrow Keys**
- Shoot with **Spacebar**
- 3 enemy types with distinct behaviors:
  - **Chaser** (tracks player x-position)
  - **ZigZag** (sin-wave style movement)
  - **Shooter** (fires enemy bullets)
- Health/lives system (**3 lives**)
- Power-ups:
  - **Rapid Fire**
  - **Shield**
  - **Health Restore**
- Wave-based progression (enemy count + difficulty scaling)
- Parallax scrolling background
- Score tracking
- Main menu, pause menu, game over UI flow
- Collision detection for player/enemies/bullets/power-ups
- Audio manager with placeholder clip slots

---

## 2) Project Structure

```
Assets/
  Audio/
    AudioGuide.md
  Prefabs/
    PrefabSetupGuide.md
  Scenes/
    SceneSetupGuide.md
  Scripts/
    AudioManager.cs
    BulletController.cs
    EnemyController.cs
    GameManager.cs
    MenuManager.cs
    ParallaxBackground.cs
    PlayerController.cs
    PowerUpController.cs
    SpawnManager.cs
    UIManager.cs
  Sprites/
    SpriteCreationGuide.md
README.md
```

---

## 3) Unity Version & Project Settings

Recommended:
- **Unity 2022.3 LTS** (or newer LTS)
- Template: **2D (Core)**

Physics/collision recommendations:
- Use **2D colliders** (`BoxCollider2D`, `CircleCollider2D`) and `isTrigger = true` for bullets/power-ups.
- Add `Rigidbody2D` to moving gameplay entities (Dynamic for player/enemy, Kinematic okay for bullets if script-driven).

---

## 4) Scene Setup

Create two scenes:
1. `MainMenu`
2. `GamePlay`

Then add them to Build Settings in this order:
1. `MainMenu`
2. `GamePlay`

Detailed setup is also in: `Assets/Scenes/SceneSetupGuide.md`

---

## 5) Gameplay Scene Object Setup (Essential)

Create these root objects in `GamePlay` scene:

- `GameManager` (attach `GameManager.cs`)
- `SpawnManager` (attach `SpawnManager.cs`)
- `AudioManager` (attach `AudioManager.cs`, add 2 AudioSource components)
- `UIManager` (attach `UIManager.cs`, hook all text panels)
- `MenuManager` (attach `MenuManager.cs`)
- `ParallaxBackground` object (attach `ParallaxBackground.cs`)
- `Player` prefab instance (attach `PlayerController.cs`)

### Player requirements
- Tag: `Player`
- Collider2D (trigger or non-trigger is fine, but be consistent with your collision setup)
- Optional Rigidbody2D
- Assign in `PlayerController`:
  - `Player Bullet Prefab`
  - `Fire Point` (child transform at ship nose)

### Enemy requirements
- Tag is set in script at runtime (`EnemyController.Start()`), but setting it in prefab is also fine.
- `EnemyController` on each enemy prefab
- Collider2D + optional Rigidbody2D
- Shooter enemy needs:
  - `Enemy Bullet Prefab`
  - `Fire Point`

### Bullet requirements
- `BulletController` script
- Collider2D set to trigger
- Player bullet prefab: `owner = Player`
- Enemy bullet prefab: `owner = Enemy`

### Power-up requirements
- `PowerUpController` on each of 3 prefabs
- Collider2D trigger
- Set type in inspector:
  - RapidFire
  - Shield
  - Health

Detailed setup is also in: `Assets/Prefabs/PrefabSetupGuide.md`

---

## 6) Input Controls

- Move: **WASD** or **Arrow keys** (Unity Horizontal/Vertical axes)
- Shoot: **Space**
- Pause/Resume: **Esc**

---

## 7) UI Wiring

In a Canvas, create:

- HUD texts:
  - Score text
  - Lives text
  - Wave text
  - Rapid Fire indicator text (initially disabled)
  - Shield indicator text (initially disabled)
- Pause panel (`pauseMenuRoot`)
  - Resume button -> `MenuManager.ResumeGame()`
  - Main menu button -> `MenuManager.ReturnToMainMenu()`
- Game over panel (`gameOverRoot`)
  - Summary text (`gameOverSummaryText`)
  - Restart button -> `MenuManager.RestartGame()`
  - Main menu button -> `MenuManager.ReturnToMainMenu()`

In `MainMenu` scene:
- Add a Canvas with title and buttons:
  - Start -> `MenuManager.StartGame()`
  - Quit -> `MenuManager.QuitGame()`

---

## 8) Sprite Creation Guide (Simple Geometry)

See full guide at: `Assets/Sprites/SpriteCreationGuide.md`

Quick method:
1. In an image editor (or Unity + external temp texture), create small PNGs:
   - Player ship: cyan triangle-ish shape on transparent background
   - Chaser enemy: red square
   - ZigZag enemy: orange diamond/square
   - Shooter enemy: purple square
   - Player bullet: thin green rectangle
   - Enemy bullet: thin magenta rectangle
   - Powerups: circles/squares in yellow/blue/green
2. Import as `Sprite (2D and UI)`.
3. Set Pixels Per Unit consistently (e.g., 100).
4. Assign to SpriteRenderers in prefabs.

If you want a no-art pass quickly, you can use default colored square textures and rename prefabs.

---

## 9) Audio Placeholders

See full guide at: `Assets/Audio/AudioGuide.md`

Assign clips in `AudioManager` inspector:
- Gameplay music loop
- Player shoot
- Enemy shoot
- Player hit
- Player death
- Enemy death
- Power-up collect

If clips are missing, the game still runs (AudioManager safely ignores null clips).

---

## 10) Script Responsibilities

- `PlayerController.cs`: movement, shooting, health/lives, rapid fire, shield
- `EnemyController.cs`: enemy AI patterns + optional enemy firing
- `BulletController.cs`: bullet travel, owner-specific collision, damage
- `PowerUpController.cs`: power-up behavior and player application
- `GameManager.cs`: score, wave progression, game states, difficulty scaling
- `UIManager.cs`: HUD and menus panel updates
- `MenuManager.cs`: main menu + pause/game over button actions
- `ParallaxBackground.cs`: scrolling texture offsets for layered background
- `AudioManager.cs`: music and SFX playback
- `SpawnManager.cs`: wave spawn counts, weighted enemy types, power-up chance

---

## 11) Windows Build Instructions (.exe)

1. Open **File -> Build Settings**
2. Select platform: **PC, Mac & Linux Standalone**
3. Target Platform: **Windows**
4. Architecture: **x86_64**
5. Ensure scenes list contains:
   - `MainMenu`
   - `GamePlay`
6. Click **Player Settings**:
   - Set Product Name: `SpaceShooterGame`
   - Set Company Name (optional)
   - Set default resolution/window mode as desired
7. Click **Build**
8. Choose output folder, e.g. `Builds/Windows/`
9. Unity generates:
   - `SpaceShooterGame.exe`
   - `SpaceShooterGame_Data/`
   - supporting files
10. Run `SpaceShooterGame.exe` on Windows desktop.

---

## 12) Testing Checklist

- Player moves in both axes and stays within bounds
- Spacebar fires continuously at configured rate
- Enemies spawn in waves and increase in difficulty
- Score updates on enemy kill
- Power-ups apply effects correctly
- Shield blocks incoming damage while active
- Health power-up does not exceed max health
- Pause menu opens/closes with Esc
- Game over displays score/wave and restart works
- Main menu start/quit buttons work
- Background scrolls continuously
- Audio clips play when assigned

---

## 13) Notes

- This setup is intentionally simple and production-structured for quick extension.
- You can later add boss waves, pooling, VFX, screen shake, and save high score.
