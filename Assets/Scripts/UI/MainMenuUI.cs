using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Main menu screen. Shows the title, high score and Play/Quit buttons.
    /// The title pulses using a PingPong scale tween.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Root")]
        [SerializeField] private GameObject _root;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private float _pulseSpeed = 1.5f;
        [SerializeField] private float _pulseAmount = 0.08f;

        [Header("Score")]
        [SerializeField] private TextMeshProUGUI _highScoreText;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;
        #endregion

        #region Private
        private Vector3 _titleBaseScale = Vector3.one;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_titleText != null) _titleBaseScale = _titleText.transform.localScale;
            if (_titleText != null) _titleText.text = "VOID ASSAULT";
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChanged;
            if (_playButton != null) _playButton.onClick.AddListener(OnPlayClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);
            RefreshHighScore();
            SetVisible(GameManager.Instance == null || GameManager.Instance.State == GameManager.GameState.MainMenu);
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
            if (_playButton != null) _playButton.onClick.RemoveListener(OnPlayClicked);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void Update()
        {
            if (_titleText == null) return;
            float scale = 1f + Mathf.PingPong(Time.unscaledTime * _pulseSpeed, _pulseAmount * 2f) - _pulseAmount;
            _titleText.transform.localScale = _titleBaseScale * scale;
        }
        #endregion

        #region UI
        private void RefreshHighScore()
        {
            int hs = ScoreManager.Instance != null
                ? ScoreManager.Instance.GetHighScore()
                : PlayerPrefs.GetInt(GameConstants.PREF_HIGH_SCORE, 0);
            if (_highScoreText != null) _highScoreText.text = $"HIGH SCORE: {hs}";
        }

        private void HandleStateChanged(GameManager.GameState state)
        {
            SetVisible(state == GameManager.GameState.MainMenu);
            if (state == GameManager.GameState.MainMenu) RefreshHighScore();
        }

        private void SetVisible(bool visible)
        {
            if (_root != null) _root.SetActive(visible);
            else gameObject.SetActive(visible);
        }
        #endregion

        #region Button Handlers
        private void OnPlayClicked()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick);
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
        }

        private void OnQuitClicked()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}
