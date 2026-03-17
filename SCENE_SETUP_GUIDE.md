# Scene Setup Guide — Space Shooter

This document provides exact, step-by-step instructions for setting up each scene in the Unity Editor.

---

## Scene 1: MainMenu

### Hierarchy Structure
```
MainMenu (Scene)
├── Main Camera
├── GameManager          ← Empty GO, GameManager.cs, DontDestroyOnLoad
├── AudioManager         ← Empty GO, AudioManager.cs + 2 AudioSources, DontDestroyOnLoad
├── Canvas
│   ├── TitleText        ← UI > Text
│   ├── HighScoreText    ← UI > Text
│   ├── PlayButton       ← UI > Button
│   ├── QuitButton       ← UI > Button
│   └── VersionText      ← UI > Text
└── EventSystem          ← Auto-created with Canvas
```

### Camera Setup
- **Clear Flags**: Solid Color
- **Background**: `#050510` (very dark blue-black)
- **Size**: 5 (default orthographic)

### GameManager GameObject
1. Create: **GameObject → Create Empty**, name `GameManager`
2. Add Component: `GameManager` script
3. Leave Player and EnemySpawner references empty (they exist only in GamePlay scene)

### AudioManager GameObject
1. Create: **GameObject → Create Empty**, name `AudioManager`
2. Add Component: `AudioManager` script
3. Add Component: `AudioSource` (for music)
   - Loop: ✓, Play On Awake: ✗
4. Add Component: `AudioSource` (for SFX)
   - Loop: ✗, Play On Awake: ✗
5. In AudioManager Inspector:
   - Drag music AudioSource → **Music Source**
   - Drag SFX AudioSource → **SFX Source**
   - Assign audio clips as available

### Canvas Setup
1. Create: **GameObject → UI → Canvas**
2. Canvas component:
   - Render Mode: `Screen Space - Overlay`
3. Canvas Scaler:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `800 × 600`
   - Match: `0.5`
4. Add Component: `MenuManager` script to Canvas

### UI Elements (children of Canvas)

#### TitleText
- Create: Right-click Canvas → **UI → Text**
- Name: `TitleText`
- Rect Transform: Anchor to top-center
  - Pos Y: `150`, Width: `600`, Height: `80`
- Text: `SPACE SHOOTER`
- Font Size: `48`
- Alignment: Center/Middle
- Color: White
- **Assign to** MenuManager → Title Text

#### HighScoreText
- Create: **UI → Text**, name `HighScoreText`
- Pos Y: `50`, Width: `400`, Height: `40`
- Font Size: `24`, Color: `#FFFF00` (Yellow)
- Text: `High Score: 0`
- **Assign to** MenuManager → High Score Text

#### PlayButton
- Create: **UI → Button**, name `PlayButton`
- Pos Y: `-50`, Width: `200`, Height: `50`
- Child Text: `PLAY`, Font Size: `28`
- Button colors: Normal=Dark Blue, Highlighted=Light Blue
- **Assign to** MenuManager → Play Button

#### QuitButton
- Create: **UI → Button**, name `QuitButton`
- Pos Y: `-120`, Width: `200`, Height: `50`
- Child Text: `QUIT`, Font Size: `28`
- **Assign to** MenuManager → Quit Button

#### VersionText
- Create: **UI → Text**, name `VersionText`
- Anchor: Bottom-Right, Font Size: `14`, Color: Gray
- **Assign to** MenuManager → Version Text

---

## Scene 2: GamePlay

### Hierarchy Structure
```
GamePlay (Scene)
├── Main Camera
├── BackgroundFar         ← SpriteRenderer + ParallaxBackground
├── BackgroundFar_Copy    ← Duplicate, offset by sprite height
├── BackgroundNear        ← SpriteRenderer + ParallaxBackground
├── BackgroundNear_Copy   ← Duplicate, offset by sprite height
├── Player                ← Player prefab instance
│   └── FirePoint         ← Child empty GO
├── EnemySpawner          ← Empty GO + EnemySpawner.cs
├── Canvas
│   ├── ScoreText         ← UI > Text
│   ├── WaveText          ← UI > Text
│   ├── HealthBar         ← UI > Slider
│   ├── WaveAnnouncementPanel ← UI > Panel (hidden)
│   │   └── WaveAnnouncementText
│   ├── PauseMenuPanel    ← UI > Panel (hidden)
│   │   ├── PausedTitle
│   │   ├── ResumeButton
│   │   ├── PauseMainMenuButton
│   │   └── PauseQuitButton
│   └── GameOverPanel     ← UI > Panel (hidden)
│       ├── GameOverTitle
│       ├── FinalScoreText
│       ├── HighScoreText
│       ├── RestartButton
│       └── GameOverMainMenuButton
└── EventSystem
```

### Camera Setup
- Same as MainMenu: Solid Color, `#050510`, Size `5`

### Background Layers

#### BackgroundFar
1. Create: **GameObject → Create Empty**, name `BackgroundFar`
2. Add: **SpriteRenderer**
   - Sprite: `BackgroundStarsFar`
   - Sorting Layer: `Background`, Order: `0`
   - Draw Mode: `Simple`
3. Add: **ParallaxBackground** script
   - Scroll Speed: `0.5`
   - Sprite Height: `10.24` (1024px ÷ 100 PPU)
4. Position: `(0, 0, 1)` (Z=1 to be behind gameplay)

#### BackgroundFar_Copy
1. Duplicate `BackgroundFar` (Ctrl+D)
2. Move to: `(0, 10.24, 1)` — exactly one sprite height above

