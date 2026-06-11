using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// OPTIONAL convenience component. Place this on a single empty GameObject in an
/// otherwise empty scene and press Play — it builds the entire game at runtime:
/// camera, player ship, bullet & enemy prefabs, the GameManager / SpawnManager,
/// the screen boundary, and a full uGUI canvas (menu, HUD, game-over).
///
/// This lets you run a fully playable game WITHOUT manually wiring prefabs and
/// the scene. Everything uses generated solid-colour sprites so no art assets
/// are required. You can still build proper prefabs later for a polished game;
/// see the README for the manual setup path.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Auto Build")]
    [Tooltip("If true, builds the whole game on Awake.")]
    public bool buildOnAwake = true;

    private GameObject bulletPrefab;
    private GameObject enemyPrefab;

    private void Awake()
    {
        if (buildOnAwake)
        {
            BuildEverything();
        }
    }

    private void BuildEverything()
    {
        SetupCamera();
        CreateBoundary();
        bulletPrefab = CreateBulletPrefab();
        enemyPrefab = CreateEnemyPrefab(bulletPrefab);

        GameObject player = CreatePlayer(bulletPrefab);
        SpawnManager spawnManager = CreateSpawnManager(enemyPrefab);
        GameManager gameManager = CreateGameManager();
        CreateUI(player.GetComponent<Health>());
    }

    // ---------------------------------------------------------------- Camera

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.04f, 0.1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    // -------------------------------------------------------------- Sprites

    /// <summary>
    /// Creates a simple solid-colour square sprite of the given pixel size.
    /// </summary>
    private Sprite CreateSquareSprite(Color color, int size = 32)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        tex.SetPixels(pixels);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Creates a triangle-ish ship sprite (just a tinted square here for simplicity).
    /// </summary>
    private Sprite CreateShipSprite(Color color, int size = 48)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Draw a simple upward-pointing triangle.
                float nx = Mathf.Abs((x - size / 2f) / (size / 2f));
                float ny = y / (float)size;
                tex.SetPixel(x, y, nx <= ny ? color : clear);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // -------------------------------------------------------------- Boundary

    private void CreateBoundary()
    {
        // A large trigger ring slightly outside the screen to despawn bullets.
        GameObject boundary = new GameObject("Boundary");
        boundary.tag = "Boundary";

        Camera cam = Camera.main;
        float height = cam.orthographicSize * 2f + 4f;
        float width = height * cam.aspect + 4f;

        // Use an edge/box collider set as trigger; bullets cross it and despawn.
        EdgeCollider2D edge = boundary.AddComponent<EdgeCollider2D>();
        edge.isTrigger = true;
        float hw = width / 2f;
        float hh = height / 2f;
        edge.points = new Vector2[]
        {
            new Vector2(-hw, -hh),
            new Vector2(hw, -hh),
            new Vector2(hw, hh),
            new Vector2(-hw, hh),
            new Vector2(-hw, -hh)
        };
    }

    // -------------------------------------------------------------- Prefabs

    private GameObject CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("BulletPrefab");

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite(new Color(1f, 0.9f, 0.3f), 8);
        bullet.transform.localScale = new Vector3(0.15f, 0.5f, 1f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        bullet.AddComponent<BulletController>();

        bullet.SetActive(false); // template only
        return bullet;
    }

    private GameObject CreateEnemyPrefab(GameObject bullet)
    {
        GameObject enemy = new GameObject("EnemyPrefab");
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite(new Color(0.9f, 0.3f, 0.3f), 32);
        enemy.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Health health = enemy.AddComponent<Health>();
        health.maxHealth = 50;
        health.isPlayer = false;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.bulletPrefab = bullet;
        ec.moveSpeed = 3f;
        ec.contactDamage = 25;

        enemy.SetActive(false); // template only
        return enemy;
    }

    // -------------------------------------------------------------- Player

    private GameObject CreatePlayer(GameObject bullet)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, -4f, 0f);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateShipSprite(new Color(0.3f, 0.8f, 1f), 48);
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Health health = player.AddComponent<Health>();
        health.maxHealth = 100;
        health.isPlayer = true;
        health.invulnerabilityTime = 0.5f;

        // Muzzle at the top of the ship.
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(player.transform);
        muzzle.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.bulletPrefab = bullet;
        pc.muzzle = muzzle.transform;
        pc.moveSpeed = 8f;
        pc.fireRate = 0.2f;

        return player;
    }

    // -------------------------------------------------------------- Managers

    private SpawnManager CreateSpawnManager(GameObject enemy)
    {
        GameObject obj = new GameObject("SpawnManager");
        SpawnManager sm = obj.AddComponent<SpawnManager>();
        sm.enemyPrefabs = new GameObject[] { enemy };
        sm.spawnY = 7f;
        sm.spawnHalfWidth = 7f;
        return sm;
    }

    private GameManager CreateGameManager()
    {
        GameObject obj = new GameObject("GameManager");
        return obj.AddComponent<GameManager>();
    }

    // -------------------------------------------------------------- UI

    private void CreateUI(Health playerHealth)
    {
        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 720);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem (needed for buttons)
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            // Fallback for older Unity versions.
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // ---- HUD ----
        GameObject hud = CreatePanel(canvasObj.transform, "HUD", new Color(0, 0, 0, 0));
        Text scoreText = CreateText(hud.transform, "ScoreText", "Score: 0", font, 28,
            TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -20), new Vector2(400, 40));
        Text waveText = CreateText(hud.transform, "WaveText", "Wave: 0", font, 28,
            TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-20, -20), new Vector2(400, 40));
        waveText.alignment = TextAnchor.UpperRight;

        // Health bar (slider)
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(hud.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(0, 0);
        sliderRect.pivot = new Vector2(0, 0);
        sliderRect.anchoredPosition = new Vector2(20, 20);
        sliderRect.sizeDelta = new Vector2(300, 24);

        GameObject bg = CreateUIImage(sliderObj.transform, "Background", new Color(0.3f, 0.1f, 0.1f));
        StretchFull(bg.GetComponent<RectTransform>());

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        StretchFull(fillAreaRect);

        GameObject fill = CreateUIImage(fillArea.transform, "Fill", new Color(0.2f, 0.9f, 0.3f));
        StretchFull(fill.GetComponent<RectTransform>());

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.targetGraphic = fill.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;

        Text healthText = CreateText(hud.transform, "HealthText", "100 / 100", font, 18,
            TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(20, 50), new Vector2(300, 30));

        // ---- Menu Panel ----
        GameObject menu = CreatePanel(canvasObj.transform, "MenuPanel", new Color(0, 0, 0, 0.75f));
        CreateText(menu.transform, "Title", "SPACE SHOOTER", font, 56, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(800, 80));
        CreateText(menu.transform, "Hint", "WASD / Arrows to move  •  Space to shoot", font, 24,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(800, 40));
        Button startBtn = CreateButton(menu.transform, "StartButton", "START (Enter)", font,
            new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(260, 60));

        // ---- Game Over Panel ----
        GameObject over = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0, 0, 0, 0.8f));
        CreateText(over.transform, "GameOverTitle", "GAME OVER", font, 56, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, 140), new Vector2(800, 80));
        Text finalScore = CreateText(over.transform, "FinalScore", "Final Score: 0", font, 30,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(800, 40));
        Text highScore = CreateText(over.transform, "HighScore", "High Score: 0", font, 26,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(800, 40));
        Button restartBtn = CreateButton(over.transform, "RestartButton", "RESTART (Enter)", font,
            new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(280, 60));

        // ---- UIManager wiring ----
        UIManager ui = canvasObj.AddComponent<UIManager>();
        ui.scoreText = scoreText;
        ui.waveText = waveText;
        ui.healthSlider = slider;
        ui.healthText = healthText;
        ui.menuPanel = menu;
        ui.hudPanel = hud;
        ui.gameOverPanel = over;
        ui.finalScoreText = finalScore;
        ui.highScoreText = highScore;
        ui.playerHealth = playerHealth;

        startBtn.onClick.AddListener(ui.OnStartButton);
        restartBtn.onClick.AddListener(ui.OnRestartButton);
    }

    // -------------------------------------------------------- UI helpers

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        RectTransform rect = panel.GetComponent<RectTransform>();
        StretchFull(rect);
        return panel;
    }

    private GameObject CreateUIImage(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    private Text CreateText(Transform parent, string name, string content, Font font, int size,
        TextAnchor anchor, Vector2 anchorPivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = anchor;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorPivot;
        rect.anchorMax = anchorPivot;
        rect.pivot = anchorPivot;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        return text;
    }

    private Button CreateButton(Transform parent, string name, string label, Font font,
        Vector2 anchorPivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.9f);
        Button button = obj.AddComponent<Button>();

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorPivot;
        rect.anchorMax = anchorPivot;
        rect.pivot = anchorPivot;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        Text text = CreateText(obj.transform, "Label", label, font, 26, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, sizeDelta);
        StretchFull(text.GetComponent<RectTransform>());
        return button;
    }

    private void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
