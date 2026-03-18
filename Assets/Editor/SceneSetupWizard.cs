using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor wizard that automatically generates the complete game scene and prefabs.
/// Access from Unity menu: Tools > Space Shooter > Setup Game Scene
/// This eliminates the need for manual drag-and-drop setup.
/// </summary>
public class SceneSetupWizard
{
    // ─────────────────────────────────────────────
    //  MENU: Create Everything (One-Click Setup)
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Space Shooter/1. Setup ENTIRE Game (One Click)", false, 0)]
    static void SetupEntireGame()
    {
        // Step 1: Create all prefabs
        CreateAllPrefabs();

        // Step 2: Create Main Menu scene
        CreateMainMenuScene();

        // Step 3: Create Game scene
        CreateGameScene();

        // Step 4: Configure build settings
        SetupBuildSettings();

        EditorUtility.DisplayDialog("Setup Complete!",
            "The Space Shooter game has been fully set up!\n\n" +
            "Scenes created:\n" +
            "  • MainMenu (Assets/Scenes/MainMenu.unity)\n" +
            "  • GameScene (Assets/Scenes/GameScene.unity)\n\n" +
            "Prefabs created in Assets/Prefabs/\n\n" +
            "To play: Open MainMenu scene and press Play.",
            "Awesome!");

        // Open MainMenu scene
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
    }

