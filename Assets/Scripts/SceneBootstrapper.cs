using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AUTO-SETUP: This script creates the entire game scene at runtime.
/// Attach this to an empty GameObject named "Bootstrapper" in an empty scene.
/// It creates all game objects, UI, prefab instances, and wires everything up.
/// This eliminates the need for complex prefab/scene files.
/// </summary>
public class SceneBootstrapper : MonoBehaviour
{
    void Awake()
    {
        // Ensure we have a square sprite in Resources
        CreateResourceSprites();

        // Build the scene
        SetupCamera();
        GameObject managers = SetupManagers();
        GameObject player = CreatePlayer();
        GameObject canvas = CreateUI();
        CreatePrefabs(managers, player);
        SetupBackground();

        // Self-destruct after setup
        // Destroy(gameObject); // Keep it for debugging
    }

    void CreateResourceSprites()
    {
        // Square sprite is created dynamically - no file needed
        // Resources.Load<Sprite>("Square") might return null, so objects 
        // that need it will use the fallback CreateSprite method
    }

    void SetupCamera()
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
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f); // Deep space blue-black
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    GameObject SetupManagers()
    {
        // GameManager
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // AudioManager
        GameObject amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();

        return gmObj;
    }

    GameObject CreatePlayer()
    {
        // Player ship - triangle shape using a sprite
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, -4f, 0f);
        player.layer = LayerMask.NameToLayer("Default");

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite();
        sr.color = new Color(0.2f, 0.8f, 1f); // Cyan
        sr.sortingOrder = 5;
        player.transform.localScale = Vector3.one * 0.6f;

        // Collider
        PolygonCollider2D col = player.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        // Rigidbody
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.parent = player.transform;
        firePoint.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        // PlayerController
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.firePoint = firePoint.transform;

        // Engine glow (child object)
        GameObject glow = new GameObject("EngineGlow");
        glow.transform.parent = player.transform;
        glow.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = CreateSquareSprite();
        glowSr.color = new Color(1f, 0.5f, 0f, 0.7f);
        glowSr.sortingOrder = 4;
        glow.transform.localScale = new Vector3(0.3f, 0.4f, 1f);

        return player;
    }

    void CreatePrefabs(GameObject managers, GameObject player)
    {
        GameManager gm = managers.GetComponent<GameManager>();
        PlayerController pc = player.GetComponent<PlayerController>();

        // === Player Bullet Prefab ===
        GameObject bulletPrefab = CreateBulletPrefab("PlayerBullet", new Color(1f, 1f, 0.3f), true);
        bulletPrefab.SetActive(false);

        // === Enemy Bullet Prefab ===
        GameObject enemyBulletPrefab = CreateBulletPrefab("EnemyBullet", new Color(1f, 0.3f, 0.3f), false);
        enemyBulletPrefab.SetActive(false);

        // === Enemy Prefabs ===
        GameObject basicEnemy = CreateEnemyPrefab("BasicEnemy",
            new Color(1f, 0.2f, 0.2f), EnemyController.MovementPattern.Straight,
            false, null, 2, 3f);
        basicEnemy.SetActive(false);

        GameObject zigzagEnemy = CreateEnemyPrefab("ZigzagEnemy",
            new Color(1f, 0.5f, 0f), EnemyController.MovementPattern.Zigzag,
            false, null, 2, 2.5f);
        zigzagEnemy.SetActive(false);

        GameObject shooterEnemy = CreateEnemyPrefab("ShooterEnemy",
            new Color(0.8f, 0.2f, 0.8f), EnemyController.MovementPattern.Sine,
            true, enemyBulletPrefab, 3, 2f);
        shooterEnemy.SetActive(false);

        GameObject diverEnemy = CreateEnemyPrefab("DiverEnemy",
            new Color(0.2f, 1f, 0.2f), EnemyController.MovementPattern.Dive,
            false, null, 1, 4f);
        diverEnemy.SetActive(false);

        // === Power-Up Prefabs ===
        GameObject healthPU = CreatePowerUpPrefab("HealthPowerUp",
            new Color(0.2f, 1f, 0.2f), PowerUpController.PowerUpType.Health);
        healthPU.SetActive(false);

        GameObject weaponPU = CreatePowerUpPrefab("WeaponPowerUp",
            new Color(1f, 1f, 0.2f), PowerUpController.PowerUpType.WeaponUpgrade);
        weaponPU.SetActive(false);

        // Wire up references
        pc.bulletPrefab = bulletPrefab;

        gm.enemyPrefabs = new GameObject[] { basicEnemy, zigzagEnemy, shooterEnemy, diverEnemy };
        gm.healthPowerUpPrefab = healthPU;
        gm.weaponPowerUpPrefab = weaponPU;
    }

    GameObject CreateBulletPrefab(string name, Color color, bool isPlayer)
    {
        GameObject bullet = new GameObject(name);
        bullet.tag = "Bullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = 3;

        if (isPlayer)
            bullet.transform.localScale = new Vector3(0.1f, 0.25f, 1f);
        else
            bullet.transform.localScale = new Vector3(0.12f, 0.2f, 1f);

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        BulletController bc = bullet.AddComponent<BulletController>();
        bc.isPlayerBullet = isPlayer;
        bc.speed = isPlayer ? 14f : 6f;
        bc.direction = isPlayer ? Vector3.up : Vector3.down;

        return bullet;
    }

    GameObject CreateEnemyPrefab(string name, Color color,
        EnemyController.MovementPattern pattern, bool canShoot,
        GameObject bulletPrefab, int health, float speed)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreateDiamondSprite();
        sr.color = color;
        sr.sortingOrder = 4;
        enemy.transform.localScale = Vector3.one * 0.5f;

        PolygonCollider2D col = enemy.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.pattern = pattern;
        ec.canShoot = canShoot;
        ec.enemyBulletPrefab = bulletPrefab;
        ec.maxHealth = health;
        ec.moveSpeed = speed;

        return enemy;
    }

    GameObject CreatePowerUpPrefab(string name, Color color, PowerUpController.PowerUpType type)
    {
        GameObject pu = new GameObject(name);
        pu.tag = "PowerUp";

        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = 6;
        pu.transform.localScale = Vector3.one * 0.4f;

        CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        PowerUpController pc = pu.AddComponent<PowerUpController>();
        pc.type = type;

        return pu;
    }

    GameObject CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // UIManager
        UIManager uiMgr = canvasObj.AddComponent<UIManager>();

        // === GAME HUD PANEL ===
        GameObject hudPanel = CreatePanel(canvasObj.transform, "GameHUDPanel", false);
        uiMgr.gameHUDPanel = hudPanel;

        // Score text (top-left)
        Text scoreText = CreateText(hudPanel.transform, "ScoreText", "SCORE: 0",
            TextAnchor.UpperLeft, new Vector2(20, -20), new Vector2(300, 50), 28);
        uiMgr.scoreText = scoreText;

        // Health text (top-right)
        Text healthText = CreateText(hudPanel.transform, "HealthText", "HP: 5 / 5",
            TextAnchor.UpperRight, new Vector2(-20, -20), new Vector2(300, 50), 28);
        RectTransform htRT = healthText.GetComponent<RectTransform>();
        htRT.anchorMin = new Vector2(1, 1);
        htRT.anchorMax = new Vector2(1, 1);
        htRT.pivot = new Vector2(1, 1);
        uiMgr.healthText = healthText;

        // Wave text (top-center)
        Text waveText = CreateText(hudPanel.transform, "WaveText", "WAVE: 1",
            TextAnchor.UpperCenter, new Vector2(0, -20), new Vector2(300, 50), 24);
        RectTransform wRT = waveText.GetComponent<RectTransform>();
        wRT.anchorMin = new Vector2(0.5f, 1);
        wRT.anchorMax = new Vector2(0.5f, 1);
        wRT.pivot = new Vector2(0.5f, 1);
        uiMgr.waveText = waveText;

        // Health bar background
        GameObject healthBarBg = CreateUIImage(hudPanel.transform, "HealthBarBg",
            new Color(0.3f, 0.3f, 0.3f, 0.8f), new Vector2(-20, -70), new Vector2(200, 20));
        RectTransform bgRT = healthBarBg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(1, 1);
        bgRT.anchorMax = new Vector2(1, 1);
        bgRT.pivot = new Vector2(1, 1);

        // Health bar fill
        GameObject healthBarFill = CreateUIImage(hudPanel.transform, "HealthBarFill",
            Color.green, new Vector2(-20, -70), new Vector2(200, 20));
        RectTransform fillRT = healthBarFill.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(1, 1);
        fillRT.anchorMax = new Vector2(1, 1);
        fillRT.pivot = new Vector2(1, 1);
        Image fillImg = healthBarFill.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        uiMgr.healthBar = healthBarFill;

        // Wave announcement (center, initially hidden)
        Text waveAnnounce = CreateText(hudPanel.transform, "WaveAnnouncement", "",
            TextAnchor.MiddleCenter, Vector2.zero, new Vector2(600, 100), 56);
        RectTransform waRT = waveAnnounce.GetComponent<RectTransform>();
        waRT.anchorMin = new Vector2(0.5f, 0.5f);
        waRT.anchorMax = new Vector2(0.5f, 0.5f);
        waRT.pivot = new Vector2(0.5f, 0.5f);
        waveAnnounce.color = new Color(1f, 1f, 0.3f);
        waveAnnounce.gameObject.SetActive(false);
        uiMgr.waveAnnouncementText = waveAnnounce;

        // === MENU PANEL ===
        GameObject menuPanel = CreatePanel(canvasObj.transform, "MenuPanel", true);
        uiMgr.menuPanel = menuPanel;

        // Dark overlay
        Image menuBg = menuPanel.GetComponent<Image>();
        menuBg.color = new Color(0f, 0f, 0.05f, 0.9f);

        // Title
        Text titleText = CreateText(menuPanel.transform, "TitleText", "SPACE SHOOTER",
            TextAnchor.MiddleCenter, new Vector2(0, 150), new Vector2(800, 120), 72);
        RectTransform ttRT = titleText.GetComponent<RectTransform>();
        ttRT.anchorMin = new Vector2(0.5f, 0.5f);
        ttRT.anchorMax = new Vector2(0.5f, 0.5f);
        ttRT.pivot = new Vector2(0.5f, 0.5f);
        titleText.color = new Color(0.3f, 0.9f, 1f);
        titleText.fontStyle = FontStyle.Bold;
        uiMgr.titleText = titleText;

        // Subtitle / controls
        Text subtitleText = CreateText(menuPanel.transform, "SubtitleText",
            "ARROW KEYS / WASD to Move\nSPACE to Shoot",
            TextAnchor.MiddleCenter, new Vector2(0, 40), new Vector2(600, 80), 24);
        RectTransform stRT = subtitleText.GetComponent<RectTransform>();
        stRT.anchorMin = new Vector2(0.5f, 0.5f);
        stRT.anchorMax = new Vector2(0.5f, 0.5f);
        stRT.pivot = new Vector2(0.5f, 0.5f);
        subtitleText.color = new Color(0.7f, 0.7f, 0.7f);

        // High score on menu
        Text hsMenuText = CreateText(menuPanel.transform, "HighScoreMenuText", "HIGH SCORE: 0",
            TextAnchor.MiddleCenter, new Vector2(0, -20), new Vector2(400, 40), 22);
        RectTransform hmRT = hsMenuText.GetComponent<RectTransform>();
        hmRT.anchorMin = new Vector2(0.5f, 0.5f);
        hmRT.anchorMax = new Vector2(0.5f, 0.5f);
        hmRT.pivot = new Vector2(0.5f, 0.5f);
        hsMenuText.color = new Color(1f, 0.8f, 0.2f);
        uiMgr.highScoreMenuText = hsMenuText;

        // Start button
        Button startBtn = CreateButton(menuPanel.transform, "StartButton", "START GAME",
            new Vector2(0, -90), new Vector2(280, 60), new Color(0.2f, 0.7f, 0.3f));
        uiMgr.startButton = startBtn;

        // Quit button
        Button quitBtn = CreateButton(menuPanel.transform, "QuitButton", "QUIT",
            new Vector2(0, -170), new Vector2(280, 60), new Color(0.7f, 0.2f, 0.2f));
        uiMgr.quitButton = quitBtn;

        // === GAME OVER PANEL ===
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", false);
        uiMgr.gameOverPanel = gameOverPanel;

        Image goBg = gameOverPanel.GetComponent<Image>();
        goBg.color = new Color(0.05f, 0f, 0f, 0.85f);

        Text goText = CreateText(gameOverPanel.transform, "GameOverText", "GAME OVER",
            TextAnchor.MiddleCenter, new Vector2(0, 150), new Vector2(700, 100), 64);
        RectTransform goRT = goText.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.5f, 0.5f);
        goRT.anchorMax = new Vector2(0.5f, 0.5f);
        goRT.pivot = new Vector2(0.5f, 0.5f);
        goText.color = new Color(1f, 0.2f, 0.2f);
        goText.fontStyle = FontStyle.Bold;
        uiMgr.gameOverText = goText;

        Text fsText = CreateText(gameOverPanel.transform, "FinalScoreText", "SCORE: 0",
            TextAnchor.MiddleCenter, new Vector2(0, 50), new Vector2(500, 60), 36);
        RectTransform fsRT = fsText.GetComponent<RectTransform>();
        fsRT.anchorMin = new Vector2(0.5f, 0.5f);
        fsRT.anchorMax = new Vector2(0.5f, 0.5f);
        fsRT.pivot = new Vector2(0.5f, 0.5f);
        fsText.color = Color.white;
        uiMgr.finalScoreText = fsText;

        Text hsText = CreateText(gameOverPanel.transform, "HighScoreText", "HIGH SCORE: 0",
            TextAnchor.MiddleCenter, new Vector2(0, -10), new Vector2(500, 50), 28);
        RectTransform hsRT = hsText.GetComponent<RectTransform>();
        hsRT.anchorMin = new Vector2(0.5f, 0.5f);
        hsRT.anchorMax = new Vector2(0.5f, 0.5f);
        hsRT.pivot = new Vector2(0.5f, 0.5f);
        hsText.color = new Color(1f, 0.8f, 0.2f);
        uiMgr.highScoreText = hsText;

        Button restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -80), new Vector2(280, 60), new Color(0.2f, 0.7f, 0.3f));
        uiMgr.restartButton = restartBtn;

        Button goQuitBtn = CreateButton(gameOverPanel.transform, "GameOverQuitButton", "QUIT",
            new Vector2(0, -160), new Vector2(280, 60), new Color(0.7f, 0.2f, 0.2f));
        uiMgr.menuQuitButton = goQuitBtn;

        return canvasObj;
    }

    void SetupBackground()
    {
        GameObject bg = new GameObject("Background");
        bg.AddComponent<BackgroundScroller>();
    }

    // === UI Helper Methods ===

    GameObject CreatePanel(Transform parent, string name, bool active)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0); // Transparent by default
        panel.SetActive(active);
        return panel;
    }

    Text CreateText(Transform parent, string name, string content,
        TextAnchor alignment, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        // Add outline for readability
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return text;
    }

    Button CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, Color bgColor)
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
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = bgColor;
        cb.highlightedColor = bgColor * 1.2f;
        cb.pressedColor = bgColor * 0.8f;
        btn.colors = cb;

        // Button text
        Text btnText = CreateText(btnObj.transform, "Text", label,
            TextAnchor.MiddleCenter, Vector2.zero, size, 26);
        RectTransform btRT = btnText.GetComponent<RectTransform>();
        btRT.anchorMin = Vector2.zero;
        btRT.anchorMax = Vector2.one;
        btRT.offsetMin = Vector2.zero;
        btRT.offsetMax = Vector2.zero;

        return btn;
    }

    GameObject CreateUIImage(Transform parent, string name, Color color,
        Vector2 position, Vector2 size)
    {
        GameObject imgObj = new GameObject(name);
        imgObj.transform.SetParent(parent, false);
        RectTransform rt = imgObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = imgObj.AddComponent<Image>();
        img.color = color;

        return imgObj;
    }

    // === Sprite Creation Methods ===

    /// <summary>Creates a simple white triangle sprite for the player ship.</summary>
    Sprite CreateTriangleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;

        // Fill transparent
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        // Draw triangle (pointing up)
        int midX = size / 2;
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            int halfWidth = (int)(t * midX);
            for (int x = midX - halfWidth; x <= midX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    pixels[y * size + x] = white;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }

    /// <summary>Creates a simple white square sprite.</summary>
    Sprite CreateSquareSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    /// <summary>Creates a diamond/rhombus sprite for enemies.</summary>
    Sprite CreateDiamondSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        int mid = size / 2;
        for (int y = 0; y < size; y++)
        {
            int dist = Mathf.Abs(y - mid);
            int halfWidth = mid - dist;
            for (int x = mid - halfWidth; x <= mid + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    pixels[y * size + x] = white;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }
}
