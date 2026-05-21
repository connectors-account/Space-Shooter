# 🚀 Space Shooter — Unity C# Game (Windows Desktop)

A complete, fully functional 2D space-shooter game built with Unity and C#.
Targets **Windows x86_64** as a standalone desktop application.

---

## Game Features

- **Player ship** with WASD / Arrow key movement and Space bar shooting
- **Enemy waves** that increase in difficulty (more enemies, faster spawns, tougher types)
- **3 enemy types**: Basic (straight line), Zigzag (weaving), Shooter (fires back)
- **Health system**: Player has 3 HP with invincibility frames on hit
- **Scoring system**: Points per enemy destroyed, persistent high score via PlayerPrefs
- **2 power-up types**: Rapid Fire (faster shooting) and Shield (absorbs one hit)
- **Collision detection** between bullets, player, and enemies using Unity 2D Triggers
- **Game Over screen** with score, high score, restart, and return-to-menu buttons
- **Main Menu** with play, quit, high score display, and controls info

---

## Project Structure

```
space_shooter_unity/
│
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs        — Game state, scoring, wave management
│   │   │   ├── PlayerController.cs   — Player movement, shooting, health, power-ups
│   │   │   ├── EnemyController.cs    — Enemy behavior, movement patterns, health
│   │   │   ├── BulletController.cs   — Bullet movement and lifetime
│   │   │   ├── PowerUpController.cs  — Power-up drift and player collection
│   │   │   └── SpawnManager.cs       — Enemy/power-up spawning and wave logic
│   │   │
│   │   ├── UI/
│   │   │   ├── UIManager.cs          — HUD, wave banner, game over screen
│   │   │   └── MainMenuController.cs — Main menu buttons and display
│   │   │
│   │   └── Utility/
│   │       ├── BackgroundScroller.cs  — Scrolling star-field background
│   │       ├── GameStarter.cs         — Auto-starts game on scene load
│   │       └── DestroyOffScreen.cs    — Cleans up off-screen objects
│   │
│   ├── Prefabs/       ← (created in Unity Editor)
│   ├── Scenes/        ← (Game.unity + MainMenu.unity)
│   ├── Materials/     ← (optional materials)
│   └── Sprites/       ← (optional custom sprites)
│
├── BUILD_INSTRUCTIONS.md   ← Full step-by-step setup & build guide
└── README.md               ← This file
```

---

## Quick Start

1. Install **Unity 2021.3 LTS** (or newer) with the **Windows Build Support** module.
2. Create a new **2D** project in Unity Hub.
3. Copy all scripts into the correct `Assets/Scripts/` subfolders.
4. Follow the **[BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)** to:
   - Set up tags
   - Create GameObjects and prefabs
   - Configure the UI
   - Build the Windows `.exe`

---

## Controls

| Input | Action |
|-------|--------|
| WASD / Arrow Keys | Move the player ship |
| Space | Shoot |
| Escape | Quit (build only) |

---

## How It Works

### Wave System
- Waves start with 4 enemies and scale by 1.2× each wave.
- Wave 1: basic enemies only. Wave 2+: zigzag enemies appear. Wave 5+: shooter enemies.
- Enemy speed increases with each wave.
- From wave 3, some enemies have 2 HP and award 200 points.

### Power-Ups
- **Rapid Fire** (gold): Increases fire rate for 5 seconds.
- **Shield** (blue): Absorbs the next hit, then breaks.
- Power-ups have a 15% chance to spawn alongside each enemy.

### Scoring
- Basic enemy: 100 pts
- Zigzag enemy: 150 pts
- Shooter enemy: 200 pts
- Tough enemy (2 HP): 200 pts
- High scores are saved locally via `PlayerPrefs`.

---

## Build Output

After building, you get:
```
Build/
├── Space Shooter.exe
├── Space Shooter_Data/
├── UnityPlayer.dll
└── MonoBleedingEdge/
```
Zip the entire folder to distribute. The `.exe` requires its companion files.

---

## Requirements

- **Unity 2021.3 LTS** or newer
- **Windows 10/11** for building and running
- No additional packages or assets required — uses Unity primitives (squares, triangles, circles)

---

## License

Free to use, modify, and distribute. No attribution required.
