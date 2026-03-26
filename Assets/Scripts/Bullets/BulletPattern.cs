// ============================================================================
// BulletPattern.cs — Configurable bullet patterns for enemies
// ============================================================================
using UnityEngine;

public enum PatternType
{
    Straight,
    Aimed,       // aims at player
    Spread,      // fan of bullets
    Circle,      // ring of bullets
    Spiral,      // rotating spiral
    Burst        // rapid burst
}

[System.Serializable]
public class BulletPatternConfig
{
    public PatternType type = PatternType.Straight;
    public GameObject bulletPrefab;
    public float bulletSpeed = 6f;
    public int bulletCount = 1;
    public float spreadAngle = 30f;
    public float fireRate = 1f;
    public int burstCount = 3;
    public float burstDelay = 0.1f;
    public int damage = 1;
    [Range(0f, 1f)] public float accuracy = 0.8f; // for aimed shots: 1 = perfect
}

public class BulletPattern : MonoBehaviour
{
    [SerializeField] private BulletPatternConfig config;
    [SerializeField] private Transform firePoint;

    private float nextFireTime;
    private float spiralAngle;
    private int burstShotsFired;
    private float burstTimer;
    private bool isBursting;

    // =========================================================================
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (config.bulletPrefab == null) return;

        if (isBursting)
        {
            HandleBurst();
            return;
        }

        if (Time.time >= nextFireTime)
        {
            ExecutePattern();
            nextFireTime = Time.time + config.fireRate;
        }
    }

    // =========================================================================
    // Pattern Execution
    // =========================================================================
    private void ExecutePattern()
    {
        switch (config.type)
        {
            case PatternType.Straight:
                FireStraight();
                break;
            case PatternType.Aimed:
                FireAimed();
                break;
            case PatternType.Spread:
                FireSpread();
                break;
            case PatternType.Circle:
                FireCircle();
                break;
            case PatternType.Spiral:
                FireSpiral();
                break;
            case PatternType.Burst:
                StartBurst();
                break;
        }
    }

    // =========================================================================
    // Pattern Implementations
    // =========================================================================
    private void FireStraight()
    {
        SpawnBullet(Vector2.down);
    }

    private void FireAimed()
    {
        GameObject player = GameManager.Instance?.PlayerShip;
        if (player == null)
        {
            FireStraight();
            return;
        }

        Vector2 toPlayer = (player.transform.position - GetFirePos()).normalized;

        // Add inaccuracy
        float inaccuracy = (1f - config.accuracy) * 30f;
        float angleOffset = Random.Range(-inaccuracy, inaccuracy);
        toPlayer = RotateDir(toPlayer, angleOffset);

        SpawnBullet(toPlayer);
    }

    private void FireSpread()
    {
        float startAngle = -config.spreadAngle / 2f;
        float step = config.bulletCount > 1 ? config.spreadAngle / (config.bulletCount - 1) : 0;

        for (int i = 0; i < config.bulletCount; i++)
        {
            float angle = startAngle + step * i;
            Vector2 dir = RotateDir(Vector2.down, angle);
            SpawnBullet(dir);
        }
    }

    private void FireCircle()
    {
        float step = 360f / config.bulletCount;
        for (int i = 0; i < config.bulletCount; i++)
        {
            float angle = step * i;
            Vector2 dir = RotateDir(Vector2.down, angle);
            SpawnBullet(dir);
        }
    }

    private void FireSpiral()
    {
        Vector2 dir = RotateDir(Vector2.down, spiralAngle);
        SpawnBullet(dir);
        spiralAngle += 25f; // rotate each shot
        if (spiralAngle >= 360f) spiralAngle -= 360f;
    }

    private void StartBurst()
    {
        isBursting = true;
        burstShotsFired = 0;
        burstTimer = 0;
    }

    private void HandleBurst()
    {
        burstTimer -= Time.deltaTime;
        if (burstTimer <= 0)
        {
            FireAimed(); // Burst fires aimed shots
            burstShotsFired++;
            burstTimer = config.burstDelay;

            if (burstShotsFired >= config.burstCount)
            {
                isBursting = false;
            }
        }
    }

    // =========================================================================
    // Helpers
    // =========================================================================
    private void SpawnBullet(Vector2 direction)
    {
        Vector3 pos = GetFirePos();
        GameObject bullet = Instantiate(config.bulletPrefab, pos, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Initialize(direction, config.bulletSpeed, false, config.damage);
        }
    }

    private Vector3 GetFirePos()
    {
        return firePoint != null ? firePoint.position : transform.position;
    }

    private Vector2 RotateDir(Vector2 dir, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(
            dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
            dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad)
        );
    }

    // =========================================================================
    // Public configuration
    // =========================================================================
    public void SetConfig(BulletPatternConfig newConfig)
    {
        config = newConfig;
    }

    public void SetFireRate(float rate)
    {
        config.fireRate = rate;
    }
}
