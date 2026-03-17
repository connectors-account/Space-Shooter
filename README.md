# 🚀 Space Shooter - Unity 2D Arcade Game

A complete arcade-style space shooter built with Unity and C#. Features wave-based enemy spawning, power-ups, parallax scrolling backgrounds, and full menu system.

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/                    # All C# game scripts
│   │   ├── PlayerController.cs     # Player movement, shooting, health
│   │   ├── EnemyController.cs      # Enemy AI and behavior patterns
│   │   ├── BulletController.cs     # Bullet movement and lifetime
│   │   ├── EnemySpawner.cs         # Wave-based enemy spawning
│   │   ├── GameManager.cs          # Game state, scoring, scene management
│   │   ├── PowerUpController.cs    # Power-up effects and behavior
│   │   ├── UIManager.cs            # HUD, pause menu, game over screen
│   │   ├── MenuManager.cs          # Main menu management
│   │   ├── ParallaxBackground.cs   # Scrolling background layers
│   │   ├── AudioManager.cs         # Sound effects and music management
│   │   └── SpriteGenerator.cs      # Editor tool to generate placeholder sprites
│   ├── Prefabs/                    # Game object prefabs
│   ├── Sprites/                    # Sprite assets
│   │   ├── Player/                 # Player ship sprites
│   │   ├── Enemies/                # Enemy ship sprites
│   │   ├── Bullets/                # Bullet sprites
│   │   ├── PowerUps/               # Power-up sprites
│   │   ├── Background/             # Background layer sprites
│   │   └── UI/                     # UI element sprites
│   ├── Audio/                      # Audio assets
│   │   ├── Music/                  # Background music tracks
│   │   └── SFX/                    # Sound effect clips
│   ├── Scenes/                     # Unity scenes
│   ├── Materials/                  # Materials (if needed)
│   └── Animations/                 # Animation assets
├── ProjectSettings/                # Unity project configuration
│   ├── ProjectSettings.asset       # General project settings
│   ├── TagManager.asset            # Tags, layers, sorting layers
│   ├── Physics2DSettings.asset     # 2D physics configuration
│   └── EditorBuildSettings.asset   # Build scene list
├── Packages/
│   └── manifest.json               # Package dependencies
├── SCENE_SETUP_GUIDE.md            # Detailed scene configuration guide
└── README.md                       # This file
```

---

## 🎮 Game Features

### Controls
| Input | Action |
|-------|--------|
| **WASD / Arrow Keys** | Move player ship |
| **Space** | Fire weapons |
| **Escape** | Pause / Resume game |

### Gameplay
- **Wave System**: Enemies spawn in waves of increasing difficulty
- **3 Enemy Types**:
  - **Basic** (Red diamond) — Moves straight down
  - **Zigzag** (Orange hexagon) — Sine-wave horizontal movement
  - **Heavy** (Dark red square) — Tougher, shoots back at player
- **3 Power-Up Types**:
  - **Weapon Upgrade** (Yellow) — Upgrades to double/triple shot
  - **Shield** (Blue) — Absorbs one hit
  - **Health Restore** (Green cross) — Recovers 30 HP
- **Scoring**: Points awarded per enemy killed, scaling with difficulty
- **High Score**: Persisted using PlayerPrefs

### Technical
- Parallax scrolling background with multiple layers
- Collision detection via Unity 2D physics triggers
- Singleton pattern for GameManager, AudioManager, UIManager
- Scene-based architecture (MainMenu → GamePlay)
- Object pooling-ready architecture

---

## 🛠️ Complete Setup Instructions

### Prerequisites
1. **Unity Hub** — Download from [unity.com/download](https://unity.com/download)
2. **Unity Editor 2021.3 LTS** or newer (2022.x / 2023.x also work)
   - When installing, include **Windows Build Support (Mono)**
3. **Visual Studio 2019/2022** or **VS Code** (for script editing)

---

### Step 1: Create the Unity Project

1. Open **Unity Hub**
2. Click **"New Project"**
3. Select template: **"2D (Built-in Render Pipeline)"**
4. Set project name: `SpaceShooter`
5. Choose a location on your computer
6. Click **"Create Project"**

---

### Step 2: Import Scripts

1. In Unity, navigate to the **Project** window
2. Right-click on the **Assets** folder → **Show in Explorer**
3. Copy the entire contents of this project's `Assets/Scripts/` folder into `YourProject/Assets/Scripts/`
4. Return to Unity — scripts will auto-import and compile

> **Alternative**: Drag and drop the script files directly into Unity's Project window.

---

### Step 3: Configure Tags and Layers

Go to **Edit → Project Settings → Tags and Layers**:

#### Tags (Add these custom tags):
| Tag Name |
|----------|
| `Player` |
| `Enemy` |
| `PlayerBullet` |
| `EnemyBullet` |
| `PowerUp` |

#### Sorting Layers (Add in this order):
| Order | Layer Name |
|-------|------------|
| 0 | Default |
| 1 | Background |
| 2 | Gameplay |
| 3 | UI |

#### Layers (Optional, for collision matrix):
| Layer # | Name |
|---------|------|
| 8 | Player |
| 9 | Enemy |
| 10 | Bullet |
| 11 | PowerUp |

---

### Step 4: Generate Placeholder Sprites

1. In Unity, go to menu: **Tools → Generate Placeholder Sprites**
2. This runs `SpriteGenerator.cs` and creates all PNG sprite files
3. Sprites appear in their respective `Assets/Sprites/` subfolders

> **If the menu item doesn't appear**, ensure `SpriteGenerator.cs` is in the `Assets/Scripts/` folder and has no compilation errors.

#### Manual Alternative — Create sprites manually:
If you prefer, create simple colored shapes in any image editor:
- **PlayerShip.png** — 64×64, blue upward-pointing triangle
- **EnemyBasic.png** — 48×48, red diamond shape
- **EnemyZigzag.png** — 48×48, orange hexagonal shape
- **EnemyHeavy.png** — 56×56, dark red square with cross detail
- **PlayerBullet.png** — 8×16, cyan elongated dot
- **EnemyBullet.png** — 8×16, red elongated dot
- **PowerUpWeapon.png** — 32×32, yellow circle
- **PowerUpShield.png** — 32×32, blue ring
- **PowerUpHealth.png** — 32×32, green cross
- **BackgroundStarsFar.png** — 512×1024, dark space with dim stars
- **BackgroundStarsNear.png** — 512×1024, transparent with bright stars

Import settings for all sprites:
- **Texture Type**: Sprite (2D and UI)
- **Pixels Per Unit**: 100
- **Filter Mode**: Point (for pixel art look) or Bilinear
- **Compression**: None

---

### Step 5: Configure Physics 2D

Go to **Edit → Project Settings → Physics 2D**:
- Set **Gravity** to `(0, 0)` — space has no gravity!

---

### Step 6: Create Prefabs

#### 6.1 Player Ship Prefab
1. Create empty GameObject, name it `Player`
2. Add components:
   - **SpriteRenderer** → Assign `PlayerShip` sprite, Sorting Layer: `Gameplay`
   - **Rigidbody2D** → Body Type: `Kinematic`
   - **BoxCollider2D** → Check `Is Trigger`, adjust size to fit sprite
   - **PlayerController** script
3. Create child empty GameObject named `FirePoint`, position at `(0, 0.5, 0)`
4. On PlayerController:
   - Assign `PlayerBullet` prefab to **Bullet Prefab**
   - Assign `FirePoint` transform to **Fire Point**
5. Set tag to `Player`
6. Drag from Hierarchy to `Assets/Prefabs/` to create prefab

#### 6.2 Player Bullet Prefab
1. Create empty GameObject, name it `PlayerBullet`
2. Add components:
   - **SpriteRenderer** → Assign `PlayerBullet` sprite, Sorting Layer: `Gameplay`
   - **Rigidbody2D** → Body Type: `Kinematic`
   - **CircleCollider2D** → Check `Is Trigger`
   - **BulletController** script → Set Damage to `10`
3. Set tag to `PlayerBullet`
4. Drag to `Assets/Prefabs/`

#### 6.3 Enemy Bullet Prefab
1. Same as Player Bullet, but:
   - Name: `EnemyBullet`
   - Sprite: `EnemyBullet`
   - Tag: `EnemyBullet`
2. Drag to `Assets/Prefabs/`

#### 6.4 Basic Enemy Prefab
1. Create empty GameObject, name it `EnemyBasic`
2. Add components:
   - **SpriteRenderer** → Assign `EnemyBasic` sprite, Sorting Layer: `Gameplay`
   - **Rigidbody2D** → Body Type: `Kinematic`
   - **BoxCollider2D** → Check `Is Trigger`
   - **EnemyController** script:
     - Enemy Type: `Basic`
     - Move Speed: `3`
     - Health: `20`
     - Score Value: `100`
     - Assign power-up prefabs to **Power Up Prefabs** array
3. Set tag to `Enemy`
4. Drag to `Assets/Prefabs/`

#### 6.5 Zigzag Enemy Prefab
1. Same as Basic, but:
   - Name: `EnemyZigzag`
   - Sprite: `EnemyZigzag`
   - Enemy Type: `Zigzag`
   - Move Speed: `2.5`
   - Health: `15`
   - Score Value: `150`
   - Zigzag Amplitude: `2`, Frequency: `2`
2. Drag to `Assets/Prefabs/`

#### 6.6 Heavy Enemy Prefab
1. Same as Basic, but:
   - Name: `EnemyHeavy`
   - Sprite: `EnemyHeavy`
   - Enemy Type: `Heavy`
   - Move Speed: `1.5`
   - Health: `50`
   - Score Value: `300`
   - Assign `EnemyBullet` prefab to **Enemy Bullet Prefab**
   - Fire Rate: `1.5`
2. Drag to `Assets/Prefabs/`

#### 6.7 Power-Up Prefabs (create 3)

**Weapon Upgrade:**
1. Create GameObject `PowerUpWeapon`
2. Add: SpriteRenderer (`PowerUpWeapon` sprite), Rigidbody2D (Kinematic), CircleCollider2D (Trigger), PowerUpController
3. Set Power Up Type: `WeaponUpgrade`, Tag: `PowerUp`
4. Drag to Prefabs

**Shield:**
1. Same but name `PowerUpShield`, sprite `PowerUpShield`, type `Shield`

**Health Restore:**
1. Same but name `PowerUpHealth`, sprite `PowerUpHealth`, type `HealthRestore`

---

### Step 7: Set Up Scenes

#### 7.1 MainMenu Scene

1. **File → New Scene**, save as `Assets/Scenes/MainMenu.unity`
2. Set Camera background to dark space color: `#050510`

