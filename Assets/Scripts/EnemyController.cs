using UnityEngine;

/// <summary>
/// Enemy AI controller. Supports multiple movement patterns and shooting.
/// Requires: Rigidbody2D, Collider2D, HealthSystem.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class EnemyController : MonoBehaviour
{
    // ── Movement Patterns ───────────────────────────────────────────────
    public enum MovementPattern { StraightDown, Zigzag, Sine, Dive }

    [Header("Movement")]
    [SerializeField] private MovementPattern pattern = MovementPattern.StraightDown;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float zigzagAmplitude = 3f;
    [SerializeField] private float zigzagFrequency = 2f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float fireRateVariance = 0.5f;
    [SerializeField] private bool canShoot = true;

    [Header("Scoring")]
    [SerializeField] private int scoreValue = 100;

    [Header("Drops")]
    [SerializeField] private GameObject[] possibleDrops;
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.15f;

    // ── Internals ───────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private HealthSystem _health;
    private float _nextFireTime;
    private float _spawnX;
    private float _aliveTime;
    private Transform _playerTransform;

    // ────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<HealthSystem>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        _spawnX = transform.position.x;
        _health.OnDeath += HandleDeath;

        // Randomize first shot
        _nextFireTime = Time.time + Random.Range(0.5f, fireRate);

        // Cache player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;

        // Set tag
        gameObject.tag = "Enemy";
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        _aliveTime += Time.deltaTime;

        if (canShoot && Time.time >= _nextFireTime)
            Shoot();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
        DestroyIfOutOfBounds();
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    // ────────────────────────────────────────────────────────────────────
    // Movement
    // ────────────────────────────────────────────────────────────────────
    private void Move()
    {
        Vector2 vel = Vector2.zero;

        switch (pattern)
        {
            case MovementPattern.StraightDown:
                vel = Vector2.down * moveSpeed;
                break;

            case MovementPattern.Zigzag:
                float xOffset = Mathf.Sin(_aliveTime * zigzagFrequency) * zigzagAmplitude;
                vel = new Vector2(xOffset, -moveSpeed);
                break;

            case MovementPattern.Sine:
                float sineX = Mathf.Cos(_aliveTime * zigzagFrequency) * zigzagAmplitude;
                vel = new Vector2(sineX, -moveSpeed);
                break;

            case MovementPattern.Dive:
                if (_playerTransform != null)
                {
                    Vector2 dir = ((Vector2)_playerTransform.position - _rb.position).normalized;
                    vel = dir * moveSpeed * 1.2f;
                }
                else
                {
                    vel = Vector2.down * moveSpeed;
                }
                break;
        }

        _rb.linearVelocity = vel;
    }

    // ────────────────────────────────────────────────────────────────────
    // Shooting
    // ────────────────────────────────────────────────────────────────────
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos,
            Quaternion.Euler(0, 0, 180)); // face downward

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
            bc.Initialize(false);

        _nextFireTime = Time.time + fireRate + Random.Range(-fireRateVariance, fireRateVariance);
    }

    // ────────────────────────────────────────────────────────────────────
    // Death
    // ────────────────────────────────────────────────────────────────────
    private void HandleDeath()
    {
        GameManager.Instance?.AddScore(scoreValue);
        TryDropItem();
    }

    private void TryDropItem()
    {
        if (possibleDrops == null || possibleDrops.Length == 0) return;
        if (Random.value > dropChance) return;

        int idx = Random.Range(0, possibleDrops.Length);
        if (possibleDrops[idx] != null)
            Instantiate(possibleDrops[idx], transform.position, Quaternion.identity);
    }

    // ────────────────────────────────────────────────────────────────────
    // Bounds
    // ────────────────────────────────────────────────────────────────────
    private void DestroyIfOutOfBounds()
    {
        if (transform.position.y < -8f || transform.position.y > 12f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Configuration (called by EnemySpawner)
    // ────────────────────────────────────────────────────────────────────
    public void Configure(MovementPattern newPattern, float speedMultiplier)
    {
        pattern = newPattern;
        moveSpeed *= speedMultiplier;
    }
}
