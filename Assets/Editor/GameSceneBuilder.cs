using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool that automatically builds the complete game scenes with all
/// GameObjects, components, and settings configured. Run from the Unity
/// menu: Tools > Space Shooter > Build All Scenes.
/// </summary>
public class GameSceneBuilder : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Build All Scenes")]
    public static void BuildAllScenes()
    {
        BuildGameScene();
        BuildMainMenuScene();
        AddScenesToBuildSettings();
        Debug.Log("All scenes built successfully! Open MainMenu scene to play.");
    }

    [MenuItem("Tools/Space Shooter/Build Game Scene Only")]
    public static void BuildGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Setup camera
        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 6;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1f);
        cam.transform.position = new Vector3(0, 0, -10);

        // Create sprite assets
        Sprite playerSprite = CreatePlayerSprite();
        Sprite droneSprite = CreateDroneSprite();
        Sprite fighterSprite = CreateFighterSprite();
        Sprite bomberSprite = CreateBomberSprite();
        Sprite bulletSprite = CreateBulletSprite();
        Sprite enemyBulletSprite = CreateEnemyBulletSprite();
        Sprite powerUpRapidSprite = CreatePowerUpSprite(Color.yellow);
        Sprite powerUpShieldSprite = CreatePowerUpSprite(Color.cyan);
        Sprite powerUpHealthSprite = CreatePowerUpSprite(Color.green);
        Sprite shieldVisualSprite = CreateShieldVisualSprite();

        // Create prefabs
        GameObject playerBulletPrefab = CreatePlayerBulletPrefab(bulletSprite);
        GameObject enemyBulletPrefab = CreateEnemyBulletPrefab(enemyBulletSprite);
        GameObject dronePrefab = CreateEnemyPrefab<EnemyDrone>("EnemyDrone", droneSprite, new Color(1f, 0.3f, 0.3f), enemyBulletPrefab);
        GameObject fighterPrefab = CreateEnemyPrefab<EnemyFighter>("EnemyFighter", fighterSprite, new Color(1f, 0.6f, 0.1f), enemyBulletPrefab);
        GameObject bomberPrefab = CreateEnemyPrefab<EnemyBomber>("EnemyBomber", bomberSprite, new Color(0.8f, 0.2f, 0.8f), enemyBulletPrefab);
        GameObject rapidFirePrefab = CreatePowerUpPrefab("PowerUp_RapidFire", powerUpRapidSprite, PowerUp.PowerUpType.RapidFire);
        GameObject shieldPrefab = CreatePowerUpPrefab("PowerUp_Shield", powerUpShieldSprite, PowerUp.PowerUpType.Shield);
        GameObject healthPrefab = CreatePowerUpPrefab("PowerUp_Health", powerUpHealthSprite, PowerUp.PowerUpType.HealthRestore);

        // Save prefabs to disk
        EnsureFolder("Assets/Prefabs");
        string playerBulletPath = "Assets/Prefabs/PlayerBullet.prefab";
        string enemyBulletPath = "Assets/Prefabs/EnemyBullet.prefab";
        string dronePath = "Assets/Prefabs/EnemyDrone.prefab";
        string fighterPath = "Assets/Prefabs/EnemyFighter.prefab";
        string bomberPath = "Assets/Prefabs/EnemyBomber.prefab";
        string rapidPath = "Assets/Prefabs/PowerUp_RapidFire.prefab";
        string shieldPath = "Assets/Prefabs/PowerUp_Shield.prefab";
        string healthPath = "Assets/Prefabs/PowerUp_Health.prefab";

        PrefabUtility.SaveAsPrefabAsset(playerBulletPrefab, playerBulletPath);
        PrefabUtility.SaveAsPrefabAsset(enemyBulletPrefab, enemyBulletPath);
        PrefabUtility.SaveAsPrefabAsset(dronePrefab, dronePath);
        PrefabUtility.SaveAsPrefabAsset(fighterPrefab, fighterPath);
        PrefabUtility.SaveAsPrefabAsset(bomberPrefab, bomberPath);
        PrefabUtility.SaveAsPrefabAsset(rapidFirePrefab, rapidPath);
        PrefabUtility.SaveAsPrefabAsset(shieldPrefab, shieldPath);
        PrefabUtility.SaveAsPrefabAsset(healthPrefab, healthPath);

        // Clean up temporary objects
        DestroyImmediate(playerBulletPrefab);
        DestroyImmediate(enemyBulletPrefab);
        DestroyImmediate(dronePrefab);
        DestroyImmediate(fighterPrefab);
        DestroyImmediate(bomberPrefab);
        DestroyImmediate(rapidFirePrefab);
        DestroyImmediate(shieldPrefab);
        DestroyImmediate(healthPrefab);

        // Load saved prefabs for references
        GameObject playerBulletAsset = AssetDatabase.LoadAssetAtPath<GameObject>(playerBulletPath);
        GameObject enemyBulletAsset = AssetDatabase.LoadAssetAtPath<GameObject>(enemyBulletPath);
        GameObject droneAsset = AssetDatabase.LoadAssetAtPath<GameObject>(dronePath);
        GameObject fighterAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fighterPath);
        GameObject bomberAsset = AssetDatabase.LoadAssetAtPath<GameObject>(bomberPath);
        GameObject rapidAsset = AssetDatabase.LoadAssetAtPath<GameObject>(rapidPath);
        GameObject shieldAsset = AssetDatabase.LoadAssetAtPath<GameObject>(shieldPath);
        GameObject healthAsset = AssetDatabase.LoadAssetAtPath<GameObject>(healthPath);

        // Create Player
        GameObject player = CreatePlayer(playerSprite, shieldVisualSprite, playerBulletAsset);

        // Create Managers
        CreateGameManager();
        CreateEnemySpawner(droneAsset, fighterAsset, bomberAsset);
        CreatePowerUpSpawner(rapidAsset, shieldAsset, healthAsset);
        CreateAudioManager();

        // Create Background
        GameObject bg = new GameObject("ParallaxBackground");
        bg.AddComponent<ParallaxBackground>();

        // Create UI Canvas
        CreateGameUI();

        // Setup Tags and Layers
        SetupTags();

        // Save scene
        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("Game Scene built successfully!");
    }

    [MenuItem("Tools/Space Shooter/Build Main Menu Scene")]
    public static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 6;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1f);
        cam.transform.position = new Vector3(0, 0, -10);

        // Background stars
        GameObject bg = new GameObject("ParallaxBackground");
        bg.AddComponent<ParallaxBackground>();

        // Audio Manager (if it doesn't exist)
        CreateAudioManager();

        // UI Canvas
        CreateMainMenuUI();

        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("Main Menu Scene built successfully!");
    }

    private static void SetupTags()
    {
        // Tags must be set up in the Tag Manager.
        // This logs instructions since tags require the TagManager asset.
        Debug.Log("IMPORTANT: Please ensure these tags exist in Edit > Project Settings > Tags and Layers:");
        Debug.Log("  - Player");
        Debug.Log("  - PlayerBullet");
        Debug.Log("  - EnemyBullet");
        Debug.Log("  - Enemy");
        Debug.Log("  - PowerUp");
    }

    private static GameObject CreatePlayer(Sprite sprite, Sprite shieldSprite, GameObject bulletPrefab)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");
        player.transform.position = new Vector3(0, -4, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.2f, 0.8f, 1f);
        sr.sortingOrder = 10;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        player.AddComponent<AudioSource>();

        PlayerController pc = player.AddComponent<PlayerController>();

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);

        // Shield visual
        GameObject shieldObj = new GameObject("ShieldVisual");
        shieldObj.transform.SetParent(player.transform);
        shieldObj.transform.localPosition = Vector3.zero;
        shieldObj.transform.localScale = Vector3.one * 2f;
        SpriteRenderer shieldSR = shieldObj.AddComponent<SpriteRenderer>();
        shieldSR.sprite = shieldSprite;
        shieldSR.color = new Color(0.3f, 0.8f, 1f, 0.3f);
        shieldSR.sortingOrder = 11;
        shieldObj.SetActive(false);

        // Set serialized fields via SerializedObject
        SerializedObject so = new SerializedObject(pc);
        so.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
        so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        so.FindProperty("shieldVisual").objectReferenceValue = shieldObj;
        so.ApplyModifiedProperties();

        player.transform.localScale = Vector3.one * 0.8f;

        return player;
    }

    private static GameObject CreatePlayerBulletPrefab(Sprite sprite)
    {
        GameObject bullet = new GameObject("PlayerBullet");
        bullet.tag = "PlayerBullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.3f, 1f, 0.3f);
        sr.sortingOrder = 5;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.4f);

        Bullet b = bullet.AddComponent<Bullet>();

        bullet.transform.localScale = Vector3.one * 0.4f;

        return bullet;
    }

    private static GameObject CreateEnemyBulletPrefab(Sprite sprite)
    {
        GameObject bullet = new GameObject("EnemyBullet");
        bullet.tag = "EnemyBullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 0.3f, 0.3f);
        sr.sortingOrder = 5;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.4f);

        Bullet b = bullet.AddComponent<Bullet>();
        SerializedObject so = new SerializedObject(b);
        so.FindProperty("direction").vector2Value = Vector2.down;
        so.FindProperty("speed").floatValue = 8f;
        so.ApplyModifiedProperties();

        bullet.transform.localScale = Vector3.one * 0.35f;

        return bullet;
    }

    private static GameObject CreateEnemyPrefab<T>(string name, Sprite sprite, Color color, GameObject bulletPrefab) where T : EnemyBase
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = 8;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.7f);

        enemy.AddComponent<AudioSource>();

        T enemyScript = enemy.AddComponent<T>();

        SerializedObject so = new SerializedObject(enemyScript);
        so.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
        so.ApplyModifiedProperties();

        enemy.transform.localScale = Vector3.one * 0.7f;

        return enemy;
    }

    private static GameObject CreatePowerUpPrefab(string name, Sprite sprite, PowerUp.PowerUpType type)
    {
        GameObject powerUp = new GameObject(name);
        powerUp.tag = "PowerUp";

        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 7;

        switch (type)
        {
            case PowerUp.PowerUpType.RapidFire:
                sr.color = new Color(1f, 1f, 0.2f);
                break;
            case PowerUp.PowerUpType.Shield:
                sr.color = new Color(0.3f, 0.8f, 1f);
                break;
            case PowerUp.PowerUpType.HealthRestore:
                sr.color = new Color(0.3f, 1f, 0.3f);
                break;
        }

        Rigidbody2D rb = powerUp.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        PowerUp pu = powerUp.AddComponent<PowerUp>();
        SerializedObject so = new SerializedObject(pu);
        so.FindProperty("type").enumValueIndex = (int)type;
        so.ApplyModifiedProperties();

        powerUp.transform.localScale = Vector3.one * 0.5f;

        return powerUp;
    }

    private static void CreateGameManager()
    {
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
    }

    private static void CreateEnemySpawner(GameObject drone, GameObject fighter, GameObject bomber)
    {
        GameObject spawner = new GameObject("EnemySpawner");
        EnemySpawner es = spawner.AddComponent<EnemySpawner>();

        SerializedObject so = new SerializedObject(es);
        so.FindProperty("dronePrefab").objectReferenceValue = drone;
        so.FindProperty("fighterPrefab").objectReferenceValue = fighter;
        so.FindProperty("bomberPrefab").objectReferenceValue = bomber;
        so.ApplyModifiedProperties();
    }

    private static void CreatePowerUpSpawner(GameObject rapid, GameObject shield, GameObject health)
    {
        GameObject spawner = new GameObject("PowerUpSpawner");
        PowerUpSpawner pus = spawner.AddComponent<PowerUpSpawner>();

        SerializedObject so = new SerializedObject(pus);
        so.FindProperty("rapidFirePrefab").objectReferenceValue = rapid;
        so.FindProperty("shieldPrefab").objectReferenceValue = shield;
        so.FindProperty("healthRestorePrefab").objectReferenceValue = health;
        so.ApplyModifiedProperties();
    }

    private static void CreateAudioManager()
    {
        GameObject am = new GameObject("AudioManager");
        am.AddComponent<AudioManager>();
    }

    private static void CreateGameUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // HUD Panel
        GameObject hudPanel = CreatePanel(canvasObj.transform, "HUDPanel", Color.clear);

        Text scoreText = CreateText(hudPanel.transform, "ScoreText", "SCORE: 0",
            TextAnchor.UpperLeft, new Vector2(20, -20), new Vector2(300, 50), 28);

        Text waveText = CreateText(hudPanel.transform, "WaveText", "WAVE 1",
            TextAnchor.UpperCenter, new Vector2(0, -20), new Vector2(300, 50), 32);

        Text healthText = CreateText(hudPanel.transform, "HealthText", "HP: 5/5",
            TextAnchor.UpperRight, new Vector2(-20, -20), new Vector2(300, 50), 28);

        // Pause Panel
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0, 0, 0, 0.7f));

        CreateText(pausePanel.transform, "PauseTitle", "PAUSED",
            TextAnchor.MiddleCenter, new Vector2(0, 80), new Vector2(400, 80), 48);

        Button resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "RESUME",
            new Vector2(0, -10), new Vector2(250, 60));

        Button pauseMenuBtn = CreateButton(pausePanel.transform, "PauseMainMenuButton", "MAIN MENU",
            new Vector2(0, -90), new Vector2(250, 60));

        pausePanel.SetActive(false);

        // Game Over Panel
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0, 0, 0, 0.8f));

        CreateText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            TextAnchor.MiddleCenter, new Vector2(0, 120), new Vector2(500, 80), 56);

        Text finalScoreText = CreateText(gameOverPanel.transform, "FinalScoreText", "FINAL SCORE\n0",
            TextAnchor.MiddleCenter, new Vector2(0, 30), new Vector2(500, 100), 36);

        Button restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -70), new Vector2(250, 60));

        Button goMenuBtn = CreateButton(gameOverPanel.transform, "GameOverMainMenuButton", "MAIN MENU",
            new Vector2(0, -150), new Vector2(250, 60));

        gameOverPanel.SetActive(false);

        // Wire up UIManager
        UIManager uiMgr = canvasObj.AddComponent<UIManager>();
        SerializedObject so = new SerializedObject(uiMgr);
        so.FindProperty("hudPanel").objectReferenceValue = hudPanel;
        so.FindProperty("scoreText").objectReferenceValue = scoreText;
        so.FindProperty("waveText").objectReferenceValue = waveText;
        so.FindProperty("healthText").objectReferenceValue = healthText;
        so.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        so.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
        so.FindProperty("pauseMainMenuButton").objectReferenceValue = pauseMenuBtn;
        so.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        so.FindProperty("finalScoreText").objectReferenceValue = finalScoreText;
        so.FindProperty("restartButton").objectReferenceValue = restartBtn;
        so.FindProperty("gameOverMainMenuButton").objectReferenceValue = goMenuBtn;
        so.ApplyModifiedProperties();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private static void CreateMainMenuUI()
    {
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        Text titleText = CreateText(canvasObj.transform, "TitleText", "SPACE SHOOTER",
            TextAnchor.MiddleCenter, new Vector2(0, 100), new Vector2(600, 100), 64);
        titleText.color = new Color(0.3f, 0.8f, 1f);

        Text subtitleText = CreateText(canvasObj.transform, "SubtitleText", "Defend the Galaxy",
            TextAnchor.MiddleCenter, new Vector2(0, 30), new Vector2(400, 50), 24);
        subtitleText.color = new Color(0.7f, 0.7f, 0.8f);

        Button startBtn = CreateButton(canvasObj.transform, "StartButton", "START GAME",
            new Vector2(0, -60), new Vector2(300, 70));

        Button quitBtn = CreateButton(canvasObj.transform, "QuitButton", "QUIT",
            new Vector2(0, -150), new Vector2(300, 70));

        Text controlsText = CreateText(canvasObj.transform, "ControlsText",
            "Controls: WASD/Arrows = Move | Space = Shoot | Esc = Pause",
            TextAnchor.MiddleCenter, new Vector2(0, -250), new Vector2(800, 40), 18);
        controlsText.color = new Color(0.5f, 0.5f, 0.6f);

        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();
        SerializedObject so = new SerializedObject(menuUI);
        so.FindProperty("startButton").objectReferenceValue = startBtn;
        so.FindProperty("quitButton").objectReferenceValue = quitBtn;
        so.FindProperty("titleText").objectReferenceValue = titleText;
        so.ApplyModifiedProperties();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private static void AddScenesToBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build settings updated with MainMenu and GameScene.");
    }

    // =========== UI Helper Methods ===========

    private static GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        if (bgColor.a > 0)
        {
            Image img = panel.AddComponent<Image>();
            img.color = bgColor;
        }

        return panel;
    }

    private static Text CreateText(Transform parent, string name, string content,
        TextAnchor alignment, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Set anchors based on alignment
        switch (alignment)
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
            default:
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;
        }

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);

        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
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

        // Button label text
        Text text = CreateText(btnObj.transform, "Label", label,
            TextAnchor.MiddleCenter, Vector2.zero, size, 24);
        text.raycastTarget = false;
        RectTransform textRT = text.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btn;
    }

    // =========== Sprite Creation Methods ===========

    private static Sprite CreatePlayerSprite()
    {
        int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = Color.clear;

        // Clear texture
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        // Draw a spaceship shape (triangle with wings)
        Color body = Color.white;
        Color wing = new Color(0.8f, 0.8f, 0.8f);
        Color cockpit = new Color(0.5f, 0.8f, 1f);

        // Main body (vertical strip)
        for (int y = 4; y < 28; y++)
            for (int x = 13; x < 19; x++)
                tex.SetPixel(x, y, body);

        // Nose (triangle top)
        for (int y = 24; y < 32; y++)
        {
            int spread = (32 - y) / 2;
            for (int x = 16 - spread; x <= 16 + spread; x++)
                if (x >= 0 && x < w) tex.SetPixel(x, y, body);
        }

        // Wings
        for (int y = 6; y < 18; y++)
        {
            int wingSpread = (18 - y) / 2 + 4;
            for (int x = 16 - wingSpread; x < 13; x++)
                if (x >= 0) tex.SetPixel(x, y, wing);
            for (int x = 19; x <= 16 + wingSpread; x++)
                if (x < w) tex.SetPixel(x, y, wing);
        }

        // Cockpit
        for (int y = 18; y < 24; y++)
            for (int x = 14; x < 18; x++)
                tex.SetPixel(x, y, cockpit);

        // Engine glow
        Color engine = new Color(1f, 0.5f, 0f);
        for (int x = 14; x < 18; x++)
            for (int y = 2; y < 6; y++)
                tex.SetPixel(x, y, engine);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/Player.png");
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 32);
    }

    private static Sprite CreateDroneSprite()
    {
        int w = 24, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, Color.clear);

        Color body = Color.white;
        // Simple diamond shape
        int cx = 12, cy = 12;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int dist = Mathf.Abs(x - cx) + Mathf.Abs(y - cy);
                if (dist <= 10)
                    tex.SetPixel(x, y, body);
            }

        // Red eye
        for (int x = 10; x < 14; x++)
            for (int y = 10; y < 14; y++)
                tex.SetPixel(x, y, new Color(1f, 0.2f, 0.2f));

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/Drone.png");
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 24);
    }

    private static Sprite CreateFighterSprite()
    {
        int w = 28, h = 28;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, Color.clear);

        Color body = Color.white;
        // Inverted triangle (enemy ship pointing down)
        for (int y = 8; y < 28; y++)
        {
            int spread = (y - 8) * 14 / 20;
            for (int x = 14 - spread; x <= 14 + spread; x++)
                if (x >= 0 && x < w) tex.SetPixel(x, y, body);
        }

        // Forward fins
        for (int y = 0; y < 12; y++)
        {
            tex.SetPixel(4, y, body);
            tex.SetPixel(5, y, body);
            tex.SetPixel(22, y, body);
            tex.SetPixel(23, y, body);
        }

        // Orange cockpit
        for (int y = 14; y < 22; y++)
            for (int x = 12; x < 16; x++)
                tex.SetPixel(x, y, new Color(1f, 0.6f, 0.1f));

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/Fighter.png");
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 28);
    }

    private static Sprite CreateBomberSprite()
    {
        int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, Color.clear);

        Color body = Color.white;
        // Large hexagonal shape
        int cx = 16, cy = 16;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist <= 14)
                    tex.SetPixel(x, y, body);
            }

        // Purple inner circle
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist <= 8)
                    tex.SetPixel(x, y, new Color(0.8f, 0.3f, 0.8f));
            }

        // Eyes
        tex.SetPixel(12, 18, Color.red);
        tex.SetPixel(13, 18, Color.red);
        tex.SetPixel(19, 18, Color.red);
        tex.SetPixel(20, 18, Color.red);

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/Bomber.png");
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 32);
    }

    private static Sprite CreateBulletSprite()
    {
        int w = 8, h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, Color.clear);

        // Elongated bullet
        for (int y = 2; y < 14; y++)
            for (int x = 2; x < 6; x++)
                tex.SetPixel(x, y, Color.white);

        // Bright tip
        for (int x = 2; x < 6; x++)
        {
            tex.SetPixel(x, 14, new Color(1f, 1f, 0.8f));
            tex.SetPixel(x, 15, new Color(1f, 1f, 0.5f));
        }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/Bullet.png");
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 16);
    }

    private static Sprite CreateEnemyBulletSprite()
    {
        int w = 8, h = 12;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, Color.clear);

        // Round bullet
        float cx = 4f, cy = 6f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist <= 3.5f)
                    tex.SetPixel(x, y, Color.white);
            }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/EnemyBullet.png");
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 12);
    }

    private static Sprite CreatePowerUpSprite(Color tint)
    {
        int w = 16, h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, Color.clear);

        // Star/diamond shape
        int cx = 8, cy = 8;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int dist = Mathf.Abs(x - cx) + Mathf.Abs(y - cy);
                if (dist <= 7)
                    tex.SetPixel(x, y, Color.white);
            }

        // Inner glow
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist <= 3)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 1f));
            }

        tex.Apply();
        string colorName = tint == Color.yellow ? "RapidFire" : tint == Color.cyan ? "Shield" : "Health";
        SaveSprite(tex, $"Assets/Sprites/PowerUp_{colorName}.png");
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 16);
    }

    private static Sprite CreateShieldVisualSprite()
    {
        int res = 32;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = res / 2f;
        float outerRadius = center - 1;
        float innerRadius = outerRadius - 3;

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= outerRadius && dist >= innerRadius)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.6f));
                else
                    tex.SetPixel(x, y, Color.clear);
            }

        tex.Apply();
        SaveSprite(tex, "Assets/Sprites/Shield.png");
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f, res);
    }

    private static void SaveSprite(Texture2D tex, string path)
    {
        EnsureFolder(System.IO.Path.GetDirectoryName(path));
        byte[] pngData = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        AssetDatabase.ImportAsset(path);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path);
            string folder = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
