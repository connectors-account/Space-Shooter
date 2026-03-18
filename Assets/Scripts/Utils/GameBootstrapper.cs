using UnityEngine;

/// <summary>
/// Bootstraps the game scene at runtime. Ensures all required managers and
/// game objects exist. This allows the game to work from a single scene
/// without requiring manual prefab placement in the editor.
///
/// Attach this script to an empty GameObject in the scene. It will
/// procedurally create all game objects if they don't already exist.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("Set these if using prefabs (optional - will auto-generate if null)")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private GameObject enemyStraightPrefab;
    [SerializeField] private GameObject enemyZigzagPrefab;
    [SerializeField] private GameObject enemyTankPrefab;
    [SerializeField] private GameObject enemyDiverPrefab;
    [SerializeField] private GameObject powerUpPrefab;

    private void Awake()
    {
        // Generate prefabs at runtime if not assigned
        if (playerBulletPrefab == null) playerBulletPrefab = CreateBulletPrefab(true);
        if (enemyBulletPrefab == null) enemyBulletPrefab = CreateBulletPrefab(false);
        if (enemyStraightPrefab == null) enemyStraightPrefab = CreateEnemyPrefab<EnemyStraight>("EnemyStraight", SpriteGenerator.CreateEnemyStraightSprite());
        if (enemyZigzagPrefab == null) enemyZigzagPrefab = CreateEnemyPrefab<EnemyZigzag>("EnemyZigzag", SpriteGenerator.CreateEnemyZigzagSprite());
        if (enemyTankPrefab == null) enemyTankPrefab = CreateEnemyPrefab<EnemyTank>("EnemyTank", SpriteGenerator.CreateEnemyTankSprite());
        if (enemyDiverPrefab == null) enemyDiverPrefab = CreateEnemyPrefab<EnemyDiver>("EnemyDiver", SpriteGenerator.CreateEnemyDiverSprite());
        if (powerUpPrefab == null) powerUpPrefab = CreatePowerUpPrefab();

        // Assign bullet prefab to enemy prefabs
        AssignBulletToEnemy<EnemyStraight>(enemyStraightPrefab, enemyBulletPrefab);
        AssignBulletToEnemy<EnemyZigzag>(enemyZigzagPrefab, enemyBulletPrefab);
        AssignBulletToEnemy<EnemyTank>(enemyTankPrefab, enemyBulletPrefab);

        // Setup managers
        SetupGameManager();
        SetupAudioManager();
        SetupPowerUpSpawner();
        SetupEnemySpawner();
        SetupPlayer();
        SetupStarField();
        SetupCamera();
        SetupUI();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f); // Dark space blue
        }
    }

    private void SetupGameManager()
    {
        if (GameManager.Instance == null)
        {
            GameObject go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
    }

    private void SetupAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            GameObject go = new GameObject("AudioManager");
            AudioSource musicSource = go.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;

            AudioManager am = go.AddComponent<AudioManager>();
            // Music source is serialized, but we set it via reflection or it remains null
            // The audio manager handles null clips gracefully
        }
    }

    private void SetupPowerUpSpawner()
    {
        if (PowerUpSpawner.Instance == null)
        {
            GameObject go = new GameObject("PowerUpSpawner");
            PowerUpSpawner ps = go.AddComponent<PowerUpSpawner>();
            // Set prefab via serialized field helper
            SetPrivateField(ps, "powerUpPrefab", powerUpPrefab);
        }
    }

    private void SetupEnemySpawner()
    {
        EnemySpawner existing = FindObjectOfType<EnemySpawner>();
        if (existing == null)
        {
            GameObject go = new GameObject("EnemySpawner");
            EnemySpawner es = go.AddComponent<EnemySpawner>();
            SetPrivateField(es, "enemyStraightPrefab", enemyStraightPrefab);
            SetPrivateField(es, "enemyZigzagPrefab", enemyZigzagPrefab);
            SetPrivateField(es, "enemyTankPrefab", enemyTankPrefab);
            SetPrivateField(es, "enemyDiverPrefab", enemyDiverPrefab);
        }
    }

    private void SetupPlayer()
    {
        PlayerController existing = FindObjectOfType<PlayerController>();
        if (existing == null)
        {
            GameObject player;
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, new Vector3(0, -3.5f, 0), Quaternion.identity);
            }
            else
            {
                player = new GameObject("Player");
                player.transform.position = new Vector3(0, -3.5f, 0);

                SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteGenerator.CreatePlayerSprite();
                sr.sortingOrder = 5;

                BoxCollider2D col = player.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.6f, 0.8f);

                Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.bodyType = RigidbodyType2D.Kinematic;

                PlayerController pc = player.AddComponent<PlayerController>();
                SetPrivateField(pc, "bulletPrefab", playerBulletPrefab);

                // Create fire point
                GameObject fp = new GameObject("FirePoint");
                fp.transform.parent = player.transform;
                fp.transform.localPosition = new Vector3(0, 0.5f, 0);
                SetPrivateField(pc, "firePoint", fp.transform);
            }
            player.tag = "Player";
        }
    }

    private void SetupStarField()
    {
        if (FindObjectOfType<StarFieldGenerator>() == null)
        {
            GameObject go = new GameObject("StarField");
            go.AddComponent<StarFieldGenerator>();
        }
    }

    private void SetupUI()
    {
        if (UIManager.Instance != null) return;

        // Create Canvas
        GameObject canvasGO = new GameObject("UICanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        UnityEngine.UI.CanvasScaler scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Add EventSystem if not present
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        UIManager uiManager = canvasGO.AddComponent<UIManager>();

        // --- HUD Panel ---
        GameObject hudPanel = CreatePanel(canvasGO.transform, "HUDPanel", new Color(0, 0, 0, 0));

        UnityEngine.UI.Text scoreText = CreateText(hudPanel.transform, "ScoreText", "SCORE: 0",
            new Vector2(20, -20), new Vector2(300, 40), TextAnchor.UpperLeft, 24);

        UnityEngine.UI.Text livesText = CreateText(hudPanel.transform, "LivesText", "LIVES: 3",
            new Vector2(20, -60), new Vector2(200, 40), TextAnchor.UpperLeft, 22);

        UnityEngine.UI.Text waveText = CreateText(hudPanel.transform, "WaveText", "WAVE 1",
            new Vector2(-20, -20), new Vector2(200, 40), TextAnchor.UpperRight, 22);
        RectTransform waveRT = waveText.GetComponent<RectTransform>();
        waveRT.anchorMin = new Vector2(1, 1);
        waveRT.anchorMax = new Vector2(1, 1);
        waveRT.pivot = new Vector2(1, 1);

        // Health bar
        GameObject healthBarBG = CreateUIElement(hudPanel.transform, "HealthBarBG",
            new Vector2(20, -100), new Vector2(200, 20), new Color(0.3f, 0.3f, 0.3f, 0.8f));
        RectTransform hbRT = healthBarBG.GetComponent<RectTransform>();
        hbRT.anchorMin = new Vector2(0, 1);
        hbRT.anchorMax = new Vector2(0, 1);
        hbRT.pivot = new Vector2(0, 1);

        GameObject healthBarFill = CreateUIElement(healthBarBG.transform, "HealthBarFill",
            Vector2.zero, Vector2.zero, Color.green);
        RectTransform fillRT = healthBarFill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        UnityEngine.UI.Image fillImg = healthBarFill.GetComponent<UnityEngine.UI.Image>();
        fillImg.type = UnityEngine.UI.Image.Type.Filled;
        fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;

        // Shield icon
        GameObject shieldIcon = CreateUIElement(hudPanel.transform, "ShieldIcon",
            new Vector2(20, -130), new Vector2(30, 30), new Color(0.3f, 0.7f, 1f, 0.9f));
        RectTransform siRT = shieldIcon.GetComponent<RectTransform>();
        siRT.anchorMin = new Vector2(0, 1);
        siRT.anchorMax = new Vector2(0, 1);
        siRT.pivot = new Vector2(0, 1);
        UnityEngine.UI.Text shieldLabel = CreateText(shieldIcon.transform, "ShieldLabel", "S",
            Vector2.zero, new Vector2(30, 30), TextAnchor.MiddleCenter, 18);
        shieldIcon.SetActive(false);

        // Wave announcement
        UnityEngine.UI.Text waveAnnouncement = CreateText(hudPanel.transform, "WaveAnnouncement", "",
            Vector2.zero, new Vector2(400, 80), TextAnchor.MiddleCenter, 48);
        RectTransform waRT = waveAnnouncement.GetComponent<RectTransform>();
        waRT.anchorMin = new Vector2(0.5f, 0.5f);
        waRT.anchorMax = new Vector2(0.5f, 0.5f);
        waRT.pivot = new Vector2(0.5f, 0.5f);
        waveAnnouncement.gameObject.SetActive(false);

        // --- Main Menu Panel ---
        GameObject mainMenuPanel = CreatePanel(canvasGO.transform, "MainMenuPanel", new Color(0, 0, 0, 0.85f));

        UnityEngine.UI.Text titleText = CreateText(mainMenuPanel.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0, 150), new Vector2(600, 80), TextAnchor.MiddleCenter, 64);
        RectTransform ttRT = titleText.GetComponent<RectTransform>();
        ttRT.anchorMin = new Vector2(0.5f, 0.5f);
        ttRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleText.color = new Color(0.3f, 0.8f, 1f);

        UnityEngine.UI.Text subtitleText = CreateText(mainMenuPanel.transform, "SubtitleText", "Defend the Galaxy!",
            new Vector2(0, 90), new Vector2(400, 40), TextAnchor.MiddleCenter, 24);
        RectTransform stRT = subtitleText.GetComponent<RectTransform>();
        stRT.anchorMin = new Vector2(0.5f, 0.5f);
        stRT.anchorMax = new Vector2(0.5f, 0.5f);

        CreateButton(mainMenuPanel.transform, "StartButton", "START GAME",
            new Vector2(0, 0), new Vector2(250, 50), () => uiManager.OnStartButtonClicked());

        CreateButton(mainMenuPanel.transform, "QuitButton", "QUIT",
            new Vector2(0, -70), new Vector2(250, 50), () => uiManager.OnQuitButtonClicked());

        UnityEngine.UI.Text highScoreMenuText = CreateText(mainMenuPanel.transform, "HighScoreMenuText", "HIGH SCORE: 0",
            new Vector2(0, -150), new Vector2(400, 40), TextAnchor.MiddleCenter, 22);
        RectTransform hsRT = highScoreMenuText.GetComponent<RectTransform>();
        hsRT.anchorMin = new Vector2(0.5f, 0.5f);
        hsRT.anchorMax = new Vector2(0.5f, 0.5f);
        highScoreMenuText.color = new Color(1f, 0.8f, 0.2f);

        UnityEngine.UI.Text controlsText = CreateText(mainMenuPanel.transform, "ControlsText",
            "WASD / Arrow Keys: Move\nSpace: Shoot\nESC: Pause",
            new Vector2(0, -220), new Vector2(400, 80), TextAnchor.MiddleCenter, 18);
        RectTransform ctRT = controlsText.GetComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0.5f, 0.5f);
        ctRT.anchorMax = new Vector2(0.5f, 0.5f);
        controlsText.color = new Color(0.7f, 0.7f, 0.7f);

        // --- Pause Menu Panel ---
        GameObject pauseMenuPanel = CreatePanel(canvasGO.transform, "PauseMenuPanel", new Color(0, 0, 0, 0.7f));

        CreateText(pauseMenuPanel.transform, "PauseTitle", "PAUSED",
            new Vector2(0, 80), new Vector2(300, 60), TextAnchor.MiddleCenter, 48);

        CreateButton(pauseMenuPanel.transform, "ResumeButton", "RESUME",
            new Vector2(0, 0), new Vector2(250, 50), () => uiManager.OnResumeButtonClicked());

        CreateButton(pauseMenuPanel.transform, "MainMenuButton", "MAIN MENU",
            new Vector2(0, -70), new Vector2(250, 50), () => uiManager.OnMainMenuButtonClicked());

        pauseMenuPanel.SetActive(false);

        // --- Game Over Panel ---
        GameObject gameOverPanel = CreatePanel(canvasGO.transform, "GameOverPanel", new Color(0, 0, 0, 0.85f));

        CreateText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0, 120), new Vector2(500, 80), TextAnchor.MiddleCenter, 56);

        UnityEngine.UI.Text finalScoreText = CreateText(gameOverPanel.transform, "FinalScoreText", "SCORE: 0",
            new Vector2(0, 40), new Vector2(400, 40), TextAnchor.MiddleCenter, 32);
        RectTransform fsRT = finalScoreText.GetComponent<RectTransform>();
        fsRT.anchorMin = new Vector2(0.5f, 0.5f);
        fsRT.anchorMax = new Vector2(0.5f, 0.5f);

        UnityEngine.UI.Text highScoreGOText = CreateText(gameOverPanel.transform, "HighScoreText", "HIGH SCORE: 0",
            new Vector2(0, -10), new Vector2(400, 40), TextAnchor.MiddleCenter, 24);
        RectTransform hsgoRT = highScoreGOText.GetComponent<RectTransform>();
        hsgoRT.anchorMin = new Vector2(0.5f, 0.5f);
        hsgoRT.anchorMax = new Vector2(0.5f, 0.5f);
        highScoreGOText.color = new Color(1f, 0.8f, 0.2f);

        CreateButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -70), new Vector2(250, 50), () => uiManager.OnRestartButtonClicked());

        CreateButton(gameOverPanel.transform, "MainMenuButton2", "MAIN MENU",
            new Vector2(0, -140), new Vector2(250, 50), () => uiManager.OnMainMenuButtonClicked());

        gameOverPanel.SetActive(false);

        // --- Assign references to UIManager ---
        SetPrivateField(uiManager, "hudPanel", hudPanel);
        SetPrivateField(uiManager, "scoreText", scoreText);
        SetPrivateField(uiManager, "livesText", livesText);
        SetPrivateField(uiManager, "waveText", waveText);
        SetPrivateField(uiManager, "healthBarFill", fillImg);
        SetPrivateField(uiManager, "shieldIcon", shieldIcon);
        SetPrivateField(uiManager, "waveAnnouncementText", waveAnnouncement);
        SetPrivateField(uiManager, "mainMenuPanel", mainMenuPanel);
        SetPrivateField(uiManager, "titleText", titleText);
        SetPrivateField(uiManager, "highScoreMenuText", highScoreMenuText);
        SetPrivateField(uiManager, "pauseMenuPanel", pauseMenuPanel);
        SetPrivateField(uiManager, "gameOverPanel", gameOverPanel);
        SetPrivateField(uiManager, "finalScoreText", finalScoreText);
        SetPrivateField(uiManager, "highScoreText", highScoreGOText);
    }

    // --- Helper Methods ---

    private GameObject CreateBulletPrefab(bool isPlayer)
    {
        GameObject go = new GameObject(isPlayer ? "PlayerBullet" : "EnemyBullet");
        go.SetActive(false); // Template

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBulletSprite(isPlayer);
        sr.sortingOrder = 3;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.3f, 0.5f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        go.AddComponent<Bullet>();

        // Move off screen so the template isn't visible
        go.transform.position = new Vector3(100, 100, 0);
        go.SetActive(true);

        return go;
    }

    private GameObject CreateEnemyPrefab<T>(string name, Sprite sprite) where T : EnemyBase
    {
        GameObject go = new GameObject(name);
        go.SetActive(false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        go.AddComponent<T>();

        go.transform.position = new Vector3(100, 100, 0);
        go.SetActive(true);

        return go;
    }

    private GameObject CreatePowerUpPrefab()
    {
        GameObject go = new GameObject("PowerUp");
        go.SetActive(false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePowerUpSprite();
        sr.sortingOrder = 3;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        go.AddComponent<PowerUp>();

        go.transform.position = new Vector3(100, 100, 0);
        go.SetActive(true);

        return go;
    }

    private void AssignBulletToEnemy<T>(GameObject enemyPrefab, GameObject bulletPrefab) where T : EnemyBase
    {
        if (enemyPrefab == null) return;
        // Use reflection to assign bulletPrefab
        var field = typeof(T).GetField("bulletPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            T comp = enemyPrefab.GetComponent<T>();
            if (comp != null)
            {
                field.SetValue(comp, bulletPrefab);
            }
        }
    }

    private GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        UnityEngine.UI.Image img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = bgColor;

        return panel;
    }

    private GameObject CreateUIElement(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = color;

        return go;
    }

    private UnityEngine.UI.Text CreateText(Transform parent, string name, string content,
        Vector2 position, Vector2 size, TextAnchor alignment, int fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        UnityEngine.UI.Text text = go.AddComponent<UnityEngine.UI.Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return text;
    }

    private void CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.15f, 0.15f, 0.3f, 0.9f);

        UnityEngine.UI.Button btn = go.AddComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = img;

        // Button colors
        var colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.3f, 0.9f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.5f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.2f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);

        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        UnityEngine.UI.Text text = labelGO.AddComponent<UnityEngine.UI.Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    /// <summary>
    /// Helper to set serialized private fields via reflection.
    /// </summary>
    private void SetPrivateField(object target, string fieldName, object value)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }
            type = type.BaseType;
        }
        Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
    }
}
