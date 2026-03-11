using Raylib_cs;
using System.Numerics;

namespace SpaceShooter;

public class Bullet : ICollidable
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Radius => IsPlayerBullet ? 4 : 5;
    public int Damage { get; }
    public Color BulletColor { get; }
    public bool IsPlayerBullet { get; }
    public bool IsActive { get; set; } = true;
    
    private float trailTimer;
    
    public Bullet(Vector2 position, Vector2 velocity, int damage, Color color, bool isPlayerBullet)
    {
        Position = position;
        Velocity = velocity;
        Damage = damage;
        BulletColor = color;
        IsPlayerBullet = isPlayerBullet;
    }
    
    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
        trailTimer += deltaTime;
    }
    
    public void Draw()
    {
        if (IsPlayerBullet)
        {
            // Player bullet - elongated shape
            Raylib.DrawRectangle((int)(Position.X - 2), (int)(Position.Y - 8), 4, 16, BulletColor);
            Raylib.DrawCircle((int)Position.X, (int)(Position.Y - 8), 2, BulletColor);
            
            // Trail
            Raylib.DrawRectangle((int)(Position.X - 1), (int)(Position.Y + 8), 2, 8, new Color(255, 255, 0, 100));
        }
        else
        {
            // Enemy bullet - circular with glow
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, Radius + 2, new Color((int)BulletColor.R, (int)BulletColor.G, (int)BulletColor.B, 100));
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, Radius, BulletColor);
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, Radius - 2, Color.White);
        }
    }
}
