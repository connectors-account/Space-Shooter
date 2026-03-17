using UnityEngine;

/// <summary>
/// Generates procedural sprites for game objects.
/// Used to create placeholder sprites without external image files.
/// </summary>
public static class SpriteGenerator
{
    /// <summary>
    /// Create a player ship sprite
    /// </summary>
    public static Sprite CreatePlayerShip(int size = 64)
    {
        Texture2D texture = new Texture2D(size, size);
        ClearTexture(texture);
        
        Color mainColor = new Color(0.3f, 0.7f, 1f);
        Color accentColor = new Color(0.1f, 0.4f, 0.8f);
        Color cockpitColor = new Color(0.8f, 0.9f, 1f);
        
        int centerX = size / 2;
        int centerY = size / 2;
        
        // Ship body (triangle)
        for (int y = 0; y < size; y++)
        {
            float normalizedY = (float)y / size;
            int halfWidth = (int)(size * 0.4f * (1f - normalizedY * 0.7f));
            
            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                {
                    Color c = (Mathf.Abs(x - centerX) < halfWidth * 0.3f) ? accentColor : mainColor;
                    texture.SetPixel(x, y, c);
                }
            }
        }
        
        // Cockpit
        DrawCircle(texture, centerX, (int)(size * 0.6f), (int)(size * 0.12f), cockpitColor);
        
        // Wings
        for (int i = 0; i < (int)(size * 0.25f); i++)
        {
            int wingY = (int)(size * 0.25f) + i / 2;
            int leftX = (int)(size * 0.15f) - i / 3;
            int rightX = (int)(size * 0.85f) + i / 3;
            
            if (leftX >= 0 && wingY < size)
                texture.SetPixel(leftX, wingY, accentColor);
            if (rightX < size && wingY < size)
                texture.SetPixel(rightX, wingY, accentColor);
        }
        
        // Engine glow
        DrawCircle(texture, centerX, 3, 4, new Color(1f, 0.6f, 0.2f));
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Create an enemy ship sprite
    /// </summary>
    public static Sprite CreateEnemyShip(int size = 64, int type = 0)
    {
        Texture2D texture = new Texture2D(size, size);
        ClearTexture(texture);
        
        Color mainColor, accentColor;
        
        switch (type)
        {
            case 1: // Zigzag enemy
                mainColor = new Color(1f, 0.6f, 0.2f);
                accentColor = new Color(0.8f, 0.4f, 0.1f);
                break;
            case 2: // Shooter enemy
                mainColor = new Color(0.8f, 0.2f, 0.8f);
                accentColor = new Color(0.6f, 0.1f, 0.6f);
                break;
            default: // Basic enemy
                mainColor = new Color(1f, 0.3f, 0.3f);
                accentColor = new Color(0.8f, 0.2f, 0.2f);
                break;
        }
        
        int centerX = size / 2;
        
        // Inverted triangle (pointing down)
        for (int y = size - 1; y >= 0; y--)
        {
            float normalizedY = 1f - (float)y / size;
            int halfWidth = (int)(size * 0.35f * (1f - normalizedY * 0.6f));
            
            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                {
                    Color c = (Mathf.Abs(x - centerX) < halfWidth * 0.4f) ? accentColor : mainColor;
                    texture.SetPixel(x, y, c);
                }
            }
        }
        
