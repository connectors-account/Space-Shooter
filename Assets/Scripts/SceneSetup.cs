using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneSetup - Runtime scene bootstrapper.
/// Attach to a GameObject in each scene to programmatically create all GameObjects,
/// components, UI, and wiring that would normally be done in the Unity Editor.
/// This ensures the game works even without .unity scene files being fully configured.
/// </summary>
public class SceneSetup : MonoBehaviour
{
    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "MainMenu":
                SetupMainMenu();
                break;
            case "GamePlay":
                SetupGamePlay();
                break;
            case "GameOver":
                SetupGameOver();
                break;
            default:
                Debug.LogWarning("SceneSetup: Unknown scene '" + sceneName + "'");
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  MAIN MENU SCENE
    // ═══════════════════════════════════════════════════════════
    private void SetupMainMenu()
    {
        // Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Canvas
        GameObject canvasObj = CreateCanvas("MenuCanvas");
        Canvas canvas = canvasObj.GetComponent<Canvas>();

        // Title
        Text titleText = CreateText(canvasObj, "TitleText", "SPACE SHOOTER",
            new Vector2(0, 100), 48, Color.cyan, TextAnchor.MiddleCenter);

        // Subtitle
        CreateText(canvasObj, "SubtitleText", "Defend the Galaxy!",
            new Vector2(0, 40), 20, Color.white, TextAnchor.MiddleCenter);

        // Play Button
        Button playBtn = CreateButton(canvasObj, "PlayButton", "PLAY",
            new Vector2(0, -40), new Vector2(200, 50), new Color(0.1f, 0.6f, 0.1f));

        // Quit Button
        Button quitBtn = CreateButton(canvasObj, "QuitButton", "QUIT",
            new Vector2(0, -110), new Vector2(200, 50), new Color(0.6f, 0.1f, 0.1f));

        // Controls info
        CreateText(canvasObj, "ControlsText",
            "Controls:\nWASD / Arrow Keys - Move\nSpace - Shoot",
            new Vector2(0, -200), 14, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleCenter);

        // MenuManager
        MenuManager mm = canvasObj.AddComponent<MenuManager>();
        mm.titleText = titleText;
        mm.playButton = playBtn;
        mm.quitButton = quitBtn;

        // Background stars effect
        CreateStarBackground();
    }

    // ═══════════════════════════════════════════════════════════
    //  GAMEPLAY SCENE
    // ═══════════════════════════════════════════════════════════
    private void SetupGamePlay()
    {
        // Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // ── Parallax Background ──
        SetupParallaxBackground();

        // ── Player ──
        GameObject player = CreatePlayer();

        // ── Bullet Prefabs (stored as inactive templates) ──
        GameObject playerBulletPrefab = CreateBulletTemplate("PlayerBulletTemplate", true);
        GameObject enemyBulletPrefab = CreateBulletTemplate("EnemyBulletTemplate", false);

        // ── Power-Up Prefabs ──
        GameObject healthPU = CreatePowerUpTemplate("HealthPowerUpTemplate", PowerUpController.PowerUpType.HealthRestore);
        GameObject rapidPU = CreatePowerUpTemplate("RapidFirePowerUpTemplate", PowerUpController.PowerUpType.RapidFire);

        // ── Enemy Prefabs ──
        GameObject straightEnemy = CreateEnemyTemplate("StraightEnemyTemplate",
            EnemyController.EnemyType.Straight, 1, 100, 3f, enemyBulletPrefab,
            new GameObject[] { healthPU, rapidPU },
            LoadSprite("enemy_straight"), new Color(0.86f, 0.16f, 0.16f));

        GameObject zigzagEnemy = CreateEnemyTemplate("ZigzagEnemyTemplate",
            EnemyController.EnemyType.Zigzag, 2, 200, 2.5f, enemyBulletPrefab,
            new GameObject[] { healthPU, rapidPU },
            LoadSprite("enemy_zigzag"), new Color(0.16f, 0.78f, 0.16f));

        GameObject chaserEnemy = CreateEnemyTemplate("ChaserEnemyTemplate",
            EnemyController.EnemyType.Chaser, 3, 350, 2f, enemyBulletPrefab,
            new GameObject[] { healthPU, rapidPU },
            LoadSprite("enemy_chaser"), new Color(0.7f, 0.16f, 0.86f));

        // ── Assign bullet prefab to player ──
        PlayerController pc = player.GetComponent<PlayerController>();
        pc.bulletPrefab = playerBulletPrefab;

        // ── GameManager ──
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.totalWaves = 5;
        gm.startingLives = 3;

        // ── SpawnManager ──
        GameObject smObj = new GameObject("SpawnManager");
        SpawnManager sm = smObj.AddComponent<SpawnManager>();
        sm.straightEnemyPrefab = straightEnemy;
        sm.zigzagEnemyPrefab = zigzagEnemy;
        sm.chaserEnemyPrefab = chaserEnemy;

        // ── HUD Canvas ──
        SetupHUD();
    }

