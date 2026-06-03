# 🚀 Space Shooter — Unity 2D Game (Windows Desktop)

A complete, fully-functional 2D space shooter built in **Unity** with **C#**. Fly your ship, blast waves of enemies, dodge incoming fire, rack up a high score, and survive as long as you can. The project includes clean, heavily-commented scripts and a **one-click editor tool** that builds the entire playable scene for you — no tedious manual setup required.

---

## 🎮 Gameplay Overview

| Feature | Description |
|---|---|
| **Player movement** | WASD or Arrow keys |
| **Shooting** | Spacebar (hold to auto-fire) |
| **Enemies** | Spawn in escalating waves, move down, shoot back |
| **Health** | Player takes damage from enemy ships & bullets |
| **Scoring** | Earn points for every enemy destroyed |
| **UI** | Live score, health, wave counter, and a Game Over / Win screen |
| **Menus** | Main menu with Start and Quit |

---

## ✅ Requirements

- **Unity 2021.3 LTS** (recommended) — the project also works on **Unity 2020.3 LTS or newer**.
- **Unity Hub** (to manage editor versions).
- **Windows** with the **"Windows Build Support (IL2CPP / Mono)"** module installed for the editor (needed only to build the `.exe`).

> The project intentionally uses Unity's **legacy UI (`UnityEngine.UI`)** and **built-in 2D physics** so it works out-of-the-box with no extra packages.

---

## 📁 Project Structure

```
SpaceShooterGame/
├── Assets/
│   ├── Scripts/              # All gameplay C# scripts (10 files)
│   │   ├── GameManager.cs        # Game state, score & health
│   │   ├── PlayerController.cs    # Movement + shooting
│   │   ├── Bullet.cs              # Player & enemy bullet behavior
│   │   ├── Enemy.cs               # Enemy movement, shooting, health
│   │   ├── EnemySpawner.cs        # Wave-based spawning
│   │   ├── UIManager.cs           # HUD + game over screen
│   │   ├── MainMenu.cs            # Main menu Start/Quit
│   │   ├── Health.cs              # Reusable health component
│   │   ├── DestroyOffScreen.cs    # Cleanup off-screen objects
│   │   └── GameBoundary.cs        # Keeps player on screen
│   ├── Editor/               # Editor-only automation (NOT in final build)
│   │   ├── SceneBuilder.cs        # One-click full game builder
│   │   └── BuildScript.cs         # Windows .exe build helper
│   ├── Scenes/               # MainMenu.unity & Game.unity (auto-generated)
│   ├── Prefabs/              # Player/Enemy/Bullet prefabs (auto-generated)
│   └── Sprites/              # Procedurally-generated sprites (auto-generated)
├── ProjectSettings/          # Unity project settings
├── Packages/                 # Package manifest
└── README.md
```

---

## ⚡ Quick Start (the easy way — recommended)

This project ships with a custom editor tool that **builds the whole game for you**.

1. **Open the project**
   - Launch **Unity Hub** → **Add** → select the `SpaceShooterGame` folder.
   - Open it with Unity 2021.3 LTS (or newer). Let Unity import (first import takes a minute).

2. **Run the one-click builder**
   - In the top menu, click **`Tools → Space Shooter → Build Game (One Click)`**.
   - This automatically creates the sprites, tags, prefabs, both scenes (Main Menu + Game), wires up all the UI, and configures Build Settings for Windows.
   - A confirmation popup appears when it's done.

3. **Play**
   - Open **`Assets/Scenes/MainMenu.unity`** and press **Play ▶**.
   - Click **Start Game**, then fly with **WASD/Arrows** and shoot with **Space**.

That's it — a fully playable game in under a minute. 🎉

---

## 🛠️ Manual Setup (the learning way)

Prefer to wire everything by hand to understand how it works? Follow these steps after opening the project.

### 1. Create the tags
`Edit → Project Settings → Tags and Layers`, and add these **Tags**:
`Player`, `Enemy`, `PlayerBullet`, `EnemyBullet`.

### 2. Bullet prefabs (Player & Enemy)
1. `GameObject → 2D Object → Sprite` (or use any small sprite). Name it **PlayerBullet**.
2. Add components: **Rigidbody 2D** (Gravity Scale = 0, Body Type = Kinematic), **Box Collider 2D** (check **Is Trigger**), and the **Bullet** script.
3. Set its **Tag** to `PlayerBullet`. Add the **DestroyOffScreen** script.
4. Drag it into `Assets/Prefabs` to make a prefab, then delete it from the scene.
5. Repeat for **EnemyBullet** (Tag = `EnemyBullet`).

### 3. Enemy prefab
1. Create a sprite GameObject named **Enemy**.
2. Add **Rigidbody 2D** (Gravity = 0), **Box Collider 2D** (Is Trigger), **Enemy** script, **DestroyOffScreen** script.
3. Set **Tag** = `Enemy`.
4. Create an empty child **FirePoint** at roughly `(0, -0.6, 0)` and assign it to the Enemy script's *Fire Point*.
5. Assign the **EnemyBullet** prefab to the Enemy script's *Bullet Prefab*.
6. Save as a prefab in `Assets/Prefabs`.

