using Raylib_cs;
using System.Numerics;

namespace SpaceShooter;

public class Star
{
    public Vector2 Position;
    public float Speed;
    public float Size;
    public byte Brightness;
}

public class ParallaxBackground
{
    private readonly int screenWidth;
    private readonly int screenHeight;
    private readonly List<Star> stars = new();
    private readonly int starCount = 150;
    
    public ParallaxBackground(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
        
        // Create stars at different depths
        for (int i = 0; i < starCount; i++)
        {
            AddStar(true);
        }
    }
    
    private void AddStar(bool randomY)
    {
        float depth = Random.Shared.NextSingle(); // 0 = far, 1 = close
        
        stars.Add(new Star
        {
            Position = new Vector2(
                Random.Shared.Next(0, screenWidth),
                randomY ? Random.Shared.Next(0, screenHeight) : -5
            ),
            Speed = 20 + depth * 100, // Farther stars move slower
            Size = 0.5f + depth * 2f,
            Brightness = (byte)(100 + depth * 155)
        });
    }
    
    public void Update(float deltaTime, float speedMultiplier)
    {
        for (int i = stars.Count - 1; i >= 0; i--)
        {
            var star = stars[i];
            star.Position.Y += star.Speed * deltaTime * speedMultiplier;
            
            // Wrap around
            if (star.Position.Y > screenHeight + 5)
            {
                stars.RemoveAt(i);
                AddStar(false);
            }
        }
    }
    
    public void Draw()
    {
        // Draw gradient background
        for (int y = 0; y < screenHeight; y += 4)
        {
            float t = y / (float)screenHeight;
            byte r = (byte)(5 + t * 10);
            byte g = (byte)(5 + t * 15);
            byte b = (byte)(20 + t * 30);
            Raylib.DrawRectangle(0, y, screenWidth, 4, new Color((int)r, (int)g, (int)b, 255));
        }
        
        // Draw stars
        foreach (var star in stars)
        {
            var color = new Color((int)star.Brightness, (int)star.Brightness, Math.Min(255, (int)star.Brightness + 50), 255);
            
            if (star.Size < 1.2f)
            {
                Raylib.DrawPixel((int)star.Position.X, (int)star.Position.Y, color);
            }
            else if (star.Size < 2f)
            {
                Raylib.DrawRectangle((int)star.Position.X, (int)star.Position.Y, 2, 2, color);
            }
            else
            {
                Raylib.DrawCircle((int)star.Position.X, (int)star.Position.Y, star.Size / 2, color);
                
                // Twinkle effect for larger stars
                if (Random.Shared.NextSingle() > 0.98f)
                {
                    Raylib.DrawLine(
                        (int)star.Position.X - 3, (int)star.Position.Y,
                        (int)star.Position.X + 3, (int)star.Position.Y,
                        Color.White
                    );
                    Raylib.DrawLine(
                        (int)star.Position.X, (int)star.Position.Y - 3,
                        (int)star.Position.X, (int)star.Position.Y + 3,
                        Color.White
                    );
                }
            }
        }
    }
}
