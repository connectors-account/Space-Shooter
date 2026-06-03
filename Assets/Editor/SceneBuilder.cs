#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// EDITOR-ONLY automation tool that builds the entire Space Shooter game for you.
///
/// Instead of manually creating dozens of GameObjects, sprites, prefabs and UI
/// elements, just run the menu command:
///
///     Tools  ->  Space Shooter  ->  Build Game (One Click)
///
/// This will:
///   1. Generate simple colored sprites (player, enemy, bullets) and save them.
///   2. Create the required tags (Player, Enemy, PlayerBullet, EnemyBullet).
///   3. Create bullet/enemy/player prefabs with all components wired up.
///   4. Build the "Game" scene (camera, player, spawner, full HUD + game over UI).
///   5. Build the "MainMenu" scene (title + Start/Quit buttons).
///   6. Add both scenes to Build Settings and set the platform to Windows.
///
/// After running it, press Play (open the MainMenu scene first) and the game
/// works immediately. This file lives in an Editor folder so it is NOT included
/// in the final build.
/// </summary>
public static class SceneBuilder
{
    // Folder paths used to store generated assets.
    private const string SpritesFolder = "Assets/Sprites";
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string ScenesFolder = "Assets/Scenes";

    [MenuItem("Tools/Space Shooter/Build Game (One Click)")]
    public static void BuildEverything()
    {
        EnsureFolders();
        EnsureTags();

        // 1. Generate sprites.
        Sprite playerSprite = CreateSprite("PlayerSprite", new Color(0.2f, 0.8f, 1f), ShapeType.Triangle);
        Sprite enemySprite = CreateSprite("EnemySprite", new Color(1f, 0.3f, 0.3f), ShapeType.TriangleDown);
        Sprite playerBulletSprite = CreateSprite("PlayerBulletSprite", new Color(1f, 1f, 0.4f), ShapeType.Square, 8, 24);
        Sprite enemyBulletSprite = CreateSprite("EnemyBulletSprite", new Color(1f, 0.5f, 0.2f), ShapeType.Square, 8, 24);

        // 2. Create prefabs (bullets first, then enemy/player that reference them).
        GameObject playerBulletPrefab = CreateBulletPrefab("PlayerBullet", playerBulletSprite, "PlayerBullet");
        GameObject enemyBulletPrefab = CreateBulletPrefab("EnemyBullet", enemyBulletSprite, "EnemyBullet");
        GameObject enemyPrefab = CreateEnemyPrefab(enemySprite, enemyBulletPrefab);
        GameObject playerPrefab = CreatePlayerPrefab(playerSprite, playerBulletPrefab);

        // 3. Build scenes.
        BuildGameScene(playerPrefab, enemyPrefab);
        BuildMainMenuScene();

        // 4. Register scenes in build settings + target Windows.
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Space Shooter",
            "Game built successfully!\n\n" +
            "Open Assets/Scenes/MainMenu.unity and press Play.\n\n" +
            "To build the Windows .exe: File > Build Settings > Build.",
            "Awesome!");
    }

    // ----------------------------------------------------------------- folders

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(SpritesFolder)) AssetDatabase.CreateFolder("Assets", "Sprites");
        if (!AssetDatabase.IsValidFolder(PrefabsFolder)) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(ScenesFolder)) AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    // ------------------------------------------------------------------- tags

    private static void EnsureTags()
    {
        string[] requiredTags = { "Player", "Enemy", "PlayerBullet", "EnemyBullet" };

        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (string tag in requiredTags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) { found = true; break; }
            }
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            }
        }
        tagManager.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------------- sprites

    private enum ShapeType { Square, Triangle, TriangleDown }

    /// <summary>
    /// Creates a simple procedural sprite texture and saves it as a PNG asset.
    /// </summary>
    private static Sprite CreateSprite(string name, Color color, ShapeType shape, int width = 48, int height = 48)
    {
        string path = $"{SpritesFolder}/{name}.png";

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool fill = false;
                float nx = (float)x / (width - 1);
                float ny = (float)y / (height - 1);

                switch (shape)
                {
                    case ShapeType.Square:
                        fill = true;
                        break;
                    case ShapeType.Triangle: // points up
                        fill = ny <= 1f - Mathf.Abs(nx - 0.5f) * 2f;
                        break;
                    case ShapeType.TriangleDown: // points down
                        fill = ny >= Mathf.Abs(nx - 0.5f) * 2f;
                        break;
                }
                tex.SetPixel(x, y, fill ? color : clear);
            }
        }
        tex.Apply();

        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        // Configure the importer so it imports as a Sprite.
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 48;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---------------------------------------------------------------- prefabs

    private static GameObject CreateBulletPrefab(string name, Sprite sprite, string tag)
    {
        GameObject go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        go.AddComponent<Bullet>();
        go.AddComponent<DestroyOffScreen>();
        go.tag = tag;

        string path = $"{PrefabsFolder}/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateEnemyPrefab(Sprite sprite, GameObject enemyBulletPrefab)
    {
        GameObject go = new GameObject("Enemy");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        var enemy = go.AddComponent<Enemy>();
        enemy.bulletPrefab = enemyBulletPrefab;

        // Fire point slightly below the enemy.
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(go.transform);
        firePoint.transform.localPosition = new Vector3(0f, -0.6f, 0f);
        enemy.firePoint = firePoint.transform;

        go.AddComponent<DestroyOffScreen>();
        go.tag = "Enemy";

        string path = $"{PrefabsFolder}/Enemy.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreatePlayerPrefab(Sprite sprite, GameObject playerBulletPrefab)
    {
        GameObject go = new GameObject("Player");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        var pc = go.AddComponent<PlayerController>();
        pc.bulletPrefab = playerBulletPrefab;

        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(go.transform);
        firePoint.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        pc.firePoint = firePoint.transform;

        go.AddComponent<GameBoundary>();
        go.tag = "Player";

        string path = $"{PrefabsFolder}/Player.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ----------------------------------------------------------------- scenes

    private static void BuildGameScene(GameObject playerPrefab, GameObject enemyPrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera.
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();
        camGo.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.12f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGo.transform.position = new Vector3(0, 0, -10);

        // Player instance.
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.transform.position = new Vector3(0, -4f, 0);

        // GameManager.
        GameObject gmGo = new GameObject("GameManager");
        gmGo.AddComponent<GameManager>();

        // Spawner.
        GameObject spawnerGo = new GameObject("EnemySpawner");
        var spawner = spawnerGo.AddComponent<EnemySpawner>();
        spawner.enemyPrefab = enemyPrefab;

        // ---- UI ----
        GameObject canvasGo = CreateCanvas(out _);
        var uiManagerGo = new GameObject("UIManager");
        var uiManager = uiManagerGo.AddComponent<UIManager>();

        Text scoreText = CreateText(canvasGo.transform, "ScoreText", "Score: 0",
            new Vector2(0, 1), new Vector2(20, -20), TextAnchor.UpperLeft);
        Text healthText = CreateText(canvasGo.transform, "HealthText", "Health: 100 / 100",
            new Vector2(0, 1), new Vector2(20, -55), TextAnchor.UpperLeft);
        Text waveText = CreateText(canvasGo.transform, "WaveText", "Wave: 1",
            new Vector2(1, 1), new Vector2(-20, -20), TextAnchor.UpperRight);

        uiManager.scoreText = scoreText;
        uiManager.healthText = healthText;
        uiManager.waveText = waveText;

        // Game Over panel.
        GameObject panel = CreatePanel(canvasGo.transform, "GameOverPanel");
        Text goText = CreateText(panel.transform, "GameOverText", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0, 80), TextAnchor.MiddleCenter, 48);
        Text finalScore = CreateText(panel.transform, "FinalScoreText", "Final Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0, 20), TextAnchor.MiddleCenter, 28);

        Button restartBtn = CreateButton(panel.transform, "RestartButton", "Restart",
            new Vector2(0.5f, 0.5f), new Vector2(0, -50));
        Button menuBtn = CreateButton(panel.transform, "MainMenuButton", "Main Menu",
            new Vector2(0.5f, 0.5f), new Vector2(0, -110));

        restartBtn.onClick.AddListener(uiManager.OnRestartButton);
        menuBtn.onClick.AddListener(uiManager.OnMainMenuButton);

        uiManager.gameOverPanel = panel;
        uiManager.gameOverText = goText;
        uiManager.finalScoreText = finalScore;
        panel.SetActive(false);

        // EventSystem for UI input.
        CreateEventSystem();

        string scenePath = $"{ScenesFolder}/Game.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
    }

    private static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();
        camGo.tag = "MainCamera";
        cam.orthographic = true;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.12f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGo.transform.position = new Vector3(0, 0, -10);

        GameObject canvasGo = CreateCanvas(out _);

        CreateText(canvasGo.transform, "Title", "SPACE SHOOTER",
            new Vector2(0.5f, 0.5f), new Vector2(0, 150), TextAnchor.MiddleCenter, 54);

        GameObject menuGo = new GameObject("MainMenu");
        var menu = menuGo.AddComponent<MainMenu>();
        menu.gameSceneName = "Game";

        Button startBtn = CreateButton(canvasGo.transform, "StartButton", "Start Game",
            new Vector2(0.5f, 0.5f), new Vector2(0, 0));
        Button quitBtn = CreateButton(canvasGo.transform, "QuitButton", "Quit",
            new Vector2(0.5f, 0.5f), new Vector2(0, -70));

        startBtn.onClick.AddListener(menu.StartGame);
        quitBtn.onClick.AddListener(menu.QuitGame);

        CreateEventSystem();

        string scenePath = $"{ScenesFolder}/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // -------------------------------------------------------------- UI helpers

    private static GameObject CreateCanvas(out Canvas canvas)
    {
        GameObject canvasGo = new GameObject("Canvas");
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();
        return canvasGo;
    }

    private static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static Text CreateText(Transform parent, string name, string content,
        Vector2 anchor, Vector2 anchoredPos, TextAnchor align, int fontSize = 28)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ??
                    Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = align;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = text.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = new Vector2(500, 60);
        rt.anchoredPosition = anchoredPos;
        return text;
    }

    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f, 1f);
        Button btn = go.AddComponent<Button>();

        RectTransform rt = img.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = new Vector2(220, 50);
        rt.anchoredPosition = anchoredPos;

        Text txt = CreateText(go.transform, "Text", label, new Vector2(0.5f, 0.5f),
            Vector2.zero, TextAnchor.MiddleCenter, 24);
        RectTransform trt = txt.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return btn;
    }

    // -------------------------------------------------------- build settings

    private static void ConfigureBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene($"{ScenesFolder}/MainMenu.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/Game.unity", true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();

        // Target Windows 64-bit standalone.
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
    }
}
#endif
