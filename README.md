# 🚀 Space Shooter - Unity Game

A complete, playable space shooter game for Windows desktop built with Unity. Defend the galaxy against waves of enemy ships, collect power-ups, and chase the high score!

---

## 📋 Table of Contents

- [Features](#features)
- [Quick Start (Automated Setup)](#quick-start-automated-setup)
- [Manual Scene Setup](#manual-scene-setup)
- [Build Instructions (Windows .exe)](#build-instructions-windows-exe)
- [Game Controls](#game-controls)
- [Project Structure](#project-structure)
- [Script Reference](#script-reference)

---

## ✨ Features

- **Player Controls**: Smooth movement with Arrow Keys/WASD, shooting with Spacebar
- **4 Enemy Types**: Basic, Zigzag, Tank (aims at player), and Fast enemies
- **Wave System**: Progressive difficulty with increasing enemy counts and variety
- **3 Power-Ups**: Health restore (green), Shield (blue), Rapid Fire (yellow)
- **Parallax Starfield**: Multi-layer scrolling background with procedural stars
- **Full UI**: Health bar, score display, wave indicator, announcements
- **Menus**: Main menu, pause menu (ESC), game over screen with restart
- **High Score**: Persisted between sessions using PlayerPrefs
- **Procedural Audio**: All sound effects generated at runtime (no audio files needed)
- **Procedural Sprites**: All game sprites generated programmatically

---

## 🚀 Quick Start (Automated Setup)

### Prerequisites
- **Unity 2022.3 LTS** or newer (2023.x / 6000.x also work)
- Windows 10/11 for building the .exe

### Step-by-Step

1. **Create a New Unity Project**
   - Open Unity Hub → New Project
   - Select **2D (Built-in Render Pipeline)** template
   - Name it `SpaceShooter` (or any name)
   - Click Create

2. **Copy Project Files**
   - Copy the contents of this project's `Assets/` folder into your Unity project's `Assets/` folder:
     ```
     Assets/Scripts/       → YourProject/Assets/Scripts/
     Assets/Editor/        → YourProject/Assets/Editor/
     ```
   - Unity will auto-import the scripts

3. **Run Automated Setup**
   - In Unity, go to the menu bar: **Tools → Space Shooter → Setup Entire Project**
   - This single command will:
     - ✅ Generate all sprite PNG files
     - ✅ Create all prefabs (Player, Enemy, Bullets, PowerUp)
     - ✅ Build the GameScene with player, spawner, background, and full HUD
     - ✅ Build the MainMenuScene with title, buttons, and controls info
     - ✅ Configure build settings (scene order)
     - ✅ Set up project settings (resolution, tags)
   - Wait for the console to show "=== Project setup complete! ==="

4. **Configure Sprite Import Settings** (Important!)
   - In Project window, select all files in `Assets/Sprites/`
   - In Inspector, set:
     - Texture Type: **Sprite (2D and UI)**
     - Pixels Per Unit: **64**
     - Filter Mode: **Bilinear**
   - Click **Apply**

5. **Play the Game**
   - Open `Assets/Scenes/MainMenuScene` in the editor
   - Press the **Play** button
   - Click **PLAY** on the main menu to start

---

## 🔧 Manual Scene Setup

If you prefer to set up scenes manually instead of using the automated tool:

### Main Menu Scene (`MainMenuScene`)

1. Create a new Scene: File → New Scene
2. Save as `Assets/Scenes/MainMenuScene.unity`

**GameObjects to create:**

| GameObject | Components | Notes |
|---|---|---|
| Main Camera | Camera (Orthographic, Size 5.5) | BG color: (0.02, 0.02, 0.08) |
| GameManager | GameManager.cs | Assign PowerUp prefab |
| AudioManager | AudioManager.cs | - |
| BackgroundScroller | BackgroundScroller.cs | Creates stars automatically |
| MainMenuCanvas | Canvas + CanvasScaler + GraphicRaycaster + MenuManager.cs | Screen Space Overlay |
| EventSystem | EventSystem + StandaloneInputModule | Required for UI |

**Canvas children (UI):**
- **MainMenuPanel** (Image, transparent overlay)
  - **TitleText** (Text: "SPACE SHOOTER", size 72, cyan, bold)
  - **SubtitleText** (Text: "DEFEND THE GALAXY", size 24)
  - **HighScoreText** (Text, size 22, yellow)
  - **PlayButton** (Button + Text "PLAY", size 36)
  - **QuitButton** (Button + Text "QUIT", size 36)
  - **ControlsText** (Text: controls info, size 18, dim color)

Wire all UI references to the MenuManager component.

### Game Scene (`GameScene`)

1. Create a new Scene: File → New Scene
2. Save as `Assets/Scenes/GameScene.unity`

**GameObjects to create:**

| GameObject | Components | Position | Notes |
|---|---|---|---|
| Main Camera | Camera (Orthographic, Size 5.5) | (0, 0, -10) | BG color: (0.02, 0.02, 0.08) |
| Player | (Use Player prefab) | (0, -3.5, 0) | Tag: Player |
| EnemySpawner | EnemySpawner.cs | Any | Assign Enemy prefab |
| BackgroundScroller | BackgroundScroller.cs | (0, 0, 0) | - |
| GameManagerFallback | GameManagerFallback.cs | Any | Auto-creates GameManager if missing |
| GameCanvas | Canvas + UIManager + MenuManager | - | All HUD and menus |
| EventSystem | EventSystem + StandaloneInputModule | - | Required for UI |

**Canvas children (HUD + Menus):**

HUD Panel:
- ScoreText (top-left, "SCORE: 0")
- HighScoreText (top-left below score)
- WaveText (top-center, "WAVE 1")
- HealthBar (Slider, top-right, non-interactable)
- HPLabel (next to health bar)
- AnnouncementText (center, hidden by default)
- PowerUpText (below center, hidden)
- MessageText (center, hidden)

PauseMenuPanel (hidden by default):
- PauseTitle ("PAUSED")
- ResumeButton ("RESUME")
- QuitButton ("MAIN MENU")

GameOverPanel (hidden by default):
- GameOverTitle ("GAME OVER", red)
- GOScoreText, GOHighScoreText, GOWaveText
- NewHighScoreText ("NEW HIGH SCORE!", hidden)
- RestartButton ("PLAY AGAIN")
- MenuButton ("MAIN MENU")

### Prefabs to Create

| Prefab | Components | Key Settings |
|---|---|---|
| **Player** | SpriteRenderer, Rigidbody2D (no gravity), BoxCollider2D (trigger), HealthSystem (100 HP, no destroy on death, 1s invincibility), PlayerController, CollisionHandler (Player) | Tag: Player |
| **Enemy** | SpriteRenderer, Rigidbody2D (no gravity), BoxCollider2D (trigger), HealthSystem (50 HP), EnemyController (assign EnemyBullet prefab), CollisionHandler (Enemy) | Tag: Enemy |
| **PlayerBullet** | SpriteRenderer (blue), Rigidbody2D (no gravity, continuous), BoxCollider2D (trigger), BulletController, CollisionHandler (PlayerBullet) | Tag: PlayerBullet |
| **EnemyBullet** | SpriteRenderer (red), Rigidbody2D (no gravity, continuous), CircleCollider2D (trigger), BulletController, CollisionHandler (EnemyBullet) | Tag: EnemyBullet |
| **PowerUp** | SpriteRenderer, CircleCollider2D (trigger), PowerUpController, CollisionHandler (PowerUp) | Tag: PowerUp |

### Required Tags
Add these in Edit → Project Settings → Tags and Layers:
- Player
- Enemy
- PlayerBullet
- EnemyBullet
- PowerUp

### Build Settings Scene Order
In File → Build Settings:
1. `Assets/Scenes/MainMenuScene.unity` (index 0)
2. `Assets/Scenes/GameScene.unity` (index 1)

---

## 🏗️ Build Instructions (Windows .exe)

### Step 1: Install Build Support
- In Unity Hub → Installs → your Unity version → Add Modules
- Ensure **Windows Build Support (IL2CPP)** is installed
- Alternatively, **Mono** backend works too

### Step 2: Configure Build Settings
1. Open Unity → **File → Build Settings**
2. Set **Platform** to **Windows, Mac, Linux** (PC, Mac & Linux Standalone)
3. Set **Target Platform** to **Windows**
4. Set **Architecture** to **x86_64** (64-bit)
5. Ensure scenes are listed in order:
   - `Scenes/MainMenuScene` (index 0)
   - `Scenes/GameScene` (index 1)
   - If scenes are missing, click **Add Open Scenes** while each scene is open

### Step 3: Configure Player Settings
1. In Build Settings, click **Player Settings...**
2. Under **Player → Resolution and Presentation**:
   - Default Screen Width: **1280**
   - Default Screen Height: **720**
   - Fullscreen Mode: **Windowed** (or Fullscreen Window)
   - Run In Background: **Unchecked**
3. Under **Player → Other Settings**:
   - Scripting Backend: **Mono** (faster builds) or **IL2CPP** (better performance)
   - API Compatibility Level: **.NET Standard 2.1**
4. Under **Player → Product Name**: "Space Shooter"
5. Under **Player → Company Name**: Your name or studio

### Step 4: Build the Executable
1. In Build Settings, click **Build**
2. Create a new folder (e.g., `Build/SpaceShooter`)
3. Name the executable: `SpaceShooter.exe`
4. Click **Save** and wait for the build to complete
5. Your build folder will contain:
   ```
   SpaceShooter.exe              ← Main executable
   SpaceShooter_Data/            ← Game data folder
   UnityPlayer.dll               ← Unity runtime
   UnityCrashHandler64.exe       ← Crash handler
   ```

### Step 5: Distribute
- Zip the **entire build folder** (all files are required)
- The recipient just extracts and runs `SpaceShooter.exe`
- No Unity installation required to play

### Build Troubleshooting
- **Pink/magenta sprites**: Sprite import settings not configured (see Step 4 in Quick Start)
- **Missing scripts**: Ensure all .cs files are in `Assets/Scripts/` and `Assets/Editor/`
- **Scenes not loading**: Check Build Settings scene list and order
- **No sound**: AudioManager should be created automatically by GameManagerFallback

---

## 🎮 Game Controls

| Action | Key |
|---|---|
| Move | Arrow Keys or WASD |
| Shoot | Spacebar (hold for continuous fire) |
| Pause | Escape |
| Navigate Menus | Mouse click |

---

## 📁 Project Structure

```
Assets/
├── Editor/
│   ├── SpriteGenerator.cs        # Editor tool to generate sprite PNGs
│   └── SceneAutoSetup.cs         # One-click project setup tool
├── Scripts/
│   ├── PlayerController.cs       # Player movement, shooting, power-ups
│   ├── BulletController.cs       # Bullet movement, auto-destroy
│   ├── EnemyController.cs        # 4 enemy types with unique behaviors
│   ├── EnemySpawner.cs           # Wave-based spawning system
│   ├── PowerUpController.cs      # 3 power-up types with effects
│   ├── GameManager.cs            # Game state, score, wave progression
│   ├── UIManager.cs              # HUD: health bar, score, waves
│   ├── MenuManager.cs            # Main menu, pause, game over
│   ├── BackgroundScroller.cs     # Parallax starfield background
│   ├── CollisionHandler.cs       # All collision logic
│   ├── AudioManager.cs           # Procedural sound effects
│   ├── HealthSystem.cs           # Reusable health/damage system
│   └── RuntimeSpriteGenerator.cs # Runtime sprite generation utility
├── Sprites/                      # Generated sprite PNGs
├── Prefabs/                      # Game prefabs
├── Scenes/
│   ├── MainMenuScene.unity
│   └── GameScene.unity
├── Audio/                        # (Empty - audio is procedural)
└── Materials/                    # (Available for custom materials)
```

---

## 📖 Script Reference

### Core Systems
- **GameManager**: Singleton managing game state (Menu/Playing/Paused/GameOver), score, high score persistence via PlayerPrefs, wave tracking, and scene transitions
- **HealthSystem**: Reusable component with health, damage, healing, shields, invincibility, and death events
- **AudioManager**: Singleton generating all SFX procedurally at runtime using waveform synthesis

### Gameplay
- **PlayerController**: WASD/Arrow movement with boundary clamping, spacebar shooting with configurable fire rate, power-up state management
- **EnemyController**: 4 enemy types (Basic/Zigzag/Tank/Fast) with unique movement patterns, configurable by difficulty multiplier
- **EnemySpawner**: Wave-based spawning with progressive difficulty (more enemies, faster spawns, tougher types)
- **BulletController**: Direction/speed-based movement, auto-destroy on timeout or out-of-bounds
- **PowerUpController**: 3 types (Health/Shield/RapidFire) with drift movement, bobbing animation, and expiry blink
- **CollisionHandler**: Tag-based collision routing for all entity interactions

### UI
- **UIManager**: Real-time HUD updates with animated wave announcements and power-up notifications
- **MenuManager**: Three menu states with button wiring and GameManager integration

### Visual
- **BackgroundScroller**: 3-layer parallax starfield with procedurally generated star sprites
- **RuntimeSpriteGenerator**: Static utility for creating sprites in code when PNG assets aren't available

---

## 🎯 Game Design

### Enemy Types
| Type | Speed | Health | Behavior | Score |
|---|---|---|---|---|
| Basic | Normal | 50 | Straight down, occasional shots | 100 |
| Zigzag | Fast | 50 | Sine-wave horizontal movement | 150 |
| Tank | Slow | 150 | Aims bullets at player | 300 |
| Fast | Very Fast | 50 | Quick weaving, no shooting | 200 |

### Power-Ups (15% drop chance from enemies)
| Type | Color | Effect |
|---|---|---|
| Health | 🟢 Green | Restores 30 HP |
| Shield | 🔵 Blue | Absorbs one hit |
| Rapid Fire | 🟡 Yellow | Double fire rate for 8 seconds |

### Wave Progression
- Wave 1: 5 enemies (mostly Basic)
- Each wave adds 2 more enemies
- Enemy variety increases over waves
- Spawn rate accelerates
- Enemy stats scale with difficulty multiplier

---

## License

This project is free to use, modify, and distribute for any purpose.
