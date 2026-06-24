# 🚀 Space Shooter — Unity C# Game (Windows Desktop)

A complete, arcade-style 2D space shooter built in Unity with C#. The project
includes a one-click scene builder so you can go from a fresh clone to a
playable, buildable game in under a minute — no manual scene wiring required.

---

## ✨ Features

- **Player ship** — smooth WASD/Arrow movement, screen-clamped, with a health bar.
- **Shooting** — Space to fire, fire-rate cooldown, 3 weapon levels (single → dual → triple spread).
- **4 enemy types** with distinct behaviours:
  - `Straight` — flies straight down.
  - `Zigzag` — sine-wave weaving.
  - `Chaser` — homes toward the player.
  - `Shooter` — slows down and fires aimed bullets.
- **Wave system** — difficulty ramps each wave (more enemies, tougher mix, faster spawns). Next wave starts when the current one is cleared.
- **Power-ups** — Health, Weapon Upgrade, Shield (dropped on enemy death).
- **Collision & damage** — bullet hits, ship-to-ship collisions, shields, invulnerability.
- **Scoring** — score per kill + persistent high score (PlayerPrefs).
- **Lives system** — 3 lives, respawn with brief shield, then game over.
- **Parallax scrolling starfield** background (two layers).
- **Full UI** — Main Menu, HUD (score/wave/lives/health), Pause menu, Game Over screen.
- **Audio manager** — fully wired SFX/music hooks (works silently until you add clips).
- **Procedural placeholder sprites** — geometric ship/enemy/bullet art generated automatically.

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/PlayerController.cs
│   │   ├── Enemies/EnemyController.cs
│   │   ├── Enemies/EnemySpawner.cs
│   │   ├── Bullets/BulletController.cs
│   │   ├── PowerUps/PowerUpController.cs
│   │   ├── Managers/GameManager.cs
│   │   ├── Managers/ScoreManager.cs
│   │   ├── Managers/AudioManager.cs
│   │   ├── UI/UIManager.cs
│   │   ├── Systems/HealthSystem.cs
│   │   ├── Environment/ParallaxBackground.cs
│   │   └── Environment/AutoDestroy.cs
│   ├── Editor/
│   │   ├── SceneBuilder.cs     ← one-click game + Windows build
│   │   └── SpriteFactory.cs    ← procedural sprite generator
│   ├── Prefabs/   (generated)
│   ├── Sprites/   (generated)
│   ├── Scenes/    (Game.unity generated)
│   ├── Audio/     (drop your sound files here — see Audio/README.md)
│   └── UI/
├── Packages/manifest.json
├── ProjectSettings/ProjectVersion.txt
└── README.md
```

---

## 🎮 Controls

| Action | Key |
|--------|-----|
| Move   | `W A S D` or Arrow keys |
| Shoot  | `Space` |
| Pause / Resume | `Esc` |

---

## 🛠️ Setup & Run (Quick Start)

> Requires **Unity 2022.3 LTS** (any 2022.3.x). Newer LTS versions also work —
> Unity will offer to upgrade the project on first open.

1. **Install Unity Hub** → https://unity.com/download
2. In Unity Hub, install **Unity 2022.3 LTS** with the **"Windows Build Support (IL2CPP)"** module checked.
3. **Open the project**: Unity Hub → *Add* → select the `space_shooter_game` folder → open it.
   - Unity imports packages (TextMeshPro, 2D, UGUI). If prompted to import *TMP Essentials*, click **Import TMP Essentials**.
4. **Build the game with one click**: in the top menu bar choose
   **`Space Shooter ▸ Build Complete Game`**.
   - This generates all sprites, prefabs, and a fully-wired `Assets/Scenes/Game.unity`, and adds it to Build Settings.
5. Press **▶ Play** to test in the editor.

That's it — a fully playable game.

---

## 🪟 Building the Windows Executable

### Option A — One-click (recommended)
1. Run **`Space Shooter ▸ Build Complete Game`** first (if you haven't).
2. Run **`Space Shooter ▸ Build Windows Executable`**.
3. Choose an output folder. Unity produces `SpaceShooter.exe` + a `*_Data` folder.
4. Double-click `SpaceShooter.exe` to play. (Ship the whole folder, not just the .exe.)

### Option B — Manual via Build Settings
1. `File ▸ Build Settings…`
2. Platform: **Windows, Mac, Linux** → Target Platform: **Windows**, Architecture: **x86_64**.
3. Click **Switch Platform** if it isn't already selected.
4. Ensure **Scenes/Game** is in *Scenes In Build* (the auto-builder adds it; otherwise click *Add Open Scenes*).
5. *(Optional)* `Player Settings…` → set Company/Product name, icon, and **Fullscreen Mode**.
6. Click **Build**, choose a folder, and wait. Run the produced `.exe`.

---

## 🔊 Adding Sound (optional)
See **`Assets/Audio/README.md`**. The game runs fine with no audio; drop clips
into `Assets/Audio` and assign them on the **Managers → Audio Manager** component.

---

## 🧩 How It Fits Together

- **GameManager** (singleton) drives state: `MainMenu → Playing → Paused → GameOver`, lives, respawn, restart.
- **EnemySpawner** runs wave coroutines; each spawned enemy reports its death so wave completion is detected.
- **HealthSystem** is a reusable component (player + enemies) raising events for UI/death/shield.
- **UIManager** subscribes to GameManager/ScoreManager/HealthSystem events and swaps panels per state.
- **AudioManager / ScoreManager** are decoupled singletons everyone calls into.

All scripts live under the `SpaceShooter` namespace and are fully implemented —
no placeholders or TODOs.

---

## 🔧 Tweaking the Game
Select a prefab in `Assets/Prefabs` (or the scene objects) and edit the exposed
Inspector fields — move speed, fire cooldown, enemy health, drop chance, wave
sizes (on **EnemySpawner**), starting lives (on **GameManager**), etc.
