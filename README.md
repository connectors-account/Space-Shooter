# Space Shooter (Unity, Windows Desktop)

Complete arcade-style 2D space shooter project scaffold for **Unity 2022.3 LTS** targeting **Windows desktop** builds.

## 1) Project Folder Structure

Use (or verify) this exact structure under `Assets/`:

```text
Assets/
├── Audio/
├── Prefabs/
├── Scenes/
├── Scripts/
│   ├── AudioManager.cs
│   ├── BulletController.cs
│   ├── CollisionHandler.cs
│   ├── EnemyController.cs
│   ├── EnemySpawner.cs
│   ├── GameManager.cs
│   ├── MenuManager.cs
│   ├── ParallaxBackground.cs
│   ├── PlayerController.cs
│   ├── PowerUpController.cs
│   └── UIManager.cs
├── Sprites/
└── UI/
```

## 2) Input Controls

- **Move:** Arrow Keys or WASD
- **Shoot:** Space
- **Pause/Resume:** ESC

(Uses Unity legacy input axes `Horizontal` / `Vertical` + key checks.)

## 3) Sprite Specs (Simple Pixel/Shape Guidance)

Use 32x32 or 64x64 transparent PNG sprites.

### Player ship
```text
   /\
  /##\
 /####\
 |####|
  \##/
   \/
```
- Colors: cyan/white body, bright cockpit.

### Enemy type A (Scout)
```text
 [====]
<| ## |>
 [====]
```
- Small, red.

### Enemy type B (ZigZag)
```text
 /\  /\
<  \/  >
 \_/\_/
```
- Purple/green accent.

### Enemy type C (Tank)
```text
 /------\
|  ####  |
| [####] |
 \------/
```
- Bigger, dark orange/gray.

### Bullets
- Player bullet: slim cyan vertical pill
- Enemy bullet: slim red/orange vertical pill

```text
Player:  ||
Enemy:   !!
```

### Power-ups
- Weapon upgrade: `W` icon (yellow)
- Shield: circle + `S` icon (blue)
- Health: plus icon (green)

### Background elements
- Star dots (white/blue)
- Nebula strips for parallax layers
- Optional planet silhouettes

## 4) Scene Setup

Create two scenes:

- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/GamePlay.unity`

### MainMenu scene hierarchy

```text
Main Camera
Canvas (Screen Space - Overlay)
└── MainMenuPanel
    ├── TitleText ("SPACE SHOOTER")
    ├── PlayButton
    └── QuitButton
EventSystem
MenuManager (empty GameObject with MenuManager.cs)
```

`MenuManager` setup:
- `mainMenuSceneName = MainMenu`
- `gameplaySceneName = GamePlay`
- Hook buttons:
  - PlayButton -> `MenuManager.OnPlayButtonPressed()`
  - QuitButton -> `MenuManager.OnQuitButtonPressed()`

### GamePlay scene hierarchy

```text
Main Camera (Orthographic, Size ~5.5)
GameSystems
├── GameManager (GameManager.cs)
├── EnemySpawner (EnemySpawner.cs)
├── UIManager (UIManager.cs)
├── AudioManager (AudioManager.cs)
└── MenuManager (MenuManager.cs)

Background
├── BG_Layer_01 (ParallaxBackground.cs, slow speed)
└── BG_Layer_02 (ParallaxBackground.cs, faster speed)

Player (prefab instance)
Canvas (Screen Space - Overlay)
├── HUDRoot
│   ├── ScoreText
│   ├── HealthText
│   ├── WaveText
│   ├── WeaponText
│   └── ShieldText
├── WaveBannerText (inactive initially)
├── PauseOverlay (inactive)
│   ├── ResumeButton
│   ├── RestartButton
│   └── MainMenuButton
└── GameOverOverlay (inactive)
    ├── GameOverStatsText
    ├── RestartButton
    └── MainMenuButton

