using UnityEngine;

/// <summary>
/// Controls the player ship:
///   - Movement using WASD or Arrow keys (via Unity's Input axes).
///   - Shooting bullets with the Spacebar, with a configurable fire rate.
///   - Reacts to collisions with enemies / enemy bullets by losing health.
/// Requires a Rigidbody2D for physics-based movement and a Collider2D
/// (set as a trigger) for hit detection.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 8f;

    [Header("Shooting")]
    [Tooltip("Bullet prefab to spawn when firing.")]
    public GameObject bulletPrefab;

    [Tooltip("Point from which bullets are spawned (usually in front of the ship).")]
    public Transform firePoint;

    [Tooltip("Minimum time (seconds) between shots.")]
    public float fireCooldown = 0.25f;

    [Header("Collision Damage")]
    [Tooltip("Damage the player takes when hit by an enemy or enemy bullet.")]
    public int collisionDamage = 20;

    // Cached references and runtime state.
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float nextFireTime;

    /// <summary>
    /// Awake caches the Rigidbody2D reference and configures it for top-down play.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // A space shooter has no gravity; movement is fully controlled by input.
        rb.gravityScale = 0f;
        // Prevent the ship from spinning when it collides with things.
        rb.freezeRotation = true;
    }

    /// <summary>
    /// Update reads player input every frame (input must be polled in Update,
    /// not FixedUpdate, to avoid missing key presses).
    /// </summary>
    private void Update()
    {
        // Do nothing if the game is not actively being played.
        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameManager.GameState.Playing)
        {
            moveInput = Vector2.zero;
            return;
        }

        // GetAxisRaw returns -1, 0, or 1 with no smoothing, giving snappy controls.
        // "Horizontal" = A/D or Left/Right arrows. "Vertical" = W/S or Up/Down arrows.
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Normalize so diagonal movement isn't faster than straight movement.
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        // Fire when Spacebar is held and the cooldown has elapsed.
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    /// <summary>
    /// FixedUpdate applies physics-based movement at a fixed timestep.
    /// </summary>
    private void FixedUpdate()
    {
        // MovePosition gives smooth, collision-aware movement.
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Spawns a bullet at the fire point.
    /// </summary>
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        // Use the fire point if assigned; otherwise spawn at the ship's position.
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Tell the bullet it belongs to the player so it only hits enemies.
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.SetOwner(Bullet.Owner.Player);
    }

    /// <summary>
    /// Trigger-based collision handling. The player's collider should be a trigger.
    /// We detect enemies and enemy bullets here.
    /// </summary>
    /// <param name="other">The collider we touched.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit by an enemy ship directly.
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamage);

            // Destroy the enemy on contact so it doesn't keep dealing damage.
            Destroy(other.gameObject);
        }
        // Hit by an enemy bullet.
        else if (other.CompareTag("EnemyBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            int dmg = bullet != null ? bullet.damage : collisionDamage;
            TakeDamage(dmg);

            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// Routes damage through the GameManager which owns the player's health value.
    /// </summary>
    /// <param name="amount">Damage amount.</param>
    private void TakeDamage(int amount)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.DamagePlayer(amount);
    }
}
