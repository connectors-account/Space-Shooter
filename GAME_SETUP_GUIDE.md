# Space Shooter — Unity 2022 LTS (C#)

A complete 2D top-down space-shooter game framework for **Unity 2022.3 LTS** on **Windows**.
Every gameplay system is implemented in full, production-quality C#: player control & weapons,
pooled bullets, data-driven enemies with movement/shoot patterns, a multi-phase boss, wave &
power-up spawning, a full UI stack (main menu, HUD, pause, game over) and a looping parallax
background.

> This repository contains **only the C# scripts and ScriptableObject definitions**. Unity
> serializes scenes, prefabs and asset references inside the Editor, so you create those visual
> assets once (following the steps below) and wire the provided components into them. No script
> contains placeholders — every method is fully implemented.

---

## 1. Prerequisites

- **Windows 10 / 11** (64-bit).
- **Unity Hub** (latest).
- **Unity 2022.3 LTS** (any 2022.3.x patch). Install via Unity Hub with these modules:
  - *Windows Build Support (IL2CPP)* — required for a standalone Windows build.
  - *(Optional)* *Visual Studio 2022 Community* with the **Game development with Unity** workload
    for editing/IntelliSense.
- Around 3 GB of free disk space for the Editor + project.

---

## 2. Project setup (step by step)

### 2.1 Create the project
1. Open **Unity Hub → Projects → New project**.
2. Choose the **2D (Built-in Render Pipeline)** template (or **2D URP** — both work; this guide
   assumes Built-in 2D).
3. Name it `SpaceShooter`, pick a location, click **Create project**.

### 2.2 Import the scripts
1. Close Unity (or keep it open — either works).
2. Copy the provided `Assets/Scripts` and `Assets/ScriptableObjects` folders into your new
   project's `Assets/` folder, preserving the folder structure:
   ```
   Assets/
     Scripts/       (Core, Player, Enemy, Bullets, Spawning, PowerUps, UI, Background, Utilities)
     ScriptableObjects/  (WaveData.cs, EnemyData.cs, PowerUpData.cs, WeaponData.cs)
   ```
3. Return to Unity and let it recompile. Open **Window → General → Console** and confirm there
   are **no compile errors**. (You will see the new menu **Assets → Create → Space Shooter**.)

### 2.3 Namespaces
All scripts live under the root namespace `SpaceShooter` with sub-namespaces
(`SpaceShooter.Core`, `SpaceShooter.Player`, `SpaceShooter.Enemy`, `SpaceShooter.Bullets`,
`SpaceShooter.Spawning`, `SpaceShooter.PowerUps`, `SpaceShooter.UI`, `SpaceShooter.Background`,
`SpaceShooter.Utilities`, `SpaceShooter.Data`). No assembly definition files are used, so
everything compiles into `Assembly-CSharp` and cross-references freely.

### 2.4 Create the scenes
Create three scenes under `Assets/Scenes/`:
- `MainMenu.unity`
- `Game.unity`
- `GameOver.unity` *(optional — the Game scene also contains an in-place Game Over panel, so a
  dedicated scene is optional. If you skip it, set the GameOverController's `standaloneScene`
  to false and use it as a panel.)*

Add them to the build in the exact order below (see §10).

---

## 3. Scene hierarchies

### 3.1 MainMenu scene
```
MainMenu (scene)
├── Main Camera            (Orthographic, size ~5, background black)
├── EventSystem            (auto-created with the first UI element)
├── Managers
│   ├── GameManager        (GameManager.cs)          ← persists via DontDestroyOnLoad
│   ├── AudioManager       (AudioManager.cs + 2 AudioSource)  ← persists
│   └── SceneLoader        (SceneLoader.cs)           ← persists
├── Background
│   └── StarFieldParticles (ParticleSystem, looping)
└── Canvas (Screen Space - Overlay)
    ├── MainMenuController  (MainMenuController.cs on the Canvas or a child)
    ├── MainPanel
    │   ├── TitleText       (Text/TMP)  ← assign to MainMenuController.titleTransform
    │   ├── PlayButton
    │   ├── OptionsButton
    │   └── QuitButton
    └── OptionsPanel        (inactive by default)
        ├── MusicVolumeSlider
        ├── SFXVolumeSlider
        ├── HighScoreText
        └── BackButton
```
> Put `GameManager`, `AudioManager`, `SceneLoader` in the **MainMenu** scene (the first scene).
> Because they call `DontDestroyOnLoad`, they survive into the Game and GameOver scenes and must
> **not** be duplicated there (the singleton guard destroys duplicates automatically, but it is
> cleanest to place them only once).

