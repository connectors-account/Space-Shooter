// ============================================================================
// PlayerHealth.cs — Player damage, health, shields, and death
// ============================================================================
using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Shield")]
    [SerializeField] private int shieldPoints;
    [SerializeField] private int maxShield = 3;
    [SerializeField] private GameObject shieldVisual; // child object to toggle

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip shieldHitSound;

    // Events
    public static event Action<int, int> OnHealthChanged; // current, max
    public static event Action<int> OnShieldChanged;
    public static event Action OnPlayerDeath;

    // Runtime
    private bool isInvincible;
    private SpriteRenderer spriteRenderer;
    private PlayerController controller;

    // Properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int ShieldPoints => shieldPoints;
    public bool IsInvincible => isInvincible;

    // =========================================================================
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateShieldVisual();
    }

    // =========================================================================
    // Damage
    // =========================================================================
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        if (currentHealth <= 0) return;

        // Shield absorbs damage first
        if (shieldPoints > 0)
        {
            shieldPoints -= damage;
            if (shieldPoints < 0) shieldPoints = 0;
            OnShieldChanged?.Invoke(shieldPoints);
            UpdateShieldVisual();

            if (SoundManager.Instance != null && shieldHitSound != null)
                SoundManager.Instance.PlaySFX(shieldHitSound, 0.5f);

            StartCoroutine(InvincibilityCoroutine(0.5f));
            return;
        }

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (SoundManager.Instance != null && hitSound != null)
            SoundManager.Instance.PlaySFX(hitSound, 0.6f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
        }
    }

    // =========================================================================
    // Healing & Shield
    // =========================================================================
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddShield(int points)
    {
        shieldPoints = Mathf.Min(shieldPoints + points, maxShield);
        OnShieldChanged?.Invoke(shieldPoints);
        UpdateShieldVisual();
    }

    private void UpdateShieldVisual()
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(shieldPoints > 0);
    }

    // =========================================================================
    // Invincibility
    // =========================================================================
    public void SetInvincible(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        float timer = 0;

        while (timer < duration)
        {
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = c.a > 0.5f ? 0.3f : 1f;
                spriteRenderer.color = c;
            }
            timer += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }
        isInvincible = false;
    }

    // =========================================================================
    // Death
    // =========================================================================
    private void Die()
    {
        if (SoundManager.Instance != null && deathSound != null)
            SoundManager.Instance.PlaySFX(deathSound, 0.8f);

        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        OnPlayerDeath?.Invoke();

        if (controller != null)
            controller.SetControllable(false);

        // Notify GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.LoseLife();

        Destroy(gameObject, 0.1f);
    }

    // =========================================================================
    // Collision
    // =========================================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            Bullet b = other.GetComponent<Bullet>();
            int dmg = b != null ? b.Damage : 1;
            TakeDamage(dmg);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }
}
