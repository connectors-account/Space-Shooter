// ============================================================================
// PlayerHealth.cs - Player health, shield, damage, and death
// ============================================================================
using UnityEngine;
using System;

/// <summary>
/// Manages the player's health pool and optional energy shield.
/// Broadcasts events for the HUD and handles invulnerability frames.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Shield")]
    [Tooltip("Maximum shield points. Shield absorbs damage before health.")]
    [SerializeField] private int maxShield = 50;
    [SerializeField] private int currentShield;
    [Tooltip("Seconds of invulnerability after taking a hit.")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    [Header("Visual Feedback")]
    [Tooltip("Sprite renderer for flashing on damage.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // ---- Events ----
    /// <summary>Passes (currentHealth, maxHealth).</summary>
    public event Action<int, int> OnHealthChanged;
    /// <summary>Passes (currentShield, maxShield).</summary>
    public event Action<int, int> OnShieldChanged;
    /// <summary>Fired once when the player dies.</summary>
    public event Action OnDeath;

    // ---- State ----
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentShield => currentShield;
    public int MaxShield => maxShield;
    public bool IsAlive => currentHealth > 0;
    public bool HasShield => currentShield > 0;

    private bool isInvulnerable;
    private float invulnerabilityTimer;
    private Color originalColor;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        currentHealth = maxHealth;
        currentShield = 0; // Shield starts at 0; granted by power-ups.

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnShieldChanged?.Invoke(currentShield, maxShield);
    }

    private void Update()
    {
        // Tick invulnerability timer.
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            // Flash effect: toggle alpha rapidly.
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }

            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
                if (spriteRenderer != null)
                    spriteRenderer.color = originalColor;
            }
        }
    }

    // ========================================================================
    // Public API
    // ========================================================================

    /// <summary>
    /// Inflicts damage, first to shield then to health. Triggers invulnerability.
    /// </summary>
    /// <param name="amount">Raw damage amount (positive).</param>
    public void TakeDamage(int amount)
    {
        if (!IsAlive || isInvulnerable) return;
        if (amount <= 0) return;

        // Shield absorbs damage first.
        if (currentShield > 0)
        {
            int shieldDamage = Mathf.Min(amount, currentShield);
            currentShield -= shieldDamage;
            amount -= shieldDamage;
            OnShieldChanged?.Invoke(currentShield, maxShield);
            AudioManager.Instance?.PlaySFX(AudioManager.SFX.ShieldHit);
        }

        // Remaining damage hits health.
        if (amount > 0)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Max(currentHealth, 0);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            AudioManager.Instance?.PlaySFX(AudioManager.SFX.Hit);
        }

        // Activate invulnerability frames.
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        // Reset combo on taking damage.
        GameManager.Instance?.ResetCombo();

        // Death check.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>Heals the player by the given amount, capped at maxHealth.</summary>
    public void Heal(int amount)
    {
        if (!IsAlive) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>Adds shield points, capped at maxShield.</summary>
    public void AddShield(int amount)
    {
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        OnShieldChanged?.Invoke(currentShield, maxShield);
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.ShieldUp);
    }

    /// <summary>Sets shield to full capacity.</summary>
    public void FullShield()
    {
        AddShield(maxShield);
    }

    // ========================================================================
    // Internals
    // ========================================================================

    private void Die()
    {
        OnDeath?.Invoke();
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.Explosion);

        // Notify GameManager after a short delay for the death animation to play.
        Invoke(nameof(NotifyGameOver), 1.5f);

        // Disable player controls.
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;
        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = false;

        // Start death animation (simple scale-down + fade).
        StartCoroutine(DeathAnimation());
    }

    private void NotifyGameOver()
    {
        GameManager.Instance?.GameOver();
    }

    private System.Collections.IEnumerator DeathAnimation()
    {
        float duration = 1.2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Shrink and fade.
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f - t;
                spriteRenderer.color = c;
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
