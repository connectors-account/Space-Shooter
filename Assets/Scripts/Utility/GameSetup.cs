using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click game setup wizard.
/// Run from the Unity Editor menu: Tools > Setup Complete Game
/// Creates all scenes, prefabs, and configures the build settings automatically.
/// 
/// Prerequisites: Run "Tools > Generate Game Sprites" first.
/// </summary>
public static class GameSetup
{
    private static string spritesPath = "Assets/Sprites/";
    private static string prefabsPath = "Assets/Prefabs/";
    private static string scenesPath = "Assets/Scenes/";

    [MenuItem("Tools/Setup Complete Game")]
    public static void SetupAll()
    {
        // Ensure directories exist
        EnsureDirectory("Assets/Prefabs");
        EnsureDirectory("Assets/Scenes");
        EnsureDirectory("Assets/Sprites");
        EnsureDirectory("Assets/Materials");

        CreatePrefabs();
        CreateGameScene();
        CreateMainMenuScene();
        ConfigureBuildSettings();

        Debug.Log("=== GAME SETUP COMPLETE ===");
        Debug.Log("1. Open Assets/Scenes/MainMenu to start playing");
        Debug.Log("2. Press Play in the editor to test");
    }

    static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    // ─── PREFABS ───────────────────────────────────────────────────────

    static void CreatePrefabs()
    {
        CreateBulletPrefab();
        CreateExplosionPrefab();
        CreatePowerUpPrefabs();
        CreateEnemyPrefabs();
        CreatePlayerPrefab();

        AssetDatabase.Refresh();
        Debug.Log("All prefabs created.");
    }

