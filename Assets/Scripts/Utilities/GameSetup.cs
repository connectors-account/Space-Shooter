// ============================================================================
// GameSetup.cs - Auto-configures the entire game scene at runtime
// Creates all GameObjects, sprites, colliders, and wires everything up.
// Attach this to a single empty GameObject in the GameScene.
// This is the BOOTSTRAP script - it builds the whole game programmatically.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameSetup creates the entire game scene programmatically.
/// No manual prefab/sprite setup is needed in the Unity Editor.
/// Simply create an empty scene, add a GameObject with this script,
/// and hit Play.
/// </summary>
public class GameSetup : MonoBehaviour
{
    // ========================================================================
    // Configuration
    // ========================================================================
    [Header("Setup toggles (disable to manually set up parts)")]
    public bool createPlayer = true;
    public bool createEnemySpawner = true;
    public bool createBackground = true;
    public bool createUI = true;
    public bool createManagers = true;

    // ---- Generated references (available after Awake) ----
    [HideInInspector] public GameObject playerObject;
    [HideInInspector] public GameObject enemySpawnerObject;

    // ---- Prefab references (generated at runtime) ----
    private GameObject _playerBulletPrefab;
    private GameObject _enemyBulletPrefab;
    private GameObject[] _enemyPrefabs;
    private GameObject[] _powerUpPrefabs;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Awake()
    {
        // Create managers first (GameManager, AudioManager)
        if (createManagers)
        {
            EnsureGameManager();
            EnsureAudioManager();
        }

        // Create bullet prefabs (needed by player and enemies)
        CreateBulletPrefabs();

        // Create power-up prefabs
        CreatePowerUpPrefabs();

        // Create enemy prefabs
        CreateEnemyPrefabs();

        // Create the player
        if (createPlayer)
            CreatePlayer();

        // Create background
        if (createBackground)
            CreateBackground();

        // Create enemy spawner
        if (createEnemySpawner)
            CreateEnemySpawner();

        // Create UI
        if (createUI)
            CreateGameUI();
    }

