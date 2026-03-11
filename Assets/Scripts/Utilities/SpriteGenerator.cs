using UnityEngine;

/// <summary>
/// Utility class to generate simple placeholder sprites at runtime.
/// Useful for testing without art assets.
/// </summary>
public static class SpriteGenerator
{
    /// <summary>
    /// Creates a simple colored square sprite.
    /// </summary>
    public static Sprite CreateSquare(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Creates a simple colored circle sprite.
    /// </summary>
    public static Sprite CreateCircle(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        int radius = size / 2;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius ? color : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Creates a simple triangle (pointing up) sprite.
    /// </summary>
    public static Sprite CreateTriangle(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Draw triangle
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth = Mathf.RoundToInt(progress * size / 2f);
            int center = size / 2;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Creates a diamond-shaped sprite.
    /// </summary>
    public static Sprite CreateDiamond(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        int half = size / 2;

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        for (int y = 0; y < size; y++)
        {
            int distFromCenter = Mathf.Abs(y - half);
            int halfWidth = half - distFromCenter;

            for (int x = half - halfWidth; x <= half + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Creates a bullet-shaped sprite (elongated oval).
    /// </summary>
    public static Sprite CreateBullet(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Vector2 center = new Vector2(width / 2f, height / 2f);
        float radiusX = width / 2f;
        float radiusY = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x - center.x) / radiusX;
                float dy = (y - center.y) / radiusY;
                if (dx * dx + dy * dy <= 1f)
                {
                    // Add gradient for 3D effect
                    float brightness = 1f - (dx * dx + dy * dy) * 0.3f;
                    Color pixelColor = color * brightness;
                    pixelColor.a = 1f;
                    pixels[y * width + x] = pixelColor;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }
}
