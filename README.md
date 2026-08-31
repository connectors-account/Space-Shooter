# Star Void — Space Shooter

A Windows desktop space shooter game built with **C# + WinForms** (no game engine required).

## Files
- `Game.cs` — Full game code (890 lines): player, enemies, bullets, power-ups, waves, particles, HUD, menus
- `StarVoid.csproj` — .NET 8 project file

## How to Build & Run

**Requirements:** [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
# Quick run
dotnet run

# Build Windows .exe
dotnet publish -c Release -r win-x64 --self-contained true
# Output: bin/Release/net8.0-windows/win-x64/publish/StarVoid.exe
```

## Controls
| Key | Action |
|-----|--------|
| WASD / Arrow Keys | Move |
| SPACE or Z | Fire |
| ESC | Pause / Resume |
| R (paused) | Restart |
| M (paused) | Main Menu |

## Features
- 3 enemy types (Scout, Fighter, Tank) with sine-wave movement and aimed fire
- Unlimited progressive waves, increasing difficulty
- 5 power-up types: Double Shot, Triple Shot, Shield, Heal, Bomb
- Particle explosion system, animated star field parallax
- Full HUD: score, health bar, wave counter, power-up timer
- Main menu, pause screen, game-over screen with high score
