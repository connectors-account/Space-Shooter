# Scene Setup Guide

## 1) MainMenu Scene

Create scene: `MainMenu`

Hierarchy (example):
- `Main Camera`
- `Canvas`
  - `TitleText`
  - `StartButton`
  - `QuitButton`
- `MenuManager` (with `MenuManager.cs`)

Button wiring:
- StartButton -> OnClick -> `MenuManager.StartGame()`
- QuitButton -> OnClick -> `MenuManager.QuitGame()`

## 2) GamePlay Scene

Create scene: `GamePlay`

Hierarchy (example):
- `Main Camera` (orthographic)
- `BackgroundLayer1` (SpriteRenderer or quad)
- `BackgroundLayer2` (SpriteRenderer or quad)
- `ParallaxSystem` (`ParallaxBackground.cs`)
- `GameManager` (`GameManager.cs`)
- `SpawnManager` (`SpawnManager.cs`)
- `AudioManager` (`AudioManager.cs` + 2 AudioSources)
- `UIManager` (`UIManager.cs`)
- `MenuManager` (`MenuManager.cs`)
- `Player` (instance of Player prefab)
- `Canvas`
  - `HUDRoot`
    - `ScoreText`
    - `HealthText`
    - `WaveText`
    - `RapidFireIndicatorText`
    - `ShieldIndicatorText`
  - `PauseMenuRoot` (set inactive initially)
    - Resume / Main Menu buttons
  - `GameOverRoot` (set inactive initially)
    - GameOverSummaryText
    - Restart / Main Menu buttons

## 3) Camera Setup

Recommended orthographic camera:
- Projection: Orthographic
- Size: ~5
- Position: `(0, 0, -10)`

## 4) Build Settings

`File -> Build Settings -> Add Open Scenes`
Order:
1. MainMenu
2. GamePlay

This order ensures app starts at menu.
