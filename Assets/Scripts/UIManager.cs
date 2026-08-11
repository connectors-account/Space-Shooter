using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SpaceShooter
{
    /// <summary>
    /// Manages the in-game HUD (score + lives) and the Game Over panel.
    /// Uses Unity's legacy UI (Text/Button) so no extra packages are required.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [Tooltip("Text element showing the current score.")]
        [SerializeField] private Text scoreText;

        [Tooltip("Text element showing remaining lives.")]
        [SerializeField] private Text livesText;

        [Header("Game Over Panel")]
        [Tooltip("Root panel shown when the player dies. Hidden at start.")]
        [SerializeField] private GameObject gameOverPanel;

        [Tooltip("Text element showing the final score on the Game Over panel.")]
        [SerializeField] private Text finalScoreText;

        [Header("Scene Names")]
        [Tooltip("Name of the gameplay scene (used by Restart).")]
        [SerializeField] private string gameSceneName = "Game";

        [Tooltip("Name of the main menu scene.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            HideGameOver();
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score;
            }
        }

        public void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = "Lives: " + lives;
            }
        }

        public void ShowGameOver(int finalScore)
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + finalScore;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        /// <summary>Hooked to the Restart button OnClick in the Inspector.</summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>Hooked to a "Main Menu" button OnClick in the Inspector.</summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
