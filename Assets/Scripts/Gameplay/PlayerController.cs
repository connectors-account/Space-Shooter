using System;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        private int maxHealth;
        private float moveSpeed;
        private float fireCooldown;

        private float nextShotTime;
        private int currentHealth;

        private bool rapidFireActive;
        private float rapidFireEndsAt;

        private bool shieldActive;
        private float shieldEndsAt;

        private Transform muzzle;
        private GameObject shieldVisual;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public event Action<int, int> OnHealthChanged;
        public event Action OnPlayerDied;

        public void Configure(int configuredMaxHealth, float configuredMoveSpeed, float configuredFireCooldown)
        {
            maxHealth = configuredMaxHealth;
            moveSpeed = configuredMoveSpeed;
            fireCooldown = configuredFireCooldown;
            currentHealth = maxHealth;
        }

        public void SetMuzzle(Transform muzzleTransform)
        {
            muzzle = muzzleTransform;
        }

        public void SetShieldVisual(GameObject visual)
        {
            shieldVisual = visual;
        }

        public void ResetForNewRun()
        {
            transform.position = new Vector3(0f, -3.8f, 0f);
            currentHealth = maxHealth;
            rapidFireActive = false;
            shieldActive = false;
            nextShotTime = 0f;
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing)
            {
                return;
            }

            HandleMovement();
            HandleFiring();
            TickPowerUps();
        }

        private void HandleMovement()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 movement = new Vector2(x, y).normalized * moveSpeed * Time.deltaTime;

            transform.position += (Vector3)movement;

            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            viewportPos.x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
            viewportPos.y = Mathf.Clamp(viewportPos.y, 0.06f, 0.94f);
            transform.position = Camera.main.ViewportToWorldPoint(viewportPos);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }

        private void HandleFiring()
        {
            if (!Input.GetKey(KeyCode.Space) || Time.time < nextShotTime)
            {
                return;
            }

            float cooldown = rapidFireActive ? fireCooldown * 0.45f : fireCooldown;
            nextShotTime = Time.time + cooldown;

            Vector3 spawnPosition = muzzle != null ? muzzle.position : transform.position + Vector3.up * 0.5f;
            EntityFactory.CreateBullet(spawnPosition, Vector2.up, BulletController.BulletOwner.Player, 12f, 15);
            FindObjectOfType<AudioManager>()?.PlayShoot();
        }

        private void TickPowerUps()
        {
            if (rapidFireActive && Time.time >= rapidFireEndsAt)
            {
                rapidFireActive = false;
            }

            if (shieldActive && Time.time >= shieldEndsAt)
            {
                shieldActive = false;
                if (shieldVisual != null)
                {
                    shieldVisual.SetActive(false);
                }
            }
        }

        public void ApplyPowerUp(PowerUpController.PowerUpType type)
        {
            switch (type)
            {
                case PowerUpController.PowerUpType.Health:
                    currentHealth = Mathf.Min(maxHealth, currentHealth + 30);
                    OnHealthChanged?.Invoke(currentHealth, maxHealth);
                    break;
                case PowerUpController.PowerUpType.RapidFire:
                    rapidFireActive = true;
                    rapidFireEndsAt = Time.time + 6f;
                    break;
                case PowerUpController.PowerUpType.Shield:
                    shieldActive = true;
                    shieldEndsAt = Time.time + 8f;
                    if (shieldVisual != null)
                    {
                        shieldVisual.SetActive(true);
                    }
                    break;
            }
        }

        public bool IsRapidFireActive() => rapidFireActive;
        public bool IsShieldActive() => shieldActive;

        public void TakeDamage(int amount)
        {
            if (shieldActive)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                gameObject.SetActive(false);
                OnPlayerDied?.Invoke();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BulletController bullet = other.GetComponent<BulletController>();
            if (bullet != null && bullet.Owner == BulletController.BulletOwner.Enemy)
            {
                TakeDamage(bullet.Damage);
                Destroy(other.gameObject);
                return;
            }

            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                TakeDamage(25);
                Destroy(enemy.gameObject);
                GameManager.Instance.ReportEnemyDestroyed(0);
                return;
            }

            PowerUpController powerUp = other.GetComponent<PowerUpController>();
            if (powerUp != null)
            {
                ApplyPowerUp(powerUp.Type);
                FindObjectOfType<AudioManager>()?.PlayPowerUp();
                Destroy(other.gameObject);
            }
        }
    }
}
