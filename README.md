# Space Shooter Game

A complete 2D space shooter game built with Unity and C# for Windows desktop.

![Unity Version](https://img.shields.io/badge/Unity-2021.3%20LTS%20or%20newer-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

## 🎮 Game Features

- **Player Controls**: Smooth movement with WASD/Arrow keys, shooting with Space
- **Multiple Enemy Types**: Basic, ZigZag, Dive bombers, and Boss enemies
- **Wave System**: Progressive difficulty with wave-based spawning
- **Power-ups**: Health, Shield, Rapid Fire, Triple Shot
- **Scoring System**: Points for destroying enemies, high score persistence
- **Visual Effects**: Parallax scrolling background, explosions, shield effects
- **Full UI**: Main menu, in-game HUD, pause menu, game over screen

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # Core game mechanics
│   │   │   ├── PlayerController.cs
│   │   │   ├── Bullet.cs
│   │   │   ├── HealthSystem.cs
│   │   │   └── Boundary.cs
│   │   ├── Enemies/        # Enemy behaviors
│   │   │   ├── EnemyBase.cs
│   │   │   ├── BasicEnemy.cs
│   │   │   ├── ZigZagEnemy.cs
│   │   │   ├── DiveEnemy.cs
│   │   │   ├── BossEnemy.cs
│   │   │   └── EnemySpawner.cs
│   │   ├── Powerups/       # Power-up system
│   │   │   ├── PowerUpBase.cs
│   │   │   └── PowerUpSpawner.cs
│   │   ├── Managers/       # Game management
│   │   │   ├── GameManager.cs
│   │   │   ├── SceneSetup.cs
│   │   │   ├── MainMenuSetup.cs
│   │   │   └── PrefabGenerator.cs
│   │   ├── Effects/        # Visual/Audio effects
│   │   │   ├── ParallaxBackground.cs
│   │   │   ├── ExplosionEffect.cs
│   │   │   └── AudioManager.cs
│   │   └── UI/             # User interface
│   │       ├── MainMenuUI.cs
│   │       ├── GameHUD.cs
│   │       ├── PauseMenuUI.cs
│   │       └── GameOverUI.cs
│   ├── Prefabs/            # Game object prefabs
│   ├── Sprites/            # Sprite assets
│   ├── Audio/              # Sound effects and music
│   ├── Scenes/             # Game scenes
│   └── UI/                 # UI assets
├── ProjectSettings/        # Unity project settings
├── Packages/               # Unity packages
└── README.md
```

## 🚀 Quick Start Guide

### Prerequisites

- **Unity Hub** (latest version)
- **Unity Editor** 2021.3 LTS or newer (2022.3 LTS recommended)
- **Windows 10/11** for building

### Step 1: Install Unity

1. Download and install [Unity Hub](https://unity.com/download)
2. Sign in with a Unity account (free Personal license works)
3. In Unity Hub, go to **Installs** → **Install Editor**
4. Select **Unity 2022.3 LTS** (or 2021.3 LTS)
5. Include these modules:
   - ✅ Windows Build Support (IL2CPP)
   - ✅ Microsoft Visual Studio Community

### Step 2: Open the Project

1. In Unity Hub, click **Open** → **Add project from disk**
2. Navigate to the `space_shooter_game` folder
3. Click **Open**
4. Wait for Unity to import all assets (first time takes a few minutes)

### Step 3: Create the Scenes

Since Unity scenes need to be created in the editor:

#### Main Menu Scene
1. Go to **File** → **New Scene**
2. Delete the default Main Camera and Directional Light
3. Create an empty GameObject, name it "SceneManager"
4. Add the `MainMenuSetup` script component to it
5. Save as `Assets/Scenes/MainMenu.unity`

#### Game Scene
1. Go to **File** → **New Scene**  
2. Delete the default Main Camera and Directional Light
3. Create an empty GameObject, name it "GameManager"
4. Add the `GameManager` script component
5. Create another empty GameObject, name it "SceneSetup"
6. Add the `SceneSetup` script component
7. Create another empty GameObject, name it "PrefabGenerator"
8. Add the `PrefabGenerator` script component
9. Save as `Assets/Scenes/Game.unity`

### Step 4: Configure Build Settings

1. Go to **File** → **Build Settings**
2. Add scenes in order:
   - `Assets/Scenes/MainMenu.unity` (index 0)
   - `Assets/Scenes/Game.unity` (index 1)
3. Select **Windows, Mac, Linux** as platform
4. Set **Target Platform** to **Windows**
5. Set **Architecture** to **x86_64**

### Step 5: Configure Player Settings

1. In Build Settings, click **Player Settings**
2. Set the following:
   - **Company Name**: Your name/company
   - **Product Name**: Space Shooter
   - **Default Screen Width**: 1920
   - **Default Screen Height**: 1080
   - **Fullscreen Mode**: Windowed (or Fullscreen)
   - **Run In Background**: ✅ Enabled

## 🔨 Building the Game

### Build for Windows

1. Go to **File** → **Build Settings**
2. Ensure **Windows** is selected as the target platform
3. Click **Build**
4. Choose a folder for the build output (e.g., `Builds/Windows`)
5. Name your executable (e.g., `SpaceShooter.exe`)
6. Click **Save** and wait for the build to complete

### Build Output
Your build folder will contain:
```
Builds/Windows/
├── SpaceShooter.exe          # Main executable
├── SpaceShooter_Data/        # Game data folder
├── MonoBleedingEdge/         # Mono runtime
└── UnityCrashHandler64.exe   # Crash handler
```

### Distributing the Game
To share your game:
1. Zip the entire build folder
2. Users can extract and run `SpaceShooter.exe` directly
3. No Unity installation required for players!

## 🎯 Game Controls

| Action | Key(s) |
|--------|--------|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Space |
| Pause | Escape |

## 🎨 Customization

### Adding Custom Sprites
1. Create your sprite images (PNG format recommended)
2. Import into `Assets/Sprites/` folders
3. Set import settings:
   - Texture Type: Sprite (2D and UI)
   - Pixels Per Unit: 32
   - Filter Mode: Point (for pixel art)
4. Assign sprites to prefabs in the Inspector

### Adding Sound Effects
1. Import audio files to `Assets/Audio/` folders
2. Supported formats: WAV, MP3, OGG
3. Assign to AudioManager component or individual scripts

### Modifying Game Balance
Edit the serialized fields in the Inspector:
- **PlayerController**: Speed, fire rate, health
- **EnemySpawner**: Wave timing, enemy counts
- **Enemy scripts**: Health, damage, speed
- **PowerUpBase**: Effect duration, spawn rates

## 🔧 Troubleshooting

### Common Issues

**Scripts not compiling:**
- Ensure all .cs files are in the Assets folder
- Check Console for compile errors
- Verify namespace references are correct

**Scenes not loading:**
- Add scenes to Build Settings
- Verify scene names match in GameManager

**Player not moving:**
- Check Input Manager settings
- Verify PlayerController is attached to Player object
- Ensure Player has "Player" tag

**Enemies not spawning:**
- Ensure EnemySpawner has prefab references
- Check that GameState is "Playing"
- Verify prefabs have correct tags ("Enemy")

**Build fails:**
- Close other Unity instances
- Delete Library folder and reimport
- Check for missing script references

## 📝 Script Reference

### Core Scripts

| Script | Purpose |
|--------|---------|
| `GameManager` | Game state, scoring, scene management |
| `PlayerController` | Player movement, shooting, power-ups |
| `Bullet` | Projectile movement and collision |
| `EnemyBase` | Base enemy behavior (abstract) |
| `EnemySpawner` | Wave-based enemy spawning |

### Enemy Types

| Enemy | Behavior |
|-------|----------|
| `BasicEnemy` | Moves straight down |
| `ZigZagEnemy` | Moves in zig-zag pattern |
| `DiveEnemy` | Dives toward player |
| `BossEnemy` | Multi-phase boss with patterns |

### UI Scripts

| Script | Purpose |
|--------|---------|
| `MainMenuUI` | Start/Quit buttons, high score |
| `GameHUD` | Score, health, wave display |
| `PauseMenuUI` | Pause functionality |
| `GameOverUI` | Final score, restart options |

## 📜 License

This project is open source. Feel free to use, modify, and distribute.

## 🙏 Credits

Created as a complete Unity game template for learning and customization.

---

**Happy Gaming! 🚀**
