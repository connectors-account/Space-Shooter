using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Main menu: Play/Options/Quit, options panel with volume sliders and difficulty dropdown,
    /// animated bobbing title, high score display and button hover scaling.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button optionsBackButton;

        [Header("Options Controls")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private TMP_Dropdown difficultyDropdown;

        [Header("Title / Score")]
        [SerializeField] private RectTransform titleTransform;
        [SerializeField] private float titleBobAmplitude = 15f;
        [SerializeField] private float titleBobFrequency = 1.5f;
        [SerializeField] private TMP_Text highScoreText;

        [Header("Hover")]
        [SerializeField] private float hoverScale = 1.1f;

        private Vector2 titleBasePos;

        private const string DifficultyKey = "SpaceShooter_Difficulty";

        private void Start()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);

            if (playButton != null) playButton.onClick.AddListener(Play);
            if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
            if (optionsBackButton != null) optionsBackButton.onClick.AddListener(CloseOptions);

            if (titleTransform != null) titleBasePos = titleTransform.anchoredPosition;

            SetupOptions();
            AddHoverEffects();

            int highScore = PlayerPrefs.GetInt("SpaceShooter_HighScore", 0);
            if (highScoreText != null) highScoreText.text = $"HIGH SCORE: {highScore:N0}";
        }

        private void SetupOptions()
        {
            if (AudioManager.Instance != null)
            {
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

            if (difficultyDropdown != null)
            {
                difficultyDropdown.ClearOptions();
                difficultyDropdown.AddOptions(new System.Collections.Generic.List<string> { "Easy", "Normal", "Hard" });
                difficultyDropdown.value = PlayerPrefs.GetInt(DifficultyKey, 1);
                difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
            }
        }

        private void OnDifficultyChanged(int value)
        {
            PlayerPrefs.SetInt(DifficultyKey, value);
            PlayerPrefs.Save();
        }

        private void AddHoverEffects()
        {
            AddHover(playButton);
            AddHover(optionsButton);
            AddHover(quitButton);
        }

        private void AddHover(Button button)
        {
            if (button == null) return;
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((data) => button.transform.localScale = Vector3.one * hoverScale);
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((data) => button.transform.localScale = Vector3.one);
            trigger.triggers.Add(exit);
        }

        private void Update()
        {
            if (titleTransform != null)
            {
                float y = titleBasePos.y + Mathf.Sin(Time.unscaledTime * titleBobFrequency) * titleBobAmplitude;
                titleTransform.anchoredPosition = new Vector2(titleBasePos.x, y);
            }
        }

        private void Play()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("MenuClick");
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
        }

        private void OpenOptions()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("MenuClick");
            if (optionsPanel != null) optionsPanel.SetActive(true);
            if (mainPanel != null) mainPanel.SetActive(false);
        }

        private void CloseOptions()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("MenuClick");
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        private void Quit()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("MenuClick");
            if (GameManager.Instance != null) GameManager.Instance.QuitGame();
        }
    }
}
