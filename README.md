# Space Shooter (Unity, Windows Desktop)

A complete, simple 2D space-shooter Unity project for Windows standalone builds.

## Unity Version
- Target: **Unity 2022.3 LTS**
- Scripting backend: Mono or IL2CPP (both supported)
- Input: **Legacy Input Manager** (`Input.GetAxisRaw`, `Input.GetKey`)

## Implemented Gameplay Features
- Player ship movement with **WASD / Arrow keys**
- Spacebar shooting
- Enemy wave spawning with progression
- Enemy bullet patterns (straight, aimed, spread)
- Player health and death handling
- Score + high score persistence (`PlayerPrefs`)
- Power-ups:
  - Shield
  - Rapid Fire
  - Health restore
- Background parallax scrolling
- Main Menu
- Pause menu (Esc)
- Game Over screen with restart / menu actions

## Controls
- Move: `WASD` or `Arrow Keys`
- Shoot: `Space`
- Pause/Resume: `Esc`

## Project Structure

```text
Space-Shooter/
├── Assets/
│   ├── Audio/
│   │   └── (optional audio files / placeholders)
│   ├── Prefabs/
│   │   └── PREFAB_CONFIGURATION.md
│   ├── Scenes/
│   │   └── SCENE_CONFIGURATION.md
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── AudioManager.cs
│   │   │   ├── EntityFactory.cs
│   │   │   ├── GameBootstrap.cs
│   │   │   ├── GameManager.cs
│   │   │   └── SpriteFactory.cs
│   │   ├── Gameplay/
│   │   │   ├── BulletController.cs
│   │   │   ├── EnemyController.cs
│   │   │   ├── PlayerController.cs
│   │   │   ├── PowerUpController.cs
│   │   │   └── SpawnManager.cs
│   │   ├── Environment/
│   │   │   └── ParallaxScroller.cs
│   │   ├── UI/
│   │   │   ├── UIBuilder.cs
│   │   │   └── UIManager.cs
│   │   └── Editor/
│   │       └── SceneSetupEditor.cs
│   └── Sprites/
│       └── (optional custom art)
├── Packages/
│   └── manifest.json
├── ProjectSettings/
│   ├── InputManager.asset
│   ├── ProjectSettings.asset
│   └── ProjectVersion.txt
└── README.md
```

## How to Open the Project in Unity
1. Open **Unity Hub**.
2. Click **Open** and select this repository root folder (`Space-Shooter`).
3. Let Unity import scripts and assets.
4. If `Assets/Scenes/Main.unity` does not exist, create it via:
   - **Tools → Space Shooter → Create Main Scene**
5. Open `Assets/Scenes/Main.unity`.
6. Press **Play**.

## Scene / Prefab Configuration Notes
- This project uses **runtime-generated entities** via `EntityFactory` (player, enemies, bullets, power-ups).
- UI is built fully in code by `UIBuilder`.
- Scene setup details are in:
  - `Assets/Scenes/SCENE_CONFIGURATION.md`
  - `Assets/Prefabs/PREFAB_CONFIGURATION.md`

## Input Setup
This project uses Unity's **Legacy Input Manager**.
Defaults expected:
- Axis `Horizontal`
- Axis `Vertical`

Both are included in `ProjectSettings/InputManager.asset`.

## Sound Effect Integration
`AudioManager` loads clips from:
- `Assets/Resources/Audio/shoot.wav`
- `Assets/Resources/Audio/explosion.wav`
- `Assets/Resources/Audio/powerup.wav`
- `Assets/Resources/Audio/wave_start.wav`
- `Assets/Resources/Audio/game_over.wav`

If any clip is missing, the game still runs silently for that sound.

### Add custom audio
1. Create folder: `Assets/Resources/Audio/`
2. Add your WAV/OGG files with the exact names above.
3. Re-enter Play mode.

## Custom Sprites (Optional)
Current sprites are generated procedurally by `SpriteFactory`.
To use your own art:
1. Import images into `Assets/Sprites/`
2. Update `EntityFactory` sprite assignments to use your imported sprites
3. Keep colliders and controllers unchanged

## Build for Windows (.exe)
1. In Unity: **File → Build Settings**
2. Add scene `Assets/Scenes/Main.unity` to **Scenes In Build**
3. Select platform: **PC, Mac & Linux Standalone**
4. Target platform: **Windows**, architecture: **x86_64**
5. Click **Player Settings** and set:
   - Product Name: `Space Shooter`
   - Default resolution: e.g. `1280x720`
6. Click **Build**
7. Choose an output folder (example: `Build/Windows`)
8. Run generated `Space Shooter.exe`

## Collision Handling Summary
- Player bullets damage enemies
- Enemy bullets damage player
- Player colliding with enemy causes player damage and removes enemy
- Player colliding with power-up applies effect and consumes pickup

## Wave Progression Logic
- Wave enemy count = `baseEnemiesPerWave + (wave-1) * enemiesPerWaveGrowth`
- Spawn interval decreases gradually as waves increase
- When all enemies in a wave are spawned and destroyed, next wave begins after delay

## Notes
- Desktop game project (not web).
- Designed for Windows standalone executable builds.
