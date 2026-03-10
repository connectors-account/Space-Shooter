using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Managers;
using SpaceShooter.Player;

namespace SpaceShooter.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Wave Display")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private GameObject waveAnnouncement;
        [SerializeField] private TextMeshProUGUI waveAnnouncementText;
        [SerializeField] private float announcementDuration = 2f;

        [Header("Health Display")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;

        [Header("Weapon Display")]
        [SerializeField] private TextMeshProUGUI weaponLevelText;
        [SerializeField] private Image[] weaponLevelIndicators;

        [Header("Shield Display")]
        [SerializeField] private GameObject shieldIndicator;

        [Header("Boss Health")]
        [SerializeField] private GameObject bossHealthPanel;
        [SerializeField] private Slider bossHealthBar;
        [SerializeField] private TextMeshProUGUI bossNameText;

        private void Start()
        {
            SubscribeToEvents();
            InitializeUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnWaveChanged += UpdateWave;
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStart += ShowWaveAnnouncement;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnWaveChanged -= UpdateWave;
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStart -= ShowWaveAnnouncement;
            }
        }

        private void InitializeUI()
        {
            if (waveAnnouncement != null)
                waveAnnouncement.SetActive(false);

            if (bossHealthPanel != null)
                bossHealthPanel.SetActive(false);

            if (shieldIndicator != null)
                shieldIndicator.SetActive(false);

            UpdateScore(0);
            UpdateWave(0);
        }

        public void SetupPlayerEvents(PlayerController player)
        {
            if (player != null)
            {
                player.OnHealthChanged += UpdateHealth;
                player.OnWeaponLevelChanged += UpdateWeaponLevel;
                player.OnShieldChanged += UpdateShieldDisplay;
                
                UpdateHealth(player.CurrentHealth, player.MaxHealth);
                UpdateWeaponLevel(player.WeaponLevel);
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";

            if (highScoreText != null && GameManager.Instance != null)
                highScoreText.text = $"High: {GameManager.Instance.HighScore}";
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null)
                waveText.text = $"Wave: {wave}";
        }

        private void ShowWaveAnnouncement(int wave)
        {
            if (waveAnnouncement != null && waveAnnouncementText != null)
            {
                waveAnnouncementText.text = $"WAVE {wave}";
                waveAnnouncement.SetActive(true);
                StartCoroutine(HideAnnouncementAfterDelay());
            }
        }

        private System.Collections.IEnumerator HideAnnouncementAfterDelay()
        {
            yield return new WaitForSeconds(announcementDuration);
            if (waveAnnouncement != null)
                waveAnnouncement.SetActive(false);
        }

        private void UpdateHealth(int current, int max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }

            if (healthText != null)
                healthText.text = $"{current}/{max}";

            if (healthFill != null)
            {
                float healthPercent = (float)current / max;
                if (healthPercent > 0.6f)
                    healthFill.color = healthyColor;
                else if (healthPercent > 0.3f)
                    healthFill.color = damagedColor;
                else
                    healthFill.color = criticalColor;
            }
        }

        private void UpdateWeaponLevel(int level)
        {
            if (weaponLevelText != null)
                weaponLevelText.text = $"Weapon Lvl: {level}";

            if (weaponLevelIndicators != null)
            {
                for (int i = 0; i < weaponLevelIndicators.Length; i++)
                {
                    if (weaponLevelIndicators[i] != null)
                        weaponLevelIndicators[i].enabled = i < level;
                }
            }
        }

        private void UpdateShieldDisplay(bool hasShield)
        {
            if (shieldIndicator != null)
                shieldIndicator.SetActive(hasShield);
        }

        public void ShowBossHealth(string bossName, float healthPercent)
        {
            if (bossHealthPanel != null)
            {
                bossHealthPanel.SetActive(true);
                
                if (bossNameText != null)
                    bossNameText.text = bossName;
                    
                if (bossHealthBar != null)
                    bossHealthBar.value = healthPercent;
            }
        }

        public void UpdateBossHealth(float healthPercent)
        {
            if (bossHealthBar != null)
                bossHealthBar.value = healthPercent;
        }

        public void HideBossHealth()
        {
            if (bossHealthPanel != null)
                bossHealthPanel.SetActive(false);
        }
    }
}
