# Space Shooter (Unity 2022 LTS)

A complete, self-contained vertical-scrolling space shooter for Windows, built
in **C#** for **Unity 2022.3 LTS**. Every gameplay system is implemented in full
— player, enemies, bosses, bullet patterns, power-ups, waves, scoring, audio,
object pooling, parallax background and all menus. **No external art or audio
assets are required**: every sprite is drawn procedurally at runtime, and audio
is optional (the game runs silently if no clips are supplied).

All user-facing text uses **British English**.

---

## 1. Prerequisites

| Requirement | Detail |
|-------------|--------|
| Unity Hub | Latest version |
| Unity Editor | **2022.3 LTS** (tested against 2022.3.40f1; any 2022.3.x works) |
| Modules | **Windows Build Support (IL2CPP)** — add via Unity Hub ▸ Installs ▸ ⋮ ▸ Add Modules |
| Platform | Windows 10/11 for producing the `.exe` |

> The project also runs in Play Mode on macOS/Linux for development; only the
> final build step is Windows-specific.

---

## 2. Opening the project

1. Clone / copy the `SpaceShooter` folder to your machine.
2. Open **Unity Hub ▸ Add ▸ Add project from disk** and select the
   `SpaceShooter` folder.
3. Open it with Unity **2022.3 LTS**. On first open, Unity restores the packages
   listed in `Packages/manifest.json` (Input System, TextMeshPro, 2D Sprite,
   2D Tilemap). This can take a couple of minutes.
4. If Unity prompts **"Enable the new Input System backend?"**, click **Yes**
   (this also restarts the Editor). See section 6.

---

## 3. Scene setup

The project is designed so that each scene needs only **one bootstrap
GameObject** — all other objects, pools and UI are created at runtime by code.
You create two scenes: `MainMenu` and `Game`.

### 3.1 Create the folders / scenes

1. In the Project window create `Assets/Scenes` (already present).
2. `File ▸ New Scene ▸ Basic 2D (URP)` **or** an *Empty* scene, then
   `File ▸ Save As` → `Assets/Scenes/MainMenu.unity`.
3. Repeat and save a second scene as `Assets/Scenes/Game.unity`.

### 3.2 MainMenu scene

1. `GameObject ▸ Create Empty`, name it **Bootstrap**.
2. Add the **MenuBootstrap** component (`Add Component ▸ MenuBootstrap`).
3. Delete any default `Main Camera` — the bootstrap creates its own orthographic
   camera (leaving one is also fine; the bootstrap reuses `Camera.main`).
4. Save the scene.

That's it. On Play, `MenuBootstrap` creates the camera, the parallax background
and the animated main-menu UI (Play / Quit + high score).

### 3.3 Game scene

1. `GameObject ▸ Create Empty`, name it **Bootstrap**.
2. Add the **GameBootstrap** component.
3. Save the scene.

On Play, `GameBootstrap` creates: all singletons (GameManager, SceneLoader,
AudioManager, ScoreManager, ObjectPool), the camera + CameraShake, every object
pool (with procedurally-generated prefab templates), the player (with all
components), the parallax background, the WaveManager + EnemySpawner, and the
HUD / Pause / Game-Over UIs.

### 3.4 Build Settings scene order

`File ▸ Build Settings ▸ Add Open Scenes`, then order them so **MainMenu is
index 0** and **Game is index 1**:

```
0  Scenes/MainMenu
1  Scenes/Game
```

The scene names must match the constants in
`Assets/Scripts/Utilities/Constants.cs` (`MainMenu`, `Game`).

### 3.5 Tags and Layers

A ready-made `ProjectSettings/TagManager.asset` ships with the project and
already defines everything below, so normally **no manual work is needed**.
Verify via `Edit ▸ Project Settings ▸ Tags and Layers`:

