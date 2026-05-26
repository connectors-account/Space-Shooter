# 🚀 Space Shooter — Unity 2D Game

A complete, production-ready 2D space shooter game built with Unity and C#.
Defend Earth against waves of alien ships, collect power-ups, and chase the high score!

## Features

- **Wave-based enemy spawning** with progressive difficulty scaling
- **4 enemy movement patterns:** Straight, Zigzag, Sine wave, and Dive-bomb
- **5 power-up types:** Weapon Upgrade, Shield, Health Restore, Rapid Fire, Score Bonus
- **3-tier weapon system** (single → double → triple shot)
- **Full UI system:** Main menu, HUD, pause screen, game over with high scores
- **Persistent high score** saved between sessions
- **Smooth player controls** with screen-boundary clamping
- **Reusable HealthSystem** component for any damageable entity
- **Audio system** with background music and SFX support

## Controls

| Action | Input |
|--------|-------|
| Move | WASD / Arrow Keys |
| Shoot | Space / Left Click |
| Pause | Escape |

## Quick Start

1. Install **Unity 2022.3 LTS** (or newer) via Unity Hub
2. Open this folder as a Unity project
3. Follow `Assets/Scenes/README_SCENE_SETUP.md` to set up the scene
4. Follow `Assets/Prefabs/README_PREFABS.md` to create prefabs
5. Press **Play** to test
6. See **BUILD_INSTRUCTIONS.md** for creating a Windows `.exe`

## Project Structure

```
Assets/
├── Scripts/           ← All game logic (10 C# scripts)
│   ├── GameManager.cs
│   ├── GameInitializer.cs
│   ├── PlayerController.cs
│   ├── EnemyController.cs
│   ├── BulletController.cs
│   ├── EnemySpawner.cs
│   ├── PowerUpController.cs
│   ├── HealthSystem.cs
│   ├── UIManager.cs
│   ├── AudioManager.cs
│   └── BackgroundScroller.cs
├── Editor/            ← Build automation script
├── Scenes/            ← Game scene
├── Prefabs/           ← Player, enemies, bullets, power-ups
├── Sprites/           ← Sprite assets (see README inside)
├── Audio/             ← Music and SFX (see README inside)
├── Materials/         ← Materials for background
└── UI/                ← UI-specific assets
```

## Building for Windows

See the full guide: **[BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)**

Quick build:
1. **File → Build Settings** (Ctrl+Shift+B)
2. Select **Windows** platform, Architecture **x86_64**
3. Click **Build** → choose output folder
4. Distribute the entire build folder (exe + data + DLLs)

## Requirements

- **Unity:** 2022.3 LTS or Unity 6 (6000.x)
- **Target OS:** Windows 10+ (64-bit)
- **Dependencies:** Physics2D, UI, Audio Unity modules (included)

## License

This project's code is provided as-is for educational and personal use.
Art and audio assets should be sourced separately (see README files in
Sprites/ and Audio/ folders for free asset recommendations).
