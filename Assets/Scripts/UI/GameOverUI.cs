using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Game Over screen: animated score count-up, high score display with a
    /// "NEW HIGH SCORE!" callout when beaten, and Restart / Main Menu buttons.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject gameOverPanel;

        [Header("Text")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private GameObject newHighScoreBadge;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Count-up")]
        [SerializeField] private float countUpDuration = 1.2f;

        private void Start()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (newHighScoreBadge != null) newHighScoreBadge.SetActive(false);

            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += Show;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver -= Show;
            }
        }

        private void Show()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            int finalScore = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;

            if (highScoreText != null) highScoreText.text = $"HIGH SCORE: {highScore}";

            bool isNewHigh = GameManager.Instance != null && GameManager.Instance.IsNewHighScore();
            if (newHighScoreBadge != null) newHighScoreBadge.SetActive(isNewHigh);

            StartCoroutine(CountUp(finalScore));
        }

        private IEnumerator CountUp(int target)
        {
            if (finalScoreText == null) yield break;

            float t = 0f;
            float duration = Mathf.Max(0.01f, countUpDuration);
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                int value = Mathf.RoundToInt(Mathf.Lerp(0f, target, t / duration));
                finalScoreText.text = $"SCORE: {value}";
                yield return null;
            }
            finalScoreText.text = $"SCORE: {target}";
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadGameScene();
            }
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadMainMenu();
            }
        }
    }
}
