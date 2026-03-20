using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically sets up the game scene at runtime if prefabs/objects are missing.
/// This is a bootstrap script that creates all necessary GameObjects programmatically.
/// Attach to an empty "_Bootstrap" GameObject in your scene.
/// </summary>
public class AutoSceneSetup : MonoBehaviour
{
    [Header("Set to true to auto-generate the scene on Start")]
    [SerializeField] private bool autoSetup = true;

    private void Start()
    {
        if (!autoSetup) return;

        // Ensure RuntimeSpriteGenerator exists
        if (RuntimeSpriteGenerator.Instance == null)
        {
            GameObject sprGen = new GameObject("RuntimeSpriteGenerator");
            sprGen.AddComponent<RuntimeSpriteGenerator>();
        }

        // Wait one frame for sprites to generate, then build scene
        StartCoroutine(SetupAfterFrame());
    }

    private System.Collections.IEnumerator SetupAfterFrame()
    {
        yield return null; // Wait one frame

        var sg = RuntimeSpriteGenerator.Instance;
        if (sg == null) yield break;

        // ---- Camera ----
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5.5f;
        Camera.main.backgroundColor = new Color(0.01f, 0.01f, 0.06f);

        // ---- Background ----
        CreateBackground(sg);

        // ---- Bullet Prefabs (create as inactive templates) ----
        GameObject playerBulletPrefab = CreateBulletTemplate("PlayerBulletTemplate", sg.PlayerBulletSprite, "PlayerBullet");
        GameObject enemyBulletPrefab = CreateBulletTemplate("EnemyBulletTemplate", sg.EnemyBulletSprite, "EnemyBullet");

        // ---- Power-Up Prefabs ----
        GameObject[] powerUpPrefabs = CreatePowerUpTemplates(sg);

        // ---- Player ----
        GameObject player = CreatePlayer(sg, playerBulletPrefab);

        // ---- Enemy Prefabs ----
        GameObject basicEnemy = CreateEnemyTemplate("BasicEnemyTemplate", sg.EnemyBasicSprite, "Enemy");
        GameObject fastEnemy = CreateEnemyTemplate("FastEnemyTemplate", sg.EnemyFastSprite, "Enemy");
        GameObject tankEnemy = CreateEnemyTemplate("TankEnemyTemplate", sg.EnemyTankSprite, "Enemy");
        GameObject shooterEnemy = CreateEnemyTemplate("ShooterEnemyTemplate", sg.EnemyShooterSprite, "Enemy");

        // ---- Audio Manager ----
        if (AudioManager.Instance == null)
        {
            GameObject audioMgr = new GameObject("AudioManager");
            audioMgr.AddComponent<AudioManager>();
        }

        // ---- Enemy Spawner ----
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();

        // Use reflection or serialized fields workaround to set prefab refs
        SetPrivateField(spawner, "basicEnemyPrefab", basicEnemy);
        SetPrivateField(spawner, "fastEnemyPrefab", fastEnemy);
        SetPrivateField(spawner, "tankEnemyPrefab", tankEnemy);
        SetPrivateField(spawner, "shooterEnemyPrefab", shooterEnemy);
        SetPrivateField(spawner, "enemyBulletPrefab", enemyBulletPrefab);
        SetPrivateField(spawner, "powerUpPrefabs", powerUpPrefabs);

        // ---- UI Canvas ----
        UIManager uiManager = CreateUICanvas();

        // ---- Game Manager ----
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gm = gmObj.AddComponent<GameManager>();
        }
        SetPrivateField(gm, "enemySpawner", spawner);
        SetPrivateField(gm, "uiManager", uiManager);
        SetPrivateField(gm, "player", player.GetComponent<PlayerController>());

