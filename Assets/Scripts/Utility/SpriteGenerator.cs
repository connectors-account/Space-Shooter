using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Procedural sprite generator for all game assets.
/// Run from the Unity Editor menu: Tools > Generate Game Sprites
/// Creates simple but recognizable sprites for player, enemies, bullets, power-ups, explosion, and background.
/// All sprites are saved to Assets/Sprites/.
/// </summary>
public static class SpriteGenerator
{
#if UNITY_EDITOR
    [MenuItem("Tools/Generate Game Sprites")]
    public static void GenerateAllSprites()
    {
        GeneratePlayerSprite();
        GenerateEnemyStraightSprite();
        GenerateEnemyZigzagSprite();
        GenerateEnemyTrackerSprite();
        GenerateBulletSprite();
        GeneratePowerUpSprite();
        GenerateExplosionSprite();
        GenerateShieldSprite();
        GenerateStarBackground();

        AssetDatabase.Refresh();
        Debug.Log("All game sprites generated in Assets/Sprites/");
    }

    static void SaveTexture(Texture2D tex, string filename)
    {
        byte[] pngData = tex.EncodeToPNG();
        string path = "Assets/Sprites/" + filename + ".png";
        System.IO.File.WriteAllBytes(Application.dataPath + "/../" + path, pngData);

        AssetDatabase.ImportAsset(path);

        // Set sprite import settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(path);
        }
    }

    static void GeneratePlayerSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(0.2f, 0.6f, 1f);    // Blue
        Color cockpit = new Color(0.4f, 0.9f, 1f);  // Light cyan
        Color engine = new Color(1f, 0.5f, 0.1f);   // Orange

        // Clear
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        // Draw triangular ship body (pointing up)
        for (int y = 4; y < 28; y++)
        {
            float progress = (float)(y - 4) / 24f;
            int halfWidth = (int)(Mathf.Lerp(8, 1, progress));
            int cx = 16;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, hull);
            }
        }

        // Cockpit (small bright area near top)
        for (int y = 20; y < 26; y++)
            for (int x = 14; x < 18; x++)
                tex.SetPixel(x, y, cockpit);

        // Engine glow at bottom
        for (int x = 12; x < 20; x++)
            tex.SetPixel(x, 3, engine);
        for (int x = 13; x < 19; x++)
            tex.SetPixel(x, 2, new Color(1f, 0.8f, 0.2f));

        // Wing accents
        for (int y = 6; y < 14; y++)
        {
            tex.SetPixel(8, y, cockpit);
            tex.SetPixel(23, y, cockpit);
        }

        tex.Apply();
        SaveTexture(tex, "Player");
        Object.DestroyImmediate(tex);
    }

    static void GenerateEnemyStraightSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(0.9f, 0.2f, 0.2f);   // Red
        Color accent = new Color(1f, 0.6f, 0.1f);    // Orange

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        // Inverted triangle (pointing down)
        for (int y = 4; y < 28; y++)
        {
            float progress = (float)(y - 4) / 24f;
            int halfWidth = (int)(Mathf.Lerp(1, 8, progress));
            int cx = 16;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, hull);
            }
        }

        // Eyes/windows
        tex.SetPixel(14, 20, accent);
        tex.SetPixel(18, 20, accent);
        tex.SetPixel(14, 21, accent);
        tex.SetPixel(18, 21, accent);

        tex.Apply();
        SaveTexture(tex, "EnemyStraight");
        Object.DestroyImmediate(tex);
    }

    static void GenerateEnemyZigzagSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(0.8f, 0.1f, 0.8f);   // Purple
        Color accent = new Color(1f, 0.4f, 1f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        // Diamond shape
        int cx = 16, cy = 16;
        for (int y = 6; y < 26; y++)
        {
            int dist = Mathf.Abs(y - cy);
            int halfWidth = 10 - dist;
            if (halfWidth < 0) continue;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, hull);
            }
        }

        // Inner accent
        for (int y = 12; y < 20; y++)
        {
            int dist = Mathf.Abs(y - cy);
            int halfWidth = 4 - dist;
            if (halfWidth < 0) continue;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, accent);
            }
        }

        tex.Apply();
        SaveTexture(tex, "EnemyZigzag");
        Object.DestroyImmediate(tex);
    }

    static void GenerateEnemyTrackerSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color hull = new Color(0.1f, 0.8f, 0.1f);   // Green
        Color accent = new Color(0.7f, 1f, 0.3f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        // Circular body
        int cx = 16, cy = 16, radius = 10;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist <= radius)
                    tex.SetPixel(x, y, hull);
                else if (dist <= radius + 1)
                    tex.SetPixel(x, y, accent);
            }
        }

        // "Eye" targeting reticle
        for (int i = -3; i <= 3; i++)
        {
            tex.SetPixel(cx + i, cy, accent);
            tex.SetPixel(cx, cy + i, accent);
        }

        tex.Apply();
        SaveTexture(tex, "EnemyTracker");
        Object.DestroyImmediate(tex);
    }

    static void GenerateBulletSprite()
    {
        int size = 8;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color core = Color.white;
        Color glow = new Color(0.5f, 1f, 1f, 0.8f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        // Elongated bullet (vertical)
        for (int y = 1; y < 7; y++)
        {
            tex.SetPixel(3, y, core);
            tex.SetPixel(4, y, core);
        }
        // Glow sides
        for (int y = 2; y < 6; y++)
        {
            tex.SetPixel(2, y, glow);
            tex.SetPixel(5, y, glow);
        }

        tex.Apply();
        SaveTexture(tex, "Bullet");
        Object.DestroyImmediate(tex);
    }

    static void GeneratePowerUpSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color fill = Color.white;  // Will be tinted by PowerUpController

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        // Diamond/star shape
        int cx = 8, cy = 8;
        for (int y = 2; y < 14; y++)
        {
            for (int x = 2; x < 14; x++)
            {
                int dx = Mathf.Abs(x - cx);
                int dy = Mathf.Abs(y - cy);
                if (dx + dy <= 6)
                    tex.SetPixel(x, y, fill);
            }
        }

        tex.Apply();
        SaveTexture(tex, "PowerUp");
        Object.DestroyImmediate(tex);
    }

    static void GenerateExplosionSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color core = new Color(1f, 1f, 0.5f, 1f);
        Color mid = new Color(1f, 0.6f, 0.1f, 0.8f);
        Color outer = new Color(1f, 0.2f, 0f, 0.4f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        int cx = 8, cy = 8;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist < 3) tex.SetPixel(x, y, core);
                else if (dist < 5) tex.SetPixel(x, y, mid);
                else if (dist < 7) tex.SetPixel(x, y, outer);
            }
        }

        tex.Apply();
        SaveTexture(tex, "Explosion");
        Object.DestroyImmediate(tex);
    }

    static void GenerateShieldSprite()
    {
        int size = 40;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        Color shield = new Color(0.3f, 0.7f, 1f, 0.4f);
        Color edge = new Color(0.5f, 0.9f, 1f, 0.8f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        int cx = 20, cy = 20;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist >= 14 && dist <= 17)
                    tex.SetPixel(x, y, edge);
                else if (dist >= 12 && dist < 14)
                    tex.SetPixel(x, y, shield);
            }
        }

        tex.Apply();
        SaveTexture(tex, "Shield");
        Object.DestroyImmediate(tex);
    }

    static void GenerateStarBackground()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size);
        Color bg = new Color(0.02f, 0.02f, 0.08f, 1f);

        // Fill background
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, bg);

        // Add random stars
        System.Random rng = new System.Random(42);
        for (int i = 0; i < 200; i++)
        {
            int x = rng.Next(0, size);
            int y = rng.Next(0, size);
            float brightness = 0.3f + (float)rng.NextDouble() * 0.7f;
            float starSize = (float)rng.NextDouble();

            Color starColor = new Color(brightness, brightness, brightness * 1.1f, 1f);
            tex.SetPixel(x, y, starColor);

            // Some bigger stars
            if (starSize > 0.8f)
            {
                if (x + 1 < size) tex.SetPixel(x + 1, y, starColor * 0.6f);
                if (y + 1 < size) tex.SetPixel(x, y + 1, starColor * 0.6f);
            }
        }

        // Add a few nebula-like colored patches
        for (int i = 0; i < 5; i++)
        {
            int cx = rng.Next(30, size - 30);
            int cy = rng.Next(30, size - 30);
            float r = 0.05f + (float)rng.NextDouble() * 0.1f;
            float g = 0.02f + (float)rng.NextDouble() * 0.05f;
            float b = 0.1f + (float)rng.NextDouble() * 0.15f;

            for (int dy = -20; dy <= 20; dy++)
            {
                for (int dx = -20; dx <= 20; dx++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < 20)
                    {
                        float falloff = 1f - (dist / 20f);
                        falloff *= falloff * 0.15f;
                        int px = cx + dx;
                        int py = cy + dy;
                        if (px >= 0 && px < size && py >= 0 && py < size)
                        {
                            Color existing = tex.GetPixel(px, py);
                            Color nebula = new Color(r * falloff, g * falloff, b * falloff, 0f);
                            tex.SetPixel(px, py, existing + nebula);
                        }
                    }
                }
            }
        }

        tex.Apply();

        // Save with Repeat wrap mode
        byte[] pngData = tex.EncodeToPNG();
        string path = "Assets/Sprites/StarBackground.png";
        System.IO.File.WriteAllBytes(Application.dataPath + "/../" + path, pngData);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(path);
        }

        Object.DestroyImmediate(tex);
    }
#endif
}
