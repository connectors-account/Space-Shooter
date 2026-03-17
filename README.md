# 🚀 Space Shooter - Unity Desktop Game

A complete 2D space shooter game built with Unity and C#. Defend the galaxy against waves of enemies, collect power-ups, and achieve the highest score!

---

## 🎮 Game Features

| Feature | Description |
|---------|-------------|
| **Player Ship** | Keyboard-controlled with WASD/Arrow keys + Space to shoot |
| **3 Enemy Types** | Basic (straight-line), Fast (zigzag), Tanky (slow + heavy fire) |
| **Wave System** | Progressive difficulty with increasing enemy count and variety |
| **Power-ups** | Rapid Fire, Shield, and Health Restore drops from enemies |
| **Parallax Background** | Procedurally-generated scrolling starfield |
| **Full UI** | Main Menu, HUD, Pause Menu, Game Over screen |
| **Collision System** | Full 2D trigger-based collision detection |
| **Audio Integration** | SFX and music hook points (add your own audio clips) |

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   └── PlayerController.cs      # Player movement, shooting, health, power-ups
│   │   ├── Enemies/
│   │   │   ├── EnemyBase.cs             # Base enemy class (health, drops, shooting)
│   │   │   ├── BasicEnemy.cs            # Standard enemy - moves down, shoots
│   │   │   ├── FastEnemy.cs             # Fast zigzag enemy
│   │   │   ├── TankyEnemy.cs            # Slow, high-HP enemy with heavy fire
│   │   │   └── EnemySpawner.cs          # Wave management and enemy spawning
│   │   ├── Weapons/
│   │   │   └── Bullet.cs                # Bullet movement and collision
│   │   ├── Powerups/
│   │   │   └── PowerUp.cs               # Power-up types and pickup logic
│   │   ├── Managers/
│   │   │   ├── GameManager.cs            # Central game state (score, game over)
│   │   │   ├── AudioManager.cs           # SFX and music playback
│   │   │   ├── GameSceneSetup.cs         # Runtime scene bootstrapper (creates all objects)
│   │   │   └── BulletCleaner.cs          # Utility to clean up projectiles
│   │   ├── UI/
│   │   │   ├── HUDManager.cs             # Score, health, wave display
│   │   │   ├── MainMenuUI.cs             # Start/Quit buttons
│   │   │   ├── MainMenuSetup.cs          # Runtime main menu builder
│   │   │   ├── GameOverUI.cs             # Game over screen
│   │   │   ├── PauseMenuUI.cs            # Pause menu (ESC key)
│   │   │   └── UISetupHelper.cs          # Runtime HUD/UI builder
│   │   ├── Environment/
│   │   │   ├── ParallaxBackground.cs     # Scrolling background layer
│   │   │   └── BackgroundSetup.cs        # Procedural starfield generator
│   │   └── Effects/
│   │       └── Explosion.cs              # Explosion visual effect
│   ├── Scenes/
│   │   ├── MainMenu.unity               # (Created in Unity Editor)
│   │   └── GameScene.unity              # (Created in Unity Editor)
│   ├── Prefabs/                          # (Created in Unity Editor)
│   ├── Sprites/                          # (Auto-generated or add your own)
│   ├── Audio/
│   │   ├── Music/                        # Place background music here
│   │   └── SFX/                          # Place sound effects here
│   └── Animations/                       # Optional animations
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   ├── EditorBuildSettings.asset
│   ├── Physics2DSettings.asset
│   ├── InputManager.asset
│   └── QualitySettings.asset
├── .gitignore
└── README.md
```

---

## 🛠️ Setup Instructions (Step by Step)

### Prerequisites
- **Unity Hub** installed ([Download here](https://unity.com/download))
- **Unity Editor 2021.3 LTS or newer** (any 2021+/2022+/2023+ version works)
- **Windows 10/11** for building the executable
- Unity **Windows Build Support** module installed

### Step 1: Create the Unity Project

1. Open **Unity Hub**
2. Click **"New Project"**
3. Select **"2D (Built-in Render Pipeline)"** template
4. Name the project: `SpaceShooter`
5. Choose your desired location
6. Click **"Create project"**

### Step 2: Import the Scripts

1. Once Unity opens, navigate to the `Assets` folder in your file explorer
2. **Copy all files from this repository's `Assets/Scripts/` folder** into your Unity project's `Assets/Scripts/` folder
3. Wait for Unity to compile (check the bottom-right spinner)
4. You should see **0 errors** in the Console window

### Step 3: Set Up Tags

The game requires specific tags. Set them up:

1. Go to **Edit → Project Settings → Tags and Layers**
2. Under **Tags**, add:
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`
   - `PowerUp`
3. The `Player` tag should already exist by default

