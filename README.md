# VOID ASSAULT — Unity 2022 LTS Space Shooter

A complete, production-quality 2D arcade space shooter written in C# for **Unity 2022.3 LTS**.
Everything — sprites, sound effects and music — is **generated procedurally at runtime**, so the
project ships with zero binary art or audio assets. Drop the scripts in, wire up a handful of
GameObjects, and you have a playable vertically-scrolling shmup with 10 hand-designed waves, a
two-phase boss, five weapon patterns, seven power-ups, an object-pooled bullet system, parallax
starfield, camera shake, and a full menu/HUD/pause/game-over UI flow.

---

## Table of Contents
1. [Prerequisites](#1-prerequisites)
2. [Project Creation](#2-project-creation)
3. [Copying the Scripts](#3-copying-the-scripts)
4. [Scene Setup](#4-scene-setup)
5. [Prefab Creation Guide](#5-prefab-creation-guide)
6. [Sprite & Audio Assignment (Bootstrap)](#6-sprite--audio-assignment-bootstrap)
7. [Physics Layers & Collision Matrix](#7-physics-layers--collision-matrix)
8. [Tags](#8-tags)
9. [Scene Hierarchy Layout](#9-scene-hierarchy-layout)
10. [Building the Windows Executable](#10-building-the-windows-executable)
11. [Controls](#11-controls)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Prerequisites

| Requirement | Version / Notes |
|-------------|-----------------|
| **Unity Hub** | Latest (https://unity.com/download) |
| **Unity Editor** | **2022.3.21f1 (LTS)** — any 2022.3.x LTS works |
| **Windows Build Support (IL2CPP)** module | Install via Unity Hub → *Installs → ⋮ → Add Modules* |
| **Visual Studio 2022** or **VS Code** | With the *Game development with Unity* / C# workload for IntelliSense (optional but recommended) |
| **OS** | Windows 10/11 x86_64 to produce a Windows build |

> The project relies on the **Input System (new)**, **TextMeshPro** and **uGUI** packages.
> These are declared in `Packages/manifest.json` and will be resolved automatically on first open.

---

## 2. Project Creation

You can either **open this folder directly** or recreate it from scratch.

### Option A — Open the provided project (recommended)
1. Copy the entire `SpaceShooter/` folder to your machine.
2. Open **Unity Hub → Open → Add project from disk** and select the `SpaceShooter/` folder.
3. Hub will detect editor version `2022.3.21f1`. If you have a different 2022.3.x LTS, click it and
   choose *Open with <your version>*. Let the Package Manager resolve dependencies (first import
   may take a minute).

### Option B — Create a new project and copy scripts in
1. In Unity Hub click **New Project**.
2. Choose the **2D (Core)** template.
3. Name it `SpaceShooter`, pick a location, click **Create project**.
4. Then follow [Section 3](#3-copying-the-scripts) to bring the scripts across, and
   [Section 6](#6-sprite--audio-assignment-bootstrap) to add the bootstrap.

---

## 3. Copying the Scripts

Copy the `Assets/Scripts/` tree into your project's `Assets/` folder, preserving the subfolders:

```
Assets/Scripts/
├── Core/        GameManager, ScoreManager, AudioManager, WaveManager, GameConstants, ObjectPool
├── Player/      PlayerController, PlayerHealth, PlayerShooter, PlayerPowerUp
├── Enemy/       EnemyBase, EnemyDiver, EnemyFormation, EnemyCircler, BossEnemy, EnemyShooter, WaveData
├── Bullet/      Bullet, BulletPool, BulletPattern
├── PowerUp/     PowerUp, PowerUpSpawner
├── Environment/ ParallaxLayer, StarField, CameraShake
├── UI/          MainMenuUI, HUDManager, PauseMenuUI, GameOverUI, WaveAnnouncerUI
└── Utilities/   SpriteGenerator, AudioGenerator
```

All scripts live under namespaces (`SpaceShooter.Core`, `SpaceShooter.Player`, etc.). After the
import completes, confirm the Console shows **no compile errors** before continuing.

> **Assembly note:** the scripts use `UnityEngine.InputSystem` guarded by `#if ENABLE_INPUT_SYSTEM`
> and `UnityEditor` guarded by `#if UNITY_EDITOR`, so they compile in both the Editor and in builds
> whether the new Input System is active or not.

---

## 4. Scene Setup

Create **two scenes**: `MainMenu` and `Game` (`Assets/Scenes/`). Add both to
*File → Build Settings → Scenes In Build* with `MainMenu` at index 0.

### Main Camera (both scenes)
- **Projection:** Orthographic
- **Size:** `5.5` (matches `GameConstants.ORTHOGRAPHIC_SIZE`; playfield is ±9 x, ±5.5 y)
- **Background:** solid dark (e.g. `#050510`)
- **Position:** `(0, 0, -10)`
- Add the **CameraShake** component to the Main Camera (or an empty `Systems` object — it grabs
  `Camera.main` automatically).

### Persistent managers (create once, they use `DontDestroyOnLoad`)
Create an empty GameObject named **`_Managers`** and add these components:
- `GameManager`  — assign the **Player** prefab and (optionally) the `WaveManager` reference.
- `ScoreManager`
- `AudioManager` — leave the source arrays empty; it builds 8 SFX sources + 1 music source itself.
- `WaveManager` — assign the four enemy prefabs (Diver, Formation, Circler, Boss).
- `BulletPool` — assign the Player-Bullet and Enemy-Bullet prefabs.
- `PowerUpSpawner` — assign the PowerUp prefab.

### Environment (Game scene)
- Empty object **`Starfield`** + `StarField` component (spawns 200 stars automatically).
- Optional **`Parallax`** objects each with a `ParallaxLayer` component and two child
  `SpriteRenderer` "tiles" (use `SpriteGenerator.GenerateBackground(layer)` — see Section 6).

---

## 5. Prefab Creation Guide

Create these prefabs in `Assets/Prefabs/`. Every gameplay object uses a **2D trigger collider** and a
`SpriteRenderer`; sprites are assigned at runtime by the bootstrap (Section 6), so you may leave the
`SpriteRenderer.sprite` empty in the prefab.

### Player (`Player.prefab`)
- Tag **Player**, Layer **Player**.
- Components: `SpriteRenderer`, `BoxCollider2D` (Is Trigger ✔), `Rigidbody2D` (Body Type **Kinematic**),
  `PlayerController`, `PlayerHealth`, `PlayerShooter`, `PlayerPowerUp`.
- Optional child **`Muzzle`** empty at the ship's nose; drag it into `PlayerShooter → Muzzle`.

### Player Bullet (`PlayerBullet.prefab`)
- Layer **PlayerBullet**.
- Components: `SpriteRenderer`, `BoxCollider2D` (Is Trigger ✔), `Rigidbody2D` (Kinematic), `Bullet`.

### Enemy Bullet (`EnemyBullet.prefab`)
- Layer **EnemyBullet**.
- Same components as the player bullet (`Bullet` sets `isEnemyBullet` at spawn time).

### Enemies
Each enemy: Tag **Enemy** (boss uses **Boss**), Layer **Enemy**, `SpriteRenderer`,
`CircleCollider2D`/`BoxCollider2D` (Is Trigger ✔), `Rigidbody2D` (Kinematic), an `EnemyShooter`
component (added automatically if missing), plus its behaviour script:
- `EnemyDiver.prefab`  → `EnemyDiver`
- `EnemyFormation.prefab` → `EnemyFormation`
- `EnemyCircler.prefab` → `EnemyCircler`
- `Boss.prefab` → `BossEnemy` (larger collider)

### Power-Up (`PowerUp.prefab`)
- Tag **PowerUp**, Layer **PowerUp**.
- Components: `SpriteRenderer`, `CircleCollider2D` (Is Trigger ✔), `Rigidbody2D` (Kinematic), `PowerUp`.

### Background layer tile (optional)
- Empty parent with `ParallaxLayer`; two child `SpriteRenderer` objects assigned to *Tile A* / *Tile B*.

---

## 6. Sprite & Audio Assignment (Bootstrap)

All art and sound is produced by `SpriteGenerator` and `AudioGenerator` at runtime.
`AudioManager` already generates and registers every clip in its `Awake`, so **audio needs no manual
wiring**. For sprites, add this tiny bootstrap so prefabs receive their generated sprites the moment
they spawn. Create `Assets/Scripts/Core/GameBootstrap.cs`:

```csharp
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Core
{
    /// <summary>Assigns procedurally generated sprites to tagged objects at runtime.</summary>
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer playerRenderer;      // set on Player prefab instead if preferred
        [SerializeField] private SpriteRenderer[] backgroundTiles;   // optional parallax tiles

        private void Awake()
        {
            // Backgrounds
            for (int i = 0; backgroundTiles != null && i < backgroundTiles.Length; i++)
                if (backgroundTiles[i] != null)
                    backgroundTiles[i].sprite = SpriteGenerator.GenerateBackground(i);
        }
    }
}
```

The **cleanest** approach is to assign sprites in each prefab's own `Awake` via a one-line helper, or
simply set `SpriteRenderer.sprite` from the relevant generator call in a small script on the prefab,
e.g. on the player: `GetComponent<SpriteRenderer>().sprite = SpriteGenerator.GeneratePlayerShip();`.
Recommended mapping:

| Prefab | Generator call |
|--------|----------------|
| Player | `SpriteGenerator.GeneratePlayerShip()` |
| EnemyDiver | `SpriteGenerator.GenerateEnemyA()` |
| EnemyFormation | `SpriteGenerator.GenerateEnemyB()` |
| EnemyCircler | `SpriteGenerator.GenerateEnemyC()` |
| Boss | `SpriteGenerator.GenerateBoss()` |
| PlayerBullet | `SpriteGenerator.GenerateBullet(false)` |
| EnemyBullet | `SpriteGenerator.GenerateBullet(true)` |
| PowerUp | `SpriteGenerator.GeneratePowerUp(type)` (called automatically in `PowerUp.Configure`) |

> **Editor-time option:** If you prefer sprites visible in the Editor before Play, wrap the same calls
> in an `[InitializeOnLoad]` editor class (in an `Editor/` folder) that writes generated textures to
> `Assets/GeneratedArt/` as PNGs and assigns them. The runtime path above is simpler and needs no
> asset files.

---

## 7. Physics Layers & Collision Matrix

Create these layers under *Edit → Project Settings → Tags and Layers* (already set in the provided
`ProjectSettings/TagManager.asset`):

| Layer | Index |
|-------|-------|
| Default | 0 |
| Player | 6 |
| Enemy | 7 |
| PlayerBullet | 8 |
| EnemyBullet | 9 |
| PowerUp | 10 |

Then open *Edit → Project Settings → Physics 2D → Layer Collision Matrix* and enable **only** these
pairs (uncheck everything else involving these gameplay layers to avoid friendly fire):

- **Player ↔ EnemyBullet** ✔
- **Player ↔ Enemy** ✔ (ramming damage)
- **Player ↔ PowerUp** ✔
- **Enemy ↔ PlayerBullet** ✔

Disable: Player↔PlayerBullet, Enemy↔EnemyBullet, PlayerBullet↔EnemyBullet, PlayerBullet↔PowerUp,
EnemyBullet↔PowerUp, Enemy↔Enemy, etc.

> Even if the matrix is left at Unity's default (all-on), the game still behaves correctly because
> `Bullet.cs` verifies the target's **tag** before applying damage — so friendly fire deals no damage.
> Tightening the matrix is purely a performance/cleanliness optimization.

All colliders should be **triggers** and every moving object needs a `Rigidbody2D` (Body Type
**Kinematic**) so Unity's 2D trigger callbacks (`OnTriggerEnter2D`) fire.

---

## 8. Tags

Create these tags under *Edit → Project Settings → Tags and Layers* (also pre-set in
`ProjectSettings/TagManager.asset`):

- **Player** (built-in)
- **Enemy** (built-in)
- **Boss** (custom)
- **Bullet** (custom)
- **PowerUp** (custom)

Assign the matching tag to each prefab as noted in Section 5.

---

## 9. Scene Hierarchy Layout

### MainMenu scene
```
Main Camera            (Orthographic, size 5.5)  + CameraShake
_Managers              GameManager, ScoreManager, AudioManager
Starfield              StarField
Canvas (Screen Space - Overlay)
├── MainMenuPanel      MainMenuUI (root = this panel)
│   ├── TitleText      TextMeshProUGUI "VOID ASSAULT"
│   ├── HighScoreText  TextMeshProUGUI
│   ├── PlayButton     Button
│   └── QuitButton     Button
EventSystem            (auto-added with the Canvas)
```

### Game scene
```
Main Camera            (Orthographic, size 5.5)  + CameraShake
_Managers              GameManager, ScoreManager, AudioManager, WaveManager, BulletPool, PowerUpSpawner
Starfield              StarField
Parallax (optional)    ParallaxLayer x N
Canvas (Screen Space - Overlay)
├── HUD                HUDManager
│   ├── ScoreText      TextMeshProUGUI
│   ├── WaveText       TextMeshProUGUI
│   ├── HealthIcons    Image x3
│   ├── ShieldIcons    Image x3
│   └── PowerUpGroup   (Image icon + Image timer bar, Image Type = Filled)
├── WaveAnnouncer      WaveAnnouncerUI + CanvasGroup
│   └── AnnounceText   TextMeshProUGUI (large, centered)
├── PausePanel         PauseMenuUI (root) + dim Image
│   ├── ResumeButton / MenuButton / QuitButton
└── GameOverPanel      GameOverUI (root)
    ├── FinalScoreText / HighScoreText / NewHighScoreLabel
    └── RetryButton / MenuButton
```

Wire each UI script's `[SerializeField]` references in the Inspector (buttons, texts, image arrays,
panel roots). The HUD's health/shield `Image[]` arrays expect exactly 3 elements each.

> **Single-scene alternative:** You can keep everything in one scene and let `GameManager` toggle the
> panels via its `OnStateChanged` event (the UI scripts already show/hide themselves by game state).

---

## 10. Building the Windows Executable

1. *File → Build Settings…*
2. Platform: **Windows, Mac, Linux** → **Switch Platform** (if not already selected).
3. **Target Platform:** Windows · **Architecture:** **x86_64**.
4. Ensure **Scenes In Build** lists `MainMenu` (index 0) and `Game`.
5. Click **Build**, choose/create an output folder (e.g. `Build/Windows/`).
6. Unity produces **`SpaceShooterGame.exe`** (name follows *Player Settings → Product Name*; set it to
   `SpaceShooterGame` if you want that exact filename) plus a `*_Data` folder and
   `UnityPlayer.dll` — ship the whole folder together.
7. Double-click the `.exe` to play.

> For a smaller/faster build set *Player Settings → Other Settings → Scripting Backend* to **IL2CPP**
> and *Managed Stripping Level* to **Low**.

---

## 11. Controls

| Action | Keys |
|--------|------|
| Move | **WASD** or **Arrow Keys** |
| Shoot | **Space** or **Left Mouse Button** (hold to auto-fire) |
| Pause / Resume | **Escape** |
| Menu navigation | Mouse click |

Power-ups are picked up by flying into them. Effects: **Shield** (cyan, +shield), **Triple** (green),
**Spread-5** (yellow), **Speed** (orange), **Laser** (magenta, continuous beam), **Health** (red, +1 HP),
**Nuke** (white, clears the screen). Timed weapons last 8 s; speed boost lasts 6 s.

---

## 12. Troubleshooting

| Symptom | Fix |
|---------|-----|
| **Compile errors about `UnityEngine.InputSystem`** | Install the *Input System* package (Package Manager). The provided `manifest.json` already lists it; if prompted to enable the new backend, choose **Yes** (this sets *Active Input Handling* to *Both*, which the code expects). |
| **`TextMeshProUGUI` not found** | Install *TextMeshPro* and run *Window → TextMeshPro → Import TMP Essential Resources*. |
| **Nothing spawns / no enemies** | Confirm the four enemy prefabs are assigned on `WaveManager`, and the Player prefab is assigned on `GameManager`. Call `GameManager.StartGame()` (the Play button does this). |
| **Bullets pass through enemies** | Ensure all colliders are **triggers**, each moving object has a **Kinematic Rigidbody2D**, and the collision matrix allows Enemy↔PlayerBullet / Player↔EnemyBullet. |
| **No sound** | `AudioManager` generates clips on `Awake`; make sure a single `AudioManager` exists in the first-loaded scene and the system volume/AudioListener (on Main Camera) is present. |
| **Invisible ship/enemies** | Sprites are runtime-generated — verify the bootstrap/`Awake` sprite assignment from Section 6 is present, or that `SpriteRenderer.sprite` is set. |
| **Duplicate managers after returning to menu** | All managers use `DontDestroyOnLoad` + a singleton guard; keep them only in the first scene, not duplicated in every scene. |
| **Player can't move** | Movement is gated on `GameManager.State == Playing`. Make sure the game actually started (not stuck in MainMenu/Paused). |
| **URP pink materials** | URP is **optional**. If you added the URP package but see pink, either create a URP asset & pipeline (*Assets → Create → Rendering → URP Asset*) and assign it in *Graphics settings*, or remove `com.unity.render-pipelines.universal` from `manifest.json` to use the built-in renderer. The game uses the default sprite shader and works fine on the **built-in** 2D renderer. |

---

### Package Notes
`Packages/manifest.json` pins:
`com.unity.inputsystem 1.7.0`, `com.unity.textmeshpro 3.0.6`, `com.unity.ugui 1.0.0`,
`com.unity.2d.sprite 1.0.0`, and `com.unity.render-pipelines.universal 14.0.9` (**optional** — see the
URP troubleshooting row above).

Enjoy defending the void. 🚀
