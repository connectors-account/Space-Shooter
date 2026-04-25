using SpaceShooter.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class GameUIManager : MonoBehaviour
    {
        private Text _scoreText;
        private Text _healthText;
        private Text _waveText;

        private GameObject _pausePanel;
        private GameObject _gameOverPanel;
        private Text _gameOverScoreText;

        private void Start()
        {
            BuildUI();
            BindEvents();
        }

        private void BindEvents()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            gameManager.OnScoreChanged += UpdateScore;
            gameManager.OnHealthChanged += UpdateHealth;
            gameManager.OnWaveChanged += UpdateWave;
            gameManager.OnStateChanged += HandleStateChanged;

            UpdateScore(gameManager.Score);
            UpdateHealth(gameManager.PlayerHealth, gameManager.MaxHealth);
            UpdateWave(gameManager.Wave);
            HandleStateChanged(gameManager.State);
        }

        private void BuildUI()
        {
            var canvas = UICreation.CreateCanvas("GameplayCanvas");

            _scoreText = UICreation.CreateLabel(canvas.transform, "Score: 0", new Vector2(-380, 340), 28, TextAnchor.UpperLeft, Color.white);
            _healthText = UICreation.CreateLabel(canvas.transform, "Health: 5/5", new Vector2(-380, 300), 28, TextAnchor.UpperLeft, new Color(0.3f, 1f, 0.3f));
            _waveText = UICreation.CreateLabel(canvas.transform, "Wave: 1", new Vector2(380, 340), 28, TextAnchor.UpperRight, Color.white);

            var hint = UICreation.CreateLabel(canvas.transform, "Esc = Pause", new Vector2(380, 300), 22, TextAnchor.UpperRight, Color.gray);
            hint.rectTransform.sizeDelta = new Vector2(300, 80);

            _pausePanel = CreateOverlayPanel(canvas.transform, "PAUSED", new Color(0f, 0f, 0f, 0.7f));
            UICreation.CreateButton(_pausePanel.transform, "Resume", new Vector2(0, 20), new Vector2(220, 60), () => GameManager.Instance?.TogglePause());
            UICreation.CreateButton(_pausePanel.transform, "Main Menu", new Vector2(0, -70), new Vector2(220, 60), () => GameManager.Instance?.GoToMainMenu());

            _gameOverPanel = CreateOverlayPanel(canvas.transform, "GAME OVER", new Color(0f, 0f, 0f, 0.8f));
            _gameOverScoreText = UICreation.CreateLabel(_gameOverPanel.transform, "Final Score: 0", new Vector2(0, 20), 32, TextAnchor.MiddleCenter, Color.white);
            UICreation.CreateButton(_gameOverPanel.transform, "Restart", new Vector2(0, -60), new Vector2(220, 60), () => GameManager.Instance?.RestartGame());
            UICreation.CreateButton(_gameOverPanel.transform, "Main Menu", new Vector2(0, -140), new Vector2(220, 60), () => GameManager.Instance?.GoToMainMenu());
        }

        private static GameObject CreateOverlayPanel(Transform canvas, string title, Color bgColor)
        {
            var panel = new GameObject(title + "Panel");
            panel.transform.SetParent(canvas, false);

            var image = panel.AddComponent<Image>();
            image.color = bgColor;

            var rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 700);
            rect.anchoredPosition = Vector2.zero;

            UICreation.CreateLabel(panel.transform, title, new Vector2(0, 180), 54, TextAnchor.MiddleCenter, Color.white);
            panel.SetActive(false);
            return panel;
        }

        private void UpdateScore(int score)
        {
            _scoreText.text = $"Score: {score}";
        }

        private void UpdateHealth(int health, int maxHealth)
        {
            _healthText.text = $"Health: {health}/{maxHealth}";
            _healthText.color = health <= 2 ? new Color(1f, 0.35f, 0.35f) : new Color(0.3f, 1f, 0.3f);
        }

        private void UpdateWave(int wave)
        {
            _waveText.text = $"Wave: {wave}";
        }

        private void HandleStateChanged(GameState state)
        {
            _pausePanel.SetActive(state == GameState.Paused);
            _gameOverPanel.SetActive(state == GameState.GameOver);

            if (state == GameState.GameOver)
            {
                _gameOverScoreText.text = $"Final Score: {GameManager.Instance.Score}";
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnHealthChanged -= UpdateHealth;
            GameManager.Instance.OnWaveChanged -= UpdateWave;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }
}
