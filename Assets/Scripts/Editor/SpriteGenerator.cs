// ============================================================================
// SpriteGenerator.cs — Editor utility that generates placeholder sprites
// Run from the Unity menu: Tools > Space Shooter > Generate Sprites
// Creates simple pixel-art style sprites for all game objects so the project
// is fully playable without importing external art assets.
// ============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SpaceShooter.EditorTools
{
    public static class SpriteGenerator
    {
        private static readonly string OutputPath = "Assets/Sprites/Generated";

        [MenuItem("Tools/Space Shooter/Generate All Sprites")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(OutputPath);

            // Player ship — cyan triangle pointing up
            CreateSprite("PlayerShip", 32, 32, (x, y) =>
            {
                // Triangle shape
                int cx = 16;
                float halfWidth = 16f * (1f - (float)y / 32f);
                if (y < 4) return Color.clear;
                if (Mathf.Abs(x - cx) <= halfWidth && y >= 4)
                    return new Color(0f, 0.9f, 1f, 1f);  // cyan
                return Color.clear;
            });

            // Player bullet — small yellow rectangle
            CreateSprite("PlayerBullet", 4, 12, (x, y) =>
            {
                return new Color(1f, 1f, 0.2f, 1f);  // bright yellow
            });

            // Enemy bullet — small red rectangle
            CreateSprite("EnemyBullet", 4, 12, (x, y) =>
            {
                return new Color(1f, 0.2f, 0.2f, 1f);  // red
            });

            // Enemy Straight — red diamond
            CreateSprite("EnemyStraight", 24, 24, (x, y) =>
            {
                int cx = 12, cy = 12;
                if (Mathf.Abs(x - cx) + Mathf.Abs(y - cy) <= 11)
                    return new Color(1f, 0.3f, 0.3f, 1f);
                return Color.clear;
            });

            // Enemy Zigzag — orange triangle pointing down
            CreateSprite("EnemyZigzag", 24, 24, (x, y) =>
            {
                int cx = 12;
                int invertedY = 23 - y;
                float halfWidth = 12f * (1f - (float)invertedY / 24f);
                if (Mathf.Abs(x - cx) <= halfWidth)
                    return new Color(1f, 0.6f, 0f, 1f);  // orange
                return Color.clear;
            });

            // Enemy Tracker — magenta arrow-like shape
            CreateSprite("EnemyTracker", 24, 24, (x, y) =>
            {
                int cx = 12;
                // Body (narrow rectangle) + wings
                bool body = Mathf.Abs(x - cx) <= 3;
                bool wing = y > 8 && y < 18 && Mathf.Abs(x - cx) <= 10 - Mathf.Abs(y - 13);
                if (body || wing)
                    return new Color(0.9f, 0.2f, 0.9f, 1f);  // magenta
                return Color.clear;
            });

            // Enemy Tank — large green hexagon
            CreateSprite("EnemyTank", 32, 32, (x, y) =>
            {
                int cx = 16, cy = 16;
                // Approximate hexagon
                float dx = Mathf.Abs(x - cx);
                float dy = Mathf.Abs(y - cy);
                if (dx + dy * 0.5f <= 14 && dy <= 13)
                    return new Color(0.3f, 0.8f, 0.3f, 1f);  // green
                return Color.clear;
            });

            // Power-up: Health — green cross
            CreateSprite("PowerUpHealth", 16, 16, (x, y) =>
            {
                bool horiz = y >= 6 && y <= 9;
                bool vert = x >= 6 && x <= 9;
                if (horiz || vert)
                    return new Color(0.2f, 1f, 0.2f, 1f);
                return Color.clear;
            });

            // Power-up: Shield — blue circle
            CreateSprite("PowerUpShield", 16, 16, (x, y) =>
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(8, 8));
                if (dist <= 7f && dist >= 4f)
                    return new Color(0.3f, 0.5f, 1f, 1f);
                return Color.clear;
            });

            // Power-up: Rapid Fire — yellow lightning bolt shape
            CreateSprite("PowerUpRapidFire", 16, 16, (x, y) =>
            {
                // Simple zigzag pattern
                if (y >= 12 && x >= 6 && x <= 10) return Color.yellow;
                if (y >= 8 && y < 12 && x >= 4 && x <= 8) return Color.yellow;
                if (y >= 4 && y < 8 && x >= 6 && x <= 10) return Color.yellow;
                if (y < 4 && x >= 4 && x <= 8) return Color.yellow;
                return Color.clear;
            });

            // Power-up: Spread Shot — white triple arrow
            CreateSprite("PowerUpSpreadShot", 16, 16, (x, y) =>
            {
                int cx = 8;
                bool center = Mathf.Abs(x - cx) <= 1 && y >= 4;
                bool left = Mathf.Abs(x - (cx - 4)) <= 1 && y >= 2 && y <= 12;
                bool right = Mathf.Abs(x - (cx + 4)) <= 1 && y >= 2 && y <= 12;
                if (center || left || right)
                    return new Color(1f, 1f, 1f, 1f);
                return Color.clear;
            });

            // Shield visual overlay — larger transparent blue circle
            CreateSprite("ShieldOverlay", 48, 48, (x, y) =>
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(24, 24));
                if (dist <= 22f && dist >= 18f)
                    return new Color(0.3f, 0.6f, 1f, 0.5f);
                return Color.clear;
            });

            // Background tile — dark blue/purple gradient with tiny stars
            CreateSprite("BackgroundTile", 64, 128, (x, y) =>
            {
                float t = (float)y / 128f;
                Color bg = Color.Lerp(new Color(0.02f, 0.02f, 0.08f), new Color(0.05f, 0.02f, 0.1f), t);
                // Pseudo-random stars
                int hash = (x * 73 + y * 137) % 97;
                if (hash < 2)
                    return Color.Lerp(bg, Color.white, 0.6f);
                return bg;
            });

            AssetDatabase.Refresh();
            Debug.Log($"[SpriteGenerator] All sprites generated in {OutputPath}");
        }

        // ====================================================================
        // Helper: creates a Texture2D, writes it as PNG, imports as Sprite
        // ====================================================================
        private static void CreateSprite(string name, int width, int height,
            System.Func<int, int, Color> pixelFunc)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;  // retro pixel look

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, pixelFunc(x, y));

            tex.Apply();

            string filePath = $"{OutputPath}/{name}.png";
            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            // Configure import settings for pixel sprites
            AssetDatabase.ImportAsset(filePath);
            var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
