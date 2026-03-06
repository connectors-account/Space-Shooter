using UnityEngine;
using System;

/// <summary>
/// Generic health system component that can be attached to any entity requiring health tracking.
/// Handles damage, healing, invincibility, and death events.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float invincibilityDuration = 0f;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool flashOnDamage = true;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    
    // Events
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnDeath;
    public event Action OnDamaged;
    public event Action OnHealed;
    
    // Properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    public bool IsAlive => currentHealth > 0;
    public bool IsInvincible { get; private set; }
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float invincibilityTimer;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    private void Update()
    {
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                IsInvincible = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
            else
            {
                // Flashing effect during invincibility
                if (spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                    Color c = originalColor;
                    c.a = 0.3f + alpha * 0.7f;
                    spriteRenderer.color = c;
                }
            }
        }
    }
    
    /// <summary>
    /// Apply damage to this entity
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!IsAlive || IsInvincible) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke();
        
        if (flashOnDamage && spriteRenderer != null)
        {
            StartCoroutine(FlashDamage());
        }
        
        if (invincibilityDuration > 0)
        {
            SetInvincible(invincibilityDuration);
        }
        
        AudioManager.Instance?.PlaySound("PlayerHit");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Heal this entity
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive) return;
        
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        if (currentHealth > previousHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnHealed?.Invoke();
            AudioManager.Instance?.PlaySound("Heal");
        }
    }
    
    /// <summary>
    /// Set full health
    /// </summary>
    public void SetFullHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Set max health (also adjusts current health proportionally)
    /// </summary>
    public void SetMaxHealth(int newMaxHealth)
    {
        float healthPercentage = HealthPercentage;
        maxHealth = newMaxHealth;
        currentHealth = Mathf.RoundToInt(maxHealth * healthPercentage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Make entity invincible for a duration
    /// </summary>
    public void SetInvincible(float duration)
    {
        IsInvincible = true;
        invincibilityTimer = duration;
    }
    
    private void Die()
    {
        OnDeath?.Invoke();
        AudioManager.Instance?.PlaySound("Explosion");
        
        if (destroyOnDeath)
        {
            // Spawn explosion effect
            GameManager.Instance?.SpawnExplosion(transform.position);
            Destroy(gameObject);
        }
    }
    
    private System.Collections.IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            if (!IsInvincible)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
}
