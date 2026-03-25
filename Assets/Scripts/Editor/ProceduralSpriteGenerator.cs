// =============================================================================
// ProceduralSpriteGenerator.cs — Editor tool to generate all game sprites
// =============================================================================
// Place this file in Assets/Scripts/Editor/ folder.
// Access via Unity menu: Tools > Space Shooter > Generate All Sprites
// =============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SpaceShooter.Editor
{
    /// <summary>
    /// Editor utility that procedurally generates all sprite textures for the game.
    /// Creates PNG files in the Assets/Sprites/ directories.
    /// </summary>
    public class ProceduralSpriteGenerator : EditorWindow
    {
        [MenuItem("Tools/Space Shooter/Generate All Sprites")]
        public static void GenerateAllSprites()
        {
            EnsureDirectories();
            GeneratePlayerShip();
            GenerateEnemyShips();
            GenerateBullets();
            GeneratePowerUps();
            GenerateExplosion();
            GenerateShield();
            GenerateBackgrounds();
            GenerateUISprites();
            AssetDatabase.Refresh();
            Debug.Log("[SpaceShooter] All sprites generated successfully!");
        }

        private static void EnsureDirectories()
        {
            string[] dirs = {
                "Assets/Sprites/Player",
                "Assets/Sprites/Enemies",
                "Assets/Sprites/Bullets",
                "Assets/Sprites/PowerUps",
                "Assets/Sprites/Effects",
                "Assets/Sprites/Backgrounds",
                "Assets/Sprites/UI"
            };
            foreach (string dir in dirs)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        // =====================================================================
        // PLAYER SHIP
        // =====================================================================
        private static void GeneratePlayerShip()
        {
            int w = 64, h = 64;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);

            // Main body (blue triangle shape)
            Color hull = new Color(0.2f, 0.5f, 1f);
            Color cockpit = new Color(0.3f, 0.9f, 1f);
            Color engine = new Color(1f, 0.6f, 0.1f);
            Color wing = new Color(0.15f, 0.35f, 0.8f);

            // Draw main hull (diamond/arrow shape)
            for (int y = 10; y < 58; y++)
            {
                float t = (float)(y - 10) / 48f;
                int halfWidth;
                if (t < 0.6f)
                    halfWidth = (int)Mathf.Lerp(1, 14, t / 0.6f);
                else
                    halfWidth = (int)Mathf.Lerp(14, 8, (t - 0.6f) / 0.4f);

                for (int x = 32 - halfWidth; x <= 32 + halfWidth; x++)
                {
                    if (x >= 0 && x < w)
                        tex.SetPixel(x, h - 1 - y, hull);
                }
            }

            // Wings
            for (int y = 30; y < 50; y++)
            {
                float t = (float)(y - 30) / 20f;
                int wingStart = 32 + (int)Mathf.Lerp(6, 14, t);
                int wingEnd = wingStart + (int)Mathf.Lerp(8, 3, t);
                for (int x = wingStart; x < wingEnd && x < w; x++)
                {
                    tex.SetPixel(x, h - 1 - y, wing);
                    tex.SetPixel(w - 1 - x, h - 1 - y, wing); // Mirror
                }
            }

            // Cockpit (bright center)
            for (int y = 16; y < 32; y++)
            {
                int hw = (int)Mathf.Lerp(1, 4, (float)(y - 16) / 16f);
                for (int x = 32 - hw; x <= 32 + hw; x++)
                    tex.SetPixel(x, h - 1 - y, cockpit);
            }

            // Engine glow
            for (int y = 52; y < 58; y++)
            {
                for (int x = 28; x < 36; x++)
                    tex.SetPixel(x, h - 1 - y, engine);
            }

            tex.Apply();
            SaveTexture(tex, "Assets/Sprites/Player/player_ship.png");
        }

        // =====================================================================
        // ENEMY SHIPS
        // =====================================================================
        private static void GenerateEnemyShips()
        {
            // Basic Enemy (red, simple)
            GenerateEnemySprite("basic_enemy", 48, new Color(0.9f, 0.2f, 0.2f), new Color(0.6f, 0.1f, 0.1f), false);
            // Fast Enemy (yellow, sleek)
            GenerateEnemySprite("fast_enemy", 40, new Color(1f, 0.8f, 0.1f), new Color(0.8f, 0.5f, 0f), true);
            // Tank Enemy (green, wide)
            GenerateEnemySprite("tank_enemy", 64, new Color(0.2f, 0.8f, 0.2f), new Color(0.1f, 0.5f, 0.1f), false);
            // Boss (purple, large)
            GenerateBossSprite();
        }

        private static void GenerateEnemySprite(string name, int size, Color main, Color dark, bool sleek)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);
            int cx = size / 2;

            // Inverted triangle (pointing down)
            for (int y = 4; y < size - 4; y++)
            {
                float t = (float)(y - 4) / (size - 8);
                int hw;
                if (sleek)
                    hw = (int)Mathf.Lerp(size / 2 - 4, 2, t);
                else
                    hw = (int)Mathf.Lerp(size / 2 - 6, 3, t * t);

                Color c = Color.Lerp(main, dark, t * 0.5f);
                for (int x = cx - hw; x <= cx + hw; x++)
                {
                    if (x >= 0 && x < size)
                        tex.SetPixel(x, size - 1 - y, c);
                }
            }

            // Eye/cockpit (bright center near top)
            Color eye = Color.white;
            for (int y = 8; y < 16; y++)
            {
                int ew = (int)Mathf.Lerp(1, 3, (float)(y - 8) / 8f);
                for (int x = cx - ew; x <= cx + ew; x++)
                    tex.SetPixel(x, size - 1 - y, eye);
            }

            tex.Apply();
            SaveTexture(tex, $"Assets/Sprites/Enemies/{name}.png");
        }

        private static void GenerateBossSprite()
        {
            int w = 128, h = 96;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);

            Color main = new Color(0.6f, 0.1f, 0.8f);
            Color accent = new Color(0.9f, 0.2f, 0.9f);
            Color eye = new Color(1f, 0f, 0f);

            int cx = w / 2;

            // Main body (wide, menacing)
            for (int y = 8; y < h - 8; y++)
            {
                float t = (float)(y - 8) / (h - 16);
                int hw = (int)Mathf.Lerp(10, cx - 8, Mathf.Sin(t * Mathf.PI));
                Color c = Color.Lerp(main, accent, Mathf.Sin(t * 3f) * 0.3f + 0.3f);

                for (int x = cx - hw; x <= cx + hw; x++)
                {
                    if (x >= 0 && x < w)
                        tex.SetPixel(x, h - 1 - y, c);
                }
            }

            // Eyes
            int eyeY = h - 30;
            DrawCircle(tex, cx - 18, eyeY, 6, eye);
            DrawCircle(tex, cx + 18, eyeY, 6, eye);
            DrawCircle(tex, cx - 18, eyeY, 3, Color.white);
            DrawCircle(tex, cx + 18, eyeY, 3, Color.white);

            tex.Apply();
            SaveTexture(tex, "Assets/Sprites/Enemies/boss_enemy.png");
        }

        // =====================================================================
        // BULLETS
        // =====================================================================
        private static void GenerateBullets()
        {
            // Player bullet (cyan beam)
            Texture2D playerBullet = new Texture2D(8, 16, TextureFormat.RGBA32, false);
            ClearTexture(playerBullet, Color.clear);
            for (int y = 0; y < 16; y++)
            {
                float t = (float)y / 16f;
                Color c = new Color(0.3f, 1f, 1f, Mathf.Lerp(0.5f, 1f, t));
                for (int x = 2; x < 6; x++)
                    playerBullet.SetPixel(x, y, c);
                // Glow edges
                playerBullet.SetPixel(1, y, new Color(0.3f, 1f, 1f, 0.3f));
                playerBullet.SetPixel(6, y, new Color(0.3f, 1f, 1f, 0.3f));
            }
            playerBullet.Apply();
            SaveTexture(playerBullet, "Assets/Sprites/Bullets/player_bullet.png");

            // Enemy bullet (red dot)
            Texture2D enemyBullet = new Texture2D(12, 12, TextureFormat.RGBA32, false);
            ClearTexture(enemyBullet, Color.clear);
            DrawCircle(enemyBullet, 6, 6, 5, new Color(1f, 0.3f, 0.3f));
            DrawCircle(enemyBullet, 6, 6, 3, new Color(1f, 0.7f, 0.3f));
            enemyBullet.Apply();
            SaveTexture(enemyBullet, "Assets/Sprites/Bullets/enemy_bullet.png");

            // Boss bullet (purple larger)
            Texture2D bossBullet = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            ClearTexture(bossBullet, Color.clear);
            DrawCircle(bossBullet, 8, 8, 7, new Color(0.8f, 0.2f, 1f));
            DrawCircle(bossBullet, 8, 8, 4, new Color(1f, 0.5f, 1f));
            bossBullet.Apply();
            SaveTexture(bossBullet, "Assets/Sprites/Bullets/boss_bullet.png");
        }

        // =====================================================================
        // POWER-UPS
        // =====================================================================
        private static void GeneratePowerUps()
        {
            GeneratePowerUpSprite("powerup_health", new Color(0.2f, 1f, 0.2f), "+");
            GeneratePowerUpSprite("powerup_shield", new Color(0.3f, 0.7f, 1f), "S");
            GeneratePowerUpSprite("powerup_rapid", new Color(1f, 1f, 0.2f), "R");
            GeneratePowerUpSprite("powerup_spread", new Color(1f, 0.5f, 0.1f), "W");
            GeneratePowerUpSprite("powerup_life", new Color(1f, 0.3f, 0.5f), "L");
            GeneratePowerUpSprite("powerup_score", new Color(1f, 0.85f, 0f), "$");
        }

        private static void GeneratePowerUpSprite(string name, Color main, string symbol)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);

            // Draw diamond background
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - size / 2f) / (size / 2f);
                    float dy = Mathf.Abs(y - size / 2f) / (size / 2f);
                    if (dx + dy < 0.85f)
                    {
                        float dist = dx + dy;
                        Color c = Color.Lerp(Color.white, main, dist / 0.85f);
                        c.a = 1f;
                        tex.SetPixel(x, y, c);
                    }
                    else if (dx + dy < 1f)
                    {
                        tex.SetPixel(x, y, new Color(main.r, main.g, main.b, 0.5f));
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, $"Assets/Sprites/PowerUps/{name}.png");
        }

        // =====================================================================
        // EFFECTS
        // =====================================================================
        private static void GenerateExplosion()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);

            // Radial gradient explosion
            float cx = size / 2f, cy = size / 2f;
            float maxR = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    if (dist < maxR)
                    {
                        float t = dist / maxR;
                        Color c;
                        if (t < 0.3f)
                            c = Color.Lerp(Color.white, Color.yellow, t / 0.3f);
                        else if (t < 0.6f)
                            c = Color.Lerp(Color.yellow, new Color(1f, 0.3f, 0f), (t - 0.3f) / 0.3f);
                        else
                            c = Color.Lerp(new Color(1f, 0.3f, 0f), new Color(0.3f, 0f, 0f, 0f), (t - 0.6f) / 0.4f);

                        c.a = 1f - t;
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "Assets/Sprites/Effects/explosion.png");
        }

        private static void GenerateShield()
        {
            int size = 80;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex, Color.clear);

            float cx = size / 2f, cy = size / 2f;
            float outerR = 36f, innerR = 30f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    if (dist < outerR && dist > innerR)
                    {
                        float t = (dist - innerR) / (outerR - innerR);
                        Color c = new Color(0.3f, 0.7f, 1f, 0.6f - t * 0.3f);
                        tex.SetPixel(x, y, c);
                    }
                    else if (dist <= innerR)
                    {
                        tex.SetPixel(x, y, new Color(0.3f, 0.7f, 1f, 0.1f));
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "Assets/Sprites/Effects/shield.png");
        }

        // =====================================================================
        // BACKGROUNDS
        // =====================================================================
        private static void GenerateBackgrounds()
        {
            // Deep space background layer 1 (far)
            GenerateSpaceBackground("bg_far", 512, 1024, 80, 0.15f, 0.5f, new Color(0.02f, 0.02f, 0.08f));
            // Nebula layer 2 (mid)
            GenerateSpaceBackground("bg_mid", 512, 1024, 40, 0.5f, 1.5f, Color.clear);
            // Close stars layer 3 (near)
            GenerateSpaceBackground("bg_near", 512, 1024, 20, 1.5f, 3f, Color.clear);
        }

        private static void GenerateSpaceBackground(string name, int w, int h, int starCount,
            float minBright, float maxBright, Color bgColor)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex, bgColor);

            for (int i = 0; i < starCount; i++)
            {
                int x = Random.Range(0, w);
                int y = Random.Range(0, h);
                float b = Random.Range(minBright, maxBright);
                int sz = Random.Range(1, 3);

                Color starColor = new Color(b, b, b * Random.Range(0.9f, 1.1f), Mathf.Clamp01(b));
                for (int dx = -sz; dx <= sz; dx++)
                    for (int dy = -sz; dy <= sz; dy++)
                        if (x + dx >= 0 && x + dx < w && y + dy >= 0 && y + dy < h)
                            tex.SetPixel(x + dx, y + dy, starColor);
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            SaveTexture(tex, $"Assets/Sprites/Backgrounds/{name}.png");
        }

        // =====================================================================
        // UI SPRITES
        // =====================================================================
        private static void GenerateUISprites()
        {
            // Life icon (small ship silhouette)
            int sz = 24;
            Texture2D life = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            ClearTexture(life, Color.clear);
            for (int y = 2; y < sz - 2; y++)
            {
                float t = (float)(y - 2) / (sz - 4);
                int hw = (int)Mathf.Lerp(1, 8, t < 0.5f ? t * 2f : 2f - t * 2f);
                for (int x = sz / 2 - hw; x <= sz / 2 + hw; x++)
                    if (x >= 0 && x < sz)
                        life.SetPixel(x, sz - 1 - y, new Color(0.3f, 0.8f, 1f));
            }
            life.Apply();
            SaveTexture(life, "Assets/Sprites/UI/life_icon.png");

            // Health bar fill (simple white rectangle)
            Texture2D hb = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            ClearTexture(hb, Color.white);
            hb.Apply();
            SaveTexture(hb, "Assets/Sprites/UI/health_fill.png");

            // Button background (rounded rect approximation)
            int bw = 256, bh = 64;
            Texture2D btn = new Texture2D(bw, bh, TextureFormat.RGBA32, false);
            ClearTexture(btn, Color.clear);
            for (int y = 0; y < bh; y++)
            {
                for (int x = 0; x < bw; x++)
                {
                    float dx = Mathf.Max(0, Mathf.Abs(x - bw / 2f) - (bw / 2f - 12));
                    float dy = Mathf.Max(0, Mathf.Abs(y - bh / 2f) - (bh / 2f - 12));
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < 12f)
                    {
                        float a = dist < 10f ? 0.8f : 0.4f;
                        btn.SetPixel(x, y, new Color(0.2f, 0.3f, 0.5f, a));
                    }
                }
            }
            btn.Apply();
            SaveTexture(btn, "Assets/Sprites/UI/button_bg.png");
        }

        // =====================================================================
        // HELPERS
        // =====================================================================
        private static void ClearTexture(Texture2D tex, Color color)
        {
            Color[] pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
        }

        private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (dist <= radius)
                        {
                            tex.SetPixel(x, y, color);
                        }
                    }
                }
            }
        }

        private static void SaveTexture(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);
        }
    }
}
#endif
