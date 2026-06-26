using System.Collections.Generic;
using SpaceShooter.Core;
using SpaceShooter.Managers;
using SpaceShooter.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Builds and drives the entire in-game user interface at runtime: the HUD (score, health bar,
    /// wave counter, lives, power-up indicators) plus the pause, game-over and victory overlays.
    /// Constructing the UI in code keeps the project free of fragile serialized scene wiring.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private PlayerController _player;

        private Canvas _canvas;
        private Font _font;

        // HUD elements.
        private Text _scoreText;
        private Text _waveText;
        private Text _livesText;
        private Image _healthFill;
        private Text _healthText;
        private Text _powerUpText;
        private Text _waveBanner;
        private float _bannerTimer;

        private readonly Dictionary<PowerUpType, float> _activePowerUps = new Dictionary<PowerUpType, float>();

        // Overlays.
        private GameObject _pausePanel;
        private GameObject _gameOverPanel;
        private GameObject _victoryPanel;
        private Text _gameOverScore;
        private Text _victoryScore;

        /// <summary>
        /// Builds the full UI and subscribes to game/player events. Called by the bootstrap.
        /// </summary>
        public void Initialize(GameManager gameManager, PlayerController player)
        {
            _gameManager = gameManager;
            _player = player;
            _font = GetDefaultFont();

            BuildCanvas();
            BuildHud();
            BuildPausePanel();
            BuildGameOverPanel();
            BuildVictoryPanel();

            // Subscriptions.
            _gameManager.ScoreChanged += OnScoreChanged;
            _gameManager.WaveChanged += OnWaveChanged;
            _gameManager.StateChanged += OnStateChanged;
            _player.HealthChanged += OnHealthChanged;
            _player.LivesChanged += OnLivesChanged;
            _player.PowerUpStateChanged += OnPowerUpStateChanged;

            OnScoreChanged(0);
            OnHealthChanged(_player.MaxHealth, _player.MaxHealth);
            OnLivesChanged(_player.Lives);
        }

        private static Font GetDefaultFont()
        {
            // LegacyRuntime.ttf is the built-in font in modern Unity; fall back to Arial on older versions.
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return font;
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("HUDCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            UIFactory.EnsureEventSystem();
        }

        private void BuildHud()
        {
            _scoreText = UIFactory.CreateText(_canvas.transform, _font, "SCORE: 0", 36, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(500f, 50f));

            _waveText = UIFactory.CreateText(_canvas.transform, _font, "WAVE: 1", 36, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(400f, 50f));

            _livesText = UIFactory.CreateText(_canvas.transform, _font, "LIVES: 3", 36, TextAnchor.UpperRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(400f, 50f));

            // Health bar (background + fill) bottom-left.
            Image healthBg = UIFactory.CreateImage(_canvas.transform, new Color(0.15f, 0.15f, 0.15f, 0.85f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(420f, 36f),
                TextAnchor.LowerLeft);

            _healthFill = UIFactory.CreateImage(healthBg.transform, new Color(0.2f, 0.85f, 0.3f, 1f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(3f, 0f), new Vector2(414f, 30f),
                TextAnchor.MiddleLeft);
            _healthFill.type = Image.Type.Filled;
            _healthFill.fillMethod = Image.FillMethod.Horizontal;
            _healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _healthFill.fillAmount = 1f;
            // Anchor the fill so it grows from the left edge.
            RectTransform fillRt = _healthFill.rectTransform;
            fillRt.anchorMin = new Vector2(0f, 0.5f);
            fillRt.anchorMax = new Vector2(0f, 0.5f);
            fillRt.pivot = new Vector2(0f, 0.5f);

            _healthText = UIFactory.CreateText(healthBg.transform, _font, "100/100", 22, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(414f, 30f));

            _powerUpText = UIFactory.CreateText(_canvas.transform, _font, "", 26, TextAnchor.LowerLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 90f), new Vector2(700f, 120f));
            _powerUpText.color = new Color(1f, 0.9f, 0.4f);

            _waveBanner = UIFactory.CreateText(_canvas.transform, _font, "", 80, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1200f, 160f));
            _waveBanner.color = new Color(1f, 1f, 1f, 0f);
        }

        private void BuildPausePanel()
        {
            _pausePanel = UIFactory.CreatePanel(_canvas.transform, "PausePanel", new Color(0f, 0f, 0f, 0.7f));
            UIFactory.CreateText(_pausePanel.transform, _font, "PAUSED", 72, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(800f, 120f));

            UIFactory.CreateButton(_pausePanel.transform, _font, "RESUME", new Vector2(0f, 60f), OnResumeClicked);
            UIFactory.CreateButton(_pausePanel.transform, _font, "RESTART", new Vector2(0f, -50f), OnRestartClicked);
            UIFactory.CreateButton(_pausePanel.transform, _font, "MAIN MENU", new Vector2(0f, -160f), OnMainMenuClicked);
            _pausePanel.SetActive(false);
        }

        private void BuildGameOverPanel()
        {
            _gameOverPanel = UIFactory.CreatePanel(_canvas.transform, "GameOverPanel", new Color(0.1f, 0f, 0f, 0.8f));
            UIFactory.CreateText(_gameOverPanel.transform, _font, "GAME OVER", 80, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(900f, 140f))
                .color = new Color(1f, 0.4f, 0.4f);

            _gameOverScore = UIFactory.CreateText(_gameOverPanel.transform, _font, "FINAL SCORE: 0", 44, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(900f, 80f));

            UIFactory.CreateButton(_gameOverPanel.transform, _font, "RESTART", new Vector2(0f, -10f), OnRestartClicked);
            UIFactory.CreateButton(_gameOverPanel.transform, _font, "MAIN MENU", new Vector2(0f, -120f), OnMainMenuClicked);
            _gameOverPanel.SetActive(false);
        }

        private void BuildVictoryPanel()
        {
            _victoryPanel = UIFactory.CreatePanel(_canvas.transform, "VictoryPanel", new Color(0f, 0.05f, 0.15f, 0.85f));
            UIFactory.CreateText(_victoryPanel.transform, _font, "VICTORY!", 80, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(900f, 140f))
                .color = new Color(0.5f, 1f, 0.6f);

            _victoryScore = UIFactory.CreateText(_victoryPanel.transform, _font, "FINAL SCORE: 0", 44, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(900f, 80f));

            UIFactory.CreateButton(_victoryPanel.transform, _font, "PLAY AGAIN", new Vector2(0f, -10f), OnRestartClicked);
            UIFactory.CreateButton(_victoryPanel.transform, _font, "MAIN MENU", new Vector2(0f, -120f), OnMainMenuClicked);
            _victoryPanel.SetActive(false);
        }

        private void Update()
        {
            // Pause toggle.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_gameManager.State == GameState.Playing || _gameManager.State == GameState.Paused)
                {
                    AudioManager.Instance?.PlayUiClick();
                    _gameManager.TogglePause();
                }
            }

            UpdateWaveBanner();
            UpdatePowerUpDisplay();
        }

        private void UpdateWaveBanner()
        {
            if (_bannerTimer <= 0f)
            {
                return;
            }

            // Banner shows for 2s total: fade in over the first 0.5s, hold, fade out over the last 0.5s.
            _bannerTimer -= Time.unscaledDeltaTime;
            float elapsed = 2f - _bannerTimer;
            float alpha;
            if (elapsed < 0.5f)
            {
                alpha = elapsed / 0.5f;          // fade in
            }
            else if (_bannerTimer < 0.5f)
            {
                alpha = Mathf.Max(0f, _bannerTimer / 0.5f); // fade out
            }
            else
            {
                alpha = 1f;                        // hold
            }

            Color c = _waveBanner.color;
            c.a = Mathf.Clamp01(alpha);
            _waveBanner.color = c;
        }

        private void UpdatePowerUpDisplay()
        {
            if (_activePowerUps.Count == 0)
            {
                if (_powerUpText.text.Length > 0)
                {
                    _powerUpText.text = string.Empty;
                }
                return;
            }

            var sb = new System.Text.StringBuilder();
            var expired = new List<PowerUpType>();
            foreach (var kvp in _activePowerUps)
            {
                if (kvp.Value <= 0f)
                {
                    expired.Add(kvp.Key);
                    continue;
                }
                sb.AppendLine($"{Label(kvp.Key)}: {kvp.Value:0.0}s");
            }
            foreach (var key in expired)
            {
                _activePowerUps.Remove(key);
            }
            _powerUpText.text = sb.ToString();
        }

        private static string Label(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield: return "SHIELD";
                case PowerUpType.RapidFire: return "RAPID FIRE";
                case PowerUpType.SpreadShot: return "SPREAD SHOT";
                case PowerUpType.ScoreMultiplier: return "x2 SCORE";
                default: return type.ToString().ToUpperInvariant();
            }
        }

        private void OnScoreChanged(int score)
        {
            _scoreText.text = $"SCORE: {score}";
        }

        private void OnWaveChanged(int wave, bool isBoss)
        {
            _waveText.text = $"WAVE: {wave}";
            _waveBanner.text = isBoss ? $"WAVE {wave}\nBOSS!" : $"WAVE {wave}";
            _bannerTimer = 2f;
        }

        private void OnStateChanged(GameState state)
        {
            _pausePanel.SetActive(state == GameState.Paused);
            _gameOverPanel.SetActive(state == GameState.GameOver);
            _victoryPanel.SetActive(state == GameState.Victory);

            if (state == GameState.GameOver)
            {
                _gameOverScore.text = $"FINAL SCORE: {_gameManager.Score}";
            }
            else if (state == GameState.Victory)
            {
                _victoryScore.text = $"FINAL SCORE: {_gameManager.Score}";
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            _healthFill.fillAmount = max > 0 ? (float)current / max : 0f;
            _healthFill.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.85f, 0.3f), _healthFill.fillAmount);
            _healthText.text = $"{current}/{max}";
        }

        private void OnLivesChanged(int lives)
        {
            _livesText.text = $"LIVES: {lives}";
        }

        private void OnPowerUpStateChanged(PowerUpType type, float remaining)
        {
            if (type == PowerUpType.Health)
            {
                return; // instant effect, nothing to display
            }

            if (remaining <= 0f)
            {
                _activePowerUps.Remove(type);
            }
            else
            {
                _activePowerUps[type] = remaining;
            }
        }

        private void OnResumeClicked()
        {
            AudioManager.Instance?.PlayUiClick();
            _gameManager.ResumeGame();
        }

        private void OnRestartClicked()
        {
            AudioManager.Instance?.PlayUiClick();
            _gameManager.RestartGame();
        }

        private void OnMainMenuClicked()
        {
            AudioManager.Instance?.PlayUiClick();
            _gameManager.GoToMainMenu();
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.ScoreChanged -= OnScoreChanged;
                _gameManager.WaveChanged -= OnWaveChanged;
                _gameManager.StateChanged -= OnStateChanged;
            }
            if (_player != null)
            {
                _player.HealthChanged -= OnHealthChanged;
                _player.LivesChanged -= OnLivesChanged;
                _player.PowerUpStateChanged -= OnPowerUpStateChanged;
            }
        }
    }
}
