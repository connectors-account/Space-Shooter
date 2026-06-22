using UnityEngine;

/// <summary>
/// Enemy ship behaviour. Moves downward toward the player, optionally drifts
/// sideways in a sine wave, periodically shoots, awards score on death, and
/// has a chance to drop a power-up. Relies on HealthSystem for damage/death.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Downward speed in units/second.")]
    public float moveSpeed = 2.5f;

    [Tooltip("Horizontal sine-wave amplitude (0 = straight down).")]
    public float weaveAmplitude = 1.5f;

    [Tooltip("How fast the sine weave oscillates.")]
    public float weaveFrequency = 2f;

    [Tooltip("Y position below which the enemy despawns (off bottom of screen).")]
    public float despawnY = -6f;

    [Header("Combat")]
    [Tooltip("Points awarded to the player when this enemy is destroyed.")]
    public int scoreValue = 100;

    [Tooltip("Bullet pool for enemy fire. Optional; if null the enemy won't shoot.")]
    public BulletPool bulletPool;

    [Tooltip("Average seconds between shots (randomized a bit). 0 = never shoots.")]
    public float fireInterval = 0f;

    [Tooltip("Damage each enemy bullet deals to the player.")]
    public int bulletDamage = 15;

    [Header("Power-up Drop")]
    [Tooltip("0-1 chance to drop a power-up on death.")]
    [Range(0f, 1f)]
    public float dropChance = 0.2f;

    [Tooltip("Power-up prefab to drop (optional).")]
    public GameObject powerUpPrefab;

    private HealthSystem health;
    private float startX;
    private float spawnTime;
    private float nextFireTime;

    private void Awake()
    {
        health = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
    }

    private void Start()
    {
        startX = transform.position.x;
        spawnTime = Time.time;
        ScheduleNextShot();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        MoveDown();
        TryShoot();

        // Remove the enemy once it travels off the bottom of the screen.
        if (transform.position.y < despawnY)
            Destroy(gameObject);
    }

    /// <summary>Move straight down with an optional sideways sine weave.</summary>
    private void MoveDown()
    {
        float elapsed = Time.time - spawnTime;
        float x = startX + Mathf.Sin(elapsed * weaveFrequency) * weaveAmplitude;
        float y = transform.position.y - moveSpeed * Time.deltaTime;
        transform.position = new Vector3(x, y, transform.position.z);
    }

    /// <summary>Fire downward at the player when the timer elapses.</summary>
    private void TryShoot()
    {
        if (fireInterval <= 0f || bulletPool == null)
            return;

        if (Time.time >= nextFireTime)
        {
            bulletPool.GetBullet(transform.position, Vector2.down, "Player", bulletDamage);
            ScheduleNextShot();
        }
    }

    private void ScheduleNextShot()
    {
        if (fireInterval > 0f)
            // Randomize +-30% so enemies don't all fire in sync.
            nextFireTime = Time.time + fireInterval * Random.Range(0.7f, 1.3f);
    }

    /// <summary>Award score, maybe drop a power-up, then remove the enemy.</summary>
    private void HandleDeath()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(scoreValue);

        TryDropPowerUp();

        Destroy(gameObject);
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefab != null && Random.value <= dropChance)
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
        }
    }
}
