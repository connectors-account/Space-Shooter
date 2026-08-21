using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Pause menu. Toggled with Escape. Handles resume/restart/main-menu and options.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button optionsBackButton;

        [Header("Options")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;

        private bool isPaused;

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);

            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
            if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
            if (optionsBackButton != null) optionsBackButton.onClick.AddListener(CloseOptions);

            SetupSliders();
        }

        private void SetupSliders()
        {
            if (AudioManager.Instance == null) return;
            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.GetSFXVolume();
                sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            }
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.GetMusicVolume();
                musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            }
        }

        private void Update()
        {
            bool pausePressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel");
            if (!pausePressed) return;

            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                Pause();
            }
            else if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                Resume();
            }
        }

        private void Pause()
        {
            isPaused = true;
            if (GameManager.Instance != null) GameManager.Instance.PauseGame();
            if (pausePanel != null) pausePanel.SetActive(true);
            Click();
        }

        public void Resume()
        {
            isPaused = false;
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            Click();
        }

        private void Restart()
        {
            Click();
            if (GameManager.Instance != null) GameManager.Instance.RestartGame();
        }

        private void GoToMainMenu()
        {
            Click();
            if (GameManager.Instance != null) GameManager.Instance.LoadMainMenu();
        }

        private void OpenOptions()
        {
            if (optionsPanel != null) optionsPanel.SetActive(true);
            Click();
        }

        private void CloseOptions()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            Click();
        }

        private void Click()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("MenuClick");
        }
    }
}
