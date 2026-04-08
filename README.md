# Space Shooter - Unity Game

A complete 2D space shooter game built with Unity and C#. All sprites are generated
procedurally at runtime — no external art assets required.

## Game Features

- **Player ship** with WASD/Arrow key movement + Space to shoot
- **3 enemy types**: Scout (straight), Weaver (sine-wave), Gunship (shoots back)
- **Wave system** with increasing difficulty (more enemies, faster, tougher)
- **3 power-ups**: Shield (absorbs 1 hit), Rapid Fire (triple shot for 8s), Health (+2 HP)
- **Parallax starfield** background with particle effects
- **Full UI**: Main Menu, HUD (score + health), Pause Menu (Esc), Game Over screen
- **High score** persistence via PlayerPrefs
- **Audio integration points** — all SFX calls wired, just drop in AudioClips

---

## Requirements

| Requirement | Version |
|---|---|
| **Unity Editor** | **2022.3 LTS** (any 2022.3.x) — also works on 2021.3 LTS and Unity 6 |
| **Build Support** | Windows Build Support module (installed via Unity Hub) |
| **OS** | Windows 10/11 for building .exe |

> **Download Unity**: https://unity.com/download
> Install via **Unity Hub** → Installs → Install Editor → 2022.3 LTS
> Make sure to check **"Windows Build Support (IL2CPP)"** or **"Mono"** during install.

---

## Project Structure

```
SpaceShooterUnity/
├── Assets/
│   ├── Scripts/                    # All C# game code
│   │   ├── PlayerController.cs     # Player movement, shooting, health
│   │   ├── Bullet.cs               # Bullet movement and lifetime
│   │   ├── Enemy.cs                # Enemy AI, health, movement patterns
│   │   ├── EnemySpawner.cs         # Wave system with difficulty scaling
│   │   ├── PowerUp.cs              # Power-up pickup behavior
│   │   ├── PowerUpSpawner.cs       # Periodic and drop-based spawning
│   │   ├── GameManager.cs          # Game state (menu/play/pause/gameover)
│   │   ├── UIManager.cs            # All UI updates and button handlers
│   │   ├── AudioManager.cs         # Centralized SFX/music playback
│   │   ├── SpriteGenerator.cs      # Procedural pixel-art sprite creation
│   │   ├── BackgroundStarfield.cs  # Particle-based scrolling stars
│   │   ├── ParallaxBackground.cs   # Sprite-based parallax scrolling
│   │   ├── GamePlaySetup.cs        # Wires GamePlay scene to GameManager
│   │   ├── SceneSetup_MainMenu.cs  # Builds Main Menu UI at runtime
│   │   ├── SceneSetup_GamePlay.cs  # Builds entire gameplay scene at runtime
│   │   └── SceneSetup_GameOver.cs  # Builds Game Over UI at runtime
│   ├── Scenes/
│   │   ├── MainMenu.unity          # Main menu scene
│   │   ├── GamePlay.unity          # Core gameplay scene
│   │   └── GameOver.unity          # Game over scene
│   ├── Prefabs/                    # (Created at runtime by SceneSetup scripts)
│   ├── Sprites/                    # (Generated procedurally by SpriteGenerator)
│   ├── Audio/                      # Drop your .wav/.ogg files here
│   ├── Materials/
│   ├── Resources/
│   └── Editor/
├── ProjectSettings/                # Unity project configuration
│   ├── ProjectSettings.asset
│   ├── TagManager.asset            # Custom tags: PlayerBullet, EnemyBullet, Enemy, PowerUp
│   ├── InputManager.asset          # WASD + Arrows + Space + Esc
│   ├── Physics2DSettings.asset     # Zero gravity for 2D
│   ├── EditorBuildSettings.asset   # Scene build order
│   ├── QualitySettings.asset
│   ├── AudioManager.asset
│   ├── TimeManager.asset
│   ├── GraphicsSettings.asset
│   └── ProjectVersion.txt
├── Packages/
│   └── manifest.json               # Required Unity packages
└── README.md                       # This file
```

