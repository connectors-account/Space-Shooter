# 🚀 Space Shooter — Unity C# (Windows Desktop)

A complete 2D top-down space shooter built with Unity and C#. Fly your ship,
blast waves of enemies, grab power-ups, and chase a high score. This repository
contains **all gameplay code, fully commented**, plus step-by-step instructions
to assemble the scene, create prefabs, and build a Windows `.exe`.

> The C# scripts are 100% complete with no placeholders. Unity-specific binary
> assets (the scene file, prefabs, sprites) must be created inside the Unity
> Editor — that's normal for Unity projects and every step is documented below.

---

## ✨ Features

| System | Description |
|---|---|
| **Player movement** | WASD / Arrow keys, clamped to the screen |
| **Shooting** | Space to fire; rapid-fire power-up supported |
| **Enemy AI** | Move down, sine-wave weaving, periodic shooting |
| **Wave progression** | More & faster enemies each wave, scaling health |
| **Health system** | Shared component for player and enemies |
| **Scoring** | Per-kill score + persistent high score (PlayerPrefs) |
| **Bullet pooling** | Reused projectiles — no GC spikes under heavy fire |
| **Collision & damage** | Trigger-based 2D collisions |
| **Power-ups** | Health, Rapid Fire, Shield (timed) |
| **Parallax background** | Seamless infinite vertical scroll, layerable |
| **Game states** | Menu → Playing → Game Over, with full UI |

---

## 📁 Project Structure

```
SpaceShooterGame/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs        # Game state machine & flow
│   │   ├── ScoreManager.cs       # Current + persistent high score
│   │   ├── HealthSystem.cs       # Shared HP/damage/shield component
│   │   ├── PlayerController.cs    # Movement, shooting, power-up state
│   │   ├── Enemy.cs              # Enemy movement, shooting, drops
│   │   ├── EnemySpawner.cs       # Wave-based spawning & difficulty ramp
│   │   ├── Bullet.cs             # Projectile movement & damage
│   │   ├── BulletPool.cs         # Object pool for bullets
│   │   ├── PowerUp.cs            # Pickup logic (3 types)
│   │   ├── ParallaxBackground.cs # Infinite scrolling background
│   │   └── UIManager.cs          # Menus, HUD, game-over screen
│   ├── Scenes/                   # MainGame.unity (created in editor)
│   └── Prefabs/                  # player/enemy/bullet/powerup (created in editor)
├── ProjectSettings/
│   └── ProjectVersion.txt        # Target Unity version
├── .gitignore
└── README.md
```

---

## 🎮 Controls

| Action | Key |
|---|---|
| Move | **W A S D** or **Arrow Keys** |
| Shoot | **Space** (hold to keep firing) |
| Start game (from menu) | **Enter** or click **Play** |
| Restart (on game over) | **R** or click **Restart** |

---

## 🛠️ Setup — Step by Step

