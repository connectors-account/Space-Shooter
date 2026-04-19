using TMPro;
using UnityEngine;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Handles all game screens and HUD updates.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("HUD Text")]
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text gameOverScoreText;

        [Header("Power-up Indicators")]
        [SerializeField] private GameObject shieldIndicator;
        [SerializeField] private GameObject rapidFireIndicator;

        [Header("References")]
        [SerializeField] private Player.PlayerController playerController;
        [SerializeField] private Player.PlayerHealth playerHealth;
        [SerializeField] private Systems.ScoreSystem scoreSystem;
        [SerializeField] private Core.GameManager gameManager;

        private void Start()
        {
            if (gameManager == null) gameManager = Core.GameManager.Instance;
            if (scoreSystem == null) scoreSystem = Systems.ScoreSystem.Instance;

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += RefreshPanels;
                gameManager.OnWaveChanged += UpdateWave;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
            }

            if (scoreSystem != null)
            {
                scoreSystem.OnScoreChanged += UpdateScore;
                UpdateScore(scoreSystem.CurrentScore);
            }

            UpdateWave(1);
            RefreshPanels(Core.GameManager.GameState.MainMenu);
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= RefreshPanels;
                gameManager.OnWaveChanged -= UpdateWave;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealth;
            }

            if (scoreSystem != null)
            {
                scoreSystem.OnScoreChanged -= UpdateScore;
            }
        }

        private void Update()
        {
            if (playerHealth != null && shieldIndicator != null)
            {
                shieldIndicator.SetActive(playerHealth.IsShieldActive);
            }

            if (playerController != null && rapidFireIndicator != null)
            {
                rapidFireIndicator.SetActive(playerController.IsRapidFireActive);
            }

            if (scoreSystem != null && highScoreText != null)
            {
                highScoreText.text = $"High: {scoreSystem.HighScore}";
            }
        }

        private void RefreshPanels(Core.GameManager.GameState state)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(state == Core.GameManager.GameState.MainMenu);
            if (hudPanel != null) hudPanel.SetActive(state == Core.GameManager.GameState.Playing || state == Core.GameManager.GameState.Paused || state == Core.GameManager.GameState.GameOver);
            if (pausePanel != null) pausePanel.SetActive(state == Core.GameManager.GameState.Paused);
            if (gameOverPanel != null) gameOverPanel.SetActive(state == Core.GameManager.GameState.GameOver);

            if (state == Core.GameManager.GameState.GameOver && scoreSystem != null && gameOverScoreText != null)
            {
                gameOverScoreText.text = $"Final Score: {scoreSystem.CurrentScore}";
            }
        }

        private void UpdateHealth(int current, int max)
        {
            if (healthText != null)
            {
                healthText.text = $"Health: {current}/{max}";
            }
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
        }

        // UI Button hooks
        public void OnStartButtonPressed() => gameManager?.StartGame();
        public void OnResumeButtonPressed() => gameManager?.TogglePause();
        public void OnRestartButtonPressed() => gameManager?.RestartGame();
        public void OnMainMenuButtonPressed() => gameManager?.ReturnToMainMenu();

        public void OnQuitButtonPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
