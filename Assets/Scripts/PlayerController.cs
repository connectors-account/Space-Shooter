using UnityEngine;

/// <summary>
/// Handles player ship movement (keyboard / gamepad), shooting, and
/// collision with enemy bullets and power-ups.
/// Requires: Rigidbody2D, Collider2D, HealthSystem on the same GameObject.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    // ── Movement ────────────────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Screen Bounds")]
    [SerializeField] private float boundaryPadding = 0.5f;

    // ── Shooting ────────────────────────────────────────────────────────
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private AudioClip shootSound;

    // ── Power-up state ──────────────────────────────────────────────────
    public int WeaponLevel { get; private set; } = 1;
    public float FireRateMultiplier { get; set; } = 1f;
    public bool HasShield { get; set; }

    // ── Internals ───────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private HealthSystem _health;
    private AudioSource _audioSource;
    private float _nextFireTime;
    private Vector2 _velocity;
    private Vector2 _currentVelocity;
    private Camera _mainCam;
    private Vector2 _minBounds, _maxBounds;

    // ────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<HealthSystem>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        _mainCam = Camera.main;
        CalculateBounds();

        // Subscribe to death
        _health.OnDeath += HandleDeath;

        // Notify UI of initial health
        _health.OnHealthChanged?.Invoke(_health.CurrentHealth, _health.MaxHealth);
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleInput();
        HandleShooting();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        MovePlayer();
        ClampPosition();
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    // ────────────────────────────────────────────────────────────────────
    // Input
    // ────────────────────────────────────────────────────────────────────
    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _velocity = new Vector2(h, v).normalized * moveSpeed;

        // Pause
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance?.TogglePause();
    }

    private void HandleShooting()
    {
        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) &&
            Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + (fireRate / FireRateMultiplier);
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Movement
    // ────────────────────────────────────────────────────────────────────
    private void MovePlayer()
    {
        Vector2 target = _velocity;
        Vector2 smooth = Vector2.SmoothDamp(
            _rb.linearVelocity, target, ref _currentVelocity, smoothTime);
        _rb.linearVelocity = smooth;
    }

    private void ClampPosition()
    {
        Vector2 pos = _rb.position;
        pos.x = Mathf.Clamp(pos.x, _minBounds.x, _maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, _minBounds.y, _maxBounds.y);
        _rb.position = pos;
    }

    private void CalculateBounds()
    {
        if (_mainCam == null) return;
        _minBounds = _mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        _maxBounds = _mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        _minBounds += Vector2.one * boundaryPadding;
        _maxBounds -= Vector2.one * boundaryPadding;
    }

    // ────────────────────────────────────────────────────────────────────
    // Shooting
    // ────────────────────────────────────────────────────────────────────
    private void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        switch (WeaponLevel)
        {
            case 1:
                SpawnBullet(firePoint.position, Quaternion.identity);
                break;
            case 2:
                SpawnBullet(firePoint.position + Vector3.left * 0.2f, Quaternion.identity);
                SpawnBullet(firePoint.position + Vector3.right * 0.2f, Quaternion.identity);
                break;
            default: // level 3+
                SpawnBullet(firePoint.position, Quaternion.identity);
                SpawnBullet(firePoint.position, Quaternion.Euler(0, 0, 15));
                SpawnBullet(firePoint.position, Quaternion.Euler(0, 0, -15));
                break;
        }

        if (shootSound != null && _audioSource != null)
            _audioSource.PlayOneShot(shootSound, 0.5f);
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
            bc.Initialize(true);
    }

    // ────────────────────────────────────────────────────────────────────
    // Power-ups
    // ────────────────────────────────────────────────────────────────────
    public void UpgradeWeapon()
    {
        WeaponLevel = Mathf.Min(WeaponLevel + 1, 3);
    }

    public void ResetWeapon()
    {
        WeaponLevel = 1;
        FireRateMultiplier = 1f;
    }

    // ────────────────────────────────────────────────────────────────────
    // Collision
    // ────────────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            if (HasShield)
            {
                HasShield = false;
                Destroy(other.gameObject);
                return;
            }
            _health.TakeDamage(25);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            if (HasShield)
            {
                HasShield = false;
                return;
            }
            _health.TakeDamage(50);
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Death
    // ────────────────────────────────────────────────────────────────────
    private void HandleDeath()
    {
        ResetWeapon();
        GameManager.Instance?.PlayerDied();
    }
}