**Create Canvas for UI:**
1. **GameObject → UI → Canvas**
   - Canvas Scaler → UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `800 × 600`

2. Add UI elements as children of Canvas:
   - **Title Text**: `UI → Text`
     - Text: "SPACE SHOOTER"
     - Font Size: 48, Color: White, Alignment: Center
     - Position: `(0, 150, 0)`
   - **High Score Text**: `UI → Text`
     - Font Size: 24, Color: Yellow
     - Position: `(0, 50, 0)`
   - **Play Button**: `UI → Button`
     - Child Text: "PLAY"
     - Position: `(0, -50, 0)`, Size: `200 × 50`
   - **Quit Button**: `UI → Button`
     - Child Text: "QUIT"
     - Position: `(0, -120, 0)`, Size: `200 × 50`
   - **Version Text**: `UI → Text`
     - Font Size: 14, Position: bottom-right corner

3. Add `MenuManager` script to Canvas
4. Assign all UI references in the Inspector

**Create persistent managers (ONLY in MainMenu scene):**
1. Create empty GameObject `GameManager`
   - Add `GameManager` script
2. Create empty GameObject `AudioManager`
   - Add `AudioManager` script
   - Add 2 AudioSource components (one for music, one for SFX)
   - Assign the AudioSources and any audio clips