        // Start the game
        gm.StartGame();
    }

    // ---- Helper Methods ----

    private void CreateBackground(RuntimeSpriteGenerator sg)
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject bg = new GameObject("Background_" + i);
            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = sg.BackgroundSprite;
            sr.sortingOrder = -100;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(12f, 12f);
            bg.transform.position = new Vector3(0, i * 12f, 10f);

            ParallaxBackground pb = bg.AddComponent<ParallaxBackground>();
            SetPrivateField(pb, "scrollSpeed", 1f + i * 0.2f);
            SetPrivateField(pb, "resetPositionY", -12f);
            SetPrivateField(pb, "startPositionY", 12f);
        }

        // Add some stars
        for (int i = 0; i < 30; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = sg.StarSprite;
            sr.sortingOrder = -90;
            float alpha = Random.Range(0.3f, 1f);
            sr.color = new Color(1, 1, 1, alpha);
            star.transform.position = new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(-6f, 6f),
                5f
            );
            star.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);

            ParallaxBackground pb = star.AddComponent<ParallaxBackground>();
            float speed = Random.Range(0.5f, 3f);
            SetPrivateField(pb, "scrollSpeed", speed);
            SetPrivateField(pb, "resetPositionY", -7f);
            SetPrivateField(pb, "startPositionY", 7f);
        }
    }

    private GameObject CreatePlayer(RuntimeSpriteGenerator sg, GameObject bulletPrefab)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3.5f, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = sg.PlayerShipSprite;
        sr.sortingOrder = 10;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        PlayerController pc = player.AddComponent<PlayerController>();
        SetPrivateField(pc, "bulletPrefab", bulletPrefab);
        SetPrivateField(pc, "spriteRenderer", sr);

        return player;
    }

    private GameObject CreateBulletTemplate(string name, Sprite sprite, string tag)
    {
        GameObject bullet = new GameObject(name);
        bullet.tag = tag;

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.15f, 0.3f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        bullet.AddComponent<BulletController>();

        bullet.SetActive(false);
        // Move off-screen as a template
        bullet.transform.position = new Vector3(-100, -100, 0);

        return bullet;
    }

    private GameObject CreateEnemyTemplate(string name, Sprite sprite, string tag)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = tag;

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 8;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        enemy.AddComponent<EnemyController>();

        enemy.SetActive(false);
        enemy.transform.position = new Vector3(-100, -100, 0);

        return enemy;
    }

    private GameObject[] CreatePowerUpTemplates(RuntimeSpriteGenerator sg)
    {
        GameObject[] powerUps = new GameObject[3];

        powerUps[0] = CreatePowerUpTemplate("PowerUpWeapon", sg.PowerUpWeaponSprite, PowerUpController.PowerUpType.WeaponUpgrade);
        powerUps[1] = CreatePowerUpTemplate("PowerUpShield", sg.PowerUpShieldSprite, PowerUpController.PowerUpType.Shield);
        powerUps[2] = CreatePowerUpTemplate("PowerUpHealth", sg.PowerUpHealthSprite, PowerUpController.PowerUpType.Health);

        return powerUps;
    }

    private GameObject CreatePowerUpTemplate(string name, Sprite sprite, PowerUpController.PowerUpType type)
    {
        GameObject pu = new GameObject(name);
        pu.tag = "PowerUp";

        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 6;

        CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        PowerUpController pc = pu.AddComponent<PowerUpController>();
        SetPrivateField(pc, "type", type);

        pu.SetActive(false);
        pu.transform.position = new Vector3(-100, -100, 0);

        return pu;
    }

    private UIManager CreateUICanvas()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Event System
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // --- HUD Panel ---
        GameObject hudPanel = CreatePanel(canvasObj.transform, "HUDPanel", new Color(0, 0, 0, 0));

        Text scoreText = CreateText(hudPanel.transform, "ScoreText", "Score: 0",
            TextAnchor.UpperLeft, new Vector2(20, -20), 28);

        Text waveText = CreateText(hudPanel.transform, "WaveText", "Wave: 0",
            TextAnchor.UpperRight, new Vector2(-20, -20), 28);

        Text healthText = CreateText(hudPanel.transform, "HealthText", "HP: 5/5",
            TextAnchor.LowerLeft, new Vector2(20, 20), 24);

        // Health Slider
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(hudPanel.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(0.3f, 0);
        sliderRect.anchoredPosition = new Vector2(0, 60);
        sliderRect.sizeDelta = new Vector2(0, 20);
        Slider healthSlider = sliderObj.AddComponent<Slider>();
        healthSlider.minValue = 0;
        healthSlider.maxValue = 5;
        healthSlider.value = 5;

        // Slider background
        GameObject sliderBg = new GameObject("Background");
        sliderBg.transform.SetParent(sliderObj.transform, false);
        Image bgImg = sliderBg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = sliderBg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Slider fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.9f, 0.3f, 0.9f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        healthSlider.fillRect = fillRect;

        // Wave announcement text (centered, large)
        Text waveAnnounce = CreateText(hudPanel.transform, "WaveAnnouncement", "",
            TextAnchor.MiddleCenter, Vector2.zero, 48);
        waveAnnounce.color = Color.yellow;
        waveAnnounce.gameObject.SetActive(false);

        // --- Pause Menu Panel ---
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PauseMenuPanel", new Color(0, 0, 0, 0.7f));
        CreateText(pausePanel.transform, "PauseTitle", "PAUSED",
            TextAnchor.MiddleCenter, new Vector2(0, 120), 48);
        CreateButton(pausePanel.transform, "ResumeBtn", "Resume", new Vector2(0, 40));
        CreateButton(pausePanel.transform, "RestartBtn", "Restart", new Vector2(0, -30));
        CreateButton(pausePanel.transform, "MenuBtn", "Main Menu", new Vector2(0, -100));
        CreateButton(pausePanel.transform, "QuitBtn", "Quit", new Vector2(0, -170));
        PauseMenuController pmc = pausePanel.AddComponent<PauseMenuController>();
        // Wire buttons
        SetPrivateField(pmc, "resumeButton", pausePanel.transform.Find("ResumeBtn").GetComponent<Button>());
        SetPrivateField(pmc, "restartButton", pausePanel.transform.Find("RestartBtn").GetComponent<Button>());
        SetPrivateField(pmc, "mainMenuButton", pausePanel.transform.Find("MenuBtn").GetComponent<Button>());
        SetPrivateField(pmc, "quitButton", pausePanel.transform.Find("QuitBtn").GetComponent<Button>());
        pausePanel.SetActive(false);

        // --- Game Over Panel ---
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0, 0, 0, 0.8f));
        Text goTitle = CreateText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            TextAnchor.MiddleCenter, new Vector2(0, 150), 56);
        goTitle.color = Color.red;
        Text finalScoreText = CreateText(gameOverPanel.transform, "FinalScore", "Final Score: 0",
            TextAnchor.MiddleCenter, new Vector2(0, 70), 32);
        Text highScoreTextGO = CreateText(gameOverPanel.transform, "HighScore", "High Score: 0",
            TextAnchor.MiddleCenter, new Vector2(0, 20), 28);
        CreateButton(gameOverPanel.transform, "RestartBtn2", "Play Again", new Vector2(0, -60));
        CreateButton(gameOverPanel.transform, "MenuBtn2", "Main Menu", new Vector2(0, -130));
        CreateButton(gameOverPanel.transform, "QuitBtn2", "Quit", new Vector2(0, -200));
        GameOverController goc = gameOverPanel.AddComponent<GameOverController>();
        SetPrivateField(goc, "restartButton", gameOverPanel.transform.Find("RestartBtn2").GetComponent<Button>());
        SetPrivateField(goc, "mainMenuButton", gameOverPanel.transform.Find("MenuBtn2").GetComponent<Button>());
        SetPrivateField(goc, "quitButton", gameOverPanel.transform.Find("QuitBtn2").GetComponent<Button>());
        SetPrivateField(goc, "gameOverTitle", goTitle);
        gameOverPanel.SetActive(false);

        // Wire UIManager references
        SetPrivateField(uiManager, "hudPanel", hudPanel);
        SetPrivateField(uiManager, "scoreText", scoreText);
        SetPrivateField(uiManager, "waveText", waveText);
        SetPrivateField(uiManager, "healthText", healthText);
        SetPrivateField(uiManager, "healthSlider", healthSlider);
        SetPrivateField(uiManager, "pauseMenuPanel", pausePanel);
        SetPrivateField(uiManager, "gameOverPanel", gameOverPanel);
        SetPrivateField(uiManager, "finalScoreText", finalScoreText);
        SetPrivateField(uiManager, "highScoreText", highScoreTextGO);
        SetPrivateField(uiManager, "waveAnnouncementText", waveAnnounce);

        return uiManager;
    }

    // ---- UI Creation Helpers ----

    private GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        if (bgColor.a <= 0.01f) img.raycastTarget = false;
        return panel;
    }

    private Text CreateText(Transform parent, string name, string content,
        TextAnchor anchor, Vector2 position, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();

        // Set anchoring based on alignment
        switch (anchor)
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
            case TextAnchor.LowerLeft:
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                break;
            default:
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;
        }

        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(400, 60);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return text;
    }

    private void CreateButton(Transform parent, string name, string label, Vector2 position)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(250, 50);
        rt.anchoredPosition = position;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.3f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.6f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.2f, 1f);
        btn.colors = colors;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }

    /// <summary>
    /// Sets a private/serialized field via reflection.
    /// </summary>
    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"Could not find field '{fieldName}' on {target.GetType().Name}");
        }
    }
}
