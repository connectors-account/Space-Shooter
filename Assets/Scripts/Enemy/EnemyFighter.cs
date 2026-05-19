using UnityEngine;

/// <summary>
/// Fighter enemy: actively moves toward the player and shoots frequently.
/// </summary>
public class EnemyFighter : EnemyBase
{
    [Header("Fighter Settings")]
    [SerializeField] private float chaseSpeed = 2f;
    [SerializeField] private float verticalSpeed = 1.5f;

    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        maxHealth = 40;
        scoreValue = 200;
        moveSpeed = 2.5f;
        fireRate = 1.5f;
        contactDamage = 25;
        bulletDamage = 20;
        currentHealth = maxHealth;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    protected override void Move()
    {
        // Move down and toward player
        float newY = transform.position.y - verticalSpeed * Time.deltaTime;

        float targetX = transform.position.x;
        if (playerTransform != null && !playerTransform.GetComponent<PlayerController>().IsDead)
        {
            targetX = Mathf.MoveTowards(transform.position.x, playerTransform.position.x,
                                          chaseSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(targetX, newY, transform.position.z);
    }

    protected override void Shoot()
    {
        if (bulletPrefab == null || playerTransform == null) return;

        // Aim at player
        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + (Vector3)dirToPlayer * 0.5f;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.Initialize(dirToPlayer, bulletSpeed, false, bulletDamage);
        }

        // Rotate bullet to face direction
        float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg - 90f;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }
}