> Both use `DontDestroyOnLoad` and will persist across scenes.

#### 7.2 GamePlay Scene

1. **File → New Scene**, save as `Assets/Scenes/GamePlay.unity`
2. Set Camera background to: `#050510`

**Background layers:**
1. Create 2 GameObjects with SpriteRenderer:
   - `BackgroundFar`: Sprite = `BackgroundStarsFar`, Sorting Layer = `Background`, Order = 0
     - Add `ParallaxBackground` script, Scroll Speed = `0.5`
   - `BackgroundNear`: Sprite = `BackgroundStarsNear`, Sorting Layer = `Background`, Order = 1
     - Add `ParallaxBackground` script, Scroll Speed = `1.5`
2. Duplicate each background and offset by sprite height for seamless tiling

**Player:**
1. Drag `Player` prefab into scene at position `(0, -3, 0)`

**Enemy Spawner:**
1. Create empty GameObject `EnemySpawner`
2. Add `EnemySpawner` script
3. Assign all 3 enemy prefabs in the Inspector

**UI Canvas (GamePlay HUD):**
1. **GameObject → UI → Canvas** (same Canvas Scaler settings as MainMenu)
2. Add `UIManager` script to Canvas
3. Create these UI elements:

   **HUD (always visible):**
   - **Score Text** — Top-left, "Score: 0"
   - **Wave Text** — Top-center, "Wave: 1"
   - **Health Bar** — `UI → Slider` at top-right
     - Uncheck "Interactable"
     - Set Width: 200, Height: 20
     - Background: dark red, Fill: green
     - Assign Fill Image to healthBarFill on UIManager

   **Wave Announcement Panel (hidden by default):**
   - Panel with Text "WAVE X", centered, large font
   - Set inactive in Inspector

   **Pause Menu Panel (hidden by default):**
   - Semi-transparent dark panel covering screen
   - "PAUSED" title text
   - Resume Button, Main Menu Button, Quit Button
   - Set inactive in Inspector

   **Game Over Panel (hidden by default):**
   - Semi-transparent dark panel
   - "GAME OVER" title
   - Final Score Text, High Score Text
   - Restart Button, Main Menu Button
   - Set inactive in Inspector