EventSystem
BoundaryTop / BoundaryBottom / BoundaryLeft / BoundaryRight (BoxCollider2D, isTrigger=true, tag="Boundary")
```

Wire references:
- `GameManager.enemySpawner -> EnemySpawner`
- `GameManager.player -> Player`
- `UIManager` text fields to HUD labels
- `MenuManager.pauseOverlay` and `gameOverOverlay` to corresponding UI objects
- Pause/GameOver buttons -> MenuManager methods

## 5) Prefab Setup Instructions

## Player prefab (`Assets/Prefabs/Player.prefab`)
- Components:
  - `SpriteRenderer`
  - `CircleCollider2D` (isTrigger = true)
  - `PlayerController`
- Child: `FirePoint` transform at ship nose
- Optional Child: `ShieldVisual` sprite (disabled by default)
- Assign in `PlayerController`:
  - `playerBulletPrefab`
  - `firePoint`
  - `shieldVisual`

## Enemy prefabs (Scout/ZigZag/Tank)
- Components:
  - `SpriteRenderer`
  - `CircleCollider2D` (isTrigger = true)
  - `EnemyController`
- Per prefab set:
  - `enemyType`
  - `moveSpeed`, `maxHealth`, `scoreValue`, `fireInterval`
  - `enemyBulletPrefab`
  - `firePoint`

## Bullet prefabs
- **PlayerBullet** and **EnemyBullet**
- Components:
  - `SpriteRenderer`
  - `CapsuleCollider2D` (isTrigger = true)
  - `BulletController`
- Leave ownership initialization to scripts (`Initialize(...)`).

## PowerUp prefabs (Weapon/Shield/Health)
- Components:
  - `SpriteRenderer`
  - `CircleCollider2D` (isTrigger = true)
  - `PowerUpController`
- Set `powerUpType` per prefab.

## EnemySpawner setup
- Add 3 entries in `enemyEntries`:
  - Scout weight 50
  - ZigZag weight 30
  - Tank weight 20

## 6) Audio Setup

Place clips under `Assets/Audio/` (or reuse existing files):
- `shoot.wav`
- `explosion.wav`
- `player_hit.wav`
- `powerup.wav`
- `wave_start.wav`
- `game_over.wav`
- optional `enemy_shoot.wav`, `hit.wav`, `button_click.wav`, and music loop

On `AudioManager` object:
- Add two `AudioSource` components:
  - SFX source (`Play On Awake` off)
  - Music source (`Loop` on)
- Map SFX types in inspector to clips via `sfxBindings`
- Assign `gameplayMusic` clip (optional)

## 7) Import + Build (Windows) Step-by-step

1. Open **Unity Hub** -> **Open** -> select this repo root (`Space-Shooter`).
2. Use Unity **2022.3 LTS** if prompted for version.
3. Let Unity import assets and compile scripts.
4. Open and configure scenes:
   - `MainMenu.unity`
   - `GamePlay.unity`
5. Save both scenes.
6. Go to **File -> Build Settings**.
7. Select platform **PC, Mac & Linux Standalone**.
8. Target Platform: **Windows**.
9. Architecture: **x86_64**.
10. Add scenes in order:
    - `Assets/Scenes/MainMenu.unity` (index 0)
    - `Assets/Scenes/GamePlay.unity` (index 1)
11. (Recommended) Player Settings:
    - Company/Product name
    - Resolution and presentation defaults
    - Fullscreen mode as desired
12. Click **Build** (or **Build And Run**), choose output folder (e.g., `Builds/Windows/`).
13. Run generated `Space Shooter.exe`.

## 8) Gameplay Loop Summary

1. Main menu -> Start.
2. Player fights waves.
3. Enemy count and difficulty scale each wave.
4. Score increases per enemy kill.
5. Random power-up drops (weapon/shield/health).
6. ESC toggles pause.
7. On player death -> game over overlay + final/high score.

## 9) Production Notes

- Scripts are fully implemented and interconnected.
- Collision/damage rules are centralized through `CollisionHandler` + `IDamageable`.
- No placeholder methods are left in the required core scripts.
