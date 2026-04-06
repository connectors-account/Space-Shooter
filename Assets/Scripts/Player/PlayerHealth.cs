using UnityEngine;

/// <summary>
/// Manages player health, damage, invincibility frames, and shield power-up.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public float invincibilityDuration = 1.5f;

    public int CurrentHealth { get; private set; }
    public bool IsInvincible { get; private set; }
    public bool HasShield { get; private set; }

    // Events
    public System.Action<int, int> OnHealthChanged; // current, max
    public System.Action OnPlayerDeath;
    public System.Action OnShieldBroken;

    private float invincibilityTimer;
    private SpriteRenderer spriteRenderer;
    private float flashTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Update()
    {
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            flashTimer += Time.deltaTime;

            // Flash the sprite during invincibility
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = Mathf.Sin(flashTimer * 20f) > 0f;
            }

            if (invincibilityTimer <= 0f)
            {
                IsInvincible = false;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = true;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsInvincible) return;

        if (HasShield)
        {
            HasShield = false;
            OnShieldBroken?.Invoke();
            ActivateInvincibility();
            AudioManager.Instance?.PlaySound("ShieldBreak");
            return;
        }

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        AudioManager.Instance?.PlaySound("PlayerHit");

        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            ActivateInvincibility();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void ActivateShield()
    {
        HasShield = true;
    }

    private void ActivateInvincibility()
    {
        IsInvincible = true;
        invincibilityTimer = invincibilityDuration;
        flashTimer = 0f;
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke();
        AudioManager.Instance?.PlaySound("PlayerDeath");
        GameManager.Instance?.GameOver();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Full reset for a new game.
    /// </summary>
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        HasShield = false;
        IsInvincible = false;
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
