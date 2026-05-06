# Unity Space Shooter (Windows Desktop)

This repository contains a complete 2D space-shooter setup for Unity with fully implemented C# scripts:

- `PlayerController.cs` (movement + shooting)
- `Bullet.cs` (bullet movement + collision)
- `Enemy.cs` (enemy behavior + damage + scoring)
- `EnemySpawner.cs` (timed enemy spawning)
- `GameManager.cs` (score + game over + restart)
- `HealthSystem.cs` (health handling)
- `UIManager.cs` (score/health/game-over UI)

---

## 1) Project Folder Structure

Create and use this structure inside your Unity project:

```text
SpaceShooterGame/
├── Assets/
│   ├── Prefabs/
│   ├── Scenes/
│   │   └── Main.unity
│   └── Scripts/
│       ├── Bullet.cs
│       ├── Enemy.cs
│       ├── EnemySpawner.cs
│       ├── GameManager.cs
│       ├── HealthSystem.cs
│       ├── PlayerController.cs
│       └── UIManager.cs
└── README.md
```

Scripts are already prepared in `Assets/Scripts` in this directory.

---

## 2) Create a New Unity Project

1. Open **Unity Hub**.
2. Click **New Project**.
3. Choose template: **2D (Core)**.
4. Project Name: `SpaceShooterGame`.
5. Location: pick your workspace location.
6. Click **Create Project**.

> Recommended Unity version: 2021 LTS+ (also works on newer versions).

---

## 3) Add Required Folders in Unity

In Unity Project window:

1. Right-click `Assets` → **Create > Folder** → `Scripts`
2. Right-click `Assets` → **Create > Folder** → `Prefabs`
3. Right-click `Assets` → **Create > Folder** → `Scenes`

Then copy/import these script files into `Assets/Scripts`:

- `PlayerController.cs`
- `Bullet.cs`
- `Enemy.cs`
- `EnemySpawner.cs`
- `GameManager.cs`
- `HealthSystem.cs`
- `UIManager.cs`

---

## 4) Scene Setup (Hierarchy)

Create a new scene and save it as: `Assets/Scenes/Main.unity`

Use this hierarchy:

```text
Main Camera
GameManager
EnemySpawner
Player
Canvas
└── HUDPanel
    ├── ScoreText
    └── HealthText
└── GameOverPanel
    └── FinalScoreText
BoundaryBottom
```

### 4.1 Camera Setup

Select **Main Camera**:
- Projection: **Orthographic**
- Size: **5** (adjust if needed)
- Position: `(0, 0, -10)`
- Tag: **MainCamera**
- Background: dark space-like color

### 4.2 GameManager Object

1. Create Empty GameObject: `GameManager`
2. Add component: `GameManager` script
3. Leave `Player Health System` empty initially (auto-detected by Player tag), or drag Player’s `HealthSystem` after Player is created.

### 4.3 EnemySpawner Object

1. Create Empty GameObject: `EnemySpawner`
2. Add component: `EnemySpawner` script
3. Position at `(0, 0, 0)` (position not critical; script uses camera viewport)
4. Set fields after Enemy prefab is created:
   - Enemy Prefab: drag `Enemy` prefab
   - Spawn Interval: `1.2`
   - Min X Viewport: `0.1`
   - Max X Viewport: `0.9`
   - Spawn Y Viewport: `1.1`

---

## 5) Create Player GameObject

1. Create `2D Object > Sprites > Triangle` (or custom ship sprite)
2. Rename to `Player`
3. Tag: **Player**
4. Position: `(0, -3.5, 0)`
5. Add components:
   - `Rigidbody2D`
     - Body Type: Dynamic
     - Gravity Scale: `0`
     - Collision Detection: Continuous
     - Freeze Rotation Z: enabled
   - `CircleCollider2D` or `BoxCollider2D` (Is Trigger: enabled)
   - `PlayerController`
   - `HealthSystem`

### 5.1 Player Fire Point

1. Right-click Player → **Create Empty** child
2. Rename child to `FirePoint`
3. Position `FirePoint` at `(0, 0.6, 0)`
4. Assign this child into PlayerController → `Fire Point`

---

## 6) Create Bullet Prefab

1. Create `2D Object > Sprites > Square`
2. Rename to `Bullet`
3. Scale to around `(0.15, 0.35, 1)`
4. Add `BoxCollider2D` (Is Trigger: enabled)
5. Add script: `Bullet`
6. (Optional) Add `Rigidbody2D` with Gravity Scale `0` and Body Type Kinematic (not required by current script)
7. Drag Bullet from Hierarchy into `Assets/Prefabs` to create prefab
8. Delete Bullet from scene hierarchy
9. Assign Bullet prefab to PlayerController → `Bullet Prefab`

