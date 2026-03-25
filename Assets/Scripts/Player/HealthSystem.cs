// =============================================================================
// HealthSystem.cs — Reusable health component for player and enemies
// =============================================================================
using UnityEngine;
using System;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Generic health system that can be attached to any game object.
    /// Fires events on damage, heal, and death for UI and game logic.
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 5;

        /// <summary>Current health points.</summary>
        public int CurrentHealth { get; private set; }

        /// <summary>Maximum health points.</summary>
        public int MaxHealth => maxHealth;

        /// <summary>Normalized health (0..1) for UI health bars.</summary>
        public float HealthPercent => maxHealth > 0 ? (float)CurrentHealth / maxHealth : 0f;

        /// <summary>Fired when health changes. Args: currentHealth, maxHealth.</summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>Fired when health reaches zero.</summary>
        public event Action OnDeath;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        /// <summary>
        /// Reduces health by the given amount. Clamps to zero and fires death event.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;
            CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// Increases health by the given amount, capped at maxHealth.
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        /// <summary>
        /// Resets health to maximum.
        /// </summary>
        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        /// <summary>
        /// Sets a new max health value, optionally healing to full.
        /// </summary>
        public void SetMaxHealth(int newMax, bool healToFull = false)
        {
            maxHealth = Mathf.Max(1, newMax);
            if (healToFull || CurrentHealth > maxHealth)
                CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