        // Eye/cockpit
        DrawCircle(texture, centerX, (int)(size * 0.55f), (int)(size * 0.1f), Color.yellow);
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Create a bullet sprite
    /// </summary>
    public static Sprite CreateBullet(int size = 16, bool isPlayerBullet = true)
    {
        Texture2D texture = new Texture2D(size, size);
        ClearTexture(texture);
        
        Color color = isPlayerBullet ? new Color(0.3f, 1f, 1f) : new Color(1f, 0.3f, 0.3f);
        Color glowColor = isPlayerBullet ? new Color(0.6f, 1f, 1f, 0.5f) : new Color(1f, 0.6f, 0.6f, 0.5f);
        
        int centerX = size / 2;
        int centerY = size / 2;
        
        // Glow
        DrawCircle(texture, centerX, centerY, size / 2 - 1, glowColor);
        
        // Core
        DrawCircle(texture, centerX, centerY, size / 4, color);
        
        // Elongate for direction
        for (int y = size / 4; y < size * 3 / 4; y++)
        {
            for (int x = centerX - 2; x <= centerX + 2; x++)
            {
                if (x >= 0 && x < size)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Create a power-up sprite
    /// </summary>
    public static Sprite CreatePowerUp(int size = 32, int type = 0)
    {
        Texture2D texture = new Texture2D(size, size);
        ClearTexture(texture);
        
        Color color;
        string symbol = "";
        
        switch (type)
        {
            case 0: // Shield
                color = new Color(0.3f, 0.7f, 1f);
                break;
            case 1: // Rapid fire
                color = new Color(1f, 0.8f, 0.2f);
                break;
            case 2: // Health
                color = new Color(0.3f, 1f, 0.3f);
                break;
            default:
                color = Color.white;
                break;
        }
        
        int centerX = size / 2;
        int centerY = size / 2;
        int radius = size / 2 - 2;
        
        // Diamond shape
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - centerX);
                int dy = Mathf.Abs(y - centerY);
                
                if (dx + dy < radius)
                {
                    float dist = (dx + dy) / (float)radius;
                    Color c = Color.Lerp(Color.white, color, dist);
                    texture.SetPixel(x, y, c);
                }
                else if (dx + dy == radius)
                {
                    texture.SetPixel(x, y, color * 0.7f);
                }
            }
        }
        
        // Add symbol indicator
        DrawSymbol(texture, centerX, centerY, type);
        
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Create explosion sprite
    /// </summary>
    public static Sprite CreateExplosion(int size = 64)
    {
        Texture2D texture = new Texture2D(size, size);
        ClearTexture(texture);
        
        int centerX = size / 2;
        int centerY = size / 2;
        
        // Create radial gradient explosion
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                float normalizedDist = dist / (size / 2f);
                
                if (normalizedDist < 1f)
                {
                    // Inner white core
                    if (normalizedDist < 0.2f)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    // Yellow middle
                    else if (normalizedDist < 0.5f)
                    {
                        float t = (normalizedDist - 0.2f) / 0.3f;
                        texture.SetPixel(x, y, Color.Lerp(Color.white, Color.yellow, t));
                    }
                    // Orange/red outer
                    else
                    {
                        float t = (normalizedDist - 0.5f) / 0.5f;
                        Color c = Color.Lerp(new Color(1f, 0.5f, 0f), new Color(1f, 0.2f, 0f, 0f), t);
                        texture.SetPixel(x, y, c);
                    }
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private static void ClearTexture(Texture2D texture)
    {
        Color[] pixels = new Color[texture.width * texture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        texture.SetPixels(pixels);
    }
    
    private static void DrawCircle(Texture2D texture, int cx, int cy, int radius, Color color)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }
    }
    
    private static void DrawSymbol(Texture2D texture, int cx, int cy, int type)
    {
        Color symbolColor = new Color(1f, 1f, 1f, 0.8f);
        int size = texture.width / 6;
        
        switch (type)
        {
            case 0: // Shield - circle
                for (int a = 0; a < 360; a += 10)
                {
                    float rad = a * Mathf.Deg2Rad;
                    int x = cx + (int)(Mathf.Cos(rad) * size);
                    int y = cy + (int)(Mathf.Sin(rad) * size);
                    if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
                        texture.SetPixel(x, y, symbolColor);
                }
                break;
            
            case 1: // Rapid fire - lightning bolt
                DrawLine(texture, cx - size, cy + size, cx, cy, symbolColor);
                DrawLine(texture, cx, cy, cx - size / 2, cy, symbolColor);
                DrawLine(texture, cx - size / 2, cy, cx + size / 2, cy - size, symbolColor);
                break;
            
            case 2: // Health - cross
                DrawLine(texture, cx - size, cy, cx + size, cy, symbolColor);
                DrawLine(texture, cx, cy - size, cx, cy + size, symbolColor);
                break;
        }
    }
    
    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            if (x0 >= 0 && x0 < texture.width && y0 >= 0 && y0 < texture.height)
                texture.SetPixel(x0, y0, color);
            
            if (x0 == x1 && y0 == y1) break;
            
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
}
