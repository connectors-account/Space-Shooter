using System;
using UnityEngine;

/// <summary>
/// Reusable health component for both the player and enemies.
/// Handles damage, healing, death notification and a brief invulnerability
/// window so a single collision does not register multiple hits.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum (and starting) hit points.")]
    public int maxHealth = 100;

    [Tooltip("Seconds of invulnerability after taking damage. Set 0 to disable.")]
    public float invulnerabilityTime = 0f;

    [Tooltip("If true, this object reports the player's death to the GameManager.")]
    public bool isPlayer = false;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    // Subscribers receive (current, max) so UI bars can update.
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private float invulnerableUntil = 0f;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
    }

    private void Start()
    {
        // Broadcast initial value so UI starts correct.
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>
    /// Applies damage. Ignored while dead or invulnerable. Triggers death at 0.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        if (invulnerabilityTime > 0f && Time.time < invulnerableUntil)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (invulnerabilityTime > 0f)
        {
            invulnerableUntil = Time.time + invulnerabilityTime;
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Restores health, clamped to maxHealth.
    /// </summary>
    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>
    /// Fully resets health (used on respawn / restart).
    /// </summary>
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
        invulnerableUntil = 0f;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        OnDied?.Invoke();

        if (isPlayer && GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
