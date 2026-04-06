using UnityEngine;

/// <summary>
/// Programmatically generates simple geometric sprites at runtime.
/// Eliminates the need for external sprite assets.
/// Attach to a GameObject in the scene; call from other scripts or use the static methods.
/// </summary>
public class SpriteGenerator : MonoBehaviour
{
    public static SpriteGenerator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Creates a triangle sprite (for ships). Points upward by default.
    /// </summary>
    public static Sprite CreateTriangleSprite(int size = 32, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        // Clear to transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Draw filled triangle pointing up
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth = Mathf.RoundToInt(progress * size / 2f);
            int center = size / 2;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates an inverted triangle sprite (for enemy ships pointing down).
    /// </summary>
    public static Sprite CreateInvertedTriangleSprite(int size = 32, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        for (int y = 0; y < size; y++)
        {
            float progress = 1f - (float)y / size;
            int halfWidth = Mathf.RoundToInt(progress * size / 2f);
            int center = size / 2;

            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates a circle sprite (for bullets).
    /// </summary>
    public static Sprite CreateCircleSprite(int size = 16, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                if (dist <= radius)
                    pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates a diamond/square sprite rotated 45 degrees (for power-ups).
    /// </summary>
    public static Sprite CreateDiamondSprite(int size = 24, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        float half = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - half);
                float dy = Mathf.Abs(y + 0.5f - half);
                if (dx + dy <= half)
                    pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates a simple square sprite.
    /// </summary>
    public static Sprite CreateSquareSprite(int size = 32, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = c;

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates a star-shaped sprite for visual effects.
    /// </summary>
    public static Sprite CreateStarSprite(int size = 8, Color? color = null)
    {
        Color c = color ?? Color.white;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Cross pattern for a simple star
        int mid = size / 2;
        for (int i = 0; i < size; i++)
        {
            pixels[mid * size + i] = c;
            pixels[i * size + mid] = c;
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
    }
}
