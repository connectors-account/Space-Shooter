using UnityEngine;
using System;

/// <summary>
/// PlayerHealth manages the player's health, damage, and death.
/// Attach this to the Player GameObject alongside PlayerController.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Audio")]
    [SerializeField] private string hurtSoundName = "PlayerHurt";
    [SerializeField] private string deathSoundName = "PlayerDeath";

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionPrefab;

    // Events
    public static event Action<int, int> OnHealthChanged; // current, max
    public static event Action OnPlayerDeath;

    // Private variables
    private int currentHealth;
    private bool isInvincible;
    private float invincibilityTimer;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        ResetHealth();
    }

    private void Update()
    {
        if (isInvincible)
        {
            UpdateInvincibility();
        }
    }

    /// <summary>
    /// Reset health to maximum
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        invincibilityTimer = 0f;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Apply damage to the player
    /// </summary>
    public void TakeDamage(int damage)
    {
        // Check if invincible
        if (isInvincible)
            return;

        // Check if shield absorbs damage
        if (playerController != null && playerController.AbsorbDamageWithShield())
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound("ShieldHit");
            }
            return;
        }

        // Apply damage
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Play hurt sound
        if (SoundManager.Instance != null && currentHealth > 0)
        {
            SoundManager.Instance.PlaySound(hurtSoundName);
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility frames
            StartInvincibility();
        }
    }

    /// <summary>
    /// Heal the player
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Player healed! Current health: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Start invincibility period after taking damage
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        StartCoroutine(InvincibilityFlash());
    }

    /// <summary>
    /// Update invincibility timer
    /// </summary>
    private void UpdateInvincibility()
    {
        invincibilityTimer -= Time.deltaTime;
        if (invincibilityTimer <= 0)
        {
            isInvincible = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }
    }

    /// <summary>
    /// Flash sprite during invincibility
    /// </summary>
    private System.Collections.IEnumerator InvincibilityFlash()
    {
        while (isInvincible && spriteRenderer != null)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    /// <summary>
    /// Handle player death
    /// </summary>
    private void Die()
    {
        Debug.Log("Player died!");

        // Play death sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(deathSoundName);
        }

        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Notify listeners (GameManager will handle game over)
        OnPlayerDeath?.Invoke();

        // Disable player (don't destroy - we might restart)
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Handle collision with enemies or enemy bullets
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Collision with enemy
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            
            // Optionally damage or destroy the enemy too
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(999); // Instant kill on collision
            }
        }
        // Collision with enemy bullet
        else if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            
            // Return bullet to pool
            other.gameObject.SetActive(false);
        }
    }
}
