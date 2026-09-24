using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Starward;

namespace StarwardEditor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/Generated/Scenes/Starward.unity";
        public const string PrefabsPath = "Assets/Generated/Prefabs/";
        static ProjectSetup() { EditorApplication.delayCall += AutoSetup; }

        static void AutoSetup()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AutoSetup;
                return;
            }
            // Do not repeatedly retry a failed setup during the same editor session.
            if (SessionState.GetBool("Starward.SetupAttempted", false)) return;
            SessionState.SetBool("Starward.SetupAttempted", true);
            if (SceneManager.GetActiveScene().isDirty)
            {
                Debug.LogWarning("Starward: save your current scene, then choose Starward > Create or Rebuild Game.");
                return;
            }
            try { Generate(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        [MenuItem("Starward/Create or Rebuild Game")]
        public static void SetupFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before rebuilding.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Rebuild generated assets?", "This replaces only Assets/Generated scene and prefabs. Copy customized generated assets elsewhere first. Runtime scripts, artwork and audio are untouched.", "Rebuild", "Cancel")) return;
            Generate();
        }

        // Also callable in batch mode: -executeMethod StarwardEditor.ProjectSetup.Generate
        public static void Generate()
        {
            Directory.CreateDirectory("Assets/Generated/Scenes");
            Directory.CreateDirectory(PrefabsPath);
            AssetDatabase.Refresh();
            Configure();
            ImportSprites();

            PlayerController player = MakeActor<PlayerController>("Player", "Player", 1.35f, 8, .32f);
            Enemy enemy = MakeActor<Enemy>("Enemy", "Enemy", 1.25f, 9, .37f);
            Bullet friendly = MakeBullet("PlayerBullet", 10, new Color(.25f, .95f, 1));
            Bullet hostile = MakeBullet("EnemyBullet", 11, new Color(1, .24f, .35f));
            Pickup repair = MakePickup("RepairPickup", "Repair", PickupKind.Repair);
            Pickup weapon = MakePickup("WeaponPickup", "Weapon", PickupKind.Weapon);
            GameObject burstObject = new GameObject("Burst");
            Burst burst = burstObject.AddComponent<Burst>();
            burst.spark = Sprite("Glow");
            burst = SavePrefab(burstObject).GetComponent<Burst>();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.012f, .021f, .054f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<WorldCamera>();

            GameObject root = new GameObject("Game");
            GameManager game = root.AddComponent<GameManager>();
            game.director = root.AddComponent<WaveDirector>();
            game.audioBank = root.AddComponent<AudioBank>();
            string[] names = { "laser.mp3", "EnemyShot.wav", "Explosion.wav", "Hit.wav", "Pickup.wav", "UI.wav", "GameOver.wav", "Wave.wav" };
            game.audioBank.clips = new AudioClip[names.Length];
            for (int i = 0; i < names.Length; i++)
                game.audioBank.clips[i] = Require<AudioClip>("Assets/Audio/" + names[i]);
            game.entities = new GameObject("Runtime Entities").transform;
            game.playerPrefab = player;
            game.enemyPrefab = enemy;
            game.playerBulletPrefab = friendly;
            game.enemyBulletPrefab = hostile;
            game.repairPrefab = repair;
            game.weaponPrefab = weapon;
            game.burstPrefab = burst;
            root.AddComponent<GameUI>().game = game;
            Starfield stars = new GameObject("Layered Starfield").AddComponent<Starfield>();
            stars.star = Sprite("Glow");

            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save " + ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildGame.ValidateProject();
            Debug.Log("Starward setup complete: scene and seven prefabs saved. Press Play. Use Starward > Build Windows x86_64 to build.");
        }

        static void Configure()
        {
            PlayerSettings.companyName = "Starward Studio";
            PlayerSettings.productName = "Starward";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            SerializedProperty input = settings.FindProperty("activeInputHandler");
            if (input == null) throw new InvalidOperationException("Expected activeInputHandler setting. Use Unity 6000.3.11f1.");
            input.intValue = 0; // Input Manager (Old), no new Input System package.
            settings.ApplyModifiedPropertiesWithoutUndo();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tags.FindProperty("layers");
            string[] names = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "Pickup" };
            for (int i = 0; i < names.Length; i++) layers.GetArrayElementAtIndex(i + 8).stringValue = names[i];
            tags.ApplyModifiedPropertiesWithoutUndo();
            Time.fixedDeltaTime = .02f;
            Physics2D.gravity = Vector2.zero;
            QualitySettings.vSyncCount = 1;
        }

        static void ImportSprites()
        {
            foreach (string name in new[] { "Player", "Enemy", "Repair", "Weapon", "Glow" })
            {
                string path = "Assets/Art/" + name + ".png";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new FileNotFoundException("Missing sprite", path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = name == "Glow" ? 64 : 256;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
        static Sprite Sprite(string name) { return Require<Sprite>("Assets/Art/" + name + ".png"); }
        static T Require<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new FileNotFoundException("Could not load " + typeof(T).Name, path);
            return asset;
        }
        static GameObject BaseObject(string name, string sprite, int layer, Vector3 scale, float radius)
        {
            GameObject go = new GameObject(name);
            go.layer = layer;
            go.transform.localScale = scale;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite(sprite);
            renderer.sortingOrder = 5;
            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            body.useFullKinematicContacts = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = radius;
            collider.isTrigger = true;
            return go;
        }
        static T MakeActor<T>(string name, string sprite, float scale, int layer, float radius) where T : Component
        {
            GameObject go = BaseObject(name, sprite, layer, Vector3.one * scale, radius);
            go.AddComponent<Health>();
            go.AddComponent<T>();
            return SavePrefab(go).GetComponent<T>();
        }
        static Bullet MakeBullet(string name, int layer, Color color)
        {
            GameObject go = BaseObject(name, "Glow", layer, new Vector3(.32f, .65f, 1), .32f);
            go.GetComponent<SpriteRenderer>().color = color;
            go.GetComponent<SpriteRenderer>().sortingOrder = 8;
            go.AddComponent<Bullet>();
            return SavePrefab(go).GetComponent<Bullet>();
        }
        static Pickup MakePickup(string name, string sprite, PickupKind kind)
        {
            GameObject go = BaseObject(name, sprite, 12, Vector3.one * .9f, .5f);
            go.AddComponent<Pickup>().kind = kind;
            return SavePrefab(go).GetComponent<Pickup>();
        }
        static GameObject SavePrefab(GameObject temporary)
        {
            try
            {
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(temporary, PrefabsPath + temporary.name + ".prefab");
                if (saved == null) throw new IOException("Prefab save failed: " + temporary.name);
                return saved;
            }
            finally { UnityEngine.Object.DestroyImmediate(temporary); }
        }
    }
}
