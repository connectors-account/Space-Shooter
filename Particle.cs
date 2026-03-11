using Raylib_cs;
using System.Numerics;

namespace SpaceShooter;

public class Particle
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }
    public Color ParticleColor { get; }
    public float Size { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    private float lifetime;
    private readonly float maxLifetime;
    
    public Particle(Vector2 position, Vector2 velocity, Color color, float size, float lifetime)
    {
        Position = position;
        Velocity = velocity;
        ParticleColor = color;
        Size = size;
        this.lifetime = lifetime;
        maxLifetime = lifetime;
    }
    
    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
        Velocity *= 0.98f; // Friction
        lifetime -= deltaTime;
        
        // Shrink over time
        float lifeRatio = lifetime / maxLifetime;
        Size *= 0.95f + lifeRatio * 0.05f;
        
        if (lifetime <= 0 || Size < 0.5f)
            IsActive = false;
    }
    
    public void Draw()
    {
        float alpha = Math.Clamp(lifetime / maxLifetime, 0, 1);
        var color = new Color(ParticleColor.R, ParticleColor.G, ParticleColor.B, (byte)(255 * alpha));
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, Size, color);
    }
}
