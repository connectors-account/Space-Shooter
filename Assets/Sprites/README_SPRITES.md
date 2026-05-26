# Sprite Assets Guide

## Required Sprites

Place the following sprite files in this folder (`Assets/Sprites/`):

### Player Ship
- **player_ship.png** — 64×64 or 128×128 px, transparent background
  - A top-down spaceship facing upward
  - Suggested: triangular/arrow shape, blue/cyan tones

### Enemies
- **enemy_basic.png** — 64×64 px, red/orange tones
- **enemy_zigzag.png** — 64×64 px, green tones  
- **enemy_dive.png** — 64×64 px, purple tones

### Bullets
- **bullet_player.png** — 8×16 px, cyan/blue elongated glow
- **bullet_enemy.png** — 8×16 px, red/orange elongated glow

### Power-Ups
- **powerup_weapon.png** — 32×32 px, orange icon (W)
- **powerup_shield.png** — 32×32 px, blue icon (S)
- **powerup_health.png** — 32×32 px, green icon (+)
- **powerup_rapid.png** — 32×32 px, yellow icon (R)
- **powerup_score.png** — 32×32 px, magenta icon ($)

### Background
- **space_bg.png** — 512×1024 px (tileable vertically), starfield

## Quick Creation Options

### Option A: Unity Primitives (No External Art Needed)
The scripts use `SpriteRenderer` with solid-color squares. Unity's built-in
white square sprite works fine — the scripts tint them via code:
1. In Unity: right-click Sprites folder → Create → Sprites → Square
2. Assign these to prefabs. Scripts will color them automatically.

### Option B: Free Asset Packs
- **Kenney.nl** → "Space Shooter Redux" (CC0 license, free)
- **OpenGameArt.org** → Search "space shooter sprites"
- **Unity Asset Store** → Search "2D space shooter" (many free packs)

### Import Settings (for all sprites)
1. Select sprite in Unity Inspector
2. Set **Texture Type** = Sprite (2D and UI)
3. Set **Pixels Per Unit** = 100 (or 64 for pixel art)
4. Set **Filter Mode** = Point (for pixel art) or Bilinear (for smooth)
5. Click **Apply**
