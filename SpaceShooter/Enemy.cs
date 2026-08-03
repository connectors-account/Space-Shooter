using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceShooter
{
    public enum EnemyType { Basic, Zigzag, Tank }

    /// <summary>
    /// Three enemy variants:
    ///   Basic  – straight-down movement, single shot.
    ///   Zigzag – sine-wave horizontal drift, faster single shot.
    ///   Tank   – slow, high HP, three-bullet spread shot.
    /// All stats scale with the current wave number.
    /// </summary>
    public class Enemy
    {
        public  RectangleF Bounds;
        public  int        Health;
        public  int        MaxHealth;
        public  int        ScoreValue;
        public  bool       Active = true;
        public  EnemyType  Type;

        private float  _speedY;
        private float  _speedX;
        private double _time;
        private float  _shootTimer;
        private float  _shootInterval;
        private Color  _bodyColor;
        private Color  _accentColor;
        private float  _flashTimer;   // brief white flash when hit

        public Enemy(float x, float y, EnemyType type, int wave)
        {
            Type = type;
            float waveScale = 1f + (wave - 1) * 0.18f;

            switch (type)
            {
                // ----- Basic -----
                case EnemyType.Basic:
                    Bounds         = new RectangleF(x - 20, y, 40, 34);
                    MaxHealth      = (int)(25 * waveScale);
                    _speedY        = 75f + wave * 10f;
                    _speedX        = 0f;
                    _shootInterval = Math.Max(0.9f, 2.2f - wave * 0.12f);
                    ScoreValue     = 100;
                    _bodyColor     = Color.FromArgb(220, 70, 70);
                    _accentColor   = Color.FromArgb(255, 140, 140);
                    break;

                // ----- Zigzag -----
                case EnemyType.Zigzag:
                    Bounds         = new RectangleF(x - 18, y, 36, 28);
                    MaxHealth      = (int)(18 * waveScale);
                    _speedY        = 65f + wave * 8f;
                    _speedX        = 110f;
                    _shootInterval = Math.Max(0.7f, 1.6f - wave * 0.1f);
                    ScoreValue     = 150;
                    _bodyColor     = Color.FromArgb(210, 165, 30);
                    _accentColor   = Color.FromArgb(255, 220, 80);
                    break;

                // ----- Tank -----
                default:  // EnemyType.Tank
                    Bounds         = new RectangleF(x - 30, y, 60, 48);
                    MaxHealth      = (int)(75 * waveScale);
                    _speedY        = 38f + wave * 4f;
                    _speedX        = 0f;
                    _shootInterval = Math.Max(0.5f, 1.0f - wave * 0.05f);
                    ScoreValue     = 350;
                    _bodyColor     = Color.FromArgb(130, 60, 210);
                    _accentColor   = Color.FromArgb(200, 130, 255);
                    break;
            }

            Health        = MaxHealth;
            _shootTimer   = _shootInterval * 0.6f;   // first shot is a bit early
        }

        public void Update(float dt, List<Bullet> bullets, int screenW)
        {
            _time += dt;

            // Horizontal movement
            float dx = Type == EnemyType.Zigzag
                ? (float)Math.Sin(_time * 2.8) * _speedX * dt
                : 0f;

            Bounds.X = Math.Clamp(Bounds.X + dx, 0, screenW - Bounds.Width);
            Bounds.Y += _speedY * dt;

            // Flash decay
            if (_flashTimer > 0) _flashTimer -= dt;

            // Shooting
            _shootTimer -= dt;
            if (_shootTimer <= 0)
            {
                Shoot(bullets);
                _shootTimer = _shootInterval;
            }
        }

        private void Shoot(List<Bullet> bullets)
        {
            float cx = Bounds.X + Bounds.Width  / 2f;
            float cy = Bounds.Y + Bounds.Height;

            switch (Type)
            {
                case EnemyType.Basic:
                case EnemyType.Zigzag:
                    bullets.Add(new Bullet(cx, cy, 290f, false));
                    break;

                case EnemyType.Tank:
                    bullets.Add(new Bullet(cx,       cy, 270f, false));
                    bullets.Add(new Bullet(cx - 12f, cy, 270f, false));
                    bullets.Add(new Bullet(cx + 12f, cy, 270f, false));
                    break;
            }
        }

        public void TakeDamage(int damage)
        {
            Health      -= damage;
            _flashTimer  = 0.08f;
            if (Health <= 0) Active = false;
        }

        public void Draw(Graphics g)
        {
            float x  = Bounds.X;
            float y  = Bounds.Y;
            float w  = Bounds.Width;
            float h  = Bounds.Height;
            float cx = x + w / 2f;

            Color body   = _flashTimer > 0 ? Color.White : _bodyColor;
            Color accent = _flashTimer > 0 ? Color.White : _accentColor;

            // --- Hull polygon (inverted player shape) ---
            var hull = new PointF[]
            {
                new(cx,            y + h),
                new(x + w * 0.85f, y + h * 0.40f),
                new(x + w * 0.60f, y),
                new(x + w * 0.40f, y),
                new(x + w * 0.15f, y + h * 0.40f),
            };
            using var hullBrush = new LinearGradientBrush(
                Bounds, accent, body, LinearGradientMode.Vertical);
            g.FillPolygon(hullBrush, hull);

            // --- Engine pods (tank only) ---
            if (Type == EnemyType.Tank)
            {
                using var podBrush = new SolidBrush(body);
                g.FillEllipse(podBrush, x + 2,     y + h * 0.3f, 12, 20);
                g.FillEllipse(podBrush, x + w - 14, y + h * 0.3f, 12, 20);
            }

            // --- Cockpit lens ---
            using var cockpitBrush = new SolidBrush(Color.FromArgb(200, 30, 30, 30));
            g.FillEllipse(cockpitBrush, cx - w * 0.15f, y + h * 0.35f, w * 0.30f, h * 0.22f);
            using var shineB = new SolidBrush(Color.FromArgb(120, accent));
            g.FillEllipse(shineB, cx - w * 0.10f, y + h * 0.37f, w * 0.12f, h * 0.08f);

            // --- Hull outline ---
            using var pen = new Pen(Color.FromArgb(180, accent), 1f);
            g.DrawPolygon(pen, hull);

            // --- Health bar ---
            float hpFrac = Math.Max(0f, Health / (float)MaxHealth);
            g.FillRectangle(Brushes.DarkRed, x, y - 7, w, 4);
            using var hpBrush = new SolidBrush(hpFrac > 0.5f ? Color.LimeGreen : Color.OrangeRed);
            g.FillRectangle(hpBrush, x, y - 7, w * hpFrac, 4);
        }
    }
}
