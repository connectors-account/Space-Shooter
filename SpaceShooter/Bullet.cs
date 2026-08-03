using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceShooter
{
    /// <summary>
    /// A projectile fired by either the player or an enemy.
    /// SpeedY is negative (up) for player bullets, positive (down) for enemy bullets.
    /// </summary>
    public class Bullet
    {
        public RectangleF Bounds;
        public float      SpeedY;
        public bool       IsPlayerBullet;
        public bool       Active;

        private readonly Color _core;
        private readonly Color _glow;

        public Bullet(float centerX, float y, float speedY, bool isPlayerBullet)
        {
            Bounds         = new RectangleF(centerX - 3, y, 6, 18);
            SpeedY         = speedY;
            IsPlayerBullet = isPlayerBullet;
            Active         = true;
            _core          = isPlayerBullet ? Color.FromArgb(0, 220, 255)   : Color.FromArgb(255, 90, 40);
            _glow          = isPlayerBullet ? Color.FromArgb(40, 0, 100, 140) : Color.FromArgb(40, 140, 40, 0);
        }

        public void Update(float dt)
        {
            Bounds.Y += SpeedY * dt;
        }

        public void Draw(Graphics g)
        {
            // outer glow
            using var glowBrush = new SolidBrush(_glow);
            g.FillRectangle(glowBrush,
                Bounds.X - 3, Bounds.Y - 2,
                Bounds.Width + 6, Bounds.Height + 4);

            // core beam
            using var coreBrush = new LinearGradientBrush(
                new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height),
                Color.White, _core, LinearGradientMode.Vertical);
            g.FillRectangle(coreBrush, Bounds);
        }
    }
}