#### BackgroundNear
1. Create another pair with `BackgroundStarsNear` sprite
2. Sorting Layer: `Background`, Order: `1`
3. Scroll Speed: `1.5`
4. Position: `(0, 0, 0.5)`
5. Duplicate and offset same as above

### Player Setup
1. Drag `Player` prefab from `Assets/Prefabs/` into scene
2. Position: `(0, -3, 0)`
3. Verify all PlayerController references are set

### EnemySpawner
1. Create: **GameObject → Create Empty**, name `EnemySpawner`
2. Add: **EnemySpawner** script
3. Assign in Inspector:
   - Basic Enemy Prefab → `EnemyBasic`
   - Zigzag Enemy Prefab → `EnemyZigzag`
   - Heavy Enemy Prefab → `EnemyHeavy`

### GamePlay Canvas
1. Create Canvas (same settings as MainMenu)
2. Add: **UIManager** script to Canvas

#### ScoreText
- Anchor: Top-Left
- Pos: `(120, -30)`, Width: `200`, Height: `30`
- Text: `Score: 0`, Font Size: `22`, Color: White
- **Assign to** UIManager → Score Text

#### WaveText
- Anchor: Top-Center
- Pos: `(0, -30)`, Width: `200`, Height: `30`
- Text: `Wave: 1`, Font Size: `22`, Color: White
- **Assign to** UIManager → Wave Text

#### HealthBar (Slider)
- Create: **UI → Slider**, name `HealthBar`
- Anchor: Top-Right
- Pos: `(-120, -30)`, Width: `200`, Height: `20`
- **Uncheck** `Interactable` on Slider component
- Min Value: `0`, Max Value: `100`, Value: `100`
- Background: Set Image color to `#330000` (dark red)
- Fill Area → Fill: Set Image color to `#00FF00` (green)
- Remove Handle Slide Area (delete the child)
- **Assign Slider to** UIManager → Health Bar
- **Assign Fill Image to** UIManager → Health Bar Fill

#### WaveAnnouncementPanel
- Create: **UI → Panel**, name `WaveAnnouncementPanel`
- Anchor: Stretch to full screen
- Image color: `rgba(0, 0, 0, 0)` (fully transparent)
- Child Text `WaveAnnouncementText`:
  - Center of screen, Font Size: `48`, Color: `#FFFF00`
  - Text: `WAVE 1`
- **Set panel inactive** in Inspector (uncheck checkbox)
- **Assign to** UIManager → Wave Announcement Panel + Text

#### PauseMenuPanel
- Create: **UI → Panel**, name `PauseMenuPanel`
- Stretch to full screen
- Image color: `rgba(0, 0, 0, 180)` (semi-transparent black)
- Children:
  - Text `PausedTitle`: "PAUSED", Font Size `42`, White, centered at `(0, 100)`
  - Button `ResumeButton`: "RESUME", `(0, 0)`
  - Button `PauseMainMenuButton`: "MAIN MENU", `(0, -70)`
  - Button `PauseQuitButton`: "QUIT", `(0, -140)`
- **Set panel inactive**
- **Assign all references** to UIManager

#### GameOverPanel
- Create: **UI → Panel**, name `GameOverPanel`
- Stretch to full screen
- Image color: `rgba(0, 0, 0, 200)`
- Children:
  - Text `GameOverTitle`: "GAME OVER", Font Size `48`, Red, `(0, 120)`
  - Text `FinalScoreText`: "Score: 0", Font Size `28`, White, `(0, 50)`
  - Text `HighScoreText`: "High Score: 0", Font Size `22`, Yellow, `(0, 10)`
  - Button `RestartButton`: "RESTART", `(0, -60)`
  - Button `GameOverMainMenuButton`: "MAIN MENU", `(0, -130)`
- **Set panel inactive**
- **Assign all references** to UIManager

---

## Build Settings Final Checklist

1. **File → Build Settings**
2. Scenes in build:
   - ☑ `Assets/Scenes/MainMenu.unity` — Index 0
   - ☑ `Assets/Scenes/GamePlay.unity` — Index 1
3. Platform: `PC, Mac & Linux Standalone`
4. Target: `Windows`, Architecture: `x86_64`
5. Player Settings:
   - Product Name: `Space Shooter`
   - Default Resolution: `800 × 600`
   - Fullscreen Mode: `Windowed`

---

## Prefab Reference Checklist

| Prefab | Script | Key Fields to Assign |
|--------|--------|---------------------|
| Player | PlayerController | bulletPrefab → PlayerBullet, firePoint → FirePoint child |
| EnemyBasic | EnemyController | powerUpPrefabs → [PowerUpWeapon, PowerUpShield, PowerUpHealth] |
| EnemyZigzag | EnemyController | powerUpPrefabs → [PowerUpWeapon, PowerUpShield, PowerUpHealth] |
| EnemyHeavy | EnemyController | enemyBulletPrefab → EnemyBullet, powerUpPrefabs → [...] |
| PlayerBullet | BulletController | damage → 10 |
| EnemyBullet | BulletController | damage → 15 |
| PowerUpWeapon | PowerUpController | powerUpType → WeaponUpgrade |
| PowerUpShield | PowerUpController | powerUpType → Shield |
| PowerUpHealth | PowerUpController | powerUpType → HealthRestore |

---

## Tag Assignment Checklist

| GameObject/Prefab | Tag |
|---|---|
| Player | `Player` |
| EnemyBasic | `Enemy` |
| EnemyZigzag | `Enemy` |
| EnemyHeavy | `Enemy` |
| PlayerBullet | `PlayerBullet` |
| EnemyBullet | `EnemyBullet` |
| PowerUpWeapon | `PowerUp` |
| PowerUpShield | `PowerUp` |
| PowerUpHealth | `PowerUp` |
