using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Main menu logic: animated title glow, Play / Quit buttons, and high-score display.
    /// A lightweight scrolling starfield is drawn on a RawImage behind the menu.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        public Text titleText;
        public Text highScoreText;
        public Text versionText;
        public Button playButton;
        public Button quitButton;
        public RawImage starfield;

        [Header("Title glow")]
        public Color glowA = new Color(0.3f, 0.9f, 1f);
        public Color glowB = new Color(0.7f, 0.4f, 1f);
        public float glowSpeed = 2f;

        public string version = "v1.0.0";

        private float _scroll;

        private void Start()
        {
            if (titleText != null) titleText.text = "SPACE SHOOTER";
            if (versionText != null) versionText.text = version;
            if (highScoreText != null)
            {
                int hs = ScoreManager.Instance != null ? ScoreManager.Instance.GetHighScore()
                    : PlayerPrefs.GetInt("SpaceShooter_HighScore", 0);
                highScoreText.text = $"HIGH SCORE: {hs:N0}";
            }

            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

            if (starfield != null && starfield.texture == null)
                starfield.texture = ParallaxBackground.GenerateStarTexture(256, 256, 60, 1.2f);

            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.MainMenu);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("wave_complete");
        }

        private void Update()
        {
            // Title glow pulse.
            if (titleText != null)
            {
                float t = (Mathf.Sin(Time.unscaledTime * glowSpeed) + 1f) * 0.5f;
                titleText.color = Color.Lerp(glowA, glowB, t);
            }

            // Scroll the starfield UVs downward for motion.
            if (starfield != null)
            {
                _scroll += Time.unscaledDeltaTime * 0.05f;
                starfield.uvRect = new Rect(0f, -_scroll, 4f, 4f);
            }
        }

        private void OnPlay()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadGame();
        }

        private void OnQuit()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
