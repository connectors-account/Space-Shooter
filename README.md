# 🚀 Space Shooter - Unity Arcade Game

A complete arcade-style space shooter game built with Unity for Windows desktop. Features progressive wave-based enemy spawning, power-ups, combo scoring, and a full menu system.

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   └── PlayerController.cs      # Player movement, shooting, power-ups, damage
│   │   ├── Enemy/
│   │   │   ├── EnemyBase.cs             # Base enemy class (health, scoring, destruction)
│   │   │   ├── BasicEnemy.cs            # Straight-down movement, single shots
│   │   │   ├── FastEnemy.cs             # Zigzag movement, hard to hit
│   │   │   ├── TankEnemy.cs             # Slow, high HP, spread shots
│   │   │   └── BomberEnemy.cs           # Horizontal flyer, drops bombs
│   │   ├── Weapons/
│   │   │   ├── Bullet.cs               # Player projectile system
│   │   │   └── EnemyBullet.cs          # Enemy projectile system
│   │   ├── Managers/
│   │   │   ├── GameManager.cs           # Game state (menu/play/pause/gameover)
│   │   │   ├── SpawnManager.cs          # Wave-based enemy & power-up spawning
│   │   │   ├── ScoreManager.cs          # Scoring with combo multiplier
│   │   │   ├── HealthManager.cs         # Reusable health component
│   │   │   └── InputManager.cs          # Centralized keyboard input
│   │   ├── PowerUps/
│   │   │   ├── PowerUpBase.cs           # Base power-up pickup behavior
│   │   │   ├── ShieldPowerUp.cs         # Absorbs one hit
│   │   │   ├── MultiShotPowerUp.cs      # Triple-shot for 5 seconds
│   │   │   ├── SpeedBoostPowerUp.cs     # 1.5x speed for 5 seconds
│   │   │   └── HealthPowerUp.cs         # Restores 30 HP
│   │   ├── Environment/
│   │   │   ├── ParallaxBackground.cs    # Single-layer parallax scroll
│   │   │   ├── BackgroundManager.cs     # Multi-layer parallax manager
│   │   │   └── ScreenBounds.cs          # Off-screen object cleanup
│   │   ├── UI/
│   │   │   ├── UIManager.cs             # In-game HUD & overlays
│   │   │   ├── MainMenuUI.cs            # Main menu screen
│   │   │   └── GameOverUI.cs            # Game over screen
│   │   ├── Audio/
│   │   │   └── AudioManager.cs          # Music & SFX management
│   │   ├── Effects/
│   │   │   └── Explosion.cs             # Auto-destroy explosion effect
│   │   └── Utilities/
│   │       ├── PlaceholderSpriteGenerator.cs  # Runtime placeholder sprites
│   │       └── GameBootstrapper.cs            # Auto-setup for quick testing
│   ├── Prefabs/
│   │   ├── Player/          # Player ship prefab
│   │   ├── Enemies/         # Enemy prefabs (Basic, Fast, Tank, Bomber)
│   │   ├── Bullets/         # Player & enemy bullet prefabs
│   │   ├── PowerUps/        # Power-up pickup prefabs
│   │   └── Effects/         # Explosion & visual effects
│   ├── Scenes/              # MainMenu & GameScene
│   ├── Sprites/             # All sprite assets organized by category
│   ├── Audio/
│   │   ├── Music/           # Background music tracks
│   │   └── SFX/             # Sound effect clips
│   ├── Materials/           # Shader materials
│   └── Animations/          # Sprite animations
└── ProjectSettings/         # Unity project settings
```

---

## 🎮 Game Features

### Core Gameplay
- **Player Ship**: 8-directional movement (WASD/Arrow Keys), continuous fire (Space)
- **4 Enemy Types**:
  - **Basic**: Moves straight down, fires single bullets
  - **Fast**: Zigzag movement, low HP but hard to hit
  - **Tank**: Slow, high HP, fires 3-shot spread patterns
  - **Bomber**: Flies horizontally, drops bombs downward
- **Wave Progression**: Each wave spawns more enemies, faster, with harder types

### Power-Up System
| Power-Up | Color | Effect | Duration |
|----------|-------|--------|----------|
| Shield | Blue | Absorbs one hit | Until hit |
| Multi-Shot | Orange | Triple bullet spread | 5 seconds |
| Speed Boost | Yellow | 1.5x movement speed | 5 seconds |
| Health | Green | Restores 30 HP | Instant |

### Scoring
- Each enemy type awards different base points
- **Combo System**: Kill enemies within 2 seconds for multiplier (up to x5)
- High score persists between sessions (PlayerPrefs)

### Controls
| Key | Action |
|-----|--------|
| WASD / Arrow Keys | Move ship |
| Space | Fire weapons |
| Escape | Pause / Resume |

---

## 🛠️ Setup Instructions

### Prerequisites
- **Unity 2021.3 LTS** or newer (2022.x or 2023.x also work)
- **Windows 10/11** for building the executable
- Unity modules: **Windows Build Support** (installed via Unity Hub)

### Step 1: Create Unity Project

1. Open **Unity Hub** → Click **New Project**
2. Select **2D (URP)** or **2D (Built-in)** template
3. Name it `SpaceShooter` and create it
4. **Close Unity** after the project is created

### Step 2: Copy Scripts Into the Project

1. Navigate to your new Unity project folder
2. Copy the entire contents of `Assets/Scripts/` from this repository into your Unity project's `Assets/Scripts/` folder
3. The folder structure should match what's shown in the project structure above

### Step 3: Open in Unity and Set Up Tags/Layers

1. Open the project in Unity
2. Go to **Edit → Project Settings → Tags and Layers**
3. Add these tags if they don't exist:
   - `Player`
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`
   - `PowerUp`
