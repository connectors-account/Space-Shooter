# Space Shooter Game - Unity Project

A complete 2D space shooter game built with Unity and C#. Features wave-based enemy spawning, power-ups, parallax scrolling backgrounds, and full menu systems.

## 📋 Requirements

- **Unity Version**: Unity 2021.3 LTS or newer (2022.3 LTS recommended)
- **Platform**: Windows 10/11 (64-bit)
- **Build Target**: Windows Standalone

## 🎮 Game Features

### Gameplay
- Player ship with smooth movement and shooting
- Three enemy types with unique behaviors:
  - **Basic Enemy**: Moves straight down
  - **Zigzag Enemy**: Weaves left and right while descending
  - **Shooter Enemy**: Fires projectiles at the player
- Wave-based progression with increasing difficulty
- Score system with high score persistence

### Power-Ups
- **Shield** (Blue): Temporary invincibility
- **Rapid Fire** (Yellow): Triple shot with faster fire rate
- **Health** (Green): Restores 30 HP

### Controls
- **Arrow Keys** or **WASD**: Move the player ship
- **Spacebar**: Fire weapons
- **ESC**: Pause/Resume game

### UI Features
- Main menu with Play, Settings, and Quit options
- In-game HUD showing score, wave number, and health
- Pause menu
- Game over screen with score and high score display

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── PlayerController.cs      # Player movement, shooting, power-ups
│   │   ├── EnemyController.cs       # Enemy AI and behavior
│   │   ├── BulletController.cs      # Bullet movement and collision
│   │   ├── PowerUpController.cs     # Power-up effects
│   │   ├── EnemySpawner.cs          # Basic enemy spawning
│   │   ├── GameManager.cs           # Game state management
│   │   ├── WaveManager.cs           # Wave configuration
│   │   ├── UIManager.cs             # HUD and UI updates
│   │   ├── MenuManager.cs           # Menu screens
│   │   ├── AudioManager.cs          # Sound effects
│   │   ├── ParallaxBackground.cs    # Scrolling background
│   │   ├── CollisionHandler.cs      # Collision utilities
│   │   ├── HealthSystem.cs          # Health management
│   │   ├── SpriteGenerator.cs       # Procedural sprites
│   │   └── GameInitializer.cs       # Auto-setup system
│   ├── Prefabs/
│   │   └── PrefabConfigurations.json
│   ├── Sprites/
│   ├── Audio/
│   ├── Scenes/
│   ├── Materials/
│   └── Resources/
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   └── Physics2DSettings.asset
├── Packages/
└── README.md
```

## 🚀 Setup Instructions

### Step 1: Install Unity

1. Download Unity Hub from [unity.com/download](https://unity.com/download)
2. Install Unity Hub and sign in with a Unity account
3. In Unity Hub, go to **Installs** → **Install Editor**
4. Select **Unity 2022.3 LTS** (or 2021.3 LTS)
5. Make sure to include:
   - **Windows Build Support (IL2CPP)** - Required for Windows builds
   - **Windows Build Support (Mono)** - Alternative, smaller builds

### Step 2: Import the Project

#### Option A: Open Existing Project
1. Open Unity Hub
2. Click **Open** → **Add project from disk**
3. Navigate to the `space_shooter_game` folder
4. Click **Open** or **Add Project**
5. Unity will import all assets (this may take a few minutes)

#### Option B: Create New Project and Copy Files
1. Open Unity Hub
2. Click **New Project**
3. Select **2D** template
4. Name it "SpaceShooter" and choose a location
5. Click **Create Project**
6. Once Unity opens, close it
7. Copy all contents from `space_shooter_game/Assets/` to your new project's `Assets/` folder
8. Copy `ProjectSettings/` files to your project's `ProjectSettings/` folder
9. Reopen the project in Unity

### Step 3: Configure the Scene

After opening the project in Unity:

#### Create the Main Scene
1. Go to **File** → **New Scene**
2. Save it as `Assets/Scenes/MainScene.unity`

#### Set Up Game Objects
1. Create an empty GameObject named "GameInitializer"
2. Add the `GameInitializer` component to it
3. In the Inspector, ensure:
   - **Initialize On Start**: ✓ (checked)
   - **Create UI**: ✓ (checked)
   - **Create Prefabs**: ✓ (checked)

4. The GameInitializer will automatically create:
   - All manager singletons (GameManager, WaveManager, AudioManager, etc.)
   - All prefabs with procedurally generated sprites
   - UI canvas with menus
   - Parallax background
   - Proper camera settings

#### Alternative: Manual Setup
If you prefer manual setup or need to customize:

1. **Create Managers:**
   - Create empty GameObjects for: GameManager, WaveManager, AudioManager, CollisionHandler, PrefabManager
   - Attach respective scripts to each

2. **Create Player Prefab:**
   - Create new GameObject "Player"
   - Add components: SpriteRenderer, PlayerController, HealthSystem, BoxCollider2D (trigger), Rigidbody2D (gravity=0)
   - Create child "FirePoint" at position (0, 0.6, 0)
   - Create sprite using `SpriteGenerator.CreatePlayerShip()`
   - Save as prefab in Assets/Prefabs/

3. **Create Enemy Prefabs:**
   - Similar process for BasicEnemy, ZigzagEnemy, ShooterEnemy
   - Each needs: SpriteRenderer, EnemyController, HealthSystem, BoxCollider2D, Rigidbody2D

4. **Create Bullet Prefabs:**
   - PlayerBullet and EnemyBullet
   - Components: SpriteRenderer, BulletController, CircleCollider2D, Rigidbody2D

5. **Create Power-Up Prefabs:**
   - ShieldPowerUp, RapidFirePowerUp, HealthPowerUp
   - Components: SpriteRenderer, PowerUpController, CircleCollider2D

6. **Create UI:**
   - Add Canvas with UIManager and MenuManager components
   - Use the CreateMenuUI() and CreateUIElements() methods or set up manually

7. **Create Background:**
   - Create empty GameObject with ParallaxBackground component

### Step 4: Configure Layers and Tags

The project includes pre-configured TagManager.asset, but verify in Unity:

1. Go to **Edit** → **Project Settings** → **Tags and Layers**
2. Ensure these **Tags** exist:
   - Player
   - Enemy
   - PlayerBullet
   - EnemyBullet
   - PowerUp

3. Ensure these **Layers** exist (User Layers 8-12):
   - Layer 8: Player
   - Layer 9: Enemy
   - Layer 10: PlayerBullet
   - Layer 11: EnemyBullet
   - Layer 12: PowerUp

### Step 5: Configure Physics Collision Matrix

1. Go to **Edit** → **Project Settings** → **Physics 2D**
2. In the **Layer Collision Matrix**, configure:

| Layer | Player | Enemy | PlayerBullet | EnemyBullet | PowerUp |
|-------|--------|-------|--------------|-------------|---------|
| Player | ❌ | ✅ | ❌ | ✅ | ✅ |
| Enemy | ✅ | ❌ | ✅ | ❌ | ❌ |
| PlayerBullet | ❌ | ✅ | ❌ | ❌ | ❌ |
| EnemyBullet | ✅ | ❌ | ❌ | ❌ | ❌ |
| PowerUp | ✅ | ❌ | ❌ | ❌ | ❌ |

### Step 6: Add Scene to Build Settings

1. Open your main scene (MainScene.unity)
2. Go to **File** → **Build Settings**
3. Click **Add Open Scenes**
4. Ensure the scene is at index 0

## 🔨 Building for Windows

### Configure Build Settings

1. Go to **File** → **Build Settings**
2. Select **Windows, Mac, Linux** under Platform
3. Click **Switch Platform** if not already selected
4. Configure:
   - **Target Platform**: Windows
   - **Architecture**: x86_64 (recommended) or x86

### Player Settings

1. Click **Player Settings** button in Build Settings
2. Configure under **Player** → **Windows, Mac, Linux**:

   **Resolution and Presentation:**
   - Fullscreen Mode: Fullscreen Window
   - Default Screen Width: 1920
   - Default Screen Height: 1080
   - Run In Background: ✓

   **Other Settings:**
   - Scripting Backend: Mono (faster builds) or IL2CPP (better performance)
   - API Compatibility Level: .NET Standard 2.1

   **Icon:**
   - Add your game icon (optional)

### Build the Game

1. In Build Settings, click **Build**
2. Choose a destination folder (e.g., "Builds/Windows")
3. Name the executable "SpaceShooter.exe"
4. Click **Save**
5. Wait for the build to complete

### Build Output

Your build folder will contain:
```
Builds/Windows/
├── SpaceShooter.exe           # Main executable
├── SpaceShooter_Data/         # Game data folder
├── MonoBleedingEdge/          # Mono runtime (if using Mono)
└── UnityPlayer.dll            # Unity player library
```

## 🎵 Adding Audio (Optional)

The AudioManager supports both assigned audio clips and procedural sounds. To add custom audio:

1. Import audio files into `Assets/Audio/`
2. Select the AudioManager GameObject
3. Add entries to the Sound Effects list:
   - PlayerShoot
   - EnemyShoot
   - PlayerHit
   - Explosion
   - PowerUp
   - Heal
   - WaveStart
   - GameOver
   - ButtonClick
   - Pause

4. Assign AudioClips to each entry

Without assigned clips, the game generates simple procedural sounds.

## 🎨 Custom Sprites (Optional)

The game generates sprites procedurally using `SpriteGenerator.cs`. To use custom sprites:

1. Create or import sprite images to `Assets/Sprites/`
2. Configure sprite settings:
   - Sprite Mode: Single
   - Pixels Per Unit: 64 (adjust as needed)
   - Filter Mode: Point (for pixel art) or Bilinear

3. Assign sprites to prefabs through the Inspector:
   - Select prefab → SpriteRenderer → Sprite field

## 🐛 Troubleshooting

### Scripts Not Compiling
- Ensure all .cs files are in `Assets/Scripts/`
- Check Console (Window → General → Console) for errors
- Verify Unity version compatibility

### Missing References
- If prefab references are missing, reassign them in the Inspector
- GameInitializer auto-creates references at runtime

### Collisions Not Working
- Verify layer assignments on GameObjects
- Check collision matrix in Physics 2D settings
- Ensure colliders have "Is Trigger" enabled

### UI Not Showing
- Verify Canvas exists with correct render mode
- Check EventSystem exists in scene
- Ensure MenuManager/UIManager components are attached

### Build Errors
- Clear Library folder and reimport (delete Library/, reopen project)
- Check for missing assets or broken references
- Verify all scenes are in Build Settings

## 📜 License

This project is provided as-is for educational purposes. Feel free to modify and use it for your own projects.

## 🎮 Game Tips

1. Collect power-ups to survive longer waves
2. Shield power-up blocks all damage temporarily
3. Rapid fire is most effective against tough shooter enemies
4. Keep moving to avoid enemy bullets
5. Enemies drop power-ups with 15% chance on death

---

**Enjoy the game!** 🚀
