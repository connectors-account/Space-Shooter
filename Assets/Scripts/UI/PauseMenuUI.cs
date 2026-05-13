// ============================================================================
// PauseMenuUI.cs - In-game pause overlay
// Shows/hides a pause panel and provides Resume, Restart, and Main Menu buttons.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the pause menu overlay. Listens to GameManager state changes
/// to show/hide automatically.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Text pauseTitleText;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Start()
    {
        // Initially hidden.
        if (pausePanel != null) pausePanel.SetActive(false);

        // Wire buttons.
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (pauseTitleText != null) pauseTitleText.text = "PAUSED";

        // Listen for state changes.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleStateChange;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleStateChange;
        }
    }

    // ========================================================================
    // State Handling
    // ========================================================================

    private void HandleStateChange(GameManager.GameState state)
    {
        if (pausePanel == null) return;

        pausePanel.SetActive(state == GameManager.GameState.Paused);
    }

    // ========================================================================
    // Button Callbacks
    // ========================================================================

    private void OnResumeClicked()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.UIClick);
        GameManager.Instance?.TogglePause();
    }

    private void OnRestartClicked()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.UIClick);
        Time.timeScale = 1f;
        GameManager.Instance?.StartGame();
    }

    private void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.UIClick);
        Time.timeScale = 1f;
        GameManager.Instance?.ReturnToMainMenu();
    }
}
