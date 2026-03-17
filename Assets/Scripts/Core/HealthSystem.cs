using UnityEngine;
using System;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Reusable health system component
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private bool destroyOnDeath = true;
        
        private int currentHealth;
        
        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public float HealthPercentage => (float)currentHealth / maxHealth;
        public bool IsAlive => currentHealth > 0;
        
        private void Awake()
        {
            currentHealth = maxHealth;
        }
        
        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void Heal(int amount)
        {
            if (!IsAlive) return;
            
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
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
        
        private void Die()
        {
            OnDeath?.Invoke();
            
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
        
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
