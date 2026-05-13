// ============================================================================
// GameSetup.cs - Runtime scene bootstrapper for the Game scene
// Creates all game objects, assigns procedural sprites, and wires references.
// This eliminates the need for manually configured prefabs in the Unity Editor.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to a single empty GameObject in the Game scene.
/// On Awake, it creates the player, enemies, bullets, UI, and background
/// entirely from code so the project runs without any pre-built prefabs.
/// </summary>
public class GameSetup : MonoBehaviour
{
    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        // Ensure singletons exist.
        EnsureSingletons();

        // Create game entities.
        GameObject playerObj = CreatePlayer();
        GameObject playerBulletPrefab = CreateBulletPrefab("PlayerBullet", true);
        GameObject enemyBulletPrefab = CreateBulletPrefab("EnemyBullet", false);

        // Create enemy prefabs.
        GameObject[] powerUpPrefabs = CreatePowerUpPrefabs();
        GameObject enemyStraight = CreateEnemyPrefab<EnemyStraight>("EnemyStraight", ProceduralSpriteGenerator.CreateEnemyStraight(), enemyBulletPrefab, powerUpPrefabs, 30, 100, 3f);
        GameObject enemyZigzag = CreateEnemyPrefab<EnemyZigzag>("EnemyZigzag", ProceduralSpriteGenerator.CreateEnemyZigzag(), enemyBulletPrefab, powerUpPrefabs, 40, 150, 2.5f);
        GameObject enemyCircling = CreateEnemyPrefab<EnemyCircling>("EnemyCircling", ProceduralSpriteGenerator.CreateEnemyCircling(), enemyBulletPrefab, powerUpPrefabs, 60, 200, 2f);
        GameObject enemyDiver = CreateEnemyPrefab<EnemyDiver>("EnemyDiver", ProceduralSpriteGenerator.CreateEnemyDiver(), null, powerUpPrefabs, 20, 120, 5f);

