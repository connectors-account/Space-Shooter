#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor wizard that automatically builds the entire game scene
/// with one click: player, camera, spawner, UI canvas, prefabs, etc.
/// Access via menu: Tools > Space Shooter > Setup Scene.
/// </summary>
public class SceneSetupWizard
{
    [MenuItem("Tools/Space Shooter/Setup Entire Scene")]
    public static void SetupScene()
    {
        // ============================================================
        // 1. CAMERA
        // ============================================================
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
        }
        mainCam.transform.position = new Vector3(0, 0, -10);
        mainCam.orthographic = true;
        mainCam.orthographicSize = 5.5f;
        mainCam.backgroundColor = new Color(0.02f, 0.02f, 0.08f); // dark space
        mainCam.clearFlags = CameraClearFlags.SolidColor;

        // ============================================================
        // 2. LIGHTING
        // ============================================================
        // Remove default directional light or add one
        Light existingLight = Object.FindObjectOfType<Light>();
        if (existingLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        // ============================================================
        // 3. MATERIALS
        // ============================================================
        Material playerMat = CreateUnlitMaterial("PlayerMat", new Color(0.2f, 0.8f, 1f));
        Material enemyMat  = CreateUnlitMaterial("EnemyMat", new Color(1f, 0.3f, 0.3f));
        Material bulletMat = CreateUnlitMaterial("BulletMat", Color.yellow);
        Material powerMat  = CreateUnlitMaterial("PowerUpMat", new Color(0.3f, 1f, 0.3f));

        // ============================================================
        // 4. BULLET PREFAB
        // ============================================================
        GameObject bulletPrefab = CreateBulletPrefab(bulletMat);

        // ============================================================
        // 5. ENEMY PREFAB
        // ============================================================
        GameObject enemyPrefab = CreateEnemyPrefab(enemyMat);

        // ============================================================
        // 6. POWER-UP PREFAB
        // ============================================================
        GameObject powerUpPrefab = CreatePowerUpPrefab(powerMat);

        // ============================================================
        // 7. PLAYER
        // ============================================================
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3.5f, 0);
        player.transform.localScale = new Vector3(1f, 0.3f, 0.5f);
        player.GetComponent<Renderer>().sharedMaterial = playerMat;

        // Replace default collider with trigger
        Object.DestroyImmediate(player.GetComponent<Collider>());
        BoxCollider playerCol = player.AddComponent<BoxCollider>();
        playerCol.isTrigger = true;

        // Add Rigidbody (kinematic, no gravity)
        Rigidbody playerRb = player.AddComponent<Rigidbody>();
        playerRb.useGravity = false;
        playerRb.isKinematic = true;

        // Fire point (child)
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.parent = player.transform;
        firePoint.transform.localPosition = new Vector3(0, 1f, 0);

        // Attach scripts
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.bulletPrefab = bulletPrefab;
        pc.firePoint = firePoint.transform;

        player.AddComponent<PlayerHealth>();

        // ============================================================
        // 8. GAME MANAGER
        // ============================================================
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // ============================================================
        // 9. ENEMY SPAWNER
        // ============================================================
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
        spawner.enemyPrefab = enemyPrefab;
        spawner.powerUpPrefab = powerUpPrefab;

        // ============================================================
        // 10. UI CANVAS
        // ============================================================
        SetupUI(player);

