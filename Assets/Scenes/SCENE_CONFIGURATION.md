# Scene Configuration

This project is designed around a single primary gameplay scene (`GameScene.unity`) with menu panels managed by `UIManager`.

## Recommended Scenes
- `Assets/Scenes/MainMenuScene.unity` (optional if using separate scene)
- `Assets/Scenes/GameScene.unity` (required)

## GameScene Required Hierarchy

```text
Main Camera
  - Camera (Orthographic)
  - SpaceShooter.Utils.ScreenBounds

GameManager
  - SpaceShooter.Managers.GameManager

AudioManager
  - SpaceShooter.Managers.AudioManager
  - AudioSource (SFX)
  - AudioSource (Music, Loop=true)

SpawnManager
  - SpaceShooter.Managers.SpawnManager

Player
  - SpriteRenderer
  - BoxCollider2D (Is Trigger = true)
  - Rigidbody2D (Kinematic)
  - SpaceShooter.Player.PlayerController
  - Child: FirePoint
  - Child: ShieldVisual (disabled initially)

StarField
  - SpaceShooter.Environment.StarField

ParallaxBackground
  - SpaceShooter.Environment.ParallaxBackground
  - Child: BG_Layer1 (SpriteRenderer)
  - Child: BG_Layer2 (SpriteRenderer)

UICanvas
  - Canvas
  - CanvasScaler
  - GraphicRaycaster
  - UIManager (with SpaceShooter.UI.UIManager)
  - MainMenuPanel
  - HUDPanel
  - GameOverPanel
  - PausePanel
  - WaveAnnouncementPanel

EventSystem
```

## Main Camera Settings
- Projection: Orthographic
- Size: ~5.5
- Position: `(0, 0, -10)`
- Background: dark color (`#070b16`)

## Build Settings
Add `GameScene.unity` to Scenes In Build.
