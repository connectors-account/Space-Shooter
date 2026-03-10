using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Sets up the main menu scene
    /// </summary>
    public class MainMenuSetup : MonoBehaviour
    {
        [SerializeField] private bool setupOnStart = true;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupMainMenu();
            }
        }
        
        public void SetupMainMenu()
        {
            SetupCamera();
            CreateBackground();
            CreateUI();
            EnsureGameManager();
        }
        
        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
            }
            
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
        
        private void CreateBackground()
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.AddComponent<SpaceShooter.Effects.ParallaxBackground>();
        }
        
        private void CreateUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Main Panel
            GameObject mainPanel = CreatePanel(canvasObj.transform, "MainPanel");
            
            // Title
            GameObject titleObj = CreateText(mainPanel.transform, "Title", "SPACE SHOOTER");
            Text titleText = titleObj.GetComponent<Text>();
            titleText.fontSize = 72;
            titleText.color = Color.cyan;
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 150);
            titleRect.sizeDelta = new Vector2(800, 100);
            
            // Subtitle
            GameObject subtitleObj = CreateText(mainPanel.transform, "Subtitle", "Press START to begin");
            Text subtitleText = subtitleObj.GetComponent<Text>();
            subtitleText.fontSize = 24;
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchoredPosition = new Vector2(0, 70);
            
            // Start Button
            GameObject startBtn = CreateButton(mainPanel.transform, "StartButton", "START GAME");
            startBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            
            // Quit Button
            GameObject quitBtn = CreateButton(mainPanel.transform, "QuitButton", "QUIT");
            quitBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -90);
            
            // High Score
            GameObject highScoreObj = CreateText(mainPanel.transform, "HighScore", "High Score: 0");
            Text highScoreText = highScoreObj.GetComponent<Text>();
            highScoreText.fontSize = 28;
            RectTransform hsRect = highScoreObj.GetComponent<RectTransform>();
            hsRect.anchorMin = new Vector2(0.5f, 0);
            hsRect.anchorMax = new Vector2(0.5f, 0);
            hsRect.pivot = new Vector2(0.5f, 0);
            hsRect.anchoredPosition = new Vector2(0, 50);
            
            // Controls Info
            GameObject controlsObj = CreateText(mainPanel.transform, "Controls", "Controls: WASD/Arrows to move, SPACE to shoot, ESC to pause");
            Text controlsText = controlsObj.GetComponent<Text>();
            controlsText.fontSize = 18;
            RectTransform ctrlRect = controlsObj.GetComponent<RectTransform>();
            ctrlRect.anchorMin = new Vector2(0.5f, 0);
            ctrlRect.anchorMax = new Vector2(0.5f, 0);
            ctrlRect.pivot = new Vector2(0.5f, 0);
            ctrlRect.anchoredPosition = new Vector2(0, 20);
            ctrlRect.sizeDelta = new Vector2(800, 30);
            
            // Add Main Menu UI controller
            SpaceShooter.UI.MainMenuUI menuUI = mainPanel.AddComponent<SpaceShooter.UI.MainMenuUI>();
            
            // Event System
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        
        private void EnsureGameManager()
        {
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }
        }
        
        private GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            return panel;
        }
        
        private GameObject CreateText(Transform parent, string name, string content)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 50);
            
            Text text = textObj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 32;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            return textObj;
        }
        
        private GameObject CreateButton(Transform parent, string name, string label)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 55);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.1f, 0.3f, 0.5f);
            
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.1f, 0.3f, 0.5f);
            colors.highlightedColor = new Color(0.2f, 0.5f, 0.8f);
            colors.pressedColor = new Color(0.05f, 0.2f, 0.3f);
            colors.selectedColor = new Color(0.2f, 0.5f, 0.8f);
            btn.colors = colors;
            
            // Button text
            GameObject textObj = CreateText(btnObj.transform, "Text", label);
            Text text = textObj.GetComponent<Text>();
            text.fontSize = 28;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            return btnObj;
        }
    }
}
