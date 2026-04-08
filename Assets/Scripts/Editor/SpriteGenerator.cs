#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to generate simple colored sprite textures for the game.
/// Run from Unity menu: Tools > Space Shooter > Generate Sprites
/// </summary>
public class SpriteGenerator : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Generate All Sprites")]
    public static void GenerateAllSprites()
    {
        string spritePath = "Assets/Sprites";
        if (!Directory.Exists(spritePath))
            Directory.CreateDirectory(spritePath);

        // Player Ship - Blue triangle/arrow pointing up
        CreatePlayerShip(spritePath);

        // Enemy Ships
        CreateStraightEnemy(spritePath);
        CreateZigzagEnemy(spritePath);
        CreateSwooperEnemy(spritePath);
        CreateTankEnemy(spritePath);

        // Bullets
        CreatePlayerBullet(spritePath);
        CreateEnemyBullet(spritePath);

        // Power-ups
        CreatePowerUpHealth(spritePath);
        CreatePowerUpRapidFire(spritePath);
        CreatePowerUpShield(spritePath);

        // Shield visual
        CreateShieldBubble(spritePath);

        // Background stars
        CreateStarBackground(spritePath);

        AssetDatabase.Refresh();
        Debug.Log("All sprites generated successfully in Assets/Sprites/");
    }

    private static void CreatePlayerShip(string path)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        // Draw a blue arrow/ship shape
        Color shipColor = new Color(0.2f, 0.5f, 1f);
        Color cockpitColor = new Color(0.4f, 0.8f, 1f);

        // Main body
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Triangle shape
                float width = (1f - ny) * 0.6f;
                if (ny > -0.8f && ny < 0.9f && Mathf.Abs(nx) < width)
                {
                    tex.SetPixel(x, y, shipColor);
                }

                // Cockpit highlight
                if (ny > 0.1f && ny < 0.7f && Mathf.Abs(nx) < 0.15f)
                {
                    tex.SetPixel(x, y, cockpitColor);
                }

                // Wing tips
                float wingY = ny + 0.3f;
                if (wingY > -0.5f && wingY < -0.2f)
                {
                    float wingWidth = 0.8f + wingY * 0.5f;
                    if (Mathf.Abs(nx) > width * 0.5f && Mathf.Abs(nx) < wingWidth)
                    {
                        tex.SetPixel(x, y, new Color(0.15f, 0.35f, 0.8f));
                    }
                }
            }
        }

        SaveSprite(tex, path + "/PlayerShip.png", size);
    }

    private static void CreateStraightEnemy(string path)
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color bodyColor = new Color(1f, 0.3f, 0.3f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Inverted triangle (pointing down)
                float width = (1f + ny) * 0.5f;
                if (ny > -0.8f && ny < 0.7f && Mathf.Abs(nx) < width)
                {
                    tex.SetPixel(x, y, bodyColor);
                }
            }
        }

        SaveSprite(tex, path + "/EnemyStraight.png", size);
    }

    private static void CreateZigzagEnemy(string path)
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color bodyColor = new Color(1f, 0.6f, 0.1f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Diamond shape
                if (Mathf.Abs(nx) + Mathf.Abs(ny) < 0.75f)
                {
                    tex.SetPixel(x, y, bodyColor);
                }
            }
        }

        SaveSprite(tex, path + "/EnemyZigzag.png", size);
    }

    private static void CreateSwooperEnemy(string path)
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color bodyColor = new Color(0.8f, 0.2f, 0.8f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Crescent / arc shape
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                if (dist < 0.7f && dist > 0.3f && ny < 0.3f)
                {
                    tex.SetPixel(x, y, bodyColor);
                }
                // Center dot
                if (dist < 0.25f)
                {
                    tex.SetPixel(x, y, new Color(1f, 0.5f, 1f));
                }
            }
        }

        SaveSprite(tex, path + "/EnemySwooper.png", size);
    }

    private static void CreateTankEnemy(string path)
    {
        int size = 56;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color bodyColor = new Color(0.5f, 0.5f, 0.5f);
        Color armorColor = new Color(0.7f, 0.2f, 0.2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Large rectangle with rounded corners
                if (Mathf.Abs(nx) < 0.65f && Mathf.Abs(ny) < 0.55f)
                {
                    tex.SetPixel(x, y, bodyColor);
                }
                // Armor plating
                if (Mathf.Abs(nx) < 0.5f && ny > -0.3f && ny < 0.3f)
                {
                    tex.SetPixel(x, y, armorColor);
                }
            }
        }

        SaveSprite(tex, path + "/EnemyTank.png", size);
    }

    private static void CreatePlayerBullet(string path)
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);
                float dist = Mathf.Sqrt(nx * nx * 4f + ny * ny);

                if (dist < 0.8f)
                {
                    float glow = 1f - dist;
                    tex.SetPixel(x, y, new Color(0.5f + glow * 0.5f, 1f, 0.5f + glow * 0.5f, 1f));
                }
            }
        }

        SaveSprite(tex, path + "/PlayerBullet.png", size);
    }

    private static void CreateEnemyBullet(string path)
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);
                float dist = Mathf.Sqrt(nx * nx * 4f + ny * ny);

                if (dist < 0.8f)
                {
                    float glow = 1f - dist;
                    tex.SetPixel(x, y, new Color(1f, 0.3f + glow * 0.3f, 0.2f, 1f));
                }
            }
        }

        SaveSprite(tex, path + "/EnemyBullet.png", size);
    }

    private static void CreatePowerUpHealth(string path)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color color = new Color(0.2f, 1f, 0.3f);
        // Cross/plus shape
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                // Circle background
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                if (dist < 0.8f)
                {
                    tex.SetPixel(x, y, new Color(0.1f, 0.4f, 0.1f));
                }

                // Plus sign
                if ((Mathf.Abs(nx) < 0.15f && Mathf.Abs(ny) < 0.5f) ||
                    (Mathf.Abs(ny) < 0.15f && Mathf.Abs(nx) < 0.5f))
                {
                    if (dist < 0.7f)
                        tex.SetPixel(x, y, color);
                }
            }
        }

        SaveSprite(tex, path + "/PowerUpHealth.png", size);
    }

    private static void CreatePowerUpRapidFire(string path)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color color = new Color(1f, 1f, 0.2f);
        // Lightning bolt shape
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                if (dist < 0.8f)
                {
                    tex.SetPixel(x, y, new Color(0.4f, 0.4f, 0.1f));
                }

                // Arrows pointing up
                if (ny > 0f && Mathf.Abs(nx) < ny * 0.5f && ny < 0.6f)
                {
                    tex.SetPixel(x, y, color);
                }
                if (Mathf.Abs(nx) < 0.1f && ny > -0.5f && ny < 0.6f)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }

        SaveSprite(tex, path + "/PowerUpRapidFire.png", size);
    }

    private static void CreatePowerUpShield(string path)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color color = new Color(0.3f, 0.6f, 1f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);

                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                // Shield circle outline
                if (dist < 0.8f && dist > 0.55f)
                {
                    tex.SetPixel(x, y, color);
                }
                // Inner S
                if (dist < 0.4f)
                {
                    tex.SetPixel(x, y, new Color(0.2f, 0.4f, 0.8f));
                }
            }
        }

        SaveSprite(tex, path + "/PowerUpShield.png", size);
    }

    private static void CreateShieldBubble(string path)
    {
        int size = 80;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                // Translucent blue circle
                if (dist < 0.9f && dist > 0.7f)
                {
                    float alpha = (0.9f - dist) / 0.2f;
                    tex.SetPixel(x, y, new Color(0.3f, 0.6f, 1f, alpha * 0.6f));
                }
                else if (dist < 0.7f)
                {
                    tex.SetPixel(x, y, new Color(0.3f, 0.6f, 1f, 0.1f));
                }
            }
        }

        SaveSprite(tex, path + "/ShieldBubble.png", size);
    }

    private static void CreateStarBackground(string path)
    {
        int width = 512;
        int height = 1024;
        Texture2D tex = new Texture2D(width, height);

        // Dark space background
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = Random.value * 0.02f;
                tex.SetPixel(x, y, new Color(0.02f + noise, 0.02f + noise, 0.05f + noise));
            }
        }

        // Add stars
        int starCount = 200;
        for (int i = 0; i < starCount; i++)
        {
            int sx = Random.Range(0, width);
            int sy = Random.Range(0, height);
            float brightness = Random.Range(0.5f, 1f);
            int starSize = Random.Range(1, 3);

            Color starColor;
            float colorRoll = Random.value;
            if (colorRoll < 0.7f)
                starColor = new Color(brightness, brightness, brightness);
            else if (colorRoll < 0.85f)
                starColor = new Color(brightness, brightness * 0.8f, brightness * 0.5f);
            else
                starColor = new Color(brightness * 0.5f, brightness * 0.7f, brightness);

            for (int dy = -starSize; dy <= starSize; dy++)
            {
                for (int dx = -starSize; dx <= starSize; dx++)
                {
                    int px = sx + dx;
                    int py = sy + dy;
                    if (px >= 0 && px < width && py >= 0 && py < height)
                    {
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist <= starSize)
                        {
                            float falloff = 1f - (dist / (starSize + 1f));
                            Color c = starColor * falloff;
                            c.a = 1f;
                            tex.SetPixel(px, py, c);
                        }
                    }
                }
            }
        }

        SaveSprite(tex, path + "/StarBackground.png", width);
    }

    private static void ClearTexture(Texture2D tex, Color color)
    {
        Color[] pixels = new Color[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
    }

    private static void SaveSprite(Texture2D tex, string path, int pixelsPerUnit)
    {
        tex.Apply();
        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(path, pngData);
        Object.DestroyImmediate(tex);
        Debug.Log("Created sprite: " + path);
    }
}
#endif
