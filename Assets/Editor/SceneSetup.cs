// =============================================================================
// SceneSetup.cs (Editor Script)
// Automated scene and prefab setup for the Space Shooter game.
// Run from the Unity Editor menu: Tools > Space Shooter > Setup Complete Game
// This creates all prefabs, configures all three scenes, and sets build settings.
// =============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class SceneSetup : MonoBehaviour
{
    // =========================================================================
    // MAIN ENTRY POINT
    // =========================================================================

    /// <summary>
    /// Sets up the entire game: generates sprites, creates prefabs, 
    /// builds all three scenes, and configures build settings.
    /// </summary>
    [MenuItem("Tools/Space Shooter/Setup Complete Game")]
    public static void SetupCompleteGame()
    {
        // Step 1: Generate sprites first
        SpriteGenerator.GenerateAllSprites();
        AssetDatabase.Refresh();

        // Small delay to let assets import
        EditorApplication.delayCall += () =>
        {
            // Step 2: Configure sprite import settings
            ConfigureSpriteImportSettings();
            AssetDatabase.Refresh();

            EditorApplication.delayCall += () =>
            {
                // Step 3: Create all prefabs
                CreateAllPrefabs();
                AssetDatabase.Refresh();

                EditorApplication.delayCall += () =>
                {
                    // Step 4: Create all scenes
                    CreateMainMenuScene();
                    CreateGamePlayScene();
                    CreateGameOverScene();

                    // Step 5: Set up build settings
                    SetupBuildSettings();

                    Debug.Log("=== SPACE SHOOTER SETUP COMPLETE ===");
                    Debug.Log("All sprites, prefabs, and scenes have been created.");
                    Debug.Log("Go to File > Build Settings to build your game!");
                    
                    EditorUtility.DisplayDialog(
                        "Setup Complete!",
                        "Space Shooter game has been fully set up!\n\n" +
                        "• All sprites generated in Assets/Sprites/\n" +
                        "• All prefabs created in Assets/Prefabs/\n" +
                        "• 3 scenes created in Assets/Scenes/\n" +
                        "• Build settings configured\n\n" +
                        "Press Play to test, or go to File > Build Settings to build!",
                        "OK"
                    );
                };
            };
        };
    }

    // =========================================================================
    // SPRITE IMPORT CONFIGURATION
    // =========================================================================

    /// <summary>
    /// Sets all generated sprite PNGs to proper import settings
    /// (Sprite mode, correct pixels per unit, filter mode).
    /// </summary>
    private static void ConfigureSpriteImportSettings()
    {
        string[] spriteFiles = Directory.GetFiles("Assets/Sprites/", "*.png");
        foreach (string file in spriteFiles)
        {
            TextureImporter importer = AssetImporter.GetAtPath(file) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point; // Pixel-art crisp look
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }

    // =========================================================================
    // PREFAB CREATION
    // =========================================================================

    /// <summary>Creates all game prefabs in Assets/Prefabs/.</summary>
    private static void CreateAllPrefabs()
    {
        EnsureDirectory("Assets/Prefabs");

        CreateExplosionPrefab();
        CreatePlayerBulletPrefab();
        CreateEnemyBulletPrefab();
        CreateShieldPowerUpPrefab();
        CreateRapidFirePowerUpPrefab();
        CreateHealthPowerUpPrefab();
        CreateBasicEnemyPrefab();
        CreateZigzagEnemyPrefab();
        CreateChargerEnemyPrefab();
        CreatePlayerPrefab();

        Debug.Log("All prefabs created successfully.");
    }

    // ---- Explosion Prefab ----
    private static void CreateExplosionPrefab()
    {
        GameObject obj = new GameObject("Explosion");
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/explosion.png");
        sr.sortingOrder = 10;

        Explosion explosion = obj.AddComponent<Explosion>();
        explosion.duration = 0.5f;
        explosion.maxScale = 2.5f;

        SavePrefab(obj, "Assets/Prefabs/Explosion.prefab");
        DestroyImmediate(obj);
    }

    // ---- Player Bullet Prefab ----
    private static void CreatePlayerBulletPrefab()
    {
        GameObject obj = new GameObject("PlayerBullet");
        obj.tag = "PlayerBullet";

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/bullet_player.png");
        sr.sortingOrder = 5;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.5f);

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        BulletController bc = obj.AddComponent<BulletController>();
        bc.speed = 14f;
        bc.damage = 1;
        bc.isPlayerBullet = true;
        bc.lifetime = 4f;

        SavePrefab(obj, "Assets/Prefabs/PlayerBullet.prefab");
        DestroyImmediate(obj);
    }

    // ---- Enemy Bullet Prefab ----
    private static void CreateEnemyBulletPrefab()
    {
        GameObject obj = new GameObject("EnemyBullet");
        obj.tag = "EnemyBullet";

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/bullet_enemy.png");
        sr.sortingOrder = 5;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.5f);

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        BulletController bc = obj.AddComponent<BulletController>();
        bc.speed = 8f;
        bc.damage = 1;
        bc.isPlayerBullet = false;
        bc.lifetime = 5f;

        SavePrefab(obj, "Assets/Prefabs/EnemyBullet.prefab");
        DestroyImmediate(obj);
    }

    // ---- Power-Up Prefabs ----
    private static void CreateShieldPowerUpPrefab()
    {
        CreatePowerUpPrefab("ShieldPowerUp", "Assets/Sprites/powerup_shield.png",
            PowerUpType.Shield, "Assets/Prefabs/ShieldPowerUp.prefab");
    }

    private static void CreateRapidFirePowerUpPrefab()
    {
        CreatePowerUpPrefab("RapidFirePowerUp", "Assets/Sprites/powerup_rapidfire.png",
            PowerUpType.RapidFire, "Assets/Prefabs/RapidFirePowerUp.prefab");
    }

    private static void CreateHealthPowerUpPrefab()
    {
        CreatePowerUpPrefab("HealthPowerUp", "Assets/Sprites/powerup_health.png",
            PowerUpType.Health, "Assets/Prefabs/HealthPowerUp.prefab");
    }

    private static void CreatePowerUpPrefab(string name, string spritePath, PowerUpType type, string savePath)
    {
        GameObject obj = new GameObject(name);
        obj.tag = "PowerUp";

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spritePath);
        sr.sortingOrder = 4;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        PowerUp pu = obj.AddComponent<PowerUp>();
        pu.powerUpType = type;
        pu.fallSpeed = 2f;
        pu.healAmount = 2;
        pu.rapidFireDuration = 5f;
        pu.lifetime = 12f;

        SavePrefab(obj, savePath);
        DestroyImmediate(obj);
    }

    // ---- Enemy Prefabs ----
    private static void CreateBasicEnemyPrefab()
    {
        CreateEnemyPrefab("BasicEnemy", "Assets/Sprites/enemy_basic.png",
            EnemyType.Basic, 3f, 1, 100, true, "Assets/Prefabs/BasicEnemy.prefab");
    }

    private static void CreateZigzagEnemyPrefab()
    {
        CreateEnemyPrefab("ZigzagEnemy", "Assets/Sprites/enemy_zigzag.png",
            EnemyType.Zigzag, 2.5f, 2, 200, false, "Assets/Prefabs/ZigzagEnemy.prefab");
    }

    private static void CreateChargerEnemyPrefab()
    {
        CreateEnemyPrefab("ChargerEnemy", "Assets/Sprites/enemy_charger.png",
            EnemyType.Charger, 2f, 1, 150, false, "Assets/Prefabs/ChargerEnemy.prefab");
    }

    private static void CreateEnemyPrefab(string name, string spritePath, EnemyType type,
        float speed, int health, int score, bool canShoot, string savePath)
    {
        GameObject obj = new GameObject(name);
        obj.tag = "Enemy";

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spritePath);
        sr.sortingOrder = 3;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        EnemyController ec = obj.AddComponent<EnemyController>();
        ec.enemyType = type;
        ec.moveSpeed = speed;
        ec.health = health;
        ec.scoreValue = score;
        ec.canShoot = canShoot;
        ec.contactDamage = 1;
        ec.dropChance = 0.15f;

        // Load prefab references
        ec.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Explosion.prefab");
        if (canShoot)
        {
            ec.enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyBullet.prefab");
            ec.fireRate = 2.5f;
        }

        // Power-up drop references
        GameObject shieldPU = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ShieldPowerUp.prefab");
        GameObject rapidPU = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RapidFirePowerUp.prefab");
        GameObject healthPU = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HealthPowerUp.prefab");
        ec.powerUpDrops = new GameObject[] { shieldPU, rapidPU, healthPU };

        SavePrefab(obj, savePath);
        DestroyImmediate(obj);
    }

    // ---- Player Prefab ----
    private static void CreatePlayerPrefab()
    {
        GameObject obj = new GameObject("Player");
        obj.tag = "Player";

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/player_ship.png");
        sr.sortingOrder = 5;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.2f);

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        PlayerController pc = obj.AddComponent<PlayerController>();
        pc.moveSpeed = 8f;
        pc.maxHealth = 5;
        pc.fireRate = 0.25f;
        pc.rapidFireRate = 0.1f;
        pc.invincibilityDuration = 1.5f;
        pc.bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerBullet.prefab");
        pc.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Explosion.prefab");

        // Fire point child
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(obj.transform);
        firePoint.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        pc.firePoint = firePoint.transform;

        // Shield visual child
        GameObject shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.SetParent(obj.transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSR = shieldVisual.AddComponent<SpriteRenderer>();
        shieldSR.sprite = LoadSprite("Assets/Sprites/shield_visual.png");
        shieldSR.sortingOrder = 6;
        shieldVisual.SetActive(false);
        pc.shieldVisual = shieldVisual;

        SavePrefab(obj, "Assets/Prefabs/Player.prefab");
        DestroyImmediate(obj);
    }

    // =========================================================================
    // SCENE CREATION
    // =========================================================================

    // ---- Main Menu Scene ----
    private static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.05f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;

        // Canvas
        GameObject canvas = CreateCanvas();

        // Title text
        GameObject titleObj = CreateUIText(canvas.transform, "TitleText",
            "SPACE SHOOTER", 48, Color.white,
            new Vector2(0, 100), new Vector2(600, 80));

        // Subtitle
        CreateUIText(canvas.transform, "SubtitleText",
            "Defend the Galaxy!", 20, new Color(0.7f, 0.7f, 1f),
            new Vector2(0, 40), new Vector2(400, 40));

        // Play button
        CreateUIButton(canvas.transform, "PlayButton", "PLAY",
            new Vector2(0, -40), new Vector2(200, 50),
            new Color(0.2f, 0.6f, 0.2f));

        // Quit button
        CreateUIButton(canvas.transform, "QuitButton", "QUIT",
            new Vector2(0, -110), new Vector2(200, 50),
            new Color(0.6f, 0.2f, 0.2f));

        // High score text
        CreateUIText(canvas.transform, "HighScoreText",
            "HIGH SCORE: 0", 18, Color.yellow,
            new Vector2(0, -180), new Vector2(300, 30));

        // MenuManager
        GameObject menuMgr = new GameObject("MenuManager");
        MenuManager mm = menuMgr.AddComponent<MenuManager>();
        mm.titleText = titleObj.GetComponent<Text>();
        mm.highScoreText = GameObject.Find("HighScoreText")?.GetComponent<Text>();

        // GameManager (singleton - will persist)
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // AudioManager (singleton - will persist)
        GameObject amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();

        // Wire buttons
        Button playBtn = GameObject.Find("PlayButton")?.GetComponent<Button>();
        if (playBtn != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                playBtn.onClick, mm.OnPlayButton);
        }

        Button quitBtn = GameObject.Find("QuitButton")?.GetComponent<Button>();
        if (quitBtn != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                quitBtn.onClick, mm.OnQuitButton);
        }

        // Save scene
        EnsureDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("MainMenu scene created.");
    }

    // ---- GamePlay Scene ----
    private static void CreateGamePlayScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        Camera.main.backgroundColor = new Color(0.01f, 0.01f, 0.03f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;

        // Background layers
        CreateBackgroundLayer("Background_Space", "Assets/Sprites/background_space.png",
            0.5f, -1, new Vector3(0, 0, 10));
        CreateBackgroundLayer("Background_Stars", "Assets/Sprites/background_stars.png",
            1.5f, 0, new Vector3(0, 0, 5));

        // Player
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (playerPrefab != null)
        {
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(0f, -3.5f, 0f);
        }

        // Enemy Spawner
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
        spawner.basicEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BasicEnemy.prefab");
        spawner.zigzagEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ZigzagEnemy.prefab");
        spawner.chargerEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ChargerEnemy.prefab");

        // HUD Canvas
        GameObject canvas = CreateCanvas();

        // Score text (top left)
        GameObject scoreObj = CreateUIText(canvas.transform, "ScoreText",
            "SCORE: 0", 22, Color.white,
            new Vector2(-280, 220), new Vector2(250, 40));

        // Wave text (top center)
        GameObject waveObj = CreateUIText(canvas.transform, "WaveText",
            "WAVE: 0", 22, Color.yellow,
            new Vector2(0, 220), new Vector2(200, 40));

        // Health text (top right)
        GameObject healthObj = CreateUIText(canvas.transform, "HealthText",
            "HP: 5 / 5", 22, Color.green,
            new Vector2(280, 220), new Vector2(200, 40));

        // Health bar background
        GameObject healthBarBG = CreateUIImage(canvas.transform, "HealthBarBG",
            new Color(0.3f, 0.3f, 0.3f, 0.8f),
            new Vector2(280, 195), new Vector2(180, 12));

        // Health bar fill
        GameObject healthBarFill = CreateUIImage(canvas.transform, "HealthBarFill",
            Color.green,
            new Vector2(280, 195), new Vector2(180, 12));
        Image fillImage = healthBarFill.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        // Wave announcement text (center of screen, large)
        GameObject waveAnnounce = CreateUIText(canvas.transform, "WaveAnnouncementText",
            "WAVE 1", 42, Color.yellow,
            new Vector2(0, 30), new Vector2(400, 60));
        waveAnnounce.SetActive(false);

        // Pause menu panel
        GameObject pausePanel = CreatePauseMenuPanel(canvas.transform);

        // UIManager
        GameObject uiMgrObj = new GameObject("UIManager");
        UIManager uiMgr = uiMgrObj.AddComponent<UIManager>();
        uiMgr.scoreText = scoreObj.GetComponent<Text>();
        uiMgr.waveText = waveObj.GetComponent<Text>();
        uiMgr.healthText = healthObj.GetComponent<Text>();
        uiMgr.healthBarFill = fillImage;
        uiMgr.waveAnnouncementText = waveAnnounce.GetComponent<Text>();
        uiMgr.pauseMenuPanel = pausePanel;

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GamePlay.unity");
        Debug.Log("GamePlay scene created.");
    }

    // ---- Game Over Scene ----
    private static void CreateGameOverScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera.main.backgroundColor = new Color(0.05f, 0.02f, 0.02f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;

        // Canvas
        GameObject canvas = CreateCanvas();

        // Game Over title
        GameObject gameOverObj = CreateUIText(canvas.transform, "GameOverText",
            "GAME OVER", 52, Color.red,
            new Vector2(0, 120), new Vector2(500, 70));

        // Final score
        GameObject scoreObj = CreateUIText(canvas.transform, "FinalScoreText",
            "SCORE: 0", 28, Color.white,
            new Vector2(0, 40), new Vector2(400, 40));

        // High score
        GameObject highScoreObj = CreateUIText(canvas.transform, "HighScoreText",
            "HIGH SCORE: 0", 22, Color.yellow,
            new Vector2(0, -10), new Vector2(400, 35));

        // New high score text (hidden by default)
        GameObject newHSObj = CreateUIText(canvas.transform, "NewHighScoreText",
            "★ NEW HIGH SCORE! ★", 24, new Color(1f, 0.84f, 0f),
            new Vector2(0, -50), new Vector2(400, 35));
        newHSObj.SetActive(false);

        // Play Again button
        CreateUIButton(canvas.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -100), new Vector2(220, 50),
            new Color(0.2f, 0.6f, 0.2f));

        // Main Menu button
        CreateUIButton(canvas.transform, "MainMenuButton", "MAIN MENU",
            new Vector2(0, -165), new Vector2(220, 50),
            new Color(0.2f, 0.2f, 0.6f));

        // MenuManager
        GameObject menuMgrObj = new GameObject("MenuManager");
        MenuManager mm = menuMgrObj.AddComponent<MenuManager>();
        mm.gameOverText = gameOverObj.GetComponent<Text>();
        mm.finalScoreText = scoreObj.GetComponent<Text>();
        mm.gameOverHighScoreText = highScoreObj.GetComponent<Text>();
        mm.newHighScoreText = newHSObj.GetComponent<Text>();

        // Wire buttons
        Button restartBtn = GameObject.Find("RestartButton")?.GetComponent<Button>();
        if (restartBtn != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                restartBtn.onClick, mm.OnRestartButton);
        }

        Button menuBtn = GameObject.Find("MainMenuButton")?.GetComponent<Button>();
        if (menuBtn != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                menuBtn.onClick, mm.OnMainMenuButton);
        }

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameOver.unity");
        Debug.Log("GameOver scene created.");
    }

    // =========================================================================
    // BUILD SETTINGS
    // =========================================================================

    /// <summary>
    /// Configures the Build Settings with all three scenes in the correct order.
    /// </summary>
    private static void SetupBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GamePlay.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameOver.unity", true)
        };

        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build settings configured with 3 scenes.");
    }

    // =========================================================================
    // UI HELPER METHODS
    // =========================================================================

    private static GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Event System
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    private static GameObject CreateUIText(Transform parent, string name, string content,
        int fontSize, Color color, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return obj;
    }

    private static GameObject CreateUIImage(Transform parent, string name,
        Color color, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        return obj;
    }

    private static GameObject CreateUIButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = bgColor * 1.2f;
        colors.pressedColor = bgColor * 0.8f;
        btn.colors = colors;

        // Button label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        Text text = labelObj.AddComponent<Text>();
        text.text = label;
        text.fontSize = 22;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return btnObj;
    }

    private static GameObject CreatePauseMenuPanel(Transform parent)
    {
        GameObject panel = new GameObject("PauseMenuPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);

        // "PAUSED" text
        CreateUIText(panel.transform, "PausedText", "PAUSED", 42, Color.white,
            new Vector2(0, 80), new Vector2(300, 60));

        // Resume button
        CreateUIButton(panel.transform, "ResumeButton", "RESUME",
            new Vector2(0, 0), new Vector2(200, 45),
            new Color(0.2f, 0.5f, 0.2f));

        // Main Menu button
        CreateUIButton(panel.transform, "PauseMainMenuButton", "MAIN MENU",
            new Vector2(0, -60), new Vector2(200, 45),
            new Color(0.5f, 0.2f, 0.2f));

        panel.SetActive(false);
        return panel;
    }

    // =========================================================================
    // BACKGROUND HELPER
    // =========================================================================

    private static void CreateBackgroundLayer(string name, string spritePath,
        float scrollSpeed, int sortingOrder, Vector3 position)
    {
        GameObject bg = new GameObject(name);
        bg.transform.position = position;

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spritePath);
        sr.sortingOrder = sortingOrder;
        // Scale to fill the screen
        bg.transform.localScale = new Vector3(5f, 3f, 1f);

        ParallaxBackground pb = bg.AddComponent<ParallaxBackground>();
        pb.scrollSpeed = scrollSpeed;
        pb.autoScroll = true;
    }

    // =========================================================================
    // UTILITY METHODS
    // =========================================================================

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void SavePrefab(GameObject obj, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(obj, path);
        Debug.Log("Created prefab: " + path);
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
#endif
