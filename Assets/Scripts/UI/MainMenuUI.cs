using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Main menu screen with Start Game and Quit buttons.
    /// Also displays the high score.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        private void Start()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Subscribe to game state changes
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }

            UpdateHighScore();
            Show();
        }

        private void OnDestroy()
        {
            if (Managers.GameManager.Instance != null)
                Managers.GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(Managers.GameState state)
        {
            if (state == Managers.GameState.MainMenu)
            {
                UpdateHighScore();
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void UpdateHighScore()
        {
            if (highScoreText != null && Managers.GameManager.Instance != null)
            {
                int hs = Managers.GameManager.Instance.HighScore;
                highScoreText.text = hs > 0 ? $"HIGH SCORE: {hs}" : "";
            }
        }

        private void OnStartClicked()
        {
            Managers.GameManager.Instance?.StartGame();
        }

        private void OnQuitClicked()
        {
            Managers.GameManager.Instance?.QuitGame();
        }

        public void Show()
        {
            if (menuPanel != null) menuPanel.SetActive(true);
        }

        public void Hide()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
        }
    }
}
