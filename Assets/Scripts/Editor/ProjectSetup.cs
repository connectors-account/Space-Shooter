#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Automated project setup wizard.
/// Run from Unity menu: Tools > Space Shooter > Setup Entire Project
/// This creates all scenes, prefabs, tags, layers, and configures everything.
/// </summary>
public class ProjectSetup : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Setup Entire Project")]
    public static void SetupProject()
    {
        Debug.Log("=== Space Shooter Project Setup Starting ===");

        // Step 1: Create tags
        SetupTags();

        // Step 2: Generate sprites
        SpriteGenerator.GenerateAllSprites();
        AssetDatabase.Refresh();

        // Step 3: Create prefabs
        CreateAllPrefabs();
        AssetDatabase.Refresh();

        // Step 4: Create scenes
        CreateMainMenuScene();
        CreateGamePlayScene();
        CreateGameOverScene();

        // Step 5: Configure build settings
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("=== Space Shooter Project Setup Complete! ===");
        Debug.Log("You can now press Play to test, or build via File > Build Settings.");

        EditorUtility.DisplayDialog("Setup Complete!",
            "Space Shooter project has been set up successfully!\n\n" +
            "- 3 Scenes created (MainMenu, GamePlay, GameOver)\n" +
            "- All prefabs created\n" +
            "- All sprites generated\n" +
            "- Build settings configured\n\n" +
            "Press Play to test or use File > Build Settings to build.",
            "OK");
    }

    private static void SetupTags()
    {
        AddTag("Player");
        AddTag("Enemy");
        AddTag("PlayerBullet");
        AddTag("EnemyBullet");
        AddTag("PowerUp");
        AddTag("Background");
        Debug.Log("Tags configured.");
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    // ========== PREFAB CREATION ==========

    private static void CreateAllPrefabs()
    {
        string prefabPath = "Assets/Prefabs";
        if (!Directory.Exists(prefabPath))
            Directory.CreateDirectory(prefabPath);

        CreatePlayerBulletPrefab(prefabPath);
        CreateEnemyBulletPrefab(prefabPath);
        CreatePowerUpPrefabs(prefabPath);
        CreateEnemyPrefabs(prefabPath);
        CreatePlayerPrefab(prefabPath);

        Debug.Log("All prefabs created.");
    }

    private static void CreatePlayerBulletPrefab(string path)
    {
        GameObject bullet = new GameObject("PlayerBullet");
        bullet.tag = "PlayerBullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerBullet.png");
        sr.sortingOrder = 5;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.4f);

        BulletController bc = bullet.AddComponent<BulletController>();
        bc.speed = 14f;
        bc.damage = 10;
        bc.isPlayerBullet = true;
        bc.direction = Vector2.up;

        PrefabUtility.SaveAsPrefabAsset(bullet, path + "/PlayerBullet.prefab");
        Object.DestroyImmediate(bullet);
    }

    private static void CreateEnemyBulletPrefab(string path)
    {
        GameObject bullet = new GameObject("EnemyBullet");
        bullet.tag = "EnemyBullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/EnemyBullet.png");
        sr.sortingOrder = 5;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.4f);

        BulletController bc = bullet.AddComponent<BulletController>();
        bc.speed = 8f;
        bc.damage = 10;
        bc.isPlayerBullet = false;
        bc.direction = Vector2.down;

        PrefabUtility.SaveAsPrefabAsset(bullet, path + "/EnemyBullet.prefab");
        Object.DestroyImmediate(bullet);
    }

    private static void CreatePowerUpPrefabs(string path)
    {
        CreateSinglePowerUp(path, "PowerUpHealth", PowerUpType.Health, "Assets/Sprites/PowerUpHealth.png");
        CreateSinglePowerUp(path, "PowerUpRapidFire", PowerUpType.RapidFire, "Assets/Sprites/PowerUpRapidFire.png");
        CreateSinglePowerUp(path, "PowerUpShield", PowerUpType.Shield, "Assets/Sprites/PowerUpShield.png");
    }

    private static void CreateSinglePowerUp(string path, string name, PowerUpType type, string spritePath)
    {
        GameObject powerUp = new GameObject(name);
        powerUp.tag = "PowerUp";

        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        sr.sortingOrder = 4;

        CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        PowerUpController pc = powerUp.AddComponent<PowerUpController>();
        pc.powerUpType = type;

        PrefabUtility.SaveAsPrefabAsset(powerUp, path + "/" + name + ".prefab");
        Object.DestroyImmediate(powerUp);
    }

    private static void CreateEnemyPrefabs(string path)
    {
        // Load shared assets
        GameObject enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path + "/EnemyBullet.prefab");
        GameObject[] powerUpPrefabs = new GameObject[]
        {
            AssetDatabase.LoadAssetAtPath<GameObject>(path + "/PowerUpHealth.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(path + "/PowerUpRapidFire.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(path + "/PowerUpShield.prefab")
        };

        CreateSingleEnemy(path, "EnemyStraight", EnemyType.Straight,
            "Assets/Sprites/EnemyStraight.png", enemyBulletPrefab, powerUpPrefabs);
        CreateSingleEnemy(path, "EnemyZigzag", EnemyType.Zigzag,
            "Assets/Sprites/EnemyZigzag.png", enemyBulletPrefab, powerUpPrefabs);
        CreateSingleEnemy(path, "EnemySwooper", EnemyType.Swooper,
            "Assets/Sprites/EnemySwooper.png", enemyBulletPrefab, powerUpPrefabs);
        CreateSingleEnemy(path, "EnemyTank", EnemyType.Tank,
            "Assets/Sprites/EnemyTank.png", enemyBulletPrefab, powerUpPrefabs);
    }

    private static void CreateSingleEnemy(string path, string name, EnemyType type,
        string spritePath, GameObject bulletPrefab, GameObject[] powerUpPrefabs)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        sr.sortingOrder = 3;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.enemyType = type;
        ec.bulletPrefab = bulletPrefab;
        ec.powerUpPrefabs = powerUpPrefabs;

        PrefabUtility.SaveAsPrefabAsset(enemy, path + "/" + name + ".prefab");
        Object.DestroyImmediate(enemy);
    }

    private static void CreatePlayerPrefab(string path)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerShip.png");
        sr.sortingOrder = 10;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.7f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path + "/PlayerBullet.prefab");

        // Create fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        pc.firePoint = firePoint.transform;

        // Create shield visual
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSR = shield.AddComponent<SpriteRenderer>();
        shieldSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ShieldBubble.png");
        shieldSR.sortingOrder = 11;
        shield.SetActive(false);
        pc.shieldVisual = shield;

        PrefabUtility.SaveAsPrefabAsset(player, path + "/Player.prefab");
        Object.DestroyImmediate(player);
    }

    // ========== SCENE CREATION ==========

    private static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.15f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
        }

        // GameManager (persistent)
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // AudioManager (persistent)
        GameObject amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();

        // Canvas
        GameObject canvas = CreateCanvas();

        // Title
        GameObject titleObj = CreateUIText(canvas, "TitleText", "SPACE SHOOTER",
            new Vector2(0, 100), 48, Color.white, TextAnchor.MiddleCenter);

        // High Score
        GameObject highScoreObj = CreateUIText(canvas, "HighScoreText", "HIGH SCORE: 0",
            new Vector2(0, 30), 24, Color.yellow, TextAnchor.MiddleCenter);

        // Play Button
        GameObject playBtn = CreateButton(canvas, "PlayButton", "PLAY",
            new Vector2(0, -40), new Vector2(200, 50));

        // Quit Button
        GameObject quitBtn = CreateButton(canvas, "QuitButton", "QUIT",
            new Vector2(0, -110), new Vector2(200, 50));

        // Controls info
        CreateUIText(canvas, "ControlsText", "WASD/Arrows: Move | Space: Shoot | Esc: Pause",
            new Vector2(0, -180), 16, Color.gray, TextAnchor.MiddleCenter);

        // MenuManager
        GameObject menuMgr = new GameObject("MenuManager");
        MenuManager mm = menuMgr.AddComponent<MenuManager>();
        mm.titleText = titleObj.GetComponent<Text>();
        mm.highScoreText = highScoreObj.GetComponent<Text>();
        mm.playButton = playBtn.GetComponent<Button>();
        mm.quitButton = quitBtn.GetComponent<Button>();

        // Save scene
        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!Directory.Exists("Assets/Scenes"))
            Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("MainMenu scene created.");
    }

    private static void CreateGamePlayScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
        }

        // Background scrollers
        CreateBackgroundLayer("Background1", 0f, new Vector3(0, 0, 10), 1f, 1f);
        CreateBackgroundLayer("Background2", 10f, new Vector3(0, 10, 10), 1f, 0.6f);

        // Player
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (playerPrefab != null)
        {
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(0, -3.5f, 0);
        }

        // Enemy Spawner
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
        spawner.straightEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyStraight.prefab");
        spawner.zigzagEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyZigzag.prefab");
        spawner.swooperEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemySwooper.prefab");
        spawner.tankEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyTank.prefab");

        // UI Canvas
        GameObject canvas = CreateCanvas();

        // Score Text (top-left)
        GameObject scoreText = CreateUIText(canvas, "ScoreText", "SCORE: 0",
            new Vector2(-300, 220), 24, Color.white, TextAnchor.MiddleLeft);

        // High Score Text (top-right)
        GameObject highScoreText = CreateUIText(canvas, "HighScoreText", "HIGH: 0",
            new Vector2(300, 220), 20, Color.yellow, TextAnchor.MiddleRight);

        // Wave Text (top-center)
        GameObject waveText = CreateUIText(canvas, "WaveText", "WAVE 1",
            new Vector2(0, 220), 22, Color.cyan, TextAnchor.MiddleCenter);

        // Wave Announcement (center screen)
        GameObject waveAnnouncement = CreateUIText(canvas, "WaveAnnouncementText", "",
            new Vector2(0, 50), 40, Color.white, TextAnchor.MiddleCenter);

        // Health Bar Background
        GameObject healthBg = CreateUIImage(canvas, "HealthBarBG",
            new Vector2(-280, -220), new Vector2(200, 20), new Color(0.3f, 0.0f, 0.0f, 0.8f));

        // Health Bar Fill
        GameObject healthFill = CreateUIImage(healthBg, "HealthBarFill",
            Vector2.zero, new Vector2(200, 20), Color.green);
        Image healthFillImage = healthFill.GetComponent<Image>();
        healthFillImage.type = Image.Type.Filled;
        healthFillImage.fillMethod = Image.FillMethod.Horizontal;

        // Health Text
        GameObject healthText = CreateUIText(canvas, "HealthText", "100 / 100",
            new Vector2(-280, -245), 14, Color.white, TextAnchor.MiddleLeft);

        // Pause Menu Panel
        GameObject pausePanel = CreatePauseMenu(canvas);

        // UIManager
        GameObject uiMgrObj = new GameObject("UIManager");
        UIManager uiMgr = uiMgrObj.AddComponent<UIManager>();
        uiMgr.scoreText = scoreText.GetComponent<Text>();
        uiMgr.highScoreText = highScoreText.GetComponent<Text>();
        uiMgr.waveText = waveText.GetComponent<Text>();
        uiMgr.waveAnnouncementText = waveAnnouncement.GetComponent<Text>();
        uiMgr.healthBarFill = healthFillImage;
        uiMgr.healthText = healthText.GetComponent<Text>();
        uiMgr.pauseMenuPanel = pausePanel;

        // Save scene
        string scenePath = "Assets/Scenes/GamePlay.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("GamePlay scene created.");
    }

    private static void CreateGameOverScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.1f, 0.02f, 0.02f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
        }

        GameObject canvas = CreateCanvas();

        // Game Over Title
        GameObject goText = CreateUIText(canvas, "GameOverText", "GAME OVER",
            new Vector2(0, 120), 52, Color.red, TextAnchor.MiddleCenter);

        // Final Score
        GameObject scoreText = CreateUIText(canvas, "FinalScoreText", "SCORE: 0",
            new Vector2(0, 50), 32, Color.white, TextAnchor.MiddleCenter);

        // High Score
        GameObject highScoreText = CreateUIText(canvas, "FinalHighScoreText", "HIGH SCORE: 0",
            new Vector2(0, 10), 24, Color.yellow, TextAnchor.MiddleCenter);

        // Restart Button
        GameObject restartBtn = CreateButton(canvas, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -50), new Vector2(220, 50));

        // Menu Button
        GameObject menuBtn = CreateButton(canvas, "MenuButton", "MAIN MENU",
            new Vector2(0, -120), new Vector2(220, 50));

        // MenuManager
        GameObject menuMgr = new GameObject("MenuManager");
        MenuManager mm = menuMgr.AddComponent<MenuManager>();
        mm.gameOverText = goText.GetComponent<Text>();
        mm.finalScoreText = scoreText.GetComponent<Text>();
        mm.finalHighScoreText = highScoreText.GetComponent<Text>();
        mm.restartButton = restartBtn.GetComponent<Button>();
        mm.menuButton = menuBtn.GetComponent<Button>();

        string scenePath = "Assets/Scenes/GameOver.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("GameOver scene created.");
    }

    // ========== UI HELPERS ==========

    private static GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        return canvasObj;
    }

    private static GameObject CreateUIText(GameObject parent, string name, string text,
        Vector2 position, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(600, 60);

        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.color = color;
        textComp.alignment = alignment;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (textComp.font == null)
            textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return textObj;
    }

    private static GameObject CreateUIImage(GameObject parent, string name,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject imgObj = new GameObject(name);
        imgObj.transform.SetParent(parent.transform, false);

        RectTransform rect = imgObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = imgObj.AddComponent<Image>();
        img.color = color;

        return imgObj;
    }

    private static GameObject CreateButton(GameObject parent, string name, string text,
        Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.4f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.6f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.3f);
        btn.colors = colors;

        // Button text
        CreateUIText(btnObj, "Text", text, Vector2.zero, 22, Color.white, TextAnchor.MiddleCenter);

        return btnObj;
    }

    private static GameObject CreatePauseMenu(GameObject canvas)
    {
        // Background overlay
        GameObject panel = new GameObject("PauseMenuPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.7f);

        // Pause title
        CreateUIText(panel, "PauseTitle", "PAUSED",
            new Vector2(0, 80), 42, Color.white, TextAnchor.MiddleCenter);

        // Resume button
        CreateButton(panel, "ResumeButton", "RESUME",
            new Vector2(0, 0), new Vector2(200, 50));

        // Quit button
        CreateButton(panel, "QuitToMenuButton", "QUIT TO MENU",
            new Vector2(0, -70), new Vector2(200, 50));

        panel.SetActive(false);
        return panel;
    }

    private static void CreateBackgroundLayer(string name, float yOffset, Vector3 position,
        float scrollSpeed, float parallaxFactor)
    {
        GameObject bg = new GameObject(name);
        bg.transform.position = position;
        bg.tag = "Background";

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/StarBackground.png");
        sr.sortingOrder = -10;
        sr.drawMode = SpriteDrawMode.Simple;

        BackgroundScroller scroller = bg.AddComponent<BackgroundScroller>();
        scroller.scrollSpeed = scrollSpeed;
        scroller.parallaxFactor = parallaxFactor;
        scroller.resetPositionY = -15f;
        scroller.startPositionY = 15f;
    }

    // ========== BUILD SETTINGS ==========

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GamePlay.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameOver.unity", true)
        };
        EditorBuildSettings.scenes = scenes;

        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.companyName = "SpaceShooterDev";
        PlayerSettings.defaultScreenWidth = 800;
        PlayerSettings.defaultScreenHeight = 600;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

        Debug.Log("Build settings configured.");
    }
}
#endif
