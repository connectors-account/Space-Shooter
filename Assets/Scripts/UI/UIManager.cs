using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all in-game UI panels: HUD, Pause Menu, and Game Over screen.
/// Coordinates with GameManager for state changes.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text healthText;
    [SerializeField] private Image[] healthIcons;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button gameOverMainMenuButton;

    private void Start()
    {
        SetupButtonListeners();
        HideAllPanels();
        ShowHUD();
    }

    /// <summary>
    /// Wires up all button click listeners.
    /// </summary>
    private void SetupButtonListeners()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    /// <summary>
    /// Hides all UI panels.
    /// </summary>
    private void HideAllPanels()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Shows the in-game HUD.
    /// </summary>
    public void ShowHUD()
    {
        HideAllPanels();
        if (hudPanel != null) hudPanel.SetActive(true);
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score:N0}";
    }

    /// <summary>
    /// Updates the wave number display.
    /// </summary>
    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = wave > 0 ? $"WAVE {wave}" : "";
    }

    /// <summary>
    /// Updates the health display with text and optional icons.
    /// </summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"HP: {current}/{max}";

        if (healthIcons != null)
        {
            for (int i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] != null)
                    healthIcons[i].enabled = i < current;
            }
        }
    }

    /// <summary>
    /// Shows the pause menu overlay.
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    /// <summary>
    /// Hides the pause menu overlay.
    /// </summary>
    public void HidePauseMenu()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    /// <summary>
    /// Shows the game over screen with the final score.
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        HideAllPanels();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = $"FINAL SCORE\n{finalScore:N0}";
    }

    private void OnResumeClicked()
    {
        GameManager.Instance.ResumeGame();
    }

    private void OnRestartClicked()
    {
        GameManager.Instance.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
