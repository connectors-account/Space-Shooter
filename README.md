# 🚀 Space Shooter — Complete Unity Project

A fully-implemented 2D space shooter game built with Unity, targeting Windows standalone. All gameplay systems, UI, audio, and visual effects are coded and ready to use.

---

## 📁 Project Structure

```
space_shooter_unity/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   └── PlayerController.cs      # Movement, shooting, health, power-ups
│   │   ├── Enemies/
│   │   │   ├── EnemyBase.cs             # Base enemy class (health, scoring, drops)
│   │   │   ├── BasicEnemy.cs            # Straight-line movement
│   │   │   ├── FastEnemy.cs             # Zigzag movement pattern
│   │   │   └── TankEnemy.cs             # Slow, tanky, frequent shooter
│   │   ├── Weapons/
│   │   │   └── Bullet.cs               # Pooled bullet (player + enemy)
│   │   ├── PowerUps/
│   │   │   ├── PowerUpItem.cs           # Collectable power-up behaviour
│   │   │   └── PowerUpSpawner.cs        # Random power-up drop manager
│   │   ├── Managers/
│   │   │   ├── GameManager.cs           # Game state, score, lifecycle
│   │   │   ├── WaveSpawner.cs           # Wave-based enemy spawning
│   │   │   ├── ObjectPoolManager.cs     # High-performance object pooling
│   │   │   ├── AudioManager.cs          # SFX with procedural placeholder sounds
│   │   │   └── SceneBootstrap.cs        # ★ Auto-builds entire scene at runtime
│   │   ├── UI/
│   │   │   ├── MainMenuUI.cs            # Start Game / Quit
│   │   │   ├── HudUI.cs                 # Score, Lives, Wave display
│   │   │   ├── PauseMenuUI.cs           # Resume / Main Menu
│   │   │   └── GameOverUI.cs            # Final Score / Restart / Main Menu
│   │   ├── Effects/
│   │   │   ├── ParallaxBackground.cs    # Scrolling background layer
│   │   │   ├── ExplosionManager.cs      # Particle explosion effects
│   │   │   └── StarfieldGenerator.cs    # Procedural multi-layer star field
│   │   └── Utils/
│   │       └── SpriteGenerator.cs       # Runtime geometric sprite generation
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Materials/
│   ├── Sprites/
│   └── Audio/
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── TagManager.asset                 # Custom tags: PlayerBullet, EnemyBullet, Enemy, PowerUp
│   ├── Physics2DSettings.asset
│   ├── QualitySettings.asset
│   └── InputManager.asset              # Arrow keys / WASD + Space
├── Packages/
│   └── manifest.json                    # TextMeshPro, 2D, Physics2D, Audio, Particles
└── docs/
    └── SCENE_SETUP.md                   # Detailed scene setup guide
```

---

## 🎮 Game Features

### Controls
| Input | Action |
|-------|--------|
| Arrow Keys / WASD | Move player ship |
| Spacebar | Fire bullets |
| Escape | Pause / Resume |

### Gameplay Systems
- **Object Pooling** — All bullets, enemies, and power-ups are pooled for zero-allocation gameplay
- **Wave Spawner** — Infinite waves with increasing enemy count and type variety
- **Difficulty Scaling** — Enemy health and speed scale per wave
- **Score System** — Points per enemy kill, persistent high score via PlayerPrefs
- **Health System** — 5 lives, invincibility frames after hit, visual flash feedback

### Enemy Types
| Type | Health | Speed | Behaviour | Score |
|------|--------|-------|-----------|-------|
| Basic | 1 | Normal | Straight down, shoots | 100 |
| Fast | 1 | High | Zigzag pattern | 150 |
| Tank | 5 | Slow | Straight down, rapid fire | 300 |

### Power-Ups (15% drop chance, 30% for tanks)
| Power-Up | Color | Effect |
|----------|-------|--------|
| Health Restore | 🟢 Green | +1 life |
| Rapid Fire | 🟡 Yellow | 2.5x fire rate for 5s |
| Shield | 🔵 Cyan | Absorbs one hit for 6s |

### Visual Effects
- **Parallax Starfield** — 3-layer procedural star particles (far/mid/near)
- **Explosions** — Small/Medium/Large particle bursts with colour gradients
- **Shield Visual** — Translucent cyan circle around player
- **Damage Flash** — Enemies flash red on hit; player flickers when invincible

### Audio
- **8 procedurally generated SFX** — no external audio files needed
  - Player shoot, Enemy shoot, Enemy death, Player hit
  - Player death, Power-up pickup, Shield break, Wave start
