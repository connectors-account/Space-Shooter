using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    [SerializeField] private int maxHealth = 5;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    private bool isDead;

    private void Awake()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
        isDead = false;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}
