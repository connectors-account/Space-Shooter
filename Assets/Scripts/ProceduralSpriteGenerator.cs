using UnityEngine;

/// <summary>
/// Generates simple procedural sprites at runtime so the game is playable
/// without importing any art assets. Attach to a GameObject in the scene and
/// it will auto-assign sprites to objects tagged appropriately.
///
/// Alternatively, call the static methods from other scripts.
/// </summary>
public class ProceduralSpriteGenerator : MonoBehaviour
{
    /// <summary>Create a colored triangle (player ship shape).</summary>
    public static Sprite CreatePlayerShip(int size = 32)
    {
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex);
        int cx = size / 2;
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            int halfWidth = Mathf.RoundToInt(t * (size / 2f));
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, new Color(0.2f, 0.8f, 1f));
            }
        }
        // Cockpit
        for (int y = size * 3 / 4; y < size; y++)
            for (int x = cx - 2; x <= cx + 2; x++)
                if (x >= 0 && x < size) tex.SetPixel(x, y, Color.white);

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Create a diamond-shaped enemy sprite.</summary>
    public static Sprite CreateEnemyShip(int size = 32, Color color = default)
    {
        if (color == default) color = Color.red;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex);
        int cx = size / 2, cy = size / 2;
        for (int y = 0; y < size; y++)
        {
            int dist = Mathf.Abs(y - cy);
            int halfW = (size / 2) - dist;
            for (int x = cx - halfW; x <= cx + halfW; x++)
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, color);
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Create a small bullet sprite.</summary>
    public static Sprite CreateBullet(int size = 8)
    {
        Texture2D tex = new Texture2D(size, size);
        float center = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, d < center ? Color.white : Color.clear);
            }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Create a star-shaped power-up sprite.</summary>
    public static Sprite CreatePowerUp(int size = 16)
    {
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex);
        int cx = size / 2;
        // Simple cross / plus shape
        for (int i = 0; i < size; i++)
        {
            tex.SetPixel(cx, i, Color.white);
            tex.SetPixel(i, cx, Color.white);
            if (i < size) { tex.SetPixel(i, i, Color.white); tex.SetPixel(i, size - 1 - i, Color.white); }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Create a starfield background sprite.</summary>
    public static Sprite CreateStarfield(int width = 256, int height = 512, int starCount = 120)
    {
        Texture2D tex = new Texture2D(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, new Color(0.02f, 0.02f, 0.08f));

        for (int i = 0; i < starCount; i++)
        {
            int sx = Random.Range(0, width);
            int sy = Random.Range(0, height);
            float brightness = Random.Range(0.3f, 1f);
            Color c = new Color(brightness, brightness, brightness);
            tex.SetPixel(sx, sy, c);
            // Some bigger stars
            if (Random.value > 0.7f)
            {
                if (sx + 1 < width) tex.SetPixel(sx + 1, sy, c * 0.5f);
                if (sy + 1 < height) tex.SetPixel(sx, sy + 1, c * 0.5f);
            }
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32);
    }

    static void ClearTexture(Texture2D tex)
    {
        Color[] clear = new Color[tex.width * tex.height];
        tex.SetPixels(clear);
    }
}
