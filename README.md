# Space Shooter (Unity Windows Desktop)

A complete retro-style 2D arcade space shooter built in Unity (C#), targeting **Windows desktop**.

## Implemented Features

- Player spaceship movement with **WASD / Arrow keys**
- Shooting with **Spacebar** or **Left Mouse Button**
- Enemy wave progression with increasing difficulty
- Multiple enemy types: **Basic, Zigzag, Tank, Spinner**
- Projectile system + enemy bullet patterns (single, aimed, radial spread)
- Full collision detection/handling (player, enemy, bullets, power-ups)
- Score + persistent high-score (`PlayerPrefs`)
- Player health system
- Power-ups:
  - Rapid Fire
  - Shield
  - Health Restore
  - Spread Shot
- Background parallax scrolling
- Sound integration via generated retro SFX (`AudioManager`)
- Complete UI flow:
  - Main Menu scene
  - In-game HUD
  - Pause menu
  - Game Over scene
- Object pooling for enemies, bullets, and power-ups

---

## Project Structure

```
Assets/
  Editor/
    ProjectAutoBuilder.cs
  Scenes/
    MainMenu.unity
    GamePlay.unity
    GameOver.unity
  Prefabs/
    Player.prefab
    EnemyBasic.prefab
    EnemyZigzag.prefab
    EnemyTank.prefab
    EnemySpinner.prefab
    PlayerBullet.prefab
    EnemyBullet.prefab
    PowerRapid.prefab
    PowerShield.prefab
    PowerHealth.prefab
    PowerSpread.prefab
  Scripts/
    Core/
      GameSession.cs
      PoolManager.cs
    Gameplay/
      GameBootstrap.cs
      PlayerController.cs
      EnemyController.cs
      EnemySpawner.cs
      Projectile.cs
      PowerUpPickup.cs
      ParallaxScroller.cs
    Managers/
      AudioManager.cs
    UI/
      MainMenuController.cs
      GameUIController.cs
      GameOverController.cs
  Sprites/
    (retro generated sprite assets)
  Audio/
    (folder ready for custom audio replacement)
ProjectSettings/
  ProjectSettings.asset
  InputManager.asset
  TagManager.asset
  Physics2DSettings.asset
  ProjectVersion.txt
```

> `ProjectAutoBuilder.cs` ensures scenes/prefabs/sprites exist and also provides:
> **Tools → Space Shooter → Regenerate Project Content**

---

## Unity Version

- Recommended: **Unity 2022.3 LTS** (configured via `ProjectVersion.txt`)

---

## How to Open the Project

1. Open **Unity Hub**.
2. Click **Open**.
3. Select this folder (repo root):
   - `Space-Shooter`
4. Let Unity import scripts/assets.
5. If needed, run:
   - **Tools → Space Shooter → Regenerate Project Content**
6. Open scene:
   - `Assets/Scenes/MainMenu.unity`

---

## Build as Windows `.exe`

1. In Unity, open **File → Build Settings**.
2. Confirm scenes are listed in order:
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/GamePlay.unity`
   - `Assets/Scenes/GameOver.unity`
3. Platform: **PC, Mac & Linux Standalone**
4. Target Platform: **Windows**
5. Architecture: **x86_64**
6. Click **Build** (or **Build And Run**)
7. Choose an output folder (e.g. `Builds/Windows`)
8. Run generated executable:
   - `Space Shooter.exe`

---

## Controls

- **Move**: `W A S D` or Arrow keys
- **Fire**: `Spacebar` or Left Mouse Button
- **Pause/Resume**: `Esc`

---

## Gameplay Notes

- Survive enemy waves; each wave spawns more and tougher enemies.
- Defeat enemies for score.
- Collect rotating power-ups for temporary buffs or health.
- On death, Game Over scene shows score/wave/high score.

---

## Customization Quick Points

- Enemy/wave pacing: `Assets/Scripts/Gameplay/EnemySpawner.cs`
- Player tuning (speed/fire/health): `Assets/Scripts/Gameplay/PlayerController.cs`
- Projectile behavior: `Assets/Scripts/Gameplay/Projectile.cs`
- Audio: `Assets/Scripts/Managers/AudioManager.cs`

---

## Production Notes

- No TODO placeholders in runtime scripts.
- Gameplay systems are fully implemented in C#.
- Pooling is used for performance-sensitive objects.
- UI and game flow are scene-driven and desktop-focused.
