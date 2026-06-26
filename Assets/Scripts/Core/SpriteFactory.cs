using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Generates simple procedural sprites at runtime so the game is fully playable
    /// even when no custom art assets have been imported. Generated textures/sprites are
    /// cached so repeated requests are cheap.
    /// </summary>
    public static class SpriteFactory
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private const float PixelsPerUnit = 100f;

        /// <summary>
        /// Returns a triangular "ship" sprite pointing up, tinted with the supplied colour.
        /// </summary>
        /// <param name="color">Fill colour of the ship body.</param>
        /// <param name="size">Texture size in pixels (square).</param>
        public static Sprite CreateShipSprite(Color color, int size = 64)
        {
            string key = $"ship_{ColorKey(color)}_{size}";
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D tex = NewTexture(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Build an upward pointing triangle.
                    float nx = (x + 0.5f) / size;          // 0..1
                    float ny = (y + 0.5f) / size;          // 0..1
                    float halfWidthAtY = ny * 0.5f;        // widens toward the bottom
                    bool inside = Mathf.Abs(nx - 0.5f) <= halfWidthAtY && ny <= 1f;
                    tex.SetPixel(x, y, inside ? color : Color.clear);
                }
            }
            tex.Apply();
            return Store(key, tex);
        }

        /// <summary>
        /// Returns a filled circle sprite of the supplied colour. Used for bullets, power-ups and bosses.
        /// </summary>
        public static Sprite CreateCircleSprite(Color color, int size = 32)
        {
            string key = $"circle_{ColorKey(color)}_{size}";
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D tex = NewTexture(size);
            float radius = size * 0.5f;
            Vector2 center = new Vector2(radius, radius);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    tex.SetPixel(x, y, dist <= radius ? color : Color.clear);
                }
            }
            tex.Apply();
            return Store(key, tex);
        }

        /// <summary>
        /// Returns a solid square sprite of the supplied colour. Used for enemies and UI fills.
        /// </summary>
        public static Sprite CreateSquareSprite(Color color, int size = 48)
        {
            string key = $"square_{ColorKey(color)}_{size}";
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D tex = NewTexture(size);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Store(key, tex);
        }

        /// <summary>
        /// Returns a small "star" point sprite (soft circle) used by the parallax backgrounds.
        /// </summary>
        public static Sprite CreateStarSprite(Color color, int size = 8)
        {
            string key = $"star_{ColorKey(color)}_{size}";
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D tex = NewTexture(size);
            float radius = size * 0.5f;
            Vector2 center = new Vector2(radius, radius);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    Color c = color;
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return Store(key, tex);
        }

        /// <summary>
        /// Returns a 1x1 white sprite. Useful for UI fills that are colour-tinted later.
        /// </summary>
        public static Sprite CreateWhitePixel()
        {
            const string key = "white_pixel";
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D tex = NewTexture(1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Store(key, tex);
        }

        private static Texture2D NewTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            return tex;
        }

        private static Sprite Store(string key, Texture2D tex)
        {
            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        private static string ColorKey(Color c)
        {
            return $"{Mathf.RoundToInt(c.r * 255)}_{Mathf.RoundToInt(c.g * 255)}_{Mathf.RoundToInt(c.b * 255)}_{Mathf.RoundToInt(c.a * 255)}";
        }
    }
}
