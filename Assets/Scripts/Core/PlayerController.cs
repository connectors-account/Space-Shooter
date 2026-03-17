using UnityEngine;
using System.Collections;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Controls player ship movement, shooting, and power-up effects
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float boundaryPadding = 0.5f;
        
        [Header("Shooting Settings")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private float fireRate = 0.2f;
        [SerializeField] private AudioClip shootSound;
        
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invincibilityDuration = 1.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        // Private variables
        private int currentHealth;
        private float nextFireTime;
        private bool canShoot = true;
        private bool isInvincible = false;
        private bool hasShield = false;
        private bool hasRapidFire = false;
        private float rapidFireMultiplier = 2f;
        private bool hasTripleShot = false;
        
        private Camera mainCamera;
        private AudioSource audioSource;
        private Vector2 screenBounds;
        private float objectWidth;
        private float objectHeight;
        
        // Events
        public delegate void HealthChangedHandler(int currentHealth, int maxHealth);
        public event HealthChangedHandler OnHealthChanged;
        
        public delegate void PlayerDeathHandler();
        public event PlayerDeathHandler OnPlayerDeath;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool HasShield => hasShield;
        
        private void Awake()
        {
            mainCamera = Camera.main;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
                
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        private void Start()
        {
            currentHealth = maxHealth;
            CalculateScreenBounds();
            
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
                
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        private void CalculateScreenBounds()
        {
            screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
            
            if (spriteRenderer != null)
            {
                objectWidth = spriteRenderer.bounds.extents.x;
                objectHeight = spriteRenderer.bounds.extents.y;
            }
            else
            {
                objectWidth = 0.5f;
                objectHeight = 0.5f;
            }
        }
        
        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;
                
            HandleMovement();
            HandleShooting();
        }
        
        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            Vector3 movement = new Vector3(horizontal, vertical, 0f).normalized;
            transform.position += movement * moveSpeed * Time.deltaTime;
            
            // Clamp position to screen bounds
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x + objectWidth + boundaryPadding, screenBounds.x - objectWidth - boundaryPadding);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y + objectHeight + boundaryPadding, screenBounds.y - objectHeight - boundaryPadding);
            transform.position = clampedPosition;
        }
        
        private void HandleShooting()
        {
            if (Input.GetKey(KeyCode.Space) && canShoot && Time.time >= nextFireTime)
            {
                Shoot();
                float actualFireRate = hasRapidFire ? fireRate / rapidFireMultiplier : fireRate;
                nextFireTime = Time.time + actualFireRate;
            }
        }
        
        private void Shoot()
        {
            if (bulletPrefab == null) return;
            
            if (hasTripleShot)
            {
                ShootTriple();
            }
            else
            {
                ShootSingle();
            }
            
            PlayShootSound();
        }
        
        private void ShootSingle()
        {
            if (firePoints != null && firePoints.Length > 0)
            {
                foreach (Transform firePoint in firePoints)
                {
                    if (firePoint != null)
                        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                }
            }
            else
            {
                Instantiate(bulletPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
        
        private void ShootTriple()
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            
            // Center bullet
            Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            
            // Left bullet
            GameObject leftBullet = Instantiate(bulletPrefab, spawnPos + Vector3.left * 0.3f, Quaternion.Euler(0, 0, 15));
            if (leftBullet.TryGetComponent<Bullet>(out var lb))
                lb.SetDirection(new Vector2(-0.2f, 1f).normalized);
            
            // Right bullet
            GameObject rightBullet = Instantiate(bulletPrefab, spawnPos + Vector3.right * 0.3f, Quaternion.Euler(0, 0, -15));
            if (rightBullet.TryGetComponent<Bullet>(out var rb))
                rb.SetDirection(new Vector2(0.2f, 1f).normalized);
        }
        
        private void PlayShootSound()
        {
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound, 0.5f);
            }
        }
        
        public void TakeDamage(int damage)
        {
            if (isInvincible) return;
            
            if (hasShield)
            {
                DeactivateShield();
                return;
            }
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityCoroutine());
            }
        }
        
        private IEnumerator InvincibilityCoroutine()
        {
            isInvincible = true;
            
            // Flash effect
            float flashDuration = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < invincibilityDuration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(flashDuration);
                elapsed += flashDuration;
            }
            
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
                
            isInvincible = false;
        }
        
        private void Die()
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            
            OnPlayerDeath?.Invoke();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            
            gameObject.SetActive(false);
        }
        
        public void Heal(int amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void ActivateShield(float duration)
        {
            StartCoroutine(ShieldCoroutine(duration));
        }
        
        private IEnumerator ShieldCoroutine(float duration)
        {
            hasShield = true;
            if (shieldVisual != null)
                shieldVisual.SetActive(true);
                
            yield return new WaitForSeconds(duration);
            
            DeactivateShield();
        }
        
        private void DeactivateShield()
        {
            hasShield = false;
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
        }
        
        public void ActivateRapidFire(float duration)
        {
            StartCoroutine(RapidFireCoroutine(duration));
        }
        
        private IEnumerator RapidFireCoroutine(float duration)
        {
            hasRapidFire = true;
            yield return new WaitForSeconds(duration);
            hasRapidFire = false;
        }
        
        public void ActivateTripleShot(float duration)
        {
            StartCoroutine(TripleShotCoroutine(duration));
        }
        
        private IEnumerator TripleShotCoroutine(float duration)
        {
            hasTripleShot = true;
            yield return new WaitForSeconds(duration);
            hasTripleShot = false;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("EnemyBullet"))
            {
                Bullet bullet = other.GetComponent<Bullet>();
                if (bullet != null)
                {
                    TakeDamage(bullet.Damage);
                }
                else
                {
                    TakeDamage(10);
                }
                Destroy(other.gameObject);
            }
            else if (other.CompareTag("Enemy"))
            {
                TakeDamage(20);
            }
        }
    }
}
