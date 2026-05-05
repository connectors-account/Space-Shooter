using UnityEngine;

/// <summary>
/// Controls enemy movement, optional shooting, and health/death behavior.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int collisionDamage = 1;
    [SerializeField] private int scoreValue = 100;

    [Header("Optional Enemy Shooting")]
    [SerializeField] private bool canShoot = false;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootInterval = 1.5f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private int bulletDamage = 1;

    private int currentHealth;
    private float shootTimer;

    public int CollisionDamage => collisionDamage;

    private void Start()
    {
        currentHealth = maxHealth;
        shootTimer = shootInterval;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);

        if (canShoot)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootInterval;
            }
        }

        if (IsBelowScreen())
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int amount)
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        currentHealth -= Mathf.Max(0, amount);
        if (currentHealth <= 0)
        {
            GameManager.Instance.AddScore(scoreValue);
            Destroy(gameObject);
        }
    }

    public void DestroySelf()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();

        if (bulletController != null)
        {
            bulletController.Initialize(Vector2.down, bulletSpeed, bulletDamage, false);
        }
    }

    private bool IsBelowScreen()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return false;
        }

        Vector3 viewportPosition = cam.WorldToViewportPoint(transform.position);
        return viewportPosition.y < -0.1f;
    }
}
