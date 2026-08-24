using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Game-over screen. Works both as an in-scene panel (Show called by UIManager) and as
    /// a dedicated GameOver scene controller (reads values from GameManager on Start).
    /// Shows final score with an animated count-up and a "NEW HIGH SCORE!" flag.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [Tooltip("If true this controller is the root of a dedicated GameOver scene and auto-shows on Start.")]
        [SerializeField] private bool standaloneScene = false;

        [Header("Texts")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private GameObject newHighScoreLabel;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Count-Up")]
        [SerializeField] private float countUpDuration = 1.25f;

        private Coroutine _countRoutine;

        private void Awake()
        {
            if (!standaloneScene && panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void OnEnable()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnGameOver += HandleGameOver;
            }
        }

        private void OnDisable()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnGameOver -= HandleGameOver;
            }
        }

        private void Start()
        {
            if (standaloneScene && GameManager.HasInstance)
            {
                Show(GameManager.Instance.Score, GameManager.Instance.HighScore);
            }
        }

        private void OnDestroy()
        {
            if (retryButton != null) retryButton.onClick.RemoveListener(OnRetry);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenu);
        }

        private void HandleGameOver()
        {
            if (GameManager.HasInstance)
            {
                Show(GameManager.Instance.Score, GameManager.Instance.HighScore);
            }
        }

        public void Show(int score, int highScore)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            bool isNewHigh = score >= highScore && score > 0;
            if (newHighScoreLabel != null)
            {
                newHighScoreLabel.SetActive(isNewHigh);
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"HIGH SCORE: {highScore:N0}";
            }

            if (_countRoutine != null)
            {
                StopCoroutine(_countRoutine);
            }
            _countRoutine = StartCoroutine(CountUpRoutine(score));
        }

        private IEnumerator CountUpRoutine(int targetScore)
        {
            if (finalScoreText == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < countUpDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                int displayed = Mathf.RoundToInt(Mathf.Lerp(0f, targetScore, elapsed / countUpDuration));
                finalScoreText.text = $"SCORE: {displayed:N0}";
                yield return null;
            }

            finalScoreText.text = $"SCORE: {targetScore:N0}";
            _countRoutine = null;
        }

        private void OnRetry()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.RestartGame();
            }
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadGameScene();
            }
        }

        private void OnMainMenu()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.SetMenuState();
            }
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadMainMenu();
            }
        }
    }
}
