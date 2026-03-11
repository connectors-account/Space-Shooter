# Space Shooter Game

A complete 2D space shooter game built with Unity for Windows desktop. This project includes all scripts, configurations, and instructions needed to build a fully functional game.

![Space Shooter Banner](https://img.shields.io/badge/Unity-2021.3%2B-blue) ![Platform](https://img.shields.io/badge/Platform-Windows-green) ![License](https://img.shields.io/badge/License-MIT-yellow)

## 🎮 Game Features

- **Player Ship**: Move in all directions and shoot enemies
- **Enemy Waves**: Progressive difficulty with multiple enemy types
- **Multiple Movement Patterns**: Straight, zigzag, sine wave, and homing enemies
- **Shooting System**: Both player and enemies can shoot
- **Health System**: Player health with visual feedback
- **Score System**: Track current score and high scores (saved locally)
- **Wave System**: Enemies spawn in increasingly difficult waves
- **UI System**: Main menu, HUD, pause menu, and game over screen
- **Power-ups**: Health, speed boost, rapid fire, spread shot (optional)

## 🖥️ System Requirements

- **Unity Version**: 2021.3 LTS or newer (recommended: 2022.3 LTS)
- **Operating System**: Windows 10/11 for building
- **Build Target**: Windows x64

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerController.cs    # Player movement
│   │   │   ├── PlayerHealth.cs        # Player health management
│   │   │   └── PlayerShooting.cs      # Player shooting mechanics
│   │   ├── Enemies/
│   │   │   ├── EnemyController.cs     # Enemy movement patterns
│   │   │   ├── EnemyHealth.cs         # Enemy health & scoring
│   │   │   ├── EnemySpawner.cs        # Wave-based enemy spawning
│   │   │   └── EnemyShooting.cs       # Enemy shooting mechanics
│   │   ├── Projectiles/
│   │   │   └── Bullet.cs              # Bullet behavior
│   │   ├── Managers/
│   │   │   ├── GameManager.cs         # Game state management
│   │   │   └── ScoreManager.cs        # Score tracking
│   │   ├── UI/
│   │   │   ├── UIManager.cs           # UI management
│   │   │   └── MainMenu.cs            # Main menu functionality
│   │   ├── Powerups/
│   │   │   └── Powerup.cs             # Power-up system
│   │   └── GameInitializer.cs         # Runtime game setup
│   ├── Editor/
│   │   └── GameSetupEditor.cs         # Editor tools for scene setup
│   ├── Prefabs/                       # Game object prefabs
│   ├── Scenes/                        # Game scenes
│   └── Materials/                     # Materials and shaders
├── ProjectSettings/
│   ├── TagManager.asset              # Custom tags definition
│   ├── ProjectSettings.asset         # Project configuration
│   └── InputManager.asset            # Input configuration
├── Packages/
│   └── manifest.json                 # Package dependencies
└── README.md                         # This file
```

## 🚀 Quick Start Guide

### Step 1: Install Unity

1. Download [Unity Hub](https://unity.com/download)
2. Install Unity **2021.3 LTS** or **2022.3 LTS** (recommended)
3. Make sure to include **Windows Build Support** during installation

### Step 2: Open the Project

1. Open Unity Hub
2. Click **"Add"** → **"Add project from disk"**
3. Navigate to and select the `space_shooter_game` folder
4. Unity will detect the project and add it to your list
5. Click on the project to open it

### Step 3: Set Up the Scene

#### Option A: Automatic Setup (Recommended)

1. In Unity, go to the menu: **Tools** → **Space Shooter** → **Setup Game Scene**
2. Click "Yes" when prompted
3. The scene will be automatically configured with all necessary objects

#### Option B: Manual Setup

1. Create a new scene: **File** → **New Scene**
2. Save it as `MainScene` in `Assets/Scenes/`
3. Create an empty GameObject named "GameInitializer"
4. Add the `GameInitializer` component to it
5. Press Play - the game will auto-initialize all systems

### Step 4: Play Test

1. Press the **Play** button in Unity
2. Use the main menu to start the game
3. Test the controls:
   - **WASD** or **Arrow Keys**: Move
   - **Space** or **Left Click**: Shoot
   - **ESC** or **P**: Pause

## 🔧 Detailed Setup Instructions

### Creating Tags (Important!)

The game requires these tags to be defined. They should be automatically loaded from `ProjectSettings/TagManager.asset`, but if you encounter "tag not found" errors:

1. Go to **Edit** → **Project Settings** → **Tags and Layers**
2. Add these tags:
   - `Player`
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`
   - `Powerup`

### Creating Prefabs (Optional)

To create prefabs for customization:

1. **Tools** → **Space Shooter** → **Create Player Prefab**
2. **Tools** → **Space Shooter** → **Create Enemy Prefab**
3. **Tools** → **Space Shooter** → **Create Bullet Prefab**

Prefabs will be saved in `Assets/Prefabs/`

### Configuring Game Settings

#### Player Settings
Select the Player object and adjust in the Inspector:
- `Move Speed`: Default 10
- `Horizontal Boundary`: Default 8
- `Vertical Boundary`: Default 4.5
- `Max Health`: Default 100
- `Fire Rate`: Default 0.2 seconds

#### Enemy Spawner Settings
Select EnemySpawner and adjust:
- `Spawn Interval`: Time between spawns
- `Enemies Per Wave`: Starting enemies
- `Additional Enemies Per Wave`: Difficulty scaling

## 🏗️ Building for Windows

### Build Steps

1. **Open Build Settings**: **File** → **Build Settings** (or `Ctrl+Shift+B`)

2. **Select Platform**: 
   - Choose **Windows, Mac, Linux** (Standalone)
   - Click **Switch Platform** if not already selected

3. **Configure Settings**:
   - Target Platform: **Windows**
   - Architecture: **x86_64** (recommended) or **x86**

4. **Add Scene**:
   - Click **Add Open Scenes** to add your main scene
   - Or drag scenes from Project window to the build list

5. **Player Settings** (click "Player Settings..." button):
   - Company Name: Your company name
   - Product Name: "Space Shooter"
   - Default Icon: (optional) drag an icon image
   - Resolution:
     - Default Screen Width: 1920
     - Default Screen Height: 1080
     - Fullscreen Mode: Windowed or Fullscreen

6. **Build**:
   - Click **Build** or **Build and Run**
   - Choose a folder for the build (e.g., `Builds/Windows/`)
   - Wait for the build to complete

### Build Output

After building, you'll have:
```
Builds/Windows/
├── Space Shooter.exe           # Main executable
├── Space Shooter_Data/         # Game data folder
├── UnityPlayer.dll            # Unity runtime
└── MonoBleedingEdge/          # Mono runtime
```

### Distribution

To distribute your game:
1. Zip the entire build folder
2. Users just need to extract and run the `.exe` file
3. No installation required!

## 🎮 Game Controls

| Action | Primary | Alternative |
|--------|---------|-------------|
| Move Up | W | Up Arrow |
| Move Down | S | Down Arrow |
| Move Left | A | Left Arrow |
| Move Right | D | Right Arrow |
| Shoot | Space | Left Mouse Button |
| Pause | Escape | P |

## 🎯 Gameplay Tips

1. **Stay Mobile**: Keep moving to avoid enemy fire
2. **Priority Targets**: Focus on shooting enemies before they shoot you
3. **Watch Health**: The health bar changes color as health decreases
4. **High Scores**: Your best score is saved automatically
5. **Wave Breaks**: Use the time between waves to position yourself

## 📝 Customization Guide

### Adding New Enemy Types

1. Open `EnemySpawner.cs`
2. Add new entry to the `enemyTypes` list
3. Configure:
   - Name
   - Spawn chance
   - Minimum wave to appear
   - Color

### Modifying Difficulty

In `EnemySpawner.cs`:
```csharp
[SerializeField] private float spawnInterval = 2f;        // Lower = harder
[SerializeField] private float minSpawnInterval = 0.5f;   // Minimum interval
[SerializeField] private int enemiesPerWave = 5;          // Starting enemies
[SerializeField] private int additionalEnemiesPerWave = 2; // Wave scaling
```

### Adding New Power-ups

1. Open `Powerup.cs`
2. Add new type to `PowerupType` enum
3. Implement effect in `ApplyEffect()` method
4. Add color in `SetColorByType()` method

## 🐛 Troubleshooting

### Common Issues

**"Tag not found" error**
- Ensure tags are defined in Project Settings → Tags and Layers
- Or re-import ProjectSettings folder

**UI not showing**
- Make sure EventSystem exists in the scene
- Check Canvas render mode is Screen Space - Overlay

**Player/Enemies not colliding**
- Verify all objects have 2D colliders (BoxCollider2D, etc.)
- Check colliders are set to "Is Trigger"
- Ensure Rigidbody2D components exist

**Game not pausing**
- Check GameManager exists and is properly initialized
- Verify Time.timeScale is being set correctly

**Build errors**
- Ensure all scenes are added to Build Settings
- Check for any compile errors in Console
- Verify .NET API compatibility level matches

### Performance Tips

- Keep enemy count reasonable (adjust spawner settings)
- Bullets are automatically destroyed when off-screen
- Use Object Pooling for better performance (advanced)

## 📚 Script Reference

### Core Classes

| Class | Purpose |
|-------|---------|
| `GameManager` | Controls game state (menu, playing, paused, game over) |
| `ScoreManager` | Tracks score and persists high scores |
| `UIManager` | Manages all UI elements and updates |
| `EnemySpawner` | Handles wave-based enemy spawning |

### Player Classes

| Class | Purpose |
|-------|---------|
| `PlayerController` | Handles player input and movement |
| `PlayerHealth` | Manages player HP and death |
| `PlayerShooting` | Controls player weapon firing |

### Enemy Classes

| Class | Purpose |
|-------|---------|
| `EnemyController` | Controls enemy movement patterns |
| `EnemyHealth` | Manages enemy HP and score rewards |
| `EnemyShooting` | Controls enemy weapon firing |

## 📄 License

This project is provided as-is for educational and personal use. Feel free to modify and distribute.

## 🤝 Contributing

Feel free to extend and improve this project:
1. Add visual effects and particles
2. Implement sound effects and music
3. Add more enemy types and bosses
4. Create power-up system
5. Add mobile touch controls

---

**Happy Gaming! 🚀**

*Built with Unity - The world's leading real-time development platform*
