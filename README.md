# 🚀 Space Shooter

A complete arcade-style space shooter game built with Unity and C#.
Defend the galaxy against waves of increasingly difficult enemies!

![Genre: Arcade Space Shooter](https://img.shields.io/badge/Genre-Space%20Shooter-blue)
![Engine: Unity](https://img.shields.io/badge/Engine-Unity%202021.3%2B-green)
![Platform: Windows](https://img.shields.io/badge/Platform-Windows-orange)

---

## 🎮 Game Features

### Core Gameplay
- **Player Ship**: Smooth movement with WASD/Arrow Keys, auto-fire with Space
- **Progressive Waves**: Enemy difficulty increases with each wave
- **Score System**: Track your score and compete against your high score
- **Lives System**: 3 lives with respawn invincibility
- **Health Bar**: Take multiple hits before losing a life

### Enemy Types (4 unique types)
| Enemy | Behavior | Health | Score |
|-------|----------|--------|-------|
| **Straight** | Flies down, fires single shots | Low | 100 |
| **Zigzag** | Weaves side-to-side, fires spread shots | Medium | 150 |
| **Diver** | Locks onto player position and dives | Low | 120 |
| **Tank** | Slow-moving, high health, aimed shots | High | 300 |

### Power-Up System (3 types)
| Power-Up | Color | Effect |
|----------|-------|--------|
| **Weapon Upgrade** | 🟠 Orange | Increases weapon level (up to 5) |
| **Shield** | 🔵 Blue | Absorbs one hit completely |
| **Health Pack** | 🟢 Green | Restores 30 health points |

### Weapon Levels
1. **Level 1**: Single shot
2. **Level 2**: Double shot
3. **Level 3**: Triple spread shot
4. **Level 4+**: Five-way spread shot

### Visual Effects
- Procedurally generated pixel-art sprites
- Multi-layer parallax starfield (3 depth layers)
- Player invincibility flash effect
- Enemy hit flash effect
- Smooth ship movement with dampening

### UI System
- **Main Menu**: Title screen with start/quit buttons and high score display
- **HUD**: Score, lives, wave counter, health bar, shield indicator
- **Wave Announcements**: Fade-in/fade-out wave number display
- **Pause Menu**: Resume/Main Menu options (ESC key)
- **Game Over Screen**: Final score, high score, restart/menu options

---

## 🎹 Controls

| Key | Action |
|-----|--------|
| **WASD** / **Arrow Keys** | Move ship |
| **Space** | Fire weapons |
| **ESC** | Pause / Resume |

---

## 📁 Project Structure

```
space_shooter_unity/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   └── PlayerController.cs       # Player movement, shooting, health
│   │   ├── Enemies/
│   │   │   ├── EnemyBase.cs              # Abstract base class for all enemies
│   │   │   ├── EnemyStraight.cs          # Straight-flying enemy
│   │   │   ├── EnemyZigzag.cs            # Zigzag pattern enemy
│   │   │   ├── EnemyTank.cs              # Heavy tank enemy
│   │   │   └── EnemyDiver.cs             # Dive-bomber enemy
│   │   ├── Weapons/
│   │   │   └── Bullet.cs                 # Projectile system
│   │   ├── PowerUps/
│   │   │   ├── PowerUp.cs                # Power-up pickup behavior
│   │   │   └── PowerUpSpawner.cs         # Power-up spawn manager
│   │   ├── Managers/
│   │   │   ├── GameManager.cs            # Game state, score, lives, waves
│   │   │   └── EnemySpawner.cs           # Wave-based enemy spawning
│   │   ├── UI/
│   │   │   └── UIManager.cs              # All UI screens and HUD
│   │   ├── Background/
│   │   │   ├── ParallaxBackground.cs     # Sprite-based parallax scrolling
│   │   │   └── StarFieldGenerator.cs     # Procedural starfield
│   │   ├── Audio/
│   │   │   └── AudioManager.cs           # Sound effects and music
│   │   └── Utils/
│   │       ├── SpriteGenerator.cs        # Procedural sprite creation
│   │       └── GameBootstrapper.cs       # Runtime scene setup
│   ├── Editor/
│   │   ├── SceneSetupEditor.cs           # One-click scene setup tool
│   │   └── QuickBuild.cs                # One-click build tool
│   ├── Prefabs/                          # (Auto-generated at runtime)
│   ├── Sprites/                          # (Procedurally generated)
│   ├── Scenes/                           # Game scene
│   └── Materials/
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   ├── Physics2DSettings.asset
│   └── QualitySettings.asset
├── BUILD_INSTRUCTIONS.md                 # Complete build guide
└── README.md                             # This file
```

---

## 🏗️ Architecture

### Design Patterns
- **Singleton**: GameManager, UIManager, AudioManager, PowerUpSpawner
- **Inheritance**: EnemyBase → EnemyStraight, EnemyZigzag, EnemyTank, EnemyDiver
- **Observer**: Events flow through manager singletons
- **Object Pooling Ready**: Bullet and enemy systems designed for easy pooling

### Game Loop
```
MainMenu → StartGame → Wave N → [All enemies defeated] → Wave N+1 → ...
                         ↕                                    ↕
                    Pause/Resume                         Player Death
                         ↕                                    ↕
                      Paused                          Lives > 0 → Respawn
                                                      Lives = 0 → GameOver
```

### Bootstrapper System
The `GameBootstrapper` component creates ALL game objects at runtime:
- No manual prefab setup required
- Procedurally generates sprites
- Builds complete UI hierarchy
- Sets up all manager singletons
- Assigns all cross-references via reflection

This means you only need **one empty GameObject** with `GameBootstrapper` attached
to have a fully working game.

---

## 🔧 Quick Start

1. Create a new **2D Unity Project** (2021.3 LTS or newer)
2. Copy the `Assets/` and `ProjectSettings/` folders into the project
3. Open Unity, go to **Tools → Space Shooter → Create New Scene and Setup**
4. Press **Play** ▶

See [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) for complete build-to-exe instructions.

---

## 🎵 Audio

The game includes a full **AudioManager** with support for:
- Background music (looping)
- 9 sound effect slots (shoot, hit, explosion, power-up, etc.)
- SFX audio source pooling (10 concurrent sounds)
- Volume control for music and SFX separately

Audio clips are optional - the game runs perfectly without them.
See BUILD_INSTRUCTIONS.md for instructions on adding free sound effects.

---

## 📊 Wave Scaling

| Wave | Enemies | Health Mult | Speed Mult | Enemy Mix |
|------|---------|-------------|------------|----------|
| 1 | 5 | 1.0x | 1.0x | 70% Straight, 20% Diver, 10% Zigzag |
| 3 | 11 | 1.3x | 1.1x | 40% Straight, 25% Zigzag, 20% Diver, 15% Tank |
| 5 | 17 | 1.6x | 1.2x | Mixed with more tanks |
| 10 | 30 (cap) | 2.35x | 1.45x | Equal distribution of all types |

---

## 📄 License

This project is provided as-is for educational and personal use.
Feel free to modify, extend, and distribute.
