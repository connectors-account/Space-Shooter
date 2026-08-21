using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Main menu controller: Play button, high score display, credits toggle,
    /// an animated pulsing title, and drifting background stars.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button creditsCloseButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private GameObject creditsPanel;

        [Header("Display")]
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text titleText;

        [Header("Title Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 0.08f;

        [Header("Background Stars")]
        [SerializeField] private RectTransform starsContainer;
        [SerializeField] private float starScrollSpeed = 30f;

        private Vector3 _titleBaseScale = Vector3.one;

        private void Awake()
        {
            if (titleText != null)
            {
                _titleBaseScale = titleText.transform.localScale;
            }
        }

        private void Start()
        {
            Time.timeScale = 1f;

            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (creditsButton != null) creditsButton.onClick.AddListener(() => ToggleCredits(true));
            if (creditsCloseButton != null) creditsCloseButton.onClick.AddListener(() => ToggleCredits(false));
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

            if (creditsPanel != null) creditsPanel.SetActive(false);

            if (highScoreText != null)
            {
                int hi = PlayerPrefs.GetInt("HighScore", 0);
                highScoreText.text = $"HIGH SCORE: {hi}";
            }

            AudioManager.Instance?.PlayMusic("menu_music");
        }

        private void Update()
        {
            AnimateTitle();
            ScrollStars();
        }

        private void AnimateTitle()
        {
            if (titleText == null) return;
            float s = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale;
            titleText.transform.localScale = _titleBaseScale * s;

            // Subtle glow via alpha pulse.
            Color c = titleText.color;
            c.a = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
            titleText.color = c;
        }

        private void ScrollStars()
        {
            if (starsContainer == null) return;
            Vector2 pos = starsContainer.anchoredPosition;
            pos.y -= starScrollSpeed * Time.unscaledDeltaTime;
            if (pos.y <= -starsContainer.rect.height)
            {
                pos.y = 0f;
            }
            starsContainer.anchoredPosition = pos;
        }

        private void OnPlay()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadGameScene();
            }
        }

        private void ToggleCredits(bool show)
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(show);
            }
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
