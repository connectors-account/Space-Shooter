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
    public static class ProjectBuilder
    {
        public const string ScenePath = "Assets/Starfall/Scenes/Starfall.unity";
        const string CatalogPath = "Assets/Starfall/Resources/GameAssets.asset";
        const string Prefabs = "Assets/Starfall/Generated/Prefabs";
        static bool building;
        static ProjectBuilder() { EditorApplication.delayCall += FirstImport; }
        static void FirstImport()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += FirstImport; return; }
            if (!File.Exists(CatalogPath))
            {
                try { Generate(); Debug.Log("Starfall content ready. Open Assets/Starfall/Scenes/Starfall.unity and press Play."); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        [MenuItem("Starfall/Rebuild Generated Content")]
        public static void Generate()
        {
            if (building) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before generating content.");
            building = true;
            try
            {
                Directory.CreateDirectory(Prefabs);
                Directory.CreateDirectory("Assets/Starfall/Resources");
                Directory.CreateDirectory("Assets/Starfall/Scenes");
                AssetDatabase.Refresh();
                Debug.Log("Starfall: generating original sprites, PCM sounds and wired prefabs.");
                var a = AssetDatabase.LoadAssetAtPath<GameAssets>(CatalogPath);
                bool fresh = a == null;
                if (fresh) a = ScriptableObject.CreateInstance<GameAssets>();
                var cyan = new Color(.25f, .85f, 1);
                var coral = new Color(1, .32f, .35f);
                var gold = new Color(1, .72f, .24f);
                var purple = new Color(.75f, .42f, 1);
                a.player = Prefab<PlayerShip>("Player", ProceduralAssets.Sprite("player", 0, cyan, 135), .25f, 5);
                a.scout = Enemy("Scout", 1, 0, coral);
                a.fan = Enemy("Fan", 2, 1, gold);
                a.spinner = Enemy("Spinner", 3, 2, purple);
                a.playerBullet = Prefab<Projectile>("PlayerBullet", ProceduralAssets.Sprite("player-bullet", 4, cyan, 190), .055f, 4, p => p.hostile = false);
                a.enemyBullet = Prefab<Projectile>("EnemyBullet", ProceduralAssets.Sprite("enemy-bullet", 5, coral, 230), .105f, 4, p => p.hostile = true);
                a.repair = Power("Repair", 6, PowerKind.Repair, new Color(.25f, 1, .55f));
                a.rapid = Power("Rapid", 7, PowerKind.Rapid, gold);
                a.spread = Power("Spread", 8, PowerKind.Spread, purple);
                a.star = ProceduralAssets.Sprite("star", 9, Color.white, 240);
                a.spark = ProceduralAssets.Sprite("spark", 10, Color.white, 370);
                a.shoot = ProceduralAssets.Sound("shoot", 0, .10f);
                a.enemyShoot = ProceduralAssets.Sound("enemy-shoot", 1, .15f);
                a.explosion = ProceduralAssets.Sound("explosion", 2, .38f);
                a.hurt = ProceduralAssets.Sound("hurt", 3, .24f);
                a.collect = ProceduralAssets.Sound("collect", 4, .3f);
                a.wave = ProceduralAssets.Sound("wave", 5, .45f);
                if (fresh) AssetDatabase.CreateAsset(a, CatalogPath);
                EditorUtility.SetDirty(a);
                ConfigureSettings();
                if (!File.Exists(ScenePath)) CreateScene();
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ValidateContent();
                Debug.Log("Starfall: content generation and serialized reference checks passed.");
            }
            finally { building = false; }
        }
        static EnemyShip Enemy(string name, int spriteKind, int pattern, Color color)
        {
            return Prefab<EnemyShip>(name, ProceduralAssets.Sprite(name.ToLowerInvariant(), spriteKind, color, 125), .32f, 3, e => e.pattern = pattern);
        }
        static Pickup Power(string name, int spriteKind, PowerKind kind, Color color)
        {
            return Prefab<Pickup>(name, ProceduralAssets.Sprite(name.ToLowerInvariant(), spriteKind, color, 155), .3f, 6, p => p.kind = kind);
        }
        static T Prefab<T>(string name, Sprite sprite, float radius, int order, Action<T> setup = null) where T : Component
        {
            var go = new GameObject(name);
            try
            {
                var image = go.AddComponent<SpriteRenderer>(); image.sprite = sprite; image.sortingOrder = order;
                var body = go.AddComponent<Rigidbody2D>(); body.gravityScale = 0;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                var collider = go.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = radius;
                T component = go.AddComponent<T>(); setup?.Invoke(component);
                var saved = PrefabUtility.SaveAsPrefabAsset(go, Prefabs + "/" + name + ".prefab");
                if (saved == null) throw new InvalidOperationException("Prefab save failed: " + name);
                return saved.GetComponent<T>();
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        static void ConfigureSettings()
        {
            PlayerSettings.companyName = "Starfall Studio";
            PlayerSettings.productName = "Starfall";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            // Input.GetKey uses the built-in (legacy) input backend. No axes or Input System package required.
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input != null && input.intValue != 0) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }
        }
        static void CreateScene()
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                new GameObject("Starfall Scene Entry").AddComponent<SceneEntry>();
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save Starfall scene.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }
        [MenuItem("Starfall/Open Game Scene")]
        public static void OpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(CatalogPath)) Generate();
            EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("Starfall/Validate Generated Content")]
        public static void ValidateContent()
        {
            var a = AssetDatabase.LoadAssetAtPath<GameAssets>(CatalogPath);
            if (a == null) throw new InvalidOperationException("Missing GameAssets catalog.");
            foreach (var field in typeof(GameAssets).GetFields())
                if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType) && (UnityEngine.Object)field.GetValue(a) == null)
                    throw new InvalidOperationException("Unwired GameAssets field: " + field.Name);
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { Prefabs }))
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(p) != 0 || p.GetComponent<SpriteRenderer>().sprite == null || !p.GetComponent<CircleCollider2D>().isTrigger || p.GetComponent<Rigidbody2D>().gravityScale != 0)
                    throw new InvalidOperationException("Invalid generated prefab: " + p.name);
            }
            Debug.Log("Starfall: catalog and prefabs have valid references.");
        }
        [MenuItem("Starfall/Build Windows x86_64")]
        public static void BuildWindows()
        {
            Generate();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("Install Windows Build Support (Mono) for this Unity Editor in Unity Hub.");
            Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/Windows/Starfall.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("Windows x86_64 build succeeded. Distribute the ENTIRE Builds/Windows folder, not only the exe.");
        }
    }
}
