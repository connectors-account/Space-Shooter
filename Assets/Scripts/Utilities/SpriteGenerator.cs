using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Procedurally generates all sprites used by the game at runtime, so that
    /// no external art assets are required. Every method draws pixels onto a
    /// <see cref="Texture2D"/> and returns a ready-to-use <see cref="Sprite"/>.
    ///
    /// Sprites are cached so repeated calls are cheap.
    /// </summary>
    public static class SpriteGenerator
    {
        private const int PixelsPerUnit = 32;

        // Cache so we only build each sprite once.
        private static readonly System.Collections.Generic.Dictionary<string, Sprite> Cache =
            new System.Collections.Generic.Dictionary<string, Sprite>();

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------
        private static Texture2D NewTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            // Clear to fully transparent.
            var clear = new Color32(0, 0, 0, 0);
            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
            tex.SetPixels32(pixels);
            return tex;
        }

        private static Sprite Finalise(Texture2D tex, string cacheKey)
        {
            tex.Apply();
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            sprite.name = cacheKey;
            Cache[cacheKey] = sprite;
            return sprite;
        }

        private static void FillRect(Texture2D tex, int x0, int y0, int x1, int y1, Color c)
        {
            int w = tex.width, h = tex.height;
            for (int y = Mathf.Max(0, y0); y <= Mathf.Min(h - 1, y1); y++)
                for (int x = Mathf.Max(0, x0); x <= Mathf.Min(w - 1, x1); x++)
                    tex.SetPixel(x, y, c);
        }

        private static void FillDisc(Texture2D tex, float cx, float cy, float radius, Color c)
        {
            int w = tex.width, h = tex.height;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= radius * radius)
                        tex.SetPixel(x, y, c);
                }
        }

        /// <summary>Draw a filled triangle by scanning rows between two edges.</summary>
        private static void FillTriangle(Texture2D tex, Vector2 a, Vector2 b, Vector2 cc, Color col)
        {
            int minY = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, cc.y)));
            int maxY = Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, cc.y)));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    if (PointInTriangle(new Vector2(x + 0.5f, y + 0.5f), a, b, cc))
                        tex.SetPixel(x, y, col);
                }
            }
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        // -----------------------------------------------------------------
        // Public sprite builders
        // -----------------------------------------------------------------
        public static Sprite CreatePlayerSprite()
        {
            const string key = "player";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            int size = 32;
            var tex = NewTexture(size);
            Color hull = new Color(0.30f, 0.75f, 1f);
            Color hullDark = new Color(0.15f, 0.45f, 0.85f);
            Color cockpit = new Color(0.85f, 0.95f, 1f);
            Color flame = new Color(1f, 0.6f, 0.1f);

            // Body triangle pointing up.
            FillTriangle(tex, new Vector2(16, 30), new Vector2(4, 4), new Vector2(28, 4), hull);
            // Wings.
            FillTriangle(tex, new Vector2(2, 10), new Vector2(8, 4), new Vector2(8, 14), hullDark);
            FillTriangle(tex, new Vector2(30, 10), new Vector2(24, 4), new Vector2(24, 14), hullDark);
            // Cockpit.
            FillDisc(tex, 16, 18, 3.2f, cockpit);
            // Engine flames.
            FillRect(tex, 12, 0, 14, 4, flame);
            FillRect(tex, 18, 0, 20, 4, flame);

            return Finalise(tex, key);
        }

        public static Sprite CreateEnemyDroneSprite()
        {
            const string key = "enemy_drone";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var tex = NewTexture(32);
            Color body = new Color(0.9f, 0.35f, 0.35f);
            Color dark = new Color(0.55f, 0.15f, 0.15f);
            Color eye = new Color(1f, 0.9f, 0.4f);

            FillDisc(tex, 16, 16, 10, body);
            FillDisc(tex, 16, 16, 5, dark);
            FillDisc(tex, 16, 16, 2.2f, eye);
            return Finalise(tex, key);
        }

        public static Sprite CreateEnemyFighterSprite()
        {
            const string key = "enemy_fighter";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var tex = NewTexture(32);
            Color body = new Color(0.85f, 0.55f, 0.2f);
            Color dark = new Color(0.55f, 0.32f, 0.1f);
            Color glass = new Color(0.9f, 0.9f, 0.5f);

            // Pointing down (towards player).
            FillTriangle(tex, new Vector2(16, 2), new Vector2(4, 28), new Vector2(28, 28), body);
            FillTriangle(tex, new Vector2(2, 22), new Vector2(8, 28), new Vector2(8, 18), dark);
            FillTriangle(tex, new Vector2(30, 22), new Vector2(24, 28), new Vector2(24, 18), dark);
            FillDisc(tex, 16, 16, 3f, glass);
            return Finalise(tex, key);
        }

        public static Sprite CreateEnemyBomberSprite()
        {
            const string key = "enemy_bomber";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var tex = NewTexture(32);
            Color body = new Color(0.6f, 0.35f, 0.7f);
            Color dark = new Color(0.4f, 0.2f, 0.5f);
            Color light = new Color(0.9f, 0.6f, 1f);

            FillRect(tex, 6, 8, 26, 24, body);
            FillRect(tex, 2, 12, 6, 20, dark);
            FillRect(tex, 26, 12, 30, 20, dark);
            FillDisc(tex, 16, 16, 4f, light);
            return Finalise(tex, key);
        }

        public static Sprite CreateBossSprite()
        {
            const string key = "boss";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            int size = 64;
            var tex = NewTexture(size);
            Color body = new Color(0.7f, 0.2f, 0.25f);
            Color dark = new Color(0.4f, 0.1f, 0.12f);
            Color core = new Color(1f, 0.5f, 0.2f);
            Color trim = new Color(0.9f, 0.8f, 0.4f);

            FillRect(tex, 8, 20, 56, 52, body);
            FillTriangle(tex, new Vector2(32, 4), new Vector2(8, 24), new Vector2(56, 24), body);
            FillRect(tex, 2, 26, 10, 46, dark);
            FillRect(tex, 54, 26, 62, 46, dark);
            FillDisc(tex, 32, 34, 7f, core);
            FillRect(tex, 14, 46, 50, 50, trim);
            return Finalise(tex, key);
        }

        public static Sprite CreateBulletSprite(Color colour)
        {
            string key = "bullet_" + colour.r.ToString("F2") + "_" + colour.g.ToString("F2") + "_" + colour.b.ToString("F2");
            if (Cache.TryGetValue(key, out var cached)) return cached;

            int size = 16;
            var tex = NewTexture(size);
            // Capsule-like bullet.
            FillDisc(tex, 8, 8, 4f, colour);
            FillRect(tex, 6, 4, 9, 12, colour);
            // Bright core.
            FillDisc(tex, 8, 8, 1.8f, Color.Lerp(colour, Color.white, 0.6f));
            return Finalise(tex, key);
        }

        public static Sprite CreatePowerUpSprite(PowerUpType type)
        {
            string key = "powerup_" + type;
            if (Cache.TryGetValue(key, out var cached)) return cached;

            int size = 32;
            var tex = NewTexture(size);
            Color ring;
            Color inner;
            switch (type)
            {
                case PowerUpType.Shield:     ring = new Color(0.3f, 0.7f, 1f);   inner = new Color(0.6f, 0.9f, 1f); break;
                case PowerUpType.RapidFire:  ring = new Color(1f, 0.85f, 0.2f);  inner = new Color(1f, 0.95f, 0.6f); break;
                case PowerUpType.TripleShot: ring = new Color(0.4f, 1f, 0.5f);   inner = new Color(0.7f, 1f, 0.75f); break;
                case PowerUpType.Bomb:       ring = new Color(1f, 0.4f, 0.3f);   inner = new Color(1f, 0.7f, 0.5f); break;
                default:                     ring = new Color(0.8f, 0.5f, 1f);   inner = new Color(0.9f, 0.75f, 1f); break; // Speed
            }

            FillDisc(tex, 16, 16, 13f, ring);
            FillDisc(tex, 16, 16, 10f, new Color(0.05f, 0.05f, 0.1f, 1f));
            FillDisc(tex, 16, 16, 8f, inner);

            // Draw a simple glyph per type.
            switch (type)
            {
                case PowerUpType.Shield:
                    FillTriangle(tex, new Vector2(16, 24), new Vector2(9, 10), new Vector2(23, 10), ring);
                    break;
                case PowerUpType.RapidFire:
                    FillRect(tex, 14, 8, 18, 24, ring);
                    break;
                case PowerUpType.TripleShot:
                    FillRect(tex, 10, 8, 12, 24, ring);
                    FillRect(tex, 15, 8, 17, 24, ring);
                    FillRect(tex, 20, 8, 22, 24, ring);
                    break;
                case PowerUpType.Bomb:
                    FillDisc(tex, 16, 15, 5f, ring);
                    FillRect(tex, 15, 20, 17, 26, ring);
                    break;
                default: // Speed – chevrons
                    FillTriangle(tex, new Vector2(16, 22), new Vector2(10, 14), new Vector2(22, 14), ring);
                    FillTriangle(tex, new Vector2(16, 16), new Vector2(10, 8), new Vector2(22, 8), ring);
                    break;
            }
            return Finalise(tex, key);
        }

        public static Sprite CreateExplosionSprite()
        {
            const string key = "explosion";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            int size = 32;
            var tex = NewTexture(size);
            Color outer = new Color(1f, 0.5f, 0.1f, 1f);
            Color mid = new Color(1f, 0.8f, 0.2f, 1f);
            Color hot = new Color(1f, 1f, 0.9f, 1f);

            FillDisc(tex, 16, 16, 14f, outer);
            FillDisc(tex, 16, 16, 9f, mid);
            FillDisc(tex, 16, 16, 4f, hot);
            return Finalise(tex, key);
        }

        public static Sprite CreateShieldSprite()
        {
            const string key = "shield";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            int size = 48;
            var tex = NewTexture(size);
            Color glow = new Color(0.4f, 0.8f, 1f, 0.55f);
            float cx = 24, cy = 24;
            // Ring only (hollow) so ship stays visible.
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d <= 22f && d >= 17f)
                        tex.SetPixel(x, y, glow);
                }
            return Finalise(tex, key);
        }

        public static Sprite CreateStarSprite()
        {
            const string key = "star";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            int size = 4;
            var tex = NewTexture(size);
            FillRect(tex, 0, 0, size - 1, size - 1, Color.white);
            return Finalise(tex, key);
        }

        /// <summary>Solid white 1x1 sprite handy for panels / bars.</summary>
        public static Sprite CreateSquareSprite()
        {
            const string key = "square";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var tex = NewTexture(4);
            FillRect(tex, 0, 0, 3, 3, Color.white);
            return Finalise(tex, key);
        }
    }
}
