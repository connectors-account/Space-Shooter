using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Creates simple geometric sprites at runtime so the project works without imported art assets.
    /// </summary>
    public static class RuntimeShapeFactory
    {
        public static Sprite CreateRectangleSprite(int width = 32, int height = 32)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
        }

        public static Sprite CreateDiamondSprite(int size = 48)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Mathf.Abs(x - center) + Mathf.Abs(y - center);
                    texture.SetPixel(x, y, distance <= center ? Color.white : Color.clear);
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }

        public static Sprite CreateCircleSprite(int size = 32)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var radius = size * 0.5f;
            var center = new Vector2(radius, radius);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(center, new Vector2(x, y));
                    texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
