using System.Collections;
using UnityEngine;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Handles player movement, firing, health, and temporary power-up states.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float smoothing = 0.06f;
        [SerializeField] private Vector2 minBounds = new Vector2(-8.5f, -4.5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(8.5f, 4.5f);

        [Header("Combat")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float baseFireInterval = 0.22f;
        [SerializeField] private float rapidFireInterval = 0.09f;
        [SerializeField] private int bulletDamage = 15;
        [SerializeField] private float bulletSpeed = 14f;

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invulnerabilityDuration = 1f;

        [Header("Power-ups")]
        [SerializeField] private float rapidFireDuration = 5f;
        [SerializeField] private float shieldDuration = 7f;
        [SerializeField] private GameObject shieldVisual;

        [Header("VFX")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.45f, 0.45f, 1f);
        [SerializeField] private GameObject explosionPrefab;

        private int currentHealth;
        private float nextShotTime;
        private float rapidFireEndTime;
        private float shieldEndTime;
        private float invulnerabilityEndTime;

        private bool isRapidFireActive;
        private bool isShieldActive;
        private bool isDead;

        private Vector2 moveVelocity;
        private Vector2 smoothDampVelocity;
        private Color baseColor = Color.white;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsShieldActive => isShieldActive;
        public bool IsRapidFireActive => isRapidFireActive;
        public bool IsDead => isDead;

        public event System.Action<int, int> OnHealthChanged;
        public event System.Action<bool, bool> OnPowerUpStateChanged;
        public event System.Action OnPlayerDeath;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
            }
        }

        private void Start()
        {
            ResetPlayer();
        }

        private void Update()
        {
            if (isDead || GameNotActive())
            {
                return;
            }

            HandleMovement();
            HandleShooting();
            TickPowerUpTimers();
            TickInvulnerabilityBlink();
        }

        private bool GameNotActive()
        {
            return Managers.GameManager.Instance != null && Managers.GameManager.Instance.CurrentState != Managers.GameManager.GameState.Playing;
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 desiredVelocity = new Vector2(horizontal, vertical).normalized * moveSpeed;
            moveVelocity = Vector2.SmoothDamp(moveVelocity, desiredVelocity, ref smoothDampVelocity, smoothing);

            Vector3 position = transform.position + (Vector3)(moveVelocity * Time.deltaTime);
            position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
            position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
            transform.position = position;
        }

        private void HandleShooting()
        {
            if (bulletPrefab == null || firePoint == null)
            {
                return;
            }

            float fireInterval = isRapidFireActive ? rapidFireInterval : baseFireInterval;
            if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= nextShotTime)
            {
                nextShotTime = Time.time + fireInterval;
                FireBullet();
            }
        }

        private void FireBullet()
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.tag = "PlayerBullet";

            Weapons.BulletController bulletController = bullet.GetComponent<Weapons.BulletController>();
            if (bulletController != null)
            {
                bulletController.Configure(Vector2.up, bulletSpeed, bulletDamage, true);
            }

            Managers.AudioManager.Instance?.PlayShootSound();
        }

        public void TakeDamage(int damage)
        {
            if (isDead || isShieldActive || Time.time < invulnerabilityEndTime)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, damage));
            invulnerabilityEndTime = Time.time + invulnerabilityDuration;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            Managers.AudioManager.Instance?.PlayPlayerHitSound();
            StartCoroutine(FlashRoutine());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (isDead)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(1, amount));
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ActivateRapidFire()
        {
            isRapidFireActive = true;
            rapidFireEndTime = Time.time + rapidFireDuration;
            OnPowerUpStateChanged?.Invoke(isRapidFireActive, isShieldActive);
        }

        public void ActivateShield()
        {
            isShieldActive = true;
            shieldEndTime = Time.time + shieldDuration;
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(true);
            }
            OnPowerUpStateChanged?.Invoke(isRapidFireActive, isShieldActive);
        }

        public void ResetPlayer()
        {
            isDead = false;
            isRapidFireActive = false;
            isShieldActive = false;
            currentHealth = maxHealth;
            nextShotTime = 0f;
            rapidFireEndTime = 0f;
            shieldEndTime = 0f;
            invulnerabilityEndTime = 0f;
            moveVelocity = Vector2.zero;
            smoothDampVelocity = Vector2.zero;

            transform.position = new Vector3(0f, -3.7f, 0f);

            Collider2D hitbox = GetComponent<Collider2D>();
            if (hitbox != null)
            {
                hitbox.enabled = true;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = baseColor;
            }

            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnPowerUpStateChanged?.Invoke(isRapidFireActive, isShieldActive);
        }

        private void TickPowerUpTimers()
        {
            if (isRapidFireActive && Time.time >= rapidFireEndTime)
            {
                isRapidFireActive = false;
                OnPowerUpStateChanged?.Invoke(isRapidFireActive, isShieldActive);
            }

            if (isShieldActive && Time.time >= shieldEndTime)
            {
                isShieldActive = false;
                if (shieldVisual != null)
                {
                    shieldVisual.SetActive(false);
                }
                OnPowerUpStateChanged?.Invoke(isRapidFireActive, isShieldActive);
            }
        }

        private void TickInvulnerabilityBlink()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (Time.time < invulnerabilityEndTime)
            {
                float alpha = Mathf.PingPong(Time.time * 16f, 1f) > 0.5f ? 1f : 0.35f;
                Color c = baseColor;
                c.a = alpha;
                spriteRenderer.color = c;
            }
            else if (spriteRenderer.color != baseColor)
            {
                spriteRenderer.color = baseColor;
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer == null)
            {
                yield break;
            }

            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(0.06f);
            spriteRenderer.color = baseColor;
        }

        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                GameObject explosion = new GameObject("PlayerExplosion");
                explosion.transform.position = transform.position;
                explosion.AddComponent<SpaceShooter.Utils.ExplosionEffect>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            Collider2D hitbox = GetComponent<Collider2D>();
            if (hitbox != null)
            {
                hitbox.enabled = false;
            }

            Managers.AudioManager.Instance?.PlayExplosionSound();
            OnPlayerDeath?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDead)
            {
                return;
            }

            if (other.CompareTag("EnemyBullet"))
            {
                Weapons.BulletController bullet = other.GetComponent<Weapons.BulletController>();
                int damage = bullet != null ? bullet.Damage : 10;
                TakeDamage(damage);
                Destroy(other.gameObject);
                return;
            }

            if (other.CompareTag("Enemy"))
            {
                TakeDamage(20);
                return;
            }

            if (other.CompareTag("PowerUp"))
            {
                SpaceShooter.PowerUps.PowerUpController powerUp = other.GetComponent<SpaceShooter.PowerUps.PowerUpController>();
                if (powerUp != null)
                {
                    powerUp.ApplyEffect(this);
                }

                Destroy(other.gameObject);
            }
        }
    }
}
