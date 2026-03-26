using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles main-menu buttons and the in-game pause menu (ESC key).
/// Attach one instance in MenuScene (main menu) and one in GameScene (pause).
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons (MenuScene only)")]
    public Button playButton;
    public Button quitButton;

    [Header("Pause Menu (GameScene only)")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button mainMenuButton;

    private void Start()
    {
        // Main-menu wiring
        if (playButton != null) playButton.onClick.AddListener(OnPlay);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        // Pause-menu wiring
        if (resumeButton   != null) resumeButton.onClick.AddListener(OnResume);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Update()
    {
        // ESC toggles pause only during gameplay
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState == GameManager.State.Playing)
            {
                GameManager.Instance.PauseGame();
                if (pausePanel != null) pausePanel.SetActive(true);
            }
            else if (GameManager.Instance.CurrentState == GameManager.State.Paused)
            {
                OnResume();
            }
        }
    }

    // ── Callbacks ─────────────────────────────────────────────────────
    private void OnPlay()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuClick();
        SceneManager.LoadScene("GameScene");

        // StartGame is called after the scene loads (see GameSceneBootstrap)
    }

    private void OnQuit()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnResume()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuClick();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    private void OnMainMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuClick();
        if (GameManager.Instance  != null) GameManager.Instance.ReturnToMenu();
    }
}
