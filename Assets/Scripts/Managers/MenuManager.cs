using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the main menu scene: start game, options, quit.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("UI Elements")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text versionText;

    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button optionsBackButton;

    private void Start()
    {
        Time.timeScale = 1f;

        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        if (optionsButton != null)
            optionsButton.onClick.AddListener(() => ShowOptions(true));
        if (optionsBackButton != null)
            optionsBackButton.onClick.AddListener(() => ShowOptions(false));

        // Volume sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.5f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 0.7f;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Display high score
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }

        // Version
        if (versionText != null)
            versionText.text = "v1.0";

        // Hide options
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // Play menu music
        AudioManager.Instance?.PlayMenuMusic();
    }

    private void OnStartClicked()
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        AudioManager.Instance?.PlayGameMusic();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }

    private void OnQuitClicked()
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            Application.Quit();
        }
    }

    private void ShowOptions(bool show)
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        if (optionsPanel != null)
            optionsPanel.SetActive(show);
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
    }
}
