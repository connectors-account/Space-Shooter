// ============================================================================
// HealthSystem.cs - Reusable health component for player AND enemies
// Attach to any GameObject that needs hit points.
// ============================================================================
using System;
using UnityEngine;

/// <summary>
/// Generic health component. Fires events on damage, heal, and death
/// so other scripts can react (UI, effects, game manager, etc.).
/// </summary>
public class HealthSystem : MonoBehaviour
{
    // ---- Configuration ----
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public int maxHealth = 100;

    [Tooltip("Is this entity invincible? (e.g. during shield power-up)")]
    public bool isInvincible = false;

    // ---- Runtime state ----
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public float HealthPercent => (float)CurrentHealth / maxHealth;

    // ---- Events ----
    /// <summary>Fired when damage is taken. Passes (damageAmount, currentHealth).</summary>
    public event Action<int, int> OnDamaged;
    /// <summary>Fired when healed. Passes (healAmount, currentHealth).</summary>
    public event Action<int, int> OnHealed;
    /// <summary>Fired once when health reaches zero.</summary>
    public event Action OnDeath;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    // ========================================================================
    // Public API
    // ========================================================================

    /// <summary>Apply damage to this entity.</summary>
    public void TakeDamage(int amount)
    {
        if (IsDead || isInvincible || amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnDamaged?.Invoke(amount, CurrentHealth);

        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    /// <summary>Heal this entity (clamped to maxHealth).</summary>
    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        int before = CurrentHealth;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        int healed = CurrentHealth - before;
        if (healed > 0)
        {
            OnHealed?.Invoke(healed, CurrentHealth);
        }
    }

    /// <summary>Reset health to maximum (e.g. on respawn).</summary>
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
    }

    /// <summary>Set max health and optionally reset current to new max.</summary>
    public void SetMaxHealth(int newMax, bool resetCurrent = true)
    {
        maxHealth = newMax;
        if (resetCurrent) CurrentHealth = maxHealth;
    }
}
