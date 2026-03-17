using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Editor utility that auto-creates the two game scenes, all prefabs,
/// layers, and tags.  Run via menu: Tools > Space Shooter > Setup Project
/// This saves HOURS of manual inspector clicking.
/// </summary>
public class SceneSetup : EditorWindow
{
    [MenuItem("Tools/Space Shooter/1 - Setup Layers and Tags")]
    public static void SetupLayersAndTags()
    {
        AddLayer("Player");
        AddLayer("Enemy");
        AddLayer("PlayerBullet");
        AddLayer("EnemyBullet");
        AddLayer("PowerUp");

        AddTag("Player");
        AddTag("Enemy");
        AddTag("Bullet");
        AddTag("PowerUp");
        AddTag("Boundary");

        Debug.Log("[SceneSetup] Layers and tags configured.");
    }

    [MenuItem("Tools/Space Shooter/2 - Generate Sprites")]
    public static void GenerateSprites()
    {
        SpriteGenerator.GenerateAllSprites();
    }

    [MenuItem("Tools/Space Shooter/3 - Create Prefabs")]
    public static void CreatePrefabs()
    {
        string prefabFolder = "Assets/Prefabs";
        if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);

        CreateBulletPrefab(prefabFolder);
        CreatePlayerPrefab(prefabFolder);
        CreateEnemyPrefabs(prefabFolder);
        CreatePowerUpPrefabs(prefabFolder);

