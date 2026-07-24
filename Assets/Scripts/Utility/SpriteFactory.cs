// ============================================================
//  SpriteFactory.cs  –  Generates all game sprites at runtime
//  No external art files required.
//
//  Usage: SpriteFactory.CreateXxx() returns a Sprite.
//         Call in Awake/Start and assign to SpriteRenderer.sprite.
// ============================================================
using UnityEngine;

public static class SpriteFactory
{
    // ── Public API ───────────────────────────────────────────

    public static Sprite CreatePlayerShip()
    {
        int w = 32, h = 40;
        var tex = Blank(w, h);

        // Body – triangle pointing up
        Color hull  = new Color(0.3f, 0.7f, 1f);
        Color engine = new Color(0.1f, 0.4f, 0.9f);
        Color cockpit = new Color(0.8f, 0.95f, 1f);

        DrawFilledTriangle(tex, new Vector2Int(w/2, h-1),
                                new Vector2Int(2,    0),
                                new Vector2Int(w-2,  0), hull);

        // Engine glow
        DrawFilledRect(tex, w/2-4, 0, 8, 6, engine);

        // Cockpit
        DrawFilledEllipse(tex, w/2, h*6/10, 5, 6, cockpit);

        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateBasicEnemy()
    {
        int w = 28, h = 24;
        var tex = Blank(w, h);
        Color body = new Color(0.9f, 0.3f, 0.3f);
        Color wing = new Color(0.7f, 0.1f, 0.1f);

        // Wings
        DrawFilledTriangle(tex, new Vector2Int(0, h/2),
                                new Vector2Int(w/2, h-1),
                                new Vector2Int(w/2, h/3), wing);
        DrawFilledTriangle(tex, new Vector2Int(w, h/2),
                                new Vector2Int(w/2, h-1),
                                new Vector2Int(w/2, h/3), wing);

        // Body
        DrawFilledEllipse(tex, w/2, h/2, 7, 10, body);
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateFastEnemy()
    {
        int w = 20, h = 30;
        var tex = Blank(w, h);
        Color c = new Color(1f, 0.6f, 0.1f);
        DrawFilledTriangle(tex, new Vector2Int(w/2, h-1),
                                new Vector2Int(0,    0),
                                new Vector2Int(w,    0), c);
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateHeavyEnemy()
    {
        int w = 40, h = 36;
        var tex = Blank(w, h);
        Color c = new Color(0.5f, 0.3f, 0.8f);
        DrawFilledRect(tex, 4, 4, w-8, h-8, c);
        DrawFilledEllipse(tex, w/2, h/2, 10, 10, new Color(0.7f, 0.5f, 1f));
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateBoss()
    {
        int w = 64, h = 52;
        var tex = Blank(w, h);
        Color body = new Color(0.6f, 0.1f, 0.1f);
        Color detail = new Color(0.9f, 0.5f, 0.1f);

        DrawFilledRect(tex, 8, 4, w-16, h-8, body);

        // Cannons
        DrawFilledRect(tex, 0,    h/3, 12, 8, detail);
        DrawFilledRect(tex, w-12, h/3, 12, 8, detail);

        // Eye
        DrawFilledEllipse(tex, w/2, h*2/3, 10, 10, new Color(1f,0.2f,0.2f));
        DrawFilledEllipse(tex, w/2, h*2/3,  4,  4, Color.white);
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreatePlayerBullet()
    {
        int w = 4, h = 14;
        var tex = Blank(w, h);
        Color c = new Color(0.4f, 0.9f, 1f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float a = 1f - Mathf.Abs(x - w * 0.5f) / (w * 0.5f);
                tex.SetPixel(x, y, new Color(c.r, c.g, c.b, a));
            }
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateEnemyBullet()
    {
        int w = 6, h = 12;
        var tex = Blank(w, h);
        Color c = new Color(1f, 0.3f, 0.1f);
        DrawFilledEllipse(tex, w/2, h/2, 3, 5, c);
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreatePowerUpSprite()
    {
        int sz = 20;
        var tex = Blank(sz, sz);
        DrawFilledEllipse(tex, sz/2, sz/2, sz/2-2, sz/2-2, Color.white);
        // Inner star outline
        DrawFilledEllipse(tex, sz/2, sz/2, sz/4, sz/4, new Color(0,0,0,0));
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateShieldSprite()
    {
        int sz = 48;
        var tex = Blank(sz, sz);
        int r = sz/2 - 2;
        // Draw ring
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                int dx = x - sz/2, dy = y - sz/2;
                float dist = Mathf.Sqrt(dx*dx + dy*dy);
                if (dist >= r-3 && dist <= r)
                    tex.SetPixel(x, y, new Color(0.4f, 0.6f, 1f, 0.8f));
            }
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateExplosionSprite(int frame, int totalFrames)
    {
        int sz = 32;
        var tex = Blank(sz, sz);
        float t = (float)frame / totalFrames;
        Color inner = Color.Lerp(Color.yellow, new Color(1f, 0.4f, 0f), t);
        Color outer = Color.Lerp(Color.red,    new Color(0.2f,0,0,0),   t);

        int ri = (int)(sz * 0.5f * t * 0.6f) + 1;
        int ro = (int)(sz * 0.5f * t) + 2;
        DrawFilledEllipse(tex, sz/2, sz/2, ro, ro, outer);
        DrawFilledEllipse(tex, sz/2, sz/2, ri, ri, inner);
        tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateSolidRect(int w, int h, Color c)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point };
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        tex.SetPixels(px);
        tex.Apply();
        return ToSprite(tex);
    }

    // ── Drawing helpers ──────────────────────────────────────

    static Texture2D Blank(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point };
        Color[] px = new Color[w * h];
        tex.SetPixels(px);   // all transparent
        return tex;
    }

    static void DrawFilledRect(Texture2D tex, int x0, int y0, int w, int h, Color c)
    {
        for (int y = y0; y < y0 + h && y < tex.height; y++)
            for (int x = x0; x < x0 + w && x < tex.width; x++)
                tex.SetPixel(x, y, c);
    }

    static void DrawFilledEllipse(Texture2D tex, int cx, int cy, int rx, int ry, Color c)
    {
        for (int y = cy - ry; y <= cy + ry; y++)
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
                float nx = (float)(x - cx) / rx;
                float ny = (float)(y - cy) / ry;
                if (nx*nx + ny*ny <= 1f) tex.SetPixel(x, y, c);
            }
    }

    static void DrawFilledTriangle(Texture2D tex, Vector2Int a, Vector2Int b, Vector2Int c, Color col)
    {
        int minX = Mathf.Min(a.x, b.x, c.x);
        int maxX = Mathf.Max(a.x, b.x, c.x);
        int minY = Mathf.Min(a.y, b.y, c.y);
        int maxY = Mathf.Max(a.y, b.y, c.y);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
                if (PointInTriangle(new Vector2(x, y), a, b, c))
                    tex.SetPixel(x, y, col);
            }
    }

    static bool PointInTriangle(Vector2 p, Vector2Int a, Vector2Int b, Vector2Int c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    static float Sign(Vector2 p, Vector2 a, Vector2 b)
        => (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);

    static Sprite ToSprite(Texture2D tex)
        => Sprite.Create(tex,
                         new Rect(0, 0, tex.width, tex.height),
                         new Vector2(0.5f, 0.5f),
                         32f);
}
