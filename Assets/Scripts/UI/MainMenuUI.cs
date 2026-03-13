using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu screen with Start, Quit buttons and high score display.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Text titleText;
    public Text highScoreText;
    public Button startButton;
    public Button quitButton;
    public Text versionText;

    private void Start()
    {
        if (titleText != null)
            titleText.text = "SPACE SHOOTER";

        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (versionText != null)
            versionText.text = "v1.0.0";

        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Allow pressing Enter or Space to start
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnStartClicked();
        }
    }

    private void OnStartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

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
