# Unity Space Shooter (Windows Desktop)

This project is a complete C# gameplay foundation for a 2D arcade space shooter in Unity.
All core game systems are implemented in code and ready to wire in the Unity Editor.

## Recommended Unity Version

- **Unity 2022.3 LTS** (recommended and tested API level for scripts)
- 2D Core template or empty Built-in Render Pipeline project

## Folder Structure

```text
space_shooter_game/
├── Assets/
│   ├── Animations/
│   ├── Materials/
│   ├── Prefabs/
│   ├── Resources/
│   │   ├── Audio/
│   │   └── Sprites/
│   ├── Scenes/
│   └── Scripts/
│       ├── Combat/
│       ├── Core/
│       ├── Data/
│       ├── Enemies/
│       ├── Environment/
│       ├── Player/
│       ├── PowerUps/
│       └── UI/
├── Packages/
└── ProjectSettings/
```

## Included Gameplay Scripts

### Core
- `GameState.cs` – game state enum
- `GameManager.cs` – state transitions, scene flow, pause/resume, restart, quit
- `ScoreManager.cs` – score tracking with events
- `WaveProgressionManager.cs` – wave loop, wave progression, wave-clear checks
- `BoundaryCleaner.cs` – destroys offscreen objects in boundary trigger

### Combat
- `Health.cs` – reusable health component with death events
- `Projectile.cs` – shared bullet system for player and enemies

### Player
- `PlayerController.cs` – WASD/Arrows movement + Space shooting input
- `PlayerWeapon.cs` – player fire rate and projectile spawning
- `PlayerPowerUpController.cs` – rapid fire, shield, health restore behavior

### Enemies
- `EnemyBase.cs` – common enemy movement/shoot/death/score/drop behavior
- `EnemyStraight.cs` – straight downward movement
- `EnemySine.cs` – sine wave movement
- `EnemyChaser.cs` – tracks player horizontally while descending
- `EnemySpawner.cs` – wave-based spawning

### Wave Data
- `WaveDefinition.cs` – serializable wave definitions and enemy entries

### Power-Ups
- `PowerUpType.cs` – power-up enum
- `PowerUpPickup.cs` – collectible logic
- `PowerUpDropper.cs` – weighted random drop system

### Environment
- `ParallaxScroller.cs` – infinite vertical parallax background

### UI
- `MainMenuUI.cs` – start/quit actions
- `HUDController.cs` – health, score, wave display
- `PauseMenuUI.cs` – resume/main-menu actions
- `GameOverUI.cs` – final score, restart/menu actions

---

## Step-by-Step Scene Setup

Create 2 scenes:
1. `Assets/Scenes/MainMenu.unity`
2. `Assets/Scenes/Gameplay.unity`

Add both scenes in **File → Build Settings → Scenes In Build** in this order:
1. MainMenu
2. Gameplay

### 1) Main Menu Scene Setup

1. Create empty object: `GameManagerBootstrap`
2. Add `GameManager` component
   - Main Menu Scene Name: `MainMenu`
   - Gameplay Scene Name: `Gameplay`
   - Initial State: `MainMenu`
3. Create UI Canvas + EventSystem
4. Add panel with title and 2 buttons: **Start**, **Quit**
5. Add `MainMenuUI` component to panel/controller object
6. Button bindings:
   - Start → `MainMenuUI.OnStartClicked()`
   - Quit → `MainMenuUI.OnQuitClicked()`

### 2) Gameplay Scene Setup

1. **Keep GameManager alive across scenes**
   - If testing Gameplay directly in editor, duplicate `GameManagerBootstrap` in this scene too.

2. **Player setup**
   - Create sprite object `Player`, set Tag = `Player`
   - Add components:
     - `Rigidbody2D` (Body Type: Kinematic, Gravity Scale: 0)
     - `BoxCollider2D` (Is Trigger: true)
     - `Health` (e.g. max 100)
     - `PlayerController`
     - `PlayerWeapon`
     - `PlayerPowerUpController`
   - Create child transform `FirePoint` at nose of ship, assign to `PlayerWeapon.firePoint`

