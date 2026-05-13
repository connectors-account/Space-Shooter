// ============================================================================
// GameOverUI.cs — Game Over screen with final score and restart option
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI newHighScoreLabel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            restartButton?.onClick.AddListener(OnRestart);
            mainMenuButton?.onClick.AddListener(OnMainMenu);

            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleStateChange;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(GameState state)
        {
            if (state == GameState.GameOver)
                ShowGameOver();
            else if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;

            if (finalScoreText != null) finalScoreText.text = $"SCORE: {score:N0}";
            if (highScoreText != null) highScoreText.text = $"HIGH SCORE: {highScore:N0}";
            if (newHighScoreLabel != null) newHighScoreLabel.gameObject.SetActive(score >= highScore && score > 0);
        }

        private void OnRestart()
        {
            GameManager.Instance?.StartGame();
        }

        private void OnMainMenu()
        {
            GameManager.Instance?.ReturnToMainMenu();
        }
    }
}
