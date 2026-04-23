# Retro Space Shooter (Unity 2021 LTS)

A complete 2D pixel-art space shooter project for **Windows desktop** built in Unity/C#.

## Features Implemented

- 5 enemy waves with progressive difficulty scaling
- 3 enemy types:
  - **Basic**: balanced speed/health
  - **Fast**: aggressive movement + faster shots
  - **Tank**: high health, slower movement, heavier damage
- 3 power-ups:
  - **Rapid Fire**
  - **Shield**
  - **Health Restore**
- Player controls:
  - Move: **WASD** or **Arrow Keys** (`Horizontal` / `Vertical` axes)
  - Shoot: **Space** (also supports `Fire1`)
  - Pause: **Esc**
- Health + score + high score (persisted with `PlayerPrefs`)
- Wave progression UI and state handling
- Main Menu, HUD, Pause Menu, Game Over screen
- Background parallax scrolling with multiple layers
- Placeholder SFX for shoot/explosion/power-up/UI click
- Collision behavior via custom layer setup automation

## Unity Version

- Target version: **Unity 2021.3.36f1 LTS**
- Compatible with Unity 2021 LTS builds using Built-in Render Pipeline (2D)

## Project Path

```text
/home/ubuntu/space_shooter_game
```

## Folder Structure

```text
space_shooter_game/
├── Assets/
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Scripts/
│   │   ├── Audio/
│   │   ├── Background/
│   │   ├── Combat/
│   │   ├── Core/
│   │   ├── Editor/
│   │   ├── Enemy/
│   │   ├── Player/
│   │   ├── Powerups/
│   │   └── UI/
│   ├── Sounds/
│   └── Sprites/
│       ├── Background/
│       ├── Bullets/
│       ├── Enemies/
│       ├── Player/
│       ├── Powerups/
│       └── UI/
├── Packages/
└── ProjectSettings/
```

## Important One-Time Setup in Unity

Because `.prefab` and `.unity` scene assets are generated from code in this repository, run the included editor generator once after opening the project:

1. Open Unity Hub.
2. Add/open `/home/ubuntu/space_shooter_game`.
3. Let Unity import scripts/assets.
4. From menu: **Tools → Space Shooter → Generate Complete Project**.
5. This will automatically:
   - Create all required prefabs (player, enemies, bullets, power-ups, managers)
   - Create and save scenes:
     - `Assets/Scenes/MainMenu.unity`
     - `Assets/Scenes/Gameplay.unity`
   - Setup build scenes in Build Settings
   - Configure game layers and collision matrix
   - Wire UI panels/buttons/references

## Script Overview

- `GameManager.cs` — game state, scene transitions, score/high score, pause
- `PersistentBootstrap.cs` — ensures persistent managers exist in scenes
- `GameplayDirector.cs` — gameplay loop finalization (all waves done)
- `PlayerController.cs` — movement, shooting, health, shield/rapid fire timers
- `Bullet.cs` — bullet travel + trigger collision handling
- `EnemyController.cs` — per-type AI movement/shooting + damage + score
- `EnemySpawner.cs` — 5-wave progression + increasing difficulty
- `PowerUp.cs` — collectible behavior and effect dispatch
- `PowerUpSpawner.cs` — spawn chance logic on enemy destruction
- `UIManager.cs` — menu/HUD/pause/game-over updates + button actions
- `ParallaxScroller.cs` — multi-layer vertical scrolling loop
- `SoundManager.cs` — centralized SFX playback
- `SpaceShooterProjectBuilder.cs` (Editor) — full prefab/scene generation pipeline

## Sprite & Audio Assets Included

Generated assets are in:

- `Assets/Sprites/...`
  - player ship
  - basic/fast/tank enemies
  - player/enemy bullets
  - 3 power-up icons
  - 3 parallax background layers
- `Assets/Sounds/...`
  - `shoot.wav`
  - `explosion.wav`
  - `powerup.wav`
  - `ui_click.wav`

All are lightweight placeholders in retro-style and can be replaced with production art/audio.

## Input Configuration

Gameplay uses Unity legacy input (`Input.GetAxisRaw`, `Input.GetButton`) plus direct `KeyCode.Space`.

- Horizontal axis supports A/D and Left/Right (default Unity mapping)
- Vertical axis supports W/S and Up/Down (default Unity mapping)
- Shoot supports Space and Fire1

No extra package is required.

## Build Instructions (Windows .exe)

### 1) Open project
- Open `/home/ubuntu/space_shooter_game` in Unity 2021 LTS.

### 2) Generate game assets/scenes
- Run **Tools → Space Shooter → Generate Complete Project**.

### 3) Verify scenes
- Open `Assets/Scenes/MainMenu.unity`.
- Press Play in editor to test.

### 4) Build configuration
1. Go to **File → Build Settings**.
2. Platform: **PC, Mac & Linux Standalone**.
3. Target Platform: **Windows**.
4. Architecture: **x86_64**.
5. Ensure scenes list contains:
   - `MainMenu`
   - `Gameplay`

### 5) Build
1. Click **Build**.
2. Choose output folder, e.g. `Builds/Windows/`.
3. Name executable: `RetroSpaceShooter.exe`.

### 6) Run
- Launch `RetroSpaceShooter.exe` from the build folder.

## Gameplay Rules

- Survive all 5 waves while maximizing score.
- Each enemy type awards different points.
- Power-ups drop randomly from destroyed enemies.
- Player dies at 0 HP.
- High score persists between runs (`PlayerPrefs`).

## Customization Tips

- Tune wave pacing in `EnemySpawner.BuildDefaultWaves()`.
- Adjust weapon balance in `PlayerController` and `EnemyController`.
- Change power-up drop chance in `PowerUpSpawner.spawnChance`.
- Replace art/audio in `Assets/Sprites` and `Assets/Sounds` while keeping file names.

## Notes

- This project uses Unity text serialization compatibility conventions and editor automation to provide reproducible scene/prefab generation.
- If you delete generated prefabs/scenes, simply run the generator menu again.
