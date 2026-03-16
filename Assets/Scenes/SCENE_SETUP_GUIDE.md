# Scene Setup Guide

This document provides step-by-step instructions for manually setting up the game scenes if the automated wizard doesn't work.

## GameScene Setup

### 1. Create Camera
- Main Camera
  - Projection: Orthographic
  - Size: 5
  - Background: RGB(5, 5, 20)
  - Clear Flags: Solid Color

### 2. Create Game Managers (Empty GameObjects)

```
Hierarchy:
├── GameManager (GameManager.cs)
├── ScoreManager (ScoreManager.cs)
├── WaveManager (WaveManager.cs)
├── EnemySpawner (EnemySpawner.cs)
├── PowerUpSpawner (PowerUpSpawner.cs)
├── SoundManager (SoundManager.cs)
├── ObjectPooler (ObjectPooler.cs)
└── ScreenBounds (ScreenBounds.cs)
```

### 3. Create Player

1. Create empty GameObject named "Player"
2. Add components:
   - SpriteRenderer (assign player sprite)
   - BoxCollider2D (Is Trigger: true, Size: 0.8, 0.8)
   - Rigidbody2D (Gravity Scale: 0, Freeze Rotation: true)
   - PlayerController.cs
   - PlayerHealth.cs
3. Set Tag: "Player"
4. Position: (0, -3, 0)
5. Create child "Shield":
   - SpriteRenderer (shield sprite, initially disabled)
   - Scale: 1.5

### 4. Create Background

1. Create empty GameObject "Background"
2. Position: (0, 0, 10)
3. Add SpriteRenderer (space background sprite)
4. Add InfiniteBackground.cs
5. Set Sorting Layer: "Background"

### 5. Setup Object Pooler

Select ObjectPooler and configure pools:

| Tag | Prefab | Size | Expandable |
|-----|--------|------|------------|
| PlayerBullet | PlayerBullet | 50 | true |
| EnemyBullet | EnemyBullet | 100 | true |
| SmallEnemy | SmallEnemy | 20 | true |
| MediumEnemy | MediumEnemy | 15 | true |
| LargeEnemy | LargeEnemy | 10 | true |
| TrackerEnemy | TrackerEnemy | 10 | true |
| BossEnemy | BossEnemy | 2 | true |
| Explosion | Explosion | 30 | true |
| PowerUp_Weapon | PowerUp_Weapon | 5 | true |
| PowerUp_Shield | PowerUp_Shield | 5 | true |
| PowerUp_Health | PowerUp_Health | 5 | true |
| PowerUp_Score | PowerUp_Score | 5 | true |

### 6. Create UI Canvas

```
UICanvas (Canvas, UIManager.cs)
├── HUDPanel
│   ├── ScoreText (TMP)
│   ├── HighScoreText (TMP)
│   ├── WaveText (TMP)
│   ├── ComboText (TMP)
│   └── HealthContainer (HealthDisplay.cs)
│       └── HealthIcons (Grid Layout)
├── MainMenuPanel (MainMenuUI.cs)
│   ├── TitleText (TMP)
│   ├── HighScoreText (TMP)
│   ├── PlayButton (Button)
│   └── QuitButton (Button)
├── PauseMenuPanel (PauseMenuUI.cs, initially inactive)
│   ├── PausedText (TMP)
│   ├── ResumeButton (Button)
│   ├── RestartButton (Button)
│   ├── MainMenuButton (Button)
│   └── QuitButton (Button)
├── GameOverPanel (GameOverUI.cs, initially inactive)
│   ├── GameOverText (TMP)
│   ├── FinalScoreText (TMP)
│   ├── WaveReachedText (TMP)
│   ├── NewHighScoreText (TMP, initially inactive)
│   ├── RestartButton (Button)
│   └── MainMenuButton (Button)
└── WaveAnnouncementPanel (initially inactive)
    └── WaveAnnouncementText (TMP)
```

### 7. Wire Up References

**UIManager:**
- Drag all UI panels and text elements to their slots

**GameManager:**
- Set scene names: "MainMenu", "GameScene"
- Drag Player prefab reference

**Button OnClick Events:**
- PlayButton → UIManager.OnPlayButtonPressed()
- ResumeButton → UIManager.OnResumeButtonPressed()
- RestartButton → UIManager.OnRestartButtonPressed()
- MainMenuButton → UIManager.OnMainMenuButtonPressed()
- QuitButton → UIManager.OnQuitButtonPressed()

## MainMenu Scene Setup

1. Create Camera (same settings as GameScene)
2. Create Background with parallax
3. Create UI Canvas with MainMenuPanel only
4. Add GameManager (will persist across scenes)
5. Add SoundManager (for menu music)

## Prefab Configuration

### Player Prefab
```
PlayerController:
  - Move Speed: 10
  - Horizontal Boundary: 8
  - Vertical Boundary Top: 4
  - Vertical Boundary Bottom: -4
  - Fire Rate: 0.2
  - Bullet Pool Tag: "PlayerBullet"

PlayerHealth:
  - Max Health: 3
  - Invincibility Duration: 2
```

### Enemy Prefabs
```
SmallEnemy: Health 1, Speed 5, Score 50
MediumEnemy: Health 2, Speed 3, Score 100, CanShoot
LargeEnemy: Health 5, Speed 1.5, Score 300, CanShoot
TrackerEnemy: Health 2, Speed 2, Score 150, CanShoot
BossEnemy: Health 50, Speed 1, Score 5000, CanShoot
```

### Bullet Prefabs
```
PlayerBullet:
  - Speed: 15
  - Damage: 1
  - Tag: "PlayerBullet"

EnemyBullet:
  - Speed: 8
  - Damage: 1
  - Tag: "EnemyBullet"
```

## Build Settings

1. File > Build Settings
2. Add scenes in order:
   - MainMenu (0)
   - GameScene (1)
3. Platform: Windows
4. Architecture: x86_64
5. Build!
