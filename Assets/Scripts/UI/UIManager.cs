using SpaceShooter.Core;
using SpaceShooter.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class UIManager : MonoBehaviour
    {
        private Text scoreText;
        private Text waveText;
        private Text healthText;
        private Text statusText;

        private GameObject mainMenuPanel;
        private GameObject hudPanel;
        private GameObject pausePanel;
        private GameObject gameOverPanel;

        private Text menuHighScoreText;
        private Text gameOverText;

        private Button startButton;
        private Button quitButton;
        private Button resumeButton;
        private Button pauseMenuButton;
        private Button restartButton;
        private Button gameOverMenuButton;

        private GameManager gameManager;

        public void Bind(GameManager manager)
        {
            gameManager = manager;
            gameManager.OnStateChanged += HandleStateChange;
            gameManager.OnScoreChanged += UpdateScore;
            gameManager.OnWaveChanged += UpdateWave;
        }

        public void BindButtons(GameManager manager)
        {
            startButton.onClick.AddListener(manager.StartNewGame);
            quitButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            resumeButton.onClick.AddListener(manager.TogglePause);
            pauseMenuButton.onClick.AddListener(manager.ReturnToMenu);
            restartButton.onClick.AddListener(manager.RestartFromGameOver);
            gameOverMenuButton.onClick.AddListener(manager.ReturnToMenu);
        }

        public void SetElements(
            Text score,
            Text wave,
            Text health,
            Text status,
            GameObject menu,
            GameObject hud,
            GameObject pause,
            GameObject gameOver,
            Text menuHighScore,
            Text gameOverSummary,
            Button menuStart,
            Button menuQuit,
            Button pauseResume,
            Button pauseMenu,
            Button overRestart,
            Button overMenu)
        {
            scoreText = score;
            waveText = wave;
            healthText = health;
            statusText = status;
            mainMenuPanel = menu;
            hudPanel = hud;
            pausePanel = pause;
            gameOverPanel = gameOver;
            menuHighScoreText = menuHighScore;
            gameOverText = gameOverSummary;
            startButton = menuStart;
            quitButton = menuQuit;
            resumeButton = pauseResume;
            pauseMenuButton = pauseMenu;
            restartButton = overRestart;
            gameOverMenuButton = overMenu;
        }

        public void RefreshMenu(int highScore)
        {
            menuHighScoreText.text = $"High Score: {highScore}";
            statusText.text = "WASD/Arrows Move • Space Shoot • Esc Pause";
        }

        public void UpdateScore(int score)
        {
            scoreText.text = $"Score: {score}";
        }

        public void UpdateWave(int wave)
        {
            waveText.text = $"Wave: {wave}";
            statusText.text = $"Wave {wave} incoming";
            CancelInvoke(nameof(ClearStatus));
            Invoke(nameof(ClearStatus), 1.5f);
        }

        public void UpdateHealth(int current, int max)
        {
            healthText.text = $"HP: {current}/{max}";
        }

        public void ShowGameOver(int finalScore, int highScore, int wave)
        {
            gameOverText.text = $"Game Over\nScore: {finalScore}\nHigh Score: {highScore}\nWave Reached: {wave}";
        }

        private void HandleStateChange(GameManager.GameState state)
        {
            mainMenuPanel.SetActive(state == GameManager.GameState.MainMenu);
            hudPanel.SetActive(state == GameManager.GameState.Playing || state == GameManager.GameState.Paused || state == GameManager.GameState.GameOver);
            pausePanel.SetActive(state == GameManager.GameState.Paused);
            gameOverPanel.SetActive(state == GameManager.GameState.GameOver);
        }

        private void ClearStatus()
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                statusText.text = string.Empty;
                return;
            }

            string rapid = player.IsRapidFireActive() ? "Rapid Fire" : string.Empty;
            string shield = player.IsShieldActive() ? "Shield" : string.Empty;
            string delimiter = string.IsNullOrEmpty(rapid) || string.IsNullOrEmpty(shield) ? string.Empty : " • ";
            statusText.text = rapid + delimiter + shield;
        }

        private void Update()
        {
            if (gameManager != null && gameManager.State == GameManager.GameState.Playing)
            {
                ClearStatus();
            }
        }
    }
}
