using UnityEngine;

/// <summary>
/// Manages player health, damage, invincibility frames, and death.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;
    public float blinkRate = 0.1f;

    private bool isInvincible = false;
    private bool hasShield = false;
    private SpriteRenderer spriteRenderer;
    private float invincibilityTimer = 0f;
    private float blinkTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateUI();
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            blinkTimer -= Time.deltaTime;

            if (blinkTimer <= 0f)
            {
                blinkTimer = blinkRate;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = true;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            // Shield absorbs hit but doesn't break
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHit();
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateUI();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHit();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartInvincibility();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void SetShield(bool active)
    {
        hasShield = active;
    }

    void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        blinkTimer = blinkRate;
    }

    void Die()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayExplosion();

        // Spawn explosion effect
        EffectsManager.SpawnExplosion(transform.position, Color.yellow);

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();

        gameObject.SetActive(false);
    }

    void UpdateUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }
}
