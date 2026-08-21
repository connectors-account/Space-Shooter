using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player health, lives, invincibility, shield and regeneration handling.
    /// Raises events so UI can update without tight coupling.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invincibilityDuration = 2f;
        [SerializeField] private int respawnHealth = 100;

        [Header("Regeneration")]
        [SerializeField] private int regenPerSecond = 5;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool HasShield { get; private set; }
        public bool IsInvincible { get; private set; }

        // Events: (current, max)
        public event Action<int, int> OnHealthChanged;
        public event Action<bool> OnShieldChanged;
        public event Action OnPlayerDied;      // per-life death
        public event Action OnPlayerRespawned;

        private PlayerController controller;
        private PlayerShooter shooter;
        private Coroutine regenRoutine;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            shooter = GetComponent<PlayerShooter>();
            CurrentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            OnShieldChanged?.Invoke(HasShield);
        }

        public void TakeDamage(int amount)
        {
            if (IsInvincible) return;

            if (HasShield)
            {
                HasShield = false;
                OnShieldChanged?.Invoke(false);
                StartInvincibility(1f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PlayerHit");
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PlayerHit");

            if (Effects.CameraShake.Instance != null) Effects.CameraShake.Instance.Shake(0.25f, 0.25f);

            if (CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartInvincibility(invincibilityDuration);
            }
        }

        private void Die()
        {
            OnPlayerDied?.Invoke();

            bool noLivesLeft = GameManager.Instance != null && GameManager.Instance.LoseLife();

            if (noLivesLeft)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("GameOver");
                if (controller != null) controller.SetControlsEnabled(false);
                if (shooter != null) shooter.SetCanShoot(false);
                if (GameManager.Instance != null) GameManager.Instance.GameOver();
                gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            if (controller != null) controller.SetControlsEnabled(false);
            if (shooter != null) shooter.SetCanShoot(false);

            yield return new WaitForSeconds(1f);

            CurrentHealth = respawnHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            if (shooter != null) shooter.ResetWeapon();
            if (controller != null)
            {
                controller.ResetToCenter();
                controller.SetControlsEnabled(true);
            }
            if (shooter != null) shooter.SetCanShoot(true);

            OnPlayerRespawned?.Invoke();
            StartInvincibility(invincibilityDuration);
        }

        private void StartInvincibility(float duration)
        {
            StartCoroutine(InvincibilityRoutine(duration));
            if (controller != null) controller.StartInvincibilityFlash(duration);
        }

        private IEnumerator InvincibilityRoutine(float duration)
        {
            IsInvincible = true;
            yield return new WaitForSeconds(duration);
            IsInvincible = false;
        }

        // ---------------- Power-ups ----------------

        public void ActivateShield()
        {
            HasShield = true;
            OnShieldChanged?.Invoke(true);
        }

        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void StartRegeneration(float duration)
        {
            if (regenRoutine != null) StopCoroutine(regenRoutine);
            regenRoutine = StartCoroutine(RegenRoutine(duration));
        }

        private IEnumerator RegenRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                yield return new WaitForSeconds(1f);
                Heal(regenPerSecond);
                elapsed += 1f;
            }
            regenRoutine = null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("EnemyBullet"))
            {
                var bullet = other.GetComponent<Weapons.Bullet>();
                int dmg = bullet != null ? bullet.Damage : 10;
                TakeDamage(dmg);
                if (bullet != null) bullet.ReturnToPool();
            }
            else if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
            {
                // Collision with enemy body.
                TakeDamage(25);
            }
        }
    }
}
