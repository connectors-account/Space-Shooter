# Scene Setup Instructions

The game is designed to be **self-bootstrapping** via the `GameSetup.cs` script.

## Quick Setup (Recommended)

1. Open Unity and create a new **2D** project (or open this project folder)
2. Create a new Scene (File → New Scene → Basic 2D)
3. Save it as `Assets/Scenes/MainScene.unity`
4. Create an **empty GameObject** in the scene:
   - Right-click in Hierarchy → Create Empty
   - Name it `GameSetup`
5. Drag the `GameSetup.cs` script onto the `GameSetup` GameObject
6. **That's it!** — The GameSetup script automatically creates all other objects at runtime:
   - Player ship
   - GameManager
   - UIManager
   - AudioManager
   - EnemySpawner
   - ParallaxBackground

## What Gets Created Automatically

| Object | Components | Purpose |
|--------|-----------|---------|
| Player | SpriteRenderer, BoxCollider2D, Rigidbody2D, PlayerController | Player ship |
| GameManager | GameManager | Game state, scoring, flow |
| UIManager | UIManager | All UI panels/buttons |
| AudioManager | AudioManager + AudioSources | Sound effects |
| EnemySpawner | EnemySpawner | Wave-based enemy spawning |
| ParallaxBackground | ParallaxBackground | Scrolling starfield |

## Required Tags

Make sure these tags exist in Project Settings → Tags & Layers:
- `Player` (built-in)
- `Enemy`
- `PlayerBullet`
- `EnemyBullet`
- `PowerUp`

The `TagManager.asset` file in ProjectSettings already defines these.
