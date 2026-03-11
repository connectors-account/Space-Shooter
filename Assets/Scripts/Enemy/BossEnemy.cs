using UnityEngine;
using System.Collections;

public class BossEnemy : EnemyBase
{
    [Header("Boss Settings")]
    public int bossPhases = 3;
    public float phaseChangeHealthPercent = 0.33f;
    public float specialAttackCooldown = 5f;

    [Header("Boss Attack Patterns")]
    public int spreadShotCount = 5;
    public float spreadAngle = 60f;
    public int circularShotCount = 12;

    private int currentPhase = 1;
    private float nextSpecialAttackTime;
    private bool isInPosition = false;

    protected override void Awake()
    {
        base.Awake();
        enemyType = EnemyType.Boss;
    }

    public override void OnObjectSpawn()
    {
        base.OnObjectSpawn();
        currentPhase = 1;
        isInPosition = false;
        nextSpecialAttackTime = Time.time + specialAttackCooldown;
    }

    protected override void Update()
    {
        if (!isActive || GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            return;

        timeSinceSpawn += Time.deltaTime;
        HandleMovement();

        if (isInPosition)
        {
            HandleShooting();
            HandleSpecialAttacks();
        }

        UpdatePhase();
    }

    protected override void BossMovement()
    {
        if (!isInPosition)
        {
            // Move to position at top of screen
            if (transform.position.y > 3f)
            {
                transform.Translate(Vector3.down * moveSpeed * 0.5f * Time.deltaTime);
            }
            else
            {
                isInPosition = true;
            }
        }
        else
        {
            // Move side to side based on phase
            float speed = circularSpeed * (1f + (currentPhase - 1) * 0.3f);
            float range = 4f - currentPhase * 0.5f;
            float xOffset = Mathf.Sin(timeSinceSpawn * speed) * range;
            transform.position = new Vector3(xOffset, transform.position.y, 0);
        }
    }

    private void HandleSpecialAttacks()
    {
        if (Time.time >= nextSpecialAttackTime)
        {
            PerformSpecialAttack();
            nextSpecialAttackTime = Time.time + specialAttackCooldown / currentPhase;
        }
    }

    private void PerformSpecialAttack()
    {
        switch (currentPhase)
        {
            case 1:
                SpreadShot();
                break;
            case 2:
                SpreadShot();
                StartCoroutine(DelayedSpreadShot(0.3f));
                break;
            case 3:
                CircularShot();
                break;
        }
    }

    private void SpreadShot()
    {
        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (spreadShotCount - 1);

        for (int i = 0; i < spreadShotCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
            SpawnBullet(direction);
        }

        AudioManager.Instance?.PlaySFX("BossAttack");
    }

    private void CircularShot()
    {
        float angleStep = 360f / circularShotCount;

        for (int i = 0; i < circularShotCount; i++)
        {
            float angle = angleStep * i;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
            SpawnBullet(direction);
        }

        AudioManager.Instance?.PlaySFX("BossAttack");
    }

    private IEnumerator DelayedSpreadShot(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpreadShot();
    }

    private void SpawnBullet(Vector2 direction)
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;

        GameObject bullet;
        if (ObjectPooler.Instance != null)
        {
            bullet = ObjectPooler.Instance.SpawnFromPool("EnemyBullet", spawnPos, Quaternion.identity);
        }
        else if (bulletPrefab != null)
        {
            bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            return;
        }

        if (bullet != null)
        {
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.SetDirection(direction, bulletSpeed);
            }
        }
    }

    private void UpdatePhase()
    {
        if (healthSystem == null) return;

        float healthPercent = (float)healthSystem.CurrentHealth / healthSystem.maxHealth;

        if (healthPercent <= phaseChangeHealthPercent && currentPhase < 3)
        {
            currentPhase = 3;
            OnPhaseChange();
        }
        else if (healthPercent <= phaseChangeHealthPercent * 2 && currentPhase < 2)
        {
            currentPhase = 2;
            OnPhaseChange();
        }
    }

    private void OnPhaseChange()
    {
        // Visual feedback for phase change
        if (spriteRenderer != null)
        {
            Color phaseColor = currentPhase switch
            {
                2 => new Color(1f, 0.8f, 0.3f),
                3 => new Color(1f, 0.3f, 0.3f),
                _ => Color.white
            };
            spriteRenderer.color = phaseColor;
        }

        // Increase fire rate
        fireRate = fireRate * 0.8f;

        AudioManager.Instance?.PlaySFX("BossPhaseChange");
    }

    protected override void HandleDeath()
    {
        // Boss gives bonus score
        ScoreManager.Instance?.AddScore(scoreValue * 5);
        AudioManager.Instance?.PlaySFX("BossDeath");

        // Notify wave spawner that boss is defeated
        WaveSpawner.Instance?.OnBossDefeated();

        base.HandleDeath();
    }
}
