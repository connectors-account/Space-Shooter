using UnityEngine;

/// <summary>
/// Boss enemy that appears at the end of waves. Has multiple attack patterns,
/// high health, and a health bar.
/// </summary>
public class EnemyBoss : EnemyBase
{
    [Header("Boss Settings")]
    public float horizontalSpeed = 2f;
    public float entrySpeed = 2f;
    public float targetY = 3.5f;

    [Header("Attack Patterns")]
    public float spreadFireRate = 0.5f;
    public float burstFireRate = 0.15f;
    public int burstCount = 5;

    private enum BossState { Entering, Fighting, Enraged }
    private BossState state = BossState.Entering;
    private float attackTimer = 0f;
    private float patternTimer = 0f;
    private float patternDuration = 5f;
    private int currentPattern = 0;
    private int burstShotsFired = 0;
    private float direction = 1f;

    // Events for UI
    public System.Action<float> OnBossHealthChanged;

    protected override void Start()
    {
        base.Start();
        maxHealth = 500;
        currentHealth = maxHealth;
        scoreValue = 1000;
        moveSpeed = horizontalSpeed;
        canShoot = false; // We handle shooting in our own patterns
    }

    protected override void Update()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.isGameActive || GameManager.Instance.isPaused))
            return;

        switch (state)
        {
            case BossState.Entering:
                EnterScreen();
                break;
            case BossState.Fighting:
            case BossState.Enraged:
                FightMovement();
                HandleAttackPatterns();
                break;
        }
    }

    private void EnterScreen()
    {
        transform.Translate(Vector3.down * entrySpeed * Time.deltaTime, Space.World);
        if (transform.position.y <= targetY)
        {
            transform.position = new Vector3(transform.position.x, targetY, 0f);
            state = BossState.Fighting;
        }
    }

    private void FightMovement()
    {
        // Move back and forth horizontally
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);

        if (GameBounds.Instance != null)
        {
            if (transform.position.x >= GameBounds.Instance.maxX - 1f)
                direction = -1f;
            else if (transform.position.x <= GameBounds.Instance.minX + 1f)
                direction = 1f;
        }
    }

    private void HandleAttackPatterns()
    {
        float rate = state == BossState.Enraged ? 0.7f : 1f;
        attackTimer -= Time.deltaTime * rate;
        patternTimer += Time.deltaTime;

        if (patternTimer >= patternDuration)
        {
            patternTimer = 0f;
            currentPattern = (currentPattern + 1) % 3;
            burstShotsFired = 0;
        }

        switch (currentPattern)
        {
            case 0: // Spread shot
                if (attackTimer <= 0f)
                {
                    SpreadShot();
                    attackTimer = spreadFireRate;
                }
                break;
            case 1: // Aimed shot at player
                if (attackTimer <= 0f)
                {
                    AimedShot();
                    attackTimer = fireRate;
                }
                break;
            case 2: // Burst fire
                if (attackTimer <= 0f && burstShotsFired < burstCount)
                {
                    BurstShot();
                    burstShotsFired++;
                    attackTimer = burstFireRate;
                    if (burstShotsFired >= burstCount)
                    {
                        burstShotsFired = 0;
                        attackTimer = 1.5f;
                    }
                }
                break;
        }
    }

    private void SpreadShot()
    {
        if (bulletPrefab == null) return;
        int bulletCount = state == BossState.Enraged ? 7 : 5;
        float angleSpread = 120f;
        float startAngle = -angleSpread / 2f;
        float angleStep = angleSpread / (bulletCount - 1);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + angleStep * i - 90f;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            SpawnBossBullet(dir);
        }
        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    private void AimedShot()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && bulletPrefab != null)
        {
            Vector2 dir = (player.transform.position - transform.position).normalized;
            SpawnBossBullet(dir);
            AudioManager.Instance?.PlaySFX("EnemyShoot");
        }
    }

    private void BurstShot()
    {
        if (bulletPrefab == null) return;
        SpawnBossBullet(Vector2.down);
        SpawnBossBullet(new Vector2(-0.2f, -1f).normalized);
        SpawnBossBullet(new Vector2(0.2f, -1f).normalized);
        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    private void SpawnBossBullet(Vector2 direction)
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.speed = 8f;
            bulletScript.damage = 15;
            bulletScript.Initialize(direction, false);
        }
    }

    public new void TakeDamage(int damage)
    {
        currentHealth -= damage;
        float healthPercent = (float)currentHealth / maxHealth;
        OnBossHealthChanged?.Invoke(healthPercent);

        // Flash on hit
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }

        // Become enraged at 30% health
        if (healthPercent <= 0.3f && state != BossState.Enraged)
        {
            state = BossState.Enraged;
            moveSpeed *= 1.5f;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            if (spriteRenderer != null)
                spriteRenderer.color = original;
        }
    }

    protected override void Die()
    {
        GameManager.Instance?.AddScore(scoreValue);
        AudioManager.Instance?.PlaySFX("BossExplosion");
        EffectsManager.Instance?.SpawnExplosion(transform.position, 3f);

        // Notify wave manager
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnBossDefeated();
        }

        Destroy(gameObject);
    }
}
