using UnityEngine;

/// <summary>
/// Handles player ship movement, shooting, and hit-flash invincibility.
/// Attach to the Player GameObject. Requires a Rigidbody2D and a PolygonCollider2D
/// tagged "Player".
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float xMin      = -8.5f;
    public float xMax      =  8.5f;
    public float yMin      = -4.8f;
    public float yMax      =  4.8f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform  firePoint;
    public float      fireRate = 0.18f;   // seconds between shots

    [Header("Invincibility after hit")]
    public float invincibleDuration = 1.5f;

    // ── Private ────────────────────────────────────────────────────────────────
    SpriteRenderer sr;
    float nextFireTime;
    float invincibleTimer;
    bool  isInvincible;

    // ── Unity ──────────────────────────────────────────────────────────────────
    void Awake() => sr = GetComponent<SpriteRenderer>();

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        Move();
        Shoot();
        TickInvincibility();
    }

    // ── Movement ───────────────────────────────────────────────────────────────
    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x + h * moveSpeed * Time.deltaTime, xMin, xMax);
        p.y = Mathf.Clamp(p.y + v * moveSpeed * Time.deltaTime, yMin, yMax);
        transform.position = p;
    }

    // ── Shooting ───────────────────────────────────────────────────────────────
    void Shoot()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            nextFireTime = Time.time + fireRate;
        }
    }

    // ── Invincibility flash ────────────────────────────────────────────────────
    void TickInvincibility()
    {
        if (!isInvincible) return;

        invincibleTimer -= Time.deltaTime;
        // Rapid flicker: visible when sine is positive
        sr.enabled = Mathf.Sin(invincibleTimer * 25f) >= 0f;

        if (invincibleTimer <= 0f)
        {
            isInvincible = false;
            sr.enabled   = true;
        }
    }

    void BeginInvincibility()
    {
        isInvincible    = true;
        invincibleTimer = invincibleDuration;
    }

    // ── Collision ──────────────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isInvincible) return;

        if (other.CompareTag("EnemyBullet") || other.CompareTag("Enemy"))
        {
            GameManager.Instance.TakeDamage(1);
            Destroy(other.gameObject);
            BeginInvincibility();
        }
    }
}
