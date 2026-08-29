# Setup Guide - Unity Space Shooter (Windows Desktop)

This guide walks you through opening the project, wiring the scene, and building a Windows executable.

## 1) Open the project in Unity

1. Open **Unity Hub**.
2. Click **Open**.
3. Select this folder: `Space-Shooter`.
4. Let Unity finish importing assets.

Recommended Unity version: **2021.3 LTS or newer** (2022.3 LTS also fine).

---

## 2) Ensure required tags exist

In Unity: **Edit -> Project Settings -> Tags and Layers**

Create tags if missing:

- `Player`
- `Enemy`
- `PlayerBullet`
- `EnemyBullet`

---

## 3) Create and save scene

1. **File -> New Scene** (2D scene).
2. Save as: `Assets/Scenes/GameScene.unity`.

---

## 4) Scene setup (GameObjects)

### A. Main Camera

- Keep as Orthographic (default).
- Position: `(0, 0, -10)`.
- Background: dark color.

### B. GameManager object

1. Create Empty GameObject named `GameManager`.
2. Add component: `GameManager` script (`Assets/Scripts/GameManager.cs`).

### C. UIManager + Canvas

1. Create `Canvas` (GameObject -> UI -> Canvas).
2. Create Empty object named `UIManager` (can be child of Canvas).
3. Add component: `UIManager` script.
4. Create UI elements and assign references in UIManager:
   - `ScoreText` (Text)
   - `HealthText` (Text)
   - `GameOverPanel` (Panel, initially disabled)
   - `GameOverScoreText` (Text inside game over panel)
   - `HighScoreText` (Text inside game over panel)
   - `PauseMenuPanel` (Panel, initially disabled)
5. Add a Text to game over panel with: `Press R to Restart`.

### D. Player

1. Create object named `Player`.
2. Tag: `Player`.
3. Add `SpriteRenderer` and assign `Assets/Sprites/player_ship.png`.
4. Add `BoxCollider2D` (check **Is Trigger**).
5. Add `Rigidbody2D` (Body Type: **Kinematic**).
6. Add `PlayerController` script.
7. Create child empty object `FirePoint` at `(0, 0.6, 0)`.
8. Assign `FirePoint` in PlayerController inspector.

### E. Bullet prefabs

#### Player bullet prefab

1. Create object `PlayerBullet`.
2. Tag: `PlayerBullet`.
3. Add SpriteRenderer with `Assets/Sprites/player_bullet.png`.
4. Add BoxCollider2D (**Is Trigger**).
5. Add Rigidbody2D (Kinematic).
6. Add `BulletController` script.
7. Drag object to `Assets/Prefabs/PlayerBullet.prefab`.
8. Delete it from scene.

#### Enemy bullet prefab

1. Duplicate PlayerBullet object setup.
2. Name `EnemyBullet`, Tag `EnemyBullet`.
3. Use `Assets/Sprites/enemy_bullet.png`.
4. Save as `Assets/Prefabs/EnemyBullet.prefab`.
5. Delete from scene.

### F. Enemy prefab

1. Create object `Enemy`.
2. Tag: `Enemy`.
3. Add SpriteRenderer with `Assets/Sprites/enemy_basic.png`.
4. Add BoxCollider2D (**Is Trigger**).
5. Add Rigidbody2D (Kinematic).
6. Add `EnemyController` script.
7. Assign EnemyController `Bullet Prefab` = `EnemyBullet.prefab` (optional; can disable shooting by unchecking `Can Shoot`).
8. Save to `Assets/Prefabs/Enemy.prefab`.
9. Delete from scene.

### G. Enemy Spawner

1. Create Empty object `EnemySpawner`.
2. Add `EnemySpawner` script.
3. In inspector, set `Enemy Prefabs` array size to `1`.
4. Assign `Enemy.prefab` into element `0`.

### H. Wire remaining references

- On **PlayerController**, assign `Bullet Prefab` = `PlayerBullet.prefab`.
- On **PlayerController**, assign `FirePoint`.
- Make sure a **UIManager** object exists with all required fields assigned.

Save scene.

---

## 5) Test in Play Mode

Press Play. Verify:

- Player moves with WASD/Arrows.
- Space shoots bullets.
- Enemies spawn from top.
- Player bullets destroy enemies and increase score.
- Enemy collision/bullets reduce player health.
- At 0 health, game over panel appears.
- Press `R` to restart.

---

## 6) Build Windows executable (.exe)

1. Open **File -> Build Settings**.
2. Click **Add Open Scenes** (ensure `GameScene` is listed).
3. Platform: **PC, Mac & Linux Standalone**.
4. Target Platform: **Windows**.
5. Architecture: **x86_64**.
6. Click **Player Settings** and set product name (e.g. `Space Shooter`).
7. Click **Build** and choose output folder (e.g. `Builds/Windows`).
8. Unity outputs:
   - `Space Shooter.exe`
   - `Space Shooter_Data/`
   - required Unity runtime files

Run the `.exe` from the output folder.
