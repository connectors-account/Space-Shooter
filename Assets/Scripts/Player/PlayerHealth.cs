using UnityEngine;
using System;

/// <summary>
/// Manages player health, damage, and death.
/// Attach this script to the Player GameObject.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    [SerializeField] private int maxHealth = 100;
    
    [Tooltip("Invincibility duration after taking damage (seconds)")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    
    [Header("Visual Feedback")]
    [Tooltip("Color to flash when taking damage")]
    [SerializeField] private Color damageColor = Color.red;
    
    [Tooltip("Number of times to flash when taking damage")]
    [SerializeField] private int flashCount = 3;
    
    // Current health value
    private int currentHealth;
    
    // Invincibility tracking
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    
    // Cached component references
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    // Events for UI and game state updates
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnPlayerDeath;
    
    /// <summary>
    /// Gets the current health value.
    /// </summary>
    public int CurrentHealth => currentHealth;
    
    /// <summary>
    /// Gets the maximum health value.
    /// </summary>
    public int MaxHealth => maxHealth;
    
    /// <summary>
    /// Checks if player is currently invincible.
    /// </summary>
    public bool IsInvincible => isInvincible;
    
    /// <summary>
    /// Initialize health on awake.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    /// <summary>
    /// Set initial health when enabled.
    /// </summary>
    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Update invincibility timer.
    /// </summary>
    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                // Reset visual state
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }
    }
    
    /// <summary>
    /// Apply damage to the player.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply</param>
    public void TakeDamage(int damageAmount)
    {
        // Don't take damage if invincible
        if (isInvincible) return;
        
        // Apply damage
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Notify listeners of health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility period
            StartInvincibility();
        }
    }
    
    /// <summary>
    /// Heal the player by a specified amount.
    /// </summary>
    /// <param name="healAmount">Amount of health to restore</param>
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Fully restore player health.
    /// </summary>
    public void FullHeal()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Start the invincibility period with visual feedback.
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        
        // Start damage flash coroutine
        StartCoroutine(DamageFlashCoroutine());
    }
    
    /// <summary>
    /// Coroutine to flash the player sprite when damaged.
    /// </summary>
    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;
        
        float flashInterval = invincibilityDuration / (flashCount * 2);
        
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
        }
    }
    
    /// <summary>
    /// Handle player death.
    /// </summary>
    private void Die()
    {
        Debug.Log("Player has died!");
        
        // Notify listeners of death
        OnPlayerDeath?.Invoke();
        
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        
        // Disable the player (don't destroy, so we can restart)
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Reset health to maximum (used when restarting game).
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        invincibilityTimer = 0f;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Handle collision with enemy bullets or enemies.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for enemy bullets
        if (other.CompareTag("EnemyBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.Damage);
                Destroy(other.gameObject);
            }
        }
        // Check for direct enemy collision
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(20); // Contact damage
        }
    }
}