### 3.2 Game scene
```
Game (scene)
├── Main Camera            (Orthographic, size ~5)
├── EventSystem
├── SceneSystems
│   ├── BulletPool         (BulletPool.cs — assign player & enemy bullet prefabs)
│   ├── EnemySpawner       (EnemySpawner.cs — assign enemy & boss prefabs)
│   ├── PowerUpSpawner     (PowerUpSpawner.cs — assign power-up prefabs)
│   └── WaveManager        (WaveManager.cs — assign the ordered list of WaveData assets)
├── Background
│   ├── ParallaxBackground (ParallaxBackground.cs — holds the 3 layers below)
│   ├── Layer0_FarStars    (ParallaxLayer.cs, speed 0.5, SpriteRenderer Draw Mode = Tiled)
│   ├── Layer1_MidNebula   (ParallaxLayer.cs, speed 1.5)
│   └── Layer2_Asteroids   (ParallaxLayer.cs, speed 3)
├── Player                 (tag = "Player", layer = "Player")
│   ├── PlayerController.cs, PlayerHealth.cs, PlayerShooter.cs, PlayerAnimator.cs
│   ├── Rigidbody2D (Gravity Scale 0, Is Kinematic OR Dynamic w/ no gravity)
│   ├── Collider2D (Is Trigger = true)
│   ├── SpriteRenderer (child recommended so tilt/flash affect the visual only)
│   ├── FirePoint (empty child at the nose)  ← assign to PlayerShooter.firePoint
│   ├── MuzzleFlash (child, disabled)         ← assign to PlayerShooter.muzzleFlash
│   └── Shield (child sprite, disabled)       ← assign to PlayerHealth.shieldVisual
└── Canvas (Screen Space - Overlay)
    ├── UIManager          (UIManager.cs — assign HUD, PauseMenu, GameOver panel)
    ├── HUDController       (HUDController.cs)
    │   ├── ScoreText, WaveText, MessageText
    │   ├── HealthBar (Slider)
    │   ├── LifeIcons[] (heart images)
    │   ├── BossHealth (CanvasGroup + Slider)
    │   └── WorldCanvas ref + ScorePopup prefab
    ├── PauseMenuPanel      (PauseMenuController.cs, inactive)
    │   ├── DimBackground (Image, semi-transparent, CanvasGroup)
    │   ├── ResumeButton, RestartButton, MainMenuButton
    └── GameOverPanel       (GameOverController.cs, inactive, standaloneScene = false)
        ├── FinalScoreText, HighScoreText, NewHighScoreLabel
        ├── RetryButton, MainMenuButton
```

---

## 4. Creating the ScriptableObjects

All four data assets appear under **Assets → Create → Space Shooter**.

### 4.1 EnemyData
`Create → Space Shooter → Enemy Data`. Fields: name, health, scoreValue, moveSpeed,
shootInterval (0 = never shoots), bulletDamage, powerUpDropChance (0–1), sprite, explosionPrefab,
tint. Make several: e.g. `Grunt`, `Shooter`, `Zigzagger`, `Diver`, and a `Boss` data asset.

### 4.2 PowerUpData
`Create → Space Shooter → PowerUp Data`. Set `type` (WeaponUpgrade / Shield / HealthPack /
SpeedBoost / BombClear), `icon`, `glowColor`, `duration` (0 = instant), `magnitude`
(heal amount / speed multiplier), `scoreValue`. Make one per type.

### 4.3 WeaponData *(optional/config)*
`Create → Space Shooter → Weapon Data`. Describes fire rate, bullet speed/damage, bullets per
shot, spread angle, sprites. Useful if you want to drive `PlayerShooter` from data; by default
`PlayerShooter` uses its own serialized fields and weapon levels 1–4.

