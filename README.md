# 🚀 Space Shooter — Unity Game

A complete 2D space-shooter game built with Unity and C#. Defend the galaxy by dodging enemy fire, collecting power-ups, and racking up the highest score!

---

## 🎮 Game Features

| Feature | Details |
|---------|---------|
| **Player Ship** | Move with WASD / Arrow keys, shoot with Space bar |
| **Enemies** | Spawn in waves, move downward, shoot at the player |
| **Scoring** | Earn 100 points per enemy destroyed |
| **Health System** | Player starts with 5 HP; enemies deal 1 damage |
| **Power-Ups** | 🟢 **Health** (+2 HP) and 🟡 **Rapid Fire** (faster shooting for 5 seconds) |
| **Difficulty Scaling** | Enemy spawn rate increases over time |
| **UI** | Score display, health display, Game Over screen, Main Menu |

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Editor/
│   │   └── SceneSetupWizard.cs      ← ONE-CLICK scene & prefab generator
│   ├── Materials/
│   ├── Prefabs/                      ← Created automatically by wizard
│   ├── Resources/
│   ├── Scenes/
│   │   ├── MainMenu.unity            ← Created automatically by wizard
│   │   └── GameScene.unity            ← Created automatically by wizard
│   └── Scripts/
│       ├── PlayerController.cs        ← Player movement, shooting, health
│       ├── Bullet.cs                  ← Bullet movement and collision
│       ├── Enemy.cs                   ← Enemy AI, health, shooting, drops
│       ├── EnemySpawner.cs            ← Wave spawning with difficulty scaling
│       ├── PowerUp.cs                 ← Health and Rapid Fire power-ups
│       ├── GameManager.cs             ← Game state, score, scene management
│       ├── UIManager.cs               ← HUD, Game Over panel
│       ├── MainMenuUI.cs              ← Main menu buttons
│       ├── ScrollingBackground.cs     ← Optional parallax scrolling background
│       ├── CameraSetup.cs             ← Camera configuration
│       └── AutoDestroy.cs             ← Utility: auto-destroy after time
├── ProjectSettings/                   ← Unity project configuration
└── README.md                          ← This file
```

---

## 🛠️ Setup Instructions (Step-by-Step)

### Prerequisites
- **Unity Hub** installed ([Download](https://unity.com/download))
- **Unity Editor** version **2021.3 LTS or newer** (2022.x or 2023.x also work)
- Make sure you have the **Windows Build Support** module installed in Unity Hub

---

### Step 1: Open the Project in Unity

1. Open **Unity Hub**
2. Click **"Open"** (or "Add project from disk")
3. Navigate to the `space_shooter_game` folder and select it
4. Unity Hub will detect the project. Select your Unity version (2021.3+ recommended)
5. Click **"Open"** — Unity will import the project (this may take 1-2 minutes)

> **Note:** If Unity asks to upgrade the project or change settings, click **"Continue"** or **"Confirm"**.

---

### Step 2: Run the One-Click Setup Wizard ⭐

This is the easiest way to set up the entire game:

1. In Unity, go to the top menu bar
2. Click: **`Tools → Space Shooter → 1. Setup ENTIRE Game (One Click)`**
3. Wait for the wizard to complete (a dialog will confirm success)
4. The wizard automatically:
   - Creates all **prefabs** (Player Bullet, Enemy Bullet, Enemy, Health Power-Up, Rapid Fire Power-Up)
   - Creates the **MainMenu** scene with title, start button, and quit button
   - Creates the **GameScene** with player, spawner, UI, and all connections
   - Configures **Build Settings** with both scenes

That's it! The game is ready to play.

---

### Step 3: Play-Test the Game

1. Make sure the **MainMenu** scene is open (it should be after the wizard runs)
   - If not: go to `Assets/Scenes/` in the Project panel and double-click `MainMenu.unity`
2. Press the **Play** button (▶) at the top of the Unity Editor
3. Click **"START GAME"** in the main menu
4. **Controls:**
   - **WASD** or **Arrow keys** — Move the ship
   - **Space bar** — Shoot
5. Destroy enemies to earn points; collect green/yellow power-ups
6. When your HP reaches 0, the Game Over screen appears
7. Click **"RESTART"** or **"MAIN MENU"**

---

### Step 4: Build as a Windows Executable

1. Go to **File → Build Settings**
2. Verify the scene list shows:
   - ✅ `Scenes/MainMenu` (index 0)
   - ✅ `Scenes/GameScene` (index 1)
   - If missing, click **"Add Open Scenes"** for each scene
3. Set **Platform** to **"PC, Mac & Linux Standalone"** (should be default)
4. Set **Target Platform** to **Windows**
5. Set **Architecture** to **x86_64** (recommended)
6. Click **"Build"** (or **"Build and Run"**)
7. Choose a folder for the output (e.g., create a `Build/` folder)
8. Unity will compile and produce:
   - `Space Shooter.exe` — The game executable
   - `Space Shooter_Data/` — Required data folder
   - `UnityPlayer.dll` — Required runtime DLL

> **To distribute:** Zip the entire build output folder. Recipients just run the `.exe` file.

---

## 🎯 Manual Setup (Alternative to Wizard)

If you prefer to set up the game manually or the wizard doesn't work, follow these steps:

### A. Ensure Tags Exist
1. Go to **Edit → Project Settings → Tags and Layers**
2. Add these tags if they don't exist: `Player`, `Enemy`, `PowerUp`

### B. Create Prefabs

#### Player Bullet Prefab
1. Create an empty GameObject, name it `PlayerBullet`
2. Add **SpriteRenderer** → use a small white/cyan rectangle sprite
3. Add **BoxCollider2D** → check **Is Trigger** → size: (0.08, 0.2)
4. Add **Bullet.cs** script → set `Is Player Bullet = true`, `Damage = 1`
5. Drag it into `Assets/Prefabs/` to create a prefab, then delete from scene

#### Enemy Bullet Prefab
1. Same as above but name it `EnemyBullet`
2. Use a red rectangle sprite
3. Set `Is Player Bullet = false` in the Bullet script

#### Enemy Prefab
1. Create an empty GameObject, name it `Enemy`, tag it as `Enemy`
2. Add **SpriteRenderer** → use a red triangle/diamond sprite
3. Add **BoxCollider2D** → check **Is Trigger** → size: (0.6, 0.6)
4. Add **Enemy.cs** script
5. Assign the `EnemyBullet` prefab to the `Bullet Prefab` field
6. Set values: Move Speed=3, Max Health=2, Fire Rate=2, Score Value=100
7. Save as prefab in `Assets/Prefabs/`

#### Power-Up Prefabs
1. Create `HealthPowerUp`: green square sprite, CircleCollider2D (Is Trigger), PowerUp.cs (type=Health, healAmount=2)
2. Create `RapidFirePowerUp`: yellow square sprite, CircleCollider2D (Is Trigger), PowerUp.cs (type=RapidFire)
3. Save both as prefabs

### C. Create the Game Scene
1. **Main Camera**: Orthographic, Size=5, Background=dark blue (0.02, 0.02, 0.08)
2. **Player** (tag: Player): at position (0, -3.5, 0)
   - Add SpriteRenderer (cyan triangle), BoxCollider2D (trigger), PlayerController.cs
   - Create a child `FirePoint` at local position (0, 0.5, 0)
   - Assign `PlayerBullet` prefab and `FirePoint` transform in the inspector
3. **EnemySpawner** (empty object): Add EnemySpawner.cs, assign Enemy prefab
4. **GameManager** (empty object): Add GameManager.cs, assign power-up prefabs array
5. **Canvas** (Screen Space Overlay):
   - `ScoreText` (top-left): "SCORE: 0"
   - `HealthText` (top-right): "HP: 5 / 5"
   - `GameOverPanel` (full-screen overlay, starts disabled):
     - "GAME OVER" text, final score text, Restart button, Main Menu button
6. **UIManager** (empty object): Add UIManager.cs, wire up all UI references
7. **EventSystem**: Required for UI button clicks

### D. Create the Main Menu Scene
1. New scene with Canvas, Title text, Start Button, Quit Button
2. Add MainMenuUI.cs to an empty object, wire up the buttons

### E. Build Settings
1. File → Build Settings → Add both scenes (MainMenu first, GameScene second)

---

## 📝 Script Reference

| Script | Purpose | Attach To |
|--------|---------|-----------|
| `PlayerController.cs` | Player movement, shooting, health, power-up effects | Player GameObject |
| `Bullet.cs` | Bullet movement and collision handling | Bullet Prefabs |
| `Enemy.cs` | Enemy AI, health, shooting, power-up drops | Enemy Prefab |
| `EnemySpawner.cs` | Spawns enemies at random positions | Empty GameObject |
| `PowerUp.cs` | Drifting power-up with player pickup logic | Power-Up Prefabs |
| `GameManager.cs` | Singleton — score, game state, scene loading | Empty GameObject |
| `UIManager.cs` | Singleton — HUD text, Game Over panel | Empty GameObject |
| `MainMenuUI.cs` | Main menu button handlers | Empty GameObject (MainMenu scene) |
| `ScrollingBackground.cs` | Optional scrolling starfield background | Background Quad |
| `CameraSetup.cs` | Ensures camera is orthographic | Main Camera |
| `AutoDestroy.cs` | Destroys object after set time | Any temporary object |

---

## 🎨 Optional Enhancements

After the base game is working, consider adding:

- **Sound effects** — Add AudioSource components and play clips on shoot/hit/collect
- **Particle effects** — Add explosions when enemies die
- **Multiple enemy types** — Create variants with different speeds, health, and fire patterns
- **High score persistence** — Use `PlayerPrefs.SetInt("HighScore", score)` to save between sessions
- **Background stars** — Use the ScrollingBackground.cs script with a tiled star texture
- **Screen shake** — Add a brief camera shake on player damage

---

## ⚙️ Troubleshooting

| Issue | Solution |
|-------|----------|
| "Tag not found" errors | Go to Edit → Project Settings → Tags and Layers; add `Player`, `Enemy`, `PowerUp` |
| Bullets pass through enemies | Make sure both have Collider2D with **Is Trigger** checked, and at least one has a Rigidbody2D (set to Kinematic) |
| UI buttons don't work | Ensure there's an **EventSystem** in the scene |
| Scenes not loading | Check File → Build Settings — both scenes must be listed and enabled |
| Player can't move | Verify the Input axes exist in Edit → Project Settings → Input Manager |

---

## 📄 License

This project is provided as-is for educational purposes. Feel free to modify and distribute.

---

**Happy gaming! 🎮🚀**
