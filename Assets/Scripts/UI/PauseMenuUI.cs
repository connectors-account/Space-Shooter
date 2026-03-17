using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Pause menu controller
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Header("Settings")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Toggle musicToggle;
        
        private bool isPaused = false;
        
        private void Start()
        {
            SetupButtons();
            SetupSettings();
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            
            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        
        private void Update()
        {
            // ESC key handling is in GameManager, but we can also handle it here
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    OnResumeClicked();
                }
                else
                {
                    OnPauseClicked();
                }
            }
        }
        
        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
                
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        private void SetupSettings()
        {
            if (volumeSlider != null)
            {
                volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
            
            if (sfxToggle != null)
            {
                sfxToggle.isOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
                sfxToggle.onValueChanged.AddListener(OnSFXToggled);
            }
            
            if (musicToggle != null)
            {
                musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
                musicToggle.onValueChanged.AddListener(OnMusicToggled);
            }
        }
        
        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Paused)
            {
                ShowPauseMenu();
            }
            else if (state == GameState.Playing)
            {
                HidePauseMenu();
            }
        }
        
        public void OnPauseClicked()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
            }
        }
        
        public void OnResumeClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }
        
        public void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
        
        public void OnMainMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
            }
        }
        
        public void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
        
        private void ShowPauseMenu()
        {
            isPaused = true;
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }
        
        private void HidePauseMenu()
        {
            isPaused = false;
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }
        
        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
            PlayerPrefs.Save();
        }
        
        private void OnSFXToggled(bool isOn)
        {
            PlayerPrefs.SetInt("SFXEnabled", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private void OnMusicToggled(bool isOn)
        {
            PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
            PlayerPrefs.Save();
            
            // Find and toggle music source
            AudioSource[] sources = FindObjectsOfType<AudioSource>();
            foreach (AudioSource source in sources)
            {
                if (source.CompareTag("Music"))
                {
                    source.mute = !isOn;
                }
            }
        }
    }
}