**Tags** (in addition to Unity's built-in `Player`):
`PlayerBullet`, `EnemyBullet`, `PowerUp`, `Enemy`, `Boss`

**Layers** (user layers):

| Index | Layer |
|-------|-------|
| 8  | Player |
| 9  | Enemy |
| 10 | PlayerBullet |
| 11 | EnemyBullet |
| 12 | PowerUp |

If you created the project without the supplied `TagManager.asset`, add the
tags/layers above manually (the code logs a clear warning if a tag is missing).

### 3.6 Physics2D collision matrix

`Edit ▸ Project Settings ▸ Physics 2D ▸ Layer Collision Matrix`. Collisions are
handled through trigger callbacks, so only meaningful pairs need to interact.
Recommended matrix (✔ = collide/overlap detection enabled):

|                | Player | Enemy | PlayerBullet | EnemyBullet | PowerUp |
|----------------|:------:|:-----:|:------------:|:-----------:|:-------:|
| **Player**       |   –    |  ✔    |      ✘       |     ✔       |   ✔     |
| **Enemy**        |  ✔     |  ✘    |      ✔       |     ✘       |   ✘     |
| **PlayerBullet** |  ✘     |  ✔    |      ✘       |     ✘       |   ✘     |
| **EnemyBullet**  |  ✔     |  ✘    |      ✘       |     ✘       |   ✘     |
| **PowerUp**      |  ✔     |  ✘    |      ✘       |     ✘       |   ✘     |

> The bootstrap does not assign layers automatically; if you want the matrix to
> take effect, set each pooled object's layer (see below). The game is fully
> playable with everything on the `Default` layer because collisions are also
> gated by **tag** checks in code, which is the primary filter.

To assign layers on the runtime-generated objects, set them in the respective
`Build*Template` methods of `GameBootstrap.cs`, or assign layers on hand-made
prefabs (section 4).

---

## 4. Prefab creation (optional — only if you prefer authored prefabs)

The bootstrap builds prefab **templates** in code, so you do **not** need to
create prefabs to play. If you would rather use real prefab assets (e.g. to tune
them in the Inspector or assign layers/art), create them as follows and wire them
into an `ObjectPool` component's `pools` list using the keys in `Constants.cs`.

General recipe for every prefab:

1. `GameObject ▸ Create Empty`, add a **SpriteRenderer**.
2. Add a **Collider2D** (Circle for ships/bullets/power-ups) and tick
   **Is Trigger**.
3. Add a **Rigidbody2D**, set **Body Type = Kinematic**, **Gravity Scale = 0**.
4. Add the script component (below) and set the tag/layer.
5. Drag into `Assets/Prefabs`.

| Prefab | Script | Tag | Layer | Notes |
|--------|--------|-----|-------|-------|
| Player | `PlayerController` + `PlayerHealth` + `PlayerShooter` + `PlayerInputHandler` + `Bomb` | Player | Player | Sprite auto-generated |
| PlayerBullet | `Bullet` | PlayerBullet | PlayerBullet | radius ≈ 0.12 |
| EnemyBullet | `Bullet` | EnemyBullet | EnemyBullet | radius ≈ 0.12 |
| EnemyDrone | `EnemyDrone` | Enemy | Enemy | HP 1, straight dive |
| EnemyFighter | `EnemyFighter` | Enemy | Enemy | HP 3, sine weave + spread |
| EnemyBomber | `EnemyBomber` | Enemy | Enemy | HP 5, aimed shots, guaranteed drop |
| EnemyBoss | `EnemyBoss` | Boss | Enemy | HP 200, 3 phases |
| Explosion | `ExplosionVFX` | Untagged | Default | no collider needed |
| PowerUp Shield | `PowerUpShield` | PowerUp | PowerUp | — |
| PowerUp RapidFire | `PowerUpRapidFire` | PowerUp | PowerUp | — |
| PowerUp TripleShot | `PowerUpTripleShot` | PowerUp | PowerUp | — |
| PowerUp Bomb | `PowerUpBomb` | PowerUp | PowerUp | — |
| PowerUp Speed | `PowerUpSpeed` | PowerUp | PowerUp | — |

Each ship/bullet/power-up script assigns its own procedurally-generated sprite in
`Awake` if the SpriteRenderer has none, so leaving the sprite empty is fine.

---

## 5. Procedural sprite generation

`Assets/Scripts/Utilities/SpriteGenerator.cs` draws every sprite at runtime using
`Texture2D.SetPixel`, so **no art files are shipped or needed**. Sprites are
cached after first creation. Available factory methods:

- `CreatePlayerSprite()`
- `CreateEnemyDroneSprite()`, `CreateEnemyFighterSprite()`, `CreateEnemyBomberSprite()`, `CreateBossSprite()`
- `CreateBulletSprite(Color)`
- `CreatePowerUpSprite(PowerUpType)`
- `CreateExplosionSprite()`, `CreateShieldSprite()`, `CreateStarSprite()`, `CreateSquareSprite()`

The parallax star-fields are also generated procedurally in
`ParallaxBackground.cs`.

---

## 6. Input System setup

The game uses the **Unity new Input System** (`com.unity.inputsystem`).
`PlayerInputHandler.cs` creates its input actions entirely in code — there is no
`.inputactions` asset to configure.

Enable the backend once:

1. `Edit ▸ Project Settings ▸ Player ▸ Other Settings ▸ Active Input Handling`.
2. Set it to **Input System Package (New)** (or **Both**).
3. Let the Editor restart when prompted.

Controls:

| Action | Keyboard / Mouse | Gamepad |
|--------|------------------|---------|
| Move | WASD / Arrow keys | Left stick / D-pad |
| Fire | Space / Left mouse (hold to auto-fire) | A / South / Right trigger |
| Bomb | B / Left Shift / Right mouse | X / West |
| Pause | Esc / P | Start |

---

## 7. Audio setup (optional)

`AudioManager.cs` loads every `AudioClip` found under **`Assets/Resources/Audio`**
by file name. The game runs silently if the folder is empty.

1. Create the folder `Assets/Resources/Audio`.
2. Import your `.wav`/`.ogg`/`.mp3` clips and **name them exactly** as the
   constants in `Constants.cs`:

**SFX:** `sfx_player_shoot`, `sfx_enemy_shoot`, `sfx_explosion`, `sfx_player_hit`,
`sfx_powerup`, `sfx_bomb`, `sfx_shield_up`, `sfx_shield_down`, `sfx_ui_click`,
`sfx_wave_start`, `sfx_boss_spawn`

**Music:** `music_menu`, `music_game`, `music_boss`

SFX play through a pool of 8 `AudioSource`s; music uses a single looping source.
Volumes are persisted in `PlayerPrefs` and adjustable from the Pause menu.

---

## 8. Object pool pre-warm configuration

`ObjectPool.cs` is a generic, dictionary-keyed pool. `GameBootstrap` registers
and pre-warms all pools in code with these default counts:

| Pool key (Constants) | Pre-warm |
|----------------------|:--------:|
| `PlayerBullet` | 48 |
| `EnemyBullet` | 120 |
| `EnemyDrone` | 24 |
| `EnemyFighter` | 16 |
| `EnemyBomber` | 8 |
| `EnemyBoss` | 1 |
| `Explosion` | 24 |
| `PowerUp*` (each) | 6 |

Adjust these on the **GameBootstrap** component in the Inspector, or configure a
manual `ObjectPool` component's `pools` list (each entry: key, prefab, prewarm
count, expandable). Pools are `expandable` by default, so they grow if exhausted.

