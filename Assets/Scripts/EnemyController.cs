using UnityEngine;

/// <summary>
/// Basic enemy AI. Moves downward (optionally weaving side to side), fires at the
/// player on a timer, and collides with the player for contact damage. Awards
/// score to the GameManager when destroyed via its Health component.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Downward movement speed in units per second.")]
    public float moveSpeed = 3f;

    [Tooltip("Horizontal weave amplitude. 0 = straight down.")]
    public float weaveAmplitude = 1.5f;

    [Tooltip("How fast the enemy weaves left/right.")]
    public float weaveFrequency = 2f;

    [Header("Shooting")]
    [Tooltip("Bullet prefab (must have a BulletController). Leave null for a non-shooting enemy.")]
    public GameObject bulletPrefab;

    [Tooltip("Average seconds between shots.")]
    public float fireInterval = 2f;

    [Tooltip("Damage each enemy bullet deals.")]
    public int bulletDamage = 20;

    [Tooltip("Speed of enemy bullets.")]
    public float bulletSpeed = 7f;

    [Header("Contact")]
    [Tooltip("Damage dealt to the player on direct collision.")]
    public int contactDamage = 25;

    [Tooltip("Y position below which the enemy despawns automatically.")]
    public float despawnY = -12f;

    private float startX;
    private float spawnTime;
    private float nextFireTime;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        // Award score and notify SpawnManager when this enemy dies.
        if (health != null)
        {
            health.OnDied += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleDeath;
        }
    }

    private void Start()
    {
        startX = transform.position.x;
        spawnTime = Time.time;
        ScheduleNextShot();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
        {
            return;
        }

        HandleMovement();
        HandleShooting();

        // Auto-despawn if it flies off the bottom of the screen.
        if (transform.position.y < despawnY)
        {
            NotifySpawner();
            Destroy(gameObject);
        }
    }

    private void HandleMovement()
    {
        float elapsed = Time.time - spawnTime;
        float newX = startX + Mathf.Sin(elapsed * weaveFrequency) * weaveAmplitude;
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    private void HandleShooting()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            ScheduleNextShot();
        }
    }

    private void ScheduleNextShot()
    {
        // Add a little randomness so enemies do not fire in perfect sync.
        nextFireTime = Time.time + fireInterval * Random.Range(0.6f, 1.4f);
    }

    private void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        BulletController bullet = bulletObj.GetComponent<BulletController>();
        if (bullet != null)
        {
            // Enemy bullets travel downward.
            bullet.Initialize(Vector2.down, BulletController.Owner.Enemy, bulletDamage, bulletSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }

            // The enemy is destroyed on contact too.
            if (health != null)
            {
                health.TakeDamage(health.maxHealth);
            }
        }
    }

    private void HandleDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemyKillScore();
        }

        NotifySpawner();
        Destroy(gameObject);
    }

    private void NotifySpawner()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.NotifyEnemyRemoved();
        }
    }
}
