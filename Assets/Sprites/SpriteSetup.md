# Sprite Setup

The game only needs a handful of simple shapes. You can create them three ways —
pick whichever is fastest for you.

## Option A — Built-in Unity shapes (no art needed)
Unity ships a white square/circle you can tint:
1. **GameObject → 2D Object → Sprites → Square / Circle / Triangle** (2022.3+).
2. Set the **SpriteRenderer.Color** to the colour listed below.
3. Scale the transform to the desired size.

## Option B — Draw a 1×1 sprite and tint it
1. Create a `Sprites/` PNG that is a single white pixel (or use the built-in
   `UISprite`/`Knob`).
2. Assign it to a SpriteRenderer and set **Color** per the table.

## Option C — Import PNGs
Drop PNG files into `Assets/Sprites/`, set **Texture Type = Sprite (2D and UI)**,
**Pixels Per Unit = 100**, **Filter Mode = Point** for crisp pixels, then Apply.

---

## Sprite list

| Asset name | Shape | Colour | Approx. size (world units) |
|------------|-------|--------|-----------------------------|
| `player` | Triangle pointing up | Blue `#3B82F6` | 0.8 × 0.8 |
| `enemy_drone` | Circle | Red `#EF4444` | 0.7 diameter |
| `enemy_fighter` | Diamond (rotated square) | Orange `#F97316` | 0.8 × 0.8 |
| `enemy_boss` | Hexagon | Purple `#8B5CF6` | 3.0 × 3.0 |
| `bullet_player` | Thin rectangle | Cyan `#22D3EE` | 0.1 × 0.4 |
| `bullet_enemy` | Thin rectangle | Magenta `#EC4899` | 0.1 × 0.4 |
| `powerup_speed` | Circle + `»` glyph | Yellow `#FACC15` | 0.5 diameter |
| `powerup_shield` | Circle + shield glyph | Cyan `#06B6D4` | 0.5 diameter |
| `powerup_triple` | Circle + trident glyph | Green `#22C55E` | 0.5 diameter |
| `powerup_bomb` | Circle + burst glyph | Red `#DC2626` | 0.5 diameter |
| `stars` | Black tile with scattered white dots | Black `#000000` bg | 20 × 10 (tiles vertically) |

### Making the star background
1. Create a **20×10** (or screen-sized) black PNG.
2. Scatter small white dots (2–4 px) randomly for stars.
3. Import as a Sprite. Make **two stacked copies** per parallax layer (`TileA`,
   `TileB`) so `ParallaxBackground` can wrap them for an infinite scroll.
4. Use 2–3 layers with different star densities and scroll speeds
   (0.5 / 1.0 / 2.0) for depth.

### Generating a triangle / diamond / hexagon quickly
* **Triangle / Diamond:** a square SpriteRenderer rotated 45° works for the diamond;
  for the triangle use the built-in triangle sprite or a `PolygonCollider2D`-shaped
  PNG.
* **Hexagon (boss):** draw a filled hexagon PNG at 256×256 and import it.

Keep the sprite's forward/up direction pointing **up (+Y)** so bullet rotation and
enemy facing line up with the code (`Vector2.up` is "forward").
