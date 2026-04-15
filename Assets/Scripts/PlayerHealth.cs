using UnityEngine;

/// <summary>
/// Handles health, damage, healing, and death for the player ship.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    private UIManager uiManager;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        uiManager = FindObjectOfType<UIManager>();

        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        currentHealth -= Mathf.Abs(damage);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        currentHealth += Mathf.Abs(amount);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }

        // Hide player when dead. You can replace this with an explosion effect later.
        gameObject.SetActive(false);
    }
}
