// ============================================================================
// PauseMenuUI.cs — Pause menu overlay
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text pauseTitle;

    // =========================================================================
    private void Start()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        if (pausePanel != null) pausePanel.SetActive(false);
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
        if (pausePanel != null)
            pausePanel.SetActive(state == GameState.Paused);
    }

    // =========================================================================
    private void OnResumeClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TogglePause();
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
    }

    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }
}
