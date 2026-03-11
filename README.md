# Space Shooter Game

A complete space shooter game built with C# and Raylib-cs framework. Features include player ship with movement and shooting, multiple enemy types with wave progression, power-ups, collision detection, scoring system, and menu screens.

## Features

- **Player Ship**: Move in all directions, shoot projectiles, collect power-ups
- **Enemy Types**:
  - Basic: Standard enemy, moves straight down
  - Fast: Quick small enemy with diagonal movement
  - Zigzag: Moves in a zigzag pattern
  - Shooter: Aims and shoots at the player
  - Tank: Large enemy with high health and spread shot
- **Power-Ups**:
  - Health: Restores 2 HP
  - Shield: Blocks one hit
  - Speed Boost: Increases movement speed
  - Rapid Fire: Faster shooting
  - Multi-Shot: Shoots 3 bullets at once
- **Wave System**: Progressive difficulty with more and harder enemies each wave
- **Visual Effects**: Parallax starfield background, particles, explosions
- **Sound Effects**: Procedurally generated sounds for shooting, explosions, hits, and power-ups
- **Menu System**: Main menu, pause menu, and game over screen

## Prerequisites

### 1. Install .NET SDK 8.0 or later

**Windows:**
1. Download the .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run the installer and follow the prompts
3. Restart your terminal/command prompt after installation

**Verify installation:**
```bash
dotnet --version
```
You should see version 8.0.x or higher.

## Building the Game

### Option 1: Run Directly (for development/testing)

1. Open a terminal/command prompt
2. Navigate to the game directory:
   ```bash
   cd path/to/space_shooter_game
   ```
3. Restore packages and run:
   ```bash
   dotnet run
   ```

### Option 2: Build Standalone Windows Executable

This creates a self-contained .exe that doesn't require .NET to be installed on the target machine.

1. Open a terminal/command prompt
2. Navigate to the game directory:
   ```bash
   cd path/to/space_shooter_game
   ```
3. Publish the game:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
   ```
4. The executable will be in the `./publish` folder:
   - `SpaceShooter.exe`

### Build for Other Platforms

**Linux (x64):**
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish-linux
```

**macOS (x64):**
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -o ./publish-mac
```

**macOS (ARM/M1/M2):**
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -o ./publish-mac-arm
```

## Running the Game

### From Source
```bash
dotnet run
```

### From Published Executable (Windows)
Double-click `SpaceShooter.exe` in the `publish` folder, or run from command line:
```bash
cd publish
.\SpaceShooter.exe
```

## Controls

| Action | Keys |
|--------|------|
| Move Up | W / Up Arrow |
| Move Down | S / Down Arrow |
| Move Left | A / Left Arrow |
| Move Right | D / Right Arrow |
| Shoot | Space / Left Mouse Button |
| Pause | P / Escape |
| Select/Confirm | Enter / Space |
| Return to Main Menu | M (when paused/game over) |

## Gameplay Tips

1. **Stay mobile**: Keep moving to avoid enemy bullets
2. **Prioritize threats**: Take out Shooters and Tanks first
3. **Collect power-ups**: They can turn the tide of battle
4. **Shield is valuable**: Save it for tough situations
5. **Multi-shot + Rapid Fire**: A deadly combination!

## Project Structure

```
space_shooter_game/
├── SpaceShooter.csproj    # Project file with dependencies
├── Program.cs             # Entry point
├── Game.cs                # Main game logic and state management
├── Player.cs              # Player ship with movement, shooting, power-ups
├── Enemy.cs               # Enemy types and behaviors
├── Bullet.cs              # Bullet class for player and enemy projectiles
├── PowerUp.cs             # Power-up items and effects
├── Particle.cs            # Particle effects for explosions
├── ICollidable.cs         # Interface for collision detection
├── CollisionSystem.cs     # Collision detection logic
├── EnemySpawner.cs        # Wave system and enemy spawning
├── ParallaxBackground.cs  # Scrolling starfield background
├── SoundManager.cs        # Procedural sound generation
├── UIManager.cs           # Menus, HUD, and UI elements
└── README.md              # This file
```

## Troubleshooting

### "dotnet" is not recognized
Make sure .NET SDK is installed and your PATH is updated. Try restarting your terminal.

### Build errors about Raylib-cs
The first build will automatically download the Raylib-cs package. Make sure you have internet connection.

### Game window doesn't open
Check if your graphics drivers are up to date. Raylib requires OpenGL 3.3 or higher.

### No sound
The game uses procedurally generated sounds through Raylib's audio system. Make sure your audio device is working.

## License

This game is provided as-is for educational and entertainment purposes.

## Credits

- Built with [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) - C# bindings for Raylib
- [Raylib](https://www.raylib.com/) - A simple and easy-to-use library for game development
