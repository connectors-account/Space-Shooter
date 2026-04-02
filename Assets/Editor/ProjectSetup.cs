using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Editor utility to automatically set up the Space Shooter project.
/// Run from: Menu > Space Shooter > Setup Project
/// This creates all prefabs, scenes, tags, layers, and configures the project.
/// </summary>
public class ProjectSetup : EditorWindow
{
    [MenuItem("Space Shooter/Setup Entire Project")]
    public static void SetupProject()
    {
        if (!EditorUtility.DisplayDialog("Setup Space Shooter",
            "This will create all prefabs, scenes, tags, and layers for the Space Shooter project.\n\nContinue?",
            "Yes, Set Up", "Cancel"))
            return;

        SetupTagsAndLayers();
        SetupPhysics2D();
        CreatePrefabs();
        CreateMainMenuScene();
        CreateGameScene();
        SetupBuildSettings();

        EditorUtility.DisplayDialog("Setup Complete",
            "Space Shooter project has been set up!\n\n" +
            "1. Scenes: MainMenu, GameScene\n" +
            "2. Prefabs: Player, Enemies, Bullets, PowerUps, Effects\n" +
            "3. Tags & Layers configured\n\n" +
            "Press Play in the MainMenu scene to start!",
            "OK");
    }

    [MenuItem("Space Shooter/Setup Tags and Layers Only")]
    public static void SetupTagsAndLayers()
    {
        // Add tags
        AddTag("Player");
        AddTag("Enemy");
        AddTag("PlayerBullet");
        AddTag("EnemyBullet");
        AddTag("PowerUp");
        AddTag("Boundary");

        // Add sorting layers
        AddSortingLayer("Background");
        AddSortingLayer("Stars");
        AddSortingLayer("Entities");
        AddSortingLayer("Bullets");
        AddSortingLayer("Effects");
        AddSortingLayer("UI");

        // Add layers for collision
        AddLayer(8, "PlayerBullet");
        AddLayer(9, "EnemyBullet");
        AddLayer(10, "Player");
        AddLayer(11, "Enemy");
        AddLayer(12, "PowerUp");

        Debug.Log("[SpaceShooter] Tags and Layers configured.");
    }

    static void SetupPhysics2D()
    {
        // Configure Physics2D collision matrix
        // PlayerBullet (8) should NOT collide with Player (10) or other PlayerBullets
        // EnemyBullet (9) should NOT collide with Enemy (11) or other EnemyBullets
        Physics2D.IgnoreLayerCollision(8, 10, true);  // PlayerBullet vs Player
        Physics2D.IgnoreLayerCollision(8, 8, true);   // PlayerBullet vs PlayerBullet
        Physics2D.IgnoreLayerCollision(9, 11, true);  // EnemyBullet vs Enemy
        Physics2D.IgnoreLayerCollision(9, 9, true);   // EnemyBullet vs EnemyBullet
        Physics2D.IgnoreLayerCollision(8, 9, true);   // PlayerBullet vs EnemyBullet
        Physics2D.IgnoreLayerCollision(12, 11, true);  // PowerUp vs Enemy
        Physics2D.IgnoreLayerCollision(12, 8, true);  // PowerUp vs PlayerBullet
        Physics2D.IgnoreLayerCollision(12, 9, true);  // PowerUp vs EnemyBullet

        Debug.Log("[SpaceShooter] Physics2D collision matrix configured.");
    }

    static void CreatePrefabs()
    {
        CreatePlayerPrefab();
        CreateBulletPrefabs();
        CreateEnemyPrefabs();
        CreatePowerUpPrefabs();
        CreateExplosionPrefab();
        Debug.Log("[SpaceShooter] All prefabs created.");
    }

    static void CreatePlayerPrefab()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = 10;

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Sprites/player_ship");
        sr.sortingLayerName = "Entities";
        sr.sortingOrder = 10;

        // Physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.6f);

