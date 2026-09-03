using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Game over screen. Shows final and high scores, a "NEW HIGH SCORE!" label if
    /// beaten, and Retry / Menu buttons. Slides in from below over 0.5 s.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Root")]
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _panel;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private GameObject _newHighScoreLabel;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _menuButton;

        [Header("Slide")]
        [SerializeField] private float _slideDuration = 0.5f;
        [SerializeField] private float _slideFromY = -800f;
        #endregion

        #region Private
        private Vector2 _shownPos;
        private Coroutine _slideRoutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_panel != null) _shownPos = _panel.anchoredPosition;
        }

        private void OnEnable()
        {
            GameManager.OnGameOver += HandleGameOver;
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetry);
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenu);
            SetVisible(false);
        }

        private void OnDisable()
        {
            GameManager.OnGameOver -= HandleGameOver;
            if (_retryButton != null) _retryButton.onClick.RemoveListener(OnRetry);
            if (_menuButton != null) _menuButton.onClick.RemoveListener(OnMenu);
        }
        #endregion

        #region Game Over
        private void HandleGameOver()
        {
            SetVisible(true);

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
            int high = ScoreManager.Instance != null ? ScoreManager.Instance.GetHighScore() : 0;
            bool beaten = ScoreManager.Instance != null && ScoreManager.Instance.WasHighScoreBeaten();

            if (_finalScoreText != null) _finalScoreText.text = $"SCORE: {score}";
            if (_highScoreText != null) _highScoreText.text = $"HIGH SCORE: {high}";
            if (_newHighScoreLabel != null) _newHighScoreLabel.SetActive(beaten);

            if (_panel != null)
            {
                if (_slideRoutine != null) StopCoroutine(_slideRoutine);
                _slideRoutine = StartCoroutine(SlideIn());
            }
        }

        private IEnumerator SlideIn()
        {
            Vector2 from = new Vector2(_shownPos.x, _slideFromY);
            float elapsed = 0f;
            _panel.anchoredPosition = from;
            while (elapsed < _slideDuration)
            {
                float t = elapsed / _slideDuration;
                // Ease-out using LerpUnclamped with a smooth curve.
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _panel.anchoredPosition = Vector2.LerpUnclamped(from, _shownPos, eased);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _panel.anchoredPosition = _shownPos;
            _slideRoutine = null;
        }
        #endregion

        #region Visibility
        private void SetVisible(bool visible)
        {
            if (_root != null) _root.SetActive(visible);
            else gameObject.SetActive(visible);
        }
        #endregion

        #region Button Handlers
        private void OnRetry()
        {
            Click();
            SetVisible(false);
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
        }

        private void OnMenu()
        {
            Click();
            SetVisible(false);
            if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
        }

        private void Click()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick);
        }
        #endregion
    }
}
