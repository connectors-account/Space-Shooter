using UnityEngine;

/// <summary>
/// Manages the player's health. Broadcasts events so the UI and
/// GameManager can react to damage and death.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Tooltip("Maximum (and starting) health.")]
    public int maxHealth = 100;

    // Current health (publicly readable, privately settable)
    public int CurrentHealth { get; private set; }

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    /// <summary>
    /// Call this to deal damage to the player.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        // Update the UI
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Heal the player by the given amount (clamped to maxHealth).
    /// </summary>
    public void Heal(int amount)
    {
        CurrentHealth += amount;
        CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(CurrentHealth, maxHealth);
    }

    /// <summary>
    /// Handle player death.
    /// </summary>
    void Die()
    {
        // Notify GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();

        // Disable the player visually (keep object alive for reference)
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Collision with enemies deals contact damage.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(20);
            Destroy(other.gameObject); // destroy the enemy on contact

            // Award partial score for collision kill
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(5);
        }
    }
}
