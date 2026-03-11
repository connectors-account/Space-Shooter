# Space Shooter Game - Unity Project

A complete, fully functional space shooter game for Windows desktop built with Unity and C#.

## Table of Contents
1. [Game Features](#game-features)
2. [Requirements](#requirements)
3. [Project Setup](#project-setup)
4. [Creating Game Objects](#creating-game-objects)
5. [Scene Configuration](#scene-configuration)
6. [Building for Windows](#building-for-windows)
7. [Controls](#controls)
8. [Troubleshooting](#troubleshooting)

---

## Game Features

- **Player ship** with smooth movement (Arrow keys/WASD) and shooting
- **Wave-based enemy spawning** with increasing difficulty
- **Three enemy movement patterns**: Straight, Zigzag, and Sine wave
- **Power-up system** (Rapid Fire)
- **Health system** with 3 lives
- **Scoring system** with persistent high score
- **Full UI**: Score, health, wave counter, game over screen, pause menu
- **Parallax scrolling background**

---

## Requirements

- **Unity Version**: Unity 2021.3 LTS or newer (2022.3 LTS recommended)
- **Platform**: Windows 10/11
- **Build Target**: Windows Standalone (x86_64)

---

## Project Setup

### Step 1: Create New Unity Project

1. Open **Unity Hub**
2. Click **"New Project"**
3. Select **"2D (Built-in Render Pipeline)"** template
4. Name it: `SpaceShooterGame`
5. Choose a location and click **"Create Project"**

### Step 2: Import Scripts

1. Copy all `.cs` files from `Assets/Scripts/` to your Unity project's `Assets/Scripts/` folder
2. Unity will automatically compile them

### Step 3: Create Tags

1. Go to **Edit > Project Settings > Tags and Layers**
2. Add the following tags:
   - `Player`
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`
   - `PowerUp`

### Step 4: Create Sorting Layers

1. In **Tags and Layers**, go to **Sorting Layers**
2. Add layers in this order (bottom to top):
   - `Background` (Order: -10)
   - `Default` (Order: 0)
   - `Enemies` (Order: 1)
   - `Player` (Order: 2)
   - `Projectiles` (Order: 3)
   - `UI` (Order: 10)

---

## Creating Game Objects

### Player Ship

1. **Create the Player**:
   - Right-click in Hierarchy > **2D Object > Sprites > Square**
   - Rename to `Player`
   - Set Position: `(0, -3, 0)`
   - Set Scale: `(0.8, 1, 1)` (ship-like shape)
   - Color: **Green** (in Sprite Renderer)

2. **Add Components**:
   - Add **BoxCollider2D** (Is Trigger: ✓)
   - Add **Rigidbody2D** (Body Type: Kinematic)
   - Add **PlayerController** script

3. **Create Fire Point**:
   - Create empty child object named `FirePoint`
   - Set Position: `(0, 0.6, 0)` (front of ship)

4. **Assign the Tag**: Set tag to `Player`

### Player Bullet

1. **Create the Bullet**:
   - Right-click in Hierarchy > **2D Object > Sprites > Square**
   - Rename to `PlayerBullet`
   - Set Scale: `(0.1, 0.3, 1)`
   - Color: **Yellow**

2. **Add Components**:
   - Add **BoxCollider2D** (Is Trigger: ✓)
   - Add **Rigidbody2D** (Body Type: Kinematic)
   - Add **Bullet** script
   - Set `Is Player Bullet`: ✓

3. **Create Prefab**:
   - Drag to `Assets/Prefabs/` folder
   - Delete from scene

### Enemy Bullet

1. **Duplicate PlayerBullet prefab** and rename to `EnemyBullet`
2. Color: **Red**
3. In Bullet script: `Is Player Bullet`: ✗

### Enemy Ship

1. **Create the Enemy**:
   - Right-click in Hierarchy > **2D Object > Sprites > Square**
   - Rename to `Enemy`
   - Set Scale: `(0.8, 0.8, 1)`
   - Color: **Red**

2. **Add Components**:
   - Add **BoxCollider2D** (Is Trigger: ✓)
   - Add **Rigidbody2D** (Body Type: Kinematic)
   - Add **Enemy** script

3. **Configure Enemy Script**:
   - Drag `EnemyBullet` prefab to `Enemy Bullet Prefab` field
   - Set `Score Value`: 100
   - Set `Power Up Drop Chance`: 0.15

4. **Assign Tag**: Set tag to `Enemy`

5. **Create Prefab**: Drag to `Assets/Prefabs/`

### Power-Up

1. **Create the Power-Up**:
   - Right-click in Hierarchy > **2D Object > Sprites > Square**
   - Rename to `PowerUp_RapidFire`
   - Set Scale: `(0.5, 0.5, 1)`
   - Color: **Cyan/Light Blue**

2. **Add Components**:
   - Add **BoxCollider2D** (Is Trigger: ✓)
   - Add **Rigidbody2D** (Body Type: Kinematic)
   - Add **PowerUp** script
   - Type: `RapidFire`

3. **Assign Tag**: Set tag to `PowerUp`

4. **Create Prefab**: Drag to `Assets/Prefabs/`

### Enemy Spawner

1. **Create Empty Object**:
   - Right-click in Hierarchy > **Create Empty**
   - Rename to `EnemySpawner`
   - Position: `(0, 0, 0)`

2. **Add EnemySpawner Script**:
   - Drag `Enemy` prefab to `Enemy Prefab` field

### Game Manager

1. **Create Empty Object**:
   - Right-click in Hierarchy > **Create Empty**
   - Rename to `GameManager`
   - Position: `(0, 0, 0)`

2. **Add GameManager Script**

---

## Scene Configuration

### Camera Setup

1. Select **Main Camera**
2. Set **Background Color**: Dark blue `(0.05, 0.05, 0.15, 1)`
3. Set **Size**: 5 (for orthographic camera)
4. Set **Position**: `(0, 0, -10)`

### Background (Optional Parallax)

1. **Create Background**:
   - Right-click > **2D Object > Sprites > Square**
   - Rename to `Background`
   - Scale: `(20, 40, 1)` (covers screen)
   - Color: Dark purple/blue gradient
   - Sorting Layer: `Background`
   - Order in Layer: -10

2. **For Parallax Effect**:
   - Duplicate background
   - Position second copy at Y = 40 (directly above)
   - Add **BackgroundScroller** script to both
   - Set `Background Height`: 40

### UI Setup

1. **Create Canvas**:
   - Right-click > **UI > Canvas**
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920 x 1080

2. **Create HUD Panel** (child of Canvas):
   - Add **Panel** (transparent background)
   - Anchor: Top-Left

3. **Add UI Elements** (children of HUD Panel):

   **Score Text**:
   - UI > Text (or TextMeshPro)
   - Position: Top-left
   - Text: "Score: 0"
   - Font Size: 24
   - Color: White

   **High Score Text**:
   - Position: Below Score
   - Text: "High Score: 0"
   - Font Size: 18
   - Color: Yellow

   **Health Text**:
   - Position: Top-right
   - Text: "Health: ♥ ♥ ♥"
   - Font Size: 24
   - Color: Red

   **Wave Text**:
   - Position: Top-center
   - Text: "Wave 1"
   - Font Size: 28
   - Color: White

4. **Create Game Over Panel**:
   - Add Panel (dark semi-transparent background)
   - Center on screen
   - Add child texts:
     - "GAME OVER" (large, centered)
     - "Final Score: 0"
     - "Press R to Restart"
   - Start **Disabled** (uncheck GameObject active)

5. **Create Pause Panel** (similar to Game Over):
   - "PAUSED"
   - "Press ESC to Resume"
   - Start **Disabled**

6. **Create UI Controller**:
   - Add empty child to Canvas named `UIController`
   - Add **UIController** script
   - Drag all text references to the appropriate fields

7. **Connect to GameManager**:
   - Select GameManager object
   - Drag UIController to the `UI Controller` field

---

## Connecting Everything

### Player Configuration

1. Select `Player` in hierarchy
2. In **PlayerController**:
   - Drag `PlayerBullet` prefab to `Bullet Prefab`
   - Drag `FirePoint` child object to `Fire Point`
   - Move Speed: 8
   - Fire Rate: 0.3
   - Max Health: 3

### Enemy Configuration

1. Open `Enemy` prefab
2. In **Enemy** script:
   - Drag `EnemyBullet` prefab to `Enemy Bullet Prefab`
   - Drag `PowerUp_RapidFire` prefab to `Power Up Prefab`
   - Power Up Drop Chance: 0.15
   - Score Value: 100

### Spawner Configuration

1. Select `EnemySpawner`
2. In **EnemySpawner** script:
   - Drag `Enemy` prefab to `Enemy Prefab`
   - Min X: -6
   - Max X: 6
   - Spawn Y: 7
   - Enemies Per Wave: 5

---

## Building for Windows

### Step 1: Configure Build Settings

1. Go to **File > Build Settings**
2. Select **Windows, Mac, Linux** (or PC, Mac & Linux Standalone)
3. Target Platform: **Windows**
4. Architecture: **x86_64**
5. Click **Add Open Scenes** to include your game scene

### Step 2: Player Settings

1. Click **Player Settings**
2. **Product Name**: "Space Shooter"
3. **Company Name**: Your name
4. **Resolution and Presentation**:
   - Fullscreen Mode: Windowed (or your preference)
   - Default Screen Width: 1280
   - Default Screen Height: 720
5. **Icon** (optional): Add your game icon

### Step 3: Build

1. Click **Build**
2. Create a new folder (e.g., `SpaceShooter_Build`)
3. Choose filename: `SpaceShooter.exe`
4. Click **Save**
5. Wait for build to complete

### Build Output

Your build folder will contain:
```
SpaceShooter_Build/
├── SpaceShooter.exe          (Main executable)
├── SpaceShooter_Data/        (Game data folder)
├── MonoBleedingEdge/         (Mono runtime)
└── UnityCrashHandler64.exe   (Crash handler)
```

**Important**: Distribute the ENTIRE folder, not just the .exe file!

---

## Controls

| Key | Action |
|-----|--------|
| Arrow Keys / WASD | Move ship |
| Space | Shoot |
| Escape | Pause/Resume |
| R (Game Over) | Restart |

---

## Troubleshooting

### Common Issues

**"NullReferenceException" errors:**
- Ensure all prefab references are assigned in Inspector
- Check that GameManager has UIController reference
- Verify tags are created and assigned

**Player doesn't move:**
- Check PlayerController script is attached
- Verify Rigidbody2D is set to Kinematic
- Ensure Time.timeScale is 1

**Bullets don't hit enemies:**
- Verify both have Collider2D with "Is Trigger" checked
- Check tags are correctly assigned
- Ensure bullet's isPlayerBullet is set correctly

**Enemies don't spawn:**
- Check EnemySpawner has Enemy Prefab assigned
- Verify spawner is active in scene
- Check spawn Y position is above camera view

**UI not updating:**
- Ensure GameManager has UIController reference
- Verify UIController has all text fields assigned
- Check Canvas is set up correctly

### Performance Tips

- Keep bullet lifetime short (3 seconds)
- Destroy enemies/bullets when off-screen
- Use object pooling for better performance (advanced)

---

## Script Overview

| Script | Purpose |
|--------|---------|
| `PlayerController.cs` | Player movement, shooting, health |
| `Bullet.cs` | Bullet movement and collision |
| `Enemy.cs` | Enemy AI, movement patterns, shooting |
| `EnemySpawner.cs` | Wave-based enemy spawning |
| `PowerUp.cs` | Power-up collection and effects |
| `GameManager.cs` | Game state, scoring, UI coordination |
| `UIController.cs` | All UI element management |
| `BackgroundScroller.cs` | Parallax background scrolling |

---

## License

This project is provided as-is for educational purposes. Feel free to modify and use it for your own projects.

---

## Version

- **Version**: 1.0
- **Unity Version**: 2021.3+ LTS
- **Last Updated**: March 2026
