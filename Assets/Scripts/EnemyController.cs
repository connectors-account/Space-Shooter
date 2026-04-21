using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Handles enemy movement patterns, firing behavior, and combat state.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        public enum EnemyType
        {
            Scout,
            ZigZag,
            Tank
        }

        [Header("Enemy Type")]
        [SerializeField] private EnemyType enemyType = EnemyType.Scout;

        [Header("Base Stats")]
        [SerializeField] private float moveSpeed = 2.8f;
        [SerializeField] private int maxHealth = 2;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private int scoreValue = 100;

        [Header("Movement")]
        [SerializeField] private float zigZagAmplitude = 1.2f;
        [SerializeField] private float zigZagFrequency = 2.3f;

        [Header("Shooting")]
        [SerializeField] private bool canShoot = true;
        [SerializeField] private GameObject enemyBulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireInterval = 1.8f;
        [SerializeField] private float bulletSpeed = 9f;
        [SerializeField] private int bulletDamage = 1;

        private int currentHealth;
        private bool isAlive = true;
        private float nextFireTime;
        private float spawnX;
        private float lifeTimer;

        public DamageTeam Team => DamageTeam.Enemy;
        public bool IsAlive => isAlive;

        private void Start()
        {
            currentHealth = maxHealth;
            spawnX = transform.position.x;
            nextFireTime = Time.time + Random.Range(0.5f, fireInterval);
        }

        private void Update()
        {
            if (!isAlive || GameManager.Instance == null || !GameManager.Instance.IsGameplayActive)
            {
                return;
            }

            lifeTimer += Time.deltaTime;
            Move();
            Shoot();

            if (transform.position.y < -7f)
            {
                DestroyEnemy(registerKill: false);
            }
        }

        public void ConfigureFromWave(int waveNumber)
        {
            float scale = 1f + (waveNumber - 1) * 0.09f;
            moveSpeed *= scale;
            maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * (1f + (waveNumber - 1) * 0.12f)));
            currentHealth = maxHealth;
            scoreValue = Mathf.RoundToInt(scoreValue * scale);
        }

        private void Move()
        {
            Vector3 velocity = Vector3.down * moveSpeed;

            if (enemyType == EnemyType.ZigZag)
            {
                float xOffset = Mathf.Sin(lifeTimer * zigZagFrequency) * zigZagAmplitude;
                transform.position = new Vector3(spawnX + xOffset, transform.position.y, transform.position.z);
            }

            transform.position += velocity * Time.deltaTime;
        }

        private void Shoot()
        {
            if (!canShoot || enemyBulletPrefab == null || Time.time < nextFireTime)
            {
                return;
            }

            nextFireTime = Time.time + Mathf.Max(0.35f, fireInterval);
            Vector3 bulletOrigin = firePoint != null ? firePoint.position : transform.position + Vector3.down * 0.5f;

            GameObject bullet = Instantiate(enemyBulletPrefab, bulletOrigin, Quaternion.identity);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController != null)
            {
                bulletController.Initialize(DamageTeam.Enemy, Vector2.down, bulletDamage, bulletSpeed);
            }

            AudioManager.Instance?.PlaySfx(AudioManager.SfxType.EnemyShoot);
        }

        public void TakeDamage(int amount, DamageTeam sourceTeam)
        {
            if (!isAlive)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, amount));
            if (currentHealth <= 0)
            {
                DestroyEnemy(registerKill: true);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAlive || other == null)
            {
                return;
            }

            // Direct body collision with player should hurt both.
            Component component = other.GetComponent(typeof(IDamageable));
            IDamageable damageable = component as IDamageable;
            if (damageable != null && damageable.Team == DamageTeam.Player)
            {
                damageable.TakeDamage(contactDamage, DamageTeam.Enemy);
                DestroyEnemy(registerKill: false);
            }
        }

        private void DestroyEnemy(bool registerKill)
        {
            if (!isAlive)
            {
                return;
            }

            isAlive = false;

            if (registerKill)
            {
                GameManager.Instance?.RegisterEnemyDestroyed(scoreValue, transform.position);
                AudioManager.Instance?.PlaySfx(AudioManager.SfxType.Explosion);
            }
            else
            {
                GameManager.Instance?.RegisterEnemyDespawned();
            }

            Destroy(gameObject);
        }
    }
}
