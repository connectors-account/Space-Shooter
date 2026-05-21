# Space Shooter Unity — Complete Build Instructions

These are **step-by-step instructions** to set up this project in Unity, configure all
GameObjects / Prefabs, and build a **Windows x86_64 standalone executable**.

---

## Prerequisites

| Software | Version | Notes |
|----------|---------|-------|
| **Unity Hub** | Latest | https://unity.com/download |
| **Unity Editor** | 2021.3 LTS or newer (2022 LTS recommended) | Install via Unity Hub with **Windows Build Support (IL2CPP)** module |

> When installing Unity through Unity Hub, make sure to check **"Windows Build Support (IL2CPP)"**
> or at minimum **"Windows Build Support (Mono)"** in the modules list.

---

## PART 1 — Create the Unity Project

1. Open **Unity Hub** → click **New Project**.
2. Select the **2D (Core)** template.
3. Name the project `SpaceShooter` (or any name you prefer).
4. Choose a location on your PC and click **Create project**.
5. Wait for the editor to finish initializing.

---

## PART 2 — Import Scripts

1. In the Unity **Project** window, navigate to `Assets/`.
2. Create the following folder structure (right-click → Create → Folder):
   ```
   Assets/
   ├── Scripts/
   │   ├── Core/
   │   ├── UI/
   │   └── Utility/
   ├── Prefabs/
   ├── Scenes/
   ├── Materials/
   └── Sprites/
   ```
3. Copy/drag all `.cs` files from this repository into the matching folders:

   | File | Destination Folder |
   |------|--------------------|
   | `GameManager.cs` | `Assets/Scripts/Core/` |
   | `PlayerController.cs` | `Assets/Scripts/Core/` |
   | `EnemyController.cs` | `Assets/Scripts/Core/` |
   | `BulletController.cs` | `Assets/Scripts/Core/` |
   | `PowerUpController.cs` | `Assets/Scripts/Core/` |
   | `SpawnManager.cs` | `Assets/Scripts/Core/` |
   | `UIManager.cs` | `Assets/Scripts/UI/` |
   | `MainMenuController.cs` | `Assets/Scripts/UI/` |
   | `BackgroundScroller.cs` | `Assets/Scripts/Utility/` |
   | `GameStarter.cs` | `Assets/Scripts/Utility/` |
   | `DestroyOffScreen.cs` | `Assets/Scripts/Utility/` |

4. Wait for Unity to compile. Check the Console for zero errors.

---

## PART 3 — Set Up Tags

Before creating any GameObjects, define the required tags:

