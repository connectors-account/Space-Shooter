# Sprite Setup Guide for Unity

All game sprites have been generated and downloaded to this directory. Follow these steps to integrate them into your Unity project.

## Available Sprites

1. **PlayerShip.png** - Blue/silver player spaceship (1024x1024)
2. **EnemyShips.png** - Three enemy types in one image (1536x1024, 4:1 ratio)
3. **BossShip.png** - Large boss enemy (1024x1024)
4. **Bullets.png** - Player and enemy bullet sprites (1536x1024, 4:1 ratio)
5. **PowerUps.png** - Five power-up icons (1536x1024, 3:2 ratio)
6. **Background.png** - Space background for parallax (1024x1024)

## Unity Import Settings

### Step 1: Import Sprites into Unity
1. Open the SpaceShooter project in Unity Editor
2. The sprites are already in `Assets/Sprites/` folder
3. Unity will auto-detect them on first project load

### Step 2: Configure Each Sprite

#### PlayerShip.png
- **Texture Type:** Sprite (2D and UI)
- **Sprite Mode:** Single
- **Pixels Per Unit:** 100
- **Filter Mode:** Point (for pixel art) or Bilinear (for smooth)
- **Compression:** None or High Quality
- **Max Size:** 1024
- Click **Apply**

#### EnemyShips.png (Multiple Sprites)
- **Texture Type:** Sprite (2D and UI)
- **Sprite Mode:** Multiple
- **Pixels Per Unit:** 100
- Click **Sprite Editor** button
- Use **Slice** → **Grid By Cell Count**
  - Columns: 3
  - Rows: 1
- Name the sprites: `EnemyBasic`, `EnemyZigzag`, `EnemyCircular`
- Click **Apply**

#### BossShip.png
- Same as PlayerShip.png settings

#### Bullets.png (Multiple Sprites)
- **Sprite Mode:** Multiple
- **Pixels Per Unit:** 100
- Use Sprite Editor to slice into 3 bullets:
  - Columns: 3, Rows: 1
- Name: `BulletPlayer`, `BulletEnemy`, `BulletSpread`
- Click **Apply**

#### PowerUps.png (Multiple Sprites)
- **Sprite Mode:** Multiple
- **Pixels Per Unit:** 100
- Use Sprite Editor to slice:
  - Manually slice or use Grid (estimate 5 icons arranged in grid)
- Name: `PowerUp_Health`, `PowerUp_Shield`, `PowerUp_RapidFire`, `PowerUp_SpreadShot`, `PowerUp_Multiplier`
- Click **Apply**

#### Background.png
- **Texture Type:** Sprite (2D and UI)
- **Sprite Mode:** Single
- **Wrap Mode:** Repeat (for tiling/parallax)
- **Pixels Per Unit:** 100
- **Max Size:** 1024
- Click **Apply**

### Step 3: Create Prefabs with Sprites

The game uses procedural sprite generation by default, but you can manually assign these sprites:

#### Assign to Existing GameObjects:

1. **Player Ship:**
   - Run the game once to generate player GameObject
   - Find "Player(Clone)" in Hierarchy
   - Add SpriteRenderer component if not present
   - Drag `PlayerShip` sprite to SpriteRenderer
   - Adjust scale if needed (e.g., 0.5 to 1.0)

2. **Enemies:**
   - The game spawns enemies at runtime
   - Modify `EnemyController.cs` to use your sprites:
   ```csharp
   // In Start() method, replace procedural sprite:
   var spriteRenderer = GetComponent<SpriteRenderer>();
   if (spriteRenderer != null) {
       // Load your sprite from Resources or assign via Inspector
       spriteRenderer.sprite = yourEnemySprite;
   }
   ```

3. **Alternative: Create Prefabs in Unity Editor:**
   - Create empty GameObject
   - Add SpriteRenderer component
   - Assign sprite
   - Add necessary scripts (EnemyController, Rigidbody2D, Collider2D)
   - Save as Prefab in `Assets/Prefabs/`
   - Reference prefab in SpawnManager

### Step 4: Update SpriteFactory (Optional)

If you want to fully replace procedural sprites with your custom ones:

1. Create a Resources folder: `Assets/Resources/Sprites/`
2. Move or copy sprites there
3. Modify `SpriteFactory.cs` to load from Resources:

```csharp
public static Sprite GetPlayerSprite() {
    Sprite sprite = Resources.Load<Sprite>("Sprites/PlayerShip");
    if (sprite != null) return sprite;
    // Fallback to procedural
    return CreateShipSprite(64, 64, Color.cyan);
}
```

## Quick Start (Easiest Method)

The game is **fully playable without any sprite setup** - it uses procedural generated shapes. However, to use your custom sprites:

1. Import sprites with settings above
2. Slice multi-sprite sheets (EnemyShips, Bullets, PowerUps)
3. In Unity, run the game once
4. Pause game, select Player/Enemy objects in Hierarchy
5. Manually drag sprites onto SpriteRenderer components
6. Note the scale/position adjustments needed
7. Update the bootstrap scripts or create prefabs with proper settings

## Sprite Dimensions Reference

- Player: ~64-128 units in game world
- Enemies: ~48-96 units
- Boss: ~128-256 units  
- Bullets: ~8-16 units
- PowerUps: ~32-48 units
- Background: Full screen, tiled

Adjust the Pixels Per Unit or GameObject scale to achieve desired in-game size.

## Notes

- All sprites have **transparent backgrounds** (except Background.png)
- Use **Point filter** for crisp pixel art look
- Use **Bilinear/Trilinear** for smoother appearance
- The game's collision detection works independently of sprites
- Sprites are visual only - collision uses Collider2D components
