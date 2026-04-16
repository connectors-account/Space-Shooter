using SpaceShooter.Core;
using SpaceShooter.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class GameOverController : MonoBehaviour
    {
        private void Start()
        {
            EnsureSession();
            EnsureEventSystem();
            BuildUi();
        }

        private static void EnsureSession()
        {
            if (GameSession.Instance == null)
            {
                var sessionGo = new GameObject("GameSession");
                sessionGo.AddComponent<GameSession>();
            }

            if (AudioManager.Instance == null)
            {
                var audio = new GameObject("AudioManager");
                audio.AddComponent<AudioManager>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void BuildUi()
        {
            var session = GameSession.Instance;
            var canvasGo = new GameObject("GameOverCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            CreateText(canvas.transform, "Title", "GAME OVER", 60, new Vector2(0f, 180f), new Color(1f, 0.4f, 0.4f));
            CreateText(canvas.transform, "Score", $"Score: {session.Score}", 34, new Vector2(0f, 90f), Color.white);
            CreateText(canvas.transform, "Wave", $"Wave Reached: {session.Wave}", 30, new Vector2(0f, 40f), Color.white);
            CreateText(canvas.transform, "High", $"High Score: {session.HighScore}", 30, new Vector2(0f, -5f), Color.white);

            var restart = CreateButton(canvas.transform, "RestartButton", "Play Again", new Vector2(0f, -90f));
            restart.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayUi();
                session.StartNewRun();
                SceneManager.LoadScene("GamePlay");
            });

            var menu = CreateButton(canvas.transform, "MenuButton", "Main Menu", new Vector2(0f, -165f));
            menu.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayUi();
                SceneManager.LoadScene("MainMenu");
            });
        }

        private static void CreateText(Transform parent, string name, string value, int size, Vector2 position, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = value;
            text.color = color;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900f, 80f);
            rect.anchoredPosition = position;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.45f, 1f);
            var button = go.AddComponent<Button>();

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300f, 56f);
            rect.anchoredPosition = anchoredPosition;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var text = labelGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 26;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            text.color = Color.white;

            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
        }
    }
}
