using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// GameSetupWizard is an Editor-only tool that automates the creation of
/// all game objects, prefabs, materials, and scene hierarchy.
/// Access via Unity menu: Tools > Space Shooter > Setup Game
/// </summary>
public class GameSetupWizard : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Setup Complete Game")]
    public static void ShowWindow()
    {
        GetWindow<GameSetupWizard>("Space Shooter Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Space Shooter Game Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This wizard creates all prefabs, materials, and scenes.", EditorStyles.wordWrappedLabel);
        GUILayout.Label("Click each button in order:", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Step 1: Create Materials", GUILayout.Height(30)))
            CreateMaterials();

        GUILayout.Space(5);

        if (GUILayout.Button("Step 2: Create Prefabs", GUILayout.Height(30)))
            CreatePrefabs();

        GUILayout.Space(5);

        if (GUILayout.Button("Step 3: Setup MainMenu Scene", GUILayout.Height(30)))
            SetupMainMenuScene();

        GUILayout.Space(5);

        if (GUILayout.Button("Step 4: Setup GamePlay Scene", GUILayout.Height(30)))
            SetupGamePlayScene();

        GUILayout.Space(5);

        if (GUILayout.Button("Step 5: Configure Build Settings", GUILayout.Height(30)))
            ConfigureBuildSettings();

        GUILayout.Space(15);
        EditorGUILayout.HelpBox(
            "After running all steps, add both scenes to Build Settings:\n" +
            "File > Build Settings > Add Open Scenes\n" +
            "MainMenu should be index 0, GamePlay index 1.",
            MessageType.Info);
    }

    // ============================================================
    // STEP 1: MATERIALS
    // ============================================================
    static void CreateMaterials()
    {
        string matPath = "Assets/Materials/";
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        CreateColorMaterial(matPath + "PlayerMat.mat", new Color(0.2f, 0.8f, 1f)); // cyan
        CreateColorMaterial(matPath + "BasicEnemyMat.mat", new Color(1f, 0.3f, 0.3f)); // red
        CreateColorMaterial(matPath + "FastEnemyMat.mat", new Color(1f, 0.8f, 0.2f)); // yellow
        CreateColorMaterial(matPath + "TankEnemyMat.mat", new Color(0.8f, 0.2f, 0.8f)); // purple
        CreateColorMaterial(matPath + "PlayerBulletMat.mat", new Color(0.5f, 1f, 0.5f)); // green
        CreateColorMaterial(matPath + "EnemyBulletMat.mat", new Color(1f, 0.5f, 0.2f)); // orange
        CreateColorMaterial(matPath + "HealthPowerUpMat.mat", new Color(0.2f, 1f, 0.2f)); // bright green
        CreateColorMaterial(matPath + "RapidFirePowerUpMat.mat", new Color(1f, 1f, 0.2f)); // bright yellow
        CreateColorMaterial(matPath + "ShieldPowerUpMat.mat", new Color(0.4f, 0.6f, 1f)); // light blue
        CreateColorMaterial(matPath + "ShieldBubbleMat.mat", new Color(0.3f, 0.7f, 1f, 0.4f)); // transparent blue
        CreateColorMaterial(matPath + "BackgroundMat.mat", new Color(0.05f, 0.05f, 0.15f)); // dark space
        CreateColorMaterial(matPath + "StarMat.mat", new Color(0.8f, 0.8f, 0.9f)); // dim white

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Space Shooter Setup] Materials created successfully!");
    }

    static void CreateColorMaterial(string path, Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
    }

    // ============================================================
    // STEP 2: PREFABS
    // ============================================================
    static void CreatePrefabs()
    {
        string prefabPath = "Assets/Prefabs/";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Create all prefabs
        CreatePlayerPrefab(prefabPath);
        CreateBulletPrefab(prefabPath, "PlayerBullet", "PlayerBullet", "Assets/Materials/PlayerBulletMat.mat");
        CreateBulletPrefab(prefabPath, "EnemyBullet", "EnemyBullet", "Assets/Materials/EnemyBulletMat.mat");
        CreateEnemyPrefab(prefabPath, "BasicEnemy", EnemyController.EnemyType.Basic, "Assets/Materials/BasicEnemyMat.mat");
        CreateEnemyPrefab(prefabPath, "FastEnemy", EnemyController.EnemyType.Fast, "Assets/Materials/FastEnemyMat.mat");
        CreateEnemyPrefab(prefabPath, "TankEnemy", EnemyController.EnemyType.Tank, "Assets/Materials/TankEnemyMat.mat");
        CreatePowerUpPrefab(prefabPath, "HealthPowerUp", PowerUpController.PowerUpType.Health, "Assets/Materials/HealthPowerUpMat.mat");
        CreatePowerUpPrefab(prefabPath, "RapidFirePowerUp", PowerUpController.PowerUpType.RapidFire, "Assets/Materials/RapidFirePowerUpMat.mat");
        CreatePowerUpPrefab(prefabPath, "ShieldPowerUp", PowerUpController.PowerUpType.Shield, "Assets/Materials/ShieldPowerUpMat.mat");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Space Shooter Setup] Prefabs created successfully!");
    }

    static void CreatePlayerPrefab(string basePath)
    {
        // Player ship: a triangle-ish shape (we'll use a quad and describe how to make it triangular)
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");

        // Sprite Renderer with a white square (will be tinted by material)
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Material playerMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PlayerMat.mat");
        if (playerMat != null) sr.color = playerMat.color;
        sr.sortingOrder = 5;
        player.transform.localScale = new Vector3(0.6f, 0.8f, 1f);

        // Collider
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        // Rigidbody2D (kinematic - we handle movement ourselves)
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Scripts
        player.AddComponent<PlayerController>();
        player.AddComponent<HealthSystem>();
        CollisionHandler ch = player.AddComponent<CollisionHandler>();
        ch.isPlayer = true;

        // Shield visual child
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        shield.transform.localScale = new Vector3(2f, 2f, 1f);
        SpriteRenderer shieldSr = shield.AddComponent<SpriteRenderer>();
        shieldSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Material shieldMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShieldBubbleMat.mat");
        if (shieldMat != null) shieldSr.color = new Color(0.3f, 0.7f, 1f, 0.3f);
        shieldSr.sortingOrder = 6;
        shield.SetActive(false);

        // Fire point child
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(player, basePath + "Player.prefab");
        DestroyImmediate(player);
    }

    static void CreateBulletPrefab(string basePath, string name, string tag, string matPath)
    {
        GameObject bullet = new GameObject(name);
        // Tag will be set at runtime by the shooter

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null) sr.color = mat.color;
        sr.sortingOrder = 3;
        bullet.transform.localScale = new Vector3(0.15f, 0.3f, 1f);

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        bullet.AddComponent<BulletController>();

        PrefabUtility.SaveAsPrefabAsset(bullet, basePath + name + ".prefab");
        DestroyImmediate(bullet);
    }

    static void CreateEnemyPrefab(string basePath, string name, EnemyController.EnemyType type, string matPath)
    {
        GameObject enemy = new GameObject(name);

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null) sr.color = mat.color;
        sr.sortingOrder = 4;

        // Different sizes per type
        switch (type)
        {
            case EnemyController.EnemyType.Basic:
                enemy.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                break;
            case EnemyController.EnemyType.Fast:
                enemy.transform.localScale = new Vector3(0.35f, 0.45f, 1f);
                break;
            case EnemyController.EnemyType.Tank:
                enemy.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                break;
        }

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        enemy.AddComponent<HealthSystem>();
        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.enemyType = type;
        CollisionHandler ch = enemy.AddComponent<CollisionHandler>();
        ch.isPlayer = false;

        PrefabUtility.SaveAsPrefabAsset(enemy, basePath + name + ".prefab");
        DestroyImmediate(enemy);
    }

    static void CreatePowerUpPrefab(string basePath, string name, PowerUpController.PowerUpType type, string matPath)
    {
        GameObject powerUp = new GameObject(name);

        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null) sr.color = mat.color;
        sr.sortingOrder = 7;
        powerUp.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody2D rb = powerUp.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        PowerUpController pc = powerUp.AddComponent<PowerUpController>();
        pc.type = type;

        PrefabUtility.SaveAsPrefabAsset(powerUp, basePath + name + ".prefab");
        DestroyImmediate(powerUp);
    }

    // ============================================================
    // STEP 3: MAIN MENU SCENE
    // ============================================================
    static void SetupMainMenuScene()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Configure camera
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.1f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;

        // Create Canvas
        GameObject canvas = CreateUICanvas();

        // Title
        CreateUIText(canvas.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0, 100), 48, Color.cyan, FontStyle.Bold);

        // Subtitle
        CreateUIText(canvas.transform, "SubtitleText", "Defend the Galaxy",
            new Vector2(0, 40), 20, Color.white, FontStyle.Italic);

        // High Score
        GameObject highScoreObj = CreateUIText(canvas.transform, "HighScoreText", "HIGH SCORE: 0",
            new Vector2(0, -20), 18, Color.yellow, FontStyle.Normal);

        // Start Button
        GameObject startBtn = CreateUIButton(canvas.transform, "StartButton", "START GAME",
            new Vector2(0, -80), new Color(0.2f, 0.7f, 0.2f));

        // Quit Button
        GameObject quitBtn = CreateUIButton(canvas.transform, "QuitButton", "QUIT",
            new Vector2(0, -140), new Color(0.7f, 0.2f, 0.2f));

        // Controls info
        CreateUIText(canvas.transform, "ControlsText",
            "WASD/Arrows: Move  |  Space: Shoot",
            new Vector2(0, -200), 14, new Color(0.7f, 0.7f, 0.7f), FontStyle.Normal);

        // Add MainMenuController
        MainMenuController mmc = canvas.AddComponent<MainMenuController>();
        mmc.startButton = startBtn.GetComponent<Button>();
        mmc.quitButton = quitBtn.GetComponent<Button>();
        mmc.highScoreText = highScoreObj.GetComponent<Text>();

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("[Space Shooter Setup] MainMenu scene created!");
    }

    // ============================================================
    // STEP 4: GAMEPLAY SCENE
    // ============================================================
    static void SetupGamePlayScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Configure camera
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.1f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;

        // -- GAME MANAGER --
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        // Load prefabs and assign references
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        gm.playerPrefab = playerPrefab;

        // Player spawn point
        GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.position = new Vector3(0f, -3.5f, 0f);
        gm.playerSpawnPoint = spawnPoint.transform;

        // -- ENEMY SPAWNER --
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
        spawner.basicEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BasicEnemy.prefab");
        spawner.fastEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FastEnemy.prefab");
        spawner.tankEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TankEnemy.prefab");

        // Set bullet prefab and power-up prefabs on enemy prefabs
        // (This needs to be done on the prefab assets themselves)
        AssignEnemyPrefabReferences();
        AssignPlayerPrefabReferences();

        // -- BACKGROUND --
        SetupBackground();

        // -- HUD CANVAS --
        SetupHUDCanvas();

        // -- GAME OVER CANVAS --
        SetupGameOverCanvas();

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GamePlay.unity");
        Debug.Log("[Space Shooter Setup] GamePlay scene created!");
    }

    static void AssignEnemyPrefabReferences()
    {
        string[] enemyPaths = {
            "Assets/Prefabs/BasicEnemy.prefab",
            "Assets/Prefabs/FastEnemy.prefab",
            "Assets/Prefabs/TankEnemy.prefab"
        };

        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyBullet.prefab");
        GameObject[] powerUpPrefabs = {
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HealthPowerUp.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RapidFirePowerUp.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ShieldPowerUp.prefab")
        };

        foreach (string path in enemyPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                EnemyController ec = prefab.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.bulletPrefab = bulletPrefab;
                    ec.powerUpPrefabs = powerUpPrefabs;
                    EditorUtility.SetDirty(prefab);
                }
            }
        }
    }

    static void AssignPlayerPrefabReferences()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (prefab != null)
        {
            PlayerController pc = prefab.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerBullet.prefab");

                // Find shield visual and fire point in children
                Transform shieldVis = prefab.transform.Find("ShieldVisual");
                if (shieldVis != null)
                    pc.shieldVisual = shieldVis.gameObject;

                Transform fp = prefab.transform.Find("FirePoint");
                if (fp != null)
                    pc.firePoint = fp;

                EditorUtility.SetDirty(prefab);
            }
        }
    }

    static void SetupBackground()
    {
        // Create background scroller
        GameObject bgParent = new GameObject("BackgroundScroller");
        BackgroundScroller scroller = bgParent.AddComponent<BackgroundScroller>();

        // Create two background quads
        GameObject bg1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg1.name = "Background1";
        bg1.transform.SetParent(bgParent.transform);
        bg1.transform.position = new Vector3(0, 0, 10);
        bg1.transform.localScale = new Vector3(20, 12, 1);
        var bg1Renderer = bg1.GetComponent<MeshRenderer>();
        Material bgMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BackgroundMat.mat");
        if (bgMat != null) bg1Renderer.material = bgMat;
        bg1Renderer.sortingOrder = -10;
        // Remove the default collider from quad
        DestroyImmediate(bg1.GetComponent<MeshCollider>());

        GameObject bg2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg2.name = "Background2";
        bg2.transform.SetParent(bgParent.transform);
        bg2.transform.position = new Vector3(0, 12, 10);
        bg2.transform.localScale = new Vector3(20, 12, 1);
        var bg2Renderer = bg2.GetComponent<MeshRenderer>();
        if (bgMat != null) bg2Renderer.material = bgMat;
        bg2Renderer.sortingOrder = -10;
        DestroyImmediate(bg2.GetComponent<MeshCollider>());

        scroller.background1 = bg1.transform;
        scroller.background2 = bg2.transform;
        scroller.tileHeight = 12f;
    }

    static void SetupHUDCanvas()
    {
        GameObject canvas = CreateUICanvas();
        canvas.name = "HUDCanvas";

        // Score text (top left)
        GameObject scoreObj = CreateUIText(canvas.transform, "ScoreText", "SCORE: 0",
            new Vector2(-300, 220), 22, Color.white, FontStyle.Bold);

        // Wave text (top center)
        GameObject waveObj = CreateUIText(canvas.transform, "WaveText", "WAVE: 1",
            new Vector2(0, 220), 22, Color.cyan, FontStyle.Bold);

        // Health text (top right)
        GameObject healthObj = CreateUIText(canvas.transform, "HealthText", "HP: 3 / 3",
            new Vector2(300, 220), 22, Color.green, FontStyle.Bold);

        // Wave announcement (center, large)
        GameObject waveAnnounce = CreateUIText(canvas.transform, "WaveAnnounceText", "WAVE 1",
            new Vector2(0, 0), 48, Color.yellow, FontStyle.Bold);

        // Power-up indicators
        GameObject rapidInd = CreateUIText(canvas.transform, "RapidFireIndicator", "\u26A1 RAPID FIRE",
            new Vector2(-300, 190), 14, Color.yellow, FontStyle.Bold);

        GameObject shieldInd = CreateUIText(canvas.transform, "ShieldIndicator", "\u26E8 SHIELD",
            new Vector2(-300, 170), 14, new Color(0.4f, 0.7f, 1f), FontStyle.Bold);

        // Add HUDController
        HUDController hud = canvas.AddComponent<HUDController>();
        hud.scoreText = scoreObj.GetComponent<Text>();
        hud.waveText = waveObj.GetComponent<Text>();
        hud.healthText = healthObj.GetComponent<Text>();
        hud.waveAnnounceText = waveAnnounce.GetComponent<Text>();
        hud.rapidFireIndicator = rapidInd;
        hud.shieldIndicator = shieldInd;
    }

    static void SetupGameOverCanvas()
    {
        GameObject canvas = CreateUICanvas();
        canvas.name = "GameOverCanvas";

        // Panel background
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.8f);

        // Title
        GameObject titleObj = CreateUIText(panel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0, 100), 48, Color.red, FontStyle.Bold);

        // Final Score
        GameObject scoreObj = CreateUIText(panel.transform, "FinalScoreText", "SCORE: 0",
            new Vector2(0, 40), 28, Color.white, FontStyle.Normal);

        // High Score
        GameObject highObj = CreateUIText(panel.transform, "HighScoreText", "HIGH SCORE: 0",
            new Vector2(0, 0), 22, Color.yellow, FontStyle.Normal);

        // New High Score text
        GameObject newHighObj = CreateUIText(panel.transform, "NewHighScoreText", "\u2605 NEW HIGH SCORE! \u2605",
            new Vector2(0, -30), 20, Color.magenta, FontStyle.Bold);

        // Buttons
        GameObject restartBtn = CreateUIButton(panel.transform, "RestartButton", "RESTART",
            new Vector2(0, -80), new Color(0.2f, 0.7f, 0.2f));

        GameObject menuBtn = CreateUIButton(panel.transform, "MainMenuButton", "MAIN MENU",
            new Vector2(0, -130), new Color(0.3f, 0.5f, 0.8f));

        GameObject quitBtn = CreateUIButton(panel.transform, "QuitButton", "QUIT",
            new Vector2(0, -180), new Color(0.7f, 0.2f, 0.2f));

        // Add GameOverController
        GameOverController goc = canvas.AddComponent<GameOverController>();
        goc.gameOverPanel = panel;
        goc.titleText = titleObj.GetComponent<Text>();
        goc.finalScoreText = scoreObj.GetComponent<Text>();
        goc.highScoreText = highObj.GetComponent<Text>();
        goc.newHighScoreText = newHighObj.GetComponent<Text>();
        goc.restartButton = restartBtn.GetComponent<Button>();
        goc.mainMenuButton = menuBtn.GetComponent<Button>();
        goc.quitButton = quitBtn.GetComponent<Button>();
    }

    // ============================================================
    // STEP 5: BUILD SETTINGS
    // ============================================================
    static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GamePlay.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[Space Shooter Setup] Build settings configured! MainMenu=0, GamePlay=1");
    }

    // ============================================================
    // UI HELPER METHODS
    // ============================================================

    static GameObject CreateUICanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem (only one per scene)
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    static GameObject CreateUIText(Transform parent, string name, string content,
        Vector2 position, int fontSize, Color color, FontStyle style)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(500, 60);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return textObj;
    }

    static GameObject CreateUIButton(Transform parent, string name, string label,
        Vector2 position, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 40);

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = bgColor * 1.2f;
        colors.pressedColor = bgColor * 0.8f;
        btn.colors = colors;

        // Button text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.fontSize = 20;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return btnObj;
    }
}
