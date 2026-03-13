using UnityEngine;

/// <summary>
/// Generates procedural sprites at runtime for all game objects.
/// Creates pixel art-style sprites for player ship, enemies, bullets, power-ups, etc.
/// No external sprite assets required.
/// </summary>
public static class SpriteGenerator
{
    /// <summary>
    /// Creates a filled rectangle sprite.
    /// </summary>
    public static Sprite CreateRectSprite(int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), Mathf.Max(width, height));
    }

    /// <summary>
    /// Creates a circle sprite with soft edges.
    /// </summary>
    public static Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - dist / radius);
                pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates a triangle sprite pointing up or down (for enemies).
    /// </summary>
    public static Sprite CreateTriangleSprite(int size, Color color, bool pointDown = false)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Color clear = new Color(0, 0, 0, 0);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        float half = size / 2f;

        for (int y = 0; y < size; y++)
        {
            float rowProgress = pointDown ? (float)(size - y) / size : (float)y / size;
            float halfWidth = half * rowProgress;
            int xStart = Mathf.RoundToInt(half - halfWidth);
            int xEnd = Mathf.RoundToInt(half + halfWidth);

            for (int x = xStart; x < xEnd; x++)
            {
                if (x >= 0 && x < size)
                {
                    // Add some detail: darker edges, lighter center
                    float distFromCenter = Mathf.Abs(x - half) / halfWidth;
                    float brightness = 1f - distFromCenter * 0.3f;

                    // Cockpit/canopy highlight
                    bool isCockpit = Mathf.Abs(x - half) < 3 && y > size * 0.5f && y < size * 0.8f;
                    if (pointDown)
                        isCockpit = Mathf.Abs(x - half) < 3 && y > size * 0.2f && y < size * 0.5f;

                    Color pixelColor;
                    if (isCockpit)
                        pixelColor = new Color(0.9f, 0.9f, 1f, 1f);
                    else
                        pixelColor = new Color(color.r * brightness, color.g * brightness, color.b * brightness, 1f);

                    pixels[y * size + x] = pixelColor;
                }
            }
        }

        // Add wing details
        int wingY = pointDown ? size * 3 / 4 : size / 4;
        for (int x = 0; x < size; x++)
        {
            if (pixels[wingY * size + x].a > 0)
            {
                pixels[wingY * size + x] = new Color(
                    color.r * 0.7f, color.g * 0.7f, color.b * 0.7f, 1f);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates the player ship sprite - a more detailed triangle/arrow shape.
    /// </summary>
    public static Sprite CreatePlayerShipSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Color clear = new Color(0, 0, 0, 0);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        float half = size / 2f;

        // Main body
        for (int y = 0; y < size; y++)
        {
            float bodyProgress = (float)y / size;
            float bodyWidth = half * bodyProgress * 0.7f;

            // Wings flare out more at bottom
            if (y < size / 3)
            {
                bodyWidth = half * bodyProgress * 1.2f;
            }

            int xStart = Mathf.RoundToInt(half - bodyWidth);
            int xEnd = Mathf.RoundToInt(half + bodyWidth);

            for (int x = xStart; x < xEnd; x++)
            {
                if (x >= 0 && x < size)
                {
                    float distFromCenter = Mathf.Abs(x - half) / Mathf.Max(bodyWidth, 1f);
                    float brightness = 1f - distFromCenter * 0.4f;

                    pixels[y * size + x] = new Color(
                        color.r * brightness, color.g * brightness, color.b * brightness, 1f);
                }
            }
        }

        // Cockpit (bright spot near top)
        for (int y = (int)(size * 0.6f); y < (int)(size * 0.85f); y++)
        {
            for (int x = (int)(half - 2); x <= (int)(half + 2); x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(half, size * 0.72f));
                    if (dist < 3f)
                    {
                        float alpha = 1f - dist / 3f;
                        pixels[y * size + x] = Color.Lerp(pixels[y * size + x],
                            new Color(0.8f, 0.9f, 1f, 1f), alpha);
                    }
                }
            }
        }

        // Wing accents
        for (int y = 2; y < size / 4; y++)
        {
            float wingWidth = half * ((float)y / size) * 1.2f;
            int leftWing = Mathf.RoundToInt(half - wingWidth);
            int rightWing = Mathf.RoundToInt(half + wingWidth);
            if (leftWing >= 0 && leftWing < size)
                pixels[y * size + leftWing] = new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.3f, 1f);
            if (rightWing >= 0 && rightWing < size)
                pixels[y * size + rightWing] = new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.3f, 1f);
        }

        // Engine glow at bottom center
        for (int y = 0; y < 4; y++)
        {
            for (int x = (int)(half - 2); x <= (int)(half + 2); x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = new Color(0.4f, 0.6f, 1f, 0.8f);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates a hexagon sprite for boss enemies.
    /// </summary>
    public static Sprite CreateHexagonSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Color clear = new Color(0, 0, 0, 0);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        float center = size / 2f;
        float radius = size / 2f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;

                // Hexagon distance function
                Vector2 p = new Vector2(Mathf.Abs(dx), Mathf.Abs(dy));
                float hex = Mathf.Max(p.x + p.y * 0.577f, p.y * 1.155f);

                if (hex < radius)
                {
                    float distFromCenter = hex / radius;
                    float brightness = 1f - distFromCenter * 0.3f;

                    // Inner detail pattern
                    bool isDetail = (Mathf.Abs(dx) < 2 || Mathf.Abs(dy) < 2) &&
                                    hex > radius * 0.3f && hex < radius * 0.7f;

                    Color pixelColor;
                    if (isDetail)
                        pixelColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);
                    else if (hex > radius - 2f)
                        pixelColor = new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.3f, 1f); // edge glow
                    else
                        pixelColor = new Color(color.r * brightness, color.g * brightness, color.b * brightness, 1f);

                    // Core glow
                    if (hex < radius * 0.2f)
                        pixelColor = Color.Lerp(pixelColor, new Color(1f, 0.5f, 0.2f, 1f), 0.5f);

                    pixels[y * size + x] = pixelColor;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates a diamond/gem sprite for power-ups.
    /// </summary>
    public static Sprite CreateDiamondSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Color clear = new Color(0, 0, 0, 0);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center);
                float dy = Mathf.Abs(y - center);
                float diamond = dx + dy;

                if (diamond < center - 1f)
                {
                    float brightness = 1f - diamond / center;
                    Color pixelColor = new Color(
                        color.r * (0.7f + brightness * 0.3f),
                        color.g * (0.7f + brightness * 0.3f),
                        color.b * (0.7f + brightness * 0.3f),
                        1f);

                    // Shine effect
                    if (dx < 2 && dy < center * 0.3f)
                        pixelColor = Color.Lerp(pixelColor, Color.white, 0.5f);

                    pixels[y * size + x] = pixelColor;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
