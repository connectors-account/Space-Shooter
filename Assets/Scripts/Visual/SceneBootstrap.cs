using UnityEngine;

/// <summary>
/// Bootstraps the entire game scene programmatically.
/// Creates all required GameObjects, components, UI, and prefabs at runtime.
/// Attach this to a single empty GameObject named "Bootstrap" in the scene.
/// This eliminates the need for manual prefab/scene setup in the Unity Editor.
/// </summary>
public class SceneBootstrap : MonoBehaviour
{
    private void Awake()
    {
        // Set up camera
        SetupCamera();

        // Create singletons
        CreateGameManager();
        CreateAudioManager();
        CreateSpriteGenerator();
        CreateGameBounds();

        // Create game objects
        CreatePlayer();
        CreateBackground();
        CreateEnemySpawner();
        CreatePowerUpSpawner();
        CreateObjectPool();

        // Create UI
        CreateUICanvas();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
        }
    }

    private void CreateGameManager()
    {
        if (GameManager.Instance != null) return;
        GameObject go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    private void CreateAudioManager()
    {
        if (AudioManager.Instance != null) return;
        GameObject go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
    }

    private void CreateSpriteGenerator()
    {
        if (SpriteGenerator.Instance != null) return;
        GameObject go = new GameObject("SpriteGenerator");
        go.AddComponent<SpriteGenerator>();
    }

    private void CreateGameBounds()
    {
        if (GameBounds.Instance != null) return;
        GameObject go = new GameObject("GameBounds");
        go.AddComponent<GameBounds>();
    }

    private void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3.5f, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateTriangleSprite(32, new Color(0.2f, 0.8f, 1f));
        sr.sortingOrder = 10;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerShooting>();
        player.AddComponent<ShieldVisual>();

        // Engine glow effect
        GameObject engineGlow = new GameObject("EngineGlow");
        engineGlow.transform.parent = player.transform;
        engineGlow.transform.localPosition = new Vector3(0, -0.5f, 0);
        SpriteRenderer glowSR = engineGlow.AddComponent<SpriteRenderer>();
        glowSR.sprite = SpriteGenerator.CreateCircleSprite(8, new Color(1f, 0.5f, 0.1f, 0.7f));
        glowSR.sortingOrder = 9;
        engineGlow.transform.localScale = Vector3.one * 0.4f;
    }

    private void CreateBackground()
    {
        GameObject bg = new GameObject("Background");
        bg.AddComponent<ParallaxBackground>();
    }

    private void CreateEnemySpawner()
    {
        GameObject go = new GameObject("EnemySpawner");
        go.AddComponent<EnemySpawner>();
    }

    private void CreatePowerUpSpawner()
    {
        GameObject go = new GameObject("PowerUpSpawner");
        go.AddComponent<PowerUpSpawner>();
    }

    private void CreateObjectPool()
    {
        GameObject poolObj = new GameObject("ObjectPool");
        ObjectPool pool = poolObj.AddComponent<ObjectPool>();
        pool.pools = new System.Collections.Generic.List<ObjectPool.Pool>();

        // Player Bullet prefab
        GameObject playerBulletPrefab = CreateBulletPrefab("PlayerBulletPrefab",
            new Color(0.3f, 1f, 0.3f), true);
        pool.pools.Add(new ObjectPool.Pool
        {
            tag = "PlayerBullet",
            prefab = playerBulletPrefab,
            initialSize = 30
        });

        // Enemy Bullet prefab
        GameObject enemyBulletPrefab = CreateBulletPrefab("EnemyBulletPrefab",
            new Color(1f, 0.3f, 0.3f), false);
        pool.pools.Add(new ObjectPool.Pool
        {
            tag = "EnemyBullet",
            prefab = enemyBulletPrefab,
            initialSize = 30
        });

        // Enemy prefabs
        pool.pools.Add(new ObjectPool.Pool
        {
            tag = "EnemyStraight",
            prefab = CreateEnemyPrefab<EnemyStraight>("EnemyStraightPrefab",
                Color.red, 1, 100, 3f, false, "EnemyStraight"),
            initialSize = 15
        });

        pool.pools.Add(new ObjectPool.Pool
        {
            tag = "EnemyZigZag",
            prefab = CreateEnemyPrefab<EnemyZigZag>("EnemyZigZagPrefab",
                new Color(1f, 0.5f, 0f), 2, 150, 2.5f, false, "EnemyZigZag"),
            initialSize = 10
        });

        pool.pools.Add(new ObjectPool.Pool
        {
            tag = "EnemyDiver",
            prefab = CreateEnemyPrefab<EnemyDiver>("EnemyDiverPrefab",
                new Color(1f, 0f, 1f), 1, 200, 4f, false, "EnemyDiver"),
            initialSize = 10
        });

        pool.pools.Add(new ObjectPool.Pool
        {
            tag = "EnemyTank",
            prefab = CreateEnemyPrefab<EnemyTank>("EnemyTankPrefab",
                new Color(0.8f, 0.2f, 0.2f), 5, 300, 1.5f, true, "EnemyTank"),
            initialSize = 5
        });

        // Power-up prefab
        GameObject powerUpPrefab = CreatePowerUpPrefab();
        pool.pools.Add(new ObjectPool.Pool
        {
            tag = "PowerUp",
            prefab = powerUpPrefab,
            initialSize = 10
        });
    }

    private GameObject CreateBulletPrefab(string name, Color color, bool isPlayerBullet)
    {
        GameObject prefab = new GameObject(name);
        prefab.SetActive(false);

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircleSprite(8, color);
        sr.sortingOrder = 8;

        BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.3f, 0.3f);

        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        Bullet bullet = prefab.AddComponent<Bullet>();
        bullet.poolTag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        prefab.transform.parent = null;
        return prefab;
    }

    private GameObject CreateEnemyPrefab<T>(string name, Color color, int health,
        int score, float speed, bool canShoot, string poolTag) where T : EnemyBase
    {
        GameObject prefab = new GameObject(name);
        prefab.SetActive(false);

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateInvertedTriangleSprite(32, color);
        sr.sortingOrder = 7;

        // Make tank enemies larger
        if (typeof(T) == typeof(EnemyTank))
        {
            prefab.transform.localScale = Vector3.one * 1.5f;
        }

        BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        T enemy = prefab.AddComponent<T>();
        enemy.maxHealth = health;
        enemy.scoreValue = score;
        enemy.moveSpeed = speed;
        enemy.canShoot = canShoot;
        enemy.poolTag = poolTag;

        return prefab;
    }

    private GameObject CreatePowerUpPrefab()
    {
        GameObject prefab = new GameObject("PowerUpPrefab");
        prefab.SetActive(false);

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateDiamondSprite(24, Color.yellow);
        sr.sortingOrder = 6;

        BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);

        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        prefab.AddComponent<PowerUp>();

        return prefab;
    }

    private void CreateUICanvas()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Event System
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // HUD
        CreateHUD(canvasObj.transform);

        // Main Menu
        CreateMainMenu(canvasObj.transform);

        // Pause Menu
        CreatePauseMenu(canvasObj.transform);

        // Game Over Screen
        CreateGameOverScreen(canvasObj.transform);
    }

    private void CreateHUD(Transform parent)
    {
        GameObject hudPanel = new GameObject("HUDPanel");
        hudPanel.transform.SetParent(parent, false);
        RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.sizeDelta = Vector2.zero;

        HUDManager hud = hudPanel.AddComponent<HUDManager>();
        hud.hudPanel = hudPanel;

        // Score text - top right
        hud.scoreText = CreateUIText(hudPanel.transform, "ScoreText", "SCORE: 0",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20),
            new Vector2(300, 50), TextAnchor.UpperRight, 28);

        // Wave text - top center
        hud.waveText = CreateUIText(hudPanel.transform, "WaveText", "WAVE 1",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20),
            new Vector2(300, 50), TextAnchor.UpperCenter, 32);

        // Health text - top left
        hud.healthText = CreateUIText(hudPanel.transform, "HealthText", "",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20),
            new Vector2(300, 50), TextAnchor.UpperLeft, 32);

        // Power-up text - bottom center
        hud.powerUpText = CreateUIText(hudPanel.transform, "PowerUpText", "",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30),
            new Vector2(500, 40), TextAnchor.LowerCenter, 20);
        hud.powerUpText.color = Color.yellow;

        hudPanel.SetActive(false);
    }

    private void CreateMainMenu(Transform parent)
    {
        GameObject menuPanel = new GameObject("MainMenuPanel");
        menuPanel.transform.SetParent(parent, false);
        RectTransform menuRect = menuPanel.AddComponent<RectTransform>();
        menuRect.anchorMin = Vector2.zero;
        menuRect.anchorMax = Vector2.one;
        menuRect.sizeDelta = Vector2.zero;

        // Semi-transparent background
        UnityEngine.UI.Image bg = menuPanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        MainMenuUI menuUI = menuPanel.AddComponent<MainMenuUI>();
        menuUI.menuPanel = menuPanel;

        // Title
        menuUI.titleText = CreateUIText(menuPanel.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero,
            new Vector2(600, 80), TextAnchor.MiddleCenter, 56);
        menuUI.titleText.color = new Color(0.2f, 0.8f, 1f);

        // High Score
        menuUI.highScoreText = CreateUIText(menuPanel.transform, "HighScoreText", "",
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero,
            new Vector2(400, 40), TextAnchor.MiddleCenter, 24);
        menuUI.highScoreText.color = Color.yellow;

        // Start Button
        menuUI.startButton = CreateUIButton(menuPanel.transform, "StartButton", "START GAME",
            new Vector2(0.5f, 0.4f), new Vector2(250, 60), 28);

        // Quit Button
        menuUI.quitButton = CreateUIButton(menuPanel.transform, "QuitButton", "QUIT",
            new Vector2(0.5f, 0.25f), new Vector2(250, 60), 28);
    }

    private void CreatePauseMenu(Transform parent)
    {
        GameObject pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(parent, false);
        RectTransform rect = pausePanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Image bg = pausePanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.8f);

        PauseMenuUI pauseUI = pausePanel.AddComponent<PauseMenuUI>();
        pauseUI.pausePanel = pausePanel;

        // Title
        CreateUIText(pausePanel.transform, "PauseTitle", "PAUSED",
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero,
            new Vector2(400, 60), TextAnchor.MiddleCenter, 48);

        // Resume Button
        pauseUI.resumeButton = CreateUIButton(pausePanel.transform, "ResumeButton", "RESUME",
            new Vector2(0.5f, 0.45f), new Vector2(250, 60), 28);

        // Main Menu Button
        pauseUI.mainMenuButton = CreateUIButton(pausePanel.transform, "PauseMainMenuBtn", "MAIN MENU",
            new Vector2(0.5f, 0.3f), new Vector2(250, 60), 28);

        pausePanel.SetActive(false);
    }

    private void CreateGameOverScreen(Transform parent)
    {
        GameObject goPanel = new GameObject("GameOverPanel");
        goPanel.transform.SetParent(parent, false);
        RectTransform rect = goPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Image bg = goPanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.1f, 0, 0, 0.85f);

        GameOverUI goUI = goPanel.AddComponent<GameOverUI>();
        goUI.gameOverPanel = goPanel;

        // Game Over text
        goUI.gameOverText = CreateUIText(goPanel.transform, "GameOverText", "GAME OVER",
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero,
            new Vector2(500, 80), TextAnchor.MiddleCenter, 56);
        goUI.gameOverText.color = new Color(1f, 0.2f, 0.2f);

        // Final score
        goUI.finalScoreText = CreateUIText(goPanel.transform, "FinalScoreText", "SCORE: 0",
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero,
            new Vector2(400, 50), TextAnchor.MiddleCenter, 32);

        // New high score
        goUI.newHighScoreText = CreateUIText(goPanel.transform, "NewHighScoreText", "NEW HIGH SCORE!",
            new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero,
            new Vector2(400, 40), TextAnchor.MiddleCenter, 24);
        goUI.newHighScoreText.color = Color.yellow;

        // Retry Button
        goUI.retryButton = CreateUIButton(goPanel.transform, "RetryButton", "RETRY",
            new Vector2(0.5f, 0.35f), new Vector2(250, 60), 28);

        // Main Menu Button
        goUI.mainMenuButton = CreateUIButton(goPanel.transform, "GOMainMenuBtn", "MAIN MENU",
            new Vector2(0.5f, 0.2f), new Vector2(250, 60), 28);

        goPanel.SetActive(false);
    }

    // ---- UI Helper Methods ----

    private UnityEngine.UI.Text CreateUIText(Transform parent, string name, string text,
        Vector2 anchorPos, Vector2 pivot, Vector2 offset, Vector2 size,
        TextAnchor alignment, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorPos;
        rect.anchorMax = anchorPos;
        rect.pivot = pivot;
        rect.anchoredPosition = offset;
        rect.sizeDelta = size;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiText.font == null)
            uiText.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        uiText.alignment = alignment;
        uiText.color = Color.white;
        uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;

        return uiText;
    }

    private UnityEngine.UI.Button CreateUIButton(Transform parent, string name, string label,
        Vector2 anchorPos, Vector2 size, int fontSize)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorPos;
        rect.anchorMax = anchorPos;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        UnityEngine.UI.Image img = buttonObj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.15f, 0.15f, 0.3f, 1f);

        UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();

        // Button color transitions
        var colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.3f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.5f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.2f);
        button.colors = colors;

        // Button label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Text labelText = labelObj.AddComponent<UnityEngine.UI.Text>();
        labelText.text = label;
        labelText.fontSize = fontSize;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (labelText.font == null)
            labelText.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;

        return button;
    }
}
