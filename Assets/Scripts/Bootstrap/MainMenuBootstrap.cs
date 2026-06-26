using SpaceShooter.Core;
using SpaceShooter.Environment;
using SpaceShooter.Managers;
using SpaceShooter.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.Bootstrap
{
    /// <summary>
    /// Entry point for the MainMenu scene. Builds the camera, a scrolling star-field background, an
    /// audio manager and the main-menu UI (title, Play and Quit buttons) entirely in code.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        [SerializeField]
        private GameConfig _config = new GameConfig();

        private Font _font;

        private void Start()
        {
            if (_config == null || _config.PlayerMaxHealth <= 0)
            {
                _config = new GameConfig();
            }

            ConfigureCamera();

            // Background star-field for visual flair.
            var bgGo = new GameObject("Background");
            bgGo.AddComponent<BackgroundScroller>().Initialize(_config);

            // Audio for button clicks.
            var audioGo = new GameObject("AudioManager");
            audioGo.AddComponent<AudioManager>().Initialize();

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
            {
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            BuildMenu();
        }

        private void ConfigureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = _config.HalfHeight;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private void BuildMenu()
        {
            var canvasGo = new GameObject("MenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            UIFactory.EnsureEventSystem();

            UIFactory.CreateText(canvas.transform, _font, "SPACE SHOOTER", 100, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(1400f, 180f))
                .color = new Color(0.5f, 0.85f, 1f);

            UIFactory.CreateText(canvas.transform, _font, "Survive 15 waves and defeat the bosses!", 36, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1400f, 80f))
                .color = new Color(0.8f, 0.85f, 0.9f);

            UIFactory.CreateButton(canvas.transform, _font, "PLAY", new Vector2(0f, 0f), OnPlayClicked);
            UIFactory.CreateButton(canvas.transform, _font, "QUIT", new Vector2(0f, -120f), OnQuitClicked);

            UIFactory.CreateText(canvas.transform, _font,
                "Move: WASD / Arrows    Shoot: Space / Left Mouse    Pause: ESC", 28, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(1600f, 60f))
                .color = new Color(0.7f, 0.75f, 0.8f);
        }

        private void OnPlayClicked()
        {
            AudioManager.Instance?.PlayUiClick();
            SceneManager.LoadScene(GameManager.GamePlaySceneName);
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlayUiClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
