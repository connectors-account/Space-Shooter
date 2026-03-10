using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// In-game HUD controller
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private string scorePrefix = "Score: ";
        
        [Header("Health Display")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        
        [Header("Wave Display")]
        [SerializeField] private Text waveText;
        [SerializeField] private string wavePrefix = "Wave ";
        [SerializeField] private GameObject waveAnnouncementPanel;
        [SerializeField] private Text waveAnnouncementText;
        [SerializeField] private float waveAnnouncementDuration = 2f;
        
        [Header("Power-up Indicators")]
        [SerializeField] private GameObject shieldIndicator;
        [SerializeField] private GameObject rapidFireIndicator;
        [SerializeField] private GameObject tripleShotIndicator;
        
        [Header("Lives Display")]
        [SerializeField] private Text livesText;
        [SerializeField] private Transform livesContainer;
        [SerializeField] private GameObject lifeIconPrefab;
        
        private PlayerController playerController;
        private float waveAnnouncementTimer;
        
        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                SubscribeToPlayerEvents();
            }
            
            // Subscribe to game manager events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnWaveChanged += UpdateWave;
                GameManager.Instance.OnLivesChanged += UpdateLives;
                
                // Initial update
                UpdateScore(GameManager.Instance.CurrentScore);
                UpdateWave(GameManager.Instance.CurrentWave);
                UpdateLives(GameManager.Instance.Lives);
                UpdateHighScore();
            }
            
            if (waveAnnouncementPanel != null)
                waveAnnouncementPanel.SetActive(false);
                
            InitializePowerupIndicators();
        }
        
        private void Update()
        {
            // Update wave announcement timer
            if (waveAnnouncementTimer > 0)
            {
                waveAnnouncementTimer -= Time.deltaTime;
                if (waveAnnouncementTimer <= 0 && waveAnnouncementPanel != null)
                {
                    waveAnnouncementPanel.SetActive(false);
                }
            }
            
            // Update power-up indicators
            UpdatePowerupIndicators();
        }
        
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnWaveChanged -= UpdateWave;
                GameManager.Instance.OnLivesChanged -= UpdateLives;
            }
            
            if (playerController != null)
            {
                playerController.OnHealthChanged -= UpdateHealth;
            }
        }
        
        private void SubscribeToPlayerEvents()
        {
            if (playerController != null)
            {
                playerController.OnHealthChanged += UpdateHealth;
                UpdateHealth(playerController.CurrentHealth, playerController.MaxHealth);
            }
        }
        
        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{scorePrefix}{score:N0}";
            }
        }
        
        private void UpdateHighScore()
        {
            if (highScoreText != null && GameManager.Instance != null)
            {
                highScoreText.text = $"High: {GameManager.Instance.HighScore:N0}";
            }
        }
        
        private void UpdateHealth(int current, int max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{current}/{max}";
            }
            
            if (healthFill != null)
            {
                float healthPercent = (float)current / max;
                if (healthPercent > 0.6f)
                {
                    healthFill.color = healthyColor;
                }
                else if (healthPercent > 0.3f)
                {
                    healthFill.color = damagedColor;
                }
                else
                {
                    healthFill.color = criticalColor;
                }
            }
        }
        
        private void UpdateWave(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"{wavePrefix}{wave}";
            }
            
            // Show wave announcement
            if (waveAnnouncementPanel != null && waveAnnouncementText != null)
            {
                waveAnnouncementText.text = $"WAVE {wave}";
                waveAnnouncementPanel.SetActive(true);
                waveAnnouncementTimer = waveAnnouncementDuration;
            }
        }
        
        private void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"x{lives}";
            }
            
            // Update life icons
            if (livesContainer != null && lifeIconPrefab != null)
            {
                // Clear existing
                foreach (Transform child in livesContainer)
                {
                    Destroy(child.gameObject);
                }
                
                // Create new icons
                for (int i = 0; i < lives; i++)
                {
                    Instantiate(lifeIconPrefab, livesContainer);
                }
            }
        }
        
        private void InitializePowerupIndicators()
        {
            if (shieldIndicator != null) shieldIndicator.SetActive(false);
            if (rapidFireIndicator != null) rapidFireIndicator.SetActive(false);
            if (tripleShotIndicator != null) tripleShotIndicator.SetActive(false);
        }
        
        private void UpdatePowerupIndicators()
        {
            if (playerController == null) return;
            
            if (shieldIndicator != null)
            {
                shieldIndicator.SetActive(playerController.HasShield);
            }
        }
        
        public void RefreshPlayerReference()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                SubscribeToPlayerEvents();
            }
        }
    }
}
