using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public static class UIBuilder
    {
        public static UIManager CreateUI()
        {
            GameObject canvasObj = new GameObject("GameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            UIManager manager = canvasObj.AddComponent<UIManager>();

            GameObject menu = CreatePanel("MainMenu", canvasObj.transform, new Color(0f, 0f, 0f, 0.55f));
            GameObject hud = CreatePanel("HUD", canvasObj.transform, Color.clear);
            GameObject pause = CreatePanel("Pause", canvasObj.transform, new Color(0f, 0f, 0f, 0.65f));
            GameObject gameOver = CreatePanel("GameOver", canvasObj.transform, new Color(0f, 0f, 0f, 0.75f));

            Text title = CreateText(menu.transform, "SPACE SHOOTER", 56, new Vector2(0.5f, 0.82f), TextAnchor.MiddleCenter);
            title.color = Color.cyan;
            Text highScore = CreateText(menu.transform, "High Score: 0", 26, new Vector2(0.5f, 0.68f), TextAnchor.MiddleCenter);
            Button startButton = CreateButton(menu.transform, "Start", new Vector2(0.5f, 0.52f));
            Button quitButton = CreateButton(menu.transform, "Quit", new Vector2(0.5f, 0.40f));

            Text score = CreateText(hud.transform, "Score: 0", 28, new Vector2(0.12f, 0.95f), TextAnchor.MiddleLeft);
            Text wave = CreateText(hud.transform, "Wave: 0", 28, new Vector2(0.50f, 0.95f), TextAnchor.MiddleCenter);
            Text health = CreateText(hud.transform, "HP: 100/100", 28, new Vector2(0.88f, 0.95f), TextAnchor.MiddleRight);
            Text status = CreateText(hud.transform, string.Empty, 22, new Vector2(0.5f, 0.08f), TextAnchor.MiddleCenter);
            status.color = new Color(1f, 0.9f, 0.3f);

            CreateText(pause.transform, "PAUSED", 48, new Vector2(0.5f, 0.72f), TextAnchor.MiddleCenter);
            Button resumeButton = CreateButton(pause.transform, "Resume", new Vector2(0.5f, 0.52f));
            Button pauseMenuButton = CreateButton(pause.transform, "Main Menu", new Vector2(0.5f, 0.40f));

            Text gameOverSummary = CreateText(gameOver.transform, "Game Over", 38, new Vector2(0.5f, 0.68f), TextAnchor.MiddleCenter);
            Button restartButton = CreateButton(gameOver.transform, "Restart", new Vector2(0.5f, 0.44f));
            Button gameOverMenuButton = CreateButton(gameOver.transform, "Main Menu", new Vector2(0.5f, 0.32f));

            manager.SetElements(
                score,
                wave,
                health,
                status,
                menu,
                hud,
                pause,
                gameOver,
                highScore,
                gameOverSummary,
                startButton,
                quitButton,
                resumeButton,
                pauseMenuButton,
                restartButton,
                gameOverMenuButton);

            pause.SetActive(false);
            gameOver.SetActive(false);

            return manager;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Text CreateText(Transform parent, string content, int fontSize, Vector2 anchor, TextAnchor alignment)
        {
            GameObject textObj = new GameObject($"Text_{content}");
            textObj.transform.SetParent(parent, false);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(700f, 70f);
            rect.anchoredPosition = Vector2.zero;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchor)
        {
            GameObject buttonObj = new GameObject($"Button_{label}");
            buttonObj.transform.SetParent(parent, false);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.16f, 0.25f, 0.45f, 0.95f);

            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.25f, 0.35f, 0.65f, 1f);
            colors.pressedColor = new Color(0.12f, 0.2f, 0.35f, 1f);
            button.colors = colors;

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(240f, 60f);
            rect.anchoredPosition = Vector2.zero;

            Text text = CreateText(buttonObj.transform, label, 28, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            text.rectTransform.sizeDelta = new Vector2(220f, 54f);

            return button;
        }
    }
}
