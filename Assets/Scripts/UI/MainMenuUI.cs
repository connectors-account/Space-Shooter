// ============================================================================
// MainMenuUI.cs - Main menu screen controller
// Provides Start Game, High Score display, and Quit buttons.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Main Menu UI. Attach to the Canvas in the MainMenu scene.
/// Buttons are wired here in code (no Inspector drag-drop required).
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text instructionsText;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Start()
    {
        // Wire button callbacks.
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Display high score.
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = $"High Score: {highScore:N0}";

        // Set title.
        if (titleText != null)
            titleText.text = "SPACE SHOOTER";

        // Display control hints.
        if (instructionsText != null)
            instructionsText.text = "WASD / Arrow Keys - Move\nSpace / Left Click - Fire\nP / Esc - Pause";

        // Play menu music.
        AudioManager.Instance?.PlayMusic("menu_music");
    }

    // ========================================================================
    // Button Callbacks
    // ========================================================================

    private void OnStartClicked()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.UIClick);
        AudioManager.Instance?.StopMusic();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }

    private void OnQuitClicked()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.UIClick);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
