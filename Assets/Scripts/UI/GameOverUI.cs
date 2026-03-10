using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Managers;

namespace SpaceShooter.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI newHighScoreText;
        [SerializeField] private TextMeshProUGUI waveReachedText;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Start()
        {
            SetupButtons();
            SubscribeToEvents();
            
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SetupButtons()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += ShowGameOver;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver -= ShowGameOver;
            }
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                StartCoroutine(FadeIn());
            }

            UpdateScoreDisplay();
            AudioManager.Instance?.PlaySound("GameOver");
        }

        private System.Collections.IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsed / fadeInDuration;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private void UpdateScoreDisplay()
        {
            if (GameManager.Instance == null) return;

            int score = GameManager.Instance.Score;
            int highScore = GameManager.Instance.HighScore;
            int wave = GameManager.Instance.CurrentWave;

            if (finalScoreText != null)
                finalScoreText.text = $"Score: {score}";

            if (highScoreText != null)
                highScoreText.text = $"High Score: {highScore}";

            if (waveReachedText != null)
                waveReachedText.text = $"Wave Reached: {wave}";

            if (newHighScoreText != null)
                newHighScoreText.gameObject.SetActive(score >= highScore && score > 0);
        }

        private void OnRestartClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.RestartGame();
        }

        private void OnMainMenuClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.LoadMainMenu();
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.QuitGame();
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }
    }
}