### 4. Player
1. Create a sprite GameObject named **Player**, place it near the bottom, e.g. `(0, -4, 0)`.
2. Add **Rigidbody 2D** (Gravity = 0, **Freeze Rotation Z**), **Box Collider 2D** (Is Trigger), **PlayerController** script, **GameBoundary** script.
3. Set **Tag** = `Player`.
4. Create an empty child **FirePoint** at `(0, 0.6, 0)`; assign it to *Fire Point*.
5. Assign the **PlayerBullet** prefab to *Bullet Prefab*.

### 5. Managers & Spawner
- Create an empty GameObject **GameManager** → add the **GameManager** script.
- Create an empty GameObject **EnemySpawner** → add the **EnemySpawner** script → assign the **Enemy** prefab.

### 6. UI (Canvas)
1. `GameObject → UI → Canvas` (a Canvas + EventSystem are created).
2. Add three **Text** elements: Score (top-left), Health (top-left below score), Wave (top-right).
3. Add a **Panel** named **GameOverPanel** (full-screen, semi-transparent). Inside it add:
   - A **Text** for the result ("GAME OVER"), a **Text** for final score, and two **Buttons** (Restart, Main Menu).
4. Create an empty GameObject **UIManager** → add the **UIManager** script, and drag each UI element into its matching field in the Inspector.
5. On the Restart button's **OnClick**, call `UIManager.OnRestartButton`. On Main Menu, call `UIManager.OnMainMenuButton`. Disable the GameOverPanel by default.

### 7. Camera
- Set **Main Camera** to **Orthographic**, Size ≈ `6`, and a dark background color.

### 8. Main Menu scene
1. Create a new scene **MainMenu**.
2. Add a Canvas with a title **Text** and two **Buttons** (Start, Quit).
3. Create a **MainMenu** GameObject with the **MainMenu** script (set *Game Scene Name* = `Game`).
4. Wire Start → `MainMenu.StartGame`, Quit → `MainMenu.QuitGame`.

### 9. Build Settings
`File → Build Settings` → **Add Open Scenes** so that **MainMenu** is index 0 and **Game** is index 1.

---

## 🖥️ Building the Windows Executable

### Option A — Editor menu (quickest)
1. Run the one-click builder first (or set up manually) so scenes exist in Build Settings.
2. Click **`Tools → Space Shooter → Build Windows EXE`**.
3. The build is created at **`Builds/Windows/SpaceShooter.exe`**. Double-click to run.

### Option B — Standard Build Settings dialog
1. `File → Build Settings`.
2. Select **Windows, Mac, Linux** and set **Target Platform = Windows**, **Architecture = x86_64**.
3. Make sure both scenes are listed (MainMenu first).
4. Click **Build**, choose an output folder, and Unity produces `SpaceShooter.exe` plus a `_Data` folder.
5. **Ship the whole folder together** (the `.exe` needs its `_Data` folder).

### Option C — Command line (headless / CI)
```bat
"C:\Program Files\Unity\Hub\Editor\2021.3.30f1\Editor\Unity.exe" ^
    -quit -batchmode -projectPath "C:\path\to\SpaceShooterGame" ^
    -executeMethod BuildScript.BuildWindows ^
    -logFile build.log
```

---

## 🎯 Controls

| Action | Key |
|---|---|
| Move | **W A S D** or **Arrow Keys** |
| Shoot | **Spacebar** (hold for continuous fire) |
| Restart | **Restart** button on the Game Over screen |

---

## 🔧 Tweaking the Game (Inspector values)

All gameplay values are exposed in the Inspector — no code changes needed:

- **PlayerController**: `moveSpeed`, `fireCooldown`, `collisionDamage`.
- **Bullet**: `speed`, `damage`, `lifetime`.
- **Enemy**: `moveSpeed`, `maxHealth`, `scoreValue`, `fireInterval`.
- **EnemySpawner**: `baseEnemiesPerWave`, `enemiesAddedPerWave`, `spawnInterval`, `timeBetweenWaves`, `maxWaves` (set > 0 to enable a win condition by clearing all waves).
- **GameManager**: `maxPlayerHealth`, `scoreToWin` (set > 0 to win by reaching a score).

---

## 🧠 How It Works (architecture)

- **GameManager** (singleton) owns the game state, score, and player health, and decides win/lose.
- **UIManager** (singleton) receives updates from the GameManager and refreshes the HUD.
- **PlayerController** reads input, moves via `Rigidbody2D`, and spawns player bullets.
- **Enemy** drifts downward and periodically spawns enemy bullets; it has its own health and awards score on death.
- **Bullet** knows its owner (Player/Enemy) which determines direction and what it can hit.
- **EnemySpawner** runs a coroutine producing progressively harder waves.
- **DestroyOffScreen** and **GameBoundary** keep the scene clean and the player contained.
- **Health** is a reusable, event-driven component you can attach to any future destructible object.

---

## ❓ Troubleshooting

- **Buttons don't click:** ensure an **EventSystem** exists in the scene (the one-click builder adds it automatically).
- **Player passes through enemies without damage:** confirm colliders are **Is Trigger** and tags are set correctly.
- **Enemies don't spawn:** check that the **Enemy prefab** is assigned in the EnemySpawner.
- **Text not visible:** make sure the Canvas render mode is **Screen Space - Overlay** and text color isn't transparent.
- **Build has no scenes:** add both scenes under `File → Build Settings`.

---

## 📜 License

Free to use, modify, and learn from. Have fun and make it your own! 🌌
