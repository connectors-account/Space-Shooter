using SpaceShooter.Audio;
using SpaceShooter.Core;
using SpaceShooter.Player;
using SpaceShooter.Weapons;
using UnityEngine;

namespace SpaceShooter.Enemies
{
    [RequireComponent(typeof(Health))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected float collisionDamage = 20f;

        [Header("Shooting")]
        [SerializeField] protected WeaponSystem weaponSystem;
        [SerializeField] protected bool canShoot = false;
        [SerializeField] protected float fireInterval = 1.8f;
        [SerializeField] protected BulletPattern shootPattern = BulletPattern.Single;

        private Health health;
        private float fireTimer;

        protected virtual void Awake()
        {
            health = GetComponent<Health>();
        }

        protected virtual void OnEnable()
        {
            health.OnDied += HandleDeath;
            health.ResetHealth();
            fireTimer = fireInterval;
        }

        protected virtual void OnDisable()
        {
            health.OnDied -= HandleDeath;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            Move();
            TryShoot();
        }

        protected abstract void Move();

        private void TryShoot()
        {
            if (!canShoot || weaponSystem == null)
            {
                return;
            }

            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f)
            {
                return;
            }

            fireTimer = fireInterval;
            weaponSystem.FirePattern(shootPattern, Vector2.down);
            AudioManager.Instance?.PlayShoot();
        }

        public void TakeDamage(float amount)
        {
            health.TakeDamage(amount);
        }

        protected virtual void HandleDeath()
        {
            GameManager.Instance?.AddScore(scoreValue);
            AudioManager.Instance?.PlayExplosion();
            Destroy(gameObject);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Bullet bullet) && bullet.FromPlayer)
            {
                TakeDamage(bullet.Damage);
                return;
            }

            if (other.TryGetComponent(out PlayerController playerController))
            {
                playerController.TakeDamage(collisionDamage);
                HandleDeath();
            }
        }
    }
}
