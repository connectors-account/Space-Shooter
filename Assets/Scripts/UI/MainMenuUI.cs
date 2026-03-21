using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu screen controller.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text titleText;
    public Text highScoreText;
    public Button startButton;
    public Button quitButton;

    [Header("Optional")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Start()
    {
        // Play menu music
        AudioManager.Instance?.PlayMenuMusic();

        // Set up title
        if (titleText != null)
            titleText.text = "SPACE SHOOTER";

        // Display high score
        if (highScoreText != null)
        {
            int hs = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = $"HIGH SCORE: {hs:N0}";
        }

        // Wire buttons
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Volume sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance != null ? AudioManager.Instance.musicVolume : 0.5f;
            musicVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance != null ? AudioManager.Instance.sfxVolume : 0.8f;
            sfxVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetSFXVolume(v));
        }
    }

    private void OnStartClicked()
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        GameManager.Instance?.StartGame();
    }

    private void OnQuitClicked()
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        GameManager.Instance?.QuitGame();
    }
}
