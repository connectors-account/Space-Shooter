using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Main menu controller. Handles Play / Options / Quit, the options panel (volume sliders
    /// and high score display), an animated pulsing title and a background star particle system.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Button optionsBackButton;

        [Header("Options")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Text highScoreText;

        [Header("Animated Title")]
        [SerializeField] private Transform titleTransform;
        [SerializeField] private float titlePulseScale = 0.08f;
        [SerializeField] private float titlePulseSpeed = 2f;

        [Header("Background")]
        [SerializeField] private ParticleSystem starField;

        private Vector3 _titleBaseScale = Vector3.one;

        private void Start()
        {
            // Ensure a clean menu state.
            if (GameManager.HasInstance)
            {
                GameManager.Instance.SetMenuState();
            }

            // Play menu music.
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusicClip);
            }

            // Wire up buttons.
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (optionsButton != null) optionsButton.onClick.AddListener(OnOpenOptions);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            if (optionsBackButton != null) optionsBackButton.onClick.AddListener(OnCloseOptions);

            // Initialize panels.
            if (mainPanel != null) mainPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);

            // Initialize volume sliders from AudioManager.
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 1f;
                musicVolumeSlider.value = AudioManager.HasInstance ? AudioManager.Instance.MusicVolume : PlayerPrefs.GetFloat(Constants.PrefKeys.MusicVolume, 0.6f);
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.value = AudioManager.HasInstance ? AudioManager.Instance.SFXVolume : PlayerPrefs.GetFloat(Constants.PrefKeys.SFXVolume, 0.8f);
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            // High score display.
            if (highScoreText != null)
            {
                int high = GameManager.HasInstance ? GameManager.Instance.HighScore : PlayerPrefs.GetInt(Constants.PrefKeys.HighScore, 0);
                highScoreText.text = $"HIGH SCORE: {high:N0}";
            }

            if (titleTransform != null)
            {
                _titleBaseScale = titleTransform.localScale;
            }

            if (starField != null && !starField.isPlaying)
            {
                starField.Play();
            }
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlay);
            if (optionsButton != null) optionsButton.onClick.RemoveListener(OnOpenOptions);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuit);
            if (optionsBackButton != null) optionsBackButton.onClick.RemoveListener(OnCloseOptions);
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }

        private void Update()
        {
            // Animate the title using a sine wave (no DOTween dependency).
            if (titleTransform != null)
            {
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * titlePulseSpeed) * titlePulseScale;
                titleTransform.localScale = _titleBaseScale * pulse;
            }
        }

        private void OnPlay()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.RestartGame();
            }
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadGameScene();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.Scenes.Game);
            }
        }

        private void OnOpenOptions()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        private void OnCloseOptions()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        private void OnQuit()
        {
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.QuitGame();
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

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
            else
            {
                PlayerPrefs.SetFloat(Constants.PrefKeys.MusicVolume, value);
            }
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
            else
            {
                PlayerPrefs.SetFloat(Constants.PrefKeys.SFXVolume, value);
            }
        }
    }
}
