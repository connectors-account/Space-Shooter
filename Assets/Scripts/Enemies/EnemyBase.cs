using UnityEngine;

/// <summary>
/// Base class for all enemy types. Handles health, damage, scoring,
/// and power-up drops.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 30;
    [SerializeField] protected int scoreValue = 100;
    [SerializeField] protected int contactDamage = 20;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Drops")]
    [SerializeField] [Range(0f, 1f)] protected float powerUpDropChance = 0.15f;

    protected int currentHealth;
    protected bool isAlive = true;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (!isAlive) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        Move();
        Attack();
        CheckBounds();
    }

    /// <summary>
    /// Override to define enemy movement behavior.
    /// </summary>
    protected abstract void Move();

    /// <summary>
    /// Override to define enemy attack behavior.
    /// </summary>
    protected abstract void Attack();

    /// <summary>
    /// Destroy enemy if it goes off screen.
    /// </summary>
    protected virtual void CheckBounds()
    {
        if (transform.position.y < -7f || transform.position.y > 7f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (!isAlive) return;

        currentHealth -= damage;

        // Flash white on hit
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (sr != null) sr.color = original;
        }
    }

    protected virtual void Die()
    {
        isAlive = false;

        AudioManager.Instance?.PlaySFX("EnemyExplosion");
        GameManager.Instance?.AddScore(scoreValue);
        GameManager.Instance?.OnEnemyDestroyed();

        // Try to drop a power-up
        if (Random.value < powerUpDropChance)
        {
            PowerUpSpawner.Instance?.SpawnRandomPowerUp(transform.position);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
            TakeDamage(maxHealth); // destroy self on contact
        }
    }

    /// <summary>
    /// Allow spawner to scale enemy stats per wave.
    /// </summary>
    public void ScaleStats(float healthMultiplier, float speedMultiplier)
    {
        maxHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
        currentHealth = maxHealth;
        moveSpeed *= speedMultiplier;
    }
}
