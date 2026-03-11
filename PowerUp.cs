using Raylib_cs;
using System.Numerics;

namespace SpaceShooter;

public enum PowerUpType
{
    Health,
    Shield,
    SpeedBoost,
    RapidFire,
    MultiShot
}

public class PowerUp : ICollidable
{
    public Vector2 Position { get; set; }
    public float Radius => 12;
    public bool IsActive { get; set; } = true;
    public PowerUpType Type { get; }
    public Color PowerUpColor { get; }
    
    private readonly float fallSpeed = 80f;
    private readonly int screenHeight;
    private float animationTimer;
    private float lifetime = 15f;
    
    public PowerUp(Vector2 position, PowerUpType type, int screenHeight)
    {
        Position = position;
        Type = type;
        this.screenHeight = screenHeight;
        
        PowerUpColor = type switch
        {
            PowerUpType.Health => Color.Pink,
            PowerUpType.Shield => Color.SkyBlue,
            PowerUpType.SpeedBoost => Color.Green,
            PowerUpType.RapidFire => Color.Red,
            PowerUpType.MultiShot => Color.Purple,
            _ => Color.White
        };
    }
    
    public void Update(float deltaTime)
    {
        Position += new Vector2(0, fallSpeed * deltaTime);
        animationTimer += deltaTime;
        lifetime -= deltaTime;
        
        if (Position.Y > screenHeight + Radius || lifetime <= 0)
            IsActive = false;
    }
    
    public void Apply(Player player)
    {
        switch (Type)
        {
            case PowerUpType.Health:
                player.Heal(2);
                break;
            case PowerUpType.Shield:
                player.HasShield = true;
                break;
            case PowerUpType.SpeedBoost:
                player.ApplySpeedBoost(8f);
                break;
            case PowerUpType.RapidFire:
                player.ApplyRapidFire(6f);
                break;
            case PowerUpType.MultiShot:
                player.ApplyMultiShot(7f);
                break;
        }
    }
    
    public void Draw()
    {
        float pulse = 1 + MathF.Sin(animationTimer * 6) * 0.2f;
        float r = Radius * pulse;
        
        // Outer glow
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, r + 4, new Color((int)PowerUpColor.R, (int)PowerUpColor.G, (int)PowerUpColor.B, 50));
        
        // Main circle
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, r, PowerUpColor);
        Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, r, Color.White);
        
        // Icon based on type
        DrawIcon();
        
        // Blinking when about to expire
        if (lifetime < 3f && (int)(animationTimer * 4) % 2 == 0)
        {
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, r + 6, Color.White);
        }
    }
    
    private void DrawIcon()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        
        switch (Type)
        {
            case PowerUpType.Health:
                // Plus sign
                Raylib.DrawRectangle(x - 1, y - 6, 3, 12, Color.White);
                Raylib.DrawRectangle(x - 6, y - 1, 12, 3, Color.White);
                break;
                
            case PowerUpType.Shield:
                // Shield shape
                Raylib.DrawCircleLines(x, y, 6, Color.White);
                break;
                
            case PowerUpType.SpeedBoost:
                // Arrow
                Raylib.DrawTriangle(
                    new Vector2(x, y - 6),
                    new Vector2(x - 5, y + 4),
                    new Vector2(x + 5, y + 4),
                    Color.White
                );
                break;
                
            case PowerUpType.RapidFire:
                // Multiple lines
                Raylib.DrawLine(x - 4, y - 5, x - 4, y + 5, Color.White);
                Raylib.DrawLine(x, y - 5, x, y + 5, Color.White);
                Raylib.DrawLine(x + 4, y - 5, x + 4, y + 5, Color.White);
                break;
                
            case PowerUpType.MultiShot:
                // Star
                Raylib.DrawPoly(new Vector2(x, y), 6, 5, 0, Color.White);
                break;
        }
    }
}
