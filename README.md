# 🚀 STELLAR ASSAULT — Space Shooter Game

A classic vertical-scrolling space shooter built with **Unity** and **C#**, featuring pixel art visuals, wave-based enemy progression, power-ups, boss fights, and a full menu system.

![Genre](https://img.shields.io/badge/genre-Space%20Shooter-blue)
![Engine](https://img.shields.io/badge/engine-Unity%202022.3%2B-black)
![Platform](https://img.shields.io/badge/platform-Windows-green)
![Language](https://img.shields.io/badge/language-C%23-purple)

---

## 📋 Table of Contents

- [Game Features](#-game-features)
- [Controls](#-controls)
- [Project Structure](#-project-structure)
- [Setup Instructions](#-setup-instructions)
- [Build Instructions](#-build-instructions-windows-executable)
- [Architecture Overview](#-architecture-overview)
- [Script Reference](#-script-reference)
- [Customization Guide](#-customization-guide)
- [Troubleshooting](#-troubleshooting)

---

## 🎮 Game Features

### Core Gameplay
- **Vertical scrolling** space shooter
- **10 waves** of increasingly difficult enemies
- **4 enemy types**: Basic, Fast, Tank, Shooter
- **Boss fights** at waves 5 and 10 with multiple attack phases
- **Wave progression** with dynamic difficulty scaling

### Player
- Smooth 8-directional movement with ship tilt animation
- Multiple weapon types: Single, Double, Triple, Spread, Laser
- Health system with hit points
- Shield system that absorbs damage
- Invincibility frames on hit with visual flashing
- Respawn system with brief invincibility

### Power-Up System (7 Types)
| Power-Up | Icon | Effect |
|----------|------|--------|
| Weapon Upgrade | 🔵 W | Upgrades weapon tier (15s duration) |
| Shield | 🟢 S | Adds 2 shield points |
| Health | 🔴 + | Restores 1 HP |
| Speed Boost | 🟡 ★ | 50% speed increase (8s) |
| Rapid Fire | 🟣 R | 2x fire rate (8s) |
| Extra Life | ⚪ 1 | Adds 1 life |
| Bomb | 🟠 B | Destroys all enemies on screen |

### Enemies & AI
- **Basic**: Flies straight down (2 HP, 100 pts)
- **Fast**: Sine-wave movement pattern (1 HP, 150 pts)
- **Tank**: Hovers and absorbs damage (8 HP, 300 pts)
- **Shooter**: Hovers and fires aimed bullets (4 HP, 200 pts)
- **Boss**: Multi-phase with circle bursts and spiral attacks (50 HP, 5000 pts)

### Bullet Patterns
- Straight, Aimed, Spread fan, Circle burst, Spiral, Burst fire

### Visual Effects
- Parallax scrolling starfield background
- Procedural particle star generator
- Explosion effects
- Ship tilt animation
- Invincibility flash
- Screen shake on impacts
- Pulsing power-up items

### Audio System
- Pooled SFX playback (16 channels)
- Separate Music/SFX/Master volume controls
- State-driven music (menu, game, boss, game over)
- Pitch randomization for variety

### UI System
- **Main Menu**: Start, Options (volume sliders), Quit
- **In-Game HUD**: Score, High Score, Lives, Health, Shield, Wave, Weapon
- **Pause Menu**: Resume, Restart, Main Menu, Quit
- **Game Over Screen**: Final score, high score, new high score indicator
- **Victory Screen**: Congratulations with final score

---

## 🕹️ Controls

| Action | Key(s) |
|--------|--------|
| Move | WASD / Arrow Keys |
| Shoot | Space / Left Mouse Button (hold for auto-fire) |
| Pause | Escape / P |
| Navigate Menus | Mouse click |

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerController.cs     # Movement, input, screen clamping
│   │   │   ├── PlayerShooting.cs       # Weapon system, firing, upgrades
│   │   │   └── PlayerHealth.cs         # HP, shields, invincibility, death
│   │   ├── Enemies/
│   │   │   ├── EnemyBase.cs            # Base class: HP, movement patterns, AI
│   │   │   ├── EnemyBasic.cs           # Simple straight-down flyer
│   │   │   ├── EnemyFast.cs            # Fast sine-wave mover
│   │   │   ├── EnemyTank.cs            # Slow tanky hoverer
│   │   │   ├── EnemyShooter.cs         # Hovers and shoots at player
│   │   │   └── BossEnemy.cs            # Multi-phase boss with attacks
│   │   ├── Bullets/
│   │   │   ├── Bullet.cs               # Universal projectile behavior
│   │   │   └── BulletPattern.cs        # Configurable enemy fire patterns
│   │   ├── PowerUps/
│   │   │   └── PowerUp.cs              # 7 power-up types with effects
│   │   ├── Managers/
│   │   │   ├── GameManager.cs          # Singleton: state, score, lives, flow
│   │   │   └── WaveManager.cs          # Wave spawning, progression, patterns
│   │   ├── UI/
│   │   │   ├── MainMenuUI.cs           # Title screen controller
│   │   │   ├── GameHUD.cs              # In-game heads-up display
│   │   │   ├── PauseMenuUI.cs          # Pause overlay
│   │   │   └── GameOverUI.cs           # Game over / victory screens
│   │   ├── Background/
│   │   │   ├── ParallaxBackground.cs   # Scrolling background layers
│   │   │   └── StarfieldGenerator.cs   # Procedural particle stars
│   │   ├── Audio/
│   │   │   └── SoundManager.cs         # Audio: SFX pool, music, volumes
│   │   └── Utils/
│   │       ├── ObjectPooler.cs         # Generic object pooling system
│   │       ├── ScreenShake.cs          # Camera shake effect
│   │       └── AutoDestroy.cs          # Timed self-destruction
│   ├── Editor/
│   │   ├── GameSetupWizard.cs          # One-click scene & prefab setup
│   │   └── SpriteImportSettings.cs     # Auto pixel-art import config
│   ├── Sprites/
│   │   ├── Player/                     # player_ship.png, player_shield.png
│   │   ├── Enemies/                    # enemy_basic/fast/tank/shooter.png, boss.png
│   │   ├── Bullets/                    # player_bullet.png, enemy_bullet.png, laser.png
│   │   ├── PowerUps/                   # powerup_weapon/shield/health/speed/rapidfire/extralife/bomb.png
│   │   ├── Background/                 # starfield_bg.png, nebula_overlay.png
│   │   ├── Effects/                    # explosion_0..4.png
│   │   └── UI/                         # heart.png, heart_empty.png, shield_icon.png, button_bg.png, panel_bg.png
│   ├── Prefabs/                        # Auto-generated by setup wizard
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Bullets/
│   │   ├── PowerUps/
│   │   └── Effects/
│   ├── Scenes/                         # Auto-generated by setup wizard
│   │   ├── MainMenu.unity
│   │   └── GameScene.unity
│   ├── Audio/                          # Place .wav/.ogg files here
│   │   ├── Music/
│   │   └── SFX/
│   ├── Materials/
│   ├── Animations/
│   └── Resources/
├── ProjectSettings/                    # Unity project configuration
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   ├── Physics2DSettings.asset
│   ├── EditorBuildSettings.asset
│   └── InputManager.asset
├── Packages/
│   └── manifest.json
├── generate_sprites.py                 # Python script that generated the sprites
└── README.md                           # This file
```

---

## 🛠️ Setup Instructions

### Prerequisites
- **Unity 2022.3 LTS** or newer (2023.x / 6000.x also work)
  - Download from: https://unity.com/download
  - During install, include the **Windows Build Support** module
- **Windows 10/11** (for building the executable)

### Step-by-Step Setup

#### 1. Open the Project in Unity

```
1. Launch Unity Hub
2. Click "Open" → "Add project from disk"
3. Navigate to and select the `space_shooter_game` folder
4. Unity will import all assets (this may take 1-2 minutes)
```

> **Note**: When Unity opens, it will auto-detect and import all sprites with
> pixel-art settings thanks to `SpriteImportSettings.cs`.

#### 2. Run the Setup Wizard (IMPORTANT!)

```
1. In Unity's menu bar, click: Space Shooter → Setup Game (Full)
2. Click "Yes, Set Up Everything" in the confirmation dialog
3. Wait for the progress bar to complete
4. Click "Got it!" when done
```

This wizard automatically:
- Creates all prefabs (Player, Enemies, Bullets, Power-Ups, Effects)
- Creates the **MainMenu** scene with UI
- Creates the **GameScene** with HUD, pause menu, and game over screens
- Configures build settings with both scenes

#### 3. Wire Up Remaining References

After the wizard runs, a few Inspector references need manual connection:

**GameManager (in GameScene)**:
1. Open `Assets/Scenes/GameScene`
2. Select the `GameManager` object in Hierarchy
3. Drag the `PlayerShip` prefab from `Assets/Prefabs/Player/` into the `Player Prefab` slot
4. Drag `PlayerSpawnPoint` from Hierarchy into the `Player Spawn Point` slot

**WaveManager (in GameScene)**:
1. Select the `WaveManager` object
2. In `Enemy Prefabs` array (size 4):
   - Element 0: `Assets/Prefabs/Enemies/EnemyBasic`
   - Element 1: `Assets/Prefabs/Enemies/EnemyFast`
   - Element 2: `Assets/Prefabs/Enemies/EnemyTank`
   - Element 3: `Assets/Prefabs/Enemies/EnemyShooter`
3. `Boss Prefab`: `Assets/Prefabs/Enemies/Boss`
4. In `Power Up Prefabs` array (size 7):
   - Drag all prefabs from `Assets/Prefabs/PowerUps/`

**PlayerShooting (on PlayerShip prefab)**:
1. Double-click `Assets/Prefabs/Player/PlayerShip` to open prefab
2. Drag `Assets/Prefabs/Bullets/PlayerBullet` into `Bullet Prefab`
3. Drag `Assets/Prefabs/Bullets/Laser` into `Laser Prefab`
4. Drag child objects `FirePoint`, `FirePointLeft`, `FirePointRight` into their slots

**PlayerHealth (on PlayerShip prefab)**:
1. Drag `Assets/Prefabs/Effects/Explosion` into `Explosion Prefab`
2. Drag child `ShieldVisual` into `Shield Visual` slot

**Enemy Shooter/Tank BulletPattern**:
1. Open each enemy prefab that has a `BulletPattern` component
2. Drag `Assets/Prefabs/Bullets/EnemyBullet` into the `Bullet Prefab` slot

**Boss Enemy**:
1. Open `Assets/Prefabs/Enemies/Boss`
2. Drag `Assets/Prefabs/Bullets/EnemyBullet` into `Bullet Prefab`

**HUD, Pause, and GameOver UI scripts**:
1. Select each UI script object and drag the corresponding Text/Button objects from the canvas hierarchy into their serialized field slots

#### 4. Play!

```
1. Open Assets/Scenes/GameScene (or MainMenu for full flow)
2. Press the Play button (▶) at the top of Unity
3. Use WASD + Space to play!
```

---

## 🏗️ Build Instructions (Windows Executable)

### Quick Build

```
1. In Unity: File → Build Settings
2. Ensure both scenes are listed (MainMenu first, GameScene second)
3. Target Platform: Windows
4. Architecture: x86_64
5. Click "Build"
6. Choose an output folder (e.g., "Build/")
7. Wait for the build to complete
8. Run "Space Shooter.exe" from the output folder
```

### Detailed Build Settings

| Setting | Recommended Value |
|---------|------------------|
| Target Platform | Windows |
| Architecture | x86_64 (Intel 64-bit) |
| Compression Method | Default |
| Copy PDB files | ✗ (for release builds) |
| Development Build | ✗ (for release builds) |
| Scripting Backend | Mono or IL2CPP |
| API Compatibility | .NET Standard 2.1 |

### Build via Command Line (CI/CD)

```bash
# From terminal/PowerShell:
"C:\Program Files\Unity\Hub\Editor\2022.3.XXf1\Editor\Unity.exe" \
  -batchmode \
  -nographics \
  -projectPath "path/to/space_shooter_game" \
  -buildWindows64Player "Build/SpaceShooter.exe" \
  -quit
```

### Build Output
```
Build/
├── Space Shooter.exe           # Main executable - run this!
├── Space Shooter_Data/         # Game data folder
│   ├── Managed/                # .NET assemblies
│   ├── Resources/              # Game resources
│   └── ...
├── UnityCrashHandler64.exe     # Crash reporter
└── UnityPlayer.dll             # Unity runtime
```

> **Distribution**: Zip the entire Build folder. Players just extract and run the .exe.

---

## 🏛️ Architecture Overview

### Design Patterns Used
- **Singleton**: GameManager, SoundManager, WaveManager, ObjectPooler, ScreenShake
- **Observer/Events**: C# events for score changes, state changes, health updates
- **Component**: Unity's ECS-style component architecture
- **Object Pooling**: Reusable bullet pool to reduce garbage collection
- **State Machine**: GameManager drives Playing/Paused/GameOver/Victory states
- **Template Method**: EnemyBase provides overridable Move()/Die() for enemy types

### Game Flow
```
MainMenu → [Start] → GameScene
                        ├── Playing → Wave 1..10
                        │     ├── Enemies spawn
                        │     ├── Player shoots
                        │     ├── Power-ups drop
                        │     └── Wave clear → next wave
                        ├── Paused (ESC) → Resume/Restart/Menu
                        ├── GameOver (0 lives) → Retry/Menu
                        └── Victory (wave 10 clear) → Retry/Menu
```

### Event System
```
GameManager.OnScoreChanged    → GameHUD updates score display
GameManager.OnLivesChanged    → GameHUD updates lives display
GameManager.OnWaveChanged     → GameHUD shows wave announcement
GameManager.OnGameStateChanged→ PauseMenuUI, GameOverUI, SoundManager react
PlayerHealth.OnHealthChanged  → GameHUD updates health bar
PlayerHealth.OnShieldChanged  → GameHUD updates shield display
PlayerHealth.OnPlayerDeath    → GameManager handles respawn/game over
EnemyBase.OnEnemyDestroyed    → WaveManager tracks remaining enemies
```

---

## 📖 Script Reference

### GameManager.cs
Central singleton managing all game state. Handles score tracking, lives, wave progression, pause/resume, scene loading, high score persistence via PlayerPrefs.

### WaveManager.cs
Controls enemy wave spawning. Supports predefined waves or auto-generates 10 waves with scaling difficulty. Spawns enemies in configurable patterns (Random, LeftToRight, VFormation, Circle, ZigZag). Tracks alive enemy count and progresses waves when all enemies are defeated.

### PlayerController.cs
Processes WASD/Arrow input, applies smooth movement with velocity damping, clamps to screen bounds, and applies visual tilt when moving horizontally.

### PlayerShooting.cs
Manages 5 weapon types with auto-fire. Weapon upgrades have timed duration. Fire rate configurable. Spawns bullet prefabs with direction and speed.

### PlayerHealth.cs
Tracks HP and shield points. Shield absorbs damage first. Invincibility frames with sprite flashing. Spawns explosion on death, notifies GameManager.

### EnemyBase.cs
Abstract base with 7 movement patterns. Health scales with difficulty multiplier. Flash white on hit. Drops score on death. Fires OnEnemyDestroyed event.

### BulletPattern.cs
Configurable enemy firing: Straight, Aimed (at player with accuracy), Spread fan, Circle burst, Spiral, and Burst fire modes.

### PowerUp.cs
7 power-up types with float-down movement, bobbing, rotation, and pulsing scale. Bomb type clears all enemies and enemy bullets.

### SoundManager.cs
16-channel SFX pool with pitch randomization. Named sound lookup dictionary. Separate volume controls with PlayerPrefs persistence. State-driven music switching.

---

## 🎨 Customization Guide

### Adding New Enemy Types
1. Create a new script extending `EnemyBase`
2. Override `Awake()` to set stats
3. Optionally override `Move()` for custom movement
4. Create sprite, make prefab, add to WaveManager

### Adding New Power-Ups
1. Add new entry to `PowerUpType` enum
2. Add handling in `PowerUp.ApplyEffect()`
3. Create sprite, make prefab, add to WaveManager's power-up array

### Adjusting Difficulty
- Edit `WaveManager.GenerateWaves()` for wave composition
- Adjust `GameManager.GetDifficultyMultiplier()` scaling curve
- Modify enemy HP/speed values in their respective scripts

### Adding Sound Effects
1. Place `.wav` or `.ogg` files in `Assets/Audio/SFX/`
2. Add entries to `SoundManager`'s Sound Effects array in Inspector
3. Call `SoundManager.Instance.PlaySFX("sound_name")` from code

### Screen Resolution
Default is 600×900 (portrait). Change in:
- `ProjectSettings.asset`: `defaultScreenWidth/Height`
- `CanvasScaler`: reference resolution in UI canvases

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| No menu after setup wizard | Ensure scenes are in Build Settings (File → Build Settings) |
| Sprites appear blurry | Select sprites, set Filter Mode to "Point (no filter)" |
| Player doesn't shoot | Check PlayerShooting has bullet prefab assigned |
| Enemies don't spawn | Check WaveManager has enemy prefabs in array |
| No sound | Add AudioClips to SoundManager's arrays in Inspector |
| Build fails | Ensure Windows Build Support module is installed in Unity Hub |
| `rb.linearVelocity` error | This API requires Unity 6+. For older versions, change to `rb.velocity` |
| Tags not found | Run the setup wizard, or manually add tags: Player, Enemy, PlayerBullet, EnemyBullet, PowerUp |

### Unity Version Compatibility

| Feature | Unity 2022.3 | Unity 2023.x | Unity 6 (6000.x) |
|---------|-------------|-------------|-------------------|
| `rb.linearVelocity` | Use `rb.velocity` | Use `rb.velocity` | ✅ `rb.linearVelocity` |
| Legacy UI (UnityEngine.UI) | ✅ | ✅ | ✅ |
| `FindObjectsByType` | ✅ | ✅ | ✅ |

> **For Unity 2022.3 / 2023.x**: Do a Find & Replace in all scripts:
> `rb.linearVelocity` → `rb.velocity` (3 occurrences in Bullet.cs and PlayerController.cs)

---

## 📝 License

This project is provided as-is for educational and personal use. All code and pixel art assets are original creations. Feel free to modify and use in your own projects.

---

## 🎯 Quick Start Summary

```
1. Install Unity 2022.3+ with Windows Build Support
2. Open this folder as a Unity project
3. Menu: Space Shooter → Setup Game (Full)
4. Wire up prefab references in Inspector (see Setup section)
5. Press Play!
6. To build: File → Build Settings → Build
```

**Have fun blasting aliens! 🛸💥**
