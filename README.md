# 🚀 Space Shooter Game

A classic 2D space shooter game built with Unity and C# for Windows desktop.

![Genre](https://img.shields.io/badge/Genre-Space%20Shooter-blue)
![Engine](https://img.shields.io/badge/Engine-Unity%202021.3+-green)
![Platform](https://img.shields.io/badge/Platform-Windows-orange)
![Language](https://img.shields.io/badge/Language-C%23-purple)

---

## 🎮 Game Features

- **Player Controls**: WASD / Arrow Keys for movement, Space to shoot
- **Multiple Enemy Types**: 4 distinct enemies with unique movement patterns
  - **Straight** — Flies straight down (basic)
  - **Zigzag** — Weaves side to side while descending
  - **Swooper** — Swoops in arcs across the screen
  - **Tank** — Slow but heavily armored, fires faster
- **Wave-Based Progression**: Increasingly difficult enemy waves
- **Power-Up System**: Health restore, Rapid Fire, and Shield pickups
- **Score Tracking**: Points per kill with difficulty multiplier, persistent high score
- **Parallax Background**: Multi-layer scrolling starfield
- **Pause System**: Press Escape to pause/resume
- **Full Menu System**: Main Menu → Gameplay → Game Over with restart

---

## 📋 Requirements

- **Unity 2021.3 LTS** or newer (2022.x or 2023.x also work)
- **Windows 10/11** for the final build
- **~500 MB disk space** for Unity project

---

## 🛠️ Setup Instructions

### Step 1: Install Unity

1. Download [Unity Hub](https://unity.com/download)
2. Install **Unity 2021.3 LTS** (or newer) with:
   - ✅ Windows Build Support (IL2CPP)
   - ✅ Microsoft Visual Studio (or VS Code)

### Step 2: Create New Unity Project

1. Open Unity Hub → **New Project**
2. Select template: **2D (Core)**
3. Project name: `SpaceShooter`
4. Click **Create Project**

### Step 3: Import Project Files

Copy the following folders from this repository into your Unity project:

```
YOUR_UNITY_PROJECT/
├── Assets/
│   ├── Scripts/          ← Copy from this repo's Assets/Scripts/
│   │   ├── Editor/       ← Contains setup wizards (SpriteGenerator.cs, ProjectSetup.cs)
│   │   ├── GameManager.cs
│   │   ├── PlayerController.cs
│   │   ├── EnemyController.cs
│   │   ├── BulletController.cs
│   │   ├── EnemySpawner.cs
│   │   ├── PowerUpController.cs
│   │   ├── UIManager.cs
│   │   ├── MenuManager.cs
│   │   ├── BackgroundScroller.cs
│   │   ├── AudioManager.cs
│   │   └── DestroyOffScreen.cs
│   └── Sprites/          ← Copy from this repo's Assets/Sprites/
│       ├── PlayerShip.png
│       ├── EnemyStraight.png
│       ├── EnemyZigzag.png
│       ├── EnemySwooper.png
│       ├── EnemyTank.png
│       ├── PlayerBullet.png
│       ├── EnemyBullet.png
│       ├── PowerUpHealth.png
│       ├── PowerUpRapidFire.png
│       ├── PowerUpShield.png
│       ├── ShieldBubble.png
│       └── StarBackground.png
└── ProjectSettings/       ← Optionally copy TagManager.asset, InputManager.asset
```

### Step 4: Run Automated Setup (Recommended)

After importing all files, use the automated setup wizard:

1. In Unity, go to the top menu: **Tools → Space Shooter → Setup Entire Project**
2. This will automatically:
   - Create all required tags (Player, Enemy, PlayerBullet, EnemyBullet, PowerUp)
   - Generate sprites (if not already present)
   - Create all prefabs with correct components and references
   - Build all 3 scenes (MainMenu, GamePlay, GameOver)
   - Configure build settings
3. You'll see a "Setup Complete!" dialog when done

### Step 5 (Alternative): Manual Setup

If the automated setup doesn't work, see:
- [docs/PREFAB_SETUP.md](docs/PREFAB_SETUP.md) — Detailed prefab configurations
- [docs/SCENE_SETUP.md](docs/SCENE_SETUP.md) — Complete scene hierarchies

---

## 🎨 Sprites

Pre-generated sprite PNGs are included in `Assets/Sprites/`. These are simple colored geometric shapes:

| Sprite | Description |
|--------|-------------|
| PlayerShip.png | Blue arrow-shaped player ship (64×64) |
| EnemyStraight.png | Red inverted triangle (48×48) |
| EnemyZigzag.png | Orange diamond shape (48×48) |
| EnemySwooper.png | Purple crescent (48×48) |
| EnemyTank.png | Gray/red rectangle (56×56) |
| PlayerBullet.png | Green elongated bullet (8×16) |
| EnemyBullet.png | Red elongated bullet (8×16) |
| PowerUpHealth.png | Green circle with plus sign (32×32) |
| PowerUpRapidFire.png | Yellow circle with arrow (32×32) |
| PowerUpShield.png | Blue circle with shield icon (32×32) |
| ShieldBubble.png | Translucent blue bubble (80×80) |
| StarBackground.png | Dark starfield (512×1024) |

**To regenerate sprites** (requires Python + Pillow):
```bash
pip install Pillow
python tools/generate_sprites.py
```

**To regenerate inside Unity**:
- Menu: **Tools → Space Shooter → Generate All Sprites**

---

## 🔊 Audio

Audio clips are **optional**. The game runs fine without them — the AudioManager gracefully handles missing clips.

To add sound effects:
1. Place `.wav` or `.mp3` files in `Assets/Audio/`
2. Select the **AudioManager** object in the scene
3. Drag audio clips to the appropriate slots:
   - Player Shoot Clip, Enemy Shoot Clip
   - Player Hit Clip, Enemy Death Clip
   - Player Death Clip, Game Over Clip
   - Power Up Clip, Shield Break Clip
   - Button Click Clip
   - Menu Music, Game Music

**Free sound effect sources:**
- [Freesound.org](https://freesound.org)
- [OpenGameArt.org](https://opengameart.org)
- [Kenney.nl](https://kenney.nl/assets?q=audio)

---

## 🏗️ Building for Windows

### From Unity Editor:

1. **File → Build Settings**
2. Verify scenes are listed in order:
   - `Scenes/MainMenu` (index 0)
   - `Scenes/GamePlay` (index 1)
   - `Scenes/GameOver` (index 2)
3. **Platform**: PC, Mac & Linux Standalone
4. **Target Platform**: Windows
5. **Architecture**: x86_64
6. Click **Build** or **Build And Run**
7. Choose an output folder (e.g., `Build/`)
8. The executable `Space Shooter.exe` will be created

### Build Output Structure:
```
Build/
├── Space Shooter.exe          ← Run this!
├── Space Shooter_Data/
├── MonoBleedingEdge/
└── UnityPlayer.dll
```

### Distribution:
Zip the entire `Build/` folder to share the game. No Unity installation needed to play!

---

## 🕹️ Controls

| Key | Action |
|-----|--------|
| W / ↑ | Move Up |
| S / ↓ | Move Down |
| A / ← | Move Left |
| D / → | Move Right |
| Space | Shoot |
| Escape | Pause / Resume |

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Editor/
│   │   │   ├── SpriteGenerator.cs    # Editor tool: generates sprite PNGs
│   │   │   └── ProjectSetup.cs       # Editor tool: automated full project setup
│   │   ├── GameManager.cs            # Game state, scoring, wave management
│   │   ├── PlayerController.cs       # Player movement, shooting, health, power-ups
│   │   ├── EnemyController.cs        # Enemy AI, movement patterns, shooting
│   │   ├── BulletController.cs       # Bullet movement and lifetime
│   │   ├── EnemySpawner.cs           # Wave-based enemy spawning
│   │   ├── PowerUpController.cs      # Power-up behavior and types
│   │   ├── UIManager.cs              # In-game HUD and pause menu
│   │   ├── MenuManager.cs            # Main menu and game over screens
│   │   ├── BackgroundScroller.cs     # Parallax scrolling background
│   │   ├── AudioManager.cs           # Sound effect management
│   │   └── DestroyOffScreen.cs       # Utility: cleanup off-screen objects
│   ├── Sprites/                      # Pre-generated PNG sprites
│   ├── Prefabs/                      # Created by setup wizard
│   ├── Scenes/                       # Created by setup wizard
│   ├── Audio/                        # Place audio files here (optional)
│   └── Materials/                    # For custom materials (optional)
├── ProjectSettings/
│   ├── ProjectSettings.asset         # Player settings, resolution
│   ├── TagManager.asset              # Custom tags and layers
│   ├── InputManager.asset            # Input axis configuration
│   └── EditorBuildSettings.asset     # Scene build order
├── Packages/
│   └── manifest.json                 # Unity package dependencies
├── tools/
│   └── generate_sprites.py           # Standalone Python sprite generator
├── docs/
│   ├── PREFAB_SETUP.md               # Detailed prefab configuration reference
│   └── SCENE_SETUP.md                # Complete scene hierarchy reference
└── README.md                         # This file
```

---

## 🎯 Game Design

### Wave Progression
| Wave | Enemy Types Available | Enemies Per Wave |
|------|----------------------|------------------|
| 1-2 | Straight only | 5-7 |
| 3-4 | Straight + Zigzag | 9-11 |
| 5-7 | Straight + Zigzag + Swooper + Tank (rare) | 13-17 |
| 8+ | All types (more Tanks & Swoopers) | 19+ |

### Difficulty Scaling
- Each wave increases difficulty multiplier by 0.15
- Enemy speed increases with difficulty
- Enemy fire rate increases with difficulty
- Score points are multiplied by difficulty level
- Wave completion bonus: `wave_number × 50` points

### Power-Up Drop Rates
- 15% chance on any enemy kill
- Equal probability for Health, Rapid Fire, or Shield
- Power-ups fall slowly and expire after 10 seconds (flash when expiring)

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| Scripts not compiling | Ensure Unity 2021.3+ and all scripts are in `Assets/Scripts/` |
| Tags missing | Run `Tools > Space Shooter > Setup Entire Project` or add manually |
| Sprites not showing | Check Texture Type is "Sprite (2D and UI)" in import settings |
| Collisions not working | Ensure colliders are triggers and tags match exactly |
| UI not scaling | Canvas Scaler should be "Scale With Screen Size" at 800×600 |
| Game doesn't start | Verify MainMenu is scene index 0 in Build Settings |
| No sound | Audio is optional; add clips to AudioManager or ignore |

---

## 📄 License

This project is provided as-is for educational and personal use. Feel free to modify and distribute.

---

*Built with Unity and C# — Happy shooting! 🚀*
