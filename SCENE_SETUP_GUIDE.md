# 🔧 Manual Scene Setup Guide

This guide walks through setting up the game scenes entirely in the Unity Editor (without the auto-bootstrap script).

---

## Prerequisites

1. Open the project in Unity 2021.3+
2. Set up custom tags (Edit → Project Settings → Tags and Layers):
   - `Player`, `Enemy`, `PlayerBullet`, `EnemyBullet`, `PowerUp`

---

## Part 1: Create Prefabs

### 1.1 Generate Sprites First
- Go to **Tools → Generate Placeholder Sprites** (requires `SpriteGenerator.cs`)
- Or use your own sprite assets imported as **Sprite (2D and UI)**, Pixels Per Unit = 32

### 1.2 Player Bullet Prefab
1. **GameObject → 2D Object → Sprite** → name it `PlayerBullet`
2. Assign `BulletPlayer` sprite
3. Set Tag to `PlayerBullet`
4. Add Components:
   - `BoxCollider2D` → Is Trigger: ✅, Size: (0.15, 0.3)
   - `Rigidbody2D` → Gravity Scale: 0, Body Type: Kinematic
   - `BulletController` script
5. **Drag from Hierarchy into `Assets/Prefabs/`** to create prefab
6. Delete from scene

### 1.3 Enemy Bullet Prefab
1. Same as above but use `BulletEnemy` sprite
2. Tag: `EnemyBullet`
3. Save as `Assets/Prefabs/EnemyBullet.prefab`

### 1.4 Enemy Prefabs (create 4)

For each enemy type:

| Prefab Name | Sprite | Tag |
|-------------|--------|-----|
| `BasicEnemy` | `EnemyBasic` | `Enemy` |
| `FastEnemy` | `EnemyFast` | `Enemy` |
| `TankEnemy` | `EnemyTank` | `Enemy` |
| `ShooterEnemy` | `EnemyShooter` | `Enemy` |

For each:
1. **GameObject → 2D Object → Sprite**
2. Assign the correct sprite
3. Set Tag to `Enemy`
4. Add Components:
   - `BoxCollider2D` → Is Trigger: ✅, Size: (0.8, 0.8)
   - `Rigidbody2D` → Gravity Scale: 0, Body Type: Kinematic
   - `EnemyController` script
5. Drag to `Assets/Prefabs/`
6. Delete from scene

### 1.5 Power-Up Prefabs (create 3)

| Prefab Name | Sprite | Type (in Inspector) |
|-------------|--------|---------------------|
| `PowerUpWeapon` | `PowerUpWeapon` | Weapon Upgrade |
| `PowerUpShield` | `PowerUpShield` | Shield |
| `PowerUpHealth` | `PowerUpHealth` | Health |

For each:
1. **GameObject → 2D Object → Sprite**
2. Set Tag to `PowerUp`
3. Add Components:
   - `CircleCollider2D` → Is Trigger: ✅, Radius: 0.35
   - `Rigidbody2D` → Gravity Scale: 0, Body Type: Kinematic
   - `PowerUpController` script → set **Type** dropdown
4. Drag to `Assets/Prefabs/`
5. Delete from scene

### 1.6 Player Prefab
1. **GameObject → 2D Object → Sprite** → name `Player`
2. Assign `PlayerShip` sprite
3. Set Tag to `Player`
4. Position: (0, -3.5, 0)
5. Add Components:
   - `BoxCollider2D` → Is Trigger: ✅, Size: (0.8, 0.8)
   - `Rigidbody2D` → Gravity Scale: 0, Body Type: Kinematic
   - `PlayerController` script
6. Create child empty object named `FirePoint` at local position (0, 0.6, 0)
7. In PlayerController Inspector:
   - Drag `PlayerBullet` prefab → **Bullet Prefab**
   - Drag `FirePoint` → **Fire Point**
   - Drag the SpriteRenderer → **Sprite Renderer**
8. Optionally save as prefab in `Assets/Prefabs/`

---

## Part 2: Set Up GameScene

### 2.1 Camera Setup
1. Select **Main Camera**
2. Set Projection: **Orthographic**
3. Orthographic Size: **5.5**
4. Background Color: Dark blue/black `(0.01, 0.01, 0.06)`

### 2.2 Background
1. Create two large sprites using the `Background` image
2. Position them at Y=0 and Y=12 to tile
3. Set Sorting Order to -100
4. Add `ParallaxBackground` script to each:
   - Scroll Speed: 1.0 (and 1.2 for second)
   - Reset Y: -12
   - Start Y: 12

### 2.3 Stars (Optional)
Create 20-30 small sprites with the `Star` sprite:
- Random positions across the screen
- Random alpha (0.3–1.0)
- Random scale (0.5–1.5)
- Add `ParallaxBackground` with varying scroll speeds (0.5–3.0)

### 2.4 Player
- Place the Player in the scene at (0, -3.5, 0)
- Ensure all Inspector references are wired

### 2.5 Game Manager
1. **GameObject → Create Empty** → name `GameManager`
2. Add `GameManager` script
3. Wire references:
   - **Enemy Spawner**: (create next)
   - **UI Manager**: (create next)
   - **Player**: drag Player from hierarchy

