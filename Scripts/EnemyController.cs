using UnityEngine;

/// <summary>
/// Controls enemy movement, damage handling, and player collision behavior.
/// Attach this to each enemy prefab.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float despawnY = -7f;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private int scoreValue = 10;

    [Header("Power-Up Drop")]
    [SerializeField] private GameObject rapidFirePowerUpPrefab;
    [SerializeField] [Range(0f, 1f)] private float powerUpDropChance = 0.1f;

    private int currentHealth;
    private float speedMultiplier = 1f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (moveSpeed * speedMultiplier * Time.deltaTime), Space.World);

        if (transform.position.y < despawnY)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Configures runtime enemy scaling from wave spawner.
    /// </summary>
    public void Configure(float additionalSpeedMultiplier, int extraHealth, int extraScore)
    {
        speedMultiplier = Mathf.Max(0.1f, additionalSpeedMultiplier);
        currentHealth = Mathf.Max(1, maxHealth + extraHealth);
        scoreValue = Mathf.Max(1, scoreValue + extraScore);
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
        TryDropPowerUp();
        Destroy(gameObject);
    }

    private void TryDropPowerUp()
    {
        if (rapidFirePowerUpPrefab == null)
        {
            return;
        }

        if (Random.value <= powerUpDropChance)
        {
            Instantiate(rapidFirePowerUpPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        HealthSystem playerHealth = other.GetComponent<HealthSystem>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }

        Destroy(gameObject);
    }
}
