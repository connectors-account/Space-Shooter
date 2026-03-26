// ============================================================================
// BossEnemy.cs — Boss enemy with multiple phases
// ============================================================================
using UnityEngine;
using System.Collections;

public class BossEnemy : EnemyBase
{
    [Header("Boss Settings")]
    [SerializeField] private int phase = 1;
    [SerializeField] private float phaseTransitionHealthPercent = 0.5f;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Boss Attack")]
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private int bulletsPerAttack = 8;
    [SerializeField] private float bulletSpeedBoss = 5f;

    [Header("Boss Movement")]
    [SerializeField] private float hoverY = 3.5f;
    [SerializeField] private float sideSpeed = 2f;
    [SerializeField] private float sideAmplitude = 3f;

    private float attackTimer;
    private bool hasEnteredArena;
    private float spiralAngle;

    // =========================================================================
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 50;
        scoreValue = 5000;
        moveSpeed = 2f;
    }

    protected override void Start()
    {
        base.Start();
        phase = 1;
    }

    // =========================================================================
    protected override void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (isDead) return;

        // Enter arena
        if (!hasEnteredArena)
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
            if (transform.position.y <= hoverY)
            {
                hasEnteredArena = true;
                transform.position = new Vector3(transform.position.x, hoverY, 0);
            }
            return;
        }

        // Horizontal movement
        float x = Mathf.Sin(Time.time * sideSpeed) * sideAmplitude;
        transform.position = new Vector3(x, hoverY, 0);

        // Check phase transition
        float healthPercent = (float)currentHealth / maxHealth;
        if (phase == 1 && healthPercent <= phaseTransitionHealthPercent)
        {
            phase = 2;
            attackInterval *= 0.6f;
            bulletsPerAttack += 4;
            sideSpeed *= 1.5f;
        }

        // Attack
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            Attack();
            attackTimer = attackInterval;
        }

        CheckBounds();
    }

    // =========================================================================
    // Boss Attacks
    // =========================================================================
    private void Attack()
    {
        if (bulletPrefab == null) return;

        switch (phase)
        {
            case 1:
                // Circular burst
                FireCirclePattern();
                break;
            case 2:
                // Spiral + aimed
                StartCoroutine(SpiralAttack());
                break;
        }
    }

    private void FireCirclePattern()
    {
        float angleStep = 360f / bulletsPerAttack;
        for (int i = 0; i < bulletsPerAttack; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnBossBullet(dir);
        }
    }

    private IEnumerator SpiralAttack()
    {
        for (int i = 0; i < bulletsPerAttack; i++)
        {
            spiralAngle += 30f;
            float rad = spiralAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            SpawnBossBullet(dir);
            yield return new WaitForSeconds(0.08f);
        }

        // Also fire aimed shot at player
        GameObject player = GameManager.Instance?.PlayerShip;
        if (player != null)
        {
            Vector2 toPlayer = (player.transform.position - transform.position).normalized;
            SpawnBossBullet(toPlayer);
            SpawnBossBullet(RotateDir(toPlayer, 10f));
            SpawnBossBullet(RotateDir(toPlayer, -10f));
        }
    }

    private void SpawnBossBullet(Vector2 direction)
    {
        if (bulletPrefab == null) return;
        Vector3 pos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Initialize(direction, bulletSpeedBoss, false, 1);
        }
    }

    private Vector2 RotateDir(Vector2 d, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        return new Vector2(d.x * Mathf.Cos(r) - d.y * Mathf.Sin(r),
                           d.x * Mathf.Sin(r) + d.y * Mathf.Cos(r));
    }

    // Override to prevent normal movement
    protected override void Move() { }
}
