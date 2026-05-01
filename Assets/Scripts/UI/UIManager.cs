using SpaceShooter.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class UIManager : MonoBehaviour
    {
        private Canvas _canvas;
        private Font _font;

        private GameObject _mainMenuPanel;
        private GameObject _optionsPanel;
        private GameObject _pausePanel;
        private GameObject _gameOverPanel;
        private GameObject _hudPanel;

        private Text _healthText;
        private Text _scoreText;
        private Text _waveText;
        private Text _gameOverSummary;

        public void Initialize(GameManager gameManager)
        {
            _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            _canvas = new GameObject("UICanvas").AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvas.gameObject.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(_canvas.gameObject);

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            _mainMenuPanel = BuildMainMenu(gameManager);
            _optionsPanel = BuildOptionsMenu();
            _pausePanel = BuildPauseMenu(gameManager);
            _gameOverPanel = BuildGameOver(gameManager);
            _hudPanel = BuildHud();

            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            _mainMenuPanel.SetActive(true);
            _optionsPanel.SetActive(false);
            _pausePanel.SetActive(false);
            _gameOverPanel.SetActive(false);
            _hudPanel.SetActive(false);
        }

        public void ShowHud()
        {
            _mainMenuPanel.SetActive(false);
            _optionsPanel.SetActive(false);
            _pausePanel.SetActive(false);
            _gameOverPanel.SetActive(false);
            _hudPanel.SetActive(true);
        }

        public void ShowPauseMenu()
        {
            _pausePanel.SetActive(true);
        }

        public void HidePauseMenu()
        {
            _pausePanel.SetActive(false);
        }

        public void ShowGameOver(int score, int wave)
        {
            _gameOverPanel.SetActive(true);
            _gameOverSummary.text = $"Final Score: {score}\nWaves Survived: {wave}";
        }

        public void RefreshHealth(int current, int max, bool shield)
        {
            _healthText.text = shield ? $"HP: {current}/{max} [SHIELD]" : $"HP: {current}/{max}";
        }

        public void RefreshScore(int score) => _scoreText.text = $"Score: {score}";

        public void RefreshWave(int wave) => _waveText.text = $"Wave: {wave}";

        private GameObject BuildHud()
        {
            var panel = CreatePanel("HUD", new Vector2(1, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0));
            _healthText = CreateText("HealthText", panel.transform, new Vector2(10, -10), TextAnchor.UpperLeft, 22, "HP: 100/100");
            _scoreText = CreateText("ScoreText", panel.transform, new Vector2(10, -40), TextAnchor.UpperLeft, 22, "Score: 0");
            _waveText = CreateText("WaveText", panel.transform, new Vector2(10, -70), TextAnchor.UpperLeft, 22, "Wave: 0");
            return panel;
        }

        private GameObject BuildMainMenu(GameManager gameManager)
        {
            var panel = CreateCenteredPanel("MainMenu", new Color(0f, 0f, 0f, 0.65f));
            CreateText("Title", panel.transform, new Vector2(0, 180), TextAnchor.MiddleCenter, 44, "SPACE SHOOTER");

            CreateButton("StartButton", panel.transform, new Vector2(0, 60), "Start", () => gameManager.StartGame());
            CreateButton("OptionsButton", panel.transform, new Vector2(0, 0), "Options", () =>
            {
                panel.SetActive(false);
                _optionsPanel.SetActive(true);
            });
            CreateButton("QuitButton", panel.transform, new Vector2(0, -60), "Quit", gameManager.QuitGame);
            return panel;
        }

        private GameObject BuildOptionsMenu()
        {
            var panel = CreateCenteredPanel("OptionsMenu", new Color(0f, 0f, 0f, 0.72f));
            CreateText("OptionsTitle", panel.transform, new Vector2(0, 180), TextAnchor.MiddleCenter, 36, "OPTIONS");
            CreateText("OptionsBody", panel.transform, new Vector2(0, 35), TextAnchor.MiddleCenter, 22,
                "Controls:\nMove: Arrow Keys / WASD\nShoot: Space / Ctrl\nPause: Esc\n\nTip: Survive waves and collect power-ups.");
            CreateButton("OptionsBack", panel.transform, new Vector2(0, -140), "Back", () =>
            {
                panel.SetActive(false);
                _mainMenuPanel.SetActive(true);
            });
            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildPauseMenu(GameManager gameManager)
        {
            var panel = CreateCenteredPanel("PauseMenu", new Color(0f, 0f, 0f, 0.7f));
            CreateText("PauseTitle", panel.transform, new Vector2(0, 120), TextAnchor.MiddleCenter, 34, "PAUSED");
            CreateButton("ResumeButton", panel.transform, new Vector2(0, 20), "Resume", gameManager.TogglePause);
            CreateButton("RestartButton", panel.transform, new Vector2(0, -40), "Restart", gameManager.RestartGame);
            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildGameOver(GameManager gameManager)
        {
            var panel = CreateCenteredPanel("GameOverMenu", new Color(0f, 0f, 0f, 0.74f));
            CreateText("GameOverTitle", panel.transform, new Vector2(0, 150), TextAnchor.MiddleCenter, 40, "GAME OVER");
            _gameOverSummary = CreateText("Summary", panel.transform, new Vector2(0, 50), TextAnchor.MiddleCenter, 24, string.Empty);
            CreateButton("Restart", panel.transform, new Vector2(0, -70), "Restart", gameManager.RestartGame);
            CreateButton("Exit", panel.transform, new Vector2(0, -130), "Quit", gameManager.QuitGame);
            panel.SetActive(false);
            return panel;
        }

        private GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(_canvas.transform, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        private GameObject CreateCenteredPanel(string name, Color color)
        {
            return CreatePanel(name, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, color);
        }

        private Text CreateText(string name, Transform parent, Vector2 anchoredPos, TextAnchor anchor, int fontSize, string content)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var text = obj.GetComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.text = content;

            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(900, 180);

            if (anchor == TextAnchor.MiddleCenter)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
            }

            return text;
        }

        private void CreateButton(string name, Transform parent, Vector2 anchoredPos, string label, UnityEngine.Events.UnityAction onClick)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            var image = obj.GetComponent<Image>();
            image.color = new Color(0.1f, 0.25f, 0.45f, 0.95f);

            var button = obj.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(240, 48);

            var text = CreateText(name + "Label", obj.transform, Vector2.zero, TextAnchor.MiddleCenter, 24, label);
            var textRt = text.rectTransform;
            textRt.anchorMin = new Vector2(0, 0);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            textRt.pivot = new Vector2(0.5f, 0.5f);
            textRt.anchoredPosition = Vector2.zero;
        }
    }
}
