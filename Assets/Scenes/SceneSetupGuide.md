# Scene Setup Guide

## MainMenu Scene Setup

### Hierarchy Structure:
```
MainMenu
├── Main Camera
├── Canvas
│   ├── MainMenuPanel
│   │   ├── TitleText
│   │   ├── PlayButton
│   │   ├── OptionsButton
│   │   ├── CreditsButton
│   │   ├── QuitButton
│   │   └── HighScoreText
│   ├── OptionsPanel
│   │   ├── OptionsTitle
│   │   ├── VolumeSlider
│   │   ├── FullscreenToggle
│   │   └── BackButton
│   └── CreditsPanel
│       ├── CreditsTitle
│       ├── CreditsText
│       └── BackButton
├── EventSystem
└── MenuManager
```

### Setup Steps:

1. **Create Scene**: File > New Scene, save as "MainMenu"

2. **Camera Setup**:
   - Set Clear Flags to "Solid Color"
   - Set Background to dark blue/black (space theme)

3. **Canvas Setup**:
   - Add Canvas (UI > Canvas)
   - Set UI Scale Mode to "Scale With Screen Size"
   - Reference Resolution: 1920x1080

4. **MainMenuPanel**:
   - Add Panel, name "MainMenuPanel"
   - Add TitleText (TextMeshPro - Text): "SPACE SHOOTER"
   - Add Buttons for Play, Options, Credits, Quit
   - Add HighScoreText below buttons

5. **OptionsPanel**:
   - Create Panel, name "OptionsPanel", disable initially
   - Add Volume Slider
   - Add Fullscreen Toggle
   - Add Back Button

6. **CreditsPanel**:
   - Create Panel, name "CreditsPanel", disable initially
   - Add title and credits text
   - Add Back Button

7. **MenuManager**:
   - Create empty GameObject, add MenuManager script
   - Assign all panel and UI references
   - Connect button OnClick events

8. **Add Background** (optional):
   - Add scrolling star background for visual appeal

---

## GameScene Setup

### Hierarchy Structure:
```
GameScene
├── Main Camera
├── Managers
│   ├── GameManager
│   ├── ScoreManager
│   ├── WaveManager
│   └── EnemySpawner
├── Player (spawned by GameManager)
├── Background
│   ├── ScrollingBackground
│   └── Stars (Parallax layers)
├── Canvas
│   ├── HUD
│   │   ├── HealthBar
│   │   ├── HealthText
│   │   ├── ScoreText
│   │   ├── HighScoreText
│   │   └── WaveText
│   ├── PausePanel
│   │   ├── PausedTitle
│   │   ├── ResumeButton
│   │   ├── RestartButton
│   │   ├── MainMenuButton
│   │   └── QuitButton
│   ├── GameOverPanel
│   │   ├── GameOverTitle
│   │   ├── FinalScoreText
│   │   ├── HighScoreText
│   │   ├── RestartButton
│   │   └── MainMenuButton
│   └── WaveCompletePanel
│       └── WaveCompleteText
├── EventSystem
└── PlayerSpawnPoint
```

### Setup Steps:

1. **Create Scene**: File > New Scene, save as "GameScene"

2. **Camera Setup**:
   - Position: (0, 0, -10)
   - Orthographic Size: 5
   - Background: Dark blue/black
   - Add CameraShake script

3. **Create Managers Object**:
   - Empty GameObject "Managers"
   - Add children with respective scripts:
     - GameManager (with player prefab reference)
     - ScoreManager
     - WaveManager
     - EnemySpawner (configure enemy types list)

4. **PlayerSpawnPoint**:
   - Empty GameObject at position (0, -3, 0)

5. **Background Setup**:
   - Add ScrollingBackground prefab
   - Or create parallax layers with ParallaxBackground script

6. **Canvas & UI**:
   - Create Canvas with Scale Mode "Scale With Screen Size"
   - Create HUD panel (always visible):
     - Health bar (Slider)
     - Score text (TextMeshPro)
     - Wave counter
   - Create PausePanel (disabled by default)
   - Create GameOverPanel (disabled by default)
   - Create WaveCompletePanel (disabled by default)

7. **UIManager**:
   - Add UIManager script to Canvas or Managers
   - Assign all UI references

8. **Connect Everything**:
   - Assign prefabs to GameManager
   - Assign enemy prefabs to EnemySpawner
   - Connect button callbacks to UIManager methods
   - Wire up HealthSystem.OnHealthChanged to UIManager.UpdateHealthBar

---

## Build Settings

1. File > Build Settings
2. Add scenes in order:
   - MainMenu (index 0)
   - GameScene (index 1)
3. Select "PC, Mac & Linux Standalone"
4. Target Platform: Windows
5. Architecture: x86_64
6. Click "Build" to create the executable