---

## 9. WaveData ScriptableObjects (optional)

Waves are driven by `WaveManager.cs`. If you assign no `WaveData` assets it
generates escalating procedural waves automatically (a boss every 5th wave).

To author fixed waves:

1. `Assets ▸ Create ▸ Space Shooter ▸ Wave Data`.
2. Configure `entries` (enemy type + count), `spawnInterval`, `hasBoss` and
   `difficultyMultiplier`.
3. Add a manual `WaveManager` to the Game scene (instead of relying on the
   bootstrap's) and drag your `WaveData` assets into its `waves` list.

Difficulty scales automatically each wave: enemy speed rises
(`difficultyPerWave`), spawn interval shrinks (`spawnIntervalScale`) and enemy
counts grow.

---

## 10. Build steps (Windows `.exe`)

1. `File ▸ Build Settings`.
2. Ensure both scenes are added and ordered (MainMenu = 0, Game = 1).
3. Select **Windows, Mac, Linux** ▸ Target Platform **Windows** ▸ Architecture
   **x86_64**.
4. Click **Switch Platform** if needed.
5. Click **Build**, choose an output folder (e.g. `Builds/Windows`).
6. Unity produces `SpaceShooter.exe` plus a `SpaceShooter_Data` folder and
   `UnityPlayer.dll`. Ship the whole folder together.

For a smaller, faster build set **Player ▸ Other Settings ▸ Scripting Backend =
IL2CPP** and **Managed Stripping Level = Low**.

---

## 11. Troubleshooting

| Symptom | Cause / Fix |
|---------|-------------|
| `InvalidOperationException: You are trying to read Input using ... UnityEngine.Input` or input does nothing | Active Input Handling is still "Old". Set it to **New** (or **Both**) — section 6 — and restart the Editor. |
| Warnings: *"Tag 'Enemy' is not defined"* | The custom tags/layers are missing. Confirm `ProjectSettings/TagManager.asset` is present, or add the tags/layers manually (section 3.5). |
| Bullets pass through enemies | Check that the objects carry the correct **tags** (collision logic is tag-based). If you assigned physics layers, verify the Physics2D collision matrix (section 3.6). |
| Nothing appears on Play | Make sure the scene contains the **Bootstrap** GameObject with `GameBootstrap` (Game) or `MenuBootstrap` (MainMenu). |
| No sound | Expected if `Assets/Resources/Audio` is empty. Add clips named per section 7. |
| Play button on the menu does nothing | The `Game` scene must be in Build Settings and named exactly `Game`. |
| TextMeshPro "import TMP Essentials" popup | Not required — the UI uses uGUI legacy `Text` with Unity's built-in font. You may dismiss the popup. |
| Boss health bar never shows | It only appears on boss waves (every 5th wave by default). |
| Editor stutters on first spawn of each type | First-time sprite generation; sprites are cached afterwards. Increase pre-warm counts to smooth it further. |

---

## 12. Project structure

```
SpaceShooter/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/        GameManager, SceneLoader, ObjectPool, GameBootstrap, MenuBootstrap
│   │   ├── Player/      PlayerController, PlayerHealth, PlayerShooter, PlayerInputHandler
│   │   ├── Enemy/       EnemyBase, Drone, Fighter, Bomber, Boss, Spawner, WaveManager, WaveData
│   │   ├── Weapons/     Bullet, BulletPattern(+Straight/Spread/Spiral/Aimed), Bomb
│   │   ├── PowerUps/    PowerUpBase + Shield/RapidFire/TripleShot/Bomb/Speed
│   │   ├── UI/          HUDController, MainMenu, PauseMenu, GameOver, HighScoreDisplay, UIFactory
│   │   ├── Audio/       AudioManager
│   │   ├── Background/  ParallaxBackground
│   │   ├── Scoring/     ScoreManager, HighScoreTable
│   │   └── Utilities/   SpriteGenerator, CameraShake, ExplosionVFX, Constants
│   ├── Scenes/          MainMenu.unity, Game.unity (you create these — section 3)
│   ├── Prefabs/         optional authored prefabs (section 4)
│   └── Resources/Audio/ optional audio clips (section 7)
├── Packages/manifest.json
├── ProjectSettings/     TagManager.asset, ProjectVersion.txt
└── README.md
```

Enjoy, and good hunting, pilot. 🚀
