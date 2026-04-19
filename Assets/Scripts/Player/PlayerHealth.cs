using UnityEngine;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player HP + temporary shield behavior.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, Combat.IDamageable
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private SpriteRenderer playerSprite;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.45f, 0.45f, 1f);
        [SerializeField] private float flashDuration = 0.1f;

        private int currentHealth;
        private Color originalColor;
        private float shieldEndTime;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsShieldActive => Time.time < shieldEndTime;
        public bool IsAlive => currentHealth > 0;

        public event System.Action<int, int> OnHealthChanged;
        public event System.Action OnDeath;

        private void Awake()
        {
            if (playerSprite == null)
            {
                playerSprite = GetComponentInChildren<SpriteRenderer>();
            }

            if (playerSprite != null)
            {
                originalColor = playerSprite.color;
            }

            ResetHealth();
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            shieldEndTime = 0f;
            UpdateVisualState();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void RestoreHealth(int amount)
        {
            if (!IsAlive)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ActivateShield(float duration)
        {
            shieldEndTime = Mathf.Max(shieldEndTime, Time.time + duration);
            UpdateVisualState();
        }

        public void ReceiveDamage(int amount, GameObject source)
        {
            if (!IsAlive || IsShieldActive)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(0, amount));
            Audio.SoundManager.Instance?.PlayPlayerHit();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (playerSprite != null)
            {
                StopAllCoroutines();
                StartCoroutine(FlashRoutine());
            }

            if (currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            playerSprite.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            UpdateVisualState();
        }

        private void Update()
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (playerSprite == null)
            {
                return;
            }

            if (IsShieldActive)
            {
                playerSprite.color = Color.cyan;
            }
            else if (playerSprite.color == Color.cyan)
            {
                playerSprite.color = originalColor;
            }
        }
    }
}
