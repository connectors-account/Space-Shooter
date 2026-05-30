using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GamePlaySceneSetup – Programmatically creates ALL GameObjects for the gameplay scene
/// at runtime. This eliminates the need for manual Unity Editor setup.
///
/// Attach this script to a single empty GameObject in the GamePlay scene, or place it
/// as part of a bootstrap prefab. It creates:
///   - Camera (orthographic, dark background)
///   - Player ship (green triangle)
///   - Bullet prefab (yellow rectangle)
///   - Enemy prefab (red diamond)
///   - Power-up prefabs (rapid fire = yellow star, shield = cyan star)
///   - Enemy spawner
///   - Power-up spawner
///   - GameManager
///   - HUD Canvas (score, health, game-over panel)
///   - Starfield background
/// </summary>
public class GamePlaySceneSetup : MonoBehaviour
{
    void Awake()
    {
        // ── 1. Camera Setup ──
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f); // deep space blue-black
        cam.transform.position = new Vector3(0, 0, -10);

        // ── 2. Create Prefab Templates (not active in scene, used for instantiation) ──
        GameObject bulletPrefab = CreateBulletPrefab();
        GameObject enemyPrefab = CreateEnemyPrefab();
        GameObject rapidFirePrefab = CreatePowerUpPrefab(PowerUp.PowerUpType.RapidFire);
        GameObject shieldPrefab = CreatePowerUpPrefab(PowerUp.PowerUpType.Shield);

        // ── 3. Player ──
        GameObject player = CreatePlayer(bulletPrefab);

        // ── 4. GameManager ──
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // ── 5. Enemy Spawner ──
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
        spawner.enemyPrefab = enemyPrefab;

        // ── 6. Power-Up Spawner ──
        GameObject puSpawnerObj = new GameObject("PowerUpSpawner");
        PowerUpSpawner puSpawner = puSpawnerObj.AddComponent<PowerUpSpawner>();
        puSpawner.rapidFirePrefab = rapidFirePrefab;
        puSpawner.shieldPrefab = shieldPrefab;

        // ── 7. Starfield Background ──
        GameObject starfield = new GameObject("Starfield");
        starfield.AddComponent<StarfieldBackground>();

