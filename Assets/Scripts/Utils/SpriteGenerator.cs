using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates simple geometric sprites at runtime.
/// All game visuals are procedurally generated — no external sprite assets needed.
/// </summary>
public static class SpriteGenerator
{
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    /// <summary>
    /// Creates a filled circle sprite.
    /// </summary>
    public static Sprite CreateCircleSprite(int radius, Color color)
    {
        string key = $"circle_{radius}_{color}";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                pixels[y * size + x] = dist < radius ? color : transparent;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Creates a player ship sprite (triangle/arrow shape).
    /// </summary>
    public static Sprite CreatePlayerShipSprite()
    {
        string key = "player_ship";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int w = 32, h = 40;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(0.2f, 0.6f, 1f);     // Blue hull
        Color cockpit = new Color(0.4f, 0.9f, 1f);   // Light blue cockpit
        Color wing = new Color(0.15f, 0.4f, 0.8f);   // Dark blue wings
        Color engine = new Color(1f, 0.5f, 0.1f);     // Orange engine glow

        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        // Ship body (symmetrical)
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int cx = x - w / 2;
                int cy = y;

                // Main body (diamond/arrow shape)
                float bodyWidth = (float)cy / h * (w / 2f - 2f);
                if (cy > h * 0.2f && cy < h * 0.95f && Mathf.Abs(cx) < bodyWidth)
                {
                    pixels[y * w + x] = hull;
                }

                // Nose cone
                float noseWidth = (1f - (float)cy / h) * 6f;
                if (cy >= h * 0.8f && Mathf.Abs(cx) < noseWidth)
                {
                    pixels[y * w + x] = cockpit;
                }

                // Wings (triangles at sides)
                if (cy > h * 0.15f && cy < h * 0.55f)
                {
                    float wingExtent = (h * 0.55f - cy) / (h * 0.4f) * (w / 2f);
                    if (Mathf.Abs(cx) >= bodyWidth - 1 && Mathf.Abs(cx) < wingExtent + bodyWidth)
                    {
                        pixels[y * w + x] = wing;
                    }
                }

                // Engine glow
                if (cy < h * 0.15f && Mathf.Abs(cx) < 4f)
                {
                    pixels[y * w + x] = engine;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Creates an enemy drone sprite (hexagonal shape).
    /// </summary>
    public static Sprite CreateEnemyDroneSprite()
    {
        string key = "enemy_drone";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int size = 28;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color body = new Color(0.8f, 0.2f, 0.2f);   // Red
        Color accent = new Color(1f, 0.4f, 0.1f);     // Orange accent

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        float center = size / 2f;
        float radius = size / 2f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Hexagonal shape approximation
                float angle = Mathf.Atan2(dy, dx);
                float hexDist = radius * 0.9f / Mathf.Cos(angle % (Mathf.PI / 3f) - Mathf.PI / 6f);

                if (dist < Mathf.Min(radius, hexDist))
                {
                    pixels[y * size + x] = dist < radius * 0.5f ? accent : body;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Creates an enemy fighter sprite (aggressive angular shape).
    /// </summary>
    public static Sprite CreateEnemyFighterSprite()
    {
        string key = "enemy_fighter";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int w = 30, h = 34;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color body = new Color(0.9f, 0.3f, 0.9f);    // Purple
        Color accent = new Color(1f, 0.5f, 1f);

        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int cx = x - w / 2;
                int cy = h - 1 - y; // Flip so point faces down

                // Inverted arrow shape
                float bodyWidth = (float)cy / h * (w / 2f - 2f);
                if (cy > h * 0.15f && cy < h * 0.9f && Mathf.Abs(cx) < bodyWidth)
                {
                    pixels[y * w + x] = Mathf.Abs(cx) < bodyWidth * 0.4f ? accent : body;
                }

                // Wings
                if (cy > h * 0.4f && cy < h * 0.75f)
                {
                    float wingExtent = (cy - h * 0.4f) / (h * 0.35f) * 6f;
                    if (Mathf.Abs(cx) >= bodyWidth - 1 && Mathf.Abs(cx) < bodyWidth + wingExtent)
                    {
                        pixels[y * w + x] = body;
                    }
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Creates an enemy bomber sprite (large, bulky shape).
    /// </summary>
    public static Sprite CreateEnemyBomberSprite()
    {
        string key = "enemy_bomber";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int size = 36;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color body = new Color(0.2f, 0.7f, 0.2f);    // Green
        Color dark = new Color(0.1f, 0.4f, 0.1f);

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;

                // Rounded rectangle
                float rx = Mathf.Abs(dx);
                float ry = Mathf.Abs(dy);

                if (rx < 0.8f && ry < 0.7f)
                {
                    // Rounded corners
                    if (rx > 0.6f && ry > 0.5f)
                    {
                        float cornerDist = Mathf.Sqrt(Mathf.Pow(rx - 0.6f, 2) + Mathf.Pow(ry - 0.5f, 2));
                        if (cornerDist > 0.25f) continue;
                    }
                    pixels[y * size + x] = (rx + ry < 0.5f) ? dark : body;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Creates an enemy swooper sprite (crescent/wing shape).
    /// </summary>
    public static Sprite CreateEnemySwooperSprite()
    {
        string key = "enemy_swooper";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int w = 32, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color body = new Color(1f, 0.6f, 0.1f);   // Orange
        Color accent = new Color(1f, 0.9f, 0.3f);

        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        float cx = w / 2f, cy = h / 2f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cy) / cy;

                // Wing/crescent shape
                float outerDist = dx * dx + dy * dy * 2f;
                float innerDist = dx * dx + (dy + 0.5f) * (dy + 0.5f) * 2f;

                if (outerDist < 1f && innerDist > 0.4f)
                {
                    pixels[y * w + x] = outerDist < 0.3f ? accent : body;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Creates a bullet sprite (elongated glow).
    /// </summary>
    public static Sprite CreateBulletSprite(Color color)
    {
        string key = $"bullet_{color}";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int w = 6, h = 14;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        float cx = w / 2f, cy = h / 2f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / (w / 2f);
                float dy = (y - cy) / (h / 2f);
                float dist = dx * dx + dy * dy;

                if (dist < 1f)
                {
                    float alpha = 1f - dist * 0.5f;
                    Color c = color;
                    if (dist < 0.3f)
                        c = Color.Lerp(Color.white, color, dist / 0.3f);
                    c.a = alpha;
                    pixels[y * w + x] = c;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Creates a diamond/gem shape for power-ups.
    /// </summary>
    public static Sprite CreatePowerUpSprite()
    {
        string key = "powerup";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int size = 20;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color fill = Color.white;

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        float center = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center);
                float dy = Mathf.Abs(y - center);

                // Diamond shape
                if (dx + dy < center - 1)
                {
                    float brightness = 1f - (dx + dy) / center;
                    pixels[y * size + x] = new Color(brightness, brightness, brightness, 1f);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Creates a shield bubble sprite.
    /// </summary>
    public static Sprite CreateShieldSprite()
    {
        string key = "shield";
        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        float center = size / 2f;
        float outerR = center - 1;
        float innerR = center - 4;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), Vector2.one * center);
                if (dist < outerR && dist > innerR)
                {
                    float alpha = 0.4f;
                    pixels[y * size + x] = new Color(0.3f, 0.6f, 1f, alpha);
                }
                else if (dist <= innerR)
                {
                    pixels[y * size + x] = new Color(0.3f, 0.6f, 1f, 0.08f);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
        spriteCache[key] = sprite;
        return sprite;
    }
}
