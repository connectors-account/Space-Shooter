using UnityEngine;

/// <summary>
/// Controls player movement, shooting, and health.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Combat")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootCooldown = 0.2f;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private int bulletDamage = 1;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;

    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private float shootTimer;
    private int currentHealth;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        GameManager.Instance.ReportPlayerHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(horizontal, vertical).normalized;

        shootTimer -= Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.Space) && shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        rb.velocity = inputDirection * moveSpeed;
        ClampToScreen();
    }

    private void ClampToScreen()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 viewportPosition = cam.WorldToViewportPoint(transform.position);
        viewportPosition.x = Mathf.Clamp01(viewportPosition.x);
        viewportPosition.y = Mathf.Clamp01(viewportPosition.y);
        transform.position = cam.ViewportToWorldPoint(viewportPosition);
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Player bullet prefab or fire point is not assigned.");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();

        if (bulletController != null)
        {
            bulletController.Initialize(Vector2.up, bulletSpeed, bulletDamage, true);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || GameManager.Instance.IsGameOver)
        {
            return;
        }

        currentHealth -= Mathf.Max(0, amount);
        currentHealth = Mathf.Max(0, currentHealth);

        GameManager.Instance.ReportPlayerHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.enabled = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        rb.velocity = Vector2.zero;
        GameManager.Instance.PlayerDied();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            TakeDamage(enemy.CollisionDamage);
            enemy.DestroySelf();
        }
    }
}
