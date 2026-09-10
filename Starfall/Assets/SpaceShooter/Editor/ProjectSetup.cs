using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Starfall.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/SpaceShooter/Scenes/Starfall.unity";
        private const string Root = "Assets/SpaceShooter/";
        private const string Prefabs = Root + "Prefabs/";

        static ProjectSetup() { EditorApplication.delayCall += AutoSetup; }

        private static void AutoSetup()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AutoSetup;
                return;
            }
            if (SessionState.GetBool("Starfall.SetupAttempted", false)) return;
            SessionState.SetBool("Starfall.SetupAttempted", true);
            try { Generate(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        [MenuItem("Starfall/Generate or Rebuild Project Assets")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before generating assets.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Debug.Log("Starfall: importing included artwork and audio, then generating prefabs and the scene.");
            Directory.CreateDirectory(Prefabs);
            Directory.CreateDirectory(Root + "Scenes");
            AssetDatabase.Refresh();
            ConfigureProject();
            string[] images = { "Player", "Scout", "Gunship", "Commander", "Bolt", "EnemyBolt", "Repair", "RapidFire", "Shield", "Star" };
            foreach (string name in images) ImportSprite(name);
            string[] sounds = { "Laser", "Hit", "Explosion", "Pickup", "GameOver" };
            AudioClip[] clips = new AudioClip[sounds.Length];
            for (int i = 0; i < sounds.Length; i++)
            {
                string path = Root + "Audio/" + sounds[i] + ".wav";
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) throw new FileNotFoundException("Missing included sound", path);
                importer.forceToMono = true;
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }

            PlayerShip player = CreateShip<PlayerShip>("Player", 8, 1.5f, 0.28f);
            EnemyShip[] enemies = new EnemyShip[3];
            string[] names = { "Scout", "Gunship", "Commander" };
            float[] sizes = { 1.4f, 1.9f, 2.1f };
            for (int i = 0; i < names.Length; i++)
            {
                GameObject obj = SpriteObject(names[i], names[i], 9, sizes[i], 0.35f);
                var enemy = obj.AddComponent<EnemyShip>();
                enemy.kind = (EnemyKind)i;
                enemy.points = i == 0 ? 100 : i == 1 ? 250 : 500;
                enemies[i] = Save<EnemyShip>(obj, names[i]);
            }
            Projectile playerBullet = CreateBullet(true);
            Projectile enemyBullet = CreateBullet(false);
            Pickup[] pickups = new Pickup[3];
            string[] pickupNames = { "Repair", "RapidFire", "Shield" };
            for (int i = 0; i < pickupNames.Length; i++)
            {
                GameObject obj = SpriteObject(pickupNames[i], pickupNames[i], 12, 0.85f, 0.44f);
                var pickup = obj.AddComponent<Pickup>();
                pickup.kind = (PickupKind)i;
                pickups[i] = Save<Pickup>(obj, pickupNames[i]);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.012f, 0.024f, 0.052f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            var gameObject = new GameObject("Game", typeof(GameController), typeof(GameUI), typeof(GameAudio));
            GameController game = gameObject.GetComponent<GameController>();
            game.playerPrefab = player;
            game.enemyPrefabs = enemies;
            game.playerBulletPrefab = playerBullet;
            game.enemyBulletPrefab = enemyBullet;
            game.pickupPrefabs = pickups;
            game.audioSystem = gameObject.GetComponent<GameAudio>();
            game.audioSystem.clips = clips;
            game.actors = new GameObject("Runtime Actors").transform;
            gameObject.GetComponent<GameUI>().playerArt = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Art/Player.png");
            Starfield background = new GameObject("Three-layer Starfield", typeof(Starfield)).GetComponent<Starfield>();
            background.starSprite = Sprite("Star");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Failed to save Starfall scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Starfall: setup complete. Open Starfall.unity and press Play. Windows build: Starfall > Build Windows x64.");
        }

        private static void ConfigureProject()
        {
            PlayerSettings.companyName = "Starfall Studio";
            PlayerSettings.productName = "Starfall";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultScreenWidth = 720;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
            QualitySettings.vSyncCount = 1;
            Time.fixedDeltaTime = 0.01f;
            var timeSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset")[0]);
            timeSettings.FindProperty("Fixed Timestep").floatValue = 0.01f;
            timeSettings.ApplyModifiedPropertiesWithoutUndo();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tags.FindProperty("layers");
            string[] names = { "Player", "Enemy", "PlayerShot", "EnemyShot", "Pickup" };
            for (int i = 0; i < names.Length; i++) layers.GetArrayElementAtIndex(8 + i).stringValue = names[i];
            tags.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ImportSprite(string name)
        {
            string path = Root + "Art/" + name + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new FileNotFoundException("Missing included sprite", path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 256;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 512;
            importer.SaveAndReimport();
            if (Sprite(name) == null) throw new InvalidOperationException("Sprite import failed: " + name);
        }

        private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/" + name + ".png");

        private static GameObject SpriteObject(string name, string spriteName, int layer, float scale, float radius)
        {
            GameObject obj = new GameObject(name, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            obj.layer = layer;
            obj.transform.localScale = Vector3.one * scale;
            var renderer = obj.GetComponent<SpriteRenderer>();
            renderer.sprite = Sprite(spriteName);
            renderer.sortingOrder = layer == 8 ? 5 : layer == 12 ? 6 : 2;
            var body = obj.GetComponent<Rigidbody2D>();
            body.gravityScale = 0;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = obj.GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = radius;
            return obj;
        }

        private static T CreateShip<T>(string name, int layer, float scale, float radius) where T : Component
        {
            GameObject obj = SpriteObject(name, name, layer, scale, radius);
            obj.AddComponent<T>();
            return Save<T>(obj, name);
        }

        private static Projectile CreateBullet(bool friendly)
        {
            string name = friendly ? "PlayerBullet" : "EnemyBullet";
            GameObject obj = SpriteObject(name, friendly ? "Bolt" : "EnemyBolt", friendly ? 10 : 11, 0.75f, 0.13f / 0.75f);
            var shot = obj.AddComponent<Projectile>();
            shot.friendly = friendly;
            return Save<Projectile>(obj, name);
        }

        private static T Save<T>(GameObject obj, string name) where T : Component
        {
            try
            {
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, Prefabs + name + ".prefab");
                if (prefab == null) throw new IOException("Prefab generation failed: " + name);
                return prefab.GetComponent<T>();
            }
            finally { UnityEngine.Object.DestroyImmediate(obj); }
        }

        [MenuItem("Starfall/Validate Generated Assets")]
        public static void ValidateGeneratedAssets()
        {
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Generate the project assets first.", ScenePath);
            string[] names = { "Player", "Scout", "Gunship", "Commander", "PlayerBullet", "EnemyBullet", "Repair", "RapidFire", "Shield" };
            foreach (string name in names)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab");
                if (prefab == null || prefab.GetComponent<Rigidbody2D>() == null || prefab.GetComponent<CircleCollider2D>() == null)
                    throw new InvalidOperationException("Invalid physics prefab: " + name);
                if (prefab.GetComponent<SpriteRenderer>().sprite == null)
                    throw new InvalidOperationException("Missing sprite: " + name);
                if (!prefab.GetComponent<CircleCollider2D>().isTrigger || prefab.GetComponent<Rigidbody2D>().gravityScale != 0)
                    throw new InvalidOperationException("Incorrect collision setup: " + name);
            }
            string[] sounds = { "Laser", "Hit", "Explosion", "Pickup", "GameOver" };
            foreach (string name in sounds)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/" + name + ".wav");
                if (clip == null || clip.samples == 0) throw new InvalidOperationException("Invalid audio: " + name);
            }
            Debug.Log("Starfall: generated scene exists; all nine physics prefabs and five audio clips passed asset checks. Play-testing is still required.");
        }

        [MenuItem("Starfall/Build Windows x64")]
        public static void BuildWindows()
        {
            if (!File.Exists(ScenePath)) Generate();
            ValidateGeneratedAssets();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("Install Windows Build Support through Unity Hub for this editor version.");
            Directory.CreateDirectory("Builds/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/Starfall.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            Debug.Log("Starfall: building Windows x64 (Mono).");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("Starfall: Windows build succeeded. Distribute the entire Builds/Windows folder, not only the EXE.");
        }
    }
}
