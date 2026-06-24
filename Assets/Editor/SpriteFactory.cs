#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// Generates simple procedural placeholder sprites (geometric shapes) and
    /// saves them as PNG assets configured for 2D use. Used by SceneBuilder so
    /// the project has visible art without external image files.
    /// </summary>
    public static class SpriteFactory
    {
        public const string SpriteFolder = "Assets/Sprites";

        public static Sprite CreateOrLoad(string name, int width, int height, System.Func<int, int, Color> painter)
        {
            string path = $"{SpriteFolder}/{name}.png";
            if (!File.Exists(path))
            {
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        tex.SetPixel(x, y, painter(x, y));
                    }
                }
                tex.Apply();

                Directory.CreateDirectory(SpriteFolder);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
                ConfigureAsSprite(path);
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void ConfigureAsSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        // ---------- Shape painters ----------

        public static System.Func<int, int, Color> Triangle(int w, int h, Color color)
        {
            // Upward-pointing triangle (ship).
            return (x, y) =>
            {
                float nx = (float)x / w;   // 0..1
                float ny = (float)y / h;   // 0..1 (0 bottom)
                float halfWidth = ny * 0.5f; // widens toward bottom
                float center = 0.5f;
                if (nx >= center - halfWidth && nx <= center + halfWidth)
                    return color;
                return Color.clear;
            };
        }

        public static System.Func<int, int, Color> InvertedTriangle(int w, int h, Color color)
        {
            return (x, y) =>
            {
                float nx = (float)x / w;
                float ny = (float)y / h;
                float halfWidth = (1f - ny) * 0.5f;
                float center = 0.5f;
                if (nx >= center - halfWidth && nx <= center + halfWidth)
                    return color;
                return Color.clear;
            };
        }

        public static System.Func<int, int, Color> Circle(int w, int h, Color color)
        {
            return (x, y) =>
            {
                float dx = x - w / 2f;
                float dy = y - h / 2f;
                float r = Mathf.Min(w, h) / 2f - 1f;
                return (dx * dx + dy * dy <= r * r) ? color : Color.clear;
            };
        }

        public static System.Func<int, int, Color> Diamond(int w, int h, Color color)
        {
            return (x, y) =>
            {
                float dx = Mathf.Abs(x - w / 2f) / (w / 2f);
                float dy = Mathf.Abs(y - h / 2f) / (h / 2f);
                return (dx + dy <= 1f) ? color : Color.clear;
            };
        }

        public static System.Func<int, int, Color> Rect(Color color)
        {
            return (x, y) => color;
        }

        public static System.Func<int, int, Color> RoundedRect(int w, int h, Color color)
        {
            return (x, y) =>
            {
                int margin = Mathf.Min(w, h) / 6;
                bool inX = x >= margin && x < w - margin;
                bool inY = y >= margin && y < h - margin;
                if (inX || inY) return color;
                return Color.clear;
            };
        }

        public static System.Func<int, int, Color> Starfield(int w, int h, Color bg, Color star, float density, int seed)
        {
            System.Random rnd = new System.Random(seed);
            bool[,] stars = new bool[w, h];
            int count = Mathf.RoundToInt(w * h * density);
            for (int i = 0; i < count; i++)
            {
                int sx = rnd.Next(0, w);
                int sy = rnd.Next(0, h);
                stars[sx, sy] = true;
            }
            return (x, y) => stars[x, y] ? star : bg;
        }
    }
}
#endif
