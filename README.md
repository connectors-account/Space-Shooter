# 🚀 Space Shooter — Unity 2D Game

A fully functional space-shooter game built with Unity and C#. Features procedurally generated sprites (no external art assets), wave-based enemy spawning, power-ups, a scoring system, and a complete UI with menus.

---

## 🎮 Game Features

| Feature | Details |
|---------|---------|
| **Player Ship** | Triangle sprite, 8-directional movement, multi-level weapons (up to 3 spread shots) |
| **Enemies** | 4 types — Straight, Zigzag, Sine-wave, Dive-bomber — with scaling difficulty |
| **Shooting** | Hold SPACE for continuous fire; bullets auto-destroy off-screen |
| **Power-Ups** | Health restore (green) and Weapon upgrade (yellow) drop from enemies |
| **Waves** | Infinite wave progression with increasing enemy count, speed, and health |
| **Scoring** | Points per kill scaling with wave; persistent high score via PlayerPrefs |
| **UI** | Start menu, HUD (score/health/wave), wave announcements, game-over screen |
| **Audio** | Procedurally generated sound effects (no audio files needed) |
| **Background** | Parallax scrolling starfield |
| **Invincibility** | Brief invincibility + blink effect after taking damage |

---

## 🎯 Controls

| Key | Action |
|-----|--------|
| **Arrow Keys** / **WASD** | Move player ship |
| **SPACE** | Shoot |
| **Mouse** | Click UI buttons (menus) |

---

## 📁 Project Structure

```
space_shooter_unity/
├── Assets/
│   ├── Scripts/
│   │   ├── SceneBootstrapper.cs    ← Auto-builds entire scene at runtime
│   │   ├── PlayerController.cs     ← Movement, shooting, health, invincibility
│   │   ├── EnemyController.cs      ← 4 movement patterns, health, shooting, drops
│   │   ├── BulletController.cs     ← Movement, collision, damage
│   │   ├── GameManager.cs          ← Wave spawning, scoring, game state
│   │   ├── UIManager.cs            ← All UI: menus, HUD, game over
│   │   ├── MenuManager.cs          ← Scene-level menu/restart
│   │   ├── PowerUpController.cs    ← Health/weapon pickups
│   │   ├── AudioManager.cs         ← Procedural sound effects
│   │   ├── BackgroundScroller.cs   ← Parallax starfield
│   │   └── ExplosionEffect.cs      ← Visual explosion on enemy death
│   ├── Scenes/
│   │   └── GameScene.unity         ← Main game scene
│   ├── Prefabs/                    ← (Created at runtime by bootstrapper)
│   ├── Materials/
│   └── Resources/
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset            ← Tags: Enemy, Bullet, PowerUp
│   ├── Physics2DSettings.asset     ← Zero gravity for 2D space
│   ├── EditorBuildSettings.asset
│   ├── InputManager.asset          ← WASD/Arrow + Space controls
│   ├── QualitySettings.asset
│   ├── AudioManager.asset
│   └── TimeManager.asset
├── Packages/
│   └── manifest.json
├── .gitignore
└── README.md
```

---

## 🛠️ Step-by-Step Setup Instructions

