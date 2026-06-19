using UnityEngine;

/// <summary>
/// Controls a single enemy ship: downward movement (with optional sine-wave
/// sway), periodic shooting, health, and rewards/power-up drops on death.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Downward speed in world units per second.")]
    public float moveSpeed = 3f;
    [Tooltip("Horizontal sway amplitude. Set 0 for straight movement.")]
    public float swayAmplitude = 0f;
    [Tooltip("How fast the sway oscillates.")]
    public float swayFrequency = 2f;

    [Header("Combat")]
    public int maxHealth = 50;
    [Tooltip("Damage dealt to the player on direct collision.")]
    public int contactDamage = 25;
    [Tooltip("Points awarded when this enemy is destroyed.")]
    public int scoreValue = 100;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    [Tooltip("Minimum seconds between shots.")]
    public float minFireInterval = 1.5f;
    [Tooltip("Maximum seconds between shots.")]
    public float maxFireInterval = 3.5f;
    public float bulletSpeed = 7f;

    [Header("Drops")]
    [Tooltip("Power-up prefab; leave null for no drops.")]
    public GameObject powerUpPrefab;
    [Range(0f, 1f)]
    [Tooltip("Chance (0-1) to drop a power-up on death.")]
    public float dropChance = 0.15f;

    private int currentHealth;
    private float fireTimer;
    private float startX;
    private float swaySeed;

    private void Start()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        swaySeed = Random.Range(0f, Mathf.PI * 2f); // desync multiple enemies
        ResetFireTimer();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        Move();
        HandleShooting();
        CheckOffScreen();
    }

    private void Move()
    {
        Vector3 pos = transform.position;
        pos.y -= moveSpeed * Time.deltaTime;

        // Optional horizontal sway based on a sine wave.
        if (swayAmplitude > 0f)
            pos.x = startX + Mathf.Sin((Time.time * swayFrequency) + swaySeed) * swayAmplitude;

        transform.position = pos;
    }

    private void HandleShooting()
    {
        if (bulletPrefab == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Fire();
            ResetFireTimer();
        }
    }

    private void Fire()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Initialize(Vector2.down, bulletSpeed, Bullet.Owner.Enemy);
    }

    private void ResetFireTimer()
    {
        fireTimer = Random.Range(minFireInterval, maxFireInterval);
    }

    /// <summary>Destroy the enemy once it travels well below the screen.</summary>
    private void CheckOffScreen()
    {
        if (Camera.main == null) return;
        float bottom = Camera.main.ViewportToWorldPoint(Vector3.zero).y;
        if (transform.position.y < bottom - 2f)
            Destroy(gameObject);
    }

    /// <summary>Apply damage and destroy the enemy when health is depleted.</summary>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die(true);
    }

    /// <summary>
    /// Remove the enemy. When <paramref name="awardScore"/> is true the player
    /// gains points and a power-up may drop (i.e. killed by a bullet).
    /// </summary>
    public void Die(bool awardScore)
    {
        if (awardScore)
        {
            GameManager.Instance?.AddScore(scoreValue);
            TryDropPowerUp();
        }
        Destroy(gameObject);
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefab != null && Random.value <= dropChance)
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
    }
}
