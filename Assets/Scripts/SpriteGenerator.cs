using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor utility to generate placeholder sprite textures for the Space Shooter game.
/// Run from Unity menu: Tools > Generate Placeholder Sprites
/// This creates simple colored shapes for all game objects.
/// </summary>
public class SpriteGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Generate Placeholder Sprites")]
    public static void GenerateAllSprites()
    {
        string basePath = "Assets/Sprites";

        // Player ship - blue triangle pointing up
        CreateSprite(basePath + "/Player/PlayerShip.png", 64, 64, (x, y) =>
        {
            // Triangle shape
            float cx = 32, cy = 10;
            float halfWidth = 24f * (1f - (float)y / 54f);
            if (y >= 10 && y <= 58 && Mathf.Abs(x - cx) <= halfWidth)
                return new Color(0.2f, 0.5f, 1f, 1f); // Blue
            if (y >= 10 && y <= 58 && Mathf.Abs(x - cx) <= halfWidth + 2)
                return new Color(0.4f, 0.7f, 1f, 1f); // Light blue edge
            return Color.clear;
        });

        // Basic Enemy - red diamond
        CreateSprite(basePath + "/Enemies/EnemyBasic.png", 48, 48, (x, y) =>
        {
            float cx = 24, cy = 24;
            float dist = Mathf.Abs(x - cx) + Mathf.Abs(y - cy);
            if (dist <= 18) return new Color(1f, 0.2f, 0.2f, 1f);
            if (dist <= 20) return new Color(1f, 0.4f, 0.4f, 1f);
            return Color.clear;
        });

        // Zigzag Enemy - orange hexagonal shape
        CreateSprite(basePath + "/Enemies/EnemyZigzag.png", 48, 48, (x, y) =>
        {
            float cx = 24, cy = 24;
            float dx = Mathf.Abs(x - cx);
            float dy = Mathf.Abs(y - cy);
            if (dx + dy * 0.5f <= 18 && dy <= 18)
                return new Color(1f, 0.6f, 0.1f, 1f); // Orange
            if (dx + dy * 0.5f <= 20 && dy <= 20)
                return new Color(1f, 0.8f, 0.3f, 1f);
            return Color.clear;
        });

        // Heavy Enemy - dark red square with details
        CreateSprite(basePath + "/Enemies/EnemyHeavy.png", 56, 56, (x, y) =>
        {
            float cx = 28, cy = 28;
            float dx = Mathf.Abs(x - cx);
            float dy = Mathf.Abs(y - cy);
            if (dx <= 22 && dy <= 22)
            {
                if (dx <= 4 || dy <= 4) return new Color(0.8f, 0.1f, 0.1f, 1f); // Cross detail
                return new Color(0.6f, 0.1f, 0.15f, 1f); // Dark red
            }
            if (dx <= 24 && dy <= 24) return new Color(0.9f, 0.2f, 0.2f, 1f);
            return Color.clear;
        });

        // Player Bullet - cyan elongated dot
        CreateSprite(basePath + "/Bullets/PlayerBullet.png", 8, 16, (x, y) =>
        {
            float cx = 4, cy = 8;
            float dx = (x - cx) / 3f;
            float dy = (y - cy) / 7f;
            if (dx * dx + dy * dy <= 1f)
                return new Color(0.3f, 1f, 1f, 1f); // Cyan
            return Color.clear;
        });

        // Enemy Bullet - red elongated dot
        CreateSprite(basePath + "/Bullets/EnemyBullet.png", 8, 16, (x, y) =>
        {
            float cx = 4, cy = 8;
            float dx = (x - cx) / 3f;
            float dy = (y - cy) / 7f;
            if (dx * dx + dy * dy <= 1f)
                return new Color(1f, 0.3f, 0.3f, 1f); // Red
            return Color.clear;
        });

        // Weapon Upgrade Power-up - yellow star shape
        CreateSprite(basePath + "/PowerUps/PowerUpWeapon.png", 32, 32, (x, y) =>
        {
            float cx = 16, cy = 16;
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            if (dist <= 12)
                return new Color(1f, 1f, 0.2f, 1f); // Yellow
            if (dist <= 14)
                return new Color(1f, 1f, 0.5f, 0.7f);
            return Color.clear;
        });

        // Shield Power-up - blue circle
        CreateSprite(basePath + "/PowerUps/PowerUpShield.png", 32, 32, (x, y) =>
        {
            float cx = 16, cy = 16;
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            if (dist <= 10)
                return new Color(0.3f, 0.5f, 1f, 0.8f);
            if (dist <= 13)
                return new Color(0.4f, 0.7f, 1f, 1f); // Ring
            if (dist <= 14)
                return new Color(0.4f, 0.7f, 1f, 0.5f);
            return Color.clear;
        });

        // Health Power-up - green cross
        CreateSprite(basePath + "/PowerUps/PowerUpHealth.png", 32, 32, (x, y) =>
        {
            float cx = 16, cy = 16;
            bool inCross = (Mathf.Abs(x - cx) <= 5 && Mathf.Abs(y - cy) <= 12) ||
                           (Mathf.Abs(y - cy) <= 5 && Mathf.Abs(x - cx) <= 12);
            if (inCross) return new Color(0.2f, 1f, 0.3f, 1f); // Green
            return Color.clear;
        });

        // Background Layer 1 (stars - far)
        CreateSprite(basePath + "/Background/BackgroundStarsFar.png", 512, 1024, (x, y) =>
        {
            Color bg = new Color(0.02f, 0.02f, 0.08f, 1f); // Dark space
            // Pseudo-random stars
            int hash = (x * 374761393 + y * 668265263) ^ (x * 1274126177);
            hash = (hash >> 13) ^ hash;
            if ((hash & 0x3FF) < 3) // ~0.3% chance of star
                return new Color(0.7f, 0.7f, 0.8f, 0.5f); // Dim star
            return bg;
        });

        // Background Layer 2 (stars - near)
        CreateSprite(basePath + "/Background/BackgroundStarsNear.png", 512, 1024, (x, y) =>
        {
            // Pseudo-random brighter stars on transparent bg
            int hash = (x * 123456789 + y * 987654321) ^ (x * 456789123);
            hash = (hash >> 13) ^ hash;
            if ((hash & 0x7FF) < 2) // ~0.1% chance of star
                return new Color(1f, 1f, 1f, 0.9f); // Bright star
            return Color.clear;
        });

        AssetDatabase.Refresh();
        Debug.Log("Placeholder sprites generated successfully!");
    }

    /// <summary>
    /// Create and save a PNG sprite texture using a pixel color function.
    /// </summary>
    private static void CreateSprite(string path, int width, int height, System.Func<int, int, Color> colorFunc)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, colorFunc(x, y));
            }
        }

        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        string fullPath = System.IO.Path.Combine(Application.dataPath, "..", path);
        string directory = System.IO.Path.GetDirectoryName(fullPath);
        if (!System.IO.Directory.Exists(directory))
            System.IO.Directory.CreateDirectory(directory);

        System.IO.File.WriteAllBytes(fullPath, pngData);
        DestroyImmediate(tex);
    }
#endif
}
