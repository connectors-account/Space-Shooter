using System;
using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Enemy hit points. Handles damage, death (with score award) and a hit flash.
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        [Tooltip("Maximum hit points.")]
        public int maxHealth = 1;

        [Tooltip("Score awarded to the player when this enemy dies.")]
        public int scoreValue = 100;

        [SerializeField] private int currentHealth;

        private bool _isDead;
        private bool _invincible;
        private SpriteRenderer _renderer;
        private Color _baseColor = Color.white;

        public int CurrentHealth => currentHealth;
        public bool IsDead => _isDead;

        /// <summary>Set to true (e.g. boss shield phase) to ignore all incoming damage.</summary>
        public bool Invincible
        {
            get => _invincible;
            set => _invincible = value;
        }

        public event Action OnDeath;
        public event Action<int> OnHealthChanged;

        private void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            if (_renderer != null) _baseColor = _renderer.color;
            if (currentHealth <= 0) currentHealth = maxHealth;
        }

        /// <summary>Configures the enemy's max health and score, resetting current health.</summary>
        public void Configure(int max, int score)
        {
            maxHealth = Mathf.Max(1, max);
            scoreValue = Mathf.Max(0, score);
            currentHealth = maxHealth;
            _isDead = false;
        }

        /// <summary>Applies damage; triggers death when health reaches zero.</summary>
        public void TakeDamage(int dmg)
        {
            if (_isDead || _invincible || dmg <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - dmg);
            OnHealthChanged?.Invoke(currentHealth);

            if (isActiveAndEnabled && Application.isPlaying)
            {
                StartCoroutine(HitFlash());
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>Kills the enemy, awarding score and raising <see cref="OnDeath"/>.</summary>
        public void Die()
        {
            if (_isDead) return;
            _isDead = true;
            currentHealth = 0;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(scoreValue);
                ScoreManager.Instance.RegisterKill();
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSFX);

            OnDeath?.Invoke();

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator HitFlash()
        {
            if (_renderer == null) yield break;
            _renderer.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (_renderer != null) _renderer.color = _baseColor;
        }
    }
}
