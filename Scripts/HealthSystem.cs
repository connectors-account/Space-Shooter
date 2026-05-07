using System;
using UnityEngine;

/// <summary>
/// Generic health component for damage/heal/death handling.
/// Attach this to the player ship.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Start()
    {
        ResetHealth();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        GameManager.Instance?.OnPlayerHealthChanged(CurrentHealth, MaxHealth);

        if (CurrentHealth == 0)
        {
            IsDead = true;
            OnDeath?.Invoke();
            GameManager.Instance?.GameOver();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        GameManager.Instance?.OnPlayerHealthChanged(CurrentHealth, MaxHealth);
    }

    public void ResetHealth()
    {
        IsDead = false;
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        GameManager.Instance?.OnPlayerHealthChanged(CurrentHealth, MaxHealth);
    }
}