    // ─────────────────────────────────────────────
    //  STEP 1: Create Prefabs
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Space Shooter/2. Create Prefabs Only", false, 20)]
    static void CreateAllPrefabs()
    {
        // Ensure directories exist
        EnsureDirectory("Assets/Prefabs");
        EnsureDirectory("Assets/Materials");

        CreatePlayerBulletPrefab();
        CreateEnemyBulletPrefab();
        CreateEnemyPrefab();
        CreateHealthPowerUpPrefab();
        CreateRapidFirePowerUpPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SceneSetupWizard] All prefabs created successfully.");
    }

    static void CreatePlayerBulletPrefab()
    {
        GameObject bullet = new GameObject("PlayerBullet");
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite(8, 20, Color.cyan);
        sr.sortingOrder = 5;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.08f, 0.2f);

        Bullet b = bullet.AddComponent<Bullet>();
        b.damage = 1;
        b.isPlayerBullet = true;
        b.lifetime = 4f;

        bullet.transform.localScale = Vector3.one;

        PrefabUtility.SaveAsPrefabAsset(bullet, "Assets/Prefabs/PlayerBullet.prefab");
        Object.DestroyImmediate(bullet);
    }

    static void CreateEnemyBulletPrefab()
    {
        GameObject bullet = new GameObject("EnemyBullet");
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite(8, 20, Color.red);
        sr.sortingOrder = 5;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.08f, 0.2f);

        Bullet b = bullet.AddComponent<Bullet>();
        b.damage = 1;
        b.isPlayerBullet = false;
        b.lifetime = 4f;

        bullet.transform.localScale = Vector3.one;

        PrefabUtility.SaveAsPrefabAsset(bullet, "Assets/Prefabs/EnemyBullet.prefab");
        Object.DestroyImmediate(bullet);
    }

    static void CreateEnemyPrefab()
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite(Color.red);
        sr.sortingOrder = 3;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);

        Enemy e = enemy.AddComponent<Enemy>();
        e.moveSpeed = 3f;
        e.maxHealth = 2;
        e.canShoot = true;
        e.fireRate = 2f;
        e.bulletSpeed = 6f;
        e.scoreValue = 100;
        e.powerUpDropChance = 0.15f;

        // Reference the enemy bullet prefab (will be linked after creation)
        GameObject enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyBullet.prefab");
        if (enemyBulletPrefab != null)
        {
            e.bulletPrefab = enemyBulletPrefab;
        }

        enemy.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        // Rotate 180 degrees so the triangle points downward
        enemy.transform.rotation = Quaternion.Euler(0, 0, 180);

        PrefabUtility.SaveAsPrefabAsset(enemy, "Assets/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(enemy);
    }

    static void CreateHealthPowerUpPrefab()
    {
        GameObject powerUp = new GameObject("HealthPowerUp");
        powerUp.tag = "PowerUp";

        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite(24, 24, Color.green);
        sr.sortingOrder = 4;

        CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        PowerUp p = powerUp.AddComponent<PowerUp>();
        p.type = PowerUp.PowerUpType.Health;
        p.healAmount = 2;
        p.driftSpeed = 2f;

        powerUp.transform.localScale = Vector3.one;

        PrefabUtility.SaveAsPrefabAsset(powerUp, "Assets/Prefabs/HealthPowerUp.prefab");
        Object.DestroyImmediate(powerUp);
    }

    static void CreateRapidFirePowerUpPrefab()
    {
        GameObject powerUp = new GameObject("RapidFirePowerUp");
        powerUp.tag = "PowerUp";

        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite(24, 24, Color.yellow);
        sr.sortingOrder = 4;

        CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        PowerUp p = powerUp.AddComponent<PowerUp>();
        p.type = PowerUp.PowerUpType.RapidFire;
        p.driftSpeed = 2f;

        powerUp.transform.localScale = Vector3.one;

        PrefabUtility.SaveAsPrefabAsset(powerUp, "Assets/Prefabs/RapidFirePowerUp.prefab");
        Object.DestroyImmediate(powerUp);
    }

    // ─────────────────────────────────────────────
    //  STEP 2: Create Main Menu Scene
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Space Shooter/3. Create Main Menu Scene", false, 40)]
    static void CreateMainMenuScene()
    {
        EnsureDirectory("Assets/Scenes");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Setup camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.1f);
        }

        // Create Canvas
        GameObject canvas = CreateCanvas("MainMenuCanvas");

        // Title text
        GameObject titleObj = CreateUIText(canvas, "TitleText", "SPACE SHOOTER",
            new Vector2(0, 120), new Vector2(500, 80), 48, Color.white, TextAnchor.MiddleCenter);

        // Subtitle
        CreateUIText(canvas, "SubtitleText", "Defend the Galaxy!",
            new Vector2(0, 60), new Vector2(400, 40), 20, Color.cyan, TextAnchor.MiddleCenter);

        // Start Button
        GameObject startBtn = CreateUIButton(canvas, "StartButton", "START GAME",
            new Vector2(0, -30), new Vector2(250, 60), new Color(0.1f, 0.6f, 0.1f));

        // Quit Button
        GameObject quitBtn = CreateUIButton(canvas, "QuitButton", "QUIT",
            new Vector2(0, -110), new Vector2(250, 60), new Color(0.6f, 0.1f, 0.1f));

        // MainMenuUI script
        GameObject menuManager = new GameObject("MainMenuUI");
        MainMenuUI menuUI = menuManager.AddComponent<MainMenuUI>();
        menuUI.startButton = startBtn.GetComponent<Button>();
        menuUI.quitButton = quitBtn.GetComponent<Button>();
        menuUI.titleText = titleObj.GetComponent<Text>();

        // Event System
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("[SceneSetupWizard] MainMenu scene created.");
    }

    // ─────────────────────────────────────────────
    //  STEP 3: Create Game Scene
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Space Shooter/4. Create Game Scene", false, 60)]
    static void CreateGameScene()
    {
        EnsureDirectory("Assets/Scenes");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ── Camera ──
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.gameObject.AddComponent<CameraSetup>();
        }

        // ── Player ──
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3.5f, 0);

        SpriteRenderer playerSR = player.AddComponent<SpriteRenderer>();
        playerSR.sprite = CreateTriangleSprite(new Color(0.2f, 0.8f, 1f));
        playerSR.sortingOrder = 3;
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        BoxCollider2D playerCol = player.AddComponent<BoxCollider2D>();
        playerCol.isTrigger = true;
        playerCol.size = new Vector2(0.6f, 0.6f);

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 8f;
        pc.fireRate = 0.3f;
        pc.bulletSpeed = 12f;
        pc.maxHealth = 5;

        // Fire point (child of player)
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.parent = player.transform;
        firePoint.transform.localPosition = new Vector3(0, 0.5f, 0);
        pc.firePoint = firePoint.transform;

        // Link player bullet prefab
        GameObject playerBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerBullet.prefab");
        if (playerBulletPrefab != null)
        {
            pc.bulletPrefab = playerBulletPrefab;
        }

        // ── Enemy Spawner ──
        GameObject spawner = new GameObject("EnemySpawner");
        EnemySpawner es = spawner.AddComponent<EnemySpawner>();
        es.initialSpawnRate = 2f;
        es.minimumSpawnRate = 0.5f;
        es.spawnRateDecrease = 0.05f;
        es.spawnYPosition = 6.5f;
        es.spawnXRange = 7f;

        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        if (enemyPrefab != null)
        {
            es.enemyPrefabs = new GameObject[] { enemyPrefab };
        }

        // ── Game Manager ──
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        // Link power-up prefabs
        GameObject healthPU = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HealthPowerUp.prefab");
        GameObject rapidPU = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RapidFirePowerUp.prefab");
        gm.powerUpPrefabs = new GameObject[] { healthPU, rapidPU };

        // ── UI ──
        GameObject canvas = CreateCanvas("GameCanvas");

        // Score text (top-left)
        GameObject scoreText = CreateUIText(canvas, "ScoreText", "SCORE: 0",
            new Vector2(-10, -10), new Vector2(300, 50), 28, Color.white, TextAnchor.UpperLeft);
        RectTransform scoreRT = scoreText.GetComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0, 1);
        scoreRT.anchorMax = new Vector2(0, 1);
        scoreRT.pivot = new Vector2(0, 1);
        scoreRT.anchoredPosition = new Vector2(15, -10);

        // Health text (top-right)
        GameObject healthText = CreateUIText(canvas, "HealthText", "HP: 5 / 5",
            new Vector2(10, -10), new Vector2(300, 50), 28, Color.green, TextAnchor.UpperRight);
        RectTransform healthRT = healthText.GetComponent<RectTransform>();
        healthRT.anchorMin = new Vector2(1, 1);
        healthRT.anchorMax = new Vector2(1, 1);
        healthRT.pivot = new Vector2(1, 1);
        healthRT.anchoredPosition = new Vector2(-15, -10);

        // ── Game Over Panel ──
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);
        RectTransform goPanelRT = gameOverPanel.AddComponent<RectTransform>();
        goPanelRT.anchorMin = Vector2.zero;
        goPanelRT.anchorMax = Vector2.one;
        goPanelRT.sizeDelta = Vector2.zero;

        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.75f);

        // "GAME OVER" text
        CreateUIText(gameOverPanel, "GameOverText", "GAME OVER",
            new Vector2(0, 80), new Vector2(400, 70), 52, Color.red, TextAnchor.MiddleCenter);

        // Final score text
        GameObject finalScoreText = CreateUIText(gameOverPanel, "FinalScoreText", "FINAL SCORE: 0",
            new Vector2(0, 10), new Vector2(400, 50), 30, Color.white, TextAnchor.MiddleCenter);

        // Restart button
        GameObject restartBtn = CreateUIButton(gameOverPanel, "RestartButton", "RESTART",
            new Vector2(0, -60), new Vector2(220, 55), new Color(0.1f, 0.5f, 0.1f));

        // Main Menu button
        GameObject menuBtn = CreateUIButton(gameOverPanel, "MainMenuButton", "MAIN MENU",
            new Vector2(0, -130), new Vector2(220, 55), new Color(0.3f, 0.3f, 0.6f));

        gameOverPanel.SetActive(false); // Hidden at start

        // ── UI Manager ──
        GameObject uiMgrObj = new GameObject("UIManager");
        UIManager uiMgr = uiMgrObj.AddComponent<UIManager>();
        uiMgr.scoreText = scoreText.GetComponent<Text>();
        uiMgr.healthText = healthText.GetComponent<Text>();
        uiMgr.gameOverPanel = gameOverPanel;
        uiMgr.finalScoreText = finalScoreText.GetComponent<Text>();
        uiMgr.restartButton = restartBtn.GetComponent<Button>();
        uiMgr.mainMenuButton = menuBtn.GetComponent<Button>();

        // Event System
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("[SceneSetupWizard] GameScene created.");
    }

    // ─────────────────────────────────────────────
    //  STEP 4: Build Settings
    // ─────────────────────────────────────────────

    [MenuItem("Tools/Space Shooter/5. Configure Build Settings", false, 80)]
    static void SetupBuildSettings()
    {
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };
        Debug.Log("[SceneSetupWizard] Build settings configured (MainMenu + GameScene).");
    }

    // ─────────────────────────────────────────────
    //  HELPER: Create UI Elements
    // ─────────────────────────────────────────────

    static GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    static GameObject CreateUIText(GameObject parent, string name, string text,
        Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text t = textObj.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = alignment;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null)
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        // Add outline for readability
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return textObj;
    }

    static GameObject CreateUIButton(GameObject parent, string name, string label,
        Vector2 position, Vector2 size, Color buttonColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = buttonColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonColor * 1.2f;
        colors.pressedColor = buttonColor * 0.8f;
        btn.colors = colors;

        // Button label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRT = labelObj.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.sizeDelta = Vector2.zero;

        Text t = labelObj.AddComponent<Text>();
        t.text = label;
        t.fontSize = 24;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null)
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontStyle = FontStyle.Bold;

        return btnObj;
    }

    static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ─────────────────────────────────────────────
    //  HELPER: Create Simple Sprites Programmatically
    // ─────────────────────────────────────────────

    static Sprite CreateRectSprite(int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    static Sprite CreateTriangleSprite(Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear to transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Draw a filled triangle (pointing up)
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth = (int)(progress * size / 2);
            int center = size / 2;
            for (int x = center - halfWidth; x <= center + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    pixels[y * size + x] = color;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // ─────────────────────────────────────────────
    //  HELPER: Ensure directory exists
    // ─────────────────────────────────────────────

    static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