> **Note:** The `TagManager.asset` file in `ProjectSettings/` contains these pre-configured, but Unity may not pick them up automatically. Verify manually.

### Step 4: Configure Physics 2D

1. Go to **Edit → Project Settings → Physics 2D**
2. Set **Gravity Y** to `0` (zero-gravity space!)
3. This is already configured in the provided `Physics2DSettings.asset`

### Step 5: Create the Main Menu Scene

1. In Unity, go to **File → New Scene** (choose "Basic 2D")
2. Save it as `Assets/Scenes/MainMenu.unity`
3. In the Hierarchy, create an **Empty GameObject** named `MainMenuController`
4. Add the **`MainMenuSetup`** component to it
5. That's it! The script builds the entire menu at runtime.

### Step 6: Create the Game Scene

1. Go to **File → New Scene** (choose "Basic 2D")
2. Save it as `Assets/Scenes/GameScene.unity`
3. In the Hierarchy, create an **Empty GameObject** named `GameBootstrapper`
4. Add the **`GameSceneSetup`** component to it
5. The script creates **everything** at runtime:
   - Player ship (with procedural sprite)
   - Enemy prefabs (with procedural sprites)
   - Bullet prefabs
   - Power-up prefabs
   - Background starfield
   - All UI (HUD, Game Over, Pause Menu)
   - Audio Manager
   - Game Manager

### Step 7: Add Scenes to Build Settings

1. Go to **File → Build Settings**
2. Click **"Add Open Scenes"** for each scene, OR drag them from the Project panel:
   - `Assets/Scenes/MainMenu` — **Index 0** (first scene loaded)
   - `Assets/Scenes/GameScene` — **Index 1**
3. Make sure **MainMenu is at index 0** (drag to reorder if needed)

### Step 8: Test in Editor

1. Open the **MainMenu** scene
2. Press **Play** ▶️
3. Click **"START GAME"** — the game scene should load
4. Use **WASD/Arrows** to move, **Space** to shoot, **ESC** to pause
5. Enemies spawn in waves; collect power-ups dropped by destroyed enemies

---

## 🎯 Controls

| Key | Action |
|-----|--------|
| `W` / `↑` | Move Up |
| `S` / `↓` | Move Down |
| `A` / `←` | Move Left |
| `D` / `→` | Move Right |
| `Space` | Shoot |
| `Escape` | Pause / Resume |

---

## 🔊 Adding Sound Effects (Optional)

The `AudioManager` has named slots for audio clips. To add sounds:

1. Import `.wav` or `.mp3` files into `Assets/Audio/SFX/` and `Assets/Audio/Music/`
2. Find the **AudioManager** GameObject in the scene (created at runtime, or create one manually)
3. Assign clips to the inspector slots:

| Slot Name | When It Plays |
|-----------|---------------|
| `backgroundMusic` | Loops during gameplay |
| `playerShoot` | Player fires a bullet |
| `enemyShoot` | Enemy fires a bullet |
| `playerHit` | Player takes damage |
| `enemyExplosion` | Enemy is destroyed |
| `playerExplosion` | Player is destroyed |
| `powerUpPickup` | Player collects a power-up |
| `shieldBreak` | Player's shield absorbs a hit |