1. Go to **Edit → Project Settings → Tags and Layers**.
2. Under **Tags**, add these custom tags (if they don't already exist):
   - `Player`
   - `Enemy`
   - `PlayerBullet`
   - `EnemyBullet`
   - `PowerUp`

---

## PART 4 — Create the Game Scene

### 4A. Create the Scene File

1. **File → New Scene** (choose Basic 2D if prompted).
2. **File → Save Scene As…** → save in `Assets/Scenes/` as `Game.unity`.

### 4B. Camera Setup

1. Select **Main Camera** in the Hierarchy.
2. Set **Projection** = Orthographic.
3. Set **Size** = 5 (default for 2D).
4. Set **Background color** = dark blue/black (e.g. `#0A0A2E`).
5. Set **Position** = `(0, 0, -10)`.

### 4C. Create the Player

1. **Hierarchy → right-click → 2D Object → Sprites → Triangle** (or Square).
2. Rename it to `Player`.
3. Set **Tag** = `Player`.
4. Set **Position** = `(0, -3.5, 0)`.
5. Set **Scale** = `(0.5, 0.5, 1)`.
6. Set the **Sprite Renderer** color to **Cyan** `#00FFFF` or **Green**.
7. If you used a triangle: set **Rotation Z** = 0 so it points upward.
8. Add components:
   - **Rigidbody2D**: set **Body Type** = `Kinematic`.
   - **Box Collider2D** (or Polygon Collider2D): check **Is Trigger** = ✓.
   - **PlayerController** script: drag it onto the Player.

#### 4C-1. Create the Fire Point

1. **Right-click the Player** → Create Empty. Rename child to `FirePoint`.
2. Set **FirePoint** local position = `(0, 0.5, 0)` (just above the ship).
3. In the **PlayerController** component, drag `FirePoint` into the **Fire Point** field.

#### 4C-2. Create the Shield Visual

1. **Right-click the Player** → 2D Object → Sprites → Circle. Rename to `ShieldVisual`.
2. Set **local position** = `(0, 0, 0)`, **Scale** = `(1.5, 1.5, 1)`.
3. Set the **Sprite Renderer** color to **light blue, alpha ≈ 80** (`#4488FF` at ~50% opacity).
4. Set **Sorting Order** = 1 (so it renders above the player).
5. Deactivate the ShieldVisual (uncheck the checkbox at the top of its Inspector).
6. Drag `ShieldVisual` into the PlayerController's **Shield Visual** field.

### 4D. Create the Player Bullet Prefab

1. **Hierarchy → right-click → 2D Object → Sprites → Square**.
2. Rename to `PlayerBullet`.
3. Set **Tag** = `PlayerBullet`.
4. Set **Scale** = `(0.1, 0.3, 1)`.
5. Set **Sprite Renderer** color = **Yellow** `#FFFF00`.
6. Add components:
   - **Rigidbody2D**: Body Type = `Kinematic`.
   - **Box Collider2D**: Is Trigger = ✓.
   - **BulletController** script: `bulletSpeed` = 12, `direction` = `(0, 1, 0)`.
   - **DestroyOffScreen** script.
7. **Drag** the `PlayerBullet` from the Hierarchy **into** `Assets/Prefabs/` to create a prefab.
8. **Delete** the instance from the Hierarchy.
9. In the **PlayerController** on the Player object, drag the `PlayerBullet` prefab into the **Bullet Prefab** field.

### 4E. Create Enemy Prefabs

#### Basic Enemy

1. **Hierarchy → 2D Object → Sprites → Square**.
2. Rename to `BasicEnemy`.
3. Set **Tag** = `Enemy`.
4. Set **Scale** = `(0.5, 0.5, 1)`.
5. Set **Sprite Renderer** color = **Red** `#FF3333`.
6. Add components:
   - **Rigidbody2D**: Body Type = `Kinematic`.
   - **Box Collider2D**: Is Trigger = ✓.
   - **EnemyController** script: `movePattern` = `StraightDown`, `moveSpeed` = 3, `scoreValue` = 100.
7. Drag into `Assets/Prefabs/` to create the prefab, then delete from Hierarchy.

#### Zigzag Enemy

1. Duplicate the Basic Enemy prefab workflow (or duplicate the prefab and edit).
2. Rename to `ZigzagEnemy`.
3. Set **Sprite Renderer** color = **Orange** `#FF8800`.
4. On the **EnemyController**: `movePattern` = `Zigzag`, `moveSpeed` = 2.5, `zigzagAmplitude` = 4, `zigzagFrequency` = 3, `scoreValue` = 150.
5. Save as prefab in `Assets/Prefabs/`.

#### Shooter Enemy

1. Same workflow. Rename to `ShooterEnemy`.
2. Set **Sprite Renderer** color = **Purple** `#AA33FF`.
3. On the **EnemyController**: `movePattern` = `StraightDown`, `moveSpeed` = 2, `canShoot` = ✓, `shootInterval` = 2.5, `scoreValue` = 200.
4. Create an **Enemy Bullet prefab** (see 4F), then assign it to the Shooter's **Enemy Bullet Prefab** field.
5. Save as prefab in `Assets/Prefabs/`.

### 4F. Create the Enemy Bullet Prefab

1. **Hierarchy → 2D Object → Sprites → Square**.
2. Rename to `EnemyBullet`.
3. Set **Tag** = `EnemyBullet`.
4. Set **Scale** = `(0.1, 0.25, 1)`.
5. Set **Sprite Renderer** color = **Magenta** `#FF00FF`.
6. Add:
   - **Rigidbody2D**: Kinematic.
   - **Box Collider2D**: Trigger.
   - **BulletController**: `bulletSpeed` = 6, `direction` = `(0, -1, 0)`.
   - **DestroyOffScreen**.
7. Prefab-ize → delete from Hierarchy.
8. Go back to the **ShooterEnemy** prefab and assign this as **Enemy Bullet Prefab**.

### 4G. Create Power-Up Prefabs

#### Rapid Fire Power-Up

1. **Hierarchy → 2D Object → Sprites → Diamond** (or Circle).
2. Rename to `RapidFirePowerUp`.
3. Set **Tag** = `PowerUp`.
4. Set **Scale** = `(0.4, 0.4, 1)`.
5. Set **Sprite Renderer** color = **Yellow** `#FFD700`.
6. Add:
   - **Rigidbody2D**: Kinematic.
   - **Circle Collider2D** (or Box): Trigger.
   - **PowerUpController**: `type` = `RapidFire`, `driftSpeed` = 1.5.
7. Prefab → delete instance.

#### Shield Power-Up

1. Same workflow. Rename to `ShieldPowerUp`.
2. Set color = **Light Blue** `#00CCFF`.
3. **PowerUpController**: `type` = `Shield`, `driftSpeed` = 1.5.
4. Prefab → delete instance.

### 4H. Create Managers

#### GameManager

1. **Hierarchy → Create Empty** → rename to `GameManager`.
2. Add **GameManager** script.
3. Add **GameStarter** script.
4. Drag the **Player** object into the `player` field.

#### SpawnManager

1. **Hierarchy → Create Empty** → rename to `SpawnManager`.
2. Add **SpawnManager** script.
3. Assign prefab references:
   - **Basic Enemy Prefab** → `BasicEnemy` from `Assets/Prefabs/`.
   - **Zigzag Enemy Prefab** → `ZigzagEnemy`.
   - **Shooter Enemy Prefab** → `ShooterEnemy`.
   - **Rapid Fire Power Up Prefab** → `RapidFirePowerUp`.
   - **Shield Power Up Prefab** → `ShieldPowerUp`.

### 4I. Create the UI

1. **Hierarchy → right-click → UI → Canvas**. A Canvas and EventSystem are created.
2. Select the **Canvas** and set:
   - **Canvas Scaler** → UI Scale Mode = **Scale With Screen Size**.
   - Reference Resolution = **1920 × 1080**.
   - Match (Width/Height) = **0.5**.

#### Score Text
1. Right-click Canvas → UI → Text (Legacy). Rename to `ScoreText`.
2. Anchor to **Top-Left**. Position = `(150, -30)`. Width=300, Height=50.
3. Font Size = 28. Color = White. Text = `SCORE: 000000`.
4. Check **Best Fit** or use a fixed font size.

#### Wave Text
1. Duplicate ScoreText. Rename to `WaveText`.
2. Position to the right of score. Text = `WAVE: 1`.

#### Health Text
1. Duplicate. Rename to `HealthText`.
2. Anchor to **Top-Right**. Position = `(-150, -30)`. Alignment = Right.
3. Text = `HP: 3 / 3`.

#### Wave Banner
1. Right-click Canvas → UI → Panel. Rename to `WaveBannerPanel`.
2. Set anchors to center. Size = `(500, 100)`.
3. Set Image color = semi-transparent black `(0, 0, 0, 180)`.
4. Create a **child Text** named `WaveBannerText`. Center it. Font size = 48. Color = White.
5. Deactivate `WaveBannerPanel` (uncheck its checkbox).

#### Game Over Panel
1. Right-click Canvas → UI → Panel. Rename to `GameOverPanel`.
2. Full-screen anchors. Image color = `(0, 0, 0, 200)`.
3. Add child elements:
   - **Text** `GameOverTitle`: "GAME OVER", font size 60, centered near top.
   - **Text** `GameOverScoreText`: font size 36, below title.
   - **Text** `GameOverHighScoreText`: font size 28, below score.
   - **Button** `RestartButton`: text = "RESTART", positioned center-ish.
   - **Button** `MainMenuButton`: text = "MAIN MENU", below restart.
4. Deactivate `GameOverPanel`.

#### Wire up UIManager
1. Add the **UIManager** script to the **Canvas** object.
2. Drag all UI elements into the matching fields in UIManager:
   - `scoreText` → ScoreText
   - `waveText` → WaveText
   - `healthText` → HealthText
   - `waveBannerPanel` → WaveBannerPanel
   - `waveBannerText` → WaveBannerText
   - `gameOverPanel` → GameOverPanel
   - `gameOverScoreText` → GameOverScoreText
   - `gameOverHighScoreText` → GameOverHighScoreText
   - `restartButton` → RestartButton
   - `mainMenuButton` → MainMenuButton

### 4J. Optional: Scrolling Background

1. **Hierarchy → 2D Object → Sprites → Square**. Rename to `Background`.
2. Set **Scale** = `(20, 20, 1)`.
3. Set **Position** = `(0, 0, 1)` (behind everything).
4. Set **Sprite Renderer** color = `#0A0A2E` (dark space blue).
5. Set **Sorting Layer** = Default, **Order in Layer** = -10.
6. Optionally add the **BackgroundScroller** script if you have a tiling star texture.

---

## PART 5 — Create the Main Menu Scene

1. **File → New Scene** → save as `Assets/Scenes/MainMenu.unity`.

2. Set camera background to dark color.

3. **Create a Canvas** (same Canvas Scaler settings as the Game scene).

4. Add UI elements:
   - **Text** `TitleText`: "SPACE SHOOTER", font size 60, centered near top, color = Cyan.
   - **Text** `HighScoreText`: font size 24, below title.
   - **Text** `ControlsText`: font size 20, center of screen (multi-line).
   - **Button** `PlayButton`: text = "PLAY", large, centered.
   - **Button** `QuitButton`: text = "QUIT", below Play.

5. Add the **MainMenuController** script to the Canvas.

6. Drag UI elements into the Inspector fields:
   - `titleText` → TitleText
   - `highScoreText` → HighScoreText
   - `controlsText` → ControlsText
   - `playButton` → PlayButton
   - `quitButton` → QuitButton

---

## PART 6 — Configure Build Settings

1. **File → Build Settings**.
2. Click **Add Open Scenes** for both scenes (open each and add):
   - `Scenes/MainMenu` → **index 0** (first / starting scene).
   - `Scenes/Game` → **index 1**.
   - Drag to reorder if needed; MainMenu MUST be index 0.
3. Set **Platform** = **PC, Mac & Linux Standalone** (should be default).
4. Click **Player Settings** (bottom-left):
   - **Company Name** = your name.
   - **Product Name** = `Space Shooter`.
   - **Resolution and Presentation**:
     - Default Screen Width = `1920`.
     - Default Screen Height = `1080`.
     - Fullscreen Mode = `Windowed` (or `Fullscreen Window`).
   - **Other Settings**:
     - **Scripting Backend** = `Mono` (simpler) or `IL2CPP` (better performance).
     - **Target Architecture** = `x86_64`.
     - **Api Compatibility Level** = `.NET Standard 2.1` (or `.NET Framework`).

---

## PART 7 — Build the Windows Executable

1. **File → Build Settings**.
2. Ensure **Target Platform** = `Windows`.
3. Ensure **Architecture** = `x86_64`.
4. Click **Build** (or **Build And Run**).
5. Choose an output folder (e.g. `Build/`).
6. Unity will produce:
   ```
   Build/
   ├── Space Shooter.exe          ← the executable
   ├── Space Shooter_Data/        ← game data folder
   ├── UnityPlayer.dll
   └── MonoBleedingEdge/          (if using Mono backend)
   ```
7. **Run** `Space Shooter.exe` to play!

### Distribution

To share the game, zip the entire `Build/` folder. All files are required —
the `.exe` alone won't work without its companion data folder and DLLs.

---

## PART 8 — Quick Troubleshooting

| Issue | Fix |
|-------|-----|
| "Tag not found" error | Re-read Part 3 — add all 5 tags in Project Settings. |
| Bullets don't hit enemies | Ensure both have Collider2D with **Is Trigger** checked and one has a Rigidbody2D. |
| No enemies spawn | Check that all prefab references are assigned on SpawnManager. |
| Game doesn't start | Make sure **GameStarter** is on the same object as **GameManager**. |
| Scenes can't be loaded | Make sure both scenes are added to Build Settings (Part 6, step 2). |
| UI text invisible | Check Canvas Scaler settings and text color is not the same as the background. |
| Player goes off-screen | Adjust `horizontalBound` and `verticalBound` on PlayerController to match your camera size. |
| Shield doesn't appear | Make sure ShieldVisual is a child of Player and assigned in the Inspector. |

---

## Physics Layer Setup (Optional but Recommended)

To prevent friendly bullets from hitting the player and enemy bullets from hitting enemies:

1. **Edit → Project Settings → Physics 2D**.
2. Create layers: `Player`, `PlayerBullet`, `Enemy`, `EnemyBullet`, `PowerUp`.
3. Assign each prefab/object to its layer.
4. In the **Layer Collision Matrix**, uncheck:
   - `Player` ↔ `PlayerBullet` (no self-hit).
   - `Enemy` ↔ `EnemyBullet` (no self-hit).
   - `PlayerBullet` ↔ `EnemyBullet` (bullets don't collide).
   - `PowerUp` ↔ `Enemy` (power-ups only interact with player).
