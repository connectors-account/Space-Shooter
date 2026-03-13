using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically sets up the entire game scene at runtime including:
/// - Camera, player, UI, managers, enemy prefabs, etc.
/// This script generates everything procedurally so the game works
/// without pre-made assets. Attach to a single empty GameObject in GameScene.
/// </summary>
public class GameSceneSetup : MonoBehaviour
{
    // Generated prefab references
    private GameObject playerBulletPrefab;
    private GameObject enemyBulletPrefab;
    private GameObject enemyStraightPrefab;
    private GameObject enemyZigzagPrefab;
    private GameObject enemyDiverPrefab;
    private GameObject bossPrefab;
    private GameObject weaponUpgradePrefab;
    private GameObject shieldPrefab;
    private GameObject healthPrefab;

    private void Awake()
    {
        SetupCamera();
        CreatePrefabs();
        CreatePlayer();
        CreateManagers();
        CreateBackground();
        CreateUI();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.transform.position = new Vector3(0, 0, -10);

        // Add GameBounds
        cam.gameObject.AddComponent<GameBounds>();
    }

    private void CreatePrefabs()
    {
        // Player Bullet prefab
        playerBulletPrefab = CreateBulletPrefab("PlayerBullet", new Color(0.2f, 1f, 0.4f), 12f, 10, true);

        // Enemy Bullet prefab
        enemyBulletPrefab = CreateBulletPrefab("EnemyBullet", new Color(1f, 0.2f, 0.2f), 8f, 10, false);

        // Enemy prefabs
        enemyStraightPrefab = CreateEnemyPrefab("EnemyStraight", new Color(1f, 0.3f, 0.3f), typeof(EnemyStraight));
        enemyZigzagPrefab = CreateEnemyPrefab("EnemyZigzag", new Color(1f, 0.6f, 0.1f), typeof(EnemyZigzag));
        enemyDiverPrefab = CreateEnemyPrefab("EnemyDiver", new Color(0.8f, 0.2f, 0.8f), typeof(EnemyDiver));
        bossPrefab = CreateBossPrefab();

        // Power-up prefabs
        weaponUpgradePrefab = CreatePowerUpPrefab("WeaponUpgrade", PowerUp.PowerUpType.WeaponUpgrade);
        shieldPrefab = CreatePowerUpPrefab("Shield", PowerUp.PowerUpType.Shield);
        healthPrefab = CreatePowerUpPrefab("Health", PowerUp.PowerUpType.Health);

        // Hide prefabs
        playerBulletPrefab.SetActive(false);
        enemyBulletPrefab.SetActive(false);
        enemyStraightPrefab.SetActive(false);
        enemyZigzagPrefab.SetActive(false);
        enemyDiverPrefab.SetActive(false);
        bossPrefab.SetActive(false);
        weaponUpgradePrefab.SetActive(false);
        shieldPrefab.SetActive(false);
        healthPrefab.SetActive(false);
    }

