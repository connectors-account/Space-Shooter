# Space Shooter - Windows Desktop Game

A complete classic arcade-style space shooter built with **C# + WinForms + GDI+**.
No Unity, no external NuGet packages — just the .NET base class libraries.

## Requirements
- .NET 6 SDK: https://dotnet.microsoft.com/download/dotnet/6.0
- Windows 10/11

## Build & Run

### Option 1: Run directly
```bash
cd SpaceShooter
dotnet run
```

### Option 2: Build a standalone Windows EXE
```bash
cd SpaceShooter
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```
The standalone `.exe` will be in `./publish/SpaceShooter.exe`.

## Controls
- **WASD** or **Arrow Keys**: Move ship
- **SPACE**: Shoot
- **ESC**: Quit
- **ENTER**: Start game (from menu)
- **R**: Restart (from game over screen)

## Power-Ups
- **Yellow (R)**: Rapid Fire - 5 seconds
- **Cyan (T)**: Triple Shot - 5 seconds
- **Blue (S)**: Shield - 5 seconds
- **Green (B)**: Speed Boost - 5 seconds

## Gameplay
- Survive waves of enemies.
- Each wave gets harder — more enemies and tougher types (Basic, Fast, Tank).
- Don't let enemies reach you or shoot you.
- Collect power-ups for temporary advantages.
- You have 5 HP — the game ends when HP reaches 0.
- Clearing a wave awards a bonus of `500 x wave number`.

## Enemy Types
- **Basic** (OrangeRed): 1 HP, moderate speed, 100 points.
- **Fast** (Yellow): 1 HP, fast with sine-wave horizontal drift, 150 points.
- **Tank** (Purple): 4 HP, slow but tough, shows a health bar, 300 points.

## Project Structure
```
SpaceShooter/
├── SpaceShooter.csproj   # .NET 6 Windows Forms project
├── Program.cs            # WinForms entry point
├── GameForm.cs           # Game loop, rendering, input, waves, HUD
├── GameObjects.cs        # Star, Bullet, PowerUp, Enemy, Player + enums
└── README.md
```
