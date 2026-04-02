# 🚀 Space Shooter - Unity C# Game

A complete, wave-based space shooter game built with Unity and C#. Defend the galaxy by destroying waves of enemies, collecting power-ups, and achieving the highest score!

---

## 📋 Table of Contents
- [Game Features](#game-features)
- [Project Structure](#project-structure)
- [Setup Instructions](#setup-instructions)
- [Automated Setup (Recommended)](#automated-setup-recommended)
- [Manual Setup](#manual-setup)
- [Building for Windows](#building-for-windows)
- [Game Controls](#game-controls)
- [Script Reference](#script-reference)
- [Customization Guide](#customization-guide)

---

## 🎮 Game Features

- **Wave-based enemy spawning** with increasing difficulty
- **3 enemy types**: Basic (straight), Zigzag (wave pattern), Charger (rushes player)
- **3 power-up types**: Shield (absorbs one hit), Rapid Fire, Health Restore
- **Scoring system** with persistent high score
- **Health system** with invincibility frames on damage
- **Parallax scrolling background** for depth effect
- **Full UI system**: Main Menu, HUD, Pause Menu, Game Over screen
- **Wave announcements** with fade-out effect
- **Sound effect system** (plug in your own audio clips)
- **Procedural sprite generation** for immediate playability

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/                    # All game logic scripts
│   │   ├── PlayerController.cs     # Player movement, shooting, health
│   │   ├── EnemyController.cs      # Enemy AI and behavior patterns
│   │   ├── BulletController.cs     # Bullet movement and damage
│   │   ├── EnemySpawner.cs         # Wave-based enemy spawning
│   │   ├── GameManager.cs          # Game state, scoring, scene flow
│   │   ├── PowerUp.cs              # Power-up collection and effects
│   │   ├── ParallaxBackground.cs   # Scrolling background effect
│   │   ├── UIManager.cs            # HUD, health bar, wave display
│   │   ├── MenuManager.cs          # Main menu and game over screens
│   │   ├── AudioManager.cs         # Sound effect management
│   │   └── Explosion.cs            # Explosion visual effect
│   ├── Editor/                     # Editor-only utility scripts
│   │   ├── SpriteGenerator.cs      # Generates placeholder sprites
│   │   ├── SceneSetup.cs           # Automated scene/prefab setup
│   │   └── TagSetup.cs             # Creates required tags
│   ├── Prefabs/                    # Game object prefabs (auto-generated)
│   ├── Scenes/                     # Game scenes (auto-generated)
│   │   ├── MainMenu.unity
│   │   ├── GamePlay.unity
│   │   └── GameOver.unity
│   ├── Sprites/                    # Sprite textures (auto-generated)
│   ├── Audio/                      # Place audio files here
│   └── Materials/                  # Materials (if needed)
├── ProjectSettings/                # Unity project configuration
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   ├── Physics2DSettings.asset
│   ├── InputManager.asset
│   └── EditorBuildSettings.asset
├── Packages/
│   └── manifest.json               # Unity package dependencies
├── .gitignore
└── README.md                       # This file
```

---

## 🛠️ Setup Instructions

### Prerequisites
- **Unity 2021.3 LTS** or newer (2022.x or 2023.x also work)
- **Windows 10/11** (for building the .exe)
- Unity Hub installed

### Step 1: Create Unity Project
1. Open **Unity Hub**
2. Click **"New Project"**
3. Select **"2D (Built-in Render Pipeline)"** template
4. Name it `SpaceShooter` and choose a location
5. Click **"Create Project"**
6. Wait for Unity to create and open the project

### Step 2: Import Game Files
1. Close Unity (or keep it open — it will auto-detect changes)
2. **Copy all files** from this repository into your Unity project folder:
   - Copy `Assets/Scripts/` → your project's `Assets/Scripts/`
   - Copy `Assets/Editor/` → your project's `Assets/Editor/`
   - Copy `ProjectSettings/TagManager.asset` → your project's `ProjectSettings/TagManager.asset`
   - Copy `ProjectSettings/InputManager.asset` → your project's `ProjectSettings/InputManager.asset`
3. Return to Unity — it will import the scripts automatically

---

## ⚡ Automated Setup (Recommended)

The project includes an automated setup system that creates everything for you:

### One-Click Setup
1. After importing the scripts, go to the Unity menu bar
2. Click **Tools → Space Shooter → Setup Complete Game**
3. Wait for the setup to complete (a dialog box will confirm)
4. Press **Play** to test the game!

This automated setup will:
- ✅ Generate all placeholder sprites
- ✅ Create all prefabs with correct components and references
- ✅ Build all 3 scenes (MainMenu, GamePlay, GameOver)
- ✅ Configure build settings
- ✅ Wire up all UI buttons

### Individual Setup Tools
You can also run individual setup steps from the menu:
- **Tools → Space Shooter → Generate All Sprites** — Creates sprite textures only
- **Tools → Space Shooter → Setup Tags** — Creates required tags only

---

## 🔧 Manual Setup

If you prefer to set up manually (or the automated setup needs tweaking):

### Step 1: Create Tags
Go to **Edit → Project Settings → Tags and Layers** and add these tags:
- `Player`
- `Enemy`
- `PlayerBullet`
- `EnemyBullet`
- `PowerUp`

### Step 2: Generate Sprites
1. Go to **Tools → Space Shooter → Generate All Sprites**
2. Check `Assets/Sprites/` for the generated PNG files
3. Select all sprites and set their import settings:
   - **Texture Type**: Sprite (2D and UI)
   - **Pixels Per Unit**: 32
   - **Filter Mode**: Point (no filter)

### Step 3: Create Prefabs

#### Player Prefab
1. Create empty GameObject, name it "Player", tag as "Player"
2. Add: `SpriteRenderer` (player_ship sprite), `BoxCollider2D` (Is Trigger ✓), `Rigidbody2D` (Gravity Scale = 0, Is Kinematic ✓), `PlayerController`
3. Create child "FirePoint" at local position (0, 0.8, 0)
4. Create child "ShieldVisual" with `SpriteRenderer` (shield_visual sprite), set inactive
5. Assign references in `PlayerController`: bullet prefab, fire point, explosion prefab, shield visual
6. Drag to `Assets/Prefabs/`

#### Enemy Prefabs (repeat for Basic, Zigzag, Charger)
1. Create empty GameObject, tag as "Enemy"
2. Add: `SpriteRenderer` (enemy sprite), `BoxCollider2D` (Is Trigger ✓), `Rigidbody2D` (Gravity Scale = 0, Is Kinematic ✓), `EnemyController`
3. Set the `EnemyType` to match (Basic/Zigzag/Charger)
4. Assign explosion prefab, enemy bullet prefab, and power-up drop array
5. Drag to `Assets/Prefabs/`

#### Bullet Prefabs
1. Create empty, add `SpriteRenderer`, `BoxCollider2D` (Trigger), `Rigidbody2D` (Kinematic), `BulletController`
2. **PlayerBullet**: tag "PlayerBullet", isPlayerBullet = true, speed = 14
3. **EnemyBullet**: tag "EnemyBullet", isPlayerBullet = false, speed = 8

#### Power-Up Prefabs (repeat for Shield, RapidFire, Health)
1. Create empty, tag as "PowerUp"
2. Add: `SpriteRenderer`, `CircleCollider2D` (Trigger), `Rigidbody2D` (Kinematic), `PowerUp`
3. Set the `PowerUpType` accordingly

#### Explosion Prefab
1. Create empty, add `SpriteRenderer` (explosion sprite), `Explosion` script
2. Duration = 0.5, MaxScale = 2.5

### Step 4: Create Scenes

#### MainMenu Scene
1. **File → New Scene**, save as `Assets/Scenes/MainMenu.unity`
2. Set camera: Orthographic, size 5, dark background color
3. Create Canvas (Screen Space - Overlay, Scale with Screen Size 800x600)
4. Add UI Text: "SPACE SHOOTER" (title), "HIGH SCORE: 0"
5. Add UI Buttons: "PLAY", "QUIT"
6. Create empty "GameManager" → add `GameManager` script
7. Create empty "AudioManager" → add `AudioManager` script
8. Create empty "MenuManager" → add `MenuManager` script
9. Wire button OnClick events to MenuManager methods

#### GamePlay Scene
1. **File → New Scene**, save as `Assets/Scenes/GamePlay.unity`
2. Set camera: Orthographic, size 5
3. Create background layers with `ParallaxBackground`
4. Instantiate Player prefab at position (0, -3.5, 0)
5. Create empty "EnemySpawner" → add `EnemySpawner` script, assign enemy prefabs
6. Create Canvas with HUD elements (Score, Wave, Health text + health bar)
7. Create empty "UIManager" → add `UIManager` script, assign all UI references
8. Add pause menu panel with Resume and Main Menu buttons

#### GameOver Scene
1. **File → New Scene**, save as `Assets/Scenes/GameOver.unity`
2. Set camera: Orthographic, dark red-tinted background
3. Create Canvas with "GAME OVER" text, score display, high score display
4. Add "PLAY AGAIN" and "MAIN MENU" buttons
5. Create empty "MenuManager" → add `MenuManager` script, wire references

### Step 5: Build Settings
1. Go to **File → Build Settings**
2. Add scenes in order: MainMenu (index 0), GamePlay (index 1), GameOver (index 2)
3. Select **Windows, Mac, Linux** as platform

---

## 🏗️ Building for Windows

### Build as Windows Executable (.exe)

1. Open Unity with the project
2. Go to **File → Build Settings** (Ctrl+Shift+B)
3. Ensure **Platform** is set to **Windows, Mac, Linux**
   - If not, select it and click **"Switch Platform"** (may take a moment)
4. Verify all 3 scenes are listed and checked:
   - `Scenes/MainMenu` (index 0)
   - `Scenes/GamePlay` (index 1)
   - `Scenes/GameOver` (index 2)
5. Set **Target Platform**: Windows
6. Set **Architecture**: x86_64 (64-bit)
7. Click **"Player Settings..."** to configure:
   - **Product Name**: Space Shooter
   - **Default Screen Width**: 800
   - **Default Screen Height**: 600
   - **Fullscreen Mode**: Windowed (or your preference)
   - **Run In Background**: ✓ (checked)
8. Click **"Build"**
9. Choose an output folder (e.g., `Build/Windows/`)
10. Wait for the build to complete
11. Run `Space Shooter.exe` from the output folder!

### Build Output
The build creates:
```
Build/Windows/
├── Space Shooter.exe           # Run this!
├── Space Shooter_Data/         # Game data (required)
├── UnityPlayer.dll             # Unity runtime (required)
└── MonoBleedingEdge/           # Mono runtime (required)
```
**Distribute the entire folder** — all files are required.

---

## 🎮 Game Controls

| Action | Key |
|--------|-----|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Space / Left Mouse Button |
| Pause | Escape |

---

## 📖 Script Reference

### Core Scripts

| Script | Purpose |
|--------|---------|
| `PlayerController.cs` | Player movement (WASD/Arrows), shooting (Space/LMB), health, invincibility, power-up states |
| `EnemyController.cs` | Enemy AI with 3 movement patterns (Basic/Zigzag/Charger), shooting, health, drops |
| `BulletController.cs` | Bullet movement, direction, lifetime, damage dealing on collision |
| `EnemySpawner.cs` | Wave system with 10 predefined + infinite procedural waves |
| `GameManager.cs` | Singleton managing game state, score, high score (persisted), scene transitions |
| `PowerUp.cs` | 3 power-up types with drift, bob, rotation, and flash-before-expire effects |
| `ParallaxBackground.cs` | Infinite scrolling background with seamless looping |
| `UIManager.cs` | HUD updates (score, wave, health bar), wave announcements, pause menu |
| `MenuManager.cs` | Main menu and game over screen logic, button callbacks |
| `AudioManager.cs` | Named SFX dictionary system, background music, volume control |
| `Explosion.cs` | Scale-up and fade-out death effect |

### Editor Scripts (not included in builds)

| Script | Purpose |
|--------|---------|
| `SpriteGenerator.cs` | Generates procedural placeholder sprites for all game objects |
| `SceneSetup.cs` | One-click automated setup of all prefabs, scenes, and build settings |
| `TagSetup.cs` | Creates all required tags in the project |

---

## 🎨 Customization Guide

### Replacing Sprites
1. Place your sprites in `Assets/Sprites/`
2. Set import type to **Sprite (2D and UI)**, Pixels Per Unit to match your art
3. Update the sprite references on each prefab's SpriteRenderer

### Adding Sound Effects
1. Place `.wav` or `.ogg` files in `Assets/Audio/`
2. Select the AudioManager in the scene (it persists from MainMenu)
3. In the Inspector, expand **Sound Effects** array
4. Add entries with these names (used in code):
   - `PlayerShoot` — player fires a bullet
   - `EnemyShoot` — enemy fires a bullet
   - `Explosion` — enemy or player destroyed
   - `PlayerHit` — player takes damage
   - `PowerUp` — power-up collected
   - `ShieldBreak` — shield absorbs a hit
   - `WaveStart` — new wave begins

### Adjusting Difficulty
- **EnemySpawner**: Edit the `predefinedWaves` array in the Inspector or modify `CreateDefaultWaves()` in code
- **Enemy speed**: Change `moveSpeed` on enemy prefabs
- **Spawn rate**: Modify `spawnInterval` in wave configs
- **Power-up frequency**: Adjust `dropChance` on enemy prefabs (0.0–1.0)
- **Player health**: Change `maxHealth` on the Player prefab

### Adding New Enemy Types
1. Add a new value to the `EnemyType` enum in `EnemyController.cs`
2. Add a new movement method (e.g., `MoveCircular()`)
3. Add the case to the `switch` statement in `Update()`
4. Create a new prefab with the new type selected
5. Add the prefab reference to the `EnemySpawner`

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| "Tag not found" errors | Run **Tools → Space Shooter → Setup Tags** |
| Missing prefab references | Re-run the automated setup, or manually assign in Inspector |
| Enemies don't die | Ensure bullets have colliders (Is Trigger ✓) and correct tags |
| Player doesn't move | Check Input settings (Edit → Project Settings → Input Manager) |
| Score doesn't display | Ensure UIManager references are assigned in the GamePlay scene |
| Game won't build | Check all 3 scenes are in Build Settings (File → Build Settings) |
| Audio warnings in console | These are non-fatal — add audio clips to AudioManager to resolve |

---

## 📄 License

This project is provided as-is for educational and personal use. Feel free to modify and extend it!
