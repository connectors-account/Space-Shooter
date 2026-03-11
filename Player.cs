using Raylib_cs;
using System.Numerics;

namespace SpaceShooter;

public class Player : ICollidable
{
    public Vector2 Position { get; set; }
    public float Radius => 15;
    public int Health { get; private set; } = 5;
    public int MaxHealth { get; } = 5;
    
    private readonly Game game;
    
    // Movement
    private float baseSpeed = 300f;
    private float speedMultiplier = 1f;
    
    // Shooting
    private float shootCooldown;
    private float baseShootDelay = 0.2f;
    private float shootDelayMultiplier = 1f;
    private int bulletCount = 1;
    
    // Power-up timers
    private float speedBoostTimer;
    private float rapidFireTimer;
    private float multiShotTimer;
    public bool HasShield { get; set; }
    
    // Invincibility
    public bool IsInvincible { get; private set; }
    private float invincibilityTimer;
    private const float INVINCIBILITY_DURATION = 2f;
    
    // Visual
    private float thrusterAnimation;
    
    public Player(float x, float y, Game game)
    {
        Position = new Vector2(x, y);
        this.game = game;
    }
    
    public void Update(float deltaTime, int screenWidth, int screenHeight)
    {
        if (Health <= 0) return;
        
        // Movement input
        var moveDirection = Vector2.Zero;
        
        if (Raylib.IsKeyDown(KeyboardKey.Left) || Raylib.IsKeyDown(KeyboardKey.A))
            moveDirection.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Right) || Raylib.IsKeyDown(KeyboardKey.D))
            moveDirection.X += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Up) || Raylib.IsKeyDown(KeyboardKey.W))
            moveDirection.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Down) || Raylib.IsKeyDown(KeyboardKey.S))
            moveDirection.Y += 1;
        
        if (moveDirection != Vector2.Zero)
        {
            moveDirection = Vector2.Normalize(moveDirection);
            Position += moveDirection * baseSpeed * speedMultiplier * deltaTime;
        }
        
        // Clamp to screen
        Position = new Vector2(
            Math.Clamp(Position.X, Radius, screenWidth - Radius),
            Math.Clamp(Position.Y, Radius, screenHeight - Radius)
        );
        
        // Shooting
        shootCooldown -= deltaTime;
        if ((Raylib.IsKeyDown(KeyboardKey.Space) || Raylib.IsMouseButtonDown(MouseButton.Left)) && shootCooldown <= 0)
        {
            Shoot();
            shootCooldown = baseShootDelay * shootDelayMultiplier;
        }
        
        // Update power-up timers
        UpdatePowerUps(deltaTime);
        
        // Update invincibility
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= deltaTime;
            if (invincibilityTimer <= 0)
                IsInvincible = false;
        }
        
        // Animation
        thrusterAnimation += deltaTime * 15;
    }
    
    private void Shoot()
    {
        game.SoundManager.PlayShoot();
        
        if (bulletCount == 1)
        {
            game.PlayerBullets.Add(new Bullet(Position + new Vector2(0, -15), new Vector2(0, -500), 1, Color.Yellow, true));
        }
        else if (bulletCount == 2)
        {
            game.PlayerBullets.Add(new Bullet(Position + new Vector2(-10, -10), new Vector2(0, -500), 1, Color.Yellow, true));
            game.PlayerBullets.Add(new Bullet(Position + new Vector2(10, -10), new Vector2(0, -500), 1, Color.Yellow, true));
        }
        else if (bulletCount >= 3)
        {
            game.PlayerBullets.Add(new Bullet(Position + new Vector2(0, -15), new Vector2(0, -500), 1, Color.Yellow, true));
            game.PlayerBullets.Add(new Bullet(Position + new Vector2(-12, -8), new Vector2(-50, -480), 1, Color.Orange, true));
            game.PlayerBullets.Add(new Bullet(Position + new Vector2(12, -8), new Vector2(50, -480), 1, Color.Orange, true));
        }
    }
    
    private void UpdatePowerUps(float deltaTime)
    {
        if (speedBoostTimer > 0)
        {
            speedBoostTimer -= deltaTime;
            if (speedBoostTimer <= 0)
                speedMultiplier = 1f;
        }
        
        if (rapidFireTimer > 0)
        {
            rapidFireTimer -= deltaTime;
            if (rapidFireTimer <= 0)
                shootDelayMultiplier = 1f;
        }
        
        if (multiShotTimer > 0)
        {
            multiShotTimer -= deltaTime;
            if (multiShotTimer <= 0)
                bulletCount = 1;
        }
    }
    
    public void ApplySpeedBoost(float duration)
    {
        speedMultiplier = 1.5f;
        speedBoostTimer = duration;
    }
    
    public void ApplyRapidFire(float duration)
    {
        shootDelayMultiplier = 0.4f;
        rapidFireTimer = duration;
    }
    
    public void ApplyMultiShot(float duration)
    {
        bulletCount = 3;
        multiShotTimer = duration;
    }
    
    public void Heal(int amount)
    {
        Health = Math.Min(Health + amount, MaxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        if (IsInvincible) return;
        
        Health -= damage;
        if (Health > 0)
        {
            IsInvincible = true;
            invincibilityTimer = INVINCIBILITY_DURATION;
        }
    }
    
    public void Draw()
    {
        if (Health <= 0) return;
        
        // Skip drawing during invincibility blink
        if (IsInvincible && ((int)(invincibilityTimer * 10) % 2 == 0))
            return;
        
        // Draw ship body (triangle)
        var tip = Position + new Vector2(0, -20);
        var left = Position + new Vector2(-15, 15);
        var right = Position + new Vector2(15, 15);
        
        Raylib.DrawTriangle(tip, left, right, Color.DarkBlue);
        Raylib.DrawTriangleLines(tip, left, right, Color.SkyBlue);
        
        // Draw cockpit
        Raylib.DrawCircle((int)Position.X, (int)(Position.Y - 5), 5, Color.SkyBlue);
        
        // Draw thruster
        float thrusterSize = 5 + MathF.Sin(thrusterAnimation) * 3;
        Raylib.DrawTriangle(
            Position + new Vector2(-8, 15),
            Position + new Vector2(8, 15),
            Position + new Vector2(0, 15 + thrusterSize + 10),
            Color.Orange
        );
        Raylib.DrawTriangle(
            Position + new Vector2(-5, 15),
            Position + new Vector2(5, 15),
            Position + new Vector2(0, 15 + thrusterSize + 5),
            Color.Yellow
        );
        
        // Draw shield if active
        if (HasShield)
        {
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, Radius + 10, Color.SkyBlue);
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, Radius + 12, new Color(135, 206, 235, 100));
        }
        
        // Draw power-up indicators
        DrawPowerUpIndicators();
    }
    
    private void DrawPowerUpIndicators()
    {
        int y = (int)Position.Y + 30;
        int x = (int)Position.X - 20;
        
        if (speedBoostTimer > 0)
        {
            Raylib.DrawRectangle(x, y, 10, 4, Color.Green);
            x += 12;
        }
        if (rapidFireTimer > 0)
        {
            Raylib.DrawRectangle(x, y, 10, 4, Color.Red);
            x += 12;
        }
        if (multiShotTimer > 0)
        {
            Raylib.DrawRectangle(x, y, 10, 4, Color.Purple);
        }
    }
}
