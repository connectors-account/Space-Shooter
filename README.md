# 🚀 Space Shooter — Complete Unity Game

A fully playable vertical space-shooter built in Unity (C#) for Windows desktop.
All game art is generated procedurally — **no external assets required**.

---

## 🎮 Features

| Feature | Details |
|---|---|
| **Player Ship** | WASD / Arrow keys movement, Space/Z to shoot, screen-clamped |
| **Wave System** | Infinite waves with progressive difficulty scaling |
| **4 Enemy Types** | Straight, Zigzag, Sine-wave, Dive-bomber — each with unique AI |
| **Enemy AI** | Enemies shoot at the player with aimed/random bullets |
| **Power-ups** | 🟡 Rapid Fire · 🔵 Shield · 🟢 Health — randomly dropped by enemies |
| **Scoring** | Points per kill, displayed on HUD |
| **Lives System** | 3 lives with respawn invincibility |
| **Full UI** | Main Menu → HUD → Pause Menu (ESC) → Game Over screen |
| **Parallax BG** | Two-layer scrolling starfield |
| **Sound System** | Pluggable SFX manager (just drop in audio clips) |
| **Procedural Art** | Ships, bullets, backgrounds generated in code — no sprites needed |

---

## 🎯 Controls

| Key | Action |
|---|---|
| `WASD` / `Arrow Keys` | Move ship |
| `Space` / `Z` | Shoot |
| `ESC` | Pause / Resume |

---

## 📁 Project Structure

```
SpaceShooterGame/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs            — Central game state & scoring
│   │   ├── PlayerController.cs       — Player movement, shooting, health, power-ups
│   │   ├── Enemy.cs                  — Enemy AI, movement patterns, shooting
│   │   ├── EnemySpawner.cs           — Wave-based spawning with difficulty scaling
│   │   ├── Bullet.cs                 — Generic bullet (player & enemy)
│   │   ├── PowerUp.cs               — Power-up types & pickup logic
│   │   ├── UIManager.cs             — Full UI system (menus, HUD, buttons)
│   │   ├── BackgroundScroller.cs     — Parallax scrolling
│   │   ├── SoundManager.cs          — Audio SFX playback pool
│   │   ├── SceneBootstrap.cs        — Auto-builds entire scene at runtime
│   │   ├── ProceduralSpriteGenerator.cs — Runtime sprite generation
│   │   ├── AutoDestroy.cs           — Timed self-destruct helper
│   │   └── TagSetup.cs              — Editor script to create required tags
│   ├── Prefabs/                      — (populated during setup)
│   ├── Scenes/                       — (create your scene here)
│   ├── Sprites/                      — (optional custom art)
│   ├── Audio/                        — (optional sound clips)
│   └── Materials/                    — (optional materials)
├── ProjectSettings/                  — (Unity generates this)
├── Packages/                         — (Unity generates this)
├── SCENE_SETUP.md                    — Detailed scene/prefab setup guide
├── BUILD_INSTRUCTIONS.md            — Step-by-step Windows build guide
└── README.md                        — This file
```

---

## 🔧 Quick Start — Import & Run

### Prerequisites
- **Unity 2021.3 LTS** or newer (any 2021+/2022+/2023+/6000+ version works)
- **Unity Hub** installed ([download](https://unity.com/download))

### Step 1: Create a New Unity Project
1. Open **Unity Hub** → click **New Project**.
2. Select the **2D (Built-in Render Pipeline)** template.
3. Name it `SpaceShooter` and choose a location. Click **Create Project**.

### Step 2: Import the Scripts
1. In the Unity Editor, navigate to the **Project** panel.
2. Copy the entire contents of this project's `Assets/` folder into your Unity project's `Assets/` folder:
   - You can drag & drop the `Scripts/` folder directly into the Project panel.
   - Or use your OS file manager to copy into `<YourProject>/Assets/`.
3. Unity will auto-import and compile the scripts.

### Step 3: Set Up the Scene (Automatic)
1. In the **Hierarchy** panel, right-click → **Create Empty** → name it `Bootstrap`.
2. In the **Inspector**, click **Add Component** → search for `SceneBootstrap` → add it.
3. Press **▶ Play** — the game runs with full procedural art!

> **Note:** The `TagSetup.cs` editor script automatically creates all required tags
> (`Player`, `Enemy`, `PlayerBullet`, `EnemyBullet`, `PowerUp`) when scripts compile.

### Step 4: Save the Scene
1. **File → Save As** → save to `Assets/Scenes/MainScene.unity`.

---

## 🏗️ Building the Windows .exe

### Step 1: Configure Build Settings
1. Go to **File → Build Settings** (Ctrl+Shift+B).
2. Click **Add Open Scenes** to add your scene to the build.
3. Under **Platform**, select **Windows, Mac, Linux** (it should be default).
4. Set **Target Platform** to **Windows**.
5. Set **Architecture** to **x86_64** (recommended).

### Step 2: Configure Player Settings
1. In Build Settings, click **Player Settings…**
2. Set:
   - **Company Name**: Your name
   - **Product Name**: Space Shooter
   - **Default Screen Width**: 600
   - **Default Screen Height**: 800
   - **Fullscreen Mode**: Windowed (or your preference)
   - **Run In Background**: ✓ checked
3. (Optional) Set an app icon under **Icon**.

### Step 3: Build
1. Back in **Build Settings**, click **Build**.
2. Choose an output folder (e.g., `Builds/Windows/`).
3. Wait for the build to complete.
4. Navigate to the output folder — you'll find:
   - `Space Shooter.exe` — the game executable
   - `Space Shooter_Data/` — required data folder
   - `UnityPlayer.dll` — required runtime
5. **Run `Space Shooter.exe`** — the game launches!

### Distribution
To share the game, zip the entire build output folder:
```
SpaceShooter_Windows.zip
├── Space Shooter.exe
├── Space Shooter_Data/
├── UnityPlayer.dll
└── MonoBleedingEdge/
```

---

## 🎨 Using Custom Art (Optional)

The game works with procedural sprites out of the box. To use custom art:

1. Create or download sprites (16×16 to 64×64 px, PNG with transparency).
2. Place them in `Assets/Sprites/`.
3. In Unity, select each texture → set **Texture Type** = Sprite, **Pixels Per Unit** = 32.
4. Assign sprites to the SpriteRenderers on your prefabs.

### Recommended Free Asset Sources
- [Kenney.nl Space Shooter Pack](https://kenney.nl/assets/space-shooter-redux) (CC0)
- [OpenGameArt.org](https://opengameart.org/art-search?keys=space+shooter)
- [itch.io Free Game Assets](https://itch.io/game-assets/free/tag-space)

---

## 🔊 Adding Sound Effects (Optional)

1. Download short .wav or .ogg clips for:
   - Player shoot
   - Enemy shoot
   - Explosion
   - Player hit
   - Power-up pickup
2. Place them in `Assets/Audio/`.
3. Select the **SoundManager** in the Hierarchy.
4. In the Inspector, expand **Sfx Entries** and add entries:
   | Name | Clip | Volume |
   |---|---|---|
   | `PlayerShoot` | player_shoot.wav | 0.4 |
   | `EnemyShoot` | enemy_shoot.wav | 0.3 |
   | `Explosion` | explosion.wav | 0.6 |
   | `PlayerHit` | hit.wav | 0.5 |
   | `PowerUp` | powerup.wav | 0.5 |

---

## 🛠️ Customisation

All gameplay values are exposed in the Inspector:

| Script | Key Settings |
|---|---|
| `PlayerController` | moveSpeed, fireRate, maxHealth, power-up durations |
| `Enemy` | health, scoreValue, moveSpeed, pattern, fireRate |
| `EnemySpawner` | baseEnemiesPerWave, extraEnemiesPerWave, spawnInterval, difficulty decay |
| `GameManager` | startingLives, respawnDelay |
| `PowerUp` | dropChance, fallSpeed |
| `BackgroundScroller` | scrollSpeed per layer |

---

## 📝 License

This project is provided as-is for educational and personal use.
All code is original. No third-party assets are included.
Feel free to modify and distribute.
