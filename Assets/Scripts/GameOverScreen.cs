using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Game-Over screen logic.
/// Attach to any GameObject in the GameOver scene.
/// Wire PLAY AGAIN and MAIN MENU button OnClick() in the Inspector.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    // Persist the high score for the session using PlayerPrefs
    const string HighScoreKey = "HighScore";

    void Start()
    {
        if (GameManager.Instance == null) return;

        int current   = GameManager.Instance.Score;
        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        if (current > highScore)
        {
            highScore = current;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        if (finalScoreText != null)
            finalScoreText.text = "SCORE\n" + current.ToString("D7");

        if (highScoreText != null)
            highScoreText.text  = "BEST\n"  + highScore.ToString("D7");
    }

    // Called by PLAY AGAIN button
    public void OnPlayAgainClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadGame();
        else
            SceneManager.LoadScene("Game");
    }

    // Called by MAIN MENU button
    public void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }
}
