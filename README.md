# 🚀 Space Shooter - Unity 2D Game

A complete, fully playable space shooter game built with Unity and C#. Features wave-based enemy progression, power-ups, multiple weapon levels, parallax scrolling backgrounds, and procedurally generated sprites and audio - no external assets required!

---

## 📋 Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Quick Start Setup](#quick-start-setup)
- [Detailed Setup Instructions](#detailed-setup-instructions)
- [Building for Windows](#building-for-windows)
- [Game Controls](#game-controls)
- [Project Architecture](#project-architecture)
- [Script Reference](#script-reference)
- [Customization Guide](#customization-guide)
- [Troubleshooting](#troubleshooting)

---

## ✨ Features

### Core Gameplay
- **Player ship** with smooth keyboard (WASD/Arrows) and optional mouse control
- **4 weapon upgrade levels** (single → double → triple spread → quad spread)
- **4 enemy types**: Basic, Fast, Tank, and Boss
- **7 movement patterns**: Straight, Zigzag, Sine wave, Diagonal, Player-tracking, Hovering
- **Wave-based progression** with increasing difficulty (enemy count, health, speed, fire rate)
- **Boss battles** every 5 waves
- **Power-up system**: Weapon upgrades, Shield, Health pickups
- **Collision detection** with invincibility frames and visual feedback

### Visual & Audio
- **All sprites generated programmatically** - no external art assets needed
- **Parallax scrolling star field** with 3 depth layers + nebula accents
- **Procedural sound effects** - laser shots, explosions, pickups, all synthesized at runtime
- **Procedural background music** - simple bass loop generated via sine waves
- **Visual effects**: Ship tilt, damage flash, invincibility blink, explosion animations

### UI System
- **Main Menu**: Play, Options (volume slider, mouse control toggle), Quit
- **In-game HUD**: Score, wave number, health bar with color gradient, weapon level
- **Pause Menu**: Resume, Restart, Main Menu (ESC to toggle)
- **Game Over Screen**: Final score, high score, wave reached, Restart/Menu/Quit
- **Wave Announcements**: Fade-in/out text for new waves and boss waves
- **High score persistence** via PlayerPrefs

### Technical
- **Object pooling** for bullets, enemies, explosions, and power-ups
- **Singleton managers** for game state, audio, object pools, and wave management
- **Event-driven architecture** for score/health/wave UI updates
- **Proper game state management** (MainMenu, Playing, Paused, GameOver)

---

## 🔧 Requirements

- **Unity 2021.3 LTS** or newer (recommended: Unity 2022.3 LTS or Unity 6)
- **Windows 10/11** for building Windows executable
- **Unity modules**: Standalone Build Support (Windows)
- No additional packages or assets required

---

## 🚀 Quick Start Setup

### Step 1: Install Unity
1. Download [Unity Hub](https://unity.com/download)
2. Install Unity **2022.3 LTS** (or newer) with **Windows Build Support**

### Step 2: Create Project from These Files
1. Open Unity Hub → **New Project**
2. Select **2D (Built-in Render Pipeline)** template
3. Name it `SpaceShooter` and create
4. **Close Unity** after the project initializes
5. Copy the contents of this repository's `Assets/` folder into the project's `Assets/` folder
6. Copy `ProjectSettings/` files into the project's `ProjectSettings/` folder
7. Re-open the project in Unity

### Step 3: Setup Scenes (One-Click)
1. In Unity, go to menu: **Space Shooter → Setup All Scenes**
   - This automatically creates both scenes and configures build settings
2. If the menu doesn't appear, do it manually (see [Detailed Setup](#detailed-setup-instructions))

### Step 4: Play!
1. Open `Assets/Scenes/MainMenuScene.unity`
2. Press **Play** ▶ in Unity Editor
3. Enjoy!

---

## 📖 Detailed Setup Instructions

### Manual Scene Creation (if the editor menu doesn't work)

#### MainMenuScene
1. **File → New Scene** (Empty Scene)
2. Create an empty GameObject, name it `MainMenuSetup`
3. Add the `MainMenuSceneSetup` component to it
4. **File → Save As** → `Assets/Scenes/MainMenuScene.unity`

#### GameScene
1. **File → New Scene** (Empty Scene)
2. Create an empty GameObject, name it `GameSetup`
3. Add the `GameSceneSetup` component to it
4. **File → Save As** → `Assets/Scenes/GameScene.unity`

#### Configure Build Settings
1. **File → Build Settings**
2. Click **Add Open Scenes** for each scene, or drag them in:
   - `Assets/Scenes/MainMenuScene.unity` (index 0 - first!)
   - `Assets/Scenes/GameScene.unity` (index 1)
3. Ensure MainMenuScene is at **index 0** (top of the list)

#### Configure Tags
1. **Edit → Project Settings → Tags and Layers**
2. Add these tags if not already present:
   - `Player`
   - `Enemy`
   - `Bullet`

> **Note**: The `GameSceneSetup.cs` and `MainMenuSceneSetup.cs` scripts handle ALL runtime object creation. Each scene only needs one empty GameObject with the corresponding setup script attached. Everything else (camera, player, enemies, UI, background, audio) is created automatically at runtime.

---

## 🔨 Building for Windows (.exe)

### Option A: Via Unity Editor
1. Open the project in Unity
2. **File → Build Settings**
3. Select **PC, Mac & Linux Standalone**
4. Set **Target Platform** to **Windows**
5. Set **Architecture** to **x86_64** (recommended)
6. Verify scenes are listed (MainMenuScene at index 0, GameScene at index 1)
7. Click **Player Settings** and configure:
   - **Product Name**: `Space Shooter`
   - **Company Name**: Your name
   - **Default Screen Width**: `1024`
   - **Default Screen Height**: `768`
   - **Fullscreen Mode**: `Windowed` (recommended for testing)
   - **Run In Background**: ✓ enabled
8. Click **Build** or **Build And Run**
9. Choose an output folder (e.g., `Build/Windows/`)
10. The output will contain:
    - `Space Shooter.exe` - the game executable
    - `Space Shooter_Data/` - required data folder
    - `UnityPlayer.dll` - required runtime
    - `MonoBleedingEdge/` - required .NET runtime

### Option B: Via Command Line (CI/CD)
```bash
# From the Unity installation directory:
Unity.exe -batchmode -nographics \
  -projectPath "C:\path\to\SpaceShooter" \
  -buildWindows64Player "C:\path\to\Build\SpaceShooter.exe" \
  -quit
```

### Distribution
To distribute the game, zip the entire build output folder. All files in the build folder are required:
```
SpaceShooter/
├── Space Shooter.exe          ← Main executable
├── Space Shooter_Data/        ← Game data (required)
├── UnityPlayer.dll            ← Unity runtime (required)
└── MonoBleedingEdge/          ← .NET runtime (required)
```

---

## 🎮 Game Controls

| Action | Key/Button |
|--------|-----------|
| Move | WASD or Arrow Keys |
| Shoot | Space or Left Mouse Button |
| Pause | Escape |
| Mouse Control | Enable in Options menu |

---

## 🏗 Project Architecture

```
Assets/
├── Scripts/
│   ├── Core/                    # Central game systems
│   │   ├── GameManager.cs       # Singleton: state, score, events
│   │   ├── GameState.cs         # Enum: MainMenu/Playing/Paused/GameOver
│   │   ├── ObjectPool.cs        # Generic object pooling system
│   │   ├── Tags.cs              # Centralized string constants
│   │   ├── GameSceneSetup.cs    # Runtime scene bootstrapper (game)
│   │   └── MainMenuSceneSetup.cs # Runtime scene bootstrapper (menu)
│   │
│   ├── Player/                  # Player systems
│   │   ├── PlayerController.cs  # Movement (keyboard + mouse)
│   │   ├── PlayerHealth.cs      # HP, damage, invincibility, shield
│   │   └── PlayerShooting.cs    # Weapon levels 1-4, fire patterns
│   │
│   ├── Enemy/                   # Enemy systems
│   │   ├── EnemyBase.cs         # Base class: health, movement, AI
│   │   └── WaveManager.cs       # Wave progression, spawning, difficulty
│   │
│   ├── Weapons/                 # Projectile systems
│   │   ├── Bullet.cs            # Bullet movement and collision
│   │   └── Explosion.cs         # Animated explosion effect
│   │
│   ├── PowerUps/                # Power-up system
│   │   ├── PowerUp.cs           # Pickup behavior and effects
│   │   └── PowerUpSpawner.cs    # Random power-up spawning
│   │
│   ├── UI/                      # All UI screens
│   │   ├── MainMenuUI.cs        # Main menu with options panel
│   │   ├── GameHUD.cs           # In-game score/health/wave display
│   │   ├── GameOverUI.cs        # Game over screen
│   │   ├── PauseMenuUI.cs       # Pause overlay
│   │   └── WaveAnnouncement.cs  # Wave start text animation
│   │
│   ├── Visual/                  # Sprite generation
│   │   └── SpriteFactory.cs     # Procedural sprite creation
│   │
│   ├── Background/              # Background effects
│   │   └── ParallaxBackground.cs # Multi-layer star parallax
│   │
│   ├── Audio/                   # Sound system
│   │   └── AudioManager.cs      # Procedural SFX + music
│   │
│   └── Editor/                  # Editor-only utilities
│       └── SceneSetupEditor.cs  # One-click scene setup menu
│
├── Scenes/
│   ├── MainMenuScene.unity      # Created via editor utility
│   └── GameScene.unity          # Created via editor utility
│
└── Prefabs/                     # (Created at runtime by ObjectPool)
```

---

## 📝 Script Reference

### GameManager (Singleton)
- `StartGame()` - Reset score/wave, set state to Playing
- `AddScore(int points)` - Add to player score
- `PauseGame()` / `ResumeGame()` / `TogglePause()`
- `RestartGame()` - Reload GameScene
- `LoadMainMenu()` - Load MainMenuScene
- Events: `OnScoreChanged`, `OnWaveChanged`, `OnPlayerHealthChanged`, `OnGameStateChanged`

### WaveManager
- Spawns enemies in waves with configurable progression
- Boss waves every 5 waves
- Stats scale per wave: health (+15%), speed (+5%), fire rate (+3%)
- Enemy cap: 30 per wave

### PlayerShooting - Weapon Levels
| Level | Pattern | Description |
|-------|---------|-------------|
| 1 | Single | One bullet straight up |
| 2 | Double | Two parallel bullets |
| 3 | Triple Spread | Three bullets in a fan |
| 4 | Quad Spread | Four bullets, wide spread |

### EnemyBase - Movement Patterns
| Pattern | Behavior |
|---------|----------|
| StraightDown | Moves straight down |
| Zigzag | Sine-wave horizontal + downward |
| Sine | Smooth sine-wave path |
| DiagonalLeft/Right | Angled descent |
| TrackPlayer | Follows player X position |
| Hover | Descends to Y=2, then strafes horizontally |

---

## 🎨 Customization Guide

### Adjust Difficulty
In `WaveManager.cs`:
```csharp
timeBetweenWaves = 4f;         // Seconds between waves
baseEnemiesPerWave = 5;        // Starting enemy count
enemiesPerWaveIncrease = 2;    // Extra enemies each wave
bossEveryNWaves = 5;           // Boss frequency
healthScalePerWave = 0.15f;    // HP scaling per wave
```

### Change Player Speed
In `PlayerController.cs`:
```csharp
moveSpeed = 8f;  // Default: 8
```

### Modify Weapon Fire Rate
In `PlayerShooting.cs`:
```csharp
baseFireRate = 0.2f;  // Seconds between shots (lower = faster)
```

### Adjust Player Health
In `GameManager.cs`:
```csharp
startingPlayerHealth = 5;  // Default: 5
```

### Change Game Bounds (play area size)
In `GameManager.cs`:
```csharp
gameBoundsX = 8f;  // Half-width of play area
gameBoundsY = 5f;  // Half-height of play area
```

### Add New Enemy Type
1. Add a new tag constant in `Tags.cs`
2. Add prefab creation in `GameSceneSetup.SetupPrefabsAndPools()`
3. Add spawn logic in `WaveManager.SpawnEnemy()`

---

## ❓ Troubleshooting

### "Tag 'Player' is not defined"
Run **Space Shooter → Setup Tags and Layers** from the Unity menu, or manually add `Player`, `Enemy`, and `Bullet` tags in **Edit → Project Settings → Tags and Layers**.

### Scenes not loading
Ensure both scenes are added to **File → Build Settings** with `MainMenuScene` at index 0.

### No visuals appearing
The `GameSceneSetup` / `MainMenuSceneSetup` script must be attached to a GameObject in each scene. Everything is created at runtime by these bootstrap scripts.

### UI text not visible
The scripts use `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`. If on a very old Unity version where this doesn't exist, change all font references to `"Arial.ttf"` instead.

### Build errors about editor scripts
`SceneSetupEditor.cs` is wrapped in `#if UNITY_EDITOR` and should be placed in an `Editor` folder. This is already the case in the project structure.

### Objects pass through each other
Ensure all colliders are set to **Is Trigger = true** (the setup scripts configure this automatically). The game uses trigger-based collision, not physics collision.

---

## 📄 License

This project is provided as-is for educational and personal use. Feel free to modify and distribute.

---

## 🎯 Version

- **v1.0** - Complete game with all systems functional
- Unity 2022.3 LTS compatible
- Tested on Windows 10/11