### Prerequisites
1. Install **[Unity Hub](https://unity.com/download)**.
2. In Unity Hub → **Installs**, install **Unity 2021.3 LTS** (or any 2021.3+ / 2022 LTS).
   - When installing, make sure **"Windows Build Support (IL2CPP)"** module is checked.

### Step 1: Open the project
1. Open **Unity Hub** → **Open** → **Add project from disk**.
2. Select the `SpaceShooterGame` folder.
3. Click the project to open it. Unity will import the scripts and generate its
   `Library/` folder (this can take a minute the first time).

### Step 2: Create placeholder sprites (optional but recommended)
You can use any sprites. The fastest way to get visuals:
1. Right-click in the **Project** window → **Create → 2D → Sprites → Square** (and **Triangle**/**Circle**).
2. Create one square for the **player**, one triangle for **enemies**, a thin
   rectangle for **bullets**, and small circles for **power-ups**. Color them in
   the SpriteRenderer (player = cyan, enemy = red, etc.).

### Step 3: Build the Scene
1. **File → New Scene** (Basic 2D). **File → Save As** → `Assets/Scenes/MainGame.unity`.
2. Select the **Main Camera**:
   - Projection = **Orthographic**, Size = **5**.
   - Background color = dark navy/black.
3. Create the manager objects (empty GameObjects via **GameObject → Create Empty**):
   - **GameManager** → add `GameManager` **and** `ScoreManager` components.
   - **BulletPool_Player** → add `BulletPool`; later assign the PlayerBullet prefab.
   - **BulletPool_Enemy** → add `BulletPool`; later assign the EnemyBullet prefab.
   - **EnemySpawner** → add `EnemySpawner`.
4. Create the **Player**:
   - **GameObject → 2D Object → Sprite** (or empty + SpriteRenderer). Name it `Player`.
   - Add components: `PlayerController`, `HealthSystem`, a **Collider2D** (check **Is Trigger**),
     and a **Rigidbody2D** (Body Type = **Kinematic**, Gravity Scale = 0).
   - Set its **Tag** to **Player** (Add Tag if it doesn't exist).
5. Create the **Background**:
   - Empty GameObject `Background` + `ParallaxBackground` component.
   - Add two child sprite tiles stacked vertically (one at y=0, one at y=10),
     assign both to the `layers` array, and set `tileHeight` to the sprite height.

### Step 4: Create the Prefabs
Drag a configured GameObject from the Hierarchy into `Assets/Prefabs/` to make a prefab.
See `Assets/Prefabs/PREFABS.md` for exact component lists. Create:
- **PlayerBullet** and **EnemyBullet** (SpriteRenderer + trigger Collider2D + `Bullet`).
- **Enemy** (SpriteRenderer + trigger Collider2D + Kinematic Rigidbody2D + `Enemy` + `HealthSystem`, **Tag = Enemy**).
- **PowerUp_Health**, **PowerUp_RapidFire**, **PowerUp_Shield** (each with `PowerUp` and matching `type`).

### Step 5: Wire up Inspector references
1. **BulletPool_Player** → `bulletPrefab` = PlayerBullet prefab.
2. **BulletPool_Enemy** → `bulletPrefab` = EnemyBullet prefab.
3. **Player → PlayerController** → `bulletPool` = BulletPool_Player; optionally set a `firePoint` child.
4. **EnemySpawner** → `enemyPrefab` = Enemy prefab, `enemyBulletPool` = BulletPool_Enemy,
   `powerUpPrefab` = one of the power-up prefabs.
5. **GameManager** → `player` = Player object, `enemySpawner` = EnemySpawner object.

### Step 6: Build the UI (Canvas)
1. **GameObject → UI → Canvas** (this also creates an EventSystem). Add the `UIManager` component to the Canvas.
2. Under the Canvas create three child panels (empty UI Images or empty objects):
   - **MenuPanel** — a Title text, a **Play** Button, a high-score text, optional **Quit** button.
   - **HUDPanel** — Score text, Wave text, a **Slider** for health, a centered Wave-banner text.
   - **GameOverPanel** — "Game Over" text, final score text, high score text, **Restart** + **Menu** buttons.
3. Assign all these UI elements to the matching fields on the `UIManager` component.
4. Hook the buttons' **OnClick** events:
   - Play → `UIManager.OnPlayButton`
   - Restart → `UIManager.OnRestartButton`
   - Menu → `UIManager.OnMenuButton`
   - Quit → `UIManager.OnQuitButton`

### Step 7: Play test
Press **Play** in the Unity Editor. You should see the menu; press **Enter** or
click **Play** to start. Move with WASD, shoot with Space.

---

## 🏗️ Build a Windows Executable (.exe)

1. **File → Build Settings**.
2. Under **Scenes In Build**, click **Add Open Scenes** so `MainGame` is listed and checked.
3. Select **Platform = Windows, Mac, Linux**. Set **Target Platform = Windows** and
   **Architecture = x86_64**. (If "Windows" isn't available, install **Windows Build Support**
   via Unity Hub → Installs → your version → Add Modules.)
4. Click **Switch Platform** if it isn't already on Windows.
5. (Optional) **Player Settings** → set the product name, company name, and icon.
6. Click **Build** (or **Build And Run**).
7. Choose an output folder (e.g. a new `Build/` folder).
8. Unity produces:
   - `SpaceShooterGame.exe` — double-click to run the game.
   - `SpaceShooterGame_Data/` and `UnityPlayer.dll` — **must stay next to the .exe**.
9. To share the game, zip the **entire** build folder (the `.exe` will not run on its own).

---

## 🧩 How the Code Fits Together

- **GameManager** is the brain: it holds the `Menu / Playing / GameOver` state and
  fires events when the state changes. It enables the player and spawner on start
  and freezes everything on death.
- **ScoreManager** and **HealthSystem** are singletons/components broadcasting
  change events so the **UIManager** updates the HUD without per-frame polling.
- **PlayerController** reads input, fires bullets from the **BulletPool**, and
  tracks timed power-up effects (rapid fire / shield).
- **EnemySpawner** runs a coroutine that spawns escalating waves of **Enemy**
  objects; each enemy uses **HealthSystem**, awards score on death, and can drop a **PowerUp**.
- **Bullet** + **BulletPool** implement efficient, reusable projectiles for both
  the player and enemies, differentiated by their target tag.

Every script is heavily commented — open any file in `Assets/Scripts/` to read
the inline explanations.

---

## ❓ Troubleshooting

- **Buttons don't respond:** make sure there's an **EventSystem** in the scene
  (auto-created with the Canvas) and the button OnClick events are assigned.
- **Bullets pass through enemies:** colliders must have **Is Trigger = true**, and
  the enemy must be tagged **Enemy** (player tagged **Player**). One object in each
  collision pair needs a Rigidbody2D.
- **Nothing spawns:** confirm `EnemySpawner.enemyPrefab` is assigned and the
  `GameManager.enemySpawner` reference points to it.
- **"Windows" build option missing:** add the Windows Build Support module in Unity Hub.

Enjoy, and happy shooting! 🛸
