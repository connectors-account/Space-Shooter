# Sprite Creation Guide (Simple Geometric Style)

Use this guide to create all visuals quickly without external art packs.

## A) Minimal Sprite Set

Create these PNG files (transparent background):

- `player_ship.png` (cyan triangle/arrow)
- `enemy_chaser.png` (red square)
- `enemy_zigzag.png` (orange square/diamond)
- `enemy_shooter.png` (purple square)
- `bullet_player.png` (green thin rectangle)
- `bullet_enemy.png` (magenta thin rectangle)
- `powerup_rapidfire.png` (yellow circle)
- `powerup_shield.png` (blue circle)
- `powerup_health.png` (green circle)
- `bg_stars_layer_1.png` (dark starfield)
- `bg_stars_layer_2.png` (slightly brighter sparse stars)

## B) Size Recommendations

- Ships/enemies: 64x64
- Bullets: 16x32
- Power-ups: 48x48
- Background layers: 512x512 tileable textures

## C) Import Settings in Unity

For each sprite:
- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Single`
- Pixels Per Unit: `100`
- Filter Mode: `Point` (retro) or `Bilinear` (smooth)
- Compression: `None` (optional for crisp clarity)

## D) Quick In-Unity Fallback (No external image editor)

If you need immediate placeholders:
1. Use Unity default white sprite (`Sprites/Default`) on a SpriteRenderer.
2. Set object color in SpriteRenderer:
   - Player cyan
   - Chaser red
   - ZigZag orange
   - Shooter purple
   - Player bullet green
   - Enemy bullet magenta
   - Powerups yellow/blue/green

This gives you fully playable visuals instantly.
