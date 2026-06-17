using UnityEngine;

/// <summary>
/// Handles player ship movement (WASD / arrow keys) and shooting (Space).
/// The ship is clamped to the visible camera bounds and fires bullets from a
/// configurable muzzle point at a controlled fire rate.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second.")]
    [SerializeField] private float moveSpeed = 8f;

    [Tooltip("Extra padding (world units) kept between the ship and screen edge.")]
    [SerializeField] private float edgePadding = 0.5f;

    [Header("Shooting")]
    [Tooltip("Bullet prefab to spawn. Must contain a BulletController.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Where bullets spawn from. If empty, spawns slightly above the ship.")]
    [SerializeField] private Transform muzzle;

    [Tooltip("Minimum seconds between shots.")]
    [SerializeField] private float fireRate = 0.25f;

    [Tooltip("Collision damage dealt to this player when an enemy hits it.")]
    [SerializeField] private int collisionDamage = 20;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private float nextFireTime;
    private Vector2 moveInput;
    private float halfWidth;
    private float halfHeight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;          // Top-down style, no gravity.
        rb.freezeRotation = true;
        mainCamera = Camera.main;

        // Cache sprite half-extents so we can clamp to the screen accurately.
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            halfWidth = sr.bounds.extents.x;
            halfHeight = sr.bounds.extents.y;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            moveInput = Vector2.zero;
            return;
        }

        // Read movement input (works with WASD and arrow keys via default axes).
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Shooting.
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void FixedUpdate()
    {
        // Move using physics for smooth, frame-rate independent motion.
        Vector2 targetPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(ClampToScreen(targetPos));
    }

    /// <summary>Keep the ship within the camera's visible area.</summary>
    private Vector2 ClampToScreen(Vector2 position)
    {
        if (mainCamera == null) return position;

        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        position.x = Mathf.Clamp(position.x, min.x + halfWidth + edgePadding, max.x - halfWidth - edgePadding);
        position.y = Mathf.Clamp(position.y, min.y + halfHeight + edgePadding, max.y - halfHeight - edgePadding);
        return position;
    }

    /// <summary>Spawn a bullet travelling upward.</summary>
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = muzzle != null
            ? muzzle.position
            : transform.position + Vector3.up * (halfHeight + 0.2f);

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Configure the bullet so it travels up and only hurts enemies.
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(Vector2.up, BulletController.BulletOwner.Player);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If an enemy touches the player, take damage and destroy the enemy.
        if (other.CompareTag("Enemy"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamagePlayer(collisionDamage);
            }

            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Die(awardScore: false);
            }
        }
    }
}
