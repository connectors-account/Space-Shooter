using UnityEngine;

/// <summary>
/// Manages enemy health, damage reactions, and death.
/// Awards score on destruction and may drop power-ups.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 1;
    public int currentHealth;
    public float powerUpDropChance = 0.15f;
    public GameObject powerUpPrefab;

    private EnemyBase enemyBase;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        enemyBase = GetComponent<EnemyBase>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Flash white on hit
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            Invoke(nameof(ResetColor), 0.05f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void Die()
    {
        // Award score
        int score = enemyBase != null ? enemyBase.scoreValue : 100;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(score);

        // Play explosion
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayExplosion();

        // Spawn explosion effect
        Color effectColor = spriteRenderer != null ? originalColor : Color.red;
        EffectsManager.SpawnExplosion(transform.position, effectColor);

        // Chance to drop power-up
        if (powerUpPrefab != null && Random.value < powerUpDropChance)
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