    private GameObject CreateBulletPrefab(string name, Color color, float speed, int damage, bool isPlayer)
    {
        GameObject bullet = new GameObject(name);
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateRectSprite(4, 12, color);
        sr.sortingLayerName = "Bullets";
        bullet.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(4f, 12f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        Bullet b = bullet.AddComponent<Bullet>();
        b.speed = speed;
        b.damage = damage;

        bullet.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";
        bullet.layer = LayerMask.NameToLayer(isPlayer ? "PlayerBullet" : "EnemyBullet");

        return bullet;
    }

    private GameObject CreateEnemyPrefab(string name, Color color, System.Type enemyType)
    {
        GameObject enemy = new GameObject(name);
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateTriangleSprite(32, color, true);
        sr.sortingLayerName = "Foreground";
        enemy.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        PolygonCollider2D col = enemy.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        EnemyBase eb = (EnemyBase)enemy.AddComponent(enemyType);
        eb.bulletPrefab = enemyBulletPrefab;

        // Create fire point
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(enemy.transform);
        fp.transform.localPosition = new Vector3(0, -0.5f, 0);
        eb.firePoint = fp.transform;

        return enemy;
    }

    private GameObject CreateBossPrefab()
    {
        GameObject boss = new GameObject("Boss");
        SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateHexagonSprite(48, new Color(0.8f, 0f, 0f));
        sr.sortingLayerName = "Foreground";
        boss.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        PolygonCollider2D col = boss.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        EnemyBoss eb = boss.AddComponent<EnemyBoss>();
        eb.bulletPrefab = enemyBulletPrefab;

        // Create fire point
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(boss.transform);
        fp.transform.localPosition = new Vector3(0, -0.8f, 0);
        eb.firePoint = fp.transform;

        return boss;
    }

    private GameObject CreatePowerUpPrefab(string name, PowerUp.PowerUpType type)
    {
        GameObject pu = new GameObject("PowerUp_" + name);
        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateDiamondSprite(16, Color.white);
        sr.sortingLayerName = "Foreground";
        sr.sortingOrder = 5;
        pu.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 8f;

        Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        PowerUp p = pu.AddComponent<PowerUp>();
        p.type = type;

        pu.tag = "PowerUp";

        return pu;
    }

    private void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        player.transform.position = new Vector3(0, -3f, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePlayerShipSprite(32, new Color(0.3f, 0.7f, 1f));
        sr.sortingLayerName = "Player";
        player.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        PolygonCollider2D col = player.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.bulletPrefab = playerBulletPrefab;

        // Create fire points
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(player.transform);
        fp.transform.localPosition = new Vector3(0, 0.6f, 0);
        pc.firePoint = fp.transform;

        GameObject fpL = new GameObject("FirePointLeft");
        fpL.transform.SetParent(player.transform);
        fpL.transform.localPosition = new Vector3(-0.3f, 0.4f, 0);
        pc.firePointLeft = fpL.transform;

        GameObject fpR = new GameObject("FirePointRight");
        fpR.transform.SetParent(player.transform);
        fpR.transform.localPosition = new Vector3(0.3f, 0.4f, 0);
        pc.firePointRight = fpR.transform;

        // Shield visual
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSR = shield.AddComponent<SpriteRenderer>();
        shieldSR.sprite = SpriteGenerator.CreateCircleSprite(48, new Color(0.3f, 0.6f, 1f, 0.3f));
        shieldSR.sortingLayerName = "Player";
        shieldSR.sortingOrder = 1;
        shield.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        shield.SetActive(false);
        pc.shieldVisual = shield;

        // Engine glow
        GameObject engineGlow = new GameObject("EngineGlow");
        engineGlow.transform.SetParent(player.transform);
        engineGlow.transform.localPosition = new Vector3(0, -0.5f, 0);
        SpriteRenderer engineSR = engineGlow.AddComponent<SpriteRenderer>();
        engineSR.sprite = SpriteGenerator.CreateCircleSprite(16, new Color(0.2f, 0.5f, 1f, 0.5f));
        engineSR.sortingLayerName = "Player";
        engineSR.sortingOrder = -1;
        engineGlow.transform.localScale = new Vector3(0.3f, 0.5f, 1f);
        engineGlow.AddComponent<EngineGlowEffect>();
    }

    private void CreateManagers()
    {
        // Effects Manager
        GameObject effectsObj = new GameObject("EffectsManager");
        effectsObj.AddComponent<EffectsManager>();

        // Wave Manager
        GameObject waveObj = new GameObject("WaveManager");
        WaveManager wm = waveObj.AddComponent<WaveManager>();
        wm.enemyStraightPrefab = enemyStraightPrefab;
        wm.enemyZigzagPrefab = enemyZigzagPrefab;
        wm.enemyDiverPrefab = enemyDiverPrefab;
        wm.bossPrefab = bossPrefab;

        // Power-Up Spawner
        GameObject puSpawner = new GameObject("PowerUpSpawner");
        PowerUpSpawner pus = puSpawner.AddComponent<PowerUpSpawner>();
        pus.weaponUpgradePrefab = weaponUpgradePrefab;
        pus.shieldPrefab = shieldPrefab;
        pus.healthPrefab = healthPrefab;
    }

    private void CreateBackground()
    {
        GameObject bgObj = new GameObject("Starfield");
        bgObj.AddComponent<StarfieldGenerator>();
    }

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

        // HUD
        CreateHUD(canvasObj.transform);

        // Pause Menu
        CreatePauseMenu(canvasObj.transform);

        // Game Over
        CreateGameOverScreen(canvasObj.transform);
    }

