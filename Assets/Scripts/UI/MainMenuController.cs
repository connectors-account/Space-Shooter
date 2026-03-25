// =============================================================================
// MainMenuController.cs — Main menu screen logic
// =============================================================================
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Controls the main menu: start game, options, quit.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Animated Title")]
        [SerializeField] private float titleBobSpeed = 2f;
        [SerializeField] private float titleBobAmount = 10f;

        private Vector3 titleStartPos;

        private void Start()
        {
            // Initialize buttons
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Show high score
            if (highScoreText != null)
            {
                int hs = PlayerPrefs.GetInt("SpaceShooter_HighScore", 0);
                highScoreText.text = $"HIGH SCORE: {hs:N0}";
            }

            // Volume sliders
            Managers.SoundManager sm = Managers.SoundManager.Instance;
            if (musicSlider != null && sm != null)
            {
                musicSlider.value = sm.musicVolume;
                musicSlider.onValueChanged.AddListener(v => sm.SetMusicVolume(v));
            }
            if (sfxSlider != null && sm != null)
            {
                sfxSlider.value = sm.sfxVolume;
                sfxSlider.onValueChanged.AddListener(v => sm.SetSFXVolume(v));
            }

            // Play menu music
            sm?.PlayMusic("menu");

            // Store title position for animation
            if (titleText != null)
                titleStartPos = titleText.transform.localPosition;
        }

        private void Update()
        {
            // Animate title
            if (titleText != null)
            {
                float offset = Mathf.Sin(Time.time * titleBobSpeed) * titleBobAmount;
                titleText.transform.localPosition = titleStartPos + Vector3.up * offset;
            }
        }

        private void OnStartClicked()
        {
            Managers.SoundManager.Instance?.PlaySFX("menu_select");
            Managers.GameManager.Instance?.StartGame();
        }

        private void OnQuitClicked()
        {
            Managers.GameManager.Instance?.QuitGame();
        }
    }
}
