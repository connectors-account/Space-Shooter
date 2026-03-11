using Raylib_cs;
using System.Numerics;

namespace SpaceShooter;

public enum EnemyType
{
    Basic,
    Fast,
    Tank,
    Shooter,
    Zigzag
}

public class Enemy : ICollidable
{
    public Vector2 Position { get; set; }
    public float Radius { get; }
    public bool IsActive { get; set; } = true;
    public int Health { get; private set; }
    public int ScoreValue { get; }
    public Color EnemyColor { get; }
    public EnemyType Type { get; }
    
    private readonly Game game;
    private Vector2 velocity;
    private float shootTimer;
    private float shootDelay;
    private float zigzagTimer;
    private float zigzagDirection = 1;
    private float animationTimer;
    
    public Enemy(Vector2 position, EnemyType type, Game game)
    {
        Position = position;
        Type = type;
        this.game = game;
        
        switch (type)
        {
            case EnemyType.Basic:
                Radius = 15;
                Health = 2;
                ScoreValue = 100;
                EnemyColor = Color.Red;
                velocity = new Vector2(0, 100);
                shootDelay = 2f;
                break;
                
            case EnemyType.Fast:
                Radius = 10;
                Health = 1;
                ScoreValue = 150;
                EnemyColor = Color.Orange;
                velocity = new Vector2(Random.Shared.NextSingle() * 100 - 50, 180);
                shootDelay = 3f;
                break;
                
            case EnemyType.Tank:
                Radius = 25;
                Health = 6;
                ScoreValue = 300;
                EnemyColor = Color.DarkGray;
                velocity = new Vector2(0, 50);
                shootDelay = 1.5f;
                break;
                
            case EnemyType.Shooter:
                Radius = 18;
                Health = 3;
                ScoreValue = 200;
                EnemyColor = Color.Purple;
                velocity = new Vector2(0, 60);
                shootDelay = 0.8f;
                break;
                
            case EnemyType.Zigzag:
                Radius = 12;
                Health = 2;
                ScoreValue = 175;
                EnemyColor = Color.Lime;
                velocity = new Vector2(150, 80);
                shootDelay = 2.5f;
                break;
        }
        
        shootTimer = Random.Shared.NextSingle() * shootDelay;
    }
    
    public void Update(float deltaTime, int screenWidth, int screenHeight)
    {
        animationTimer += deltaTime;
        
        // Movement based on type
        switch (Type)
        {
            case EnemyType.Zigzag:
                zigzagTimer += deltaTime;
                if (zigzagTimer > 0.8f)
                {
                    zigzagTimer = 0;
                    zigzagDirection *= -1;
                }
                velocity.X = 150 * zigzagDirection;
                break;
                
            case EnemyType.Shooter:
                // Shooter tries to align with player
                float targetX = game.Player.Position.X;
                float diff = targetX - Position.X;
                velocity.X = Math.Clamp(diff, -80, 80);
                break;
        }
        
        Position += velocity * deltaTime;
        
        // Keep in horizontal bounds
        if (Position.X < Radius || Position.X > screenWidth - Radius)
        {
            velocity.X *= -1;
            Position = new Vector2(
                Math.Clamp(Position.X, Radius, screenWidth - Radius),
                Position.Y
            );
        }
        
        // Deactivate if off screen
        if (Position.Y > screenHeight + Radius * 2)
        {
            IsActive = false;
        }
        
        // Shooting
        shootTimer -= deltaTime;
        if (shootTimer <= 0 && Position.Y > 0 && Position.Y < screenHeight - 100)
        {
            Shoot();
            shootTimer = shootDelay;
        }
    }
    
