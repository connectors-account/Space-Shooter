# Sprite Creation Guide

Since this project uses placeholder sprites, here's how to create simple geometric shapes in Unity or any image editor.

## Method 1: Unity's Built-in Sprites

Unity doesn't have built-in shape sprites, but you can:
1. Create simple colored textures programmatically
2. Import the placeholder images below
3. Use the UI system's Image component with color

## Method 2: Create Simple Sprites (Recommended)

Create these PNG files in any image editor (Paint, GIMP, Photoshop):

### Player Ship (64x64 px)
- Triangle pointing up
- Color: Cyan/Light Blue (#00FFFF)
- Or simple spaceship shape

### Player Bullet (8x16 px)
- Vertical rectangle
- Color: Yellow (#FFFF00)

### Enemy Bullet (8x16 px)
- Vertical rectangle
- Color: Red (#FF0000)

### Basic Enemy (48x48 px)
- Square or diamond
- Color: Red (#FF4444)

### Fast Enemy (32x32 px)
- Small triangle or diamond
- Color: Cyan (#00FFFF)

### Tank Enemy (64x64 px)
- Large square
- Color: Purple (#8800FF)

### Boss (128x128 px)
- Large hexagon or complex shape
- Color: Dark Red (#880000)

### Power-Up: Weapon (32x32 px)
- Star shape or "W"
- Color: Orange (#FF8800)

### Power-Up: Health (32x32 px)
- Cross or heart
- Color: Green (#00FF00)

### Power-Up: Shield (32x32 px)
- Circle or shield icon
- Color: Blue (#0088FF)

### Background (1920x1080 px)
- Dark blue/black gradient
- Add white dots for stars
- Color: #000020 to #000040

## Import Settings

After importing sprites to Unity:
1. Select sprite in Project window
2. In Inspector:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Single
   - Pixels Per Unit: 100
   - Filter Mode: Point (for pixel art) or Bilinear
   - Compression: None (for small sprites)
3. Click Apply

## Sorting Layers Order

1. Background (furthest back)
2. Midground
3. Player
4. Enemies
5. Bullets
6. PowerUps
7. UI (frontmost)
