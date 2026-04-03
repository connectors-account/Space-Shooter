using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Editor tool that automatically sets up the entire game scene, creates prefabs,
/// generates sprite textures, and configures all game objects.
/// Run from menu: SpaceShooter > Setup Game
/// </summary>
public class GameSetup : EditorWindow
{
    [MenuItem("SpaceShooter/Setup Game (Full Auto Setup)")]
    public static void SetupGame()
    {
        if (!EditorUtility.DisplayDialog("Space Shooter Setup",
            "This will create the game scene, all prefabs, sprites, and materials.\n\nProceed?",
            "Yes, Set Up Everything", "Cancel"))
            return;

        Debug.Log("=== Space Shooter Auto Setup Starting ===");

        CreateDirectories();
        CreateSprites();
        CreateMaterials();
        CreatePrefabs();
        CreateGameScene();
        ConfigureTags();
        ConfigureBuildSettings();
        ConfigurePlayerSettings();

        Debug.Log("=== Space Shooter Setup Complete! Press Play to test. ===");
        EditorUtility.DisplayDialog("Setup Complete",
            "Game setup is complete!\n\n" +
            "1. Press PLAY to test the game\n" +
            "2. Use File > Build Settings to build the .exe\n\n" +
            "Controls:\n- WASD/Arrows: Move\n- Space: Shoot\n- Esc: Pause",
            "OK");
    }

    static void CreateDirectories()
    {
        string[] dirs = {
            "Assets/Sprites",
            "Assets/Prefabs",
            "Assets/Materials",
            "Assets/Scenes"
        };
        foreach (string dir in dirs)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        AssetDatabase.Refresh();
    }

    // ======================== SPRITE GENERATION ========================
    static void CreateSprites()
    {
        Debug.Log("Creating sprite textures...");

        // Player ship - triangle/arrow shape
        CreatePlayerShipSprite();
        // Enemy sprites
        CreateEnemyStraightSprite();
        CreateEnemyZigzagSprite();
        CreateEnemySwooperSprite();
        CreateEnemyBossSprite();
        // Bullet sprites
        CreateBulletSprite("PlayerBullet", Color.yellow, 8, 16);
        CreateBulletSprite("EnemyBullet", Color.red, 8, 12);
        // Power-up sprite
        CreatePowerUpSprite();

        AssetDatabase.Refresh();
    }

    static void CreatePlayerShipSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color hull = new Color(0.2f, 0.6f, 1f);
        Color cockpit = new Color(0.5f, 0.9f, 1f);
        Color engine = new Color(1f, 0.5f, 0.2f);

