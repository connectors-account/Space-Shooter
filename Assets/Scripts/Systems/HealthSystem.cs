using System;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Reusable health component for both the player and enemies.
    /// Tracks current/max health, handles damage, healing, shields and death.
    /// Exposes C# events so UI and other systems can react without tight coupling.
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool isInvulnerable = false;

        private float currentHealth;
        private bool shieldActive = false;
        private float shieldTimer = 0f;

        // Events: (current, max)
        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<bool> OnShieldChanged;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public bool IsDead => currentHealth <= 0f;
        public bool ShieldActive => shieldActive;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            // Broadcast initial value so UI is in sync at scene start.
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (shieldActive)
            {
                shieldTimer -= Time.deltaTime;
                if (shieldTimer <= 0f)
                {
                    SetShield(false);
                }
            }
        }

        /// <summary>Apply damage. Ignored while shielded or invulnerable.</summary>
        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            if (isInvulnerable || shieldActive) return;
            if (amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                OnDeath?.Invoke();
            }
        }

        /// <summary>Restore health up to the maximum.</summary>
        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>Fully reset health (used on respawn / new game).</summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            SetShield(false);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetMaxHealth(float newMax, bool healToFull = false)
        {
            maxHealth = Mathf.Max(1f, newMax);
            if (healToFull) currentHealth = maxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>Activate a timed shield that blocks all incoming damage.</summary>
        public void ActivateShield(float duration)
        {
            shieldTimer = Mathf.Max(shieldTimer, duration);
            SetShield(true);
        }

        private void SetShield(bool active)
        {
            if (shieldActive == active) return;
            shieldActive = active;
            OnShieldChanged?.Invoke(shieldActive);
        }

        public void SetInvulnerable(bool value)
        {
            isInvulnerable = value;
        }
    }
}
