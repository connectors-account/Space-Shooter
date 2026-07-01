# Space Shooter — Unity (Windows Desktop)

A complete, simple 2D arcade space-shooter built in Unity with C#. Fly your ship,
blast waves of enemies, survive as long as you can, and rack up a high score.
All visuals are generated procedurally from geometric shapes, so the project runs
with **zero imported art assets**.

---

## Features

- **Player**: keyboard movement (WASD / arrow keys), continuous shooting (Space / Left-Click), health with an on-screen HP bar.
- **Enemies**: three movement patterns (straight, sine wave, diagonal bounce), randomized shooting, health, and score rewards.
- **Bullets**: shared bullet logic for both player and enemy projectiles, with off-screen cleanup.
- **Game loop**: main menu → play → game over → restart.
- **Waves**: progressively larger enemy waves with an on-screen wave banner.
- **Scoring & HUD**: live score, current wave, and health display.
- **Parallax background**: procedurally generated two-layer scrolling starfield.
- **Auto-built scene**: a single `GameBootstrap` object assembles the whole game at runtime — no manual scene wiring required.

---

## Controls

| Action | Keys |
|--------|------|
| Move   | `W` `A` `S` `D` or Arrow keys |
| Shoot  | `Space` or Left Mouse Button |
| Menu / Quit | On-screen buttons |

---

## Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scenes/
│   │   └── Main.unity            # The single game scene (contains GameBootstrap)
│   ├── Scripts/
│   │   ├── GameBootstrap.cs      # Builds camera, managers & prefabs at runtime
│   │   ├── GameManager.cs        # Game state, scoring, waves, spawning
│   │   ├── PlayerController.cs    # Player movement, shooting, health
│   │   ├── EnemyController.cs     # Enemy movement patterns & shooting
│   │   ├── BulletController.cs    # Bullet movement/collision + shape factory
│   │   ├── UIManager.cs           # HUD, main menu, game over (auto-built UI)
│   │   └── BackgroundScroller.cs  # Parallax starfield
│   ├── Prefabs/                  # (optional) drop custom prefabs here
│   └── Resources/                # (optional) runtime-loadable assets
└── ProjectSettings/
    ├── ProjectVersion.txt         # Unity version
    ├── ProjectSettings.asset      # Product/company name (used for .exe name)
    ├── TagManager.asset           # Custom tags: Enemy, PlayerBullet, EnemyBullet
    ├── InputManager.asset         # Horizontal/Vertical/Fire1 axes
    └── EditorBuildSettings.asset  # Registers Main.unity in the build
