using UnityEngine;
using System;

/// <summary>
/// Reusable health component for any damageable entity (player, enemies).
/// Attach to any GameObject that can take damage.
/// </summary>
public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public bool isInvulnerable;

    [Header("Visual Feedback")]
    public float flashDuration = 0.1f;
    public Color damageColor = Color.red;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public int CurrentHealth => currentHealth;
    public float HealthPercent => (float)currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action<int, int> OnHealthChanged;   // current, max
    public event Action OnDeath;
    public event Action<int> OnDamageTaken;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    /// <summary>Apply damage to this entity.</summary>
    public void TakeDamage(int damage)
    {
        if (isInvulnerable || IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke(damage);

        if (spriteRenderer != null)
        {
            StartCoroutine(FlashDamage());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>Heal the entity.</summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>Fully restore health.</summary>
    public void FullHeal()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    /// <summary>Reset health to max (used on respawn).</summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvulnerable = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
