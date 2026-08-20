using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.UI
{
    /// <summary>
    /// End-of-run screen. Shows "GAME OVER" or "VICTORY!", the final score, the persisted
    /// high score (with a "NEW RECORD!" flag), and Restart / Main Menu buttons.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panelRoot;

        [Header("Text")]
        public Text titleText;
        public Text finalScoreText;
        public Text highScoreText;
        public Text newRecordText;

        [Header("Buttons")]
        public Button restartButton;
        public Button mainMenuButton;

        private void Start()
        {
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoMainMenu);
            if (panelRoot != null) panelRoot.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += () => Show(false);
                GameManager.Instance.OnVictory += () => Show(true);
            }
        }

        public void Show(bool victory)
        {
            if (panelRoot != null) panelRoot.SetActive(true);

            if (titleText != null)
            {
                titleText.text = victory ? "VICTORY!" : "GAME OVER";
                titleText.color = victory ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.35f, 0.35f);
            }

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore()
                : (GameManager.Instance != null ? GameManager.Instance.Score : 0);
            int high = ScoreManager.Instance != null ? ScoreManager.Instance.GetHighScore()
                : PlayerPrefs.GetInt("SpaceShooter_HighScore", 0);

            if (finalScoreText != null) finalScoreText.text = $"FINAL SCORE: {score:N0}";
            if (highScoreText != null) highScoreText.text = $"HIGH SCORE: {high:N0}";
            if (newRecordText != null) newRecordText.text = (score >= high && score > 0) ? "NEW RECORD!" : "";
        }

        private void Restart()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
            if (panelRoot != null) panelRoot.SetActive(false);
            if (SceneLoader.Instance != null) SceneLoader.Instance.ReloadGame();
        }

        private void GoMainMenu()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
            if (panelRoot != null) panelRoot.SetActive(false);
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadMainMenu();
        }
    }
}
