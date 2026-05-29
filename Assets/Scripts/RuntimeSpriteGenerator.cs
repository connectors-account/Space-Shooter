using UnityEngine;

/// <summary>
/// Generates simple geometric sprites at runtime.
/// Used as a fallback when pre-made sprites are not available.
/// Call from any script to get sprites programmatically.
/// </summary>
public static class RuntimeSpriteGenerator
{
    /// <summary>
    /// Create a simple triangle sprite (for player ship).
    /// </summary>
    public static Sprite CreatePlayerShipSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        ClearTexture(tex);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);
                float halfWidth = 0.5f * (1f - ny * 0.8f);
                if (ny > -0.8f && ny < 0.9f && Mathf.Abs(nx) < halfWidth)
                {
                    float edgeDist = 1f - Mathf.Abs(nx) / halfWidth;
                    Color c = Color.Lerp(new Color(0.2f, 0.5f, 1f), new Color(0.4f, 0.8f, 1f), edgeDist);
                    if (ny > 0.2f && Mathf.Abs(nx) < 0.15f)
                        c = Color.Lerp(c, Color.cyan, 0.7f);
                    if (ny < -0.5f && Mathf.Abs(nx) < 0.2f)
                        c = Color.Lerp(c, new Color(1f, 0.5f, 0.1f), (-0.5f - ny) * 2f);
                    tex.SetPixel(x, y, c);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Create an enemy ship sprite with the given color.
    /// </summary>
    public static Sprite CreateEnemySprite(Color baseColor)
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        ClearTexture(tex);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);
                float halfWidth = 0.5f * (1f + ny * 0.7f);
                if (ny > -0.8f && ny < 0.8f && Mathf.Abs(nx) < halfWidth)
                {
                    float edgeDist = 1f - Mathf.Abs(nx) / halfWidth;
                    Color c = Color.Lerp(baseColor * 0.6f, baseColor, edgeDist);
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Create a bullet sprite.
    /// </summary>
    public static Sprite CreateBulletSprite(Color color, int size = 16)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        ClearTexture(tex);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - center.x) / (size / 2f);
                float ny = (y - center.y) / (size / 2f);
                float distSq = nx * nx * 4f + ny * ny;
                if (distSq < 0.8f)
                {
                    float intensity = 1f - distSq / 0.8f;
                    Color c = Color.Lerp(color, Color.white, intensity * 0.5f);
                    c.a = intensity;
                    tex.SetPixel(x, y, c);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Create a diamond-shaped power-up sprite.
    /// </summary>
    public static Sprite CreatePowerUpSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        ClearTexture(tex);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);
                if (Mathf.Abs(nx) + Mathf.Abs(ny) < 0.7f)
                {
                    float dist = Mathf.Abs(nx) + Mathf.Abs(ny);
                    float intensity = 1f - dist / 0.7f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, intensity));
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static void ClearTexture(Texture2D tex)
    {
        Color[] clear = new Color[tex.width * tex.height];
        tex.SetPixels(clear);
    }
}
