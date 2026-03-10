using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Managers;

namespace SpaceShooter.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Options")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Button backButton;

        private void Start()
        {
            SetupButtons();
            UpdateHighScore();
            ShowMainPanel();
        }

        private void SetupButtons()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (optionsButton != null)
                optionsButton.onClick.AddListener(OnOptionsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (backButton != null)
                backButton.onClick.AddListener(ShowMainPanel);

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

        private void UpdateHighScore()
        {
            if (highScoreText != null)
            {
                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                highScoreText.text = $"High Score: {highScore}";
            }
        }

        private void OnPlayClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.StartGame();
        }

        private void OnOptionsClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            ShowOptionsPanel();
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlaySound("ButtonClick");
            GameManager.Instance?.QuitGame();
        }

        private void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void ShowOptionsPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
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