### 4.4 WaveData
`Create → Space Shooter → Wave Data`. Set `waveNumber`, then add `EnemySpawnEntry` items to the
`enemies` list — each has an `EnemyData`, `count`, `spawnInterval` and a `formation`
(Line / VShape / Random / Flanks). Toggle `hasBoss` and assign `bossData` for boss waves. Set
`timeBetweenWaves`. Create `Wave1`, `Wave2`, … and drag them (in order) into
`WaveManager.waves`.

---

## 5. Setting up prefabs

Create these under `Assets/Prefabs/`.

### 5.1 Player
- Empty GameObject `Player`, tag **Player**, layer **Player**.
- Add `SpriteRenderer` (ship sprite) as a child so tilting/flashing affects only the visual.
- Add `Rigidbody2D` (Gravity Scale = 0; Body Type = Kinematic), `BoxCollider2D`/`PolygonCollider2D`
  with **Is Trigger = ON**.
- Add components: `PlayerController`, `PlayerHealth`, `PlayerShooter`, `PlayerAnimator`.
- Add children `FirePoint`, `MuzzleFlash` (disabled), `Shield` (disabled) and assign them in the
  inspector.

### 5.2 Player bullet
- Empty `PlayerBullet`, tag **PlayerBullet**, layer **PlayerBullet**.
- `SpriteRenderer` + `Rigidbody2D` (Kinematic) + `Collider2D` (**Is Trigger = ON**).
- Add `Bullet.cs`. Assign to `BulletPool.playerBulletPrefab`.

### 5.3 Enemy bullet
- Empty `EnemyBullet`, tag **EnemyBullet**, layer **EnemyBullet**.
- Same physics setup. Add `EnemyBullet.cs`. Assign to `BulletPool.enemyBulletPrefab`.

### 5.4 Standard enemy
- Empty `Enemy`, tag **Enemy**, layer **Enemy**.
- `SpriteRenderer` (child), `Rigidbody2D` (Kinematic), `Collider2D` (**Is Trigger = ON**).
- Add `EnemyHealth`, `EnemyMover`, *(optional)* `EnemyShooter`, and the concrete `StandardEnemy`
  component. (`EnemyBase` is abstract — `StandardEnemy` is the ready-to-use subclass.)
- Assign a `FirePoint` child if it shoots. Leave `data` empty here — the spawner injects it.
- Assign this prefab to `EnemySpawner.enemyPrefab`.

### 5.5 Boss
- Like the enemy, but tag **Boss** (or Enemy), add `BossEnemy` instead of `StandardEnemy`.
- Add a main `EnemyShooter`, plus 2 child `EnemyShooter` objects for side cannons and assign them
  to `BossEnemy.sideCannons`. Assign this prefab to `EnemySpawner.bossPrefab`.

### 5.6 Power-ups
- Empty `PowerUp`, tag **PowerUp**, layer **PowerUp**.
- `SpriteRenderer` (child so it can spin independently), `Collider2D` (**Is Trigger = ON**).
- Add `PowerUpBase.cs` and assign a `PowerUpData` asset. Make one prefab per type and drag them
  into `PowerUpSpawner.powerUpPrefabs` (optionally set matching `weights`).

### 5.7 Explosion & score popup
- `Explosion` prefab: a `ParticleSystem` set to **Play On Awake** + **Stop Action = Destroy**.
  Assign to each `EnemyData.explosionPrefab` and to `PlayerHealth.explosionPrefab`.
- `ScorePopup` prefab: a small UI object with a `Text` child. Assign to
  `HUDController.scorePopupPrefab` and set `HUDController.worldCanvas` to your overlay Canvas.

### 5.8 Background layers
- Import a seamless star/nebula/asteroid sprite for each layer.
- On each layer's `SpriteRenderer` set **Draw Mode = Tiled** and size it taller than the screen,
  OR duplicate the tile and assign the duplicate to `ParallaxLayer.secondTile` for a two-tile loop.

---

## 6. Wiring component references (Inspector)

