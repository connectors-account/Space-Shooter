using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// RuntimeSceneBuilder programmatically constructs all GameObjects, prefabs,
/// and UI needed for the game at runtime. This eliminates the need for
/// manually configuring scenes in the Unity Editor.
///
/// HOW IT WORKS:
///   1. Place this script on a single GameObject in EACH scene (MainMenu, GamePlay).
///   2. It detects which scene it is in and builds the appropriate objects.
///   3. For GamePlay it creates: Player, Camera, Background, Spawners, HUD, Menus.
///   4. For MainMenu it creates: Camera, Background, Main-Menu UI.
///
/// This approach lets you open a brand-new Unity project, add empty scenes,
/// drop this one script in, and have a fully playable game.
/// </summary>
public class RuntimeSceneBuilder : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    // Entry Point
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Ensure persistent singletons exist
        EnsureSingletons();

        if (sceneName == "MainMenu")
            BuildMainMenuScene();
        else if (sceneName == "GamePlay")
            BuildGamePlayScene();
    }

    // ──────────────────────────────────────────────────────────
    // Singleton Bootstrapping
    // ──────────────────────────────────────────────────────────

    private void EnsureSingletons()
    {
        // GameManager
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        // AudioManager
        if (AudioManager.Instance == null)
        {
            GameObject am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
        }
    }

    // ══════════════════════════════════════════════════════════
    //  MAIN MENU SCENE
    // ══════════════════════════════════════════════════════════

    private void BuildMainMenuScene()
    {
        // Camera
        SetupCamera(new Color(0.02f, 0.02f, 0.08f));

        // Scrolling background
        GameObject bg = new GameObject("ParallaxBackground");
        bg.AddComponent<ParallaxBackground>();

        // --- UI Canvas ---
        GameObject canvasObj = CreateCanvas("MainMenuCanvas");
        Canvas canvas = canvasObj.GetComponent<Canvas>();

        // Title
        Text titleText = CreateText(canvasObj.transform, "TitleText",
            "SPACE SHOOTER", 48, Color.cyan,
            new Vector2(0, 120), new Vector2(600, 80));
        titleText.fontStyle = FontStyle.Bold;

        // High Score
        Text highScoreText = CreateText(canvasObj.transform, "HighScoreText",
            "HIGH SCORE: 0", 22, Color.yellow,
            new Vector2(0, 50), new Vector2(400, 40));

        // Start Button
        Button startBtn = CreateButton(canvasObj.transform, "StartButton",
            "START GAME", new Vector2(0, -30), new Vector2(220, 50),
            new Color(0.1f, 0.6f, 0.1f));

        // Quit Button
        Button quitBtn = CreateButton(canvasObj.transform, "QuitButton",
            "QUIT", new Vector2(0, -100), new Vector2(220, 50),
            new Color(0.6f, 0.1f, 0.1f));

        // Controls info
        CreateText(canvasObj.transform, "ControlsText",
            "ARROW KEYS / WASD = Move   |   SPACE = Shoot   |   ESC = Pause",
            14, Color.gray, new Vector2(0, -180), new Vector2(600, 30));

        // MenuManager
        GameObject menuMgr = new GameObject("MenuManager");
        MenuManager mm = menuMgr.AddComponent<MenuManager>();

        // Wire up serialized fields via reflection-free approach:
        // We'll set them through a helper since they're serialized private
        SetMenuManagerMainMenu(mm, canvasObj, startBtn, quitBtn, titleText, highScoreText);

        // Play menu music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();
    }

    // ══════════════════════════════════════════════════════════
    //  GAMEPLAY SCENE
    // ══════════════════════════════════════════════════════════

    private void BuildGamePlayScene()
    {
        // Camera
        SetupCamera(new Color(0.02f, 0.02f, 0.08f));

        // Scrolling background
        GameObject bg = new GameObject("ParallaxBackground");
        bg.AddComponent<ParallaxBackground>();

        // ── Player ───────────────────────────────────────────
        GameObject player = CreatePlayer();

        // ── Bullet Prefab (template, disabled) ───────────────
        GameObject bulletPrefab = CreateBulletPrefab();

        // ── Enemy Prefab (template, disabled) ────────────────
        GameObject enemyPrefab = CreateEnemyPrefab(bulletPrefab);

        // ── Power-Up Prefab (template, disabled) ─────────────
        GameObject powerUpPrefab = CreatePowerUpPrefab();

        // ── Assign bullet prefab to player ───────────────────
        PlayerController pc = player.GetComponent<PlayerController>();
        SetPrivateField(pc, "bulletPrefab", bulletPrefab);

        // ── Enemy Spawner ────────────────────────────────────
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner es = spawnerObj.AddComponent<EnemySpawner>();
        SetPrivateField(es, "enemyPrefab", enemyPrefab);

        // ── Power-Up Spawner ─────────────────────────────────
        GameObject puSpawnerObj = new GameObject("PowerUpSpawner");
        PowerUpSpawner pus = puSpawnerObj.AddComponent<PowerUpSpawner>();
        SetPrivateField(pus, "powerUpPrefab", powerUpPrefab);

        // ── UI Canvas (HUD) ──────────────────────────────────
        GameObject canvasObj = CreateCanvas("HUDCanvas");
        BuildHUD(canvasObj);

        // ── Pause & Game Over panels ─────────────────────────
        GameObject pausePanel = BuildPausePanel(canvasObj);
        GameOverPanelResult goResult = BuildGameOverPanel(canvasObj);

        // ── Menu Manager (in-game) ───────────────────────────
        GameObject menuMgr = new GameObject("MenuManager");
        MenuManager mm = menuMgr.AddComponent<MenuManager>();
        SetPrivateField(mm, "pauseMenuPanel", pausePanel);
        SetPrivateField(mm, "gameOverPanel", goResult.panel);
        SetPrivateField(mm, "finalScoreText", goResult.scoreText);
        SetPrivateField(mm, "gameOverHighScoreText", goResult.highScoreText);
        SetPrivateField(mm, "restartButton", goResult.restartBtn);
        SetPrivateField(mm, "menuButton", goResult.menuBtn);
        SetPrivateField(mm, "resumeButton", pausePanel.transform.Find("ResumeButton")?.GetComponent<Button>());
        SetPrivateField(mm, "pauseQuitButton", pausePanel.transform.Find("QuitButton")?.GetComponent<Button>());

        // ── Gameplay Bootstrap ───────────────────────────────
        GameObject bootstrap = new GameObject("GameplayBootstrap");
        bootstrap.AddComponent<GameplaySceneBootstrap>();
    }

    // ══════════════════════════════════════════════════════════
    //  PREFAB BUILDERS
    // ══════════════════════════════════════════════════════════

    private GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");
        player.transform.position = new Vector3(0f, -3.5f, 0f);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePlayerShip();
        sr.sortingOrder = 10;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        player.AddComponent<PlayerController>();

        // Shield visual child
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSR = shield.AddComponent<SpriteRenderer>();
        shieldSR.sprite = SpriteGenerator.CreateShield();
        shieldSR.sortingOrder = 11;
        shield.SetActive(false);

        PlayerController pc = player.GetComponent<PlayerController>();
        SetPrivateField(pc, "shieldVisual", shield);

        return player;
    }

    private GameObject CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("BulletPrefab");
        bullet.SetActive(false); // template, not active in scene

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBullet();
        sr.sortingOrder = 5;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.3f, 0.6f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        bullet.AddComponent<BulletController>();

        // Keep it around for Instantiate
        DontDestroyOnLoad(bullet);
        return bullet;
    }

    private GameObject CreateEnemyPrefab(GameObject bulletPrefab)
    {
        GameObject enemy = new GameObject("EnemyPrefab");
        enemy.tag = "Enemy";
        enemy.SetActive(false);

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateEnemyShip();
        sr.sortingOrder = 10;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        SetPrivateField(ec, "bulletPrefab", bulletPrefab);

        DontDestroyOnLoad(enemy);
        return enemy;
    }

    private GameObject CreatePowerUpPrefab()
    {
        GameObject pu = new GameObject("PowerUpPrefab");
        pu.tag = "Untagged";
        pu.SetActive(false);

        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePowerUp();
        sr.sortingOrder = 8;

        CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        pu.AddComponent<PowerUpController>();

        DontDestroyOnLoad(pu);
        return pu;
    }

    // ══════════════════════════════════════════════════════════
    //  UI BUILDERS
    // ══════════════════════════════════════════════════════════

    private GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    private void BuildHUD(GameObject canvasObj)
    {
        // Score – top left
        Text scoreText = CreateText(canvasObj.transform, "ScoreText",
            "SCORE: 0", 28, Color.white,
            new Vector2(-750, 480), new Vector2(300, 40));
        scoreText.alignment = TextAnchor.UpperLeft;

        // Wave – top center
        Text waveText = CreateText(canvasObj.transform, "WaveText",
            "WAVE 1", 28, Color.yellow,
            new Vector2(0, 480), new Vector2(200, 40));

        // Health – top right
        Text healthText = CreateText(canvasObj.transform, "HealthText",
            "HP: 5 / 5", 28, Color.green,
            new Vector2(750, 480), new Vector2(300, 40));
        healthText.alignment = TextAnchor.UpperRight;

        // Wave Announcement – center screen
        Text waveAnnouncement = CreateText(canvasObj.transform, "WaveAnnouncement",
            "", 56, Color.white,
            new Vector2(0, 0), new Vector2(600, 80));
        waveAnnouncement.fontStyle = FontStyle.Bold;
        waveAnnouncement.gameObject.SetActive(false);

        // UIManager
        GameObject uiMgr = new GameObject("UIManager");
        UIManager uim = uiMgr.AddComponent<UIManager>();
        SetPrivateField(uim, "scoreText", scoreText);
        SetPrivateField(uim, "waveText", waveText);
        SetPrivateField(uim, "healthText", healthText);
        SetPrivateField(uim, "waveAnnouncementText", waveAnnouncement);
    }

    private GameObject BuildPausePanel(GameObject canvasObj)
    {
        // Semi-transparent dark overlay
        GameObject panel = CreatePanel(canvasObj.transform, "PauseMenuPanel",
            new Color(0, 0, 0, 0.75f));

        CreateText(panel.transform, "PauseTitle", "PAUSED", 48, Color.white,
            new Vector2(0, 80), new Vector2(400, 60));

        Button resumeBtn = CreateButton(panel.transform, "ResumeButton", "RESUME",
            new Vector2(0, -10), new Vector2(220, 50), new Color(0.1f, 0.6f, 0.1f));

        Button quitBtn = CreateButton(panel.transform, "QuitButton", "MAIN MENU",
            new Vector2(0, -80), new Vector2(220, 50), new Color(0.6f, 0.1f, 0.1f));

        panel.SetActive(false);
        return panel;
    }

    private struct GameOverPanelResult
    {
        public GameObject panel;
        public Text scoreText;
        public Text highScoreText;
        public Button restartBtn;
        public Button menuBtn;
    }

    private GameOverPanelResult BuildGameOverPanel(GameObject canvasObj)
    {
        GameObject panel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0, 0, 0, 0.85f));

        CreateText(panel.transform, "GameOverTitle", "GAME OVER", 56, Color.red,
            new Vector2(0, 120), new Vector2(500, 70));

        Text scoreText = CreateText(panel.transform, "FinalScoreText", "SCORE: 0",
            36, Color.white, new Vector2(0, 40), new Vector2(400, 50));

        Text highScoreText = CreateText(panel.transform, "HighScoreText", "HIGH SCORE: 0",
            28, Color.yellow, new Vector2(0, -20), new Vector2(400, 40));

        Button restartBtn = CreateButton(panel.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -90), new Vector2(220, 50), new Color(0.1f, 0.6f, 0.1f));

        Button menuBtn = CreateButton(panel.transform, "MenuButton", "MAIN MENU",
            new Vector2(0, -160), new Vector2(220, 50), new Color(0.4f, 0.4f, 0.4f));

        panel.SetActive(false);

        return new GameOverPanelResult
        {
            panel = panel,
            scoreText = scoreText,
            highScoreText = highScoreText,
            restartBtn = restartBtn,
            menuBtn = menuBtn
        };
    }

    // ══════════════════════════════════════════════════════════
    //  UI UTILITY METHODS
    // ══════════════════════════════════════════════════════════

    private Text CreateText(Transform parent, string name, string content,
        int fontSize, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return text;
    }

    private Button CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = bgColor * 1.2f;
        cb.pressedColor = bgColor * 0.8f;
        btn.colors = cb;

        // Button label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);

        RectTransform lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        Text text = labelObj.AddComponent<Text>();
        text.text = label;
        text.fontSize = 22;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", 22);

        return btn;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        return obj;
    }

    // ══════════════════════════════════════════════════════════
    //  CAMERA
    // ══════════════════════════════════════════════════════════

    private void SetupCamera(Color bgColor)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = bgColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    // ══════════════════════════════════════════════════════════
    //  REFLECTION HELPER – set [SerializeField] private fields
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Sets a private serialized field on a MonoBehaviour by name.
    /// Used to wire up references that would normally be dragged in the Inspector.
    /// </summary>
    private void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null) return;

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

        Debug.LogWarning($"RuntimeSceneBuilder: Could not find field '{fieldName}' on {target.GetType().Name}");
    }

    // ══════════════════════════════════════════════════════════
    //  MAIN MENU HELPER
    // ══════════════════════════════════════════════════════════

    private void SetMenuManagerMainMenu(MenuManager mm, GameObject canvas,
        Button startBtn, Button quitBtn, Text titleText, Text highScoreText)
    {
        SetPrivateField(mm, "mainMenuPanel", canvas);
        SetPrivateField(mm, "startButton", startBtn);
        SetPrivateField(mm, "quitButton", quitBtn);
        SetPrivateField(mm, "titleText", titleText);
        SetPrivateField(mm, "highScoreText", highScoreText);
    }
}
