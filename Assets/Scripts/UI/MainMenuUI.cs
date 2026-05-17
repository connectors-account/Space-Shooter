using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu screen controller. Handles Play, Quit, and high score display.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;
    public Text titleText;
    public Text highScoreText;

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + PlayerPrefs.GetInt("HighScore", 0);
    }

    void OnPlayClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
        {
            // Fallback if GameManager doesn't exist yet
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }

    void OnQuitClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
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
