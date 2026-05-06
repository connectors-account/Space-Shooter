using UnityEngine;

public enum EnemyType
{
    Chaser,
    ZigZag,
    Shooter
}

[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Chaser;

    [Header("Stats")]
    [SerializeField] private int baseHealth = 1;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Pattern")]
    [SerializeField] private float zigZagAmplitude = 1.4f;
    [SerializeField] private float zigZagFrequency = 2f;

    [Header("Shooter")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootInterval = 1.5f;

    private int currentHealth;
    private float spawnTime;
    private float shootTimer;
    private Transform playerTransform;

    private void Start()
    {
        spawnTime = Time.time;
        shootTimer = shootInterval;
        currentHealth = Mathf.CeilToInt(baseHealth * GameManager.Instance.GetEnemyHealthMultiplier());
        moveSpeed *= GameManager.Instance.GetEnemySpeedMultiplier();

        PlayerController player = FindObjectOfType<PlayerController>();
        playerTransform = player != null ? player.transform : null;

        gameObject.tag = "Enemy";
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        Move();

        if (enemyType == EnemyType.Shooter)
        {
            HandleShooting();
        }

        if (transform.position.y < -6.5f)
        {
            Destroy(gameObject);
        }
    }

    private void Move()
    {
        Vector3 delta = Vector3.down * moveSpeed * Time.deltaTime;

        switch (enemyType)
        {
            case EnemyType.Chaser:
                if (playerTransform != null)
                {
                    float horizontalDirection = Mathf.Sign(playerTransform.position.x - transform.position.x);
                    delta.x = horizontalDirection * moveSpeed * 0.45f * Time.deltaTime;
                }
                break;

            case EnemyType.ZigZag:
                float xOffset = Mathf.Sin((Time.time - spawnTime) * zigZagFrequency) * zigZagAmplitude;
                Vector3 target = new Vector3(xOffset, transform.position.y - moveSpeed * Time.deltaTime, 0f);
                transform.position = new Vector3(target.x, target.y, 0f);
                return;

            case EnemyType.Shooter:
                break;
        }

        transform.position += delta;
    }

    private void HandleShooting()
    {
        if (enemyBulletPrefab == null || firePoint == null)
        {
            return;
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            shootTimer = shootInterval;
            Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
            AudioManager.Instance?.PlaySfx(AudioManager.Instance.EnemyShootClip);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance?.AddScore(scoreValue);
        AudioManager.Instance?.PlaySfx(AudioManager.Instance.EnemyDeathClip);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }

            Destroy(gameObject);
        }
    }
}
