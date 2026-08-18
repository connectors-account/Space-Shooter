using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// Main menu screen. Wires the Play/Quit buttons, shows the persisted high score
    /// and animates the title with a gentle sine bob.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text highScoreText;
        [SerializeField] private RectTransform titleTransform;

        [Header("Title animation")]
        [SerializeField] private float bobAmplitude = 15f;
        [SerializeField] private float bobFrequency = 1.5f;

        private Vector2 _titleBasePos;

        private void Awake()
        {
            if (titleTransform != null) _titleBasePos = titleTransform.anchoredPosition;
        }

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

            int high = ScoreManager.Instance != null
                ? ScoreManager.Instance.GetHighScore()
                : PlayerPrefs.GetInt(ScoreManager.HighScoreKey, 0);
            if (highScoreText != null) highScoreText.text = $"HIGH SCORE: {high}";

            if (AudioManager.Instance != null && AudioManager.Instance.menuMusic != null)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
            }

            if (GameManager.Instance != null) GameManager.Instance.EnterMainMenu();
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlay);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuit);
        }

        private void Update()
        {
            if (titleTransform == null) return;
            float y = Mathf.Sin(Time.unscaledTime * bobFrequency) * bobAmplitude;
            titleTransform.anchoredPosition = _titleBasePos + new Vector2(0f, y);
        }

        private void OnPlay()
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadGame();
            if (GameManager.Instance != null) GameManager.Instance.NewGame();
        }

        private void OnQuit()
        {
            // Application.Quit() is a no-op in the editor; it takes effect in a build.
            Application.Quit();
        }
    }
}
