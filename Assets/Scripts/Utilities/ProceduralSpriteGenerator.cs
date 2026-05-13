// ============================================================================
// ProceduralSpriteGenerator.cs - Generates placeholder sprites at runtime
// Creates simple ship, bullet, and power-up sprites so the game runs
// without any imported image assets.
// ============================================================================
using UnityEngine;

/// <summary>
/// Static utility class that generates simple colored sprites at runtime.
/// Called by GameSetup to create textures for all game objects.
/// </summary>
public static class ProceduralSpriteGenerator
{
    private static int ppu = 32; // Pixels per unit for all generated sprites.

    // ========================================================================
    // Player Ship
    // ========================================================================

    /// <summary>
    /// Creates a triangular player ship sprite (pointing up).
    /// </summary>
    public static Sprite CreatePlayerShip()
    {
        int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[w * h];

        // Transparent background.
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        // Draw a symmetrical ship shape.
        Color hull = new Color(0.2f, 0.6f, 1f); // Blue hull.
        Color cockpit = new Color(0.9f, 0.95f, 1f); // Light cockpit.
        Color engine = new Color(1f, 0.5f, 0.1f); // Orange engine glow.

        // Main body triangle.
        for (int y = 4; y < 28; y++)
        {
            float t = (float)(y - 4) / 24f;
            int halfWidth = Mathf.RoundToInt(Mathf.Lerp(1, 12, 1f - t));
            int cx = w / 2;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < w)
                    pixels[y * w + x] = hull;
            }
        }

        // Cockpit highlight (small bright area near the top).
        for (int y = 20; y < 26; y++)
        {
            for (int x = 14; x <= 17; x++)
            {
                pixels[y * w + x] = cockpit;
            }
        }

        // Engine glow at the bottom.
        for (int y = 4; y < 8; y++)
        {
            for (int x = 13; x <= 18; x++)
            {
                pixels[y * w + x] = engine;
            }
        }

        // Wing accents.
        Color wing = new Color(0.15f, 0.4f, 0.8f);
        for (int y = 8; y < 16; y++)
        {
            pixels[y * w + 4] = wing;
            pixels[y * w + 5] = wing;
            pixels[y * w + 26] = wing;
            pixels[y * w + 27] = wing;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }

    // ========================================================================
    // Enemy Ships
    // ========================================================================

    /// <summary>Creates a red inverted-triangle enemy sprite.</summary>
    public static Sprite CreateEnemyStraight()
    {
        return CreateEnemySprite(new Color(1f, 0.2f, 0.2f), new Color(0.8f, 0.1f, 0.1f));
    }

    /// <summary>Creates a green enemy sprite for zigzag type.</summary>
    public static Sprite CreateEnemyZigzag()
    {
        return CreateEnemySprite(new Color(0.2f, 1f, 0.3f), new Color(0.1f, 0.7f, 0.2f));
    }

    /// <summary>Creates a purple enemy sprite for circling type.</summary>
    public static Sprite CreateEnemyCircling()
    {
        return CreateEnemySprite(new Color(0.7f, 0.2f, 1f), new Color(0.5f, 0.1f, 0.8f));
    }

    /// <summary>Creates a yellow enemy sprite for diver type.</summary>
    public static Sprite CreateEnemyDiver()
    {
        return CreateEnemySprite(new Color(1f, 0.9f, 0.2f), new Color(0.9f, 0.7f, 0.1f));
    }

    private static Sprite CreateEnemySprite(Color primary, Color secondary)
    {
        int w = 28, h = 28;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        // Inverted triangle (pointing down) for enemies.
        for (int y = 6; y < 24; y++)
        {
            float t = (float)(y - 6) / 18f;
            int halfWidth = Mathf.RoundToInt(Mathf.Lerp(10, 2, t));
            int cx = w / 2;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < w)
                {
                    Color c = (Mathf.Abs(x - cx) < halfWidth / 2) ? primary : secondary;
                    pixels[y * w + x] = c;
                }
            }
        }

        // Eye/window detail.
        Color eye = new Color(1f, 1f, 0.8f);
        for (int dy = 0; dy < 3; dy++)
        {
            for (int dx = 0; dx < 3; dx++)
            {
                pixels[(10 + dy) * w + (12 + dx)] = eye;
                pixels[(10 + dy) * w + (14 + dx)] = eye;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }

    // ========================================================================
    // Bullets
    // ========================================================================

    /// <summary>Creates a small bright cyan player bullet sprite.</summary>
    public static Sprite CreatePlayerBullet()
    {
        return CreateBulletSprite(new Color(0.3f, 1f, 1f), new Color(1f, 1f, 1f));
    }

    /// <summary>Creates a small red-orange enemy bullet sprite.</summary>
    public static Sprite CreateEnemyBullet()
    {
        return CreateBulletSprite(new Color(1f, 0.4f, 0.1f), new Color(1f, 0.8f, 0.3f));
    }

    private static Sprite CreateBulletSprite(Color outer, Color inner)
    {
        int w = 8, h = 12;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        // Simple elongated pill shape.
        for (int y = 1; y < h - 1; y++)
        {
            for (int x = 2; x < w - 2; x++)
            {
                pixels[y * w + x] = outer;
            }
        }
        // Inner bright core.
        for (int y = 3; y < h - 3; y++)
        {
            for (int x = 3; x < w - 3; x++)
            {
                pixels[y * w + x] = inner;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }

    // ========================================================================
    // Power-Ups
    // ========================================================================

    /// <summary>Creates a green health power-up (cross/plus shape).</summary>
    public static Sprite CreateHealthPowerUp()
    {
        int w = 16, h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        Color main = new Color(0.2f, 1f, 0.3f);
        Color border = new Color(0.1f, 0.6f, 0.15f);

        // Background diamond.
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x - w / 2f);
                float dy = Mathf.Abs(y - h / 2f);
                if (dx + dy < 7)
                    pixels[y * w + x] = border;
            }
        }

        // Plus/cross symbol.
        for (int i = 4; i < 12; i++)
        {
            pixels[8 * w + i] = main;     // Horizontal.
            pixels[i * w + 8] = main;     // Vertical.
            pixels[7 * w + i] = main;
            pixels[i * w + 7] = main;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }

    /// <summary>Creates a blue shield power-up (circle shape).</summary>
    public static Sprite CreateShieldPowerUp()
    {
        int w = 16, h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        Color rim = new Color(0.3f, 0.6f, 1f);
        Color center = new Color(0.5f, 0.8f, 1f, 0.7f);
        float cx = w / 2f, cy = h / 2f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist < 5)
                    pixels[y * w + x] = center;
                else if (dist < 7)
                    pixels[y * w + x] = rim;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }

    /// <summary>Creates an orange weapon-upgrade power-up (arrow up shape).</summary>
    public static Sprite CreateWeaponPowerUp()
    {
        int w = 16, h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        Color main = new Color(1f, 0.7f, 0.1f);
        Color accent = new Color(1f, 0.9f, 0.4f);

        // Arrow-up shape.
        for (int y = 4; y < 14; y++)
        {
            float t = (float)(y - 4) / 10f;
            int halfW;
            if (y < 10)
            {
                halfW = Mathf.RoundToInt(Mathf.Lerp(1, 6, t * 1.5f)); // Arrowhead.
            }
            else
            {
                halfW = 2; // Shaft.
            }
            for (int x = 8 - halfW; x <= 8 + halfW; x++)
            {
                if (x >= 0 && x < w)
                    pixels[y * w + x] = (Mathf.Abs(x - 8) <= 1) ? accent : main;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }

    // ========================================================================
    // Shield Visual (attached to player)
    // ========================================================================

    /// <summary>Creates a semi-transparent blue circle to represent the player's shield.</summary>
    public static Sprite CreateShieldVisual()
    {
        int w = 48, h = 48;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        float cx = w / 2f, cy = h / 2f;
        float radius = 20f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist > radius - 2 && dist < radius + 1)
                {
                    // Rim.
                    float alpha = 1f - Mathf.Abs(dist - radius) / 2f;
                    pixels[y * w + x] = new Color(0.3f, 0.7f, 1f, alpha * 0.8f);
                }
                else if (dist < radius - 2)
                {
                    // Inner glow.
                    float alpha = 0.1f * (1f - dist / radius);
                    pixels[y * w + x] = new Color(0.4f, 0.7f, 1f, alpha);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }
}
