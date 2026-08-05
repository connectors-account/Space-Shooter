using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Scoring;
using SpaceShooter.Utilities;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Main menu controller. Provides Play and Quit buttons, shows the current
    /// high score, animates the title with a gentle scale pulse and lets the
    /// parallax background scroll. Builds its UI at runtime if not wired.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text highScoreText;
        [SerializeField] private RectTransform titleTransform;

        [Header("Title pulse")]
        [SerializeField] private float pulseScale = 0.06f;
        [SerializeField] private float pulseSpeed = 2f;

        private Vector3 _titleBaseScale = Vector3.one;

        private void Awake()
        {
            EnsureManagers();
            BuildIfNeeded();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.EnterMainMenuState();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(Constants.MusicMenu);

            RefreshHighScore();
            if (titleTransform != null) _titleBaseScale = titleTransform.localScale;
        }

        private void Update()
        {
            if (titleTransform != null)
            {
                float s = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale;
                titleTransform.localScale = _titleBaseScale * s;
            }
        }

        private void EnsureManagers()
        {
            // Guarantee core singletons exist even if the menu scene is opened directly.
            if (GameManager.Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>();
            if (SceneLoader.Instance == null)
                new GameObject("SceneLoader").AddComponent<SceneLoader>();
            if (AudioManager.Instance == null)
                new GameObject("AudioManager").AddComponent<AudioManager>();
            if (ScoreManager.Instance == null)
                new GameObject("ScoreManager").AddComponent<ScoreManager>();
        }

        private void RefreshHighScore()
        {
            int best = HighScoreTable.Best();
            if (best <= 0 && ScoreManager.Instance != null) best = ScoreManager.Instance.HighScore;
            if (highScoreText != null)
                highScoreText.text = "HIGH SCORE  " + best.ToString("N0");
        }

        // -----------------------------------------------------------------
        // Button actions
        // -----------------------------------------------------------------
        public void OnPlayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(Constants.SfxUiClick);
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetForNewGame();
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
        }

        public void OnQuitClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(Constants.SfxUiClick);
            if (GameManager.Instance != null) GameManager.Instance.QuitGame();
            else Application.Quit();
        }

        // -----------------------------------------------------------------
        // Runtime UI
        // -----------------------------------------------------------------
        private void BuildIfNeeded()
        {
            if (playButton != null) return;

            var canvas = UIFactory.CreateCanvas("MainMenuCanvas", transform, 50);

            var title = UIFactory.CreateText(canvas.transform, "SPACE SHOOTER", 72,
                new Vector2(0f, 480f), new Vector2(900f, 120f));
            title.color = new Color(0.5f, 0.85f, 1f);
            titleTransform = title.rectTransform;

            UIFactory.CreateText(canvas.transform, "Arrow keys / WASD to move  •  Space to fire  •  B for bomb",
                22, new Vector2(0f, 360f), new Vector2(900f, 40f));

            highScoreText = UIFactory.CreateText(canvas.transform, "HIGH SCORE  0", 34,
                new Vector2(0f, 260f), new Vector2(700f, 50f));
            highScoreText.color = new Color(1f, 0.9f, 0.4f);

            playButton = UIFactory.CreateButton(canvas.transform, "PLAY",
                new Vector2(0f, 60f), new Vector2(320f, 90f), OnPlayClicked);
            quitButton = UIFactory.CreateButton(canvas.transform, "QUIT",
                new Vector2(0f, -60f), new Vector2(320f, 90f), OnQuitClicked);
        }
    }
}