        // Draw ship body (triangle)
        for (int y = 0; y < size; y++)
        {
            float progress = (float)y / size;
            int halfWidth = (int)(progress * size / 2.5f);
            for (int x = size / 2 - halfWidth; x <= size / 2 + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, hull);
            }
        }

        // Cockpit (center bright spot)
        for (int x = size / 2 - 2; x <= size / 2 + 2; x++)
            for (int y = size / 2; y < size / 2 + 6; y++)
                if (x >= 0 && x < size && y >= 0 && y < size)
                    tex.SetPixel(x, y, cockpit);

        // Engine glow at bottom
        for (int x = size / 2 - 3; x <= size / 2 + 3; x++)
            for (int y = 0; y < 4; y++)
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, engine);

        // Wings
        for (int y = 4; y < 12; y++)
        {
            tex.SetPixel(2, y, hull);
            tex.SetPixel(3, y, hull);
            tex.SetPixel(size - 3, y, hull);
            tex.SetPixel(size - 4, y, hull);
        }

        SaveSprite(tex, "PlayerShip", size);
    }

    static void CreateEnemyStraightSprite()
    {
        int size = 24;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color body = Color.red;
        // Inverted triangle
        for (int y = 0; y < size; y++)
        {
            float progress = 1f - (float)y / size;
            int halfWidth = (int)(progress * size / 2.5f);
            for (int x = size / 2 - halfWidth; x <= size / 2 + halfWidth; x++)
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, body);
        }

        SaveSprite(tex, "EnemyStraight", size);
    }

    static void CreateEnemyZigzagSprite()
    {
        int size = 24;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color body = new Color(1f, 0.3f, 1f); // Magenta
        // Diamond shape
        int center = size / 2;
        for (int y = 0; y < size; y++)
        {
            int dist = Mathf.Abs(y - center);
            int halfWidth = center - dist;
            for (int x = center - halfWidth; x <= center + halfWidth; x++)
                if (x >= 0 && x < size)
                    tex.SetPixel(x, y, body);
        }

        SaveSprite(tex, "EnemyZigzag", size);
    }

    static void CreateEnemySwooperSprite()
    {
        int size = 24;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color body = new Color(0.2f, 1f, 0.2f); // Green
        // Crescent/wing shape
        int center = size / 2;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < center && dist > center - 5 && y < center + 2)
                    tex.SetPixel(x, y, body);
                // Center dot
                if (dist < 3)
                    tex.SetPixel(x, y, Color.white);
            }
        }

        SaveSprite(tex, "EnemySwooper", size);
    }

    static void CreateEnemyBossSprite()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        Color body = new Color(1f, 0.2f, 0f);
        Color accent = Color.yellow;

        int center = size / 2;
        // Large hexagonal shape
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = Mathf.Abs(x - center);
                float dy = Mathf.Abs(y - center);
                if (dx + dy * 0.7f < center - 2)
                {
                    tex.SetPixel(x, y, body);
                }
                // Accent border
                if (dx + dy * 0.7f < center && dx + dy * 0.7f > center - 4)
                {
                    tex.SetPixel(x, y, accent);
                }
            }
        }
        // Eyes
        tex.SetPixel(center - 6, center + 4, Color.white);
        tex.SetPixel(center - 5, center + 4, Color.white);
        tex.SetPixel(center + 5, center + 4, Color.white);
        tex.SetPixel(center + 6, center + 4, Color.white);

        SaveSprite(tex, "EnemyBoss", size);
    }

    static void CreateBulletSprite(string name, Color color, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        ClearTexture(tex, Color.clear);

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                float cx = (float)x / width;
                float cy = (float)y / height;
                // Rounded rectangle with glow
                Color c = color;
                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }
        // Bright center
        for (int x = 2; x < width - 2; x++)
            for (int y = 2; y < height - 2; y++)
                tex.SetPixel(x, y, Color.white);

        SaveSprite(tex, name, Mathf.Max(width, height));
    }

    static void CreatePowerUpSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        ClearTexture(tex, Color.clear);

        // Diamond power-up shape
        int center = size / 2;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int dist = Mathf.Abs(x - center) + Mathf.Abs(y - center);
                if (dist < center)
                    tex.SetPixel(x, y, Color.white);
            }
        }

        SaveSprite(tex, "PowerUp", size);
    }

    static void SaveSprite(Texture2D tex, string name, int pixelsPerUnit)
    {
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        byte[] png = tex.EncodeToPNG();
        string path = $"Assets/Sprites/{name}.png";
        File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path);

        // Configure as sprite
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    static void ClearTexture(Texture2D tex, Color color)
    {
        Color[] pixels = new Color[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
    }

    // ======================== MATERIALS ========================
    static void CreateMaterials()
    {
        Debug.Log("Creating materials...");

        // Background material
        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = new Color(0.02f, 0.02f, 0.08f);
        AssetDatabase.CreateAsset(bgMat, "Assets/Materials/BackgroundMat.mat");

        AssetDatabase.Refresh();
    }

    // ======================== PREFABS ========================
    static void CreatePrefabs()
    {
        Debug.Log("Creating prefabs...");

        // Player Bullet Prefab
        CreateBulletPrefab("PlayerBullet", "Sprites/PlayerBullet", Color.yellow, true, 12f, "Bullet");

        // Enemy Bullet Prefab
        CreateBulletPrefab("EnemyBullet", "Sprites/EnemyBullet", Color.red, false, 6f, "Bullet");

        // Power-Up Prefab
        CreatePowerUpPrefab();

        // Enemy Prefabs
        CreateEnemyPrefab("EnemyStraight", EnemyBase.EnemyType.Straight,
            "Sprites/EnemyStraight", Color.red, 1, 100, 3f);
        CreateEnemyPrefab("EnemyZigzag", EnemyBase.EnemyType.Zigzag,
            "Sprites/EnemyZigzag", new Color(1f, 0.3f, 1f), 2, 150, 2.5f);
        CreateEnemyPrefab("EnemySwooper", EnemyBase.EnemyType.Swooper,
            "Sprites/EnemySwooper", new Color(0.2f, 1f, 0.2f), 1, 200, 4f);
        CreateEnemyPrefab("EnemyBoss", EnemyBase.EnemyType.Boss,
            "Sprites/EnemyBoss", new Color(1f, 0.2f, 0f), 20, 1000, 1f);

        AssetDatabase.Refresh();
    }

    static void CreateBulletPrefab(string name, string spritePath, Color color,
        bool isPlayer, float speed, string tag)
    {
        GameObject obj = new GameObject(name);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/{spritePath}.png");
        sr.color = color;
        sr.sortingOrder = 3;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.3f, 0.5f);

        Bullet bullet = obj.AddComponent<Bullet>();
        bullet.speed = speed;
        bullet.isPlayerBullet = isPlayer;
        bullet.direction = isPlayer ? Vector3.up : Vector3.down;

        obj.tag = tag;
        obj.layer = isPlayer ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("Default");

        PrefabUtility.SaveAsPrefabAsset(obj, $"Assets/Prefabs/{name}.prefab");
        Object.DestroyImmediate(obj);
    }

    static void CreatePowerUpPrefab()
    {
        GameObject obj = new GameObject("PowerUp");

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PowerUp.png");
        sr.color = Color.white;
        sr.sortingOrder = 4;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        obj.AddComponent<PowerUp>();
        obj.tag = "PowerUp";

        PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Prefabs/PowerUp.prefab");
        Object.DestroyImmediate(obj);
    }

    static void CreateEnemyPrefab(string name, EnemyBase.EnemyType type,
        string spritePath, Color color, int health, int score, float speed)
    {
        GameObject obj = new GameObject(name);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/{spritePath}.png");
        sr.color = color;
        sr.sortingOrder = 2;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        float colSize = (type == EnemyBase.EnemyType.Boss) ? 1.5f : 0.7f;
        col.size = new Vector2(colSize, colSize);

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;

        EnemyBase eb = obj.AddComponent<EnemyBase>();
        eb.enemyType = type;
        eb.moveSpeed = speed;
        eb.scoreValue = score;
        eb.canShoot = true;
        eb.shootInterval = (type == EnemyBase.EnemyType.Boss) ? 1.5f : 2.5f;

        EnemyHealth eh = obj.AddComponent<EnemyHealth>();
        eh.maxHealth = health;
        eh.powerUpDropChance = (type == EnemyBase.EnemyType.Boss) ? 1f : 0.15f;

        obj.tag = "Enemy";

        // Scale boss larger
        if (type == EnemyBase.EnemyType.Boss)
            obj.transform.localScale = Vector3.one * 2f;

        PrefabUtility.SaveAsPrefabAsset(obj, $"Assets/Prefabs/{name}.prefab");
        Object.DestroyImmediate(obj);
    }

    // ======================== SCENE SETUP ========================
    static void CreateGameScene()
    {
        Debug.Log("Creating game scene...");

        // Create new scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Configure camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        // Load prefabs
        GameObject playerBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerBullet.prefab");
        GameObject enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyBullet.prefab");
        GameObject powerUpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PowerUp.prefab");
        GameObject straightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyStraight.prefab");
        GameObject zigzagPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyZigzag.prefab");
        GameObject swooperPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemySwooper.prefab");
        GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyBoss.prefab");

        // ---- Create Player ----
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, -3.5f, 0);
        player.tag = "Player";

        SpriteRenderer playerSR = player.AddComponent<SpriteRenderer>();
        playerSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerShip.png");
        playerSR.color = new Color(0.2f, 0.6f, 1f);
        playerSR.sortingOrder = 1;

        BoxCollider2D playerCol = player.AddComponent<BoxCollider2D>();
        playerCol.isTrigger = true;
        playerCol.size = new Vector2(0.6f, 0.8f);

        Rigidbody2D playerRB = player.AddComponent<Rigidbody2D>();
        playerRB.isKinematic = true;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.bulletPrefab = playerBulletPrefab;
        pc.bulletSpeed = 12f;

        PlayerHealth ph = player.AddComponent<PlayerHealth>();
        ph.maxHealth = 5;

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);
        pc.firePoint = firePoint.transform;

        // ---- Game Manager ----
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.playerObject = player;

        // ---- Score Manager ----
        GameObject scoreObj = new GameObject("ScoreManager");
        scoreObj.AddComponent<ScoreManager>();

        // ---- Audio Manager ----
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<AudioManager>();

        // ---- UI Manager ----
        GameObject uiObj = new GameObject("UIManager");
        uiObj.AddComponent<UIManager>();

        // ---- Enemy Spawner ----
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
        spawner.straightEnemyPrefab = straightPrefab;
        spawner.zigzagEnemyPrefab = zigzagPrefab;
        spawner.swooperEnemyPrefab = swooperPrefab;
        spawner.bossEnemyPrefab = bossPrefab;
        gm.enemySpawner = spawner;

        // Wire enemy bullet prefabs into enemy prefabs
        // (This is done via the prefab's serialized field - we need to update prefabs)
        WireEnemyBullets(enemyBulletPrefab, powerUpPrefab);

        // ---- Parallax Background ----
        GameObject bgObj = new GameObject("ParallaxBackground");
        bgObj.AddComponent<ParallaxBackground>();

        // ---- Effects Manager ----
        GameObject fxObj = new GameObject("EffectsManager");
        fxObj.AddComponent<EffectsManager>();

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
        Debug.Log("Scene saved to Assets/Scenes/MainScene.unity");
    }

    static void WireEnemyBullets(GameObject enemyBulletPrefab, GameObject powerUpPrefab)
    {
        string[] enemyPrefabs = {
            "Assets/Prefabs/EnemyStraight.prefab",
            "Assets/Prefabs/EnemyZigzag.prefab",
            "Assets/Prefabs/EnemySwooper.prefab",
            "Assets/Prefabs/EnemyBoss.prefab"
        };

        foreach (string path in enemyPrefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // Load as prefab content
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) continue;

            EnemyBase eb = instance.GetComponent<EnemyBase>();
            if (eb != null) eb.enemyBulletPrefab = enemyBulletPrefab;

            EnemyHealth eh = instance.GetComponent<EnemyHealth>();
            if (eh != null) eh.powerUpPrefab = powerUpPrefab;

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }
    }

    // ======================== TAGS & LAYERS ========================
    static void ConfigureTags()
    {
        Debug.Log("Configuring tags...");

        // Tags are configured via the TagManager asset
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Enemy");
        AddTag(tagsProp, "Bullet");
        AddTag(tagsProp, "PowerUp");

        tagManager.ApplyModifiedProperties();
    }

    static void AddTag(SerializedProperty tagsProp, string tag)
    {
        // Check if tag exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
    }

    // ======================== BUILD SETTINGS ========================
    static void ConfigureBuildSettings()
    {
        Debug.Log("Configuring build settings...");

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainScene.unity", true)
        };
    }

    static void ConfigurePlayerSettings()
    {
        Debug.Log("Configuring player settings...");

        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.companyName = "SpaceShooterDev";
        PlayerSettings.defaultScreenWidth = 800;
        PlayerSettings.defaultScreenHeight = 600;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = false;

        // Set 2D physics settings for trigger collisions
        Physics2D.queriesHitTriggers = true;
    }
}