| Component | Field | Assign |
|-----------|-------|--------|
| `BulletPool` | playerBulletPrefab / enemyBulletPrefab | the two bullet prefabs |
| `EnemySpawner` | enemyPrefab / bossPrefab | standard enemy / boss prefabs |
| `PowerUpSpawner` | powerUpPrefabs[] | one prefab per power-up type |
| `WaveManager` | waves[] | ordered WaveData assets; infiniteEnemyData = a WaveData-free EnemyData |
| `PlayerShooter` | firePoint, muzzleFlash | child transforms |
| `PlayerHealth` | explosionPrefab, shieldVisual | explosion prefab / shield child |
| `UIManager` | hud, pauseMenu, gameOverPanel | the three UI controllers |
| `HUDController` | scoreText, waveText, healthBar, lifeIcons[], bossHealthBar, bossHealthGroup, messageText, scorePopupPrefab, worldCanvas | matching UI objects |
| `MainMenuController` | buttons, panels, sliders, highScoreText, titleTransform, starField | menu UI objects |
| `PauseMenuController` | panelRoot, dimGroup, buttons | pause UI objects |
| `GameOverController` | panelRoot, texts, buttons | game-over UI objects |
| `ParallaxBackground` | layers[] | the ParallaxLayer components |

The singletons (`GameManager`, `AudioManager`, `SceneLoader`, `BulletPool`, `EnemySpawner`,
`PowerUpSpawner`, `UIManager`) are accessed through `Instance` at runtime — no manual references
between them are required beyond what the table lists.

---

## 7. Input System (legacy and new)

The scripts support **both** input backends via `#if ENABLE_INPUT_SYSTEM` directives.

### 7.1 Legacy Input Manager (default, simplest)
- **Edit → Project Settings → Player → Active Input Handling = Input Manager (Old)**.
- Uses the default `Horizontal`, `Vertical`, and `Fire1` axes (already defined by Unity).
- Movement: Arrow keys / WASD. Fire: **Space** or **Left Ctrl** (`Fire1`). Pause: **Esc**.

### 7.2 New Input System
- Install **Window → Package Manager → Input System**.
- **Player → Active Input Handling = Input System Package (New)** *(or Both)*. Restart when asked.
- Unity then defines `ENABLE_INPUT_SYSTEM`, so the scripts automatically read
  `Keyboard.current` / `Gamepad.current` — no `.inputactions` asset is required for this project.
- Controls are the same (WASD/arrows, Space/Ctrl or gamepad South button, Esc to pause).

> If you choose **Both**, the new-input code path compiles and is used.

---

## 8. Physics 2D layers & collision matrix

### 8.1 Create layers
**Edit → Project Settings → Tags and Layers** — add user layers:
`Player`, `Enemy`, `PlayerBullet`, `EnemyBullet`, `PowerUp`, `Background`.
Also add the tags: `Player`, `Enemy`, `EnemyBullet`, `PlayerBullet`, `PowerUp`, `Ground`, `Boss`.

### 8.2 Collision matrix
**Edit → Project Settings → Physics 2D → Layer Collision Matrix**. Enable **only** these pairs
(everything else unchecked to avoid friendly-fire and self-collision):

| A | B | Collide? |
|---|---|----------|
| PlayerBullet | Enemy | ✅ |
| EnemyBullet | Player | ✅ |
| PowerUp | Player | ✅ |
| Enemy | Player | ✅ (optional body-contact damage) |
| PlayerBullet | EnemyBullet | ❌ |
| PlayerBullet | Player | ❌ |
| EnemyBullet | Enemy | ❌ |
| Enemy | Enemy | ❌ |
| Background | (anything) | ❌ |

All gameplay colliders use **Is Trigger = ON**; damage is handled in `OnTriggerEnter2D`. Give each
object a `Rigidbody2D` (Kinematic, Gravity Scale 0) so triggers fire.

---

## 9. Canvas & UI hierarchy

1. Create a **Canvas** (`GameObject → UI → Canvas`) — Render Mode **Screen Space - Overlay**.
2. Set the **Canvas Scaler** to **Scale With Screen Size** (reference 1920×1080) for resolution
   independence.
3. An **EventSystem** is added automatically with the first UI element; keep it.
4. Build the HUD, PauseMenuPanel and GameOverPanel as children (see §3.2) and attach the matching
   controllers. Mark the Pause and GameOver panels **inactive** by default.
5. For the boss bar, put a `Slider` under a child GameObject that has a **CanvasGroup**, and assign
   both to `HUDController` (`bossHealthBar`, `bossHealthGroup`).
6. `Text` components: the scripts use `UnityEngine.UI.Text`. If you prefer **TextMeshPro**, swap the
   field types to `TMP_Text` and update the `using` lines — otherwise use the built-in `Text`.

