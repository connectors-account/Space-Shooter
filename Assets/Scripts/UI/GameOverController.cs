using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;
using SpaceShooter.Systems;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Game-over screen: animated score count-up, high score, wave reached, new-high-score flash.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject gameOverPanel;

        [Header("Texts")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text waveReachedText;
        [SerializeField] private GameObject newHighScoreBadge;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Animation")]
        [SerializeField] private float countUpDuration = 1.2f;

        private void Start()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (newHighScoreBadge != null) newHighScoreBadge.SetActive(false);

            if (retryButton != null) retryButton.onClick.AddListener(Retry);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);

            if (GameManager.Instance != null) GameManager.Instance.OnGameOver += Show;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnGameOver -= Show;
        }

        private void Show()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
            int highScore = ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0;
            int wave = GameManager.Instance != null ? GameManager.Instance.WaveNumber : 0;

            if (ScoreManager.Instance != null) ScoreManager.Instance.SaveHighScore();

            if (highScoreText != null) highScoreText.text = $"HIGH SCORE: {ScoreManager.FormatScore(Mathf.Max(highScore, finalScore))}";
            if (waveReachedText != null) waveReachedText.text = $"WAVE REACHED: {wave}";

            bool isNewHigh = finalScore >= highScore && finalScore > 0;
            if (newHighScoreBadge != null) newHighScoreBadge.SetActive(isNewHigh);
            if (isNewHigh) StartCoroutine(FlashBadge());

            StartCoroutine(CountUp(finalScore));
        }

        private IEnumerator CountUp(int target)
        {
            if (finalScoreText == null) yield break;
            float elapsed = 0f;
            while (elapsed < countUpDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                int value = Mathf.RoundToInt(Mathf.Lerp(0, target, elapsed / countUpDuration));
                finalScoreText.text = $"SCORE: {ScoreManager.FormatScore(value)}";
                yield return null;
            }
            finalScoreText.text = $"SCORE: {ScoreManager.FormatScore(target)}";
        }

        private IEnumerator FlashBadge()
        {
            if (newHighScoreBadge == null) yield break;
            Transform t = newHighScoreBadge.transform;
            while (newHighScoreBadge.activeSelf)
            {
                float scale = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.15f;
                t.localScale = Vector3.one * scale;
                yield return null;
            }
        }

        private void Retry()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("MenuClick");
            if (GameManager.Instance != null) GameManager.Instance.RestartGame();
        }

        private void GoToMainMenu()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("MenuClick");
            if (GameManager.Instance != null) GameManager.Instance.LoadMainMenu();
        }
    }
}
