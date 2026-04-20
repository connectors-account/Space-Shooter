# Scene Configuration Reference

Use this as the exact wiring reference for `GameScene.unity`.

## Root Hierarchy (recommended)

- Main Camera
- GameSystems
  - GameManager (component)
  - SpawnManager (component)
  - AudioManager (component)
- Environment
  - ParallaxRoot (ParallaxBackground)
    - BgLayerFar (SpriteRenderer)
    - BgLayerNear (SpriteRenderer)
  - StarFieldRoot (StarField)
- PlayerShip (PlayerController)
  - FirePoint
  - ShieldVisual (optional, disabled by default)
- Canvas
  - MainMenuPanel
  - HUDPanel
  - PausePanel
  - GameOverPanel
  - WaveBannerPanel
  - UIRoot (UIManager)
- EventSystem

## Component Reference Wiring

### SpawnManager

- Basic Enemy Prefab -> EnemyBasic prefab
- Zigzag Enemy Prefab -> EnemyZigzag prefab
- Tank Enemy Prefab -> EnemyTank prefab

### PlayerController

- Bullet Prefab -> PlayerBullet prefab
- Fire Point -> PlayerShip/FirePoint
- Shield Visual -> PlayerShip/ShieldVisual (optional)
- Explosion Prefab -> Explosion prefab (optional)

### EnemyController (per enemy prefab)

- Bullet Prefab -> EnemyBullet prefab
- Fire Point -> prefab child `FirePoint`
- PowerUp Prefabs -> [PowerUpHealth, PowerUpWeapon, PowerUpShield]
- Explosion Prefab -> Explosion prefab (optional)

### UIManager

Assign all panel and UI references from Canvas children:

- Main menu fields/buttons
- HUD texts + slider + icons
- Pause menu buttons
- Game over texts/buttons
- Wave banner panel and text

## Scene Defaults

- Game starts in Main Menu state.
- `Start` button transitions to active play and first wave.
- `Escape` pauses/resumes.
- Player death transitions to Game Over.
- `Restart` resets gameplay state and starts from wave 1.