---

## Step-by-Step Build Instructions

### Step 1: Install Unity

1. Download **Unity Hub** from https://unity.com/download
2. Open Unity Hub → **Installs** → **Install Editor**
3. Select **Unity 2022.3 LTS** (any 2022.3.x version)
4. In the modules list, make sure to check:
   - ✅ **Windows Build Support (Mono)**
   - ✅ **Windows Build Support (IL2CPP)** (optional, for better performance)
5. Click **Install** and wait for completion

### Step 2: Open the Project

1. Open **Unity Hub**
2. Click **Projects** → **Open** (or **Add project from disk**)
3. Navigate to the `SpaceShooterUnity` folder and select it
4. Unity will detect the project version and open it
5. If prompted about version mismatch, click **Continue** — the project is compatible

### Step 3: Configure Tags (IMPORTANT — First Time Only)

The game uses custom tags. Unity's scene files reference setup scripts, but you need
to verify the tags are properly registered:

1. Go to **Edit → Project Settings → Tags and Layers**
2. Under **Tags**, ensure these exist (add if missing):
   - `PlayerBullet`
   - `EnemyBullet`
   - `Enemy`
   - `PowerUp`
3. The `Player` tag is built-in and should already exist

### Step 4: Configure Build Scenes

1. Go to **File → Build Settings**
2. Click **Add Open Scenes** or drag scenes from `Assets/Scenes/`:
   - **Scene 0**: `Assets/Scenes/MainMenu.unity` ← Must be first!
   - **Scene 1**: `Assets/Scenes/GamePlay.unity`
   - **Scene 2**: `Assets/Scenes/GameOver.unity`
3. Make sure all three scenes are checked ✅

### Step 5: Attach Scripts to Scene Objects

Because the scene YAML files may not preserve script GUIDs perfectly across
different Unity installations, you should verify script attachments:

#### MainMenu Scene:
1. Open `Assets/Scenes/MainMenu.unity`
2. Select the **SceneSetup** GameObject in the Hierarchy
3. In the Inspector, it should have the **SceneSetup_MainMenu** script
4. If the script shows "Missing", drag `SceneSetup_MainMenu.cs` from
   `Assets/Scripts/` onto the Inspector

#### GamePlay Scene:
1. Open `Assets/Scenes/GamePlay.unity`
2. Select the **GamePlaySetup** GameObject
3. It should have the **SceneSetup_GamePlay** script attached
4. If missing, drag `SceneSetup_GamePlay.cs` onto it

#### GameOver Scene:
1. Open `Assets/Scenes/GameOver.unity`
2. Select the **SceneSetup** GameObject
3. It should have the **SceneSetup_GameOver** script attached
4. If missing, drag `SceneSetup_GameOver.cs` onto it

### Step 6: Build the Game

1. Go to **File → Build Settings**
2. Set **Target Platform** to **Windows**
3. Set **Architecture** to **x86_64**
4. Click **Player Settings** and configure:
   - **Product Name**: Space Shooter
   - **Company Name**: (your name)
   - **Default Screen Width**: 800
   - **Default Screen Height**: 600
   - **Fullscreen Mode**: Windowed (or your preference)
   - **Run In Background**: ✅ checked
5. Click **Build** (or **Build And Run**)
6. Choose an output folder (e.g., `Build/`)
7. Wait for the build to complete
8. Your `SpaceShooter.exe` will be in the output folder!

### Step 7: Distribute

The build folder contains:
```
Build/
├── SpaceShooter.exe              # The game executable
├── SpaceShooter_Data/            # Game data (required!)
├── UnityCrashHandler64.exe       # Crash handler
└── UnityPlayer.dll               # Unity runtime (required!)
```

**To distribute**: Zip the entire `Build/` folder. All files are needed to run.

---

## Alternative: Quick Setup Method

If scene script references don't load properly, you can create fresh scenes:

