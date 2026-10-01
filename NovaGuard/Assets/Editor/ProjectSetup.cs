using System;
using System.IO;
using NovaGuard;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Generates real serialized prefabs and a playable scene using public Unity Editor APIs.</summary>
[InitializeOnLoad]
public static class ProjectSetup
{
    public const string ScenePath = "Assets/Generated/Scenes/NovaGuard.unity";
    private const string Prefabs = "Assets/Generated/Prefabs/";
    private static readonly string[] ArtNames = { "Player", "Scout", "Gunner", "Tank", "Repair", "Rapid", "Shield", "PlayerBolt", "EnemyBolt" };

    static ProjectSetup() { EditorApplication.delayCall += FirstImport; }

    private static void FirstImport()
    {
        if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += FirstImport;
            return;
        }
        if (SceneManager.GetActiveScene().isDirty)
        {
            Debug.LogWarning("Nova Guard: save your current scene, then choose Tools > Nova Guard > Generate Project.");
            return;
        }
        Generate();
    }

    [MenuItem("Tools/Nova Guard/Generate Project")]
    public static void GenerateFromMenu()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Regenerate Nova Guard?",
            "This replaces the generated scene and prefabs. Custom changes inside Assets/Generated will be overwritten. Scripts and art are kept.",
            "Regenerate", "Cancel")) return;
        Generate();
    }

    // Command-line entry point: -executeMethod ProjectSetup.GenerateBatch
    public static void GenerateBatch() { Generate(); }

    private static void Generate()
    {
        Debug.Log("Nova Guard: generating assets and Windows desktop scene.");
        Directory.CreateDirectory(Prefabs);
        Directory.CreateDirectory("Assets/Generated/Scenes");
        AssetDatabase.Refresh();
        ConfigureProject();
        ImportArt();
        PlayerShip player = CreatePlayer();
        EnemyShip[] enemies = {
            CreateEnemy("Scout", EnemyKind.Scout, 2, 100, 2.0f, 2.3f, 1.25f),
            CreateEnemy("Gunner", EnemyKind.Gunner, 3, 175, 1.5f, 1.9f, 1.4f),
            CreateEnemy("Tank", EnemyKind.Tank, 7, 350, 1.0f, 2.7f, 1.85f)
        };
        Bullet friendly = CreateBullet("PlayerBolt", PhysicsRules.FriendlyLayer);
        Bullet hostile = CreateBullet("EnemyBolt", PhysicsRules.HostileLayer);
        PowerUp[] pickups = {
            CreatePickup("Repair", PickupKind.Repair),
            CreatePickup("Rapid", PickupKind.Rapid),
            CreatePickup("Shield", PickupKind.Shield)
        };

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(FixedAspect));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 0, -10);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 9;
        camera.nearClipPlane = .1f;
        camera.farClipPlane = 50;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.008f, .015f, .045f);
        // Black clear camera keeps letterbox bars clean on ultrawide or tall displays.
        var bars = new GameObject("Letterbox Camera", typeof(Camera));
        Camera barCamera = bars.GetComponent<Camera>();
        barCamera.depth = -10;
        barCamera.cullingMask = 0;
        barCamera.clearFlags = CameraClearFlags.SolidColor;
        barCamera.backgroundColor = Color.black;
        new GameObject("Three-layer starfield", typeof(Starfield));
        var root = new GameObject("Run Objects");
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var systems = new GameObject("Game Systems", typeof(PhysicsRules), typeof(GameController),
            typeof(WaveDirector), typeof(GameUI), typeof(AudioSource), typeof(SoundBank));
        GameController controller = systems.GetComponent<GameController>();
        controller.playerPrefab = player;
        controller.enemyPrefabs = enemies;
        controller.playerBulletPrefab = friendly;
        controller.enemyBulletPrefab = hostile;
        controller.powerUpPrefabs = pickups;
        controller.worldRoot = root.transform;
        controller.waves = systems.GetComponent<WaveDirector>();
        controller.ui = systems.GetComponent<GameUI>();
        controller.sound = systems.GetComponent<SoundBank>();
        controller.sound.clips = new AudioClip[Enum.GetValues(typeof(SoundCue)).Length];
        foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
        {
            string path = "Assets/Audio/" + cue + ".wav";
            controller.sound.clips[(int)cue] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (controller.sound.clips[(int)cue] == null) throw new InvalidOperationException("Missing sound: " + path);
        }
        AudioSource audio = systems.GetComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.spatialBlend = 0;
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save " + ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        ProjectValidation.Validate();
        Debug.Log("Nova Guard: generated 9 prefabs and NovaGuard.unity. Press Play to test. Build through Tools > Nova Guard > Build Windows x64.");
    }

    private static void ConfigureProject()
    {
        PlayerSettings.companyName = "Nova Guard Studio";
        PlayerSettings.productName = "Nova Guard";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = false;
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Standard);
        QualitySettings.vSyncCount = 1;
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tags.FindProperty("layers");
        string[] names = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "Pickup" };
        for (int i = 0; i < names.Length; i++) layers.GetArrayElementAtIndex(i + 8).stringValue = names[i];
        tags.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ImportArt()
    {
        foreach (string name in ArtNames)
        {
            string path = "Assets/Art/" + name + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing sprite: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 512;
            importer.SaveAndReimport();
        }
    }

    private static GameObject Visual(string name, int layer, float height)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/" + name + ".png");
        if (sprite == null) throw new InvalidOperationException("Sprite import failed: " + name);
        var item = new GameObject(name, typeof(SpriteRenderer));
        item.layer = layer;
        SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 5;
        item.transform.localScale = Vector3.one * (height / sprite.bounds.size.y);
        return item;
    }

    private static void Body(GameObject item)
    {
        Rigidbody2D body = item.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.bodyType = RigidbodyType2D.Dynamic;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private static void Circle(GameObject item, float worldRadius)
    {
        CircleCollider2D collider = item.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = worldRadius / item.transform.localScale.x;
    }

    private static T Save<T>(GameObject item) where T : Component
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(item, Prefabs + item.name + ".prefab");
        UnityEngine.Object.DestroyImmediate(item);
        if (prefab == null) throw new IOException("Prefab save failed for " + typeof(T).Name);
        return prefab.GetComponent<T>();
    }

    private static PlayerShip CreatePlayer()
    {
        GameObject item = Visual("Player", PhysicsRules.PlayerLayer, 1.5f);
        Body(item);
        Circle(item, .36f);
        item.AddComponent<PlayerShip>();
        return Save<PlayerShip>(item);
    }

    private static EnemyShip CreateEnemy(string name, EnemyKind kind, int health, int points, float speed, float interval, float height)
    {
        GameObject item = Visual(name, PhysicsRules.EnemyLayer, height);
        Body(item);
        Circle(item, height * .32f);
        EnemyShip enemy = item.AddComponent<EnemyShip>();
        enemy.kind = kind;
        enemy.baseHealth = health;
        enemy.points = points;
        enemy.speed = speed;
        enemy.shotInterval = interval;
        return Save<EnemyShip>(item);
    }

    private static Bullet CreateBullet(string name, int layer)
    {
        GameObject item = Visual(name, layer, .8f);
        Body(item);
        var collider = item.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(.13f, .55f) / item.transform.localScale.x;
        item.AddComponent<Bullet>();
        return Save<Bullet>(item);
    }

    private static PowerUp CreatePickup(string name, PickupKind kind)
    {
        GameObject item = Visual(name, PhysicsRules.PickupLayer, .9f);
        Circle(item, .52f);
        item.AddComponent<PowerUp>().kind = kind;
        return Save<PowerUp>(item);
    }

    [MenuItem("Tools/Nova Guard/Open Game Scene")]
    public static void OpenScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!File.Exists(ScenePath)) Generate();
        else EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Tools/Nova Guard/Build Windows x64")]
    public static void BuildWindows()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!File.Exists(ScenePath)) Generate();
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            throw new InvalidOperationException("Install Windows Build Support for Unity 2022.3 in Unity Hub before building.");
        Directory.CreateDirectory("Builds/Windows");
        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/Windows/NovaGuard.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("Windows build failed: " + report.summary.result + ". Read the Unity Console / build log.");
        Debug.Log("Windows build succeeded: Builds/Windows/NovaGuard.exe. Distribute the entire Windows folder, not only the EXE.");
    }
}
