# 🚀 Space Shooter — Complete Unity Project

A fully-featured 2D space-shooter built with Unity. Includes wave-based enemy spawning,
four enemy types (Basic, Zigzag, Tank, Boss), four power-up types, parallax backgrounds,
an audio system, and full menu/pause/game-over UI flow.

> **Unity Version:** 2022.3 LTS (any 2022.3.x patch). Also works on 2023.x / Unity 6.

---

## Table of Contents

1. [Project Setup from Scratch](#1-project-setup-from-scratch)
2. [Layer & Tag Configuration](#2-layer--tag-configuration)
3. [Sorting Layers](#3-sorting-layers-for-parallax)
4. [Scene Hierarchy — MenuScene](#4-scene-hierarchy--menuscene)
5. [Scene Hierarchy — GameScene](#5-scene-hierarchy--gamescene)
6. [Prefab Creation](#6-prefab-creation)
7. [Sprite Creation Guidance](#7-sprite-creation-guidance)
8. [Audio Setup](#8-audio-setup)
9. [Input Configuration](#9-input-configuration)
10. [Physics / Collision Matrix](#10-physics--collision-matrix)
11. [Build for Windows](#11-step-by-step-windows-build)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Project Setup from Scratch

1. Open **Unity Hub → New Project → 2D (Core)** template.
2. Name it `SpaceShooter`, choose a location, and click **Create**.
3. Copy the entire `Assets/Scripts/` folder from this repository into the project's
   `Assets/Scripts/` folder (or drag-and-drop in the Unity Editor).
4. Unity will compile automatically. **All scripts should compile with zero errors.**

---

## 2. Layer & Tag Configuration

### Tags (Edit → Project Settings → Tags and Layers → Tags)

| Tag            | Used By                        |
|----------------|--------------------------------|
| `Player`       | Player ship GameObject         |
| `Enemy`        | All enemy prefabs              |
| `PlayerBullet` | Bullets fired by the player    |
| `EnemyBullet`  | Bullets fired by enemies       |
| `PowerUp`      | All power-up prefabs           |

### Layers (same panel → Layers)

| Layer # | Name           |
|---------|----------------|
| 8       | Player         |
| 9       | Enemy          |
| 10      | PlayerBullet   |
| 11      | EnemyBullet    |
| 12      | PowerUp        |

---

## 3. Sorting Layers (for Parallax)

Go to **Edit → Project Settings → Tags and Layers → Sorting Layers** and add these
**in this exact order** (top = rendered first / behind):

| Order | Sorting Layer |
|-------|---------------|
| 0     | Background    |
| 1     | Midground     |
| 2     | Default       |
| 3     | Foreground    |
| 4     | UI            |

Assign layers:
- Far stars sprite → `Background`
- Near stars sprite → `Midground`
- Player, enemies, bullets, power-ups → `Default`

---

## 4. Scene Hierarchy — MenuScene

Create a new scene: **File → New Scene → Save As `MenuScene`**.

```
MenuScene
├── Main Camera           (default)
├── GameManager           [Add GameManager.cs — ONLY if not in a preload scene]
├── AudioManager          [Add AudioManager.cs]
├── Canvas (Screen Space - Overlay)
│   ├── TitleText         [UI → Text] "SPACE SHOOTER" centered, font size 48
│   ├── PlayButton        [UI → Button] label "PLAY"
│   └── QuitButton        [UI → Button] label "QUIT"
└── MenuManager           [Add MenuManager.cs]
     ↳ Drag PlayButton → playButton slot
     ↳ Drag QuitButton → quitButton slot
```

> **Important:** Add both `MenuScene` and `GameScene` to **Build Settings → Scenes In Build**
> (MenuScene at index 0, GameScene at index 1).

---

## 5. Scene Hierarchy — GameScene

Create: **File → New Scene → Save As `GameScene`**.

```
GameScene
├── Main Camera           (Orthographic, Size 5, Background: dark blue/black)
│   └── [Add ScreenBounds.cs]
│
├── --- MANAGERS ---
├── GameSceneBootstrap     [Add GameSceneBootstrap.cs]
│   ↳ Drag EnemySpawner GO → enemySpawner slot
├── EnemySpawner           [Add EnemySpawner.cs]
│   ↳ Drag enemy prefabs into the 4 prefab slots
│
├── --- PLAYER ---
├── Player                 [Instantiate from prefab or place directly]
│   ├── SpriteRenderer     (assign player ship sprite, Sorting Layer: Default)
│   ├── BoxCollider2D      (Is Trigger ✓, size fits the ship)
│   ├── Rigidbody2D        (Body Type: Kinematic)
│   └── PlayerController.cs
│       ↳ Drag BulletPrefab → bulletPrefab slot
│   Tag: Player | Layer: Player
│
├── --- BACKGROUND ---
├── ParallaxBG             [Add ParallaxBackground.cs]
│   ├── StarsFar           (SpriteRenderer, Sorting Layer: Background, scroll speed 0.5)
│   └── StarsNear          (SpriteRenderer, Sorting Layer: Midground, scroll speed 1.5)
│   ↳ Drag children into layers[] array on ParallaxBackground component
│
├── --- UI ---
├── Canvas (Screen Space - Overlay)
│   ├── HUD
│   │   ├── ScoreText      [UI → Text] top-left, "Score: 0"
│   │   ├── HealthText     [UI → Text] top-right, "♥ ♥ ♥"
│   │   ├── WaveText       [UI → Text] top-center, "Wave 1"
│   │   └── PowerUpTimer   [UI → Text] below wave, hidden by default
│   │
│   ├── WaveAnnouncement   [UI → Text] center screen, font size 40, hidden
│   │
│   ├── GameOverPanel      [UI → Panel] hidden by default
│   │   ├── GameOverTitle  [UI → Text] "GAME OVER"
│   │   ├── FinalScoreText [UI → Text] "Final Score: 0"
│   │   ├── RestartButton  [UI → Button] "RESTART"
│   │   └── MenuButton     [UI → Button] "MAIN MENU"
│   │
│   └── PausePanel         [UI → Panel] hidden by default
│       ├── PausedTitle    [UI → Text] "PAUSED"
│       ├── ResumeButton   [UI → Button] "RESUME"
│       └── MainMenuBtn    [UI → Button] "MAIN MENU"
│
├── UIManager              [Add UIManager.cs]
│   ↳ Drag all text/button/panel references into the Inspector slots
│
└── MenuManager            [Add MenuManager.cs]  ← for pause functionality
    ↳ Drag PausePanel, ResumeButton, MainMenuBtn into slots
    ↳ Leave playButton/quitButton empty (those are MenuScene-only)
```

---

## 6. Prefab Creation

### 6a. Bullet Prefab

1. Create an empty GameObject, name it `Bullet`.
2. Add **SpriteRenderer** → assign a small rectangle/circle sprite, tint yellow.
3. Add **BoxCollider2D** → Is Trigger ✓, fit to sprite.
4. Add **Rigidbody2D** → Body Type: **Kinematic**.
5. Add **Bullet.cs** script.
6. Drag into `Assets/Prefabs/` folder → delete from scene.

### 6b. Enemy Prefabs

Create four prefabs following this pattern:

| Prefab Name     | EnemyType | maxHealth | scoreValue | moveSpeed | shootInterval | Sprite Color |
|-----------------|-----------|-----------|------------|-----------|---------------|-------------|
| `EnemyBasic`    | Basic     | 1         | 100        | 3         | 2.0           | Red          |
| `EnemyZigzag`  | Zigzag    | 1         | 150        | 3         | 1.8           | Orange       |
| `EnemyTank`    | Tank      | 4         | 300        | 2         | 1.5           | Purple       |
| `EnemyBoss`    | Boss      | 25        | 2000       | 1.5       | 1.0           | Dark Red     |

For each:
1. Create GameObject, add **SpriteRenderer** (triangle or diamond sprite, tinted).
2. Add **BoxCollider2D** (Is Trigger ✓), **Rigidbody2D** (Kinematic).
3. Add **Enemy.cs**, set values per table above.
4. **Tag: Enemy | Layer: Enemy**.
5. Optionally drag power-up prefabs into the `powerUpPrefabs` array.
6. Drag into `Assets/Prefabs/`.

### 6c. Power-Up Prefabs

Create four prefabs:

| Prefab Name         | PowerUpType   | Sprite Color |
|---------------------|---------------|-------------|
| `PowerUp_Health`    | HealthRestore | Green        |
| `PowerUp_Spread`   | SpreadShot    | Yellow       |
| `PowerUp_Shield`   | Shield        | Cyan         |
| `PowerUp_RapidFire`| RapidFire     | Red          |

For each:
1. Create GameObject, add **SpriteRenderer** (small circle sprite, tinted).
2. Add **CircleCollider2D** (Is Trigger ✓), **Rigidbody2D** (Kinematic).
3. Add **PowerUp.cs**, set the `type` enum.
4. **Tag: PowerUp | Layer: PowerUp**.
5. Drag into `Assets/Prefabs/`.

---

## 7. Sprite Creation Guidance

You do **not** need external art. Unity's built-in sprites work great for prototyping:

### Using Unity's Default Sprites
- **Right-click in Project → Create → Sprites → Square / Circle / Triangle / Diamond**.
- Tint them via the SpriteRenderer's **Color** field.

### Recommended Shapes
| Object       | Shape          | Color           |
|-------------|----------------|-----------------|
| Player Ship | Triangle (▲)   | White / Blue    |
| Basic Enemy | Triangle (▼)   | Red             |
| Zigzag      | Diamond (◆)    | Orange          |
| Tank        | Square (■)     | Purple          |
| Boss        | Large Diamond  | Dark Red        |
| Bullet      | Small Circle   | Yellow / Green  |
| Power-Up    | Small Circle   | Per type color  |
| Stars (BG)  | Custom or PNG  | White dots      |

### Creating Star Background Sprites
1. Open any image editor (GIMP, Paint.NET, or Photoshop).
2. Create a **512 × 1024** black image.
3. Scatter tiny white dots randomly.
4. Save as `StarsFar.png` (sparse dots) and `StarsNear.png` (more/brighter dots).
5. Import into `Assets/Sprites/`.
6. Set **Texture Type: Sprite**, **Wrap Mode: Repeat**, **Filter: Point**.

### Free Asset Alternatives
- [Kenney Space Shooter Redux](https://kenney.nl/assets/space-shooter-redux) (CC0)
- Unity Asset Store → search "2D space shooter" (many free packs)

---

## 8. Audio Setup

### Where to Get Free SFX
- [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds) (CC0)
- [FreeSFX.co](https://freesfx.co.uk/) — space / laser / explosions
- [JSFXR](https://sfxr.me/) — generate retro SFX in-browser, download as `.wav`
- Unity Asset Store → search "free SFX"

### Assigning Clips
1. Import `.wav` / `.ogg` files into `Assets/Audio/`.
2. Select the **AudioManager** GameObject.
3. Drag each clip into the matching slot:
   - `shootClip`, `explosionClip`, `powerupClip`, `playerHitClip`, `menuClickClip`, `bossWarningClip`

### Recommended Clip Mapping

| Slot            | JSFXR Preset | Description               |
|-----------------|-------------|---------------------------|
| shootClip       | Laser/Shoot | Short pew sound            |
| explosionClip   | Explosion   | Brief boom                 |
| powerupClip     | Powerup     | Rising chime               |
| playerHitClip   | Hit/Hurt    | Impact thud                |
| menuClickClip   | Click       | UI button click            |
| bossWarningClip | Alarm       | Warning siren, 1-2 seconds |

> **Tip:** The game runs fine without audio clips — you'll just get silent playback.
> No errors are thrown for null clips.

---

## 9. Input Configuration

The game uses Unity's **legacy Input Manager** (works out-of-the-box):

| Action    | Keys                     | Input API                          |
|-----------|--------------------------|------------------------------------|
| Move      | WASD / Arrow Keys        | `Input.GetAxisRaw("Horizontal/Vertical")` |
| Shoot     | Space                    | `Input.GetKey(KeyCode.Space)`      |
| Pause     | Escape                   | `Input.GetKeyDown(KeyCode.Escape)` |

No additional Input Manager configuration is needed — Unity's defaults handle WASD
and Arrows on the `Horizontal` / `Vertical` axes.

---

## 10. Physics / Collision Matrix

Go to **Edit → Project Settings → Physics 2D → Layer Collision Matrix**.

Enable collisions **only** between:

| Layer A       | Layer B     | Why                              |
|---------------|-------------|----------------------------------|
| PlayerBullet  | Enemy       | Player shots hit enemies         |
| EnemyBullet   | Player      | Enemy shots hit player           |
| PowerUp       | Player      | Player collects power-ups        |
| Enemy         | Player      | Body collision damages player    |

**Disable everything else** to avoid wasted physics checks.

---

## 11. Step-by-Step Windows Build

1. **File → Build Settings** (Ctrl+Shift+B).
2. **Add Open Scenes** — make sure both `MenuScene` (index 0) and `GameScene` (index 1) are listed.
3. **Platform:** select **PC, Mac & Linux Standalone**.
4. **Target Platform:** Windows.
5. **Architecture:** x86_64.
6. Click **Player Settings**:
   - **Product Name:** Space Shooter
   - **Company Name:** (your name)
   - **Resolution → Fullscreen Mode:** Windowed (for testing) or Fullscreen.
   - **Default Resolution:** 1280 × 720 or 1920 × 1080.
7. Click **Build** (or **Build and Run**).
8. Choose an output folder (e.g. `Builds/Windows/`).
9. Unity compiles and produces:
   ```
   Builds/Windows/
   ├── SpaceShooter.exe          ← run this
   ├── SpaceShooter_Data/
   ├── UnityCrashHandler64.exe
   └── UnityPlayer.dll
   ```
10. **Distribute** by zipping the entire folder.

---

## 12. Troubleshooting

### Scripts won't compile
- Make sure **every** `.cs` file is inside `Assets/Scripts/`.
- Check Unity Console (Window → General → Console) for red errors.
- Ensure the filenames match the class names exactly (C# is case-sensitive).

### Player doesn't move
- Confirm the Player GameObject has `PlayerController.cs` attached.
- Confirm `GameManager.Instance.CurrentState` is `Playing` — you need `GameSceneBootstrap`
  calling `GameManager.Instance.StartGame()`.

### Bullets pass through enemies
- Both objects need **Collider2D** components with **Is Trigger ✓**.
- At least one needs a **Rigidbody2D** (Kinematic is fine).
- Check Tags: enemy must be tagged `Enemy`, player tagged `Player`.
- Check the Physics 2D collision matrix.

### Enemies never spawn
- `EnemySpawner` must have prefabs assigned in the Inspector.
- `GameSceneBootstrap` must reference the `EnemySpawner` and call `BeginSpawning()`.
- Ensure `GameManager.StartGame()` is called (sets state to `Playing`).

### Background doesn't scroll / loop
- Each background layer child needs a **SpriteRenderer** with a sprite assigned.
- The sprite's **Wrap Mode** should be **Repeat** for seamless tiling.
- The `ParallaxBackground` component's `layers[]` array must reference the child Transforms.
- Duplicate each layer sprite and stack them vertically so one is always visible.

### No sound plays
- Assign AudioClips in the AudioManager Inspector slots.
- Make sure the AudioManager GameObject is in the scene (or persists via DontDestroyOnLoad).
- Check `sfxVolume` is > 0.

### Game Over doesn't trigger
- `GameManager.TakeDamage()` calls `GameOver()` when health reaches 0.
- Make sure enemy bullets have `owner = Enemy` and the player is tagged `Player`.

### Pause menu doesn't appear
- The `MenuManager` in GameScene needs the `pausePanel` reference assigned.
- Press **Escape** during gameplay (state must be `Playing`).

### Build is a black screen
- Verify `MenuScene` is at index 0 in Build Settings → Scenes In Build.
- Make sure the Camera background is set (not transparent/clear).

---

## Script Overview

| Script                  | Role                                           |
|-------------------------|-------------------------------------------------|
| `GameManager.cs`        | Singleton state machine, score, health, waves   |
| `PlayerController.cs`   | Movement, shooting, invincibility, power-ups    |
| `Bullet.cs`             | Bullet flight, off-screen destroy, collision     |
| `BulletSpawner.cs`      | Static pattern factory (single, spread, burst)   |
| `Enemy.cs`              | 4 enemy types with movement, shooting, drops     |
| `EnemySpawner.cs`       | Wave logic, difficulty scaling, boss waves       |
| `PowerUp.cs`            | 4 power-up types, float-down, pickup trigger     |
| `UIManager.cs`          | HUD, game-over screen, wave announcements        |
| `MenuManager.cs`        | Main menu buttons, pause menu (ESC)              |
| `ParallaxBackground.cs` | Multi-layer scrolling star field                 |
| `AudioManager.cs`       | SFX pooling, volume control                      |
| `ScreenBounds.cs`       | World-space screen edge utility                  |
| `GameSceneBootstrap.cs` | Scene initializer for GameScene                  |

---

## License

This project is provided as-is for educational and personal use.
All code is original. Art and audio assets are your responsibility to source
(see suggestions above for CC0 / free options).

Happy shooting! 🎮
