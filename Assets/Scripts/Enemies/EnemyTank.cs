using UnityEngine;

/// <summary>
/// Heavy enemy that moves slowly, has high health, and fires aimed shots at the player.
/// </summary>
public class EnemyTank : EnemyBase
{
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 2.5f;

    private float fireTimer;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 100;
        scoreValue = 300;
        moveSpeed = 1.5f;
        contactDamage = 40;
        powerUpDropChance = 0.35f;
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
            FireAimedShot();
            fireTimer = fireRate;
        }
    }

    private void FireAimedShot()
    {
        if (bulletPrefab == null) return;

        // Find player and aim at them
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null || !player.IsAlive) return;

        Vector2 direction = (player.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Initialize(false, 15, 5f);
        }
        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }
}
