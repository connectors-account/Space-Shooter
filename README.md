# Space Shooter (Unity · C# · Windows Desktop)

A simple, fully playable 2D space-shooter for **Windows desktop**, built with **Unity + C#**.

- **Player:** move with WASD/arrow keys, shoot with Space.
- **Enemies:** spawn periodically from the top, move down, and shoot back.
- **Rules:** destroy enemies for points; you have 3 lives; run out → Game Over.
- **Screens:** Main Menu (Start/Quit) and Game Over (final score, Restart / Main Menu).
- **Extras:** single-layer scrolling background and shoot/explosion/game-over sound hooks.

## This is a Unity project (source), not a prebuilt .exe
The Unity Editor is required to compile the Windows executable (Unity is free). All C# code is
complete and included under `Assets/Scripts`. Scenes, prefabs and UI are assembled in the Editor.

➡️ **Follow [`SETUP.md`](./SETUP.md)** for exact, click-by-click steps to open the project,
wire everything up, and build `SpaceShooter.exe` for Windows.

## Scripts
| Script | Responsibility |
|---|---|
| `GameManager.cs` | Score, lives, game-over state |
| `PlayerController.cs` | Movement, shooting, damage/invulnerability |
| `Bullet.cs` | Player & enemy projectiles, collision routing |
| `EnemyController.cs` | Enemy movement, shooting, death + scoring |
| `EnemySpawner.cs` | Periodic enemy spawning from the top |
| `UIManager.cs` | HUD (score/lives) + Game Over panel, restart |
| `MainMenu.cs` | Start / Quit buttons |
| `ParallaxBackground.cs` | Single-layer scrolling background |
| `AudioManager.cs` | Shoot / explosion / game-over SFX |
| `SelfDestruct.cs` | Auto-cleanup for temporary effects |

All code is in the `SpaceShooter` namespace and uses only built-in Unity modules.