    private void CreateHUD(Transform parent)
    {
        GameObject hudObj = new GameObject("HUD");
        hudObj.transform.SetParent(parent, false);
        GameHUD hud = hudObj.AddComponent<GameHUD>();

        // Score text (top left)
        hud.scoreText = CreateUIText(hudObj.transform, "ScoreText", "SCORE: 0",
            new Vector2(20, -20), new Vector2(300, 40), TextAnchor.UpperLeft,
            24, Color.white);

        // High score (top center)
        hud.highScoreText = CreateUIText(hudObj.transform, "HighScoreText", "HI: 0",
            new Vector2(0, -20), new Vector2(300, 40), TextAnchor.UpperCenter,
            20, new Color(0.7f, 0.7f, 0.7f));
        RectTransform hiRT = hud.highScoreText.GetComponent<RectTransform>();
        hiRT.anchorMin = new Vector2(0.5f, 1f);
        hiRT.anchorMax = new Vector2(0.5f, 1f);
        hiRT.pivot = new Vector2(0.5f, 1f);

        // Lives text (top right)
        hud.livesText = CreateUIText(hudObj.transform, "LivesText", "LIVES: 3",
            new Vector2(-20, -20), new Vector2(200, 40), TextAnchor.UpperRight,
            24, Color.white);
        RectTransform livesRT = hud.livesText.GetComponent<RectTransform>();
        livesRT.anchorMin = new Vector2(1f, 1f);
        livesRT.anchorMax = new Vector2(1f, 1f);
        livesRT.pivot = new Vector2(1f, 1f);

        // Wave text (top right, below lives)
        hud.waveText = CreateUIText(hudObj.transform, "WaveText", "WAVE 1",
            new Vector2(-20, -60), new Vector2(200, 40), TextAnchor.UpperRight,
            20, new Color(0.5f, 0.8f, 1f));
        RectTransform waveRT = hud.waveText.GetComponent<RectTransform>();
        waveRT.anchorMin = new Vector2(1f, 1f);
        waveRT.anchorMax = new Vector2(1f, 1f);
        waveRT.pivot = new Vector2(1f, 1f);

        // Combo text (center)
        hud.comboText = CreateUIText(hudObj.transform, "ComboText", "",
            new Vector2(0, 100), new Vector2(300, 50), TextAnchor.MiddleCenter,
            30, Color.yellow);
        RectTransform comboRT = hud.comboText.GetComponent<RectTransform>();
        comboRT.anchorMin = new Vector2(0.5f, 0.5f);
        comboRT.anchorMax = new Vector2(0.5f, 0.5f);
        comboRT.pivot = new Vector2(0.5f, 0.5f);

        // Wave announcement (center)
        hud.waveAnnouncementText = CreateUIText(hudObj.transform, "WaveAnnouncement", "",
            new Vector2(0, 50), new Vector2(500, 60), TextAnchor.MiddleCenter,
            40, Color.white);
        RectTransform waRT = hud.waveAnnouncementText.GetComponent<RectTransform>();
        waRT.anchorMin = new Vector2(0.5f, 0.5f);
        waRT.anchorMax = new Vector2(0.5f, 0.5f);
        waRT.pivot = new Vector2(0.5f, 0.5f);

        // Health bar (bottom left)
        CreateHealthBar(hudObj.transform, hud);

        // Boss health panel
        CreateBossHealthBar(hudObj.transform, hud);
    }

