using UnityEngine;

/// <summary>
/// Controls the player ship: movement via WASD/Arrow keys, shooting with
/// Space, screen-bounds clamping, and the active state for power-ups
/// (rapid fire and shield). Works together with HealthSystem for damage.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units/second.")]
    public float moveSpeed = 8f;

    [Tooltip("Half-size of the playable area horizontally (clamps the ship).")]
    public float horizontalLimit = 8.5f;

    [Tooltip("Half-size of the playable area vertically.")]
    public float verticalLimit = 4.5f;

    [Header("Shooting")]
    [Tooltip("The bullet pool used to fire projectiles.")]
    public BulletPool bulletPool;

    [Tooltip("Spawn point for bullets. If null, the ship's position is used.")]
    public Transform firePoint;

    [Tooltip("Damage each player bullet deals.")]
    public int bulletDamage = 25;

    [Tooltip("Normal seconds between shots.")]
    public float fireCooldown = 0.25f;

    [Tooltip("Fire cooldown while the rapid-fire power-up is active.")]
    public float rapidFireCooldown = 0.08f;

    [Header("Power-up Visuals (optional)")]
    [Tooltip("A child GameObject (e.g. a glowing ring) toggled while shielded.")]
    public GameObject shieldVisual;

    // Runtime state
    private HealthSystem health;
    private float lastFireTime;
    private bool rapidFireActive;
    private float rapidFireEndTime;
    private float shieldEndTime;

    private void Awake()
    {
        health = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        // Subscribe to our own death so we can tell the GameManager.
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        // Only respond to input while actually playing.
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();
        HandleShooting();
        HandlePowerUpTimers();
    }

    /// <summary>Read input axes and move the ship, clamped to the screen.</summary>
    private void HandleMovement()
    {
        // GetAxisRaw covers both WASD and Arrow keys by default in Unity.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, v, 0f).normalized * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + move;

        // Keep the ship inside the visible play area.
        newPos.x = Mathf.Clamp(newPos.x, -horizontalLimit, horizontalLimit);
        newPos.y = Mathf.Clamp(newPos.y, -verticalLimit, verticalLimit);

        transform.position = newPos;
    }

    /// <summary>Fire a bullet on Space, respecting the current cooldown.</summary>
    private void HandleShooting()
    {
        float cooldown = rapidFireActive ? rapidFireCooldown : fireCooldown;

        if (Input.GetKey(KeyCode.Space) && Time.time - lastFireTime >= cooldown)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    private void Shoot()
    {
        if (bulletPool == null)
            return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        // Player bullets travel up and damage enemies.
        bulletPool.GetBullet(spawnPos, Vector2.up, "Enemy", bulletDamage);
    }

    /// <summary>Expire rapid-fire and shield power-ups when their timers run out.</summary>
    private void HandlePowerUpTimers()
    {
        if (rapidFireActive && Time.time >= rapidFireEndTime)
            rapidFireActive = false;

        if (health.shieldActive && Time.time >= shieldEndTime)
        {
            health.SetShield(false);
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
        }
    }

    // -------- Power-up activation (called by PowerUp.cs) --------

    /// <summary>Heal the player by a flat amount.</summary>
    public void ApplyHealth(int amount)
    {
        health.Heal(amount);
    }

    /// <summary>Enable rapid fire for the given duration (refreshes if re-collected).</summary>
    public void ApplyRapidFire(float duration)
    {
        rapidFireActive = true;
        rapidFireEndTime = Time.time + duration;
    }

    /// <summary>Enable an invulnerability shield for the given duration.</summary>
    public void ApplyShield(float duration)
    {
        health.SetShield(true);
        shieldEndTime = Time.time + duration;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);
    }

    /// <summary>Reset all transient state at the start of a new game.</summary>
    public void ResetState()
    {
        rapidFireActive = false;
        lastFireTime = 0f;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    /// <summary>Player died -> notify the GameManager to end the game.</summary>
    private void HandleDeath()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EndGame();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Direct collision with an enemy ship damages the player.
        if (other.CompareTag("Enemy"))
        {
            health.TakeDamage(34); // ~3 hits from full health
            // Also damage/destroy the enemy on the crash.
            var enemyHealth = other.GetComponent<HealthSystem>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(9999);
        }
    }
}
