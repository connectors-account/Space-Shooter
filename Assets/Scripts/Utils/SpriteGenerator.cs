using UnityEngine;

/// <summary>
/// Procedurally generates simple geometric sprites for all game objects.
/// Attach to a GameObject in the scene and call GenerateAll() from an
/// editor script, or use at runtime for dynamic sprite creation.
/// </summary>
public static class SpriteGenerator
{
    /// <summary>
    /// Creates a player ship sprite (arrow/triangle pointing up).
    /// </summary>
    public static Sprite CreatePlayerSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(0.2f, 0.6f, 1f); // Blue
        Color cockpit = new Color(0.4f, 0.9f, 1f); // Light blue
        Color engine = new Color(1f, 0.5f, 0f); // Orange

        // Clear
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        // Draw ship body (triangle)
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth = Mathf.RoundToInt(Mathf.Lerp(size / 2f, 1, progress));
            int center = size / 2;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x < 0 || x >= size) continue;

                if (y < 4) // Engine glow at bottom
                    tex.SetPixel(x, y, engine);
                else if (y > size - 6 && Mathf.Abs(x - center) < 2) // Cockpit at top
                    tex.SetPixel(x, y, cockpit);
                else
                    tex.SetPixel(x, y, hull);
            }
        }

        // Wing accents
        for (int y = 4; y < 12; y++)
        {
            tex.SetPixel(size / 2 - 10 + y, y, engine);
            tex.SetPixel(size / 2 + 10 - y, y, engine);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }

    /// <summary>
    /// Creates a basic enemy sprite (inverted triangle).
    /// </summary>
    public static Sprite CreateEnemyStraightSprite()
    {
        int size = 24;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(1f, 0.2f, 0.2f); // Red
        Color accent = new Color(1f, 0.6f, 0f); // Orange

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth = Mathf.RoundToInt(Mathf.Lerp(1, size / 2f, progress));
            int center = size / 2;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x < 0 || x >= size) continue;
                if (y > size - 4)
                    tex.SetPixel(x, y, accent);
                else
                    tex.SetPixel(x, y, hull);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24);
    }

    /// <summary>
    /// Creates a zigzag enemy sprite (diamond shape).
    /// </summary>
    public static Sprite CreateEnemyZigzagSprite()
    {
        int size = 24;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(0.8f, 0.2f, 0.8f); // Purple
        Color accent = new Color(1f, 0.4f, 1f);

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        int center = size / 2;
        for (int y = 0; y < size; y++)
        {
            int halfWidth;
            if (y < center)
                halfWidth = y;
            else
                halfWidth = size - 1 - y;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x < 0 || x >= size) continue;
                if (Mathf.Abs(x - center) <= 2)
                    tex.SetPixel(x, y, accent);
                else
                    tex.SetPixel(x, y, hull);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24);
    }

    /// <summary>
    /// Creates a tank enemy sprite (large square with details).
    /// </summary>
    public static Sprite CreateEnemyTankSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(0.6f, 0.1f, 0.1f); // Dark red
        Color armor = new Color(0.4f, 0.4f, 0.4f); // Gray armor
        Color eye = new Color(1f, 1f, 0f); // Yellow

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        // Main body (rounded rectangle)
        for (int y = 4; y < size - 4; y++)
        {
            for (int x = 4; x < size - 4; x++)
            {
                tex.SetPixel(x, y, hull);
            }
        }

        // Armor plates on sides
        for (int y = 6; y < size - 6; y++)
        {
            tex.SetPixel(2, y, armor);
            tex.SetPixel(3, y, armor);
            tex.SetPixel(size - 3, y, armor);
            tex.SetPixel(size - 4, y, armor);
        }

        // Eye/cockpit
        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                tex.SetPixel(size / 2 + dx, size / 2 + dy, eye);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24);
    }

    /// <summary>
    /// Creates a diver enemy sprite (arrow pointing down).
    /// </summary>
    public static Sprite CreateEnemyDiverSprite()
    {
        int size = 20;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(1f, 0.5f, 0f); // Orange
        Color trail = new Color(1f, 1f, 0.3f); // Yellow

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth = Mathf.RoundToInt(Mathf.Lerp(size / 2f, 1, progress));
            int center = size / 2;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x < 0 || x >= size) continue;
                if (y > size - 4)
                    tex.SetPixel(x, y, trail);
                else
                    tex.SetPixel(x, y, hull);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 20);
    }

    /// <summary>
    /// Creates a bullet sprite (small elongated rectangle).
    /// </summary>
    public static Sprite CreateBulletSprite(bool isPlayer)
    {
        int w = 4, h = 8;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color c = isPlayer ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Brighter at center
                float dist = Mathf.Abs(x - w / 2f) / (w / 2f);
                Color pixel = Color.Lerp(Color.white, c, dist);
                tex.SetPixel(x, y, pixel);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 8);
    }

    /// <summary>
    /// Creates a power-up sprite (rotating diamond/gem shape).
    /// </summary>
    public static Sprite CreatePowerUpSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color transparent = new Color(0, 0, 0, 0);

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        int center = size / 2;
        for (int y = 0; y < size; y++)
        {
            int halfWidth;
            if (y < center)
                halfWidth = y;
            else
                halfWidth = size - 1 - y;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x < 0 || x >= size) continue;
                // White/bright core, colored by PowerUp.Initialize()
                tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
    }
}
