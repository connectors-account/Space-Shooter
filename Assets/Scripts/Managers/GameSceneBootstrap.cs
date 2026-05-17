using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bootstraps the GameScene by creating all required GameObjects at runtime
/// when prefabs/scene references are not yet wired in the editor.
/// This ensures the game is fully playable even without manual Unity editor setup.
/// </summary>
public class GameSceneBootstrap : MonoBehaviour
{
    void Awake()
    {
        SetupCamera();
        CreateManagers();
        CreateBackground();
        CreatePlayer();
        CreateUI();
        CreatePrefabs();
    }

    void Start()
    {
        // Initialize game state
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        if (GameManager.Instance != null)
            GameManager.Instance.IsGameActive_Set(true);

        // Start spawning after a short delay
        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.StartSpawning();

        // Initialize health display
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && UIManager.Instance != null)
            UIManager.Instance.UpdateHealthDisplay(player.CurrentHealth, player.MaxHealth);
    }

    void SetupCamera()
    {
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }

    void CreateManagers()
    {
        // GameManager (might already exist from DontDestroyOnLoad)
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        // ScoreManager
        if (ScoreManager.Instance == null)
        {
            GameObject scoreObj = new GameObject("ScoreManager");
            scoreObj.AddComponent<ScoreManager>();
        }

        // WaveSpawner
        if (WaveSpawner.Instance == null)
        {
            GameObject waveObj = new GameObject("WaveSpawner");
            waveObj.AddComponent<WaveSpawner>();
        }
    }

    void CreateBackground()
    {
        // Starfield
        GameObject starfield = new GameObject("Starfield");
        starfield.AddComponent<StarfieldGenerator>();
    }

    void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3.5f, 0);

        // Create player sprite (triangle shape)
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite(Color.cyan);
        sr.sortingOrder = 10;
        player.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        // Collider
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        // Rigidbody
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Player controller
        PlayerController pc = player.AddComponent<PlayerController>();

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.parent = player.transform;
        firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);
        pc.firePoint = firePoint.transform;

        // Create bullet prefab and assign
        pc.bulletPrefab = CreatePlayerBulletPrefab();

        // Create engine glow effect
        CreateEngineGlow(player.transform);
    }

    void CreateEngineGlow(Transform parent)
    {
        GameObject glow = new GameObject("EngineGlow");
        glow.transform.parent = parent;
        glow.transform.localPosition = new Vector3(0, -0.6f, 0);

        SpriteRenderer sr = glow.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(new Color(0f, 0.8f, 1f, 0.5f));
        sr.sortingOrder = 9;
        glow.transform.localScale = new Vector3(0.3f, 0.4f, 1f);
    }

    GameObject CreatePlayerBulletPrefab()
    {
        GameObject bullet = new GameObject("PlayerBullet");
        bullet.tag = "PlayerBullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite(Color.yellow);
        sr.sortingOrder = 5;
        bullet.transform.localScale = new Vector3(0.1f, 0.3f, 1f);

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        Bullet b = bullet.AddComponent<Bullet>();
        b.speed = 12f;
        b.direction = Vector2.up;
        b.damage = 1;

        // Store as inactive prefab
        bullet.SetActive(false);

        // We return this as a "prefab" (inactive template object)
        return bullet;
    }

    GameObject CreateEnemyBulletPrefab()
    {
        GameObject bullet = new GameObject("EnemyBullet");
        bullet.tag = "EnemyBullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite(new Color(1f, 0.3f, 0.3f));
        sr.sortingOrder = 5;
        bullet.transform.localScale = new Vector3(0.1f, 0.2f, 1f);

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        Bullet b = bullet.AddComponent<Bullet>();
        b.speed = 6f;
        b.direction = Vector2.down;
        b.damage = 1;

        bullet.SetActive(false);
        return bullet;
    }

    GameObject CreateHealthPickupPrefab()
    {
        GameObject pickup = new GameObject("HealthPickup");
        pickup.tag = "HealthPickup";

        SpriteRenderer sr = pickup.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(Color.green);
        sr.sortingOrder = 5;
        pickup.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        BoxCollider2D col = pickup.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        pickup.AddComponent<HealthPickup>();

        pickup.SetActive(false);
        return pickup;
    }

    void CreatePrefabs()
    {
        // Create enemy prefabs and assign to WaveSpawner
        GameObject enemyBulletPrefab = CreateEnemyBulletPrefab();
        GameObject healthPickupPrefab = CreateHealthPickupPrefab();

        // Basic enemy
        GameObject basicEnemy = CreateEnemyPrefab("BasicEnemy", Color.red, 2, 100, 2f,
            EnemyBase.EnemyMovePattern.StraightDown, true, enemyBulletPrefab, healthPickupPrefab);

        // Fast enemy
        GameObject fastEnemy = CreateEnemyPrefab("FastEnemy", new Color(1f, 0.5f, 0f), 1, 150, 4f,
            EnemyBase.EnemyMovePattern.SineWave, false, enemyBulletPrefab, healthPickupPrefab);

        // Tank enemy
        GameObject tankEnemy = CreateEnemyPrefab("TankEnemy", new Color(0.6f, 0f, 0.6f), 5, 250, 1.5f,
            EnemyBase.EnemyMovePattern.StraightDown, true, enemyBulletPrefab, healthPickupPrefab);

        // Assign to WaveSpawner
        WaveSpawner ws = WaveSpawner.Instance;
        if (ws != null)
        {
            ws.enemyPrefabs.Clear();
            ws.enemyPrefabs.Add(basicEnemy);
            ws.enemyPrefabs.Add(fastEnemy);
            ws.enemyPrefabs.Add(tankEnemy);
        }
    }

    GameObject CreateEnemyPrefab(string name, Color color, int health, int score, float speed,
        EnemyBase.EnemyMovePattern pattern, bool canShoot, GameObject bulletPrefab, GameObject pickupPrefab)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite(color);
        sr.sortingOrder = 8;
        // Flip upside down for enemies
        enemy.transform.localScale = new Vector3(0.5f, -0.5f, 1f);

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        EnemyBase eb = enemy.AddComponent<EnemyBase>();
        eb.maxHealth = health;
        eb.scoreValue = score;
        eb.moveSpeed = speed;
        eb.movePattern = pattern;
        eb.canShoot = canShoot;
        eb.enemyBulletPrefab = bulletPrefab;
        eb.shootInterval = 2f;
        eb.healthPickupPrefab = pickupPrefab;
        eb.healthDropChance = 0.15f;

        enemy.SetActive(false);
        return enemy;
    }

    void CreateUI()
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

        // UIManager component
        UIManager uiMgr = canvasObj.AddComponent<UIManager>();

        // === HUD ===
        // Score Text
        uiMgr.scoreText = CreateUIText(canvasObj.transform, "ScoreText", "SCORE: 0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), 28, TextAnchor.UpperLeft);

        // High Score Text
        uiMgr.highScoreText = CreateUIText(canvasObj.transform, "HighScoreText", "HI: 0",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), 22, TextAnchor.UpperRight);

        // Wave Text
        uiMgr.waveText = CreateUIText(canvasObj.transform, "WaveText", "WAVE 1",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), 24, TextAnchor.UpperCenter);

        // Health Text
        uiMgr.healthText = CreateUIText(canvasObj.transform, "HealthText", "♥ ♥ ♥ ♥ ♥",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, 20), 30, TextAnchor.LowerLeft);
        uiMgr.healthText.color = Color.red;

        // Combo Text
        uiMgr.comboText = CreateUIText(canvasObj.transform, "ComboText", "",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 60), 26, TextAnchor.LowerRight);
        uiMgr.comboText.color = Color.yellow;

        // === Wave Announcement Panel ===
        GameObject waveAnnounce = CreatePanel(canvasObj.transform, "WaveAnnouncementPanel",
            new Color(0, 0, 0, 0.6f), true);
        uiMgr.waveAnnouncementPanel = waveAnnounce;
        uiMgr.waveAnnouncementText = CreateUIText(waveAnnounce.transform, "WaveAnnouncementText", "WAVE 1",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 60, TextAnchor.MiddleCenter);

        // === Game Over Panel ===
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0, 0, 0, 0.8f), false);
        uiMgr.gameOverPanel = gameOverPanel;

        CreateUIText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, 60, TextAnchor.MiddleCenter);

        uiMgr.finalScoreText = CreateUIText(gameOverPanel.transform, "FinalScoreText", "SCORE: 0",
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, 36, TextAnchor.MiddleCenter);

        uiMgr.finalHighScoreText = CreateUIText(gameOverPanel.transform, "FinalHighScoreText", "HIGH SCORE: 0",
            new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), Vector2.zero, 28, TextAnchor.MiddleCenter);

        uiMgr.restartButton = CreateUIButton(gameOverPanel.transform, "RestartButton", "RESTART",
            new Vector2(0.5f, 0.35f), new Vector2(250, 50));

        uiMgr.mainMenuButton = CreateUIButton(gameOverPanel.transform, "MainMenuButton", "MAIN MENU",
            new Vector2(0.5f, 0.25f), new Vector2(250, 50));

        // === Pause Panel ===
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel",
            new Color(0, 0, 0, 0.7f), false);
        uiMgr.pausePanel = pausePanel;

        CreateUIText(pausePanel.transform, "PausedTitle", "PAUSED",
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero, 60, TextAnchor.MiddleCenter);

        uiMgr.resumeButton = CreateUIButton(pausePanel.transform, "ResumeButton", "RESUME",
            new Vector2(0.5f, 0.45f), new Vector2(250, 50));

        uiMgr.pauseMainMenuButton = CreateUIButton(pausePanel.transform, "PauseMainMenuButton", "MAIN MENU",
            new Vector2(0.5f, 0.35f), new Vector2(250, 50));

        uiMgr.quitButton = CreateUIButton(pausePanel.transform, "QuitButton", "QUIT",
            new Vector2(0.5f, 0.25f), new Vector2(250, 50));
    }

    // === Sprite Creation Helpers ===

    Sprite CreateTriangleSprite(Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedY = (float)y / size;
                float halfWidth = normalizedY * 0.5f;
                float normalizedX = (float)x / size - 0.5f;

                if (Mathf.Abs(normalizedX) < halfWidth)
                {
                    // Add a lighter edge for a 3D-ish look
                    float edgeDist = halfWidth - Mathf.Abs(normalizedX);
                    float edgeFactor = Mathf.Clamp01(edgeDist * 10f);
                    Color pixelColor = Color.Lerp(Color.white, color, edgeFactor);
                    pixelColor.a = 1f;
                    pixels[y * size + x] = pixelColor;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }

    Sprite CreateRectSprite(Color color)
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    Sprite CreateCircleSprite(Color color)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float radius = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist < radius)
                {
                    float alpha = Mathf.Clamp01((radius - dist) / 2f);
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha * color.a);
                }
                else
                    pixels[y * size + x] = Color.clear;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    // === UI Creation Helpers ===

    Text CreateUIText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetPos, int fontSize, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = offsetPos;
        rt.sizeDelta = new Vector2(400, 60);

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        // Add outline for readability
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        return text;
    }

    GameObject CreatePanel(Transform parent, string name, Color bgColor, bool activeByDefault)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;

        panel.SetActive(activeByDefault);
        return panel;
    }

    Button CreateUIButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.4f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.6f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.3f);
        btn.colors = colors;

        // Button label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRt = labelObj.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.sizeDelta = Vector2.zero;

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (labelText.font == null)
            labelText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        labelText.fontSize = 24;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;

        return btn;
    }
}
