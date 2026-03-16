# Space Shooter - Unity Game

A classic arcade-style space shooter game built with Unity for Windows Desktop.

![Space Shooter](docs/game_preview.png)

## 🎮 Game Features

- **Player System**: Smooth movement, upgradeable weapons, shield power-up
- **Enemy System**: Multiple enemy types with different behaviors
  - Small Enemy: Fast, low health, sine wave movement
  - Medium Enemy: Standard enemy, shoots at player
  - Large Enemy: Tank type, triple shot attack
  - Tracker Enemy: Follows player position, aimed shots
  - Boss Enemy: High health, multiple attack phases
- **Wave System**: Progressive difficulty with boss waves every 5 levels
- **Power-Ups**: Weapon upgrades, shields, health, score bonuses
- **Object Pooling**: Efficient memory management for bullets and enemies
- **Parallax Background**: Infinite scrolling space background
- **Score System**: Combo multipliers, high score tracking
- **Full UI**: Main menu, pause menu, game over screen with restart

## 🛠 Requirements

- **Unity Version**: 2022.3 LTS or newer (2023.x also works)
- **Platform**: Windows Desktop
- **Dependencies**: TextMeshPro (included in Unity)

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/           # Player movement, health, shooting
│   │   ├── Enemy/            # Enemy types and behaviors
│   │   ├── Bullet/           # Bullet mechanics
│   │   ├── Systems/          # Core game systems
│   │   ├── UI/               # UI management
│   │   ├── Managers/         # Sound manager
│   │   ├── Utils/            # Utilities (parallax, power-ups, etc.)
│   │   └── Editor/           # Editor tools
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Bullets/
│   │   ├── PowerUps/
│   │   └── Effects/
│   ├── Sprites/
│   ├── Scenes/
│   ├── Audio/
│   └── Materials/
├── ProjectSettings/
└── README.md
```

## 🚀 Quick Setup Guide

### Step 1: Create New Unity Project

1. Open Unity Hub
2. Click **"New Project"**
3. Select **"2D Core"** template
4. Name your project (e.g., "SpaceShooter")
5. Choose a location
6. Click **"Create project"**

### Step 2: Import Scripts

1. Copy the entire `Assets/Scripts` folder into your Unity project's `Assets` folder
2. Unity will automatically compile the scripts
3. If there are errors, install TextMeshPro:
   - Go to **Window > Package Manager**
   - Find "TextMeshPro" and click Install
   - Import TMP Essential Resources when prompted

### Step 3: Generate Sprites

1. In Unity, go to **Tools > Space Shooter > Generate Sprites**
2. Click **"Generate All Sprites"**
3. This creates placeholder sprites for all game objects

### Step 4: Setup Game Scene

1. Go to **Tools > Space Shooter > Setup Game Scene**
2. Click **"Setup Everything (All Steps)"**
3. This creates all game managers, prefabs, and UI elements

### Step 5: Configure Object Pooler

1. Select the **ObjectPooler** GameObject in the hierarchy
2. In the Inspector, add pools:

```
Pool Settings:
├── PlayerBullet    | Prefab: PlayerBullet  | Size: 50
├── EnemyBullet     | Prefab: EnemyBullet   | Size: 100
├── SmallEnemy      | Prefab: SmallEnemy    | Size: 20
├── MediumEnemy     | Prefab: MediumEnemy   | Size: 15
├── LargeEnemy      | Prefab: LargeEnemy    | Size: 10
├── TrackerEnemy    | Prefab: TrackerEnemy  | Size: 10
├── BossEnemy       | Prefab: BossEnemy     | Size: 2
├── Explosion       | Prefab: Explosion     | Size: 30
├── PowerUp_Weapon  | Prefab: PowerUp_Weapon | Size: 5
├── PowerUp_Shield  | Prefab: PowerUp_Shield | Size: 5
├── PowerUp_Health  | Prefab: PowerUp_Health | Size: 5
└── PowerUp_Score   | Prefab: PowerUp_Score  | Size: 5
```

### Step 6: Assign Sprites to Prefabs

1. Go to `Assets/Prefabs/Player/Player` and assign the player sprite
2. Do the same for all enemy prefabs, bullet prefabs, and power-up prefabs
3. Assign sprites from `Assets/Sprites/` to each prefab's SpriteRenderer

### Step 7: Setup UI Canvas

1. Select the **UICanvas** GameObject
2. Add TextMeshPro text elements for:
   - Score display
   - High score display
   - Wave number
   - Combo counter
3. Create buttons for menus (Play, Resume, Restart, Quit)
4. Link UI elements to the UIManager component

### Step 8: Create Scenes

1. Create two scenes in `Assets/Scenes/`:
   - **MainMenu** - Contains main menu UI
   - **GameScene** - Contains the game
2. Add both scenes to Build Settings (**File > Build Settings**)

### Step 9: Configure Camera

1. Set Main Camera:
   - **Size**: 5 (orthographic)
   - **Background**: Dark blue/black color
   - **Clear Flags**: Solid Color

## 🎯 How to Play

### Controls

| Action | Key |
|--------|-----|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Space / Left Mouse |
| Pause | Escape |

### Gameplay

1. **Survive** waves of increasingly difficult enemies
2. **Shoot** enemies to earn points
3. **Collect power-ups** dropped by defeated enemies:
   - ⭐ **Yellow Star**: Weapon upgrade (up to 4 levels)
   - 🔵 **Blue Circle**: Shield (absorbs one hit)
   - 💚 **Green Cross**: Restore health
   - 🟠 **Orange Diamond**: Score bonus
4. **Build combos** by killing enemies quickly for bonus points
5. **Defeat the boss** every 5 waves for major rewards

## 🔧 Building for Windows

### Build Steps

1. Open Unity and load your project
2. Go to **File > Build Settings**
3. Select **Windows, Mac, Linux** platform
4. Set **Target Platform**: Windows
5. Set **Architecture**: x86_64 (64-bit)
6. Click **"Add Open Scenes"** to add your scenes
7. Ensure scene order is:
   - MainMenu (index 0)
   - GameScene (index 1)
8. Click **"Build"** or **"Build And Run"**
9. Choose output folder
10. Wait for build to complete

### Build Output

Your built game will include:
```
SpaceShooter_Build/
├── SpaceShooter.exe          # Main executable
├── SpaceShooter_Data/        # Game data folder
├── UnityPlayer.dll           # Unity runtime
└── MonoBleedingEdge/         # Mono runtime
```

### Player Settings (Optional)

Before building, configure in **Edit > Project Settings > Player**:
- **Company Name**: Your name/company
- **Product Name**: Space Shooter
- **Version**: 1.0
- **Default Icon**: Your game icon
- **Resolution**: 1920x1080 or windowed

## 📝 Script Overview

### Core Scripts

| Script | Description |
|--------|-------------|
| `PlayerController.cs` | Handles player input, movement, and shooting |
| `PlayerHealth.cs` | Manages player health, damage, and death |
| `Enemy.cs` | Base enemy class with movement patterns |
| `EnemyTypes.cs` | Specific enemy type implementations |
| `Bullet.cs` | Bullet movement and collision |
| `GameManager.cs` | Central game state controller |
| `WaveManager.cs` | Wave progression and difficulty |
| `ObjectPooler.cs` | Object pooling system |
| `ScoreManager.cs` | Score tracking and combos |

### UI Scripts

| Script | Description |
|--------|-------------|
| `UIManager.cs` | Central UI controller |
| `HealthDisplay.cs` | Health bar/icons display |
| `MainMenuUI.cs` | Main menu interactions |
| `PauseMenuUI.cs` | Pause menu functionality |
| `GameOverUI.cs` | Game over screen |

### Utility Scripts

| Script | Description |
|--------|-------------|
| `SoundManager.cs` | Audio playback system |
| `ParallaxBackground.cs` | Scrolling background |
| `PowerUp.cs` | Power-up behavior |
| `Explosion.cs` | Explosion effect |

## 🎵 Adding Audio

1. Import audio files to `Assets/Audio/SFX/` and `Assets/Audio/Music/`
2. Select the **SoundManager** GameObject
3. Add sound effects to the Sound Effects list:
   - PlayerShoot, PlayerHurt, PlayerDeath
   - EnemyShoot, EnemyDeath
   - PowerUp, ShieldHit
   - ButtonClick, WaveComplete, GameOver
4. Add music tracks to the Music Tracks list:
   - MenuMusic, GameMusic

## 🐛 Troubleshooting

### Common Issues

**Scripts not compiling:**
- Ensure TextMeshPro is installed via Package Manager
- Check for missing namespace: `using TMPro;`

**Player not moving:**
- Verify Input settings in Edit > Project Settings > Input Manager
- Check Rigidbody2D constraints

**Enemies not spawning:**
- Verify ObjectPooler has all pool configurations
- Check that prefabs have correct tags (Enemy, EnemyBullet, etc.)

**UI not updating:**
- Ensure UIManager has references to all UI elements
- Check that events are properly subscribed

**Bullets going through enemies:**
- Set proper collision layers in Physics2D settings
- Ensure both objects have Collider2D (trigger) and Rigidbody2D

## 📜 License

This project is provided as-is for educational purposes. Feel free to modify and use in your own projects.

## 🎮 Credits

Created as a learning project for Unity game development.

---

**Enjoy the game! 🚀**
