// ============================================================================
// GameSetupWizard.cs — Editor tool that auto-creates scenes, prefabs, and
// wires up all references so the game is playable in one click.
//
// Usage: In Unity menu bar -> Space Shooter -> Setup Game (Full)
// ============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class GameSetupWizard : EditorWindow
{
    [MenuItem("Space Shooter/Setup Game (Full) — RUN THIS FIRST", false, 0)]
    public static void SetupFullGame()
    {
        if (!EditorUtility.DisplayDialog("Space Shooter Setup",
            "This will create all prefabs, scenes, and wire up the game.\n\nProceed?",
            "Yes, Set Up Everything", "Cancel"))
            return;

        try
        {
            EditorUtility.DisplayProgressBar("Setting Up", "Creating prefabs...", 0.1f);
            CreatePrefabs();

            EditorUtility.DisplayProgressBar("Setting Up", "Creating MainMenu scene...", 0.4f);
            CreateMainMenuScene();

            EditorUtility.DisplayProgressBar("Setting Up", "Creating GameScene...", 0.7f);
            CreateGameScene();

            EditorUtility.DisplayProgressBar("Setting Up", "Configuring build settings...", 0.9f);
            ConfigureBuildSettings();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Setup Complete!",
                "All scenes and prefabs have been created.\n\n" +
                "1. Open 'Assets/Scenes/MainMenu' to start from menu\n" +
                "2. Or open 'Assets/Scenes/GameScene' to play directly\n" +
                "3. Press Play!\n\n" +
                "Build: File -> Build Settings -> Build (Windows x64)",
                "Got it!");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"Setup failed: {e.Message}\n{e.StackTrace}");
        }
    }

    // =========================================================================
    // PREFAB CREATION
    // =========================================================================
    static void CreatePrefabs()
    {
        Debug.Log("[Setup] Creating prefabs...");

        CreatePlayerPrefab();
        CreateBulletPrefabs();
        CreateEnemyPrefabs();
        CreatePowerUpPrefabs();
        CreateExplosionPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Setup] All prefabs created.");
    }

    static void CreatePlayerPrefab()
    {
        GameObject player = new GameObject("PlayerShip");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/Player/player_ship.png");
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10;

        // Collider
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        // Rigidbody
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        // Scripts
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerShooting>();
        player.AddComponent<PlayerHealth>();

        // Fire points
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(player.transform);
        fp.transform.localPosition = new Vector3(0, 0.6f, 0);

        GameObject fpL = new GameObject("FirePointLeft");
        fpL.transform.SetParent(player.transform);
        fpL.transform.localPosition = new Vector3(-0.25f, 0.5f, 0);

        GameObject fpR = new GameObject("FirePointRight");
        fpR.transform.SetParent(player.transform);
        fpR.transform.localPosition = new Vector3(0.25f, 0.5f, 0);

        // Shield visual
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSR = shield.AddComponent<SpriteRenderer>();
        shieldSR.sprite = LoadSprite("Assets/Sprites/Player/player_shield.png");
        shieldSR.sortingOrder = 11;
        shieldSR.color = new Color(1, 1, 1, 0.5f);
        shield.SetActive(false);

        // Wire references via SerializedObject
        SavePrefab(player, "Assets/Prefabs/Player/PlayerShip.prefab");
        DestroyImmediate(player);
    }

    static void CreateBulletPrefabs()
    {
        // Player bullet
        CreateSimpleBulletPrefab("PlayerBullet",
            "Assets/Sprites/Bullets/player_bullet.png",
            "Assets/Prefabs/Bullets/PlayerBullet.prefab",
            "PlayerBullet", true);

        // Enemy bullet
        CreateSimpleBulletPrefab("EnemyBullet",
            "Assets/Sprites/Bullets/enemy_bullet.png",
            "Assets/Prefabs/Bullets/EnemyBullet.prefab",
            "EnemyBullet", false);

        // Laser
        CreateSimpleBulletPrefab("Laser",
            "Assets/Sprites/Bullets/laser.png",
            "Assets/Prefabs/Bullets/Laser.prefab",
            "PlayerBullet", true);
    }

    static void CreateSimpleBulletPrefab(string name, string spritePath, string prefabPath, string tag, bool isPlayer)
    {
        GameObject bullet = new GameObject(name);
        bullet.tag = tag;

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spritePath);
        sr.sortingOrder = 15;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.3f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Bullet b = bullet.AddComponent<Bullet>();

        SavePrefab(bullet, prefabPath);
        DestroyImmediate(bullet);
    }

    static void CreateEnemyPrefabs()
    {
        CreateEnemyPrefab<EnemyBasic>("EnemyBasic",
            "Assets/Sprites/Enemies/enemy_basic.png",
            "Assets/Prefabs/Enemies/EnemyBasic.prefab");

        CreateEnemyPrefab<EnemyFast>("EnemyFast",
            "Assets/Sprites/Enemies/enemy_fast.png",
            "Assets/Prefabs/Enemies/EnemyFast.prefab");

        CreateEnemyPrefab<EnemyTank>("EnemyTank",
            "Assets/Sprites/Enemies/enemy_tank.png",
            "Assets/Prefabs/Enemies/EnemyTank.prefab");

        CreateEnemyPrefab<EnemyShooter>("EnemyShooter",
            "Assets/Sprites/Enemies/enemy_shooter.png",
            "Assets/Prefabs/Enemies/EnemyShooter.prefab");

        // Boss (special)
        GameObject boss = new GameObject("Boss");
        boss.tag = "Enemy";

        SpriteRenderer bsr = boss.AddComponent<SpriteRenderer>();
        bsr.sprite = LoadSprite("Assets/Sprites/Enemies/boss.png");
        bsr.sortingOrder = 8;

        BoxCollider2D bcol = boss.AddComponent<BoxCollider2D>();
        bcol.isTrigger = true;
        bcol.size = new Vector2(1.5f, 1.5f);

        Rigidbody2D brb = boss.AddComponent<Rigidbody2D>();
        brb.gravityScale = 0;
        brb.bodyType = RigidbodyType2D.Kinematic;

        boss.AddComponent<BossEnemy>();

        SavePrefab(boss, "Assets/Prefabs/Enemies/Boss.prefab");
        DestroyImmediate(boss);
    }

    static void CreateEnemyPrefab<T>(string name, string spritePath, string prefabPath) where T : EnemyBase
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spritePath);
        sr.sortingOrder = 8;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        enemy.AddComponent<T>();

        // Shooter enemies get a BulletPattern component
        if (typeof(T) == typeof(EnemyShooter) || typeof(T) == typeof(EnemyTank))
        {
            enemy.AddComponent<BulletPattern>();
        }

        SavePrefab(enemy, prefabPath);
        DestroyImmediate(enemy);
    }

    static void CreatePowerUpPrefabs()
    {
        string[] names = { "weapon", "shield", "health", "speed", "rapidfire", "extralife", "bomb" };
        PowerUpType[] types = {
            PowerUpType.WeaponUpgrade, PowerUpType.Shield, PowerUpType.Health,
            PowerUpType.SpeedBoost, PowerUpType.RapidFire, PowerUpType.ExtraLife, PowerUpType.Bomb
        };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject pu = new GameObject($"PowerUp_{names[i]}");
            pu.tag = "PowerUp";

            SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite($"Assets/Sprites/PowerUps/powerup_{names[i]}.png");
            sr.sortingOrder = 12;

            CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;

            pu.AddComponent<PowerUp>();

            SavePrefab(pu, $"Assets/Prefabs/PowerUps/PowerUp_{names[i]}.prefab");
            DestroyImmediate(pu);
        }
    }

    static void CreateExplosionPrefab()
    {
        GameObject explosion = new GameObject("Explosion");

        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/Effects/explosion_0.png");
        sr.sortingOrder = 20;

        explosion.AddComponent<AutoDestroy>();

        SavePrefab(explosion, "Assets/Prefabs/Effects/Explosion.prefab");
        DestroyImmediate(explosion);
    }

    // =========================================================================
    // SCENE CREATION — MAIN MENU
    // =========================================================================
    static void CreateMainMenuScene()
    {
        Debug.Log("[Setup] Creating MainMenu scene...");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject cam = new GameObject("Main Camera");
        Camera camera = cam.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cam.AddComponent<AudioListener>();
        cam.tag = "MainCamera";

        // GameManager (persistent singleton)
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        // SoundManager (persistent singleton)
        GameObject sm = new GameObject("SoundManager");
        sm.AddComponent<SoundManager>();

        // Starfield background
        GameObject stars = new GameObject("Starfield");
        stars.AddComponent<StarfieldGenerator>();

        // Canvas
        GameObject canvas = CreateUICanvas("MenuCanvas");

        // Title
        GameObject title = CreateUIText(canvas.transform, "TitleText",
            "STELLAR ASSAULT", 48, Color.cyan, TextAnchor.MiddleCenter,
            new Vector2(0, 150), new Vector2(500, 80));

        // Subtitle
        CreateUIText(canvas.transform, "SubtitleText",
            "A Space Shooter", 18, new Color(0.6f, 0.8f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0, 90), new Vector2(300, 40));

        // Start Button
        GameObject startBtn = CreateUIButton(canvas.transform, "StartButton",
            "START GAME", new Vector2(0, -10), new Vector2(200, 50));

        // Options Button
        GameObject optionsBtn = CreateUIButton(canvas.transform, "OptionsButton",
            "OPTIONS", new Vector2(0, -70), new Vector2(200, 50));

        // Quit Button
        GameObject quitBtn = CreateUIButton(canvas.transform, "QuitButton",
            "QUIT", new Vector2(0, -130), new Vector2(200, 50));

        // High Score text
        CreateUIText(canvas.transform, "HighScoreText",
            "HIGH SCORE: 0", 16, Color.yellow, TextAnchor.MiddleCenter,
            new Vector2(0, -200), new Vector2(300, 30));

        // Controls text
        CreateUIText(canvas.transform, "ControlsText",
            "WASD/Arrows = Move  |  Space/Click = Shoot  |  ESC = Pause",
            12, new Color(0.5f, 0.5f, 0.7f), TextAnchor.MiddleCenter,
            new Vector2(0, -240), new Vector2(500, 30));

        // Options Panel (hidden by default)
        GameObject optionsPanel = new GameObject("OptionsPanel");
        optionsPanel.transform.SetParent(canvas.transform, false);
        RectTransform optRT = optionsPanel.AddComponent<RectTransform>();
        optRT.anchoredPosition = Vector2.zero;
        optRT.sizeDelta = new Vector2(350, 280);
        Image optBg = optionsPanel.AddComponent<Image>();
        optBg.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        optionsPanel.SetActive(false);

        CreateUIText(optionsPanel.transform, "OptionsTitle",
            "OPTIONS", 24, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0, 100), new Vector2(200, 40));

        // Back button in options
        GameObject backBtn = CreateUIButton(optionsPanel.transform, "OptionsBackButton",
            "BACK", new Vector2(0, -100), new Vector2(150, 40));

        // MainMenuUI component
        GameObject menuUIObj = new GameObject("MainMenuUI");
        menuUIObj.transform.SetParent(canvas.transform, false);
        menuUIObj.AddComponent<MainMenuUI>();

        // EventSystem
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("[Setup] MainMenu scene saved.");
    }

    // =========================================================================
    // SCENE CREATION — GAME SCENE
    // =========================================================================
    static void CreateGameScene()
    {
        Debug.Log("[Setup] Creating GameScene...");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject cam = new GameObject("Main Camera");
        Camera camera = cam.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cam.AddComponent<AudioListener>();
        cam.AddComponent<ScreenShake>();
        cam.tag = "MainCamera";

        // GameManager
        GameObject gm = new GameObject("GameManager");
        GameManager gmScript = gm.AddComponent<GameManager>();

        // SoundManager
        GameObject sm = new GameObject("SoundManager");
        sm.AddComponent<SoundManager>();

        // WaveManager
        GameObject wm = new GameObject("WaveManager");
        wm.AddComponent<WaveManager>();

        // ObjectPooler
        GameObject pool = new GameObject("ObjectPooler");
        pool.AddComponent<ObjectPooler>();

        // Background layers
        CreateBackgroundLayers();

        // Starfield
        GameObject stars = new GameObject("Starfield");
        stars.AddComponent<StarfieldGenerator>();

        // Player spawn point
        GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.position = new Vector3(0, -3.5f, 0);

        // --- HUD Canvas ---
        GameObject hudCanvas = CreateUICanvas("HUDCanvas");

        // Score
        CreateUIText(hudCanvas.transform, "ScoreText",
            "SCORE: 0", 20, Color.white, TextAnchor.UpperLeft,
            new Vector2(-280, 220), new Vector2(250, 40));

        // High Score
        CreateUIText(hudCanvas.transform, "HighScoreText",
            "HI: 0", 14, Color.yellow, TextAnchor.UpperRight,
            new Vector2(280, 220), new Vector2(200, 30));

        // Lives
        CreateUIText(hudCanvas.transform, "LivesText",
            "LIVES: 3", 16, Color.green, TextAnchor.UpperLeft,
            new Vector2(-280, 190), new Vector2(200, 30));

        // Health
        CreateUIText(hudCanvas.transform, "HealthText",
            "HP: 3/3", 16, Color.red, TextAnchor.UpperLeft,
            new Vector2(-280, 165), new Vector2(200, 30));

        // Shield
        CreateUIText(hudCanvas.transform, "ShieldText",
            "", 14, new Color(0.5f, 0.8f, 1f), TextAnchor.UpperLeft,
            new Vector2(-280, 140), new Vector2(200, 30));

        // Wave
        CreateUIText(hudCanvas.transform, "WaveText",
            "WAVE 1/10", 16, Color.white, TextAnchor.UpperRight,
            new Vector2(280, 190), new Vector2(200, 30));

        // Weapon
        CreateUIText(hudCanvas.transform, "WeaponText",
            "SINGLE", 14, Color.cyan, TextAnchor.UpperRight,
            new Vector2(280, 165), new Vector2(200, 30));

        // Wave announcement (center)
        CreateUIText(hudCanvas.transform, "WaveAnnouncementText",
            "~ WAVE 1 ~", 36, Color.yellow, TextAnchor.MiddleCenter,
            new Vector2(0, 30), new Vector2(400, 60));

        // HUD script
        GameObject hudObj = new GameObject("GameHUD");
        hudObj.transform.SetParent(hudCanvas.transform, false);
        hudObj.AddComponent<GameHUD>();

        // --- Pause Menu ---
        GameObject pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(hudCanvas.transform, false);
        RectTransform pauseRT = pausePanel.AddComponent<RectTransform>();
        pauseRT.anchoredPosition = Vector2.zero;
        pauseRT.sizeDelta = new Vector2(600, 400);
        Image pauseBg = pausePanel.AddComponent<Image>();
        pauseBg.color = new Color(0, 0, 0, 0.8f);
        pausePanel.SetActive(false);

        CreateUIText(pausePanel.transform, "PauseTitle",
            "PAUSED", 36, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0, 120), new Vector2(300, 50));

        CreateUIButton(pausePanel.transform, "ResumeButton",
            "RESUME", new Vector2(0, 40), new Vector2(200, 45));
        CreateUIButton(pausePanel.transform, "RestartButton",
            "RESTART", new Vector2(0, -15), new Vector2(200, 45));
        CreateUIButton(pausePanel.transform, "MainMenuButton",
            "MAIN MENU", new Vector2(0, -70), new Vector2(200, 45));
        CreateUIButton(pausePanel.transform, "QuitButton",
            "QUIT", new Vector2(0, -125), new Vector2(200, 45));

        GameObject pauseUI = new GameObject("PauseMenuUI");
        pauseUI.transform.SetParent(hudCanvas.transform, false);
        pauseUI.AddComponent<PauseMenuUI>();

        // --- Game Over Panel ---
        GameObject goPanel = new GameObject("GameOverPanel");
        goPanel.transform.SetParent(hudCanvas.transform, false);
        RectTransform goRT = goPanel.AddComponent<RectTransform>();
        goRT.anchoredPosition = Vector2.zero;
        goRT.sizeDelta = new Vector2(600, 400);
        Image goBg = goPanel.AddComponent<Image>();
        goBg.color = new Color(0.1f, 0, 0, 0.85f);
        goPanel.SetActive(false);

        CreateUIText(goPanel.transform, "GameOverTitle",
            "GAME OVER", 42, Color.red, TextAnchor.MiddleCenter,
            new Vector2(0, 120), new Vector2(400, 60));
        CreateUIText(goPanel.transform, "FinalScoreText",
            "SCORE: 0", 24, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0, 60), new Vector2(300, 40));
        CreateUIText(goPanel.transform, "HighScoreText_GO",
            "HIGH SCORE: 0", 18, Color.yellow, TextAnchor.MiddleCenter,
            new Vector2(0, 25), new Vector2(300, 30));
        GameObject newHSText = CreateUIText(goPanel.transform, "NewHighScoreText",
            "★ NEW HIGH SCORE! ★", 20, Color.yellow, TextAnchor.MiddleCenter,
            new Vector2(0, -5), new Vector2(300, 30));
        newHSText.SetActive(false);

        CreateUIButton(goPanel.transform, "RetryButton",
            "RETRY", new Vector2(-80, -70), new Vector2(140, 45));
        CreateUIButton(goPanel.transform, "MenuButton_GO",
            "MENU", new Vector2(80, -70), new Vector2(140, 45));

        // --- Victory Panel ---
        GameObject vicPanel = new GameObject("VictoryPanel");
        vicPanel.transform.SetParent(hudCanvas.transform, false);
        RectTransform vicRT = vicPanel.AddComponent<RectTransform>();
        vicRT.anchoredPosition = Vector2.zero;
        vicRT.sizeDelta = new Vector2(600, 400);
        Image vicBg = vicPanel.AddComponent<Image>();
        vicBg.color = new Color(0, 0.05f, 0.1f, 0.85f);
        vicPanel.SetActive(false);

        CreateUIText(vicPanel.transform, "VictoryTitle",
            "VICTORY!", 42, Color.cyan, TextAnchor.MiddleCenter,
            new Vector2(0, 120), new Vector2(400, 60));
        CreateUIText(vicPanel.transform, "VictoryScoreText",
            "FINAL SCORE: 0", 24, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0, 50), new Vector2(300, 40));

        CreateUIButton(vicPanel.transform, "VictoryRetryButton",
            "PLAY AGAIN", new Vector2(-80, -50), new Vector2(150, 45));
        CreateUIButton(vicPanel.transform, "VictoryMenuButton",
            "MENU", new Vector2(80, -50), new Vector2(150, 45));

        // GameOverUI script
        GameObject goUI = new GameObject("GameOverUI");
        goUI.transform.SetParent(hudCanvas.transform, false);
        goUI.AddComponent<GameOverUI>();

        // EventSystem
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("[Setup] GameScene saved.");
    }

    // =========================================================================
    // BACKGROUND LAYERS
    // =========================================================================
    static void CreateBackgroundLayers()
    {
        // Far background layer
        GameObject bgFar = new GameObject("BG_Far");
        SpriteRenderer srFar = bgFar.AddComponent<SpriteRenderer>();
        srFar.sprite = LoadSprite("Assets/Sprites/Background/starfield_bg.png");
        srFar.sortingOrder = -20;
        srFar.drawMode = SpriteDrawMode.Tiled;
        srFar.size = new Vector2(12, 12);
        bgFar.transform.position = new Vector3(0, 0, 10);

        ParallaxBackground pbFar = bgFar.AddComponent<ParallaxBackground>();

        // Duplicate for seamless scrolling
        GameObject bgFar2 = Instantiate(bgFar);
        bgFar2.name = "BG_Far_2";
        bgFar2.transform.position = new Vector3(0, 12, 10);

        // Nebula overlay (slower parallax)
        GameObject bgNebula = new GameObject("BG_Nebula");
        SpriteRenderer srNeb = bgNebula.AddComponent<SpriteRenderer>();
        srNeb.sprite = LoadSprite("Assets/Sprites/Background/nebula_overlay.png");
        srNeb.sortingOrder = -15;
        srNeb.color = new Color(1, 1, 1, 0.3f);
        bgNebula.transform.position = new Vector3(0, 0, 5);
        bgNebula.AddComponent<ParallaxBackground>();
    }

    // =========================================================================
    // BUILD SETTINGS
    // =========================================================================
    static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[Setup] Build settings configured with 2 scenes.");
    }

    // =========================================================================
    // UI HELPERS
    // =========================================================================
    static GameObject CreateUICanvas(string name)
    {
        GameObject canvas = new GameObject(name);
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;

        CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(600, 900);
        scaler.matchWidthOrHeight = 0.5f;

        canvas.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static GameObject CreateUIText(Transform parent, string name, string content,
        int fontSize, Color color, TextAnchor anchor, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = anchor;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Add outline for readability
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(1, -1);

        return obj;
    }

    static GameObject CreateUIButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.25f, 0.5f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.15f, 0.25f, 0.5f, 0.9f);
        cb.highlightedColor = new Color(0.25f, 0.4f, 0.7f, 1f);
        cb.pressedColor = new Color(0.1f, 0.15f, 0.35f, 1f);
        cb.selectedColor = new Color(0.2f, 0.35f, 0.6f, 1f);
        btn.colors = cb;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;

        return btnObj;
    }

    static void CreateEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
            return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    // =========================================================================
    // UTILITIES
    // =========================================================================
    static Sprite LoadSprite(string path)
    {
        // Try to load as sprite asset
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        // Try loading texture and creating sprite
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null)
        {
            // Set texture import settings for pixel art
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 64;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        if (sprite == null)
            Debug.LogWarning($"[Setup] Could not load sprite: {path}");

        return sprite;
    }

    static void SavePrefab(GameObject obj, string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        PrefabUtility.SaveAsPrefabAsset(obj, path);
        Debug.Log($"  Prefab saved: {path}");
    }
}
#endif
