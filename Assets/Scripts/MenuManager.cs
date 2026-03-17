using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the Main Menu scene UI: play, quit buttons, and high score display.
/// Attach to a Canvas GameObject in the MainMenu scene.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text versionText;

    [Header("Title Animation")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinScale = 0.95f;
    [SerializeField] private float pulseMaxScale = 1.05f;

    private void Start()
    {
        Time.timeScale = 1f;

        // Display high score
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "High Score: " + highScore.ToString("N0");
        }

        // Version text
        if (versionText != null)
            versionText.text = "v1.0";

        // Wire up buttons
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void Update()
    {
        // Animate title with a pulsing scale effect
        if (titleText != null)
        {
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            titleText.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    /// <summary>
    /// Start the game when Play is clicked.
    /// </summary>
    private void OnPlayClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            // Fallback if GameManager isn't loaded yet
            SceneManager.LoadScene("GamePlay");
        }
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