        AssetDatabase.Refresh();
        Debug.Log("[SceneSetup] All prefabs created in " + prefabFolder);
    }

    [MenuItem("Tools/Space Shooter/4 - Create Game Scene")]
    public static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ── Camera setup ─────────────────────────────────────────────
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.transform.position = new Vector3(0, 0, -10);
        }

        // ── GameManager ──────────────────────────────────────────────
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // ── SpawnManager ─────────────────────────────────────────────
        GameObject spawnObj = new GameObject("SpawnManager");
        SpawnManager sm = spawnObj.AddComponent<SpawnManager>();

        // Assign enemy prefabs
        string[] enemyPaths = {
            "Assets/Prefabs/Enemy_Basic.prefab",
            "Assets/Prefabs/Enemy_Fast.prefab",
            "Assets/Prefabs/Enemy_Tank.prefab"
        };
        GameObject[] enemyPrefabs = new GameObject[enemyPaths.Length];
        for (int i = 0; i < enemyPaths.Length; i++)
            enemyPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPaths[i]);

        SerializedObject smSO = new SerializedObject(sm);
        SerializedProperty epProp = smSO.FindProperty("enemyPrefabs");
        epProp.arraySize = enemyPrefabs.Length;
        for (int i = 0; i < enemyPrefabs.Length; i++)
            epProp.GetArrayElementAtIndex(i).objectReferenceValue = enemyPrefabs[i];
        smSO.ApplyModifiedProperties();

        // ── AudioManager ─────────────────────────────────────────────
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<AudioManager>();

        // ── Player ───────────────────────────────────────────────────
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (playerPrefab != null)
        {
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(0, -3.5f, 0);
        }

        // ── Background ──────────────────────────────────────────────
        CreateBackgroundObjects();

        // ── Canvas / UI ──────────────────────────────────────────────
        CreateGameUI();

        // ── Save scene ───────────────────────────────────────────────
        string scenePath = "Assets/Scenes/GameScene.unity";
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[SceneSetup] GameScene saved to " + scenePath);

        // Add to build settings
        AddSceneToBuildSettings(scenePath);
    }

    [MenuItem("Tools/Space Shooter/5 - Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        }

        // ── Canvas ───────────────────────────────────────────────────
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        MainMenuController menuCtrl = canvasObj.AddComponent<MainMenuController>();

        // Title
        GameObject titleObj = CreateUIText(canvasObj.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0, 120), 48, Color.white, TextAnchor.MiddleCenter);

        // Subtitle
        CreateUIText(canvasObj.transform, "SubtitleText", "Arrow Keys + Space to Play",
            new Vector2(0, 50), 18, Color.gray, TextAnchor.MiddleCenter);

        // Play button
        GameObject playBtn = CreateUIButton(canvasObj.transform, "PlayButton", "PLAY",
            new Vector2(0, -30), new Vector2(200, 50));
        playBtn.GetComponent<Button>().onClick.AddListener(menuCtrl.OnPlayButton);

        // Quit button
        GameObject quitBtn = CreateUIButton(canvasObj.transform, "QuitButton", "QUIT",
            new Vector2(0, -100), new Vector2(200, 50));
        quitBtn.GetComponent<Button>().onClick.AddListener(menuCtrl.OnQuitButton);

        // Wire title text
        SerializedObject menuSO = new SerializedObject(menuCtrl);
        menuSO.FindProperty("titleText").objectReferenceValue = titleObj.GetComponent<Text>();
        menuSO.ApplyModifiedProperties();

        // EventSystem
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath);
        Debug.Log("[SceneSetup] MainMenu scene saved to " + scenePath);
    }

    [MenuItem("Tools/Space Shooter/Run ALL Setup Steps (1-5)")]
    public static void RunAllSetupSteps()
    {
        SetupLayersAndTags();
        GenerateSprites();
        AssetDatabase.Refresh();

        // Short delay to let sprites import
        EditorApplication.delayCall += () =>
        {
            SetSpriteImportSettings();
            CreatePrefabs();

            EditorApplication.delayCall += () =>
            {
                CreateMainMenuScene();
                CreateGameScene();
                Debug.Log("=== ALL SETUP COMPLETE! Press Play to test. ===");
            };
        };
    }

    // ═══════════════════════════════════════════════════════════════════
    //  HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════

    private static void SetSpriteImportSettings()
    {
        string[] spritePaths = {
            "Assets/Sprites/Player.png",
            "Assets/Sprites/Enemy_Basic.png",
            "Assets/Sprites/Enemy_Fast.png",
            "Assets/Sprites/Enemy_Tank.png",
            "Assets/Sprites/Bullet.png",
            "Assets/Sprites/PowerUp_Weapon.png",
            "Assets/Sprites/PowerUp_Health.png",
            "Assets/Sprites/PowerUp_Shield.png",
            "Assets/Sprites/Background.png"
        };

        foreach (string path in spritePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static void CreateBulletPrefab(string folder)
    {
        GameObject go = new GameObject("Bullet");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bullet.png");
        sr.sortingOrder = 3;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.15f, 0.3f);

        go.AddComponent<BulletController>();

        PrefabUtility.SaveAsPrefabAsset(go, folder + "/Bullet.prefab");
        Object.DestroyImmediate(go);
    }

    private static void CreatePlayerPrefab(string folder)
    {
        GameObject go = new GameObject("Player");
        go.layer = LayerMask.NameToLayer("Player");
        go.tag = "Player";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player.png");
        sr.sortingOrder = 2;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.8f);

        PlayerController pc = go.AddComponent<PlayerController>();

        // Fire point
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(go.transform);
        fp.transform.localPosition = new Vector3(0, 0.6f, 0);

        // Assign bullet prefab and fire point via serialized object
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/Bullet.prefab");
        SerializedObject so = new SerializedObject(pc);
        so.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
        so.FindProperty("firePoint").objectReferenceValue = fp.transform;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(go, folder + "/Player.prefab");
        Object.DestroyImmediate(go);
    }

    private static void CreateEnemyPrefabs(string folder)
    {
        string[] names   = { "Enemy_Basic", "Enemy_Fast", "Enemy_Tank" };
        string[] sprites = { "Enemy_Basic", "Enemy_Fast", "Enemy_Tank" };

        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/Bullet.prefab");
        GameObject[] powerUpPrefabs = {
            AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/PowerUp_Weapon.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/PowerUp_Health.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/PowerUp_Shield.prefab")
        };

        // Create power-up prefabs first if they don't exist
        if (powerUpPrefabs[0] == null) CreatePowerUpPrefabs(folder);
        powerUpPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/PowerUp_Weapon.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/PowerUp_Health.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/PowerUp_Shield.prefab")
        };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject go = new GameObject(names[i]);
            go.layer = LayerMask.NameToLayer("Enemy");
            go.tag = "Enemy";

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/" + sprites[i] + ".png");
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.7f, 0.7f);

            EnemyController ec = go.AddComponent<EnemyController>();

            SerializedObject so = new SerializedObject(ec);
            so.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;

            // Assign power-up drop array
            SerializedProperty puProp = so.FindProperty("powerUpPrefabs");
            puProp.arraySize = powerUpPrefabs.Length;
            for (int j = 0; j < powerUpPrefabs.Length; j++)
                puProp.GetArrayElementAtIndex(j).objectReferenceValue = powerUpPrefabs[j];

            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, folder + "/" + names[i] + ".prefab");
            Object.DestroyImmediate(go);
        }
    }

    private static void CreatePowerUpPrefabs(string folder)
    {
        string[] names = { "PowerUp_Weapon", "PowerUp_Health", "PowerUp_Shield" };
        PowerUpController.PowerUpType[] types = {
            PowerUpController.PowerUpType.WeaponUpgrade,
            PowerUpController.PowerUpType.Health,
            PowerUpController.PowerUpType.Shield
        };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject go = new GameObject(names[i]);
            int puLayer = LayerMask.NameToLayer("PowerUp");
            if (puLayer >= 0) go.layer = puLayer;
            go.tag = "PowerUp";

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/" + names[i] + ".png");
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 0.5f);

            PowerUpController pc = go.AddComponent<PowerUpController>();
            SerializedObject so = new SerializedObject(pc);
            so.FindProperty("type").enumValueIndex = (int)types[i];
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, folder + "/" + names[i] + ".prefab");
            Object.DestroyImmediate(go);
        }
    }

    private static void CreateBackgroundObjects()
    {
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Background.png");

        GameObject bgParent = new GameObject("Background");
        bgParent.AddComponent<ParallaxBackground>();

        for (int i = 0; i < 2; i++)
        {
            GameObject panel = new GameObject("BG_Panel_" + i);
            panel.transform.SetParent(bgParent.transform);
            SpriteRenderer sr = panel.AddComponent<SpriteRenderer>();
            sr.sprite = bgSprite;
            sr.sortingOrder = -10;
            // Scale to fill camera view
            panel.transform.localScale = new Vector3(5f, 5f, 1f);
            float panelH = sr.bounds != null ? 10f : 10f;
            panel.transform.localPosition = new Vector3(0, i * panelH, 5f);
        }
    }

    private static void CreateGameUI()
    {
        // ── Canvas ───────────────────────────────────────────────────
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        canvasObj.AddComponent<GraphicRaycaster>();

        UIManager uiMgr = canvasObj.AddComponent<UIManager>();
        SerializedObject uiSO = new SerializedObject(uiMgr);

        // Score text (top-left)
        GameObject scoreObj = CreateUIText(canvasObj.transform, "ScoreText", "SCORE: 0",
            new Vector2(-280, 270), 24, Color.white, TextAnchor.MiddleLeft);
        uiSO.FindProperty("scoreText").objectReferenceValue = scoreObj.GetComponent<Text>();

        // Wave text (top-center)
        GameObject waveObj = CreateUIText(canvasObj.transform, "WaveText", "WAVE 1",
            new Vector2(0, 270), 24, Color.yellow, TextAnchor.MiddleCenter);
        uiSO.FindProperty("waveText").objectReferenceValue = waveObj.GetComponent<Text>();

        // Health text (top-right)
        GameObject healthObj = CreateUIText(canvasObj.transform, "HealthText", "♥ ♥ ♥ ♥ ♥",
            new Vector2(260, 270), 24, Color.red, TextAnchor.MiddleRight);
        uiSO.FindProperty("healthText").objectReferenceValue = healthObj.GetComponent<Text>();

        // ── Pause Panel ──────────────────────────────────────────────
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0, 0, 0, 0.7f));
        CreateUIText(pausePanel.transform, "PauseTitle", "PAUSED",
            new Vector2(0, 80), 40, Color.white, TextAnchor.MiddleCenter);

        GameObject resumeBtn = CreateUIButton(pausePanel.transform, "ResumeButton", "RESUME",
            new Vector2(0, 0), new Vector2(200, 50));
        GameObject menuBtn1 = CreateUIButton(pausePanel.transform, "MainMenuButton", "MAIN MENU",
            new Vector2(0, -70), new Vector2(200, 50));

        uiSO.FindProperty("pausePanel").objectReferenceValue = pausePanel;

        // ── Game Over Panel ──────────────────────────────────────────
        GameObject goPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.1f, 0, 0, 0.8f));
        CreateUIText(goPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0, 100), 44, Color.red, TextAnchor.MiddleCenter);

        GameObject finalScoreObj = CreateUIText(goPanel.transform, "FinalScoreText", "FINAL SCORE\n0",
            new Vector2(0, 30), 28, Color.white, TextAnchor.MiddleCenter);
        uiSO.FindProperty("finalScoreText").objectReferenceValue = finalScoreObj.GetComponent<Text>();

        GameObject restartBtn = CreateUIButton(goPanel.transform, "RestartButton", "RESTART",
            new Vector2(0, -50), new Vector2(200, 50));
        GameObject menuBtn2 = CreateUIButton(goPanel.transform, "MenuButton2", "MAIN MENU",
            new Vector2(0, -120), new Vector2(200, 50));

        uiSO.FindProperty("gameOverPanel").objectReferenceValue = goPanel;

        uiSO.ApplyModifiedProperties();

        // Wire button callbacks via SerializedObject or direct
        // (buttons wired to UIManager methods)
        resumeBtn.GetComponent<Button>().onClick.AddListener(uiMgr.OnResumeButton);
        menuBtn1.GetComponent<Button>().onClick.AddListener(uiMgr.OnMainMenuButton);
        restartBtn.GetComponent<Button>().onClick.AddListener(uiMgr.OnRestartButton);
        menuBtn2.GetComponent<Button>().onClick.AddListener(uiMgr.OnMainMenuButton);

        pausePanel.SetActive(false);
        goPanel.SetActive(false);

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ── UI Factory Helpers ───────────────────────────────────────────
    private static GameObject CreateUIText(Transform parent, string name, string content,
        Vector2 pos, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(400, 60);

        Text txt = go.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = alignment;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;

        return go;
    }

    private static GameObject CreateUIButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
        cb.pressedColor = new Color(0.15f, 0.15f, 0.25f);
        btn.colors = cb;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.sizeDelta = Vector2.zero;

        Text txt = labelObj.AddComponent<Text>();
        txt.text = label;
        txt.fontSize = 22;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return btnObj;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        return go;
    }

    // ── Layer / Tag helpers ──────────────────────────────────────────
    private static void AddLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        // Check if already exists
        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                return;
        }

        // Add to first empty slot (user layers start at index 8)
        for (int i = 8; i < layers.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
            {
                layers.GetArrayElementAtIndex(i).stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
        Debug.LogWarning("No empty layer slots for: " + layerName);
    }

    private static void AddTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tagName)
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
            if (s.path == scenePath) return;

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
