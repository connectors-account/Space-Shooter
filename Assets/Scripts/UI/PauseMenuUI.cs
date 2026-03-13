using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu overlay with Resume, Restart, and Main Menu buttons.
/// Shown when the game is paused via ESC key.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Text pauseTitle;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePaused += OnGamePaused;
        }
    }

    private void OnGamePaused(bool isPaused)
    {
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
    }

    private void OnResumeClicked()
    {
        GameManager.Instance?.TogglePause();
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
            GameManager.Instance.OnGamePaused -= OnGamePaused;
        }
    }
}
