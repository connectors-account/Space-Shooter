# Sprite Assets Guide

## Sprite Organization

The game generates basic geometric sprites at runtime, but you can replace them with custom sprites.

### Player Folder (Assets/Sprites/Player/)
- `player_ship.png` - Main player ship sprite (64x64 recommended)
- `player_shield.png` - Shield effect overlay (128x128, semi-transparent)

### Enemies Folder (Assets/Sprites/Enemies/)
- `enemy_basic.png` - Basic enemy ship (32x32)
- `enemy_zigzag.png` - ZigZag enemy (32x32)
- `enemy_dive.png` - Dive bomber enemy (32x32)
- `enemy_boss.png` - Boss enemy (128x128)

### Bullets Folder (Assets/Sprites/Bullets/)
- `bullet_player.png` - Player bullet (8x16)
- `bullet_enemy.png` - Enemy bullet (8x8)
- `bullet_boss.png` - Boss special bullet (16x16)

### Powerups Folder (Assets/Sprites/Powerups/)
- `powerup_health.png` - Health restore (24x24)
- `powerup_shield.png` - Shield power-up (24x24)
- `powerup_rapidfire.png` - Rapid fire power-up (24x24)
- `powerup_tripleshot.png` - Triple shot power-up (24x24)

### Background Folder (Assets/Sprites/Background/)
- `star_small.png` - Small star (4x4)
- `star_large.png` - Large star (8x8)
- `nebula.png` - Background nebula (optional)

### UI Folder (Assets/Sprites/UI/)
- `health_bar_bg.png` - Health bar background
- `health_bar_fill.png` - Health bar fill
- `life_icon.png` - Life indicator icon

## Sprite Import Settings

For 2D sprites:
1. Select sprite in Project window
2. In Inspector:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Single
   - Pixels Per Unit: 32 (or 64 for larger sprites)
   - Filter Mode: Point (no filter) for pixel art
   - Compression: None for pixel art

## Free Sprite Resources

- [Kenney.nl](https://kenney.nl/assets) - Free game assets
- [OpenGameArt.org](https://opengameart.org) - Community assets
- [itch.io](https://itch.io/game-assets/free) - Free game assets
