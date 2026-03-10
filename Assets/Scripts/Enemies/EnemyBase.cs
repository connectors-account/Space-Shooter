using UnityEngine;
using System;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Base class for all enemy types
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected int maxHealth = 30;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected int contactDamage = 20;
        
        [Header("Shooting")]
        [SerializeField] protected bool canShoot = true;
        [SerializeField] protected GameObject bulletPrefab;
        [SerializeField] protected float fireRate = 2f;
        [SerializeField] protected Transform[] firePoints;
        
        [Header("Visual Effects")]
        [SerializeField] protected GameObject explosionPrefab;
        [SerializeField] protected GameObject hitEffectPrefab;
        
        [Header("Audio")]
        [SerializeField] protected AudioClip shootSound;
        [SerializeField] protected AudioClip hitSound;
        [SerializeField] protected AudioClip deathSound;
        
        protected int currentHealth;
        protected float nextFireTime;
        protected AudioSource audioSource;
        protected bool isAlive = true;
        
        public event Action<EnemyBase> OnEnemyDestroyed;
        
        public int ScoreValue => scoreValue;
        public bool IsAlive => isAlive;
        
        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }
        
        protected virtual void Update()
        {
            if (!isAlive) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
            
            Move();
            
            if (canShoot && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            
            CheckBounds();
        }
        
        protected abstract void Move();
        
        protected virtual void Shoot()
        {
            if (bulletPrefab == null) return;
            
            if (firePoints != null && firePoints.Length > 0)
            {
                foreach (Transform firePoint in firePoints)
                {
                    if (firePoint != null)
                    {
                        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                        Bullet bulletScript = bullet.GetComponent<Bullet>();
                        if (bulletScript != null)
                        {
                            bulletScript.SetDirection(Vector2.down);
                        }
                    }
                }
            }
            else
            {
                GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetDirection(Vector2.down);
                }
            }
            
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound, 0.3f);
            }
        }
        
        public virtual void TakeDamage(int damage)
        {
            if (!isAlive) return;
            
            currentHealth -= damage;
            
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f);
            }
            
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound, 0.5f);
            }
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        protected virtual void Die()
        {
            if (!isAlive) return;
            isAlive = false;
            
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnemyDestroyed(scoreValue);
            }
            
            OnEnemyDestroyed?.Invoke(this);
            
            Destroy(gameObject);
        }
        
        protected virtual void CheckBounds()
        {
            // Destroy if too far below screen
            if (transform.position.y < -10f)
            {
                OnEnemyDestroyed?.Invoke(this);
                Destroy(gameObject);
            }
        }
        
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(contactDamage);
                }
            }
        }
    }
}
