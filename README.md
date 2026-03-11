# Space Shooter Game

A classic arcade-style space shooter game built with Unity for Windows desktop.

![Space Shooter Banner](Docs/banner_placeholder.png)

## 🎮 Game Features

- **Wave-Based Combat**: Battle through 10 waves of increasingly difficult enemies
- **Multiple Enemy Types**: 5 unique enemy behaviors (Basic, Zigzag, Circular, Charger, Boss)
- **Power-Up System**: Collect shields, rapid fire, health, extra lives, and score bonuses
- **Scoring System**: Combo multipliers and high score tracking
- **Boss Battles**: Face challenging boss enemies every 5 waves
- **Parallax Scrolling**: Immersive starfield background
- **Full Audio Support**: Music and sound effects (bring your own assets)

## 🎯 How to Play

### Controls

| Action | Key |
|--------|-----|
| Move Up | W / ↑ Arrow |
| Move Down | S / ↓ Arrow |
| Move Left | A / ← Arrow |
| Move Right | D / → Arrow |
| Shoot | Space |
| Pause | Escape |

### Objective

- Destroy all enemies in each wave to progress
- Collect power-ups to gain advantages
- Survive all 10 waves to achieve victory
- Aim for the highest score possible!

### Power-Ups

| Power-Up | Color | Effect |
|----------|-------|--------|
| Shield | Cyan | Temporary invincibility |
| Rapid Fire | Yellow | Increased fire rate |
| Health | Green | Restore health |
| Extra Life | Magenta | Gain an extra life |
| Score Bonus | White | Instant score boost |

### Tips

- Keep moving to avoid enemy bullets
- Prioritize Charger enemies - they track your position
- Save your lives for boss battles
- Build combos by defeating enemies quickly for higher scores

## 📁 Project Structure

```
space_shooter_game/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # GameManager, ObjectPooler
│   │   ├── Player/         # PlayerController, PlayerShield
│   │   ├── Enemy/          # EnemyBase, BossEnemy, EnemySpawner
│   │   ├── Combat/         # Bullet, HealthSystem, DamageOnContact
│   │   ├── Systems/        # WaveSpawner, ScoreManager, PowerUp, Parallax
│   │   ├── UI/             # UIManager, MainMenu, HealthBarUI
│   │   └── Audio/          # AudioManager, SoundEffectPlayer
│   ├── Prefabs/            # Game object prefabs
│   ├── Sprites/            # 2D sprite assets
│   ├── Scenes/             # MainMenu, GameScene
│   ├── Audio/              # Music and sound effects
│   └── Materials/          # Materials (optional)
├── ProjectSettings/        # Unity project settings
├── Docs/                   # Documentation
│   ├── PREFAB_DEFINITIONS.md
│   ├── ASSET_GUIDELINES.md
│   ├── SCENE_SETUP.md
│   └── BUILD_INSTRUCTIONS.md
└── README.md
```

## 🚀 Quick Start

### Prerequisites

- Unity 2021.3 LTS or newer (2022.3 LTS recommended)
- Windows Build Support module installed
- TextMeshPro package (install via Package Manager)

### Setup Steps

1. **Create Unity Project**
   - Open Unity Hub
   - Create new 2D project
   - Name it "SpaceShooter"

2. **Import Scripts**
   - Copy all `.cs` files from `Assets/Scripts/` to your Unity project
   - Maintain the folder structure

3. **Configure Project**
   - Add required Tags: Player, Enemy, PlayerBullet, EnemyBullet, PowerUp
   - Add Layers: Player, Enemies, PlayerBullets, EnemyBullets, PowerUps
   - Configure Physics 2D collision matrix
   - Add Sorting Layers: Background, Stars, Projectiles, Pickups, Characters, Effects

4. **Create Assets**
   - Create or import sprites (see `Docs/ASSET_GUIDELINES.md`)
   - Create prefabs (see `Docs/PREFAB_DEFINITIONS.md`)

5. **Set Up Scenes**
   - Create MainMenu and GameScene (see `Docs/SCENE_SETUP.md`)

