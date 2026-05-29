using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Editor script that automatically creates both game scenes with all GameObjects,
/// components, prefabs, and UI elements fully configured.
/// Run from Unity menu: Tools > Space Shooter > Setup Entire Project
/// </summary>
public class SceneAutoSetup : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Setup Entire Project")]
    public static void SetupProject()
    {
        // Ensure directories exist
        EnsureDirectories();

        // Generate sprites first
        SpriteGenerator.GenerateAllSprites();

        // Create prefabs
        CreatePrefabs();

        // Create scenes
        CreateGameScene();
        CreateMainMenuScene();

        // Configure build settings
        ConfigureBuildSettings();

        // Configure project settings
        ConfigureProjectSettings();

        Debug.Log("=== Project setup complete! ===");
        Debug.Log("1. Open MainMenuScene to start playing");
        Debug.Log("2. Press Play in the editor");
        Debug.Log("3. Build via File > Build Settings > Build");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = { "Assets/Sprites", "Assets/Prefabs", "Assets/Scenes", "Assets/Audio", "Assets/Materials" };
        foreach (string dir in dirs)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }

    // ============================================
    // PREFAB CREATION
    // ============================================

    private static void CreatePrefabs()
    {
        CreatePlayerBulletPrefab();
        CreateEnemyBulletPrefab();
        CreateEnemyPrefab();
        CreatePowerUpPrefab();
        CreatePlayerPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Prefabs created.");
    }

    private static void CreatePlayerBulletPrefab()
    {
        GameObject bullet = new GameObject("PlayerBullet");
        var sr = bullet.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.3f, 0.7f, 1f, 1f);
        sr.sortingOrder = 3;
        LoadSprite(sr, "Assets/Sprites/PlayerBullet.png");

        var rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.4f);

        bullet.AddComponent<BulletController>();

        var ch = bullet.AddComponent<CollisionHandler>();
        SetPrivateField(ch, "ownerType", CollisionHandler.ColliderOwner.PlayerBullet);

        bullet.tag = "PlayerBullet";
        bullet.layer = LayerMask.NameToLayer("Default");

        PrefabUtility.SaveAsPrefabAsset(bullet, "Assets/Prefabs/PlayerBullet.prefab");
        Object.DestroyImmediate(bullet);
    }

    private static void CreateEnemyBulletPrefab()
    {
        GameObject bullet = new GameObject("EnemyBullet");
        var sr = bullet.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.3f, 0.1f, 1f);
        sr.sortingOrder = 3;
        LoadSprite(sr, "Assets/Sprites/EnemyBullet.png");

        var rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = bullet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        bullet.AddComponent<BulletController>();

        var ch = bullet.AddComponent<CollisionHandler>();
        SetPrivateField(ch, "ownerType", CollisionHandler.ColliderOwner.EnemyBullet);

        bullet.tag = "EnemyBullet";

        PrefabUtility.SaveAsPrefabAsset(bullet, "Assets/Prefabs/EnemyBullet.prefab");
        Object.DestroyImmediate(bullet);
    }

    private static void CreateEnemyPrefab()
    {
        GameObject enemy = new GameObject("Enemy");
        var sr = enemy.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        sr.sortingOrder = 2;
        LoadSprite(sr, "Assets/Sprites/EnemyBasic.png");

        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);

        var hs = enemy.AddComponent<HealthSystem>();
        SetPrivateField(hs, "maxHealth", 50);
        SetPrivateField(hs, "destroyOnDeath", true);

        var ec = enemy.AddComponent<EnemyController>();
        // Assign enemy bullet prefab reference
        GameObject enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyBullet.prefab");
        SetPrivateField(ec, "bulletPrefab", enemyBulletPrefab);

        var ch = enemy.AddComponent<CollisionHandler>();
        SetPrivateField(ch, "ownerType", CollisionHandler.ColliderOwner.Enemy);

        enemy.tag = "Enemy";

        PrefabUtility.SaveAsPrefabAsset(enemy, "Assets/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(enemy);
    }

    private static void CreatePowerUpPrefab()
    {
        GameObject powerUp = new GameObject("PowerUp");
        var sr = powerUp.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sortingOrder = 4;
        LoadSprite(sr, "Assets/Sprites/PowerUp.png");

        var col = powerUp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        powerUp.AddComponent<PowerUpController>();

        var ch = powerUp.AddComponent<CollisionHandler>();
        SetPrivateField(ch, "ownerType", CollisionHandler.ColliderOwner.PowerUp);

        powerUp.tag = "PowerUp";

        PrefabUtility.SaveAsPrefabAsset(powerUp, "Assets/Prefabs/PowerUp.prefab");
        Object.DestroyImmediate(powerUp);
    }

    private static void CreatePlayerPrefab()
    {
        GameObject player = new GameObject("Player");
        var sr = player.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sortingOrder = 2;
        LoadSprite(sr, "Assets/Sprites/PlayerShip.png");

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.7f);

        var hs = player.AddComponent<HealthSystem>();
        SetPrivateField(hs, "maxHealth", 100);
        SetPrivateField(hs, "destroyOnDeath", false);
        SetPrivateField(hs, "invincibilityDuration", 1f);

        var pc = player.AddComponent<PlayerController>();
        GameObject playerBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerBullet.prefab");
        SetPrivateField(pc, "bulletPrefab", playerBulletPrefab);
        SetPrivateField(pc, "spriteRenderer", sr);

        // Create fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        SetPrivateField(pc, "firePoint", firePoint.transform);

        var ch = player.AddComponent<CollisionHandler>();
        SetPrivateField(ch, "ownerType", CollisionHandler.ColliderOwner.Player);

        player.tag = "Player";

        PrefabUtility.SaveAsPrefabAsset(player, "Assets/Prefabs/Player.prefab");
        Object.DestroyImmediate(player);
    }

    // ============================================
    // GAME SCENE
    // ============================================

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Configure camera
        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.transform.position = new Vector3(0, 0, -10);

        // --- Game Manager (if not DontDestroyOnLoad) ---
        // GameManager is instantiated via MainMenuScene, but we add a fallback
        GameObject gmCheck = new GameObject("GameManagerFallback");
        var fallback = gmCheck.AddComponent<GameManagerFallback>();

        // --- Player ---
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (playerPrefab != null)
        {
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(0f, -3.5f, 0f);
        }

        // --- Enemy Spawner ---
        GameObject spawnerObj = new GameObject("EnemySpawner");
        var spawner = spawnerObj.AddComponent<EnemySpawner>();
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        SetPrivateField(spawner, "enemyPrefab", enemyPrefab);

        // --- Background Scroller ---
        GameObject bgObj = new GameObject("BackgroundScroller");
        bgObj.AddComponent<BackgroundScroller>();

        // --- Game Canvas (HUD) ---
        CreateGameHUD();

        // --- Menu Manager (for pause and game over) ---
        // MenuManager is on the Canvas

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("GameScene created.");
    }

    private static void CreateGameHUD()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Add UIManager
        var uiManager = canvasObj.AddComponent<UIManager>();

        // Add MenuManager
        var menuManager = canvasObj.AddComponent<MenuManager>();

        // --- HUD Panel ---
        GameObject hudPanel = CreatePanel(canvasObj.transform, "HUDPanel", Vector2.zero, new Vector2(1920, 1080));
        var hudImg = hudPanel.GetComponent<Image>();
        hudImg.color = Color.clear;

        // Score Text (top-left)
        Text scoreText = CreateText(hudPanel.transform, "ScoreText", "SCORE: 0",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(400, 50), 28, TextAnchor.UpperLeft);
        scoreText.color = Color.white;

        // High Score Text (top-left, below score)
        Text highScoreText = CreateText(hudPanel.transform, "HighScoreText", "HIGH: 0",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -60), new Vector2(400, 40), 20, TextAnchor.UpperLeft);
        highScoreText.color = new Color(0.7f, 0.7f, 0.7f);

        // Wave Text (top-center)
        Text waveText = CreateText(hudPanel.transform, "WaveText", "WAVE 1",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -20), new Vector2(300, 50), 28, TextAnchor.UpperCenter);
        waveText.color = Color.yellow;

        // Health Bar (top-right)
        GameObject healthBarObj = CreateHealthBar(hudPanel.transform);

        // Announcement Text (center)
        Text announcementText = CreateText(hudPanel.transform, "AnnouncementText", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 50), new Vector2(600, 80), 48, TextAnchor.MiddleCenter);
        announcementText.color = Color.yellow;
        announcementText.fontStyle = FontStyle.Bold;

        // Power-Up Text (center-bottom)
        Text powerUpText = CreateText(hudPanel.transform, "PowerUpText", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -80), new Vector2(400, 40), 24, TextAnchor.MiddleCenter);
        powerUpText.color = Color.green;

        // Message Text (center)
        Text messageText = CreateText(hudPanel.transform, "MessageText", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -20), new Vector2(500, 50), 30, TextAnchor.MiddleCenter);
        messageText.color = Color.white;

        // Wire up UIManager references
        SetPrivateField(uiManager, "scoreText", scoreText);
        SetPrivateField(uiManager, "highScoreText", highScoreText);
        SetPrivateField(uiManager, "waveText", waveText);
        SetPrivateField(uiManager, "announcementText", announcementText);
        SetPrivateField(uiManager, "powerUpText", powerUpText);
        SetPrivateField(uiManager, "messageText", messageText);

        // Health bar references
        Slider healthSlider = healthBarObj.GetComponent<Slider>();
        Image fillImage = healthBarObj.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
        SetPrivateField(uiManager, "healthBar", healthSlider);
        if (fillImage != null)
            SetPrivateField(uiManager, "healthBarFill", fillImage);

        // --- Pause Menu Panel ---
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PauseMenuPanel", Vector2.zero, new Vector2(1920, 1080));
        pausePanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

        Text pauseTitle = CreateText(pausePanel.transform, "PauseTitle", "PAUSED",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 100), new Vector2(400, 80), 52, TextAnchor.MiddleCenter);
        pauseTitle.color = Color.white;
        pauseTitle.fontStyle = FontStyle.Bold;

        Button resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "RESUME",
            new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(250, 60), 28);
        Button pauseQuitBtn = CreateButton(pausePanel.transform, "QuitButton", "MAIN MENU",
            new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(250, 60), 28);

        SetPrivateField(menuManager, "pauseMenuPanel", pausePanel);
        SetPrivateField(menuManager, "resumeButton", resumeBtn);
        SetPrivateField(menuManager, "pauseQuitButton", pauseQuitBtn);

        // --- Game Over Panel ---
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", Vector2.zero, new Vector2(1920, 1080));
        gameOverPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        Text gameOverTitle = CreateText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 150), new Vector2(600, 80), 56, TextAnchor.MiddleCenter);
        gameOverTitle.color = Color.red;
        gameOverTitle.fontStyle = FontStyle.Bold;

        Text goScoreText = CreateText(gameOverPanel.transform, "GOScoreText", "SCORE: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 60), new Vector2(400, 50), 32, TextAnchor.MiddleCenter);
        goScoreText.color = Color.white;

        Text goHighScoreText = CreateText(gameOverPanel.transform, "GOHighScoreText", "HIGH SCORE: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 10), new Vector2(400, 40), 24, TextAnchor.MiddleCenter);
        goHighScoreText.color = new Color(1f, 0.85f, 0.3f);

        Text goWaveText = CreateText(gameOverPanel.transform, "GOWaveText", "WAVES: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -30), new Vector2(400, 40), 24, TextAnchor.MiddleCenter);
        goWaveText.color = Color.white;

        Text newHighText = CreateText(gameOverPanel.transform, "NewHighScoreText", "NEW HIGH SCORE!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 100), new Vector2(400, 40), 26, TextAnchor.MiddleCenter);
        newHighText.color = Color.yellow;
        newHighText.fontStyle = FontStyle.Bold;

        Button restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(250, 60), 28);
        Button menuBtn = CreateButton(gameOverPanel.transform, "MenuButton", "MAIN MENU",
            new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(250, 60), 28);

        SetPrivateField(menuManager, "gameOverPanel", gameOverPanel);
        SetPrivateField(menuManager, "gameOverScoreText", goScoreText);
        SetPrivateField(menuManager, "gameOverHighScoreText", goHighScoreText);
        SetPrivateField(menuManager, "gameOverWaveText", goWaveText);
        SetPrivateField(menuManager, "newHighScoreText", newHighText);
        SetPrivateField(menuManager, "restartButton", restartBtn);
        SetPrivateField(menuManager, "menuButton", menuBtn);
    }

    // ============================================
    // MAIN MENU SCENE
    // ============================================

    private static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Configure camera
        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.transform.position = new Vector3(0, 0, -10);

        // Background stars
        GameObject bgObj = new GameObject("BackgroundScroller");
        bgObj.AddComponent<BackgroundScroller>();

        // Game Manager
        GameObject gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GameManager>();
        GameObject powerUpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PowerUp.prefab");
        SetPrivateField(gm, "powerUpPrefab", powerUpPrefab);

        // Audio Manager
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<AudioManager>();

        // --- Main Menu Canvas ---
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        var menuManager = canvasObj.AddComponent<MenuManager>();

        // Main Menu Panel
        GameObject menuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", Vector2.zero, new Vector2(1920, 1080));
        menuPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);

        // Title
        Text titleText = CreateText(menuPanel.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 200), new Vector2(800, 120), 72, TextAnchor.MiddleCenter);
        titleText.color = new Color(0.3f, 0.8f, 1f);
        titleText.fontStyle = FontStyle.Bold;

        // Subtitle
        Text subtitleText = CreateText(menuPanel.transform, "SubtitleText", "DEFEND THE GALAXY",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 130), new Vector2(600, 50), 24, TextAnchor.MiddleCenter);
        subtitleText.color = new Color(0.7f, 0.7f, 0.9f);

        // High Score
        Text hsText = CreateText(menuPanel.transform, "HighScoreText", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 60), new Vector2(400, 40), 22, TextAnchor.MiddleCenter);
        hsText.color = new Color(1f, 0.85f, 0.3f);

        // Play Button
        Button playBtn = CreateButton(menuPanel.transform, "PlayButton", "PLAY",
            new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(300, 70), 36);

        // Quit Button
        Button quitBtn = CreateButton(menuPanel.transform, "QuitButton", "QUIT",
            new Vector2(0.5f, 0.5f), new Vector2(0, -130), new Vector2(300, 70), 36);

        // Controls info
        Text controlsText = CreateText(menuPanel.transform, "ControlsText",
            "CONTROLS: Arrow Keys / WASD to Move | SPACE to Shoot | ESC to Pause",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 60), new Vector2(800, 40), 18, TextAnchor.MiddleCenter);
        controlsText.color = new Color(0.5f, 0.5f, 0.6f);

        // Wire up MenuManager
        SetPrivateField(menuManager, "mainMenuPanel", menuPanel);
        SetPrivateField(menuManager, "playButton", playBtn);
        SetPrivateField(menuManager, "quitButton", quitBtn);
        SetPrivateField(menuManager, "titleText", titleText);
        SetPrivateField(menuManager, "mainMenuHighScoreText", hsText);

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenuScene.unity");
        Debug.Log("MainMenuScene created.");
    }

    // ============================================
    // BUILD & PROJECT SETTINGS
    // ============================================

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build settings configured with MainMenuScene (0) and GameScene (1).");
    }

    private static void ConfigureProjectSettings()
    {
        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.companyName = "SpaceShooterDev";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = false;

        // Set layers and tags
        AddTag("Player");
        AddTag("Enemy");
        AddTag("PlayerBullet");
        AddTag("EnemyBullet");
        AddTag("PowerUp");

        Debug.Log("Project settings configured.");
    }

    // ============================================
    // UI HELPER METHODS
    // ============================================

    private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.5f);

        return panel;
    }

    private static Text CreateText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size,
        int fontSize, TextAnchor alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        // Add shadow for readability
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);

        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.3f, 0.6f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.3f, 0.6f, 0.9f);
        colors.highlightedColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        colors.pressedColor = new Color(0.1f, 0.2f, 0.5f, 1f);
        colors.selectedColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        btn.colors = colors;

        // Button text
        Text text = CreateText(btnObj.transform, "Text", label,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, fontSize, TextAnchor.MiddleCenter);
        RectTransform textRt = text.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return btn;
    }

    private static GameObject CreateHealthBar(Transform parent)
    {
        // Create health bar using Unity's Slider
        GameObject sliderObj = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderObj.name = "HealthBar";
        sliderObj.transform.SetParent(parent, false);

        RectTransform rt = sliderObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-150, -30);
        rt.sizeDelta = new Vector2(250, 25);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.interactable = false;
        slider.maxValue = 100;
        slider.value = 100;

        // Remove the handle
        Transform handle = sliderObj.transform.Find("Handle Slide Area");
        if (handle != null)
            Object.DestroyImmediate(handle.gameObject);

        // Style the background
        Image bgImage = sliderObj.transform.Find("Background")?.GetComponent<Image>();
        if (bgImage != null)
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Style the fill
        Image fillImage = sliderObj.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
        if (fillImage != null)
            fillImage.color = Color.green;

        // Add "HP" label
        Text hpLabel = CreateText(parent, "HPLabel", "HP",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-290, -30), new Vector2(40, 25), 18, TextAnchor.MiddleCenter);
        hpLabel.color = Color.white;

        return sliderObj;
    }

    // ============================================
    // UTILITY
    // ============================================

    private static void LoadSprite(SpriteRenderer sr, string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
            EditorUtility.SetDirty(target as Object);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        // Check if tag already exists
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}

/// <summary>
/// Fallback component to ensure GameManager exists in GameScene
/// even when loaded directly (not through MainMenu).
/// </summary>
public class GameManagerFallback : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            var manager = gm.AddComponent<GameManager>();

            // Also create AudioManager if missing
            if (AudioManager.Instance == null)
            {
                GameObject am = new GameObject("AudioManager");
                am.AddComponent<AudioManager>();
            }
        }

        // Start spawning if we loaded directly into GameScene
        Invoke(nameof(AutoStart), 0.5f);
    }

    private void AutoStart()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.MainMenu)
        {
            // We loaded directly, start the game
            GameManager.Instance.StartGame();
        }

        Destroy(gameObject);
    }
}