    private void Start()
    {
        // Ensure game is in Playing state
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            GameManager.Instance.StartGame();
        }
    }

    // ========================================================================
    // Manager Creation
    // ========================================================================

    private void EnsureGameManager()
    {
        if (GameManager.Instance != null) return;

        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
    }

    private void EnsureAudioManager()
    {
        if (AudioManager.Instance != null) return;

        GameObject amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();
    }

    // ========================================================================
    // Player Creation
    // ========================================================================

    private void CreatePlayer()
    {
        playerObject = new GameObject("Player");
        playerObject.tag = "Player";
        playerObject.layer = LayerMask.NameToLayer("Default");

        // Sprite
        SpriteRenderer sr = playerObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateTriangle(32, new Color(0.2f, 0.8f, 1f));
        sr.sortingOrder = 5;

        // Physics
        Rigidbody2D rb = playerObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        BoxCollider2D col = playerObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1f);

        // Scripts
        HealthSystem hs = playerObject.AddComponent<HealthSystem>();
        hs.maxHealth = 100;

        CollisionHandler ch = playerObject.AddComponent<CollisionHandler>();
        ch.contactDamage = 20;

        PlayerController pc = playerObject.AddComponent<PlayerController>();
        pc.bulletPrefab = _playerBulletPrefab;
        pc.moveSpeed = 8f;
        pc.baseFireRate = 0.2f;
        pc.bulletSpeed = 15f;
        pc.bulletDamage = 25;

        // Fire point (child object at tip of ship)
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(playerObject.transform);
        firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);
        pc.firePoint = firePoint.transform;

        // Starting position (bottom center)
        playerObject.transform.position = new Vector3(0, -3.5f, 0);
    }

    // ========================================================================
    // Bullet Prefab Creation
    // ========================================================================

    private void CreateBulletPrefabs()
    {
        // Player bullet (cyan/white, moves up)
        _playerBulletPrefab = CreateBulletPrefab("PlayerBullet", "PlayerBullet",
            new Color(0.5f, 1f, 1f), 8);

        // Enemy bullet (red/orange, moves down)
        _enemyBulletPrefab = CreateBulletPrefab("EnemyBullet", "EnemyBullet",
            new Color(1f, 0.3f, 0.1f), 8);
    }

    private GameObject CreateBulletPrefab(string name, string tag, Color color, int size)
    {
        GameObject bullet = new GameObject(name);

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(size, color);
        sr.sortingOrder = 4;

        CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        bullet.AddComponent<BulletController>();

        // Make it a "prefab" by deactivating and keeping reference
        bullet.SetActive(false);
        bullet.tag = tag;

        return bullet;
    }

    // ========================================================================
    // Enemy Prefab Creation
    // ========================================================================

    private void CreateEnemyPrefabs()
    {
        _enemyPrefabs = new GameObject[3];

        // Type 0: Basic enemy (red diamond, straight movement, single shot)
        _enemyPrefabs[0] = CreateEnemyPrefab("EnemyBasic",
            SpriteGenerator.CreateDiamond(28, new Color(1f, 0.2f, 0.2f)),
            EnemyController.MovementPattern.StraightDown,
            EnemyController.ShootPattern.SingleForward,
            30, 3f, 1.5f, 100);

        // Type 1: Fast enemy (yellow diamond, zigzag, aimed shots)
        _enemyPrefabs[1] = CreateEnemyPrefab("EnemyFast",
            SpriteGenerator.CreateDiamond(24, new Color(1f, 0.9f, 0.1f)),
            EnemyController.MovementPattern.Zigzag,
            EnemyController.ShootPattern.Aimed,
            20, 5f, 2f, 150);

        // Type 2: Tank enemy (purple diamond, sine wave, spread shots)
        _enemyPrefabs[2] = CreateEnemyPrefab("EnemyTank",
            SpriteGenerator.CreateDiamond(36, new Color(0.7f, 0.1f, 0.9f)),
            EnemyController.MovementPattern.Sine,
            EnemyController.ShootPattern.Spread,
            60, 2f, 2.5f, 250);
    }

    private GameObject CreateEnemyPrefab(string name, Sprite sprite,
        EnemyController.MovementPattern movePattern,
        EnemyController.ShootPattern shootPattern,
        int health, float speed, float fireRate, int scoreValue)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        // Flip enemies upside down so they face the player
        enemy.transform.localScale = new Vector3(1, -1, 1);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        HealthSystem hs = enemy.AddComponent<HealthSystem>();
        hs.maxHealth = health;

        CollisionHandler ch = enemy.AddComponent<CollisionHandler>();
        ch.contactDamage = 15;
        ch.scoreValue = scoreValue;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.movementPattern = movePattern;
        ec.shootPattern = shootPattern;
        ec.bulletPrefab = _enemyBulletPrefab;
        ec.moveSpeed = speed;
        ec.fireRate = fireRate;
        ec.bulletSpeed = 8f;
        ec.bulletDamage = 10;
        ec.scoreValue = scoreValue;

        enemy.SetActive(false);
        return enemy;
    }

    // ========================================================================
    // Power-Up Prefab Creation
    // ========================================================================

    private void CreatePowerUpPrefabs()
    {
        _powerUpPrefabs = new GameObject[4];

        // Health (green hexagon)
        _powerUpPrefabs[0] = CreatePowerUpPrefab("PowerUp_Health",
            PowerUpController.PowerUpType.Health, Color.green);

        // Weapon Upgrade (orange hexagon)
        _powerUpPrefabs[1] = CreatePowerUpPrefab("PowerUp_Weapon",
            PowerUpController.PowerUpType.WeaponUpgrade, new Color(1f, 0.5f, 0f));

        // Shield (blue hexagon)
        _powerUpPrefabs[2] = CreatePowerUpPrefab("PowerUp_Shield",
            PowerUpController.PowerUpType.Shield, new Color(0.3f, 0.7f, 1f));

        // Speed (yellow hexagon)
        _powerUpPrefabs[3] = CreatePowerUpPrefab("PowerUp_Speed",
            PowerUpController.PowerUpType.SpeedBoost, Color.yellow);
    }

    private GameObject CreatePowerUpPrefab(string name,
        PowerUpController.PowerUpType type, Color color)
    {
        GameObject pu = new GameObject(name);
        pu.tag = "PowerUp";

        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateHexagon(20, color);
        sr.sortingOrder = 6;

        CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        PowerUpController puc = pu.AddComponent<PowerUpController>();
        puc.type = type;

        pu.SetActive(false);
        return pu;
    }

    // ========================================================================
    // Background Creation
    // ========================================================================

    private void CreateBackground()
    {
        // Create two layers for parallax effect

        // Layer 1: Far stars (slow)
        CreateStarLayer("Background_Far", 200, 0.5f, -10, 256);

        // Layer 2: Near stars (fast)
        CreateStarLayer("Background_Near", 100, 1.5f, -5, 128);
    }

    private void CreateStarLayer(string name, int starCount, float speed,
        int sortOrder, int texSize)
    {
        GameObject bg = new GameObject(name);

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateStarfield(texSize, texSize * 2, starCount);
        sr.sortingOrder = sortOrder;
        sr.drawMode = SpriteDrawMode.Tiled;

        // Scale to cover the screen
        bg.transform.localScale = new Vector3(2f, 2f, 1f);
        bg.transform.position = new Vector3(0, 0, 10);

        BackgroundScroller bs = bg.AddComponent<BackgroundScroller>();
        bs.scrollSpeed = speed;
        bs.tileHeight = 20f;
    }

    // ========================================================================
    // Enemy Spawner Creation
    // ========================================================================

    private void CreateEnemySpawner()
    {
        enemySpawnerObject = new GameObject("EnemySpawner");

        EnemySpawner spawner = enemySpawnerObject.AddComponent<EnemySpawner>();
        spawner.enemyPrefabs = _enemyPrefabs;
        spawner.powerUpPrefabs = _powerUpPrefabs;
        spawner.spawnInterval = 0.8f;
        spawner.spawnRangeX = 6f;
        spawner.spawnY = 6.5f;
        spawner.powerUpDropChance = 0.5f;
    }

    // ========================================================================
    // Game UI Creation (Canvas + all panels)
    // ========================================================================

    private void CreateGameUI()
    {
        // ---- Canvas ----
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // ---- Event System (needed for UI buttons) ----
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ---- UIManager component ----
        UIManager uiMgr = canvasObj.AddComponent<UIManager>();

        // ---- HUD ----
        uiMgr.scoreText = CreateUIText(canvasObj.transform, "ScoreText",
            "SCORE: 0", TextAnchor.UpperLeft,
            new Vector2(20, -20), new Vector2(300, 40));

        uiMgr.waveText = CreateUIText(canvasObj.transform, "WaveText",
            "WAVE: 1", TextAnchor.UpperCenter,
            new Vector2(0, -20), new Vector2(300, 40));

        uiMgr.highScoreText = CreateUIText(canvasObj.transform, "HighScoreText",
            "HIGH: 0", TextAnchor.UpperRight,
            new Vector2(-20, -20), new Vector2(300, 40));

        uiMgr.healthText = CreateUIText(canvasObj.transform, "HealthText",
            "HP: 100/100", TextAnchor.LowerLeft,
            new Vector2(20, 20), new Vector2(300, 40));

        // Health bar
        uiMgr.healthBar = CreateHealthBar(canvasObj.transform);

        // Wave announcement (center of screen, large text)
        uiMgr.waveAnnouncementText = CreateUIText(canvasObj.transform, "WaveAnnouncement",
            "", TextAnchor.MiddleCenter,
            new Vector2(0, 50), new Vector2(600, 100));
        uiMgr.waveAnnouncementText.fontSize = 48;
        uiMgr.waveAnnouncementText.color = Color.yellow;

        // ---- Pause Panel ----
        uiMgr.pausePanel = CreatePanel(canvasObj.transform, "PausePanel",
            new Color(0, 0, 0, 0.7f));

        CreateUIText(uiMgr.pausePanel.transform, "PauseTitle",
            "PAUSED", TextAnchor.MiddleCenter,
            new Vector2(0, 100), new Vector2(400, 60)).fontSize = 42;

        uiMgr.resumeButton = CreateUIButton(uiMgr.pausePanel.transform,
            "ResumeBtn", "RESUME", new Vector2(0, 0));

        uiMgr.pauseMainMenuButton = CreateUIButton(uiMgr.pausePanel.transform,
            "PauseMenuBtn", "MAIN MENU", new Vector2(0, -70));

        uiMgr.pausePanel.SetActive(false);

        // ---- Game Over Panel ----
        uiMgr.gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Color(0.1f, 0, 0, 0.8f));

        CreateUIText(uiMgr.gameOverPanel.transform, "GameOverTitle",
            "GAME OVER", TextAnchor.MiddleCenter,
            new Vector2(0, 150), new Vector2(500, 70)).fontSize = 52;

        uiMgr.finalScoreText = CreateUIText(uiMgr.gameOverPanel.transform,
            "FinalScore", "FINAL SCORE: 0", TextAnchor.MiddleCenter,
            new Vector2(0, 60), new Vector2(400, 50));
        uiMgr.finalScoreText.fontSize = 32;

        uiMgr.gameOverHighScoreText = CreateUIText(uiMgr.gameOverPanel.transform,
            "GOHighScore", "HIGH SCORE: 0", TextAnchor.MiddleCenter,
            new Vector2(0, 10), new Vector2(400, 40));
        uiMgr.gameOverHighScoreText.fontSize = 24;
        uiMgr.gameOverHighScoreText.color = Color.yellow;

        uiMgr.restartButton = CreateUIButton(uiMgr.gameOverPanel.transform,
            "RestartBtn", "RESTART", new Vector2(0, -60));

        uiMgr.gameOverMainMenuButton = CreateUIButton(uiMgr.gameOverPanel.transform,
            "GOMenuBtn", "MAIN MENU", new Vector2(0, -130));

        uiMgr.gameOverPanel.SetActive(false);
    }

    // ========================================================================
    // UI Helper Methods
    // ========================================================================

    private Text CreateUIText(Transform parent, string name, string content,
        TextAnchor anchor, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;

        // Position based on anchor
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                break;
            case TextAnchor.UpperRight:
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                break;
            case TextAnchor.UpperCenter:
                rt.anchorMin = new Vector2(0.5f, 1);
                rt.anchorMax = new Vector2(0.5f, 1);
                rt.pivot = new Vector2(0.5f, 1);
                break;
            case TextAnchor.LowerLeft:
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                break;
            case TextAnchor.MiddleCenter:
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;
        }
        rt.anchoredPosition = position;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        // Add outline for readability
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        return text;
    }

    private Slider CreateHealthBar(Transform parent)
    {
        // Container
        GameObject sliderObj = new GameObject("HealthBar");
        sliderObj.transform.SetParent(parent, false);

        RectTransform rt = sliderObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(20, 50);
        rt.sizeDelta = new Vector2(200, 20);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = new Vector2(-10, 0);
        fillAreaRt.anchoredPosition = new Vector2(-5, 0);

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(0, 1);
        fillRt.sizeDelta = new Vector2(10, 0);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = Color.green;

        slider.fillRect = fillRt;
        slider.interactable = false; // Display only

        return slider;
    }

    private GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;

        return panel;
    }

    private Button CreateUIButton(Transform parent, string name, string label,
        Vector2 position)
    {
        // Button container
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(250, 50);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.6f);
        btn.colors = colors;

        // Button label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRt = labelObj.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.sizeDelta = Vector2.zero;

        Text text = labelObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        return btn;
    }
}
