# 🚀 Space Shooter Game

A complete 2D space-shooter game built with Unity and C#. Defend the galaxy by shooting down enemy ships, collecting power-ups, and surviving as long as you can!

![Genre](https://img.shields.io/badge/Genre-Space%20Shooter-blue)
![Engine](https://img.shields.io/badge/Engine-Unity%202021.3+-green)
![Platform](https://img.shields.io/badge/Platform-Windows-orange)

---

## 🎮 Game Features

- **Player spaceship** — moves left/right, shoots bullets upward
- **Enemy waves** — spawn from the top and move downward with increasing difficulty
- **Collision detection** — bullets destroy enemies, enemies damage the player
- **3 lives** — player health system with heart display
- **Scoring system** — earn 10 points per enemy destroyed
- **Power-ups**:
  - ⚡ **Rapid Fire** (yellow star) — dramatically increases fire rate for 5 seconds
  - 🛡️ **Shield** (cyan star) — absorbs one enemy hit for 5 seconds
- **Difficulty ramp** — enemies spawn faster and move quicker over time
- **Scrolling starfield** — parallax space background
- **Full UI** — Main menu, in-game HUD, Game Over screen

## 🎯 Controls

| Action        | Keys                    |
|---------------|-------------------------|
| Move Left     | `A` or `←` Arrow        |
| Move Right    | `D` or `→` Arrow        |
| Shoot         | `Spacebar`              |

---

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity          # Main menu scene
│   │   └── GamePlay.unity          # Core gameplay scene
│   ├── Scripts/
│   │   ├── GameManager.cs          # Game state, score, difficulty control
│   │   ├── PlayerController.cs     # Player movement, shooting, health, power-ups
│   │   ├── Bullet.cs               # Bullet movement and enemy collision
│   │   ├── Enemy.cs                # Enemy movement (straight + sway patterns)
│   │   ├── EnemySpawner.cs         # Spawns enemies with increasing difficulty
│   │   ├── PowerUp.cs              # Power-up behavior (Rapid Fire / Shield)
│   │   ├── PowerUpSpawner.cs       # Periodically spawns random power-ups
│   │   ├── HUDManager.cs           # In-game UI (score, health, game over)
│   │   ├── MainMenuController.cs   # Main menu button handlers
│   │   ├── StarfieldBackground.cs  # Scrolling starfield effect
│   │   ├── GamePlaySceneSetup.cs   # Creates all gameplay objects at runtime
│   │   └── MainMenuSceneSetup.cs   # Creates all menu UI at runtime
│   ├── Prefabs/                    # (Generated at runtime)
│   ├── Materials/                  # (Available for custom materials)
│   └── Resources/                  # (Available for runtime resources)
├── ProjectSettings/
│   ├── ProjectSettings.asset       # Player settings, resolution, etc.
│   ├── EditorBuildSettings.asset   # Scene build order
│   ├── InputManager.asset          # Input axis configuration
│   ├── TagManager.asset            # Tags and layers
│   ├── Physics2DSettings.asset     # 2D physics (zero gravity)
│   ├── QualitySettings.asset       # Graphics quality presets
│   ├── TimeManager.asset           # Fixed timestep settings
│   └── AudioManager.asset          # Audio configuration
├── Packages/
│   └── manifest.json               # Unity package dependencies
├── .gitignore
└── README.md                       # This file
```

---

## 🛠️ How to Open, Build, and Run

### Prerequisites
- **Unity Hub** installed — [Download Unity Hub](https://unity.com/download)
- **Unity 2021.3 LTS** (or newer) installed via Unity Hub
  - When installing, make sure to include the **Windows Build Support** module

### Step 1: Open the Project in Unity

1. **Download/clone** this project folder to your computer
2. Open **Unity Hub**
3. Click **"Open"** (or "Add" in older versions) → navigate to the `space_shooter_game` folder → select it
4. Unity Hub may prompt you to choose a Unity version — select **2021.3 LTS** or any **2022.x / 2023.x / 6000.x** version
5. Wait for Unity to import all assets (first time may take 1-3 minutes)

### Step 2: Configure the Scenes (Important — First Time Only)

Since the scene files reference scripts by class name, Unity needs to resolve them on first open:

1. In Unity, go to **File → Build Settings** (or press `Ctrl+Shift+B`)
2. Click **"Add Open Scenes"** or drag both scenes from `Assets/Scenes/` into the **Scenes In Build** list:
   - `Assets/Scenes/MainMenu.unity` — **must be index 0** (first in list)
   - `Assets/Scenes/GamePlay.unity` — **must be index 1**
3. Make sure **MainMenu** is at the top (drag to reorder if needed)
4. Close the Build Settings window

#### Attach the Setup Scripts (One-Time)

The scenes contain empty GameObjects that need the setup scripts attached:

**For MainMenu scene:**
1. Open `Assets/Scenes/MainMenu.unity` (double-click in Project window)
2. In the **Hierarchy**, select the `MenuSetup` GameObject
3. In the **Inspector**, click **"Add Component"** → search for **`MainMenuSceneSetup`** → add it
4. Save the scene (`Ctrl+S`)

**For GamePlay scene:**
1. Open `Assets/Scenes/GamePlay.unity` (double-click in Project window)
2. In the **Hierarchy**, select the `GameSetup` GameObject
3. In the **Inspector**, click **"Add Component"** → search for **`GamePlaySceneSetup`** → add it
4. Also make sure the **Tags** are set up:
   - Go to **Edit → Project Settings → Tags and Layers**
   - Under **Tags**, add a tag called `PowerUp` if it doesn't exist
   - Under **Tags**, ensure `Enemy` exists (usually built-in but verify)
5. Save the scene (`Ctrl+S`)

> **💡 Alternative Quick Setup:** If the setup scripts aren't auto-linked, you can delete the `MenuSetup`/`GameSetup` objects and create new empty GameObjects, then drag the scripts onto them.

### Step 3: Test in the Editor

1. With the **MainMenu** scene open, click the **▶ Play** button in Unity
2. You should see the main menu with a starfield background
3. Click **"START GAME"** to play
4. Use **WASD/Arrow keys** to move, **Spacebar** to shoot
5. Collect power-ups (colored stars) for buffs
6. Press **▶** again to stop

### Step 4: Build as Windows Executable (.exe)

1. Go to **File → Build Settings** (`Ctrl+Shift+B`)
2. Ensure **Platform** is set to **"PC, Mac & Linux Standalone"** (select it and click **"Switch Platform"** if not)
3. Under **Target Platform**, select **Windows**
4. Under **Architecture**, select **x86_64** (64-bit)
5. Verify both scenes are in the build list with MainMenu as index 0
6. Click **"Build"**
7. Choose a folder for the output (e.g., create a `Build` folder)
8. Wait for the build to complete (1-2 minutes)

### Step 5: Run the Game

1. Navigate to your build output folder
2. Double-click **`SpaceShooterGame.exe`** (or whatever you named it)
3. The game launches in a window — enjoy! 🎉

---

## 🎨 Visual Design

All graphics are generated **programmatically at runtime** — no external art assets needed:

| Object       | Shape           | Color           |
|--------------|-----------------|-----------------|
| Player       | Triangle (▲)    | Bright Green    |
| Bullets      | Small Rectangle | Yellow          |
| Enemies      | Diamond (◆)     | Red-Orange      |
| Rapid Fire   | Star (★)        | Gold-Yellow     |
| Shield       | Star (★)        | Cyan            |
| Shield Aura  | Circle          | Translucent Cyan|
| Stars (BG)   | Dots            | White (varying) |

---

## 🔧 Customization

All gameplay values are exposed as public fields on the scripts. In the Unity Inspector, you can easily tweak:

| Setting                | Script              | Default |
|------------------------|---------------------|---------|
| Player speed           | PlayerController    | 8       |
| Fire rate              | PlayerController    | 4/sec   |
| Rapid-fire rate        | PlayerController    | 10/sec  |
| Player lives           | PlayerController    | 3       |
| Bullet speed           | Bullet              | 12      |
| Score per kill         | Bullet              | 10      |
| Enemy base speed       | EnemySpawner        | 3       |
| Initial spawn interval | GameManager         | 2.0s    |
| Min spawn interval     | GameManager         | 0.4s    |
| Difficulty ramp rate   | GameManager         | 0.02    |
| Power-up interval      | PowerUpSpawner      | 8-15s   |
| Power-up duration      | PowerUp             | 5s      |

---

## 📝 Architecture Notes

- **Runtime Setup Pattern**: The `GamePlaySceneSetup` and `MainMenuSceneSetup` scripts create all GameObjects, sprites, prefabs, and UI programmatically in `Awake()`. This means the scenes are self-bootstrapping — minimal manual Editor configuration is needed.
- **Singleton Pattern**: `GameManager` and `HUDManager` use singletons for easy cross-script communication.
- **Zero Gravity**: Physics2D gravity is set to (0,0) since all movement is script-driven.
- **Prefab Templates**: Bullet, Enemy, and PowerUp "prefabs" are created as inactive GameObjects at runtime and used as templates for `Instantiate()`.

---

## 📄 License

This project is provided as-is for educational and personal use. Feel free to modify and distribute.
