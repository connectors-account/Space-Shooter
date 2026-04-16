using System;
using UnityEngine;

namespace SpaceShooter.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private bool destroyOnDeath = true;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDead { get; private set; }

        public event Action<int, int> OnHealthChanged;
        public event Action<Health> OnDied;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            IsDead = false;
        }

        public void ResetHealthToMax()
        {
            IsDead = false;
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                return;
            }

            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }

            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            OnDied?.Invoke(this);

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
