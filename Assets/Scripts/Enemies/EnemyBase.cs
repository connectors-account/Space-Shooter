using SpaceShooter.Combat;
using SpaceShooter.Core;
using SpaceShooter.PowerUps;
using UnityEngine;

namespace SpaceShooter.Enemies
{
    [RequireComponent(typeof(Health))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] private int scoreValue = 100;
        [SerializeField] protected float moveSpeed = 2.5f;

        [Header("Shooting")]
        [SerializeField] private bool canShoot = true;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireInterval = 1.5f;
        [SerializeField] private int projectileDamage = 10;
        [SerializeField] private float projectileSpeed = 8f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shootSfx;
        [SerializeField] private AudioClip deathSfx;

        [Header("Drops")]
        [SerializeField] private PowerUpDropper powerUpDropper;

        protected Health Health { get; private set; }

        private float nextShotTime;

        protected virtual void Awake()
        {
            Health = GetComponent<Health>();
        }

        protected virtual void OnEnable()
        {
            Health.OnDied += HandleDeath;
        }

        protected virtual void OnDisable()
        {
            Health.OnDied -= HandleDeath;
        }

        protected virtual void Update()
        {
            MoveEnemy();
            HandleShooting();
        }

        protected abstract Vector2 GetMovementDirection();

        private void MoveEnemy()
        {
            Vector2 direction = GetMovementDirection().normalized;
            transform.Translate(direction * (moveSpeed * Time.deltaTime), Space.World);
        }

        private void HandleShooting()
        {
            if (!canShoot || projectilePrefab == null || firePoint == null)
            {
                return;
            }

            if (Time.time < nextShotTime)
            {
                return;
            }

            nextShotTime = Time.time + fireInterval;

            GameObject projectileGo = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            if (projectileGo.TryGetComponent(out Projectile projectile))
            {
                projectile.Initialize(Vector2.down, ProjectileOwner.Enemy, projectileDamage, projectileSpeed);
            }

            if (audioSource != null && shootSfx != null)
            {
                audioSource.PlayOneShot(shootSfx);
            }
        }

        private void HandleDeath(Health deadHealth)
        {
            ScoreManager.AddScore(scoreValue);

            if (powerUpDropper != null)
            {
                powerUpDropper.TryDrop(transform.position);
            }

            if (audioSource != null && deathSfx != null)
            {
                AudioSource.PlayClipAtPoint(deathSfx, transform.position);
            }
        }

        private void OnBecameInvisible()
        {
            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (other.TryGetComponent(out Health playerHealth))
            {
                playerHealth.TakeDamage(25);
            }

            Health.TakeDamage(Health.MaxHealth);
        }
    }
}