### 2.6 Enemy Spawner
1. **GameObject → Create Empty** → name `EnemySpawner`
2. Add `EnemySpawner` script
3. Wire prefab references in Inspector:
   - **Basic Enemy Prefab** → `BasicEnemy` prefab
   - **Fast Enemy Prefab** → `FastEnemy` prefab
   - **Tank Enemy Prefab** → `TankEnemy` prefab
   - **Shooter Enemy Prefab** → `ShooterEnemy` prefab
   - **Enemy Bullet Prefab** → `EnemyBullet` prefab
   - **Power Up Prefabs** → array of 3 power-up prefabs

### 2.7 Audio Manager
1. **GameObject → Create Empty** → name `AudioManager`
2. Add `AudioManager` script
3. (Optional) Add AudioClips to Sound Effects array
4. (Optional) Assign Background Music clip

---

## Part 3: Set Up UI Canvas

### 3.1 Create Canvas
1. **GameObject → UI → Canvas**
2. Canvas: Render Mode = **Screen Space - Overlay**
3. Canvas Scaler: UI Scale Mode = **Scale With Screen Size**, Reference = 1920×1080
4. Add `UIManager` script to Canvas

### 3.2 HUD Panel
1. Create child **Panel** named `HUDPanel`
2. Set Image color to transparent (alpha = 0)
3. Add children:
   - **Text** `ScoreText`: anchored top-left, "Score: 0", size 28
   - **Text** `WaveText`: anchored top-right, "Wave: 0", size 28
   - **Text** `HealthText`: anchored bottom-left, "HP: 5/5", size 24
   - **Slider** `HealthSlider`: anchored bottom-left, min=0, max=5, value=5
   - **Text** `WaveAnnouncement`: centered, size 48, yellow, initially inactive

### 3.3 Pause Menu Panel
1. Create child **Panel** named `PauseMenuPanel`
2. Set Image color to semi-transparent black (0, 0, 0, 0.7)
3. Add `PauseMenuController` script
4. Add children:
   - **Text**: "PAUSED", size 48, centered at top
   - **Button** `ResumeBtn`: "Resume"
   - **Button** `RestartBtn`: "Restart"
   - **Button** `MenuBtn`: "Main Menu"
   - **Button** `QuitBtn`: "Quit"
5. Wire button references in PauseMenuController Inspector
6. **Set panel inactive** (uncheck checkbox at top of Inspector)

### 3.4 Game Over Panel
1. Create child **Panel** named `GameOverPanel`
2. Set Image color to semi-transparent black (0, 0, 0, 0.8)
3. Add `GameOverController` script
4. Add children:
   - **Text** `GameOverTitle`: "GAME OVER", size 56, red
   - **Text** `FinalScore`: "Final Score: 0", size 32
   - **Text** `HighScore`: "High Score: 0", size 28
   - **Button** `RestartBtn`: "Play Again"
   - **Button** `MenuBtn`: "Main Menu"
   - **Button** `QuitBtn`: "Quit"
5. Wire references in GameOverController Inspector
6. **Set panel inactive**

### 3.5 Wire UIManager References
Select the Canvas and in the UIManager Inspector, drag:
- `HUDPanel`, `ScoreText`, `WaveText`, `HealthText`, `HealthSlider`
- `PauseMenuPanel`
- `GameOverPanel`, `FinalScore`, `HighScore`
- `WaveAnnouncement`

### 3.6 Event System
If not auto-created: **GameObject → UI → Event System**

---

## Part 4: Create MainMenuScene

1. **File → New Scene** → save as `Assets/Scenes/MainMenuScene.unity`
2. Set camera background to dark blue
3. Create **Canvas** with:
   - **Text** `TitleText`: "SPACE SHOOTER", size 64, white, centered near top
   - **Text** `HighScoreText`: "High Score: 0", size 28, centered
   - **Button** `StartButton`: "START GAME", centered
   - **Button** `QuitButton`: "QUIT", centered below start
4. Add `MainMenuController` to Canvas
5. Wire references: TitleText, HighScoreText, StartButton, QuitButton

---

## Part 5: Build Settings

1. **File → Build Settings**
2. Add both scenes:
   - `MainMenuScene` at index 0
   - `GameScene` at index 1
3. Platform: **PC, Mac & Linux Standalone**
4. Target: **Windows**, Architecture: **x86_64**
5. Click **Build** and choose output folder

---

## Hierarchy Summary (GameScene)

```
GameScene
├── Main Camera
├── Background_0          [ParallaxBackground]
├── Background_1          [ParallaxBackground]
├── Star_0..Star_29       [ParallaxBackground]
├── Player                [PlayerController, SpriteRenderer, BoxCollider2D, Rigidbody2D]
│   └── FirePoint
├── GameManager           [GameManager]
├── EnemySpawner          [EnemySpawner]
├── AudioManager          [AudioManager]
├── GameCanvas            [Canvas, CanvasScaler, GraphicRaycaster, UIManager]
│   ├── HUDPanel
│   │   ├── ScoreText
│   │   ├── WaveText
│   │   ├── HealthText
│   │   ├── HealthSlider
│   │   └── WaveAnnouncement
│   ├── PauseMenuPanel    [PauseMenuController] (inactive)
│   │   ├── PauseTitle
│   │   ├── ResumeBtn
│   │   ├── RestartBtn
│   │   ├── MenuBtn
│   │   └── QuitBtn
│   └── GameOverPanel     [GameOverController] (inactive)
│       ├── GameOverTitle
│       ├── FinalScore
│       ├── HighScore
│       ├── RestartBtn
│       ├── MenuBtn
│       └── QuitBtn
└── EventSystem
```
