using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to generate all game sprites as PNG files.
/// Run from Unity menu: Tools > Space Shooter > Generate All Sprites
/// Creates simple geometric sprites for all game objects.
/// </summary>
public class SpriteGenerator : EditorWindow
{
    private static string spritePath = "Assets/Sprites/";

    [MenuItem("Tools/Space Shooter/Generate All Sprites")]
    public static void GenerateAllSprites()
    {
        if (!Directory.Exists(spritePath))
        {
            Directory.CreateDirectory(spritePath);
        }

        GeneratePlayerShip();
        GenerateEnemyBasic();
        GenerateEnemyZigzag();
        GenerateEnemyTank();
        GenerateEnemyFast();
        GeneratePlayerBullet();
        GenerateEnemyBullet();
        GeneratePowerUpSprite();
        GenerateShieldSprite();

        AssetDatabase.Refresh();
        Debug.Log("All sprites generated successfully in " + spritePath);
    }

    // --- Player Ship: Arrow/triangle pointing up ---
    private static void GeneratePlayerShip()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        // Main body - pointed triangle
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Triangle shape pointing up
                float halfWidth = 0.5f * (1f - ny * 0.8f);
                if (ny > -0.8f && ny < 0.9f && Mathf.Abs(nx) < halfWidth)
                {
                    // Gradient from center to edges
                    float edgeDist = 1f - Mathf.Abs(nx) / halfWidth;
                    Color c = Color.Lerp(new Color(0.2f, 0.5f, 1f), new Color(0.4f, 0.8f, 1f), edgeDist);

                    // Cockpit glow
                    if (ny > 0.2f && Mathf.Abs(nx) < 0.15f)
                    {
                        c = Color.Lerp(c, Color.cyan, 0.7f);
                    }

                    // Engine glow at bottom
                    if (ny < -0.5f && Mathf.Abs(nx) < 0.2f)
                    {
                        c = Color.Lerp(c, new Color(1f, 0.5f, 0.1f), (-0.5f - ny) * 2f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
        }

        SaveTexture(tex, "PlayerShip");
    }

    // --- Basic Enemy: Inverted triangle ---
    private static void GenerateEnemyBasic()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Inverted triangle
                float halfWidth = 0.5f * (1f + ny * 0.7f);
                if (ny > -0.8f && ny < 0.8f && Mathf.Abs(nx) < halfWidth)
                {
                    float edgeDist = 1f - Mathf.Abs(nx) / halfWidth;
                    Color c = Color.Lerp(new Color(0.8f, 0.2f, 0.2f), new Color(1f, 0.4f, 0.3f), edgeDist);

                    // Eye/cockpit
                    if (ny > 0f && ny < 0.4f && Mathf.Abs(nx) < 0.2f)
                    {
                        c = new Color(1f, 1f, 0.3f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
        }

        SaveTexture(tex, "EnemyBasic");
    }

    // --- Zigzag Enemy: Diamond shape ---
    private static void GenerateEnemyZigzag()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Diamond shape
                if (Mathf.Abs(nx) + Mathf.Abs(ny) < 0.75f)
                {
                    float dist = Mathf.Abs(nx) + Mathf.Abs(ny);
                    Color c = Color.Lerp(new Color(0.9f, 0.5f, 0.9f), new Color(0.6f, 0.2f, 0.6f), dist / 0.75f);
                    tex.SetPixel(x, y, c);
                }
            }
        }

        SaveTexture(tex, "EnemyZigzag");
    }

    // --- Tank Enemy: Wide hexagonal shape ---
    private static void GenerateEnemyTank()
    {
        int size = 56;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Wide hexagonal shape
                float absX = Mathf.Abs(nx);
                float absY = Mathf.Abs(ny);
                if (absX < 0.7f && absY < 0.6f && (absX + absY * 0.5f) < 0.8f)
                {
                    float dist = absX + absY;
                    Color c = Color.Lerp(new Color(0.3f, 0.7f, 0.3f), new Color(0.15f, 0.4f, 0.15f), dist);

                    // Armor plates visual
                    if (Mathf.Abs(nx) > 0.3f && absY < 0.3f)
                    {
                        c = Color.Lerp(c, new Color(0.5f, 0.5f, 0.5f), 0.3f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
        }

        SaveTexture(tex, "EnemyTank");
    }

    // --- Fast Enemy: Narrow elongated shape ---
    private static void GenerateEnemyFast()
    {
        int size = 40;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Thin elongated triangle pointing down
                float halfWidth = 0.3f * (1f + ny * 0.9f);
                if (ny > -0.9f && ny < 0.9f && Mathf.Abs(nx) < halfWidth)
                {
                    Color c = Color.Lerp(new Color(1f, 0.6f, 0.1f), new Color(1f, 0.9f, 0.3f),
                        1f - Mathf.Abs(nx) / halfWidth);
                    tex.SetPixel(x, y, c);
                }
            }
        }

        SaveTexture(tex, "EnemyFast");
    }

    // --- Player Bullet: Small bright elongated shape ---
    private static void GeneratePlayerBullet()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - center.x) / (size / 2f);
                float ny = (y - center.y) / (size / 2f);

                // Elongated vertically
                float distSq = nx * nx * 4f + ny * ny;
                if (distSq < 0.8f)
                {
                    float intensity = 1f - distSq / 0.8f;
                    Color c = Color.Lerp(new Color(0.3f, 0.7f, 1f), Color.white, intensity);
                    c.a = intensity;
                    tex.SetPixel(x, y, c);
                }
            }
        }

        SaveTexture(tex, "PlayerBullet");
    }

    // --- Enemy Bullet: Small red/orange dot ---
    private static void GenerateEnemyBullet()
    {
        int size = 12;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                if (dist < 1f)
                {
                    float intensity = 1f - dist;
                    Color c = Color.Lerp(new Color(1f, 0.3f, 0.1f), new Color(1f, 1f, 0.5f), intensity);
                    c.a = intensity;
                    tex.SetPixel(x, y, c);
                }
            }
        }

        SaveTexture(tex, "EnemyBullet");
    }

    // --- Power-Up: Diamond/gem shape ---
    private static void GeneratePowerUpSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Diamond shape
                if (Mathf.Abs(nx) + Mathf.Abs(ny) < 0.7f)
                {
                    float dist = Mathf.Abs(nx) + Mathf.Abs(ny);
                    float intensity = 1f - dist / 0.7f;
                    Color c = new Color(1f, 1f, 1f, intensity); // White - tinted by PowerUpController
                    tex.SetPixel(x, y, c);
                }
            }
        }

        SaveTexture(tex, "PowerUp");
    }

    // --- Shield Visual: Circle outline ---
    private static void GenerateShieldSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                // Ring shape
                float ringDist = Mathf.Abs(dist - 0.75f);
                if (ringDist < 0.15f)
                {
                    float alpha = 1f - ringDist / 0.15f;
                    tex.SetPixel(x, y, new Color(0.3f, 0.8f, 1f, alpha * 0.6f));
                }
            }
        }

        SaveTexture(tex, "Shield");
    }

    // --- Utility Methods ---

    private static void ClearTexture(Texture2D tex)
    {
        Color[] clear = new Color[tex.width * tex.height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;
        tex.SetPixels(clear);
    }

    private static void SaveTexture(Texture2D tex, string name)
    {
        tex.Apply();
        byte[] pngData = tex.EncodeToPNG();
        string fullPath = spritePath + name + ".png";
        File.WriteAllBytes(fullPath, pngData);
        Debug.Log("Saved sprite: " + fullPath);
    }
}
