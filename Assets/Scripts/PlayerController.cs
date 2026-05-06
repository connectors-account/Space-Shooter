using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Vector2 screenPadding = new Vector2(0.5f, 0.5f);

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private float lastShotTime;
    private Camera mainCamera;
    private HealthSystem healthSystem;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("PlayerController: Main Camera not found. Tag a camera as MainCamera.");
        }

        if (firePoint == null)
        {
            firePoint = transform;
            Debug.LogWarning("PlayerController: FirePoint not assigned. Using player transform.");
        }

        if (healthSystem != null)
        {
            healthSystem.OnDeath += OnPlayerDeath;
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= OnPlayerDeath;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            movementInput = Vector2.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(horizontal, vertical).normalized;

        if (Input.GetKey(KeyCode.Space))
        {
            TryShoot();
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = movementInput * moveSpeed;
        ClampToScreen();
    }

    private void TryShoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("PlayerController: Bullet prefab not assigned.");
            return;
        }

        if (Time.time < lastShotTime + fireRate)
        {
            return;
        }

        lastShotTime = Time.time;
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    private void ClampToScreen()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, min.x + screenPadding.x, max.x - screenPadding.x);
        pos.y = Mathf.Clamp(pos.y, min.y + screenPadding.y, max.y - screenPadding.y);

        transform.position = pos;
    }

    private void OnPlayerDeath()
    {
        rb.velocity = Vector2.zero;
        enabled = false;
    }
}
