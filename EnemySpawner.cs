using System.Numerics;

namespace SpaceShooter;

public class EnemySpawner
{
    private readonly Game game;
    private readonly int screenWidth;
    private readonly int screenHeight;
    
    private float spawnTimer;
    private float spawnDelay = 2f;
    private int enemiesSpawnedThisWave;
    private int enemiesPerWave = 10;

    private float waveCooldown;
    private const float WAVE_COOLDOWN_DURATION = 3f;
    private bool waveInProgress = true;
    
    public EnemySpawner(Game game, int screenWidth, int screenHeight)
    {
        this.game = game;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
    }
    
    public void Update(float deltaTime)
    {
        // Handle wave cooldown
        if (!waveInProgress)
        {
            waveCooldown -= deltaTime;
            if (waveCooldown <= 0)
            {
                StartNextWave();
            }
            return;
        }
        
        // Check if wave is complete
        int activeEnemies = game.Enemies.Count;
        if (enemiesSpawnedThisWave >= enemiesPerWave && activeEnemies == 0)
        {
            waveInProgress = false;
            waveCooldown = WAVE_COOLDOWN_DURATION;
            return;
        }
        
        // Spawn enemies
        if (enemiesSpawnedThisWave < enemiesPerWave)
        {
            spawnTimer -= deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnEnemy();
                spawnTimer = spawnDelay;
                enemiesSpawnedThisWave++;
            }
        }
    }
    
    private void StartNextWave()
    {
        game.Wave++;
        waveInProgress = true;
        enemiesSpawnedThisWave = 0;
        
        // Increase difficulty each wave
        enemiesPerWave = 10 + game.Wave * 3;
        spawnDelay = Math.Max(0.5f, 2f - game.Wave * 0.1f);
    }
    
    private void SpawnEnemy()
    {
        float x = Random.Shared.Next(50, screenWidth - 50);
        var position = new Vector2(x, -30);
        
        // Determine enemy type based on wave and randomness
        EnemyType type = GetEnemyTypeForWave();
        
        var enemy = new Enemy(position, type, game);
        game.Enemies.Add(enemy);
    }
    
    private EnemyType GetEnemyTypeForWave()
    {
        int wave = game.Wave;
        float roll = Random.Shared.NextSingle();
        
        if (wave == 1)
        {
            // Wave 1: Only basic enemies
            return EnemyType.Basic;
        }
        else if (wave == 2)
        {
            // Wave 2: Basic and Fast
            return roll < 0.7f ? EnemyType.Basic : EnemyType.Fast;
        }
        else if (wave == 3)
        {
            // Wave 3: Add Zigzag
            if (roll < 0.5f) return EnemyType.Basic;
            if (roll < 0.75f) return EnemyType.Fast;
            return EnemyType.Zigzag;
        }
        else if (wave == 4)
        {
            // Wave 4: Add Shooter
            if (roll < 0.35f) return EnemyType.Basic;
            if (roll < 0.55f) return EnemyType.Fast;
            if (roll < 0.75f) return EnemyType.Zigzag;
            return EnemyType.Shooter;
        }
        else
        {
            // Wave 5+: All enemy types including Tank
            if (roll < 0.25f) return EnemyType.Basic;
            if (roll < 0.40f) return EnemyType.Fast;
            if (roll < 0.55f) return EnemyType.Zigzag;
            if (roll < 0.75f) return EnemyType.Shooter;
            return EnemyType.Tank;
        }
    }
}
