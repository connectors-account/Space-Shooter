using UnityEngine;

/// <summary>
/// Controls an enemy ship:
///   - Moves steadily downward toward the player's side of the screen.
///   - Periodically fires bullets downward at the player.
///   - Has its own health and awards score to the player when destroyed.
/// Requires a Rigidbody2D and a trigger Collider2D. The GameObject must be
/// tagged "Enemy" so player bullets can detect it.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Downward movement speed in units per second.")]
    public float moveSpeed = 2.5f;

    [Header("Health")]
    [Tooltip("Hit points before the enemy is destroyed.")]
    public int maxHealth = 50;

    [Header("Scoring")]
    [Tooltip("Points awarded to the player when this enemy is destroyed.")]
    public int scoreValue = 10;

    [Header("Shooting")]
    [Tooltip("Bullet prefab fired at the player. Leave empty for a non-shooting enemy.")]
    public GameObject bulletPrefab;

    [Tooltip("Point from which bullets spawn (usually below the enemy).")]
    public Transform firePoint;

    [Tooltip("Average seconds between shots.")]
    public float fireInterval = 2f;

    [Tooltip("Random variation added/subtracted from the fire interval.")]
    public float fireIntervalVariance = 0.75f;

    // Runtime state.
    private int currentHealth;
    private Rigidbody2D rb;
    private float nextFireTime;

    /// <summary>
    /// Awake caches references and initializes health.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Start sets the downward velocity and schedules the first shot.
    /// </summary>
    private void Start()
    {
        // Continuous downward drift.
        rb.velocity = new Vector2(0f, -moveSpeed);

        // Stagger the first shot so enemies don't all fire simultaneously.
        ScheduleNextShot();
    }

    /// <summary>
    /// Update handles the shooting timer.
    /// </summary>
    private void Update()
    {
        // Only shoot while the game is being played.
        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        if (bulletPrefab != null && Time.time >= nextFireTime)
        {
            Shoot();
            ScheduleNextShot();
        }
    }

    /// <summary>
    /// Picks a randomized time for the next shot.
    /// </summary>
    private void ScheduleNextShot()
    {
        float variance = Random.Range(-fireIntervalVariance, fireIntervalVariance);
        nextFireTime = Time.time + Mathf.Max(0.1f, fireInterval + variance);
    }

    /// <summary>
    /// Spawns an enemy bullet aimed downward.
    /// </summary>
    private void Shoot()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Mark the bullet as enemy-owned so it travels down and hits the player.
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.SetOwner(Bullet.Owner.Enemy);
    }

    /// <summary>
    /// Applies damage to the enemy. Destroys it and awards score at zero health.
    /// Called by the player's Bullet script.
    /// </summary>
    /// <param name="amount">Damage to apply.</param>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Awards score and removes the enemy from the scene.
    /// </summary>
    private void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        Destroy(gameObject);
    }
}
