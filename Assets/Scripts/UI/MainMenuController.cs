using SpaceShooter.Core;
using SpaceShooter.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class MainMenuController : MonoBehaviour
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
            var canvasGo = new GameObject("MainMenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var title = CreateText(canvas.transform, "Title", "SPACE SHOOTER", 58, new Vector2(0f, 180f));
            title.color = new Color(0.65f, 0.9f, 1f);

            var subtitle = CreateText(canvas.transform, "Subtitle", "Retro arcade survival", 22, new Vector2(0f, 130f));
            subtitle.color = new Color(0.9f, 0.9f, 1f);

            var high = CreateText(canvas.transform, "HighScore", $"High Score: {GameSession.Instance.HighScore}", 28, new Vector2(0f, 70f));
            high.color = Color.white;

            var start = CreateButton(canvas.transform, "StartButton", "Start Game", new Vector2(0f, -20f));
            start.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayUi();
                GameSession.Instance.StartNewRun();
                SceneManager.LoadScene("GamePlay");
            });

            var quit = CreateButton(canvas.transform, "QuitButton", "Quit", new Vector2(0f, -95f));
            quit.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayUi();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = value;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800f, 80f);
            rect.anchoredPosition = anchoredPosition;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.16f, 0.22f, 0.45f, 1f);
            var button = go.AddComponent<Button>();

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300f, 56f);
            rect.anchoredPosition = anchoredPosition;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var text = labelGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
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