        // ============================================================
        // TAGS (reminder)
        // ============================================================
        Debug.Log("============================================");
        Debug.Log("Scene setup complete!");
        Debug.Log("Make sure these tags exist (Edit > Project Settings > Tags & Layers):");
        Debug.Log("  - Player, Enemy, PowerUp");
        Debug.Log("Press Play to start the game!");
        Debug.Log("============================================");
    }

    // -----------------------------------------------------------------
    // Helper: Create a simple unlit-colored material
    // -----------------------------------------------------------------
    static Material CreateUnlitMaterial(string name, Color color)
    {
        // Try to find existing
        string path = "Assets/Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Glossiness", 0.3f);
        // Make it emissive so it glows in dark space
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 0.5f);

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    // -----------------------------------------------------------------
    // Helper: Create bullet prefab
    // -----------------------------------------------------------------
    static GameObject CreateBulletPrefab(Material mat)
    {
        // Ensure Prefabs folder
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        string prefabPath = "Assets/Prefabs/Bullet.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null) return existing;

        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Bullet";
        bullet.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
        bullet.GetComponent<Renderer>().sharedMaterial = mat;

        // Collider as trigger
        Object.DestroyImmediate(bullet.GetComponent<Collider>());
        SphereCollider col = bullet.AddComponent<SphereCollider>();
        col.isTrigger = true;

        Rigidbody rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        bullet.AddComponent<Bullet>();

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bullet, prefabPath);
        Object.DestroyImmediate(bullet);
        return prefab;
    }

    // -----------------------------------------------------------------
    // Helper: Create enemy prefab
    // -----------------------------------------------------------------
    static GameObject CreateEnemyPrefab(Material mat)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        string prefabPath = "Assets/Prefabs/Enemy.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null) return existing;

        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = "Enemy";
        enemy.tag = "Enemy";
        enemy.transform.localScale = new Vector3(0.8f, 0.8f, 0.5f);
        enemy.GetComponent<Renderer>().sharedMaterial = mat;

        Object.DestroyImmediate(enemy.GetComponent<Collider>());
        BoxCollider col = enemy.AddComponent<BoxCollider>();
        col.isTrigger = true;

        Rigidbody rb = enemy.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        enemy.AddComponent<EnemyController>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
        Object.DestroyImmediate(enemy);
        return prefab;
    }

    // -----------------------------------------------------------------
    // Helper: Create power-up prefab
    // -----------------------------------------------------------------
    static GameObject CreatePowerUpPrefab(Material mat)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        string prefabPath = "Assets/Prefabs/PowerUp.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null) return existing;

        GameObject powerUp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        powerUp.name = "PowerUp";
        powerUp.tag = "PowerUp";
        powerUp.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        powerUp.GetComponent<Renderer>().sharedMaterial = mat;

        Object.DestroyImmediate(powerUp.GetComponent<Collider>());
        SphereCollider col = powerUp.AddComponent<SphereCollider>();
        col.isTrigger = true;

        Rigidbody rb = powerUp.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        powerUp.AddComponent<PowerUp>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(powerUp, prefabPath);
        Object.DestroyImmediate(powerUp);
        return prefab;
    }

    // -----------------------------------------------------------------
    // Helper: Build the UI canvas with all HUD elements
    // -----------------------------------------------------------------
    static void SetupUI(GameObject player)
    {
        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        UIManager uiMgr = canvasObj.AddComponent<UIManager>();

        // --- Score Text (top-left) ---
        GameObject scoreObj = CreateUIText("ScoreText", canvasObj.transform,
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(10, -10), new Vector2(250, 40),
            "SCORE: 0", 20, TextAnchor.UpperLeft, Color.white);
        uiMgr.scoreText = scoreObj.GetComponent<Text>();

        // --- Health Text (top-right) ---
        GameObject healthObj = CreateUIText("HealthText", canvasObj.transform,
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-260, -10), new Vector2(250, 40),
            "HP: 100 / 100", 20, TextAnchor.UpperRight, Color.white);
        uiMgr.healthText = healthObj.GetComponent<Text>();

        // --- Health Bar Background ---
        GameObject healthBarBg = new GameObject("HealthBarBg");
        healthBarBg.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = healthBarBg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(1, 1);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.anchoredPosition = new Vector2(-135, -50);
        bgRect.sizeDelta = new Vector2(200, 16);
        Image bgImage = healthBarBg.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        // --- Health Bar Fill ---
        GameObject healthBarFillObj = new GameObject("HealthBarFill");
        healthBarFillObj.transform.SetParent(healthBarBg.transform, false);
        RectTransform fillRect = healthBarFillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = healthBarFillObj.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        uiMgr.healthBarFill = fillImage;

        // --- Game Over Panel (hidden by default) ---
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform goRect = gameOverPanel.AddComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero;
        goRect.anchorMax = Vector2.one;
        goRect.offsetMin = Vector2.zero;
        goRect.offsetMax = Vector2.zero;
        Image goImage = gameOverPanel.AddComponent<Image>();
        goImage.color = new Color(0, 0, 0, 0.75f);

        // Game Over title
        CreateUIText("GameOverTitle", gameOverPanel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 60), new Vector2(400, 60),
            "GAME OVER", 42, TextAnchor.MiddleCenter, Color.red);

        // Final score
        GameObject finalScoreObj = CreateUIText("FinalScoreText", gameOverPanel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(400, 40),
            "FINAL SCORE: 0", 28, TextAnchor.MiddleCenter, Color.white);
        uiMgr.finalScoreText = finalScoreObj.GetComponent<Text>();

        // Restart instruction
        GameObject restartObj = CreateUIText("RestartText", gameOverPanel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -50), new Vector2(400, 40),
            "Press R to Restart", 22, TextAnchor.MiddleCenter, Color.yellow);
        uiMgr.restartText = restartObj.GetComponent<Text>();

        uiMgr.gameOverPanel = gameOverPanel;
        gameOverPanel.SetActive(false);

        // EventSystem (required for UI)
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // -----------------------------------------------------------------
    // Helper: Create a UI Text element
    // -----------------------------------------------------------------
    static GameObject CreateUIText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 position, Vector2 size,
        string content, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        // Fallback if LegacyRuntime not available
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Add outline for readability
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        return obj;
    }
}
#endif
