# Space Shooter (Unity C# - Windows Desktop)

A simple but fully functional 2D space-shooter project for Unity.

## Features Included

- Player movement and shooting
- Enemy wave spawner with wave progression
- Bullet system with patterns (Single, Spread, Burst)
- Collision and damage handling
- Player health + lives system
- Score system
- Power-ups:
  - Shield
  - Rapid Fire
  - Health Restore
- Parallax scrolling background
- UI:
  - Main Menu
  - HUD (Health, Score, Wave, Lives)
  - Pause Menu
  - Game Over screen
- Game state management (MainMenu/Playing/Paused/GameOver)
- Audio integration points (shoot/explosion/hit/power-up/game over/wave start/ui click)

---

## Project Structure

```text
Assets/
  Audio/
  Editor/
    InputManagerSetup.cs
    PlaceholderSpriteGenerator.cs
  Prefabs/
  Scenes/
  Scripts/
    Audio/
      AudioManager.cs
    Background/
      BackgroundManager.cs
      ParallaxLayer.cs
    Core/
      GameManager.cs
      Health.cs
    Enemies/
      EnemyBase.cs
      StraightEnemy.cs
      ZigZagEnemy.cs
      TankEnemy.cs
      EnemySpawner.cs
    Player/
      PlayerController.cs
    PowerUps/
      PowerUp.cs
      PowerUpSpawner.cs
    UI/
      MainMenuUI.cs
      HUDController.cs
      PauseMenuUI.cs
      GameOverUI.cs
    Utils/
      DestroyOnBoundsExit.cs
    Weapons/
      Bullet.cs
      WeaponSystem.cs
  Sprites/
ProjectSettings/
README.md
```

---

## Unity Version

Use **Unity 2022.3 LTS** (or newer LTS with 2D support).

---

## Step-by-Step Setup

### 1) Open project

1. Open Unity Hub.
2. Click **Add** and select this repository folder.
3. Open the project with Unity 2022.3 LTS.

### 2) Configure tags and layers

In **Edit > Project Settings > Tags and Layers** create tags:
- `Player`
- `Enemy`
- `Bounds`

### 3) Configure input (legacy input system)

This project uses Unity legacy input (`Input.GetAxis`, `Input.GetButton`).

- Horizontal: A/D or Left/Right
- Vertical: W/S or Up/Down
- Fire1: Left Mouse (or Ctrl if mapped)

To auto-configure missing axes:
1. In Unity top menu go to **Tools > Space Shooter > Configure Legacy Input**.

### 4) Generate placeholder sprites (optional)

If you want quick placeholder art:
1. In Unity top menu go to **Tools > Space Shooter > Generate Placeholder Sprites**.
2. This writes simple PNGs into `Assets/Sprites`.

### 5) Create scenes

Create 2 scenes in `Assets/Scenes`:
- `MainMenu.unity`
- `Game.unity`

Add both scenes to **File > Build Settings > Scenes In Build** in this order:
1. MainMenu
2. Game

### 6) MainMenu scene setup

1. Create empty object: `GameBootstrap`
   - Add `GameManager` (Scripts/Core)
   - Add `AudioManager` (Scripts/Audio)
   - Add an `AudioSource` and assign in `AudioManager`.
2. Add Canvas with title + buttons:
   - Play button -> `MainMenuUI.OnStartClicked()`
   - Quit button -> `MainMenuUI.OnQuitClicked()`
3. Add `MainMenuUI` component to a UI controller object.

### 7) Game scene setup

#### Player
1. Create `Player` object with:
   - SpriteRenderer
   - Rigidbody2D (Kinematic)
   - Collider2D (Is Trigger enabled)
   - `Health`
   - `PlayerController`
   - Child fire point transform
   - `WeaponSystem` (assign bullet prefab + fire point, firedByPlayer=true)
2. Tag object as `Player`.
3. Optional child `ShieldVisual` object and assign to `PlayerController`.

#### Bullet prefabs
Create two prefabs (player and enemy bullets):
- SpriteRenderer
- Collider2D (Is Trigger)
- `Bullet`
- `DestroyOnBoundsExit`

For enemy bullet weapon, use `WeaponSystem` with `firedByPlayer=false`.

#### Enemies (3 prefabs)
Create prefabs for:
- `StraightEnemy`
- `ZigZagEnemy`
- `TankEnemy`

Each should have:
- SpriteRenderer
- Rigidbody2D (Kinematic)
- Collider2D (Is Trigger)
- `Health`
- One of the enemy scripts above
- Optional child fire point + `WeaponSystem` for shooting enemies
- Tag: `Enemy`

#### Enemy spawner
1. Create empty object `EnemySpawner`.
2. Add `EnemySpawner` script.
3. Populate enemy prefab list and spawn chances.

#### Power-ups
1. Create 3 power-up prefabs using `PowerUp` script:
   - Shield
   - RapidFire
   - HealthRestore
2. Add collider trigger.
3. Create object `PowerUpSpawner` and add `PowerUpSpawner` script.
4. Assign prefabs.

#### Background
1. Create two background layer objects with tiled sprites.
2. Add `ParallaxLayer` to each (different speeds for depth effect).
3. Parent them under `BackgroundRoot` with `BackgroundManager`.

#### HUD + Menus
1. Add Canvas HUD texts/sliders and assign to `HUDController`:
   - Score text
   - Wave text
   - Lives text
   - Health slider
2. Add pause panel and hook to `PauseMenuUI`.
3. Add game-over panel and hook to `GameOverUI`.

### 8) Audio integration

`AudioManager` has clip slots for:
- shoot
- explosion
- player hit
- power-up
- game over
- wave start
- ui click

Assign clips from `Assets/Audio` to these fields.

### 9) Play controls

- Move: **WASD / Arrow Keys**
- Shoot: **Left Mouse / Fire1**
- Pause: **Escape**

---

## Build Windows .exe

1. Open **File > Build Settings**.
2. Platform: **PC, Mac & Linux Standalone**.
3. Target Platform: **Windows**.
4. Architecture: **x86_64**.
5. Click **Build**.
6. Choose output folder (for example `Builds/Windows`).
7. Unity generates:
   - `SpaceShooter.exe`
   - Data folder and required runtime files.

Run `SpaceShooter.exe` on Windows.

---

## Notes

- This is intentionally lightweight and easy to extend.
- For polishing, you can add animation, particle effects, enemy bosses, and progression upgrades.
