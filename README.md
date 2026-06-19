# 🚀 Space Shooter — Unity (Windows Desktop)

A complete, fully-implemented 2D space-shooter prototype written in C# for Unity.
Every script is production-ready — no placeholders, no TODOs.

---

## ✨ Features

| Category | Implementation |
|----------|----------------|
| **Player** | WASD / Arrow-key movement, screen clamping, Spacebar shooting |
| **Enemies** | Wave spawner, downward movement + optional sine sway, enemy shooting |
| **Bullets** | Shared `Bullet` script for player & enemy, faction-based collision |
| **Health** | Player health with damage, healing, and game over at 0 HP |
| **Score** | Points awarded per enemy killed, live HUD update |
| **Power-up** | Rapid Fire **and** Shield (drops from enemies) |
| **Background** | Endless vertical parallax scrolling (multi-layer) |
| **UI** | Score, health bar/text, wave & power-up banners, Game Over screen |
| **Menus** | Main Menu (Start / Quit) + restart / menu buttons on Game Over |

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs        # Score, health, game state (singleton)
│   │   ├── PlayerController.cs    # Movement, shooting, power-up states
│   │   ├── Bullet.cs             # Projectile for player & enemies
│   │   ├── EnemyController.cs     # Enemy movement, shooting, health, drops
│   │   ├── EnemySpawner.cs        # Escalating wave spawning
│   │   ├── PowerUp.cs            # Rapid-fire / shield pickup
│   │   ├── ParallaxBackground.cs  # Looping parallax scroll
│   │   ├── UIManager.cs          # HUD, banners, game-over panel (singleton)
│   │   └── MainMenu.cs           # Start / Quit buttons
│   ├── Editor/
│   │   └── BuildScript.cs        # One-click / CLI Windows build
│   ├── Prefabs/                  # (you create these in step 2)
│   ├── Scenes/                   # MainMenu.unity, Game.unity (step 1)
│   └── Sprites/                  # Your art (or use built-in squares)
├── Packages/manifest.json        # Package dependencies (uGUI, Physics2D…)
├── ProjectSettings/ProjectVersion.txt
└── README.md
```

**9 C# scripts total** (well under the 15-file limit).

---

## 🎮 Controls

| Action | Key |
|--------|-----|
| Move | `W A S D` or Arrow keys |
| Shoot | `Spacebar` |

---

## 🛠️ PART 1 — Set Up the Unity Project

1. **Install Unity Hub** and **Unity Editor `2022.3 LTS`** (any 2022.3.x works; the
   `ProjectVersion.txt` targets `2022.3.40f1`). During install, **enable
   "Windows Build Support (IL2CPP)"** under *Add Modules*.
2. **Open the project**: In Unity Hub → *Open* → select the
   `space_shooter_game` folder. Unity will import and generate the `Library/` folder
   (this takes a minute the first time).
3. **Configure the player input** (default Input Manager already supports
   `Horizontal`/`Vertical`, so no change needed).

### Create the two scenes
1. `File → New Scene → Basic 2D` → **Save As** `Assets/Scenes/MainMenu.unity`.
2. Repeat to create `Assets/Scenes/Game.unity`.
3. `File → Build Settings → Add Open Scenes`. Ensure the order is:
   - **Index 0:** `MainMenu`
   - **Index 1:** `Game`

### Camera setup (both scenes)
- Select **Main Camera** → set **Projection = Orthographic**, **Size = 5**.
- Set the background color to dark (e.g. near-black) if not using a sprite background.

---

## 🧩 PART 2 — Create Game Objects & Prefabs

> Tip: You can use simple **square/triangle sprites**. Create one via
> `Assets → Create → Sprite → Square` (or import your own PNGs into `Assets/Sprites/`).
> Every moving object needs a **Rigidbody2D (Body Type = Kinematic, Gravity = 0)**
> and a **Collider2D with "Is Trigger" = ON** so trigger collisions fire.

### A. Tags (set up once: `Edit → Project Settings → Tags and Layers`)
Add tags: **`Player`** and **`Enemy`** (the `Bullet`, `PlayerController`, and
`PowerUp` scripts compare against these).

### B. Player Ship (in the `Game` scene)
1. Create a sprite GameObject, name it **Player**, set **Tag = Player**.
2. Add **Rigidbody2D** (Kinematic, Gravity Scale 0) and **BoxCollider2D** (Is Trigger ✔).
3. Add the **`PlayerController`** script.
4. Create an empty child **FirePoint** at the ship's nose (top). Drag it into the
   `Fire Point` field.
5. *(Optional shield visual)* add a child circle sprite, disable it, drag it into
   `Shield Visual`.
6. Leave `Bullet Prefab` empty for now — you'll assign it after step C.

### C. Bullet Prefab
1. Create a small sprite, name **Bullet**.
2. Add **Rigidbody2D** (Kinematic) + **BoxCollider2D** (Is Trigger ✔).
3. Add the **`Bullet`** script (leave defaults; it's configured at runtime).
4. Drag it into `Assets/Prefabs/` to make a prefab, then delete it from the scene.
5. Select **Player** → assign this Bullet prefab to its `Bullet Prefab` field.

### D. Enemy Prefab
1. Create a sprite, name **Enemy**, **Tag = Enemy**.
2. Add **Rigidbody2D** (Kinematic) + **BoxCollider2D** (Is Trigger ✔).
3. Add the **`EnemyController`** script.
4. Assign the **Bullet prefab** to its `Bullet Prefab` field.
5. *(Optional)* set `Sway Amplitude` > 0 for weaving enemies.
6. Drag into `Assets/Prefabs/`, then delete from the scene.

### E. Power-up Prefab
1. Create a distinct sprite, name **PowerUp**.
2. Add **Rigidbody2D** (Kinematic) + **CircleCollider2D** (Is Trigger ✔).
3. Add the **`PowerUp`** script and choose `Type` = RapidFire or Shield.
4. Drag into `Assets/Prefabs/`, then delete from the scene.
5. Select the **Enemy prefab** → assign this PowerUp prefab to `Power Up Prefab`
   and set `Drop Chance` (e.g. 0.15).

### F. Enemy Spawner
1. Create an empty GameObject **EnemySpawner**.
2. Add the **`EnemySpawner`** script.
3. Expand `Enemy Prefabs`, set size ≥ 1, and drag in your Enemy prefab(s).

### G. Game Manager
1. Create an empty GameObject **GameManager**, add the **`GameManager`** script.
   (Set `Starting Health` = 100.)

### H. Parallax Background
1. Create an empty **Background** GameObject.
2. Add **two** stacked star/space sprites as children, one directly above the other
   (each exactly one screen tall, sharing the same X).
3. Add the **`ParallaxBackground`** script to **Background**; set `Tiles` size = 2
   and drag both child sprites in. (Add more layers with different `Scroll Speed`
   values for depth.)
4. Put the background sprites on a sorting layer **behind** gameplay
   (Sprite Renderer → Order in Layer = -10).

### I. HUD & Game-Over UI (in the `Game` scene)
1. `GameObject → UI → Canvas` (Render Mode = Screen Space - Overlay). An
   **EventSystem** is added automatically — keep it.
2. Add **Text** elements: `ScoreText` (top-left), `HealthText` (top-right),
   and a centered `BannerText` (for wave/power-up messages).
3. *(Optional)* add an **Image** (Image Type = Filled, Horizontal) as a health bar.
4. Create a **GameOverPanel** (a full-screen Image) with:
   - a `FinalScoreText` Text,
   - a **Restart** Button,
   - a **Main Menu** Button.
   Disable the panel by default (the script also hides it on Start).
5. Add the **`UIManager`** script to the **Canvas**. Drag each UI element into its
   matching field (`Score Text`, `Health Text`, `Health Bar Fill`, `Banner Text`,
   `Game Over Panel`, `Final Score Text`).
6. Wire the buttons:
   - **Restart** Button → `OnClick` → `UIManager.OnRestartButton`.
   - **Main Menu** Button → `OnClick` → `UIManager.OnMainMenuButton`.

### J. Main Menu (in the `MainMenu` scene)
1. Add a **Canvas** with a title Text and two **Buttons** (`Start`, `Quit`).
2. Create an empty **MainMenu** GameObject, add the **`MainMenu`** script
   (set `Gameplay Scene Name` = `Game`).
3. Wire buttons:
   - **Start** → `OnClick` → `MainMenu.OnStartButton`.
   - **Quit** → `OnClick` → `MainMenu.OnQuitButton`.

4. **Save both scenes** (`Ctrl+S`).

### Quick test
Press **Play** in the `Game` scene: enemies should spawn in waves, you can move and
shoot, score rises, health drops on hits, and the Game Over panel appears at 0 HP.

---

## 🏗️ PART 3 — Build the Windows `.exe`

### Option 1 — One-click menu (recommended)
The included `Assets/Editor/BuildScript.cs` adds a menu item:
1. In the Unity menu bar click **`Build → Build Windows (x64)`**.
2. The build is written to `space_shooter_game/Builds/Windows/SpaceShooter.exe`.
3. Run `SpaceShooter.exe` (the accompanying `SpaceShooter_Data/` folder must stay
   alongside the exe).

### Option 2 — Standard Build Settings dialog
1. `File → Build Settings`.
2. Confirm both scenes are listed (`MainMenu` index 0, `Game` index 1).
3. Select **Platform = Windows, Mac, Linux** → **Target Platform = Windows**,
   **Architecture = x86_64**. Click **Switch Platform** if needed.
4. Click **Build**, choose an output folder (e.g. `Builds/Windows`), name it
   `SpaceShooter`, and wait for the build to finish.

### Option 3 — Command line (CI / no editor UI)
```bash
"C:\Program Files\Unity\Hub\Editor\2022.3.40f1\Editor\Unity.exe" ^
  -quit -batchmode -nographics ^
  -projectPath "C:\path\to\space_shooter_game" ^
  -executeMethod BuildScript.BuildWindows ^
  -logFile build.log
```
The exe is produced at `Builds/Windows/SpaceShooter.exe`.

---

## ⚙️ Tuning Cheatsheet
- **Difficulty:** `EnemySpawner` → `Base Enemies Per Wave`, `Enemies Added Per Wave`,
  `Time Between Waves`.
- **Player feel:** `PlayerController` → `Move Speed`, `Fire Cooldown`, `Bullet Speed`.
- **Power-up length:** `PlayerController` → `Rapid Fire Duration`, `Shield Duration`.
- **Drop rate:** `EnemyController` → `Drop Chance`.

---

## 📝 Notes
- The project uses Unity's built-in **2D physics (trigger colliders)** — make sure
  every gameplay object has a `Rigidbody2D` + trigger `Collider2D`, or collisions
  won't register.
- Sprites are intentionally left to you — the gameplay works with plain Unity
  square sprites so you can ship a playable build with zero external art.
