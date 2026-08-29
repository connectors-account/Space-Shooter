using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Player movement, shooting, health, and power-up effects.
    /// Input: WASD/Arrow Keys move, Space shoots.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private Vector2 minBounds = new Vector2(-8.5f, -4.6f);
        [SerializeField] private Vector2 maxBounds = new Vector2(8.5f, 4.6f);

        [Header("Combat")]
        [SerializeField] private GameObject playerBulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float baseFireInterval = 0.18f;
        [SerializeField] private float upgradedFireInterval = 0.1f;
        [SerializeField] private float bulletSpeed = 14f;
        [SerializeField] private int bulletDamage = 1;

        [Header("Health")]
        [SerializeField] private int maxHealth = 5;

        [Header("Power-Ups")]
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private float shieldDuration = 6f;
        [SerializeField] private float weaponUpgradeDuration = 8f;

        private int currentHealth;
        private int weaponLevel = 1;
        private float nextFireTime;
        private float shieldEndTime;
        private float weaponUpgradeEndTime;
        private bool isAlive = true;

        public DamageTeam Team => DamageTeam.Player;
        public bool IsAlive => isAlive;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int WeaponLevel => weaponLevel;

        private void Start()
        {
            currentHealth = maxHealth;
            ToggleShieldVisual(false);
            UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
            UIManager.Instance?.UpdateWeaponLevel(weaponLevel);
        }

        private void Update()
        {
            if (!isAlive || GameManager.Instance == null || !GameManager.Instance.IsGameplayActive)
            {
                return;
            }

            HandleMovement();
            HandleShooting();
            UpdateTimedPowerUps();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 movement = new Vector3(horizontal, vertical, 0f).normalized * moveSpeed * Time.deltaTime;
            transform.position += movement;

            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
                Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
                transform.position.z);
        }

        private void HandleShooting()
        {
            if (!Input.GetKey(KeyCode.Space) || Time.time < nextFireTime || playerBulletPrefab == null)
            {
                return;
            }

            float interval = Time.time < weaponUpgradeEndTime ? upgradedFireInterval : baseFireInterval;
            nextFireTime = Time.time + interval;

            FirePattern();
            AudioManager.Instance?.PlaySfx(AudioManager.SfxType.PlayerShoot);
        }

        private void FirePattern()
        {
            Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.55f;

            SpawnBullet(origin, Vector2.up);

            if (weaponLevel >= 2)
            {
                SpawnBullet(origin + Vector3.left * 0.22f, (Vector2.up + Vector2.left * 0.08f).normalized);
                SpawnBullet(origin + Vector3.right * 0.22f, (Vector2.up + Vector2.right * 0.08f).normalized);
            }

            if (weaponLevel >= 3)
            {
                SpawnBullet(origin + Vector3.left * 0.4f, (Vector2.up + Vector2.left * 0.18f).normalized);
                SpawnBullet(origin + Vector3.right * 0.4f, (Vector2.up + Vector2.right * 0.18f).normalized);
            }
        }

        private void SpawnBullet(Vector3 position, Vector2 direction)
        {
            GameObject bullet = Instantiate(playerBulletPrefab, position, Quaternion.identity);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController != null)
            {
                bulletController.Initialize(DamageTeam.Player, direction, bulletDamage, bulletSpeed);
            }
        }

        public void TakeDamage(int amount, DamageTeam sourceTeam)
        {
            if (!isAlive)
            {
                return;
            }

            if (IsShieldActive())
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, amount));
            UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
            AudioManager.Instance?.PlaySfx(AudioManager.SfxType.PlayerHit);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isAlive = false;
            gameObject.SetActive(false);
            AudioManager.Instance?.PlaySfx(AudioManager.SfxType.Explosion);
            GameManager.Instance?.HandlePlayerDefeated();
        }

        public void ApplyPowerUp(PowerUpController.PowerUpType powerUpType, int amount, float duration)
        {
            if (!isAlive)
            {
                return;
            }

            switch (powerUpType)
            {
                case PowerUpController.PowerUpType.WeaponUpgrade:
                    weaponLevel = Mathf.Clamp(weaponLevel + amount, 1, 3);
                    weaponUpgradeEndTime = Time.time + Mathf.Max(duration, weaponUpgradeDuration);
                    UIManager.Instance?.UpdateWeaponLevel(weaponLevel);
                    break;

                case PowerUpController.PowerUpType.Shield:
                    shieldEndTime = Time.time + Mathf.Max(duration, shieldDuration);
                    ToggleShieldVisual(true);
                    UIManager.Instance?.SetShieldActive(true);
                    break;

                case PowerUpController.PowerUpType.Health:
                    currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
                    UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
                    break;
            }

            AudioManager.Instance?.PlaySfx(AudioManager.SfxType.PowerUp);
        }

        private void UpdateTimedPowerUps()
        {
            if (weaponLevel > 1 && Time.time >= weaponUpgradeEndTime)
            {
                weaponLevel = 1;
                UIManager.Instance?.UpdateWeaponLevel(weaponLevel);
            }

            if (IsShieldActive() && Time.time >= shieldEndTime)
            {
                ToggleShieldVisual(false);
                UIManager.Instance?.SetShieldActive(false);
            }
        }

        private bool IsShieldActive()
        {
            return Time.time < shieldEndTime;
        }

        private void ToggleShieldVisual(bool state)
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(state);
            }
        }
    }
}
