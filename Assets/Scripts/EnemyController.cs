using UnityEngine;

/// <summary>
/// Controls enemy movement pattern, shooting, and health.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public enum MovementPattern
    {
        Straight,
        ZigZag,
        SineWave
    }

    [Header("Stats")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int contactDamage = 20;
    [SerializeField] private int scoreValue = 100;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private MovementPattern movementPattern = MovementPattern.Straight;
    [SerializeField] private float waveFrequency = 3f;
    [SerializeField] private float waveAmplitude = 1.2f;

    [Header("Shooting")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootInterval = 1.5f;
    [SerializeField] private float bulletSpeed = 7f;

    private int currentHealth;
    private float shootTimer;
    private float spawnX;
    private float elapsed;

    public int ContactDamage => contactDamage;

    public void ConfigureForWave(float speedMultiplier, float shootIntervalMultiplier, MovementPattern pattern)
    {
        moveSpeed *= Mathf.Max(0.1f, speedMultiplier);
        shootInterval = Mathf.Max(0.25f, shootInterval * shootIntervalMultiplier);
        movementPattern = pattern;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        spawnX = transform.position.x;
        shootTimer = Random.Range(0.2f, shootInterval);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
        {
            return;
        }

        elapsed += Time.deltaTime;
        Move();
        TryShoot();
        CheckDespawn();
    }

    private void Move()
    {
        Vector3 pos = transform.position;
        pos.y -= moveSpeed * Time.deltaTime;

        switch (movementPattern)
        {
            case MovementPattern.ZigZag:
                pos.x += Mathf.Sin(elapsed * waveFrequency) * waveAmplitude * Time.deltaTime;
                break;
            case MovementPattern.SineWave:
                pos.x = spawnX + Mathf.Sin(elapsed * waveFrequency) * waveAmplitude;
                break;
        }

        transform.position = pos;
    }

    private void TryShoot()
    {
        if (enemyBulletPrefab == null || firePoint == null)
        {
            return;
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f)
        {
            return;
        }

        shootTimer = shootInterval;
        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);

        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Configure(false, bulletSpeed, 12);
        }
    }

    private void CheckDespawn()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 bottom = cam.ViewportToWorldPoint(new Vector3(0f, -0.1f, cam.nearClipPlane));
        if (transform.position.y < bottom.y)
        {
            SpawnManager.Instance?.ReportEnemyDestroyed(this, false);
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance?.AddScore(scoreValue);
        SpawnManager.Instance?.ReportEnemyDestroyed(this, true);
        Destroy(gameObject);
    }
}
