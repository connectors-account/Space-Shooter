# Space Shooter (Unity 2D, Windows)

Complete, playable Unity space shooter project using C#.

## Unity Version
- Recommended: **Unity 2022.3 LTS** (Built-in Render Pipeline)

## Implemented Features
- Player ship movement (`WASD` / Arrow keys)
- Shooting with bullet patterns (normal + rapid-fire spread)
- Wave-based enemy spawning with increasing difficulty
- Three enemy types with unique behavior:
  - Basic: straight movement + simple shots
  - Zigzag: sinusoidal movement + aimed shots
  - Tank: slower + burst fire
- Player health, damage, invulnerability window, death flow
- Scoring and high score persistence (`PlayerPrefs`)
- Power-ups:
  - Health restore
  - Rapid fire
  - Shield
- Parallax background + procedural star field
- UI flow:
  - Main Menu
  - In-game HUD (health, score, wave)
  - Pause panel
  - Game Over panel (score + restart/menu)
- Sound hooks using placeholder audio files:
  - `shoot.wav`, `explosion.wav`, `powerup.wav`, `player_hit.wav`, `wave_start.wav`, `game_over.wav`, `button_click.wav`

## Folder Structure

```text
Assets/
  Audio/
  Prefabs/
  Scenes/
  Scripts/
    Enemy/
    Environment/
    Managers/
    Player/
    PowerUps/
    UI/
    Utils/
    Weapons/
  Sprites/
ProjectSettings/
README.md
SETUP_GUIDE.md
```

## Main Scripts
- `Assets/Scripts/Managers/GameManager.cs`
- `Assets/Scripts/Managers/SpawnManager.cs`
- `Assets/Scripts/Managers/AudioManager.cs`
- `Assets/Scripts/Player/PlayerController.cs`
- `Assets/Scripts/Enemy/EnemyController.cs`
- `Assets/Scripts/Weapons/BulletController.cs`
- `Assets/Scripts/PowerUps/PowerUpController.cs`
- `Assets/Scripts/UI/UIManager.cs`
- `Assets/Scripts/Environment/ParallaxBackground.cs`
- `Assets/Scripts/Environment/StarField.cs`

## Scenes + Prefabs
- Scene configuration references are in:
  - `Assets/Scenes/SCENE_CONFIGURATION.md`
- Prefab setup references are in:
  - `Assets/Prefabs/PREFAB_SETUP.md`

## Build Windows .exe
Follow full instructions in `SETUP_GUIDE.md`.
