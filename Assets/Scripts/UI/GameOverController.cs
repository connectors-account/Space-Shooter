// =============================================================================
// GameOverController.cs — Game over screen logic
// =============================================================================
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Controls the game over screen: displays final score and options.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text gameOverText;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text newHighScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            Managers.GameManager gm = Managers.GameManager.Instance;

            if (finalScoreText != null && gm != null)
                finalScoreText.text = $"FINAL SCORE: {gm.Score:N0}";

            if (highScoreText != null && gm != null)
                highScoreText.text = $"HIGH SCORE: {gm.HighScore:N0}";

            // Show "NEW HIGH SCORE!" if applicable
            if (newHighScoreText != null && gm != null)
                newHighScoreText.gameObject.SetActive(gm.Score >= gm.HighScore && gm.Score > 0);

            // Setup buttons
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Play game over music
            Managers.SoundManager.Instance?.PlayMusic("gameover");
        }

        private void Update()
        {
            // Pulsing game over text
            if (gameOverText != null)
            {
                float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
                gameOverText.transform.localScale = Vector3.one * scale;
            }
        }

        private void OnRestartClicked()
        {
            Managers.SoundManager.Instance?.PlaySFX("menu_select");
            Managers.GameManager.Instance?.StartGame();
        }

        private void OnMainMenuClicked()
        {
            Managers.SoundManager.Instance?.PlaySFX("menu_select");
            Managers.GameManager.Instance?.GoToMainMenu();
        }

        private void OnQuitClicked()
        {
            Managers.GameManager.Instance?.QuitGame();
        }
    }
}
