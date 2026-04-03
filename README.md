# 🚀 Space Shooter - Unity Desktop Game

A complete wave-based space shooter game for Windows desktop, built with Unity and C#.
Defend the galaxy across 5 increasingly difficult waves of enemies!

---

## 📋 Table of Contents

- [Game Features](#-game-features)
- [Requirements](#-requirements)
- [Project Setup](#-project-setup-step-by-step)
- [Manual Scene Setup](#-manual-scene-setup-alternative)
- [Controls](#-controls)
- [Game Architecture](#-game-architecture)
- [Building for Windows](#-building-for-windows-executable)
- [Troubleshooting](#-troubleshooting)

---

## 🎮 Game Features

- **Player Ship**: Move in all directions, shoot bullets upward
- **3 Enemy Types**:
  - 🔴 **Basic** (Red) — Straight movement, moderate shooting, 2 HP
  - 🟡 **Fast** (Yellow) — Zigzag movement, fragile, 1 HP
  - 🟣 **Tank** (Purple) — Slow and tanky, frequent shooting, 5 HP
- **3 Power-Up Types**:
  - 💚 **Health** (Green) — Restores 1 HP
  - ⚡ **Rapid Fire** (Yellow) — Doubles fire rate for 8 seconds
  - 🛡️ **Shield** (Blue) — Absorbs one hit for 10 seconds
- **5 Waves** with increasing difficulty
- **Scoring System** with persistent high scores
- **Parallax scrolling** star-field background
- **UI System**: Main menu, in-game HUD, game over / victory screens

### Wave Composition

| Wave | Basic | Fast | Tank | Total |
|------|-------|------|------|-------|
| 1    | 4     | 0    | 0    | 4     |
| 2    | 5     | 2    | 0    | 7     |
| 3    | 4     | 3    | 1    | 8     |
| 4    | 3     | 4    | 2    | 9     |
| 5    | 4     | 4    | 3    | 11    |

---

## 📦 Requirements

- **Unity 2021.3 LTS** or newer (2022.x / 2023.x / 6000.x all work)
  - Download from: https://unity.com/download
- **Windows 10/11** (for build target)
- Unity modules needed:
  - **Windows Build Support (IL2CPP or Mono)** — install via Unity Hub

---

## 🛠️ Project Setup (Step-by-Step)

### Method 1: Using the Setup Wizard (Recommended)

1. **Open Unity Hub** → Click "Open" → Navigate to this project folder and select it
2. Unity will import the project (this takes a minute on first load)
3. **Configure Tags** (IMPORTANT — do this first):
   - Go to **Edit → Project Settings → Tags and Layers**
   - Under **Tags**, add these custom tags:
     - `PlayerBullet`
     - `EnemyBullet`
     - `Enemy`
     - `PowerUp`
   - The `Player` tag already exists by default in Unity
4. **Run the Setup Wizard**:
   - Go to **Tools → Space Shooter → Setup Complete Game** (top menu bar)
   - Click each button **in order**:
     1. **"Step 1: Create Materials"** — Creates colored materials for all objects
     2. **"Step 2: Create Prefabs"** — Creates Player, Enemy, Bullet, and PowerUp prefabs
     3. **"Step 3: Setup MainMenu Scene"** — Creates the main menu scene
     4. **"Step 4: Setup GamePlay Scene"** — Creates the gameplay scene with all objects
     5. **"Step 5: Configure Build Settings"** — Sets up scene build order
5. **Add AutoStartGame to GamePlay scene**:
   - Open `Assets/Scenes/GamePlay.unity`
   - Select the `GameManager` object in the Hierarchy
   - Click **Add Component** → search for `AutoStartGame` → add it
   - Also add the `PauseController` component
6. **Press Play** to test!

### Method 2: Open Existing Project

If the Wizard doesn't appear (happens if Unity can't compile Editor scripts on first load):

1. Open Unity Hub → Open → Select this folder
2. Wait for compilation to finish
3. Check the Console (Window → Console) for any errors
4. If no errors, proceed with the Wizard from step 4 above
5. If errors appear, see Troubleshooting section

---

## 🔧 Manual Scene Setup (Alternative)

If you prefer to set up scenes manually instead of using the Wizard:

### Scene 1: MainMenu

1. **File → New Scene** → Save as `Assets/Scenes/MainMenu.unity`
2. Set Camera:
   - Select **Main Camera** → Inspector
   - Set **Projection** to `Orthographic`
   - Set **Size** to `5`
   - Set **Background** to dark blue `(5, 5, 26)`
3. Create **Canvas** (right-click Hierarchy → UI → Canvas)
4. Add UI elements:
   - **Title Text**: "SPACE SHOOTER" (centered, large, cyan)
   - **Start Button**: Green background, "START GAME" text
   - **Quit Button**: Red background, "QUIT" text
   - **High Score Text**: Yellow, "HIGH SCORE: 0"
5. Add `MainMenuController.cs` to the Canvas
6. Wire up button and text references in the Inspector

### Scene 2: GamePlay

1. **File → New Scene** → Save as `Assets/Scenes/GamePlay.unity`
2. Set Camera (same as MainMenu)
3. Create **empty GameObjects**:
   - `GameManager` — add `GameManager.cs`, `AutoStartGame.cs`, `PauseController.cs`
   - `EnemySpawner` — add `EnemySpawner.cs`
   - `PlayerSpawnPoint` — position at `(0, -3.5, 0)`
   - `BackgroundScroller` — add `BackgroundScroller.cs` with 2 child quads
4. Create **HUD Canvas** — add `HUDController.cs`, wire up Text references
5. Create **GameOver Canvas** — add `GameOverController.cs`, wire up references

### Creating Prefabs Manually

For each prefab, create a GameObject in the scene, configure it, then drag it to `Assets/Prefabs/`:

#### Player Prefab
- Create Sprite (square/circle) → tint **Cyan**
- Scale: `(0.6, 0.8, 1)`
- Add components: `PlayerController`, `HealthSystem` (maxHealth=3), `CollisionHandler` (isPlayer=✓)
- Add `BoxCollider2D` (isTrigger=✓), `Rigidbody2D` (Kinematic)
- Create child `FirePoint` at local pos `(0, 0.6, 0)`
- Create child `ShieldVisual` (transparent blue circle, initially disabled)
- Tag: `Player`

#### Enemy Prefabs (BasicEnemy, FastEnemy, TankEnemy)
- Create Sprite → tint **Red/Yellow/Purple** respectively
- Add: `EnemyController`, `HealthSystem`, `CollisionHandler` (isPlayer=✗)
- Add `BoxCollider2D` (isTrigger=✓), `Rigidbody2D` (Kinematic)
- Set `enemyType` on EnemyController to match
- Assign `EnemyBullet` prefab and power-up prefabs
- Tag: `Enemy`

#### Bullet Prefabs (PlayerBullet, EnemyBullet)
- Small rectangle sprite → tint **Green/Orange**
- Scale: `(0.15, 0.3, 1)`
- Add: `BulletController`, `BoxCollider2D` (isTrigger=✓), `Rigidbody2D` (Kinematic)
- Tags set at runtime: `PlayerBullet` or `EnemyBullet`

#### Power-Up Prefabs (HealthPowerUp, RapidFirePowerUp, ShieldPowerUp)
- Small circle sprite → tint **Green/Yellow/Blue**
- Scale: `(0.4, 0.4, 1)`
- Add: `PowerUpController`, `CircleCollider2D` (isTrigger=✓), `Rigidbody2D` (Kinematic)
- Set `type` on PowerUpController to match
- Tag: `PowerUp`

---

## 🎮 Controls

| Input          | Action         |
|----------------|----------------|
| W / ↑          | Move Up        |
| S / ↓          | Move Down      |
| A / ←          | Move Left      |
| D / →          | Move Right     |
| Space          | Shoot (hold)   |
| Escape         | Pause/Unpause  |

---

## 🏗️ Game Architecture

```
Assets/
├── Editor/
│   └── GameSetupWizard.cs        # Editor tool to auto-create everything
├── Materials/                     # Colored materials for all game objects
├── Prefabs/                       # Player, Enemies, Bullets, PowerUps
├── Scenes/
│   ├── MainMenu.unity            # Start screen
│   └── GamePlay.unity            # Main game scene
└── Scripts/
    ├── Core/
    │   ├── GameManager.cs         # Central game state, scoring, waves
    │   ├── PlayerController.cs    # Player input, movement, shooting
    │   ├── BulletController.cs    # Bullet movement and lifetime
    │   ├── EnemyController.cs     # Enemy AI, movement patterns, shooting
    │   ├── EnemySpawner.cs        # Wave-based spawning system
    │   ├── PowerUpController.cs   # Power-up collection and effects
    │   ├── HealthSystem.cs        # Reusable HP management
    │   ├── CollisionHandler.cs    # Collision detection and damage
    │   └── BackgroundScroller.cs  # Parallax scrolling background
    ├── UI/
    │   ├── MainMenuController.cs  # Main menu buttons
    │   ├── HUDController.cs       # In-game score, health, wave display
    │   └── GameOverController.cs  # Game over / victory screen
    └── Utility/
        ├── AutoStartGame.cs       # Auto-starts game when scene loads
        └── PauseController.cs     # Escape key pause toggle
```

### Script Relationships

```
GameManager (Singleton)
├── Manages: GameState, Score, Waves, Player reference
├── Events: OnScoreChanged, OnWaveChanged, OnStateChanged, OnGameOver, OnVictory
│
├── EnemySpawner
│   └── Listens to: OnWaveChanged → spawns enemies per wave
│
├── PlayerController
│   ├── Has: HealthSystem, CollisionHandler
│   └── Manages: Movement, Shooting, Power-up state
│
├── EnemyController (on each enemy)
│   ├── Has: HealthSystem, CollisionHandler
│   ├── Types: Basic (straight), Fast (zigzag), Tank (slow+tanky)
│   └── On death: Awards score, may drop PowerUp
│
├── HUDController
│   └── Listens to: OnScoreChanged, OnWaveChanged, HealthSystem.OnHealthChanged
│
└── GameOverController
    └── Listens to: OnGameOver, OnVictory
```

---

## 📦 Building for Windows Executable

### Step-by-Step Build Instructions

1. **Ensure Build Settings are configured**:
   - Go to **File → Build Settings**
   - Verify both scenes are listed:
     - `Scenes/MainMenu` (index 0)
     - `Scenes/GamePlay` (index 1)
   - If missing, open each scene and click **"Add Open Scenes"**
   - **Platform**: Select `Windows, Mac, Linux` (PC icon)
   - **Target Platform**: `Windows`
   - **Architecture**: `x86_64` (recommended)
   - Click **"Switch Platform"** if not already selected

2. **Configure Player Settings** (click "Player Settings..." button):
   - **Product Name**: `Space Shooter`
   - **Company Name**: (your choice)
   - **Resolution and Presentation**:
     - Default Screen Width: `800`
     - Default Screen Height: `600`
     - Fullscreen Mode: `Windowed` (for testing) or `Fullscreen Window`
     - Resizable Window: ✓
   - **Other Settings**:
     - Scripting Backend: `Mono` (faster build) or `IL2CPP` (better performance)
     - API Compatibility Level: `.NET Standard 2.1`

3. **Build the game**:
   - Click **"Build"** (or **"Build and Run"** to test immediately)
   - Create a new folder for the build (e.g., `Build/Windows`)
   - Name the executable: `SpaceShooter.exe`
   - Wait for the build to complete

4. **Your build folder** will contain:
   ```
   Build/Windows/
   ├── SpaceShooter.exe              # The game executable
   ├── SpaceShooter_Data/            # Game data folder (REQUIRED)
   ├── MonoBleedingEdge/             # Mono runtime (if using Mono backend)
   └── UnityPlayer.dll               # Unity runtime
   ```

5. **To distribute**: Zip the entire build folder. All files are required!

### Quick Build via Script

You can also build from the command line (useful for CI/CD):

```bash
"C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Unity.exe" \
  -batchmode -nographics \
  -projectPath "path/to/space_shooter_game" \
  -buildWindows64Player "Build/SpaceShooter.exe" \
  -quit
```

---

## ❓ Troubleshooting

### "Tag not found" errors
- Go to **Edit → Project Settings → Tags and Layers**
- Add missing tags: `PlayerBullet`, `EnemyBullet`, `Enemy`, `PowerUp`

### Player doesn't shoot
- Check that the `PlayerBullet` prefab is assigned in PlayerController's `bulletPrefab` field
- Verify the `FirePoint` child exists on the Player prefab

### Enemies don't shoot
- Check that the `EnemyBullet` prefab is assigned in each enemy prefab's `bulletPrefab` field

### Power-ups don't spawn
- Ensure the power-up prefab array is assigned on each enemy prefab
- Check `powerUpDropChance` value (default 15%)

### Collisions not working
- Verify all objects have `Collider2D` components set to **Is Trigger = true**
- Verify all objects have `Rigidbody2D` set to **Kinematic**
- Check tags are correctly assigned

### Scene transitions don't work
- Ensure both scenes are added to Build Settings (File → Build Settings → Add Open Scenes)
- MainMenu must be at index 0, GamePlay at index 1

### No text visible in UI
- Unity may not find the default font. In each Text component, assign a font manually
- Use **Arial** or any system font

### Game doesn't start when entering Play mode
- Ensure `AutoStartGame` component is on the GameManager object in GamePlay scene

---

## 📝 License

This project is provided as-is for educational purposes. Feel free to modify and distribute.

---

**Have fun defending the galaxy! 🌌**
