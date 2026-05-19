using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically builds the entire GamePlay scene at runtime.
/// Attach to a single empty GameObject in the GamePlay scene.
/// Creates: Player, Background, Spawners, UI Canvas, Camera setup.
/// </summary>
public class GamePlaySetup : MonoBehaviour
{
    private void Awake()
    {
        SetupCamera();
        CreateBackground();
        CreatePlayer();
        CreateSpawners();
        CreateUI();
        CreatePrefabReferences();
    }

    private void Start()
    {
        // Start music if not playing
        AudioManager.Instance?.PlayMusic();
    }

    // ────────────────────────────── CAMERA ──────────────────────────────

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f); // Deep space
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
        }
    }

    // ────────────────────────────── BACKGROUND ──────────────────────────

    private void CreateBackground()
    {
        GameObject bg = new GameObject("ParallaxBackground");
        bg.AddComponent<ParallaxBackground>();
    }

    // ────────────────────────────── PLAYER ──────────────────────────────

    private void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");
        player.transform.position = new Vector3(0, -3.5f, 0);

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePlayerShipSprite();
        sr.sortingOrder = 5;

        // Physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Collider
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.25f, 0.35f);
        col.isTrigger = true;

        // Controller
        PlayerController pc = player.AddComponent<PlayerController>();

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.parent = player.transform;
        firePoint.transform.localPosition = new Vector3(0, 0.3f, 0);

        // Shield visual
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.parent = player.transform;
        shield.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSr = shield.AddComponent<SpriteRenderer>();
        shieldSr.sprite = SpriteGenerator.CreateShieldSprite();
        shieldSr.sortingOrder = 6;
        shield.transform.localScale = Vector3.one * 1.5f;
        shield.SetActive(false);

        // Set references via reflection-safe method
        SetPrivateField(pc, "firePoint", firePoint.transform);
        SetPrivateField(pc, "shieldVisual", shield);
        SetPrivateField(pc, "spriteRenderer", sr);

        // Create bullet prefab for player
        GameObject bulletPrefab = CreatePlayerBulletPrefab();
        SetPrivateField(pc, "bulletPrefab", bulletPrefab);
    }

    private GameObject CreatePlayerBulletPrefab()
    {
        GameObject bullet = new GameObject("PlayerBullet");
        bullet.SetActive(false); // Prefab template

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBulletSprite(new Color(0.3f, 0.8f, 1f));
        sr.sortingOrder = 4;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.06f, 0.14f);
        col.isTrigger = true;

        Bullet b = bullet.AddComponent<Bullet>();

        // Store as a resource-like prefab
        bullet.transform.position = new Vector3(0, -100, 0);
        DontDestroyOnLoad(bullet);
        bullet.SetActive(true);
        bullet.SetActive(false); // Keep deactivated

        return bullet;
    }

    // ────────────────────────────── SPAWNERS ────────────────────────────

    private void CreateSpawners()
    {
        // Enemy Spawner
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();

        // Create enemy prefabs
        GameObject dronePrefab = CreateEnemyPrefab("Drone", SpriteGenerator.CreateEnemyDroneSprite(), typeof(EnemyDrone));
        GameObject fighterPrefab = CreateEnemyPrefab("Fighter", SpriteGenerator.CreateEnemyFighterSprite(), typeof(EnemyFighter));
        GameObject bomberPrefab = CreateEnemyPrefab("Bomber", SpriteGenerator.CreateEnemyBomberSprite(), typeof(EnemyBomber));
        GameObject swooperPrefab = CreateEnemyPrefab("Swooper", SpriteGenerator.CreateEnemySwooperSprite(), typeof(EnemySwooper));

        // Create enemy bullet prefab
        GameObject enemyBulletPrefab = CreateEnemyBulletPrefab();

        // Set enemy bullet prefab on each enemy prefab
        SetEnemyBulletPrefab(dronePrefab, enemyBulletPrefab);
        SetEnemyBulletPrefab(fighterPrefab, enemyBulletPrefab);
        SetEnemyBulletPrefab(bomberPrefab, enemyBulletPrefab);
        SetEnemyBulletPrefab(swooperPrefab, enemyBulletPrefab);

        SetPrivateField(spawner, "dronePrefab", dronePrefab);
        SetPrivateField(spawner, "fighterPrefab", fighterPrefab);
        SetPrivateField(spawner, "bomberPrefab", bomberPrefab);
        SetPrivateField(spawner, "swooperPrefab", swooperPrefab);

        // Power-up Spawner
        GameObject puSpawnerObj = new GameObject("PowerUpSpawner");
        PowerUpSpawner puSpawner = puSpawnerObj.AddComponent<PowerUpSpawner>();
        GameObject puPrefab = CreatePowerUpPrefab();
        SetPrivateField(puSpawner, "powerUpPrefab", puPrefab);
    }

    private GameObject CreateEnemyPrefab(string name, Sprite sprite, System.Type enemyType)
    {
        GameObject enemy = new GameObject($"Enemy_{name}");
        enemy.SetActive(false);

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.25f, 0.25f);
        col.isTrigger = true;

        enemy.AddComponent(enemyType);

        enemy.transform.position = new Vector3(0, -100, 0);
        DontDestroyOnLoad(enemy);

        return enemy;
    }

    private void SetEnemyBulletPrefab(GameObject enemyPrefab, GameObject bulletPrefab)
    {
        EnemyBase eb = enemyPrefab.GetComponent<EnemyBase>();
        if (eb != null)
        {
            SetPrivateField(eb, "bulletPrefab", bulletPrefab);
        }
    }

    private GameObject CreateEnemyBulletPrefab()
    {
        GameObject bullet = new GameObject("EnemyBullet");
        bullet.SetActive(false);

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBulletSprite(new Color(1f, 0.3f, 0.2f));
        sr.sortingOrder = 4;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.06f, 0.14f);
        col.isTrigger = true;

        bullet.AddComponent<Bullet>();

        bullet.transform.position = new Vector3(0, -100, 0);
        DontDestroyOnLoad(bullet);

        return bullet;
    }

    private GameObject CreatePowerUpPrefab()
    {
        GameObject pu = new GameObject("PowerUp");
        pu.SetActive(false);

        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePowerUpSprite();
        sr.sortingOrder = 4;

        CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
        col.radius = 0.12f;
        col.isTrigger = true;

        pu.AddComponent<PowerUp>();

        pu.transform.position = new Vector3(0, -100, 0);
        DontDestroyOnLoad(pu);

        return pu;
    }

    // ────────────────────────────── UI ──────────────────────────────────

    private void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // UI Manager
        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // ─── HUD ───
        // Score Text
        Text scoreText = CreateUIText(canvasObj.transform, "ScoreText",
            "SCORE: 0", TextAnchor.UpperLeft, new Vector2(20, -20),
            new Vector2(300, 40), 28, Color.white);

        // Combo Text
        Text comboText = CreateUIText(canvasObj.transform, "ComboText",
            "", TextAnchor.UpperLeft, new Vector2(20, -60),
            new Vector2(200, 30), 22, Color.yellow);
        comboText.gameObject.SetActive(false);

        // Wave Text
        Text waveText = CreateUIText(canvasObj.transform, "WaveText",
            "WAVE 1", TextAnchor.UpperRight, new Vector2(-20, -20),
            new Vector2(200, 40), 28, Color.cyan);

        // Health Bar Background
        GameObject healthBg = CreateUIImage(canvasObj.transform, "HealthBarBG",
            new Color(0.2f, 0.2f, 0.2f, 0.8f), new Vector2(0, -30),
            new Vector2(200, 20));
        RectTransform healthBgRT = healthBg.GetComponent<RectTransform>();
        healthBgRT.anchorMin = new Vector2(0.5f, 1f);
        healthBgRT.anchorMax = new Vector2(0.5f, 1f);
        healthBgRT.pivot = new Vector2(0.5f, 1f);

        // Health Bar Fill
        GameObject healthFill = CreateUIImage(healthBg.transform, "HealthBarFill",
            Color.green, Vector2.zero, new Vector2(200, 20));
        Image healthBarImage = healthFill.GetComponent<Image>();
        healthBarImage.type = Image.Type.Filled;
        healthBarImage.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRT = healthFill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // Health Text
        Text healthText = CreateUIText(healthBg.transform, "HealthText",
            "100%", TextAnchor.MiddleCenter, Vector2.zero,
            new Vector2(200, 20), 14, Color.white);
        RectTransform htRT = healthText.GetComponent<RectTransform>();
        htRT.anchorMin = Vector2.zero;
        htRT.anchorMax = Vector2.one;
        htRT.offsetMin = Vector2.zero;
        htRT.offsetMax = Vector2.zero;

        // ─── Wave Announcement ───
        GameObject waveAnnounce = new GameObject("WaveAnnouncement");
        waveAnnounce.transform.SetParent(canvasObj.transform, false);
        RectTransform waRT = waveAnnounce.AddComponent<RectTransform>();
        waRT.anchorMin = new Vector2(0.5f, 0.5f);
        waRT.anchorMax = new Vector2(0.5f, 0.5f);
        waRT.anchoredPosition = new Vector2(0, 50);
        waRT.sizeDelta = new Vector2(500, 80);

        Image waBg = waveAnnounce.AddComponent<Image>();
        waBg.color = new Color(0, 0, 0, 0.6f);

        Text waText = CreateUIText(waveAnnounce.transform, "WaveAnnouncementText",
            "- WAVE 1 -", TextAnchor.MiddleCenter, Vector2.zero,
            new Vector2(500, 80), 42, Color.cyan);
        RectTransform watRT = waText.GetComponent<RectTransform>();
        watRT.anchorMin = Vector2.zero;
        watRT.anchorMax = Vector2.one;
        watRT.offsetMin = Vector2.zero;
        watRT.offsetMax = Vector2.zero;

        waveAnnounce.SetActive(false);

        // ─── Pause Menu ───
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PauseMenu",
            new Color(0, 0, 0, 0.7f));

        CreateUIText(pausePanel.transform, "PausedTitle",
            "PAUSED", TextAnchor.MiddleCenter, new Vector2(0, 100),
            new Vector2(400, 60), 48, Color.white);

        Button resumeBtn = CreateButton(pausePanel.transform, "ResumeButton",
            "RESUME", new Vector2(0, 20), new Vector2(250, 50));
        Button restartBtnP = CreateButton(pausePanel.transform, "RestartButton",
            "RESTART", new Vector2(0, -40), new Vector2(250, 50));
        Button menuBtnP = CreateButton(pausePanel.transform, "MainMenuButton",
            "MAIN MENU", new Vector2(0, -100), new Vector2(250, 50));

        pausePanel.SetActive(false);

        // ─── Game Over Panel ───
        GameObject goPanel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0.1f, 0, 0, 0.85f));

        CreateUIText(goPanel.transform, "GameOverTitle",
            "GAME OVER", TextAnchor.MiddleCenter, new Vector2(0, 140),
            new Vector2(500, 70), 56, new Color(1f, 0.3f, 0.3f));

        Text goScore = CreateUIText(goPanel.transform, "GameOverScore",
            "SCORE: 0", TextAnchor.MiddleCenter, new Vector2(0, 70),
            new Vector2(400, 40), 32, Color.white);

        Text goHighScore = CreateUIText(goPanel.transform, "GameOverHighScore",
            "HIGH SCORE: 0", TextAnchor.MiddleCenter, new Vector2(0, 30),
            new Vector2(400, 35), 26, Color.yellow);

        Text goWave = CreateUIText(goPanel.transform, "GameOverWave",
            "REACHED WAVE 1", TextAnchor.MiddleCenter, new Vector2(0, -10),
            new Vector2(400, 35), 24, Color.cyan);

        Button restartBtnGO = CreateButton(goPanel.transform, "RestartButton",
            "PLAY AGAIN", new Vector2(0, -70), new Vector2(250, 50));
        Button menuBtnGO = CreateButton(goPanel.transform, "MainMenuButton",
            "MAIN MENU", new Vector2(0, -130), new Vector2(250, 50));

        goPanel.SetActive(false);

        // ─── Wire up UIManager references ───
        SetPrivateField(uiManager, "scoreText", scoreText);
        SetPrivateField(uiManager, "comboText", comboText);
        SetPrivateField(uiManager, "waveText", waveText);
        SetPrivateField(uiManager, "healthBar", healthBarImage);
        SetPrivateField(uiManager, "healthText", healthText);
        SetPrivateField(uiManager, "waveAnnouncement", waveAnnounce);
        SetPrivateField(uiManager, "waveAnnouncementText", waText);
        SetPrivateField(uiManager, "pauseMenuPanel", pausePanel);
        SetPrivateField(uiManager, "gameOverPanel", goPanel);
        SetPrivateField(uiManager, "gameOverScoreText", goScore);
        SetPrivateField(uiManager, "gameOverHighScoreText", goHighScore);
        SetPrivateField(uiManager, "gameOverWaveText", goWave);

        // ─── Wire up Button Callbacks ───
        resumeBtn.onClick.AddListener(() => uiManager.OnResumeClicked());
        restartBtnP.onClick.AddListener(() => uiManager.OnRestartClicked());
        menuBtnP.onClick.AddListener(() => uiManager.OnMainMenuClicked());
        restartBtnGO.onClick.AddListener(() => uiManager.OnRestartClicked());
        menuBtnGO.onClick.AddListener(() => uiManager.OnMainMenuClicked());
    }

    // ────────────────────────────── UI HELPERS ──────────────────────────

    private Text CreateUIText(Transform parent, string name, string content,
        TextAnchor alignment, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(
            alignment == TextAnchor.UpperLeft || alignment == TextAnchor.MiddleLeft ? 0 :
            alignment == TextAnchor.UpperRight || alignment == TextAnchor.MiddleRight ? 1 : 0.5f,
            alignment == TextAnchor.UpperLeft || alignment == TextAnchor.UpperCenter || alignment == TextAnchor.UpperRight ? 1 :
            alignment == TextAnchor.LowerLeft || alignment == TextAnchor.LowerCenter ? 0 : 0.5f
        );
        rt.anchorMax = rt.anchorMin;
        rt.pivot = rt.anchorMin;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        // Add outline for readability
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(1, -1);

        return text;
    }

    private GameObject CreateUIImage(Transform parent, string name, Color color,
        Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        return obj;
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

        Image bg = panel.AddComponent<Image>();
        bg.color = bgColor;

        return panel;
    }

    private Button CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.3f, 0.5f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.3f, 0.5f, 0.9f);
        colors.highlightedColor = new Color(0.2f, 0.5f, 0.8f, 1f);
        colors.pressedColor = new Color(0.1f, 0.2f, 0.4f, 1f);
        btn.colors = colors;

        // Button text
        Text text = CreateUIText(btnObj.transform, "Text", label,
            TextAnchor.MiddleCenter, Vector2.zero, size, 22, Color.white);
        RectTransform textRT = text.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btn;
    }

    // ────────────────────────────── PREFAB REFERENCES ───────────────────

    private void CreatePrefabReferences()
    {
        // Ensure persistent managers exist
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
        if (ScoreManager.Instance == null)
        {
            GameObject sm = new GameObject("ScoreManager");
            sm.AddComponent<ScoreManager>();
        }
        if (AudioManager.Instance == null)
        {
            GameObject am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
        }
    }

    // ────────────────────────────── UTILITY ─────────────────────────────

    /// <summary>
    /// Sets a serialized private field using reflection.
    /// Used to wire up references that would normally be set in the Inspector.
    /// </summary>
    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.FlattenHierarchy);

        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            // Try base type for inherited fields
            var type = target.GetType().BaseType;
            while (type != null)
            {
                field = type.GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
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
}
