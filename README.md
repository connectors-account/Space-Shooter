# 🚀 Space Shooter Game

A complete arcade-style space shooter game built with Unity for Windows desktop.

## 📋 Table of Contents
- [Features](#features)
- [Requirements](#requirements)
- [Project Setup](#project-setup)
- [Game Controls](#game-controls)
- [Building for Windows](#building-for-windows)
- [Project Structure](#project-structure)
- [Customization](#customization)

## ✨ Features

### Core Gameplay
- **Player Ship**: Smooth movement with WASD/Arrow keys
- **Shooting System**: Hold Space to fire, with upgradeable weapons (5 levels)
- **Health System**: Player health bar with visual feedback
- **Shield Power-up**: Absorbs damage when active

### Enemy System
- **Basic Enemy**: Standard enemy with straight movement
- **Fast Enemy**: Quick zigzag movement pattern
- **Tank Enemy**: Slow but high health, shoots at player
- **Shooter Enemy**: Fires spread shots at the player
- **Boss Enemy**: Multi-phase attacks, high health, appears every 5 waves

### Power-ups
- **Weapon Upgrade** (Yellow): Increases weapon level (up to 5)
- **Health Pack** (Green): Restores 30 health points
- **Shield** (Blue): Absorbs 3 hits
- **Speed Boost** (Pink): Temporary speed increase

### Progression
- **Wave System**: Progressive difficulty with enemy waves
- **Infinite Waves**: Game continues with increasing difficulty
- **Boss Waves**: Every 5th wave features a boss
- **Score System**: Points for destroying enemies, high score tracking

### UI System
- **Main Menu**: Play, Options, Quit
- **Game HUD**: Health bar, score, wave number, weapon level
- **Pause Menu**: Resume, Restart, Main Menu, Quit
- **Game Over Screen**: Final score, high score, wave reached

### Visual Effects
- **Parallax Background**: Scrolling star field
- **Explosions**: Visual feedback for enemy destruction
- **Damage Flash**: Visual feedback when hit

## 💻 Requirements

- **Unity Version**: 2021.3 LTS or newer (recommended: 2022.3 LTS)
- **Platform**: Windows 10/11 (64-bit)
- **Build Target**: Windows Standalone

## 🎮 Project Setup

### Step 1: Open Project in Unity

1. Open Unity Hub
2. Click "Open" or "Add project from disk"
3. Navigate to the `space_shooter_game` folder
4. Select the folder and click "Open"
5. Wait for Unity to import and compile the project

### Step 2: Run the Setup Wizard

Once Unity opens the project:

1. Go to menu: **Space Shooter > Setup Game**
2. A setup window will appear
3. Click buttons **in order**:
   - **"1. Create Sprite Assets"** - Generates all game sprites
   - **"2. Create All Prefabs"** - Creates player, enemies, bullets, power-ups
   - **"3. Setup Scenes"** - Creates MainMenu and GameScene

### Step 3: Configure Prefab References

After running the setup wizard, you need to link prefabs:

#### Configure WaveManager:
1. Open `GameScene` from Assets/Scenes
2. Select `WaveManager` in the Hierarchy
3. In the Inspector, assign prefabs:
   - Basic Enemy Prefab: `Assets/Prefabs/EnemyBasic.prefab`
   - Fast Enemy Prefab: `Assets/Prefabs/EnemyFast.prefab`
   - Tank Enemy Prefab: `Assets/Prefabs/EnemyTank.prefab`
   - Shooter Enemy Prefab: `Assets/Prefabs/EnemyShooter.prefab`
   - Boss Prefab: `Assets/Prefabs/Boss.prefab`

#### Configure GameInitializer:
1. Select `GameInitializer` in the Hierarchy
2. Assign:
   - Player Prefab: `Assets/Prefabs/Player.prefab`
   - Player Spawn Point: Drag `PlayerSpawnPoint` from Hierarchy
   - Game HUD: Drag `GameHUD` from Canvas in Hierarchy

#### Configure Player Prefab:
1. Open `Assets/Prefabs/Player.prefab`
2. Select the Player object
3. In PlayerController component, assign:
   - Fire Point: `FirePoint` (child object)
   - Bullet Prefab: `Assets/Prefabs/PlayerBullet.prefab`
   - Shield Visual: `ShieldVisual` (child object)

#### Configure Enemy Prefabs (EnemyTank, EnemyShooter, Boss):
1. Open each enemy prefab that shoots
2. Assign Bullet Prefab: `Assets/Prefabs/EnemyBullet.prefab`
3. Assign Fire Point: `FirePoint` (child object)

#### Configure EffectsManager:
1. Select `EffectsManager` in the Hierarchy
2. Assign Explosion Prefab: `Assets/Prefabs/Explosion.prefab`

### Step 4: Test the Game

1. Open `Assets/Scenes/MainMenu.unity`
2. Press Play in Unity Editor
3. Click "Play" button in the main menu

## 🎮 Game Controls

| Action | Key |
|--------|-----|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Space (hold for continuous fire) |
| Pause | Escape |

## 🔨 Building for Windows

### Build Settings

1. Go to **File > Build Settings**
2. Ensure scenes are in the list (in order):
   - MainMenu
   - GameScene
3. Select **Windows, Mac, Linux** platform
4. Click **Switch Platform** if not already selected

### Build Configuration

1. Click **Player Settings**
2. Configure:
   - **Product Name**: Space Shooter
   - **Company Name**: Your Name
   - **Resolution**: 1280x720 (default)
   - **Fullscreen Mode**: Windowed or Fullscreen

### Create Build

1. Click **Build** or **Build And Run**
2. Choose output folder (e.g., `Builds/Windows`)
3. Name the executable (e.g., `SpaceShooter.exe`)
4. Wait for build to complete

### Build Output

The build folder will contain:
```
Builds/Windows/
├── SpaceShooter.exe          # Main executable
├── SpaceShooter_Data/        # Game data folder
├── UnityCrashHandler64.exe   # Crash handler
└── UnityPlayer.dll           # Unity runtime
```

**To distribute**: Zip the entire build folder and share it.

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   └── PlayerController.cs
│   │   ├── Enemy/
│   │   │   ├── EnemyBase.cs
│   │   │   ├── EnemyFast.cs
│   │   │   ├── EnemyTank.cs
│   │   │   ├── EnemyShooter.cs
│   │   │   └── BossEnemy.cs
│   │   ├── Combat/
│   │   │   ├── Bullet.cs
│   │   │   └── CollisionHandler.cs
│   │   ├── PowerUps/
│   │   │   ├── PowerUpBase.cs
│   │   │   ├── WeaponUpgrade.cs
│   │   │   ├── HealthPack.cs
│   │   │   ├── ShieldPowerUp.cs
│   │   │   └── SpeedBoost.cs
│   │   ├── Managers/
│   │   │   ├── GameManager.cs
│   │   │   ├── WaveManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   ├── EffectsManager.cs
│   │   │   └── GameInitializer.cs
│   │   ├── UI/
│   │   │   ├── MainMenuUI.cs
│   │   │   ├── GameHUD.cs
│   │   │   ├── PauseMenuUI.cs
│   │   │   └── GameOverUI.cs
│   │   ├── Effects/
│   │   │   ├── Explosion.cs
│   │   │   ├── ParallaxBackground.cs
│   │   │   └── StarField.cs
│   │   └── Utils/
│   │       ├── ObjectPool.cs
│   │       ├── ScreenBounds.cs
│   │       └── SpriteGenerator.cs
│   ├── Editor/
│   │   └── GameSetupEditor.cs    # Editor setup wizard
│   ├── Prefabs/                   # Game prefabs (generated)
│   ├── Sprites/                   # Sprite assets (generated)
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   └── GameScene.unity
│   ├── Audio/                     # Place audio files here
│   ├── UI/                        # UI assets
│   └── Materials/                 # Materials
├── ProjectSettings/               # Unity project settings
├── Packages/                      # Package dependencies
└── README.md                      # This file
```

## 🎨 Customization

### Adding Custom Sprites

1. Replace files in `Assets/Sprites/` with your own PNG images
2. Ensure sprites have the same pivot point (center)
3. Update sprite references in prefabs if needed

### Adding Sound Effects

1. Place audio files (.wav, .mp3, .ogg) in `Assets/Audio/`
2. Open `AudioManager` in the scene
3. Add entries to the `Sound Effects` list:
   - Name: "PlayerShoot", "EnemyExplosion", "PowerUp", etc.
   - Assign the audio clip
   - Adjust volume and pitch

### Adjusting Difficulty

In `WaveManager`:
- `difficultyScaling`: How much harder each wave gets (default: 1.2)
- Modify `predefinedWaves` to customize wave compositions

In enemy scripts:
- Adjust `maxHealth`, `moveSpeed`, `fireRate`, `scoreValue`

### Adding New Enemy Types

1. Create a new script inheriting from `EnemyBase`
2. Override `Move()` for custom movement patterns
3. Override `Fire()` for custom shooting patterns
4. Create a prefab with the new script
5. Add to `WaveManager` enemy prefab list

### UI Customization

The UI scripts use Unity's UI system. To customize:
1. Open the scene in Unity
2. Select UI elements in the Canvas
3. Modify Text, colors, layouts as needed
4. Add TextMeshPro components for better text rendering

## 🐛 Troubleshooting

### Scripts not compiling
- Ensure you're using Unity 2021.3 or newer
- Check Console for specific errors
- Try: Assets > Reimport All

### Prefabs not working
- Re-run the setup wizard
- Manually check prefab references in Inspector
- Ensure all prefabs have required components

### Game not starting
- Check that scenes are in Build Settings
- Verify GameManager exists in both scenes
- Check Console for runtime errors

### No sound
- Check AudioManager has sound clips assigned
- Verify volume settings in AudioManager
- Check Player Settings audio configuration

## 📄 License

This project is provided as-is for educational and personal use.

## 🎮 Have Fun!

Enjoy playing and customizing your space shooter game!
