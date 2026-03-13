using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game over screen showing final score, high score, and restart/menu options.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public Text gameOverTitle;
    public Text finalScoreText;
    public Text highScoreText;
    public Text newHighScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
        }

        if (newHighScoreText != null)
            newHighScoreText.gameObject.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverTitle != null)
            gameOverTitle.text = "GAME OVER";

        if (GameManager.Instance != null)
        {
            if (finalScoreText != null)
                finalScoreText.text = "SCORE: " + GameManager.Instance.CurrentScore.ToString("N0");

            if (highScoreText != null)
                highScoreText.text = "HIGH SCORE: " + GameManager.Instance.HighScore.ToString("N0");

            if (newHighScoreText != null)
            {
                bool isNewHighScore = GameManager.Instance.CurrentScore >= GameManager.Instance.HighScore;
                newHighScoreText.gameObject.SetActive(isNewHighScore);
                if (isNewHighScore)
                    newHighScoreText.text = "NEW HIGH SCORE!";
            }
        }
    }

    private void Update()
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                OnRestartClicked();
            }
        }
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }
}
