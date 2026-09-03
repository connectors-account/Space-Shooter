using UnityEngine;
using SpaceShooter.Pickups;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Generates every game sprite procedurally with Texture2D pixel drawing.
    /// All textures are RGBA32, point-filtered, no mipmaps. Returns Sprites ready to use.
    /// </summary>
    public static class SpriteGenerator
    {
        #region Constants
        private const float PIXELS_PER_UNIT = 32f;
        private static readonly Color Clear = new Color(0f, 0f, 0f, 0f);
        #endregion

        #region Texture Helpers
        private static Texture2D NewTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        private static Color[] NewBuffer(int w, int h)
        {
            Color[] buf = new Color[w * h];
            for (int i = 0; i < buf.Length; i++) buf[i] = Clear;
            return buf;
        }

        private static void SetPx(Color[] buf, int w, int h, int x, int y, Color c)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) return;
            buf[y * w + x] = c;
        }

        /// <summary>Mirrors pixels across the vertical center for symmetric ships.</summary>
        private static void SetPxMirror(Color[] buf, int w, int h, int x, int y, Color c)
        {
            SetPx(buf, w, h, x, y, c);
            SetPx(buf, w, h, w - 1 - x, y, c);
        }

        private static Sprite BuildSprite(Color[] buf, int w, int h, float ppu = PIXELS_PER_UNIT)
        {
            Texture2D tex = NewTexture(w, h);
            tex.SetPixels(buf);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }

        private static void FillRect(Color[] buf, int w, int h, int x0, int y0, int x1, int y1, Color c)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    SetPx(buf, w, h, x, y, c);
        }

        private static void FillCircle(Color[] buf, int w, int h, int cx, int cy, float radius, Color c)
        {
            int r = Mathf.CeilToInt(radius);
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    if (x * x + y * y <= radius * radius)
                        SetPx(buf, w, h, cx + x, cy + y, c);
        }
        #endregion

        #region Ships
        /// <summary>32x32 blue/white player ship silhouette pointing up.</summary>
        public static Sprite GeneratePlayerShip()
        {
            int w = 32, h = 32;
            Color[] buf = NewBuffer(w, h);
            Color body = new Color(0.25f, 0.55f, 1f);
            Color light = new Color(0.75f, 0.9f, 1f);
            Color cockpit = new Color(0.9f, 1f, 1f);
            Color engine = new Color(1f, 0.6f, 0.2f);

            // Nose to tail (y grows upward). Build a triangular hull, mirrored.
            for (int y = 4; y < 28; y++)
            {
                int spread = Mathf.RoundToInt((y - 4) * 0.5f); // widens toward the back
                int cx = w / 2;
                for (int x = cx - spread; x <= cx; x++)
                    SetPxMirror(buf, w, h, x, y, body);
            }

            // Wings near the back.
            FillRect(buf, w, h, 2, 8, 8, 12, light);
            FillRect(buf, w, h, w - 9, 8, w - 3, 12, light);

            // Cockpit.
            FillCircle(buf, w, h, w / 2, 20, 3f, cockpit);

            // Engine glow at the tail.
            FillRect(buf, w, h, w / 2 - 3, 3, w / 2 + 2, 6, engine);

            return BuildSprite(buf, w, h);
        }

        /// <summary>24x24 red enemy fighter pointing down.</summary>
        public static Sprite GenerateEnemyA()
        {
            int w = 24, h = 24;
            Color[] buf = NewBuffer(w, h);
            Color body = new Color(0.9f, 0.2f, 0.2f);
            Color trim = new Color(1f, 0.6f, 0.4f);

            for (int y = 4; y < 20; y++)
            {
                int spread = Mathf.RoundToInt((20 - y) * 0.5f);
                int cx = w / 2;
                for (int x = cx - spread; x <= cx; x++)
                    SetPxMirror(buf, w, h, x, y, body);
            }
            FillRect(buf, w, h, 2, 12, 7, 16, trim);
            FillRect(buf, w, h, w - 8, 12, w - 3, 16, trim);
            FillCircle(buf, w, h, w / 2, 10, 2.5f, new Color(1f, 0.9f, 0.6f));
            return BuildSprite(buf, w, h);
        }

        /// <summary>28x28 yellow interceptor pointing down.</summary>
        public static Sprite GenerateEnemyB()
        {
            int w = 28, h = 28;
            Color[] buf = NewBuffer(w, h);
            Color body = new Color(0.95f, 0.85f, 0.2f);
            Color trim = new Color(1f, 0.7f, 0.1f);

            // Diamond-ish interceptor.
            for (int y = 4; y < 24; y++)
            {
                int dist = Mathf.Abs(y - 14);
                int spread = 10 - dist;
                if (spread < 0) spread = 0;
                int cx = w / 2;
                for (int x = cx - spread; x <= cx; x++)
                    SetPxMirror(buf, w, h, x, y, body);
            }
            FillRect(buf, w, h, 1, 13, 5, 15, trim);
            FillRect(buf, w, h, w - 6, 13, w - 2, 15, trim);
            FillCircle(buf, w, h, w / 2, 14, 2.5f, new Color(1f, 1f, 0.7f));
            return BuildSprite(buf, w, h);
        }

        /// <summary>30x30 green orbiter, roughly circular.</summary>
        public static Sprite GenerateEnemyC()
        {
            int w = 30, h = 30;
            Color[] buf = NewBuffer(w, h);
            Color body = new Color(0.2f, 0.85f, 0.35f);
            Color ring = new Color(0.6f, 1f, 0.7f);
            Color core = new Color(0.9f, 1f, 0.9f);

            FillCircle(buf, w, h, w / 2, h / 2, 11f, body);
            // Ring accent.
            for (int a = 0; a < 360; a += 6)
            {
                float rad = a * Mathf.Deg2Rad;
                int x = Mathf.RoundToInt(w / 2 + Mathf.Cos(rad) * 9f);
                int y = Mathf.RoundToInt(h / 2 + Mathf.Sin(rad) * 9f);
                SetPx(buf, w, h, x, y, ring);
            }
            FillCircle(buf, w, h, w / 2, h / 2, 3.5f, core);
            return BuildSprite(buf, w, h);
        }

        /// <summary>64x64 purple boss dreadnought.</summary>
        public static Sprite GenerateBoss()
        {
            int w = 64, h = 64;
            Color[] buf = NewBuffer(w, h);
            Color hull = new Color(0.55f, 0.25f, 0.8f);
            Color hullDark = new Color(0.35f, 0.15f, 0.55f);
            Color trim = new Color(0.85f, 0.6f, 1f);
            Color core = new Color(1f, 0.4f, 0.9f);

            // Main body: wide hexagonal hull.
            for (int y = 12; y < 52; y++)
            {
                int dist = Mathf.Abs(y - 32);
                int spread = 28 - dist / 2;
                if (spread < 0) spread = 0;
                int cx = w / 2;
                for (int x = cx - spread; x <= cx; x++)
                    SetPxMirror(buf, w, h, x, y, (y % 4 == 0) ? hullDark : hull);
            }

            // Side cannons.
            FillRect(buf, w, h, 2, 26, 10, 38, trim);
            FillRect(buf, w, h, w - 11, 26, w - 3, 38, trim);

            // Central core.
            FillCircle(buf, w, h, w / 2, 28, 6f, core);

            // Bridge accents.
            FillRect(buf, w, h, w / 2 - 12, 44, w / 2 + 11, 48, trim);
            return BuildSprite(buf, w, h);
        }
        #endregion

        #region Projectiles
        /// <summary>6x12 capsule bullet; yellow for player, red for enemy.</summary>
        public static Sprite GenerateBullet(bool isEnemy)
        {
            int w = 6, h = 12;
            Color[] buf = NewBuffer(w, h);
            Color main = isEnemy ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 1f, 0.4f);
            Color glow = isEnemy ? new Color(1f, 0.6f, 0.6f) : new Color(1f, 1f, 0.8f);

            FillRect(buf, w, h, 1, 1, w - 2, h - 2, main);
            // Rounded ends.
            FillCircle(buf, w, h, w / 2, 2, 2f, main);
            FillCircle(buf, w, h, w / 2, h - 3, 2f, main);
            // Bright center line.
            for (int y = 2; y < h - 2; y++)
                SetPx(buf, w, h, w / 2, y, glow);
            return BuildSprite(buf, w, h, 32f);
        }
        #endregion

        #region VFX
        /// <summary>32x32 explosion frame (0-4). Larger, dimmer as frame increases.</summary>
        public static Sprite GenerateExplosion(int frame)
        {
            int w = 32, h = 32;
            Color[] buf = NewBuffer(w, h);
            frame = Mathf.Clamp(frame, 0, 4);

            float radius = 4f + frame * 5f;
            float alpha = 1f - frame * 0.18f;

            Color inner = new Color(1f, 1f, 0.7f, alpha);
            Color mid = new Color(1f, 0.6f, 0.15f, alpha);
            Color outer = new Color(0.9f, 0.25f, 0.1f, alpha * 0.7f);

            FillCircle(buf, w, h, w / 2, h / 2, radius, outer);
            FillCircle(buf, w, h, w / 2, h / 2, radius * 0.66f, mid);
            FillCircle(buf, w, h, w / 2, h / 2, radius * 0.33f, inner);
            return BuildSprite(buf, w, h);
        }

        /// <summary>4x4 white star pixel.</summary>
        public static Sprite GenerateStar()
        {
            int w = 4, h = 4;
            Color[] buf = NewBuffer(w, h);
            FillRect(buf, w, h, 0, 0, w - 1, h - 1, Color.white);
            return BuildSprite(buf, w, h, 4f);
        }
        #endregion

        #region PowerUps
        /// <summary>16x16 coloured power-up icon per type.</summary>
        public static Sprite GeneratePowerUp(PowerUpType t)
        {
            int w = 16, h = 16;
            Color[] buf = NewBuffer(w, h);
            Color c = PowerUp.GetColour(t);
            Color border = Color.white;

            // Filled diamond icon with a white border.
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int dist = Mathf.Abs(x - w / 2) + Mathf.Abs(y - h / 2);
                    if (dist <= 7) SetPx(buf, w, h, x, y, c);
                    if (dist == 7) SetPx(buf, w, h, x, y, border);
                }
            }

            // A small distinctive glyph in the center per type.
            Color glyph = new Color(0f, 0f, 0f, 0.85f);
            switch (t)
            {
                case PowerUpType.HealthPack: // plus
                    FillRect(buf, w, h, 7, 4, 8, 11, glyph);
                    FillRect(buf, w, h, 4, 7, 11, 8, glyph);
                    break;
                case PowerUpType.Shield: // ring
                    FillCircle(buf, w, h, 8, 8, 4f, glyph);
                    FillCircle(buf, w, h, 8, 8, 2.2f, c);
                    break;
                default: // dot
                    FillCircle(buf, w, h, 8, 8, 2.2f, glyph);
                    break;
            }
            return BuildSprite(buf, w, h, 16f);
        }
        #endregion

        #region Backgrounds
        /// <summary>
        /// 1920x1080 gradient/solid background per parallax layer.
        /// layer 0 = deep space gradient, 1 = nebula tint, 2 = near dark band.
        /// </summary>
        public static Sprite GenerateBackground(int layer)
        {
            int w = 1920, h = 1080;
            Texture2D tex = NewTexture(w, h);
            Color[] buf = new Color[w * h];

            Color top, bottom;
            switch (layer)
            {
                case 1:
                    top = new Color(0.10f, 0.02f, 0.18f, 0.5f);
                    bottom = new Color(0.02f, 0.05f, 0.15f, 0.5f);
                    break;
                case 2:
                    top = new Color(0.03f, 0.03f, 0.06f, 0.35f);
                    bottom = new Color(0f, 0f, 0.02f, 0.35f);
                    break;
                default:
                    top = new Color(0.02f, 0.02f, 0.08f, 1f);
                    bottom = new Color(0.0f, 0.0f, 0.02f, 1f);
                    break;
            }

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / (h - 1);
                Color row = Color.Lerp(bottom, top, t);
                for (int x = 0; x < w; x++)
                    buf[y * w + x] = row;
            }

            tex.SetPixels(buf);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
        #endregion
    }
}
