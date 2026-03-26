// ============================================================================
// MainMenuUI.cs — Main menu screen controller
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text versionText;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Options")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button optionsBackButton;

    [Header("Animation")]
    [SerializeField] private float titlePulseSpeed = 2f;
    [SerializeField] private float titlePulseAmount = 0.05f;

    // =========================================================================
    private void Start()
    {
        // Setup buttons
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        if (optionsBackButton != null) optionsBackButton.onClick.AddListener(OnOptionsBackClicked);

        // Volume sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        // Show high score
        int hs = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = $"HIGH SCORE: {hs:N0}";

        if (versionText != null)
            versionText.text = $"v{Application.version}";

        // Default panel state
        if (mainPanel != null) mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    private void Update()
    {
        // Pulsing title
        if (titleText != null)
        {
            float scale = 1f + Mathf.Sin(Time.time * titlePulseSpeed) * titlePulseAmount;
            titleText.transform.localScale = Vector3.one * scale;
        }

        // Press Enter to start
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnStartClicked();
        }
    }

    // =========================================================================
    // Button Handlers
    // =========================================================================
    private void OnStartClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnOptionsClicked()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    private void OnOptionsBackClicked()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    private void OnQuitClicked()
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

    // =========================================================================
    // Volume
    // =========================================================================
    private void OnMasterVolumeChanged(float v)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetMasterVolume(v);
    }

    private void OnSFXVolumeChanged(float v)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetSFXVolume(v);
    }

    private void OnMusicVolumeChanged(float v)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetMusicVolume(v);
    }
}
