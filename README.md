# Space Shooter - Unity Desktop Game

A complete space-shooter game for Windows desktop built with Unity and C#. Features wave-based enemy spawning, multiple enemy types, power-ups, procedurally generated sprites and sound effects, and full UI.

---

## 🎮 Controls

| Action        | Key(s)                          |
|---------------|--------------------------------|
| Move          | Arrow Keys or WASD             |
| Shoot         | Spacebar (hold for continuous) |
| Pause/Resume  | Escape                         |

---

## 🚀 Game Features

### Player
- Smooth ship movement with screen-edge clamping
- Visual tilt when moving horizontally
- Engine glow effect
- Invincibility frames after taking damage (flashing sprite)

### Enemies (4 Types)
| Enemy       | Color   | Behavior                              | Health | Score | Appears |
|-------------|---------|---------------------------------------|--------|-------|---------|
| Straight    | Red     | Moves straight down                   | 1      | 100   | Wave 1+ |
| ZigZag      | Orange  | Sine-wave horizontal movement         | 2      | 150   | Wave 3+ |
| Diver       | Magenta | Dives toward player position          | 1      | 200   | Wave 5+ |
| Tank        | Dark Red| Slow, heavy, shoots at player         | 5      | 300   | Wave 7+ |

### Power-Ups (4 Types)
| Power-Up    | Color  | Effect                                 |
|-------------|--------|----------------------------------------|
| Rapid Fire  | Yellow | 3x fire rate for 5 seconds             |
| Shield      | Cyan   | Absorbs one hit                        |
| Spread Shot | Orange | Fires 3 bullets in a spread for 5 sec  |
| Health      | Green  | Restores 1 health point                |

### Wave System
- Progressive difficulty: more enemies per wave
- New enemy types unlock at higher waves
- Spawn rate increases with wave number
- Brief cooldown between waves

