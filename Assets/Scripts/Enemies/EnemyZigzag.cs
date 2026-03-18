using UnityEngine;

/// <summary>
/// Enemy that moves in a zigzag pattern while descending.
/// Fires bursts of bullets.
/// </summary>
public class EnemyZigzag : EnemyBase
{
    [Header("Zigzag")]
    [SerializeField] private float zigzagAmplitude = 3f;
    [SerializeField] private float zigzagFrequency = 2f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 2f;

    private float fireTimer;
    private float startX;
    private float timeOffset;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 40;
        scoreValue = 150;
        moveSpeed = 2.5f;
        powerUpDropChance = 0.2f;
        startX = transform.position.x;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
        fireTimer = Random.Range(0.5f, fireRate);
    }

    protected override void Move()
    {
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        float newX = startX + Mathf.Sin((Time.time + timeOffset) * zigzagFrequency) * zigzagAmplitude;
        transform.position = new Vector3(newX, newY, 0);
    }

    protected override void Attack()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireBurst();
            fireTimer = fireRate;
        }
    }

    private void FireBurst()
    {
        if (bulletPrefab == null) return;

        // Fire 3 bullets in a spread
        for (int i = -1; i <= 1; i++)
        {
            float angle = 180f + i * 15f;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));
            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.Initialize(false, 8, 6f);
            }
        }
        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }
}