    private void CreateHealthBar(Transform parent, GameHUD hud)
    {
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(parent, false);
        RectTransform rt = healthBarObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(20, 20);
        rt.sizeDelta = new Vector2(300, 25);

        // Background
        Image bgImg = healthBarObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Slider
        Slider slider = healthBarObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;

        // Fill area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(healthBarObj.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(2, 2);
        fillAreaRT.offsetMax = new Vector2(-2, -2);

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.green;

        slider.fillRect = fillRT;
        hud.healthBar = slider;
        hud.healthBarFill = fillImg;

        // Label
        CreateUIText(healthBarObj.transform, "HealthLabel", "HP",
            new Vector2(-30, 0), new Vector2(40, 25), TextAnchor.MiddleCenter,
            16, Color.white);
    }

    private void CreateBossHealthBar(Transform parent, GameHUD hud)
    {
        GameObject bossPanel = new GameObject("BossHealthPanel");
        bossPanel.transform.SetParent(parent, false);
        RectTransform rt = bossPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -60);
        rt.sizeDelta = new Vector2(500, 30);
        hud.bossHealthPanel = bossPanel;

        // Boss name
        hud.bossNameText = CreateUIText(bossPanel.transform, "BossName", "BOSS",
            new Vector2(0, 25), new Vector2(200, 25), TextAnchor.MiddleCenter,
            18, Color.red);
        RectTransform bossNameRT = hud.bossNameText.GetComponent<RectTransform>();
        bossNameRT.anchorMin = new Vector2(0.5f, 1f);
        bossNameRT.anchorMax = new Vector2(0.5f, 1f);
        bossNameRT.pivot = new Vector2(0.5f, 0f);

