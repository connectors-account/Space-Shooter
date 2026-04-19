# Setup Guide - Space Shooter (Unity 2022.3 LTS)

## 1) Create/Open Project
- Open this repository in Unity Hub.
- Use Unity **2022.3 LTS**.

## 2) Create Main Scene
- Create `Assets/Scenes/Main.unity`.
- Add an orthographic camera and black background.

## 3) Add Managers
Create empty GameObject `Managers`, then child objects:
- `InputHandler` + `InputHandler.cs`
- `GameManager` + `GameManager.cs`
- `ScoreSystem` + `ScoreSystem.cs`
- `PowerUpSystem` + `PowerUpSystem.cs`
- `SoundManager` + `AudioSource` + `SoundManager.cs`
- `EnemySpawner` + `EnemySpawner.cs`

Assign references in `GameManager`:
- Enemy Spawner
- Player Controller
- Score System

## 4) Create Player
- SpriteRenderer + ship sprite
- Rigidbody2D (Kinematic)
- Collider2D (trigger = false)
- Scripts:
  - `PlayerController`
  - `PlayerHealth`
  - `CollisionHandler` (role = Player)
  - `FactionAffiliation` (Faction = Player)
- Child object `FirePoint` slightly above ship center.

Assign in `PlayerController`:
- Bullet prefab
- Fire point
- PlayerHealth reference

## 5) Create Enemy Prefab
- SpriteRenderer + enemy sprite
- Rigidbody2D (Kinematic)
- Collider2D
- Scripts:
  - `EnemyAI`
  - `Damageable`
  - `CollisionHandler` (role = Enemy)
  - `FactionAffiliation` (Faction = Enemy)
- Child `FirePoint` below enemy center.
- Save as prefab and assign to `EnemySpawner.enemyPrefab`.

## 6) Create Bullet Prefabs
Player bullet prefab:
- Sprite + trigger collider
- `Bullet` script (damage 10, speed 14)

Enemy bullet prefab:
- Sprite + trigger collider
- `Bullet` script (damage 8, speed 9)

Assign bullet prefabs in:
- PlayerController (player bullet)
- EnemyAI (enemy bullet)

## 7) Create Power-up Prefabs
Create 3 prefabs with sprite + trigger collider + `PowerUpPickup`:
- Shield
- RapidFire
- HealthRestore

Assign them to `PowerUpSystem` fields.

## 8) UI Setup
Create Canvas and panels:
- Main Menu (Start, Quit)
- HUD (Health text, Score text, Wave text, optional status icons)
- Pause panel (Resume, Menu)
- Game Over panel (Final score, Restart, Menu)

Attach `UIManager` and wire button events:
- Start -> `UIManager.OnStartButtonPressed`
- Resume -> `UIManager.OnResumeButtonPressed`
- Restart -> `UIManager.OnRestartButtonPressed`
- Main Menu -> `UIManager.OnMainMenuButtonPressed`
- Quit -> `UIManager.OnQuitButtonPressed`

## 9) Parallax Background
- Add two background layers.
- Add `ParallaxScroller` to any object.
- Assign near/far layer transforms.

## 10) Build Windows Executable
- File -> Build Settings
- Platform: PC, Mac & Linux Standalone
- Target: Windows x86_64
- Add `Main.unity` scene
- Build