---

## 10. Building for Windows

1. **File → Build Settings**.
2. **Scenes In Build** — click *Add Open Scenes* / drag to set this exact order:
   1. `MainMenu`  (index 0)
   2. `Game`      (index 1)
   3. `GameOver`  (index 2, if you created it)
   > The scene **names** must match `Constants.Scenes` (`MainMenu`, `Game`, `GameOver`).
3. **Platform → Windows, Mac, Linux → Target Platform = Windows**, Architecture **x86_64**.
   Click **Switch Platform** if needed.
4. **Player Settings → Other Settings → Scripting Backend**:
   - **IL2CPP** (recommended for shipping — faster, harder to decompile; requires the IL2CPP
     module + Visual Studio C++ tools), or
   - **Mono** (faster iteration, easier build).
5. Set **Company Name**, **Product Name**, icon and default resolution as desired.
6. Back in **Build Settings → Build**, choose an empty output folder. Unity produces
   `SpaceShooter.exe` plus a `SpaceShooter_Data/` folder — ship the whole folder.
7. *(Optional)* **Build And Run** to build and immediately launch.

---

## 11. Troubleshooting

- **"The name 'Keyboard' does not exist"** — the Input System package isn't installed but a script
  hit the new-input path. Either install **Input System** (Package Manager) or set *Active Input
  Handling* to **Input Manager (Old)**; the `#if` guards then compile the legacy path.
- **Player/enemies pass through bullets** — a collider isn't a **Trigger**, an object is missing a
  **Rigidbody2D**, or the **Physics 2D collision matrix** has that layer pair disabled. Check §8.
- **Bullets never appear** — `BulletPool` prefabs not assigned, or the prefabs lack the `Bullet` /
  `EnemyBullet` component. Confirm `BulletPool` is in the Game scene.
- **`NullReferenceException` on UIManager/HUD** — a UI reference is unassigned. Every method
  null-checks, but fields you leave empty simply do nothing; assign them per §6.
- **Score/high score not saving** — high score persists via `PlayerPrefs`; it's stored per user
  profile. Deleting it: `Edit → Clear All PlayerPrefs` (or `PlayerPrefs.DeleteAll()`).
- **Two GameManagers / audio doubling** — you placed the persistent managers in more than one
  scene. Keep them only in `MainMenu`; the singleton guard destroys extras but avoid duplicating.
- **Enemies spawn off-screen or stacked** — adjust `EnemySpawner.horizontalPadding` /
  `spawnHeightOffset`, and make sure the **Main Camera** is Orthographic and tagged `MainCamera`.
- **Parallax jumps instead of looping** — set the layer `SpriteRenderer` **Draw Mode = Tiled** and
  make it taller than the screen, or assign a duplicate tile to `ParallaxLayer.secondTile`.
- **Boss health bar never hides** — ensure `HUDController.bossHealthGroup` (a CanvasGroup) is
  assigned; `BossEnemy.OnDeath` calls `HideBossHealthBar`.
- **Game frozen after pause → main menu** — the scripts reset `Time.timeScale = 1f` on resume /
  scene change; if you added custom flows, remember to restore the timescale.

---

## Script overview

| Folder | Scripts |
|--------|---------|
| `Core` | GameManager, AudioManager, SceneLoader |
| `Player` | PlayerController, PlayerHealth, PlayerShooter, PlayerAnimator |
| `Enemy` | EnemyBase (abstract), StandardEnemy, BossEnemy, EnemyHealth, EnemyMover, EnemyShooter |
| `Bullets` | Bullet, EnemyBullet, BulletPool |
| `Spawning` | WaveManager, EnemySpawner |
| `PowerUps` | PowerUpBase, PowerUpSpawner |
| `UI` | UIManager, MainMenuController, HUDController, GameOverController, PauseMenuController |
| `Background` | ParallaxLayer, ParallaxBackground |
| `Utilities` | ObjectPool, Singleton, Constants |
| `ScriptableObjects` | WaveData, EnemyData, PowerUpData, WeaponData |

Enjoy building your shooter! Every script is complete and ready to attach — the only work left is
creating the visual assets (sprites, scenes, prefabs) and wiring them in the Inspector as above.
