# Space Shooter (Unity 2D, Windows Build)

A simple but fully playable 2D space-shooter project for Unity using C#.

## Features Included
- Player ship movement + shooting
- Enemy wave spawning with multiple enemy behaviors
- Bullet/projectile system
- Collision + damage handling
- Health + scoring systems
- Power-up system (Health, Rapid Fire, Shield)
- Game manager with game states and wave progression
- UI flow: Main Menu, HUD, Game Over, Pause
- Background scrolling (parallax + starfield)
- Script-based particle explosion effects
- Input handling via a centralized input script

## Project Structure

```text
Assets/
├── Audio/
├── Prefabs/
│   └── PREFAB_SETUP.md
├── Resources/
│   └── .gitkeep
├── Scenes/
│   └── SCENE_SETUP.md
├── Scripts/
│   ├── Effects/
│   │   └── ExplosionParticles.cs
│   ├── Enemy/
│   │   └── EnemyController.cs
│   ├── Environment/
│   │   ├── ParallaxBackground.cs
│   │   └── StarField.cs
│   ├── Input/
│   │   └── InputHandler.cs
│   ├── Managers/
│   │   ├── AudioManager.cs
│   │   ├── GameManager.cs
│   │   └── SpawnManager.cs
│   ├── Player/
│   │   └── PlayerController.cs
│   ├── PowerUps/
│   │   └── PowerUpController.cs
│   ├── UI/
│   │   ├── UIManager.cs
│   │   ├── MainMenuUI.cs
│   │   ├── HUDUI.cs
│   │   └── GameOverUI.cs
│   ├── Utils/
│   │   ├── AutoDestroy.cs
│   │   ├── ExplosionEffect.cs
│   │   └── ScreenBounds.cs
│   └── Weapons/
│       └── BulletController.cs
└── Sprites/
```

## Core Gameplay Scripts
- `PlayerController.cs`: movement, shooting, health integration, power-up states
- `SpawnManager.cs`: wave spawn orchestration
- `EnemyController.cs`: enemy AI movement, shooting, death, score reward
- `BulletController.cs`: projectile movement, lifetime, damage payload
- `PowerUpController.cs`: pickup effects
- `GameManager.cs`: game state machine + score + waves

## UI Scripts
- `UIManager.cs`: scene-level UI orchestration
- `MainMenuUI.cs`: menu panel logic
- `HUDUI.cs`: score/wave/health HUD updates
- `GameOverUI.cs`: game over panel logic

## Build for Windows (.exe)
1. Open project in Unity Hub.
2. Open `File -> Build Settings`.
3. Add scene(s) to **Scenes In Build**.
4. Set platform to **PC, Mac & Linux Standalone**.
5. Target Platform = **Windows**, Architecture = **x86_64**.
6. In **Player Settings**, set Product Name and desired resolution.
7. Click **Build** and choose output folder.
8. Run generated `Space Shooter.exe` in the build folder.

For full wiring/setup instructions, see `SETUP_GUIDE.md`.
