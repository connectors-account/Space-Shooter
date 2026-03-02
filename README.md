# Space Shooter Game

A complete 2D space shooter game built with Unity and C#. Features wave-based enemy spawning, multiple enemy types, power-ups, and a full UI system.

## Features

- **Player Controls**: Smooth movement with WASD/Arrow keys, shooting with Spacebar
- **Wave-Based Gameplay**: Progressively harder waves of enemies
- **3 Enemy Types**:
  - Basic Enemy: Moves straight down
  - ZigZag Enemy: Weaves side to side while descending
  - Dive Bomber: Hovers, tracks player, then dives
- **Power-Up System**: Health pickups that restore player health
- **Score System**: Points for destroying enemies, persistent high score
- **Full UI**: Main menu, pause menu, game over screen, HUD
- **Visual Effects**: Scrolling starfield background, damage flash effects

## Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerController.cs    # Player movement and shooting
│   │   │   └── PlayerHealth.cs        # Player health management
│   │   ├── Enemy/
│   │   │   ├── EnemyBase.cs           # Base enemy class
│   │   │   ├── BasicEnemy.cs          # Simple enemy type
│   │   │   ├── ZigZagEnemy.cs         # Zigzag movement enemy
│   │   │   ├── DiveBomberEnemy.cs     # Diving attack enemy
│   │   │   └── EnemySpawner.cs        # Wave-based spawning
│   │   ├── Weapons/
│   │   │   └── Bullet.cs              # Bullet behavior
│   │   ├── Systems/
│   │   │   ├── GameManager.cs         # Game state management
│   │   │   ├── ScoreManager.cs        # Score tracking
│   │   │   ├── PowerUp.cs             # Power-up behavior
│   │   │   └── PowerUpSpawner.cs      # Power-up spawning
│   │   ├── UI/
│   │   │   ├── UIManager.cs           # UI state management
│   │   │   └── HealthDisplay.cs       # Health bar logic
│   │   ├── Utilities/
│   │   │   ├── ParallaxBackground.cs  # Parallax scrolling
│   │   │   ├── ScrollingBackground.cs # Simple background scroll
│   │   │   ├── DestroyAfterTime.cs    # Auto-destroy utility
│   │   │   ├── ScreenShake.cs         # Camera shake effect
│   │   │   ├── GameSetup.cs           # Runtime game object creation
│   │   │   └── UISetup.cs             # Runtime UI creation
│   │   └── SceneInitializer.cs        # Main scene setup
│   ├── Prefabs/                       # Store prefabs here
│   ├── Scenes/                        # Store scenes here
│   ├── Materials/                     # Store materials here
│   ├── Sprites/                       # Store sprite assets here
│   └── Audio/                         # Store audio files here
└── ProjectSettings/                   # Unity project settings
```

## How to Open the Project in Unity

### Prerequisites
- Unity Hub installed
- Unity Editor version 2021.3 LTS or newer (recommended)

### Steps

1. **Open Unity Hub**

2. **Add Project**:
   - Click "Add" button in Unity Hub
   - Navigate to the `space_shooter_game` folder
   - Select the folder and click "Open"

3. **Open Project**:
   - Unity Hub will detect it as a Unity project
   - Click on the project to open it in Unity Editor
   - If prompted about Unity version, select your installed version (2021.3+ recommended)

4. **Create Initial Scene**:
   - Go to `File > New Scene`
   - Save it as `Assets/Scenes/MainScene.unity`
   - Create an empty GameObject (Right-click in Hierarchy > Create Empty)
   - Name it "SceneInitializer"
   - Add the `SceneInitializer` component to it
   - Save the scene (Ctrl+S)

5. **Play the Game**:
   - Press the Play button (▶) in Unity Editor
   - The game will automatically set up all objects at runtime

## How to Build for Windows

### Step-by-Step Build Instructions

1. **Open Build Settings**:
   - Go to `File > Build Settings` (or press `Ctrl+Shift+B`)

2. **Add Scene to Build**:
   - Click "Add Open Scenes" to add your MainScene
   - Or drag `Assets/Scenes/MainScene.unity` into the "Scenes In Build" list

3. **Select Platform**:
   - Select "Windows, Mac, Linux" in the Platform list
   - Click "Switch Platform" if not already selected

4. **Configure Player Settings** (Optional):
   - Click "Player Settings..." button
   - Set Company Name and Product Name
   - Under "Resolution and Presentation":
     - Set default resolution (e.g., 1920x1080)
     - Check "Run In Background" if desired
   - Under "Other Settings":
     - Set API Compatibility Level to ".NET Standard 2.1"

5. **Build the Game**:
   - Click "Build" button
   - Choose a destination folder (e.g., `Builds/Windows`)
   - Click "Select Folder"
   - Wait for the build to complete

6. **Run the Game**:
   - Navigate to your build folder
   - Run the `.exe` file

### Build Settings Recommendations

| Setting | Recommended Value |
|---------|------------------|
| Architecture | x86_64 |
| Compression Method | LZ4HC |
| Build App Bundle | No (for testing) |
| Development Build | No (for release) |

## Controls

| Action | Key(s) |
|--------|--------|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Spacebar (hold for continuous fire) |
| Pause | Escape |
| Restart (when game over) | R |

## Gameplay Tips

1. **Don't stop moving** - Standing still makes you an easy target
2. **Watch for dive bombers** - They hover before attacking
3. **Collect health pickups** - Green power-ups restore 25 health
4. **Prioritize dangerous enemies** - Take out shooters first
5. **Use invincibility frames** - After taking damage, you're briefly invincible

## Adding Custom Assets

### Replacing Placeholder Sprites

The game uses colored rectangles as placeholder sprites. To add custom artwork:

1. **Import Your Sprites**:
   - Drag sprite files into `Assets/Sprites/` folder
   - Set texture type to "Sprite (2D and UI)"
   - Set Pixels Per Unit to match your sprite size (32 or 64 recommended)

2. **Create Prefabs Manually**:
   - Instead of using `GameSetup.cs` auto-generation:
   - Create GameObjects with your sprites
   - Add the appropriate scripts (PlayerController, BasicEnemy, etc.)
   - Save as prefabs in `Assets/Prefabs/`

3. **Assign Prefabs**:
   - Select the EnemySpawner object
   - Drag your enemy prefabs to the prefab slots
   - Select the Player object
   - Drag your bullet prefab to the bulletPrefab slot

### Adding Audio

1. **Import Audio Files**:
   - Drag audio files (.wav, .mp3, .ogg) into `Assets/Audio/`

2. **Assign to Components**:
   - PlayerController: `shootSound`
   - PlayerHealth: `hurtSound`, `deathSound`
   - EnemyBase: `shootSound`, `deathSound`
   - GameManager: `backgroundMusic`, `gameOverSound`
   - PowerUp: `pickupSound`

### Sprite Recommendations

| Object | Recommended Size | Notes |
|--------|-----------------|-------|
| Player | 32x48 pixels | Triangle/ship shape |
| Basic Enemy | 32x32 pixels | Square/simple shape |
| ZigZag Enemy | 40x32 pixels | Wide shape |
| Dive Bomber | 48x32 pixels | Larger, aggressive look |
| Bullet | 8x16 pixels | Small, elongated |
| Power-Up | 24x24 pixels | Distinctive, easy to see |

## Customization Options

### Adjusting Difficulty

Edit values in the Inspector or modify script defaults:

**EnemySpawner.cs**:
- `baseEnemiesPerWave`: Starting enemies per wave
- `enemiesPerWaveIncrease`: Additional enemies each wave
- `spawnInterval`: Time between enemy spawns

**Enemy Scripts**:
- `health`: Enemy durability
- `moveSpeed`: How fast enemies move
- `fireRate`: Time between enemy shots

**PlayerController.cs**:
- `moveSpeed`: Player movement speed
- `fireRate`: Time between player shots

**PlayerHealth.cs**:
- `maxHealth`: Starting health
- `invincibilityDuration`: Damage immunity time

## Troubleshooting

### Common Issues

**"Script not found" errors**:
- Make sure all `.cs` files are in the correct folders
- Check that file names match class names exactly

**Objects not spawning**:
- Ensure tags are set correctly ("Player", "Enemy", "PlayerBullet", "EnemyBullet")
- Check that colliders are set as triggers

**UI not appearing**:
- Verify Canvas is present in scene
- Check that UIManager references are assigned

**Bullets passing through enemies**:
- Ensure both objects have Rigidbody2D components
- Verify colliders are set as triggers
- Check that tags match what scripts expect

## License

This project is provided as-is for educational purposes. Feel free to modify and use for your own projects.

## Credits

Created as a complete Unity space shooter template.
