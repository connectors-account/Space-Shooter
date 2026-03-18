// ============================================================================
// PlayerController.cs - Player ship movement, shooting, and power-up handling
// ============================================================================
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the player ship: WASD/Arrow movement, spacebar shooting,
/// weapon upgrades, shield, and speed boost integration.
/// Requires: HealthSystem, CollisionHandler, Rigidbody2D, Collider2D
/// </summary>
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ---- Movement ----
    [Header("Movement")]
    [Tooltip("Base movement speed in units/sec")]
    public float moveSpeed = 8f;

    [Tooltip("Boundary padding from screen edge")]
    public float boundaryPadding = 0.5f;

    // ---- Shooting ----
    [Header("Shooting")]
    [Tooltip("Prefab for player bullets")]
    public GameObject bulletPrefab;

    [Tooltip("Transform where bullets spawn (tip of ship)")]
    public Transform firePoint;

    [Tooltip("Seconds between shots at weapon level 1")]
    public float baseFireRate = 0.25f;

    [Tooltip("Bullet speed")]
    public float bulletSpeed = 15f;

    [Tooltip("Base bullet damage")]
    public int bulletDamage = 10;

    // ---- Weapon Upgrade ----
    [Header("Weapon Levels")]
    [Tooltip("Current weapon level (1 = single shot, 2 = double, 3 = triple spread)")]
    public int weaponLevel = 1;
    public int maxWeaponLevel = 3;

    // ---- Power-Up State ----
    [Header("Power-Up State")]
    public bool shieldActive = false;
    public float speedBoostMultiplier = 1f;

    // ---- Internal ----
    private float _nextFireTime;
    private HealthSystem _health;
    private Rigidbody2D _rb;
    private Camera _mainCam;
    private Vector2 _screenBoundsMin;
    private Vector2 _screenBoundsMax;

    // ---- Shield visual (child object) ----
    private GameObject _shieldVisual;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Awake()
    {
        _health = GetComponent<HealthSystem>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f; // no gravity in space!
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        _mainCam = Camera.main;
        CalculateScreenBounds();

        // Subscribe to death event
        _health.OnDeath += HandleDeath;

        // Create shield visual (hidden by default)
        CreateShieldVisual();
    }

    private void Update()
    {
        // Don't accept input if game isn't playing
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();
        HandleShooting();
        ClampPosition();

        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PauseGame();
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    // ========================================================================
    // Movement
    // ========================================================================
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        Vector2 direction = new Vector2(h, v).normalized;
        float currentSpeed = moveSpeed * speedBoostMultiplier;

        _rb.linearVelocity = direction * currentSpeed;
    }

    /// <summary>Keep the player within the visible screen area.</summary>
    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, _screenBoundsMin.x + boundaryPadding,
                                     _screenBoundsMax.x - boundaryPadding);
        pos.y = Mathf.Clamp(pos.y, _screenBoundsMin.y + boundaryPadding,
                                     _screenBoundsMax.y - boundaryPadding);
        transform.position = pos;
    }

    private void CalculateScreenBounds()
    {
        if (_mainCam == null) return;
        _screenBoundsMin = _mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        _screenBoundsMax = _mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));
    }

    // ========================================================================
    // Shooting
    // ========================================================================
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + GetFireRate();
        }
    }

    private void Fire()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;

        // Play shoot sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("PlayerShoot");

        switch (weaponLevel)
        {
            case 1: // Single shot straight up
                SpawnBullet(spawnPos, Vector2.up);
                break;

            case 2: // Double shot (two parallel bullets)
                SpawnBullet(spawnPos + Vector3.left * 0.25f, Vector2.up);
                SpawnBullet(spawnPos + Vector3.right * 0.25f, Vector2.up);
                break;

            case 3: // Triple spread
            default:
                SpawnBullet(spawnPos, Vector2.up);
                SpawnBullet(spawnPos, Quaternion.Euler(0, 0, 15) * Vector2.up);
                SpawnBullet(spawnPos, Quaternion.Euler(0, 0, -15) * Vector2.up);
                break;
        }
    }

    private void SpawnBullet(Vector3 position, Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        bullet.tag = "PlayerBullet";

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, bulletDamage);
        }
    }

    private float GetFireRate()
    {
        // Slightly faster fire rate at higher weapon levels
        return baseFireRate * (1f - (weaponLevel - 1) * 0.1f);
    }

    // ========================================================================
    // Power-Up Integration
    // ========================================================================

    /// <summary>Upgrade weapon level (called by PowerUpController).</summary>
    public void UpgradeWeapon()
    {
        if (weaponLevel < maxWeaponLevel)
            weaponLevel++;
    }

    /// <summary>Activate shield for a duration.</summary>
    public void ActivateShield(float duration)
    {
        StartCoroutine(ShieldCoroutine(duration));
    }

    private IEnumerator ShieldCoroutine(float duration)
    {
        shieldActive = true;
        _health.isInvincible = true;
        if (_shieldVisual != null) _shieldVisual.SetActive(true);

        yield return new WaitForSeconds(duration);

        shieldActive = false;
        _health.isInvincible = false;
        if (_shieldVisual != null) _shieldVisual.SetActive(false);
    }

    /// <summary>Activate speed boost for a duration.</summary>
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        speedBoostMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        speedBoostMultiplier = 1f;
    }

    // ========================================================================
    // Death
    // ========================================================================
    private void HandleDeath()
    {
        // Play explosion sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Explosion");

        // Notify game manager
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();

        // Disable the player visually (don't destroy, GameOver screen needs reference)
        gameObject.SetActive(false);
    }

    // ========================================================================
    // Shield Visual Helper
    // ========================================================================
    private void CreateShieldVisual()
    {
        _shieldVisual = new GameObject("ShieldVisual");
        _shieldVisual.transform.SetParent(transform);
        _shieldVisual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = _shieldVisual.AddComponent<SpriteRenderer>();
        // Create a simple circle sprite for shield
        sr.sprite = CreateCircleSprite(1.2f, new Color(0.3f, 0.7f, 1f, 0.4f));
        sr.sortingOrder = 10;

        _shieldVisual.SetActive(false);
    }

    /// <summary>Programmatically create a circle sprite for the shield effect.</summary>
    private Sprite CreateCircleSprite(float radius, Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radiusPixels = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist < radiusPixels && dist > radiusPixels - 4f)
                    tex.SetPixel(x, y, color);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / (radius * 2));
    }
}
