using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player health + lives. Handles damage, invincibility frames (2s), shield absorption,
    /// death/respawn, and reports to the GameManager. Fires events for the HUD.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        public int maxHealth = 100;
        public int currentHealth;

        [Header("Lives")]
        public int lives = 3;

        [Header("Invincibility")]
        public float invincibleDuration = 2f;

        public bool IsInvincible { get; private set; }
        public bool HasShield { get; private set; }

        // Events: (current, max) for damage/heal; parameterless for death.
        public event Action<int, int> OnDamage;
        public event Action<int, int> OnHeal;
        public event Action OnDeath;
        public event Action<int> OnLivesChanged;

        private PlayerController _controller;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        private void Start()
        {
            currentHealth = maxHealth;
            if (GameManager.Instance != null)
            {
                lives = GameManager.Instance.Lives;
                GameManager.Instance.SetLives(lives);
            }
            OnLivesChanged?.Invoke(lives);
        }

        public void TakeDamage(int amount)
        {
            if (IsInvincible) return;

            // Shield absorbs a single hit and pops.
            if (HasShield)
            {
                SetShield(false);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("hit");
                StartCoroutine(InvincibilityRoutine());
                return;
            }

            currentHealth -= amount;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("hit");
            OnDamage?.Invoke(Mathf.Max(0, currentHealth), maxHealth);

            if (currentHealth <= 0)
            {
                HandleDeath();
            }
            else
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHeal?.Invoke(currentHealth, maxHealth);
        }

        private void HandleDeath()
        {
            currentHealth = 0;
            OnDeath?.Invoke();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("explosion");

            lives--;
            OnLivesChanged?.Invoke(lives);
            if (GameManager.Instance != null) GameManager.Instance.SetLives(lives);

            if (lives > 0)
            {
                Respawn();
            }
            else
            {
                // GameManager.SetLives already triggers game over at 0.
                gameObject.SetActive(false);
            }
        }

        private void Respawn()
        {
            currentHealth = maxHealth;
            OnHeal?.Invoke(currentHealth, maxHealth);

            // Reset to bottom-center of the screen.
            if (ScreenBounds.Instance != null)
            {
                transform.position = new Vector3(0f, ScreenBounds.Instance.Bottom + 1.5f, 0f);
            }
            StartCoroutine(InvincibilityRoutine());
        }

        private IEnumerator InvincibilityRoutine()
        {
            IsInvincible = true;
            yield return new WaitForSeconds(invincibleDuration);
            IsInvincible = false;
        }

        public void SetShield(bool active)
        {
            HasShield = active;
            if (_controller != null) _controller.SetShieldVisual(active);
        }
    }
}
