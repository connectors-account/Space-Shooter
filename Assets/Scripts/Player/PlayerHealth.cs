using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// PlayerHealth manages the player's health, damage, and death.
/// Attach this script to the Player GameObject.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public int maxHealth = 100;
    
    [Tooltip("Current health points")]
    private int currentHealth;

    [Header("Invincibility Settings")]
    [Tooltip("Duration of invincibility after taking damage")]
    public float invincibilityDuration = 1.5f;
    
    [Tooltip("Blink rate during invincibility")]
    public float blinkRate = 0.1f;
    
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // current, max
    public UnityEvent OnPlayerDeath;

    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Notify UI of initial health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        // Handle invincibility timer and blinking effect
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // Blink effect
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time / blinkRate, 1f) > 0.5f ? 1f : 0.3f;
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                // Reset sprite alpha
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }
    }

    /// <summary>
    /// Apply damage to the player
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    public void TakeDamage(int damage)
    {
        // Ignore damage if invincible
        if (isInvincible) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Play hurt sound
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        
        // Notify listeners of health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility period
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    /// <summary>
    /// Heal the player
    /// </summary>
    /// <param name="amount">Amount of health to restore</param>
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        // Notify listeners of health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Handle player death
    /// </summary>
    void Die()
    {
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Disable player control
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetCanControl(false);
        }
        
        // Notify listeners
        OnPlayerDeath?.Invoke();
        
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        
        // Hide player (or play death animation)
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Reset health to maximum
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Get current health value
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Get maximum health value
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Check if player is currently invincible
    /// </summary>
    public bool IsInvincible()
    {
        return isInvincible;
    }
}
