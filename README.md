# 🚀 Space Shooter - Unity Desktop Game

A complete 2D space shooter game built with Unity and C#. Features player ship with upgradeable weapons, multiple enemy types with different behaviors, power-ups, wave-based progression, and a full UI system.

---

## 📋 Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Project Setup](#project-setup)
- [Building the Game](#building-the-game)
- [Game Controls](#game-controls)
- [Project Structure](#project-structure)
- [Game Architecture](#game-architecture)
- [Customization](#customization)

---

## ✨ Features

- **Player Ship**: Smooth movement with WASD/Arrow keys, multi-level weapon system
- **4 Enemy Types**: Basic (straight), Zigzag, Bomber (spread shot), Elite (aimed shots)
- **Wave System**: Progressive difficulty with increasing enemy count and variety
- **5 Power-Up Types**: Weapon Upgrade, Shield, Health, Speed Boost, Score Bonus
- **Full UI System**: Main Menu, HUD (score, health, wave, lives), Pause Menu, Game Over screen
- **Parallax Background**: Scrolling starfield with depth layers
- **Audio System**: Full sound effect integration (plug in your own audio clips)
- **High Score**: Persistent high score saved between sessions
- **Collision System**: Proper layer-based collision filtering

---

## 🔧 Requirements

- **Unity Editor**: Version **2022.3 LTS** or newer (2023.x / 6000.x also work)
  - Download from: https://unity.com/download
  - Or install via Unity Hub: https://unity.com/unity-hub
- **OS**: Windows 10/11 (for building .exe), macOS, or Linux (for development)
- **Build Target**: Windows (Standalone) module must be installed in Unity Hub

### Installing Unity

1. Download and install **Unity Hub** from https://unity.com/unity-hub
2. In Unity Hub, go to **Installs** → **Install Editor**
3. Select **Unity 2022.3 LTS** (or newer)
4. In the module selection, ensure **Windows Build Support (Mono)** is checked
5. Click **Install**

---

## 🚀 Project Setup

### Step 1: Open the Project

1. Open **Unity Hub**
2. Click **Open** → **Add project from disk**
3. Navigate to this `space_shooter_unity` folder and select it
4. Unity will import the project (this may take a few minutes on first open)

### Step 2: Run the Automated Setup

Once Unity opens the project:

1. Go to the menu bar: **Space Shooter** → **Setup Entire Project**
2. Click **"Yes, Set Up"** in the confirmation dialog
3. Wait for the setup to complete (creates prefabs, scenes, configures tags/layers)
4. You'll see a confirmation dialog when done

> **What the setup does:**
> - Creates all game prefabs (Player, Enemies, Bullets, PowerUps, Explosion)
> - Creates MainMenu and GameScene scenes with full UI
> - Configures Tags (Player, Enemy, PlayerBullet, EnemyBullet, PowerUp, Boundary)
> - Configures Sorting Layers (Background, Stars, Entities, Bullets, Effects, UI)
> - Configures Physics Layers (PlayerBullet, EnemyBullet, Player, Enemy, PowerUp)
> - Sets up the collision matrix (so friendly bullets don't hit friendlies)
> - Configures Build Settings with both scenes

### Step 3: Configure Sprites (if needed)

The setup script loads sprites from `Assets/Resources/Sprites/`. If sprites don't appear:

1. Select all PNG files in `Assets/Resources/Sprites/` in the Project panel
2. In the Inspector, set **Texture Type** to **Sprite (2D and UI)**
3. Set **Pixels Per Unit** to **100**
4. Click **Apply**

### Step 4: Play Test

1. Open `Assets/Scenes/MainMenu.unity`
2. Press **Play** (▶) in the Unity Editor
3. Click **START GAME** to begin playing!

---

## 🔨 Building the Game (Windows .exe)

### Method 1: Unity Editor Build

1. Open the project in Unity
2. Go to **File** → **Build Settings** (Ctrl+Shift+B)
3. Verify scenes are listed:
   - `Scenes/MainMenu` (index 0)
   - `Scenes/GameScene` (index 1)
4. Set **Target Platform** to **Windows**
5. Set **Architecture** to **x86_64**
6. Click **Build** or **Build and Run**
7. Choose an output folder (e.g., `Build/`)
8. The .exe will be created in the chosen folder

### Method 2: Command Line Build (Batch Mode)

```bash
# Windows (PowerShell)
"C:\Program Files\Unity\Hub\Editor\2022.3.XXf1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "PATH_TO\space_shooter_unity" `
  -buildWindows64Player "Build\SpaceShooter.exe" `
  -logFile build.log

# macOS/Linux
/Applications/Unity/Hub/Editor/2022.3.XXf1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "/path/to/space_shooter_unity" \
  -buildWindows64Player "Build/SpaceShooter.exe" \
  -logFile build.log
```

> Replace `2022.3.XXf1` with your installed Unity version.

### Build Output

The build produces:
```
Build/
├── SpaceShooter.exe           # Main executable
├── SpaceShooter_Data/         # Game data folder
├── UnityCrashHandler64.exe    # Crash handler
└── UnityPlayer.dll            # Unity runtime
```

Distribute the **entire Build folder** to players.

---

## 🎮 Game Controls

| Action        | Key(s)                    |
|---------------|---------------------------|
| Move Up       | W / ↑ Arrow               |
| Move Down     | S / ↓ Arrow               |
| Move Left     | A / ← Arrow               |
| Move Right    | D / → Arrow               |
| Shoot         | Space / Left Mouse Button |
| Pause         | Escape                    |

---

## 📁 Project Structure

```
space_shooter_unity/
├── Assets/
│   ├── Editor/
│   │   └── ProjectSetup.cs          # Auto-setup editor script
│   ├── Prefabs/
│   │   ├── Player/                   # Player ship prefab
│   │   ├── Enemies/                  # Enemy prefabs (4 types)
│   │   ├── Bullets/                  # Bullet prefabs
│   │   ├── PowerUps/                 # Power-up prefabs (5 types)
│   │   └── Effects/                  # Explosion effect prefab
│   ├── Resources/
│   │   ├── Sprites/                  # All sprite PNG assets
│   │   └── Audio/                    # Audio clips (add your own)
│   ├── Scenes/
│   │   ├── MainMenu.unity           # Main menu scene
│   │   └── GameScene.unity          # Gameplay scene
│   └── Scripts/
│       ├── Player/
│       │   └── PlayerController.cs   # Player movement, shooting, health
│       ├── Enemy/
│       │   └── EnemyController.cs    # Enemy AI, movement patterns, shooting
│       ├── Bullets/
│       │   └── BulletController.cs   # Bullet movement and damage
│       ├── PowerUps/
│       │   └── PowerUpController.cs  # Power-up effects and collection
│       ├── Managers/
│       │   ├── GameManager.cs        # Game state, scoring, lives
│       │   ├── SpawnManager.cs       # Wave spawning logic
│       │   ├── UIManager.cs          # HUD and UI panels
│       │   ├── AudioManager.cs       # Sound effects system
│       │   └── MenuManager.cs        # Main menu logic
│       ├── Effects/
│       │   ├── ParallaxBackground.cs # Scrolling background
│       │   ├── ExplosionEffect.cs    # Explosion animation
│       │   └── StarfieldGenerator.cs # Procedural star background
│       └── Utils/
│           ├── CollisionHandler.cs   # Collision response logic
│           ├── GameInitializer.cs    # Scene bootstrap
│           ├── AutoDestroy.cs        # Timed self-destruction
│           └── ScreenWrapper.cs      # Screen edge wrapping
├── ProjectSettings/                  # Unity project configuration
├── Packages/
│   └── manifest.json                 # Package dependencies
├── generate_sprites.py               # Sprite generation script (Python)
├── .gitignore                        # Git ignore for Unity
└── README.md                         # This file
```

---

## 🏗 Game Architecture

### Singleton Managers
- **GameManager**: Central game state (score, lives, waves, difficulty). Persists across scenes.
- **AudioManager**: Handles all music and SFX. Persists across scenes.
- **SpawnManager**: Controls enemy wave spawning (scene-specific).
- **UIManager**: Manages all UI elements (scene-specific).

### Enemy Types
| Type    | Health | Speed | Pattern      | Shooting     | Score |
|---------|--------|-------|--------------|--------------|-------|
| Basic   | 1      | 3.0   | Straight     | Single down  | 100   |
| Zigzag  | 2      | 2.5   | Zigzag       | Single down  | 200   |
| Bomber  | 4      | 1.5   | Sine Wave    | 3-way spread | 350   |
| Elite   | 6      | 2.0   | Circle Entry | Aimed at player | 500 |

### Power-Up Types
| Type           | Color   | Effect                          | Duration |
|----------------|---------|----------------------------------|----------|
| Weapon Upgrade | Orange  | Adds bullet streams (up to 4x)  | 8s       |
| Shield         | Blue    | Absorbs one hit                  | 8s       |
| Health         | Green   | Restores 2 HP                    | Instant  |
| Speed Boost    | Yellow  | Bonus weapon upgrade             | 4s       |
| Score Bonus    | Magenta | +500 points                      | Instant  |

### Wave Progression
- Waves start with 5 enemies, increasing by 2 per wave (max 30)
- Early waves: Basic enemies only
- Waves 3-5: Basic + Zigzag
- Waves 6-8: All types mixed
- Wave 9+: Heavy mix with more Elites and Bombers
- Difficulty multiplier increases over time (affects score)

---

## 🎨 Customization

### Adding Sound Effects
1. Place `.wav` or `.mp3` files in `Assets/Resources/Audio/`
2. Select the **AudioManager** object in the scene (or the prefab)
3. Drag audio clips to the corresponding fields:
   - `playerShootClip`, `enemyShootClip`, `playerHitClip`
   - `enemyExplosionClip`, `playerExplosionClip`, `powerUpClip`
   - `shieldActivateClip`, `weaponUpgradeClip`, `healClip`
   - `buttonClickClip`, `waveStartClip`, `gameOverClip`
   - `menuMusic`, `gameMusic`

### Modifying Sprites
Replace PNG files in `Assets/Resources/Sprites/` with your own artwork. Ensure:
- **Texture Type**: Sprite (2D and UI)
- **Pixels Per Unit**: 100
- Same filenames, or update prefab references

### Tweaking Difficulty
- **SpawnManager**: Adjust `baseEnemiesPerWave`, `enemiesPerWaveIncrease`, `timeBetweenWaves`
- **GameManager**: Adjust `difficultyScaleRate`, `startingLives`
- **Enemy Prefabs**: Modify health, speed, fire rate, drop chance
- **PlayerController**: Adjust `moveSpeed`, `fireRate`, `maxHealth`

### Regenerating Sprites
If you need to regenerate the basic sprites:
```bash
pip install Pillow
python generate_sprites.py
```

---

## 🐛 Troubleshooting

### "Setup Entire Project" menu not appearing
- Wait for Unity to finish compiling scripts (check the bottom-left progress bar)
- If errors appear in the Console, fix them first (usually missing package imports)

### Sprites appear white/missing
- Select sprites in Project panel → Inspector → Set Texture Type to "Sprite (2D and UI)" → Apply
- Ensure sprites are in `Assets/Resources/Sprites/` folder

### Physics not working (bullets pass through)
- Run **Space Shooter → Setup Tags and Layers Only** to reconfigure
- Verify colliders are set to **Is Trigger = true** on prefabs
- Check that Rigidbody2D exists on all physics objects

### Build fails
- Ensure both scenes are in Build Settings (File → Build Settings)
- Check Console for compilation errors
- Ensure Windows Build Support module is installed in Unity Hub

---

## 📄 License

This project is provided as-is for educational and personal use. Feel free to modify and distribute.

---

*Built with Unity 2022.3+ | C# | 2D Sprite Renderer | Unity UI*
