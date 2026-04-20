# Unity Space Shooter (Windows Desktop)

A complete 2D top-down space shooter project structure for Unity, with production-ready C# gameplay scripts and setup instructions.

## Implemented Systems

- Player movement (WASD + Arrow keys) and shooting
- Player health, temporary invulnerability, shield, rapid-fire weapon upgrade
- Enemy waves with 3 enemy types:
  - Basic (straight + down-fire)
  - Zigzag (sine movement + aimed shots)
  - Tank (high HP + spread burst fire)
- Wave progression and adaptive spawn intervals
- Combat + collisions for player, enemies, bullets, and power-ups
- Power-ups: health restore, weapon upgrade (rapid fire), shield
- Scoring + combo multiplier + persistent high score (PlayerPrefs)
- UI flow: main menu, HUD, pause menu, game over
- Background systems: parallax scrolling + procedural starfield
- Explosion VFX helper
- Audio manager with placeholder clip references

---

## Project Structure

```text
Space-Shooter/
├── Assets/
│   ├── Animations/
│   ├── Audio/
│   ├── Materials/
│   ├── Prefabs/
│   ├── Scenes/
│   │   ├── .gitkeep
│   │   └── SCENE_CONFIGURATION.md
│   ├── Scripts/
│   │   ├── Enemy/
│   │   │   └── EnemyController.cs
│   │   ├── Environment/
│   │   │   ├── ParallaxBackground.cs
│   │   │   └── StarField.cs
│   │   ├── Managers/
│   │   │   ├── AudioManager.cs
│   │   │   ├── GameManager.cs
│   │   │   └── SpawnManager.cs
│   │   ├── Player/
│   │   │   └── PlayerController.cs
│   │   ├── PowerUps/
│   │   │   └── PowerUpController.cs
│   │   ├── UI/
│   │   │   └── UIManager.cs
│   │   ├── Utils/
│   │   │   ├── AutoDestroy.cs
│   │   │   ├── ExplosionEffect.cs
│   │   │   └── ScreenBounds.cs
│   │   ├── Weapons/
│   │   │   └── BulletController.cs
│   │   └── Data/
│   └── Sprites/
├── ProjectSettings/
├── README.md
└── SETUP_GUIDE.md
```

---

## Quick Start

1. Open this folder in Unity Hub (recommended: Unity 2022.3 LTS or newer).
2. Follow **SETUP_GUIDE.md** to wire all prefabs, scene objects, and UI references.
3. Open **File > Build Settings**, select **Windows x86_64**, and build.

---

## Controls

- Move: **WASD** or **Arrow Keys**
- Shoot: **Spacebar** (and optional left mouse button support)
- Pause/Resume: **Esc**

---

## Build Output

Unity will generate:

- `Space Shooter.exe`
- `Space Shooter_Data/`
- `UnityPlayer.dll`

Ship the full build folder together (not only the `.exe`).
