using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles main menu specific functionality.
/// This is a helper script for additional menu features.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    
    [Header("Audio")]
    [SerializeField] private AudioSource menuMusic;
    [SerializeField] private AudioClip buttonClickSound;
    
    private AudioSource audioSource;
    
    /// <summary>
    /// Initialize menu.
    /// </summary>
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Setup button listeners if assigned
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
        
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        // Setup options if assigned
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }
        
        // Hide options panel initially
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        
        // Start menu music if assigned
        if (menuMusic != null && !menuMusic.isPlaying)
        {
            menuMusic.Play();
        }
    }
    
    /// <summary>
    /// Handle play button click.
    /// </summary>
    public void OnPlayClicked()
    {
        PlayButtonSound();
        
        // Stop menu music
        if (menuMusic != null)
        {
            menuMusic.Stop();
        }
        
        // Start the game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }
    
    /// <summary>
    /// Handle options button click.
    /// </summary>
    public void OnOptionsClicked()
    {
        PlayButtonSound();
        
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }
    }
    
    /// <summary>
    /// Handle quit button click.
    /// </summary>
    public void OnQuitClicked()
    {
        PlayButtonSound();
        
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
    
    /// <summary>
    /// Handle volume slider change.
    /// </summary>
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
    
    /// <summary>
    /// Handle fullscreen toggle.
    /// </summary>
    private void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    /// <summary>
    /// Play button click sound.
    /// </summary>
    private void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    /// <summary>
    /// Close options panel.
    /// </summary>
    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Load saved settings.
    /// </summary>
    private void LoadSettings()
    {
        // Load volume
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = savedVolume;
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }
        
        // Load fullscreen
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = savedFullscreen;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = savedFullscreen;
        }
    }
}
