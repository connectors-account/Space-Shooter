// =============================================================================
// EnemyBase.cs — Base class for all enemy types
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Defines the different enemy archetypes.
    /// </summary>
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank,
        Boss
    }

    /// <summary>
    /// Base enemy class with health, movement, shooting, and scoring.
    /// Override virtual methods in subclasses for specialized behaviors.
    /// </summary>
    public class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Config")]
        [SerializeField] protected EnemyType enemyType = EnemyType.Basic;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected int maxHealth = 3;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected int contactDamage = 1;

        [Header("Shooting")]
        [SerializeField] protected float fireRate = 1.5f;
        [SerializeField] protected Weapons.BulletPattern bulletPattern;

        [Header("Drops")]
        [SerializeField, Range(0f, 1f)] protected float powerUpDropChance = 0.15f;

        [Header("Visual")]
        [SerializeField] protected GameObject explosionPrefab;

        protected int currentHealth;
        protected float nextFireTime;
        protected Transform playerTransform;
        protected bool isAlive = true;

        /// <summary>Score awarded when this enemy dies.</summary>
        public int ScoreValue => scoreValue;

        /// <summary>The type of this enemy.</summary>
        public EnemyType Type => enemyType;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        protected virtual void Update()
        {
            if (!isAlive) return;
            Move();
            TryShoot();
            CheckBounds();
        }

        /// <summary>
        /// Default downward movement. Override for custom patterns.
        /// </summary>
        protected virtual void Move()
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime, Space.World);
        }

        /// <summary>
        /// Attempts to fire based on fireRate timer.
        /// </summary>
        protected virtual void TryShoot()
        {
            if (bulletPattern == null) return;
            if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + fireRate;
            bulletPattern.Fire(transform.position, Vector2.down);
        }

        /// <summary>
        /// Destroy self if far below screen.
        /// </summary>
        protected virtual void CheckBounds()
        {
            if (transform.position.y < -12f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Apply damage to this enemy.
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            if (!isAlive) return;
            currentHealth -= damage;
            Managers.SoundManager.Instance?.PlaySFX("enemy_hit");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Handles enemy death: score, effects, drops, destruction.
        /// </summary>
        protected virtual void Die()
        {
            isAlive = false;

            // Award score
            Managers.GameManager.Instance?.AddScore(scoreValue);

            // Play explosion
            Managers.SoundManager.Instance?.PlaySFX("enemy_explode");
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Chance to drop power-up
            if (Random.value <= powerUpDropChance)
            {
                Managers.GameManager.Instance?.SpawnRandomPowerUp(transform.position);
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAlive) return;

            if (other.CompareTag("PlayerBullet"))
            {
                Weapons.Bullet bullet = other.GetComponent<Weapons.Bullet>();
                int dmg = bullet != null ? bullet.Damage : 1;
                TakeDamage(dmg);
                Destroy(other.gameObject);
            }
            else if (other.CompareTag("Player"))
            {
                Player.PlayerController pc = other.GetComponent<Player.PlayerController>();
                if (pc != null) pc.TakeDamage(contactDamage);
            }
        }
    }
}
