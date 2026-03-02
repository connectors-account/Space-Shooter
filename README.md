# Space Shooter Game - Unity Project

A complete 2D space shooter game built with Unity for Windows desktop. Features player movement, shooting mechanics, enemy waves, score tracking, and health management.

## 📋 Requirements

- **Unity Version**: Unity 2021.3 LTS or newer (2022.3 LTS recommended)
- **Platform**: Windows 10/11
- **Build Target**: Standalone (Windows)

## 🎮 Game Features

- Player ship movement (WASD or Arrow keys)
- Shooting mechanics (Space or Left Mouse Button)
- Wave-based enemy spawning with increasing difficulty
- Multiple enemy movement patterns (straight, zigzag, diagonal, sine wave)
- Health system (3-5 lives)
- Score tracking with persistent high score
- Game over and restart functionality
- Pause menu (ESC or P key)

## 📁 Project Structure

```
space_shooter_game/
├── Scripts/
│   ├── PlayerController.cs   # Player movement and shooting
│   ├── EnemySpawner.cs       # Enemy wave management
│   ├── Enemy.cs              # Enemy behavior and AI
│   ├── Bullet.cs             # Bullet mechanics
│   ├── GameManager.cs        # Game state management
│   └── UIManager.cs          # UI display and updates
└── README.md                 # This file
```

## 🚀 Setup Instructions

### Step 1: Create New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select **2D (Built-in Render Pipeline)** template
4. Name your project "SpaceShooter"
5. Choose a location and click "Create Project"

### Step 2: Import Scripts

1. In Unity, navigate to the Project window
2. Right-click in the Assets folder → Create → Folder → Name it "Scripts"
3. Copy all `.cs` files from this project's `Scripts/` folder into Unity's `Assets/Scripts/` folder
4. Wait for Unity to compile the scripts

### Step 3: Configure Tags

