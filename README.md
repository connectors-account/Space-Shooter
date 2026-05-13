# ★ STAR BLASTER — Space Shooter Game

A complete arcade-style vertical scrolling space shooter built with Unity (C#).  
Retro pixel-art aesthetic, progressive wave difficulty, combo scoring, power-ups, and full menu system.

---

## Table of Contents

1. [Game Features](#game-features)
2. [Project Structure](#project-structure)
3. [Requirements](#requirements)
4. [Quick Start — One-Click Setup](#quick-start--one-click-setup)
5. [Manual Setup (Alternative)](#manual-setup-alternative)
6. [Building a Windows Executable](#building-a-windows-executable)
7. [Controls](#controls)
8. [Game Systems Reference](#game-systems-reference)
9. [Adding Custom Audio](#adding-custom-audio)
10. [Customization Guide](#customization-guide)
11. [Troubleshooting](#troubleshooting)

---

## Game Features

| Feature | Details |
|---------|---------|
| **Player Ship** | WASD / Arrow key movement, Space / LMB to shoot |
| **4 Enemy Types** | Straight, Zigzag, Tracker (aims at player), Tank (high HP + shoots) |
| **Wave System** | Progressive difficulty — more enemies, faster spawns, tougher types |
| **Combo Scoring** | Kill streaks multiply points (up to ×8), resets on damage |
| **Power-Ups** | Health, Shield, Rapid Fire, Spread Shot — dropped by enemies |
| **Player Lives** | 3 lives with invincibility frames on respawn |
| **Parallax BG** | Scrolling starfield + tiled background layers |
| **Full UI** | Health bar, score, wave counter, combo display, lives |
| **Menu Screens** | Main Menu → Gameplay → Pause (Esc) → Game Over |
| **High Score** | Persistent across sessions (PlayerPrefs) |
| **Object Pooling** | Zero-allocation bullet and enemy recycling |

---

## Project Structure

```
space_shooter_unity/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs        ← Central game state, scoring, combo
│   │   │   ├── ObjectPool.cs         ← Generic object pooling
│   │   │   └── GameBounds.cs         ← Screen boundary calculations
│   │   ├── Player/
│   │   │   ├── PlayerController.cs   ← Movement (WASD / Arrows)
│   │   │   ├── PlayerHealth.cs       ← HP, lives, shield, invincibility
│   │   │   └── PlayerShooting.cs     ← Fire modes, power-up timers
│   │   ├── Enemies/
│   │   │   ├── EnemyBase.cs          ← Abstract base + PowerUpSpawner
│   │   │   ├── EnemyStraight.cs      ← Flies straight down
│   │   │   ├── EnemyZigzag.cs        ← Sine-wave lateral movement
│   │   │   ├── EnemyTracker.cs       ← Homes in on player X position
│   │   │   ├── EnemyTank.cs          ← Slow, tough, shoots back
│   │   │   └── EnemySpawner.cs       ← Wave management & difficulty curve
│   │   ├── Combat/
│   │   │   ├── Bullet.cs             ← Universal bullet (player & enemy)
│   │   │   └── ContactDamage.cs      ← Enemy collision with player
│   │   ├── PowerUps/
│   │   │   └── PowerUp.cs            ← Pickup behaviour & effect application
│   │   ├── UI/
│   │   │   ├── HUDManager.cs         ← In-game HUD bindings
│   │   │   ├── MainMenuUI.cs         ← Start / Quit screen
│   │   │   ├── PauseMenuUI.cs        ← Pause overlay
│   │   │   └── GameOverUI.cs         ← Score display & restart
│   │   ├── Background/
│   │   │   ├── ParallaxBackground.cs ← Infinite tiling scroll
│   │   │   └── StarFieldGenerator.cs ← Procedural particle stars
│   │   ├── Audio/
│   │   │   └── AudioManager.cs       ← Music & SFX playback
│   │   └── Editor/
│   │       ├── SpriteGenerator.cs    ← Generates all pixel-art sprites
│   │       └── SceneSetupWizard.cs   ← One-click full game setup
│   ├── Scenes/        (created by setup wizard)
│   ├── Prefabs/       (created by setup wizard)
│   ├── Sprites/       (created by sprite generator)
│   ├── Audio/Music/   (place your .ogg/.wav music here)
│   ├── Audio/SFX/     (place your .ogg/.wav sound effects here)
│   └── UI/
├── ProjectSettings/
└── README.md
```

---

## Requirements

| Tool | Version |
|------|---------|
| **Unity** | 2021.3 LTS or newer (2022.3 LTS also works) |
| **TextMeshPro** | Included with Unity — import the TMP Essentials when prompted |
| **Platform** | Windows 10/11 (build target) |

> **No third-party packages required.** Everything uses built-in Unity features.

---

## Quick Start — One-Click Setup

This is the **recommended** approach. The editor wizard automatically generates sprites, creates prefabs, builds both scenes, configures physics layers, and sets up build settings.

### Step 1: Create a New Unity Project

1. Open **Unity Hub**
2. Click **New Project**
3. Select **2D (Core)** template
4. Name it `StarBlaster` (or anything you like)
5. Click **Create project**

### Step 2: Import the Scripts

1. In your new Unity project, navigate to the `Assets/` folder in your file explorer
2. **Copy the entire contents** of this repository's `Assets/Scripts/` folder into your project's `Assets/Scripts/`
3. Wait for Unity to compile (watch the bottom status bar)

> **Important:** When Unity prompts you to import **TextMeshPro Essentials**, click **Import TMP Essentials**. This is required for all UI text.

### Step 3: Run the Setup Wizard

1. In Unity's menu bar, click **Tools → Space Shooter → Setup Complete Game**
2. Wait ~10 seconds for the wizard to finish
3. You'll see a green log: `✅ Complete game setup finished!`

### Step 4: Play!

1. Open **File → Build Settings**
2. Verify both scenes are listed (MainMenu at index 0, Gameplay at index 1)
3. Open **Assets/Scenes/MainMenu** in the Project window
4. Press the **▶ Play** button
5. Click **START GAME** and enjoy!

---

## Manual Setup (Alternative)

If you prefer to set things up by hand instead of using the wizard:

### Tags & Layers

**Tags:** `Player`, `Enemy`, `PlayerBullet`, `EnemyBullet`, `PowerUp`

**Physics Layers:**
| Layer # | Name |
|---------|------|
| 6 | Player |
| 7 | Enemy |
| 8 | PlayerBullet |
| 9 | EnemyBullet |
| 10 | PowerUp |

**Layer Collision Matrix** (Edit → Project Settings → Physics 2D):
- Disable: Player↔PlayerBullet, Enemy↔EnemyBullet, PlayerBullet↔EnemyBullet, same-type bullets

**Sorting Layers** (in order): `Background`, `Gameplay`, `Projectiles`, `UI`

### Generate Sprites

Menu: **Tools → Space Shooter → Generate All Sprites**

### Create Prefabs

For each entity (PlayerBullet, EnemyBullet, 4 enemies, 4 power-ups):
1. Create a GameObject with SpriteRenderer + BoxCollider2D (trigger) + Rigidbody2D (kinematic)
2. Attach the appropriate script (Bullet.cs, EnemyStraight.cs, etc.)
3. Configure Inspector values as documented in each script's header comments
4. Drag to `Assets/Prefabs/` to create prefab

### Scene: MainMenu (Build Index 0)

- Orthographic camera, size 5.4, dark background
- GameManager object (persists via DontDestroyOnLoad)
- AudioManager object (persists)
- UI Canvas with: Title text, High Score text, Start button, Quit button
- Attach MainMenuUI.cs to the canvas; wire references

### Scene: Gameplay (Build Index 1)

- Orthographic camera, size 5.4
- GameBounds object
- Player ship at (0, -3.5) with PlayerController, PlayerHealth, PlayerShooting
- ObjectPool with all pool entries configured
- EnemySpawner with enemy weight table
- PowerUpSpawner with prefab references
- ParallaxBG + StarField for background
- HUD Canvas, Pause Canvas, GameOver Canvas

---

## Building a Windows Executable

### From Unity Editor

1. **File → Build Settings**
2. Select **PC, Mac & Linux Standalone**
3. Set **Target Platform** to **Windows**
4. Set **Architecture** to **x86_64** (recommended)
5. Click **Player Settings** and verify:
   - Company Name: `IndieStudio` (or your name)
   - Product Name: `Star Blaster`
   - Default Screen Width: `1920`
   - Default Screen Height: `1080`
   - Fullscreen Mode: `Windowed` (or Fullscreen)
6. Click **Build**
7. Choose an output folder (e.g., `Build/`)
8. Wait for the build to complete

### Output

```
Build/
├── StarBlaster.exe            ← Run this!
├── StarBlaster_Data/          ← Game data (must ship with .exe)
├── UnityCrashHandler64.exe
└── UnityPlayer.dll
```

### Distributing

To share the game, zip the entire `Build/` folder. The recipient just runs `StarBlaster.exe` — no Unity installation needed.

### Command-Line Build (CI/CD)

```bash
# Ensure Unity is in your PATH (adjust path for your Unity version)
"C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe" \
  -batchmode \
  -nographics \
  -projectPath "C:\path\to\StarBlaster" \
  -buildWindows64Player "C:\path\to\Build\StarBlaster.exe" \
  -quit
```

---

## Controls

| Action | Keys |
|--------|------|
| Move | **WASD** or **Arrow Keys** |
| Shoot | **Space** or **Left Mouse Button** |
| Pause | **Escape** |

---

## Game Systems Reference

### Scoring & Combo
- Each enemy kill awards base points × current combo multiplier
- Combo increments on each kill (max ×8) and resets after 2 seconds of no kills
- Taking damage immediately resets the combo to ×1
- High score is saved to PlayerPrefs and persists between sessions

### Wave Progression
- Each wave spawns `6 + (wave-1) × 2` enemies
- Spawn interval decreases by 0.05s per wave (min 0.4s)
- New enemy types unlock at specific waves:
  - Wave 1: Straight enemies
  - Wave 2: + Zigzag enemies
  - Wave 3: + Tracker enemies
  - Wave 4: + Tank enemies

### Power-Ups
| Power-Up | Effect | Duration |
|----------|--------|----------|
| Health (green +) | Restores 30 HP | Instant |
| Shield (blue ring) | Absorbs all damage | 5 seconds |
| Rapid Fire (yellow bolt) | 2.5× fire rate | 5 seconds |
| Spread Shot (white arrows) | 3-bullet fan pattern | 5 seconds |

Power-ups have a 15% drop chance from destroyed enemies.

### Object Pooling
All bullets and enemies use pre-allocated pools (configurable size in ObjectPool Inspector). Pools auto-grow if exhausted — zero `Instantiate`/`Destroy` calls during normal gameplay.

---

## Adding Custom Audio

1. Place audio files in `Assets/Audio/Music/` and `Assets/Audio/SFX/`
2. Select the **AudioManager** GameObject (on GameManager in the scene hierarchy)
3. Assign clips to the `Menu Music` and `Gameplay Music` fields
4. Add SFX entries to the `Sounds` list using these keys:

| Key | When Played |
|-----|-------------|
| `PlayerShoot` | Player fires a bullet |
| `PlayerHit` | Player takes damage |
| `Explosion` | Enemy destroyed / player dies |
| `EnemyShoot` | Enemy fires a bullet |
| `PowerUp` | Any power-up collected |

> The game runs without audio — all AudioManager calls safely no-op when clips are null.

---

## Customization Guide

### Adjusting Difficulty
Edit the **EnemySpawner** Inspector values:
- `Initial Spawn Interval` — time between spawns at wave 1
- `Minimum Spawn Interval` — fastest possible spawn rate
- `Enemies Per Wave Base/Growth` — how many enemies per wave

### Adding New Enemy Types
1. Create a new class inheriting from `EnemyBase`
2. Override the `Move()` method
3. Create a prefab with the script + collider + sprite
4. Add a pool entry in ObjectPool
5. Add an EnemyWeight entry in EnemySpawner

### Changing Player Stats
Select the Player ship in the Gameplay scene:
- `PlayerController` → Move Speed
- `PlayerHealth` → Max Health, Starting Lives, Invincibility Duration
- `PlayerShooting` → Fire Rate, Rapid Fire Duration, Spread Angle

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| **TMP text not showing** | Window → TextMeshPro → Import TMP Essential Resources |
| **Bullets pass through enemies** | Ensure colliders are set to `Is Trigger = true` and both objects have Rigidbody2D |
| **Enemies don't spawn** | Check ObjectPool has entries with matching tags; check EnemySpawner enemyTypes array |
| **Player can't move** | Verify Input axes "Horizontal"/"Vertical" exist in Edit → Project Settings → Input Manager |
| **Sprites invisible** | Run Tools → Space Shooter → Generate All Sprites, then check sorting layers |
| **Build errors with Editor scripts** | Ensure SpriteGenerator.cs and SceneSetupWizard.cs are in an `Editor/` folder |

---

## Technical Notes

- **Frame Independence**: All movement uses `Time.deltaTime` (Update) or physics (FixedUpdate)
- **Singleton Pattern**: GameManager, ObjectPool, AudioManager, GameBounds use lightweight singletons
- **Event-Driven UI**: HUD subscribes to C# events — no polling in Update loops
- **Clean Architecture**: Namespaces separate concerns (Core, Player, Enemies, Combat, PowerUps, UI, Audio, Background)
- **Target**: Unity 2021.3 LTS, .NET Standard 2.1, IL2CPP build recommended for release

---

## License

Free to use, modify, and distribute. No attribution required.