        // Assign bullet prefab to player.
        PlayerShooting shooting = playerObj.GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            SetPrivateField(shooting, "bulletPrefab", playerBulletPrefab);
        }

        // Create enemy spawner.
        CreateEnemySpawner(enemyStraight, enemyZigzag, enemyCircling, enemyDiver);

        // Create background.
        CreateBackground();

        // Create UI.
        CreateGameUI();

        // Set camera.
        SetupCamera();

        // Deactivate prefab templates (they'll be instantiated by the spawner).
        enemyStraight.SetActive(false);
        enemyZigzag.SetActive(false);
        enemyCircling.SetActive(false);
        enemyDiver.SetActive(false);
        playerBulletPrefab.SetActive(false);
        enemyBulletPrefab.SetActive(false);

        // Play game music.
        AudioManager.Instance?.PlayMusic("game_music");
    }

    // ========================================================================
    // Singleton Bootstrapping
    // ========================================================================

    private void EnsureSingletons()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
        if (AudioManager.Instance == null)
        {
            GameObject amObj = new GameObject("AudioManager");
            amObj.AddComponent<AudioManager>();
        }
        if (GameBounds.Instance == null)
        {
            GameObject gbObj = new GameObject("GameBounds");
            gbObj.AddComponent<GameBounds>();
        }
    }

    // ========================================================================
    // Player Creation
    // ========================================================================

    private GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        player.transform.position = new Vector3(0f, -3.5f, 0f);

        // Sprite.
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteGenerator.CreatePlayerShip();
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 0;

        // Physics.
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Collider.
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.8f);
        col.isTrigger = true;

        // Components.
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerShooting>();

        // Shield visual child.
        GameObject shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.SetParent(player.transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSR = shieldVisual.AddComponent<SpriteRenderer>();
        shieldSR.sprite = ProceduralSpriteGenerator.CreateShieldVisual();
        shieldSR.sortingLayerName = "Effects";
        shieldSR.sortingOrder = 1;
        shieldVisual.AddComponent<ShieldVisual>();

        return player;
    }

    // ========================================================================
    // Bullet Prefab Creation
    // ========================================================================

    private GameObject CreateBulletPrefab(string name, bool isPlayer)
    {
        GameObject bullet = new GameObject(name);
        bullet.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";
        bullet.layer = LayerMask.NameToLayer(isPlayer ? "PlayerBullet" : "EnemyBullet");

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = isPlayer ? ProceduralSpriteGenerator.CreatePlayerBullet() : ProceduralSpriteGenerator.CreateEnemyBullet();
        sr.sortingLayerName = "Effects";
        sr.sortingOrder = 0;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
        col.radius = 0.1f;
        col.isTrigger = true;

        Bullet bulletScript = bullet.AddComponent<Bullet>();
        // Set speed and damage via reflection.
        if (isPlayer)
        {
            SetPrivateField(bulletScript, "speed", 14f);
            SetPrivateField(bulletScript, "damage", 15);
        }
        else
        {
            SetPrivateField(bulletScript, "speed", 8f);
            SetPrivateField(bulletScript, "damage", 10);
        }

        return bullet;
    }

    // ========================================================================
    // Enemy Prefab Creation
    // ========================================================================

    private GameObject CreateEnemyPrefab<T>(string name, Sprite sprite, GameObject bulletPrefab, GameObject[] powerUpPrefabs, int health, int score, float speed) where T : EnemyBase
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Foreground";
        sr.sortingOrder = 0;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.7f, 0.7f);
        col.isTrigger = true;

        T enemyScript = enemy.AddComponent<T>();
        SetPrivateField(enemyScript, "maxHealth", health);
        SetPrivateField(enemyScript, "scoreValue", score);
        SetPrivateField(enemyScript, "moveSpeed", speed);
        SetPrivateField(enemyScript, "bulletPrefab", bulletPrefab);
        SetPrivateField(enemyScript, "powerUpPrefabs", powerUpPrefabs);
        SetPrivateField(enemyScript, "canShoot", bulletPrefab != null);

        return enemy;
    }

    // ========================================================================
    // Power-Up Prefab Creation
    // ========================================================================

    private GameObject[] CreatePowerUpPrefabs()
    {
        GameObject[] prefabs = new GameObject[3];

        prefabs[0] = CreatePowerUpPrefab("HealthPowerUp", ProceduralSpriteGenerator.CreateHealthPowerUp(), PowerUp.PowerUpType.Health, 30);
        prefabs[1] = CreatePowerUpPrefab("ShieldPowerUp", ProceduralSpriteGenerator.CreateShieldPowerUp(), PowerUp.PowerUpType.Shield, 50);
        prefabs[2] = CreatePowerUpPrefab("WeaponPowerUp", ProceduralSpriteGenerator.CreateWeaponPowerUp(), PowerUp.PowerUpType.WeaponUpgrade, 1);

        // Deactivate templates; they'll be instantiated by enemies on death.
        foreach (var p in prefabs) p.SetActive(false);

        return prefabs;
    }

    private GameObject CreatePowerUpPrefab(string name, Sprite sprite, PowerUp.PowerUpType type, int amount)
    {
        GameObject obj = new GameObject(name);
        obj.tag = "PowerUp";

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Effects";
        sr.sortingOrder = 2;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.isTrigger = true;

        PowerUp pu = obj.AddComponent<PowerUp>();
        SetPrivateField(pu, "type", type);
        SetPrivateField(pu, "effectAmount", amount);

        return obj;
    }

    // ========================================================================
    // Enemy Spawner
    // ========================================================================

    private void CreateEnemySpawner(GameObject straight, GameObject zigzag, GameObject circling, GameObject diver)
    {
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
        SetPrivateField(spawner, "enemyStraightPrefab", straight);
        SetPrivateField(spawner, "enemyZigzagPrefab", zigzag);
        SetPrivateField(spawner, "enemyCirclingPrefab", circling);
        SetPrivateField(spawner, "enemyDiverPrefab", diver);
    }

    // ========================================================================
    // Background
    // ========================================================================

    private void CreateBackground()
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.AddComponent<BackgroundSetup>();
    }

    // ========================================================================
    // Camera Setup
    // ========================================================================

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }
    }

    // ========================================================================
    // Game UI (Canvas + HUD + PauseMenu)
    // ========================================================================

    private void CreateGameUI()
    {
        // --- Main Canvas ---
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- HUD ---
        HUDManager hud = canvasObj.AddComponent<HUDManager>();

        // Score text (top-left).
        Text scoreText = CreateUIText(canvasObj.transform, "ScoreText", "Score: 0",
            new Vector2(20, -20), new Vector2(300, 40), TextAnchor.UpperLeft, 28, Color.white);
        SetPrivateField(hud, "scoreText", scoreText);

        // High score text (top-left below score).
        Text highScoreText = CreateUIText(canvasObj.transform, "HighScoreText", "Best: 0",
            new Vector2(20, -60), new Vector2(300, 30), TextAnchor.UpperLeft, 20, Color.gray);
        SetPrivateField(hud, "highScoreText", highScoreText);

        // Wave text (top-center).
        Text waveText = CreateUIText(canvasObj.transform, "WaveText", "Wave 1",
            new Vector2(0, -20), new Vector2(200, 40), TextAnchor.UpperCenter, 24, Color.yellow);
        RectTransform waveRT = waveText.GetComponent<RectTransform>();
        waveRT.anchorMin = new Vector2(0.5f, 1f);
        waveRT.anchorMax = new Vector2(0.5f, 1f);
        waveRT.pivot = new Vector2(0.5f, 1f);
        SetPrivateField(hud, "waveText", waveText);

        // Wave announcement (center screen, large).
        Text waveAnnounce = CreateUIText(canvasObj.transform, "WaveAnnouncement", "",
            new Vector2(0, 50), new Vector2(600, 80), TextAnchor.MiddleCenter, 48, Color.yellow);
        RectTransform waRT = waveAnnounce.GetComponent<RectTransform>();
        waRT.anchorMin = new Vector2(0.5f, 0.5f);
        waRT.anchorMax = new Vector2(0.5f, 0.5f);
        waRT.pivot = new Vector2(0.5f, 0.5f);
        waveAnnounce.enabled = false;
        SetPrivateField(hud, "waveAnnouncementText", waveAnnounce);

        // Combo text (top-right).
        Text comboText = CreateUIText(canvasObj.transform, "ComboText", "",
            new Vector2(-20, -20), new Vector2(200, 40), TextAnchor.UpperRight, 28, new Color(1f, 0.5f, 0f));
        RectTransform comboRT = comboText.GetComponent<RectTransform>();
        comboRT.anchorMin = new Vector2(1f, 1f);
        comboRT.anchorMax = new Vector2(1f, 1f);
        comboRT.pivot = new Vector2(1f, 1f);
        SetPrivateField(hud, "comboText", comboText);

        // Weapon level text (top-right below combo).
        Text weaponText = CreateUIText(canvasObj.transform, "WeaponText", "Weapon Lv.1",
            new Vector2(-20, -60), new Vector2(200, 30), TextAnchor.UpperRight, 20, new Color(1f, 0.7f, 0.1f));
        RectTransform weapRT = weaponText.GetComponent<RectTransform>();
        weapRT.anchorMin = new Vector2(1f, 1f);
        weapRT.anchorMax = new Vector2(1f, 1f);
        weapRT.pivot = new Vector2(1f, 1f);
        SetPrivateField(hud, "weaponLevelText", weaponText);

        // Health bar (bottom-left).
        Slider healthBar = CreateUISlider(canvasObj.transform, "HealthBar",
            new Vector2(20, 20), new Vector2(250, 25),
            new Color(0.2f, 0.8f, 0.2f), new Color(0.1f, 0.3f, 0.1f));
        SetPrivateField(hud, "healthBar", healthBar);

        // Health text.
        Text healthText = CreateUIText(canvasObj.transform, "HealthText", "100/100",
            new Vector2(20, 50), new Vector2(200, 25), TextAnchor.LowerLeft, 18, Color.white);
        SetPrivateField(hud, "healthText", healthText);

        // Shield bar (bottom-left, above health).
        Slider shieldBar = CreateUISlider(canvasObj.transform, "ShieldBar",
            new Vector2(20, 75), new Vector2(200, 20),
            new Color(0.3f, 0.6f, 1f), new Color(0.1f, 0.2f, 0.4f));
        shieldBar.gameObject.SetActive(false);
        SetPrivateField(hud, "shieldBar", shieldBar);

        // --- Pause Menu Panel ---
        CreatePauseMenu(canvasObj.transform);
    }

    private void CreatePauseMenu(Transform canvasTransform)
    {
        // Semi-transparent overlay.
        GameObject panel = new GameObject("PausePanel");
        panel.transform.SetParent(canvasTransform, false);
        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.7f);

        // Title.
        Text pauseTitle = CreateUIText(panel.transform, "PauseTitle", "PAUSED",
            new Vector2(0, 100), new Vector2(400, 60), TextAnchor.MiddleCenter, 48, Color.white);
        CenterRectTransform(pauseTitle.GetComponent<RectTransform>());

        // Resume button.
        Button resumeBtn = CreateUIButton(panel.transform, "ResumeButton", "RESUME",
            new Vector2(0, 20), new Vector2(250, 50));

        // Restart button.
        Button restartBtn = CreateUIButton(panel.transform, "RestartButton", "RESTART",
            new Vector2(0, -40), new Vector2(250, 50));

        // Main Menu button.
        Button menuBtn = CreateUIButton(panel.transform, "MainMenuButton", "MAIN MENU",
            new Vector2(0, -100), new Vector2(250, 50));

        // Wire PauseMenuUI.
        PauseMenuUI pauseUI = panel.AddComponent<PauseMenuUI>();
        SetPrivateField(pauseUI, "pausePanel", panel);
        SetPrivateField(pauseUI, "resumeButton", resumeBtn);
        SetPrivateField(pauseUI, "restartButton", restartBtn);
        SetPrivateField(pauseUI, "mainMenuButton", menuBtn);
        SetPrivateField(pauseUI, "pauseTitleText", pauseTitle);

        panel.SetActive(false);
    }

    // ========================================================================
    // UI Helper Methods
    // ========================================================================

    private Text CreateUIText(Transform parent, string name, string content, Vector2 position, Vector2 size, TextAnchor alignment, int fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
    }

    private Slider CreateUISlider(Transform parent, string name, Vector2 position, Vector2 size, Color fillColor, Color bgColor)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);

        RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0f, 0f);
        sliderRT.anchorMax = new Vector2(0f, 0f);
        sliderRT.pivot = new Vector2(0f, 0f);
        sliderRT.anchoredPosition = position;
        sliderRT.sizeDelta = size;

        // Background.
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRT = bgObj.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = bgColor;

        // Fill area.
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        // Fill.
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fillObj.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = fillColor;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRT;
        slider.targetGraphic = fillImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.interactable = false;

        return slider;
    }

    private Button CreateUIButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
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
        img.color = new Color(0.15f, 0.15f, 0.3f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.3f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.5f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.2f);
        btn.colors = colors;

        // Button label text.
        Text text = CreateUIText(btnObj.transform, "Label", label,
            Vector2.zero, size, TextAnchor.MiddleCenter, 24, Color.white);
        RectTransform textRT = text.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        textRT.anchoredPosition = Vector2.zero;

        return btn;
    }

    private void CenterRectTransform(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    // ========================================================================
    // Reflection Helper
    // ========================================================================

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.FlattenHierarchy);

        if (field == null)
        {
            // Try base types explicitly.
            var type = target.GetType();
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                type = type.BaseType;
            }
        }

        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"[GameSetup] Could not find field '{fieldName}' on {target.GetType().Name}");
        }
    }
}
