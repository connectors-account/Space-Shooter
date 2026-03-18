using UnityEngine;

/// <summary>
/// Basic enemy that flies straight down and shoots periodically.
/// </summary>
public class EnemyStraight : EnemyBase
{
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 1.5f;
    private float fireTimer;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 30;
        scoreValue = 100;
        moveSpeed = 3f;
        fireTimer = Random.Range(0.5f, fireRate);
    }

    protected override void Move()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
    }

    protected override void Attack()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireBullet();
            fireTimer = fireRate;
        }
    }

    private void FireBullet()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.Euler(0, 0, 180f));
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Initialize(false, 10, 7f);
        }
        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }
}
