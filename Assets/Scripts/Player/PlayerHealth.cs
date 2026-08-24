using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;
using SpaceShooter.UI;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Manages player health, invincibility frames, shield, damage, healing, death and respawn.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = Constants.PlayerMaxHealth;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = Constants.InvincibilityDuration;
        [SerializeField] private float flashInterval = 0.1f;

        [Header("Effects")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private GameObject shieldVisual;

        [Header("Respawn")]
        [SerializeField] private Vector3 respawnPosition = new Vector3(0f, -3.5f, 0f);

        private int _currentHealth;
        private bool _isInvincible;
        private bool _shieldActive;
        private SpriteRenderer _spriteRenderer;
        private Coroutine _shieldRoutine;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsInvincible => _isInvincible;
        public bool ShieldActive => _shieldActive;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }
        }

        private void Start()
        {
            _currentHealth = maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            UpdateHud();
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || _isInvincible || _shieldActive)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            UpdateHud();

            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.playerHitSFX);
            }

            if (_currentHealth <= 0)
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
            if (amount <= 0 || _currentHealth <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            UpdateHud();
        }

        /// <summary>
        /// Activates an invulnerable shield for the given duration.
        /// </summary>
        public void ActivateShield(float duration)
        {
            if (_shieldRoutine != null)
            {
                StopCoroutine(_shieldRoutine);
            }
            _shieldRoutine = StartCoroutine(ShieldRoutine(duration));
        }

        private IEnumerator ShieldRoutine(float duration)
        {
            _shieldActive = true;
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(true);
            }

            yield return new WaitForSeconds(duration);

            _shieldActive = false;
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }
            _shieldRoutine = null;
        }

        private IEnumerator InvincibilityRoutine()
        {
            _isInvincible = true;
            float elapsed = 0f;
            bool visible = false;

            while (elapsed < invincibilityDuration)
            {
                if (_spriteRenderer != null)
                {
                    visible = !visible;
                    SetSpriteAlpha(visible ? 0.35f : 1f);
                }
                elapsed += flashInterval;
                yield return new WaitForSeconds(flashInterval);
            }

            if (_spriteRenderer != null)
            {
                SetSpriteAlpha(1f);
            }
            _isInvincible = false;
        }

        private void SetSpriteAlpha(float alpha)
        {
            Color c = _spriteRenderer.color;
            c.a = alpha;
            _spriteRenderer.color = c;
        }

        private void Die()
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSFX);
            }

            OnDeath?.Invoke();

            if (GameManager.HasInstance)
            {
                GameManager.Instance.LoseLife();

                if (GameManager.Instance.Lives > 0)
                {
                    Respawn();
                }
                else
                {
                    // GameManager.LoseLife already triggers GameOver; just hide the ship.
                    gameObject.SetActive(false);
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void Respawn()
        {
            _currentHealth = maxHealth;
            transform.position = respawnPosition;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            UpdateHud();
            ActivateShield(2f);
            StartCoroutine(InvincibilityRoutine());
        }

        private void UpdateHud()
        {
            if (UIManager.HasInstance)
            {
                UIManager.Instance.UpdateHealth(_currentHealth, maxHealth);
            }
        }
    }
}