3. **Projectiles**
   - Create prefab `PlayerProjectile`:
     - Sprite (small rectangle/circle)
     - `Collider2D` trigger
     - `Projectile` script
   - Create prefab `EnemyProjectile` similarly
   - Assign player projectile prefab to `PlayerWeapon`
   - Assign enemy projectile prefab to each enemy prefab

4. **Enemies (3 prefabs)**
   - Prefab A: `EnemyStraight` + `Health` + collider(trigger)
   - Prefab B: `EnemySine` + `Health` + collider(trigger)
   - Prefab C: `EnemyChaser` + `Health` + collider(trigger)
   - All enemy objects must use Tag = `Enemy`
   - Optional: add `PowerUpDropper` for drop chance

5. **Spawner + waves**
   - Create object `EnemySystems`
   - Add `EnemySpawner`
   - Add `WaveProgressionManager`
   - In `WaveProgressionManager.waves`, create multiple waves and populate enemy prefabs/counts/spawn intervals

6. **Boundaries**
   - Create large trigger collider object below playfield, tag it `Bounds`
   - Add `BoundaryCleaner` to it

7. **Background parallax**
   - Create 2 background sprites stacked vertically
   - Add `ParallaxScroller` to each, with different speeds (e.g. 0.5 and 1.2 for depth effect)

8. **Power-up prefabs**
   - Create 3 prefabs with `PowerUpPickup`:
     - RapidFire
     - Shield
     - HealthRestore
   - Assign corresponding `PowerUpType` on each
   - In `PowerUpDropper.dropTable`, assign entries and weights

9. **Gameplay UI**
   - Canvas with texts: Health, Score, Wave
   - Add `HUDController` and assign text references
   - Add pause panel with Resume + Main Menu buttons
   - Add `PauseMenuUI` and assign panel
   - Add game over panel with Final Score + Restart + Main Menu buttons
   - Add `GameOverUI` and assign references
   - Button bindings:
     - Pause Resume → `PauseMenuUI.OnResumeClicked()`
     - Pause Main Menu → `PauseMenuUI.OnMainMenuClicked()`
     - Game Over Restart → `GameOverUI.OnRestartClicked()`
     - Game Over Main Menu → `GameOverUI.OnMainMenuClicked()`

---

## Placeholder Asset Generation (Simple Colored Shapes)

You can create the whole game with primitive-looking sprites:

1. In any image editor (or Unity Sprite Editor textures), create:
   - Player: cyan triangle
   - Enemy A/B/C: red/orange/purple rectangles/diamonds
   - Bullets: thin white/yellow rectangles
   - Powerups: colored circles
     - Rapid fire: yellow
     - Shield: blue
     - Health restore: green
2. Export as PNG with transparent background.
3. Put files in `Assets/Resources/Sprites/`.
4. Set texture import type to **Sprite (2D and UI)**.

---

## Sound Effects Integration Approach

The scripts already expose `AudioSource` and `AudioClip` fields.

1. Add SFX files (`.wav` or `.mp3`) to `Assets/Resources/Audio/`.
2. Add an `AudioSource` to player/enemy objects.
3. Assign clips in inspector:
   - Player shoot/death on `PlayerWeapon` and `PlayerController`
   - Enemy shoot/death on `EnemyBase`
4. Leave clips unassigned if you want silent prototyping; gameplay still works.

---

## Controls

- Move: **WASD** or **Arrow Keys**
- Shoot: **Space**
- Pause/Resume: **Esc**

---

## Build Instructions (Windows .exe)

1. Open Unity Hub.
2. Click **Open** and select folder: `space_shooter_game`.
3. Let Unity import scripts.
4. Open **File → Build Settings**.
5. Platform: **PC, Mac & Linux Standalone**.
6. Target Platform: **Windows**.
7. Architecture: **x86_64**.
8. Ensure scenes added:
   - `MainMenu`
   - `Gameplay`
9. Click **Build**.
10. Choose output folder (e.g. `Builds/Windows/`).
11. Run generated `.exe`.

---

## Quality Checklist Before Shipping

- Confirm all prefab references are assigned (projectiles, fire points, wave enemy prefabs, UI text fields)
- Verify tags: `Player`, `Enemy`, `Bounds`
- Check all projectile/enemy/player colliders are triggers and layers collide as intended
- Playtest for at least 5 waves and verify score, power-ups, pause, and game-over flow
