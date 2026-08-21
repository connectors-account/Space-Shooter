using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Bullets;
using SpaceShooter.Core;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player health with invincibility frames, shield power-up support, and damage flash.
    /// Implements IDamageable so enemy bullets and collisions can damage it.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 2f;
        [SerializeField] private float flashInterval = 0.1f;

        [Header("Damage Flash")]
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.15f;

        private SpriteRenderer _sprite;
        private Color _originalColor;
        private bool _isInvincible;
        private bool _hasShield;
        private bool _isDead;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool HasShield => _hasShield;
        public bool IsInvincible => _isInvincible;

        // Events
        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action OnDeath;
        public event Action OnShieldActivated;
        public event Action OnShieldBroken;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _originalColor = _sprite.color;
        }

        private void OnEnable()
        {
            ResetHealth();
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            _isDead = false;
            _isInvincible = false;
            _hasShield = false;
            if (_sprite != null)
            {
                _sprite.color = _originalColor;
            }
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (_isDead || _isInvincible || amount <= 0) return;

            // Shield absorbs one hit entirely.
            if (_hasShield)
            {
                _hasShield = false;
                OnShieldBroken?.Invoke();
                StartCoroutine(InvincibilityRoutine());
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            AudioManager.Instance?.PlaySFX("hit");
            StartCoroutine(DamageFlashRoutine());

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }

        public void Heal(int amount)
        {
            if (_isDead || amount <= 0) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ActivateShield()
        {
            _hasShield = true;
            OnShieldActivated?.Invoke();
        }

        private IEnumerator DamageFlashRoutine()
        {
            if (_sprite == null) yield break;
            _sprite.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            if (!_isInvincible)
            {
                _sprite.color = _originalColor;
            }
        }

        private IEnumerator InvincibilityRoutine()
        {
            _isInvincible = true;
            float timer = 0f;
            bool visible = true;

            while (timer < invincibilityDuration)
            {
                visible = !visible;
                if (_sprite != null)
                {
                    Color c = _originalColor;
                    c.a = visible ? 1f : 0.35f;
                    _sprite.color = c;
                }
                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval;
            }

            _isInvincible = false;
            if (_sprite != null)
            {
                _sprite.color = _originalColor;
            }
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;
            OnDeath?.Invoke();

            AudioManager.Instance?.PlaySFX("explosion");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseLife();
                if (GameManager.Instance.Lives > 0)
                {
                    // Respawn with fresh health if lives remain.
                    ResetHealth();
                    StartCoroutine(InvincibilityRoutine());
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Direct collision with an enemy body also damages the player.
            if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
            {
                TakeDamage(25);
            }
        }
    }
}
