using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceShooter
{
    /// <summary>
    /// Manages all UI: main menu, HUD, pause menu and game over screen.
    /// Subscribes to GameManager / ScoreManager / HealthSystem events and shows
    /// the correct panel for the current game state.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("HUD Elements")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text waveBannerText;
        [SerializeField] private TMP_Text powerUpText;

        [Header("Menu Elements")]
        [SerializeField] private TMP_Text menuHighScoreText;

        [Header("Game Over Elements")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text gameOverHighScoreText;

        [Header("References")]
        [SerializeField] private HealthSystem playerHealth;

        [Header("Tuning")]
        [SerializeField] private Color healthHighColor = Color.green;
        [SerializeField] private Color healthLowColor = Color.red;

        private GameManager gameManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            gameManager = GameManager.Instance;

            if (gameManager != null)
            {
                gameManager.OnStateChanged += HandleStateChanged;
                gameManager.OnLivesChanged += UpdateLives;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScore;
                ScoreManager.Instance.OnHighScoreChanged += UpdateHighScore;
                UpdateHighScore(ScoreManager.Instance.HighScore);
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
            }

            HideAllPanels();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= HandleStateChanged;
                gameManager.OnLivesChanged -= UpdateLives;
            }
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
                ScoreManager.Instance.OnHighScoreChanged -= UpdateHighScore;
            }
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealth;
            }
        }

        // ---------- State-driven panel switching ----------

        private void HandleStateChanged(GameState state)
        {
            HideAllPanels();
            switch (state)
            {
                case GameState.MainMenu:
                    if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                    if (ScoreManager.Instance != null) UpdateHighScore(ScoreManager.Instance.HighScore);
                    break;
                case GameState.Playing:
                    if (hudPanel != null) hudPanel.SetActive(true);
                    break;
                case GameState.Paused:
                    if (hudPanel != null) hudPanel.SetActive(true);
                    if (pausePanel != null) pausePanel.SetActive(true);
                    break;
                case GameState.GameOver:
                    if (gameOverPanel != null) gameOverPanel.SetActive(true);
                    ShowGameOver();
                    break;
            }
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        // ---------- HUD updates ----------

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"SCORE: {score}";
        }

        private void UpdateHighScore(int highScore)
        {
            if (menuHighScoreText != null) menuHighScoreText.text = $"HIGH SCORE: {highScore}";
        }

        private void UpdateLives(int lives)
        {
            if (livesText != null) livesText.text = $"LIVES: {lives}";
        }

        private void UpdateHealth(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }
            if (healthFill != null)
            {
                float pct = max > 0f ? current / max : 0f;
                healthFill.color = Color.Lerp(healthLowColor, healthHighColor, pct);
            }
        }

        public void SetWaveText(int wave)
        {
            if (waveText != null) waveText.text = $"WAVE: {wave}";
        }

        // ---------- Transient banners ----------

        public void ShowWaveBanner(int wave)
        {
            SetWaveText(wave);
            if (waveBannerText == null) return;
            waveBannerText.text = $"WAVE {wave}";
            StopCoroutine(nameof(FadeBanner));
            StartCoroutine(FadeBanner());
        }

        private IEnumerator FadeBanner()
        {
            waveBannerText.gameObject.SetActive(true);
            Color c = waveBannerText.color;
            c.a = 1f;
            waveBannerText.color = c;

            yield return new WaitForSeconds(1.5f);

            float t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                c.a = t;
                waveBannerText.color = c;
                yield return null;
            }
            waveBannerText.gameObject.SetActive(false);
        }

        public void ShowPowerUpText(string label)
        {
            if (powerUpText == null) return;
            powerUpText.text = $"{label.ToUpper()}!";
            StopCoroutine(nameof(FadePowerUpText));
            StartCoroutine(FadePowerUpText());
        }

        private IEnumerator FadePowerUpText()
        {
            powerUpText.gameObject.SetActive(true);
            Color c = powerUpText.color;
            c.a = 1f;
            powerUpText.color = c;

            yield return new WaitForSeconds(1f);

            float t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime * 1.5f;
                c.a = t;
                powerUpText.color = c;
                yield return null;
            }
            powerUpText.gameObject.SetActive(false);
        }

        private void ShowGameOver()
        {
            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
            int highScore = ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0;
            if (finalScoreText != null) finalScoreText.text = $"SCORE: {finalScore}";
            if (gameOverHighScoreText != null) gameOverHighScoreText.text = $"HIGH SCORE: {highScore}";
        }

        // ---------- Button hooks (wire these in the Inspector) ----------

        public void OnStartButton()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.StartGame();
        }

        public void OnRestartButton()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.RestartGame();
        }

        public void OnResumeButton()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.ResumeGame();
        }

        public void OnMainMenuButton()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.ReturnToMainMenu();
        }

        public void OnQuitButton()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.QuitGame();
        }
    }
}