1. Create a new empty scene, save as `MainMenu`
2. Create an empty GameObject, attach `SceneSetup_MainMenu.cs`
3. That's it — the script builds everything at runtime!
4. Repeat for `GamePlay` (attach `SceneSetup_GamePlay.cs`) and `GameOver`
   (attach `SceneSetup_GameOver.cs`)
5. Add all three scenes to Build Settings in order

This works because **all game objects, sprites, prefabs, and UI are created
procedurally** by the SceneSetup scripts. Each scene only needs:
- A Main Camera (with Orthographic projection, size 5.5)
- One empty GameObject with the appropriate SceneSetup script

---

## Controls

| Action | Key |
|---|---|
| Move | WASD or Arrow Keys |
| Shoot | Space |
| Pause | Escape |

---

## How It Works (Architecture)

### Runtime Scene Construction
Each scene has a `SceneSetup_*` MonoBehaviour that constructs the entire scene
at runtime in `Start()`. This means:
- No prefab assets needed on disk
- No sprite files needed
- No UI prefabs needed
- Everything is generated procedurally

### Sprite Generation
`SpriteGenerator.cs` creates all sprites as `Texture2D` objects with pixel-by-pixel
drawing. Sprites are cached and reused. Includes:
- **Player ship**: 32×32 blue triangle with engine glow
- **Enemy Scout**: 28×28 red inverted triangle
- **Enemy Weaver**: 28×28 orange inverted triangle with wings
- **Enemy Gunship**: 32×32 purple hexagonal with cannons
- **Bullets**: 4×10 (player green) and 4×8 (enemy red)
- **Power-ups**: 20×20 colored circles (blue=shield, yellow=rapid, green=health)

### Game Flow
```
MainMenu → [Play] → GamePlay → [Die] → GameOver → [Restart/Menu]
                      ↕ [Esc]
                    Paused
```

### Manager Singletons
- **GameManager**: Score, state, scene transitions (persists across scenes)
- **AudioManager**: SFX playback by name (persists across scenes)
- **SpriteGenerator**: Cached procedural sprites (persists across scenes)
- **UIManager**: Per-scene UI (recreated each scene)

---

## Adding Sound Effects

1. Place `.wav` or `.ogg` files in `Assets/Audio/`
2. Select the **AudioManager** GameObject (created by SceneSetup_MainMenu)
3. In the Inspector, drag audio clips to the matching slots:
   - Player Shoot Clip
   - Player Hit Clip
   - Player Death Clip
   - Enemy Explosion Clip
   - Power Up Clip
   - Background Music

Or modify `AudioManager.cs` to load from Resources:
```csharp
playerShootClip = Resources.Load<AudioClip>("Audio/shoot");
```

---

## Customization

### Difficulty Tuning (EnemySpawner)
- `baseEnemiesPerWave`: Starting enemy count (default: 4)
- `enemiesPerWaveIncrease`: Added per wave (default: 2)
- `speedIncreasePerWave`: Speed boost per wave (default: 0.3)
- `timeBetweenWaves`: Seconds between waves (default: 4)

### Player Tuning (PlayerController)
- `moveSpeed`: Player movement speed (default: 8)
- `fireRate`: Seconds between shots (default: 0.25)
- `maxHealth`: Starting health (default: 5)
- `bulletSpeed`: Bullet travel speed (default: 12)

### Enemy Tuning (Enemy)
- `maxHealth`: Hits to kill (default: 2)
- `moveSpeed`: Downward speed (default: 3)
- `dropChance`: Power-up drop probability (default: 15%)

---

## Troubleshooting

| Issue | Solution |
|---|---|
| "Missing script" on GameObjects | Re-attach the correct `SceneSetup_*.cs` script |
| Tags not found errors | Add tags in Edit → Project Settings → Tags and Layers |
| No fonts rendering | Unity's built-in font should work; if not, import any .ttf |
| Scenes not loading | Check File → Build Settings has all 3 scenes in order |
| Black screen | Ensure camera is Orthographic with size 5.5 |
| Objects not colliding | Verify Rigidbody2D and Trigger colliders are present |

---

## License

Free to use, modify, and distribute. Built as a learning project.