        // Background
        Image bgImg = bossPanel.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.1f, 0.1f, 0.8f);

        // Slider
        Slider slider = bossPanel.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        // Fill area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(bossPanel.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(2, 2);
        fillAreaRT.offsetMax = new Vector2(-2, -2);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.red;

        slider.fillRect = fillRT;
        hud.bossHealthBar = slider;

        bossPanel.SetActive(false);
    }

    private void CreatePauseMenu(Transform parent)
    {
        GameObject pauseObj = new GameObject("PauseMenu");
        pauseObj.transform.SetParent(parent, false);
        PauseMenuUI pauseUI = pauseObj.AddComponent<PauseMenuUI>();

        // Panel
        GameObject panel = CreatePanel(pauseObj.transform, "PausePanel",
            new Color(0, 0, 0, 0.7f), Vector2.zero, new Vector2(400, 350));
        pauseUI.pausePanel = panel;

        // Title
        pauseUI.pauseTitle = CreateUIText(panel.transform, "PauseTitle", "PAUSED",
            new Vector2(0, 120), new Vector2(300, 60), TextAnchor.MiddleCenter,
            40, Color.white);
        CenterAnchors(pauseUI.pauseTitle.GetComponent<RectTransform>());

        // Resume button
        pauseUI.resumeButton = CreateUIButton(panel.transform, "ResumeBtn", "RESUME",
            new Vector2(0, 30), new Vector2(250, 50), new Color(0.2f, 0.6f, 0.2f));
        CenterAnchors(pauseUI.resumeButton.GetComponent<RectTransform>());

        // Restart button
        pauseUI.restartButton = CreateUIButton(panel.transform, "RestartBtn", "RESTART",
            new Vector2(0, -40), new Vector2(250, 50), new Color(0.6f, 0.6f, 0.2f));
        CenterAnchors(pauseUI.restartButton.GetComponent<RectTransform>());

        // Main Menu button
        pauseUI.mainMenuButton = CreateUIButton(panel.transform, "MenuBtn", "MAIN MENU",
            new Vector2(0, -110), new Vector2(250, 50), new Color(0.6f, 0.2f, 0.2f));
        CenterAnchors(pauseUI.mainMenuButton.GetComponent<RectTransform>());

        panel.SetActive(false);
    }

    private void CreateGameOverScreen(Transform parent)
    {
        GameObject goObj = new GameObject("GameOverScreen");
        goObj.transform.SetParent(parent, false);
        GameOverUI goUI = goObj.AddComponent<GameOverUI>();

        // Panel
        GameObject panel = CreatePanel(goObj.transform, "GameOverPanel",
            new Color(0, 0, 0, 0.8f), Vector2.zero, new Vector2(500, 450));
        goUI.gameOverPanel = panel;

        // Title
        goUI.gameOverTitle = CreateUIText(panel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0, 160), new Vector2(400, 70), TextAnchor.MiddleCenter,
            50, Color.red);
        CenterAnchors(goUI.gameOverTitle.GetComponent<RectTransform>());

        // Final score
        goUI.finalScoreText = CreateUIText(panel.transform, "FinalScore", "SCORE: 0",
            new Vector2(0, 80), new Vector2(400, 40), TextAnchor.MiddleCenter,
            30, Color.white);
        CenterAnchors(goUI.finalScoreText.GetComponent<RectTransform>());

        // High score
        goUI.highScoreText = CreateUIText(panel.transform, "HighScore", "HIGH SCORE: 0",
            new Vector2(0, 30), new Vector2(400, 35), TextAnchor.MiddleCenter,
            24, new Color(1f, 0.8f, 0.2f));
        CenterAnchors(goUI.highScoreText.GetComponent<RectTransform>());

        // New high score flash
        goUI.newHighScoreText = CreateUIText(panel.transform, "NewHighScore", "NEW HIGH SCORE!",
            new Vector2(0, -15), new Vector2(400, 35), TextAnchor.MiddleCenter,
            28, new Color(1f, 1f, 0f));
        CenterAnchors(goUI.newHighScoreText.GetComponent<RectTransform>());

        // Restart button
        goUI.restartButton = CreateUIButton(panel.transform, "RestartBtn", "PLAY AGAIN",
            new Vector2(0, -80), new Vector2(250, 50), new Color(0.2f, 0.6f, 0.2f));
        CenterAnchors(goUI.restartButton.GetComponent<RectTransform>());

        // Main Menu button
        goUI.mainMenuButton = CreateUIButton(panel.transform, "MenuBtn", "MAIN MENU",
            new Vector2(0, -150), new Vector2(250, 50), new Color(0.6f, 0.2f, 0.2f));
        CenterAnchors(goUI.mainMenuButton.GetComponent<RectTransform>());

        panel.SetActive(false);
    }

    // UI Helper methods
    private Text CreateUIText(Transform parent, string name, string text,
        Vector2 position, Vector2 size, TextAnchor alignment, int fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null)
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = alignment;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;

        // Add shadow
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(1, -1);

        return txt;
    }

    private Button CreateUIButton(Transform parent, string name, string text,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        Button btn = obj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        btn.colors = colors;

        // Button text
        Text btnText = CreateUIText(obj.transform, "Text", text,
            Vector2.zero, size, TextAnchor.MiddleCenter, 22, Color.white);
        RectTransform textRT = btnText.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btn;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color,
        Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        return obj;
    }

    private void CenterAnchors(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }
}

/// <summary>
/// Simple engine glow pulsing effect for the player ship.
/// </summary>
public class EngineGlowEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    private Vector3 baseScale;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float pulse = 0.8f + Mathf.Sin(Time.time * 10f) * 0.2f;
        transform.localScale = baseScale * pulse;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.3f + Mathf.Sin(Time.time * 8f) * 0.2f;
            sr.color = c;
        }
    }
}
