using System;
using UnityEngine;

/// <summary>
/// Reusable health component attached to both the player and enemies.
/// Handles taking damage, healing, death, and exposes events so UI / other
/// systems can react (e.g. update a health bar or trigger game over).
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum (and starting) health value.")]
    public int maxHealth = 100;

    /// <summary>Current health. Read-only from outside.</summary>
    public int CurrentHealth { get; private set; }

    /// <summary>True while the entity still has health left.</summary>
    public bool IsAlive => CurrentHealth > 0;

    // Events: (current, max) for damage/heal; parameterless for death.
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    [Header("Shield (optional)")]
    [Tooltip("If true, all incoming damage is ignored. Used by the shield power-up.")]
    public bool shieldActive = false;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        // Notify listeners of the starting value once everything is wired up.
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>
    /// Apply damage. If a shield is active, the damage is fully absorbed.
    /// Triggers OnDeath exactly once when health crosses to zero.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (!IsAlive)
            return;

        // A shield blocks all damage while it is active.
        if (shieldActive)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth == 0)
            OnDeath?.Invoke();
    }

    /// <summary>Restore health, clamped to maxHealth (used by health power-up).</summary>
    public void Heal(int amount)
    {
        if (!IsAlive)
            return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>Reset to full health (called when (re)starting the game).</summary>
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        shieldActive = false;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>Enable or disable the damage-blocking shield.</summary>
    public void SetShield(bool active)
    {
        shieldActive = active;
    }
}
