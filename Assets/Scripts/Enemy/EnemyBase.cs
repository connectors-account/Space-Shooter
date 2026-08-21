using System;
using UnityEngine;
using SpaceShooter.Bullets;
using SpaceShooter.Core;
using SpaceShooter.PowerUps;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Abstract base for all enemies. Handles health, scoring, death, power-up drops,
    /// and an explosion effect. Implements IDamageable for bullet collisions.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] protected int maxHealth = 30;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected float moveSpeed = 3f;

        [Header("Drops")]
        [Range(0f, 1f)]
        [SerializeField] protected float powerUpDropChance = 0.2f;
        [SerializeField] protected PowerUp[] powerUpPrefabs;

        [Header("Death FX")]
        [SerializeField] protected GameObject explosionPrefab;
        [SerializeField] protected Color hitFlashColor = Color.white;

        protected int currentHealth;
        protected SpriteRenderer spriteRenderer;
        protected Color originalColor;
        protected bool isDead;

        public int ScoreValue => scoreValue;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public event Action<EnemyBase> OnEnemyDied;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        protected virtual void OnEnable()
        {
            currentHealth = maxHealth;
            isDead = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        public virtual void TakeDamage(int amount)
        {
            if (isDead || amount <= 0) return;

            currentHealth -= amount;
            OnDamaged();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>Hook for subclasses (e.g. hit flash, phase transitions). Default: brief flash.</summary>
        protected virtual void OnDamaged()
        {
            if (spriteRenderer != null)
            {
                CancelInvoke(nameof(RestoreColor));
                spriteRenderer.color = hitFlashColor;
                Invoke(nameof(RestoreColor), 0.06f);
            }
        }

        private void RestoreColor()
        {
            if (spriteRenderer != null && !isDead)
            {
                spriteRenderer.color = originalColor;
            }
        }

        public virtual void Die()
        {
            if (isDead) return;
            isDead = true;

            GameManager.Instance?.AddScore(scoreValue);
            AudioManager.Instance?.PlaySFX("explosion");

            SpawnExplosion();
            TryDropPowerUp();

            Despawn();
        }

        protected void SpawnExplosion()
        {
            if (explosionPrefab != null)
            {
                GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }

        protected void TryDropPowerUp()
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
            if (UnityEngine.Random.value > powerUpDropChance) return;

            PowerUp prefab = powerUpPrefabs[UnityEngine.Random.Range(0, powerUpPrefabs.Length)];
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Deactivates the enemy (pool-friendly) and notifies listeners exactly once.
        /// Called on death and when the enemy leaves the play area.
        /// </summary>
        public virtual void Despawn()
        {
            var handler = OnEnemyDied;
            OnEnemyDied = null;
            handler?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
