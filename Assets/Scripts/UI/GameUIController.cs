using SpaceShooter.Core;
using SpaceShooter.Gameplay;
using SpaceShooter.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class GameUIController : MonoBehaviour
    {
        private Text _scoreText;
        private Text _waveText;
        private Text _healthText;
        private Text _powerText;

        private GameObject _pausePanel;

        private GameSession _session;
        private PlayerController _player;

        public void Setup(GameSession session, PlayerController player, EnemySpawner spawner)
        {
            _session = session;
            _player = player;

            BuildHud();
            BuildPausePanel();

            _session.ScoreChanged += OnScoreChanged;
            _session.WaveChanged += OnWaveChanged;
            _player.HealthChanged += OnHealthChanged;
            _player.PowerStateChanged += OnPowerStateChanged;
            _player.PlayerDied += OnPlayerDied;
            spawner.WaveStarted += OnWaveChanged;

            OnScoreChanged(_session.Score);
            OnWaveChanged(_session.Wave);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        private void BuildHud()
        {
            var canvas = CreateCanvas("HUDCanvas", RenderMode.ScreenSpaceOverlay);
            var panel = CreatePanel(canvas.transform, "HUD", new Color(0f, 0f, 0f, 0.25f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(760f, 90f));

            _scoreText = CreateText(panel.transform, "ScoreText", "Score: 0", 22, new Vector2(-250f, 0f));
            _waveText = CreateText(panel.transform, "WaveText", "Wave: 1", 22, new Vector2(0f, 0f));
            _healthText = CreateText(panel.transform, "HealthText", "Health: 6/6", 22, new Vector2(250f, 0f));
            _powerText = CreateText(canvas.transform, "PowerText", "", 18, new Vector2(0f, -330f));
        }

        private void BuildPausePanel()
        {
            var canvas = CreateCanvas("PauseCanvas", RenderMode.ScreenSpaceOverlay);
            _pausePanel = CreatePanel(canvas.transform, "PausePanel", new Color(0f, 0f, 0f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 280f));
            _pausePanel.SetActive(false);

            CreateText(_pausePanel.transform, "PauseTitle", "PAUSED", 36, new Vector2(0f, 70f));

            var resume = CreateButton(_pausePanel.transform, "ResumeButton", "Resume", new Vector2(0f, 10f));
            resume.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayUi();
                TogglePause();
            });

            var menu = CreateButton(_pausePanel.transform, "MenuButton", "Main Menu", new Vector2(0f, -70f));
            menu.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayUi();
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            });
        }

        private void TogglePause()
        {
            var paused = Time.timeScale < 0.01f;
            if (paused)
            {
                Time.timeScale = 1f;
                _pausePanel.SetActive(false);
            }
            else
            {
                Time.timeScale = 0f;
                _pausePanel.SetActive(true);
            }
        }

        private void OnScoreChanged(int score)
        {
            _scoreText.text = $"Score: {score}";
        }

        private void OnWaveChanged(int wave)
        {
            _waveText.text = $"Wave: {wave}";
        }

        private void OnHealthChanged(int health, int maxHealth)
        {
            _healthText.text = $"Health: {health}/{maxHealth}";
        }

        private void OnPowerStateChanged(bool rapid, bool shield, bool spread)
        {
            var text = "";
            if (rapid) text += "RapidFire ";
            if (shield) text += "Shield ";
            if (spread) text += "Spread ";
            _powerText.text = text.Trim();
        }

        private void OnPlayerDied()
        {
            Time.timeScale = 1f;
            _session.EndRun();
            SceneManager.LoadScene("GameOver");
        }

        private static Canvas CreateCanvas(string name, RenderMode mode)
        {
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = mode;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = color;
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = value;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280f, 36f);
            rect.anchoredPosition = anchoredPos;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);
            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.18f, 0.18f, 0.35f, 0.95f);
            var button = buttonGO.AddComponent<Button>();

            var rect = buttonGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 52f);
            rect.anchoredPosition = anchoredPosition;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(buttonGO.transform, false);
            var text = labelGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = label;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
        }
    }
}
