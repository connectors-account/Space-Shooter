using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Concrete serializable UnityEvent that carries an int payload.
/// Unity cannot serialize the generic UnityEvent&lt;int&gt; directly in the
/// Inspector, so we declare this named subclass for that purpose.
/// </summary>
[Serializable]
public class IntEvent : UnityEvent<int> { }

/// <summary>
/// A reusable, generic health component that can be attached to any GameObject
/// (player, enemy, destructible obstacle, etc.).
///
/// It exposes UnityEvents so you can hook up reactions (sounds, effects, score)
/// directly in the Inspector without writing extra code:
///   - onDamaged   : invoked whenever damage is taken.
///   - onHealed    : invoked whenever health is restored.
///   - onDeath     : invoked once when health reaches zero.
///
/// This is intentionally decoupled from the GameManager so it can be reused.
/// In this project the player's health is tracked by the GameManager directly,
/// while this component is handy for enemies, bosses, or future objects.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Values")]
    [Tooltip("Maximum and starting health.")]
    public int maxHealth = 100;

    [Tooltip("If true, the GameObject is destroyed automatically on death.")]
    public bool destroyOnDeath = true;

    [Header("Events")]
    [Tooltip("Invoked when this object takes damage. Passes remaining health.")]
    public IntEvent onDamaged;

    [Tooltip("Invoked when this object is healed. Passes current health.")]
    public IntEvent onHealed;

    [Tooltip("Invoked once when this object dies.")]
    public UnityEvent onDeath;

    // Current runtime health.
    private int currentHealth;

    // Prevents death logic from running more than once.
    private bool isDead;

    /// <summary>Read-only access to the current health.</summary>
    public int CurrentHealth => currentHealth;

    /// <summary>True once health has reached zero.</summary>
    public bool IsDead => isDead;

    /// <summary>
    /// Awake initializes current health to the maximum.
    /// </summary>
    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    /// <summary>
    /// Applies damage, clamps health, fires events, and handles death.
    /// </summary>
    /// <param name="amount">Damage amount (positive number).</param>
    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        // Notify listeners of the damage.
        onDamaged?.Invoke(currentHealth);

        if (currentHealth == 0)
            Die();
    }

    /// <summary>
    /// Restores health up to the maximum and fires the healed event.
    /// </summary>
    /// <param name="amount">Healing amount (positive number).</param>
    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        onHealed?.Invoke(currentHealth);
    }

    /// <summary>
    /// Fully resets health back to maximum (e.g., on respawn).
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    /// <summary>
    /// Handles death: fires the death event and optionally destroys the object.
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        onDeath?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject);
    }
}
