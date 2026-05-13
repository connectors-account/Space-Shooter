// ============================================================================
// PlayerHealth.cs — Player health, lives, shield, and damage handling
// ============================================================================
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;

namespace SpaceShooter.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        public static PlayerHealth Instance { get; private set; }

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int startingLives = 3;
        [SerializeField] private float invincibilityDuration = 1.5f; // after taking damage

        [Header("Shield")]
        [SerializeField] private float shieldDuration = 5f;
        [SerializeField] private GameObject shieldVisual;            // child object toggled on/off

        // ---- Events ----
        public event System.Action<int, int> OnHealthChanged;        // current, max
        public event System.Action<int> OnLivesChanged;
        public event System.Action OnPlayerDeath;
        public event System.Action<bool> OnShieldChanged;

        // ---- Public state ----
        public int CurrentHealth { get; private set; }
        public int Lives { get; private set; }
        public bool IsShielded { get; private set; }
        public bool IsInvincible { get; private set; }

        private float _invincibilityTimer;
        private float _shieldTimer;
        private SpriteRenderer _sr;

        private void Awake()
        {
            Instance = this;
            _sr = GetComponent<SpriteRenderer>();
            ResetHealth();
        }

        private void Update()
        {
            // Invincibility flash effect
            if (IsInvincible)
            {
                _invincibilityTimer -= Time.deltaTime;
                if (_sr != null)
                {
                    // Blink every 0.1 s
                    _sr.enabled = Mathf.FloorToInt(_invincibilityTimer * 10f) % 2 == 0;
                }
                if (_invincibilityTimer <= 0f)
                {
                    IsInvincible = false;
                    if (_sr != null) _sr.enabled = true;
                }
            }

            // Shield countdown
            if (IsShielded)
            {
                _shieldTimer -= Time.deltaTime;
                if (_shieldTimer <= 0f)
                    DeactivateShield();
            }
        }

        // ====================================================================
        // Public API
        // ====================================================================

        public void TakeDamage(int amount)
        {
            if (IsInvincible || IsShielded) return;

            CurrentHealth -= amount;
            CurrentHealth = Mathf.Max(CurrentHealth, 0);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            AudioManager.Instance?.PlaySFX("PlayerHit");
            GameManager.Instance?.ResetCombo();

            if (CurrentHealth <= 0)
            {
                HandleDeath();
            }
            else
            {
                // Brief invincibility after being hit
                IsInvincible = true;
                _invincibilityTimer = invincibilityDuration;
            }
        }

        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            AudioManager.Instance?.PlaySFX("PowerUp");
        }

        public void ActivateShield()
        {
            IsShielded = true;
            _shieldTimer = shieldDuration;
            if (shieldVisual != null) shieldVisual.SetActive(true);
            OnShieldChanged?.Invoke(true);
            AudioManager.Instance?.PlaySFX("PowerUp");
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            Lives = startingLives;
            IsShielded = false;
            IsInvincible = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            OnLivesChanged?.Invoke(Lives);
        }

        // ====================================================================
        // Internal
        // ====================================================================
        private void HandleDeath()
        {
            Lives--;
            OnLivesChanged?.Invoke(Lives);
            AudioManager.Instance?.PlaySFX("Explosion");

            if (Lives <= 0)
            {
                OnPlayerDeath?.Invoke();
                GameManager.Instance?.GameOver();
                gameObject.SetActive(false);
            }
            else
            {
                // Respawn with full health + brief invincibility
                CurrentHealth = maxHealth;
                OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
                IsInvincible = true;
                _invincibilityTimer = invincibilityDuration * 2f;
            }
        }

        private void DeactivateShield()
        {
            IsShielded = false;
            _shieldTimer = 0f;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            OnShieldChanged?.Invoke(false);
        }
    }
}
