using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// Game-over screen. Shows the final and high scores, flags a new high score,
    /// and wires the Retry / Main Menu buttons. Shown via GameManager.OnGameOver.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private GameObject newHighScoreLabel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (panel != null) panel.SetActive(false);
            if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);

            if (GameManager.Instance != null) GameManager.Instance.OnGameOver += Show;
        }

        private void OnDestroy()
        {
            if (retryButton != null) retryButton.onClick.RemoveListener(OnRetry);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenu);
            if (GameManager.Instance != null) GameManager.Instance.OnGameOver -= Show;
        }

        /// <summary>Displays the game-over panel and fills in the score fields.</summary>
        public void Show()
        {
            if (panel != null) panel.SetActive(true);

            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
            int highScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetHighScore() : 0;

            if (finalScoreText != null) finalScoreText.text = $"SCORE: {finalScore}";
            if (highScoreText != null) highScoreText.text = $"HIGH SCORE: {highScore}";

            bool isNewHigh = finalScore >= highScore && finalScore > 0;
            if (newHighScoreLabel != null) newHighScoreLabel.SetActive(isNewHigh);
        }

        private void OnRetry()
        {
            if (panel != null) panel.SetActive(false);
            if (SceneLoader.Instance != null) SceneLoader.Instance.ReloadGame();
            if (GameManager.Instance != null) GameManager.Instance.NewGame();
        }

        private void OnMainMenu()
        {
            if (panel != null) panel.SetActive(false);
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadMainMenu();
            if (GameManager.Instance != null) GameManager.Instance.EnterMainMenu();
        }
    }
}
