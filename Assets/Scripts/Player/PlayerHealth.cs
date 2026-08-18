using System;
using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Player health, shield and invincibility handling.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Tooltip("Maximum hit points.")]
        public int maxHealth = 3;

        [Tooltip("Seconds of invincibility after taking damage (with sprite flashing).")]
        public float invincibilityDuration = 1.5f;

        [SerializeField] private int currentHealth;
        [SerializeField] private bool hasShield;
        [SerializeField] private int shieldHP;

        private float _invincibleUntil;
        private bool _isDead;
        private SpriteRenderer _renderer;
        private Coroutine _flashRoutine;

        public int CurrentHealth => currentHealth;
        public bool HasShield => hasShield;
        public int ShieldHP => shieldHP;
        public bool IsInvincible => invincibilityDuration > 0f && Time.time < _invincibleUntil;

        public event Action<int> OnHealthChanged;
        public event Action<int> OnShieldChanged;
        public event Action OnDeath;

        private void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            ResetHealth();
        }

        /// <summary>Restores health to full and clears shield/invincibility.</summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            hasShield = false;
            shieldHP = 0;
            _invincibleUntil = 0f;
            _isDead = false;
            OnHealthChanged?.Invoke(currentHealth);
            OnShieldChanged?.Invoke(shieldHP);
        }

        /// <summary>
        /// Applies <paramref name="dmg"/> damage. The shield absorbs damage first and
        /// any overflow reduces health. Ignored during invincibility frames.
        /// </summary>
        public void TakeDamage(int dmg)
        {
            if (_isDead || dmg <= 0) return;
            if (IsInvincible) return;

            int remaining = dmg;

            if (hasShield && shieldHP > 0)
            {
                int absorbed = Mathf.Min(shieldHP, remaining);
                shieldHP -= absorbed;
                remaining -= absorbed;
                if (shieldHP <= 0) hasShield = false;
                OnShieldChanged?.Invoke(shieldHP);
            }

            if (remaining > 0)
            {
                currentHealth = Mathf.Max(0, currentHealth - remaining);
                OnHealthChanged?.Invoke(currentHealth);

                // Breaking the kill chain when hit.
                if (ScoreManager.Instance != null) ScoreManager.Instance.OnPlayerDamaged();
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSFX);

            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            StartInvincibility();
        }

        /// <summary>Heals the player up to <see cref="maxHealth"/>.</summary>
        public void Heal(int amount)
        {
            if (_isDead || amount <= 0) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth);
        }

        /// <summary>Grants a shield with <paramref name="hp"/> hit points.</summary>
        public void ActivateShield(int hp)
        {
            if (hp <= 0) return;
            hasShield = true;
            shieldHP = hp;
            OnShieldChanged?.Invoke(shieldHP);
        }

        /// <summary>Kills the player, raising <see cref="OnDeath"/> exactly once.</summary>
        public void Die()
        {
            if (_isDead) return;
            _isDead = true;
            currentHealth = 0;
            OnHealthChanged?.Invoke(currentHealth);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSFX);

            OnDeath?.Invoke();

            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }

        private void StartInvincibility()
        {
            if (invincibilityDuration <= 0f) return;
            _invincibleUntil = Time.time + invincibilityDuration;
            if (isActiveAndEnabled)
            {
                if (_flashRoutine != null) StopCoroutine(_flashRoutine);
                _flashRoutine = StartCoroutine(FlashRoutine());
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (_renderer == null) yield break;
            const float interval = 0.1f;
            while (Time.time < _invincibleUntil)
            {
                _renderer.enabled = !_renderer.enabled;
                yield return new WaitForSeconds(interval);
            }
            _renderer.enabled = true;
        }
    }
}
