using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Data;
using SpaceShooter.Utilities;
using SpaceShooter.Spawning;
using SpaceShooter.PowerUps;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Abstract base for all enemies. Wires up the health/mover/shooter components,
    /// handles death rewards (score, power-up drops, explosion) and notifies the spawner.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] protected EnemyData data;

        protected EnemyHealth health;
        protected EnemyMover mover;
        protected EnemyShooter shooter;
        protected SpriteRenderer spriteRenderer;

        private bool _deathHandled;

        public EnemyData Data => data;

        protected virtual void Awake()
        {
            health = GetComponent<EnemyHealth>();
            mover = GetComponent<EnemyMover>();
            shooter = GetComponent<EnemyShooter>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected virtual void Start()
        {
            InitializeEnemy();
        }

        /// <summary>
        /// Assigns the data asset (called by the spawner before Start if spawned at runtime).
        /// </summary>
        public void SetData(EnemyData enemyData)
        {
            data = enemyData;
        }

        /// <summary>
        /// Configures all sub-components from the EnemyData asset.
        /// </summary>
        public abstract void InitializeEnemy();

        /// <summary>
        /// Applies common visual/stat setup from the data asset. Call from InitializeEnemy.
        /// </summary>
        protected void ApplyCommonData()
        {
            if (data == null)
            {
                return;
            }

            if (spriteRenderer != null && data.sprite != null)
            {
                spriteRenderer.sprite = data.sprite;
                spriteRenderer.color = data.tint;
            }

            if (health != null)
            {
                health.Initialize(data.health);
            }
        }

        /// <summary>
        /// Called by EnemyHealth when this enemy dies.
        /// </summary>
        public virtual void OnDeath()
        {
            if (_deathHandled)
            {
                return;
            }
            _deathHandled = true;

            // Explosion particle.
            if (data != null && data.explosionPrefab != null)
            {
                Instantiate(data.explosionPrefab, transform.position, Quaternion.identity);
            }

            // Explosion SFX.
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSFX);
            }

            // Score reward + floating popup.
            if (data != null && GameManager.HasInstance)
            {
                GameManager.Instance.AddScore(data.scoreValue);
                if (SpaceShooter.UI.UIManager.HasInstance)
                {
                    SpaceShooter.UI.UIManager.Instance.ShowScorePopup(data.scoreValue, transform.position);
                }
            }

            // Power-up drop chance.
            if (data != null && data.powerUpDropChance > 0f && PowerUpSpawner.HasInstance)
            {
                PowerUpSpawner.Instance.TrySpawn(transform.position, data.powerUpDropChance);
            }

            // Notify the spawner so wave tracking updates.
            if (EnemySpawner.HasInstance)
            {
                EnemySpawner.Instance.EnemyDestroyed(this);
            }

            Destroy(gameObject);
        }
    }
}
