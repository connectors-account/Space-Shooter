# Space Shooter — Complete Build Instructions

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Project Setup](#project-setup)
3. [Generate Sprites & Assets](#generate-sprites--assets)
4. [Automated Scene & Prefab Setup](#automated-scene--prefab-setup)
5. [Manual Setup (Alternative)](#manual-setup-alternative)
6. [Configure Tags](#configure-tags)
7. [Wire Up References](#wire-up-references)
8. [Testing in Editor](#testing-in-editor)
9. [Build for Windows](#build-for-windows)
10. [Troubleshooting](#troubleshooting)

---

## Prerequisites

| Requirement | Version |
|---|---|
| **Unity Editor** | 2021.3 LTS or newer (2022.3 LTS recommended) |
| **Build Support** | Windows Build Support (IL2CPP or Mono) |
| **OS** | Windows 10/11 (for building .exe) |
| **.NET** | .NET Framework 4.x or .NET Standard 2.1 |

### Install Unity
1. Download **Unity Hub** from https://unity.com/download
2. In Unity Hub, install **Unity 2022.3 LTS** (or latest LTS)
3. During install, ensure **Windows Build Support (Mono)** is checked
4. Also ensure **2D Template** packages are included

---

## Project Setup

### Option A: Open This Project Directly
1. Open **Unity Hub**
2. Click **Open** → **Add project from disk**
3. Navigate to this `space_shooter_game` folder and select it
4. Unity will import all scripts and create the necessary `.meta` files
5. Wait for compilation to complete (check bottom status bar)

### Option B: Create Fresh Project and Import
1. In Unity Hub, click **New Project**
2. Select the **2D (Built-in Render Pipeline)** template
3. Name it `SpaceShooter` and choose a location
4. Once the empty project opens, close it
5. **Copy these folders** from this project into your new project:
   - `Assets/Scripts/` → entire folder
   - `Assets/Sprites/` → entire folder (if pre-generated)
   - `ProjectSettings/TagManager.asset` → replace existing
   - `ProjectSettings/InputManager.asset` → replace existing
   - `ProjectSettings/Physics2DSettings.asset` → replace existing
6. Reopen the project in Unity

---

## Generate Sprites & Assets

After the project compiles successfully:

1. In Unity, go to the top menu: **Tools → Space Shooter → Generate All Sprites**
2. This will procedurally create all game sprites as PNG files:
   - `Assets/Sprites/Player/player_ship.png`
   - `Assets/Sprites/Enemies/basic_enemy.png`, `fast_enemy.png`, `tank_enemy.png`, `boss_enemy.png`
   - `Assets/Sprites/Bullets/player_bullet.png`, `enemy_bullet.png`, `boss_bullet.png`
   - `Assets/Sprites/PowerUps/powerup_health.png`, `powerup_shield.png`, etc.
   - `Assets/Sprites/Effects/explosion.png`, `shield.png`
   - `Assets/Sprites/Backgrounds/bg_far.png`, `bg_mid.png`, `bg_near.png`
   - `Assets/Sprites/UI/life_icon.png`, `health_fill.png`, `button_bg.png`
3. Wait for Unity to reimport assets (progress bar at bottom)

### Configure Sprite Import Settings
After generation, select all sprites in the Project window and verify:
- **Texture Type**: Sprite (2D and UI)
- **Pixels Per Unit**: 64
- **Filter Mode**: Point (no filter) — for pixel-art look
- **Compression**: None

---

## Automated Scene & Prefab Setup

The **Game Setup Wizard** automates everything:

1. Go to menu: **Tools → Space Shooter → Full Game Setup**
2. Click **"Yes, Set Up Everything"** in the confirmation dialog
3. The wizard will automatically:
   - ✅ Generate all sprites (if not already done)
   - ✅ Create all prefabs with correct components
   - ✅ Build the **MainMenu** scene with UI
   - ✅ Build the **GamePlay** scene with player, spawner, HUD
   - ✅ Build the **GameOver** scene with UI
   - ✅ Configure Build Settings with all 3 scenes
4. Review the console log for any warnings

---

## Manual Setup (Alternative)

If the wizard has issues, follow these manual steps:

### Create Tags
1. **Edit → Project Settings → Tags and Layers**
2. Add these tags (some may already exist):
   - `Player`
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`
   - `PowerUp`

### Create Prefabs Manually

#### Player Prefab
1. Create empty GameObject, name it "Player"
2. Add components:
   - **SpriteRenderer** → assign `player_ship` sprite
   - **BoxCollider2D** → Is Trigger ✓, Size (0.5, 0.6)
   - **Rigidbody2D** → Gravity Scale: 0, Freeze Rotation ✓
   - **HealthSystem** script → Max Health: 5
   - **PlayerController** script
3. Set tag to **Player**
4. Create child "FirePoint" at position (0, 0.5, 0)
5. On PlayerController: assign bullet prefab and fire point
6. Drag to `Assets/Prefabs/Player/`

#### Enemy Prefabs
Repeat for each enemy type (Basic, Fast, Tank, Boss):
1. Create empty GameObject with enemy name
2. Add: SpriteRenderer, BoxCollider2D (trigger), Rigidbody2D (no gravity)
3. Add the appropriate enemy script (BasicEnemy, FastEnemy, etc.)
4. Add **BulletPattern** component, assign enemy bullet prefab
5. Set tag to **Enemy**
6. Drag to `Assets/Prefabs/Enemies/`

**Recommended values:**

| Enemy | Health | Speed | Score | Fire Rate |
|-------|--------|-------|-------|-----------|
| Basic | 3 | 3.0 | 100 | 1.5s |
| Fast | 2 | 5.0 | 150 | 2.0s |
| Tank | 8 | 1.5 | 300 | 1.0s |
| Boss | 50 | 2.0 | 5000 | 0.8s |

#### Bullet Prefabs
For each bullet type:
1. Create GameObject with SpriteRenderer and appropriate bullet sprite
2. Add BoxCollider2D (trigger) and Rigidbody2D (no gravity)
3. Add **Bullet** script
4. Set tag to `PlayerBullet` or `EnemyBullet`
5. Save as prefab

#### Power-Up Prefabs
For each power-up type (Health, Shield, RapidFire, SpreadShot, ExtraLife, ScoreBonus):
1. Create GameObject with SpriteRenderer and matching sprite
2. Add CircleCollider2D (trigger, radius 0.3) and Rigidbody2D (no gravity)
3. Add **PowerUp** script, set the Type enum
4. Set tag to `PowerUp`
5. Save as prefab

### Create Scenes

#### MainMenu Scene
1. **File → New Scene** (Basic 2D)
2. Set Camera background to dark blue (0.02, 0.02, 0.08)
3. Add **StarfieldGenerator** to an empty GameObject
4. Create Canvas (Screen Space - Overlay):
   - Title Text: "SPACE SHOOTER" (font size 48, cyan)
   - High Score Text
   - Start Button
   - Quit Button
   - Music/SFX volume sliders
5. Add **MainMenuController** script to a GameObject
6. Wire button onClick events to the controller
7. Add GameManager, SoundManager, InputHandler GameObjects
8. **Save as** `Assets/Scenes/MainMenu.unity`

#### GamePlay Scene
1. **File → New Scene** (Basic 2D)
2. Camera: orthographic, size 5, dark background
3. Add background layers with **ParallaxBackground**
4. Add **StarfieldGenerator**
5. Place **Player** prefab at (0, -3.5, 0)
6. Add **EnemySpawner** GameObject:
   - Assign enemy prefabs to the serialized fields
7. Create HUD Canvas with:
   - Score text (top-left)
   - Lives text + icons
   - Health bar (Slider)
   - Wave announcement text (center, CanvasGroup for fade)
   - Pause panel (hidden by default)
8. Add **HUDController** and wire all UI references
9. **Save as** `Assets/Scenes/GamePlay.unity`

#### GameOver Scene
1. Similar to MainMenu but with:
   - "GAME OVER" title in red
   - Final score and high score text
   - "NEW HIGH SCORE!" conditional text
   - Restart, Main Menu, and Quit buttons
2. Add **GameOverController** and wire references
3. **Save as** `Assets/Scenes/GameOver.unity`

### Wire Up References

#### GameManager (on MainMenu scene)
- Assign all 6 power-up prefabs to the `powerUpPrefabs` array

#### PlayerController
- Bullet Prefab → PlayerBullet prefab
- Fire Point → child FirePoint transform
- Shield Visual → ShieldVisual prefab (or child object)
- Explosion Prefab → Explosion prefab

#### Enemy Scripts
- Each enemy's BulletPattern → assign EnemyBullet prefab (or BossBullet for boss)
- Explosion Prefab → Explosion prefab

#### EnemySpawner
- Basic/Fast/Tank/Boss enemy prefab references

#### HUDController
- Wire all Text, Slider, Image, and panel references

#### MainMenuController
- Wire button and slider references

#### GameOverController
- Wire all text and button references

---

## Configure Tags

Ensure these tags exist (**Edit → Project Settings → Tags and Layers**):

| Tag | Used By |
|---|---|
| `Player` | Player ship |
| `Enemy` | All enemy ships |
| `PlayerBullet` | Player's projectiles |
| `EnemyBullet` | Enemy projectiles |
| `PowerUp` | All power-up pickups |

---

## Testing in Editor

1. Open `Assets/Scenes/MainMenu.unity`
2. Press **Play** ▶️
3. Click **START GAME**
4. **Controls:**
   - **WASD** or **Arrow Keys** — Move
   - **Space** or **Left Mouse** — Fire (also auto-fires)
   - **Escape** — Pause/Resume
5. Verify:
   - ✅ Player moves within screen bounds
   - ✅ Bullets fire and destroy enemies
   - ✅ Enemies spawn in waves with increasing difficulty
   - ✅ Score updates on enemy kills
   - ✅ Power-ups drop and apply effects
   - ✅ Health bar updates on damage
   - ✅ Lives decrement on death with respawn
   - ✅ Game over triggers at 0 lives
   - ✅ Pause menu works
   - ✅ Scene transitions work (menu → game → game over → menu)

---

## Build for Windows

### Configure Build Settings
1. **File → Build Settings** (Ctrl+Shift+B)
2. Verify these scenes are listed in order:
   - `Scenes/MainMenu` — Index 0
   - `Scenes/GamePlay` — Index 1
   - `Scenes/GameOver` — Index 2
3. If missing, click **Add Open Scenes** for each
4. Set **Platform** to **Windows, Mac, Linux** (PC)
5. Click **Switch Platform** if needed

### Configure Player Settings
1. Click **Player Settings** (bottom-left of Build Settings)
2. Set:
   - **Company Name**: Your name/studio
   - **Product Name**: Space Shooter
   - **Default Icon**: (optional, can use player ship sprite)
   - **Resolution**:
     - Default Width: 800
     - Default Height: 600
     - Fullscreen Mode: Windowed (or Fullscreen Window)
     - Resizable Window: ✓
   - **Splash Screen**: Customize or disable (requires Unity Pro)
3. Under **Other Settings**:
   - **Scripting Backend**: Mono (faster builds) or IL2CPP (better performance)
   - **Api Compatibility Level**: .NET Framework or .NET Standard 2.1

### Build
1. In Build Settings, click **Build**
2. Choose an output folder (e.g., `Builds/Windows/`)
3. Name the executable: `SpaceShooter.exe`
4. Click **Save**
5. Wait for the build to complete
6. The output folder will contain:
   ```
   Builds/Windows/
   ├── SpaceShooter.exe              ← Run this!
   ├── SpaceShooter_Data/            ← Game data (required)
   ├── UnityCrashHandler64.exe       ← Crash handler
   └── UnityPlayer.dll               ← Unity runtime
   ```

### Build & Run
- Alternatively, click **Build And Run** to build and launch immediately

### Distribution
To distribute the game, **zip the entire output folder** including:
- The `.exe` file
- The `_Data` folder
- All `.dll` files
All files must stay together in the same directory.

---

## Troubleshooting

### Scripts Won't Compile
- Ensure you're using Unity 2021.3+ with 2D packages
- Check **Window → Console** for exact error messages
- Verify all script files are in the correct folders under `Assets/Scripts/`
- Ensure namespace references match (SpaceShooter.Player, SpaceShooter.Enemy, etc.)

### Tags Not Found
- Open **Edit → Project Settings → Tags and Layers**
- Add missing tags: `Player`, `Enemy`, `PlayerBullet`, `EnemyBullet`, `PowerUp`

### Missing References on Prefabs
- Select each prefab and check the Inspector for yellow warning icons
- Re-assign any missing sprite, prefab, or component references
- Use the Game Setup Wizard to recreate prefabs if needed

### No Enemies Spawning
- Verify EnemySpawner has enemy prefab references assigned
- Check that enemy prefabs have the "Enemy" tag
- Ensure GameManager.StartGame() is being called (check main menu button)

### Player Not Taking Damage
- Verify enemy bullets have tag "EnemyBullet"
- Check that colliders are set to "Is Trigger"
- Verify both objects have Rigidbody2D components

### No Sound
- SoundManager needs AudioClip references assigned
- The game works without sound — audio is optional
- You can add .wav or .ogg files to `Assets/Audio/SFX/` and assign them

### Build Fails
- Check Console for build errors
- Ensure all scenes are added to Build Settings
- Try **Clean Build** (delete the output folder first)
- Switch scripting backend to Mono if IL2CPP fails

---

## Project Structure Reference

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerController.cs      — Player movement, shooting, power-ups
│   │   │   └── HealthSystem.cs          — Reusable HP component
│   │   ├── Enemy/
│   │   │   ├── EnemyBase.cs             — Base enemy class
│   │   │   ├── BasicEnemy.cs            — Straight-moving enemy
│   │   │   ├── FastEnemy.cs             — Zigzag movement enemy
│   │   │   ├── TankEnemy.cs             — Heavy, slow enemy
│   │   │   ├── BossEnemy.cs             — Multi-phase boss
│   │   │   └── EnemySpawner.cs          — Wave-based spawn system
│   │   ├── Weapons/
│   │   │   ├── Bullet.cs                — Basic projectile
│   │   │   ├── HomingBullet.cs          — Target-tracking projectile
│   │   │   └── BulletPattern.cs         — Pattern firing (spread, circle, aimed)
│   │   ├── PowerUps/
│   │   │   └── PowerUp.cs              — All power-up types
│   │   ├── Managers/
│   │   │   ├── GameManager.cs           — State, score, lives, scenes
│   │   │   ├── SoundManager.cs          — Audio playback
│   │   │   ├── InputHandler.cs          — Centralized input
│   │   │   └── ScoreManager.cs          — Score + combo system
│   │   ├── UI/
│   │   │   ├── HUDController.cs         — In-game HUD
│   │   │   ├── MainMenuController.cs    — Main menu logic
│   │   │   └── GameOverController.cs    — Game over screen
│   │   ├── Effects/
│   │   │   ├── ParallaxBackground.cs    — Scrolling backgrounds
│   │   │   ├── ExplosionEffect.cs       — Explosion animation
│   │   │   └── StarfieldGenerator.cs    — Particle starfield
│   │   ├── Utils/
│   │   │   ├── ObjectPooler.cs          — Object pooling
│   │   │   ├── AutoDestroy.cs           — Timed self-destruct
│   │   │   └── ScreenWrapper.cs         — Screen edge wrapping
│   │   └── Editor/
│   │       ├── ProceduralSpriteGenerator.cs — Sprite creation tool
│   │       └── GameSetupWizard.cs           — One-click setup wizard
│   ├── Sprites/                         — Generated sprite PNGs
│   ├── Prefabs/                         — Game object prefabs
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── GamePlay.unity
│   │   └── GameOver.unity
│   ├── Audio/
│   │   ├── Music/                       — Background music (add .ogg/.wav)
│   │   └── SFX/                         — Sound effects (add .ogg/.wav)
│   └── Resources/
├── ProjectSettings/                     — Unity project configuration
├── Packages/
│   └── manifest.json                    — Package dependencies
└── BUILD_INSTRUCTIONS.md                — This file
```

---

## Game Features Summary

| Feature | Implementation |
|---|---|
| Player Movement | WASD/Arrows, clamped to screen bounds |
| Shooting | Auto-fire + Space/Mouse, configurable rate |
| Enemy Types | Basic, Fast (zigzag), Tank (pause+fire), Boss (phases) |
| Wave System | Predefined + procedural waves, boss every 5th |
| Bullet Patterns | Straight, Spread3, Spread5, Circle, Aimed, Homing |
| Power-Ups | Health, Shield, Rapid Fire, Spread Shot, Extra Life, Score |
| Health System | Player HP bar, enemy HP, damage events |
| Scoring | Points per kill, high score persistence (PlayerPrefs) |
| Parallax BG | Multi-layer scrolling + particle starfield |
| Sound | SFX pool system, music per scene, volume controls |
| UI | HUD, wave announcements, pause menu |
| Scenes | Main Menu → Gameplay → Game Over (full flow) |
| Persistence | High score saved between sessions |

---

*Created with ❤️ — A complete, production-ready Unity space shooter game.*
