using SpaceShooter.Core;
using SpaceShooter.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text waveText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Text statusEffectsText;

        [Header("Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject mainMenuPanel;

        [Header("Game Over")]
        [SerializeField] private Text gameOverScoreText;
        [SerializeField] private Text gameOverHighScoreText;

        private PlayerController player;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
            GameManager.Instance.OnWaveChanged += HandleWaveChanged;
            GameManager.Instance.OnStateChanged += HandleStateChanged;

            player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.OnHealthChanged += HandleHealthChanged;
                player.OnShieldChanged += HandleShieldStatus;
                player.OnRapidFireChanged += HandleRapidFireStatus;
            }

            HandleScoreChanged(GameManager.Instance.Score);
            HandleWaveChanged(GameManager.Instance.CurrentWave);
            HandleStateChanged(GameManager.Instance.CurrentState);
            UpdateHighScore();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
                GameManager.Instance.OnWaveChanged -= HandleWaveChanged;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }

            if (player != null)
            {
                player.OnHealthChanged -= HandleHealthChanged;
                player.OnShieldChanged -= HandleShieldStatus;
                player.OnRapidFireChanged -= HandleRapidFireStatus;
            }
        }

        private void HandleScoreChanged(int score)
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
            UpdateHighScore();
        }

        private void HandleWaveChanged(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {Mathf.Max(1, wave)} / 5";
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthText != null)
            {
                healthText.text = $"HP: {current}/{max}";
            }
        }

        private void HandleShieldStatus(bool active, float timer)
        {
            RefreshStatusEffectsText(activeShield: active, shieldTime: timer, activeRapid: null, rapidTime: null);
        }

        private void HandleRapidFireStatus(bool active, float timer)
        {
            RefreshStatusEffectsText(activeShield: null, shieldTime: null, activeRapid: active, rapidTime: timer);
        }

        private bool shieldActive;
        private float shieldRemaining;
        private bool rapidActive;
        private float rapidRemaining;

        private void RefreshStatusEffectsText(bool? activeShield, float? shieldTime, bool? activeRapid, float? rapidTime)
        {
            if (activeShield.HasValue) shieldActive = activeShield.Value;
            if (shieldTime.HasValue) shieldRemaining = shieldTime.Value;
            if (activeRapid.HasValue) rapidActive = activeRapid.Value;
            if (rapidTime.HasValue) rapidRemaining = rapidTime.Value;

            if (statusEffectsText == null) return;

            string shield = shieldActive ? $"Shield {shieldRemaining:0.0}s" : string.Empty;
            string rapid = rapidActive ? $"Rapid {rapidRemaining:0.0}s" : string.Empty;
            string separator = !string.IsNullOrEmpty(shield) && !string.IsNullOrEmpty(rapid) ? " | " : string.Empty;

            statusEffectsText.text = shield + separator + rapid;
        }

        private void HandleStateChanged(GameState state)
        {
            bool onMainMenu = SceneManager.GetActiveScene().name == "MainMenu";

            if (mainMenuPanel != null) mainMenuPanel.SetActive(onMainMenu);
            if (hudPanel != null) hudPanel.SetActive(!onMainMenu && state == GameState.Playing);
            if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
            if (gameOverPanel != null) gameOverPanel.SetActive(!onMainMenu && state == GameState.GameOver);

            if (state == GameState.GameOver)
            {
                if (gameOverScoreText != null)
                {
                    gameOverScoreText.text = $"Score: {GameManager.Instance.Score}";
                }
                if (gameOverHighScoreText != null)
                {
                    gameOverHighScoreText.text = $"High Score: {GameManager.Instance.HighScore}";
                }
            }
        }

        private void UpdateHighScore()
        {
            if (highScoreText != null && GameManager.Instance != null)
            {
                highScoreText.text = $"High Score: {GameManager.Instance.HighScore}";
            }
        }

        public void OnStartGamePressed() => GameManager.Instance?.StartGame();
        public void OnResumePressed() => GameManager.Instance?.TogglePause();
        public void OnRestartPressed() => GameManager.Instance?.StartGame();
        public void OnMainMenuPressed() => GameManager.Instance?.BackToMainMenu();
        public void OnQuitPressed() => GameManager.Instance?.QuitGame();
    }
}
