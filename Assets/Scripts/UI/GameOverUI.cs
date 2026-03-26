// ============================================================================
// GameOverUI.cs — Game Over / Victory screen
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Game Over UI")]
    [SerializeField] private Text gameOverTitleText;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text newHighScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Victory UI")]
    [SerializeField] private Text victoryTitleText;
    [SerializeField] private Text victoryScoreText;
    [SerializeField] private Button victoryRetryButton;
    [SerializeField] private Button victoryMenuButton;

    // =========================================================================
    private void Start()
    {
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        if (victoryRetryButton != null) victoryRetryButton.onClick.AddListener(OnRetryClicked);
        if (victoryMenuButton != null) victoryMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            ShowGameOver();
        }
        else if (state == GameState.Victory)
        {
            ShowVictory();
        }
        else
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }
    }

    // =========================================================================
    private void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;

        if (finalScoreText != null)
            finalScoreText.text = $"SCORE: {score:N0}";

        if (highScoreText != null)
            highScoreText.text = $"HIGH SCORE: {Mathf.Max(score, highScore):N0}";

        if (newHighScoreText != null)
            newHighScoreText.gameObject.SetActive(score > highScore);
    }

    private void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);

        int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        if (victoryScoreText != null)
            victoryScoreText.text = $"FINAL SCORE: {score:N0}";
    }

    // =========================================================================
    private void OnRetryClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
    }

    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }
}
