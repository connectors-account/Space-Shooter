using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Scoring;
using SpaceShooter.Utilities;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Game-over overlay. Shows the final score and high score, a "NEW HIGH
    /// SCORE!" banner when beaten, and Restart / Main Menu buttons. Builds its
    /// UI at runtime if not wired. Commits the score to the high-score table.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private GameObject newHighScoreBanner;

        private void Awake()
        {
            BuildIfNeeded();
            Hide();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState previous, GameState next)
        {
            if (next == GameState.GameOver) ShowGameOver();
            else Hide();
        }

        private void ShowGameOver()
        {
            bool newRecord = false;
            int finalScore = 0;
            int highScore = 0;

            if (ScoreManager.Instance != null)
            {
                finalScore = ScoreManager.Instance.Score;
                newRecord = ScoreManager.Instance.CommitHighScore();
                highScore = ScoreManager.Instance.HighScore;
            }
            else
            {
                highScore = HighScoreTable.Best();
            }

            if (finalScoreText != null) finalScoreText.text = "SCORE  " + finalScore.ToString("N0");
            if (highScoreText != null) highScoreText.text = "BEST  " + highScore.ToString("N0");
            if (newHighScoreBanner != null) newHighScoreBanner.SetActive(newRecord);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.PlaySFX(Constants.SfxExplosion);
            }

            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // -----------------------------------------------------------------
        // Button actions
        // -----------------------------------------------------------------
        public void OnRestartClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(Constants.SfxUiClick);
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetForNewGame();
            if (GameManager.Instance != null) GameManager.Instance.RestartGame();
        }

        public void OnMainMenuClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(Constants.SfxUiClick);
            if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
        }

        // -----------------------------------------------------------------
        // Runtime UI
        // -----------------------------------------------------------------
        private void BuildIfNeeded()
        {
            if (panelRoot != null) return;

            var canvas = UIFactory.CreateCanvas("GameOverCanvas", transform, 210);
            var dim = UIFactory.CreatePanel(canvas.transform, new Color(0.05f, 0f, 0f, 0.85f));
            panelRoot = dim.gameObject;

            UIFactory.CreateText(panelRoot.transform, "GAME OVER", 72, new Vector2(0f, 380f), new Vector2(800f, 110f))
                .color = new Color(1f, 0.4f, 0.4f);

            var banner = UIFactory.CreateText(panelRoot.transform, "NEW HIGH SCORE!", 40, new Vector2(0f, 280f), new Vector2(700f, 60f));
            banner.color = new Color(1f, 0.9f, 0.3f);
            newHighScoreBanner = banner.gameObject;
            newHighScoreBanner.SetActive(false);

            finalScoreText = UIFactory.CreateText(panelRoot.transform, "SCORE  0", 44, new Vector2(0f, 160f), new Vector2(700f, 60f));
            highScoreText = UIFactory.CreateText(panelRoot.transform, "BEST  0", 34, new Vector2(0f, 100f), new Vector2(700f, 50f));
            highScoreText.color = new Color(1f, 0.9f, 0.4f);

            // Top-5 high-score list.
            var list = UIFactory.CreateText(panelRoot.transform, BuildScoreList(), 26, new Vector2(0f, -60f), new Vector2(500f, 220f));
            list.alignment = TextAnchor.UpperCenter;
            var display = list.gameObject.AddComponent<HighScoreDisplay>();
            display.AttachText(list);

            UIFactory.CreateButton(panelRoot.transform, "RESTART", new Vector2(-180f, -300f), new Vector2(300f, 84f), OnRestartClicked);
            UIFactory.CreateButton(panelRoot.transform, "MAIN MENU", new Vector2(180f, -300f), new Vector2(300f, 84f), OnMainMenuClicked);
        }

        private string BuildScoreList()
        {
            var scores = HighScoreTable.Load();
            if (scores.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("TOP SCORES");
            for (int i = 0; i < scores.Count; i++)
                sb.AppendLine($"{i + 1}.  {scores[i]:N0}");
            return sb.ToString();
        }
    }
}
