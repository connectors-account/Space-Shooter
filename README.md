# SPACE SHOOTER

A complete 2D top-down arcade space shooter built in **Unity** with **C#**. Ten escalating
waves, two enemy types, boss battles with multi-phase bullet patterns, power-ups, parallax
starfield, procedurally-generated art & audio (no external asset files required), and full
menu / pause / game-over flow.

Everything — sprites and sound effects included — is generated **procedurally at runtime**, so
the project runs with zero imported art or audio assets. A one-click editor script builds all
tags, layers, prefabs and scenes for you.

---

## 1. Prerequisites

- **Unity 2021.3 LTS** or **2022.3 LTS** (any patch release). Newer 6.x versions also work.
- A Unity install with the **Windows Build Support (IL2CPP/Mono)** module if you want to build
  a `.exe` (see `BUILD_INSTRUCTIONS.md`).
- No packages beyond Unity's built-in modules and uGUI (already referenced in
  `Packages/manifest.json`). The game uses the **legacy Input Manager**, so no Input System
  package is needed.

---

## 2. How to open the project

1. Launch **Unity Hub**.
2. Click **Add ▸ Add project from disk** and select this `SpaceShooter` folder.
3. If Unity Hub asks which editor version to use, pick an installed **2021.3+** editor.
4. Open the project. On first import Unity will generate its `Library/`, `ProjectSettings/`
   and IDE project files automatically (these are intentionally not committed).
5. Wait for the initial script compilation to finish (bottom-right spinner).

---

## 3. Run the Editor setup script (IMPORTANT — do this first)

The gameplay scenes and prefabs are produced by an editor script so the repo stays lightweight
and art-free.

1. In the Unity menu bar, click **Space Shooter ▸ Setup Game**.
2. A progress bar runs while it:
   - creates the tags `Player, Enemy, PlayerBullet, EnemyBullet, PowerUp`,
   - creates matching user layers,
   - generates every prefab into `Assets/Prefabs/`,
   - builds `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/GameScene.unity`, fully wired,
   - adds both scenes to **Build Settings**,
   - sets product name and 1920×1080 as the default resolution.
3. When the "Setup complete!" dialog appears, open **`Assets/Scenes/MainMenu.unity`** and press
   **Play**.

> Re-running **Space Shooter ▸ Setup Game** is safe — it will recreate the scenes/prefabs.

If you ever want to test the gameplay scene directly, you can open `GameScene.unity` and press
Play; the `WaveManager` will start the run automatically.

---

## 4. How to build a Windows `.exe`

Short version (full step-by-step in `BUILD_INSTRUCTIONS.md`):

1. **File ▸ Build Settings…**
2. Ensure **MainMenu** (index 0) and **GameScene** (index 1) are listed under *Scenes In Build*.
   If not, click **Add Open Scenes** with each scene open, or re-run *Space Shooter ▸ Setup Game*.
3. Select **Windows, Mac, Linux** as the platform; set **Target Platform: Windows** and
   **Architecture: x86_64**. Click **Switch Platform** if needed.
4. Click **Build**, choose an output folder (e.g. `Builds/Windows`), and wait.
5. Run the generated **`Space Shooter.exe`**.

---

## 5. Controls

| Action              | Keys                                   |
|---------------------|----------------------------------------|
| Move                | **W A S D** or **Arrow keys**          |
| Fire                | **Space** (hold) or **Left Mouse**     |
| Pause / Resume      | **Esc**                                |
| Menu navigation     | Mouse click                            |

---

## 6. Game features

- **10 waves** of increasing difficulty, defined inline in `WaveManager`:
  1. 5× Type A · 2. 8× Type A · 3. Type A + Type B mix · 4. 10× Type B · 5. **Boss**
  · 6–9. escalating mixes · 10. **Final Boss** (tougher).
- **Enemy Type A** – descends straight, fires a single downward shot every 2s (HP 30).
- **Enemy Type B** – sine-wave zigzag, fires aimed shots at the player every 1.5s (HP 50).
- **Boss** – enters, patrols side-to-side, radial 8-bullet bursts in phase 1, and a rotating
  spiral + aimed shots below 50% HP (HP 500 / 900 final). Has its own world-space health bar
  plus a HUD boss bar.
- **Power-ups** (20% enemy drop chance): **Shield** (absorbs one hit), **Rapid Fire**
  (2× fire rate, 10s), **Triple Shot** (3-way fire, 10s).
- **Object-pooled bullets** for smooth performance, with reusable pattern helpers
  (single / spread / circle / spiral / aimed).
- **Scoring** with a combo multiplier for rapid kills, plus a persisted high score
  (`PlayerPrefs`).
- **Health + 3 lives** with 2-second invincibility frames and a flashing respawn effect.
- **Three-layer parallax starfield** generated on procedural textures.
- **Procedural audio** – all SFX (shoot, explosion, power-up, hit, boss shoot, wave complete,
  game over, menu click) are synthesized from sine/square/saw/noise waveforms at runtime. Drop
  real clips into `Assets/Resources/Audio/` (matching names) to override them.
- **Full UI flow** – animated Main Menu, in-game HUD, Pause menu with volume sliders, and a
  Game Over / Victory screen.

---

## 7. Project layout

```
Assets/Scripts/
  Core/        GameManager, AudioManager, SceneLoader
  Player/      PlayerController, PlayerHealth, PlayerShooter
  Enemy/       EnemyBase, EnemyTypeA, EnemyTypeB, BossEnemy, EnemySpawner, WaveManager
  Projectiles/ Bullet, BulletPattern, ObjectPool
  PowerUps/    PowerUpBase, ShieldPowerUp, RapidFirePowerUp, TripleShotPowerUp
  UI/          UIManager, HUDController, MainMenuController, PauseMenuController, GameOverController
  Background/  ParallaxBackground
  Utilities/   ScoreManager, ScreenBounds, SpriteGenerator
  Editor/      SpaceShooterSetup   (menu: Space Shooter ▸ Setup Game)
Assets/Resources/Audio/AudioClipPlaceholder   (optional custom-clip loader)
```

Prefabs (`Assets/Prefabs/`) and scenes (`Assets/Scenes/`) are produced by the setup script.