6. **Build**
   - File > Build Settings
   - Add both scenes (MainMenu first)
   - Select Windows platform
   - Click Build

For detailed instructions, see `Docs/BUILD_INSTRUCTIONS.md`.

## 📄 Documentation

| Document | Description |
|----------|-------------|
| [PREFAB_DEFINITIONS.md](Docs/PREFAB_DEFINITIONS.md) | Detailed prefab specifications |
| [ASSET_GUIDELINES.md](Docs/ASSET_GUIDELINES.md) | Sprite and audio asset specs |
| [SCENE_SETUP.md](Docs/SCENE_SETUP.md) | Scene configuration guide |
| [BUILD_INSTRUCTIONS.md](Docs/BUILD_INSTRUCTIONS.md) | Step-by-step build guide |

## 🎨 Creating Your Own Assets

### Sprites

You can create simple sprites using:
- Unity's built-in sprite creator
- Free tools like Aseprite, GIMP, or Piskel
- Free asset packs from Kenney.nl or OpenGameArt.org

### Audio

Find free audio at:
- Freesound.org
- OpenGameArt.org
- Incompetech.com (royalty-free music)

## 🛠️ Customization

### Adjusting Difficulty

In `WaveSpawner.cs`:
```csharp
public int totalWaves = 10;           // Number of waves
public float difficultyMultiplier = 1.1f;  // Enemy count increase per wave
public int baseEnemyCount = 5;        // Starting enemies per wave
```

### Adjusting Player

In `PlayerController.cs`:
```csharp
public float moveSpeed = 8f;          // Movement speed
public float fireRate = 0.2f;         // Seconds between shots
public float rapidFireRate = 0.1f;    // Fire rate with power-up
```

### Adjusting Power-Ups

In `PowerUpSpawner.cs`:
```csharp
public float spawnChance = 0.15f;     // 15% chance on enemy death
public float shieldWeight = 20f;       // Relative spawn weights
public float rapidFireWeight = 25f;
public float healthWeight = 30f;
public float extraLifeWeight = 10f;
public float scoreBonusWeight = 15f;
```

## 🎯 Game Flow

```
MainMenu
    │
    ├── Play → GameScene
    │              │
    │              ├── Wave 1-4 (Basic enemies)
    │              │
    │              ├── Wave 5 (Boss battle)
    │              │
    │              ├── Wave 6-9 (Mixed enemies)
    │              │
    │              ├── Wave 10 (Final boss)
    │              │
    │              ├── Victory → VictoryScreen
    │              │
    │              └── Game Over → GameOverScreen
    │                                    │
    │                                    ├── Restart → GameScene
    │                                    │
    │                                    └── Main Menu → MainMenu
    │
    └── Quit → Exit Game
```

## 🔧 Technical Details

### Architecture

- **Singleton Pattern**: GameManager, ScoreManager, AudioManager
- **Object Pooling**: Bullets and enemies for performance
- **Event System**: Decoupled communication between systems
- **State Machine**: Game states (MainMenu, Playing, Paused, GameOver, Victory)

### Collision System

- Uses Unity's 2D physics with trigger colliders
- Layer-based collision filtering for performance
- Player has invincibility frames after taking damage

### Performance

- Object pooling prevents garbage collection spikes
- Background parallax uses efficient transform manipulation
- Enemies and bullets automatically deactivate when off-screen

## 📝 System Requirements

- **OS**: Windows 7/8/10/11 (64-bit)
- **Processor**: 1.5 GHz or faster
- **Memory**: 2 GB RAM
- **Graphics**: DirectX 11 compatible
- **Storage**: 100 MB available space

## 📜 License

This project is provided as-is for educational purposes. Feel free to modify and use it for your own projects.

## 🤝 Contributing

Feel free to fork this project and submit improvements:
- Bug fixes
- New enemy types
- Additional power-ups
- Visual effects
- Sound design

## 📞 Support

If you encounter issues:
1. Check the documentation files in `Docs/`
2. Verify Unity version compatibility
3. Ensure all prefab references are assigned
4. Check the Console for error messages

---

**Happy Gaming! 🚀👾**
