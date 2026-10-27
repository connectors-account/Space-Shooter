using System;
using System.Collections.Generic;

namespace MinimalSpaceShooter
{
    // Pure C# rules; Unity only supplies input and draws this state.
    public sealed class ShooterModel
    {
        public const float HalfWidth = 8f;
        public const float HalfHeight = 4.5f;
        public const float PlayerHalfWidth = 0.5f;
        public const float PlayerHalfHeight = 0.45f;
        public const float EnemyHalfSize = 0.45f;
        public const float BulletHalfWidth = 0.09f;
        public const float BulletHalfHeight = 0.22f;
        public const float PlayerSpeed = 6f;
        public const float BulletSpeed = 12f;
        public const float EnemySpeed = 2f;
        public const float ShotInterval = 0.18f;
        public const float SpawnInterval = 0.85f;

        public sealed class Body
        {
            public readonly int Id;
            public float X;
            public float Y;
            public Body(int id, float x, float y) { Id = id; X = x; Y = y; }
        }

        internal readonly List<Body> bullets = new List<Body>();
        internal readonly List<Body> enemies = new List<Body>();
        public IReadOnlyList<Body> Bullets { get { return bullets; } }
        public IReadOnlyList<Body> Enemies { get { return enemies; } }
        public float PlayerX { get; private set; }
        public float PlayerY { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }
        private readonly Random random;
        private float shotTimer;
        private float spawnTimer;
        private int nextId;

        public ShooterModel(int seed)
        {
            random = new Random(seed);
            Restart();
        }

        public void Restart()
        {
            bullets.Clear();
            enemies.Clear();
            PlayerX = 0f;
            PlayerY = -3.3f;
            Score = 0;
            GameOver = false;
            shotTimer = 0f;
            spawnTimer = SpawnInterval;
            nextId = 1;
        }

        public void Step(float dt, float horizontal, float vertical, bool fire)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt <= 0f || dt > 0.05f)
                throw new ArgumentOutOfRangeException("dt", "Use a positive timestep of at most 0.05 seconds.");
            if (float.IsNaN(horizontal) || float.IsInfinity(horizontal) ||
                float.IsNaN(vertical) || float.IsInfinity(vertical))
                throw new ArgumentException("Movement must be finite.");
            if (GameOver) return;

            float oldX = PlayerX;
            float oldY = PlayerY;
            float length = (float)Math.Sqrt(horizontal * horizontal + vertical * vertical);
            if (length > 1f) { horizontal /= length; vertical /= length; }
            PlayerX = Clamp(PlayerX + horizontal * PlayerSpeed * dt, -7.5f, 7.5f);
            // Reserve the top strip for arrivals and the HUD.
            PlayerY = Clamp(PlayerY + vertical * PlayerSpeed * dt, -4.05f, 3.15f);
            shotTimer -= dt;
            if (fire && shotTimer <= 0f)
            {
                bullets.Add(new Body(nextId++, PlayerX, PlayerY + 0.7f));
                shotTimer = ShotInterval;
            }
            spawnTimer -= dt;
            if (spawnTimer <= 0f)
            {
                float x = ((float)random.NextDouble() * 2f - 1f) * 7.4f;
                enemies.Add(new Body(nextId++, x, HalfHeight + EnemyHalfSize));
                spawnTimer += SpawnInterval;
            }

            for (int i = 0; i < bullets.Count; i++) bullets[i].Y += BulletSpeed * dt;
            for (int i = 0; i < enemies.Count; i++) enemies[i].Y -= EnemySpeed * dt;

            // Sweep relative bullet/enemy motion so fast shots cannot skip a target.
            for (int b = bullets.Count - 1; b >= 0; b--)
            {
                Body bullet = bullets[b];
                for (int e = enemies.Count - 1; e >= 0; e--)
                {
                    Body enemy = enemies[e];
                    float relativeY = bullet.Y - enemy.Y;
                    if (SegmentHitsBox(bullet.X - enemy.X,
                        relativeY - (BulletSpeed + EnemySpeed) * dt,
                        bullet.X - enemy.X, relativeY,
                        BulletHalfWidth + EnemyHalfSize, BulletHalfHeight + EnemyHalfSize))
                    {
                        bullets.RemoveAt(b);
                        enemies.RemoveAt(e);
                        Score += 10;
                        break;
                    }
                }
            }
            for (int e = 0; e < enemies.Count; e++)
            {
                Body enemy = enemies[e];
                if (SegmentHitsBox(oldX - enemy.X, oldY - enemy.Y - EnemySpeed * dt,
                    PlayerX - enemy.X, PlayerY - enemy.Y,
                    PlayerHalfWidth + EnemyHalfSize, PlayerHalfHeight + EnemyHalfSize)
                    || enemy.Y + EnemyHalfSize < -HalfHeight)
                {
                    GameOver = true;
                    return;
                }
            }
            for (int b = bullets.Count - 1; b >= 0; b--)
                if (bullets[b].Y - BulletHalfHeight > HalfHeight) bullets.RemoveAt(b);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static bool SegmentHitsBox(float x0, float y0, float x1, float y1, float halfX, float halfY)
        {
            float near = 0f, far = 1f;
            return ClipAxis(x0, x1 - x0, halfX, ref near, ref far)
                && ClipAxis(y0, y1 - y0, halfY, ref near, ref far);
        }

        private static bool ClipAxis(float start, float delta, float extent, ref float near, ref float far)
        {
            if (Math.Abs(delta) < 0.000001f) return Math.Abs(start) <= extent;
            float a = (-extent - start) / delta;
            float b = (extent - start) / delta;
            if (a > b) { float temp = a; a = b; b = temp; }
            near = Math.Max(near, a);
            far = Math.Min(far, b);
            return near <= far;
        }
    }
}
