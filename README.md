# Space Shooter Game - Unity Project

A simple but fully functional space-shooter game built with Unity for Windows desktop.

## 🎮 Game Features

- **Player Ship**: Move with WASD or Arrow keys, shoot with Spacebar
- **Enemy Ships**: Spawn from the top and move downward, some can shoot back
- **Collision Detection**: Bullets hitting ships, enemies colliding with player
- **Health System**: Player has 3 health points
- **Score Tracking**: Earn points by destroying enemies
- **Difficulty Scaling**: Game gets harder over time
- **UI System**: Health display, score counter, game over screen
- **Pause Menu**: Press ESC to pause

## 🎯 Controls

| Action | Key |
|--------|-----|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Spacebar |
| Pause | Escape |
| Restart (Game Over) | R |

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   └── Scripts/
│       ├── PlayerController.cs    # Player movement, shooting, health
│       ├── EnemyController.cs     # Enemy behavior and AI
│       ├── BulletController.cs    # Bullet movement and collision
│       ├── GameManager.cs         # Game state, score, flow control
│       ├── UIManager.cs           # UI elements and displays
│       └── EnemySpawner.cs        # Enemy wave spawning system
└── README.md
```

---

## 🚀 Setup Instructions

### Prerequisites

- **Unity Hub** installed
- **Unity Editor** version **2021.3 LTS** or newer (recommended: 2022.3 LTS)
- Windows 10/11 for building

### Step 1: Create New Unity Project

1. Open **Unity Hub**
2. Click **"New Project"**
3. Select **"2D Core"** template
4. Name it `SpaceShooterGame`
5. Choose a location and click **Create Project**

### Step 2: Import Scripts

1. In Unity, navigate to `Assets` folder in the Project window
2. Create a folder called `Scripts` (Right-click → Create → Folder)
3. Copy all `.cs` files from this repository's `Assets/Scripts/` folder into `Assets/Scripts/`
4. Wait for Unity to compile the scripts

### Step 3: Create Tags

1. Go to **Edit → Project Settings → Tags and Layers**
2. Under **Tags**, add these new tags:
   - `Player`
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`

### Step 4: Create Player Ship

1. **Create Sprite**: Right-click in Hierarchy → **2D Object → Sprites → Triangle**
2. **Rename** to `Player`
3. **Set Position**: (0, -3, 0)
4. **Set Scale**: (0.5, 0.7, 1)
5. **Set Tag**: `Player`
6. **Add Components**:
   - **Rigidbody2D**: Set Body Type to `Kinematic`
   - **Box Collider 2D**: Check `Is Trigger`
   - **PlayerController** script (drag from Scripts folder)

#### Create Fire Point for Player:
1. Right-click on `Player` in Hierarchy → **Create Empty**
2. Rename to `FirePoint`
3. Set Position: (0, 0.5, 0)

#### Assign Fire Point:
1. Select `Player`
2. In PlayerController component, drag `FirePoint` to the "Fire Point" field

### Step 5: Create Bullet Prefab

1. **Create Sprite**: Right-click in Hierarchy → **2D Object → Sprites → Capsule**
2. **Rename** to `Bullet`
3. **Set Scale**: (0.1, 0.3, 1)
4. **Change Color**: Click on Sprite Renderer → Color → Set to Yellow
5. **Add Components**:
   - **Rigidbody2D**: Body Type = `Kinematic`
   - **Box Collider 2D**: Check `Is Trigger`
   - **BulletController** script

