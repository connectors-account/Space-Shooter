using UnityEngine;
using SpaceShooter.Managers;
using SpaceShooter.Combat;

namespace SpaceShooter.Enemy
{
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank,
        Shooter,
        Boss
    }

    public class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] protected EnemyType enemyType = EnemyType.Basic;
        [SerializeField] protected int maxHealth = 20;
        [SerializeField] protected int currentHealth;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected int damage = 10;

        [Header("Movement")]
        [SerializeField] protected float horizontalAmplitude = 2f;
        [SerializeField] protected float horizontalFrequency = 1f;
        [SerializeField] protected bool useWaveMovement = false;

        [Header("Shooting")]
        [SerializeField] protected bool canShoot = false;
        [SerializeField] protected float fireRate = 2f;
        [SerializeField] protected GameObject bulletPrefab;
        [SerializeField] protected Transform firePoint;

        [Header("Power-up Drop")]
        [SerializeField] protected float powerUpDropChance = 0.1f;
        [SerializeField] protected GameObject[] powerUpPrefabs;

        [Header("Visual")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Color damageColor = Color.white;

        protected float nextFireTime;
        protected Vector3 startPosition;
        protected float timeAlive;
        protected Color originalColor;

        public int ScoreValue => scoreValue;
        public EnemyType Type => enemyType;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        protected virtual void Start()
        {
            startPosition = transform.position;
        }

        protected virtual void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver))
                return;

            timeAlive += Time.deltaTime;
            Move();
            
            if (canShoot)
                HandleShooting();

            CheckBounds();
        }

        protected virtual void Move()
        {
            Vector3 newPosition = transform.position;
            newPosition.y -= moveSpeed * Time.deltaTime;

            if (useWaveMovement)
            {
                newPosition.x = startPosition.x + Mathf.Sin(timeAlive * horizontalFrequency) * horizontalAmplitude;
            }

            transform.position = newPosition;
        }

        protected virtual void HandleShooting()
        {
            if (Time.time >= nextFireTime && bulletPrefab != null)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
        }

        protected virtual void Fire()
        {
            if (firePoint == null) return;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(Vector2.down, false, damage);
            }
            
            AudioManager.Instance?.PlaySound("EnemyShoot");
        }

        protected virtual void CheckBounds()
        {
            if (transform.position.y < -6f)
            {
                Destroy(gameObject);
            }
        }

        public virtual void TakeDamage(int damage)
        {
            currentHealth -= damage;
            StartCoroutine(FlashDamage());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected System.Collections.IEnumerator FlashDamage()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = damageColor;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = originalColor;
            }
        }

        protected virtual void Die()
        {
            GameManager.Instance?.AddScore(scoreValue);
            AudioManager.Instance?.PlaySound("EnemyExplosion");
            EffectsManager.Instance?.SpawnExplosion(transform.position, 1f);
            
            TryDropPowerUp();
            
            WaveManager.Instance?.OnEnemyDestroyed();
            Destroy(gameObject);
        }

        protected virtual void TryDropPowerUp()
        {
            if (powerUpPrefabs != null && powerUpPrefabs.Length > 0)
            {
                if (Random.value <= powerUpDropChance)
                {
                    int randomIndex = Random.Range(0, powerUpPrefabs.Length);
                    if (powerUpPrefabs[randomIndex] != null)
                    {
                        Instantiate(powerUpPrefabs[randomIndex], transform.position, Quaternion.identity);
                    }
                }
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("PlayerBullet"))
            {
                Bullet bullet = other.GetComponent<Bullet>();
                if (bullet != null)
                {
                    TakeDamage(bullet.Damage);
                    Destroy(other.gameObject);
                }
            }
            else if (other.CompareTag("Player"))
            {
                Die();
            }
        }

        public void Initialize(int healthMultiplier = 1, int scoreMultiplier = 1)
        {
            maxHealth *= healthMultiplier;
            currentHealth = maxHealth;
            scoreValue *= scoreMultiplier;
        }
    }
}
