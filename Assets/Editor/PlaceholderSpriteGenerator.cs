#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    public static class PlaceholderSpriteGenerator
    {
        private const string SpriteFolder = "Assets/Sprites";

        [MenuItem("Tools/Space Shooter/Generate Placeholder Sprites")]
        public static void Generate()
        {
            if (!Directory.Exists(SpriteFolder))
            {
                Directory.CreateDirectory(SpriteFolder);
            }

            CreateSolidSprite("player_ship.png", new Color(0.2f, 0.85f, 1f));
            CreateSolidSprite("enemy_basic.png", new Color(1f, 0.35f, 0.35f));
            CreateSolidSprite("enemy_zigzag.png", new Color(1f, 0.55f, 0.2f));
            CreateSolidSprite("enemy_tank.png", new Color(0.75f, 0.2f, 1f));
            CreateSolidSprite("player_bullet.png", new Color(0.8f, 1f, 0.8f), 8, 20);
            CreateSolidSprite("enemy_bullet.png", new Color(1f, 0.8f, 0.3f), 8, 20);
            CreateSolidSprite("powerup_shield.png", new Color(0.2f, 1f, 1f));
            CreateSolidSprite("powerup_rapidfire.png", new Color(1f, 1f, 0.2f));
            CreateSolidSprite("powerup_health.png", new Color(0.2f, 1f, 0.4f));
            CreateSolidSprite("bg_layer1.png", new Color(0.04f, 0.06f, 0.14f), 512, 512);
            CreateSolidSprite("bg_layer2.png", new Color(0.08f, 0.1f, 0.2f), 512, 512);

            AssetDatabase.Refresh();
            Debug.Log("Placeholder sprites generated in Assets/Sprites.");
        }

        private static void CreateSolidSprite(string fileName, Color color, int width = 64, int height = 64)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            byte[] pngData = tex.EncodeToPNG();
            string path = Path.Combine(SpriteFolder, fileName);
            File.WriteAllBytes(path, pngData);
        }
    }
}
#endif
