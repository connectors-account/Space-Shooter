using System.Collections;
using SpaceShooter.Audio;
using SpaceShooter.Core;
using SpaceShooter.PowerUps;
using SpaceShooter.Weapons;
using UnityEngine;

namespace SpaceShooter.Player
{
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private Vector2 minBounds = new Vector2(-8.5f, -4.5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(8.5f, 4.5f);

        [Header("Shooting")]
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private float fireCooldown = 0.25f;

        [Header("Power Up Durations")]
        [SerializeField] private float shieldDuration = 6f;
        [SerializeField] private float rapidFireDuration = 6f;
        [SerializeField] private float rapidFireMultiplier = 2.2f;

        [Header("Visual")]
        [SerializeField] private GameObject shieldVisual;

        private Health health;
        private float fireTimer;
        private bool shieldActive;
        private bool rapidFireActive;
        private Coroutine shieldRoutine;
        private Coroutine rapidFireRoutine;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDied += HandleDeath;
        }

        private void Start()
        {
            health.ResetHealth();
            GameManager.Instance.RegisterPlayerHealth(health.MaxHealth);
            SetShieldVisual(false);
        }

        private void OnDisable()
        {
            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDied -= HandleDeath;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            HandleMovement();
            HandleShooting();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 delta = new Vector3(horizontal, vertical, 0f).normalized * (moveSpeed * Time.deltaTime);
            transform.position += delta;

            Vector3 clamped = transform.position;
            clamped.x = Mathf.Clamp(clamped.x, minBounds.x, maxBounds.x);
            clamped.y = Mathf.Clamp(clamped.y, minBounds.y, maxBounds.y);
            transform.position = clamped;
        }

        private void HandleShooting()
        {
            fireTimer -= Time.deltaTime;
            if (!Input.GetButton("Fire1") || fireTimer > 0f)
            {
                return;
            }

            weaponSystem.Fire(Vector2.up);
            AudioManager.Instance?.PlayShoot();

            float adjustedCooldown = rapidFireActive ? fireCooldown / rapidFireMultiplier : fireCooldown;
            fireTimer = adjustedCooldown;
        }

        public void TakeDamage(float amount)
        {
            if (shieldActive)
            {
                return;
            }

            AudioManager.Instance?.PlayPlayerHit();
            health.TakeDamage(amount);
        }

        public void ApplyPowerUp(PowerUpType powerUpType, float value)
        {
            switch (powerUpType)
            {
                case PowerUpType.Shield:
                    if (shieldRoutine != null)
                    {
                        StopCoroutine(shieldRoutine);
                    }
                    shieldRoutine = StartCoroutine(ShieldTimer(shieldDuration));
                    break;
                case PowerUpType.RapidFire:
                    if (rapidFireRoutine != null)
                    {
                        StopCoroutine(rapidFireRoutine);
                    }
                    rapidFireRoutine = StartCoroutine(RapidFireTimer(rapidFireDuration));
                    break;
                case PowerUpType.HealthRestore:
                    health.Heal(value);
                    break;
            }

            AudioManager.Instance?.PlayPowerUp();
        }

        private IEnumerator ShieldTimer(float duration)
        {
            shieldActive = true;
            SetShieldVisual(true);
            yield return new WaitForSeconds(duration);
            shieldActive = false;
            SetShieldVisual(false);
        }

        private IEnumerator RapidFireTimer(float duration)
        {
            rapidFireActive = true;
            yield return new WaitForSeconds(duration);
            rapidFireActive = false;
        }

        private void SetShieldVisual(bool visible)
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(visible);
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            GameManager.Instance?.SetPlayerHealth(current);
        }

        private void HandleDeath()
        {
            GameManager.Instance?.OnPlayerDestroyed();
            AudioManager.Instance?.PlayGameOver();
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                TakeDamage(25f);
                return;
            }

            if (other.TryGetComponent(out Bullet bullet) && !bullet.FromPlayer)
            {
                TakeDamage(bullet.Damage);
            }
        }
    }
}