        // Scripts
        player.AddComponent<PlayerController>();
        CollisionHandler ch = player.AddComponent<CollisionHandler>();

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        // Shield visual
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSR = shield.AddComponent<SpriteRenderer>();
        shieldSR.sprite = LoadSprite("Sprites/shield");
        shieldSR.sortingLayerName = "Effects";
        shieldSR.sortingOrder = 5;
        shield.SetActive(false);

        // Save prefab
        string path = "Assets/Prefabs/Player/Player.prefab";
        EnsureDirectory(path);
        PrefabUtility.SaveAsPrefabAsset(player, path);
        DestroyImmediate(player);
    }

    static void CreateBulletPrefabs()
    {
        // Player bullet
        CreateBulletPrefab("PlayerBullet", "Sprites/bullet_player", true, 8,
            "Assets/Prefabs/Bullets/PlayerBullet.prefab");
        // Enemy bullet
        CreateBulletPrefab("EnemyBullet", "Sprites/bullet_enemy", false, 9,
            "Assets/Prefabs/Bullets/EnemyBullet.prefab");
    }

    static void CreateBulletPrefab(string name, string spritePath, bool isPlayer, int layer, string savePath)
    {
        GameObject bullet = new GameObject(name);
        bullet.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";
        bullet.layer = layer;

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spritePath);
        sr.sortingLayerName = "Bullets";
        sr.sortingOrder = 0;
        sr.color = isPlayer ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.3f, 0.2f);

        CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.1f;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        bullet.AddComponent<BulletController>();

        EnsureDirectory(savePath);
        PrefabUtility.SaveAsPrefabAsset(bullet, savePath);
        DestroyImmediate(bullet);
    }

    static void CreateEnemyPrefabs()
    {
        CreateEnemyPrefab("EnemyBasic", "Sprites/enemy_basic",
            EnemyController.EnemyType.Basic, EnemyController.MovementPattern.StraightDown,
            1, 3f, 100, 2.5f, "Assets/Prefabs/Enemies/EnemyBasic.prefab");

        CreateEnemyPrefab("EnemyZigzag", "Sprites/enemy_zigzag",
            EnemyController.EnemyType.Zigzag, EnemyController.MovementPattern.Zigzag,
            2, 2.5f, 200, 2f, "Assets/Prefabs/Enemies/EnemyZigzag.prefab");

        CreateEnemyPrefab("EnemyBomber", "Sprites/enemy_bomber",
            EnemyController.EnemyType.Bomber, EnemyController.MovementPattern.SineWave,
            4, 1.5f, 350, 3f, "Assets/Prefabs/Enemies/EnemyBomber.prefab");

        CreateEnemyPrefab("EnemyElite", "Sprites/enemy_elite",
            EnemyController.EnemyType.Elite, EnemyController.MovementPattern.CircleEntry,
            6, 2f, 500, 1.5f, "Assets/Prefabs/Enemies/EnemyElite.prefab");
    }

    static void CreateEnemyPrefab(string name, string spritePath,
        EnemyController.EnemyType type, EnemyController.MovementPattern pattern,
        int health, float speed, int score, float fireRate, string savePath)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";
        enemy.layer = 11;

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spritePath);
        sr.sortingLayerName = "Entities";
        sr.sortingOrder = 5;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        EnemyController ec = enemy.AddComponent<EnemyController>();

        // Load bullet prefab reference
        GameObject enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Bullets/EnemyBullet.prefab");

        // We need to set serialized fields via SerializedObject
        SerializedObject so = new SerializedObject(ec);
        so.FindProperty("enemyType").enumValueIndex = (int)type;
        so.FindProperty("movementPattern").enumValueIndex = (int)pattern;
        so.FindProperty("maxHealth").intValue = health;
        so.FindProperty("moveSpeed").floatValue = speed;
        so.FindProperty("scoreValue").intValue = score;
        so.FindProperty("fireRate").floatValue = fireRate;
        so.FindProperty("canShoot").boolValue = true;
        so.FindProperty("bulletSpeed").floatValue = 5f;
        if (enemyBulletPrefab != null)
            so.FindProperty("bulletPrefab").objectReferenceValue = enemyBulletPrefab;

        // Load power-up prefabs for drops
        so.FindProperty("dropChance").floatValue = type == EnemyController.EnemyType.Elite ? 0.5f : 0.15f;
        so.ApplyModifiedProperties();

        // Explosion reference
        GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Effects/Explosion.prefab");
        if (explosionPrefab != null)
        {
            SerializedObject so2 = new SerializedObject(ec);
            so2.FindProperty("explosionPrefab").objectReferenceValue = explosionPrefab;
            so2.ApplyModifiedProperties();
        }

        EnsureDirectory(savePath);
        PrefabUtility.SaveAsPrefabAsset(enemy, savePath);
        DestroyImmediate(enemy);
    }

    static void CreatePowerUpPrefabs()
    {
        CreatePowerUpPrefab("PowerUp_Weapon", "Sprites/powerup_weapon",
            PowerUpController.PowerUpType.WeaponUpgrade, "Assets/Prefabs/PowerUps/PowerUp_Weapon.prefab");
        CreatePowerUpPrefab("PowerUp_Shield", "Sprites/powerup_shield",
            PowerUpController.PowerUpType.Shield, "Assets/Prefabs/PowerUps/PowerUp_Shield.prefab");
        CreatePowerUpPrefab("PowerUp_Health", "Sprites/powerup_health",
            PowerUpController.PowerUpType.Health, "Assets/Prefabs/PowerUps/PowerUp_Health.prefab");
        CreatePowerUpPrefab("PowerUp_Speed", "Sprites/powerup_speed",
            PowerUpController.PowerUpType.SpeedBoost, "Assets/Prefabs/PowerUps/PowerUp_Speed.prefab");
        CreatePowerUpPrefab("PowerUp_Score", "Sprites/powerup_score",
            PowerUpController.PowerUpType.ScoreBonus, "Assets/Prefabs/PowerUps/PowerUp_Score.prefab");
    }

    static void CreatePowerUpPrefab(string name, string spritePath,
        PowerUpController.PowerUpType type, string savePath)
    {
        GameObject powerUp = new GameObject(name);
        powerUp.tag = "PowerUp";
        powerUp.layer = 12;

        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(spritePath);
        sr.sortingLayerName = "Entities";
        sr.sortingOrder = 8;

        CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        Rigidbody2D rb = powerUp.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        PowerUpController pc = powerUp.AddComponent<PowerUpController>();

        SerializedObject so = new SerializedObject(pc);
        so.FindProperty("powerUpType").enumValueIndex = (int)type;
        so.ApplyModifiedProperties();

        EnsureDirectory(savePath);
        PrefabUtility.SaveAsPrefabAsset(powerUp, savePath);
        DestroyImmediate(powerUp);
    }

    static void CreateExplosionPrefab()
    {
        GameObject explosion = new GameObject("Explosion");

        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Sprites/explosion");
        sr.sortingLayerName = "Effects";
        sr.sortingOrder = 20;

        explosion.AddComponent<ExplosionEffect>();
        AutoDestroy ad = explosion.AddComponent<AutoDestroy>();

        string path = "Assets/Prefabs/Effects/Explosion.prefab";
        EnsureDirectory(path);
        PrefabUtility.SaveAsPrefabAsset(explosion, path);
        DestroyImmediate(explosion);
    }

    static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.orthographicSize = 5f;
        }

        // Game Initializer
        GameObject initializer = new GameObject("GameInitializer");
        initializer.AddComponent<GameInitializer>();

        // Starfield
        GameObject starfield = new GameObject("Starfield");
        starfield.AddComponent<StarfieldGenerator>();

        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Title
        GameObject titleObj = CreateUIText(canvasObj.transform, "Title", "SPACE SHOOTER",
            new Vector2(0, 200), 72, TextAnchor.MiddleCenter, Color.cyan);

        // High Score
        GameObject highScoreObj = CreateUIText(canvasObj.transform, "HighScore", "HIGH SCORE: 0",
            new Vector2(0, 100), 28, TextAnchor.MiddleCenter, Color.yellow);

        // Start Button
        GameObject startBtn = CreateUIButton(canvasObj.transform, "StartButton", "START GAME",
            new Vector2(0, -20), new Vector2(300, 60));

        // Options Button
        GameObject optionsBtn = CreateUIButton(canvasObj.transform, "OptionsButton", "OPTIONS",
            new Vector2(0, -100), new Vector2(300, 60));

        // Quit Button
        GameObject quitBtn = CreateUIButton(canvasObj.transform, "QuitButton", "QUIT",
            new Vector2(0, -180), new Vector2(300, 60));

        // Version text
        GameObject versionObj = CreateUIText(canvasObj.transform, "Version", "v1.0",
            new Vector2(0, -320), 18, TextAnchor.MiddleCenter, Color.gray);

        // Options Panel (hidden by default)
        GameObject optionsPanel = new GameObject("OptionsPanel");
        optionsPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform optRT = optionsPanel.AddComponent<RectTransform>();
        optRT.anchorMin = Vector2.zero;
        optRT.anchorMax = Vector2.one;
        optRT.sizeDelta = Vector2.zero;
        Image optBg = optionsPanel.AddComponent<Image>();
        optBg.color = new Color(0, 0, 0, 0.8f);

        CreateUIText(optionsPanel.transform, "OptionsTitle", "OPTIONS",
            new Vector2(0, 150), 48, TextAnchor.MiddleCenter, Color.white);

        // Music slider
        CreateUIText(optionsPanel.transform, "MusicLabel", "MUSIC",
            new Vector2(-200, 50), 24, TextAnchor.MiddleLeft, Color.white);
        GameObject musicSlider = CreateUISlider(optionsPanel.transform, "MusicSlider",
            new Vector2(100, 50), new Vector2(300, 30));

        CreateUIText(optionsPanel.transform, "SFXLabel", "SFX",
            new Vector2(-200, -20), 24, TextAnchor.MiddleLeft, Color.white);
        GameObject sfxSlider = CreateUISlider(optionsPanel.transform, "SFXSlider",
            new Vector2(100, -20), new Vector2(300, 30));

        GameObject backBtn = CreateUIButton(optionsPanel.transform, "BackButton", "BACK",
            new Vector2(0, -120), new Vector2(200, 50));

        optionsPanel.SetActive(false);

        // MenuManager
        MenuManager mm = canvasObj.AddComponent<MenuManager>();
        SerializedObject mmSO = new SerializedObject(mm);
        mmSO.FindProperty("startButton").objectReferenceValue = startBtn.GetComponent<Button>();
        mmSO.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
        mmSO.FindProperty("titleText").objectReferenceValue = titleObj.GetComponent<Text>();
        mmSO.FindProperty("highScoreText").objectReferenceValue = highScoreObj.GetComponent<Text>();
        mmSO.FindProperty("versionText").objectReferenceValue = versionObj.GetComponent<Text>();
        mmSO.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
        mmSO.FindProperty("optionsButton").objectReferenceValue = optionsBtn.GetComponent<Button>();
        mmSO.FindProperty("optionsBackButton").objectReferenceValue = backBtn.GetComponent<Button>();
        mmSO.FindProperty("musicVolumeSlider").objectReferenceValue = musicSlider.GetComponent<Slider>();
        mmSO.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider.GetComponent<Slider>();
        mmSO.ApplyModifiedProperties();

        // EventSystem
        if (GameObject.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        EnsureDirectory("Assets/Scenes/MainMenu.unity");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("[SpaceShooter] MainMenu scene created.");
    }

    static void CreateGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.orthographicSize = 5f;
        }

        // Game Initializer
        GameObject initializer = new GameObject("GameInitializer");
        GameInitializer gi = initializer.AddComponent<GameInitializer>();
        SerializedObject giSO = new SerializedObject(gi);
        giSO.FindProperty("isGameScene").boolValue = true;
        giSO.ApplyModifiedProperties();

        // Starfield
        GameObject starfield = new GameObject("Starfield");
        starfield.AddComponent<StarfieldGenerator>();

        // Background
        Sprite bgSprite = LoadSprite("Sprites/background");
        if (bgSprite != null)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject bg = new GameObject("Background_" + i);
                SpriteRenderer bgSR = bg.AddComponent<SpriteRenderer>();
                bgSR.sprite = bgSprite;
                bgSR.sortingLayerName = "Background";
                bgSR.sortingOrder = -100;
                bg.transform.position = new Vector3(0, i * 10f, 10f);
                bg.transform.localScale = new Vector3(2f, 2f, 1f);
                ParallaxBackground pb = bg.AddComponent<ParallaxBackground>();
            }
        }

        // Player
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
        if (playerPrefab != null)
        {
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(0f, -3.5f, 0f);

            // Wire up bullet prefab
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Prefabs/Bullets/PlayerBullet.prefab");
                GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Prefabs/Effects/Explosion.prefab");

                SerializedObject pcSO = new SerializedObject(pc);
                if (bulletPrefab != null)
                    pcSO.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
                if (explosionPrefab != null)
                    pcSO.FindProperty("explosionPrefab").objectReferenceValue = explosionPrefab;

                Transform firePoint = player.transform.Find("FirePoint");
                if (firePoint != null)
                    pcSO.FindProperty("firePoint").objectReferenceValue = firePoint;

                Transform shieldVisual = player.transform.Find("ShieldVisual");
                if (shieldVisual != null)
                    pcSO.FindProperty("shieldVisual").objectReferenceValue = shieldVisual.gameObject;

                pcSO.ApplyModifiedProperties();
            }
        }

        // Spawn Manager
        GameObject spawnMgr = new GameObject("SpawnManager");
        SpawnManager sm = spawnMgr.AddComponent<SpawnManager>();
        SerializedObject smSO = new SerializedObject(sm);

        GameObject basicPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EnemyBasic.prefab");
        GameObject zigzagPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EnemyZigzag.prefab");
        GameObject bomberPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EnemyBomber.prefab");
        GameObject elitePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EnemyElite.prefab");

        if (basicPrefab != null) smSO.FindProperty("basicEnemyPrefab").objectReferenceValue = basicPrefab;
        if (zigzagPrefab != null) smSO.FindProperty("zigzagEnemyPrefab").objectReferenceValue = zigzagPrefab;
        if (bomberPrefab != null) smSO.FindProperty("bomberEnemyPrefab").objectReferenceValue = bomberPrefab;
        if (elitePrefab != null) smSO.FindProperty("eliteEnemyPrefab").objectReferenceValue = elitePrefab;
        smSO.ApplyModifiedProperties();

        // --- UI Canvas ---
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // HUD
        GameObject scoreText = CreateUIText(canvasObj.transform, "ScoreText", "SCORE: 0",
            new Vector2(-750, 480), 32, TextAnchor.UpperLeft, Color.white);
        GameObject waveText = CreateUIText(canvasObj.transform, "WaveText", "WAVE: 1",
            new Vector2(0, 480), 32, TextAnchor.UpperCenter, Color.yellow);
        GameObject livesText = CreateUIText(canvasObj.transform, "LivesText", "LIVES: 3",
            new Vector2(750, 480), 32, TextAnchor.UpperRight, Color.green);

        // Health bar
        GameObject healthBar = CreateUISlider(canvasObj.transform, "HealthBar",
            new Vector2(0, -450), new Vector2(400, 20));
        Slider healthSlider = healthBar.GetComponent<Slider>();
        healthSlider.interactable = false;
        // Set fill color to red
        Transform fill = healthBar.transform.Find("Fill Area/Fill");
        if (fill != null)
        {
            Image fillImg = fill.GetComponent<Image>();
            if (fillImg != null) fillImg.color = Color.red;
        }

        GameObject healthText = CreateUIText(canvasObj.transform, "HealthText", "5 / 5",
            new Vector2(0, -480), 20, TextAnchor.MiddleCenter, Color.white);

        // Wave Announcement
        GameObject waveAnnouncement = new GameObject("WaveAnnouncement");
        waveAnnouncement.transform.SetParent(canvasObj.transform, false);
        RectTransform waRT = waveAnnouncement.AddComponent<RectTransform>();
        waRT.anchoredPosition = new Vector2(0, 100);
        waRT.sizeDelta = new Vector2(600, 100);
        GameObject waveAnnouncementText = CreateUIText(waveAnnouncement.transform, "WaveAnnouncementText",
            "WAVE 1", Vector2.zero, 64, TextAnchor.MiddleCenter, Color.yellow);
        waveAnnouncement.SetActive(false);

        // Pause Menu Panel
        GameObject pausePanel = new GameObject("PauseMenuPanel");
        pausePanel.transform.SetParent(canvasObj.transform, false);
        RectTransform ppRT = pausePanel.AddComponent<RectTransform>();
        ppRT.anchorMin = Vector2.zero;
        ppRT.anchorMax = Vector2.one;
        ppRT.sizeDelta = Vector2.zero;
        Image ppBg = pausePanel.AddComponent<Image>();
        ppBg.color = new Color(0, 0, 0, 0.7f);

        CreateUIText(pausePanel.transform, "PausedTitle", "PAUSED",
            new Vector2(0, 150), 56, TextAnchor.MiddleCenter, Color.white);
        GameObject resumeBtn = CreateUIButton(pausePanel.transform, "ResumeButton", "RESUME",
            new Vector2(0, 30), new Vector2(250, 55));
        GameObject pMenuBtn = CreateUIButton(pausePanel.transform, "PauseMainMenuButton", "MAIN MENU",
            new Vector2(0, -40), new Vector2(250, 55));
        GameObject pQuitBtn = CreateUIButton(pausePanel.transform, "PauseQuitButton", "QUIT",
            new Vector2(0, -110), new Vector2(250, 55));
        pausePanel.SetActive(false);

        // Game Over Panel
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform goRT = gameOverPanel.AddComponent<RectTransform>();
        goRT.anchorMin = Vector2.zero;
        goRT.anchorMax = Vector2.one;
        goRT.sizeDelta = Vector2.zero;
        Image goBg = gameOverPanel.AddComponent<Image>();
        goBg.color = new Color(0, 0, 0, 0.8f);

        CreateUIText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0, 180), 64, TextAnchor.MiddleCenter, Color.red);
        GameObject goScoreText = CreateUIText(gameOverPanel.transform, "GameOverScore", "SCORE: 0",
            new Vector2(0, 80), 36, TextAnchor.MiddleCenter, Color.white);
        GameObject goHighScoreText = CreateUIText(gameOverPanel.transform, "GameOverHighScore", "HIGH SCORE: 0",
            new Vector2(0, 30), 28, TextAnchor.MiddleCenter, Color.yellow);
        GameObject restartBtn = CreateUIButton(gameOverPanel.transform, "RestartButton", "RESTART",
            new Vector2(0, -60), new Vector2(250, 55));
        GameObject goMenuBtn = CreateUIButton(gameOverPanel.transform, "GameOverMainMenuButton", "MAIN MENU",
            new Vector2(0, -130), new Vector2(250, 55));
        gameOverPanel.SetActive(false);

        // UIManager
        UIManager uiMgr = canvasObj.AddComponent<UIManager>();
        SerializedObject uiSO = new SerializedObject(uiMgr);
        uiSO.FindProperty("scoreText").objectReferenceValue = scoreText.GetComponent<Text>();
        uiSO.FindProperty("waveText").objectReferenceValue = waveText.GetComponent<Text>();
        uiSO.FindProperty("livesText").objectReferenceValue = livesText.GetComponent<Text>();
        uiSO.FindProperty("healthSlider").objectReferenceValue = healthSlider;
        uiSO.FindProperty("healthText").objectReferenceValue = healthText.GetComponent<Text>();
        uiSO.FindProperty("waveAnnouncement").objectReferenceValue = waveAnnouncement;
        uiSO.FindProperty("waveAnnouncementText").objectReferenceValue = waveAnnouncementText.GetComponent<Text>();
        uiSO.FindProperty("pauseMenuPanel").objectReferenceValue = pausePanel;
        uiSO.FindProperty("resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
        uiSO.FindProperty("pauseMainMenuButton").objectReferenceValue = pMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("pauseQuitButton").objectReferenceValue = pQuitBtn.GetComponent<Button>();
        uiSO.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        uiSO.FindProperty("gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<Text>();
        uiSO.FindProperty("gameOverHighScoreText").objectReferenceValue = goHighScoreText.GetComponent<Text>();
        uiSO.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        uiSO.FindProperty("gameOverMainMenuButton").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        // EventSystem
        if (GameObject.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        EnsureDirectory("Assets/Scenes/GameScene.unity");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("[SpaceShooter] GameScene created.");
    }

    static void SetupBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[SpaceShooter] Build settings configured.");
    }

    // =================== HELPERS ===================

    static Sprite LoadSprite(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            // Try loading as texture and create sprite
            Texture2D tex = Resources.Load<Texture2D>(resourcePath);
            if (tex != null)
            {
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return sprite;
    }

    static void EnsureDirectory(string assetPath)
    {
        string dir = Path.GetDirectoryName(assetPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }
    }

    static GameObject CreateUIText(Transform parent, string name, string text,
        Vector2 position, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(600, 80);

        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = color;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        // Add outline for readability
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(2, -2);

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
        img.color = new Color(0.15f, 0.3f, 0.6f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.3f, 0.6f, 0.9f);
        colors.highlightedColor = new Color(0.25f, 0.45f, 0.8f, 1f);
        colors.pressedColor = new Color(0.1f, 0.2f, 0.4f, 1f);
        colors.selectedColor = new Color(0.2f, 0.4f, 0.7f, 1f);
        btn.colors = colors;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform labelRT = labelObj.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.sizeDelta = Vector2.zero;

        Text t = labelObj.AddComponent<Text>();
        t.text = label;
        t.fontSize = 24;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return btnObj;
    }

    static GameObject CreateUISlider(Transform parent, string name,
        Vector2 position, Vector2 size)
    {
        // Create slider using DefaultControls
        DefaultControls.Resources uiResources = new DefaultControls.Resources();
        GameObject sliderObj = DefaultControls.CreateSlider(uiResources);
        sliderObj.name = name;
        sliderObj.transform.SetParent(parent, false);

        RectTransform rt = sliderObj.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;

        // Style the fill
        Transform fillArea = sliderObj.transform.Find("Fill Area/Fill");
        if (fillArea != null)
        {
            Image fillImg = fillArea.GetComponent<Image>();
            if (fillImg != null) fillImg.color = new Color(0.3f, 0.6f, 1f);
        }

        return sliderObj;
    }

    static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    static void AddSortingLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");

        for (int i = 0; i < sortingLayers.arraySize; i++)
        {
            if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName)
                return;
        }

        sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
        SerializedProperty newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
        newLayer.FindPropertyRelative("name").stringValue = layerName;
        newLayer.FindPropertyRelative("uniqueID").intValue = layerName.GetHashCode();
        tagManager.ApplyModifiedProperties();
    }

    static void AddLayer(int index, string layerName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        if (index < layers.arraySize)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
            }
        }
    }
}
