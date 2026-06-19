using UnityEngine;

/// <summary>
/// Handles player ship movement (WASD / arrow keys), screen clamping and
/// shooting (spacebar). Also manages the rapid-fire and shield power-up states.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second.")]
    public float moveSpeed = 8f;

    [Header("Shooting")]
    [Tooltip("Bullet prefab fired by the player.")]
    public GameObject bulletPrefab;
    [Tooltip("Where bullets spawn (usually a child at the ship's nose).")]
    public Transform firePoint;
    [Tooltip("Seconds between shots under normal fire.")]
    public float fireCooldown = 0.25f;
    [Tooltip("Speed given to spawned bullets.")]
    public float bulletSpeed = 12f;

    [Header("Power-up Tuning")]
    [Tooltip("Fire cooldown while rapid-fire is active.")]
    public float rapidFireCooldown = 0.08f;
    [Tooltip("How long rapid-fire lasts (seconds).")]
    public float rapidFireDuration = 6f;
    [Tooltip("How long the shield lasts (seconds).")]
    public float shieldDuration = 6f;

    [Header("Power-up Visuals (optional)")]
    [Tooltip("Child object enabled while the shield is active.")]
    public GameObject shieldVisual;

    // Runtime state
    private float fireTimer;
    private float rapidFireTimer;
    private float shieldTimer;
    private bool shieldActive;
    private Camera mainCamera;
    private Vector2 shipHalfSize;

    public bool ShieldActive => shieldActive;

    private void Start()
    {
        mainCamera = Camera.main;

        // Cache half the sprite size so we can keep the ship fully on-screen.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        shipHalfSize = sr != null ? sr.bounds.extents : new Vector2(0.5f, 0.5f);

        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        HandleMovement();
        HandleShooting();
        UpdatePowerUpTimers();
    }

    /// <summary>Read input and move the ship, clamping to the camera view.</summary>
    private void HandleMovement()
    {
        // GetAxisRaw covers both WASD and arrow keys via Unity's default input.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(h, v, 0f).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        ClampToScreen();
    }

    /// <summary>Keep the ship inside the visible camera bounds.</summary>
    private void ClampToScreen()
    {
        if (mainCamera == null) return;

        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, min.x + shipHalfSize.x, max.x - shipHalfSize.x);
        p.y = Mathf.Clamp(p.y, min.y + shipHalfSize.y, max.y - shipHalfSize.y);
        transform.position = p;
    }

    /// <summary>Fire bullets on spacebar respecting the current cooldown.</summary>
    private void HandleShooting()
    {
        fireTimer -= Time.deltaTime;

        bool rapid = rapidFireTimer > 0f;
        float cooldown = rapid ? rapidFireCooldown : fireCooldown;

        if (Input.GetKey(KeyCode.Space) && fireTimer <= 0f && bulletPrefab != null)
        {
            Fire();
            fireTimer = cooldown;
        }
    }

    private void Fire()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            // Player bullets travel up and only hit enemies.
            bullet.Initialize(Vector2.up, bulletSpeed, Bullet.Owner.Player);
        }
    }

    /// <summary>Count down active power-up timers each frame.</summary>
    private void UpdatePowerUpTimers()
    {
        if (rapidFireTimer > 0f) rapidFireTimer -= Time.deltaTime;

        if (shieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                shieldActive = false;
                if (shieldVisual != null) shieldVisual.SetActive(false);
            }
        }
    }

    /// <summary>Called by a power-up pickup to enable rapid fire.</summary>
    public void ActivateRapidFire()
    {
        rapidFireTimer = rapidFireDuration;
    }

    /// <summary>Called by a power-up pickup to enable the shield.</summary>
    public void ActivateShield()
    {
        shieldActive = true;
        shieldTimer = shieldDuration;
        if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    /// <summary>
    /// Apply damage to the player. Returns true if damage was absorbed by the
    /// shield (so the caller knows the hit was blocked).
    /// </summary>
    public bool TakeDamage(int amount)
    {
        if (shieldActive)
            return true; // shield absorbs the hit, no health lost

        GameManager.Instance?.DamagePlayer(amount);
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Direct collision with an enemy ship damages the player.
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            int contactDamage = enemy != null ? enemy.contactDamage : 25;

            bool blocked = TakeDamage(contactDamage);
            // The enemy is destroyed on contact regardless of the shield.
            if (enemy != null) enemy.Die(false);
        }
    }
}
