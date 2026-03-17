using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Effects;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Sets up the game scene with all necessary objects
    /// Attach this to an empty GameObject in your scene
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool createPlayer = true;
        [SerializeField] private bool createUI = true;
        [SerializeField] private bool createBackground = true;
        [SerializeField] private bool createSpawners = true;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupScene();
            }
        }
        
        public void SetupScene()
        {
            SetupCamera();
            
            if (createBackground)
                CreateBackground();
                
            if (createPlayer)
                CreatePlayer();
                
            if (createSpawners)
                CreateSpawners();
                
            if (createUI)
                CreateUI();
                
            CreateBoundary();
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
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.15f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
        
        private void CreateBackground()
        {
            GameObject bgObj = new GameObject("Background");
            ParallaxBackground parallax = bgObj.AddComponent<ParallaxBackground>();
        }
        
        private void CreatePlayer()
        {
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            playerObj.layer = LayerMask.NameToLayer("Default");
            
            // Add sprite renderer
            SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlayerSprite();
            sr.color = Color.cyan;
            sr.sortingOrder = 10;
            
            // Add collider
            BoxCollider2D col = playerObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.8f);
            
            // Add rigidbody
            Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // Add player controller
            PlayerController pc = playerObj.AddComponent<PlayerController>();
            
            // Create fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(playerObj.transform);
            firePoint.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            // Create shield visual
            GameObject shield = new GameObject("ShieldVisual");
            shield.transform.SetParent(playerObj.transform);
            shield.transform.localPosition = Vector3.zero;
            SpriteRenderer shieldSr = shield.AddComponent<SpriteRenderer>();
            shieldSr.sprite = CreateCircleSprite();
            shieldSr.color = new Color(0f, 0.8f, 1f, 0.3f);
            shield.transform.localScale = Vector3.one * 2f;
            shield.SetActive(false);
            
            // Position player
            playerObj.transform.position = new Vector3(0, -3.5f, 0);
        }
        
        private void CreateSpawners()
        {
            // Enemy Spawner
            GameObject enemySpawnerObj = new GameObject("EnemySpawner");
            EnemySpawner es = enemySpawnerObj.AddComponent<EnemySpawner>();
            
            // Power-up Spawner
            GameObject powerUpSpawnerObj = new GameObject("PowerUpSpawner");
            PowerUpSpawner ps = powerUpSpawnerObj.AddComponent<PowerUpSpawner>();
        }
        
        private void CreateUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // HUD Panel
            GameObject hudPanel = CreatePanel(canvasObj.transform, "HUDPanel");
            
            // Score Text
            GameObject scoreObj = CreateText(hudPanel.transform, "ScoreText", "Score: 0");
            RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.pivot = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(20, -20);
            
            // Wave Text
            GameObject waveObj = CreateText(hudPanel.transform, "WaveText", "Wave 1");
            RectTransform waveRect = waveObj.GetComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0.5f, 1);
            waveRect.anchorMax = new Vector2(0.5f, 1);
            waveRect.pivot = new Vector2(0.5f, 1);
            waveRect.anchoredPosition = new Vector2(0, -20);
            
            // Health Bar
            GameObject healthBarObj = CreateHealthBar(hudPanel.transform);
            
            // Add HUD controller
            hudPanel.AddComponent<SpaceShooter.UI.GameHUD>();
            
            // Pause Menu
            GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel");
            pausePanel.SetActive(false);
            CreateText(pausePanel.transform, "PauseTitle", "PAUSED").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
            
            GameObject resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "Resume");
            resumeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            
            GameObject menuBtn = CreateButton(pausePanel.transform, "MenuButton", "Main Menu");
            menuBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
            
            pausePanel.AddComponent<SpaceShooter.UI.PauseMenuUI>();
            
            // Game Over Panel
            GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel");
            gameOverPanel.SetActive(false);
            CreateText(gameOverPanel.transform, "GameOverTitle", "GAME OVER").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
            CreateText(gameOverPanel.transform, "FinalScoreText", "Score: 0").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);
            
            GameObject restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "Restart");
            restartBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
            
            gameOverPanel.AddComponent<SpaceShooter.UI.GameOverUI>();
            
            // Event System
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        
        private void CreateBoundary()
        {
            GameObject boundary = new GameObject("Boundary");
            boundary.tag = "Boundary";
            
            BoxCollider2D col = boundary.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(20f, 15f);
            col.offset = Vector2.zero;
            
            boundary.AddComponent<Boundary>();
        }
        
        // Helper methods for creating sprites
        private Sprite CreatePlayerSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    // Create a simple triangle/arrow shape
                    bool isInTriangle = y >= 32 - x && y >= x && y < 28;
                    bool isInBody = x >= 12 && x <= 19 && y >= 4 && y <= 20;
                    
                    colors[y * 32 + x] = (isInTriangle || isInBody) ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }
        
        private Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            
            Vector2 center = new Vector2(32, 32);
            float radius = 30f;
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    colors[y * 64 + x] = dist <= radius ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
        }
        
        // UI Helper methods
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
            rect.sizeDelta = new Vector2(300, 50);
            
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
            rect.sizeDelta = new Vector2(200, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.3f);
            
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.2f);
            btn.colors = colors;
            
            // Button text
            GameObject textObj = CreateText(btnObj.transform, "Text", label);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            return btnObj;
        }
        
        private GameObject CreateHealthBar(Transform parent)
        {
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(parent, false);
            
            RectTransform rect = healthBarObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);
            rect.sizeDelta = new Vector2(200, 30);
            
            // Background
            Image bgImg = healthBarObj.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f);
            
            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(healthBarObj.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = new Vector2(-4, -4);
            
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.green;
            
            // Slider
            Slider slider = healthBarObj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.interactable = false;
            slider.maxValue = 100;
            slider.value = 100;
            
            return healthBarObj;
        }
    }
}
