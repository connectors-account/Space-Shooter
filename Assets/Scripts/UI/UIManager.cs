using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Connects gameplay data/events to HUD and menu panels.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject waveBannerPanel;

        [Header("Main Menu")]
        [SerializeField] private Text menuHighScoreText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        [Header("HUD")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text waveText;
        [SerializeField] private Text comboText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Image rapidFireIcon;
        [SerializeField] private Image shieldIcon;

        [Header("Wave Banner")]
        [SerializeField] private Text waveBannerText;
        [SerializeField] private float waveBannerDuration = 1.8f;

        [Header("Pause")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseToMenuButton;

        [Header("Game Over")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalWaveText;
        [SerializeField] private Text gameOverHighScoreText;
        [SerializeField] private Text newRecordText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button gameOverMenuButton;

        private Managers.GameManager gameManager;
        private Player.PlayerController player;

        private void Start()
        {
            gameManager = Managers.GameManager.Instance;
            player = FindObjectOfType<Player.PlayerController>();

            BindButtons();
            BindGameEvents();
            BindPlayerEvents();

            ShowOnly(mainMenuPanel);
            RefreshMenuHighScore();
            RefreshPowerUpIcons(false, false);
            UpdateCombo(0, 1);
        }

        private void BindButtons()
        {
            if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (pauseToMenuButton != null) pauseToMenuButton.onClick.AddListener(OnMenuClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(OnMenuClicked);
        }

        private void BindGameEvents()
        {
            if (gameManager == null)
            {
                return;
            }

            gameManager.OnGameStateChanged += HandleGameStateChanged;
            gameManager.OnScoreChanged += UpdateScore;
            gameManager.OnWaveChanged += UpdateWave;
            gameManager.OnComboChanged += UpdateCombo;
        }

        private void BindPlayerEvents()
        {
            if (player == null)
            {
                return;
            }

            player.OnHealthChanged += UpdateHealth;
            player.OnPowerUpStateChanged += RefreshPowerUpIcons;
            player.OnPlayerDeath += HandlePlayerDeath;
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {wave}";
            }

            if (waveBannerPanel != null && waveBannerText != null)
            {
                StopAllCoroutines();
                StartCoroutine(ShowWaveBannerRoutine(wave));
            }

            Managers.AudioManager.Instance?.PlayWaveStartSound();
        }

        private void UpdateCombo(int comboCount, int comboMultiplier)
        {
            if (comboText == null)
            {
                return;
            }

            comboText.text = comboCount > 1
                ? $"Combo x{comboMultiplier} ({comboCount})"
                : "Combo x1";
        }

        private System.Collections.IEnumerator ShowWaveBannerRoutine(int wave)
        {
            waveBannerText.text = $"WAVE {wave}";
            waveBannerPanel.SetActive(true);
            yield return new WaitForSeconds(waveBannerDuration);
            waveBannerPanel.SetActive(false);
        }

        private void UpdateHealth(int current, int max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthText != null)
            {
                healthText.text = $"{current} / {max}";
            }
        }

        private void RefreshPowerUpIcons(bool rapidFireActive, bool shieldActive)
        {
            if (rapidFireIcon != null)
            {
                rapidFireIcon.gameObject.SetActive(rapidFireActive);
            }

            if (shieldIcon != null)
            {
                shieldIcon.gameObject.SetActive(shieldActive);
            }
        }

        private void HandlePlayerDeath()
        {
            gameManager?.GameOver();
            Managers.AudioManager.Instance?.PlayGameOverSound();
        }

        private void HandleGameStateChanged(Managers.GameManager.GameState state)
        {
            switch (state)
            {
                case Managers.GameManager.GameState.MainMenu:
                    ShowOnly(mainMenuPanel);
                    RefreshMenuHighScore();
                    break;
                case Managers.GameManager.GameState.Playing:
                    ShowOnly(hudPanel);
                    if (pausePanel != null) pausePanel.SetActive(false);
                    if (gameOverPanel != null) gameOverPanel.SetActive(false);
                    break;
                case Managers.GameManager.GameState.Paused:
                    if (pausePanel != null) pausePanel.SetActive(true);
                    break;
                case Managers.GameManager.GameState.GameOver:
                    ShowGameOverPanel();
                    break;
            }
        }

        private void ShowGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (gameManager == null)
            {
                return;
            }

            if (finalScoreText != null) finalScoreText.text = $"Score: {gameManager.Score}";
            if (finalWaveText != null) finalWaveText.text = $"Wave: {gameManager.CurrentWave}";
            if (gameOverHighScoreText != null) gameOverHighScoreText.text = $"High Score: {gameManager.HighScore}";
            if (newRecordText != null) newRecordText.gameObject.SetActive(gameManager.Score >= gameManager.HighScore && gameManager.Score > 0);
        }

        private void OnStartClicked()
        {
            Managers.AudioManager.Instance?.PlayButtonClickSound();

            SpawnAndResetIfNeeded();
            gameManager?.StartGame();
        }

        private void OnRestartClicked()
        {
            Managers.AudioManager.Instance?.PlayButtonClickSound();

            SpawnAndResetIfNeeded();
            gameManager?.RestartGame();
        }

        private void OnMenuClicked()
        {
            Managers.AudioManager.Instance?.PlayButtonClickSound();

            Managers.SpawnManager spawner = FindObjectOfType<Managers.SpawnManager>();
            if (spawner != null)
            {
                spawner.ClearAllEnemies();
            }

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

        private void SpawnAndResetIfNeeded()
        {
            if (player == null)
            {
                player = FindObjectOfType<Player.PlayerController>();
                BindPlayerEvents();
            }

            if (player != null)
            {
                player.ResetPlayer();
            }

            Managers.SpawnManager spawner = FindObjectOfType<Managers.SpawnManager>();
            if (spawner != null)
            {
                spawner.ClearAllEnemies();
            }
        }

        private void ShowOnly(GameObject activePanel)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(activePanel == mainMenuPanel);
            if (hudPanel != null) hudPanel.SetActive(activePanel == hudPanel);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (waveBannerPanel != null) waveBannerPanel.SetActive(false);
        }

        private void RefreshMenuHighScore()
        {
            if (menuHighScoreText != null && gameManager != null)
            {
                menuHighScoreText.text = $"High Score: {gameManager.HighScore}";
            }
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
                gameManager.OnScoreChanged -= UpdateScore;
                gameManager.OnWaveChanged -= UpdateWave;
                gameManager.OnComboChanged -= UpdateCombo;
            }

            if (player != null)
            {
                player.OnHealthChanged -= UpdateHealth;
                player.OnPowerUpStateChanged -= RefreshPowerUpIcons;
                player.OnPlayerDeath -= HandlePlayerDeath;
            }
        }
    }
}
