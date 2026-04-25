using SpaceShooter.Audio;
using SpaceShooter.Environment;
using SpaceShooter.Player;
using SpaceShooter.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.Core
{
    public class RuntimeBootstrap : MonoBehaviour
    {
        private void Start()
        {
            var sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "MainMenu")
            {
                BuildMainMenu();
                return;
            }

            if (sceneName == "GamePlay")
            {
                BuildGamePlay();
            }
        }

        private void BuildMainMenu()
        {
            EnsureCamera();
            EnsureEventSystem();

            var canvas = UICreation.CreateCanvas("MainMenuCanvas");
            UICreation.CreateLabel(canvas.transform, "SPACE SHOOTER", new Vector2(0, 180), 56, TextAnchor.MiddleCenter, Color.white);

            UICreation.CreateButton(canvas.transform, "Start Game", new Vector2(0, 40), new Vector2(260, 70), () =>
            {
                SceneManager.LoadScene("GamePlay");
            });

            UICreation.CreateButton(canvas.transform, "Quit", new Vector2(0, -60), new Vector2(260, 70), () =>
            {
                Application.Quit();
            });

            UICreation.CreateLabel(canvas.transform, "WASD / Arrow Keys to move | Space to shoot", new Vector2(0, -190), 22, TextAnchor.MiddleCenter, Color.gray);
        }

        private void BuildGamePlay()
        {
            EnsureCamera();
            EnsureEventSystem();

            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                new GameObject("GameManager").AddComponent<GameManager>();
            }

            if (FindObjectOfType<AudioManager>() == null)
            {
                new GameObject("AudioManager").AddComponent<AudioManager>();
            }

            if (FindObjectOfType<ParallaxBackground>() == null)
            {
                new GameObject("ParallaxBackground").AddComponent<ParallaxBackground>();
            }

            if (FindObjectOfType<Enemy.EnemySpawner>() == null)
            {
                new GameObject("EnemySpawner").AddComponent<Enemy.EnemySpawner>();
            }

            if (FindObjectOfType<PlayerController>() == null)
            {
                var playerObject = new GameObject("Player");
                playerObject.AddComponent<PlayerController>();
            }

            if (FindObjectOfType<GameUIManager>() == null)
            {
                new GameObject("UIManager").AddComponent<GameUIManager>();
            }

            GameManager.Instance.StartGame();
        }

        private static void EnsureCamera()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.backgroundColor = new Color(0.03f, 0.04f, 0.09f);
                return;
            }

            var cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.04f, 0.09f);
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
    }

    public static class UICreation
    {
        public static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static Text CreateLabel(Transform parent, string text, Vector2 anchoredPosition, int fontSize, TextAnchor anchor, Color color)
        {
            var textObject = new GameObject(text + "Label");
            textObject.transform.SetParent(parent, false);

            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.alignment = anchor;
            textComponent.color = color;

            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(900, 120);
            rectTransform.anchoredPosition = anchoredPosition;
            return textComponent;
        }

        public static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction callback)
        {
            var buttonObject = new GameObject(label + "Button");
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.22f, 0.35f, 0.95f);

            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(callback);

            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;

            var labelText = CreateLabel(buttonObject.transform, label, Vector2.zero, 28, TextAnchor.MiddleCenter, Color.white);
            labelText.rectTransform.sizeDelta = size;
            return button;
        }
    }
}