    // ═══════════════════════════════════════════════════════════
    //  GAME OVER SCENE
    // ═══════════════════════════════════════════════════════════
    private void SetupGameOver()
    {
        // Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.05f, 0.0f, 0.0f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Canvas
        GameObject canvasObj = CreateCanvas("GameOverCanvas");

        // Title
        Text goTitle = CreateText(canvasObj, "GameOverTitle", "GAME OVER",
            new Vector2(0, 120), 48, Color.red, TextAnchor.MiddleCenter);

        // Score
        Text scoreText = CreateText(canvasObj, "FinalScore", "Score: 0",
            new Vector2(0, 50), 28, Color.white, TextAnchor.MiddleCenter);

        // Wave
        Text waveText = CreateText(canvasObj, "FinalWave", "Wave Reached: 1",
            new Vector2(0, 10), 22, Color.gray, TextAnchor.MiddleCenter);

        // Restart Button
        Button restartBtn = CreateButton(canvasObj, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -60), new Vector2(220, 50), new Color(0.1f, 0.6f, 0.1f));

        // Main Menu Button
        Button menuBtn = CreateButton(canvasObj, "MainMenuButton", "MAIN MENU",
            new Vector2(0, -130), new Vector2(220, 50), new Color(0.3f, 0.3f, 0.6f));

        // MenuManager
        MenuManager mm = canvasObj.AddComponent<MenuManager>();
        mm.gameOverTitleText = goTitle;
        mm.finalScoreText = scoreText;
        mm.finalWaveText = waveText;
        mm.restartButton = restartBtn;
        mm.mainMenuButton = menuBtn;

