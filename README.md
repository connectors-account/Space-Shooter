# Space Shooter Game

A classic 2D space shooter game built with Unity and C#. Fight through waves of enemies, collect power-ups, and achieve the highest score!

## Table of Contents
- [Game Features](#game-features)
- [Controls](#controls)
- [Requirements](#requirements)
- [Project Setup](#project-setup)
- [Building for Windows](#building-for-windows)
- [Game Mechanics](#game-mechanics)
- [Scripts Overview](#scripts-overview)

---

## Game Features

- **Player Ship**: Smooth movement and shooting mechanics
- **3 Enemy Types**:
  - **Basic**: Moves straight down
  - **Zigzag**: Weaves side to side while descending
  - **Tank**: Slower, takes multiple hits, can shoot back
- **5 Progressive Waves**: Increasing difficulty
- **3 Power-Up Types**:
  - **Rapid Fire** (Yellow): Faster shooting for 5 seconds
  - **Shield** (Blue): Blocks one hit
  - **Health** (Green): Restores 1 health point
- **Parallax Star Field**: Dynamic scrolling background
- **UI System**: Main menu, HUD, pause menu, game over, and victory screens
- **Sound Effects**: Procedurally generated placeholder sounds
- **High Score System**: Persists between sessions

---

## Controls

| Action | Key |
|--------|-----|
| Move | WASD or Arrow Keys |
| Shoot | Spacebar (hold for continuous fire) |
| Pause | Escape |
| Start Game | Enter (from main menu) |
| Restart | R (after game over/victory) |

---

## Requirements

- **Unity Version**: 2021.3 LTS or newer (2022.3 LTS recommended)
- **Platform**: Windows 10/11
- **Disk Space**: ~500MB for Unity project

---

## Project Setup

### Step 1: Install Unity

1. Download and install [Unity Hub](https://unity.com/download)
2. In Unity Hub, install Unity Editor version **2022.3 LTS** (or 2021.3 LTS)
3. During installation, ensure **Windows Build Support** is selected

### Step 2: Create New Unity Project

1. Open Unity Hub
2. Click **"New Project"**
3. Select **"2D (Built-in Render Pipeline)"** template
4. Name the project: `SpaceShooter`
5. Choose a location and click **"Create project"**

### Step 3: Import Scripts

1. Navigate to your Unity project's `Assets` folder in File Explorer
2. Copy all `.cs` files from this project's `Assets/Scripts/` folder into your Unity project's `Assets/Scripts/` folder
   - If `Scripts` folder doesn't exist, create it
3. Return to Unity Editor - it will auto-import the scripts

### Step 4: Set Up Tags

1. In Unity, go to **Edit > Project Settings > Tags and Layers**
2. Under **Tags**, click **"+"** to add these tags:
   - `Player`
   - `Enemy`
   - `Bullet`
   - `PowerUp`

### Step 5: Create the Game Scene

#### Option A: Automatic Setup (Recommended)

1. In Unity, create an empty GameObject: **GameObject > Create Empty**
2. Name it `GameSetup`
3. In the Inspector, click **Add Component** and add the `GameSetup` script
4. Ensure **"Auto Setup On Start"** is checked
5. Press **Play** - the game will auto-configure itself!
6. After testing, you can remove the GameSetup object

#### Option B: Manual Setup

Follow these steps to manually set up the scene:

##### Create the Player

1. **GameObject > 2D Object > Sprites > Square**
2. Name it `Player`
3. Set Tag to `Player`
4. Set Position to `(0, -3, 0)`
5. Set Scale to `(0.5, 0.5, 1)`
6. Add Component: **Box Collider 2D**
   - Check **Is Trigger**
   - Size: `(0.8, 0.8)`
7. Add Component: **Rigidbody 2D**
   - Gravity Scale: `0`
   - Freeze Rotation: `Z` (under Constraints)
8. Add Component: **PlayerController** script
9. Create empty child object named `FirePoint` at position `(0, 0.6, 0)`
10. Drag `FirePoint` to the `Fire Point` field in PlayerController

##### Create the Bullet Prefab

1. **GameObject > 2D Object > Sprites > Square**
2. Name it `Bullet`
3. Set Tag to `Bullet`
4. Set Scale to `(0.1, 0.3, 1)`
5. Set Color to Cyan
6. Add Component: **Box Collider 2D**
   - Check **Is Trigger**
   - Size: `(0.2, 0.5)`
7. Add Component: **Rigidbody 2D**
   - Gravity Scale: `0`
8. Add Component: **BulletController** script
9. Drag from Hierarchy to **Assets/Prefabs** folder to create prefab
10. Delete from scene

##### Create the Enemy Prefab

1. **GameObject > 2D Object > Sprites > Square**
2. Name it `Enemy`
3. Set Tag to `Enemy`
4. Set Scale to `(0.6, 0.6, 1)`
5. Set Color to Red
6. Add Component: **Box Collider 2D**
   - Check **Is Trigger**
   - Size: `(0.9, 0.9)`
7. Add Component: **Rigidbody 2D**
   - Gravity Scale: `0`
8. Add Component: **EnemyController** script
9. Assign the Bullet prefab to the `Bullet Prefab` field
10. Drag to Prefabs folder, then delete from scene

##### Create the PowerUp Prefab

1. **GameObject > 2D Object > Sprites > Circle**
2. Name it `PowerUp`
3. Set Tag to `PowerUp`
4. Set Scale to `(0.4, 0.4, 1)`
5. Set Color to Yellow
6. Add Component: **Circle Collider 2D**
   - Check **Is Trigger**
   - Radius: `0.4`
7. Add Component: **Rigidbody 2D**
   - Gravity Scale: `0`
8. Add Component: **PowerUpController** script
9. Drag to Prefabs folder, then delete from scene

##### Create Manager Objects

Create empty GameObjects and add the corresponding scripts:

1. `GameManager` - Add **GameManager** script
2. `SpawnManager` - Add **SpawnManager** script
   - Assign Enemy and PowerUp prefabs
3. `UIManager` - Add **UIManager** script
4. `AudioManager` - Add **AudioManager** script
5. `ParallaxBackground` - Add **ParallaxBackground** script

##### Assign Prefab References

1. Select `Player` and assign the Bullet prefab to `Bullet Prefab` field
2. Select `SpawnManager` and assign:
   - Enemy prefab to `Enemy Prefab`
   - PowerUp prefab to `Power Up Prefab`

##### Configure Camera

1. Select `Main Camera`
2. Set **Projection** to `Orthographic`
3. Set **Size** to `5`
4. Set **Background** to dark blue `(13, 13, 38)` or `#0D0D26`

### Step 6: Test the Game

1. Press **Play** in Unity Editor
2. Press **Enter** to start the game
3. Use WASD to move, Spacebar to shoot
4. Test all features work correctly

---

## Building for Windows

### Configure Build Settings

1. Go to **File > Build Settings**
2. Select **Windows, Mac, Linux** platform
3. Click **Switch Platform** (if not already selected)
4. Click **Add Open Scenes** to include your game scene
5. Ensure your main scene is at index 0

### Configure Player Settings

1. In Build Settings, click **Player Settings**
2. Configure these settings:

**Product Name Tab:**
- Company Name: `Your Name`
- Product Name: `Space Shooter`
- Version: `1.0`

**Resolution and Presentation:**
- Fullscreen Mode: `Windowed` (for testing) or `Fullscreen Window`
- Default Screen Width: `1280`
- Default Screen Height: `720`
- Resizable Window: `Yes` (optional)

**Other Settings:**
- Scripting Backend: `Mono` (default) or `IL2CPP` for better performance
- API Compatibility Level: `.NET Standard 2.1`

**Icon (Optional):**
- Add a custom icon for your game

### Build the Game

1. In Build Settings, click **Build**
2. Create a new folder named `Build` or `SpaceShooter_Windows`
3. Select the folder and click **Select Folder**
4. Wait for the build to complete (may take a few minutes)
5. Your executable will be in the selected folder:
   - `SpaceShooter.exe` - The game executable
   - `SpaceShooter_Data/` - Required game data folder
   - `UnityPlayer.dll` - Required runtime library
   - `MonoBleedingEdge/` - Mono runtime files

### Running the Built Game

1. Navigate to your build folder
2. Double-click `SpaceShooter.exe` to run
3. The game will start in the configured resolution

### Distribution

To share your game:
1. Zip the entire build folder (including all subfolders)
2. Recipients only need to extract and run the `.exe` file
3. No Unity installation required to play!

---

## Game Mechanics

### Scoring
- Basic Enemy: 100 points
- Zigzag Enemy: 150 points
- Tank Enemy: 300 points

### Wave Progression

| Wave | Basic | Zigzag | Tank | Spawn Rate |
|------|-------|--------|------|------------|
| 1 | 5 | 0 | 0 | 1.5s |
| 2 | 4 | 3 | 0 | 1.2s |
| 3 | 5 | 3 | 1 | 1.0s |
| 4 | 6 | 4 | 2 | 0.9s |
| 5 | 8 | 5 | 3 | 0.8s |

### Player Stats
- Health: 3 hits
- Invincibility after hit: 1.5 seconds
- Normal fire rate: 0.2 seconds
- Rapid fire rate: 0.08 seconds

### Power-Up Drop Rate
- 15% chance when destroying an enemy

---

## Scripts Overview

| Script | Purpose |
|--------|----------|
| `PlayerController.cs` | Player movement, shooting, power-ups, health |
| `EnemyController.cs` | Enemy AI, movement patterns, shooting |
| `BulletController.cs` | Bullet physics, collision, lifetime |
| `SpawnManager.cs` | Wave system, enemy/power-up spawning |
| `GameManager.cs` | Game state, score, high score, pause |
| `PowerUpController.cs` | Power-up types and effects |
| `UIManager.cs` | All UI screens and HUD |
| `ParallaxBackground.cs` | Scrolling star field effect |
| `CollisionHandler.cs` | Collision detection helper |
| `AudioManager.cs` | Sound effects (procedurally generated) |
| `SpriteGenerator.cs` | Procedural sprite generation |
| `GameSetup.cs` | Automatic scene setup |

---

## Troubleshooting

### "Tag not found" Error
- Ensure all tags (Player, Enemy, Bullet, PowerUp) are created in Project Settings

### Player/Enemies Don't Collide
- Check that all colliders have **Is Trigger** enabled
- Ensure Rigidbody2D is attached to moving objects

### UI Not Showing
- Make sure a Canvas exists in the scene
- UIManager will auto-create UI if references are null

### Build Fails
- Check the Console for errors
- Ensure all script compilation errors are fixed
- Try **Assets > Reimport All**

### Game Runs Slowly
- Reduce number of stars in ParallaxBackground
- Use IL2CPP scripting backend for builds

---

## License

This project is provided as-is for educational purposes. Feel free to modify and distribute.

---

## Credits

Created with Unity and C#.
