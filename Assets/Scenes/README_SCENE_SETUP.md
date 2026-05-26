# Scene Setup Guide — GameScene

Open Unity and create/configure the scene as follows:

---

## Camera Setup
1. Select **Main Camera** in Hierarchy
2. Set **Background Color** = dark navy/black (#0A0A2E)
3. **Projection** = Orthographic
4. **Size** = 6 (fits 1080×1920 portrait, adjust to taste)
5. **Position** = (0, 0, -10)

---

## Scene Hierarchy Structure

```
GameScene
├── Main Camera
├── --- MANAGERS ---
│   ├── GameManager          (GameManager.cs)
│   ├── UIManager            (UIManager.cs + Canvas child)
│   ├── AudioManager         (AudioManager.cs)
│   └── GameInitializer      (GameInitializer.cs)
├── --- GAMEPLAY ---
│   ├── EnemySpawner         (EnemySpawner.cs)
│   ├── PlayerSpawnPoint     (empty, position = (0, -4, 0))
│   └── Background           (quad with scrolling material)
└── --- UI ---
    └── Canvas               (Screen Space - Overlay)
        ├── HUD_Panel
        │   ├── ScoreText        (Text, top-left)
        │   ├── HighScoreText    (Text, top-right)
        │   ├── WaveText         (Text, top-center)
        │   ├── LivesText        (Text, bottom-left)
        │   ├── HealthBar        (Slider, bottom-center)
        │   └── HealthText       (Text, on health bar)
        ├── MainMenu_Panel
        │   ├── TitleText        (Text, "SPACE SHOOTER")
        │   ├── StartButton      (Button, "START GAME")
        │   └── QuitButton       (Button, "QUIT")
        ├── Pause_Panel
        │   ├── PausedText       (Text, "PAUSED")
        │   └── ResumeButton     (Button, "RESUME")
        └── GameOver_Panel
            ├── GameOverText     (Text, "GAME OVER")
            ├── FinalScoreText   (Text)
            ├── FinalHighScore   (Text)
            ├── RestartButton    (Button, "PLAY AGAIN")
            └── MainMenuButton   (Button, "MAIN MENU")
```

---

## Wiring References in Inspector

### GameInitializer
- Game Manager → drag GameManager object
- UI Manager → drag UIManager object
- Enemy Spawner → drag EnemySpawner object
- Player Prefab → drag Player.prefab from Assets/Prefabs
- Player Spawn Point → drag PlayerSpawnPoint object

### UIManager
- Drag each UI element to the matching serialized field
- Wire buttons: StartButton, ResumeButton, RestartButton, MainMenuButton, QuitButton

### EnemySpawner
- Enemy Prefabs → array of enemy prefabs [Enemy_Basic, Enemy_Zigzag, Enemy_Dive]

### AudioManager
- Background Music → drag bgm_gameplay audio clip

---

## Physics Layer Matrix (Edit → Project Settings → Physics 2D)

Disable collisions between layers that shouldn't interact:
- PlayerBullet ↔ Player = **OFF**
- PlayerBullet ↔ PlayerBullet = **OFF**
- EnemyBullet ↔ Enemy = **OFF**
- EnemyBullet ↔ EnemyBullet = **OFF**
- PlayerBullet ↔ EnemyBullet = **OFF**
