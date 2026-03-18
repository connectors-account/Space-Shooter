// ============================================================================
// SpriteGenerator.cs - Runtime procedural sprite creation
// Generates all placeholder sprites programmatically so no external
// image files are needed. Attach to a GameObject in EACH scene.
// ============================================================================
using UnityEngine;

/// <summary>
/// Generates simple geometric sprites at runtime for all game objects.
/// This removes the need for external sprite files during development.
/// Call the static methods from any script, or let the component auto-generate
/// sprites for tagged objects on scene load.
/// </summary>
public class SpriteGenerator : MonoBehaviour
{
    // ========================================================================
    // Static Sprite Factory Methods
    // ========================================================================

    /// <summary>Create a triangle sprite (for the player ship).</summary>
    public static Sprite CreateTriangle(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex);

        // Draw filled triangle pointing up
        int halfW = size / 2;
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int width = Mathf.RoundToInt(halfW * (1f - progress));
            for (int x = halfW - width; x <= halfW + width; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, color);
            }
        }

        // Add a cockpit (darker center area)
        Color cockpitColor = color * 0.6f;
        cockpitColor.a = 1f;
        for (int y = size / 4; y < size / 2; y++)
        {
            for (int x = halfW - 3; x <= halfW + 3; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, cockpitColor);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size / 2f);
    }

    /// <summary>Create a diamond/rhombus sprite (for enemies).</summary>
    public static Sprite CreateDiamond(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex);

        int half = size / 2;
        for (int y = 0; y < size; y++)
        {
            int dist = Mathf.Abs(y - half);
            int width = half - dist;
            for (int x = half - width; x <= half + width; x++)
            {
                if (x >= 0 && x < size)
                {
                    // Edge glow effect
                    int edgeDist = Mathf.Min(Mathf.Abs(x - (half - width)),
                                              Mathf.Abs(x - (half + width)));
                    Color c = edgeDist < 2 ? Color.white : color;
                    tex.SetPixel(x, y, c);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size / 2f);
    }

    /// <summary>Create a circle sprite (for bullets and power-ups).</summary>
    public static Sprite CreateCircle(int size, Color color, bool filled = true)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex);

        float center = size / 2f;
        float radius = size / 2f - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (filled && dist <= radius)
                {
                    // Gradient from center for glow effect
                    float t = dist / radius;
                    Color c = Color.Lerp(Color.white, color, t);
                    tex.SetPixel(x, y, c);
                }
                else if (!filled && dist <= radius && dist >= radius - 2)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size / 2f);
    }

    /// <summary>Create a rectangular sprite (for UI elements, background).</summary>
    public static Sprite CreateRect(int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), Mathf.Min(width, height) / 2f);
    }

    /// <summary>Create a star field background texture.</summary>
    public static Sprite CreateStarfield(int width, int height, int starCount)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;

        // Fill with dark space background
        Color bgColor = new Color(0.02f, 0.02f, 0.08f, 1f);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, bgColor);

        // Add random stars
        for (int i = 0; i < starCount; i++)
        {
            int sx = Random.Range(0, width);
            int sy = Random.Range(0, height);
            float brightness = Random.Range(0.3f, 1f);
            Color starColor = new Color(brightness, brightness, brightness * Random.Range(0.8f, 1f), 1f);

            tex.SetPixel(sx, sy, starColor);

            // Some stars are larger (2x2)
            if (Random.value > 0.7f)
            {
                if (sx + 1 < width) tex.SetPixel(sx + 1, sy, starColor * 0.7f);
                if (sy + 1 < height) tex.SetPixel(sx, sy + 1, starColor * 0.7f);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), width / 20f);
    }

    /// <summary>Create a hexagon sprite (for power-ups).</summary>
    public static Sprite CreateHexagon(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex);

        float center = size / 2f;
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                // Hexagon distance function
                float angle = Mathf.Atan2(dy, dx);
                float hexDist = Mathf.Cos(Mathf.PI / 6f) /
                    Mathf.Cos(angle - Mathf.PI / 3f * Mathf.Floor(angle / (Mathf.PI / 3f) + 0.5f));
                float dist = new Vector2(dx, dy).magnitude;

                if (dist <= radius * hexDist)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size / 2f);
    }

    // ========================================================================
    // Helper
    // ========================================================================
    private static void ClearTexture(Texture2D tex)
    {
        Color[] clear = new Color[tex.width * tex.height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;
        tex.SetPixels(clear);
    }
}
