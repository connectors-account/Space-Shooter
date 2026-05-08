using UnityEngine;
using System;

/// <summary>
/// Manages player health, damage, invincibility frames, and shield.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 1.5f;

    [Header("Shield")]
    [SerializeField] private GameObject shieldVisual; // Assigned at runtime

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool HasShield { get; private set; }
    public bool IsInvincible { get; private set; }

    public event Action OnDeath;
    public event Action OnDamaged;
    public event Action OnShieldBroken;

    private float invincibilityTimer;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        CurrentHealth = maxHealth;
        NotifyHealthUI();
    }

    private void Update()
    {
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            // Flash effect during invincibility
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 8f, 1f) > 0.5f ? 1f : 0.3f;
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            if (invincibilityTimer <= 0f)
            {
                IsInvincible = false;
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = 1f;
                    spriteRenderer.color = c;
                }
            }
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (IsInvincible || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // Shield absorbs one hit
        if (HasShield)
        {
            HasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            OnShieldBroken?.Invoke();
            ActivateInvincibility();
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            return;
        }

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        NotifyHealthUI();
        OnDamaged?.Invoke();
        AudioManager.Instance?.PlaySFX("PlayerHit");

        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            ActivateInvincibility();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        NotifyHealthUI();
        AudioManager.Instance?.PlaySFX("Heal");
    }

    public void ActivateShield()
    {
        HasShield = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);
        AudioManager.Instance?.PlaySFX("ShieldUp");
    }

    public void SetShieldVisual(GameObject visual)
    {
        shieldVisual = visual;
    }

    private void ActivateInvincibility()
    {
        IsInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    private void Die()
    {
        OnDeath?.Invoke();
        AudioManager.Instance?.PlaySFX("PlayerDeath");
        GameManager.Instance?.PlayerDied();

        // Spawn explosion effect
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Spawn(Tags.Explosion, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);
    }

    private void NotifyHealthUI()
    {
        GameManager.Instance?.NotifyPlayerHealthChanged(CurrentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        HasShield = false;
        IsInvincible = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        NotifyHealthUI();
    }
}
