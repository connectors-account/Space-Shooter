using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceShooter
{
    public enum GameState { Menu, Playing, GameOver }
    public enum PowerUpType { RapidFire, TripleShot, Shield, SpeedBoost }
    public enum EnemyType { Basic, Fast, Tank }

    /// <summary>
    /// A single background star that scrolls downward to give a sense of motion.
    /// </summary>
    public class Star
    {
        public float X;
        public float Y;
        public float Speed;
        public float Size;
        public byte Alpha;

        private readonly int _fieldWidth;
        private readonly int _fieldHeight;
        private static readonly Random _rng = new Random();

        public Star(int fieldWidth, int fieldHeight)
        {
            _fieldWidth = fieldWidth;
            _fieldHeight = fieldHeight;
            X = (float)(_rng.NextDouble() * fieldWidth);
            Y = (float)(_rng.NextDouble() * fieldHeight);
            Speed = 0.5f + (float)(_rng.NextDouble() * 2.5); // 0.5 - 3
            Size = 1f + (float)(_rng.NextDouble() * 2f);      // 1 - 3
            Alpha = (byte)(100 + _rng.Next(0, 156));          // 100 - 255
        }

        public void Update()
        {
            Y += Speed;
            if (Y > _fieldHeight + Size)
            {
                Y = -Size;
                X = (float)(_rng.NextDouble() * _fieldWidth);
            }
        }

        public void Draw(Graphics g)
        {
            using (var b = new SolidBrush(Color.FromArgb(Alpha, 255, 255, 255)))
            {
                g.FillEllipse(b, X, Y, Size, Size);
            }
        }
    }

    /// <summary>
    /// A projectile fired by the player or an enemy.
    /// </summary>
    public class Bullet
    {
        public float X;
        public float Y;
        public float SpeedX;
        public float SpeedY;
        public int Damage;
        public bool IsPlayerBullet;
        public bool Active = true;
        public Color Color;

        private const int Width = 6;
        private const int Height = 15;

        public Bullet(float x, float y, float speedX, float speedY, int damage, bool isPlayerBullet, Color color)
        {
            X = x;
            Y = y;
            SpeedX = speedX;
            SpeedY = speedY;
            Damage = damage;
            IsPlayerBullet = isPlayerBullet;
            Color = color;
        }

        public Rectangle BoundingBox =>
            new Rectangle((int)(X - Width / 2f), (int)(Y - Height / 2f), Width, Height);

        public void Update()
        {
            X += SpeedX;
            Y += SpeedY;
        }

        public bool IsOffScreen(int height)
        {
            return Y < -20 || Y > height + 20;
        }

        public void Draw(Graphics g)
        {
            // Glow: a larger, semi-transparent rectangle drawn behind the core.
            using (var glow = new SolidBrush(Color.FromArgb(90, Color)))
            {
                g.FillRectangle(glow, X - Width, Y - Height / 2f - 3, Width * 2, Height + 6);
            }
            using (var core = new SolidBrush(Color))
            {
                g.FillRectangle(core, X - Width / 2f, Y - Height / 2f, Width, Height);
            }
        }
    }

    /// <summary>
    /// A collectible power-up that drifts downward.
    /// </summary>
    public class PowerUp
    {
        public float X;
        public float Y;
        public PowerUpType Type;
        public bool Active = true;
        public float Speed = 1.5f;

        private const int Size = 26;

        public PowerUp(float x, float y, PowerUpType type)
        {
            X = x;
            Y = y;
            Type = type;
        }

        public Rectangle BoundingBox =>
            new Rectangle((int)(X - Size / 2f), (int)(Y - Size / 2f), Size, Size);

        public void Update()
        {
            Y += Speed;
        }

        public Color GetColor()
        {
            switch (Type)
            {
                case PowerUpType.RapidFire: return Color.Yellow;
                case PowerUpType.TripleShot: return Color.Cyan;
                case PowerUpType.Shield: return Color.DodgerBlue;
                case PowerUpType.SpeedBoost: return Color.LimeGreen;
                default: return Color.White;
            }
        }

        public string GetLabel()
        {
            switch (Type)
            {
                case PowerUpType.RapidFire: return "R";
                case PowerUpType.TripleShot: return "T";
                case PowerUpType.Shield: return "S";
                case PowerUpType.SpeedBoost: return "B";
                default: return "?";
            }
        }

        public void Draw(Graphics g)
        {
            Color c = GetColor();
            float half = Size / 2f;
            PointF[] diamond =
            {
                new PointF(X, Y - half),
                new PointF(X + half, Y),
                new PointF(X, Y + half),
                new PointF(X - half, Y)
            };

            // Glow
            PointF[] glowDiamond =
            {
                new PointF(X, Y - half - 5),
                new PointF(X + half + 5, Y),
                new PointF(X, Y + half + 5),
                new PointF(X - half - 5, Y)
            };
            using (var glow = new SolidBrush(Color.FromArgb(80, c)))
            {
                g.FillPolygon(glow, glowDiamond);
            }
            using (var fill = new SolidBrush(Color.FromArgb(200, c)))
            {
                g.FillPolygon(fill, diamond);
            }
            using (var pen = new Pen(Color.White, 2f))
            {
                g.DrawPolygon(pen, diamond);
            }

            // Label letter
            using (var font = new Font("Consolas", 11f, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.Black))
            {
                string label = GetLabel();
                SizeF sz = g.MeasureString(label, font);
                g.DrawString(label, font, textBrush, X - sz.Width / 2f, Y - sz.Height / 2f);
            }
        }
    }

    /// <summary>
    /// An enemy ship. Behaviour and stats vary by <see cref="EnemyType"/>.
    /// </summary>
    public class Enemy
    {
        public float X;
        public float Y;
        public int Health;
        public int MaxHealth;
        public EnemyType Type;
        public bool Active = true;
        public float Speed;
        public float ShootTimer;
        public float ShootInterval;
        public int ScoreValue;
        public Color Color;
        public float HorizontalSpeed;
        public float HorizontalTimer;

        private readonly int _width;
        private readonly int _height;
        private static readonly Random _rng = new Random();

        public Enemy(EnemyType type, float x, float y)
        {
            Type = type;
            X = x;
            Y = y;

            switch (type)
            {
                case EnemyType.Basic:
                    MaxHealth = 1;
                    Speed = 1.5f;
                    ShootInterval = 120f;
                    ScoreValue = 100;
                    Color = Color.OrangeRed;
                    _width = 34;
                    _height = 30;
                    HorizontalSpeed = 0f;
                    break;
                case EnemyType.Fast:
                    MaxHealth = 1;
                    Speed = 3f;
                    ShootInterval = 180f;
                    ScoreValue = 150;
                    Color = Color.Yellow;
                    _width = 30;
                    _height = 26;
                    HorizontalSpeed = 1.6f;
                    break;
                case EnemyType.Tank:
                default:
                    MaxHealth = 4;
                    Speed = 0.8f;
                    ShootInterval = 80f;
                    ScoreValue = 300;
                    Color = Color.MediumPurple;
                    _width = 52;
                    _height = 44;
                    HorizontalSpeed = 0f;
                    break;
            }

            Health = MaxHealth;
            // Stagger initial shoot timers so enemies don't all fire on the same frame.
            ShootTimer = ShootInterval * (0.5f + (float)_rng.NextDouble() * 0.5f);
            HorizontalTimer = (float)(_rng.NextDouble() * Math.PI * 2);
        }

        public Rectangle BoundingBox =>
            new Rectangle((int)(X - _width / 2f), (int)(Y - _height / 2f), _width, _height);

        public void Update(float deltaTime)
        {
            Y += Speed * deltaTime;

            if (Type == EnemyType.Fast)
            {
                HorizontalTimer += 0.05f * deltaTime;
                X += (float)Math.Sin(HorizontalTimer) * HorizontalSpeed * deltaTime;
            }

            ShootTimer -= deltaTime;
        }

        public bool ShouldShoot()
        {
            if (ShootTimer <= 0)
            {
                ShootTimer = ShootInterval;
                return true;
            }
            return false;
        }

        public bool TakeDamage(int dmg)
        {
            Health -= dmg;
            if (Health <= 0)
            {
                Active = false;
                return true;
            }
            return false;
        }

        public void Draw(Graphics g)
        {
            float w = _width;
            float h = _height;
            // Downward pointing ship polygon.
            PointF[] ship =
            {
                new PointF(X, Y + h / 2f),           // nose (bottom)
                new PointF(X - w / 2f, Y - h / 2f),  // top-left
                new PointF(X - w / 6f, Y - h / 4f),  // inner-left
                new PointF(X + w / 6f, Y - h / 4f),  // inner-right
                new PointF(X + w / 2f, Y - h / 2f)   // top-right
            };

            using (var body = new SolidBrush(Color))
            {
                g.FillPolygon(body, ship);
            }
            using (var outline = new Pen(Color.FromArgb(220, 255, 255, 255), 1.5f))
            {
                g.DrawPolygon(outline, ship);
            }

            // Cockpit
            using (var cockpit = new SolidBrush(Color.FromArgb(200, 20, 20, 40)))
            {
                g.FillEllipse(cockpit, X - w / 8f, Y - h / 6f, w / 4f, h / 4f);
            }

            // Health bar above the ship for Tank enemies (or any damaged multi-HP enemy).
            if (Type == EnemyType.Tank || MaxHealth > 1)
            {
                float barW = w;
                float barH = 4f;
                float bx = X - barW / 2f;
                float by = Y - h / 2f - 9f;
                using (var back = new SolidBrush(Color.FromArgb(180, 40, 40, 40)))
                {
                    g.FillRectangle(back, bx, by, barW, barH);
                }
                float ratio = Math.Max(0f, (float)Health / MaxHealth);
                using (var front = new SolidBrush(ratio > 0.5f ? Color.LimeGreen : Color.Orange))
                {
                    g.FillRectangle(front, bx, by, barW * ratio, barH);
                }
            }
        }
    }

    /// <summary>
    /// The player-controlled ship.
    /// </summary>
    public class Player
    {
        public float X;
        public float Y;
        public int Health;
        public int MaxHealth = 5;
        public bool Active = true;

        public float ShootTimer;
        public float ShootCooldown = 18f;

        public bool RapidFire;
        public bool TripleShot;
        public bool HasShield;
        public float SpeedBoost;

        public float RapidFireTimer;
        public float TripleShotTimer;
        public float ShieldTimer;
        public float SpeedBoostTimer;
        public float InvincibleTimer;

        public float Speed = 4.5f;
        public Color ShipColor = Color.DeepSkyBlue;

        private const int ShipWidth = 36;
        private const int ShipHeight = 40;

        public Player(int formWidth, int formHeight)
        {
            X = formWidth / 2f;
            Y = formHeight - 90f;
            Health = MaxHealth;
        }

        public Rectangle BoundingBox =>
            new Rectangle((int)(X - ShipWidth / 2f), (int)(Y - ShipHeight / 2f), ShipWidth, ShipHeight);

        public float CurrentSpeed => Speed + SpeedBoost;

        public void Update()
        {
            if (RapidFireTimer > 0)
            {
                RapidFireTimer--;
                if (RapidFireTimer <= 0) RapidFire = false;
            }
            if (TripleShotTimer > 0)
            {
                TripleShotTimer--;
                if (TripleShotTimer <= 0) TripleShot = false;
            }
            if (ShieldTimer > 0)
            {
                ShieldTimer--;
                if (ShieldTimer <= 0) HasShield = false;
            }
            if (SpeedBoostTimer > 0)
            {
                SpeedBoostTimer--;
                if (SpeedBoostTimer <= 0) SpeedBoost = 0f;
            }
            if (InvincibleTimer > 0)
            {
                InvincibleTimer--;
            }
            if (ShootTimer > 0)
            {
                ShootTimer--;
            }
        }

        public bool CanShoot()
        {
            return ShootTimer <= 0;
        }

        public void ResetShootTimer()
        {
            ShootTimer = RapidFire ? 6f : 18f;
        }

        public bool TakeDamage(int dmg)
        {
            if (HasShield) return false;
            if (InvincibleTimer > 0) return false;
            Health -= dmg;
            InvincibleTimer = 90f;
            if (Health <= 0)
            {
                Health = 0;
                Active = false;
                return true;
            }
            return false;
        }

        public void ApplyPowerUp(PowerUpType type)
        {
            const float duration = 300f;
            switch (type)
            {
                case PowerUpType.RapidFire:
                    RapidFire = true;
                    RapidFireTimer = duration;
                    break;
                case PowerUpType.TripleShot:
                    TripleShot = true;
                    TripleShotTimer = duration;
                    break;
                case PowerUpType.Shield:
                    HasShield = true;
                    ShieldTimer = duration;
                    break;
                case PowerUpType.SpeedBoost:
                    SpeedBoost = 3f;
                    SpeedBoostTimer = duration;
                    break;
            }
        }

        public void Draw(Graphics g, int frame)
        {
            // Flash while invincible: skip drawing on odd frames.
            if (InvincibleTimer > 0 && (frame % 2 == 1))
            {
                return;
            }

            float w = ShipWidth;
            float h = ShipHeight;

            // Engine glow (small yellow rectangle at the bottom).
            using (var glow = new SolidBrush(Color.FromArgb(200, 255, 200, 40)))
            {
                g.FillRectangle(glow, X - 4f, Y + h / 2f - 4f, 8f, 10f);
            }
            using (var glow2 = new SolidBrush(Color.FromArgb(120, 255, 120, 20)))
            {
                g.FillRectangle(glow2, X - 6f, Y + h / 2f - 2f, 12f, 8f);
            }

            // Upward pointing player ship polygon.
            PointF[] ship =
            {
                new PointF(X, Y - h / 2f),            // nose (top)
                new PointF(X - w / 2f, Y + h / 2f),   // bottom-left
                new PointF(X - w / 6f, Y + h / 4f),   // inner-left
                new PointF(X + w / 6f, Y + h / 4f),   // inner-right
                new PointF(X + w / 2f, Y + h / 2f)    // bottom-right
            };

            using (var body = new SolidBrush(ShipColor))
            {
                g.FillPolygon(body, ship);
            }
            using (var accent = new SolidBrush(Color.FromArgb(255, 200, 240, 255)))
            {
                PointF[] fin =
                {
                    new PointF(X, Y - h / 2f),
                    new PointF(X - w / 10f, Y),
                    new PointF(X + w / 10f, Y)
                };
                g.FillPolygon(accent, fin);
            }
            using (var outline = new Pen(Color.White, 1.5f))
            {
                g.DrawPolygon(outline, ship);
            }
            using (var cockpit = new SolidBrush(Color.FromArgb(230, 10, 30, 60)))
            {
                g.FillEllipse(cockpit, X - w / 10f, Y - h / 8f, w / 5f, h / 5f);
            }

            // Shield bubble.
            if (HasShield)
            {
                float r = w * 1.1f;
                using (var shieldPen = new Pen(Color.FromArgb(180, 80, 180, 255), 2f))
                using (var shieldFill = new SolidBrush(Color.FromArgb(50, 80, 180, 255)))
                {
                    g.FillEllipse(shieldFill, X - r, Y - r, r * 2, r * 2);
                    g.DrawEllipse(shieldPen, X - r, Y - r, r * 2, r * 2);
                }
            }
        }

        public List<Bullet> GetBullets()
        {
            var bullets = new List<Bullet>();
            Color c = Color.FromArgb(255, 120, 255, 120);
            float top = Y - ShipHeight / 2f - 4f;
            float bulletSpeed = -11f;

            if (TripleShot)
            {
                bullets.Add(new Bullet(X, top, 0f, bulletSpeed, 1, true, c));
                bullets.Add(new Bullet(X - 12f, top + 6f, -2.5f, bulletSpeed, 1, true, c));
                bullets.Add(new Bullet(X + 12f, top + 6f, 2.5f, bulletSpeed, 1, true, c));
            }
            else
            {
                bullets.Add(new Bullet(X, top, 0f, bulletSpeed, 1, true, c));
            }
            return bullets;
        }
    }
}
