# Space Shooter (Unity, Windows Desktop)

Simple, fully functional **2D space-shooter** Unity project for Windows build target.

## Included Core Scripts

Located in `Assets/Scripts/`:

- `PlayerController.cs` - WASD/Arrow movement + Space shooting + player damage/death
- `EnemyController.cs` - enemy movement, optional enemy shooting, score value, death
- `BulletController.cs` - bullet movement and auto-destroy behavior
- `GameManager.cs` - score state, pause, game over, restart logic
- `UIManager.cs` - score/health text, game-over panel, pause panel
- `EnemySpawner.cs` - timed enemy spawning + gradual difficulty increase

## Project Structure

```text
Space-Shooter/
├── Assets/
│   ├── Audio/
│   ├── Materials/
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Scripts/
│   │   ├── BulletController.cs
│   │   ├── EnemyController.cs
│   │   ├── EnemySpawner.cs
│   │   ├── GameManager.cs
│   │   ├── PlayerController.cs
│   │   └── UIManager.cs
│   ├── Sprites/
│   └── UI/
├── ProjectSettings/
├── SETUP_GUIDE.md
└── README.md
```

## Controls

- Move: `WASD` or `Arrow keys`
- Shoot: `Space`
- Pause/Resume: `Esc`
- Restart after game over: `R`

## Build Instructions

See **[SETUP_GUIDE.md](./SETUP_GUIDE.md)** for full step-by-step setup and Windows `.exe` build instructions.
