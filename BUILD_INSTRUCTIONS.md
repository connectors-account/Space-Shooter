# Space Shooter — Build Instructions (Windows)

A complete Unity 2D space shooter. This document explains how to open the
project, finish the tiny bit of editor-side setup (assigning sprites), and
produce a standalone Windows `.exe`.

---

## 1. Requirements

| Item | Version / Notes |
|------|-----------------|
| Unity Editor | **2022.3 LTS** (project was authored on 2022.3.20f1). Any 2022.3.x patch release works. |
| Unity module | **Windows Build Support (IL2CPP / Mono)** — install via Unity Hub → *Add Modules*. |
| Packages | **Input System** (`com.unity.inputsystem` 1.7.0) and **TextMeshPro** (`com.unity.textmeshpro` 3.0.6). Both are already declared in `Packages/manifest.json` and restore automatically on first open. |
| OS to build on | Windows 10/11 recommended. (You can build a Windows player from macOS/Linux too, as long as the Windows module is installed.) |

---

## 2. Opening the Project

1. Open **Unity Hub → Projects → Add → Add project from disk**.
2. Select the `SpaceShooter` folder (the one containing `Assets/`, `Packages/`,
   and `ProjectSettings/`).
3. Open it with a **2022.3 LTS** editor version. First open takes a couple of
   minutes while Unity imports assets and compiles scripts.

### First-open prompts you may see

- **"Enable the new Input System backend?"** — The project ships with
  `activeInputHandler: 2` (**Both** the old and new backends enabled), so the
  UI EventSystem and the gameplay Input System both work. If Unity still asks
  to switch/enable the backend, click **Yes** and let the editor **restart**.
- **"Import TMP Essentials?"** — Open any scene, then go to
  **Window → TextMeshPro → Import TMP Essential Resources** and click
  **Import**. This is required for all on-screen text (score, waves, menus) to
  render.

---

## 3. One-Time Editor Setup — Assign Sprites

To keep the project self-contained, all game objects use **programmatic
colored shapes** (each prefab has a `SpriteRenderer` with a color already set,
plus a working `BoxCollider2D` trigger and `Rigidbody2D`). Collisions,
movement, and all gameplay run correctly **without** any sprite asset. If you
want visible art instead of the editor's default gizmo, assign a sprite:

### Quick way — a single square sprite for everything

1. In the **Project** window, right-click **Assets → Create → 2D → Sprites →
   Square**. Name it `Square`. (Alternatively import your own PNGs.)
2. Select a prefab in `Assets/Prefabs/` (e.g. `Player.prefab`).
3. In the **Inspector**, find the **Sprite Renderer** component and drag the
   `Square` sprite into its **Sprite** field. The prefab's **Color** is already
   set (see table below), so each entity keeps its intended color.
4. Repeat for the other prefabs, or select several prefabs at once and assign
   the same sprite to all of them.

### Prefab color reference

| Prefab | Color |
|--------|-------|
| `Player` | Cyan |
| `EnemyBasic` | Red |
| `EnemyFast` | Yellow |
| `EnemyTank` | Purple |
| `Boss` | Orange |
| `BulletPlayer` | White |
| `BulletEnemy` | Red-orange |
| `PowerUpShield` | Blue |
| `PowerUpRapidFire` | Orange |
| `PowerUpTripleShot` | Green |
| `PowerUpSpeedBoost` | Yellow |
| `Explosion` | Orange |

> **Note:** Sprites are purely cosmetic here. If you skip this step the game is
> still fully playable — objects appear as small colored squares/gizmos and all
> collision, scoring, waves, power-ups, and boss logic function normally.

---

## 4. Scenes & Build Settings

Two scenes are included and already registered in
`ProjectSettings/EditorBuildSettings.asset`:

1. `Assets/Scenes/MainMenu.unity` — build index **0**
2. `Assets/Scenes/GameScene.unity` — build index **1**

To confirm/adjust:

1. **File → Build Settings…**
2. Ensure **MainMenu** is first (index 0) and **GameScene** second (index 1).
   If the list is empty, drag both scenes from `Assets/Scenes/` into
   **Scenes In Build**.
