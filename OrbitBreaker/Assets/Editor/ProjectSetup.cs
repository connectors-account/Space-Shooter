using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace OrbitBreaker.Editor
{
    /// <summary>Creates native Unity assets through Unity APIs, never hand-written scene YAML.</summary>
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string Generated = "Assets/Generated";
        public const string ScenePath = Generated + "/Scenes/OrbitBreaker.unity";
        public const string AssetsPath = Generated + "/GameAssets.asset";
        private static bool building;

        static ProjectSetup()
        {
            if (!Application.isBatchMode) EditorApplication.delayCall += FirstOpen;
        }

        private static void FirstOpen()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += FirstOpen;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode || building) return;
            // Existing generated scenes are never silently overwritten on a normal reload.
            if (File.Exists(ScenePath)) return;
            try { CreateProject(); }
            catch (Exception e) { Debug.LogException(e); }
        }

        [MenuItem("Orbit Breaker/Open Game Scene")]
        public static void OpenScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) CreateProject();
            else EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Orbit Breaker/Rebuild Generated Assets")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before rebuilding generated assets.");
            if (!EditorUtility.DisplayDialog("Rebuild Orbit Breaker?",
                "This overwrites the generated prefabs, asset catalogue, and game scene. Source scripts, PNGs and WAVs are preserved.", "Rebuild", "Cancel")) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateProject();
        }

        // Public so batch mode can invoke it explicitly without relying on delayCall.
        public static void SetupBatch()
        {
            if (!File.Exists(ScenePath)) CreateProject();
            ValidateProject();
        }

        private static void CreateProject()
        {
            if (building) return;
            building = true;
            try
            {
                Debug.Log("Orbit Breaker: configuring imports and creating the complete desktop game.");
                Directory.CreateDirectory(Generated + "/Prefabs");
                Directory.CreateDirectory(Generated + "/Scenes");
                AssetDatabase.Refresh();
                ConfigureSprites();
                ConfigureSettings();

                var catalogue = AssetDatabase.LoadAssetAtPath<GameAssets>(AssetsPath);
                if (catalogue == null)
                {
                    catalogue = ScriptableObject.CreateInstance<GameAssets>();
                    AssetDatabase.CreateAsset(catalogue, AssetsPath);
                }
                catalogue.player = CreatePlayer();
                catalogue.enemies = new[] { CreateEnemy("scout", EnemyPattern.Aimed, .60f),
                    CreateEnemy("fan", EnemyPattern.Fan, .68f), CreateEnemy("spinner", EnemyPattern.Radial, .62f) };
                catalogue.playerBullet = CreateBullet(true);
                catalogue.enemyBullet = CreateBullet(false);
                catalogue.powerups = new[] { CreatePowerup("repair", PowerupKind.Repair),
                    CreatePowerup("rapid", PowerupKind.Rapid), CreatePowerup("triple", PowerupKind.Triple) };
                catalogue.stars = new[] { Sprite("stars-far"), Sprite("stars-near") };
                catalogue.spark = Sprite("spark");
                catalogue.shoot = Sound("shoot"); catalogue.enemyShot = Sound("enemy-shot");
                catalogue.hit = Sound("hit"); catalogue.explode = Sound("explode");
                catalogue.pickup = Sound("pickup"); catalogue.wave = Sound("wave");
                catalogue.gameOver = Sound("game-over"); catalogue.ui = Sound("ui");
                EditorUtility.SetDirty(catalogue);

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(FixedAspectCamera));
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 0, -10);
                var camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true; camera.orthographicSize = 5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.014f, .022f, .052f);
                camera.nearClipPlane = .1f; camera.farClipPlane = 50f;

                var root = new GameObject("Game", typeof(AudioSource), typeof(WaveDirector), typeof(GameController));
                var game = root.GetComponent<GameController>();
                game.assets = catalogue;
                game.director = root.GetComponent<WaveDirector>();
                game.audioSource = root.GetComponent<AudioSource>();
                game.audioSource.playOnAwake = false; game.audioSource.spatialBlend = 0f;
                game.audioSource.volume = .8f;
                var canvas = new GameObject("Interface", typeof(RectTransform), typeof(GameUI));
                game.ui = canvas.GetComponent<GameUI>(); game.ui.game = game;
                new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
                var background = new GameObject("Parallax", typeof(ParallaxBackground)).GetComponent<ParallaxBackground>();
                background.assets = catalogue; background.game = game;
                EditorSceneManager.SaveScene(scene, ScenePath);
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
                AssetDatabase.SaveAssets();
                ValidateProject();
                Selection.activeGameObject = root;
                Debug.Log("Orbit Breaker setup complete. Press Play to launch. Windows build: Orbit Breaker > Build Windows x64.");
            }
            finally { building = false; }
        }

        private static void ConfigureSettings()
        {
            PlayerSettings.companyName = "Orbit Breaker";
            PlayerSettings.productName = "Orbit Breaker";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Standard); // .NET Standard 2.1 in 2022.3.
            Time.fixedDeltaTime = 1f / 60f;
            Time.maximumDeltaTime = .1f;
            QualitySettings.vSyncCount = 1;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input == null) throw new InvalidOperationException("Expected Unity 2022.3 activeInputHandler setting not found.");
            input.intValue = 0; // Input Manager (Old); no com.unity.inputsystem package.
            settings.ApplyModifiedPropertiesWithoutUndo();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("layers");
            string[] names = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "Powerup" };
            for (int i = 0; i < names.Length; i++) layers.GetArrayElementAtIndex(8 + i).stringValue = names[i];
            tags.ApplyModifiedPropertiesWithoutUndo();
            GameController.ConfigureCollisionLayers();
        }

        private static void ConfigureSprites()
        {
            foreach (string path in Directory.GetFiles("Assets/Art", "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 128;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }
        }

        private static Sprite Sprite(string name)
        {
            var value = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/" + name + ".png");
            if (value == null) throw new InvalidOperationException("Missing sprite: " + name);
            return value;
        }

        private static AudioClip Sound(string name)
        {
            var value = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + name + ".wav");
            if (value == null) throw new InvalidOperationException("Missing audio: " + name);
            return value;
        }

        private static GameObject Actor(string name, string sprite, int layer, float scale, float radius)
        {
            var go = new GameObject(name);
            go.layer = layer; go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Sprite(sprite); sr.sortingOrder = 1;
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0; body.drag = 0; body.angularDrag = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<CircleCollider2D>(); collider.radius = radius; collider.isTrigger = true;
            return go;
        }

        private static T Save<T>(GameObject go) where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, Generated + "/Prefabs/" + go.name + ".prefab");
            UnityEngine.Object.DestroyImmediate(go);
            if (prefab == null) throw new InvalidOperationException("Failed to save prefab.");
            return prefab.GetComponent<T>();
        }

        private static PlayerController CreatePlayer()
        {
            var go = Actor("Player", "player", 8, .66f, .47f);
            go.AddComponent<PlayerController>();
            return Save<PlayerController>(go);
        }

        private static Enemy CreateEnemy(string sprite, EnemyPattern pattern, float scale)
        {
            var go = Actor("Enemy-" + pattern, sprite, 9, scale, .66f);
            go.AddComponent<Enemy>().pattern = pattern;
            return Save<Enemy>(go);
        }

        private static Bullet CreateBullet(bool friendly)
        {
            var go = Actor(friendly ? "PlayerBullet" : "EnemyBullet", friendly ? "bolt" : "orb", friendly ? 10 : 11, 1f, .09f);
            // Long friendly bolts use a box; enemies use a circle slightly smaller than the visible orb.
            if (friendly)
            {
                UnityEngine.Object.DestroyImmediate(go.GetComponent<CircleCollider2D>());
                var box = go.AddComponent<BoxCollider2D>(); box.size = new Vector2(.12f, .30f); box.isTrigger = true;
            }
            go.GetComponent<SpriteRenderer>().sortingOrder = 5;
            go.AddComponent<Bullet>();
            return Save<Bullet>(go);
        }

        private static Powerup CreatePowerup(string sprite, PowerupKind kind)
        {
            var go = Actor("Powerup-" + kind, sprite, 12, 1.4f, .35f);
            go.AddComponent<Powerup>().kind = kind;
            return Save<Powerup>(go);
        }

        [MenuItem("Orbit Breaker/Validate Generated Project")]
        public static void ValidateProject()
        {
            var a = AssetDatabase.LoadAssetAtPath<GameAssets>(AssetsPath);
            if (a == null || a.player == null || a.playerBullet == null || a.enemyBullet == null ||
                a.enemies == null || a.enemies.Length != 3 || a.powerups == null || a.powerups.Length != 3 ||
                a.stars == null || a.stars.Length != 2 || a.spark == null)
                throw new BuildFailedException("Incomplete GameAssets catalogue. Rebuild generated assets.");
            var serialized = new SerializedObject(a);
            var property = serialized.GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                    throw new BuildFailedException("Unassigned GameAssets field: " + property.propertyPath);
            ValidateActor(a.player.gameObject, 8);
            ValidateActor(a.playerBullet.gameObject, 10);
            ValidateActor(a.enemyBullet.gameObject, 11);
            foreach (var enemy in a.enemies) ValidateActor(enemy.gameObject, 9);
            foreach (var pickup in a.powerups) ValidateActor(pickup.gameObject, 12);
            if (!File.Exists(ScenePath) || !EditorBuildSettings.scenes.Any(s => s.enabled && s.path == ScenePath))
                throw new BuildFailedException("Game scene missing from build settings.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path == ScenePath)
            {
                var games = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<GameController>(true)).ToArray();
                if (games.Length != 1 || games[0].assets != a || games[0].ui == null || games[0].ui.game != games[0] ||
                    games[0].director == null || games[0].audioSource == null)
                    throw new BuildFailedException("Game scene references are incomplete.");
            }
            Debug.Log("Orbit Breaker: catalogue references, nine actor prefabs, body/collider layers and build scene validated in Unity.");
        }

        private static void ValidateActor(GameObject go, int layer)
        {
            var rb = go.GetComponent<Rigidbody2D>(); var collider = go.GetComponent<Collider2D>();
            if (go.layer != layer || rb == null || rb.bodyType != RigidbodyType2D.Dynamic || rb.gravityScale != 0 ||
                rb.collisionDetectionMode != CollisionDetectionMode2D.Continuous || collider == null || !collider.isTrigger ||
                go.GetComponent<SpriteRenderer>() == null || go.GetComponent<SpriteRenderer>().sprite == null)
                throw new BuildFailedException("Invalid actor physics or sprite: " + go.name);
        }

        [MenuItem("Orbit Breaker/Build Windows x64")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new BuildFailedException("Exit Play mode before building.");
            SetupBatch();
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Standard);
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new BuildFailedException("Install Windows Build Support for Unity 2022.3.62f3 in Unity Hub.");
            Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/OrbitBreaker.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Windows build failed: " + report.summary.result);
            Debug.Log("Windows x86_64 build created: Builds/Windows/OrbitBreaker.exe (ship the entire Windows folder).");
        }
    }
}