---

## 7) Create Enemy Prefab

1. Create `2D Object > Sprites > Capsule` (or enemy sprite)
2. Rename to `Enemy`
3. Tag: **Enemy** (create tag if missing)
4. Add components:
   - `BoxCollider2D` (Is Trigger: enabled)
   - Script: `Enemy`
5. Drag Enemy into `Assets/Prefabs` to create prefab
6. Delete Enemy from scene hierarchy
7. Assign prefab to EnemySpawner → `Enemy Prefab`

---

## 8) Create Boundary Bottom (enemy missed line)

1. Create Empty GameObject: `BoundaryBottom`
2. Tag: **Boundary** (create tag if missing)
3. Add `BoxCollider2D`
   - Is Trigger: enabled
4. Set scale large across screen width (for example X=20, Y=1)
5. Position slightly below visible camera area (for example Y = `-5.8`)

This object detects enemies leaving the play area and applies player damage.

---

## 9) UI Setup

1. Create `UI > Canvas`
   - Render Mode: Screen Space - Overlay
2. Create `UI > Text` under Canvas named `ScoreText`
   - Anchor: Top Left
   - Position near top-left
   - Default text: `Score: 0`
3. Create `UI > Text` under Canvas named `HealthText`
   - Anchor: Top Right
   - Position near top-right
   - Default text: `Health: 5/5`
4. Create `UI > Panel` named `GameOverPanel`
   - Stretch full screen
   - Semi-transparent background
   - Set inactive initially
5. Create child `UI > Text` under `GameOverPanel` named `FinalScoreText`
   - Center aligned
   - Large font
   - Default text: `Game Over`
6. Create Empty GameObject `UIManager`
7. Add script `UIManager`
8. Assign references in UIManager:
   - Score Text → `ScoreText`
   - Health Text → `HealthText`
   - Game Over Panel → `GameOverPanel`
   - Final Score Text → `FinalScoreText`
   - Player Health → Player’s `HealthSystem` component (optional; auto-find via Player tag if omitted)

---

## 10) Physics and Collision Layer Setup

Use 2D trigger colliders for this project.

### 10.1 Layer recommendations

Create layers:
- `Player`
- `Enemy`
- `Projectile`
- `Boundary`

Assign objects to layers accordingly.

### 10.2 Collision Matrix

Open **Edit > Project Settings > Physics 2D** and ensure interactions:

- Projectile ↔ Enemy: **enabled**
- Enemy ↔ Player: **enabled**
- Enemy ↔ Boundary: **enabled**
- Projectile ↔ Boundary: **enabled**

You can disable unnecessary interactions to optimize (like Projectile ↔ Player).

---

## 11) Script Wiring Checklist

Before pressing Play, verify:

- Player has `PlayerController`, `HealthSystem`, Rigidbody2D, Trigger Collider
- PlayerController has `Bullet Prefab` and `Fire Point` assigned
- EnemySpawner has `Enemy Prefab` assigned
- GameManager exists in scene
- UIManager exists and references UI objects
- Main Camera tagged `MainCamera`
- BoundaryBottom tagged `Boundary`

---

## 12) Controls

- Move: **WASD** or **Arrow Keys**
- Shoot: **Spacebar**
- Restart after game over: **R**

---

## 13) Build Windows Executable

1. Open **File > Build Settings**
2. Click **Add Open Scenes** (ensure `Main.unity` is listed)
3. Platform: **PC, Mac & Linux Standalone**
4. Target Platform: **Windows**
5. Architecture: **x86_64**
6. Click **Switch Platform** (if needed)
7. Click **Player Settings** and set:
   - Product Name: `SpaceShooterGame`
   - Company Name: your choice
   - Resolution defaults as desired
8. Back in Build Settings, click **Build**
9. Choose output folder, e.g. `Builds/Windows/`
10. Unity generates:
    - `SpaceShooterGame.exe`
    - `SpaceShooterGame_Data/` folder

Run the `.exe` file to play.

---

## 14) Optional Improvements

- Add explosion VFX and sound effects
- Add enemy shooting patterns
- Add power-ups (rapid fire, shield, heal)
- Add wave progression and boss enemies
- Replace legacy `UI Text` with TextMeshPro

---

## 15) Troubleshooting

- **Player does not shoot:** Ensure Bullet Prefab is assigned on PlayerController.
- **No enemies spawn:** Ensure Enemy Prefab is assigned on EnemySpawner.
- **UI not updating:** Ensure UIManager references are assigned.
- **Instant game over:** Check Enemy contact damage and Boundary position.
- **Script compile errors:** Confirm scripts are in `Assets/Scripts` and class/file names match exactly.
