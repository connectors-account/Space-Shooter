using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceShooter
{
    /// <summary>Player ship: movement, shooting, invincibility frames, health.</summary>
    public class Player
    {
        public  RectangleF Bounds;
        public  int        Health;
        public  int        MaxHealth = 100;
        public  float      Speed     = 300f;

        private float _shootCooldown;
        private const float ShootRate      = 0.18f;   // seconds between shots
        private float _invincibleTimer;
        private const float InvincibleTime = 1.8f;    // seconds of flicker after hit

        // Engine flame animation
        private float _flameTimer;

        public Player(float centerX, float y)
        {
            Bounds = new RectangleF(centerX - 20, y - 25, 40, 50);
            Health = MaxHealth;
        }

        public void Update(bool left, bool right, bool up, bool down, bool shoot,
                           float dt, List<Bullet> bullets, int screenW, int screenH)
        {
            // --- Movement ---
            float dx = 0, dy = 0;
            if (left)  dx -= Speed * dt;
            if (right) dx += Speed * dt;
            if (up)    dy -= Speed * dt;
            if (down)  dy += Speed * dt;

            Bounds.X = MathF.Max(0,           MathF.Min(Bounds.X + dx, screenW - Bounds.Width));
            Bounds.Y = MathF.Max(0,           MathF.Min(Bounds.Y + dy, screenH - Bounds.Height));

            // --- Shooting ---
            _shootCooldown -= dt;
            if (shoot && _shootCooldown <= 0)
            {
                float cx = Bounds.X + Bounds.Width / 2f;
                bullets.Add(new Bullet(cx, Bounds.Y - 2, -620f, true));
                _shootCooldown = ShootRate;
            }

            // --- Timers ---
            if (_invincibleTimer > 0) _invincibleTimer -= dt;
            _flameTimer += dt;
        }

        public void TakeDamage(int damage)
        {
            if (_invincibleTimer > 0) return;
            Health            -= damage;
            _invincibleTimer   = InvincibleTime;
        }

        public bool IsInvincible => _invincibleTimer > 0;

        public void Draw(Graphics g)
        {
            // Flicker when invincible
            if (IsInvincible && (int)(_invincibleTimer * 8) % 2 == 0) return;

            float x  = Bounds.X;
            float y  = Bounds.Y;
            float w  = Bounds.Width;
            float h  = Bounds.Height;
            float cx = x + w / 2f;

            // --- Engine flame ---
            float flameH = 14 + 6 * MathF.Abs(MathF.Sin(_flameTimer * 12f));
            using var flameBrush = new LinearGradientBrush(
                new RectangleF(cx - 8, y + h, 16, flameH),
                Color.FromArgb(255, 200, 0), Color.Transparent, LinearGradientMode.Vertical);
            g.FillEllipse(flameBrush, cx - 8, y + h - 2, 16, flameH);

            // --- Wings ---
            var leftWing = new PointF[]
            {
                new(x,           y + h * 0.55f),
                new(x + w * 0.2f, y + h * 0.35f),
                new(x + w * 0.2f, y + h),
            };
            var rightWing = new PointF[]
            {
                new(x + w,           y + h * 0.55f),
                new(x + w * 0.8f, y + h * 0.35f),
                new(x + w * 0.8f, y + h),
            };
            using var wingBrush = new SolidBrush(Color.FromArgb(60, 130, 220));
            g.FillPolygon(wingBrush, leftWing);
            g.FillPolygon(wingBrush, rightWing);

            // --- Hull ---
            var hull = new PointF[]
            {
                new(cx,             y),
                new(x + w * 0.85f,  y + h * 0.65f),
                new(x + w * 0.65f,  y + h),
                new(x + w * 0.35f,  y + h),
                new(x + w * 0.15f,  y + h * 0.65f),
            };
            using var hullBrush = new LinearGradientBrush(
                Bounds, Color.FromArgb(90, 170, 255), Color.FromArgb(30, 80, 160),
                LinearGradientMode.Vertical);
            g.FillPolygon(hullBrush, hull);

            // --- Cockpit glass ---
            using var glassBrush = new SolidBrush(Color.FromArgb(200, 230, 255));
            g.FillEllipse(glassBrush, cx - w * 0.18f, y + h * 0.12f, w * 0.36f, h * 0.28f);

            // --- Hull outline ---
            using var pen = new Pen(Color.FromArgb(140, 200, 255), 1.2f);
            g.DrawPolygon(pen, hull);
        }
    }
}
