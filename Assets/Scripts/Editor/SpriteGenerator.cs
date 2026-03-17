using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility that generates simple placeholder sprites for the game.
/// Access via menu: Tools > Space Shooter > Generate Sprites
/// </summary>
public class SpriteGenerator : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Generate All Sprites")]
    public static void GenerateAllSprites()
    {
        string folder = "Assets/Sprites";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        CreatePlayerSprite(folder);
        CreateEnemySprite(folder, "Enemy_Basic", new Color(0.9f, 0.2f, 0.2f));
        CreateEnemySprite(folder, "Enemy_Fast",  new Color(1f, 0.5f, 0.1f));
        CreateEnemySprite(folder, "Enemy_Tank",  new Color(0.6f, 0.1f, 0.8f));
        CreateBulletSprite(folder);
        CreatePowerUpSprite(folder, "PowerUp_Weapon", new Color(1f, 0.6f, 0f));
        CreatePowerUpSprite(folder, "PowerUp_Health", new Color(0f, 1f, 0.3f));
        CreatePowerUpSprite(folder, "PowerUp_Shield", new Color(0.3f, 0.6f, 1f));
        CreateBackgroundSprite(folder);

        AssetDatabase.Refresh();
        Debug.Log("[SpriteGenerator] All placeholder sprites generated in " + folder);
    }

    // ── Player ship ──────────────────────────────────────────────────
    private static void CreatePlayerSprite(string folder)
    {
        int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h);
        Color bg = new Color(0, 0, 0, 0);
        Color hull = new Color(0.2f, 0.6f, 1f);
        Color cockpit = new Color(0.8f, 0.9f, 1f);

        FillTexture(tex, bg);

        // Triangle ship shape
        for (int y = 0; y < h; y++)
        {
            int halfWidth = (y * w / 2) / h;
            for (int x = w / 2 - halfWidth; x <= w / 2 + halfWidth; x++)
            {
                if (x >= 0 && x < w)
                    tex.SetPixel(x, y, hull);
            }
        }
        // Cockpit dot
        for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
                if (dx * dx + dy * dy <= 4)
                    tex.SetPixel(w / 2 + dx, h / 2 + dy, cockpit);

        // Wing accents
        for (int i = 0; i < 4; i++)
        {
            tex.SetPixel(w / 2 - 6 + i, 8, Color.cyan);
            tex.SetPixel(w / 2 + 3 + i, 8, Color.cyan);
        }

        SaveSprite(tex, folder + "/Player.png");
    }

    // ── Enemy ship ───────────────────────────────────────────────────
    private static void CreateEnemySprite(string folder, string name, Color color)
    {
        int w = 28, h = 28;
        Texture2D tex = new Texture2D(w, h);
        FillTexture(tex, new Color(0, 0, 0, 0));

        // Inverted triangle (points down)
        for (int y = 0; y < h; y++)
        {
            int halfWidth = ((h - y) * w / 2) / h;
            for (int x = w / 2 - halfWidth; x <= w / 2 + halfWidth; x++)
            {
                if (x >= 0 && x < w)
                    tex.SetPixel(x, y, color);
            }
        }
        // Eyes
        tex.SetPixel(w / 2 - 3, h - 8, Color.yellow);
        tex.SetPixel(w / 2 + 3, h - 8, Color.yellow);

        SaveSprite(tex, folder + "/" + name + ".png");
    }

    // ── Bullet ───────────────────────────────────────────────────────
    private static void CreateBulletSprite(string folder)
    {
        int w = 6, h = 12;
        Texture2D tex = new Texture2D(w, h);
        FillTexture(tex, new Color(0, 0, 0, 0));

        for (int y = 0; y < h; y++)
            for (int x = 1; x < w - 1; x++)
                tex.SetPixel(x, y, Color.white);

        // Bright center
        for (int y = 2; y < h - 2; y++)
            for (int x = 2; x < w - 2; x++)
                tex.SetPixel(x, y, new Color(1f, 1f, 0.6f));

        SaveSprite(tex, folder + "/Bullet.png");
    }

    // ── Power-up ─────────────────────────────────────────────────────
    private static void CreatePowerUpSprite(string folder, string name, Color color)
    {
        int size = 20;
        Texture2D tex = new Texture2D(size, size);
        FillTexture(tex, new Color(0, 0, 0, 0));

        int r = size / 2 - 1;
        int cx = size / 2, cy = size / 2;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist < r)
                {
                    float t = 1f - dist / r;
                    tex.SetPixel(x, y, Color.Lerp(color, Color.white, t * 0.4f));
                }
            }

        SaveSprite(tex, folder + "/" + name + ".png");
    }

    // ── Background ───────────────────────────────────────────────────
    private static void CreateBackgroundSprite(string folder)
    {
        int w = 256, h = 512;
        Texture2D tex = new Texture2D(w, h);

        // Dark space gradient
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            Color bg = Color.Lerp(new Color(0.02f, 0.02f, 0.08f), new Color(0.05f, 0.02f, 0.12f), t);
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, bg);
        }

        // Scatter stars
        System.Random rng = new System.Random(42);
        for (int i = 0; i < 200; i++)
        {
            int x = rng.Next(w);
            int y = rng.Next(h);
            float brightness = 0.5f + (float)rng.NextDouble() * 0.5f;
            Color star = new Color(brightness, brightness, brightness + 0.1f);
            tex.SetPixel(x, y, star);
            // Some brighter/bigger stars
            if (rng.NextDouble() > 0.7)
            {
                if (x + 1 < w) tex.SetPixel(x + 1, y, star * 0.7f);
                if (y + 1 < h) tex.SetPixel(x, y + 1, star * 0.7f);
            }
        }

        SaveSprite(tex, folder + "/Background.png");
    }

    // ── Helpers ──────────────────────────────────────────────────────
    private static void FillTexture(Texture2D tex, Color c)
    {
        Color[] pixels = new Color[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
        tex.SetPixels(pixels);
    }

    private static void SaveSprite(Texture2D tex, string path)
    {
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(path, png);

        // Import settings will be applied after AssetDatabase.Refresh
        Debug.Log("  Created sprite: " + path);
    }
}