4. Wire all UI references to `UIManager` script fields in Inspector

#### 7.3 Add Scenes to Build Settings
1. **File → Build Settings**
2. Click **"Add Open Scenes"** for each scene, or drag from Project window
3. Ensure order is:
   - `Scenes/MainMenu` — Index 0
   - `Scenes/GamePlay` — Index 1

---

### Step 8: Configure Collision Matrix (Optional)

Go to **Edit → Project Settings → Physics 2D → Layer Collision Matrix**:

| | Player | Enemy | Bullet | PowerUp |
|---|--------|-------|--------|---------|
| **Player** | ✗ | ✓ | ✓ | ✓ |
| **Enemy** | ✓ | ✗ | ✓ | ✗ |
| **Bullet** | ✓ | ✓ | ✗ | ✗ |
| **PowerUp** | ✓ | ✗ | ✗ | ✗ |

---

### Step 9: Add Audio (Optional)

Create or download `.wav` / `.ogg` sound effect files and place them in `Assets/Audio/SFX/`:

| SFX Name (in AudioManager) | Suggested File | Description |
|---|---|---|
| `PlayerShoot` | `laser_shoot.wav` | Short laser sound |
| `PlayerHurt` | `player_hit.wav` | Impact/hurt sound |
| `PlayerDeath` | `explosion_large.wav` | Large explosion |
| `EnemyExplosion` | `explosion_small.wav` | Small explosion |
| `PowerUp` | `powerup_collect.wav` | Positive chime |

For background music, place in `Assets/Audio/Music/`:
| Clip | File | Description |
|---|---|---|
| Menu Music | `menu_theme.ogg` | Calm space ambience |
| Game Music | `battle_theme.ogg` | Upbeat action track |