        CreateStarBackground();
    }

    // ═══════════════════════════════════════════════════════════
    //  HELPER METHODS
    // ═══════════════════════════════════════════════════════════

    private GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    private Text CreateText(GameObject parent, string name, string content,
        Vector2 position, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(600, 80);

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
    }

    private Button CreateButton(GameObject parent, string name, string label,
        Vector2 position, Vector2 size, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();

        // Button label
        Text btnText = CreateText(btnObj, "Label", label,
            Vector2.zero, 24, Color.white, TextAnchor.MiddleCenter);
        RectTransform textRt = btnText.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;

        return btn;
    }

    private GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -5f, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("player_ship");
        if (sr.sprite == null)
        {
            // Fallback: create a simple colored quad
            sr.color = new Color(0f, 0.7f, 1f);
        }
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 8f;
        pc.fireRate = 0.25f;
        pc.bulletSpeed = 12f;

        // Audio sources (ready for sound files)
        pc.shootAudioSource = AddAudioSource(player, "ShootSFX");
        pc.hitAudioSource = AddAudioSource(player, "HitSFX");
        pc.powerUpAudioSource = AddAudioSource(player, "PowerUpSFX");

        return player;
    }

    private GameObject CreateBulletTemplate(string name, bool isPlayer)
    {
        GameObject bullet = new GameObject(name);
        bullet.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(isPlayer ? "bullet_player" : "bullet_enemy");
        sr.color = isPlayer ? Color.cyan : new Color(1f, 0.3f, 0f);
        sr.sortingOrder = 5;

        // Scale bullet down
        bullet.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        BulletController bc = bullet.AddComponent<BulletController>();
        bc.isPlayerBullet = isPlayer;

        // Hide template - it will be Instantiated at runtime
        bullet.SetActive(false);

        return bullet;
    }

    private GameObject CreateEnemyTemplate(string name, EnemyController.EnemyType type,
        int health, int score, float speed, GameObject bulletPrefab,
        GameObject[] powerUps, Sprite sprite, Color fallbackColor)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (sr.sprite == null)
            sr.color = fallbackColor;
        sr.sortingOrder = 8;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.7f);

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.enemyType = type;
        ec.health = health;
        ec.scoreValue = score;
        ec.moveSpeed = speed;
        ec.bulletPrefab = bulletPrefab;
        ec.powerUpPrefabs = powerUps;
        ec.powerUpDropChance = 0.2f;
        ec.canShoot = true;
        ec.fireRate = type == EnemyController.EnemyType.Chaser ? 2.5f : 3f;

        ec.hitAudioSource = AddAudioSource(enemy, "EnemyHitSFX");

        enemy.SetActive(false);

        return enemy;
    }

    private GameObject CreatePowerUpTemplate(string name, PowerUpController.PowerUpType type)
    {
        GameObject pu = new GameObject(name);
        pu.tag = "PowerUp";

        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(type == PowerUpController.PowerUpType.HealthRestore
            ? "powerup_health" : "powerup_rapidfire");
        sr.color = type == PowerUpController.PowerUpType.HealthRestore
            ? Color.green : Color.yellow;
        sr.sortingOrder = 9;

        pu.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        PowerUpController pc = pu.AddComponent<PowerUpController>();
        pc.type = type;

        pu.SetActive(false);

        return pu;
    }

    private void SetupParallaxBackground()
    {
        GameObject bgParent = new GameObject("ParallaxBackground");
        ParallaxBackground pb = bgParent.AddComponent<ParallaxBackground>();
        pb.baseScrollSpeed = 0.5f;

        // Layer 1 - deep stars
        GameObject layer1 = new GameObject("BG_Layer1_Stars");
        layer1.transform.SetParent(bgParent.transform);
        layer1.transform.position = new Vector3(0, 0, 10);
        SpriteRenderer sr1 = layer1.AddComponent<SpriteRenderer>();
        sr1.sprite = LoadSprite("bg_layer1_stars");
        sr1.sortingOrder = -10;
        sr1.drawMode = SpriteDrawMode.Simple;

        // Layer 2 - nebula
        GameObject layer2 = new GameObject("BG_Layer2_Nebula");
        layer2.transform.SetParent(bgParent.transform);
        layer2.transform.position = new Vector3(0, 0, 9);
        SpriteRenderer sr2 = layer2.AddComponent<SpriteRenderer>();
        sr2.sprite = LoadSprite("bg_layer2_nebula");
        sr2.sortingOrder = -9;

        // Parallax will auto-create copies in Start()
    }

    private void SetupHUD()
    {
        GameObject canvasObj = CreateCanvas("HUDCanvas");

        // Score text - top left
        Text scoreText = CreateText(canvasObj, "ScoreText", "Score: 0",
            new Vector2(-350, 900), 28, Color.white, TextAnchor.MiddleLeft);

        // Wave text - top center
        Text waveText = CreateText(canvasObj, "WaveText", "Wave 1",
            new Vector2(0, 900), 28, Color.yellow, TextAnchor.MiddleCenter);

        // Lives text - top right
        Text livesText = CreateText(canvasObj, "LivesText", "Lives: 3",
            new Vector2(350, 900), 28, Color.red, TextAnchor.MiddleRight);

        // ScoreUI component
        ScoreUI scoreUI = canvasObj.AddComponent<ScoreUI>();
        scoreUI.scoreText = scoreText;
        scoreUI.waveText = waveText;

        // HealthUI component
        HealthUI healthUI = canvasObj.AddComponent<HealthUI>();
        healthUI.livesText = livesText;
    }

    private AudioSource AddAudioSource(GameObject obj, string name)
    {
        AudioSource src = obj.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D sound
        return src;
    }

    private Sprite LoadSprite(string name)
    {
        // Try loading from Resources first, then from Sprites folder
        Sprite sprite = Resources.Load<Sprite>("Sprites/" + name);
        if (sprite == null)
            sprite = Resources.Load<Sprite>(name);
        return sprite;
    }

    private void CreateStarBackground()
    {
        // Simple procedural star background using particles or just camera bg color
        // The camera background is already dark, which works for menus
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        }
    }
}
