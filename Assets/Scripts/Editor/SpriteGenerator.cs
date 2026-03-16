#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// SpriteGenerator creates simple geometric sprites for the game.
/// Run from Unity Editor: Tools > Space Shooter > Generate Sprites
/// </summary>
public class SpriteGenerator : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Generate Sprites")]
    public static void ShowWindow()
    {
        GetWindow<SpriteGenerator>("Sprite Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Space Shooter Sprite Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generate All Sprites", GUILayout.Height(40)))
        {
            GenerateAllSprites();
        }

        GUILayout.Space(10);
        GUILayout.Label("Individual Sprites:", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Player Sprite"))
            GeneratePlayerSprite();

        if (GUILayout.Button("Generate Enemy Sprites"))
            GenerateEnemySprites();

        if (GUILayout.Button("Generate Bullet Sprites"))
            GenerateBulletSprites();

        if (GUILayout.Button("Generate Power-Up Sprites"))
            GeneratePowerUpSprites();

        if (GUILayout.Button("Generate UI Sprites"))
            GenerateUISprites();

        if (GUILayout.Button("Generate Background"))
            GenerateBackground();
    }

    private void GenerateAllSprites()
    {
        GeneratePlayerSprite();
        GenerateEnemySprites();
        GenerateBulletSprites();
        GeneratePowerUpSprites();
        GenerateUISprites();
        GenerateBackground();
        
        AssetDatabase.Refresh();
        Debug.Log("All sprites generated successfully!");
    }

    private void GeneratePlayerSprite()
    {
        // Player ship - triangle pointing up
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        Color playerColor = new Color(0.2f, 0.6f, 1f); // Light blue
        Color cockpitColor = new Color(0.1f, 0.3f, 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Triangle shape
                float normX = (x - size / 2f) / (size / 2f);
                float normY = (y - size / 2f) / (size / 2f);
                
                // Main body triangle
                float triWidth = 0.8f * (1f - normY * 0.5f);
                if (normY > -0.8f && normY < 0.8f && Mathf.Abs(normX) < triWidth * 0.5f)
                {
                    pixels[y * size + x] = playerColor;
                }
                // Cockpit (small circle)
                else if (Vector2.Distance(new Vector2(normX, normY + 0.2f), Vector2.zero) < 0.2f)
                {
                    pixels[y * size + x] = cockpitColor;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/Player/player_ship.png");

        // Shield effect
        GenerateCircleSprite("Assets/Sprites/Player/shield.png", 64, new Color(0f, 1f, 1f, 0.5f), true);
    }

    private void GenerateEnemySprites()
    {
        // Small enemy - diamond
        GenerateDiamondSprite("Assets/Sprites/Enemies/enemy_small.png", 32, Color.red);

        // Medium enemy - hexagon
        GenerateHexagonSprite("Assets/Sprites/Enemies/enemy_medium.png", 48, new Color(1f, 0.5f, 0f));

        // Large enemy - octagon
        GenerateOctagonSprite("Assets/Sprites/Enemies/enemy_large.png", 64, new Color(0.8f, 0f, 0.8f));

        // Tracker enemy - arrow
        GenerateArrowSprite("Assets/Sprites/Enemies/enemy_tracker.png", 48, Color.yellow);

        // Boss - large complex shape
        GenerateBossSprite("Assets/Sprites/Enemies/enemy_boss.png", 128, new Color(0.5f, 0f, 0f));
    }

    private void GenerateBulletSprites()
    {
        // Player bullet - elongated oval
        GenerateOvalSprite("Assets/Sprites/Bullets/bullet_player.png", 8, 16, Color.cyan);

        // Enemy bullet - circle
        GenerateCircleSprite("Assets/Sprites/Bullets/bullet_enemy.png", 12, Color.red, false);
    }

    private void GeneratePowerUpSprites()
    {
        // Weapon upgrade - star
        GenerateStarSprite("Assets/Sprites/PowerUps/powerup_weapon.png", 32, Color.yellow);

        // Shield - circle
        GenerateCircleSprite("Assets/Sprites/PowerUps/powerup_shield.png", 32, Color.cyan, false);

        // Health - cross/plus
        GenerateCrossSprite("Assets/Sprites/PowerUps/powerup_health.png", 32, Color.green);

        // Score bonus - diamond
        GenerateDiamondSprite("Assets/Sprites/PowerUps/powerup_score.png", 32, new Color(1f, 0.5f, 0f));
    }

    private void GenerateUISprites()
    {
        // Heart full
        GenerateHeartSprite("Assets/Sprites/UI/heart_full.png", 32, Color.red);

        // Heart empty
        GenerateHeartSprite("Assets/Sprites/UI/heart_empty.png", 32, new Color(0.3f, 0f, 0f));
    }

    private void GenerateBackground()
    {
        // Dark space background
        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        Color bgColor = new Color(0.02f, 0.02f, 0.08f);

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bgColor;
        }

        // Add some stars
        for (int i = 0; i < 100; i++)
        {
            int x = Random.Range(0, size);
            int y = Random.Range(0, size);
            float brightness = Random.Range(0.3f, 1f);
            pixels[y * size + x] = new Color(brightness, brightness, brightness);
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/Background/space_bg.png");
    }

    // Helper methods for generating shapes
    private void GenerateCircleSprite(string path, int size, Color color, bool outline)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        float center = size / 2f;
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (outline)
                {
                    if (dist < radius && dist > radius - 3)
                        pixels[y * size + x] = color;
                    else
                        pixels[y * size + x] = Color.clear;
                }
                else
                {
                    if (dist < radius)
                        pixels[y * size + x] = color;
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateDiamondSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center) / center;
                float dy = Mathf.Abs(y - center) / center;
                if (dx + dy < 0.8f)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateHexagonSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center) / center;
                float dy = Mathf.Abs(y - center) / center;
                // Hexagon approximation
                if (dx < 0.8f && dy < 0.7f && (dx + dy * 0.5f) < 0.85f)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateOctagonSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center) / center;
                float dy = Mathf.Abs(y - center) / center;
                // Octagon approximation
                if (dx < 0.85f && dy < 0.85f && (dx + dy) < 1.1f)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateArrowSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normX = (x - center) / center;
                float normY = (y - center) / center;
                
                // Arrow pointing down
                float arrowWidth = 0.6f * (1f + normY * 0.5f);
                if (normY > -0.7f && normY < 0.7f && Mathf.Abs(normX) < arrowWidth * 0.4f)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateBossSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normX = (x - center) / center;
                float normY = (y - center) / center;
                
                // Main body (wide)
                bool inBody = Mathf.Abs(normY) < 0.5f && Mathf.Abs(normX) < 0.9f;
                // Wings
                bool inWing = Mathf.Abs(normY) < 0.3f + Mathf.Abs(normX) * 0.3f && Mathf.Abs(normX) < 0.95f;
                // Cockpit
                bool inCockpit = Vector2.Distance(new Vector2(normX, normY), Vector2.zero) < 0.25f;
                
                if (inBody || inWing)
                    pixels[y * size + x] = color;
                else if (inCockpit)
                    pixels[y * size + x] = Color.red;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateOvalSprite(string path, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        float centerX = width / 2f;
        float centerY = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x - centerX) / (width / 2f);
                float dy = (y - centerY) / (height / 2f);
                if (dx * dx + dy * dy < 0.8f)
                    pixels[y * width + x] = color;
                else
                    pixels[y * width + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateStarSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / center;
                
                // 5-pointed star
                float starDist = 0.4f + 0.35f * Mathf.Cos(5f * angle);
                
                if (dist < starDist)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateCrossSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float thickness = size * 0.25f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inVertical = Mathf.Abs(x - center) < thickness / 2f && y > 2 && y < size - 2;
                bool inHorizontal = Mathf.Abs(y - center) < thickness / 2f && x > 2 && x < size - 2;
                
                if (inVertical || inHorizontal)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void GenerateHeartSprite(string path, int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normX = (x - center) / center;
                float normY = (y - center) / center;
                
                // Heart formula approximation
                float heart = Mathf.Pow(normX * normX + normY * normY - 0.3f, 3) - 
                              normX * normX * normY * normY * normY;
                
                if (heart < 0 && normY < 0.5f)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SaveSprite(tex, path);
    }

    private void SaveSprite(Texture2D tex, string path)
    {
        // Ensure directory exists
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(path, pngData);
        DestroyImmediate(tex);
    }
}
#endif
