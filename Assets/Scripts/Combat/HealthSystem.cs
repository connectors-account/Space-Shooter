using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Visual Feedback")]
    public bool flashOnDamage = true;
    public Color damageFlashColor = Color.red;
    public float flashDuration = 0.1f;

    // Events
    public event Action OnDeath;
    public event Action<int> OnDamageTaken;
    public event Action<int> OnHealed;
    public event Action<int, int> OnHealthChanged; // current, max

    public int CurrentHealth => currentHealth;
    public float HealthPercent => (float)currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (flashOnDamage && !isFlashing)
        {
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        int previousHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        int actualHealed = currentHealth - previousHealth;
        if (actualHealed > 0)
        {
            OnHealed?.Invoke(actualHealed);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
    {
        maxHealth = newMaxHealth;
        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        isFlashing = true;
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    public void SetInvincible(bool invincible)
    {
        // Could be used for invincibility frames
    }
}
