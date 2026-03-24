using UnityEngine;

/// <summary>
/// SpriteGenerator creates simple pixel-art placeholder sprites at runtime.
/// Attach to any persistent GameObject. Other scripts can call the static
/// methods to get sprites for the player ship, enemies, bullets, and power-ups
/// without needing external art assets.
/// </summary>
public static class SpriteGenerator
{
    // ──────────────────────────────────────────────────────────
    // Player Ship – a small arrow/triangle shape
    // ──────────────────────────────────────────────────────────
    public static Sprite CreatePlayerShip()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex, Color.clear);

        // Draw a simple triangle / arrow pointing up
        Color c = new Color(0.2f, 0.8f, 1f); // cyan
        // Body
        for (int y = 0; y < 12; y++)
        {
            int halfWidth = y / 2 + 1;
            int cx = size / 2;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y + 2, c);
            }
        }
        // Nose
        tex.SetPixel(7, 14, Color.white);
        tex.SetPixel(8, 14, Color.white);
        tex.SetPixel(7, 15, Color.white);
        tex.SetPixel(8, 15, Color.white);
        // Wings
        for (int i = 0; i < 3; i++)
        {
            tex.SetPixel(2 + i, 3 + i, c);
            tex.SetPixel(13 - i, 3 + i, c);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    // ──────────────────────────────────────────────────────────
    // Enemy Ship – a small diamond/hexagon
    // ──────────────────────────────────────────────────────────
    public static Sprite CreateEnemyShip()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex, Color.clear);

        Color c = new Color(1f, 0.2f, 0.2f); // red
        int cx = size / 2;
        // Diamond shape
        for (int y = 0; y < size; y++)
        {
            int dist = (y < cx) ? y : (size - 1 - y);
            int halfW = dist / 2 + 1;
            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    // ──────────────────────────────────────────────────────────
    // Bullet – small rectangle
    // ──────────────────────────────────────────────────────────
    public static Sprite CreateBullet()
    {
        int w = 4, h = 8;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color c = Color.white;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 8f);
    }

    // ──────────────────────────────────────────────────────────
    // Power-Up – small circle-ish shape
    // ──────────────────────────────────────────────────────────
    public static Sprite CreatePowerUp()
    {
        int size = 12;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex, Color.clear);

        float r = size / 2f - 1f;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (Vector2.Distance(new Vector2(x, y), center) <= r)
                    tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 12f);
    }

    // ──────────────────────────────────────────────────────────
    // Shield – ring
    // ──────────────────────────────────────────────────────────
    public static Sprite CreateShield()
    {
        int size = 24;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex, Color.clear);

        float outerR = size / 2f - 1f;
        float innerR = outerR - 2f;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        Color c = new Color(0.3f, 0.6f, 1f, 0.5f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                if (d <= outerR && d >= innerR)
                    tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    // ──────────────────────────────────────────────────────────
    // Utility
    // ──────────────────────────────────────────────────────────
    private static void ClearTexture(Texture2D tex, Color c)
    {
        Color[] px = new Color[tex.width * tex.height];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        tex.SetPixels(px);
    }
}
