// This is an Editor script to generate placeholder sprites
// Place this in Assets/Editor folder
// Access via menu: Tools > Space Shooter > Create Placeholder Sprites

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreatePlaceholderSprites : MonoBehaviour
{
    [MenuItem("Tools/Space Shooter/Create Placeholder Sprites")]
    static void CreateSprites()
    {
        string spritePath = "Assets/Sprites/";
        
        // Ensure directory exists
        if (!Directory.Exists(spritePath))
        {
            Directory.CreateDirectory(spritePath);
        }

        // Create Player sprite (cyan triangle)
        CreateTriangleSprite(spritePath + "Player.png", 64, 64, new Color(0, 1, 1, 1));
        
        // Create Player Bullet (yellow rectangle)
        CreateRectSprite(spritePath + "PlayerBullet.png", 8, 16, new Color(1, 1, 0, 1));
        
        // Create Enemy Bullet (red rectangle)
        CreateRectSprite(spritePath + "EnemyBullet.png", 8, 16, new Color(1, 0, 0, 1));
        
        // Create Basic Enemy (red square)
        CreateSquareSprite(spritePath + "EnemyBasic.png", 48, new Color(1, 0.25f, 0.25f, 1));
        
        // Create Fast Enemy (cyan small square)
        CreateSquareSprite(spritePath + "EnemyFast.png", 32, new Color(0, 1, 1, 1));
        
        // Create Tank Enemy (purple large square)
        CreateSquareSprite(spritePath + "EnemyTank.png", 64, new Color(0.5f, 0, 1, 1));
        
        // Create Boss (dark red large square)
        CreateSquareSprite(spritePath + "Boss.png", 128, new Color(0.5f, 0, 0, 1));
        
        // Create Power-ups
        CreateCircleSprite(spritePath + "PowerUpWeapon.png", 32, new Color(1, 0.5f, 0, 1));
        CreateCircleSprite(spritePath + "PowerUpHealth.png", 32, new Color(0, 1, 0, 1));
        CreateCircleSprite(spritePath + "PowerUpShield.png", 32, new Color(0, 0.5f, 1, 1));

        AssetDatabase.Refresh();
        Debug.Log("Placeholder sprites created successfully!");
    }

    static void CreateTriangleSprite(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedY = (float)y / height;
                float halfWidth = (width / 2f) * (1f - normalizedY);
                float centerX = width / 2f;
                
                if (x >= centerX - halfWidth && x <= centerX + halfWidth)
                {
                    pixels[y * width + x] = color;
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateRectSprite(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        SaveTexture(tex, path);
    }

    static void CreateSquareSprite(string path, int size, Color color)
    {
        CreateRectSprite(path, size, size, color);
    }

    static void CreateCircleSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f - 1;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                if (dist <= radius)
                {
                    pixels[y * size + x] = color;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        SaveTexture(tex, path);
    }

    static void SaveTexture(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        DestroyImmediate(tex);
    }
}
#endif
