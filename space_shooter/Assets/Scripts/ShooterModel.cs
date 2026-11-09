using System;
using System.Collections.Generic;

namespace SpaceShooter
{
    // Pure C# simulation: no Unity dependencies, so gameplay can be tested separately.
    public sealed class ShooterModel
    {
        public const float HalfWidth = 8f;
        public const float Bottom = -9f;
        public const float Top = 9f;
        public const float PlayerSpeed = 7f;
        public const float BulletSpeed = 16f;
        public const float EnemySpeed = 3f;
        public const float FireInterval = 0.18f;
        public const float SpawnInterval = 0.8f;

        public sealed class Body
        {
            public int Id;
            public float X, Y;
            public readonly float Width, Height;
            public Body(int id, float x, float y, float width, float height)
            {
                Id = id; X = x; Y = y; Width = width; Height = height;
            }
        }

        public readonly Body Player = new Body(0, 0, -7, 0.8f, 0.7f);
        public readonly List<Body> Bullets = new List<Body>();
        public readonly List<Body> Enemies = new List<Body>();
        public int Score { get; private set; }
        public bool GameOver { get; private set; }
        public string EndReason { get; private set; }
        private readonly Random random;
        private float fireTimer, spawnTimer;
        private int nextId;

        public ShooterModel(int seed)
        {
            random = new Random(seed);
            Restart();
        }

        public void Restart()
        {
            Bullets.Clear(); Enemies.Clear();
            Player.X = 0; Player.Y = -7;
            Score = 0; GameOver = false; EndReason = string.Empty;
            fireTimer = 0; spawnTimer = 1.2f; nextId = 1;
        }

        public void Tick(float dt, float horizontal, float vertical, bool shoot)
        {
            if (GameOver || dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            dt = Math.Min(dt, 0.05f);
            float previousX = Player.X, previousY = Player.Y;
            float magnitude = (float)Math.Sqrt(horizontal * horizontal + vertical * vertical);
            if (magnitude > 1) { horizontal /= magnitude; vertical /= magnitude; }
            Player.X = Clamp(Player.X + horizontal * PlayerSpeed * dt, -7.6f, 7.6f);
            Player.Y = Clamp(Player.Y + vertical * PlayerSpeed * dt, -8f, 6.5f);

            fireTimer -= dt;
            if (shoot && fireTimer <= 0)
            {
                Bullets.Add(new Body(nextId++, Player.X, Player.Y + 0.65f, 0.16f, 0.45f));
                fireTimer = FireInterval;
            }
            spawnTimer -= dt;
            if (spawnTimer <= 0)
            {
                Enemies.Add(new Body(nextId++, (float)(random.NextDouble() * 14 - 7), 8.5f, 0.9f, 0.7f));
                spawnTimer += SpawnInterval;
            }

            // Swept AABBs use relative motion, avoiding tunnelling between updates.
            for (int b = Bullets.Count - 1; b >= 0; b--)
            {
                Body bullet = Bullets[b];
                int hit = -1;
                float earliest = float.PositiveInfinity;
                for (int e = 0; e < Enemies.Count; e++)
                {
                    float time;
                    if (Sweep(bullet.X - Enemies[e].X, bullet.Y - Enemies[e].Y,
                        0, (BulletSpeed + EnemySpeed) * dt,
                        (bullet.Width + Enemies[e].Width) / 2,
                        (bullet.Height + Enemies[e].Height) / 2, out time) && time < earliest)
                    { hit = e; earliest = time; }
                }
                if (hit >= 0)
                {
                    Enemies.RemoveAt(hit); Bullets.RemoveAt(b); Score += 10;
                }
                else
                {
                    bullet.Y += BulletSpeed * dt;
                    if (bullet.Y > Top + 1) Bullets.RemoveAt(b);
                }
            }

            foreach (Body enemy in Enemies)
            {
                float time;
                bool hitPlayer = Sweep(enemy.X - previousX, enemy.Y - previousY,
                    previousX - Player.X, -EnemySpeed * dt + previousY - Player.Y,
                    (enemy.Width + Player.Width) / 2, (enemy.Height + Player.Height) / 2, out time);
                enemy.Y -= EnemySpeed * dt;
                if (hitPlayer) { End("Your ship was hit."); return; }
                if (enemy.Y + enemy.Height / 2 < Bottom)
                { End("An enemy escaped past the bottom."); return; }
            }
        }

        private void End(string reason) { GameOver = true; EndReason = reason; }
        private static float Clamp(float value, float min, float max)
        { return Math.Max(min, Math.Min(max, value)); }

        private static bool Sweep(float x, float y, float dx, float dy,
            float halfWidth, float halfHeight, out float entry)
        {
            entry = 0;
            float exit = 1;
            return Axis(x, dx, halfWidth, ref entry, ref exit)
                && Axis(y, dy, halfHeight, ref entry, ref exit);
        }

        private static bool Axis(float origin, float delta, float extent, ref float entry, ref float exit)
        {
            if (Math.Abs(delta) < 0.000001f) return Math.Abs(origin) <= extent;
            float first = (-extent - origin) / delta;
            float last = (extent - origin) / delta;
            if (first > last) { float swap = first; first = last; last = swap; }
            entry = Math.Max(entry, first); exit = Math.Min(exit, last);
            return entry <= exit;
        }
    }
}
