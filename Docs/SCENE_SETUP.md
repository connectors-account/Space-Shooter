# Scene Setup Guide

This guide explains how to set up the scenes for the Space Shooter game.

## Overview

The game requires two scenes:
1. **MainMenu** - Title screen with play/quit buttons
2. **GameScene** - The main gameplay scene

---

## Scene 1: MainMenu

### Step-by-Step Setup

1. **Create New Scene**
   - File > New Scene
   - Save as `Assets/Scenes/MainMenu.unity`

2. **Camera Setup**
   - Select Main Camera
   - Set Background Color: Dark blue (#0a0a2a)
   - Set Size: 5 (orthographic)

3. **Create Canvas**
   ```
   Hierarchy:
   └── Canvas
       ├── Canvas Scaler:
       │   ├── UI Scale Mode: Scale With Screen Size
       │   ├── Reference Resolution: 1920 x 1080
       │   └── Screen Match Mode: Match Width Or Height (0.5)
       └── Graphic Raycaster
   ```

4. **Add Title Text**
   - Right-click Canvas > UI > Text - TextMeshPro
   - Name: "TitleText"
   - Text: "SPACE SHOOTER"
   - Font Size: 72
   - Alignment: Center
   - Position: (0, 150, 0)
   - Color: Cyan (#00FFFF)

5. **Add High Score Text**
   - Create TextMeshPro text
   - Name: "HighScoreText"
   - Font Size: 36
   - Position: (0, 50, 0)

6. **Add Play Button**
   - Right-click Canvas > UI > Button - TextMeshPro
   - Name: "PlayButton"
   - Position: (0, -50, 0)
   - Size: (200, 50)
   - Button text: "PLAY"

7. **Add Quit Button**
   - Duplicate Play Button
   - Name: "QuitButton"
   - Position: (0, -120, 0)
   - Button text: "QUIT"

8. **Add MainMenu Script**
   - Create empty GameObject: "MenuManager"
   - Add MainMenu.cs component
   - Assign references:
     - Title Text
     - High Score Text
     - Play Button
     - Quit Button

9. **Add GameManager** (if not using DontDestroyOnLoad)
   - Create empty GameObject: "GameManager"
   - Add GameManager.cs component
   - Add ScoreManager.cs component
   - Add AudioManager.cs component

10. **Optional: Add Background**
    - Add parallax background or static image
    - Add StarfieldGenerator for animated stars

---

## Scene 2: GameScene

### Step-by-Step Setup

1. **Create New Scene**
   - File > New Scene
   - Save as `Assets/Scenes/GameScene.unity`

2. **Camera Setup**
   - Select Main Camera
   - Set Background Color: Black (#000000)
   - Set Size: 5 (orthographic)
   - Add tag: "MainCamera"

3. **Create Game Managers**

   ```
   Hierarchy:
   └── Managers (Empty GameObject)
       ├── GameManager (Script)
       ├── ScoreManager (Script)  
       ├── AudioManager (Script)
       └── Children:
           ├── MusicSource (AudioSource)
           └── SFXSource (AudioSource)
   ```

4. **Create Object Pooler**
   ```
   Hierarchy:
   └── ObjectPooler
       └── ObjectPooler (Script)
           └── Pools:
               ├── {Tag: "PlayerBullet", Prefab: PlayerBullet, Size: 30}
               ├── {Tag: "EnemyBullet", Prefab: EnemyBullet, Size: 50}
               ├── {Tag: "BasicEnemy", Prefab: BasicEnemy, Size: 20}
               ├── {Tag: "ZigzagEnemy", Prefab: ZigzagEnemy, Size: 15}
               ├── {Tag: "CircularEnemy", Prefab: CircularEnemy, Size: 15}
               ├── {Tag: "ChargerEnemy", Prefab: ChargerEnemy, Size: 10}
               └── {Tag: "PowerUp", Prefab: PowerUp, Size: 10}
   ```

5. **Create Spawners**
   ```
   Hierarchy:
   └── Spawners (Empty GameObject)
       ├── EnemySpawner
       │   └── EnemySpawner (Script)
       │       └── Assign all enemy prefabs
       ├── WaveSpawner
       │   └── WaveSpawner (Script)
       │       └── Configure waves or use auto-generation
       └── PowerUpSpawner
           └── PowerUpSpawner (Script)
               └── Assign PowerUp prefab
   ```

6. **Create Player**
   - Drag Player prefab into scene
   - Position: (0, -3, 0)
   - Ensure all references are set:
     - Bullet Prefab
     - Fire Point

7. **Create Background**
   ```
   Hierarchy:
   └── Background (Empty GameObject, Position: (0, 0, 1))
       ├── ParallaxBackground (Script)
       ├── StarfieldGenerator (Script)
       └── Children:
           ├── Layer1 (SpriteRenderer with background sprite)
           │   └── Sorting Layer: Background, Order: -10
           └── Layer2 (SpriteRenderer with nebula overlay)
               └── Sorting Layer: Background, Order: -5
   ```

8. **Create UI Canvas**
   ```
   Hierarchy:
   └── Canvas
       ├── UIManager (Script)
       └── Children:
           ├── HUDPanel
           │   ├── ScoreText (top-left)
           │   ├── HighScoreText (top-center)
           │   ├── WaveText (top-right)
           │   ├── LivesText (bottom-left)
           │   ├── HealthBar (bottom-left, below lives)
           │   ├── MultiplierText (center-right)
           │   └── ComboText (center)
           ├── PausePanel (centered, initially inactive)
           │   ├── Background (semi-transparent black)
           │   ├── PauseTitle
           │   ├── ResumeButton
           │   ├── RestartButton
           │   └── MainMenuButton
           ├── GameOverPanel (centered, initially inactive)
           │   ├── Background
           │   ├── GameOverTitle
           │   ├── FinalScoreText
           │   ├── FinalWaveText
           │   ├── HighScoreText
           │   ├── RestartButton
           │   └── MainMenuButton
           └── VictoryPanel (centered, initially inactive)
               ├── Background
               ├── VictoryTitle
               ├── FinalScoreText
               ├── RestartButton
               └── MainMenuButton
   ```

9. **Connect UI References**
   - Select Canvas
   - In UIManager component, assign all text and panel references
   - Set up button OnClick events:
     - Resume: UIManager.OnResumeButton()
     - Restart: UIManager.OnRestartButton()
     - Main Menu: UIManager.OnMainMenuButton()

10. **Set Initial Panel States**
    - HUDPanel: Active
    - PausePanel: Inactive
    - GameOverPanel: Inactive
    - VictoryPanel: Inactive

---

## UI Layout Reference

### HUD Layout (1920x1080 reference)

```
┌────────────────────────────────────────────────────────────────┐
│ Score: 0          HIGH SCORE: 1000              Wave 1         │
│                                                                 │
│                                                                 │
│                                                          x2     │
│                                                                 │
│                            15 COMBO!                            │
│                                                                 │
│                                                                 │
│                                                                 │
│ Lives: 3                                                        │
│ [████████████████████]                                          │
└────────────────────────────────────────────────────────────────┘
```

### Pause/GameOver Panel Layout

```
┌────────────────────────────────────────────────────────────────┐
│                                                                 │
│                                                                 │
│                    ┌─────────────────────┐                     │
│                    │                     │                     │
│                    │       PAUSED        │                     │
│                    │                     │                     │
│                    │   [ RESUME ]        │                     │
│                    │   [ RESTART ]       │                     │
│                    │   [ MAIN MENU ]     │                     │
│                    │                     │                     │
│                    └─────────────────────┘                     │
│                                                                 │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## Tag Configuration

Create these tags in Unity (Edit > Project Settings > Tags and Layers):

1. Player
2. Enemy
3. PlayerBullet
4. EnemyBullet
5. PowerUp

---

## Layer Configuration

Create these layers:

1. Player (Layer 8)
2. Enemies (Layer 9)
3. PlayerBullets (Layer 10)
4. EnemyBullets (Layer 11)
5. PowerUps (Layer 12)

### Physics 2D Collision Matrix

Go to Edit > Project Settings > Physics 2D and configure:

- Player collides with: Enemies, EnemyBullets, PowerUps
- Enemies collides with: Player, PlayerBullets
- PlayerBullets collides with: Enemies
- EnemyBullets collides with: Player
- PowerUps collides with: Player

---

## Sorting Layers

Create these sorting layers (Edit > Project Settings > Tags and Layers):

1. Background (Order: -100)
2. Stars (Order: -50)
3. Projectiles (Order: 0)
4. Pickups (Order: 5)
5. Characters (Order: 10)
6. Effects (Order: 15)
7. UI (Order: 100)

---

## Build Settings

1. Open File > Build Settings
2. Add scenes in order:
   - Scenes/MainMenu (Index 0)
   - Scenes/GameScene (Index 1)
3. Set Platform: PC, Mac & Linux Standalone
4. Target Platform: Windows

---

## Quick Checklist

### MainMenu Scene
- [ ] Camera configured
- [ ] Canvas with proper scaling
- [ ] Title text
- [ ] High score display
- [ ] Play button with OnClick
- [ ] Quit button with OnClick
- [ ] MainMenu script attached
- [ ] GameManager present (or using DontDestroyOnLoad)

### GameScene
- [ ] Camera configured
- [ ] GameManager with all sub-managers
- [ ] ObjectPooler with all pools
- [ ] EnemySpawner with prefabs
- [ ] WaveSpawner configured
- [ ] PowerUpSpawner with prefab
- [ ] Player with all components
- [ ] Background with parallax/stars
- [ ] UI Canvas with UIManager
- [ ] All UI panels created
- [ ] All button events connected
- [ ] Tags created and assigned
- [ ] Layers created and assigned
- [ ] Sorting layers configured
