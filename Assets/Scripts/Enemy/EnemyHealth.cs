using System;
using System.Collections;
using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Health component for enemies. Flashes red on hit and calls the owning
    /// EnemyBase.OnDeath when health reaches zero.
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.08f;

        private int _currentHealth;
        private SpriteRenderer _spriteRenderer;
        private Color _baseColor = Color.white;
        private Coroutine _flashRoutine;
        private EnemyBase _enemyBase;
        private bool _isDead;

        public event Action<int, int> OnHealthChanged;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public float HealthPercent => maxHealth > 0 ? (float)_currentHealth / maxHealth : 0f;
        public bool IsDead => _isDead;

        private void Awake()
        {
            _enemyBase = GetComponent<EnemyBase>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _baseColor = _spriteRenderer.color;
            }
        }

        /// <summary>
        /// Sets the maximum health and refills. Call when initializing from EnemyData.
        /// </summary>
        public void Initialize(int newMaxHealth)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            _currentHealth = maxHealth;
            _isDead = false;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _baseColor;
            }
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private void OnEnable()
        {
            _isDead = false;
        }

        public void TakeDamage(int amount)
        {
            if (_isDead || amount <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                Flash();
            }
        }

        private void Flash()
        {
            if (_spriteRenderer == null)
            {
                return;
            }
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            _spriteRenderer.color = _baseColor;
            _flashRoutine = null;
        }

        public void Die()
        {
            if (_isDead)
            {
                return;
            }
            _isDead = true;

            if (_enemyBase != null)
            {
                _enemyBase.OnDeath();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
