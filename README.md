# Space Shooter (Unity Windows Desktop)

Complete vertical 2D space-shooter project skeleton for **Unity 2022.3 LTS (Built-in Render Pipeline)**.

- Platform target: **Windows x86_64 (.exe)**
- Input: **Arrow keys / WASD** (move), **Space** (shoot), **Esc** (pause)
- Gameplay: **5 waves**, scaling difficulty, score, lives, power-ups, parallax background

## Folder Structure

```text
Space-Shooter/
├── Assets/
│   ├── Audio/
│   │   ├── button_click.wav
│   │   ├── explosion.wav
│   │   ├── game_over.wav
│   │   ├── player_hit.wav
│   │   ├── powerup.wav
│   │   ├── shoot.wav
│   │   └── wave_start.wav
│   ├── Prefabs/
│   │   └── .gitkeep
│   ├── Scenes/
│   │   └── .gitkeep
│   ├── Scripts/
│   │   ├── AudioManager.cs
│   │   ├── BulletController.cs
│   │   ├── EnemyController.cs
│   │   ├── GameManager.cs
│   │   ├── MenuManager.cs
│   │   ├── ParallaxBackground.cs
│   │   ├── PlayerController.cs
│   │   ├── PowerUpController.cs
│   │   ├── SpawnManager.cs
│   │   └── UIManager.cs
│   └── Sprites/
│       ├── bg_layer1.png
│       ├── bg_layer2.png
│       ├── enemy_basic.png
│       ├── enemy_bullet.png
│       ├── enemy_tank.png
│       ├── enemy_zigzag.png
│       ├── explosion.png
│       ├── player_bullet.png
│       ├── player_ship.png
│       ├── powerup_health.png
│       ├── powerup_rapidfire.png
│       ├── powerup_shield.png
│       └── shield_bubble.png
├── ProjectSettings/
└── README.md
```

## Scripts Included (Complete, No Placeholders)

1. `PlayerController.cs` — movement, shooting, lives, shield, rapid fire, health restore hooks
2. `EnemyController.cs` — movement patterns, enemy shooting, score value, optional power-up drop
3. `BulletController.cs` — owner-aware bullet logic (player/enemy), collision damage
4. `PowerUpController.cs` — Shield / Rapid Fire / Health Restore behavior
5. `GameManager.cs` — game state, score, 5-wave progression, pause/game over/victory
6. `SpawnManager.cs` — enemy wave configs and spawn loop with increasing difficulty
7. `UIManager.cs` — HUD score/lives/wave/power-up status + transient messages
8. `ParallaxBackground.cs` — 2-layer scrolling and wrap logic
9. `MenuManager.cs` — Start/Pause/Game Over/Victory + scene transitions
10. `AudioManager.cs` — centralized SFX playback

> Collision handling uses built-in Unity 2D triggers (`OnTriggerEnter2D`) directly in gameplay scripts.

## Scene Setup (MainMenu + Game)

## 1) Create scenes

- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/Game.unity`

Add both scenes to **File > Build Settings** in this order:
1. `MainMenu`
2. `Game`

## 2) MainMenu Scene

Create:
- `Canvas`
  - `MainMenuPanel`
    - Title text
    - `Start Button` (OnClick -> `MenuManager.OnStartClicked`)
    - `Quit Button` (OnClick -> `MenuManager.OnQuitClicked`)
- `EventSystem`
- `MenuSystem` GameObject with `MenuManager`
  - `gameSceneName = "Game"`
  - `mainMenuSceneName = "MainMenu"`
  - Assign `mainMenuPanel`

## 3) Game Scene

Create root objects:
- `Managers`
  - `GameManager` (+ script)
  - `SpawnManager` (+ script)
  - `UIManager` (+ script)
  - `MenuManager` (+ script)
  - `AudioManager` (+ script)
- `Background`
  - `ParallaxRoot` (+ `ParallaxBackground`)
  - Layer sprites (tile pairs per layer)
- `Player` (SpriteRenderer + Rigidbody2D kinematic + CircleCollider2D trigger + `PlayerController`)
- `Spawners` (optional anchor for organization)
- `Canvas` (HUD + Pause/GameOver/Victory panels)
- `EventSystem`

`GameManager` references:
- SpawnManager, UIManager, MenuManager, Player
- `autoStartGameOnSceneLoad = true`

## Prefab Setup

Create these prefabs in `Assets/Prefabs`:

1. **Player.prefab**
   - Sprite: `player_ship`
   - `Rigidbody2D` (Body Type: Kinematic, Gravity Scale 0)
   - Trigger collider
   - `PlayerController`
   - Child `FirePoint` transform
   - Optional child `ShieldVisual` sprite (`shield_bubble`), initially disabled

2. **PlayerBullet.prefab**
   - Sprite: `player_bullet`
   - Trigger collider
   - `BulletController`

3. **EnemyBullet.prefab**
   - Sprite: `enemy_bullet`
   - Trigger collider
   - `BulletController`

4. **Enemy prefabs** (at least 3):
   - `Enemy_Basic.prefab`, `Enemy_Zigzag.prefab`, `Enemy_Tank.prefab`
   - Sprite + `EnemyController` + trigger collider + kinematic `Rigidbody2D`
   - Assign enemy bullet prefab in `EnemyController`
   - Assign power-up prefabs array for drop chance

5. **Power-up prefabs**
   - `PowerUp_Shield.prefab` with `PowerUpController` type `Shield`
   - `PowerUp_Rapid.prefab` with type `RapidFire`
   - `PowerUp_Health.prefab` with type `HealthRestore`

## Sprite Setup / Generation

You can use existing sprites in `Assets/Sprites`. For simple geometric fallback:

1. Right click in Project -> **Create > Sprites > Square/Triangle/Circle**
2. Color-code by role:
   - Player: cyan/green
   - Enemies: red/orange
   - Player bullet: yellow
   - Enemy bullet: magenta
   - Power-ups: blue/purple/green
3. Set all sprite import mode to **Sprite (2D and UI)**, Pixels Per Unit = 100

## UI Canvas Setup

In Game scene Canvas, create:
- `HUDPanel`:
  - `ScoreText`
  - `LivesText`
  - `WaveText`
  - `PowerUpText`
  - `MessageText` (inactive by default)
- `PausePanel`:
  - Resume button -> `MenuManager.OnResumeClicked`
  - Main Menu button -> `MenuManager.OnMainMenuClicked`
- `GameOverPanel`:
  - score/wave labels
  - Restart button -> `MenuManager.OnRestartClicked`
  - Main Menu button -> `MenuManager.OnMainMenuClicked`
- `VictoryPanel`:
  - final score label
  - Restart / Main Menu buttons

Assign all text/panel references in `UIManager` and `MenuManager` inspectors.

## Audio Integration

In `AudioManager` component, map clips:
- `shoot.wav` -> Shoot
- `explosion.wav` -> Explosion
- `player_hit.wav` -> PlayerHit
- `powerup.wav` -> PowerUp
- `wave_start.wav` -> WaveStart
- `game_over.wav` -> GameOver
- `button_click.wav` -> ButtonClick

All major game actions already call these methods in code.

## Tags / Physics

Create tags in **Project Settings > Tags and Layers**:
- `Player`
- `Enemy`
- `PlayerBullet`
- `EnemyBullet`
- `PowerUp`
- `Bounds` (optional kill-zone object)

Use trigger colliders for bullets, player, enemies, power-ups.

## Windows Build (.exe)

1. Open **File > Build Settings**
2. Platform: **PC, Mac & Linux Standalone**
3. Target platform: **Windows**, architecture **x86_64**
4. Add `MainMenu` and `Game` scenes
5. In **Player Settings**:
   - Company/Product name as desired
   - Default screen: 1280x720 or 1920x1080 windowed
6. Click **Build**, select output folder (example: `Build/Windows`)
7. Run generated `Space Shooter.exe`

Expected build output:
- `Space Shooter.exe`
- `Space Shooter_Data/`
- `UnityPlayer.dll`
- `MonoBleedingEdge/`

## Notes

- This project is intentionally simple and fully script-complete for fast setup.
- If you want richer polish (enemy boss, VFX, music loop, save profiles), extend from this baseline.
