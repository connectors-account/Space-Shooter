using UnityEngine;

/// <summary>
/// Base class for all enemy types. Handles health, scoring, and destruction.
/// Subclasses override movement and attack behavior.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 50;
    public int scoreValue = 100;
    public int collisionDamage = 30;
    public float moveSpeed = 3f;

    [Header("Effects")]
    public GameObject explosionPrefab;
    public GameObject deathDropPrefab; // optional item drop
    public float dropChance = 0.05f;

    protected int currentHealth;
    protected bool isDead;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (isDead) return;
        Move();
        Attack();
        CheckBounds();
    }

    /// <summary>Override for custom movement patterns.</summary>
    protected virtual void Move()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>Override for custom attack patterns.</summary>
    protected virtual void Attack() { }

    /// <summary>Apply damage to this enemy.</summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        // Flash white briefly
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // Score
        ScoreManager.Instance?.AwardKillPoints(scoreValue);

        // Effects
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        AudioManager.Instance?.PlaySFX("EnemyExplosion");

        // Optional item drop
        if (deathDropPrefab != null && Random.value < dropChance)
        {
            Instantiate(deathDropPrefab, transform.position, Quaternion.identity);
        }

        SpawnManager.Instance?.OnEnemyDestroyed();
        Destroy(gameObject);
    }

    /// <summary>Destroy if it goes off the bottom of the screen.</summary>
    protected virtual void CheckBounds()
    {
        if (transform.position.y < -7f)
        {
            SpawnManager.Instance?.OnEnemyDestroyed();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.HandleHit(collisionDamage);
            TakeDamage(maxHealth); // destroy self on collision
        }
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        if (sr != null) sr.color = original;
    }
}
