using Raylib_cs;
using System.Numerics;

namespace SpaceShooter;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class Game
{
    private readonly int screenWidth;
    private readonly int screenHeight;
    
    public GameState State { get; private set; } = GameState.MainMenu;
    
    // Game entities
    public Player Player { get; private set; } = null!;
    public List<Enemy> Enemies { get; } = new();
    public List<Bullet> PlayerBullets { get; } = new();
    public List<Bullet> EnemyBullets { get; } = new();
    public List<PowerUp> PowerUps { get; } = new();
    public List<Particle> Particles { get; } = new();
    
    // Systems
    public EnemySpawner EnemySpawner { get; private set; } = null!;
    public ParallaxBackground Background { get; private set; } = null!;
    public SoundManager SoundManager { get; private set; } = null!;
    public UIManager UI { get; private set; } = null!;
    
    // Game stats
    public int Score { get; set; }
    public int Wave { get; set; } = 1;
    public int HighScore { get; private set; }
    
    // Timing
    private float gameOverTimer;
    private const float GAME_OVER_DELAY = 2f;
    
    public Game(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
        
        SoundManager = new SoundManager();
        Background = new ParallaxBackground(width, height);
        UI = new UIManager(this, width, height);
        
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        Player = new Player(screenWidth / 2, screenHeight - 80, this);
        EnemySpawner = new EnemySpawner(this, screenWidth, screenHeight);
        
        Enemies.Clear();
        PlayerBullets.Clear();
        EnemyBullets.Clear();
        PowerUps.Clear();
        Particles.Clear();
        
        Score = 0;
        Wave = 1;
        gameOverTimer = 0;
    }
    
    public void StartGame()
    {
        InitializeGame();
        State = GameState.Playing;
        SoundManager.PlayMusic();
    }
    
    public void Update(float deltaTime)
    {
        switch (State)
        {
            case GameState.MainMenu:
                UpdateMainMenu();
                break;
            case GameState.Playing:
                UpdatePlaying(deltaTime);
                break;
            case GameState.Paused:
                UpdatePaused();
                break;
            case GameState.GameOver:
                UpdateGameOver(deltaTime);
                break;
        }
        
        Background.Update(deltaTime, State == GameState.Playing ? 1f : 0.3f);
        SoundManager.UpdateMusic();
    }
    
