// ============================================================================
// EnemyBase.cs — Base class for all enemy types
// Handles health, scoring, power-up drops, and off-screen cleanup.
// Concrete enemy movement is implemented in subclasses.
// ============================================================================
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;

namespace SpaceShooter.Enemies
{
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected int maxHealth = 1;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected string poolTag = "Enemy";      // for returning to pool

        [Header("Shooting (optional)")]
        [SerializeField] protected bool canShoot = false;
        [SerializeField] protected float shootInterval = 2f;
        [SerializeField] protected string bulletPoolTag = "EnemyBullet";

        [Header("Power-Up Drop")]
        [SerializeField, Range(0f, 1f)] protected float powerUpDropChance = 0.15f;

        protected int _currentHealth;
        protected float _shootTimer;

        protected virtual void OnEnable()
        {
            _currentHealth = maxHealth;
            _shootTimer = shootInterval;   // first shot after full interval
        }

        protected virtual void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            Move();

            // Off-screen cleanup
            if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position))
            {
                ReturnToPool();
                return;
            }

            // Shooting
            if (canShoot)
            {
                _shootTimer -= Time.deltaTime;
                if (_shootTimer <= 0f)
                {
                    Shoot();
                    _shootTimer = shootInterval;
                }
            }
        }

        // ---- Subclass must implement movement behaviour ----
        protected abstract void Move();

        // ---- Damage / destruction ----
        public void TakeDamage(int amount)
        {
            _currentHealth -= amount;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            GameManager.Instance?.AddScore(scoreValue);
            AudioManager.Instance?.PlaySFX("Explosion");

            // Chance to drop a power-up
            if (Random.value < powerUpDropChance)
            {
                PowerUpSpawner.Instance?.SpawnRandom(transform.position);
            }

            ReturnToPool();
        }

        // ---- Shooting (default: straight down) ----
        protected virtual void Shoot()
        {
            if (ObjectPool.Instance == null) return;
            ObjectPool.Instance.Get(bulletPoolTag, transform.position + Vector3.down * 0.5f, Quaternion.identity);
            AudioManager.Instance?.PlaySFX("EnemyShoot");
        }

        // ---- Pool recycling ----
        protected void ReturnToPool()
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.ReturnToPool(poolTag, gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    // ====================================================================
    // Placeholder static reference for the power-up spawner (set at runtime).
    // ====================================================================
    public class PowerUpSpawner : MonoBehaviour
    {
        public static PowerUpSpawner Instance { get; private set; }

        [SerializeField] private GameObject[] powerUpPrefabs;   // assign in Inspector

        private void Awake() { Instance = this; }

        public void SpawnRandom(Vector3 position)
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
            int index = Random.Range(0, powerUpPrefabs.Length);
            Instantiate(powerUpPrefabs[index], position, Quaternion.identity);
        }
    }
}