### Prerequisites
- **Unity Hub** installed ([download here](https://unity.com/download))
- **Unity Editor 2021.3 LTS or newer** (2022.3 LTS recommended)
  - Must include **Windows Build Support (Mono)** module

---

### Step 1: Open the Project in Unity

1. **Launch Unity Hub**
2. Click **"Open"** (or **"Add"** in older Hub versions)
3. **Browse** to the `space_shooter_unity` folder and select it
4. Unity Hub will detect it as a Unity project
5. Select your Unity version (2021.3+ or 2022.3+ recommended)
6. Click **"Open"** — Unity will import the project (first time may take 2-5 minutes)

> ⚠️ If Unity warns about a version mismatch, click **"Continue"** — the project is compatible with 2021.3+

---

### Step 2: Set Up the Scene

Since the project uses a **runtime bootstrapper** that auto-creates everything, minimal manual setup is needed. However, you must ensure the **Tags** are configured and the scene is loaded:

#### 2a. Verify Tags (IMPORTANT — do this first!)

1. Go to **Edit → Project Settings → Tags and Layers**
2. Expand the **Tags** section
3. Ensure these tags exist (add them if missing):
   - `Enemy`
   - `Bullet`
   - `PowerUp`
4. The `Player` tag is built-in and should already exist

#### 2b. Open the Game Scene

1. In the **Project** window, navigate to `Assets/Scenes/`
2. Double-click **`GameScene`** to open it
3. You should see:
   - **Main Camera** — already configured as orthographic
   - **Bootstrapper** — empty GameObject with `SceneBootstrapper` script

#### 2c. Attach the Bootstrapper Script (if not already attached)

The scene file includes the Bootstrapper object, but if the script reference is broken:

1. Select the **Bootstrapper** GameObject in the Hierarchy
2. In the Inspector, click **"Add Component"**
3. Search for **"SceneBootstrapper"** and add it
4. That's it! The bootstrapper creates everything else at runtime:
   - Player ship with controls
   - All enemy prefabs (4 types)
   - Bullet prefabs
   - Power-up prefabs
   - Full UI (menus, HUD, game over)
   - Background starfield
   - Audio manager

#### 2d. Verify the Scene is in Build Settings

1. Go to **File → Build Settings**
2. If `GameScene` is not listed, click **"Add Open Scenes"**
3. Ensure **GameScene** is checked and at index 0

---

### Step 3: Test in the Editor

1. Click the **▶ Play** button at the top of the Unity Editor
2. The main menu should appear with **"SPACE SHOOTER"** title
3. Click **"START GAME"**
4. Use Arrow Keys/WASD to move, SPACE to shoot
5. Enemies will spawn in waves with increasing difficulty
6. Collect green (health) and yellow (weapon) power-ups
7. When health reaches 0, the Game Over screen appears
8. Click **"PLAY AGAIN"** to restart

---

### Step 4: Build as a Windows Desktop Executable

1. Go to **File → Build Settings**
2. Select **"PC, Mac & Linux Standalone"** as the platform
3. Set **Target Platform** to **Windows**
4. Set **Architecture** to **x86_64**
5. Click **"Player Settings..."** to customize:
   - **Product Name**: Space Shooter
   - **Default Screen Width**: 1024
   - **Default Screen Height**: 768
   - **Fullscreen Mode**: Windowed (recommended for testing)
   - **Run In Background**: ✓ Checked
6. Click **"Build"** (or **"Build and Run"**)
7. Choose a destination folder (e.g., `Builds/Windows/`)
8. Wait for the build to complete
9. Run **`Space Shooter.exe`** from the build folder

---

## 🔧 Customization Guide

### Difficulty Tuning
Edit values in `GameManager.cs`:
```csharp
public float timeBetweenWaves = 3f;      // Seconds between waves
public int baseEnemiesPerWave = 5;        // Starting enemy count
public float spawnInterval = 1f;          // Seconds between spawns
```

### Player Settings
Edit values in `PlayerController.cs`:
```csharp
public float moveSpeed = 8f;              // Ship movement speed
public float fireRate = 0.2f;             // Seconds between shots
public int maxHealth = 5;                 // Starting health
```

### Enemy Behavior
Edit `EnemyController.cs` or the values set in `SceneBootstrapper.cs`:
- Change movement patterns (Straight, Zigzag, Sine, Dive)
- Adjust health, speed, score values
- Toggle enemy shooting ability
- Modify power-up drop chance

### Adding New Enemy Types
1. Create a new entry in `SceneBootstrapper.CreatePrefabs()`
2. Choose color, pattern, and stats
3. Add to the `enemyPrefabs` array

---

## 🏗️ Architecture Notes

### Runtime Bootstrapper Pattern
This project uses a **SceneBootstrapper** that creates all GameObjects, prefabs, and UI at runtime. This approach:
- Eliminates complex `.prefab` and `.scene` serialization issues
- Makes the project portable across Unity versions
- All sprites are procedurally generated (triangle for player, diamond for enemies, square for bullets/effects)
- No external art assets required

### Singleton Managers
- `GameManager.Instance` — Central game state, spawning, scoring
- `UIManager.Instance` — All UI management
- `AudioManager.Instance` — Procedural sound effects

### Tag System
Objects communicate through Unity's tag system:
- `Player` — Player ship
- `Enemy` — All enemy types
- `Bullet` — Player and enemy bullets
- `PowerUp` — Collectible power-ups

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| **"Tag not found" errors** | Add tags manually: Edit → Project Settings → Tags and Layers → Add `Enemy`, `Bullet`, `PowerUp` |
| **Script not attached to Bootstrapper** | Select Bootstrapper in Hierarchy → Add Component → SceneBootstrapper |
| **No text visible in UI** | Ensure `com.unity.ugui` package is installed (check Packages/manifest.json) |
| **Build fails** | Ensure Windows Build Support module is installed in Unity Hub → Installs → your version → Add Modules |
| **Scene is empty when playing** | Make sure the scene has the Bootstrapper object with SceneBootstrapper attached |
| **Enemies don't collide** | Check that Physics2D trigger collisions are enabled in Project Settings |

---

## 📋 Requirements

- Unity 2021.3 LTS or newer (2022.3 LTS recommended)
- Windows Build Support (Mono) module
- No additional packages or assets needed — everything is self-contained

---

## 📄 License

This project is provided as-is for educational and personal use.
