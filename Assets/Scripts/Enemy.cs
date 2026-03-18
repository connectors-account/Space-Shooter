using UnityEngine;

/// <summary>
/// Enemy ship AI: moves downward toward the player, optionally shoots.
/// Attach this to the Enemy prefab.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Health")]
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Shooting")]
    public bool canShoot = true;
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 6f;
    private float nextFireTime;

    [Header("Scoring")]
    public int scoreValue = 100;

    [Header("Power-Up Drop")]
    [Tooltip("Chance (0-1) that this enemy drops a power-up on death.")]
    public float powerUpDropChance = 0.15f;

    void Start()
    {
        currentHealth = maxHealth;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        // Move downward
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        // Shoot at the player periodically
        if (canShoot && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        // Destroy if off-screen (below camera)
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Fires a bullet downward toward the player.
    /// </summary>
    void Shoot()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(Vector2.down, bulletSpeed);
            bulletScript.isPlayerBullet = false;
        }
    }

    /// <summary>
    /// Called when the enemy takes damage from a bullet.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die(true);
        }
        else
        {
            // Flash white briefly to show hit feedback
            StartCoroutine(FlashHit());
        }
    }

    System.Collections.IEnumerator FlashHit()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    /// <summary>
    /// Handles enemy death. If awardScore is true, adds score to the player.
    /// </summary>
    public void Die(bool awardScore)
    {
        if (awardScore && GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // Chance to drop a power-up
        if (awardScore && Random.value <= powerUpDropChance)
        {
            SpawnPowerUp();
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Spawns a random power-up at the enemy's position.
    /// </summary>
    void SpawnPowerUp()
    {
        if (GameManager.Instance == null) return;

        GameObject[] powerUpPrefabs = GameManager.Instance.powerUpPrefabs;
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        int index = Random.Range(0, powerUpPrefabs.Length);
        Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
    }
}