#### Save as Prefab:
1. Create `Prefabs` folder in Assets
2. Drag `Bullet` from Hierarchy into `Prefabs` folder
3. Delete `Bullet` from Hierarchy (it's now a prefab)

### Step 6: Create Enemy Prefab

1. **Create Sprite**: Right-click in Hierarchy → **2D Object → Sprites → Hexagon Flat-Top**
2. **Rename** to `Enemy`
3. **Set Scale**: (0.6, 0.6, 1)
4. **Set Rotation**: (0, 0, 180) - to face downward
5. **Change Color**: Set to Red
6. **Set Tag**: `Enemy`
7. **Add Components**:
   - **Rigidbody2D**: Body Type = `Kinematic`
   - **Polygon Collider 2D**: Check `Is Trigger`
   - **EnemyController** script

#### Configure Enemy:
1. In EnemyController, assign the Bullet prefab to "Bullet Prefab" field

#### Save as Prefab:
1. Drag `Enemy` into `Prefabs` folder
2. Delete from Hierarchy

### Step 7: Assign Prefab to Player

1. Select `Player` in Hierarchy
2. In PlayerController, drag `Bullet` prefab to "Bullet Prefab" field

### Step 8: Create Game Manager

1. **Create Empty**: Right-click in Hierarchy → Create Empty
2. **Rename** to `GameManager`
3. **Add Component**: GameManager script

### Step 9: Create Enemy Spawner

1. **Create Empty**: Right-click in Hierarchy → Create Empty
2. **Rename** to `EnemySpawner`
3. **Add Component**: EnemySpawner script
4. **Configure**: 
   - Drag Enemy prefab to "Enemy Prefabs" array (Size: 1, Element 0: Enemy)

### Step 10: Create UI

#### Create Canvas:
1. Right-click in Hierarchy → **UI → Canvas**
2. Set **UI Scale Mode** to "Scale With Screen Size"
3. Set Reference Resolution: 1920 x 1080

#### Create Score Text:
1. Right-click on Canvas → **UI → Legacy → Text**
2. Rename to `ScoreText`
3. Set Anchor to **Top-Left**
4. Position: (120, -40)
5. Size: (200, 50)
6. Text: "Score: 0"
7. Font Size: 32
8. Color: White

#### Create Health Text:
1. Right-click on Canvas → **UI → Legacy → Text**
2. Rename to `HealthText`
3. Set Anchor to **Top-Right**
4. Position: (-120, -40)
5. Size: (200, 50)
6. Text: "Health: 3/3"
7. Font Size: 32
8. Color: White
9. Alignment: Right

#### Create Game Over Panel:
1. Right-click on Canvas → **UI → Panel**
2. Rename to `GameOverPanel`
3. Set Color: (0, 0, 0, 200) - semi-transparent black

Inside GameOverPanel, add:
- **Text** "GAME OVER" (FontSize: 72, centered)
- **Text** named `GameOverScoreText` - "Final Score: 0"
- **Text** named `HighScoreText` - "High Score: 0"
- **Button** with text "RESTART" 
- **Button** with text "QUIT"

#### Create Pause Menu Panel:
1. Right-click on Canvas → **UI → Panel**
2. Rename to `PauseMenuPanel`
3. Add Text "PAUSED"
4. Add "RESUME" and "QUIT" buttons

### Step 11: Create UI Manager

1. **Create Empty** under Canvas, rename to `UIManager`
2. **Add Component**: UIManager script
3. **Assign UI Elements**:
   - Score Text → ScoreText
   - Health Text → HealthText
   - Game Over Panel → GameOverPanel
   - Game Over Score Text → GameOverScoreText
   - High Score Text → HighScoreText
   - Pause Menu Panel → PauseMenuPanel

### Step 12: Connect Button Events

For Restart Button:
1. Select the Restart button
2. In OnClick(), add UIManager → OnRestartButtonClicked

For Resume Button:
1. Add UIManager → OnResumeButtonClicked

For Quit Buttons:
1. Add UIManager → OnQuitButtonClicked

### Step 13: Camera Setup

1. Select **Main Camera**
2. Set Background color to dark blue: (0.05, 0.05, 0.15)
3. Ensure Projection is **Orthographic**
4. Size: 5

---

## 🔨 Building for Windows

### Step 1: Configure Build Settings

1. Go to **File → Build Settings**
2. Select **Windows, Mac, Linux** platform
3. Click **Switch Platform** (if not already selected)
4. Click **Add Open Scenes** to add your game scene

### Step 2: Configure Player Settings

1. Click **Player Settings**
2. Set **Company Name** and **Product Name**
3. Under **Resolution and Presentation**:
   - Set Default Screen Width: 1280
   - Set Default Screen Height: 720
   - Fullscreen Mode: Windowed
4. Under **Other Settings**:
   - Scripting Backend: Mono or IL2CPP
   - Target Architecture: x86_64

### Step 3: Build

1. Back in Build Settings, click **Build**
2. Create/select a folder for the build (e.g., `Builds/Windows`)
3. Name your executable (e.g., `SpaceShooter.exe`)
4. Click **Save** and wait for build to complete

### Build Output

Your build folder will contain:
```
Builds/Windows/
├── SpaceShooter.exe           # Main executable
├── SpaceShooter_Data/         # Game data folder
├── UnityCrashHandler64.exe    # Crash handler
└── UnityPlayer.dll            # Unity runtime
```

**Distribute** the entire folder contents together.

---

## 🎨 Optional Enhancements

### Add Sound Effects
1. Import audio files into `Assets/Audio`
2. Add AudioSource components to relevant objects
3. Play sounds on shoot, hit, explosion events

### Add Particle Effects
1. Create particle systems for explosions
2. Instantiate on enemy/player death

### Add Background
1. Create a space background sprite
2. Add scrolling script for parallax effect

### Add Power-ups
1. Create power-up prefabs (health, speed boost, rapid fire)
2. Spawn randomly and handle collection

---

## 🐛 Troubleshooting

### Scripts not compiling
- Ensure all script files are in `Assets/Scripts`
- Check for any missing `using` statements
- Restart Unity Editor

### Bullets not spawning
- Verify Bullet Prefab is assigned in PlayerController
- Check FirePoint is assigned and positioned correctly

### Enemies not taking damage
- Verify tags are set correctly (`Enemy`, `PlayerBullet`)
- Ensure colliders are set as triggers

### UI not updating
- Check UIManager singleton is in scene
- Verify all UI element references are assigned

### Game doesn't pause
- Ensure GameManager is in the scene
- Check that Time.timeScale is being modified

---

## 📋 Unity Version Compatibility

| Unity Version | Status |
|---------------|--------|
| 2021.3 LTS | ✅ Recommended |
| 2022.3 LTS | ✅ Recommended |
| 2023.x | ✅ Compatible |
| 2020.x | ⚠️ Should work |
| 2019.x | ⚠️ May need adjustments |

---

## 📜 License

This project is provided for educational purposes. Feel free to modify and use it for your own projects.

---

## 🎯 Quick Start Checklist

- [ ] Create new Unity 2D project
- [ ] Import all scripts to Assets/Scripts
- [ ] Create required tags (Player, Enemy, PlayerBullet, EnemyBullet)
- [ ] Create Player with PlayerController
- [ ] Create Bullet prefab with BulletController
- [ ] Create Enemy prefab with EnemyController
- [ ] Create GameManager GameObject
- [ ] Create EnemySpawner and assign Enemy prefab
- [ ] Create UI Canvas with all elements
- [ ] Create UIManager and assign UI references
- [ ] Assign Bullet prefab to Player and Enemy
- [ ] Test in Play mode
- [ ] Build for Windows