### Other Features
- Parallax scrolling starfield background (3 depth layers)
- Procedurally generated geometric sprites (no art assets needed)
- Procedurally generated sound effects (beeps, sweeps, noise bursts)
- Object pooling for all bullets, enemies, and power-ups
- Score tracking with persistent high score (via PlayerPrefs)
- Full menu system: Main Menu → Gameplay → Pause → Game Over

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs        # Central game state, scoring, wave control
│   │   │   ├── GameState.cs          # Game state enum
│   │   │   ├── ObjectPool.cs         # Generic object pooling system
│   │   │   └── GameBounds.cs         # Screen boundary calculations
│   │   ├── Player/
│   │   │   ├── PlayerController.cs   # WASD/Arrow key movement
│   │   │   ├── PlayerHealth.cs       # Health, damage, invincibility, shield
│   │   │   └── PlayerShooting.cs     # Firing, rapid fire, spread shot
│   │   ├── Enemies/
│   │   │   ├── EnemyBase.cs          # Abstract base for all enemies
│   │   │   ├── EnemyStraight.cs      # Basic straight-moving enemy
│   │   │   ├── EnemyZigZag.cs        # Sine-wave movement enemy
│   │   │   ├── EnemyDiver.cs         # Player-targeting dive enemy
│   │   │   ├── EnemyTank.cs          # Heavy shooting enemy
│   │   │   └── EnemySpawner.cs       # Wave-based spawn controller
│   │   ├── Combat/
│   │   │   └── Bullet.cs             # Bullet movement and collision
│   │   ├── PowerUps/
│   │   │   ├── PowerUp.cs            # Power-up pickup and effects
│   │   │   └── PowerUpSpawner.cs     # Random power-up spawning
│   │   ├── UI/
│   │   │   ├── HUDManager.cs         # In-game HUD (health, score, wave)
│   │   │   ├── MainMenuUI.cs         # Start screen
│   │   │   ├── PauseMenuUI.cs        # Pause overlay
│   │   │   └── GameOverUI.cs         # Game over screen
│   │   ├── Visual/
│   │   │   ├── SpriteGenerator.cs    # Procedural sprite creation
│   │   │   ├── ParallaxBackground.cs # Scrolling starfield
│   │   │   ├── ShieldVisual.cs       # Shield power-up visual effect
│   │   │   └── SceneBootstrap.cs     # Auto-creates entire scene at runtime
│   │   └── Audio/
│   │       └── AudioManager.cs       # Procedural sound effect generation
│   ├── Editor/
│   │   └── ProjectSetupHelper.cs     # One-click scene and build setup
│   ├── Prefabs/                      # (Created at runtime by SceneBootstrap)
│   ├── Scenes/                       # MainScene.unity (created by setup)
│   ├── Resources/
│   └── Materials/
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   └── Physics2DSettings.asset
└── README.md
```

---

## 🛠️ Setup Instructions

### Prerequisites
- **Unity 2021.3 LTS** or later (any 2021+, 2022+, or 2023+ version works)
- **Windows 10/11** for building the .exe
- Unity modules: **Windows Build Support (IL2CPP or Mono)**

### Step 1: Open in Unity
1. Open **Unity Hub**
2. Click **Open** → Navigate to the `space_shooter_game` folder → Click **Open**
3. Unity will import the project (this may take a few minutes on first open)

### Step 2: Automatic Scene Setup
1. In Unity, go to the menu bar: **SpaceShooter → Setup Scene**
2. This will automatically:
   - Create a new scene with the Bootstrap object
   - Configure project settings (resolution, orientation)
   - Save the scene to `Assets/Scenes/MainScene.unity`
   - Set up build settings

### Step 3: Test in Editor
1. Press the **Play** button (▶) in the Unity Editor
2. The game should start at the Main Menu
3. Click **START GAME** to play

### Alternative Manual Setup (if menu doesn't appear)
1. Create a new Scene: **File → New Scene → Basic (Built-in)**
2. Create an empty GameObject: **GameObject → Create Empty**
3. Name it **"Bootstrap"**
4. Attach the **SceneBootstrap** script to it (drag from Assets/Scripts/Visual/)
5. Save the scene as `Assets/Scenes/MainScene.unity`
6. Press **Play** to test

---

## 🏗️ Building a Windows Executable (.exe)

### Method 1: Using the Built-in Menu
1. Go to **SpaceShooter → Build Windows** in the Unity menu bar
2. The .exe will be created at `Builds/Windows/SpaceShooter.exe`

### Method 2: Manual Build
1. Go to **File → Build Settings**
2. Ensure `Scenes/MainScene` is in the **Scenes In Build** list
   - If not, click **Add Open Scenes**
3. Set **Platform** to **PC, Mac & Linux Standalone**
4. Set **Target Platform** to **Windows**
5. Set **Architecture** to **x86_64**
6. Click **Build**
7. Choose an output folder and filename (e.g., `SpaceShooter.exe`)
8. Wait for the build to complete

### Build Output
The build creates:
```
Builds/Windows/
├── SpaceShooter.exe           # Main executable
├── SpaceShooter_Data/         # Game data folder
├── UnityCrashHandler64.exe    # Crash handler
└── UnityPlayer.dll            # Unity runtime
```

**To distribute:** Zip the entire `Windows/` folder. Players just run `SpaceShooter.exe`.

---

## 🎯 How the Architecture Works

### SceneBootstrap (Key Design Decision)
Instead of manually configuring prefabs and scenes in the Unity Editor, the **SceneBootstrap** script creates everything programmatically at runtime:
- All sprites are generated as textures in memory
- All prefabs are created as GameObjects and registered with the Object Pool
- All UI elements (Canvas, panels, buttons, text) are built via code
- Sound effects are synthesized as AudioClips from sine waves and noise

This means you only need **one empty GameObject** with SceneBootstrap attached to run the entire game.

### Object Pooling
All frequently created/destroyed objects (bullets, enemies, power-ups) use the ObjectPool system to avoid garbage collection stalls. The pool auto-grows if exhausted.

### Game State Machine
The GameManager controls state transitions:
```
MainMenu → Playing → Paused → Playing → GameOver → MainMenu
                                                   → Playing (Retry)
```

---

## ⚙️ Customization

Key values you can tweak in the Inspector (or in code):

| Script            | Property              | Default | Description                    |
|-------------------|-----------------------|---------|--------------------------------|
| PlayerController  | moveSpeed             | 8       | Player movement speed          |
| PlayerHealth      | maxHealth             | 5       | Starting health points         |
| PlayerShooting    | fireRate              | 0.25    | Seconds between shots          |
| PlayerShooting    | rapidFireDuration     | 5       | Rapid fire power-up duration   |
| GameManager       | enemiesPerWave        | 5       | Base enemies in wave 1         |
| GameManager       | waveCooldown          | 3       | Seconds between waves          |
| EnemySpawner      | spawnInterval         | 1.5     | Seconds between enemy spawns   |
| EnemyBase         | powerUpDropChance     | 0.15    | Chance of power-up drop (0-1)  |

---

## 📝 License

This project is provided as-is for educational and personal use. Feel free to modify and extend it.
