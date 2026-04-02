// =============================================================================
// SpriteGenerator.cs (Editor Script)
// Generates simple procedural sprite textures for all game objects.
// Run from the Unity Editor menu: Tools > Space Shooter > Generate All Sprites
// This creates placeholder sprites so the game is immediately playable.
// =============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteGenerator : MonoBehaviour
{
    // Output directory for generated sprites
    private static string spritePath = "Assets/Sprites/";

    // -------------------------------------------------------------------------
    // Menu Command: Generate All Sprites
    // -------------------------------------------------------------------------

    /// <summary>
    /// Generates all placeholder sprites for the game.
    /// Accessible from the Unity menu bar: Tools > Space Shooter > Generate All Sprites
    /// </summary>
    [MenuItem("Tools/Space Shooter/Generate All Sprites")]
    public static void GenerateAllSprites()
    {
        // Ensure the Sprites directory exists
        if (!Directory.Exists(spritePath))
        {
            Directory.CreateDirectory(spritePath);
        }

        // Generate each sprite
        GeneratePlayerSprite();
        GenerateBasicEnemySprite();
        GenerateZigzagEnemySprite();
        GenerateChargerEnemySprite();
        GeneratePlayerBulletSprite();
        GenerateEnemyBulletSprite();
        GenerateShieldPowerUpSprite();
        GenerateRapidFirePowerUpSprite();
        GenerateHealthPowerUpSprite();
        GenerateExplosionSprite();
        GenerateShieldVisualSprite();
        GenerateBackgroundSprite();
        GenerateStarsBackgroundSprite();

        // Refresh the asset database so Unity recognizes the new files
        AssetDatabase.Refresh();

        Debug.Log("SpriteGenerator: All sprites generated successfully in " + spritePath);
    }

    // -------------------------------------------------------------------------
    // Player Ship Sprite (Triangle/Arrow shape pointing up)
    // -------------------------------------------------------------------------
    private static void GeneratePlayerSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        // Draw a triangular ship shape
        Color shipColor = new Color(0.2f, 0.6f, 1f); // Blue
        Color cockpitColor = new Color(0.4f, 0.9f, 1f); // Light blue

        // Main body triangle
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth = (int)(progress * size * 0.4f);
            int centerX = size / 2;

            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, shipColor);
            }
        }

        // Cockpit highlight (small bright area at top)
        for (int y = size - 20; y < size - 5; y++)
        {
            for (int x = size / 2 - 3; x <= size / 2 + 3; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                    tex.SetPixel(x, y, cockpitColor);
            }
        }

        // Wing accents
        Color wingColor = new Color(0.1f, 0.4f, 0.8f);
        for (int y = 5; y < 25; y++)
        {
            float progress = (float)y / size;
            int halfWidth = (int)(progress * size * 0.4f);
            int centerX = size / 2;
            // Left wing edge
            if (centerX - halfWidth >= 0 && centerX - halfWidth < size)
                tex.SetPixel(centerX - halfWidth, y, wingColor);
            // Right wing edge
            if (centerX + halfWidth >= 0 && centerX + halfWidth < size)
                tex.SetPixel(centerX + halfWidth, y, wingColor);
        }

        SaveSprite(tex, "player_ship.png");
    }

    // -------------------------------------------------------------------------
    // Basic Enemy Sprite (Inverted triangle / wide top)
    // -------------------------------------------------------------------------
    private static void GenerateBasicEnemySprite()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        Color enemyColor = new Color(1f, 0.3f, 0.3f); // Red
        Color eyeColor = new Color(1f, 1f, 0.3f); // Yellow eyes

        // Inverted triangle (wide at top, narrow at bottom)
        for (int y = 0; y < size; y++)
        {
            float progress = 1f - ((float)y / size);
            int halfWidth = (int)(progress * size * 0.45f);
            int centerX = size / 2;

            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, enemyColor);
            }
        }

        // Eyes
        DrawCircle(tex, size / 2 - 6, size - 15, 3, eyeColor);
        DrawCircle(tex, size / 2 + 6, size - 15, 3, eyeColor);

        SaveSprite(tex, "enemy_basic.png");
    }

    // -------------------------------------------------------------------------
    // Zigzag Enemy Sprite (Diamond shape)
    // -------------------------------------------------------------------------
    private static void GenerateZigzagEnemySprite()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        Color enemyColor = new Color(0.6f, 0.2f, 0.8f); // Purple
        Color accentColor = new Color(0.9f, 0.5f, 1f); // Light purple

        int centerX = size / 2;
        int centerY = size / 2;

        // Diamond shape
        for (int y = 0; y < size; y++)
        {
            int distFromCenter = Mathf.Abs(y - centerY);
            int halfWidth = (size / 2) - distFromCenter;

            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                {
                    // Accent color on edges
                    if (Mathf.Abs(x - centerX) >= halfWidth - 2)
                        tex.SetPixel(x, y, accentColor);
                    else
                        tex.SetPixel(x, y, enemyColor);
                }
            }
        }

        SaveSprite(tex, "enemy_zigzag.png");
    }

    // -------------------------------------------------------------------------
    // Charger Enemy Sprite (Pointed arrow shape)
    // -------------------------------------------------------------------------
    private static void GenerateChargerEnemySprite()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        Color enemyColor = new Color(1f, 0.6f, 0f); // Orange
        Color tipColor = new Color(1f, 0.9f, 0.3f); // Bright yellow tip

        // Arrow/dart shape pointing down
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth;

            if (progress < 0.5f)
            {
                // Top half: narrow to wide
                halfWidth = (int)(progress * 2f * size * 0.4f);
            }
            else
            {
                // Bottom half: wide to narrow (point)
                halfWidth = (int)((1f - progress) * 2f * size * 0.4f);
            }

            int centerX = size / 2;
            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                {
                    if (y < 10)
                        tex.SetPixel(x, y, tipColor);
                    else
                        tex.SetPixel(x, y, enemyColor);
                }
            }
        }

        SaveSprite(tex, "enemy_charger.png");
    }

    // -------------------------------------------------------------------------
    // Bullet Sprites
    // -------------------------------------------------------------------------
    private static void GeneratePlayerBulletSprite()
    {
        int width = 8;
        int height = 16;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        Color bulletColor = new Color(0.3f, 1f, 0.3f); // Green
        Color coreColor = new Color(0.8f, 1f, 0.8f); // Bright center

        for (int y = 0; y < height; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                if (x >= 3 && x <= 4)
                    tex.SetPixel(x, y, coreColor);
                else
                    tex.SetPixel(x, y, bulletColor);
            }
        }

        SaveSprite(tex, "bullet_player.png");
    }

    private static void GenerateEnemyBulletSprite()
    {
        int width = 8;
        int height = 16;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        Color bulletColor = new Color(1f, 0.4f, 0.4f); // Red

        for (int y = 0; y < height; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                tex.SetPixel(x, y, bulletColor);
            }
        }

        SaveSprite(tex, "bullet_enemy.png");
    }

    // -------------------------------------------------------------------------
    // Power-Up Sprites
    // -------------------------------------------------------------------------
    private static void GenerateShieldPowerUpSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        Color shieldColor = new Color(0.3f, 0.7f, 1f); // Blue

        // Circle shape
        DrawCircle(tex, size / 2, size / 2, size / 2 - 2, shieldColor);
        // Inner ring (hollow center)
        DrawCircle(tex, size / 2, size / 2, size / 2 - 6, Color.clear);
        // S letter in center
        for (int x = size / 2 - 4; x <= size / 2 + 4; x++)
        {
            tex.SetPixel(x, size / 2 + 4, shieldColor);
            tex.SetPixel(x, size / 2, shieldColor);
            tex.SetPixel(x, size / 2 - 4, shieldColor);
        }

        SaveSprite(tex, "powerup_shield.png");
    }

    private static void GenerateRapidFirePowerUpSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        Color fireColor = new Color(1f, 0.8f, 0f); // Gold/Yellow

        // Diamond with lightning bolt pattern
        int center = size / 2;
        DrawCircle(tex, center, center, size / 2 - 2, fireColor);

        // Lightning bolt in center
        Color boltColor = Color.white;
        for (int y = size / 2 - 6; y <= size / 2 + 6; y++)
        {
            int xOffset = (y > size / 2) ? -2 : 2;
            tex.SetPixel(center + xOffset, y, boltColor);
            tex.SetPixel(center + xOffset + 1, y, boltColor);
        }

        SaveSprite(tex, "powerup_rapidfire.png");
    }

    private static void GenerateHealthPowerUpSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        Color healthColor = new Color(0.2f, 1f, 0.2f); // Green
        Color crossColor = Color.white;

        // Circle background
        DrawCircle(tex, size / 2, size / 2, size / 2 - 2, healthColor);

        // White cross in center
        int center = size / 2;
        for (int i = -5; i <= 5; i++)
        {
            // Horizontal bar
            tex.SetPixel(center + i, center, crossColor);
            tex.SetPixel(center + i, center + 1, crossColor);
            tex.SetPixel(center + i, center - 1, crossColor);
            // Vertical bar
            tex.SetPixel(center, center + i, crossColor);
            tex.SetPixel(center + 1, center + i, crossColor);
            tex.SetPixel(center - 1, center + i, crossColor);
        }

        SaveSprite(tex, "powerup_health.png");
    }

    // -------------------------------------------------------------------------
    // Explosion Sprite
    // -------------------------------------------------------------------------
    private static void GenerateExplosionSprite()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        int center = size / 2;

        // Outer orange glow
        DrawCircle(tex, center, center, 20, new Color(1f, 0.5f, 0f, 0.6f));
        // Middle yellow ring
        DrawCircle(tex, center, center, 14, new Color(1f, 0.8f, 0f, 0.8f));
        // Inner white core
        DrawCircle(tex, center, center, 8, new Color(1f, 1f, 0.8f, 1f));

        SaveSprite(tex, "explosion.png");
    }

    // -------------------------------------------------------------------------
    // Shield Visual Sprite
    // -------------------------------------------------------------------------
    private static void GenerateShieldVisualSprite()
    {
        int size = 80;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        int center = size / 2;
        Color shieldColor = new Color(0.3f, 0.7f, 1f, 0.3f); // Translucent blue
        Color edgeColor = new Color(0.5f, 0.9f, 1f, 0.7f); // Brighter edge

        // Filled translucent circle
        DrawCircle(tex, center, center, size / 2 - 2, shieldColor);
        // Bright edge ring
        DrawRing(tex, center, center, size / 2 - 2, size / 2 - 5, edgeColor);

        SaveSprite(tex, "shield_visual.png");
    }

    // -------------------------------------------------------------------------
    // Background Sprites
    // -------------------------------------------------------------------------
    private static void GenerateBackgroundSprite()
    {
        int width = 256;
        int height = 512;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Dark space gradient
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float darkness = 0.02f + 0.03f * ((float)y / height);
                tex.SetPixel(x, y, new Color(darkness, darkness, darkness * 1.5f));
            }
        }

        // Add some random stars
        System.Random rng = new System.Random(42);
        for (int i = 0; i < 100; i++)
        {
            int sx = rng.Next(0, width);
            int sy = rng.Next(0, height);
            float brightness = 0.3f + (float)rng.NextDouble() * 0.7f;
            Color starColor = new Color(brightness, brightness, brightness);
            tex.SetPixel(sx, sy, starColor);
            // Some stars are slightly bigger
            if (rng.NextDouble() > 0.7f)
            {
                if (sx + 1 < width) tex.SetPixel(sx + 1, sy, starColor * 0.5f);
                if (sy + 1 < height) tex.SetPixel(sx, sy + 1, starColor * 0.5f);
            }
        }

        SaveSprite(tex, "background_space.png");
    }

    private static void GenerateStarsBackgroundSprite()
    {
        int width = 256;
        int height = 512;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        ClearTexture(tex, Color.clear);

        // Sparse foreground stars (larger, brighter - for parallax layer)
        System.Random rng = new System.Random(99);
        for (int i = 0; i < 40; i++)
        {
            int sx = rng.Next(0, width);
            int sy = rng.Next(0, height);
            float brightness = 0.5f + (float)rng.NextDouble() * 0.5f;
            float size = 1 + (float)rng.NextDouble() * 2f;
            Color starColor = new Color(brightness, brightness, brightness * 1.1f);

            DrawCircle(tex, sx, sy, (int)size, starColor);
        }

        SaveSprite(tex, "background_stars.png");
    }

    // -------------------------------------------------------------------------
    // Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>Clears the entire texture to a single color.</summary>
    private static void ClearTexture(Texture2D tex, Color color)
    {
        Color[] pixels = new Color[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
    }

    /// <summary>Draws a filled circle on the texture.</summary>
    private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color)
    {
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= radius)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }
    }

    /// <summary>Draws a ring (hollow circle) on the texture.</summary>
    private static void DrawRing(Texture2D tex, int cx, int cy, int outerRadius, int innerRadius, Color color)
    {
        for (int y = cy - outerRadius; y <= cy + outerRadius; y++)
        {
            for (int x = cx - outerRadius; x <= cx + outerRadius; x++)
            {
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= outerRadius && dist >= innerRadius)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }
    }

    /// <summary>Saves a Texture2D as a PNG file in the sprites directory.</summary>
    private static void SaveSprite(Texture2D tex, string filename)
    {
        tex.Apply();
        byte[] pngData = tex.EncodeToPNG();
        string fullPath = spritePath + filename;
        File.WriteAllBytes(fullPath, pngData);
        Debug.Log("Generated sprite: " + fullPath);
    }
}
#endif
