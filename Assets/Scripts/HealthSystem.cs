using UnityEngine;
using System;

/// <summary>
/// Reusable health system for player and enemies.
/// Attach to any GameObject that needs health tracking.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float invincibilityDuration = 0f;

    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private bool hasShield = false;

    public event Action<int, int> OnHealthChanged;   // currentHealth, maxHealth
    public event Action OnDeath;
    public event Action OnDamageTaken;
    public event Action OnShieldBroken;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    public bool HasShield => hasShield;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }
    }

    /// <summary>
    /// Apply damage to this entity. Returns actual damage dealt.
    /// </summary>
    public int TakeDamage(int damage)
    {
        if (!IsAlive || isInvincible) return 0;

        if (hasShield)
        {
            hasShield = false;
            OnShieldBroken?.Invoke();
            ActivateInvincibility(0.5f);
            return 0;
        }

        int actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke();

        if (invincibilityDuration > 0f)
        {
            ActivateInvincibility(invincibilityDuration);
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        return actualDamage;
    }

    /// <summary>
    /// Heal this entity by the specified amount.
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Fully restore health.
    /// </summary>
    public void FullHeal()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Activate a shield that absorbs one hit.
    /// </summary>
    public void ActivateShield()
    {
        hasShield = true;
    }

    /// <summary>
    /// Make this entity invincible for a duration.
    /// </summary>
    public void ActivateInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
    }

    /// <summary>
    /// Set max health (also heals to new max if current > new max).
    /// </summary>
    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