    static Sprite LoadSprite(string name)
    {
        string path = spritesPath + name + ".png";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning("Sprite not found: " + path + " — run 'Tools > Generate Game Sprites' first.");
        }
        return sprite;
    }

    static void CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("Bullet");
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Bullet");
        sr.sortingOrder = 5;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.4f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        bullet.AddComponent<BulletController>();
        bullet.tag = "PlayerBullet";

        PrefabUtility.SaveAsPrefabAsset(bullet, prefabsPath + "Bullet.prefab");
        Object.DestroyImmediate(bullet);
    }

    static void CreateExplosionPrefab()
    {
        GameObject explosion = new GameObject("Explosion");
        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Explosion");
        sr.sortingOrder = 10;

        explosion.AddComponent<ExplosionEffect>();

        PrefabUtility.SaveAsPrefabAsset(explosion, prefabsPath + "Explosion.prefab");
        Object.DestroyImmediate(explosion);
    }

    static void CreatePowerUpPrefabs()
    {
        string[] names = { "PowerUp_Shield", "PowerUp_RapidFire", "PowerUp_SpreadShot" };
        PowerUpType[] types = { PowerUpType.Shield, PowerUpType.RapidFire, PowerUpType.SpreadShot };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject pu = new GameObject(names[i]);
            SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("PowerUp");
            sr.sortingOrder = 6;

            CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;

            Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;

            PowerUpController pc = pu.AddComponent<PowerUpController>();
            pc.powerUpType = types[i];

            pu.tag = "PowerUp";

            PrefabUtility.SaveAsPrefabAsset(pu, prefabsPath + names[i] + ".prefab");
            Object.DestroyImmediate(pu);
        }
    }

    static void CreateEnemyPrefabs()
    {
        GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Explosion.prefab");
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Bullet.prefab");

        CreateSingleEnemyPrefab("Enemy_Straight", "EnemyStraight", EnemyType.StraightDown, 1, 3f, 100, explosionPrefab, bulletPrefab);
        CreateSingleEnemyPrefab("Enemy_Zigzag", "EnemyZigzag", EnemyType.Zigzag, 2, 2.5f, 150, explosionPrefab, bulletPrefab);
        CreateSingleEnemyPrefab("Enemy_Tracker", "EnemyTracker", EnemyType.Tracker, 3, 2f, 200, explosionPrefab, bulletPrefab);
    }

    static void CreateSingleEnemyPrefab(string prefabName, string spriteName, EnemyType type,
        int health, float speed, int score, GameObject explosionPrefab, GameObject bulletPrefab)
    {
        GameObject enemy = new GameObject(prefabName);
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spriteName);
        sr.sortingOrder = 3;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.enemyType = type;
        ec.health = health;
        ec.moveSpeed = speed;
        ec.scoreValue = score;
        ec.canShoot = true;
        ec.bulletPrefab = bulletPrefab;
        ec.explosionPrefab = explosionPrefab;
        ec.fireRate = type == EnemyType.Tracker ? 1.5f : 2.5f;
        ec.bulletSpeed = 5f;
        ec.powerUpDropChance = 0.2f;

        enemy.tag = "Enemy";

        PrefabUtility.SaveAsPrefabAsset(enemy, prefabsPath + prefabName + ".prefab");
        Object.DestroyImmediate(enemy);
    }

    static void CreatePlayerPrefab()
    {
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Bullet.prefab");
        GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Explosion.prefab");

        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, -3.5f, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Player");
        sr.sortingOrder = 4;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.bulletPrefab = bulletPrefab;
        pc.explosionPrefab = explosionPrefab;
        pc.moveSpeed = 8f;
        pc.fireRate = 0.25f;
        pc.bulletSpeed = 12f;

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0, 0.5f, 0);
        pc.firePoint = firePoint.transform;

        // Shield visual
        GameObject shieldObj = new GameObject("ShieldVisual");
        shieldObj.transform.SetParent(player.transform);
        shieldObj.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSr = shieldObj.AddComponent<SpriteRenderer>();
        shieldSr.sprite = LoadSprite("Shield");
        shieldSr.sortingOrder = 5;
        shieldObj.SetActive(false);
        pc.shieldVisual = shieldObj;

        player.tag = "Player";

        PrefabUtility.SaveAsPrefabAsset(player, prefabsPath + "Player.prefab");
        Object.DestroyImmediate(player);
    }

    // ─── GAME SCENE ────────────────────────────────────────────────────

    static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5;
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        Camera.main.transform.position = new Vector3(0, 0, -10);

        // Load prefabs
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Player.prefab");
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Bullet.prefab");
        GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Explosion.prefab");
        GameObject enemyStraightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Enemy_Straight.prefab");
        GameObject enemyZigzagPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Enemy_Zigzag.prefab");
        GameObject enemyTrackerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "Enemy_Tracker.prefab");
        GameObject shieldPU = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "PowerUp_Shield.prefab");
        GameObject rapidPU = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "PowerUp_RapidFire.prefab");
        GameObject spreadPU = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "PowerUp_SpreadShot.prefab");

        // Instantiate player
        if (playerPrefab != null)
        {
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(0, -3.5f, 0);
        }

        // Create Background
        CreateBackground();

        // GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.totalWaves = 10;

        // SpawnManager
        GameObject smObj = new GameObject("SpawnManager");
        SpawnManager sm = smObj.AddComponent<SpawnManager>();
        sm.enemyStraightPrefab = enemyStraightPrefab;
        sm.enemyZigzagPrefab = enemyZigzagPrefab;
        sm.enemyTrackerPrefab = enemyTrackerPrefab;
        sm.shieldPowerUpPrefab = shieldPU;
        sm.rapidFirePowerUpPrefab = rapidPU;
        sm.spreadShotPowerUpPrefab = spreadPU;
        sm.totalWaves = 10;

        // AudioManager
        GameObject amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();

        // Create UI Canvas
        CreateGameUI();

        EditorSceneManager.SaveScene(scene, scenesPath + "Game.unity");
        Debug.Log("Game scene created.");
    }

    static void CreateBackground()
    {
        // Background layer 1 (slower, further)
        GameObject bg1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg1.name = "Background_Far";
        bg1.transform.position = new Vector3(0, 0, 5);
        bg1.transform.localScale = new Vector3(12, 12, 1);

        // Remove 3D collider
        Object.DestroyImmediate(bg1.GetComponent<MeshCollider>());

        BackgroundScroller scroller1 = bg1.AddComponent<BackgroundScroller>();
        scroller1.scrollSpeed = 0.3f;

        // Background layer 2 (faster, closer)
        GameObject bg2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg2.name = "Background_Near";
        bg2.transform.position = new Vector3(0, 0, 4);
        bg2.transform.localScale = new Vector3(12, 12, 1);

        Object.DestroyImmediate(bg2.GetComponent<MeshCollider>());

        BackgroundScroller scroller2 = bg2.AddComponent<BackgroundScroller>();
        scroller2.scrollSpeed = 0.6f;

        // Try to set material from star background texture
        Texture2D bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritesPath + "StarBackground.png");
        if (bgTex != null)
        {
            Material mat1 = new Material(Shader.Find("Unlit/Transparent"));
            if (mat1.shader == null || mat1.shader.name == "Hidden/InternalErrorShader")
                mat1 = new Material(Shader.Find("Unlit/Texture"));
            mat1.mainTexture = bgTex;
            mat1.mainTextureScale = new Vector2(2, 2);
            AssetDatabase.CreateAsset(mat1, "Assets/Materials/Background_Far.mat");
            bg1.GetComponent<Renderer>().material = mat1;

            Material mat2 = new Material(Shader.Find("Unlit/Transparent"));
            if (mat2.shader == null || mat2.shader.name == "Hidden/InternalErrorShader")
                mat2 = new Material(Shader.Find("Unlit/Texture"));
            mat2.mainTexture = bgTex;
            mat2.mainTextureScale = new Vector2(3, 3);
            Color semiTransparent = new Color(1, 1, 1, 0.5f);
            mat2.color = semiTransparent;
            AssetDatabase.CreateAsset(mat2, "Assets/Materials/Background_Near.mat");
            bg2.GetComponent<Renderer>().material = mat2;
        }
    }

    static void CreateGameUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // UIManager
        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // --- HUD ---
        // Score text
        GameObject scoreObj = CreateUIText(canvasObj.transform, "ScoreText", "Score: 0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(300, 40));
        uiManager.scoreText = scoreObj.GetComponent<Text>();

        // Wave text
        GameObject waveObj = CreateUIText(canvasObj.transform, "WaveText", "Wave: 1",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(200, 40));
        waveObj.GetComponent<Text>().alignment = TextAnchor.UpperCenter;
        uiManager.waveText = waveObj.GetComponent<Text>();

        // Health icons
        Image[] healthIcons = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject healthObj = new GameObject("Health_" + i);
            healthObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rt = healthObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-120 + i * 40, -30);
            rt.sizeDelta = new Vector2(30, 30);
            Image img = healthObj.AddComponent<Image>();
            img.color = Color.green;
            healthIcons[i] = img;
        }
        uiManager.healthIcons = healthIcons;

        // --- Game Over Panel ---
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0, 0, 0, 0.8f));
        uiManager.gameOverPanel = gameOverPanel;

        GameObject goTitle = CreateUIText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(400, 60));
        goTitle.GetComponent<Text>().fontSize = 48;
        goTitle.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        uiManager.gameOverTitleText = goTitle.GetComponent<Text>();

        GameObject goScore = CreateUIText(gameOverPanel.transform, "FinalScore", "Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(300, 40));
        goScore.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        uiManager.finalScoreText = goScore.GetComponent<Text>();

        GameObject goHighScore = CreateUIText(gameOverPanel.transform, "FinalHighScore", "High Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(300, 40));
        goHighScore.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        uiManager.finalHighScoreText = goHighScore.GetComponent<Text>();

        GameObject restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "Restart",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-100, -70), new Vector2(160, 45));
        uiManager.restartButton = restartBtn.GetComponent<Button>();

        GameObject menuBtn = CreateButton(gameOverPanel.transform, "MainMenuButton", "Main Menu",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(100, -70), new Vector2(160, 45));
        uiManager.mainMenuButton = menuBtn.GetComponent<Button>();

        gameOverPanel.SetActive(false);

        // --- Pause Panel ---
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0, 0, 0, 0.7f));
        uiManager.pausePanel = pausePanel;

        CreateUIText(pausePanel.transform, "PausedText", "PAUSED",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(300, 60))
            .GetComponent<Text>().fontSize = 48;

        GameObject resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "Resume",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(160, 45));
        uiManager.resumeButton = resumeBtn.GetComponent<Button>();

        GameObject pauseMenuBtn = CreateButton(pausePanel.transform, "PauseMainMenuButton", "Main Menu",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -65), new Vector2(160, 45));
        uiManager.pauseMainMenuButton = pauseMenuBtn.GetComponent<Button>();

        pausePanel.SetActive(false);

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ─── MAIN MENU SCENE ──────────────────────────────────────────────

    static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5;
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.1f);

        // Canvas
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        MainMenu menu = canvasObj.AddComponent<MainMenu>();

        // Title
        GameObject titleObj = CreateUIText(canvasObj.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(600, 80));
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.fontSize = 64;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(0.3f, 0.8f, 1f);
        menu.titleText = titleTxt;

        // High Score
        GameObject hsObj = CreateUIText(canvasObj.transform, "HighScoreText", "High Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(400, 40));
        hsObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        menu.highScoreText = hsObj.GetComponent<Text>();

        // Start Button
        GameObject startBtn = CreateButton(canvasObj.transform, "StartButton", "START GAME",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(250, 55));
        menu.startButton = startBtn.GetComponent<Button>();

        // Quit Button
        GameObject quitBtn = CreateButton(canvasObj.transform, "QuitButton", "QUIT",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(250, 55));
        menu.quitButton = quitBtn.GetComponent<Button>();

        // Instructions
        GameObject instrObj = CreateUIText(canvasObj.transform, "InstructionsText", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -170), new Vector2(400, 120));
        Text instrTxt = instrObj.GetComponent<Text>();
        instrTxt.fontSize = 18;
        instrTxt.alignment = TextAnchor.MiddleCenter;
        instrTxt.color = new Color(0.7f, 0.7f, 0.7f);
        menu.instructionsText = instrTxt;

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Background scroller
        CreateBackground();

        EditorSceneManager.SaveScene(scene, scenesPath + "MainMenu.unity");
        Debug.Log("MainMenu scene created.");
    }

    // ─── BUILD SETTINGS ────────────────────────────────────────────────

    static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenesPath + "MainMenu.unity", true),
            new EditorBuildSettingsScene(scenesPath + "Game.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build settings configured with MainMenu and Game scenes.");
    }

    // ─── UI HELPERS ────────────────────────────────────────────────────

    static GameObject CreateUIText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Text txt = obj.AddComponent<Text>();
        txt.text = content;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null)
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 24;
        txt.color = Color.white;
        txt.alignment = TextAnchor.UpperLeft;

        return obj;
    }

    static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.4f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.5f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.6f);
        btn.colors = colors;

        // Button text
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform txtRt = txtObj.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        Text txt = txtObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null)
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 22;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;

        return btnObj;
    }

    static GameObject CreatePanel(Transform parent, string name, Color bgColor)
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
}
#endif
