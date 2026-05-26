using UnityEngine;

/// <summary>
/// Reusable health component for any damageable entity (player, enemies, etc.).
/// Attach to any GameObject that should have hit-points.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = true;

    [Header("Visual Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public bool IsInvincible { get; set; }

    // Events
    public System.Action<int, int> OnHealthChanged;   // (current, max)
    public System.Action OnDeath;
    public System.Action<int> OnDamageTaken;           // damage amount

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    // ────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        CurrentHealth = maxHealth;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    // ────────────────────────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────────────────────────

    /// <summary>Apply damage. Respects invincibility.</summary>
    public void TakeDamage(int amount)
    {
        if (!IsAlive || IsInvincible || amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnDamageTaken?.Invoke(amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (_spriteRenderer != null)
            StartCoroutine(FlashRoutine());

        if (CurrentHealth <= 0)
            Die();
    }

    /// <summary>Heal by the given amount, capped at maxHealth.</summary>
    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>Fully restore health.</summary>
    public void FullHeal()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>Set a new maximum health (also heals to new max).</summary>
    public void SetMaxHealth(int newMax)
    {
        maxHealth = Mathf.Max(1, newMax);
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    // ────────────────────────────────────────────────────────────────────
    // Internal
    // ────────────────────────────────────────────────────────────────────
    private void Die()
    {
        OnDeath?.Invoke();
        if (destroyOnDeath)
            Destroy(gameObject);
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        if (_spriteRenderer == null) yield break;
        _spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
    }
}