- **Music hook** — `AudioManager.PlayMusic(clip)` ready for custom tracks

### UI Screens
1. **Main Menu** — Title, High Score, Start Game, Quit
2. **HUD** — Score (top-left), Wave (top-center), Lives (top-right)
3. **Pause Menu** — Triggered by ESC, Resume + Main Menu buttons
4. **Game Over** — Final score, New High Score detection, Play Again + Main Menu

---

## 🛠️ Quick Start — Scene Setup

### Option A: Automatic (Recommended)

The **SceneBootstrap** script creates the entire game scene at runtime — no manual prefab/UI setup needed.

1. **Open Unity** (2021.3 LTS or newer recommended)
2. Open the project folder (`space_shooter_unity/`)
3. If prompted about TextMeshPro, click **Import TMP Essentials**
4. Open (or create) the main scene: `Assets/Scenes/MainScene.unity`
5. Create **one empty GameObject** in the scene
6. Rename it `SceneBootstrap`
7. Attach the script: `Scripts/Managers/SceneBootstrap.cs`
8. **Ensure the following tags exist** in Project Settings → Tags & Layers:
   - `Player`
   - `PlayerBullet`
   - `EnemyBullet`
   - `Enemy`
   - `PowerUp`
9. Press **Play** ▶️

> The SceneBootstrap will automatically create the player, all enemy/bullet prefabs, object pools, UI canvases, star field, explosion system, audio manager, and wire everything together.

### Option B: Manual Setup

See [docs/SCENE_SETUP.md](docs/SCENE_SETUP.md) for step-by-step manual configuration of each GameObject, prefab, and UI element.

---

## 🏗️ Build Instructions (Windows Standalone)

### Prerequisites
- Unity 2021.3 LTS or newer (2022.3 / 6000.x also work)
- Windows Build Support module installed in Unity Hub

### Steps

1. **Open the project** in Unity
2. **File → Build Settings**
3. Set **Target Platform** to **Windows**
4. Set **Architecture** to **x86_64** (or x86 for 32-bit)
5. Click **Add Open Scenes** (ensure MainScene is in the list)
6. **Player Settings** (optional tweaks):
   - Company Name: `SpaceShooterStudio`
   - Product Name: `Space Shooter`
   - Resolution: `1280 × 720`, Windowed or Fullscreen
   - Icon: (use any .png)
7. Click **Build** or **Build and Run**
8. Choose an output folder (e.g., `Builds/Windows/`)
9. The executable `Space Shooter.exe` and `Space Shooter_Data/` folder are your distributable

### Build Output
```
Builds/
└── Windows/
    ├── Space Shooter.exe          ← Run this
    ├── Space Shooter_Data/
    ├── MonoBleedingEdge/
    └── UnityPlayer.dll
```

To distribute: **zip the entire `Windows/` folder**.

---

## 🔧 Customisation Guide

### Tweak Difficulty
Edit `WaveSpawner.cs`:
- `baseEnemiesPerWave` — Starting enemy count per wave
- `enemiesPerWaveIncrease` — How many more per wave
- `timeBetweenSpawns` — Delay between individual enemy spawns
- `timeBetweenWaves` — Cooldown between waves

### Add New Enemy Types
1. Create `Assets/Scripts/Enemies/MyEnemy.cs` extending `EnemyBase`
2. Override `Move()` for custom movement
3. Set stats in `Awake()`: health, speed, score, shooting
4. Register a pool tag in `SceneBootstrap.SetupObjectPools()`

### Replace Placeholder Audio
In `AudioManager.cs`, replace the `GeneratePlaceholderSounds()` calls with:
```csharp
sfxClips["PlayerShoot"] = Resources.Load<AudioClip>("Audio/player_shoot");
```
Place `.wav` or `.ogg` files in `Assets/Resources/Audio/`.

### Add Background Music
```csharp
AudioClip music = Resources.Load<AudioClip>("Audio/bgm");
AudioManager.Instance.PlayMusic(music);
```

---

## 📋 Technical Notes

- **No external assets required** — all sprites, particles, and audio are generated procedurally
- **Zero-allocation gameplay** — object pooling for all runtime objects
- **Single-scene architecture** — everything runs in one scene, no scene loading
- **Namespace-organised** — all code under `SpaceShooter.*` namespaces
- **Event-driven UI** — managers fire C# events; UI panels subscribe
- **Reflection bootstrap** — `SceneBootstrap` uses reflection to wire serialized fields at runtime, eliminating manual inspector setup

---

## 📄 License

Free to use, modify, and distribute. No attribution required.
