using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor utility that generates simple placeholder sprites for the game.
/// Run from Unity menu: Tools > Generate Placeholder Sprites.
/// Creates colored geometric shapes as Texture2D assets.
/// </summary>
public class SpriteGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Generate Placeholder Sprites")]
    public static void GenerateAllSprites()
    {
        string spritePath = "Assets/Sprites/";

        // Ensure directory exists
        if (!System.IO.Directory.Exists(spritePath))
            System.IO.Directory.CreateDirectory(spritePath);

        // Player Ship (green triangle pointing up)
        CreateTriangleSprite(spritePath + "PlayerShip.png", 32, 32,
            new Color(0.2f, 0.9f, 0.3f), true);

        // Basic Enemy (red triangle pointing down)
        CreateTriangleSprite(spritePath + "EnemyBasic.png", 28, 28,
            new Color(0.9f, 0.2f, 0.2f), false);

        // Fast Enemy (orange diamond)
        CreateDiamondSprite(spritePath + "EnemyFast.png", 24, 24,
            new Color(1f, 0.6f, 0.1f));

        // Tank Enemy (dark red large square)
        CreateSquareSprite(spritePath + "EnemyTank.png", 36, 36,
            new Color(0.6f, 0.1f, 0.1f));

        // Shooter Enemy (purple triangle pointing down)
        CreateTriangleSprite(spritePath + "EnemyShooter.png", 30, 30,
            new Color(0.7f, 0.2f, 0.9f), false);

        // Player Bullet (white thin rectangle)
        CreateRectSprite(spritePath + "BulletPlayer.png", 4, 12,
            new Color(1f, 1f, 0.5f));

        // Enemy Bullet (red small circle)
        CreateCircleSprite(spritePath + "BulletEnemy.png", 8, 8,
            new Color(1f, 0.3f, 0.3f));

        // Power-Up Weapon (yellow "W")
        CreateSquareSprite(spritePath + "PowerUpWeapon.png", 20, 20,
            new Color(1f, 1f, 0.2f));

        // Power-Up Shield (cyan "S")
        CreateCircleSprite(spritePath + "PowerUpShield.png", 20, 20,
            new Color(0.3f, 0.8f, 1f));

        // Power-Up Health (green "+")
        CreateCrossSprite(spritePath + "PowerUpHealth.png", 20, 20,
            new Color(0.2f, 1f, 0.3f));

        // Star (white tiny dot for background)
        CreateCircleSprite(spritePath + "Star.png", 4, 4,
            Color.white);

        // Background (dark blue)
        CreateSquareSprite(spritePath + "Background.png", 64, 64,
            new Color(0.02f, 0.02f, 0.1f));

        AssetDatabase.Refresh();
        Debug.Log("All placeholder sprites generated in Assets/Sprites/");
    }

    private static void CreateTriangleSprite(string path, int width, int height, Color color, bool pointUp)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            int row = pointUp ? y : (height - 1 - y);
            float progress = (float)row / height;
            int halfWidth = Mathf.RoundToInt(progress * width / 2f);
            int centerX = width / 2;

            for (int x = 0; x < width; x++)
            {
                if (x >= centerX - halfWidth && x <= centerX + halfWidth)
                    pixels[y * width + x] = color;
                else
                    pixels[y * width + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateSquareSprite(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateRectSprite(string path, int width, int height, Color color)
    {
        CreateSquareSprite(path, width, height, color);
    }

    private static void CreateCircleSprite(string path, int size, int height, Color color)
    {
        Texture2D tex = new Texture2D(size, height);
        Color[] pixels = new Color[size * height];
        float cx = size / 2f;
        float cy = height / 2f;
        float rx = size / 2f;
        float ry = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateDiamondSprite(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        float cx = width / 2f;
        float cy = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Abs(x - cx) / cx;
                float dy = Mathf.Abs(y - cy) / cy;
                if (dx + dy <= 1f)
                    pixels[y * width + x] = color;
                else
                    pixels[y * width + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateCrossSprite(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        float cx = width / 2f;
        float cy = height / 2f;
        float armWidth = width * 0.3f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inHorizontal = Mathf.Abs(y - cy) <= armWidth / 2f;
                bool inVertical = Mathf.Abs(x - cx) <= armWidth / 2f;
                if (inHorizontal || inVertical)
                    pixels[y * width + x] = color;
                else
                    pixels[y * width + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }
#endif
}
