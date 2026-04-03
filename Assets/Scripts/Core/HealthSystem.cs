using UnityEngine;

/// <summary>
/// HealthSystem is a reusable component for anything that has hit points:
/// the player ship, enemies, etc.
/// It fires events when health changes or reaches zero.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    [Header("Health")]
    [Tooltip("Maximum (and starting) health")]
    public int maxHealth = 3;

    /// <summary>Current health value.</summary>
    public int CurrentHealth { get; private set; }

    /// <summary>True if current health is > 0.</summary>
    public bool IsAlive => CurrentHealth > 0;

    // ============================================================
    // EVENTS
    // ============================================================

    /// <summary>Fires whenever health changes. Args: (currentHealth, maxHealth).</summary>
    public event System.Action<int, int> OnHealthChanged;

    /// <summary>Fires once when health first reaches zero.</summary>
    public event System.Action OnDeath;

    // ============================================================
    // VISUAL FEEDBACK
    // ============================================================

    [Header("Damage Flash")]
    [Tooltip("Duration of the red damage flash in seconds")]
    public float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float flashTimer = 0f;
    private bool isFlashing = false;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Awake()
    {
        // Initialize health to max
        CurrentHealth = maxHealth;

        // Cache renderer for damage flash
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        // Handle damage flash timer
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                isFlashing = false;
                if (spriteRenderer != null)
                    spriteRenderer.color = originalColor;
            }
        }
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Apply damage to this entity. Clamps health to 0.
    /// Triggers death event if health reaches zero.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (!IsAlive) return; // already dead
        if (amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        // Visual feedback: flash red
        TriggerFlash();

        // Check for death
        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// Heal this entity. Clamps to maxHealth.
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive) return;
        if (amount <= 0) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>
    /// Reset health to maximum (used on respawn/restart).
    /// </summary>
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    // ============================================================
    // VISUAL FEEDBACK
    // ============================================================

    /// <summary>
    /// Briefly flash the sprite red to indicate a hit.
    /// </summary>
    void TriggerFlash()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = Color.red;
        isFlashing = true;
        flashTimer = flashDuration;
    }
}
