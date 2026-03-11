using Raylib_cs;

namespace SpaceShooter;

public class UIManager
{
    private readonly Game game;
    private readonly int screenWidth;
    private readonly int screenHeight;
    private float animationTimer;
    
    public UIManager(Game game, int width, int height)
    {
        this.game = game;
        screenWidth = width;
        screenHeight = height;
    }
    
    public void DrawMainMenu()
    {
        animationTimer += Raylib.GetFrameTime();
        
        // Title
        string title = "SPACE SHOOTER";
        int titleSize = 50;
        int titleWidth = Raylib.MeasureText(title, titleSize);
        
        // Animated title with glow
        float pulse = 1 + MathF.Sin(animationTimer * 3) * 0.1f;
        int titleX = (screenWidth - titleWidth) / 2;
        int titleY = 120;
        
        // Glow effect
        Raylib.DrawText(title, titleX + 2, titleY + 2, titleSize, new Color(0, 100, 200, 100));
        Raylib.DrawText(title, titleX, titleY, titleSize, Color.White);
        
        // Subtitle
        string subtitle = "Defend the Galaxy!";
        int subWidth = Raylib.MeasureText(subtitle, 20);
        Raylib.DrawText(subtitle, (screenWidth - subWidth) / 2, titleY + 60, 20, Color.SkyBlue);
        
        // Draw animated ship
        float shipY = 280 + MathF.Sin(animationTimer * 2) * 10;
        DrawMenuShip(screenWidth / 2, (int)shipY);
        
        // Instructions
        DrawCenteredText("Press ENTER or SPACE to Start", 400, 24, 
            (int)(animationTimer * 2) % 2 == 0 ? Color.Yellow : Color.White);
        
        // Controls
        DrawCenteredText("Controls:", 470, 20, Color.Gray);
        DrawCenteredText("WASD / Arrow Keys - Move", 495, 16, Color.LightGray);
        DrawCenteredText("SPACE / Left Click - Shoot", 515, 16, Color.LightGray);
        DrawCenteredText("P / ESC - Pause", 535, 16, Color.LightGray);
        
        // High score
        if (game.HighScore > 0)
        {
            string hsText = $"High Score: {game.HighScore}";
            int hsWidth = Raylib.MeasureText(hsText, 18);
            Raylib.DrawText(hsText, (screenWidth - hsWidth) / 2, 570, 18, Color.Gold);
        }
    }
    
    private void DrawMenuShip(int x, int y)
    {
        // Draw a larger version of the player ship
        var tip = new System.Numerics.Vector2(x, y - 40);
        var left = new System.Numerics.Vector2(x - 30, y + 30);
        var right = new System.Numerics.Vector2(x + 30, y + 30);
        
        Raylib.DrawTriangle(tip, left, right, Color.DarkBlue);
        Raylib.DrawTriangleLines(tip, left, right, Color.SkyBlue);
        Raylib.DrawCircle(x, y - 10, 10, Color.SkyBlue);
        
        // Thruster
        float thrusterSize = 10 + MathF.Sin(animationTimer * 15) * 5;
        Raylib.DrawTriangle(
            new System.Numerics.Vector2(x - 15, y + 30),
            new System.Numerics.Vector2(x + 15, y + 30),
            new System.Numerics.Vector2(x, y + 30 + thrusterSize + 20),
            Color.Orange
        );
    }
    
    public void DrawHUD()
    {
        // Health bar
        int barWidth = 150;
        int barHeight = 20;
        int barX = 10;
        int barY = 10;
        
        Raylib.DrawRectangle(barX, barY, barWidth, barHeight, Color.DarkGray);
        float healthPercent = game.Player.Health / (float)game.Player.MaxHealth;
        Color healthColor = healthPercent > 0.5f ? Color.Green : healthPercent > 0.25f ? Color.Yellow : Color.Red;
        Raylib.DrawRectangle(barX, barY, (int)(barWidth * healthPercent), barHeight, healthColor);
        Raylib.DrawRectangleLines(barX, barY, barWidth, barHeight, Color.White);
        
        // Health text
        string healthText = $"HP: {game.Player.Health}/{game.Player.MaxHealth}";
        Raylib.DrawText(healthText, barX + 5, barY + 3, 14, Color.White);
        
        // Shield indicator
        if (game.Player.HasShield)
        {
            Raylib.DrawText("SHIELD", barX, barY + 25, 14, Color.SkyBlue);
        }
        
        // Score
        string scoreText = $"Score: {game.Score}";
        Raylib.DrawText(scoreText, screenWidth - 150, 10, 20, Color.White);
        
        // Wave
        string waveText = $"Wave: {game.Wave}";
        Raylib.DrawText(waveText, screenWidth - 150, 35, 18, Color.Yellow);
        
        // Enemy count
        string enemyText = $"Enemies: {game.Enemies.Count}";
        Raylib.DrawText(enemyText, screenWidth - 150, 55, 14, Color.Gray);
    }
    
    public void DrawPauseMenu()
    {
        // Darken background
        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 150));
        
        // Pause title
        string pauseText = "PAUSED";
        int textWidth = Raylib.MeasureText(pauseText, 50);
        Raylib.DrawText(pauseText, (screenWidth - textWidth) / 2, 200, 50, Color.White);
        
        // Instructions
        DrawCenteredText("Press P or ESC to Resume", 300, 20, Color.Yellow);
        DrawCenteredText("Press M for Main Menu", 330, 20, Color.Gray);
        
        // Current stats
        DrawCenteredText($"Score: {game.Score}", 400, 24, Color.White);
        DrawCenteredText($"Wave: {game.Wave}", 430, 20, Color.SkyBlue);
    }
    
    public void DrawGameOver()
    {
        // Darken background
        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 180));
        
        animationTimer += Raylib.GetFrameTime();
        
        // Game Over text
        string gameOverText = "GAME OVER";
        int goWidth = Raylib.MeasureText(gameOverText, 60);
        
        // Red pulsing effect
        float pulse = 0.7f + MathF.Sin(animationTimer * 4) * 0.3f;
        var color = new Color((int)(255 * pulse), 0, 0, 255);
        Raylib.DrawText(gameOverText, (screenWidth - goWidth) / 2, 150, 60, color);
        
        // Stats
        DrawCenteredText($"Final Score: {game.Score}", 250, 30, Color.White);
        DrawCenteredText($"Waves Survived: {game.Wave}", 290, 24, Color.SkyBlue);
        
        // New high score?
        if (game.Score >= game.HighScore && game.Score > 0)
        {
            DrawCenteredText("NEW HIGH SCORE!", 340, 28, Color.Gold);
        }
        else if (game.HighScore > 0)
        {
            DrawCenteredText($"High Score: {game.HighScore}", 340, 20, Color.Gray);
        }
        
        // Instructions
        DrawCenteredText("Press ENTER or SPACE to Play Again", 420, 20, 
            (int)(animationTimer * 2) % 2 == 0 ? Color.Yellow : Color.White);
        DrawCenteredText("Press M for Main Menu", 450, 18, Color.Gray);
    }
    
    private void DrawCenteredText(string text, int y, int fontSize, Color color)
    {
        int textWidth = Raylib.MeasureText(text, fontSize);
        Raylib.DrawText(text, (screenWidth - textWidth) / 2, y, fontSize, color);
    }
}
