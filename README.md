# 🚀 Space Shooter - Unity Desktop Game

A complete space-shooter game built with Unity for Windows desktop. Features wave-based enemy spawning, multiple enemy types, power-ups, parallax scrolling backgrounds, procedural audio, and a full UI system.

![Game Type: 2D Space Shooter](https://img.shields.io/badge/Genre-Space%20Shooter-blue)
![Platform: Windows](https://img.shields.io/badge/Platform-Windows-green)
![Engine: Unity](https://img.shields.io/badge/Engine-Unity%202021.3%2B-black)

---

## 🎮 Game Features

### Core Gameplay
- **Player ship** with smooth WASD/Arrow key movement
- **Shooting system** with Space bar firing
- **Wave-based enemy spawning** with increasing difficulty
- **4 enemy types**: Straight, Zigzag, Swooper, and Boss
- **Boss battles** every 5 waves
- **Collision detection** with trigger-based 2D physics
- **Score system** with combo multiplier (up to 8x)
- **High score persistence** via PlayerPrefs

### Power-Up System
- 🟡 **Rapid Fire** - Increased fire rate
- 🟠 **Spread Shot** - 5-bullet spread pattern
- 🔵 **Shield** - Absorbs damage for a duration
- 🟢 **Health** - Restores 1 HP

### Visual Effects
- **Parallax scrolling starfield** with 3 depth layers
- **Explosion particle effects** on enemy/player death
- **Shield visual** around player when active
- **Damage flash** on enemies when hit
- **Player invincibility blink** after taking damage

### Audio
- **Procedurally generated sound effects** (no external audio files needed)
  - Shoot, explosion, hit, power-up, and menu select sounds
- **Ambient background music** (procedural drone)

### UI System
- **Main Menu** with Start Game, Quit, controls info, and high score
- **Game HUD** with score, combo multiplier, health bar, and wave counter
- **Pause Menu** (Esc key) with Resume, Main Menu, and Quit
- **Game Over Screen** with final score, wave reached, and high score
- **Wave announcements** and power-up pickup notifications

---

## 📋 Prerequisites

- **Unity 2021.3 LTS** or newer (2022.x or 2023.x also work)
  - Download from: https://unity.com/download
  - During installation, make sure to include:
    - **Windows Build Support (IL2CPP)** or **Windows Build Support (Mono)**
- **Windows 10/11** (for building the .exe)

---

## 🛠️ Setup Instructions

### Step 1: Open Project in Unity

1. Open **Unity Hub**
2. Click **"Open"** (or "Add project from disk")
3. Navigate to this project folder (`space_shooter_game/`)
4. Select the folder and click **Open**
5. Unity will import the project (this may take a few minutes on first open)

### Step 2: Run Auto-Setup

Once Unity finishes importing:

1. In the Unity menu bar, click: **SpaceShooter > Setup Game (Full Auto Setup)**
2. Click **"Yes, Set Up Everything"** in the confirmation dialog
3. Wait for setup to complete (creates sprites, prefabs, scene, and configures settings)
4. You'll see a success dialog when done

### Step 3: Test the Game

1. Press the **Play ▶** button in Unity Editor
2. The Main Menu should appear
3. Click **START GAME** to play
4. Use **WASD/Arrow Keys** to move, **Space** to shoot, **Esc** to pause

---

## 🏗️ Build Instructions (Windows .exe)

### Method 1: Unity Editor GUI

1. Go to **File > Build Settings**
2. Ensure **"Windows, Mac, Linux"** (Standalone) is selected as the platform
3. Ensure **"Assets/Scenes/MainScene"** is listed in the Scenes list
   - If not, click **"Add Open Scenes"**
4. Click **"Player Settings..."** and verify:
   - Product Name: "Space Shooter"
   - Default Screen Width: 800
   - Default Screen Height: 600
   - Fullscreen Mode: Windowed
5. Click **"Build"**
6. Choose an output folder (e.g., `Build/`)
7. Wait for the build to complete
8. Run `Space Shooter.exe` from the build folder

### Method 2: Command Line Build

```bash
# From command line (adjust Unity path for your installation)
"C:\Program Files\Unity\Hub\Editor\2021.3.xxf1\Editor\Unity.exe" \
  -batchmode \
  -nographics \
  -projectPath "path/to/space_shooter_game" \
  -buildWindows64Player "path/to/Build/SpaceShooter.exe" \
  -quit
```

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerController.cs    # Movement, shooting, power-up activation
│   │   │   └── PlayerHealth.cs        # Health, damage, invincibility, death
│   │   ├── Enemy/
│   │   │   ├── EnemyBase.cs           # Movement patterns (4 types), shooting AI
│   │   │   ├── EnemyHealth.cs         # Health, damage flash, death, loot drops
│   │   │   └── EnemySpawner.cs        # Wave system, difficulty progression
│   │   ├── Weapons/
│   │   │   └── Bullet.cs              # Projectile movement, collision handling
│   │   ├── PowerUps/
│   │   │   └── PowerUp.cs             # Power-up types, pickup, visual feedback
│   │   ├── Managers/
│   │   │   ├── GameManager.cs         # Game state (menu/play/pause/gameover)
│   │   │   ├── ScoreManager.cs        # Score tracking, combos, high scores
│   │   │   └── AudioManager.cs        # Procedural SFX and music generation
│   │   ├── UI/
│   │   │   └── UIManager.cs           # All UI panels built programmatically
│   │   ├── Effects/
│   │   │   ├── EffectsManager.cs      # Explosion particle spawning
│   │   │   └── ParallaxBackground.cs  # 3-layer scrolling starfield
│   │   └── Editor/
│   │       └── GameSetup.cs           # Auto-setup: creates scene, prefabs, sprites
│   ├── Scenes/
│   │   └── MainScene.unity            # (Created by auto-setup)
│   ├── Prefabs/                        # (Created by auto-setup)
│   │   ├── PlayerBullet.prefab
│   │   ├── EnemyBullet.prefab
│   │   ├── EnemyStraight.prefab
│   │   ├── EnemyZigzag.prefab
│   │   ├── EnemySwooper.prefab
│   │   ├── EnemyBoss.prefab
│   │   └── PowerUp.prefab
│   ├── Sprites/                        # (Created by auto-setup)
│   │   ├── PlayerShip.png
│   │   ├── EnemyStraight.png
│   │   ├── EnemyZigzag.png
│   │   ├── EnemySwooper.png
│   │   ├── EnemyBoss.png
│   │   ├── PlayerBullet.png
│   │   ├── EnemyBullet.png
│   │   └── PowerUp.png
│   └── Materials/
│       └── BackgroundMat.mat           # (Created by auto-setup)
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   ├── Physics2DSettings.asset
│   ├── InputManager.asset
│   ├── QualitySettings.asset
│   ├── AudioManager.asset
│   ├── TimeManager.asset
│   └── EditorBuildSettings.asset
├── Packages/
│   └── manifest.json
├── .gitignore
└── README.md
```

---

## 🎮 Controls

| Action     | Key(s)           |
|------------|------------------|
| Move       | WASD / Arrow Keys|
| Shoot      | Space            |
| Pause      | Escape           |

---

## 🔧 Customization

### Difficulty Tuning
Select objects in the Unity Inspector to adjust:
- **EnemySpawner**: `baseEnemiesPerWave`, `enemiesPerWaveIncrease`, `spawnInterval`
- **PlayerHealth**: `maxHealth`, `invincibilityDuration`
- **PlayerController**: `moveSpeed`, `fireRate`, `bulletSpeed`
- **EnemyBase**: `moveSpeed`, `shootInterval`, per enemy type

### Adding New Enemy Types
1. Create a new sprite in `Assets/Sprites/`
2. Add a new `EnemyType` enum value in `EnemyBase.cs`
3. Implement movement logic in `HandleMovement()`
4. Create a prefab with the Editor script or manually

---

## 📝 Technical Notes

- **No external assets required** - all sprites are pixel art generated by the Editor setup script
- **No external audio files** - all sounds are procedurally generated at runtime
- **All UI is built programmatically** - no manual UI prefab setup needed
- **Singleton pattern** used for managers (GameManager, ScoreManager, AudioManager, UIManager)
- **Tag-based identification** for game objects (Player, Enemy, Bullet, PowerUp)
- **Trigger-based 2D collisions** for all gameplay interactions

---

## 📄 License

Free to use, modify, and distribute. No attribution required.
