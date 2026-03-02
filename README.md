# Space Shooter Game - Unity Project

A complete 2D space shooter game for Windows desktop built with Unity.

## 🎮 Game Features

- **Player Controls**: WASD/Arrow keys for movement, Space bar to shoot
- **Wave-based Gameplay**: Progressive enemy waves with increasing difficulty
- **Multiple Enemy Types**: Basic, Fast, Tank, and Boss enemies
- **Bullet Patterns**: Single, spread, and burst shooting patterns
- **Power-ups**: Weapon upgrade, health recovery, and shield
- **Health & Scoring System**: Track your health and compete for high scores
- **Parallax Scrolling Background**: Immersive space environment
- **Complete UI**: Main menu, pause menu, game over screen, HUD
- **Sound Effect Integration Points**: Ready for audio implementation

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/           # All C# game scripts
│   │   ├── PlayerController.cs
│   │   ├── Enemy.cs
│   │   ├── EnemySpawner.cs
│   │   ├── Bullet.cs
│   │   ├── PowerUp.cs
│   │   ├── HealthSystem.cs
│   │   ├── GameManager.cs
│   │   ├── ScoreManager.cs
│   │   ├── WaveManager.cs
│   │   ├── UIManager.cs
│   │   ├── MenuManager.cs
│   │   ├── ParallaxBackground.cs
│   │   ├── AudioManager.cs
│   │   └── CameraShake.cs
│   ├── Scenes/            # Unity scene files (created in Unity)
│   │   └── SceneSetupGuide.md
│   ├── Prefabs/           # Prefab configurations
│   │   └── PrefabSetupGuide.md
│   ├── Sprites/           # Placeholder sprite assets
│   │   ├── Player.png
│   │   ├── PlayerBullet.png
│   │   ├── EnemyBullet.png
│   │   ├── EnemyBasic.png
│   │   ├── EnemyFast.png
│   │   ├── EnemyTank.png
│   │   ├── Boss.png
│   │   ├── PowerUpWeapon.png
│   │   ├── PowerUpHealth.png
│   │   ├── PowerUpShield.png
│   │   └── Background.png
│   ├── Audio/             # Audio files (add your own)
│   ├── Materials/         # Materials and shaders
│   ├── Animations/        # Animation assets
│   └── Editor/            # Editor scripts
│       └── CreatePlaceholderSprites.cs
├── ProjectSettings/       # Unity project settings
│   ├── ProjectSettings.asset
│   ├── EditorBuildSettings.asset
│   ├── TagManager.asset
│   ├── InputManager.asset
│   ├── Physics2DSettings.asset
│   ├── QualitySettings.asset
│   ├── TimeManager.asset
│   └── AudioManager.asset
├── Packages/
│   └── manifest.json
└── README.md
```

## 🚀 Quick Start Guide

### Prerequisites
- **Unity 2022.3 LTS** or newer (recommended)
- **Windows 10/11** for building Windows executables
- Basic familiarity with Unity Editor

### Step 1: Open Project in Unity

1. Download and install [Unity Hub](https://unity.com/download)
2. Install Unity 2022.3 LTS (or newer) with Windows Build Support
3. In Unity Hub, click "Open" → Browse to `space_shooter_game` folder
4. Select the folder and click "Open"
5. Unity will import the project (this may take a few minutes)

### Step 2: Setup Scenes

The project requires two scenes to be created. Follow these detailed steps:

#### Create MainMenu Scene

1. **File → New Scene** (choose "Basic 2D")
2. **File → Save As** → Save to `Assets/Scenes/MainMenu.unity`

3. **Setup Camera**:
   - Select "Main Camera" in Hierarchy
   - Set Background color to dark blue (e.g., #0A0A20)

4. **Create UI Canvas**:
   - Right-click Hierarchy → **UI → Canvas**
   - Select Canvas, in Inspector set:
     - Canvas Scaler → UI Scale Mode: "Scale With Screen Size"
     - Reference Resolution: 1920 x 1080

5. **Create Main Menu Panel**:
   - Right-click Canvas → **UI → Panel**, name it "MainMenuPanel"
   - Right-click MainMenuPanel → **UI → Text - TextMeshPro**
     - Name: "TitleText", Text: "SPACE SHOOTER"
     - Font Size: 72, Align Center, Position Y: 200
   - Create 4 buttons (UI → Button - TextMeshPro):
     - "PlayButton" (text: "PLAY")
     - "OptionsButton" (text: "OPTIONS")
     - "CreditsButton" (text: "CREDITS")
     - "QuitButton" (text: "QUIT")
   - Add Text for HighScore display

6. **Create Options & Credits Panels** (similar structure, disabled by default)

7. **Add MenuManager**:
   - Create empty GameObject, name "MenuManager"
   - Add "MenuManager" script component
   - Drag panels and buttons to script fields
   - Connect button OnClick() events to MenuManager functions

8. **Add EventSystem** if not present (UI → Event System)

#### Create GameScene

1. **File → New Scene** (choose "Basic 2D")
2. **File → Save As** → Save to `Assets/Scenes/GameScene.unity`

3. **Setup Camera**:
   - Position: (0, 0, -10)
   - Orthographic Size: 5
   - Background: Dark space color
   - Add "CameraShake" script

4. **Create Managers**:
   - Create empty "Managers" GameObject
   - Create children and add scripts:
     - "GameManager" → GameManager.cs
     - "ScoreManager" → ScoreManager.cs
     - "WaveManager" → WaveManager.cs
     - "EnemySpawner" → EnemySpawner.cs

5. **Create Player Spawn Point**:
   - Create empty "PlayerSpawnPoint" at position (0, -3, 0)

6. **Setup UI Canvas** (similar to MainMenu):
   - Create HUD Panel with:
     - Health Slider
     - Score Text (TextMeshPro)
     - Wave Counter Text
   - Create PausePanel (disabled by default)
   - Create GameOverPanel (disabled by default)
   - Create WaveCompletePanel (disabled by default)
   - Add "UIManager" script and connect all references

### Step 3: Create Prefabs

Follow `Assets/Prefabs/PrefabSetupGuide.md` for detailed instructions. Quick summary:

#### Player Prefab
1. Create empty GameObject named "Player"
2. Add: SpriteRenderer (Player.png), Rigidbody2D (Kinematic), BoxCollider2D (Is Trigger), PlayerController, HealthSystem, AudioSource
3. Create child "FirePoint" at (0, 0.5, 0)
4. Set Tag: "Player"
5. Drag to Prefabs folder

#### Bullet Prefabs
1. Create "PlayerBullet": SpriteRenderer, Rigidbody2D (Kinematic), BoxCollider2D (Is Trigger), Bullet script
2. Tag: "PlayerBullet", configure: isPlayerBullet = true
3. Create "EnemyBullet" similarly with red sprite, isPlayerBullet = false
4. Drag both to Prefabs folder

#### Enemy Prefabs
1. Create "EnemyBasic": SpriteRenderer (EnemyBasic.png), Rigidbody2D (Kinematic), BoxCollider2D (Is Trigger), Enemy script, AudioSource
2. Tag: "Enemy", configure enemy settings
3. Create variants: EnemyFast, EnemyTank, Boss
4. Drag all to Prefabs folder

#### Power-up Prefabs
1. Create "PowerUpWeapon", "PowerUpHealth", "PowerUpShield"
2. Add: SpriteRenderer, Rigidbody2D (Kinematic), CircleCollider2D (Is Trigger), PowerUp script
3. Tag: "PowerUp"
4. Drag to Prefabs folder

### Step 4: Connect Everything

1. **GameManager**: Assign Player Prefab and PlayerSpawnPoint
2. **EnemySpawner**: Add enemy prefabs to Enemy Types list
3. **WaveManager**: Assign Boss prefab
4. **Enemy Prefabs**: Assign EnemyBullet prefab and PowerUp prefabs
5. **Player Prefab**: Assign PlayerBullet prefab
6. **UIManager**: Connect all UI element references

### Step 5: Configure Build Settings

1. **File → Build Settings**
2. Click "Add Open Scenes" for both scenes
3. Ensure order is:
   - MainMenu (index 0)
   - GameScene (index 1)
4. Select **PC, Mac & Linux Standalone**
5. Target Platform: **Windows**
6. Architecture: **x86_64**

### Step 6: Build the Game

1. Click **Build** in Build Settings
2. Choose output folder (e.g., `SpaceShooter_Build`)
3. Name the executable (e.g., `SpaceShooter.exe`)
4. Click **Save**
5. Wait for build to complete

### Step 7: Run the Game

1. Navigate to your build folder
2. Double-click `SpaceShooter.exe`
3. Enjoy the game!

## 🎮 Controls

| Action | Key |
|--------|-----|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Space |
| Pause | Escape |
| Restart (Game Over) | R |

## 🔧 Customization

### Adding Sound Effects

1. Import audio files to `Assets/Audio/`
2. In PlayerController, Enemy, and PowerUp prefabs, assign AudioClip fields
3. Or use AudioManager for centralized audio control

### Adjusting Difficulty

In **EnemySpawner** component:
- `Spawn Interval`: Time between spawns
- `Enemies Per Wave`: Base enemies per wave
- `Enemies Increase Per Wave`: Additional enemies each wave

In **WaveManager** component:
- `Time Between Waves`: Delay between waves
- `Boss Wave Interval`: Boss appears every N waves

### Creating Custom Enemies

1. Duplicate an existing enemy prefab
2. Modify Enemy component settings:
   - `Enemy Type`: Basic, Fast, Tank, Boss
   - `Movement Pattern`: Straight, Zigzag, Circular, Homing
   - `Shooting Pattern`: None, Single, Spread, Burst
3. Adjust stats (health, speed, score value)
4. Add to EnemySpawner's enemy list

## 📝 Scripts Reference

| Script | Purpose |
|--------|---------|
| PlayerController | Player movement, shooting, power-up effects |
| HealthSystem | Health management with events |
| Bullet | Projectile behavior and collision |
| Enemy | Enemy AI, movement patterns, shooting |
| EnemySpawner | Wave-based enemy spawning |
| PowerUp | Power-up types and collection |
| GameManager | Game state, pause, restart |
| ScoreManager | Score tracking with persistence |
| WaveManager | Wave progression and boss spawning |
| UIManager | HUD and menu updates |
| MenuManager | Main menu navigation |
| ParallaxBackground | Scrolling background layers |
| AudioManager | Centralized audio control |
| CameraShake | Screen shake effects |

## 🐛 Troubleshooting

### "Scene not found" error
- Ensure scenes are added to Build Settings in correct order

### Objects not colliding
- Check that colliders have "Is Trigger" enabled
- Verify tags match (Player, Enemy, PlayerBullet, EnemyBullet)
- Ensure Rigidbody2D is present on moving objects

### UI not responding
- Verify EventSystem exists in scene
- Check Canvas has GraphicRaycaster component
- Ensure buttons have correct OnClick events assigned

### Build fails
- Check console for specific errors
- Verify all scripts compile without errors
- Ensure all prefab references are assigned

## 📄 License

This project is provided as-is for educational purposes. Feel free to modify and use for your own projects.

## 🎯 Next Steps

1. Add custom sprites and animations
2. Implement sound effects and music
3. Add particle effects for explosions
4. Create more enemy types and patterns
5. Add achievements and leaderboards
6. Polish UI with animations
7. Add screen transitions
