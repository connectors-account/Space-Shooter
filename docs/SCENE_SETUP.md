# Scene Setup Reference

All scenes are created automatically by the setup wizard. This document is for manual reference.

---

## Scene 1: MainMenu (Build Index 0)

### Hierarchy:
```
Main Camera
  - Background Color: (0.05, 0.05, 0.15)
  - Orthographic Size: 5

GameManager (Script: GameManager.cs)
  - DontDestroyOnLoad

AudioManager (Script: AudioManager.cs)
  - DontDestroyOnLoad

Canvas (Screen Space - Overlay, Scale with Screen 800x600)
  ├── TitleText: "SPACE SHOOTER" (size 48, white, center)
  ├── HighScoreText: "HIGH SCORE: 0" (size 24, yellow, center)
  ├── PlayButton: "PLAY" (200x50 button)
  ├── QuitButton: "QUIT" (200x50 button)
  └── ControlsText: "WASD/Arrows: Move | Space: Shoot | Esc: Pause" (size 16, gray)

EventSystem

MenuManager (Script: MenuManager.cs)
  - References: TitleText, HighScoreText, PlayButton, QuitButton
```

---

## Scene 2: GamePlay (Build Index 1)

### Hierarchy:
```
Main Camera
  - Background Color: (0.02, 0.02, 0.08)
  - Orthographic Size: 5

Background1 (BackgroundScroller, Parallax=1.0, Speed=1.0)
Background2 (BackgroundScroller, Parallax=0.6, Speed=1.0)

Player (Prefab instance at position 0, -3.5, 0)

EnemySpawner (Script: EnemySpawner.cs)
  - References to all 4 enemy prefabs

Canvas (Screen Space - Overlay, Scale with Screen 800x600)
  ├── ScoreText: top-left (size 24, white)
  ├── HighScoreText: top-right (size 20, yellow)
  ├── WaveText: top-center (size 22, cyan)
  ├── WaveAnnouncementText: center (size 40, white, starts hidden)
  ├── HealthBarBG: bottom-left (200x20, dark red)
  │   └── HealthBarFill: (200x20, green, Filled horizontal)
  ├── HealthText: below health bar (size 14, white)
  └── PauseMenuPanel (starts hidden)
      ├── PauseTitle: "PAUSED" (size 42)
      ├── ResumeButton: "RESUME"
      └── QuitToMenuButton: "QUIT TO MENU"

EventSystem

UIManager (Script: UIManager.cs)
  - References to all UI elements
```

---

## Scene 3: GameOver (Build Index 2)

### Hierarchy:
```
Main Camera
  - Background Color: (0.1, 0.02, 0.02)
  - Orthographic Size: 5

Canvas (Screen Space - Overlay, Scale with Screen 800x600)
  ├── GameOverText: "GAME OVER" (size 52, red, center)
  ├── FinalScoreText: "SCORE: 0" (size 32, white)
  ├── FinalHighScoreText: "HIGH SCORE: 0" (size 24, yellow)
  ├── RestartButton: "PLAY AGAIN" (220x50)
  └── MenuButton: "MAIN MENU" (220x50)

EventSystem

MenuManager (Script: MenuManager.cs)
  - References: GameOverText, FinalScoreText, FinalHighScoreText, RestartButton, MenuButton
```

---

## Build Settings (File > Build Settings)

| Index | Scene Path |
|-------|------------|
| 0 | Assets/Scenes/MainMenu.unity |
| 1 | Assets/Scenes/GamePlay.unity |
| 2 | Assets/Scenes/GameOver.unity |

**Target Platform**: PC, Mac & Linux Standalone  
**Target**: Windows  
**Architecture**: x86_64