1. Go to **Edit → Project Settings → Tags and Layers**
2. Add the following tags:
   - `Player`
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`

### Step 4: Create the Player

1. **Create Player GameObject:**
   - Right-click in Hierarchy → 2D Object → Sprites → Square (or Triangle)
   - Rename it to "Player"
   - Set Tag to "Player"
   - Scale: (0.5, 0.5, 1)
   - Position: (0, -3, 0)

2. **Add Components to Player:**
   - Add Component → **Rigidbody2D**
     - Body Type: Kinematic
     - Collision Detection: Continuous
   - Add Component → **Box Collider 2D**
     - Check "Is Trigger"
   - Add Component → **PlayerController** (drag script or use Add Component)

3. **Optional - Change Player Color:**
   - Select Player in Hierarchy
   - In SpriteRenderer, change Color to green or your preferred color

### Step 5: Create Bullet Prefab

1. **Create Bullet GameObject:**
   - Right-click in Hierarchy → 2D Object → Sprites → Square
   - Rename to "Bullet"
   - Scale: (0.1, 0.3, 1)
   - Color: Yellow

2. **Add Components to Bullet:**
   - Add Component → **Rigidbody2D**
     - Body Type: Kinematic
   - Add Component → **Box Collider 2D**
     - Check "Is Trigger"
   - Add Component → **Bullet** script

3. **Create Prefab:**
   - Create folder: Assets → Create → Folder → "Prefabs"
   - Drag "Bullet" from Hierarchy into the Prefabs folder
   - Delete the Bullet from Hierarchy (it's now a prefab)

4. **Create Enemy Bullet Prefab:**
   - Duplicate the Bullet prefab
   - Rename to "EnemyBullet"
   - Select it and in the Bullet script inspector, uncheck "Is Player Bullet"

### Step 6: Create Enemy Prefab

1. **Create Enemy GameObject:**
   - Right-click in Hierarchy → 2D Object → Sprites → Square
   - Rename to "Enemy"
   - Set Tag to "Enemy"
   - Scale: (0.6, 0.6, 1)
   - Color: Red

2. **Add Components to Enemy:**
   - Add Component → **Rigidbody2D**
     - Body Type: Kinematic
   - Add Component → **Box Collider 2D**
     - Check "Is Trigger"
   - Add Component → **Enemy** script

3. **Configure Enemy Script:**
   - Max Health: 1
   - Score Value: 100
   - Move Speed: 3
   - Movement Pattern: Choose from dropdown

4. **Create Prefab:**
   - Drag "Enemy" into the Prefabs folder
   - Delete from Hierarchy

5. **Create Enemy Variants (Optional):**
   - Duplicate Enemy prefab
   - Create "Enemy_Fast" (higher speed, lower health)
   - Create "Enemy_Tank" (slower, more health, higher score)
   - Create "Enemy_Zigzag" (zigzag movement pattern)

### Step 7: Create Game Managers

1. **Create GameManager:**
   - Right-click in Hierarchy → Create Empty
   - Rename to "GameManager"
   - Add Component → **GameManager** script
   - Configure settings:
     - Starting Health: 3
     - Max Health: 5
     - Invincibility Duration: 2

2. **Create EnemySpawner:**
   - Right-click in Hierarchy → Create Empty
   - Rename to "EnemySpawner"
   - Add Component → **EnemySpawner** script
   - Configure settings:
     - Drag your Enemy prefab(s) into the "Enemy Prefabs" array
     - Initial Spawn Rate: 2
     - Minimum Spawn Rate: 0.5
     - Spawn Range X: 7
     - Spawn Position Y: 6

### Step 8: Set Up UI

1. **Create Canvas:**
   - Right-click in Hierarchy → UI → Canvas
   - Canvas Scaler → UI Scale Mode: "Scale With Screen Size"
   - Reference Resolution: 1920 x 1080

2. **Create UI Elements:**

   **Score Text:**
   - Right-click Canvas → UI → Legacy → Text
   - Rename to "ScoreText"
   - Anchor: Top Left
   - Position: (150, -30, 0)
   - Text: "Score: 0"
   - Font Size: 32
   - Color: White

   **Health Text:**
   - Create another Text, rename to "HealthText"
   - Anchor: Top Right
   - Position: (-150, -30, 0)
   - Text: "Health: 3/5"
   - Font Size: 32

   **Wave Text:**
   - Create another Text, rename to "WaveText"
   - Anchor: Top Center
   - Position: (0, -30, 0)
   - Text: "Wave: 1"
   - Font Size: 32

3. **Create Game Over Panel:**
   - Right-click Canvas → UI → Panel
   - Rename to "GameOverPanel"
   - Color: (0, 0, 0, 200) - semi-transparent black
   - Add child Text elements:
     - "FinalScoreText" - "Final Score: 0"
     - "GameOverHighScoreText" - "High Score: 0"
     - "RestartInstructionText" - "Press 'R' to Restart"

4. **Create UIManager:**
   - Right-click in Hierarchy → Create Empty
   - Rename to "UIManager"
   - Add Component → **UIManager** script
   - Drag all UI Text elements to their respective slots in the inspector
   - Drag GameOverPanel to its slot

### Step 9: Connect Everything

1. **Player Setup:**
   - Select Player in Hierarchy
   - In PlayerController, drag the Bullet prefab to "Bullet Prefab" field

2. **EnemySpawner Setup:**
   - Select EnemySpawner in Hierarchy
   - Drag all Enemy prefabs into the "Enemy Prefabs" array

### Step 10: Configure Physics (Important!)

1. Go to **Edit → Project Settings → Physics 2D**
2. In Layer Collision Matrix, ensure these layers can interact:
   - Player can collide with Enemy and EnemyBullet
   - PlayerBullet can collide with Enemy

### Step 11: Camera Setup

1. Select Main Camera
2. Set Background color to black or dark blue (space theme)
3. Size: 5-6 (adjust based on preference)
4. Position: (0, 0, -10)

## 🏗️ Building for Windows

### Build Settings

1. Go to **File → Build Settings**
2. Select **PC, Mac & Linux Standalone**
3. Target Platform: **Windows**
4. Architecture: **x86_64** (recommended) or x86
5. Click **Switch Platform** (if not already selected)

### Build Configuration

1. Click **Player Settings**
2. In **Player** section:
   - Company Name: YourName
   - Product Name: Space Shooter
   - Default Icon: (optional - add your game icon)
3. In **Resolution and Presentation**:
   - Fullscreen Mode: Windowed (for testing) or Fullscreen Window
   - Default Screen Width: 1920
   - Default Screen Height: 1080
   - Resizable Window: Yes
4. In **Other Settings**:
   - API Compatibility Level: .NET Standard 2.1

### Building the Executable

1. Go to **File → Build Settings**
2. Ensure your game scene is in "Scenes In Build"
   - If not, click "Add Open Scenes"
3. Click **Build**
4. Create a new folder (e.g., "SpaceShooterBuild")
5. Name your executable (e.g., "SpaceShooter")
6. Click **Save**
7. Wait for build to complete

### Build Output

Your build folder will contain:
- `SpaceShooter.exe` - The game executable
- `SpaceShooter_Data/` - Game data folder (required)
- `UnityPlayer.dll` - Unity runtime (required)
- `MonoBleedingEdge/` - Mono runtime folder

**Important:** To distribute your game, include ALL files/folders from the build directory.

## 🎹 Controls

| Action | Key/Button |
|--------|------------|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Space / Left Mouse Button |
| Pause | Escape / P |
| Restart (Game Over) | R |

## 🎯 Gameplay

1. **Objective:** Destroy enemy ships and survive as long as possible
2. **Scoring:** Each enemy destroyed gives points (varies by enemy type)
3. **Health:** Start with 3 health, lose 1 when hit by enemy or enemy bullet
4. **Waves:** Enemies spawn in waves, each wave is harder than the last
5. **Game Over:** When health reaches 0, press R to restart

## 🔧 Customization Tips

### Adjusting Difficulty
- **EnemySpawner**: Lower spawn rate = more enemies
- **Enemy**: Higher move speed = harder to hit
- **GameManager**: Lower starting health = harder game

### Adding More Enemy Types
1. Duplicate existing Enemy prefab
2. Modify stats (health, speed, score, movement pattern)
3. Add to EnemySpawner's enemy prefabs array

### Adding Sound Effects
1. Import audio files (`.wav` or `.mp3`) into Assets/Audio/
2. Drag sounds to the appropriate script fields:
   - PlayerController: shootSound, hitSound
   - Enemy: (add fields for death sound)

### Adding Background
1. Create a new Sprite in Hierarchy
2. Set Sorting Layer to "Background" (create if needed)
3. Position at Z = 1 (behind other objects)
4. Add scrolling script for moving starfield effect

## 🐛 Troubleshooting

### Common Issues

**Bullets don't hit enemies:**
- Ensure both have Collider2D with "Is Trigger" checked
- Check that tags are set correctly ("PlayerBullet", "Enemy")
- Verify Physics2D layer collision matrix

**Player doesn't move:**
- Check that PlayerController script is attached
- Verify Input settings (Edit → Project Settings → Input Manager)

**Enemies don't spawn:**
- Ensure enemy prefabs are assigned in EnemySpawner
- Check that EnemySpawner GameObject is active

**UI doesn't update:**
- Verify UIManager has all Text fields assigned
- Check that UIManager GameObject exists and script is attached

**Build errors:**
- Ensure all scripts compile without errors in Unity
- Check Build Settings for correct platform

## 📝 License

This project is free to use for educational purposes. Feel free to modify and expand upon it!

---

Happy coding and enjoy your space shooter game! 🚀
