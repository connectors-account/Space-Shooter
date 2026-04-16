using SpaceShooter.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text finalScoreText;

        private void Start()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (gameOverPanel == null)
            {
                return;
            }

            bool show = state == GameState.GameOver;
            gameOverPanel.SetActive(show);

            if (show && finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {ScoreManager.CurrentScore}";
            }
        }

        public void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGameplay();
            }
        }

        public void OnMainMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BackToMainMenu();
            }
        }
    }
}
