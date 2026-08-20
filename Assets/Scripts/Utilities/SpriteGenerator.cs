using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Generates sprites procedurally at runtime so the game requires no external art assets.
    /// All textures are created pixel-by-pixel and wrapped into Sprites at 100 pixels-per-unit.
    /// </summary>
    public static class SpriteGenerator
    {
        private const float PixelsPerUnit = 100f;

        private static Sprite MakeSprite(Texture2D tex)
        {
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), PixelsPerUnit);
        }

        private static Texture2D NewTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var clear = new Color32(0, 0, 0, 0);
            var pixels = new Color32[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
            tex.SetPixels32(pixels);
            return tex;
        }

        /// <summary>Simple filled rectangle sprite.</summary>
        public static Sprite CreateRect(int w, int h, Color color)
        {
            w = Mathf.Max(1, w);
            h = Mathf.Max(1, h);
            var tex = NewTexture(w, h);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, color);
            return MakeSprite(tex);
        }

        /// <summary>Filled circle sprite.</summary>
        public static Sprite CreateCircle(int radius, Color color)
        {
            radius = Mathf.Max(1, radius);
            int size = radius * 2;
            var tex = NewTexture(size, size);
            Vector2 center = new Vector2(radius - 0.5f, radius - 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius - 0.5f)
                    {
                        tex.SetPixel(x, y, color);
                    }
                    else if (dist <= radius)
                    {
                        // anti-alias edge
                        Color edge = color;
                        edge.a *= Mathf.Clamp01(radius - dist);
                        tex.SetPixel(x, y, edge);
                    }
                }
            }
            return MakeSprite(tex);
        }

        /// <summary>Draws a simple upward-facing ship shape pixel by pixel.</summary>
        public static Sprite CreateShip(Color bodyColor, Color accentColor)
        {
            int w = 32, h = 32;
            var tex = NewTexture(w, h);
            // Ship body is a triangle-ish hull pointing up with wings.
            for (int y = 0; y < h; y++)
            {
                // Normalized height 0 (bottom) -> 1 (top)
                float t = y / (float)(h - 1);
                // half-width of hull tapers to a point at the top
                int hullHalf = Mathf.RoundToInt(Mathf.Lerp(6f, 1f, t));
                int cx = w / 2;
                for (int dx = -hullHalf; dx <= hullHalf; dx++)
                {
                    int x = cx + dx;
                    if (x >= 0 && x < w) tex.SetPixel(x, y, bodyColor);
                }
            }
            // Wings near the bottom third
            for (int y = 2; y < 12; y++)
            {
                int span = 14 - y; // wider at bottom
                int cx = w / 2;
                for (int dx = 6; dx <= 6 + span; dx++)
                {
                    if (cx + dx < w) tex.SetPixel(cx + dx, y, accentColor);
                    if (cx - dx >= 0) tex.SetPixel(cx - dx, y, accentColor);
                }
            }
            // Cockpit accent
            for (int y = 16; y < 24; y++)
            {
                for (int x = w / 2 - 1; x <= w / 2 + 1; x++)
                {
                    if (x >= 0 && x < w) tex.SetPixel(x, y, accentColor);
                }
            }
            // Engine flames accent at bottom center
            for (int y = 0; y < 3; y++)
            {
                for (int x = w / 2 - 2; x <= w / 2 + 2; x++)
                {
                    if (x >= 0 && x < w) tex.SetPixel(x, y, accentColor);
                }
            }
            return MakeSprite(tex);
        }

        /// <summary>Draws a larger, meaner ship shape for the boss.</summary>
        public static Sprite CreateBoss(Color color)
        {
            int w = 96, h = 64;
            var tex = NewTexture(w, h);
            Color accent = Color.Lerp(color, Color.black, 0.35f);
            int cx = w / 2;
            // Main hull: wide diamond/arrow pointing DOWN (boss faces the player below)
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1); // 0 bottom -> 1 top
                // widest in the middle
                int half = Mathf.RoundToInt(Mathf.Lerp(4f, 40f, 1f - Mathf.Abs(t - 0.55f) * 1.6f));
                half = Mathf.Max(0, half);
                for (int dx = -half; dx <= half; dx++)
                {
                    int x = cx + dx;
                    if (x >= 0 && x < w) tex.SetPixel(x, y, color);
                }
            }
            // Side cannons
            for (int y = 20; y < 44; y++)
            {
                for (int x = 4; x < 12; x++) tex.SetPixel(x, y, accent);
                for (int x = w - 12; x < w - 4; x++) tex.SetPixel(x, y, accent);
            }
            // Core
            for (int y = h / 2 - 8; y < h / 2 + 8; y++)
                for (int x = cx - 8; x < cx + 8; x++)
                    if (x >= 0 && x < w && y >= 0 && y < h)
                        tex.SetPixel(x, y, accent);
            return MakeSprite(tex);
        }

        /// <summary>Four-point star / sparkle sprite.</summary>
        public static Sprite CreateStar(int size, Color color)
        {
            size = Mathf.Max(3, size);
            var tex = NewTexture(size, size);
            int c = size / 2;
            for (int i = 0; i < size; i++)
            {
                tex.SetPixel(c, i, color); // vertical
                tex.SetPixel(i, c, color); // horizontal
            }
            // small diagonal glints
            tex.SetPixel(c, c, color);
            return MakeSprite(tex);
        }
    }
}
