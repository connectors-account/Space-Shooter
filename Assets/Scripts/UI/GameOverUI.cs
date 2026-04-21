using SpaceShooter.Audio;
using SpaceShooter.Core;
using TMPro;
using UnityEngine;

namespace SpaceShooter.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text finalScoreText;

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void Start()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        public void OnRetryClicked()
        {
            AudioManager.Instance?.PlayUIClick();
            GameManager.Instance.StartNewGame();
        }

        public void OnMainMenuClicked()
        {
            AudioManager.Instance?.PlayUIClick();
            GameManager.Instance.ReturnToMainMenu();
        }

        private void HandleGameStateChanged(GameState state)
        {
            bool isGameOver = state == GameState.GameOver;
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(isGameOver);
            }

            if (isGameOver)
            {
                if (finalScoreText != null)
                {
                    finalScoreText.text = $"Final Score: {GameManager.Instance.Score}";
                }

                AudioManager.Instance?.PlayGameOver();
            }
        }
    }
}