        // ── 8. HUD Canvas ──
        CreateHUD();
    }

    // ──────────────────────────────────────────────────────────────
    //  PLAYER
    // ──────────────────────────────────────────────────────────────
    GameObject CreatePlayer(GameObject bulletPrefab)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -4f, 0);

        // Visual: green triangle
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite(32);
        sr.color = new Color(0.2f, 1f, 0.3f); // bright green
        sr.sortingOrder = 2;
        player.transform.localScale = Vector3.one * 0.8f;

        // Physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        // Controller
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.bulletPrefab = bulletPrefab;

        return player;
    }

    // ──────────────────────────────────────────────────────────────
    //  BULLET PREFAB
    // ──────────────────────────────────────────────────────────────
    GameObject CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("BulletPrefab");

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite(4, 12);
        sr.color = Color.yellow;
        sr.sortingOrder = 1;
        bullet.transform.localScale = Vector3.one * 0.3f;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 1.2f);

        bullet.AddComponent<Bullet>();

        // Hide the "prefab" – it's only used as a template
        bullet.SetActive(false);

        return bullet;
    }

    // ──────────────────────────────────────────────────────────────
    //  ENEMY PREFAB
    // ──────────────────────────────────────────────────────────────
    GameObject CreateEnemyPrefab()
    {
        GameObject enemy = new GameObject("EnemyPrefab");
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreateDiamondSprite(32);
        sr.color = new Color(1f, 0.25f, 0.2f); // red-orange
        sr.sortingOrder = 2;
        enemy.transform.localScale = Vector3.one * 0.7f;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        enemy.AddComponent<Enemy>();

        enemy.SetActive(false);
        return enemy;
    }

    // ──────────────────────────────────────────────────────────────
    //  POWER-UP PREFAB
    // ──────────────────────────────────────────────────────────────
    GameObject CreatePowerUpPrefab(PowerUp.PowerUpType type)
    {
        string name = type == PowerUp.PowerUpType.RapidFire ? "RapidFirePrefab" : "ShieldPrefab";
        GameObject pu = new GameObject(name);
        pu.tag = "PowerUp";

        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = CreateStarSprite(32);
        sr.sortingOrder = 3;
        pu.transform.localScale = Vector3.one * 0.5f;

        if (type == PowerUp.PowerUpType.RapidFire)
            sr.color = new Color(1f, 0.9f, 0.2f); // gold-yellow
        else
            sr.color = new Color(0.3f, 0.9f, 1f);  // cyan

        Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        PowerUp puScript = pu.AddComponent<PowerUp>();
        puScript.type = type;

        pu.SetActive(false);
        return pu;
    }

    // ──────────────────────────────────────────────────────────────
    //  HUD CANVAS
    // ──────────────────────────────────────────────────────────────
    void CreateHUD()
    {
        // -- Canvas --
        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        canvasObj.AddComponent<GraphicRaycaster>();

        // -- HUDManager component --
        HUDManager hud = canvasObj.AddComponent<HUDManager>();

        // -- Score Text (top-left) --
        GameObject scoreObj = CreateUIText(canvasObj.transform, "ScoreText",
            "SCORE: 0", TextAnchor.UpperLeft,
            new Vector2(10, -10), new Vector2(300, 40));
        hud.scoreText = scoreObj.GetComponent<Text>();

        // -- Health Text (top-right) --
        GameObject healthObj = CreateUIText(canvasObj.transform, "HealthText",
            "LIVES: \u2665 \u2665 \u2665", TextAnchor.UpperRight,
            new Vector2(-10, -10), new Vector2(300, 40));
        RectTransform hrt = healthObj.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(1, 1);
        hrt.anchorMax = new Vector2(1, 1);
        hrt.pivot = new Vector2(1, 1);
        hud.healthText = healthObj.GetComponent<Text>();

        // -- Game Over Panel (center, hidden by default) --
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform goPanelRT = gameOverPanel.AddComponent<RectTransform>();
        goPanelRT.anchorMin = Vector2.zero;
        goPanelRT.anchorMax = Vector2.one;
        goPanelRT.offsetMin = Vector2.zero;
        goPanelRT.offsetMax = Vector2.zero;

        // Semi-transparent dark overlay
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.75f);

        // "GAME OVER" title
        CreateUIText(gameOverPanel.transform, "GameOverTitle",
            "GAME OVER", TextAnchor.MiddleCenter,
            new Vector2(0, 80), new Vector2(400, 60), 42, Color.red,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // Final score
        GameObject finalScoreObj = CreateUIText(gameOverPanel.transform, "FinalScoreText",
            "FINAL SCORE: 0", TextAnchor.MiddleCenter,
            new Vector2(0, 20), new Vector2(400, 40), 28, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        hud.finalScoreText = finalScoreObj.GetComponent<Text>();

        // Restart button
        GameObject restartBtn = CreateUIButton(gameOverPanel.transform, "RestartButton",
            "PLAY AGAIN", new Vector2(-90, -50), new Vector2(160, 45));
        hud.restartButton = restartBtn.GetComponent<Button>();

        // Menu button
        GameObject menuBtn = CreateUIButton(gameOverPanel.transform, "MenuButton",
            "MAIN MENU", new Vector2(90, -50), new Vector2(160, 45));
        hud.menuButton = menuBtn.GetComponent<Button>();

        hud.gameOverPanel = gameOverPanel;
        gameOverPanel.SetActive(false);

        // -- EventSystem (required for UI interaction) --
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ──────────────────────────────────────────────────────────────
    //  UI HELPERS
    // ──────────────────────────────────────────────────────────────

    GameObject CreateUIText(Transform parent, string name, string content,
        TextAnchor alignment, Vector2 position, Vector2 size,
        int fontSize = 24, Color? color = null,
        Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? pivot = null)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin ?? new Vector2(0, 1);
        rt.anchorMax = anchorMax ?? new Vector2(0, 1);
        rt.pivot = pivot ?? new Vector2(0, 1);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        // Fallback font if LegacyRuntime not available
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = color ?? Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return obj;
    }

    GameObject CreateUIButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size)
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
        img.color = new Color(0.2f, 0.2f, 0.3f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.5f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.25f);
        btn.colors = colors;

        // Button label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        Text text = labelObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        return btnObj;
    }

    // ──────────────────────────────────────────────────────────────
    //  SPRITE GENERATION (simple geometric shapes)
    // ──────────────────────────────────────────────────────────────

    /// <summary>Creates an upward-pointing triangle sprite (player ship).</summary>
    static Sprite CreateTriangleSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = Color.clear;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Fill triangle: apex at top-center, base at bottom
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size; // 0 at bottom, 1 at top
            float halfWidth = (1f - progress) * (size / 2f);
            int center = size / 2;
            for (int x = (int)(center - halfWidth); x <= (int)(center + halfWidth); x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, Color.white);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Creates a rectangle sprite (bullet).</summary>
    static Sprite CreateRectSprite(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), Mathf.Max(width, height));
    }

    /// <summary>Creates a diamond sprite (enemy).</summary>
    static Sprite CreateDiamondSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = Color.clear;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        int half = size / 2;
        for (int y = 0; y < size; y++)
        {
            int distFromCenter = Mathf.Abs(y - half);
            int halfWidth = half - distFromCenter;
            for (int x = half - halfWidth; x <= half + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, Color.white);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Creates a simple 5-pointed star sprite (power-ups).</summary>
    static Sprite CreateStarSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = Color.clear;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        float center = size / 2f;
        float outerRadius = size / 2f - 1;
        float innerRadius = outerRadius * 0.4f;

        // 5-pointed star vertices
        Vector2[] points = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float angle = Mathf.PI / 2f + i * Mathf.PI / 5f;
            float r = (i % 2 == 0) ? outerRadius : innerRadius;
            points[i] = new Vector2(
                center + r * Mathf.Cos(angle),
                center + r * Mathf.Sin(angle)
            );
        }

        // Fill using point-in-polygon for each pixel
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (IsPointInPolygon(new Vector2(x, y), points))
                    tex.SetPixel(x, y, Color.white);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Ray-casting point-in-polygon test.</summary>
    static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) *
                (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
            {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }
}
