using UnityEngine;

namespace SpaceShooter.Enemies
{
    /// <summary>
    /// Base class for all enemy types. Handles health, scoring, and power-up drops.
    /// Subclasses override movement and shooting behaviour.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] protected int maxHealth = 1;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected int scoreValue = 100;

        [Header("Shooting")]
        [SerializeField] protected bool canShoot = false;
        [SerializeField] protected float shootInterval = 2f;
        [SerializeField] protected string bulletPoolTag = "EnemyBullet";

        [Header("Drops")]
        [SerializeField, Range(0f, 1f)] protected float powerUpDropChance = 0.15f;

        // State
        protected Rigidbody2D rb;
        protected int currentHealth;
        protected float shootTimer;
        protected bool isInitialized;

        public int ScoreValue => scoreValue;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        protected virtual void OnEnable()
        {
            currentHealth = maxHealth;
            shootTimer = shootInterval;
            isInitialized = true;
        }

        protected virtual void Update()
        {
            if (!isInitialized) return;

            Move();
            HandleShooting();
            CheckOffScreen();
        }

        /// <summary>
        /// Override in subclasses to define movement pattern.
        /// </summary>
        protected virtual void Move()
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        }

        protected virtual void HandleShooting()
        {
            if (!canShoot) return;

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootInterval;
            }
        }

        protected virtual void Shoot()
        {
            GameObject bullet = Managers.ObjectPoolManager.Instance?.GetFromPool(
                bulletPoolTag, transform.position + Vector3.down * 0.5f, Quaternion.identity);

            if (bullet != null)
            {
                Weapons.Bullet bulletComp = bullet.GetComponent<Weapons.Bullet>();
                if (bulletComp != null)
                {
                    bulletComp.Initialize(Vector2.down, false);
                }
            }

            Managers.AudioManager.Instance?.PlaySFX("EnemyShoot");
        }

        public virtual void TakeDamage(int damage)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Flash red briefly
                StartCoroutine(FlashDamage());
            }
        }

        private System.Collections.IEnumerator FlashDamage()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color original = sr.color;
                sr.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                if (sr != null && gameObject.activeInHierarchy)
                    sr.color = original;
            }
        }

        protected virtual void Die()
        {
            // Add score
            Managers.GameManager.Instance?.AddScore(scoreValue);

            // Explosion effect
            Effects.ExplosionManager.Instance?.SpawnExplosion(transform.position, Effects.ExplosionType.Medium);
            Managers.AudioManager.Instance?.PlaySFX("EnemyDeath");

            // Try dropping a power-up
            if (Random.value <= powerUpDropChance)
            {
                PowerUps.PowerUpSpawner.Instance?.SpawnRandomPowerUp(transform.position);
            }

            gameObject.SetActive(false);
        }

        protected void CheckOffScreen()
        {
            if (Camera.main == null) return;
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.y < -0.15f || vp.x < -0.3f || vp.x > 1.3f)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Damage player is handled by PlayerController
                Die();
            }
        }

        /// <summary>
        /// Called by the wave spawner to configure enemy stats for difficulty scaling.
        /// </summary>
        public void ConfigureForWave(int waveNumber)
        {
            float difficultyMultiplier = 1f + (waveNumber - 1) * 0.1f;
            maxHealth = Mathf.CeilToInt(maxHealth * difficultyMultiplier);
            moveSpeed *= (1f + (waveNumber - 1) * 0.05f);
            currentHealth = maxHealth;
        }
    }
}
