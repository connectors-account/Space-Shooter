using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Manages all UI elements: Main Menu, HUD, Game Over screen, Pause menu.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject waveAnnouncementPanel;

        [Header("Main Menu Elements")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text highScoreMenuText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        [Header("HUD Elements")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text waveText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Image shieldIcon;
        [SerializeField] private Image rapidFireIcon;

        [Header("Game Over Elements")]
        [SerializeField] private Text gameOverTitleText;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalWaveText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text newHighScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Pause Elements")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseMenuButton;

        [Header("Wave Announcement")]
        [SerializeField] private Text waveAnnouncementText;
        [SerializeField] private float announcementDuration = 2f;

        private Managers.GameManager gameManager;

        private void Start()
        {
            gameManager = Managers.GameManager.Instance;

            // Setup button listeners
            SetupButtons();

            // Subscribe to GameManager events
            if (gameManager != null)
            {
                gameManager.OnScoreChanged += UpdateScore;
                gameManager.OnWaveChanged += UpdateWave;
                gameManager.OnGameStateChanged += HandleGameStateChanged;
            }

            // Find and subscribe to player events
            Player.PlayerController player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
            {
                player.OnHealthChanged += UpdateHealth;
                player.OnPlayerDeath += OnPlayerDeath;
            }

            // Show main menu initially
            ShowMainMenu();
        }

        private void SetupButtons()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (pauseMenuButton != null)
                pauseMenuButton.onClick.AddListener(OnMenuClicked);
        }

        // ========== PANEL MANAGEMENT ==========

        private void ShowMainMenu()
        {
            SetAllPanelsInactive();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

            if (highScoreMenuText != null && gameManager != null)
                highScoreMenuText.text = "High Score: " + gameManager.HighScore;
        }

        private void ShowHUD()
        {
            SetAllPanelsInactive();
            if (hudPanel != null) hudPanel.SetActive(true);
        }

        private void ShowGameOver()
        {
            // Keep HUD visible behind game over
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            if (gameManager != null)
            {
                if (finalScoreText != null)
                    finalScoreText.text = "Score: " + gameManager.Score;

                if (finalWaveText != null)
                    finalWaveText.text = "Wave Reached: " + gameManager.CurrentWave;

                if (highScoreText != null)
                    highScoreText.text = "High Score: " + gameManager.HighScore;

                if (newHighScoreText != null)
                    newHighScoreText.gameObject.SetActive(gameManager.Score >= gameManager.HighScore && gameManager.Score > 0);
            }
        }

        private void ShowPauseMenu()
        {
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        private void HidePauseMenu()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void SetAllPanelsInactive()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (waveAnnouncementPanel != null) waveAnnouncementPanel.SetActive(false);
        }

        // ========== EVENT HANDLERS ==========

        private void HandleGameStateChanged(Managers.GameManager.GameState newState)
        {
            switch (newState)
            {
                case Managers.GameManager.GameState.MainMenu:
                    ShowMainMenu();
                    break;

                case Managers.GameManager.GameState.Playing:
                    ShowHUD();
                    HidePauseMenu();
                    break;

                case Managers.GameManager.GameState.Paused:
                    ShowPauseMenu();
                    break;

                case Managers.GameManager.GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }

        private void UpdateScore(int newScore)
        {
            if (scoreText != null)
                scoreText.text = "Score: " + newScore;
        }

        private void UpdateWave(int newWave)
        {
            if (waveText != null)
                waveText.text = "Wave " + newWave;

            // Show wave announcement
            StartCoroutine(ShowWaveAnnouncement(newWave));

            Managers.AudioManager.Instance?.PlayWaveStartSound();
        }

        private void UpdateHealth(int currentHP, int maxHP)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = maxHP;
                healthBar.value = currentHP;
            }

            if (healthText != null)
                healthText.text = currentHP + " / " + maxHP;
        }

        private void OnPlayerDeath()
        {
            Managers.AudioManager.Instance?.PlayGameOverSound();
            gameManager?.GameOver();
        }

        // ========== BUTTON CALLBACKS ==========

        private void OnStartClicked()
        {
            Managers.AudioManager.Instance?.PlayButtonClickSound();

            // Reset player
            Player.PlayerController player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
            {
                player.ResetPlayer();
                // Re-subscribe in case events were lost
                player.OnHealthChanged -= UpdateHealth;
                player.OnPlayerDeath -= OnPlayerDeath;
                player.OnHealthChanged += UpdateHealth;
                player.OnPlayerDeath += OnPlayerDeath;
            }

            // Clear existing enemies
            Managers.SpawnManager spawner = FindObjectOfType<Managers.SpawnManager>();
            if (spawner != null)
                spawner.ClearAllEnemies();

            gameManager?.StartGame();
        }

        private void OnRestartClicked()
        {
            Managers.AudioManager.Instance?.PlayButtonClickSound();

            Player.PlayerController player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
            {
                player.ResetPlayer();
                player.OnHealthChanged -= UpdateHealth;
                player.OnPlayerDeath -= OnPlayerDeath;
                player.OnHealthChanged += UpdateHealth;
                player.OnPlayerDeath += OnPlayerDeath;
            }

            Managers.SpawnManager spawner = FindObjectOfType<Managers.SpawnManager>();
            if (spawner != null)
                spawner.ClearAllEnemies();

            gameManager?.RestartGame();
        }

        private void OnMenuClicked()
        {
            Managers.AudioManager.Instance?.PlayButtonClickSound();

            Managers.SpawnManager spawner = FindObjectOfType<Managers.SpawnManager>();
            if (spawner != null)
                spawner.ClearAllEnemies();

            gameManager?.ReturnToMenu();
        }

        private void OnResumeClicked()
        {
            Managers.AudioManager.Instance?.PlayButtonClickSound();
            gameManager?.TogglePause();
        }

        private void OnQuitClicked()
        {
            Managers.AudioManager.Instance?.PlayButtonClickSound();

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        // ========== WAVE ANNOUNCEMENT ==========

        private System.Collections.IEnumerator ShowWaveAnnouncement(int wave)
        {
            if (waveAnnouncementPanel != null && waveAnnouncementText != null)
            {
                waveAnnouncementText.text = "WAVE " + wave;
                waveAnnouncementPanel.SetActive(true);

                yield return new WaitForSeconds(announcementDuration);

                waveAnnouncementPanel.SetActive(false);
            }
        }

        // ========== HUD POWER-UP ICONS ==========

        private void Update()
        {
            // Update power-up status icons
            Player.PlayerController player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
            {
                if (shieldIcon != null)
                    shieldIcon.gameObject.SetActive(player.IsShieldActive);

                if (rapidFireIcon != null)
                    rapidFireIcon.gameObject.SetActive(player.IsRapidFireActive);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                gameManager.OnScoreChanged -= UpdateScore;
                gameManager.OnWaveChanged -= UpdateWave;
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }
    }
}
