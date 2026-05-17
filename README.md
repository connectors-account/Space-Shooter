# 🚀 Space Shooter Game

A classic top-down space shooter built with Unity (C#). Defend the galaxy from waves of enemy ships with increasing difficulty!

![Genre](https://img.shields.io/badge/genre-space%20shooter-blue)
![Engine](https://img.shields.io/badge/engine-Unity%202021.3%2B-green)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

---

## 🎮 Game Features

- **Player Ship** — Smooth movement with WASD/Arrow keys, rapid-fire shooting
- **3 Enemy Types** — Basic (red), Fast (orange, sine-wave movement), Tank (purple, high HP)
- **Wave Progression** — Enemies get tougher, faster, and more numerous each wave
- **Combo Scoring** — Chain kills for multiplier bonuses
- **Health System** — 5 hearts with invincibility frames on hit
- **Health Pickups** — Dropped by enemies, heals 1 heart
- **Procedural Starfield** — Multi-layer parallax background with particle stars
- **Full UI** — Score HUD, health display, wave announcements, game over screen
- **Main Menu** — Title screen with Play/Quit and high score display
- **Pause Menu** — Press ESC to pause, resume, or quit
- **High Score Persistence** — Saved via PlayerPrefs across sessions

---

## 🕹️ Controls

| Action          | Key(s)                    |
|-----------------|---------------------------|
| Move            | `W` `A` `S` `D` or Arrow Keys |
| Shoot           | `Space` (hold for rapid fire) |
| Pause / Resume  | `Escape`                  |

---

## 📋 Requirements

- **Unity 2021.3 LTS** or newer (any 2021.3.x, 2022.x, or 2023.x version)
- **Windows** PC for building a standalone executable
- No additional packages required — all dependencies are built-in Unity modules

---

## 🛠️ How to Open the Project in Unity

### Step 1: Install Unity
1. Download and install [Unity Hub](https://unity.com/download)
2. In Unity Hub, install **Unity 2021.3 LTS** (or newer) with the **Windows Build Support** module

### Step 2: Open the Project
1. Open **Unity Hub**
2. Click **"Open"** (or "Add project from disk")
3. Navigate to this project's root folder (`space_shooter_game/`)
4. Select the folder and click **"Open"**
5. Unity will import assets and compile scripts (first time may take 1-2 minutes)

### Step 3: Automatic Setup
When the project opens for the first time, the **ProjectSetup** editor script will automatically:
- Configure custom tags (`PlayerBullet`, `EnemyBullet`, `Enemy`, `HealthPickup`)
- Set up the build scenes list (MainMenuScene → GameScene)

If auto-setup doesn't trigger, run it manually: **Menu Bar → SpaceShooter → Setup Project**

### Step 4: Play in Editor
1. Open `Assets/Scenes/MainMenuScene` in the Project window
2. Press the **Play** button (▶) in the Unity toolbar
3. Click **PLAY** on the main menu to start the game

---

## 🏗️ How to Build for Windows (Standalone Executable)

### Option A: Via Unity Editor
1. Open the project in Unity
2. Go to **File → Build Settings**
3. Ensure both scenes are listed (MainMenuScene at index 0, GameScene at index 1)
   - If not, click **"Add Open Scenes"** for each scene
4. Select **"PC, Mac & Linux Standalone"** as the platform
5. Set **Target Platform** to **Windows**
6. Set **Architecture** to **x86_64** (recommended)
7. Click **"Build"** or **"Build And Run"**
8. Choose an output folder (e.g., `Build/`)
9. Unity will compile and create a Windows executable

### Option B: Via Command Line (Headless Build)
```bash
# Replace paths as appropriate for your system
"C:\Program Files\Unity\Hub\Editor\2021.3.x\Editor\Unity.exe" \
  -quit -batchmode \
  -projectPath "C:\path\to\space_shooter_game" \
  -buildWindows64Player "C:\path\to\Build\SpaceShooter.exe"
```

### Build Output
The build will produce:
```
Build/
├── SpaceShooter.exe          ← Run this to play!
├── SpaceShooter_Data/        ← Game data (required)
├── UnityPlayer.dll           ← Unity runtime (required)
└── UnityCrashHandler64.exe   ← Crash handler (optional)
```

**To distribute**: Zip the entire `Build/` folder. All files in the folder are needed to run the game.

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Editor/
│   │   └── ProjectSetup.cs              # Auto-configures tags & build settings
│   ├── Materials/                        # (empty - materials created at runtime)
│   ├── Prefabs/                          # (empty - prefabs created at runtime)
│   ├── Resources/                        # (empty - resources loaded at runtime)
│   ├── Scenes/
│   │   ├── MainMenuScene.unity           # Title screen scene
│   │   └── GameScene.unity               # Main gameplay scene
│   └── Scripts/
│       ├── Background/
│       │   ├── ParallaxBackground.cs     # Scrolling background layer
│       │   └── StarfieldGenerator.cs     # Procedural star particles
│       ├── Bullets/
│       │   └── Bullet.cs                 # Generic projectile behavior
│       ├── Enemy/
│       │   ├── EnemyBase.cs              # Enemy AI, health, movement patterns
│       │   └── WaveSpawner.cs            # Wave-based spawn management
│       ├── Managers/
│       │   ├── GameManager.cs            # Game state, pause, scene transitions
│       │   ├── GameSceneBootstrap.cs     # Runtime setup for gameplay scene
│       │   ├── MainMenuBootstrap.cs      # Runtime setup for menu scene
│       │   └── ScoreManager.cs           # Score tracking, combos, high scores
│       ├── Pickups/
│       │   └── HealthPickup.cs           # Health drop behavior
│       └── UI/
│           ├── MainMenuUI.cs             # Main menu button handlers
│           └── UIManager.cs              # In-game HUD, overlays, announcements
├── Packages/
│   └── manifest.json                     # Unity package dependencies
├── ProjectSettings/
│   ├── AudioManager.asset
│   ├── EditorBuildSettings.asset
│   ├── GraphicsSettings.asset
│   ├── InputManager.asset
│   ├── Physics2DSettings.asset
│   ├── ProjectSettings.asset
│   ├── QualitySettings.asset
│   ├── TagManager.asset
│   └── TimeManager.asset
├── .gitignore
└── README.md
```

---

## 🎨 How It Works (Architecture)

### Runtime Bootstrap Pattern
This project uses a **runtime bootstrap** pattern instead of pre-built prefabs. When each scene loads:

1. **MainMenuBootstrap** / **GameSceneBootstrap** creates all GameObjects programmatically
2. Sprites are generated procedurally (triangles for ships, rectangles for bullets, circles for pickups)
3. UI Canvas, panels, buttons, and text are all built in code
4. Enemy prefab templates are created as inactive GameObjects and passed to the WaveSpawner

This means the game works immediately without any manual Unity Editor prefab wiring.

### Key Systems
- **GameManager** — Singleton, persists across scenes (`DontDestroyOnLoad`), manages game state
- **WaveSpawner** — Spawns waves with increasing enemy count, speed, and health
- **ScoreManager** — Tracks score with combo multiplier (chain kills = bigger points)
- **EnemyBase** — Configurable enemy with 5 movement patterns (straight, sine, diagonal, zigzag)
- **UIManager** — Updates all HUD elements, shows wave announcements and game over screen

### Collision Flow
```
PlayerBullet hits Enemy → Enemy.TakeDamage() → Die → ScoreManager.AddScore()
EnemyBullet hits Player → Player.TakeDamage() → Invincibility frames
Enemy hits Player → Player.TakeDamage(2) → Higher damage for contact
HealthPickup hits Player → Player.Heal(1)
```

---

## ⚙️ Customization

All values are exposed as public fields on the scripts. In the Unity Inspector, you can tweak:

| Setting | Script | Default |
|---------|--------|---------|
| Player move speed | PlayerController | 8 |
| Fire rate | PlayerController | 0.2s |
| Player max health | PlayerController | 5 |
| Enemies per wave | WaveSpawner | 5 (base) +2/wave |
| Spawn rate | WaveSpawner | 1s (decreasing) |
| Enemy health | EnemyBase | 2/1/5 by type |
| Enemy speed | EnemyBase | 2/4/1.5 by type |
| Health drop chance | EnemyBase | 15% |
| Star count | StarfieldGenerator | 200 |

---

## 📝 License

This project is provided as-is for educational purposes. Feel free to modify and distribute.
