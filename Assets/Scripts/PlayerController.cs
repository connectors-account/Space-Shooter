using UnityEngine;

/// <summary>
/// Handles player ship: keyboard movement, shooting, health and collisions.
/// Requires a Rigidbody2D (set to Kinematic) and a Collider2D on the same object.
/// If no bullet prefab is assigned, a simple one is generated at runtime.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second.")]
    public float moveSpeed = 8f;
    [Tooltip("Horizontal clamp (half screen width in world units).")]
    public float horizontalLimit = 8.5f;
    [Tooltip("Vertical clamp (half screen height in world units).")]
    public float verticalLimit = 4.5f;

    [Header("Shooting")]
    [Tooltip("Bullet prefab. If null, one is created programmatically.")]
    public GameObject bulletPrefab;
    [Tooltip("Seconds between shots.")]
    public float fireRate = 0.25f;
    [Tooltip("Speed of fired bullets.")]
    public float bulletSpeed = 12f;
    [Tooltip("Local offset from ship where bullets spawn.")]
    public Vector2 muzzleOffset = new Vector2(0f, 0.6f);

    [Header("Health")]
    public int maxHealth = 100;
    [Tooltip("Damage taken when hit by an enemy bullet.")]
    public int bulletDamage = 20;
    [Tooltip("Damage taken when colliding with an enemy ship.")]
    public int collisionDamage = 40;

    private int currentHealth;
    private float nextFireTime;

    private void Start()
    {
        currentHealth = maxHealth;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        // Only respond while the game is actively being played.
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
        {
            return;
        }

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Uses the classic Input axes (Horizontal/Vertical) mapped to WASD + arrows.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(h, v, 0f).normalized;
        Vector3 pos = transform.position + dir * moveSpeed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, -horizontalLimit, horizontalLimit);
        pos.y = Mathf.Clamp(pos.y, -verticalLimit, verticalLimit);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        // Fire on Space or left mouse button, respecting the fire rate.
        bool firePressed = Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1");
        if (firePressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        Vector3 spawnPos = transform.position + (Vector3)muzzleOffset;

        GameObject bullet;
        if (bulletPrefab != null)
        {
            bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            bullet = BulletFactory.CreateBullet(spawnPos, new Color(0.3f, 0.9f, 1f));
        }

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc == null) bc = bullet.AddComponent<BulletController>();
        bc.Initialize(Vector2.up, bulletSpeed, true, bulletDamage);
    }

    /// <summary>Apply damage to the player and update the HUD.</summary>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied();
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(bulletDamage);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamage);
            // Destroy the enemy that rammed us.
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.DestroyEnemy(false);
            }
        }
    }
}
