=== SPRITE PLACEHOLDER INFORMATION ===

This game generates all sprites procedurally at runtime via SpriteGenerator.cs.
No external sprite files are required to play the game.

However, if you want to replace the procedural sprites with custom art:

1. Player Ship:  Place a 64x64 or 128x128 PNG here named "player_ship.png"
2. Enemy Ship:   Place a 64x64 PNG here named "enemy_ship.png"
3. Bullet:       Place a 16x32 PNG here named "bullet.png"
4. Power-Up:     Place a 32x32 PNG here named "powerup.png"
5. Shield:       Place a 64x64 PNG with transparency named "shield.png"
6. Background:   Place a 512x512 tileable PNG named "background.png"

To use custom sprites:
- Import them into Unity (drag into this folder)
- Set Texture Type to "Sprite (2D and UI)"
- Set Pixels Per Unit to match the size (e.g., 64 for 64x64)
- Assign them to the appropriate components in the Inspector

The RuntimeSceneBuilder.cs and SpriteGenerator.cs handle all visual
setup automatically when no custom sprites are provided.
