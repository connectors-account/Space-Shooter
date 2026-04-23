using System;
using SpaceShooter.Audio;
using SpaceShooter.Combat;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float boundsPadding = 0.35f;

        [Header("Combat")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float shootCooldown = 0.2f;

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float damageFlashDuration = 0.12f;

        private Rigidbody2D rb;
        private Camera cam;
        private Vector2 input;
        private float nextShotTime;
        private float rapidFireTimer;
        private float shieldTimer;
        private bool isShielded;
        private Color defaultColor;

        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public event Action<int, int> OnHealthChanged;
        public event Action<bool, float> OnShieldChanged;
        public event Action<bool, float> OnRapidFireChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            cam = Camera.main;
            gameObject.layer = GameLayers.GetLayerOrDefault(GameLayers.Player);
            CurrentHealth = maxHealth;
            if (spriteRenderer != null)
            {
                defaultColor = spriteRenderer.color;
            }
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            GatherInput();
            HandleShooting();
            TickPowerupTimers();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.TogglePause();
            }
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            rb.velocity = input * moveSpeed;
            ClampInsideCamera();
        }

        private void GatherInput()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            input = new Vector2(x, y).normalized;
        }

        private void HandleShooting()
        {
            bool wantsShoot = Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1");
            if (!wantsShoot || Time.time < nextShotTime || bulletPrefab == null || firePoint == null)
            {
                return;
            }

            float activeCooldown = rapidFireTimer > 0f ? shootCooldown * 0.45f : shootCooldown;
            nextShotTime = Time.time + activeCooldown;

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(BulletOwner.Player, 1, Vector2.up, 12f);
            }

            SoundManager.Instance?.PlayShoot();
        }

        private void ClampInsideCamera()
        {
            Vector2 min = ScreenBounds.MinWorld(cam);
            Vector2 max = ScreenBounds.MaxWorld(cam);

            Vector3 clamped = transform.position;
            clamped.x = Mathf.Clamp(clamped.x, min.x + boundsPadding, max.x - boundsPadding);
            clamped.y = Mathf.Clamp(clamped.y, min.y + boundsPadding, max.y - boundsPadding);
            transform.position = clamped;
        }

        private void TickPowerupTimers()
        {
            if (rapidFireTimer > 0f)
            {
                rapidFireTimer -= Time.deltaTime;
                if (rapidFireTimer <= 0f)
                {
                    rapidFireTimer = 0f;
                    OnRapidFireChanged?.Invoke(false, 0f);
                }
                else
                {
                    OnRapidFireChanged?.Invoke(true, rapidFireTimer);
                }
            }

            if (shieldTimer > 0f)
            {
                shieldTimer -= Time.deltaTime;
                if (shieldTimer <= 0f)
                {
                    shieldTimer = 0f;
                    isShielded = false;
                    OnShieldChanged?.Invoke(false, 0f);
                }
                else
                {
                    OnShieldChanged?.Invoke(true, shieldTimer);
                }
            }
        }

        public void ApplyPowerup(Powerups.PowerUpType type, float duration, int healthAmount)
        {
            switch (type)
            {
                case Powerups.PowerUpType.RapidFire:
                    rapidFireTimer = Mathf.Max(rapidFireTimer, duration);
                    OnRapidFireChanged?.Invoke(true, rapidFireTimer);
                    break;
                case Powerups.PowerUpType.Shield:
                    shieldTimer = Mathf.Max(shieldTimer, duration);
                    isShielded = true;
                    OnShieldChanged?.Invoke(true, shieldTimer);
                    break;
                case Powerups.PowerUpType.HealthRestore:
                    Heal(healthAmount);
                    break;
            }

            SoundManager.Instance?.PlayPowerUp();
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            if (isShielded) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            if (spriteRenderer != null)
            {
                StopAllCoroutines();
                StartCoroutine(DamageFlash());
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            SoundManager.Instance?.PlayExplosion();
            GameManager.Instance?.GameOver();
            gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = defaultColor;
        }
    }
}