3. Under **Platform**, select **Windows, Mac, Linux**. Set **Target Platform =
   Windows** and **Architecture = x86_64**.
4. Click **Switch Platform** if Windows is not already the active platform.

---

## 5. Building the Windows Executable

1. In **Build Settings**, click **Build** (or **Build And Run**).
2. Choose/create an output folder, e.g. `Build/Windows/`.
3. Unity produces:
   - `SpaceShooter.exe`
   - a `SpaceShooter_Data/` folder
   - `UnityPlayer.dll` and supporting files
4. **Ship the whole output folder together** — the `.exe` needs its
   `_Data` folder and DLLs beside it to run.

To run: double-click `SpaceShooter.exe`.

---

## 6. Controls

| Action | Keys |
|--------|------|
| Move | **WASD** or **Arrow Keys** |
| Fire | **Space** or **Left Mouse Button** |
| Pause / Resume | **Esc** |
| Menu navigation | Mouse click on buttons |

---

## 7. Gameplay Overview

- **Objective:** Survive 10 waves and defeat the bosses on waves **5** and
  **10** while maximizing your score.
- **Lives:** Start with 3. Losing all lives ends the game.
- **Score & High Score:** Score is earned per kill; the high score persists via
  `PlayerPrefs` (key `HighScore`) between sessions.
- **Enemies:** Basic (red), Fast (yellow), Tank (purple), plus Bosses (orange,
  500 HP, two attack phases).
- **Power-ups** dropped by enemies: **Shield**, **Rapid Fire**, **Triple
  Shot**, **Speed Boost** (timed effects).
- **Parallax background** scrolls across three layers for depth.

---

## 8. Project Structure

```
SpaceShooter/
├── Assets/
│   ├── Scenes/            MainMenu.unity, GameScene.unity
│   ├── Prefabs/           Player, enemies, boss, bullets, power-ups, explosion
│   ├── InputActions/      PlayerInputActions.inputactions
│   └── Scripts/
│       ├── Core/          GameManager, SceneLoader, AudioManager
│       ├── Player/        PlayerController, PlayerHealth, PlayerShooter,
│       │                  PlayerPowerUp, PlayerInputActions
│       ├── Enemy/         EnemyBase, StandardEnemy, EnemyMovement,
│       │                  EnemyShooter, EnemySpawner, WaveManager, BossEnemy
│       ├── Bullets/       Bullet, BulletPool, BulletPattern
│       ├── PowerUps/      PowerUp
│       ├── UI/            HUD, MainMenuUI, PauseMenuUI, GameOverUI,
│       │                  WaveAnnouncerUI
│       ├── Background/    ParallaxBackground
│       └── Utilities/     ObjectPool, ScreenBounds
├── Packages/              manifest.json (Input System, TextMeshPro, uGUI)
└── ProjectSettings/       Tags/Layers, Physics2D matrix, Input, version
```

---

## 9. Troubleshooting

| Symptom | Fix |
|---------|-----|
| Editor asks to enable the new Input System backend | Click **Yes** and allow the editor to **restart**. The project already targets **Both** backends, so nothing else is needed. |
| On-screen text is missing or shows as boxes | **Window → TextMeshPro → Import TMP Essential Resources**. |
| Compile errors about `InputSystem` or `TMPro` namespaces | Make sure the Input System and TextMeshPro packages finished importing (**Window → Package Manager**). They are declared in `Packages/manifest.json`. |
| "The referenced script on this Behaviour is missing" on a prefab/scene object | Let Unity finish its first import/compile pass, then reopen the scene. If it persists, re-add the component — the script GUIDs match the files under `Assets/Scripts/`. |
| Objects are invisible / only gizmos show | Sprites are intentionally not assigned. Follow **Section 3** to assign a square sprite, or play as-is (colored shapes). |
| Nothing happens when pressing Play | Open `Assets/Scenes/MainMenu.unity` first, then Play; use the menu's **Start** button, or open `GameScene.unity` directly — its `WaveManager` auto-starts the game. |
| Build button greyed out | Install the **Windows Build Support** module via Unity Hub and **Switch Platform** to Windows in Build Settings. |

---

Enjoy, and good hunting, pilot. 🚀
