using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MenuManager - Handles Main Menu and Game Over screen UI logic.
/// Attach to a Canvas in MainMenu and GameOver scenes.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Main Menu Elements")]
    public Text titleText;
    public Button playButton;
    public Button quitButton;

    [Header("Game Over Elements")]
    public Text gameOverTitleText;
    public Text finalScoreText;
    public Text finalWaveText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Audio")]
    public AudioSource buttonClickAudio;

    private void Start()
    {
        Time.timeScale = 1f;

        // Wire up buttons based on which scene we're in
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            SetupMainMenu();
        }
        else if (sceneName == "GameOver")
        {
            SetupGameOverScreen();
        }
    }

    private void SetupMainMenu()
    {
        if (titleText != null)
            titleText.text = "SPACE SHOOTER";

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void SetupGameOverScreen()
    {
        int score = PlayerPrefs.GetInt("FinalScore", 0);
        int wave = PlayerPrefs.GetInt("FinalWave", 1);
        bool won = PlayerPrefs.GetInt("GameWon", 0) == 1;

        if (gameOverTitleText != null)
            gameOverTitleText.text = won ? "YOU WIN!" : "GAME OVER";

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score.ToString("N0");

        if (finalWaveText != null)
            finalWaveText.text = "Wave Reached: " + wave;

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void OnPlayClicked()
    {
        PlayButtonSound();
        SceneManager.LoadScene("GamePlay");
    }

    public void OnRestartClicked()
    {
        PlayButtonSound();
        SceneManager.LoadScene("GamePlay");
    }

    public void OnMainMenuClicked()
    {
        PlayButtonSound();
        SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitClicked()
    {
        PlayButtonSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayButtonSound()
    {
        if (buttonClickAudio != null)
            buttonClickAudio.Play();
    }
}