    private void UpdateMainMenu()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            StartGame();
            SoundManager.PlaySelect();
        }
    }
    
    private void UpdatePlaying(float deltaTime)
    {
        // Pause
        if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsKeyPressed(KeyboardKey.P))
        {
            State = GameState.Paused;
            SoundManager.PlaySelect();
            return;
        }
        
        // Update player
        Player.Update(deltaTime, screenWidth, screenHeight);
        
        // Update enemies
        EnemySpawner.Update(deltaTime);
        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            Enemies[i].Update(deltaTime, screenWidth, screenHeight);
            if (!Enemies[i].IsActive)
                Enemies.RemoveAt(i);
        }
        
        // Update bullets
        UpdateBullets(PlayerBullets, deltaTime);
        UpdateBullets(EnemyBullets, deltaTime);
        
        // Update power-ups
        for (int i = PowerUps.Count - 1; i >= 0; i--)
        {
            PowerUps[i].Update(deltaTime);
            if (!PowerUps[i].IsActive)
                PowerUps.RemoveAt(i);
        }
        
        // Update particles
        for (int i = Particles.Count - 1; i >= 0; i--)
        {
            Particles[i].Update(deltaTime);
            if (!Particles[i].IsActive)
                Particles.RemoveAt(i);
        }
        
        // Check collisions
        CheckCollisions();
        
        // Check game over
        if (Player.Health <= 0 && gameOverTimer == 0)
        {
            gameOverTimer = GAME_OVER_DELAY;
            SpawnExplosion(Player.Position, 30, Color.Orange);
            SoundManager.PlayExplosion();
        }
        
        if (gameOverTimer > 0)
        {
            gameOverTimer -= deltaTime;
            if (gameOverTimer <= 0)
            {
                if (Score > HighScore)
                    HighScore = Score;
                State = GameState.GameOver;
            }
        }
    }
    
    private void UpdatePaused()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsKeyPressed(KeyboardKey.P))
        {
            State = GameState.Playing;
            SoundManager.PlaySelect();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.M))
        {
            State = GameState.MainMenu;
            SoundManager.PlaySelect();
        }
    }
    
    private void UpdateGameOver(float deltaTime)
    {
        // Update particles for visual effect
        for (int i = Particles.Count - 1; i >= 0; i--)
        {
            Particles[i].Update(deltaTime);
            if (!Particles[i].IsActive)
                Particles.RemoveAt(i);
        }
        
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            StartGame();
            SoundManager.PlaySelect();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.M))
        {
            State = GameState.MainMenu;
            SoundManager.PlaySelect();
        }
    }
    
    private void UpdateBullets(List<Bullet> bullets, float deltaTime)
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            bullets[i].Update(deltaTime);
            if (!bullets[i].IsActive || 
                bullets[i].Position.Y < -20 || 
                bullets[i].Position.Y > screenHeight + 20 ||
                bullets[i].Position.X < -20 ||
                bullets[i].Position.X > screenWidth + 20)
            {
                bullets.RemoveAt(i);
            }
        }
    }
    
    private void CheckCollisions()
    {
        // Player bullets vs enemies
        foreach (var bullet in PlayerBullets.ToList())
        {
            foreach (var enemy in Enemies.ToList())
            {
                if (bullet.IsActive && enemy.IsActive && 
                    CollisionSystem.CheckCollision(bullet, enemy))
                {
                    bullet.IsActive = false;
                    enemy.TakeDamage(bullet.Damage);
                    
                    SpawnHitParticles(bullet.Position, enemy.EnemyColor);
                    SoundManager.PlayHit();
                    
                    if (!enemy.IsActive)
                    {
                        Score += enemy.ScoreValue;
                        SpawnExplosion(enemy.Position, 15, enemy.EnemyColor);
                        SoundManager.PlayExplosion();
                        
                        // Chance to spawn power-up
                        if (Random.Shared.NextDouble() < 0.15)
                        {
                            SpawnPowerUp(enemy.Position);
                        }
                    }
                }
            }
        }
        
        // Enemy bullets vs player (only if player is alive and not invincible)
        if (Player.Health > 0 && !Player.IsInvincible)
        {
            foreach (var bullet in EnemyBullets.ToList())
            {
                if (bullet.IsActive && CollisionSystem.CheckCollision(bullet, Player))
                {
                    bullet.IsActive = false;
                    
                    if (Player.HasShield)
                    {
                        Player.HasShield = false;
                        SpawnHitParticles(bullet.Position, Color.SkyBlue);
                        SoundManager.PlayShieldHit();
                    }
                    else
                    {
                        Player.TakeDamage(1);
                        SpawnHitParticles(Player.Position, Color.Red);
                        SoundManager.PlayPlayerHit();
                    }
                }
            }
        }
        
        // Enemies vs player
        if (Player.Health > 0 && !Player.IsInvincible)
        {
            foreach (var enemy in Enemies.ToList())
            {
                if (enemy.IsActive && CollisionSystem.CheckCollision(Player, enemy))
                {
                    enemy.IsActive = false;
                    SpawnExplosion(enemy.Position, 15, enemy.EnemyColor);
                    
                    if (Player.HasShield)
                    {
                        Player.HasShield = false;
                        SoundManager.PlayShieldHit();
                    }
                    else
                    {
                        Player.TakeDamage(2);
                        SoundManager.PlayPlayerHit();
                    }
                }
            }
        }
        
        // Power-ups vs player
        foreach (var powerUp in PowerUps.ToList())
        {
            if (powerUp.IsActive && CollisionSystem.CheckCollision(Player, powerUp))
            {
                powerUp.Apply(Player);
                powerUp.IsActive = false;
                SpawnHitParticles(powerUp.Position, powerUp.PowerUpColor);
                SoundManager.PlayPowerUp();
            }
        }
    }
    
    public void SpawnExplosion(Vector2 position, int count, Color color)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Shared.NextSingle() * MathF.PI * 2;
            float speed = 50 + Random.Shared.NextSingle() * 150;
            var velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed);
            float size = 2 + Random.Shared.NextSingle() * 4;
            float lifetime = 0.3f + Random.Shared.NextSingle() * 0.5f;
            
            Particles.Add(new Particle(position, velocity, color, size, lifetime));
        }
    }
    
    private void SpawnHitParticles(Vector2 position, Color color)
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = Random.Shared.NextSingle() * MathF.PI * 2;
            float speed = 30 + Random.Shared.NextSingle() * 80;
            var velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed);
            float size = 1 + Random.Shared.NextSingle() * 2;
            
            Particles.Add(new Particle(position, velocity, color, size, 0.2f));
        }
    }
    
    private void SpawnPowerUp(Vector2 position)
    {
        var type = (PowerUpType)Random.Shared.Next(0, 5);
        PowerUps.Add(new PowerUp(position, type, screenHeight));
    }
    
    public void Draw()
    {
        Background.Draw();
        
        switch (State)
        {
            case GameState.MainMenu:
                UI.DrawMainMenu();
                break;
            case GameState.Playing:
            case GameState.Paused:
                DrawGameplay();
                if (State == GameState.Paused)
                    UI.DrawPauseMenu();
                else
                    UI.DrawHUD();
                break;
            case GameState.GameOver:
                DrawGameplay();
                UI.DrawGameOver();
                break;
        }
    }
    
    private void DrawGameplay()
    {
        // Draw power-ups
        foreach (var powerUp in PowerUps)
            powerUp.Draw();
        
        // Draw bullets
        foreach (var bullet in PlayerBullets)
            bullet.Draw();
        foreach (var bullet in EnemyBullets)
            bullet.Draw();
        
        // Draw enemies
        foreach (var enemy in Enemies)
            enemy.Draw();
        
        // Draw player
        if (Player.Health > 0 || gameOverTimer > 0)
            Player.Draw();
        
        // Draw particles
        foreach (var particle in Particles)
            particle.Draw();
    }
    
    public void Unload()
    {
        SoundManager.Unload();
    }
}