```

---

## Requirements

- **Unity 2022.3 LTS** (any 2022.3.x). The project is pinned to `2022.3.20f1`
  but any recent 2022.3 LTS opens it fine (accept the upgrade prompt if shown).
- **Windows Build Support (Mono)** module — install it via Unity Hub if not present.
- Uses the built-in **UGUI** and legacy **Input Manager** (no extra packages needed).

---

## 1. Import the Project into Unity

1. Install **Unity Hub** and a **Unity 2022.3 LTS** editor.
   - In Unity Hub → *Installs* → *Install Editor* → pick 2022.3.x.
   - During install, tick **Windows Build Support (Mono)**.
2. In Unity Hub → *Projects* → **Add** → select the `space_shooter_game` folder.
3. Click the project to open it. Unity will import and generate its `Library/`
   and `.meta` files on first open (this can take a minute).

> If Unity ever asks about the scripting/input backend, keep the defaults
> (Mono + built-in Input Manager). This project does **not** use the new Input System package.

---

## 2. Set Up / Verify the Scene

The scene is ready to go — you normally don't need to change anything.

1. In the **Project** window open `Assets/Scenes/Main.unity`.
2. You should see a single GameObject named **GameBootstrap** in the Hierarchy.
3. Press **Play** ▶. The bootstrapper creates the camera, background, UI, and
   player/enemy prefabs automatically. Click **START** to play.

**If you ever need to recreate the scene from scratch:**
1. `File → New Scene` → *Basic (Built-in)* → save as `Assets/Scenes/Main.unity`.
2. `GameObject → Create Empty`, rename it to `GameBootstrap`.
3. With it selected, in the Inspector click **Add Component** → search **GameBootstrap** → add it.
4. Delete any extra lights/objects you don't need (a 2D game needs none). Save.

**Tags used by the game** (already defined in `TagManager.asset`): `Player`
(built-in), `Enemy`, `PlayerBullet`, `EnemyBullet`. If you build prefabs
manually, tag them accordingly. Optional layers `Player`, `Enemies`, `Bullets`
are provided if you want physics-based collision filtering.

---

## 3. Configure Build Settings for Windows Desktop

1. `File → Build Settings…`
2. Under **Scenes In Build**, click **Add Open Scenes** (make sure
   `Scenes/Main.unity` is listed and ticked). It's already registered, but this
   guarantees it.
3. In the **Platform** list select **Windows, Mac & Linux** (a.k.a. *PC, Mac &
   Linux Standalone*). If it isn't the active platform, click
   **Switch Platform** (wait for the reimport).
4. Set:
   - **Target Platform**: `Windows`
   - **Architecture**: `x86_64` (Intel/AMD 64-bit — standard for modern Windows)
5. (Optional) `Player Settings…` → set **Company Name** and **Product Name**
   (Product Name becomes the `.exe` filename). Defaults are `IndieDev` /
   `SpaceShooter`.

---

## 4. Compile to a Windows Executable (.exe)

1. Still in **Build Settings**, click **Build** (or **Build And Run**).
2. Choose/create an output folder, e.g. `Builds/Windows/`.
3. Unity compiles the game. When finished you'll get:
   ```
   Builds/Windows/
   ├── SpaceShooter.exe          ← double-click to run
   ├── UnityPlayer.dll
   ├── SpaceShooter_Data/         (all game data — keep next to the .exe)
   └── MonoBleedingEdge/
   ```
4. Run `SpaceShooter.exe`. To distribute, zip the **entire** output folder —
   the `.exe` needs the `_Data` folder and DLLs beside it.

> **Note on cross-building:** producing the Windows `.exe` must be done from a
> Unity editor with the **Windows Build Support** module installed. Building
> from Windows is the most reliable; building a Windows target from macOS/Linux
> Unity also works if that module is installed.

---

## Tuning the Game

Select the relevant object at runtime, or edit defaults on the scripts:

- **Difficulty / waves**: `GameManager` → `baseEnemiesPerWave`,
  `enemiesAddedPerWave`, `spawnInterval`, `timeBetweenWaves`.
- **Player feel**: `PlayerController` → `moveSpeed`, `fireRate`, `bulletSpeed`,
  `maxHealth`, `bulletDamage`, `collisionDamage`.
- **Enemy behavior**: `EnemyController` → `moveSpeed`, `shootChance`,
  `min/maxShootInterval`, `maxHealth`, `scoreValue`.

## Using Custom Art (optional)

The game draws simple shapes by default. To use sprites instead:
1. Import your sprites into `Assets/` (e.g. `Assets/Resources/`).
2. Build Player / Enemy / Bullet prefabs with `SpriteRenderer`, a `Rigidbody2D`
   (Kinematic, gravity 0), a trigger `Collider2D`, and the matching controller script.
3. Assign them in the Inspector:
   - `GameBootstrap.playerPrefabOverride` / `enemyPrefabOverride`
   - `PlayerController.bulletPrefab` / `EnemyController.bulletPrefab`

---

## Troubleshooting

- **Nothing happens on Play**: ensure the `GameBootstrap` component is on an
  active GameObject in the scene, then click the **START** button on the menu.
- **Buttons don't click**: the UI builder auto-adds an `EventSystem`; if you
  built a custom UI, add one via `GameObject → UI → Event System`.
- **"Tag not defined" errors**: confirm `Enemy`, `PlayerBullet`, `EnemyBullet`
  exist under `Project Settings → Tags and Layers` (they ship in `TagManager.asset`).
- **Fonts missing / blank text**: the UI uses Unity's built-in legacy font
  (`LegacyRuntime.ttf`, falling back to `Arial.ttf`) which is always available.
