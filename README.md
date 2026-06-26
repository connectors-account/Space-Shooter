# Space Shooter

A complete, production-ready 2D arcade space-shooter built in **C# for Unity**, targeting **Windows desktop**.
The entire game world (camera, player, enemies, bullets, power-ups, UI, particles and audio) is assembled
**at runtime in code by a bootstrap component**, so the project is fully playable the moment you press Play —
no manual scene wiring and **no custom art or audio assets are required**. Sprites and sound effects are
generated procedurally as fallbacks, and the code is structured so you can drop in your own assets later.

---

## Table of contents
1. [Features](#features)
2. [Requirements](#requirements)
3. [Opening the project](#opening-the-project)
4. [Running in the editor](#running-in-the-editor)
5. [Controls](#controls)
6. [Game mechanics](#game-mechanics)
7. [Project structure](#project-structure)
8. [Architecture overview](#architecture-overview)
9. [Building for Windows](#building-for-windows)
10. [Adding your own art & audio](#adding-your-own-art--audio)
11. [Troubleshooting](#troubleshooting)

---

## Features
- **Player ship** with smooth 8-directional movement, health, lives and an invulnerability window after being hit.
- **Four enemy archetypes** with distinct movement and attack behaviour:
  - *Basic* — descends straight down, fires straight bullets.
  - *Zigzag* — oscillates horizontally, fires bullets aimed at the player.
  - *Circular* — follows a sine path, fires rotating radial sprays.
  - *Boss* — appears every 5th wave, hovers and sweeps, with three health-based attack phases.
- **Bullet patterns**: single, spread (3-way), rapid fire, aimed, and radial rings.
- **Five power-ups**: Health, Shield (invincibility), Rapid Fire, Spread Shot, and Score Multiplier.
- **Wave-based progression** (15 waves) with difficulty scaling enemy count, health and speed.
- **Full UI**: main menu, in-game HUD (score, health bar, wave, lives, active power-ups), pause menu,
  game-over screen and victory screen — all built at runtime.
- **Object pooling** for bullets, enemies, power-ups and explosions for smooth performance.
- **Three-layer parallax** scrolling star-field background.
- **Procedural audio** (synthesised SFX + ambient music) via a centralised `AudioManager` ready for asset integration.
- **Particle explosions** for enemy/player destruction.
- Clean, documented, component-based C# with XML doc comments on all public members.

---

## Requirements
- **Unity 2022.3 LTS** (developed against `2022.3.40f1`). Any `2022.3.x` build will work; newer LTS streams
  (2023 / 6000.x) also open the project, but 2022.3 is recommended.
- A Unity install with the **Windows Build Support (IL2CPP/Mono)** module if you plan to build a `.exe`.
- No external packages beyond Unity's built-in modules (uGUI, Physics2D, Particle System, Audio) — these are
  declared in `Packages/manifest.json` and resolved automatically.

---

## Opening the project
1. Install **Unity Hub** and a **Unity 2022.3 LTS** editor (with *Windows Build Support*).
2. In Unity Hub choose **Open → Add project from disk** and select the `SpaceShooter/` folder
   (the folder that contains `Assets/`, `Packages/` and `ProjectSettings/`).
3. Open the project. Unity will import assets and compile the scripts on first launch (this can take a minute).

---

## Running in the editor
1. In the **Project** window open `Assets/Scenes/MainMenu.unity`.
2. Press the **Play** button. You'll see the title screen — click **PLAY** to load the gameplay scene.
   - You can also open `Assets/Scenes/GamePlay.unity` and press Play to jump straight into a match.
3. Both scenes are already registered in **Build Settings** (MainMenu is index 0, GamePlay is index 1).

> The scenes contain only a single *Bootstrap* GameObject. All other objects are created in code at runtime,
> so the Hierarchy will populate itself once you enter Play mode.

---

## Controls
| Action            | Keys                                   |
|-------------------|----------------------------------------|
| Move              | **W A S D** or **Arrow keys**          |
| Shoot             | **Space** or **Left Mouse Button**     |
| Pause / Resume    | **Esc**                                |
| Menu navigation   | Mouse (click buttons)                  |

---

## Game mechanics
- **Lives & health**: You start with **3 lives** and **100 health**. Taking damage reduces health; at 0 health
  you lose a life and respawn with full health and a brief invulnerability period. Losing all lives ends the game.
- **Scoring**: Each enemy awards points (Basic 100, Zigzag 150, Circular 200, Bosses 2000×tier). The
  *Score Multiplier* power-up doubles points while active.
- **Waves**: Clear all enemies in a wave to advance. Difficulty rises each wave (more enemies, more health,
  faster movement, more enemy types). Every **5th wave** is a **boss fight**. Clearing **wave 15** wins the game.
- **Power-ups** drop randomly from defeated enemies (bosses always drop one):
  - **Health** — restores 35 health instantly.
  - **Shield** — 6s of invincibility.
  - **Rapid Fire** — 8s of greatly increased fire rate.
  - **Spread Shot** — 8s of 3-way fire.
  - **Score Multiplier** — 10s of ×2 score.

All balance values live in a single class, `Assets/Scripts/Core/GameConfig.cs`, for easy tuning.

---

## Project structure
```
SpaceShooter/
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity        # Title screen (MainMenuBootstrap)
│   │   └── GamePlay.unity        # Gameplay scene (GameBootstrap)
│   ├── Scripts/
│   │   ├── Core/                 # Enums, config, pooling, sprite factory, interfaces
│   │   ├── Player/               # PlayerController
│   │   ├── Enemies/              # Enemy (all archetypes + boss)
│   │   ├── Weapons/              # Bullet, BulletManager (patterns)
│   │   ├── PowerUps/             # PowerUp, PowerUpManager
│   │   ├── Managers/             # GameManager, SpawnManager, AudioManager, CollisionHandler
│   │   ├── UI/                   # UIManager, UIFactory
│   │   ├── Environment/          # BackgroundScroller, ExplosionEffect, ExplosionManager
│   │   └── Bootstrap/            # GameBootstrap, MainMenuBootstrap
│   ├── Prefabs/                  # Player.prefab, Enemy.prefab, Bullet.prefab (also built at runtime)
│   ├── Sprites/                  # SPRITE_INSTRUCTIONS.md (drop custom art here)
│   ├── Audio/                    # AUDIO_INSTRUCTIONS.md (drop custom SFX/music here)
│   └── Materials/
├── Packages/manifest.json        # Built-in module dependencies
├── ProjectSettings/              # ProjectVersion, ProjectSettings, InputManager, TagManager, EditorBuildSettings
├── README.md
└── BUILD_INSTRUCTIONS.md
```

### Spec deliverable mapping

This project was grown from a minimal brief into a fuller game, so a few of the
originally-requested file names live under clearer, more descriptive names. The
required behaviour for every item is fully implemented:

| Requested file        | Where it lives                                   | Notes |
|-----------------------|--------------------------------------------------|-------|
| `PlayerController.cs`  | `Assets/Scripts/Player/PlayerController.cs`      | WASD/arrows move, Space/click shoots, clamped to bounds |
| `BulletController.cs`  | `Assets/Scripts/Weapons/Bullet.cs`               | Moves in its direction, dies on hit / off-screen (pooled) |
| `EnemyController.cs`   | `Assets/Scripts/Enemies/Enemy.cs`                | Moves down with several AI archetypes incl. boss |
| `EnemySpawner.cs`      | `Assets/Scripts/Managers/SpawnManager.cs`        | Timed waves with increasing difficulty |
| `GameManager.cs`       | `Assets/Scripts/Managers/GameManager.cs`         | State, score, game over, restart |
| `UIManager.cs`         | `Assets/Scripts/UI/UIManager.cs`                 | Health bar, score, game-over screen (Canvas) |
| `HealthSystem.cs`      | `Assets/Scripts/Core/HealthSystem.cs`            | Reusable health container ("N hits = death") |
| `MainScene.unity`      | `Assets/Scenes/GamePlay.unity` (+ `MainMenu.unity`) | Gameplay scene assembled by `GameBootstrap` |
| `Player/Enemy/Bullet.prefab` | `Assets/Prefabs/*.prefab`                  | SpriteRenderer + Rigidbody2D + trigger collider + scripts |
| `ProjectSettings/*`    | `ProjectSettings/`                               | ProjectVersion, ProjectSettings, InputManager, TagManager, EditorBuildSettings |

> Note: the game uses a **bootstrap pattern** — `GameBootstrap`/`MainMenuBootstrap`
> build the camera, managers, player, background and UI in code at runtime, so the
> scenes and prefabs do not need manual wiring. The prefabs in `Assets/Prefabs/`
> are provided as ready-to-drag reference objects and for custom workflows.

---

## Architecture overview
- **Bootstrap pattern** — `GameBootstrap` (gameplay) and `MainMenuBootstrap` (menu) construct and wire every
  system on `Start()` in a deterministic order. This removes fragile serialized references and guarantees a
  valid scene every run.
- **Managers are singletons** exposing an `Instance` once initialised by the bootstrap:
  `GameManager`, `SpawnManager`, `AudioManager`, `BulletManager`, `PowerUpManager`, `ExplosionManager`.
- **Separation of concerns**:
  - *Detection* of collisions happens on the active colliders (`Bullet`, `PlayerController`).
  - *Resolution* of what a collision means lives in `CollisionHandler` (bullet↔enemy, bullet↔player,
    player↔enemy, player↔power-up).
  - `IDamageable` lets bullets damage the player and enemies generically.
- **Object pooling** (`ObjectPool` + `IPoolable`) recycles bullets, enemies, power-ups and explosions.
- **Procedural content** (`SpriteFactory`, `AudioManager`) keeps the game playable with zero imported assets.
- **Events** decouple gameplay from UI: `GameManager` and `PlayerController` raise C# events that `UIManager`
  subscribes to for HUD updates.

---

## Building for Windows
See **[BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)** for the full step-by-step guide. Quick version:
1. `File → Build Settings…`
2. Ensure **MainMenu** (index 0) and **GamePlay** (index 1) are in *Scenes In Build*.
3. Select **Windows, Mac, Linux** platform and set **Target Platform = Windows**, **Architecture = x86_64**.
4. Click **Build**, choose an output folder, and run the generated `SpaceShooter.exe`.

---

## Adding your own art & audio
- **Sprites**: import PNGs into `Assets/Sprites/` (see `SPRITE_INSTRUCTIONS.md` for the recommended list and
  dimensions). You can assign them to the relevant `SpriteRenderer`s, or extend `SpriteFactory` / the entity
  `Configure` methods to use your sprites instead of the generated ones.
- **Audio**: import clips into `Assets/Audio/` (see `AUDIO_INSTRUCTIONS.md`) and call
  `AudioManager.Instance.RegisterClip("explosion", yourClip)` — any registered clip overrides the synthesised
  default with the same key (`player_shot`, `enemy_shot`, `explosion`, `player_hit`, `powerup`, `ui_click`,
  `wave`, `music`).

---

## Troubleshooting
- **Nothing happens in the scene view before Play** — expected; objects are created at runtime.
- **No sound** — ensure the editor/system volume is up; SFX are synthesised and quiet by design (tune in
  `AudioManager`).
- **Buttons don't click** — the bootstrap creates an `EventSystem` automatically; if you added one manually,
  make sure there is only one in the scene.
- **Build has no scenes** — re-open `File → Build Settings…` and confirm both scenes are listed and enabled.
