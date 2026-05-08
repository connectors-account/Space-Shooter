using UnityEngine;

/// <summary>
/// Creates simple geometric sprites programmatically at runtime.
/// All game visuals are generated here - no external sprite assets needed.
/// </summary>
public static class SpriteFactory
{
    /// <summary>
    /// Create a colored circle sprite.
    /// </summary>
    public static Sprite CreateCircle(int radius = 16, Color? color = null)
    {
        Color c = color ?? Color.white;
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist < radius - 1)
                    tex.SetPixel(x, y, c);
                else if (dist < radius)
                    tex.SetPixel(x, y, new Color(c.r, c.g, c.b, (radius - dist)));
                else
                    tex.SetPixel(x, y, clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Create a colored square sprite.
    /// </summary>
    public static Sprite CreateSquare(int size = 32, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Create a triangle (pointing up) sprite - used for the player ship.
    /// </summary>
    public static Sprite CreateTriangle(int size = 32, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        Vector2 top = new Vector2(size / 2f, size - 1);
        Vector2 botLeft = new Vector2(1, 1);
        Vector2 botRight = new Vector2(size - 2, 1);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 p = new Vector2(x, y);
                if (IsInsideTriangle(p, top, botLeft, botRight))
                    tex.SetPixel(x, y, c);
                else
                    tex.SetPixel(x, y, clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Create a diamond shape - used for enemies.
    /// </summary>
    public static Sprite CreateDiamond(int size = 32, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        float half = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Mathf.Abs(x - half) / half + Mathf.Abs(y - half) / half;
                if (dist < 0.95f)
                    tex.SetPixel(x, y, c);
                else if (dist < 1f)
                    tex.SetPixel(x, y, new Color(c.r, c.g, c.b, (1f - dist) * 20f));
                else
                    tex.SetPixel(x, y, clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Create a player ship sprite (arrowhead shape with details).
    /// </summary>
    public static Sprite CreatePlayerShip(int size = 48)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        Color hull = new Color(0.2f, 0.6f, 1f);      // Blue
        Color cockpit = new Color(0.5f, 0.9f, 1f);    // Light blue
        Color wing = new Color(0.15f, 0.4f, 0.8f);    // Darker blue
        Color engine = new Color(1f, 0.5f, 0.1f);     // Orange glow

        float half = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                tex.SetPixel(x, y, clear);

                float nx = (x - half) / half; // -1 to 1
                float ny = (y - half) / half; // -1 to 1

                // Main hull (narrowing triangle)
                float width = (1f - ny) * 0.5f + 0.05f;
                if (ny > -0.8f && Mathf.Abs(nx) < width)
                {
                    if (ny > 0.5f && Mathf.Abs(nx) < 0.15f)
                        tex.SetPixel(x, y, cockpit); // Cockpit
                    else if (Mathf.Abs(nx) > width * 0.7f)
                        tex.SetPixel(x, y, wing); // Wings
                    else
                        tex.SetPixel(x, y, hull);
                }

                // Engine glow at bottom
                if (ny < -0.6f && ny > -0.9f && Mathf.Abs(nx) < 0.2f)
                {
                    tex.SetPixel(x, y, engine);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
    }

    /// <summary>
    /// Create an enemy ship sprite (inverted triangle / menacing shape).
    /// </summary>
    public static Sprite CreateEnemyShip(int size = 32, Color? color = null)
    {
        Color c = color ?? new Color(1f, 0.2f, 0.2f);
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        Color dark = new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f);
        Color eye = new Color(1f, 1f, 0f); // Yellow eye

        float half = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                tex.SetPixel(x, y, clear);

                float nx = (x - half) / half;
                float ny = (y - half) / half;

                // Inverted triangle (wider at top)
                float width = (ny + 1f) * 0.4f + 0.1f;
                if (ny < 0.8f && ny > -0.7f && Mathf.Abs(nx) < width)
                {
                    if (ny > 0.2f && ny < 0.5f && Mathf.Abs(nx) < 0.15f)
                        tex.SetPixel(x, y, eye); // Eye
                    else if (Mathf.Abs(nx) > width * 0.75f)
                        tex.SetPixel(x, y, dark);
                    else
                        tex.SetPixel(x, y, c);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
    }

    /// <summary>
    /// Create a boss enemy sprite (larger, more detailed).
    /// </summary>
    public static Sprite CreateBossShip(int size = 64)
    {
        Color mainColor = new Color(0.8f, 0.1f, 0.5f);
        Color accent = new Color(1f, 0.3f, 0.1f);
        Color core = new Color(1f, 1f, 0f);

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);
        float half = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                tex.SetPixel(x, y, clear);
                float nx = (x - half) / half;
                float ny = (y - half) / half;

                // Wide body
                float bodyWidth = 0.7f - Mathf.Abs(ny) * 0.3f;
                if (Mathf.Abs(nx) < bodyWidth && ny > -0.8f && ny < 0.8f)
                {
                    if (Mathf.Abs(nx) < 0.15f && ny > 0f && ny < 0.4f)
                        tex.SetPixel(x, y, core);
                    else if (Mathf.Abs(nx) > bodyWidth * 0.8f)
                        tex.SetPixel(x, y, accent);
                    else
                        tex.SetPixel(x, y, mainColor);
                }

                // Wing extensions
                if (Mathf.Abs(ny) < 0.2f && Mathf.Abs(nx) > 0.5f && Mathf.Abs(nx) < 0.95f)
                {
                    tex.SetPixel(x, y, accent);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 3f);
    }

    /// <summary>
    /// Create a bullet sprite (small elongated shape).
    /// </summary>
    public static Sprite CreateBullet(int width = 6, int height = 16, Color? color = null)
    {
        Color c = color ?? Color.yellow;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        float halfW = width / 2f;
        float halfH = height / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float nx = (x - halfW) / halfW;
                float ny = (y - halfH) / halfH;
                float dist = nx * nx + ny * ny * 0.3f;

                if (dist < 0.8f)
                {
                    float brightness = 1f - dist * 0.3f;
                    tex.SetPixel(x, y, new Color(
                        Mathf.Min(1f, c.r * brightness + 0.2f),
                        Mathf.Min(1f, c.g * brightness + 0.2f),
                        Mathf.Min(1f, c.b * brightness + 0.2f), 1f));
                }
                else
                    tex.SetPixel(x, y, clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), width);
    }

    /// <summary>
    /// Create a star sprite for background.
    /// </summary>
    public static Sprite CreateStar(int size = 4, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        float half = size / 2f;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(half, half)) / half;
                if (dist < 1f)
                    tex.SetPixel(x, y, new Color(c.r, c.g, c.b, 1f - dist));
                else
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Create a shield visual (ring shape).
    /// </summary>
    public static Sprite CreateShieldRing(int size = 48)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float half = size / 2f;
        float outerR = half - 1;
        float innerR = half - 5;
        Color c = new Color(0.3f, 0.5f, 1f, 0.5f);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(half, half));
                if (dist > innerR && dist < outerR)
                    tex.SetPixel(x, y, c);
                else
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
    }

    /// <summary>
    /// Create a power-up gem sprite.
    /// </summary>
    public static Sprite CreatePowerUpGem(int size = 20)
    {
        return CreateDiamond(size, Color.white);
    }

    // --- Utility ---

    private static bool IsInsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}
