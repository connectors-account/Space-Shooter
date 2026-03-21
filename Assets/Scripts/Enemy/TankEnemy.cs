using UnityEngine;

/// <summary>
/// Tank enemy: slow, high health, fires spread shots.
/// </summary>
public class TankEnemy : EnemyBase
{
    [Header("Tank Enemy")]
    public GameObject bulletPrefab;
    public float fireRate = 3f;
    public int spreadCount = 3;
    public float spreadAngle = 30f;

    private float nextFireTime;

    protected override void Start()
    {
        base.Start();
        maxHealth = 150;
        scoreValue = 300;
        moveSpeed = 1.2f;
        currentHealth = maxHealth;
        nextFireTime = Time.time + Random.Range(1f, fireRate);
    }

    protected override void Attack()
    {
        if (Time.time < nextFireTime || bulletPrefab == null) return;
        nextFireTime = Time.time + fireRate;

        AudioManager.Instance?.PlaySFX("EnemyShoot");

        // Fire spread shot
        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadCount > 1 ? spreadAngle / (spreadCount - 1) : 0f;

        for (int i = 0; i < spreadCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.down;

            GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f,
                Quaternion.identity);
            EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
            if (eb != null) eb.SetDirection(direction);
        }
    }
}