4. Add these sorting layers (in order):
   - `Background` (bottom)
   - `Default`
   - `Enemies`
   - `Player`
   - `Projectiles`
   - `UI` (top)

### Step 4: Configure Physics 2D

1. Go to **Edit → Project Settings → Physics 2D**
2. Open the **Layer Collision Matrix** and disable collisions between:
   - `PlayerBullet` ↔ `Player` (player can't shoot itself)
   - `EnemyBullet` ↔ `Enemy` (enemies can't shoot each other)
   - `PlayerBullet` ↔ `PlayerBullet` (bullets don't collide)
   - `EnemyBullet` ↔ `EnemyBullet`

### Step 5: Create the Main Menu Scene

1. **File → New Scene** → Save as `Assets/Scenes/MainMenu.unity`
2. Create a **Canvas** (GameObject → UI → Canvas):
   - Set Canvas Scaler to **Scale With Screen Size**, Reference: 1920×1080
3. Add UI elements:
   - **Title Text**: "SPACE SHOOTER" (centered, top portion, font size 72)
   - **High Score Text**: Below title (font size 36)
   - **Start Button**: Center of screen
   - **Quit Button**: Below Start button
4. Create an **empty GameObject** named `MainMenuController`:
   - Add `MainMenuUI` script
   - Drag UI elements to the inspector fields
5. Create an **empty GameObject** named `GameManager`:
   - Add `GameManager` script
   - This will persist across scenes (DontDestroyOnLoad)
6. Create an **empty GameObject** named `AudioManager`:
   - Add `AudioManager` script
   - Add two **AudioSource** components and assign to Music/SFX fields
   - This will also persist across scenes
7. **Add both scenes to Build Settings** (File → Build Settings → Add Open Scenes)

### Step 6: Create the Game Scene

1. **File → New Scene** → Save as `Assets/Scenes/GameScene.unity`
2. Set up the **Camera**:
   - Orthographic, Size = 5
   - Background color: dark blue/black `(#050515)`
3. Create these **empty GameObjects** and attach scripts:

| GameObject | Script(s) | Notes |
|-----------|-----------|-------|
| `GameBootstrapper` | `GameBootstrapper` | Enable "Use Bootstrapper" for quick testing |
| `SpawnManager` | `SpawnManager` | Assign enemy/power-up prefabs |
| `ScoreManager` | `ScoreManager` | Combo settings |
| `InputManager` | `InputManager` | Input polling |
| `PlaceholderSpriteGen` | `PlaceholderSpriteGenerator` | Dev tool, remove for production |

4. Create the **HUD Canvas** (same Canvas Scaler settings):
   - Top-left: Score Text, Wave Text
   - Top-right: Lives Text
   - Top-center: Health Bar (Slider)
   - Center: Wave Announcement text (hidden by default)
   - Center: Combo text (hidden by default)
   - Full-screen panel: Game Over panel (hidden by default)
     - Contains: Score, High Score, Restart Button, Main Menu Button
   - Full-screen panel: Pause panel (hidden by default)
     - Contains: "PAUSED" text, Resume Button, Main Menu Button
5. Add `UIManager` script to Canvas and wire up all references

### Step 7: Create Prefabs

#### Quick Testing (No Art Required)
Enable the `GameBootstrapper` component in the GameScene. It automatically generates:
- A player ship (cyan triangle)
- Placeholder bullets (yellow/red circles)
- A scrolling starfield background
- Shield visual (semi-transparent blue circle)

#### Production Prefabs

**Player Prefab** (`Assets/Prefabs/Player/Player.prefab`):
1. Create a new empty GameObject → name it `Player`
2. Add components: `SpriteRenderer`, `BoxCollider2D` (Is Trigger ✓), `Rigidbody2D` (Kinematic), `HealthManager`, `PlayerController`
3. Set tag to `Player`
4. Create child `FirePoint` at local position (0, 0.6, 0)
5. Create child `Shield` with SpriteRenderer (semi-transparent blue circle)
6. Drag to Prefabs folder

**Enemy Prefabs** (`Assets/Prefabs/Enemies/`):
For each enemy type (Basic, Fast, Tank, Bomber):
1. Create GameObject with `SpriteRenderer`, `BoxCollider2D` (Is Trigger ✓), `Rigidbody2D` (Kinematic)
2. Add the appropriate enemy script (e.g., `BasicEnemy`)
3. Set tag to `Enemy`
4. Assign bullet prefab references for enemies that shoot
5. Drag to Prefabs folder

**Bullet Prefabs** (`Assets/Prefabs/Bullets/`):
1. **PlayerBullet**: `SpriteRenderer` + `BoxCollider2D` (Trigger) + `Rigidbody2D` (Kinematic) + `Bullet` script
   - Set `isPlayerBullet = true`, `speed = 12`, `damage = 25`
2. **EnemyBullet**: Same setup + `EnemyBullet` script
   - Set `speed = 6`, `damage = 20`

**Power-Up Prefabs** (`Assets/Prefabs/PowerUps/`):
For each power-up (Shield, MultiShot, SpeedBoost, Health):
1. Create GameObject with `SpriteRenderer`, `CircleCollider2D` (Trigger), `Rigidbody2D` (Kinematic)
2. Add the appropriate power-up script
3. Set tag to `PowerUp`
4. Use distinctive colors: Shield=Blue, MultiShot=Orange, Speed=Yellow, Health=Green
5. Drag to Prefabs folder

**Explosion Prefab** (`Assets/Prefabs/Effects/Explosion.prefab`):
1. Create GameObject with `SpriteRenderer` or `ParticleSystem`
2. Add `Explosion` script (auto-destroys after 1 second)

### Step 8: Wire Up Prefab References

1. Select `SpawnManager` in the GameScene hierarchy
2. In the Inspector, drag and drop:
   - `BasicEnemy` prefab → Basic Enemy Prefab slot
   - `FastEnemy` prefab → Fast Enemy Prefab slot
   - `TankEnemy` prefab → Tank Enemy Prefab slot
   - `BomberEnemy` prefab → Bomber Enemy Prefab slot
   - Power-up prefabs → their respective slots
3. Select the `Player` prefab and assign:
   - `PlayerBullet` prefab → Bullet Prefab
   - `PlayerBullet` prefab → Triple Shot Bullet Prefab (or create a variant)

---

## 🎨 Sprite Recommendations

For placeholder/prototype sprites, you can use:

### Free Asset Sources
- **Kenney.nl**: [Space Shooter Redux](https://kenney.nl/assets/space-shooter-redux) (CC0 license, free)
- **OpenGameArt.org**: Search "space shooter sprites"
- **Unity Asset Store**: Search for free 2D space shooter packs

### Minimum Sprites Needed
| Asset | Suggested Size | Description |
|-------|---------------|-------------|
| `player_ship.png` | 64×64 | Player spacecraft |
| `enemy_basic.png` | 48×48 | Basic enemy |
| `enemy_fast.png` | 32×48 | Fast/small enemy |
| `enemy_tank.png` | 64×64 | Large tank enemy |
| `enemy_bomber.png` | 56×48 | Bomber enemy |
| `bullet_player.png` | 8×16 | Player projectile |
| `bullet_enemy.png` | 8×16 | Enemy projectile |
| `powerup_shield.png` | 32×32 | Blue shield icon |
| `powerup_multishot.png` | 32×32 | Orange triple-shot icon |
| `powerup_speed.png` | 32×32 | Yellow speed icon |
| `powerup_health.png` | 32×32 | Green health icon |
| `explosion.png` | 64×64 | Explosion sprite (or spritesheet) |
| `background_stars.png` | 512×1024 | Tiling starfield |
| `shield_bubble.png` | 80×80 | Semi-transparent shield around player |

### Sprite Import Settings
For all sprites in Unity:
1. Select sprite in Project window
2. In Inspector:
   - **Texture Type**: Sprite (2D and UI)
   - **Sprite Mode**: Single
   - **Pixels Per Unit**: 32 (for pixel art) or 100 (for HD)
   - **Filter Mode**: Point (pixel art) or Bilinear (smooth)
   - **Compression**: None (for pixel art)

---

## 🔊 Audio Recommendations

### Sound Effects Needed
| SFX Name (in AudioManager) | Description | Suggested Source |
|---------------------------|-------------|-----------------|
| `PlayerShoot` | Laser/blaster fire | [sfxr](https://sfxr.me/) - "Laser" preset |
| `EnemyShoot` | Different pitch laser | sfxr - "Laser" with lower pitch |
| `PlayerHit` | Impact/damage sound | sfxr - "Hit" preset |
| `PlayerExplosion` | Large explosion | sfxr - "Explosion" preset |
| `EnemyExplosion` | Smaller explosion | sfxr - "Explosion" (shorter) |
| `PowerUp` | Positive pickup jingle | sfxr - "Powerup" preset |
| `ShieldHit` | Energy absorption | sfxr - "Blip" preset |
| `ButtonClick` | UI click | sfxr - "Blip" preset |

### Free Audio Sources
- **[sfxr.me](https://sfxr.me/)**: Generate retro sound effects in-browser
- **[freesound.org](https://freesound.org)**: Community sound library
- **[opengameart.org](https://opengameart.org)**: Game music and SFX

### Setting Up AudioManager
1. In the `AudioManager` Inspector, expand `SFX Clips` array
2. Set array size to 8
3. For each entry, set the `Name` (must match the names in the table above) and drag the AudioClip

---

## 🏗️ Build Instructions (Windows .exe)

### Step 1: Verify Build Settings

1. Open Unity → **File → Build Settings**
2. Ensure **Windows, Mac, Linux** platform is selected (click **Switch Platform** if not)
3. **Add scenes** in this order:
   - `Scenes/MainMenu` (Index 0 — this loads first)
   - `Scenes/GameScene` (Index 1)
4. Architecture: **x86_64** (recommended) or **x86**

### Step 2: Configure Player Settings

1. Click **Player Settings** in Build Settings window
2. Under **Product Name**: `Space Shooter`
3. Under **Company Name**: Your name/studio
4. Under **Resolution and Presentation**:
   - Default Screen Width: `1920`
   - Default Screen Height: `1080`
   - Fullscreen Mode: `Windowed` (or `Fullscreen Window`)
   - Allow Fullscreen Switch: ✓
5. Under **Other Settings**:
   - API Compatibility Level: `.NET Standard 2.1`
   - Color Space: `Linear` (recommended) or `Gamma`
6. Under **Icon**: Optionally set a custom application icon

### Step 3: Build the Executable

1. Go to **File → Build Settings**
2. Click **Build** (or **Build And Run** to test immediately)
3. Choose/create a folder (e.g., `Builds/Windows`)
4. Name the executable: `SpaceShooter.exe`
5. Click **Save** — Unity will compile and build
6. The output folder will contain:
   ```
   Builds/Windows/
   ├── SpaceShooter.exe              ← Run this!
   ├── SpaceShooter_Data/            ← Required game data
   ├── UnityCrashHandler64.exe
   └── UnityPlayer.dll
   ```

### Step 4: Distribute

To share the game:
1. **Zip the entire build folder** (all files are required)
2. The recipient just unzips and runs `SpaceShooter.exe`
3. No Unity installation needed to play

---

## 🧪 Quick Testing Guide

### Fastest Way to Test (No Assets Needed)

1. Set up the two scenes (MainMenu + GameScene) as described above
2. In GameScene, create a `GameBootstrapper` GameObject with the script enabled
3. Make sure `GameManager` and `AudioManager` exist in MainMenu scene
4. Press **Play** — the bootstrapper auto-generates placeholder graphics
5. Use WASD to move, Space to shoot

### What to Verify
- [ ] Player moves with WASD/Arrow keys within screen bounds
- [ ] Bullets fire when holding Space
- [ ] Enemies spawn in waves and move downward
- [ ] Enemies take damage and explode when shot
- [ ] Player takes damage on collision with enemies/bullets
- [ ] Score increases when enemies are destroyed
- [ ] Combo multiplier works for rapid kills
- [ ] Power-ups spawn periodically and apply effects
- [ ] Shield absorbs one hit then disappears
- [ ] Wave counter increases between waves
- [ ] Pause works with Escape key
- [ ] Game Over screen appears when lives reach 0
- [ ] Restart and Main Menu buttons work
- [ ] High score persists between sessions

---

## 🔧 Customization

### Difficulty Tuning
Key values to adjust in the Inspector:

| Component | Property | Default | Effect |
|-----------|----------|---------|--------|
| `SpawnManager` | Base Enemies Per Wave | 5 | Starting enemy count |
| `SpawnManager` | Enemies Per Wave Increase | 2 | More enemies each wave |
| `SpawnManager` | Spawn Delay Base | 1.5s | Time between enemy spawns |
| `SpawnManager` | Power Up Spawn Chance | 15% | Likelihood of power-ups |
| `PlayerController` | Move Speed | 8 | Player movement speed |
| `PlayerController` | Fire Rate | 0.2s | Shots per second |
| `HealthManager` (Player) | Max Health | 100 | Player hit points |
| `GameManager` | Starting Lives | 3 | Lives per game |
| `ScoreManager` | Combo Window | 2s | Time to chain combo kills |

### Adding New Enemy Types
1. Create a new script inheriting from `EnemyBase`
2. Override `Move()` for custom movement
3. Override `Attack()` for custom shooting
4. Set `maxHealth`, `scoreValue`, `moveSpeed` in `Start()`
5. Create a prefab and add it to `SpawnManager`

### Adding New Power-Ups
1. Create a new script inheriting from `PowerUpBase`
2. Override `ApplyPowerUp(PlayerController player)`
3. Create a prefab with a distinctive color
4. Add the prefab reference to `SpawnManager`

---

## 📝 License

This project template is provided as-is for educational and personal use. Art and audio assets should be sourced from properly licensed resources.

---

## 💡 Tips

- **Performance**: Enemy and bullet objects are instantiated/destroyed frequently. For better performance in large wave counts, consider implementing an **Object Pool**.
- **Mobile Port**: The input system uses `Input.GetAxisRaw` which works with virtual joysticks. Add touch controls for mobile.
- **Visual Polish**: Add particle effects for engine thrust, bullet impacts, and explosions using Unity's Particle System.
- **Screen Shake**: Add a camera shake effect on player damage for more game feel.