**Free audio resources:**
- [OpenGameArt.org](https://opengameart.org/)
- [Freesound.org](https://freesound.org/)
- [Kenney.nl](https://kenney.nl/assets?q=audio)

In the `AudioManager` Inspector:
1. Assign Music and SFX AudioSource components
2. Assign menu/game music clips
3. Add entries to the **Sound Effects** array with matching names

---

### Step 10: Test the Game

1. Open the `MainMenu` scene
2. Press **Play** (▶) in Unity Editor
3. Click "PLAY" button → Game should transition to GamePlay scene
4. Test controls: WASD to move, Space to shoot, Escape to pause
5. Verify enemies spawn, bullets work, power-ups drop, scoring works

---

## 🏗️ Build as Windows Executable (.exe)

### Build Steps

1. **File → Build Settings** (Ctrl+Shift+B)
2. Select **Platform**: `Windows, Mac, Linux` (or `PC, Mac & Linux Standalone`)
   - If not available, click **"Install with Unity Hub"** to add the build module
3. Set **Target Platform**: `Windows`
4. Set **Architecture**: `x86_64` (64-bit)
5. Verify scenes are listed:
   - `Scenes/MainMenu` — Index 0 ✓
   - `Scenes/GamePlay` — Index 1 ✓
6. Click **"Player Settings"** and configure:
   - **Product Name**: `Space Shooter`
   - **Company Name**: Your name
   - **Default Screen Width**: `800`
   - **Default Screen Height**: `600`
   - **Fullscreen Mode**: `Windowed` (or `Fullscreen Window`)
   - **Resizable Window**: ✓
   - **Run In Background**: ✓ (optional)
7. Click **"Build"**
8. Choose output folder (e.g., `Builds/Windows/`)
9. Wait for build to complete

### Build Output
```
Builds/Windows/
├── Space Shooter.exe          ← Run this!
├── Space Shooter_Data/        ← Game data (required)
├── MonoBleedingEdge/          ← Mono runtime (required)
└── UnityPlayer.dll            ← Unity runtime (required)
```

### Distribution
To share the game, zip the **entire output folder** — all files are required to run.

---

## 🎯 Game Architecture Overview

```
GameManager (Singleton, DontDestroyOnLoad)
├── Manages game state (playing, paused, game over)
├── Score tracking and high score persistence
├── Scene transitions
└── References to Player and EnemySpawner

AudioManager (Singleton, DontDestroyOnLoad)
├── Named SFX playback via dictionary
├── Background music management
└── Volume control

UIManager (Per-scene Singleton)
├── HUD: Score, Wave, Health Bar
├── Wave announcement overlay
├── Pause menu panel
└── Game over panel

PlayerController
├── WASD/Arrow movement (screen-clamped)
├── Space to shoot (weapon levels 1-3)
├── Health system with invincibility frames
├── Power-up application
└── Collision handling (triggers)

EnemyController
├── 3 behavior types: Basic, Zigzag, Heavy
├── Difficulty scaling per wave
├── Power-up drop on death
└── Score awarding

EnemySpawner
├── Coroutine-based wave spawning
├── Progressive difficulty (more enemies, faster spawns)
├── Weighted enemy type selection
└── Wave clear detection

BulletController
├── Directional movement
├── Auto-destruction on lifetime
└── Damage value

PowerUpController
├── 3 types: WeaponUpgrade, Shield, HealthRestore
├── Floating/bobbing movement
└── Expiration with visual warning

ParallaxBackground
├── Auto-scrolling layers
├── Seamless tiling reset
└── Speed control
```

---

## 🔧 Customization

### Difficulty Tuning
Edit `EnemySpawner` Inspector values:
- `Base Enemies Per Wave`: Starting enemy count (default: 5)
- `Enemies Per Wave Increase`: Added per wave (default: 2)
- `Time Between Spawns`: Seconds between spawns (default: 0.8)
- `Time Between Waves`: Rest time between waves (default: 4)

### Player Stats
Edit `PlayerController` Inspector values:
- `Move Speed`: Player movement speed (default: 8)
- `Fire Rate`: Seconds between shots (default: 0.2)
- `Max Health`: Starting health (default: 100)

### Enemy Stats (per prefab)
- `Move Speed`, `Health`, `Score Value`
- `Fire Rate` (Heavy only)
- `Power Up Drop Chance` (default: 15%)

---

## 📝 Troubleshooting

| Issue | Solution |
|-------|----------|
| Scripts have errors | Ensure Unity 2021.3+ is used. Check Console for missing references. |
| No enemies spawn | Verify EnemySpawner has prefabs assigned and is in GamePlay scene. |
| Bullets don't hit | Check that colliders are set to **Is Trigger** and tags match. |
| Game doesn't transition | Verify both scenes are in **Build Settings** scene list. |
| No sound | Assign AudioClips in AudioManager Inspector. SFX names must match. |
| Player falls off screen | Ensure Physics 2D gravity is set to `(0, 0)`. |
| UI not showing | Check Canvas has UIManager/MenuManager with all fields assigned. |

---

## 📄 License

This project is provided as-is for educational and personal use. Feel free to modify, extend, and distribute.

---

*Built with Unity 2021.3 LTS+ | C# | 2D Built-in Render Pipeline*
