using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the entire GamePlay scene at runtime: player, spawners, HUD, background.
/// Attach to an empty GameObject in the GamePlay scene.
/// </summary>
public class SceneSetup_GamePlay : MonoBehaviour
{
    void Start()
    {
        EnsureManagers();

        Camera.main.backgroundColor = new Color(0.01f, 0.01f, 0.06f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5.5f;

        SpriteGenerator sg = SpriteGenerator.Instance;

        // --- Starfield ---
        GameObject stars = new GameObject("Starfield");
        stars.AddComponent<BackgroundStarfield>();

        // --- Player ---
        GameObject player = CreateSprite("Player", sg.PlayerShip, Vector3.down * 3.5f, "Player");
        player.AddComponent<Rigidbody2D>().gravityScale = 0;
        BoxCollider2D pc = player.AddComponent<BoxCollider2D>();
        pc.isTrigger = true;
        pc.size = new Vector2(0.7f, 0.8f);
        PlayerController playerCtrl = player.AddComponent<PlayerController>();

        // --- Bullet Prefabs (create, hide, assign) ---
        GameObject playerBulletPrefab = CreateBulletPrefab("PlayerBulletPrefab", sg.BulletPlayer, true);
        GameObject enemyBulletPrefab  = CreateBulletPrefab("EnemyBulletPrefab",  sg.BulletEnemy,  false);
        playerCtrl.bulletPrefab = playerBulletPrefab;

        // --- Enemy Prefabs ---
        GameObject enemyBasicPrefab   = CreateEnemyPrefab("EnemyBasicPrefab",   sg.EnemyBasic,   Enemy.MovementType.Straight, false, null);
        GameObject enemySinePrefab    = CreateEnemyPrefab("EnemySinePrefab",    sg.EnemySine,    Enemy.MovementType.Sine,     false, null);
        GameObject enemyShooterPrefab = CreateEnemyPrefab("EnemyShooterPrefab", sg.EnemyShooter, Enemy.MovementType.Straight, true,  enemyBulletPrefab);

        // --- Enemy Spawner ---
        GameObject spawnerGO = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        spawner.enemyBasicPrefab   = enemyBasicPrefab;
        spawner.enemySinePrefab    = enemySinePrefab;
        spawner.enemyShooterPrefab = enemyShooterPrefab;

        // --- Power-Up Prefabs & Spawner ---
        GameObject puShield = CreatePowerUpPrefab("PU_Shield", sg.PowerUpShield, PowerUp.PowerUpType.Shield);
        GameObject puRapid  = CreatePowerUpPrefab("PU_Rapid",  sg.PowerUpRapid,  PowerUp.PowerUpType.RapidFire);
        GameObject puHealth = CreatePowerUpPrefab("PU_Health", sg.PowerUpHealth, PowerUp.PowerUpType.Health);

        GameObject puSpawnerGO = new GameObject("PowerUpSpawner");
        PowerUpSpawner puSpawner = puSpawnerGO.AddComponent<PowerUpSpawner>();
        puSpawner.shieldPrefab    = puShield;
        puSpawner.rapidFirePrefab = puRapid;
        puSpawner.healthPrefab    = puHealth;

        // --- HUD Canvas ---
        BuildHUD();

        // --- Wire to GameManager ---
        GameObject setupGO = new GameObject("GamePlaySetup");
        GamePlaySetup setup = setupGO.AddComponent<GamePlaySetup>();
        setup.enemySpawner = spawner;
        setup.player = playerCtrl;
    }

    void EnsureManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject go = new GameObject("GameManager"); go.AddComponent<GameManager>();
        }
        if (AudioManager.Instance == null)
        {
            GameObject go = new GameObject("AudioManager"); go.AddComponent<AudioManager>();
        }
        if (SpriteGenerator.Instance == null)
        {
            GameObject go = new GameObject("SpriteGenerator"); go.AddComponent<SpriteGenerator>();
        }
    }

    GameObject CreateSprite(string name, Sprite sprite, Vector3 pos, string tag)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        go.transform.position = pos;
        go.tag = tag;
        return go;
    }

    GameObject CreateBulletPrefab(string name, Sprite sprite, bool isPlayer)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.4f);
        Bullet b = go.AddComponent<Bullet>();
        b.isPlayerBullet = isPlayer;
        go.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";
        go.SetActive(false); // it's a template; Instantiate will activate copies
        return go;
    }

    GameObject CreateEnemyPrefab(string name, Sprite sprite,
        Enemy.MovementType moveType, bool canShoot, GameObject bulletPrefab)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;
        sr.flipY = true; // enemies face downward
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.7f);
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        Enemy e = go.AddComponent<Enemy>();
        e.movementType = moveType;
        e.canShoot = canShoot;
        e.bulletPrefab = bulletPrefab;
        go.tag = "Enemy";
        go.SetActive(false);
        return go;
    }

    GameObject CreatePowerUpPrefab(string name, Sprite sprite, PowerUp.PowerUpType type)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;
        PowerUp pu = go.AddComponent<PowerUp>();
        pu.type = type;
        go.tag = "PowerUp";
        go.SetActive(false);
        return go;
    }

    void BuildHUD()
    {
        // Canvas
        GameObject canvasGO = new GameObject("HUDCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(800, 600);
        canvasGO.AddComponent<GraphicRaycaster>();

        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        UIManager ui = canvasGO.AddComponent<UIManager>();

        // Score text (top-left)
        ui.scoreText = CreateHUDText(canvasGO.transform, "ScoreText", "Score: 0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(250, 40),
            TextAnchor.UpperLeft, 22, Color.white);

        // Health text (top-right)
        ui.healthText = CreateHUDText(canvasGO.transform, "HealthText", "HP: 5 / 5",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(250, 40),
            TextAnchor.UpperRight, 22, Color.green);

        // Wave banner (center)
        ui.waveBannerText = CreateHUDText(canvasGO.transform, "WaveBanner", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(400, 60),
            TextAnchor.MiddleCenter, 36, Color.yellow);
        ui.waveBannerText.fontStyle = FontStyle.Bold;

        // Pause panel
        ui.pausePanel = BuildPausePanel(canvasGO.transform, ui);
    }

    Text CreateHUDText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        TextAnchor alignment, int fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;

        return text;
    }

    GameObject BuildPausePanel(Transform parent, UIManager ui)
    {
        GameObject panel = new GameObject("PausePanel");
        panel.transform.SetParent(parent, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        // "PAUSED" label
        CreateHUDText(panel.transform, "PausedLabel", "PAUSED",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(300, 60),
            TextAnchor.MiddleCenter, 42, Color.white).fontStyle = FontStyle.Bold;

        // Resume button
        CreateUIButton(panel.transform, "ResumeBtn", "Resume",
            new Vector2(0, 0), new Vector2(200, 50), () => ui.OnResumeButton());

        // Main Menu button
        CreateUIButton(panel.transform, "MenuBtn", "Main Menu",
            new Vector2(0, -70), new Vector2(200, 50), () => ui.OnMainMenuButton());

        panel.SetActive(false);
        return panel;
    }

    void CreateUIButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.3f, 0.6f, 0.95f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text text = textGO.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }
}
