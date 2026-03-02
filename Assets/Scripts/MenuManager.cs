using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("UI Elements")]
    public TextMeshProUGUI highScoreText;
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    [Header("Audio")]
    public AudioSource menuMusic;
    public AudioClip buttonClickSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Show main menu panel
        ShowMainMenu();

        // Load and display high score
        UpdateHighScoreDisplay();

        // Load settings
        LoadSettings();

        // Play menu music
        if (menuMusic != null && !menuMusic.isPlaying)
        {
            menuMusic.Play();
        }
    }

    void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = $"High Score: {highScore:N0}";
        }
    }

    void LoadSettings()
    {
        // Load volume
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }
        AudioListener.volume = volume;

        // Load fullscreen
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
        }
        Screen.fullScreen = isFullscreen;
    }

    void SaveSettings()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        }

        if (fullscreenToggle != null)
        {
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        PlayButtonSound();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        PlayButtonSound();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void OnPlayButtonClicked()
    {
        PlayButtonSound();
        SceneManager.LoadScene("GameScene");
    }

    public void OnOptionsButtonClicked()
    {
        ShowOptions();
    }

    public void OnCreditsButtonClicked()
    {
        ShowCredits();
    }

    public void OnBackButtonClicked()
    {
        PlayButtonSound();
        ShowMainMenu();
    }

    public void OnQuitButtonClicked()
    {
        PlayButtonSound();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
        SaveSettings();
    }

    public void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        SaveSettings();
    }

    public void OnClearHighScoreClicked()
    {
        PlayButtonSound();
        PlayerPrefs.DeleteKey("HighScore");
        UpdateHighScoreDisplay();
    }

    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}
