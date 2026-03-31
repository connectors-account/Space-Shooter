#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor wizard that automatically builds the entire Space Shooter scene
/// with one click: Menu > Space Shooter > Setup Scene.
/// This creates all GameObjects, prefabs, UI, camera, and wires everything together.
/// </summary>
public class SceneSetupWizard
{
    [MenuItem("Space Shooter/Setup Scene (Auto-Build Everything)")]
    public static void SetupScene()
    {
        // ─── Ensure Tags exist ───
        // Tags must be set in TagManager.asset (already provided).
        // If you get tag errors, add "Enemy", "Bullet", "Player" in Edit > Project Settings > Tags & Layers.

        // ─── 1. CAMERA ───
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.tag = "MainCamera";
        }
        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = new Color(0.05f, 0.02f, 0.15f); // deep-space purple-black
        cam.clearFlags = CameraClearFlags.SolidColor;

        // ─── 2. PLAYER ───
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = CreateSprite("Player", Color.cyan, new Vector3(0.5f, 0.7f, 1f));
        }
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3.5f, 0);

        // Collider
        var playerCol = player.GetComponent<BoxCollider2D>();
        if (playerCol == null) playerCol = player.AddComponent<BoxCollider2D>();
        playerCol.isTrigger = true;

        // Rigidbody2D (kinematic so physics doesn't push it around)
        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        // Fire point
        Transform firePoint = player.transform.Find("FirePoint");
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(player.transform);
            fp.transform.localPosition = new Vector3(0, 0.45f, 0);
            firePoint = fp.transform;
        }

        // PlayerController script
        var pc = player.GetComponent<PlayerController>();
        if (pc == null) pc = player.AddComponent<PlayerController>();
        pc.firePoint = firePoint;

        // ─── 3. BULLET PREFAB ───
        string bulletPrefabPath = "Assets/Prefabs/Bullet.prefab";
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);
        if (bulletPrefab == null)
        {
            GameObject bullet = CreateSprite("Bullet", Color.yellow, new Vector3(0.15f, 0.3f, 1f));
            bullet.tag = "Bullet";

            var bulletCol = bullet.AddComponent<BoxCollider2D>();
            bulletCol.isTrigger = true;

            var bulletRb = bullet.AddComponent<Rigidbody2D>();
            bulletRb.bodyType = RigidbodyType2D.Kinematic;

            bullet.AddComponent<BulletController>();

            // Ensure Prefabs folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            bulletPrefab = PrefabUtility.SaveAsPrefabAsset(bullet, bulletPrefabPath);
            Object.DestroyImmediate(bullet);
        }
        pc.bulletPrefab = bulletPrefab;

        // ─── 4. ENEMY PREFAB ───
        string enemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);
        if (enemyPrefab == null)
        {
            GameObject enemy = CreateSprite("Enemy", Color.red, new Vector3(0.5f, 0.5f, 1f));
            enemy.tag = "Enemy";

            var enemyCol = enemy.AddComponent<BoxCollider2D>();
            enemyCol.isTrigger = true;

            var enemyRb = enemy.AddComponent<Rigidbody2D>();
            enemyRb.bodyType = RigidbodyType2D.Kinematic;

            enemy.AddComponent<EnemyController>();

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            enemyPrefab = PrefabUtility.SaveAsPrefabAsset(enemy, enemyPrefabPath);
            Object.DestroyImmediate(enemy);
        }

        // ─── 5. GAME MANAGER ───
        GameObject gmGO = GameObject.Find("GameManager");
        if (gmGO == null)
        {
            gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
        }

        // ─── 6. ENEMY SPAWNER ───
        GameObject spawnerGO = GameObject.Find("EnemySpawner");
        if (spawnerGO == null)
        {
            spawnerGO = new GameObject("EnemySpawner");
            var es = spawnerGO.AddComponent<EnemySpawner>();
            es.enemyPrefab = enemyPrefab;
        }
        else
        {
            var es = spawnerGO.GetComponent<EnemySpawner>();
            if (es != null) es.enemyPrefab = enemyPrefab;
        }

        // ─── 7. CANVAS & UI ───
        GameObject canvasGO = GameObject.Find("Canvas");
        Canvas canvas;
        if (canvasGO == null)
        {
            canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvas = canvasGO.GetComponent<Canvas>();
        }

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // -- Score Text (top-left)
        Text scoreText = CreateUIText(canvasGO, "ScoreText",
            new Vector2(10, -10), new Vector2(250, 40),
            "Score: 0", TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

        // -- Health Text (top-right)
        Text healthText = CreateUIText(canvasGO, "HealthText",
            new Vector2(-10, -10), new Vector2(250, 40),
            "Health: 5", TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));

        // -- Game Over Panel (centered, disabled by default)
        GameObject goPanel = CreateGameOverPanel(canvasGO);

        // -- UIManager
        GameObject uiMgrGO = GameObject.Find("UIManager");
        if (uiMgrGO == null)
        {
            uiMgrGO = new GameObject("UIManager");
            uiMgrGO.transform.SetParent(canvasGO.transform, false);
        }
        var uiMgr = uiMgrGO.GetComponent<UIManager>();
        if (uiMgr == null) uiMgr = uiMgrGO.AddComponent<UIManager>();

        uiMgr.scoreText = scoreText;
        uiMgr.healthText = healthText;
        uiMgr.gameOverPanel = goPanel;
        uiMgr.finalScoreText = goPanel.transform.Find("FinalScoreText")?.GetComponent<Text>();
        uiMgr.restartButton = goPanel.transform.Find("RestartButton")?.GetComponent<Button>();

        // ─── 8. SAVE ───
        string scenePath = "Assets/Scenes/MainScene.unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene(), scenePath);

        EditorUtility.DisplayDialog("Space Shooter",
            "Scene setup complete!\n\n" +
            "\u2022 Player (cyan) at bottom\n" +
            "\u2022 Enemy & Bullet prefabs in Assets/Prefabs\n" +
            "\u2022 UI with Score, Health, Game Over panel\n" +
            "\u2022 GameManager + EnemySpawner wired up\n\n" +
            "Press Play to test!", "OK");

        Debug.Log("[Space Shooter] Scene setup complete. Press Play!");
    }

    // ── Helper: create a colored sprite from the built-in white texture ──
    private static GameObject CreateSprite(string name, Color color, Vector3 scale)
    {
        GameObject go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();

        // Create a 1x1 white sprite
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        sr.color = color;
        go.transform.localScale = scale;
        return go;
    }

    // ── Helper: create anchored UI Text ──
    private static Text CreateUIText(GameObject canvas, string name,
        Vector2 anchoredPos, Vector2 size, string defaultText,
        TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        Transform existing = canvas.transform.Find(name);
        if (existing != null) return existing.GetComponent<Text>();

        GameObject go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var txt = go.AddComponent<Text>();
        txt.text = defaultText;
        txt.fontSize = 24;
        txt.color = Color.white;
        txt.alignment = alignment;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null)
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return txt;
    }

    // ── Helper: create the Game Over panel ──
    private static GameObject CreateGameOverPanel(GameObject canvas)
    {
        Transform existing = canvas.transform.Find("GameOverPanel");
        if (existing != null) return existing.gameObject;

        // Panel background
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvas.transform, false);
        var prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        var img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.75f);

        // "GAME OVER" title
        GameObject titleGO = new GameObject("GameOverTitle");
        titleGO.transform.SetParent(panel.transform, false);
        var trt = titleGO.AddComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 60);
        trt.sizeDelta = new Vector2(400, 60);
        var titleTxt = titleGO.AddComponent<Text>();
        titleTxt.text = "GAME OVER";
        titleTxt.fontSize = 48;
        titleTxt.color = Color.red;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (titleTxt.font == null)
            titleTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Final score text
        GameObject scoreTxtGO = new GameObject("FinalScoreText");
        scoreTxtGO.transform.SetParent(panel.transform, false);
        var srt = scoreTxtGO.AddComponent<RectTransform>();
        srt.anchoredPosition = new Vector2(0, 0);
        srt.sizeDelta = new Vector2(400, 40);
        var sTxt = scoreTxtGO.AddComponent<Text>();
        sTxt.text = "Final Score: 0";
        sTxt.fontSize = 30;
        sTxt.color = Color.white;
        sTxt.alignment = TextAnchor.MiddleCenter;
        sTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (sTxt.font == null)
            sTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Restart button
        GameObject btnGO = new GameObject("RestartButton");
        btnGO.transform.SetParent(panel.transform, false);
        var brt = btnGO.AddComponent<RectTransform>();
        brt.anchoredPosition = new Vector2(0, -60);
        brt.sizeDelta = new Vector2(200, 50);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        // Button label
        GameObject btnLabel = new GameObject("Text");
        btnLabel.transform.SetParent(btnGO.transform, false);
        var blrt = btnLabel.AddComponent<RectTransform>();
        blrt.anchorMin = Vector2.zero;
        blrt.anchorMax = Vector2.one;
        blrt.offsetMin = Vector2.zero;
        blrt.offsetMax = Vector2.zero;
        var blTxt = btnLabel.AddComponent<Text>();
        blTxt.text = "RESTART";
        blTxt.fontSize = 28;
        blTxt.color = Color.white;
        blTxt.alignment = TextAnchor.MiddleCenter;
        blTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (blTxt.font == null)
            blTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        panel.SetActive(false);
        return panel;
    }
}
#endif
