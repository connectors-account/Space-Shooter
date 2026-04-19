# Space Shooter (Unity, Windows Desktop)

A complete, simple 2D space-shooter Unity project targeting **Windows standalone builds**.

## Unity Version

- Recommended: **Unity 2022.3 LTS** (tested structure for `2022.3.51f1`)
- Scripting backend and project setup are standard for 2D desktop games.

## Project Structure

```text
Space-Shooter/
├── Assets/
│   ├── Audio/
│   ├── Sprites/
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Materials/
│   ├── Animations/
│   ├── ScriptableObjects/
│   └── Scripts/
│       ├── Audio/
│       │   └── SoundManager.cs
│       ├── Combat/
│       │   ├── Bullet.cs
│       │   ├── CollisionHandler.cs
│       │   └── Damageable.cs
│       ├── Core/
│       │   └── GameManager.cs
│       ├── Enemy/
│       │   ├── EnemyAI.cs
│       │   └── EnemySpawner.cs
│       ├── Environment/
│       │   └── ParallaxScroller.cs
│       ├── Input/
│       │   └── InputHandler.cs
│       ├── Player/
│       │   ├── PlayerController.cs
│       │   └── PlayerHealth.cs
│       ├── Systems/
│       │   ├── PowerUpSystem.cs
│       │   └── ScoreSystem.cs
│       └── UI/
│           └── UIManager.cs
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
└── ProjectSettings/
    ├── InputManager.asset
    ├── Physics2DSettings.asset
    ├── ProjectSettings.asset
    ├── ProjectVersion.txt
    └── TagManager.asset
```

## Implemented Systems

All requested systems are fully implemented in C#:

1. Player controller with movement and shooting (`PlayerController.cs`)
2. Enemy spawner with wave progression (`EnemySpawner.cs`)
3. Enemy AI with multiple patterns (`EnemyAI.cs`)
4. Bullet system for player/enemies (`Bullet.cs`)
5. Collision and damage handling (`CollisionHandler.cs`, `Damageable.cs`)
6. Health system for player (`PlayerHealth.cs`)
7. Scoring system (`ScoreSystem.cs`)
8. Power-up system: shield, rapid fire, health restore (`PowerUpSystem.cs`)
9. Background parallax scrolling (`ParallaxScroller.cs`)
10. Game state manager (`GameManager.cs`)
11. UI manager for all screens (`UIManager.cs`)
12. Sound manager (`SoundManager.cs`)
13. Input handler for keyboard controls (`InputHandler.cs`)

## How to Open in Unity

1. Open **Unity Hub**.
2. Click **Open**.
3. Select this folder:
   - `/home/ubuntu/space_shooter_unity` (deliverable copy)
   - or your cloned repo folder `Space-Shooter`.
4. Open the project with **Unity 2022.3 LTS**.
5. Let Unity finish initial import.

## Minimal Scene Setup (Required Once)

Create a scene (e.g., `Assets/Scenes/Main.unity`) and wire these objects:

- **Managers**
  - `GameManager` (attach `GameManager.cs`)
  - `InputHandler` (attach `InputHandler.cs`)
  - `ScoreSystem` (attach `ScoreSystem.cs`)
  - `PowerUpSystem` (attach `PowerUpSystem.cs`)
  - `SoundManager` + `AudioSource` (attach `SoundManager.cs`)
  - `EnemySpawner` (attach `EnemySpawner.cs`)
- **Player**
  - Sprite + Collider2D + Rigidbody2D (kinematic)
  - Attach: `PlayerController`, `PlayerHealth`, `CollisionHandler`, `FactionAffiliation`
  - Add child `FirePoint` transform.
- **Enemy Prefab**
  - Sprite + Collider2D + Rigidbody2D (kinematic)
  - Attach: `EnemyAI`, `Damageable`, `CollisionHandler`, `FactionAffiliation`
  - Add child `FirePoint` transform.
- **Bullet Prefabs**
  - Player bullet and enemy bullet with Collider2D trigger + `Bullet` script.
- **Power-up Prefabs**
  - 3 prefabs with `PowerUpPickup` set to Shield / RapidFire / HealthRestore.
- **UI Canvas**
  - Main Menu panel
  - HUD panel (health, score, wave)
  - Pause panel
  - Game Over panel (final score + restart)
  - Hook buttons to `UIManager` public methods.
- **Background**
  - Add two layers and set references in `ParallaxScroller`.

## Build for Windows (Standalone Executable)

1. In Unity: **File → Build Settings**.
2. Platform: **PC, Mac & Linux Standalone**.
3. Target: **Windows**, Architecture **x86_64**.
4. Add your scene(s) to **Scenes In Build**.
5. In **Player Settings** set product name (e.g., `Space Shooter`).
6. Click **Build** and choose an output folder.
7. Run the generated `.exe` from the build output folder.

## Controls

- Move: **WASD** or **Arrow Keys**
- Shoot: **Space**
- Pause/Resume: **Esc**

## Gameplay Summary

- Defeat waves of enemies.
- Waves increase enemy count, speed, and pressure.
- Collect power-ups:
  - **Shield**: temporary invulnerability
  - **Rapid Fire**: reduced shoot cooldown
  - **Health Restore**: heals player
- Score increases per enemy destroyed.
- Game ends when player health reaches zero.

## Notes

- This is a Unity desktop project, not a web app.
- The game logic is intentionally simple and clean for easy extension.
