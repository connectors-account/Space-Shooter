using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Managers;

namespace SpaceShooter.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI pauseTitle;

        [Header("Settings")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        private void Start()
        {
            SetupButtons();
            SubscribeToEvents();
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
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

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused += ShowPauseMenu;
                GameManager.Instance.OnGameResumed += HidePauseMenu;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused -= ShowPauseMenu;
                GameManager.Instance.OnGameResumed -= HidePauseMenu;
            }
        }

        private void ShowPauseMenu()
        {
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        private void HidePauseMenu()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnResumeClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.TogglePause();
        }

        private void OnRestartClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.RestartGame();
        }

        private void OnMainMenuClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.LoadMainMenu();
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.QuitGame();
        }

        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            AudioManager.Instance?.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            AudioManager.Instance?.SetSFXVolume(value);
        }
    }
}
