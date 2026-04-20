using System.Collections;
using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Enemy AI with three ship behaviors and projectile patterns.
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyType
        {
            Basic,
            Zigzag,
            Tank
        }

        [Header("Type")]
        [SerializeField] private EnemyType enemyType = EnemyType.Basic;

        [Header("Stats")]
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private int contactDamage = 20;
        [SerializeField] private int scoreValue = 100;

        [Header("Shooting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 7f;
        [SerializeField] private float fireInterval = 1.4f;

        [Header("Zigzag Movement")]
        [SerializeField] private float zigzagAmplitude = 2.2f;
        [SerializeField] private float zigzagFrequency = 3.2f;

        [Header("Tank Pattern")]
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstStepDelay = 0.1f;
        [SerializeField] private float spreadAngle = 20f;

        [Header("Drops")]
        [SerializeField] private GameObject[] powerUpPrefabs;
        [SerializeField, Range(0f, 1f)] private float powerUpDropChance = 0.18f;

        [Header("VFX")]
        [SerializeField] private GameObject explosionPrefab;

        private int currentHealth;
        private float nextFireTime;
        private float lifetime;
        private float spawnX;
        private bool isDead;

        public event System.Action<int> OnEnemyDestroyed;

        private void Start()
        {
            ApplyTypeDefaults();
            currentHealth = maxHealth;
            nextFireTime = Time.time + Random.Range(0.2f, fireInterval);
            spawnX = transform.position.x;
        }

        private void Update()
        {
            if (isDead || GameIsNotRunning())
            {
                return;
            }

            lifetime += Time.deltaTime;
            HandleMovement();
            HandleShooting();

            if (transform.position.y < -7.2f)
            {
                Destroy(gameObject);
            }
        }

        private bool GameIsNotRunning()
        {
            return Managers.GameManager.Instance != null && Managers.GameManager.Instance.CurrentState != Managers.GameManager.GameState.Playing;
        }

        private void ApplyTypeDefaults()
        {
            switch (enemyType)
            {
                case EnemyType.Basic:
                    maxHealth = Mathf.Max(maxHealth, 25);
                    scoreValue = Mathf.Max(scoreValue, 100);
                    break;

                case EnemyType.Zigzag:
                    maxHealth = Mathf.Max(15, maxHealth - 10);
                    moveSpeed = Mathf.Max(3.8f, moveSpeed);
                    fireInterval = Mathf.Min(fireInterval, 1.1f);
                    scoreValue = Mathf.Max(scoreValue, 150);
                    break;

                case EnemyType.Tank:
                    maxHealth = Mathf.Max(80, maxHealth);
                    moveSpeed = Mathf.Min(moveSpeed, 1.8f);
                    fireInterval = Mathf.Max(fireInterval, 2.1f);
                    scoreValue = Mathf.Max(scoreValue, 280);
                    break;
            }
        }

        private void HandleMovement()
        {
            if (enemyType == EnemyType.Basic)
            {
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                return;
            }

            if (enemyType == EnemyType.Zigzag)
            {
                float x = spawnX + Mathf.Sin(lifetime * zigzagFrequency) * zigzagAmplitude;
                float y = transform.position.y - moveSpeed * Time.deltaTime;
                transform.position = new Vector3(x, y, transform.position.z);
                return;
            }

            // Tank
            if (transform.position.y > 1.8f)
            {
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
            }
        }

        private void HandleShooting()
        {
            if (bulletPrefab == null || firePoint == null || Time.time < nextFireTime)
            {
                return;
            }

            switch (enemyType)
            {
                case EnemyType.Basic:
                    FireBullet(Vector2.down);
                    break;
                case EnemyType.Zigzag:
                    FireAimedBullet();
                    break;
                case EnemyType.Tank:
                    StartCoroutine(FireTankBurst());
                    break;
            }

            nextFireTime = Time.time + fireInterval;
        }

        private void FireAimedBullet()
        {
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            Vector2 direction = player == null ? Vector2.down : ((Vector2)player.position - (Vector2)firePoint.position).normalized;
            FireBullet(direction);
        }

        private IEnumerator FireTankBurst()
        {
            for (int i = 0; i < burstCount; i++)
            {
                float t = burstCount <= 1 ? 0.5f : i / (float)(burstCount - 1);
                float angle = Mathf.Lerp(-spreadAngle, spreadAngle, t);
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.down;
                FireBullet(direction.normalized);
                yield return new WaitForSeconds(burstStepDelay);
            }
        }

        private void FireBullet(Vector2 direction)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.tag = "EnemyBullet";

            Weapons.BulletController bulletController = bullet.GetComponent<Weapons.BulletController>();
            if (bulletController != null)
            {
                bulletController.Configure(direction, bulletSpeed, 12, false);
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDead)
            {
                return;
            }

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            SpawnExplosion();
            TryDropPowerUp();
            Managers.AudioManager.Instance?.PlayExplosionSound();
            OnEnemyDestroyed?.Invoke(scoreValue);
            Destroy(gameObject);
        }

        private void SpawnExplosion()
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                GameObject explosion = new GameObject("Explosion");
                explosion.transform.position = transform.position;
                explosion.AddComponent<SpaceShooter.Utils.ExplosionEffect>();
            }
        }

        private void TryDropPowerUp()
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0 || Random.value > powerUpDropChance)
            {
                return;
            }

            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            if (powerUpPrefabs[randomIndex] != null)
            {
                Instantiate(powerUpPrefabs[randomIndex], transform.position, Quaternion.identity);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDead)
            {
                return;
            }

            if (other.CompareTag("PlayerBullet"))
            {
                Weapons.BulletController bullet = other.GetComponent<Weapons.BulletController>();
                int damage = bullet != null ? bullet.Damage : 10;
                TakeDamage(damage);
                Destroy(other.gameObject);
                return;
            }

            if (other.CompareTag("Player"))
            {
                SpaceShooter.Player.PlayerController player = other.GetComponent<SpaceShooter.Player.PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(contactDamage);
                }

                Die();
            }
        }
    }
}
