using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Master bootstrap that creates the entire game scene programmatically.
    /// Attach this single script to an empty GameObject in the scene.
    /// It builds all game objects, prefabs, UI, pools, and wiring automatically.
    ///
    /// This eliminates the need to manually configure the scene in the Unity Editor.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            BuildScene();
        }

        private void BuildScene()
        {
            // ── Camera setup ──
            Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5f;

            // ── Manager GameObjects ──
            CreateManagers();

            // ── Player ──
            GameObject player = CreatePlayer();

            // ── Prefabs for pooling ──
            GameObject playerBulletPrefab = CreateBulletPrefab("PlayerBulletPrefab", Color.cyan, true);
            GameObject enemyBulletPrefab = CreateBulletPrefab("EnemyBulletPrefab", Color.red, false);
            GameObject basicEnemyPrefab = CreateEnemyPrefab<Enemies.BasicEnemy>("BasicEnemyPrefab", Color.red, 0.4f);
            GameObject fastEnemyPrefab = CreateEnemyPrefab<Enemies.FastEnemy>("FastEnemyPrefab", Color.magenta, 0.3f);
            GameObject tankEnemyPrefab = CreateEnemyPrefab<Enemies.TankEnemy>("TankEnemyPrefab", new Color(0.8f, 0.3f, 0f), 0.6f);
            GameObject powerUpPrefab = CreatePowerUpPrefab();

            // ── Object Pool setup ──
            SetupObjectPools(playerBulletPrefab, enemyBulletPrefab,
                basicEnemyPrefab, fastEnemyPrefab, tankEnemyPrefab, powerUpPrefab);

            // ── Background ──
            CreateBackground();

            // ── UI ──
            CreateUI();
        }

        // ═══════════════════════════════════════════
        //  MANAGERS
        // ═══════════════════════════════════════════

        private void CreateManagers()
        {
            // GameManager
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();

            // AudioManager
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();

            // WaveSpawner
            GameObject waveObj = new GameObject("WaveSpawner");
            waveObj.AddComponent<WaveSpawner>();

            // ExplosionManager
            GameObject expObj = new GameObject("ExplosionManager");
            expObj.AddComponent<Effects.ExplosionManager>();

            // PowerUpSpawner
            GameObject puObj = new GameObject("PowerUpSpawner");
            puObj.AddComponent<PowerUps.PowerUpSpawner>();

            // ObjectPoolManager (pools configured later)
            GameObject poolObj = new GameObject("ObjectPoolManager");
            poolObj.AddComponent<ObjectPoolManager>();
        }

        // ═══════════════════════════════════════════
        //  PLAYER
        // ═══════════════════════════════════════════

        private GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");
            player.transform.position = new Vector3(0f, -3.5f, 0f);

            // Sprite
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = Utils.SpriteGenerator.CreateTriangle(32, new Color(0.2f, 0.8f, 1f));
            sr.sortingOrder = 5;

            // Physics
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            BoxCollider2D col = player.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.6f, 0.8f);

            // Player controller
            player.AddComponent<Player.PlayerController>();

            // Shield visual (child object)
            GameObject shield = new GameObject("ShieldVisual");
            shield.transform.SetParent(player.transform);
            shield.transform.localPosition = Vector3.zero;
            SpriteRenderer shieldSr = shield.AddComponent<SpriteRenderer>();
            shieldSr.sprite = Utils.SpriteGenerator.CreateCircle(48, new Color(0.3f, 0.8f, 1f, 0.3f));
            shieldSr.sortingOrder = 6;
            shield.SetActive(false);

            // Wire shield visual into PlayerController via reflection/serialization workaround
            // We'll use a helper for this
            var pc = player.GetComponent<Player.PlayerController>();
            SetPrivateField(pc, "shieldVisual", shield);
            SetPrivateField(pc, "spriteRenderer", sr);

            // Fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(player.transform);
            firePoint.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            SetPrivateField(pc, "firePoint", firePoint.transform);

            return player;
        }

        // ═══════════════════════════════════════════
        //  PREFABS
        // ═══════════════════════════════════════════

        private GameObject CreateBulletPrefab(string name, Color color, bool isPlayer)
        {
            GameObject prefab = new GameObject(name);
            prefab.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";

            SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = Utils.SpriteGenerator.CreateSquare(8, color);
            sr.sortingOrder = 4;
            prefab.transform.localScale = new Vector3(0.3f, 0.6f, 1f);

            Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 1f);

            prefab.AddComponent<Weapons.Bullet>();

            prefab.SetActive(false);
            prefab.transform.SetParent(transform); // hide in bootstrap
            return prefab;
        }

        private GameObject CreateEnemyPrefab<T>(string name, Color color, float scale) where T : Enemies.EnemyBase
        {
            GameObject prefab = new GameObject(name);
            prefab.tag = "Enemy";

            SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
            if (typeof(T) == typeof(Enemies.TankEnemy))
                sr.sprite = Utils.SpriteGenerator.CreateSquare(32, color);
            else if (typeof(T) == typeof(Enemies.FastEnemy))
                sr.sprite = Utils.SpriteGenerator.CreateDiamond(32, color);
            else
                sr.sprite = Utils.SpriteGenerator.CreateTriangle(32, color);
            sr.sortingOrder = 3;
            sr.flipY = true; // enemies face downward
            prefab.transform.localScale = Vector3.one * scale;

            Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.8f);

            prefab.AddComponent<T>();

            prefab.SetActive(false);
            prefab.transform.SetParent(transform);
            return prefab;
        }

        private GameObject CreatePowerUpPrefab()
        {
            GameObject prefab = new GameObject("PowerUpPrefab");
            prefab.tag = "PowerUp";

            SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = Utils.SpriteGenerator.CreateDiamond(24, Color.green);
            sr.sortingOrder = 4;
            prefab.transform.localScale = Vector3.one * 0.5f;

            BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);

            prefab.AddComponent<PowerUps.PowerUpItem>();

            prefab.SetActive(false);
            prefab.transform.SetParent(transform);
            return prefab;
        }

        // ═══════════════════════════════════════════
        //  OBJECT POOLS
        // ═══════════════════════════════════════════

        private void SetupObjectPools(GameObject playerBullet, GameObject enemyBullet,
            GameObject basicEnemy, GameObject fastEnemy, GameObject tankEnemy, GameObject powerUp)
        {
            var poolMgr = FindFirstObjectByType<ObjectPoolManager>();
            if (poolMgr == null) return;

            // Use reflection to set the pools list since it's serialized
            var poolsList = new System.Collections.Generic.List<ObjectPoolManager.Pool>
            {
                new ObjectPoolManager.Pool { tag = "PlayerBullet", prefab = playerBullet, initialSize = 30 },
                new ObjectPoolManager.Pool { tag = "EnemyBullet", prefab = enemyBullet, initialSize = 30 },
                new ObjectPoolManager.Pool { tag = "BasicEnemy", prefab = basicEnemy, initialSize = 15 },
                new ObjectPoolManager.Pool { tag = "FastEnemy", prefab = fastEnemy, initialSize = 10 },
                new ObjectPoolManager.Pool { tag = "TankEnemy", prefab = tankEnemy, initialSize = 5 },
                new ObjectPoolManager.Pool { tag = "PowerUp", prefab = powerUp, initialSize = 5 },
            };

            SetPrivateField(poolMgr, "pools", poolsList);

            // Re-initialize pools after setting the list
            // We need to call Awake again, but it already ran. Use a manual init method.
            // Since Awake already ran with empty pools, we'll use reflection to call InitializePools
            var method = typeof(ObjectPoolManager).GetMethod("InitializePools",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(poolMgr, null);
        }

        // ═══════════════════════════════════════════
        //  BACKGROUND
        // ═══════════════════════════════════════════

        private void CreateBackground()
        {
            // Background colour is already set on camera.
            // Use procedural starfield for parallax effect.
            GameObject starfield = new GameObject("Starfield");
            starfield.AddComponent<Effects.StarfieldGenerator>();
        }

        // ═══════════════════════════════════════════
        //  UI
        // ═══════════════════════════════════════════

        private void CreateUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("UICanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // Event System
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // ── Main Menu ──
            CreateMainMenuUI(canvasObj.transform);

            // ── HUD ──
            CreateHudUI(canvasObj.transform);

            // ── Pause Menu ──
            CreatePauseMenuUI(canvasObj.transform);

            // ── Game Over ──
            CreateGameOverUI(canvasObj.transform);
        }

        private void CreateMainMenuUI(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "MainMenuPanel", new Color(0, 0, 0, 0.85f));

            TextMeshProUGUI title = CreateText(panel.transform, "Title", "SPACE SHOOTER",
                48, Color.cyan, new Vector2(0, 120));

            TextMeshProUGUI highScore = CreateText(panel.transform, "HighScore", "",
                24, Color.yellow, new Vector2(0, 50));

            Button startBtn = CreateButton(panel.transform, "StartButton", "START GAME",
                new Vector2(0, -30), new Color(0.1f, 0.6f, 0.1f));

            Button quitBtn = CreateButton(panel.transform, "QuitButton", "QUIT",
                new Vector2(0, -110), new Color(0.6f, 0.1f, 0.1f));

            // Attach MainMenuUI script
            GameObject menuHost = new GameObject("MainMenuController");
            menuHost.transform.SetParent(parent);
            UI.MainMenuUI menuUI = menuHost.AddComponent<UI.MainMenuUI>();
            SetPrivateField(menuUI, "menuPanel", panel);
            SetPrivateField(menuUI, "startButton", startBtn);
            SetPrivateField(menuUI, "quitButton", quitBtn);
            SetPrivateField(menuUI, "titleText", title);
            SetPrivateField(menuUI, "highScoreText", highScore);
        }

        private void CreateHudUI(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "HudPanel", new Color(0, 0, 0, 0));

            // Score (top-left)
            TextMeshProUGUI scoreText = CreateText(panel.transform, "ScoreText", "SCORE: 0",
                28, Color.white, new Vector2(-700, 470));

            // Wave (top-center)
            TextMeshProUGUI waveText = CreateText(panel.transform, "WaveText", "WAVE 1",
                28, Color.yellow, new Vector2(0, 470));

            // Health (top-right)
            TextMeshProUGUI healthText = CreateText(panel.transform, "HealthText", "LIVES: 5/5",
                28, Color.green, new Vector2(700, 470));

            // Attach HudUI script
            GameObject hudHost = new GameObject("HudController");
            hudHost.transform.SetParent(parent);
            UI.HudUI hudUI = hudHost.AddComponent<UI.HudUI>();
            SetPrivateField(hudUI, "hudPanel", panel);
            SetPrivateField(hudUI, "scoreText", scoreText);
            SetPrivateField(hudUI, "waveText", waveText);
            SetPrivateField(hudUI, "healthText", healthText);
        }

        private void CreatePauseMenuUI(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "PausePanel", new Color(0, 0, 0, 0.8f));

            TextMeshProUGUI title = CreateText(panel.transform, "PauseTitle", "PAUSED",
                48, Color.white, new Vector2(0, 80));

            Button resumeBtn = CreateButton(panel.transform, "ResumeButton", "RESUME",
                new Vector2(0, -20), new Color(0.1f, 0.5f, 0.8f));

            Button menuBtn = CreateButton(panel.transform, "MainMenuButton", "MAIN MENU",
                new Vector2(0, -100), new Color(0.6f, 0.1f, 0.1f));

            GameObject pauseHost = new GameObject("PauseMenuController");
            pauseHost.transform.SetParent(parent);
            UI.PauseMenuUI pauseUI = pauseHost.AddComponent<UI.PauseMenuUI>();
            SetPrivateField(pauseUI, "pausePanel", panel);
            SetPrivateField(pauseUI, "resumeButton", resumeBtn);
            SetPrivateField(pauseUI, "mainMenuButton", menuBtn);
            SetPrivateField(pauseUI, "pauseTitle", title);
        }

        private void CreateGameOverUI(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "GameOverPanel", new Color(0, 0, 0, 0.85f));

            TextMeshProUGUI title = CreateText(panel.transform, "GameOverTitle", "GAME OVER",
                48, Color.red, new Vector2(0, 120));

            TextMeshProUGUI score = CreateText(panel.transform, "FinalScore", "SCORE: 0",
                32, Color.white, new Vector2(0, 50));

            TextMeshProUGUI highScore = CreateText(panel.transform, "HighScore", "",
                24, Color.yellow, new Vector2(0, 10));

            Button restartBtn = CreateButton(panel.transform, "RestartButton", "PLAY AGAIN",
                new Vector2(0, -60), new Color(0.1f, 0.6f, 0.1f));

            Button menuBtn = CreateButton(panel.transform, "MainMenuButton", "MAIN MENU",
                new Vector2(0, -140), new Color(0.5f, 0.5f, 0.5f));

            GameObject goHost = new GameObject("GameOverController");
            goHost.transform.SetParent(parent);
            UI.GameOverUI goUI = goHost.AddComponent<UI.GameOverUI>();
            SetPrivateField(goUI, "gameOverPanel", panel);
            SetPrivateField(goUI, "finalScoreText", score);
            SetPrivateField(goUI, "highScoreText", highScore);
            SetPrivateField(goUI, "gameOverTitle", title);
            SetPrivateField(goUI, "restartButton", restartBtn);
            SetPrivateField(goUI, "mainMenuButton", menuBtn);
        }

        // ═══════════════════════════════════════════
        //  UI HELPERS
        // ═══════════════════════════════════════════

        private GameObject CreatePanel(Transform parent, string name, Color bgColor)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = panel.AddComponent<Image>();
            img.color = bgColor;

            return panel;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string content,
            int fontSize, Color color, Vector2 position)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(600, 60);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string label,
            Vector2 position, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(300, 60);

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.3f;
            colors.pressedColor = bgColor * 0.7f;
            btn.colors = colors;

            // Button label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);

            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return btn;
        }

        // ═══════════════════════════════════════════
        //  REFLECTION HELPER
        // ═══════════════════════════════════════════

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
                type = type.BaseType;
            }
            Debug.LogWarning($"SceneBootstrap: Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
