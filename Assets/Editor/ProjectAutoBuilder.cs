#if UNITY_EDITOR
using System.IO;
using SpaceShooter.Gameplay;
using SpaceShooter.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.EditorTools
{
    public static class ProjectAutoBuilder
    {
        private const string Root = "Assets";
        private const string ScenesPath = "Assets/Scenes";
        private const string PrefabsPath = "Assets/Prefabs";
        private const string SpritesPath = "Assets/Sprites";

        [InitializeOnLoadMethod]
        public static void EnsureProjectContentExists()
        {
            EditorApplication.delayCall += BuildIfMissing;
        }

        [MenuItem("Tools/Space Shooter/Regenerate Project Content")]
        public static void RebuildContent()
        {
            BuildAll(force: true);
        }

        private static void BuildIfMissing()
        {
            if (File.Exists(Path.Combine(ScenesPath, "MainMenu.unity")) &&
                File.Exists(Path.Combine(ScenesPath, "GamePlay.unity")) &&
                File.Exists(Path.Combine(ScenesPath, "GameOver.unity")))
            {
                return;
            }

            BuildAll(force: false);
        }

        private static void BuildAll(bool force)
        {
            EnsureFolders();
            CreateSpriteAssets(force);
            CreatePrefabAssets(force);
            CreateScenes(force);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder(Root, "Scenes");
            EnsureFolder(Root, "Prefabs");
            EnsureFolder(Root, "Sprites");
            EnsureFolder(Root, "Audio");
            EnsureFolder(Root, "Scripts");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var target = Path.Combine(parent, child);
            if (!AssetDatabase.IsValidFolder(target))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void CreateSpriteAssets(bool force)
        {
            CreateSpriteAsset("player_ship", 36, 28, new Color(0.4f, 0.95f, 1f), force);
            CreateSpriteAsset("enemy_basic", 30, 24, new Color(0.95f, 0.35f, 0.35f), force);
            CreateSpriteAsset("enemy_zigzag", 30, 24, new Color(0.95f, 0.75f, 0.35f), force);
            CreateSpriteAsset("enemy_tank", 34, 24, new Color(0.55f, 0.85f, 1f), force);
            CreateSpriteAsset("enemy_spinner", 28, 28, new Color(0.85f, 0.55f, 1f), force);
            CreateSpriteAsset("player_bullet", 6, 12, new Color(0.9f, 1f, 0.3f), force);
            CreateSpriteAsset("enemy_bullet", 6, 12, new Color(1f, 0.45f, 0.45f), force);
            CreateSpriteAsset("powerup_rapidfire", 18, 18, new Color(1f, 0.6f, 0.2f), force);
            CreateSpriteAsset("powerup_shield", 18, 18, new Color(0.3f, 0.8f, 1f), force);
            CreateSpriteAsset("powerup_health", 18, 18, new Color(0.35f, 1f, 0.45f), force);
            CreateSpriteAsset("powerup_spread", 18, 18, new Color(0.9f, 0.55f, 1f), force);
            CreateSpriteAsset("bg_layer1", 16, 16, new Color(0.07f, 0.07f, 0.2f), force);
            CreateSpriteAsset("bg_layer2", 16, 16, new Color(0.12f, 0.12f, 0.3f), force);
        }

        private static void CreateSpriteAsset(string assetName, int width, int height, Color color, bool force)
        {
            var texturePath = $"{SpritesPath}/{assetName}.png";
            if (!force && File.Exists(texturePath))
            {
                return;
            }

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            File.WriteAllBytes(texturePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(texturePath);
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 24;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        private static void CreatePrefabAssets(bool force)
        {
            CreatePrefab($"{PrefabsPath}/Player.prefab", "player_ship", force);
            CreatePrefab($"{PrefabsPath}/EnemyBasic.prefab", "enemy_basic", force);
            CreatePrefab($"{PrefabsPath}/EnemyZigzag.prefab", "enemy_zigzag", force);
            CreatePrefab($"{PrefabsPath}/EnemyTank.prefab", "enemy_tank", force);
            CreatePrefab($"{PrefabsPath}/EnemySpinner.prefab", "enemy_spinner", force);
            CreatePrefab($"{PrefabsPath}/PlayerBullet.prefab", "player_bullet", force);
            CreatePrefab($"{PrefabsPath}/EnemyBullet.prefab", "enemy_bullet", force);
            CreatePrefab($"{PrefabsPath}/PowerRapid.prefab", "powerup_rapidfire", force);
            CreatePrefab($"{PrefabsPath}/PowerShield.prefab", "powerup_shield", force);
            CreatePrefab($"{PrefabsPath}/PowerHealth.prefab", "powerup_health", force);
            CreatePrefab($"{PrefabsPath}/PowerSpread.prefab", "powerup_spread", force);
        }

        private static void CreatePrefab(string path, string spriteName, bool force)
        {
            if (!force && File.Exists(path))
            {
                return;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/{spriteName}.png");
            var go = new GameObject(Path.GetFileNameWithoutExtension(path));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            go.AddComponent<CircleCollider2D>().isTrigger = true;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateScenes(bool force)
        {
            CreateScene("MainMenu", typeof(MainMenuController), force);
            CreateScene("GamePlay", typeof(GameBootstrap), force);
            CreateScene("GameOver", typeof(GameOverController), force);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesPath}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/GamePlay.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/GameOver.unity", true)
            };
        }

        private static void CreateScene(string sceneName, System.Type controllerType, bool force)
        {
            var scenePath = $"{ScenesPath}/{sceneName}.unity";
            if (!force && File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            cameraGo.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5.6f;
            camera.backgroundColor = new Color(0.03f, 0.03f, 0.08f);
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            var root = new GameObject($"{sceneName}Controller");
            root.AddComponent(controllerType);

            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
#endif