> **Tip:** For quick prototyping, free sound effects are available at [freesound.org](https://freesound.org) or [kenney.nl/assets](https://kenney.nl/assets)

---

## 🎨 Replacing Procedural Sprites with Custom Art

The game generates simple geometric sprites at runtime. To use custom art:

1. Import your sprite images (`.png` recommended, with transparency) into `Assets/Sprites/`
2. Select each sprite in Unity and set:
   - **Texture Type:** Sprite (2D and UI)
   - **Pixels Per Unit:** 64 (or adjust to match your art scale)
3. Create **Prefabs** manually:
   - Create a player prefab with your sprite, `PlayerController` component, `BoxCollider2D` (trigger), `Rigidbody2D` (gravity=0)
   - Create enemy prefabs with sprites and the appropriate enemy script
   - Create bullet/explosion/power-up prefabs similarly
4. Assign these prefabs to the `GameSceneSetup` component's inspector slots
5. When prefab slots are filled, the runtime setup skips procedural generation

---

## 🏗️ Building as Windows Executable (.exe)

### Method 1: Build from Unity Editor

1. Open your project in Unity
2. Go to **File → Build Settings**
3. Select **"PC, Mac & Linux Standalone"** as the platform
4. Set **Target Platform** to **Windows**
5. Set **Architecture** to **x86_64**
6. Verify both scenes are listed (MainMenu at index 0, GameScene at index 1)
7. Click **"Player Settings"** and configure:
   - **Product Name:** `Space Shooter`
   - **Company Name:** (your name)
   - **Default Screen Width:** `1024`
   - **Default Screen Height:** `768`
   - **Fullscreen Mode:** `Windowed` (or your preference)
   - **Run in Background:** ✅ Enabled
8. Click **"Build"**
9. Choose an output folder (e.g., `Build/`)
10. Wait for the build to complete

### Method 2: Build from Command Line (CI/CD)

```bash
# Windows command line (Unity must be installed)
"C:\Program Files\Unity\Hub\Editor\2021.3.XXf1\Editor\Unity.exe" \
    -batchmode \
    -nographics \
    -projectPath "C:\path\to\SpaceShooter" \
    -buildTarget Win64 \
    -buildWindowsPlayer "C:\output\SpaceShooter.exe" \
    -quit
```

### Build Output

After building, your output folder will contain:
```
Build/
├── SpaceShooter.exe            # The executable
├── SpaceShooter_Data/          # Game data folder (REQUIRED)
│   ├── Managed/
│   ├── Resources/
│   └── ...
├── MonoBleedingEdge/           # Mono runtime (REQUIRED)
└── UnityCrashHandler64.exe
```

> ⚠️ **Important:** To distribute the game, you must include the **entire Build folder**, not just the `.exe` file. The `_Data` folder and `MonoBleedingEdge` folder are required for the game to run.

---

## 🎮 Game Mechanics Deep Dive

### Wave Progression
- **Wave 1:** 5 basic enemies
- **Wave 2:** 7 enemies (basic + fast introduced)
- **Wave 3+:** 9+ enemies (all types, tanky introduced)
- Each wave adds +2 enemies (capped at 30)
- Enemy type distribution shifts toward harder enemies over time

### Enemy Types
| Type | HP | Speed | Shoots? | Score | Behavior |
|------|-----|-------|---------|-------|----------|
| Basic | 1 | Medium | Yes (slow) | 100 | Moves straight down |
| Fast | 1 | Fast | No | 200 | Zigzag pattern, hard to hit |
| Tanky | 5 | Slow | Yes (fast) | 500 | Parks at top, strafes side-to-side |

### Power-ups
| Type | Color | Effect | Duration |
|------|-------|--------|----------|
| Rapid Fire | Yellow | 2.5x fire rate | 8 seconds |
| Shield | Blue | Absorbs one hit | Until hit |
| Health | Green | Restores 2 HP | Instant |

Power-ups have a 10-35% drop chance (higher for tanky enemies).

### Player Stats
- **Health:** 5 HP
- **Fire Rate:** 4 shots/sec (10/sec with rapid fire)
- **Invincibility:** 1.5 seconds after taking damage
- **Movement Speed:** 8 units/sec

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| **"Tag 'Enemy' is not defined"** | Add the `Enemy` tag in Edit → Project Settings → Tags and Layers |
| **"Tag 'Player' is not defined"** | The `Player` tag is built-in; make sure your Player object uses it |
| **Bullets pass through enemies** | Ensure both have `Collider2D` (isTrigger=true) and at least one has `Rigidbody2D` |
| **Nothing appears on screen** | Check that `GameSceneSetup` component is on a GameObject in the scene |
| **Scenes don't load** | Verify both scenes are added in File → Build Settings with correct order |
| **UI not clickable** | Ensure an `EventSystem` exists in the scene (created automatically by UISetupHelper) |
| **Enemies don't take damage** | Make sure enemy GameObjects have the `Enemy` tag |

---

## 📝 Architecture Notes

### Design Decisions
- **Runtime object creation:** The `GameSceneSetup` script creates all game objects (player, enemies, bullets, UI) at runtime with procedurally generated sprites. This means scenes only need a single empty GameObject to bootstrap the entire game.
- **Singleton pattern:** `GameManager`, `AudioManager`, `HUDManager`, `EnemySpawner`, `GameOverUI`, `PauseMenuUI` use singletons for easy cross-script access.
- **Prefab-ready:** All runtime-created objects can be replaced with proper prefabs by assigning them in the `GameSceneSetup` inspector slots.
- **Modular enemies:** `EnemyBase` provides shared logic; each enemy type only overrides what's unique (movement pattern, stats).

### Extending the Game
- **New enemy types:** Create a new class inheriting from `EnemyBase`, override `Start()` for stats and `Move()` for behavior
- **New power-ups:** Add a new enum value to `PowerUp.PowerUpType`, add handling in `PlayerController`
- **Boss enemies:** Create a `BossEnemy` class, spawn it at specific wave milestones in `EnemySpawner`
- **Weapon upgrades:** Add spread-shot logic in `PlayerController.Shoot()` based on a power-up level

---

## 📄 License

This project is provided as-is for educational purposes. Feel free to modify and use it for your own games!
