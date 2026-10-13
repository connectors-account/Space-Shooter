using System;
using System.Collections.Generic;

namespace SpaceShooter
{
    // Engine-independent rules, exercised by Tests/ModelTests.cs.
    public sealed class ShooterModel
    {
        public sealed class Body
        {
            public float X, Y;
            public Body(float x, float y) { X = x; Y = y; }
        }

        public const float HalfWidth = 8f, HalfHeight = 5f;
        public const float PlayerSpeed = 6f, BulletSpeed = 12f, EnemySpeed = 2f;
        public const float PlayerHalfSize = 0.4f, EnemyHalfSize = 0.4f;
        public const float BulletHalfWidth = 0.08f, BulletHalfHeight = 0.18f;
        public readonly List<Body> Bullets = new List<Body>();
        public readonly List<Body> Enemies = new List<Body>();
        public float PlayerX { get; private set; }
        public float PlayerY { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }
        private readonly Random random;
        private float spawnTimer, shotTimer;

        public ShooterModel(int seed) { random = new Random(seed); Restart(); }

        public void Restart()
        {
            PlayerX = 0f;
            PlayerY = -3.8f;
            Score = 0;
            GameOver = false;
            Bullets.Clear();
            Enemies.Clear();
            spawnTimer = 1.2f;
            shotTimer = 0f;
        }

        public void Advance(float seconds, float horizontal, float vertical, bool firing)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0f)
                throw new ArgumentOutOfRangeException("seconds");
            if (float.IsNaN(horizontal) || float.IsInfinity(horizontal) ||
                float.IsNaN(vertical) || float.IsInfinity(vertical))
                throw new ArgumentException("Movement must be finite.");
            // Bound catch-up and use small collision steps, including after a slow frame.
            float remaining = Math.Min(seconds, 0.1f);
            while (remaining > 0f && !GameOver)
            {
                float dt = Math.Min(remaining, 1f / 120f);
                Tick(dt, horizontal, vertical, firing);
                remaining -= dt;
            }
        }

        private void Tick(float dt, float horizontal, float vertical, bool firing)
        {
            horizontal = Clamp(horizontal, -1f, 1f);
            vertical = Clamp(vertical, -1f, 1f);
            float length = (float)Math.Sqrt(horizontal * horizontal + vertical * vertical);
            if (length > 1f) { horizontal /= length; vertical /= length; }
            PlayerX = Clamp(PlayerX + horizontal * PlayerSpeed * dt, -7.6f, 7.6f);
            PlayerY = Clamp(PlayerY + vertical * PlayerSpeed * dt, -4.6f, 4.2f);
            shotTimer -= dt;
            if (firing && shotTimer <= 0f)
            {
                Bullets.Add(new Body(PlayerX, PlayerY + 0.55f));
                shotTimer = 0.18f;
            }
            spawnTimer -= dt;
            if (spawnTimer <= 0f)
            {
                Enemies.Add(new Body((float)(random.NextDouble() * 14.8 - 7.4), 5.5f));
                spawnTimer += 0.85f;
            }
            foreach (Body bullet in Bullets) bullet.Y += BulletSpeed * dt;
            foreach (Body enemy in Enemies) enemy.Y -= EnemySpeed * dt;

            // A bullet can destroy only one enemy, and an enemy awards points only once.
            for (int b = Bullets.Count - 1; b >= 0; b--)
            {
                Body bullet = Bullets[b];
                bool hit = false;
                for (int e = Enemies.Count - 1; e >= 0; e--)
                {
                    Body enemy = Enemies[e];
                    if (Overlap(bullet.X, bullet.Y, enemy.X, enemy.Y,
                        BulletHalfWidth + EnemyHalfSize, BulletHalfHeight + EnemyHalfSize))
                    {
                        Enemies.RemoveAt(e);
                        Score += 10;
                        hit = true;
                        break;
                    }
                }
                if (hit || bullet.Y > 5.8f) Bullets.RemoveAt(b);
            }
            foreach (Body enemy in Enemies)
            {
                if (enemy.Y < -5.4f || Overlap(PlayerX, PlayerY, enemy.X, enemy.Y,
                    PlayerHalfSize + EnemyHalfSize, PlayerHalfSize + EnemyHalfSize))
                {
                    GameOver = true;
                    break;
                }
            }
        }

        private static bool Overlap(float ax, float ay, float bx, float by, float w, float h)
        { return Math.Abs(ax - bx) <= w && Math.Abs(ay - by) <= h; }
        private static float Clamp(float value, float min, float max)
        { return Math.Max(min, Math.Min(max, value)); }
    }
}
