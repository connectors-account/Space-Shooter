using UnityEngine;

/// <summary>
/// Utility script that generates simple colored placeholder sprites at runtime.
/// Attach this to a GameObject in your scene to auto-generate placeholder sprites
/// for all game objects that are missing sprite references.
/// 
/// This is a DEVELOPMENT TOOL. Remove before final build and replace with real art.
/// </summary>
public class PlaceholderSpriteGenerator : MonoBehaviour
{
    public static PlaceholderSpriteGenerator Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>Create a simple colored square sprite.</summary>
    public static Sprite CreateSquareSprite(Color color, int size = 32)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Create a triangle-shaped sprite (for player ship).</summary>
    public static Sprite CreateTriangleSprite(Color color, int size = 32)
    {
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            float widthAtRow = (float)y / size;
            int halfWidth = Mathf.RoundToInt(widthAtRow * size / 2f);
            int center = size / 2;

            for (int x = 0; x < size; x++)
            {
                if (x >= center - halfWidth && x <= center + halfWidth)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = transparent;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Create a circle sprite (for bullets/power-ups).</summary>
    public static Sprite CreateCircleSprite(Color color, int size = 16)
    {
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];

        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = dist <= radius ? color : transparent;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Create a diamond sprite (for power-ups).</summary>
    public static Sprite CreateDiamondSprite(Color color, int size = 24)
    {
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];

        int half = size / 2;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - half);
                int dy = Mathf.Abs(y - half);
                pixels[y * size + x] = (dx + dy <= half) ? color : transparent;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
