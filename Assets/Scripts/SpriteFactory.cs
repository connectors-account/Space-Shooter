using UnityEngine;

/// <summary>
/// Static utility class that generates basic geometric sprites at runtime.
/// Eliminates the need for external sprite assets.
/// </summary>
public static class SpriteFactory
{
    /// <summary>
    /// Creates a filled triangle sprite (for player/enemy ships).
    /// </summary>
    public static Sprite CreateTriangleSprite(Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Draw filled triangle: tip at top-center, base at bottom
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            int halfWidth = (int)((1f - t) * size / 2f);
            int center = size / 2;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        // Add a bright cockpit center line
        for (int y = size / 4; y < size * 3 / 4; y++)
        {
            int x = size / 2;
            if (y * size + x < pixels.Length)
            {
                pixels[y * size + x] = Color.white;
                if (x + 1 < size) pixels[y * size + x + 1] = new Color(1, 1, 1, 0.5f);
                if (x - 1 >= 0) pixels[y * size + x - 1] = new Color(1, 1, 1, 0.5f);
            }
        }

        tex.SetPixels(pixels);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }

    /// <summary>
    /// Creates a simple rectangle sprite (for bullets).
    /// </summary>
    public static Sprite CreateRectSprite(Color color)
    {
        int w = 8, h = 16;
        Texture2D tex = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        // Bright center
        for (int y = 0; y < h; y++)
        {
            pixels[y * w + w / 2] = Color.white;
        }

        tex.SetPixels(pixels);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    /// <summary>
    /// Creates a diamond/rhombus sprite (for power-ups).
    /// </summary>
    public static Sprite CreateDiamondSprite(Color color)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        int center = size / 2;

        for (int y = 0; y < size; y++)
        {
            int distY = Mathf.Abs(y - center);
            int halfWidth = center - distY;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                {
                    // Edge glow effect
                    int distX = Mathf.Abs(x - center);
                    float edgeDist = (float)(distX + distY) / center;
                    Color c = Color.Lerp(Color.white, color, edgeDist);
                    pixels[y * size + x] = c;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    /// <summary>
    /// Creates the player ship sprite — a more detailed triangle with wings.
    /// </summary>
    public static Sprite CreatePlayerShipSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Color bodyColor = new Color(0.2f, 0.6f, 1f);
        Color wingColor = new Color(0.15f, 0.4f, 0.8f);
        Color cockpitColor = new Color(0.5f, 0.9f, 1f);
        Color engineColor = new Color(1f, 0.6f, 0.2f);

        // Main body triangle
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            int halfWidth = (int)((1f - t) * size / 3f);
            int center = size / 2;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    pixels[y * size + x] = bodyColor;
            }
        }

        // Wings (wider at the bottom)
        for (int y = 0; y < size / 2; y++)
        {
            float t = (float)y / (size / 2);
            int wingExtent = (int)(t * size / 4f) + 2;
            int bodyEdge = (int)((1f - (float)y / size) * size / 3f);
            int center = size / 2;

            for (int x = 0; x < wingExtent; x++)
            {
                int lx = center - bodyEdge - x - 1;
                int rx = center + bodyEdge + x + 1;
                if (lx >= 0 && lx < size) pixels[y * size + lx] = wingColor;
                if (rx >= 0 && rx < size) pixels[y * size + rx] = wingColor;
            }
        }

        // Cockpit glow
        for (int y = size / 2; y < size * 3 / 4; y++)
        {
            for (int x = size / 2 - 2; x <= size / 2 + 2; x++)
            {
                if (x >= 0 && x < size)
                    pixels[y * size + x] = cockpitColor;
            }
        }

        // Engine glow at bottom
        for (int y = 0; y < 4; y++)
        {
            for (int x = size / 2 - 3; x <= size / 2 + 3; x++)
            {
                if (x >= 0 && x < size)
                    pixels[y * size + x] = engineColor;
            }
        }

        tex.SetPixels(pixels);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }
}
