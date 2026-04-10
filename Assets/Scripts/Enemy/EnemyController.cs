using UnityEngine;
using SpaceShooter.Effects;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Enemy types: Basic (moves straight down), Zigzag (weaves left/right), Tank (slow but tough).
    /// Handles AI movement, shooting, health, and drops.
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        // ---- Enemy Type Enum ----
        public enum EnemyType
        {
            Basic,   // Moves straight down, occasional shots
            Zigzag,  // Weaves side to side, faster shots
            Tank     // Slow, high HP, burst fire
        }

        [Header("Enemy Configuration")]
        [SerializeField] private EnemyType enemyType = EnemyType.Basic;
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private int contactDamage = 20;

        [Header("Shooting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private float bulletSpeed = 6f;

        [Header("Zigzag Settings")]
        [SerializeField] private float zigzagAmplitude = 3f;
        [SerializeField] private float zigzagFrequency = 2f;

        [Header("Tank Settings")]
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstInterval = 0.15f;

        [Header("Power-Up Drop")]
        [SerializeField] private GameObject[] powerUpPrefabs;  // Assign in inspector
        [SerializeField] [Range(0f, 1f)] private float dropChance = 0.15f;

        // ---- Runtime State ----
        private int currentHealth;
        private float nextFireTime;
        private float spawnX;  // used for zigzag calculation
        private float aliveTime;
        private bool isDead;

        // ---- Events ----
        public event System.Action<int> OnEnemyDestroyed;  // passes scoreValue

        // ---- Public Properties ----
        public EnemyType Type => enemyType;
        public int ScoreValue => scoreValue;

        private void Start()
        {
            currentHealth = maxHealth;
            spawnX = transform.position.x;
            aliveTime = 0f;
            isDead = false;

            // Randomize first shot timing
            nextFireTime = Time.time + Random.Range(0.5f, fireRate);

            // Apply type-specific defaults
            ApplyTypeDefaults();
        }

        /// <summary>
        /// Sets baseline stats per enemy type if not overridden in Inspector.
        /// </summary>
        private void ApplyTypeDefaults()
        {
            switch (enemyType)
            {
                case EnemyType.Basic:
                    if (maxHealth == 30) maxHealth = 30;
                    if (moveSpeed == 3f) moveSpeed = 3f;
                    if (scoreValue == 100) scoreValue = 100;
                    break;

                case EnemyType.Zigzag:
                    if (maxHealth == 30) maxHealth = 20;
                    if (moveSpeed == 3f) moveSpeed = 4f;
                    if (scoreValue == 100) scoreValue = 150;
                    fireRate = 1f;
                    break;

                case EnemyType.Tank:
                    if (maxHealth == 30) maxHealth = 80;
                    if (moveSpeed == 3f) moveSpeed = 1.5f;
                    if (scoreValue == 100) scoreValue = 300;
                    fireRate = 2.5f;
                    break;
            }
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (isDead) return;

            aliveTime += Time.deltaTime;

            HandleMovement();
            HandleShooting();
            CheckOutOfBounds();
        }

        // ========== MOVEMENT AI ==========
        private void HandleMovement()
        {
            switch (enemyType)
            {
                case EnemyType.Basic:
                    // Straight downward movement
                    transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                    break;

                case EnemyType.Zigzag:
                    // Sinusoidal horizontal movement + downward drift
                    float newX = spawnX + Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
                    float newY = transform.position.y - moveSpeed * Time.deltaTime;
                    transform.position = new Vector3(newX, newY, transform.position.z);
                    break;

                case EnemyType.Tank:
                    // Slow downward movement, stops at a y position to act as turret
                    if (transform.position.y > 2f)
                    {
                        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                    }
                    break;
            }
        }

        // ========== SHOOTING AI ==========
        private void HandleShooting()
        {
            if (Time.time < nextFireTime) return;
            if (bulletPrefab == null || firePoint == null) return;

            switch (enemyType)
            {
                case EnemyType.Basic:
                    FireSingleShot();
                    break;

                case EnemyType.Zigzag:
                    FireAimedShot();
                    break;

                case EnemyType.Tank:
                    StartCoroutine(FireBurst());
                    break;
            }

            nextFireTime = Time.time + fireRate;
        }

        /// <summary>Fires a single bullet straight down.</summary>
        private void FireSingleShot()
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.tag = "EnemyBullet";

            Weapons.BulletController bc = bullet.GetComponent<Weapons.BulletController>();
            if (bc != null)
            {
                bc.SetDirection(Vector2.down);
                bc.SetSpeed(bulletSpeed);
            }
        }

        /// <summary>Fires a bullet aimed at the player's current position.</summary>
        private void FireAimedShot()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Vector2 dir = Vector2.down;

            if (playerObj != null)
            {
                dir = ((Vector2)playerObj.transform.position - (Vector2)firePoint.position).normalized;
            }

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.tag = "EnemyBullet";

            Weapons.BulletController bc = bullet.GetComponent<Weapons.BulletController>();
            if (bc != null)
            {
                bc.SetDirection(dir);
                bc.SetSpeed(bulletSpeed);
            }
        }

        /// <summary>Fires a burst of bullets (Tank specialty).</summary>
        private System.Collections.IEnumerator FireBurst()
        {
            for (int i = 0; i < burstCount; i++)
            {
                FireSingleShot();
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // ========== HEALTH & DAMAGE ==========
        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;

            // Flash white briefly
            StartCoroutine(DamageFlash());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;

            // Notify GameManager of score
            OnEnemyDestroyed?.Invoke(scoreValue);

            // Also notify via GameManager singleton
            Managers.GameManager.Instance?.AddScore(scoreValue);

            // Try to drop a power-up
            TryDropPowerUp();

            ExplosionParticles.Spawn(transform.position);
            Managers.AudioManager.Instance?.PlayExplosionSound();

            // Destroy the enemy
            Destroy(gameObject);
        }

        private void TryDropPowerUp()
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

            if (Random.value <= dropChance)
            {
                int index = Random.Range(0, powerUpPrefabs.Length);
                if (powerUpPrefabs[index] != null)
                {
                    Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
                }
            }
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            sr.color = original;
        }

        // ========== BOUNDS CHECK ==========
        private void CheckOutOfBounds()
        {
            // Destroy if fallen below the screen
            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }

        // ========== COLLISION ==========
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDead) return;

            if (other.CompareTag("PlayerBullet"))
            {
                Weapons.BulletController bullet = other.GetComponent<Weapons.BulletController>();
                if (bullet != null)
                {
                    TakeDamage(bullet.Damage);
                }
                Destroy(other.gameObject);
            }
        }

        /// <summary>
        /// Configure this enemy externally (used by SpawnManager).
        /// </summary>
        public void Configure(EnemyType type, int health, float speed, int score, float shootRate)
        {
            enemyType = type;
            maxHealth = health;
            currentHealth = health;
            moveSpeed = speed;
            scoreValue = score;
            fireRate = shootRate;
        }
    }
}
