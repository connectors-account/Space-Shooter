// ============================================================================
// EnemyController.cs - Enemy behavior, movement patterns, and shooting
// ============================================================================
using UnityEngine;

/// <summary>
/// Controls individual enemy ships. Supports multiple movement patterns
/// and shooting behaviors. Requires HealthSystem and CollisionHandler.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class EnemyController : MonoBehaviour
{
    // ---- Enemy Types ----
    public enum MovementPattern
    {
        StraightDown,   // Moves straight down
        Zigzag,         // Zigzags horizontally while moving down
        Sine,           // Sine wave pattern
        Dive,           // Dives toward the player
        Circle          // Circular orbit pattern
    }

    public enum ShootPattern
    {
        None,           // Doesn't shoot
        SingleForward,  // Shoots straight down
        Spread,         // Shoots 3-way spread
        Aimed           // Shoots toward the player
    }

    // ---- Configuration ----
    [Header("Movement")]
    public MovementPattern movementPattern = MovementPattern.StraightDown;
    public float moveSpeed = 3f;
    [Tooltip("Amplitude for zigzag/sine patterns")]
    public float horizontalAmplitude = 2f;
    [Tooltip("Frequency for zigzag/sine patterns")]
    public float horizontalFrequency = 2f;

    [Header("Shooting")]
    public ShootPattern shootPattern = ShootPattern.SingleForward;
    public GameObject bulletPrefab;
    public float fireRate = 1.5f;
    public float bulletSpeed = 8f;
    public int bulletDamage = 10;

    [Header("Scoring")]
    public int scoreValue = 100;

    // ---- Internal ----
    private HealthSystem _health;
    private float _nextFireTime;
    private float _spawnTime;
    private Vector3 _startPosition;
    private Transform _playerTransform;
    private float _circleAngle;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Awake()
    {
        _health = GetComponent<HealthSystem>();
    }

    private void Start()
    {
        _spawnTime = Time.time;
        _startPosition = transform.position;
        _nextFireTime = Time.time + Random.Range(0.5f, fireRate);

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;

        // Subscribe to death
        _health.OnDeath += HandleDeath;

        // Apply wave difficulty scaling
        ApplyDifficultyScaling();
    }

    private void Update()
    {
        HandleMovement();
        HandleShooting();
        CheckOffScreen();
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    // ========================================================================
    // Movement Patterns
    // ========================================================================
    private void HandleMovement()
    {
        float elapsed = Time.time - _spawnTime;

        switch (movementPattern)
        {
            case MovementPattern.StraightDown:
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                break;

            case MovementPattern.Zigzag:
                float zigzagX = Mathf.PingPong(elapsed * horizontalFrequency, 1f) * 2f - 1f;
                Vector3 zigzagMove = new Vector3(zigzagX * horizontalAmplitude * Time.deltaTime,
                                                  -moveSpeed * Time.deltaTime, 0);
                transform.Translate(zigzagMove, Space.World);
                break;

            case MovementPattern.Sine:
                float sineX = Mathf.Sin(elapsed * horizontalFrequency) * horizontalAmplitude;
                Vector3 sinePos = new Vector3(_startPosition.x + sineX,
                                               _startPosition.y - moveSpeed * elapsed, 0);
                transform.position = sinePos;
                break;

            case MovementPattern.Dive:
                if (_playerTransform != null)
                {
                    Vector3 dirToPlayer = (_playerTransform.position - transform.position).normalized;
                    transform.Translate(dirToPlayer * moveSpeed * Time.deltaTime, Space.World);
                }
                else
                {
                    transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                }
                break;

            case MovementPattern.Circle:
                _circleAngle += horizontalFrequency * Time.deltaTime;
                float cx = _startPosition.x + Mathf.Cos(_circleAngle) * horizontalAmplitude;
                float cy = _startPosition.y - moveSpeed * elapsed * 0.3f +
                           Mathf.Sin(_circleAngle) * horizontalAmplitude;
                transform.position = new Vector3(cx, cy, 0);
                break;
        }
    }

    // ========================================================================
    // Shooting
    // ========================================================================
    private void HandleShooting()
    {
        if (shootPattern == ShootPattern.None || bulletPrefab == null) return;
        if (Time.time < _nextFireTime) return;

        _nextFireTime = Time.time + fireRate;

        switch (shootPattern)
        {
            case ShootPattern.SingleForward:
                SpawnBullet(Vector2.down);
                break;

            case ShootPattern.Spread:
                SpawnBullet(Vector2.down);
                SpawnBullet(Quaternion.Euler(0, 0, 20) * Vector2.down);
                SpawnBullet(Quaternion.Euler(0, 0, -20) * Vector2.down);
                break;

            case ShootPattern.Aimed:
                if (_playerTransform != null)
                {
                    Vector2 dir = (_playerTransform.position - transform.position).normalized;
                    SpawnBullet(dir);
                }
                else
                {
                    SpawnBullet(Vector2.down);
                }
                break;
        }

        // Play enemy shoot sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("EnemyShoot");
    }

    private void SpawnBullet(Vector2 direction)
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.tag = "EnemyBullet";

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, bulletDamage);
        }
    }

    // ========================================================================
    // Difficulty Scaling
    // ========================================================================
    private void ApplyDifficultyScaling()
    {
        if (GameManager.Instance == null) return;

        moveSpeed *= GameManager.Instance.GetSpeedMultiplier();
        int scaledHealth = Mathf.RoundToInt(_health.maxHealth * GameManager.Instance.GetHealthMultiplier());
        _health.SetMaxHealth(scaledHealth, true);
    }

    // ========================================================================
    // Death & Cleanup
    // ========================================================================
    private void HandleDeath()
    {
        // Award score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            GameManager.Instance.EnemyDestroyed();
        }

        // Play explosion sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Explosion");

        // TODO: Spawn explosion particle effect here

        Destroy(gameObject);
    }

    /// <summary>Destroy if it goes too far off-screen.</summary>
    private void CheckOffScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.y < -0.2f || vp.y > 1.5f || vp.x < -0.5f || vp.x > 1.5f)
        {
            // Notify GameManager that this enemy is gone (but not "killed")
            if (GameManager.Instance != null)
                GameManager.Instance.EnemyDestroyed();
            Destroy(gameObject);
        }
    }
}
