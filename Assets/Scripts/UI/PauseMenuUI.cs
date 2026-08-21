using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Pause menu toggled with Escape. Offers Resume, Restart and Main Menu,
    /// plus music and SFX volume sliders bound to the AudioManager.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject pausePanel;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Volume Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private InputAction _pauseAction;

        private void Awake()
        {
            _pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        }

        private void OnEnable()
        {
            _pauseAction.Enable();
            _pauseAction.performed += OnPausePressed;
        }

        private void OnDisable()
        {
            _pauseAction.performed -= OnPausePressed;
            _pauseAction.Disable();
        }

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);

            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);

            if (AudioManager.Instance != null)
            {
                if (musicSlider != null)
                {
                    musicSlider.value = AudioManager.Instance.MusicVolume;
                    musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
                }
                if (sfxSlider != null)
                {
                    sfxSlider.value = AudioManager.Instance.SfxVolume;
                    sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);
                }
            }
        }

        private void OnPausePressed(InputAction.CallbackContext ctx)
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.State == GameState.Playing)
            {
                Pause();
            }
            else if (GameManager.Instance.State == GameState.Paused)
            {
                Resume();
            }
        }

        private void Pause()
        {
            GameManager.Instance.PauseGame();
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        private void Resume()
        {
            GameManager.Instance.ResumeGame();
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void Restart()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadGameScene();
            }
        }

        private void GoToMainMenu()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadMainMenu();
            }
        }
    }
}
