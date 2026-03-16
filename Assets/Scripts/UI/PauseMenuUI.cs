using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PauseMenuUI handles the pause menu interactions.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI pausedText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void OnEnable()
    {
        // Focus on resume button
        if (resumeButton != null)
        {
            resumeButton.Select();
        }
    }

    private void Start()
    {
        // Setup button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        // Setup volume sliders
        SetupVolumeSliders();

        // Hide settings panel initially
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Setup volume sliders with current values
    /// </summary>
    private void SetupVolumeSliders()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    /// <summary>
    /// Called when Resume button is clicked
    /// </summary>
    private void OnResumeClicked()
    {
        PlayButtonSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    /// <summary>
    /// Called when Restart button is clicked
    /// </summary>
    private void OnRestartClicked()
    {
        PlayButtonSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Called when Main Menu button is clicked
    /// </summary>
    private void OnMainMenuClicked()
    {
        PlayButtonSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Called when Quit button is clicked
    /// </summary>
    private void OnQuitClicked()
    {
        PlayButtonSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    /// <summary>
    /// Called when music volume slider changes
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicVolume(value);
        }
    }

    /// <summary>
    /// Called when SFX volume slider changes
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }
    }

    /// <summary>
    /// Toggle settings panel visibility
    /// </summary>
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    /// <summary>
    /// Play button click sound
    /// </summary>
    private void PlayButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }
    }
}
