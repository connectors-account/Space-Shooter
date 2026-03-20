using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the Main Menu scene: Start Game, Quit, and High Score display.
/// Attach to a Canvas in the MainMenuScene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("Animation")]
    [SerializeField] private float titlePulseSpeed = 2f;
    [SerializeField] private float titlePulseMin = 0.8f;

    private void Start()
    {
        // Display high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore.ToString("N0");

        // Wire buttons
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Pulse the title text for a simple animated effect
        if (titleText != null)
        {
            float scale = titlePulseMin + (1f - titlePulseMin) *
                (0.5f + 0.5f * Mathf.Sin(Time.time * titlePulseSpeed));
            titleText.transform.localScale = Vector3.one * scale;
        }
    }

    /// <summary>
    /// Loads the game scene.
    /// </summary>
    public void OnStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void OnQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
