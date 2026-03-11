# Asset Guidelines

This document provides specifications for creating or sourcing assets for the Space Shooter game.

## Sprite Specifications

### General Guidelines
- **Format:** PNG with transparency
- **Color Mode:** RGBA
- **Pixels Per Unit:** 100 (default Unity setting)
- **Filter Mode:** Point (for pixel art) or Bilinear (for smooth graphics)
- **Compression:** None or Low Quality for best results

---

## Player Assets

### Player Ship (`player_ship.png`)
- **Size:** 64x64 pixels or 128x128 pixels
- **Design:** Triangle or classic spaceship shape pointing upward
- **Colors:** Blue/cyan primary with white/silver accents
- **Style:** Your choice - pixel art or smooth vector style

**Simple Placeholder:**
Create a 64x64 image with:
- Isoceles triangle pointing up
- Fill: #00AAFF (cyan blue)
- Outline: #FFFFFF (white), 2px

### Player Bullet (`bullet_player.png`)
- **Size:** 8x16 pixels or 16x32 pixels
- **Design:** Elongated oval or energy bolt
- **Colors:** Bright cyan (#00FFFF) with white center

---

## Enemy Assets

### Basic Enemy (`enemy_basic.png`)
- **Size:** 48x48 pixels or 64x64 pixels
- **Design:** Simple angular shape (square with cut corners, hexagon)
- **Colors:** Red (#FF4444) with darker red outline

### Zigzag Enemy (`enemy_zigzag.png`)
- **Size:** 48x48 pixels
- **Design:** Diamond or arrow shape
- **Colors:** Orange (#FF8800)

### Circular Enemy (`enemy_circular.png`)
- **Size:** 48x48 pixels
- **Design:** Circular or ring shape
- **Colors:** Purple (#AA44FF)

### Charger Enemy (`enemy_charger.png`)
- **Size:** 48x48 pixels
- **Design:** Pointed/aggressive shape
- **Colors:** Yellow (#FFFF44)

### Boss Enemy (`enemy_boss.png`)
- **Size:** 128x128 pixels or 192x192 pixels
- **Design:** Large imposing ship, more detailed
- **Colors:** Dark red (#880000) with glowing accents

### Enemy Bullet (`bullet_enemy.png`)
- **Size:** 12x12 pixels
- **Design:** Small circle or diamond
- **Colors:** Red (#FF3333) with orange glow

---

## Power-Up Assets

### Generic Power-Up (`powerup.png`)
- **Size:** 32x32 pixels
- **Design:** Diamond, star, or capsule shape
- **Note:** Color is set via script, so use white base

### Shield Power-Up Icon
- **Color:** Cyan (#00DDFF)
- **Symbol:** Shield outline

### Rapid Fire Power-Up Icon
- **Color:** Yellow (#FFDD00)
- **Symbol:** Lightning bolt or triple arrows

### Health Power-Up Icon
- **Color:** Green (#44FF44)
- **Symbol:** Plus sign or heart

### Extra Life Power-Up Icon
- **Color:** Magenta (#FF44FF)
- **Symbol:** Small ship or 1UP text

---

## Background Assets

### Space Background (`background_space.png`)
- **Size:** 1920x1080 pixels or tiling texture (512x512)
- **Design:** Dark blue/black gradient with distant stars
- **Colors:** 
  - Top: Dark blue (#0a0a2a)
  - Bottom: Near black (#050510)
  - Stars: White dots of varying sizes

### Nebula Overlay (`background_nebula.png`)
- **Size:** 512x512 pixels (tiling)
- **Design:** Semi-transparent colorful clouds
- **Colors:** Purple, blue, pink hues at low opacity (20-40%)

---

## UI Assets

### Button Backgrounds
- **Size:** 200x50 pixels (stretchable)
- **Design:** Rounded rectangle or sci-fi panel style
- **Colors:** Dark blue (#1a1a3a) with bright border (#00AAFF)

### Health Bar
- **Background:** Dark gray (#333333)
- **Fill:** Gradient from green to red based on health
- **Border:** White or cyan

### Panel Backgrounds
- **Style:** Semi-transparent dark panels
- **Colors:** #000000 at 80% opacity with colored border

---

## Creating Placeholder Sprites

If you don't have art assets, you can create simple placeholders:

### Using Unity's Built-in Shapes:
1. In Project window: Right-click > Create > 2D > Sprites
2. Choose Circle, Square, Triangle, etc.
3. Tint via SpriteRenderer color property

### Using Code-Generated Sprites:
```csharp
// Create a simple colored square sprite
public static Sprite CreateSquareSprite(int size, Color color)
{
    Texture2D texture = new Texture2D(size, size);
    Color[] pixels = new Color[size * size];
    for (int i = 0; i < pixels.Length; i++)
        pixels[i] = color;
    texture.SetPixels(pixels);
    texture.Apply();
    return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
}
```

---

## Free Asset Sources

Here are some free asset sources you can use:

1. **OpenGameArt.org** - Free game assets (check licenses)
2. **Kenney.nl** - High-quality free game assets (CC0)
3. **itch.io** - Many free sprite packs
4. **Unity Asset Store** - Search for free 2D space assets

### Recommended Free Packs:
- Kenney's "Space Shooter Redux" (free, CC0)
- Kenney's "Space Shooter Extension" (free, CC0)

---

## Audio Specifications

### Sound Effects
- **Format:** WAV or OGG
- **Sample Rate:** 44100 Hz
- **Channels:** Mono (for SFX) or Stereo
- **Duration:** 0.1s - 2s for most effects

### Required Sound Effects:
| Name | Description | Duration |
|------|-------------|----------|
| PlayerShoot | Laser/pew sound | 0.1-0.3s |
| PlayerHit | Impact/damage | 0.2-0.5s |
| PlayerDeath | Explosion | 0.5-1s |
| EnemyShoot | Enemy laser | 0.1-0.3s |
| EnemyDeath | Small explosion | 0.3-0.5s |
| BossSpawn | Warning/alarm | 1-2s |
| BossDeath | Large explosion | 1-2s |
| PowerUp | Pickup chime | 0.3-0.5s |
| WaveStart | Alert/notification | 0.5-1s |
| WaveComplete | Victory fanfare | 1-2s |

### Music
- **Format:** OGG (recommended) or MP3
- **Duration:** 2-4 minutes, loopable
- **Style:** Electronic, synthwave, or orchestral space themes

### Free Audio Sources:
- **Freesound.org** - Sound effects (check licenses)
- **OpenGameArt.org** - Music and SFX
- **Incompetech.com** - Royalty-free music by Kevin MacLeod

---

## Font Specifications

### Recommended Fonts:
- **Sci-Fi Style:** Orbitron, Audiowide, Exo
- **Pixel Art:** Press Start 2P, VT323, Silkscreen

### Usage:
1. Download TTF/OTF font files
2. Import into `Assets/Fonts/`
3. Create TextMeshPro Font Asset:
   - Window > TextMeshPro > Font Asset Creator
   - Drag font, generate atlas

---

## Layer Setup

Configure these sorting layers in order (bottom to top):
1. Background
2. Stars
3. Projectiles
4. Pickups
5. Characters
6. Effects
7. UI

## Physics Layers

Configure these physics layers:
- Default
- Player
- Enemies
- PlayerBullets
- EnemyBullets
- PowerUps

### Layer Collision Matrix:
| | Player | Enemies | PlayerBullets | EnemyBullets | PowerUps |
|---|---|---|---|---|---|
| Player | - | ✓ | - | ✓ | ✓ |
| Enemies | ✓ | - | ✓ | - | - |
| PlayerBullets | - | ✓ | - | - | - |
| EnemyBullets | ✓ | - | - | - | - |
| PowerUps | ✓ | - | - | - | - |
