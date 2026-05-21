using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Game over screen showing final score with Restart and Main Menu buttons.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI gameOverTitle;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            var gm = Managers.GameManager.Instance;
            if (gm != null)
            {
                gm.OnGameOver += ShowGameOver;
                gm.OnGameStateChanged += OnGameStateChanged;
            }

            Hide();
        }

        private void OnDestroy()
        {
            var gm = Managers.GameManager.Instance;
            if (gm != null)
            {
                gm.OnGameOver -= ShowGameOver;
                gm.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(Managers.GameState state)
        {
            if (state != Managers.GameState.GameOver)
                Hide();
        }

        private void ShowGameOver(int finalScore)
        {
            if (finalScoreText != null)
                finalScoreText.text = $"SCORE: {finalScore}";

            if (highScoreText != null)
            {
                int hs = Managers.GameManager.Instance?.HighScore ?? 0;
                highScoreText.text = $"HIGH SCORE: {hs}";
                if (finalScore >= hs)
                    highScoreText.text = "NEW HIGH SCORE!";
            }

            Show();
        }

        private void OnRestartClicked()
        {
            Managers.GameManager.Instance?.RestartGame();
        }

        private void OnMainMenuClicked()
        {
            Managers.GameManager.Instance?.GoToMainMenu();
        }

        public void Show()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        public void Hide()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }
    }
}
