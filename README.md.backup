# 🚀 Space Shooter - Unity Game

A complete 2D space shooter game built with Unity and C#. Features player ship combat, 3 enemy types, power-ups, wave progression, and full UI.

![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue)
![C#](https://img.shields.io/badge/C%23-.NET-purple)
![Platform](https://img.shields.io/badge/Platform-Windows-green)

---

## 🎮 Game Features

- **Player ship** with smooth keyboard movement (Arrow Keys/WASD) and shooting (Spacebar)
- **3 enemy types** with unique AI behaviors:
  - **Basic** — Flies straight down, fires occasionally
  - **Zigzag** — Weaves left/right, fires aimed shots at player
  - **Tank** — Slow-moving, high health, fires burst shots
- **Wave progression** — Enemies increase in number and difficulty each wave
- **3 Power-up types:**
  - 💚 **Health Pack** — Restores 30 HP
  - 🔶 **Rapid Fire** — Doubles fire rate for 5 seconds
  - 🔵 **Shield** — Blocks all damage for 8 seconds
- **Collision system** with trigger-based 2D physics
- **Health system** with invincibility frames after damage
- **Score tracking** with persistent high score (saved via PlayerPrefs)
- **Full UI system:** Main Menu → Gameplay HUD → Game Over Screen → Pause Menu
- **Procedural star field** background with parallax scrolling
- **Sound effects** for shooting, explosions, power-ups, and UI

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   └── PlayerController.cs      # Ship movement, shooting, health, power-ups
│   │   ├── Enemy/
│   │   │   └── EnemyController.cs       # 3 enemy types with AI, shooting, drops
│   │   ├── Weapons/
│   │   │   └── BulletController.cs      # Universal bullet behavior
│   │   ├── PowerUps/
│   │   │   └── PowerUpController.cs     # 3 power-up types with effects
│   │   ├── Managers/
│   │   │   ├── GameManager.cs           # Score, waves, game state machine
│   │   │   ├── SpawnManager.cs          # Enemy wave spawning logic
│   │   │   └── AudioManager.cs          # Centralized sound management
│   │   ├── UI/
│   │   │   └── UIManager.cs             # All UI panels and HUD
│   │   ├── Environment/
│   │   │   ├── ParallaxBackground.cs    # 2-layer parallax scrolling
│   │   │   └── StarField.cs             # Procedural star generation
│   │   └── Utils/
│   │       ├── ScreenBounds.cs          # Screen boundary calculations
│   │       ├── AutoDestroy.cs           # Timed object cleanup
│   │       └── ExplosionEffect.cs       # Visual explosion animation
│   ├── Sprites/                         # All sprite assets (PNG)
│   │   ├── player_ship.png
│   │   ├── player_bullet.png
│   │   ├── enemy_bullet.png
│   │   ├── enemy_basic.png
│   │   ├── enemy_zigzag.png
│   │   ├── enemy_tank.png
│   │   ├── powerup_health.png
│   │   ├── powerup_rapidfire.png
│   │   ├── powerup_shield.png
│   │   ├── shield_bubble.png
│   │   ├── bg_layer1.png
│   │   ├── bg_layer2.png
│   │   └── explosion.png
│   ├── Audio/                           # All sound effects (WAV)
│   │   ├── shoot.wav
│   │   ├── explosion.wav
│   │   ├── powerup.wav
│   │   ├── player_hit.wav
│   │   ├── wave_start.wav
│   │   ├── game_over.wav
│   │   └── button_click.wav
│   ├── Prefabs/                         # (Created in Unity Editor)
│   ├── Scenes/                          # (Created in Unity Editor)
│   ├── Materials/
│   └── Animations/
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   ├── Physics2DSettings.asset
│   └── InputManager.asset
├── SETUP_GUIDE.md                       # Detailed step-by-step Unity setup
└── README.md                            # This file
```

---

## 🛠️ Quick Start

### Prerequisites
- [Unity Hub](https://unity.com/download) installed
- Unity Editor **2021.3 LTS** or newer (2022.3 LTS recommended)
- Windows 10/11 for building Windows executables

### Opening the Project
1. Open **Unity Hub**
2. Click **"Open"** → navigate to the `space_shooter_game` folder → Select it
3. Unity imports all assets automatically (~1-2 min)
4. Open `Assets/Scenes/GameScene.unity` (create it if needed per SETUP_GUIDE.md)

### First-Time Setup
👉 **See [SETUP_GUIDE.md](SETUP_GUIDE.md) for complete step-by-step instructions** on:
- Creating the game scene
- Setting up all GameObjects and prefabs
- Configuring the UI Canvas
- Wiring up all script references
- Testing and building

---

## 🏗️ Building for Windows (.exe)

1. Open **File → Build Settings** in Unity
2. Add your game scene to **Scenes In Build**
3. Select **PC, Mac & Linux Standalone** platform
4. Set Target Platform: **Windows**, Architecture: **x86_64**
5. Configure in **Player Settings**:
   - Product Name: `Space Shooter`
   - Resolution: 1024 × 768
   - Fullscreen Mode: Windowed
6. Click **Build**
7. Choose output folder → Wait for build
8. Run the generated `.exe` file!

### Distribution
Zip the entire build output folder:
```
SpaceShooter_Build/
├── Space Shooter.exe
├── Space Shooter_Data/
├── UnityPlayer.dll
└── MonoBleedingEdge/
```

---

## 🎯 Controls

| Key | Action |
|---|---|
| `↑ ↓ ← →` or `W A S D` | Move ship |
| `Spacebar` | Fire weapon |
| `ESC` | Pause / Resume |

---

## 📋 Script Reference

### Core Scripts

| Script | Namespace | Purpose |
|---|---|---|
| `PlayerController.cs` | `SpaceShooter.Player` | Player movement, shooting, health, power-up activation |
| `EnemyController.cs` | `SpaceShooter.Enemy` | Enemy AI (3 types), shooting patterns, health, power-up drops |
| `BulletController.cs` | `SpaceShooter.Weapons` | Bullet movement, damage, lifetime management |
| `PowerUpController.cs` | `SpaceShooter.PowerUps` | Power-up drift, bob animation, effect application |

### Manager Scripts

| Script | Pattern | Purpose |
|---|---|---|
| `GameManager.cs` | Singleton | Game state machine, score, wave progression, high score |
| `SpawnManager.cs` | Component | Wave-based enemy spawning with difficulty scaling |
| `AudioManager.cs` | Singleton | Centralized SFX and music playback |

### UI & Environment

| Script | Purpose |
|---|---|
| `UIManager.cs` | Main menu, HUD, game over, pause panel management |
| `ParallaxBackground.cs` | 2-layer infinite scrolling background |
| `StarField.cs` | Procedural star field generation |
| `ScreenBounds.cs` | Screen-to-world boundary utility |
| `ExplosionEffect.cs` | Animated explosion visual effect |
| `AutoDestroy.cs` | Timed auto-destruction for temporary objects |

---

## ⚙️ Architecture

### Game State Machine
```
MainMenu → Playing → Paused → Playing → GameOver → MainMenu
                                    └─────────────→ GameOver
```

### Event-Driven Communication
- `GameManager` fires events: `OnScoreChanged`, `OnWaveChanged`, `OnGameStateChanged`
- `PlayerController` fires events: `OnHealthChanged`, `OnPlayerDeath`
- `EnemyController` fires events: `OnEnemyDestroyed`
- `UIManager` subscribes to all events to update display

### Collision Matrix
| Object A | Object B | Result |
|---|---|---|
| PlayerBullet | Enemy | Enemy takes damage, bullet destroyed |
| EnemyBullet | Player | Player takes damage, bullet destroyed |
| Enemy | Player | Player takes contact damage |
| PowerUp | Player | Power-up effect applied, pickup destroyed |

---

## 🔧 Customization

### Adjusting Difficulty
In `GameManager` inspector:
- `Enemies Per Wave Base` — Starting enemies (default: 5)
- `Enemies Per Wave Increment` — Extra enemies per wave (default: 3)
- `Wave Cooldown` — Seconds between waves (default: 3)

In `SpawnManager` inspector:
- `Base Spawn Interval` — Time between enemy spawns (default: 1.5s)
- `Spawn Interval Reduction` — Faster spawns per wave (default: 0.1s)

### Adjusting Player
- `Move Speed` — Ship speed (default: 8)
- `Fire Rate` — Seconds between shots (default: 0.25)
- `Max Health` — Starting HP (default: 100)
- `Invincibility Duration` — I-frames after hit (default: 1.5s)

### Enemy Types
Each enemy type has configurable health, speed, score value, and fire rate in the inspector.

---

## 📝 License

This project is provided as-is for educational purposes. Feel free to modify and distribute.

---

## 🙏 Credits

- Built with **Unity Engine**
- Placeholder sprites generated programmatically (replace with your own art!)
- Sound effects generated procedurally (replace with proper audio assets!)
