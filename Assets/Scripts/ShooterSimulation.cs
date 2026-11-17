using System;
using System.Collections.Generic;

namespace SpaceShooter
{
    // Engine-independent simulation. All dimensions are world-space half-extents.
    public sealed class ShooterSimulation
    {
        public const float HalfWidth = 8f, HalfHeight = 6f;
        public const float PlayerHalfX = .45f, PlayerHalfY = .5f;
        public const float EnemyHalfX = .5f, EnemyHalfY = .4f;
        public const float BulletHalfX = .09f, BulletHalfY = .24f;
        public const float PlayerSpeed = 5f, EnemySpeed = 2f, BulletSpeed = 14f;
        public sealed class Body
        {
            public int Id;
            public float X, Y, PreviousY;
        }
        public readonly List<Body> Enemies = new List<Body>();
        public readonly List<Body> Bullets = new List<Body>();
        public float PlayerX { get; private set; }
        public float PlayerY { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }
        public string EndReason { get; private set; }
        private Random random;
        private int nextId;
        private float spawnTimer, shotTimer;

        public ShooterSimulation(int seed) { Reset(seed); }

        public void Reset(int seed)
        {
            random = new Random(seed);
            Enemies.Clear();
            Bullets.Clear();
            PlayerX = 0f;
            PlayerY = -4.5f;
            Score = 0;
            GameOver = false;
            EndReason = "";
            nextId = 0;
            spawnTimer = 1f;
            shotTimer = 0f;
        }

        public void Step(float dt, float inputX, float inputY, bool firing)
        {
            if (GameOver || dt <= 0f || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            dt = Math.Min(dt, .05f);
            float oldX = PlayerX, oldY = PlayerY;
            float magnitude = (float)Math.Sqrt(inputX * inputX + inputY * inputY);
            if (magnitude > 1f) { inputX /= magnitude; inputY /= magnitude; }
            PlayerX = Clamp(PlayerX + inputX * PlayerSpeed * dt, -7.55f, 7.55f);
            PlayerY = Clamp(PlayerY + inputY * PlayerSpeed * dt, -5.5f, 4.5f);
            shotTimer = Math.Max(0f, shotTimer - dt);
            if (firing && shotTimer <= 0f)
            {
                Bullets.Add(NewBody(PlayerX, PlayerY + .78f));
                shotTimer = .18f;
            }
            spawnTimer -= dt;
            if (spawnTimer <= 0f)
            {
                Enemies.Add(NewBody((float)(random.NextDouble() * 14.8 - 7.4), 6.5f));
                spawnTimer += .85f;
            }
            foreach (Body enemy in Enemies)
            {
                enemy.PreviousY = enemy.Y;
                enemy.Y -= EnemySpeed * dt;
            }
            foreach (Body bullet in Bullets)
            {
                bullet.PreviousY = bullet.Y;
                bullet.Y += BulletSpeed * dt;
            }
            // Relative swept AABBs detect hits even when a projectile crosses an
            // entire enemy between steps. Each bullet damages only its nearest hit.
            for (int b = Bullets.Count - 1; b >= 0; b--)
            {
                Body bullet = Bullets[b];
                int hit = -1;
                float nearest = float.PositiveInfinity;
                for (int e = 0; e < Enemies.Count; e++)
                {
                    Body enemy = Enemies[e];
                    float time;
                    if (Sweep(bullet.X - enemy.X, bullet.PreviousY - enemy.PreviousY,
                        bullet.X - enemy.X, bullet.Y - enemy.Y,
                        BulletHalfX + EnemyHalfX, BulletHalfY + EnemyHalfY, out time)
                        && time < nearest)
                    { nearest = time; hit = e; }
                }
                if (hit >= 0)
                {
                    Enemies.RemoveAt(hit);
                    Bullets.RemoveAt(b);
                    Score += 10;
                }
                else if (bullet.Y > 7f) Bullets.RemoveAt(b);
            }
            // Shots resolve before enemy contact in the same step: a last-second
            // defensive shot is deliberately allowed to save the player.
            foreach (Body enemy in Enemies)
            {
                float time;
                if (Sweep(oldX - enemy.X, oldY - enemy.PreviousY,
                    PlayerX - enemy.X, PlayerY - enemy.Y,
                    PlayerHalfX + EnemyHalfX, PlayerHalfY + EnemyHalfY, out time))
                { End("Your ship was hit."); break; }
                if (enemy.Y - EnemyHalfY <= -HalfHeight)
                { End("An enemy reached the bottom."); break; }
            }
        }

        private Body NewBody(float x, float y)
        { return new Body { Id = ++nextId, X = x, Y = y, PreviousY = y }; }
        private void End(string reason) { GameOver = true; EndReason = reason; }
        private static float Clamp(float v, float min, float max)
        { return Math.Max(min, Math.Min(max, v)); }

        public static bool Sweep(float x0, float y0, float x1, float y1,
            float halfX, float halfY, out float time)
        {
            float enter = 0f, exit = 1f;
            bool hit = Slab(x0, x1 - x0, halfX, ref enter, ref exit)
                && Slab(y0, y1 - y0, halfY, ref enter, ref exit);
            time = enter;
            return hit;
        }
        private static bool Slab(float start, float delta, float half,
            ref float enter, ref float exit)
        {
            if (Math.Abs(delta) < .000001f) return Math.Abs(start) <= half;
            float a = (-half - start) / delta, b = (half - start) / delta;
            if (a > b) { float swap = a; a = b; b = swap; }
            enter = Math.Max(enter, a);
            exit = Math.Min(exit, b);
            return enter <= exit;
        }
    }
}
