using UnityEngine;

/// <summary>
/// Bomber enemy: slow, tanky, fires spread shots.
/// </summary>
public class EnemyBomber : EnemyBase
{
    [Header("Bomber Settings")]
    [SerializeField] private int spreadCount = 3;
    [SerializeField] private float spreadAngle = 30f;

    protected override void Start()
    {
        base.Start();
        maxHealth = 80;
        scoreValue = 350;
        moveSpeed = 1.5f;
        fireRate = 2.5f;
        contactDamage = 35;
        bulletDamage = 15;
        powerUpDropChance = 0.3f;
        currentHealth = maxHealth;
    }

    protected override void Shoot()
    {
        if (bulletPrefab == null) return;

        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadCount > 1 ? spreadAngle / (spreadCount - 1) : 0f;

        for (int i = 0; i < spreadCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;

            Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet bulletComp = bullet.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                bulletComp.Initialize(direction, bulletSpeed * 0.8f, false, bulletDamage);
            }

            float rotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            bullet.transform.rotation = Quaternion.Euler(0, 0, rotAngle);
        }

        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }
}
