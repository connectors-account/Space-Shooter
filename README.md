# 🚀 Space Shooter

A complete arcade-style space shooter game built with Unity and C#.

## Features
- **Wave-based enemy spawning** with increasing difficulty
- **3 enemy types**: Basic (red), Fast (yellow), Tank (purple)
- **5 movement patterns**: Straight, Zigzag, Sine, Dive, Circle
- **4 shoot patterns**: None, Single, Spread, Aimed
- **4 power-ups**: Health, Weapon Upgrade, Shield, Speed Boost
- **3 weapon levels**: Single → Double → Triple spread
- **Full UI**: Main Menu, HUD, Pause Menu, Game Over Screen
- **Procedural sprites**: No external assets needed
- **Parallax scrolling** star background
- **Sound system** with integration points

## Quick Start
1. Install Unity 2021.3+ with Windows Build Support
2. Create a new 2D project
3. Copy the `Assets/Scripts/` folder into your project
4. Add tags: `PlayerBullet`, `EnemyBullet`, `PowerUp`
5. Create `MainMenu` scene with `MainMenuSetup` component
6. Create `GameScene` scene with `GameSetup` component
7. Build and play!

See [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) for detailed setup guide.

## Controls
| Key | Action |
|-----|--------|
| WASD / Arrows | Move |
| Space | Shoot |
| Escape | Pause |

## License
Free to use for any purpose.