    private void Shoot()
    {
        Vector2 bulletVel;
        
        switch (Type)
        {
            case EnemyType.Shooter:
                // Aimed shot
                var direction = Vector2.Normalize(game.Player.Position - Position);
                bulletVel = direction * 250;
                game.EnemyBullets.Add(new Bullet(Position + new Vector2(0, Radius), bulletVel, 1, Color.Magenta, false));
                break;
                
            case EnemyType.Tank:
                // Spread shot
                for (int i = -1; i <= 1; i++)
                {
                    bulletVel = new Vector2(i * 80, 200);
                    game.EnemyBullets.Add(new Bullet(Position + new Vector2(i * 10, Radius), bulletVel, 1, Color.DarkGray, false));
                }
                break;
                
            default:
                bulletVel = new Vector2(0, 200);
                game.EnemyBullets.Add(new Bullet(Position + new Vector2(0, Radius), bulletVel, 1, Color.Red, false));
                break;
        }
    }
    
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
            IsActive = false;
    }
    
    public void Draw()
    {
        switch (Type)
        {
            case EnemyType.Basic:
                DrawBasicEnemy();
                break;
            case EnemyType.Fast:
                DrawFastEnemy();
                break;
            case EnemyType.Tank:
                DrawTankEnemy();
                break;
            case EnemyType.Shooter:
                DrawShooterEnemy();
                break;
            case EnemyType.Zigzag:
                DrawZigzagEnemy();
                break;
        }
        
        // Health bar for tanks
        if (Type == EnemyType.Tank)
        {
            float healthPercent = Health / 6f;
            Raylib.DrawRectangle((int)(Position.X - Radius), (int)(Position.Y - Radius - 8), (int)(Radius * 2 * healthPercent), 4, Color.Green);
            Raylib.DrawRectangleLines((int)(Position.X - Radius), (int)(Position.Y - Radius - 8), (int)(Radius * 2), 4, Color.White);
        }
    }
    
    private void DrawBasicEnemy()
    {
        // Diamond shape
        var top = Position + new Vector2(0, -Radius);
        var bottom = Position + new Vector2(0, Radius);
        var left = Position + new Vector2(-Radius, 0);
        var right = Position + new Vector2(Radius, 0);
        
        Raylib.DrawTriangle(top, right, bottom, EnemyColor);
        Raylib.DrawTriangle(top, bottom, left, EnemyColor);
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, 5, Color.DarkGray);
    }
    
    private void DrawFastEnemy()
    {
        // Small triangle
        var tip = Position + new Vector2(0, Radius);
        var left = Position + new Vector2(-Radius, -Radius);
        var right = Position + new Vector2(Radius, -Radius);
        
        Raylib.DrawTriangle(tip, right, left, EnemyColor);
        Raylib.DrawTriangleLines(tip, right, left, Color.Yellow);
    }
    
    private void DrawTankEnemy()
    {
        // Large hexagon-ish shape
        Raylib.DrawRectangle((int)(Position.X - Radius * 0.7f), (int)(Position.Y - Radius * 0.8f), 
            (int)(Radius * 1.4f), (int)(Radius * 1.6f), EnemyColor);
        Raylib.DrawRectangleLines((int)(Position.X - Radius * 0.7f), (int)(Position.Y - Radius * 0.8f), 
            (int)(Radius * 1.4f), (int)(Radius * 1.6f), Color.White);
        
        // Cannons
        Raylib.DrawRectangle((int)(Position.X - Radius * 0.8f), (int)(Position.Y + Radius * 0.3f), 8, 15, Color.DarkGray);
        Raylib.DrawRectangle((int)(Position.X + Radius * 0.5f), (int)(Position.Y + Radius * 0.3f), 8, 15, Color.DarkGray);
        Raylib.DrawRectangle((int)(Position.X - 4), (int)(Position.Y + Radius * 0.3f), 8, 15, Color.DarkGray);
        
        // Core
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, 8, Color.Red);
    }
    
    private void DrawShooterEnemy()
    {
        // Pentagon-like
        float pulse = 1 + MathF.Sin(animationTimer * 5) * 0.1f;
        float r = Radius * pulse;
        
        Raylib.DrawPoly(Position, 5, r, 180, EnemyColor);
        Raylib.DrawPolyLines(Position, 5, r, 180, Color.White);
        
        // Eye
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, 4, Color.White);
        Raylib.DrawCircle((int)Position.X, (int)Position.Y + 1, 2, Color.Black);
    }
    
    private void DrawZigzagEnemy()
    {
        // Lightning bolt shape
        float rot = zigzagDirection > 0 ? 15 : -15;
        Raylib.DrawPoly(Position, 3, Radius, rot, EnemyColor);
        Raylib.DrawPoly(Position, 3, Radius * 0.6f, rot + 60, Color.Yellow);
    }
}
