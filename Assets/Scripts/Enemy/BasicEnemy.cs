using UnityEngine;

/// <summary>
/// Basic enemy: moves straight down, occasionally fires a single bullet.
/// </summary>
public class BasicEnemy : EnemyBase
{
    [Header("Basic Enemy")]
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float fireChance = 0.3f;

    private float nextFireTime;

    protected override void Start()
    {
        base.Start();
        maxHealth = 50;
        scoreValue = 100;
        moveSpeed = 2.5f;
        currentHealth = maxHealth;
        nextFireTime = Time.time + Random.Range(1f, fireRate);
    }

    protected override void Attack()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            if (Random.value < fireChance && bulletPrefab != null)
            {
                Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
                AudioManager.Instance?.PlaySFX("EnemyShoot");
            }
        }
    }
}
