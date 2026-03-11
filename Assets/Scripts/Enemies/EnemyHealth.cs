using UnityEngine;
using System;

/// <summary>
/// Manages enemy health, damage, and death.
/// Attach this script to enemy GameObjects.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    [SerializeField] private int maxHealth = 20;
    
    [Header("Score Settings")]
    [Tooltip("Points awarded when this enemy is destroyed")]
    [SerializeField] private int scoreValue = 100;
    
    [Header("Visual Feedback")]
    [Tooltip("Color to flash when taking damage")]
    [SerializeField] private Color damageColor = Color.red;
    
    [Tooltip("Duration of damage flash")]
    [SerializeField] private float flashDuration = 0.1f;
    
    [Header("Effects")]
    [Tooltip("Particle effect to spawn on death")]
    [SerializeField] private GameObject deathEffectPrefab;
    
    // Current health
    private int currentHealth;
    
    // Cached components
    private SpriteRenderer spriteRenderer;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    
    // Events
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnDeath;
    
    /// <summary>
    /// Gets the current health value.
    /// </summary>
    public int CurrentHealth => currentHealth;
    
    /// <summary>
    /// Gets the maximum health value.
    /// </summary>
    public int MaxHealth => maxHealth;
    
    /// <summary>
    /// Gets the score value of this enemy.
    /// </summary>
    public int ScoreValue => scoreValue;
    
    /// <summary>
    /// Initialize health and cache components.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }
    
    /// <summary>
    /// Set initial health.
    /// </summary>
    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// Apply damage to the enemy.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply</param>
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Visual feedback
        StartCoroutine(DamageFlash());
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Flash the enemy sprite when damaged.
    /// </summary>
    private System.Collections.IEnumerator DamageFlash()
    {
        SetColor(damageColor);
        yield return new WaitForSeconds(flashDuration);
        SetColor(originalColor);
    }
    
    /// <summary>
    /// Set the color of the enemy renderer.
    /// </summary>
    /// <param name="color">Color to set</param>
    private void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        else if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }
    
    /// <summary>
    /// Handle enemy death.
    /// </summary>
    private void Die()
    {
        // Notify listeners
        OnDeath?.Invoke();
        
        // Award score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }
        
        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Destroy the enemy
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Set the health values.
    /// </summary>
    /// <param name="max">Maximum health</param>
    public void SetMaxHealth(int max)
    {
        maxHealth = max;
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// Set the score value.
    /// </summary>
    /// <param name="score">Score value</param>
    public void SetScoreValue(int score)
    {
        scoreValue = score;
    }
    
    /// <summary>
    /// Handle collision with player bullets.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.Damage);
                Destroy(other.gameObject);
            }
        }
    }
}
