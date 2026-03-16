using UnityEngine;

/// <summary>
/// Small fast enemy - low health, fast movement
/// </summary>
public class SmallEnemy : Enemy
{
    protected override void OnEnable()
    {
        maxHealth = 1;
        moveSpeed = 5f;
        scoreValue = 50;
        canShoot = false;
        movementPattern = EnemyMovementPattern.Sine;
        sineAmplitude = 1.5f;
        sineFrequency = 3f;
        powerUpDropChance = 0.05f;
        base.OnEnable();
    }
}

/// <summary>
/// Medium enemy - standard enemy type
/// </summary>
public class MediumEnemy : Enemy
{
    protected override void OnEnable()
    {
        maxHealth = 2;
        moveSpeed = 3f;
        scoreValue = 100;
        canShoot = true;
        fireRate = 2.5f;
        movementPattern = EnemyMovementPattern.Straight;
        powerUpDropChance = 0.1f;
        base.OnEnable();
    }
}

/// <summary>
/// Large enemy - tank type with lots of health
/// </summary>
public class LargeEnemy : Enemy
{
    protected override void OnEnable()
    {
        maxHealth = 5;
        moveSpeed = 1.5f;
        scoreValue = 300;
        canShoot = true;
        fireRate = 1.5f;
        movementPattern = EnemyMovementPattern.ZigZag;
        sineAmplitude = 3f;
        sineFrequency = 1f;
        powerUpDropChance = 0.25f;
        base.OnEnable();
    }

    /// <summary>
    /// Large enemy fires three bullets in a spread
    /// </summary>
    protected override void Fire()
    {
        if (ObjectPooler.Instance == null) return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(shootSoundName);
        }

        // Fire spread of 3 bullets
        Vector2[] directions = new Vector2[]
        {
            new Vector2(-0.3f, -1f).normalized,
            Vector2.down,
            new Vector2(0.3f, -1f).normalized
        };

        float[] angles = new float[] { -15f, 0f, 15f };

        for (int i = 0; i < 3; i++)
        {
            GameObject bullet = ObjectPooler.Instance.SpawnFromPool(bulletPoolTag, transform.position, Quaternion.Euler(0, 0, 180 + angles[i]));
            if (bullet != null)
            {
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetDirection(directions[i]);
                }
            }
        }
    }
}

/// <summary>
/// Tracking enemy - follows the player
/// </summary>
public class TrackerEnemy : Enemy
{
    protected override void OnEnable()
    {
        maxHealth = 2;
        moveSpeed = 2f;
        scoreValue = 150;
        canShoot = true;
        fireRate = 3f;
        movementPattern = EnemyMovementPattern.Tracking;
        sineAmplitude = 4f;
        powerUpDropChance = 0.15f;
        base.OnEnable();
    }

    /// <summary>
    /// Tracker fires aimed shot at player
    /// </summary>
    protected override void Fire()
    {
        if (ObjectPooler.Instance == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            base.Fire();
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(shootSoundName);
        }

        // Calculate direction to player
        Vector2 direction = (player.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        GameObject bullet = ObjectPooler.Instance.SpawnFromPool(bulletPoolTag, transform.position, Quaternion.Euler(0, 0, angle));
        if (bullet != null)
        {
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(direction);
            }
        }
    }
}

/// <summary>
/// Boss enemy - very high health, complex patterns
/// </summary>
public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] private int phase = 1;
    private float patternTimer;
    private int patternIndex;

    protected override void OnEnable()
    {
        maxHealth = 50;
        moveSpeed = 1f;
        scoreValue = 5000;
        canShoot = true;
        fireRate = 0.5f;
        movementPattern = EnemyMovementPattern.Sine;
        sineAmplitude = 3f;
        sineFrequency = 0.5f;
        powerUpDropChance = 1f; // Always drops power-up
        base.OnEnable();
    }

    protected override void Update()
    {
        base.Update();
        UpdatePhase();
    }

    /// <summary>
    /// Update boss phase based on remaining health
    /// </summary>
    private void UpdatePhase()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        
        if (healthPercent <= 0.33f && phase < 3)
        {
            phase = 3;
            fireRate = 0.2f;
        }
        else if (healthPercent <= 0.66f && phase < 2)
        {
            phase = 2;
            fireRate = 0.35f;
        }
    }

    /// <summary>
    /// Boss has complex firing patterns
    /// </summary>
    protected override void Fire()
    {
        if (ObjectPooler.Instance == null) return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(shootSoundName);
        }

        switch (phase)
        {
            case 1:
                // Simple spread
                FireSpread(5, 30f);
                break;
            case 2:
                // Wide spread
                FireSpread(7, 45f);
                break;
            case 3:
                // Bullet hell
                FireSpread(9, 60f);
                break;
        }
    }

    /// <summary>
    /// Fire a spread of bullets
    /// </summary>
    private void FireSpread(int bulletCount, float spreadAngle)
    {
        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (bulletCount - 1);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            float radians = (angle + 270f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            GameObject bullet = ObjectPooler.Instance.SpawnFromPool(bulletPoolTag, transform.position, Quaternion.Euler(0, 0, angle + 180f));
            if (bullet != null)
            {
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetDirection(direction);
                }
            }
        }
    }

    /// <summary>
    /// Boss doesn't leave screen
    /// </summary>
    protected override void CheckBounds()
    {
        // Boss stays at top of screen, doesn't leave
        if (transform.position.y < 3f)
        {
            transform.position = new Vector3(transform.position.x, 3f, transform.position.z);
        }
    }

    /// <summary>
    /// Override die to trigger special boss death
    /// </summary>
    protected override void Die()
    {
        // Multiple explosions for boss death
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), 0);
            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.SpawnFromPool("Explosion", transform.position + randomOffset, Quaternion.identity);
            }
        }

        base.Die();
        
        // Notify wave manager boss is defeated
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnBossDefeated();
        }
    }
}
