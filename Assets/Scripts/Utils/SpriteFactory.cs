using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Utils
{
    public enum ShapeType
    {
        Square,
        Triangle,
        Circle
    }

    public static class SpriteFactory
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite GetSprite(Color color, ShapeType shape, int size = 32)
        {
            var key = $"{color}-{shape}-{size}";
            if (Cache.TryGetValue(key, out var sprite))
            {
                return sprite;
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var drawPixel = shape switch
                    {
                        ShapeType.Square => true,
                        ShapeType.Circle => IsInsideCircle(x, y, size),
                        ShapeType.Triangle => IsInsideTriangle(x, y, size),
                        _ => true
                    };

                    pixels[y * size + x] = drawPixel ? color : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            Cache[key] = sprite;
            return sprite;
        }

        private static bool IsInsideCircle(int x, int y, int size)
        {
            var center = (size - 1) / 2f;
            var dx = x - center;
            var dy = y - center;
            var radius = size * 0.48f;
            return (dx * dx) + (dy * dy) <= radius * radius;
        }

        private static bool IsInsideTriangle(int x, int y, int size)
        {
            var normalizedY = y / (float)(size - 1);
            var halfWidth = normalizedY * 0.5f;
            var center = 0.5f;
            var normalizedX = x / (float)(size - 1);
            return normalizedX >= center - halfWidth && normalizedX <= center + halfWidth;
        }
    }
}
